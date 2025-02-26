using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using EnvyLevelLoader;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EnvyLevelLoader.UnityComponents
{
    public class AddressableReplacer : MonoBehaviour
    {
        public string targetAddress;
        public bool oneTime = true;
        public bool moveToParent = true;
        public bool destroyThis = true;
        public bool IsBoss = false;
        public string BossName;
        public bool IsSanded = false;
        public bool IsPuppet = false;
        public bool IsRadient = false;
        public float RadienceTier;
        public float DamageTier;
        public float SpeedTier;
        public float HealthTier;

        internal EnemyIdentifier eid;

        private void OnEnable()
        {
            Debugger.Log("HIIIIIIII SDASD ASD " + transform.name + "-> " + targetAddress);
            Activate();
        }

        private bool _activated = false;

        public void Activate()
        {
            Debugger.Log("HIIIIIIII SDASD ASD " + transform.name + "-> " + targetAddress);
            if (oneTime && _activated)
                return;

            _activated = true;

            if (targetAddress == "FirstRoom" || targetAddress == "FirstRoom Player Only" || targetAddress == "Player_2" || targetAddress == "FirstRoom Spawner")
            {
                GameObject targetObject;
                if (targetAddress == "FirstRoom" || targetAddress == "FirstRoom Spawner")
                    targetObject = Plugin.FirstRoomTemp;
                else if (targetAddress == "FirstRoom Player Only" || targetAddress == "Player_2")
                    targetObject = Plugin.PlayerTemp;
                else
                {
                    Debugger.LogError("Broken addressable specials wtf");
                    return;
                }
                
                GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);
                
                if (moveToParent)
                    instantiatedObject.transform.SetParent(transform.parent, true);

                PostInstantiate(instantiatedObject);
                
                if (destroyThis)
                {
                    Destroy(gameObject);
                    gameObject.SetActive(false);
                }

                enabled = false;
                return;
            }
            
            var task = Addressables.LoadAssetAsync<GameObject>(targetAddress);
            task.Completed += (a) =>
            {
                GameObject targetObject = a.Result;
                if (targetObject == null || a.Status != AsyncOperationStatus.Succeeded)
                {
                    Debugger.LogWarn($"Tried to load asset at address {targetAddress}, but it does not exist");
                    enabled = false;
                    return;
                }

                GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);

                eid = instantiatedObject.GetComponent<EnemyIdentifier>();

                // If eid is still null, try getting the component from the first child
                if (eid == null && instantiatedObject.transform.childCount > 0)
                {
                    eid = instantiatedObject.transform.GetChild(0).GetComponent<EnemyIdentifier>();
                }

                if (moveToParent)
                    instantiatedObject.transform.SetParent(transform.parent, true);

                PostInstantiate(instantiatedObject);

                if (eid != null && IsBoss)
                {
                    BossHealthBar bossHealthBar = eid.gameObject.AddComponent<BossHealthBar>();
                    if (!string.IsNullOrEmpty(BossName))
                    {
                        bossHealthBar.bossName = BossName;
                    }
                }

                if (eid != null && IsSanded)
                    eid.Sandify(false);

                if (eid != null && IsPuppet)
                {
                    eid.PuppetSpawn();
                    eid.puppet = true;
                }

                if (eid != null && IsRadient)
                {
                    eid.radianceTier = RadienceTier;
                    eid.healthBuffModifier = HealthTier;
                    eid.speedBuffModifier = SpeedTier;
                    eid.damageBuffModifier = DamageTier;
                    eid.BuffAll();
                }

                if (destroyThis)
                {
                    Destroy(gameObject);
                    gameObject.SetActive(false);
                }

                enabled = false;
            };
        }
        protected virtual void PostInstantiate(GameObject instantiatedObject) { }
    }
}
namespace DoomahLevelLoader.UnityComponents
{
    public class AddressableReplacer : MonoBehaviour
    {
        public string targetAddress;
        public bool oneTime = true;
        public bool moveToParent = true;
        public bool destroyThis = true;
        public bool IsBoss = false;
        public string BossName;
        public bool IsSanded = false;
        public bool IsPuppet = false;
        public bool IsRadient = false;
        public float RadienceTier;
        public float DamageTier;
        public float SpeedTier;
        public float HealthTier;

        internal EnemyIdentifier eid;

        private void OnEnable()
        {
            Debugger.Log("HIIIIIIII SDASD ASD " + transform.name + "-> " + targetAddress);
            Activate();
        }

        private bool _activated = false;

        public void Activate()
        {
            Debugger.Log("HIIIIIIII SDASD ASD " + transform.name + "-> " + targetAddress);
            if (oneTime && _activated)
                return;

            _activated = true;

            if (targetAddress == "FirstRoom" || targetAddress == "FirstRoom Player Only" || targetAddress == "Player_2" || targetAddress == "FirstRoom Spawner")
            {
                GameObject targetObject;
                if (targetAddress == "FirstRoom" || targetAddress == "FirstRoom Spawner")
                    targetObject = Plugin.FirstRoomTemp;
                else if (targetAddress == "FirstRoom Player Only" || targetAddress == "Player_2")
                    targetObject = Plugin.PlayerTemp;
                else
                {
                    Debugger.LogError("Broken addressable specials wtf");
                    return;
                }
                
                GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);
                
                if (moveToParent)
                    instantiatedObject.transform.SetParent(transform.parent, true);

                PostInstantiate(instantiatedObject);
                
                if (destroyThis)
                {
                    Destroy(gameObject);
                    gameObject.SetActive(false);
                }

                enabled = false;
                return;
            }
            
            var task = Addressables.LoadAssetAsync<GameObject>(targetAddress);
            task.Completed += (a) =>
            {
                GameObject targetObject = a.Result;
                if (targetObject == null || a.Status != AsyncOperationStatus.Succeeded)
                {
                    Debugger.LogWarn($"Tried to load asset at address {targetAddress}, but it does not exist");
                    enabled = false;
                    return;
                }

                GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);

                eid = instantiatedObject.GetComponent<EnemyIdentifier>();

                // If eid is still null, try getting the component from the first child
                if (eid == null && instantiatedObject.transform.childCount > 0)
                {
                    eid = instantiatedObject.transform.GetChild(0).GetComponent<EnemyIdentifier>();
                }

                if (moveToParent)
                    instantiatedObject.transform.SetParent(transform.parent, true);

                PostInstantiate(instantiatedObject);

                if (eid != null && IsBoss)
                {
                    BossHealthBar bossHealthBar = eid.gameObject.AddComponent<BossHealthBar>();
                    if (!string.IsNullOrEmpty(BossName))
                    {
                        bossHealthBar.bossName = BossName;
                    }
                }

                if (eid != null && IsSanded)
                    eid.Sandify(false);

                if (eid != null && IsPuppet)
                {
                    eid.PuppetSpawn();
                    eid.puppet = true;
                }

                if (eid != null && IsRadient)
                {
                    eid.radianceTier = RadienceTier;
                    eid.healthBuffModifier = HealthTier;
                    eid.speedBuffModifier = SpeedTier;
                    eid.damageBuffModifier = DamageTier;
                    eid.BuffAll();
                }

                if (destroyThis)
                {
                    Destroy(gameObject);
                    gameObject.SetActive(false);
                }

                enabled = false;
            };
        }
        protected virtual void PostInstantiate(GameObject instantiatedObject) { }
    }
}