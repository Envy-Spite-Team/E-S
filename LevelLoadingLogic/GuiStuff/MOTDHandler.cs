using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.TextCore;

namespace DoomahLevelLoader
{
    public static class MOTDManager
    {
        private const string motdUrl = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/refs/heads/emoji/MOTD.txt";
        private const string fallbackImageUrl = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/image.png";
        private const string discordCdnBaseUrl = "https://cdn.discordapp.com/emojis/";
        private static Dictionary<string, Texture2D> emojiTextures = new Dictionary<string, Texture2D>();
        private static Vector2 emojiPivot = new Vector2(0.5f, 0.5f);
        private static float baseTMPSize = 48f;

        public static IEnumerator LoadMOTD(TextMeshProUGUI motdText, Image motdImageComponent)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(motdUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debugger.LogError("Failed to fetch MOTD: " + webRequest.error);
                }
                else
                {
                    string motdRawText = webRequest.downloadHandler.text;
                    if (!string.IsNullOrEmpty(motdRawText))
                    {
                        MonoBehaviour.FindObjectOfType<MonoBehaviour>().StartCoroutine(ConvertMarkdownToTMP($"<size=24>{motdRawText}</size>", motdText, (formattedText) =>
                        {
                            motdText.text = formattedText;
                            motdImageComponent.gameObject.SetActive(false);
                        }));
                    }
                    else
                    {
                        yield return LoadFallbackImage(motdImageComponent);
                        motdText.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static IEnumerator ConvertMarkdownToTMP(string input, TextMeshProUGUI motdText, System.Action<string> callback)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\*\*(.*?)\*\*", "<b>$1</b>");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\*(.*?)\*", "<i>$1</i>");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"__(.*?)__", "<u>$1</u>");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"~~(.*?)~~", "<s>$1</s>");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\*\*\*(.*?)\*\*\*", "<b><i>$1</i></b>");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"^# (.*?)$", "<size=40>$1</size>", System.Text.RegularExpressions.RegexOptions.Multiline);
            input = System.Text.RegularExpressions.Regex.Replace(input, @"^## (.*?)$", "<size=30>$1</size>", System.Text.RegularExpressions.RegexOptions.Multiline);
            input = System.Text.RegularExpressions.Regex.Replace(input, @"^### (.*?)$", "<size=20>$1</size>", System.Text.RegularExpressions.RegexOptions.Multiline);

            float currentSize = baseTMPSize;
            var sizeMatches = System.Text.RegularExpressions.Regex.Matches(input, @"<size=(\d+)>");

            foreach (System.Text.RegularExpressions.Match sizeMatch in sizeMatches)
            {
                float sizeValue = float.Parse(sizeMatch.Groups[1].Value);
                currentSize = sizeValue > currentSize ? sizeValue : currentSize;
            }

            var emojiMatches = System.Text.RegularExpressions.Regex.Matches(input, @"<:([^:]+):(\d+)>");
            Dictionary<string, int> emojiIndexMap = new Dictionary<string, int>();

            foreach (System.Text.RegularExpressions.Match match in emojiMatches)
            {
                string emojiName = match.Groups[1].Value;
                string emojiId = match.Groups[2].Value;
                string emojiKey = $"{emojiName}:{emojiId}";

                if (!emojiTextures.ContainsKey(emojiKey))
                {
                    string emojiUrl = $"{discordCdnBaseUrl}{emojiId}.png";
                    yield return LoadEmojiSprite(emojiUrl, emojiKey, motdText);
                }

                if (!emojiIndexMap.ContainsKey(emojiKey))
                {
                    emojiIndexMap[emojiKey] = emojiTextures.Count - 1;
                }

                int index = emojiIndexMap[emojiKey];
                input = input.Replace(match.Value, $"<sprite index={index}>");
            }

            Texture2D spriteSheet = SpriteSheetGenerator.MakeSpriteSheet(emojiTextures.Values.ToArray());

            if (spriteSheet == null)
            {
                Debugger.LogError("Failed to generate sprite sheet, spriteSheet is null.");
                callback(input);
                yield break;
            }

            TMP_SpriteAsset asset = ScriptableObject.Instantiate(Resources.Load("Sprite Assets/EmojiOne") as TMP_SpriteAsset);//ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.version = "1.1.0";
            Sprite[] sprites = new Sprite[emojiTextures.Values.Count];
            int i = 0;

            foreach (var tex in emojiTextures.Values)
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                Debugger.Log(tex.name);
                tex.name = i.ToString();
                sprites[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), emojiPivot, currentSize / baseTMPSize * tex.width);
                sprites[i].texture.filterMode = FilterMode.Point;
                i++;
            }

            asset.spriteInfoList = new List<TMP_Sprite>();
            for (i = 0; i < emojiTextures.Values.Count; i++)
            {
                sprites[i].name = i.ToString();
                TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph
                {
                    index = (uint)i,
                    sprite = sprites[i],
                    metrics = new GlyphMetrics(sprites[i].rect.width, sprites[i].rect.height, 0, 116, sprites[i].rect.width),
                    glyphRect = new GlyphRect(0, 0, (int)(sprites[i].rect.width), (int)(sprites[i].rect.height)),
                    scale = 1f,
                    atlasIndex = 0,
                };

                TMP_SpriteCharacter spriteCharacter = new TMP_SpriteCharacter((uint)i, spriteGlyph)
                {
                    name = sprites[i].name,
                    scale = 1f,
                    glyphIndex = (uint)i,
                    glyph = spriteGlyph,
                    unicode = (i == 0) ? 65534u : ((uint)i),
                };

                TMP_Sprite spr = new TMP_Sprite()
                {
                    unicode = i,
                    scale = 1f,
                    id = i,
                    sprite = sprites[i],
                    name = sprites[i].name,
                    x = 0, y = 0,
                    width = sprites[i].rect.width, height = sprites[i].rect.height,
                    yOffset = 116, xOffset = 0, xAdvance = 0
                };

                asset.spriteInfoList.Add(spr);

                asset.spriteGlyphTable.Add(spriteGlyph);
                asset.spriteCharacterTable.Add(spriteCharacter);
            }

            asset.spriteSheet = spriteSheet;
            asset.material = new Material(Shader.Find("TextMeshPro/Sprite"));
            asset.material.mainTexture = spriteSheet;
            asset.UpdateLookupTables();

            if (asset != null)
            {
                motdText.spriteAsset = asset;
            }
            else
            {
                Debugger.LogError("TMP_SpriteAsset is null. Cannot assign sprite asset to TextMeshPro.");
            }

            callback(input);
        }

        private static IEnumerator LoadEmojiSprite(string emojiUrl, string emojiKey, TextMeshProUGUI motdText)
        {
            using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(emojiUrl))
            {
                yield return imageRequest.SendWebRequest();

                if (imageRequest.isNetworkError || imageRequest.isHttpError)
                {
                    Debugger.LogError("Failed to fetch emoji image: " + imageRequest.error);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                    texture.filterMode = FilterMode.Point;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    emojiTextures[emojiKey] = texture;
                }
            }
        }

        private static IEnumerator LoadFallbackImage(Image motdImageComponent)
        {
            using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(fallbackImageUrl))
            {
                yield return imageRequest.SendWebRequest();

                if (imageRequest.isNetworkError || imageRequest.isHttpError)
                {
                    Debugger.LogError("Failed to fetch fallback image: " + imageRequest.error);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
                    sprite.texture.filterMode = FilterMode.Point;
                    motdImageComponent.sprite = sprite;
                    motdImageComponent.gameObject.SetActive(true);
                }
            }
        }
    }

    public static class SpriteSheetGenerator
    {
        public static Texture2D MakeSpriteSheet(Texture2D[] textures)
        {
            if (textures.Length == 0)
                return null;

            int textureWidth = textures[0].width;
            int textureHeight = textures[0].height;
            int sheetWidth = textures.Length;
            int sheetHeight = 1;

            Texture2D spriteSheet = new Texture2D(textureWidth * sheetWidth, textureHeight * sheetHeight);
            spriteSheet.name = "EmojiSpriteSheet";
            int textureIndex = 0;

            for (int y = 0; y < sheetHeight; y++)
            {
                for (int x = 0; x < sheetWidth; x++)
                {
                    if (textureIndex >= textures.Length)
                        break;

                    Texture2D currentTexture = textures[textureIndex];
                    Color[] pixels = currentTexture.GetPixels();

                    spriteSheet.SetPixels(x * textureWidth, y * textureHeight, textureWidth, textureHeight, pixels);
                    textureIndex++;
                }
            }

            spriteSheet.Apply();
            return spriteSheet;
        }
    }
}
