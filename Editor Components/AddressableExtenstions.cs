using System.IO;
using EnvyLevelLoader;
using EnvyLevelLoader.Loaders;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoomahLevelLoader.UnityComponents
{
    [RequireComponent(typeof(MeshRenderer))]
    public class AddressableMaterials : MonoBehaviour
    {
        public string[] mirrorMaterialsAddresableList;
        public bool swapOnAwake = true;

        public void Awake()
        {
            if(swapOnAwake)
                Swap();
        }
        
        public void Swap()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            for (int i = 0; i < mirrorMaterialsAddresableList.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(mirrorMaterialsAddresableList[i]))
                    continue;
                int myIdx = i + 0;
                Addressables.LoadAssetAsync<Material>(mirrorMaterialsAddresableList[i]).Completed += (handle =>
                {
                    EnvyUtility.RunOnMainThread((() =>
                    {
                        if (handle.Result != null)
                        {
                            var list = mr.materials;
                            list[myIdx] = handle.Result;
                            mr.materials = list;
                        }
                    }));
                });
            }
        }
    }
    [RequireComponent(typeof(AudioSource))]
    public class AddressableSound : MonoBehaviour
    {
        public AudioSource target;
        public string soundAddress;
        public bool swapOnAwake = true;

        public void Awake()
        {
            if(swapOnAwake)
                Swap();
        }
        public void Swap()
        {
            target = target ?? GetComponent<AudioSource>();

            Addressables.LoadAssetAsync<AudioClip>(ResourceLoader.FindKeyFromFileNameNoExt(soundAddress)).Completed += (handle =>
            {
                target.clip = handle.Result;
                if(target?.playOnAwake ?? false)
                    target?.Play();
            });
        }
    }
    [RequireComponent(typeof(MeshFilter))]
    public class AddressableMesh : MonoBehaviour
    {
        public string meshAddress;
        public bool swapOnAwake = true;

        public void Awake()
        {
            if(swapOnAwake)
                Swap();
        }
        
        public void Swap()
        {
            var target = GetComponent<MeshFilter>();
            Addressables.LoadAssetAsync<Mesh>(ResourceLoader.FindKeyFromFileNameNoExt(meshAddress)).Completed += (handle =>
            {
                target.sharedMesh = handle.Result;
            });
        }
    }
}
