using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.TextCore;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using System;

namespace EnvyLevelLoader
{
    public static class MOTDManager
    {
        // Represents a downloaded emoji texture or a frame
        // of an animated emoji
        private class EmojiTexture
        {
            public readonly Texture2D texture; // Downloaded texture
            public readonly uint id; // Atlas id

            // Current sprite sheet rect info. Is not initialized until
            // the emoji asset is built.
            public int x = 0;
            public int y = 0;
            public int width = 0;
            public int height = 0;

            public bool valid => width != 0 && height != 0;

            public EmojiTexture(Texture2D texture, uint id)
            {
                this.texture = texture;
                this.id = id;
            }
        }

        // Represents an emoji character
        private class EmojiAsset
        {
            // Edit emoji character size from here
            public const float EMOJI_SCALE = 2;

            public List<EmojiTexture> originalTextures { get; private set; } // Emoji texture or frames
            public string originalName { get; private set; } // Name of the emoji based on the tag
            public uint idStart { get; private set; } // Id of the first texture on sprite atlas
            public uint idEnd { get; private set; } // Id of the last texture on sprite atlas, same as idStart if the emoji is not animated
            public int fps { get; private set; } // Animated emoji frame rate

            public int textureCount => originalTextures.Count;

            // Text mesh pro atlas objects
            public List<TMP_SpriteGlyph> glyphs { get; private set; }
            public List<TMP_SpriteCharacter> characters { get; private set; }

            // Current atlas id, must only be incremented from the constructor
            private static uint m_currentIndex = 0;

            // Constructor for animated emojis
            public EmojiAsset(List<Texture2D> textures, string name, int fps)
            {
                originalTextures = new List<EmojiTexture>();
                originalName = name;
                this.fps = fps;
                idStart = m_currentIndex;

                foreach (Texture2D tex in textures)
                {
                    idEnd = m_currentIndex;
                    originalTextures.Add(new EmojiTexture(tex, m_currentIndex++));
                }

                glyphs = new List<TMP_SpriteGlyph>();
                characters = new List<TMP_SpriteCharacter>();

                // No TMP atlas objects generated until the atlas is rebuilt
                for (int i = 0; i < textureCount; i++)
                {
                    glyphs.Add(null);
                    characters.Add(null);
                }
            }

            // Constructor for non-animated emojis
            public EmojiAsset(Texture2D texture, string name) : this(new List<Texture2D>() { texture }, name, 0) { }

            // Create TMP atlas object based on the new sprite atlas texture
            public void Rebuild(Texture2D spriteAtlas, int emojiIndex)
            {
                EmojiTexture emoji = originalTextures[emojiIndex];
                int x = emoji.x;
                int y = emoji.y;
                int width = emoji.width;
                int height = emoji.height;

                Sprite sprite = Sprite.Create(spriteAtlas, new Rect(x, y, width, height), new Vector2(0.5f, 0.5f));

                TMP_SpriteGlyph glyph = new TMP_SpriteGlyph();
                glyph.index = emoji.id;
                glyph.atlasIndex = (int)emoji.id;
                glyph.sprite = sprite;
                glyph.metrics = new GlyphMetrics(width, height, 0, 0, width);
                glyph.glyphRect = new GlyphRect(x, y, width, height);
                glyph.scale = 1f;
                glyph.atlasIndex = 0;

                TMP_SpriteCharacter character = new TMP_SpriteCharacter();
                character.glyph = glyph;
                character.glyphIndex = emoji.id;
                character.unicode = 65534u;
                character.name = originalName;
                character.scale = EMOJI_SCALE;

                glyphs[emojiIndex] = glyph;
                characters[emojiIndex] = character;
            }
        }

        // Stores and retrives emojis from local cache
        private static class EmojiCache
        {
            public static string CacheDirectory => Path.Combine(Paths.CachePath, "Envy", "Emojis");
            public static string CacheDatabasePath => Path.Combine(CacheDirectory, "cacheDatabase.json");

            private class EmojiCacheInfo
            {
                public List<string> textureNames;
                public int fps { get; set; }

                public EmojiCacheInfo()
                {
                    textureNames = new List<string>();
                    fps = 0;
                }
            }

            private class CacheDatabase
            {
                [JsonIgnore]
                public const string CURRENT_VERSION = "1.0.0";

                public string version { get; set; }
                public Dictionary<string, EmojiCacheInfo> emojiPaths;

                public CacheDatabase()
                {
                    version = CURRENT_VERSION;
                    emojiPaths = new Dictionary<string, EmojiCacheInfo>();
                }
            }

            private static CacheDatabase _cacheDatabase;

            static EmojiCache()
            {
                if (!Directory.Exists(CacheDirectory))
                    Directory.CreateDirectory(CacheDirectory);

                if (File.Exists(CacheDatabasePath))
                {
                    try
                    {
                        _cacheDatabase = JsonConvert.DeserializeObject<CacheDatabase>(File.ReadAllText(CacheDatabasePath));

                        if (_cacheDatabase.emojiPaths == null)
                            _cacheDatabase = null;

                        if (_cacheDatabase.version != CacheDatabase.CURRENT_VERSION)
                        {
                            // Handle outdated or invalid cache database
                            switch (_cacheDatabase.version)
                            {
                                default:
                                    Debug.LogWarning("Cache database version is invalid, discarding the cached emojis");
                                    _cacheDatabase = null;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Envy failed to read cache database file:");
                        Debug.LogException(e);
                        _cacheDatabase = new CacheDatabase();
                    }
                }

                if (_cacheDatabase == null)
                {
                    _cacheDatabase = new CacheDatabase();
                }
            }

            public static bool TryGetEmoji(string emojiId, out List<Texture2D> textures, out int fps)
            {
                if (!_cacheDatabase.emojiPaths.TryGetValue(emojiId, out EmojiCacheInfo emojiCache))
                {
                    textures = null;
                    fps = 0;
                    return false;
                }

                textures = new List<Texture2D>();
                foreach (string textureName in emojiCache.textureNames)
                {
                    string texturePath = Path.Combine(CacheDirectory, textureName);
                    if (!File.Exists(texturePath))
                    {
                        // Release previously created textures
                        foreach (Texture2D createdTexture in textures)
                            UnityEngine.Object.Destroy(createdTexture);

                        textures = null;
                        fps = 0;
                        return false;
                    }

                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(File.ReadAllBytes(texturePath));
                    textures.Add(texture);
                }

                fps = emojiCache.fps;
                return true;
            }

            public static void AddEmoji(string emojiId, EmojiAsset emoji)
            {
                EmojiCacheInfo emojiCache = new EmojiCacheInfo();

                emojiCache.fps = emoji.fps;
                emojiCache.textureNames = new List<string>();

                // Add every emoji texture to the database
                int currentTextureIndex = 0;
                foreach (Texture2D texture in emoji.originalTextures.Select(tex => tex.texture))
                {
                    string textureName = $"{emojiId}_{currentTextureIndex++}.png";
                    string texturePath = Path.Combine(CacheDirectory, textureName);
                    File.WriteAllBytes(texturePath, texture.EncodeToPNG());

                    emojiCache.textureNames.Add(textureName);
                }

                _cacheDatabase.emojiPaths[emojiId] = emojiCache;
                File.WriteAllText(CacheDatabasePath, JsonConvert.SerializeObject(_cacheDatabase));
            }
        }

        // Manages all the emoji downloads and storage
        private class EmojiManager : MonoBehaviour
        {
            // Coroutines are ran trough a hidden instance
            private static EmojiManager _instance = null;
            public static EmojiManager Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("EnvyEmojiManager").AddComponent<EmojiManager>();
                        _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    }

                    return _instance;
                }
            }

            private static Dictionary<string, EmojiAsset> emojiTextures = new Dictionary<string, EmojiAsset>();
            public static TMP_SpriteAsset currentEmojiAtlas { get; private set; } = null;

            // Emoji atlas constraints, edit quality from here
            public const int EMOJI_ATLAS_MAX_WIDTH = 4096;
            public const int EMOJI_ATLAS_MAX_HEIGHT = 4096;
            public const int EMOJI_WIDTH = 128;
            public const int EMOJI_HEIGHT = 128;
            public const int EMOJI_ATLAS_COLUMN_CAPACITY = EMOJI_ATLAS_MAX_WIDTH / EMOJI_WIDTH;

            public static bool EmojiExists(string emojiId)
            {
                return emojiTextures.ContainsKey(emojiId);
            }

            public static EmojiAsset GetEmoji(string emojiId)
            {
                if (emojiTextures.TryGetValue(emojiId, out EmojiAsset emoji))
                    return emoji;
                return null;
            }

            public static bool TryGetEmoji(string emojiId, out EmojiAsset emoji)
            {
                if (emojiTextures.TryGetValue(emojiId, out emoji))
                    return true;

                return false;
            }

            #region Load Emoji Methods
            private class LoadEmojiResult
            {
                public bool isDone = false;
            }

            private static Dictionary<string, LoadEmojiResult> _loadEmojiTasks = new Dictionary<string, LoadEmojiResult>();

            public static IEnumerator LoadEmojiFromURL(string emojiUrl, string emojiId)
            {
                if (EmojiExists(emojiId))
                    yield break;

                // Attempt to load from local cache
                if (EmojiCache.TryGetEmoji(emojiId, out List<Texture2D> cachedEmojiTextures, out int fps))
                {
                    emojiTextures[emojiId] = new EmojiAsset(cachedEmojiTextures, emojiId, fps);
                    yield break;
                }

                if (!_loadEmojiTasks.TryGetValue(emojiId, out LoadEmojiResult currentTask) || currentTask.isDone)
                {
                    currentTask = new LoadEmojiResult();
                    Instance.StartCoroutine(_LoadEmoji(emojiUrl, emojiId, currentTask));
                    _loadEmojiTasks[emojiId] = currentTask;
                }

                yield return new WaitUntil(() => currentTask.isDone);
            }

            private static IEnumerator _LoadEmoji(string emojiUrl, string emojiId, LoadEmojiResult result)
            {
                try
                {
                    using (UnityWebRequest gifRequest = UnityWebRequestTexture.GetTexture($"{emojiUrl}.gif"))
                    {
                        yield return gifRequest.SendWebRequest();

                        if (gifRequest.isNetworkError || gifRequest.isHttpError)
                        {
                            using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture($"{emojiUrl}.png"))
                            {
                                yield return imageRequest.SendWebRequest();

                                if (imageRequest.isNetworkError || imageRequest.isHttpError)
                                {
                                    Debugger.LogError("Failed to fetch emoji image: " + imageRequest.error);
                                }
                                else
                                {
                                    Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                                    texture.name = emojiId;
                                    texture.filterMode = FilterMode.Point;
                                    texture.wrapMode = TextureWrapMode.Clamp;

                                    EmojiAsset emojiAsset = new EmojiAsset(texture, emojiId);
                                    emojiTextures[emojiId] = emojiAsset;
                                    EmojiCache.AddEmoji(emojiId, emojiAsset);
                                }
                            }
                        }
                        else
                        {
                            yield return UniGif.GetTextureListCoroutine(gifRequest.downloadHandler.data, (gifTexList, loopCount, width, height) =>
                            {
                                gifTexList.ForEach(frame =>
                                {
                                    frame.m_texture2d.filterMode = FilterMode.Point;
                                    frame.m_texture2d.wrapMode = TextureWrapMode.Clamp;
                                });

                                float duration = gifTexList.Sum(frame => frame.m_delaySec);
                                EmojiAsset emojiAsset = new EmojiAsset(
                                    gifTexList.Select(frame => frame.m_texture2d).ToList(),
                                    emojiId,
                                    (int)(gifTexList.Count / duration)
                                );

                                emojiTextures[emojiId] = emojiAsset;
                                EmojiCache.AddEmoji(emojiId, emojiAsset);
                            });
                        }
                    }
                }
                finally
                {
                    result.isDone = true;
                }
            }
            #endregion

            #region Regenerate Emoji Atlas Methods
            private class RegenerateEmojiAtlasResult
            {
                public bool isDone = false;
                public TMP_SpriteAsset result = null;
            }

            private static RegenerateEmojiAtlasResult _regenerateEmojiAtlasResult = null;

            public static IEnumerator RegenerateEmojiAtlas(TextMeshProUGUI motdText)
            {
                if (_regenerateEmojiAtlasResult != null && !_regenerateEmojiAtlasResult.isDone)
                {
                    yield return new WaitUntil(() => _regenerateEmojiAtlasResult.isDone);
                    motdText.spriteAsset = _regenerateEmojiAtlasResult.result;
                }
                else
                {
                    _regenerateEmojiAtlasResult = new RegenerateEmojiAtlasResult();
                    Instance.StartCoroutine(_RegenerateEmojiAtlas(motdText, _regenerateEmojiAtlasResult));
                    yield return new WaitUntil(() => _regenerateEmojiAtlasResult.isDone);
                }
            }

            private static IEnumerator _RegenerateEmojiAtlas(TextMeshProUGUI motdText, RegenerateEmojiAtlasResult status)
            {
                try
                {
                    if (currentEmojiAtlas != null)
                    {
                        if (currentEmojiAtlas.spriteSheet != null)
                            UnityEngine.Object.Destroy(currentEmojiAtlas.spriteSheet);

                        UnityEngine.Object.Destroy(currentEmojiAtlas);
                    }

                    TMP_SpriteAsset atlasData = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                    currentEmojiAtlas = atlasData;

                    // Create the sprite atlas
                    int textureCount = emojiTextures.Values.Sum(emoji => emoji.textureCount);
                    int rowCount = (textureCount - 1) / EMOJI_ATLAS_COLUMN_CAPACITY + 1;
                    if (rowCount <= 0)
                        yield break;
                    int columnSize = Mathf.Min(EMOJI_ATLAS_MAX_WIDTH, EMOJI_WIDTH * textureCount);
                    Texture2D atlas = new Texture2D(columnSize, EMOJI_HEIGHT * rowCount, TextureFormat.RGBA32, false);

                    // Make the atlas transparent
                    atlas.SetPixels(Enumerable.Repeat(new Color(0, 0, 0, 0), atlas.width * atlas.height).ToArray());

                    int currentOffsetX = 0;
                    int currentOffsetY = 0;
                    RenderTexture rt = new RenderTexture(EMOJI_WIDTH, EMOJI_HEIGHT, 24);
                    RenderTexture previousRt = RenderTexture.active;
                    RenderTexture.active = rt;

                    // Copy emoji textures to the atlas
                    foreach (EmojiAsset emoji in emojiTextures.Values.OrderBy(e => e.idStart))
                    {
                        for (int i = 0; i < emoji.textureCount; i++)
                        {
                            EmojiTexture tex = emoji.originalTextures[i];
                            Graphics.Blit(tex.texture, rt);
                            atlas.ReadPixels(new Rect(0, 0, EMOJI_WIDTH, EMOJI_HEIGHT), currentOffsetX, currentOffsetY);

                            tex.x = currentOffsetX;
                            tex.y = currentOffsetY;
                            tex.width = EMOJI_WIDTH;
                            tex.height = EMOJI_HEIGHT;

                            currentOffsetX += EMOJI_WIDTH;
                            if (currentOffsetX > EMOJI_ATLAS_MAX_WIDTH - EMOJI_WIDTH)
                            {
                                currentOffsetX = 0;
                                currentOffsetY += EMOJI_HEIGHT;

                                if (currentOffsetY > EMOJI_ATLAS_MAX_HEIGHT - EMOJI_HEIGHT)
                                {
                                    // Ran out of space
                                    Debug.LogWarning("Envy MOTD emoji atlas cannot store more emojis, some emojis may be overwritten");
                                    currentOffsetX = 0;
                                    currentOffsetY = 0;
                                }
                            }
                        }
                    }

                    RenderTexture.active = previousRt;
                    Destroy(rt);
                    atlas.Apply(false);

                    foreach (EmojiAsset emoji in emojiTextures.Values.OrderBy(e => e.idStart))
                    {
                        for (int i = 0; i < emoji.textureCount; i++)
                        {
                            if (!emoji.originalTextures[i].valid)
                                continue;

                            emoji.Rebuild(atlas, i);
                            atlasData.spriteGlyphTable.Add(emoji.glyphs[i]);
                            atlasData.spriteCharacterTable.Add(emoji.characters[i]);
                        }
                    }

                    atlasData.UpdateLookupTables();
                    atlasData.material = new Material(Shader.Find("TextMeshPro/Sprite"));
                    atlasData.material.mainTexture = atlas;
                    atlasData.spriteSheet = atlas;

                    status.result = atlasData;
                    motdText.spriteAsset = atlasData;
                }
                finally
                {
                    status.isDone = true;
                }
            }
            #endregion

            public static IEnumerator LoadFallbackImage(Image motdImageComponent)
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

        private const string motdUrl = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/refs/heads/emoji/MOTD.txt";
        private const string fallbackImageUrl = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/image.png";
        private const string discordCdnBaseUrl = "https://cdn.discordapp.com/emojis/";

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
                        motdText.text = "Loading...";

                        motdText.StartCoroutine(ConvertMarkdownToTMP($"<size=24>{motdRawText}</size>", motdText, (formattedText) =>
                        {
                            motdText.text = formattedText;
                            motdImageComponent.gameObject.SetActive(false);
                        }));
                    }
                    else
                    {
                        yield return EmojiManager.LoadFallbackImage(motdImageComponent);
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

            bool regenerateEmojiAtlas = false;
            foreach (System.Text.RegularExpressions.Match match in emojiMatches)
            {
                string emojiId = match.Groups[2].Value;

                // Check the cache for the emoji
                if (!EmojiManager.EmojiExists(emojiId))
                {
                    string emojiUrl = $"{discordCdnBaseUrl}{emojiId}";
                    yield return EmojiManager.LoadEmojiFromURL(emojiUrl, emojiId);

                    regenerateEmojiAtlas = true;
                }

                if (EmojiManager.TryGetEmoji(emojiId, out EmojiAsset emoji))
                {
                    if (emoji.textureCount == 1)
                        input = input.Replace(match.Value, $"<sprite index={emoji.idStart}>");
                    else
                        input = input.Replace(match.Value, $"<sprite anim=\"{emoji.idStart},{emoji.idEnd},{emoji.fps}\">");
                }
                else
                {
                    input = input.Replace(match.Value, "?");
                }
            }

            // If there were new emojis added, the whole sprite
            // atlas must be rebuilt
            if (regenerateEmojiAtlas)
                yield return EmojiManager.RegenerateEmojiAtlas(motdText);

            motdText.spriteAsset = EmojiManager.currentEmojiAtlas;
            motdText.text = input;

            callback(input);
        }
    }
}
