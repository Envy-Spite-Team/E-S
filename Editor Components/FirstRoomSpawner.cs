using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using EnvyLevelLoader;
using EnvyLevelLoader.Loaders;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

// warning : i removed the component in the EnvyLevelLoader namespace because i don't think any level made so far has an EnvyLevelLoader namespace referenced

namespace DoomahLevelLoader.UnityComponents
{
    public class FirstRoomSpawner : MonoBehaviour
    {
        public enum FirstRoomType
        {
            Normal,
            Secret,
            Prime,
            Encore,
            Pit
        }

        public static string FirstRoomTypeToAddress(FirstRoomType firstRoomType)
        {
            switch (firstRoomType)
            {
                case FirstRoomType.Normal:
                    return "FirstRoom";
                case FirstRoomType.Secret:
                    return "FirstRoom Secret";
                case FirstRoomType.Prime:
                    return "FirstRoom Prime";
                case FirstRoomType.Encore:
                    return "Assets/Prefabs/Levels/Special Rooms/FirstRoom Encore.prefab";
                case FirstRoomType.Pit:
                    return "FirstRoom Pit";
            }
            return "FirstRoom";
        }
        
        public FirstRoomType firstRoomType = FirstRoomType.Normal;
        [Header("Do NOT enable 'parentToThis' AND 'destroyThis' at the same time")]
        public bool parentToThis = false;
        public bool destroyThis = false;
        public UltrakillEvent onSpawn;
        [Tooltip("Passes the gameobject of the room spawned to any function called.")]
        public UnityEvent<GameObject> onSpawnSpecial;
        [Header("You almost always want this on!")]
        public bool autoSpawn = true;
        [Space(10)]
        [Header("Player Stats")]
        public bool overrideStats = false;
        public float walkSpeed = 750;
        public float jumpPower = 90;
        public float airAccel = 6000;
        public float wallJumpPower = 150;
        public int hp = 100;
        public float antiHP = 0;
        
        public GameObject LoadRoomAtAddress(string address)
        {
            GameObject targetObject;
            if (ResourceLoader.IsGameobjectPreloaded(address))
            {
                targetObject = ResourceLoader.LoadGameobjectAtAddress(address);
            }
            else
            {
                targetObject = Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            }
            GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);
            if (!parentToThis)
            {
                instantiatedObject.transform.SetParent(null, true);
            }
            if (destroyThis)
            {
                Destroy(this.gameObject);
            }

            if (overrideStats)
            {
                NewMovement nm = instantiatedObject.GetComponentInChildren<NewMovement>();
                if (nm == null) nm = NewMovement.Instance;
                
                if (nm != null)
                {
                    nm.walkSpeed = walkSpeed;
                    nm.jumpPower = jumpPower;
                    nm.wallJumpPower = wallJumpPower;
                    nm.hp = hp;
                    nm.antiHp = antiHP;
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        nm.walkSpeed = walkSpeed;
                        nm.jumpPower = jumpPower;
                        nm.wallJumpPower = wallJumpPower;
                        nm.hp = hp;
                        nm.antiHp = antiHP;
                    }, 0.125f);
                }
                else
                {
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        nm = NewMovement.Instance;
                        if (nm == null) return;
                        nm.walkSpeed = walkSpeed;
                        nm.jumpPower = jumpPower;
                        nm.wallJumpPower = wallJumpPower;
                        nm.hp = hp;
                        nm.antiHp = antiHP;
                    }, 0.125f);
                }
            }
            
            onSpawn?.Invoke();
            onSpawnSpecial?.Invoke(instantiatedObject);
            return instantiatedObject;
        }
        
        public void Awake()
        {
            if(!autoSpawn) return;
            SpawnRoom();
        }

        public void SpawnRoom()
        {
            Debugger.Log("Spawning first room via FirstRoomSpawner");
            LoadRoomAtAddress(FirstRoomTypeToAddress(firstRoomType));
        }
    }
}