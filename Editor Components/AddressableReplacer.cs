using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Reflection;
using EnvyLevelLoader;
using EnvyLevelLoader.Loaders;
using UnityEngine.ResourceManagement.AsyncOperations;

// warning : i removed the component in EnvyLevelLoader because i don't think any level made so far has an envylevelloader namespace referenced

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
        
        [HideInInspector]
        public bool usePropertyOverrides = false;
        [HideInInspector]
        public Dictionary<string, object> PropertyOverrides = new Dictionary<string, object>();
        
        internal EnemyIdentifier eid;

        private void OnEnable()
        {
            Activate();
        }

        public static string FixAddress(string address)
        {
            string newAddress = address;
            if (address.Contains("StatueEnemy"))
            {
                newAddress = "Assets/Prefabs/Enemies/Cerberus.prefab";
            }
            else if (address.Contains("StatueFake"))
            {
                newAddress = "Assets/Prefabs/Enemies/CerberusStatue.prefab";
            }
            Debugger.Log($"{address} -> {newAddress}");
            return newAddress;
        }

        private bool _activated = false;

        private void HandleInstantiatedObject(GameObject instantiatedObject)
        {
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

            if (eid != null && usePropertyOverrides)
            {
                void ApplyPropertyOverrides()
                {
                    Type t = eid.GetType();
                    foreach (var key in PropertyOverrides.Keys)
                    {
                        object value = PropertyOverrides[key];
                        if (key == "health")
                        {
                            float? maybeHp = value as float?;
                            if(maybeHp == null)
                                continue;
                            float hp = maybeHp.Value;
                            
                            if (eid.enemyType == EnemyType.Drone || eid.enemyType == EnemyType.Virtue)
                            {
                                if (!(bool) (UnityEngine.Object) eid.drone)
                                    eid.drone = this.GetComponent<Drone>();
                                if (!(bool) (UnityEngine.Object) eid.drone)
                                    continue;
                                eid.drone.health = hp;
                            }
                            else if (eid.enemyType == EnemyType.MaliciousFace)
                            {
                                if (!(bool) (UnityEngine.Object) eid.spider)
                                    eid.spider = this.GetComponent<SpiderBody>();
                                if (!(bool) (UnityEngine.Object) eid.spider)
                                    continue;
                                eid.spider.health = hp;
                            }
                            else
                            {
                                switch (eid.enemyClass)
                                {
                                    case EnemyClass.Husk:
                                        if (!(bool) (UnityEngine.Object) eid.zombie)
                                            eid.zombie = this.GetComponent<Zombie>();
                                        if (!(bool) (UnityEngine.Object) eid.zombie)
                                            break;
                                        eid.zombie.health = hp;
                                        break;
                                    case EnemyClass.Machine:
                                        if (!(bool) (UnityEngine.Object) eid.machine)
                                            eid.machine = this.GetComponent<Machine>();
                                        if (!(bool) (UnityEngine.Object) eid.machine)
                                            break;
                                        eid.machine.health = hp;
                                        break;
                                    case EnemyClass.Demon:
                                        if (!(bool) (UnityEngine.Object) eid.statue)
                                            eid.statue = this.GetComponent<Statue>();
                                        if (!(bool) (UnityEngine.Object) eid.statue)
                                            break;
                                        eid.statue.health = hp;
                                        break;
                                }
                            }
                            eid.ForceGetHealth();
                            continue;
                        }
                        FieldInfo m = t.GetField(key);
                        if (m == null)
                        {
                            Debugger.LogWarn("Bad property override on addressable replacer ( '"+key+"' isn't a field)");
                            continue;
                        }

                        try
                        {
                            m.SetValue(eid, value);
                        }
                        catch (ArgumentException e)
                        {
                            Debugger.LogError($"Value: {value} [{value.GetType().FullName}] doesn't match {m.FieldType.FullName}");
                        }
                    }
                }

                ApplyPropertyOverrides();
                EnvyUtility.RunOnMainThread(ApplyPropertyOverrides, 0.125f);
            }

            if (destroyThis)
            {
                Destroy(gameObject);
                gameObject.SetActive(false);
            }

            enabled = false;
        }
        
        public void Activate()
        {
            targetAddress = FixAddress(targetAddress);
            Debugger.Log("HIIIIIIII SDASD ASD " + transform.name + "-> " + targetAddress);
            if (oneTime && _activated)
                return;

            _activated = true;

            if (ResourceLoader.IsGameobjectPreloaded(targetAddress))
            {
                GameObject targetObject = ResourceLoader.LoadGameobjectAtAddress(targetAddress);
                
                GameObject instantiatedObject = Instantiate(targetObject, transform.position, transform.rotation, transform);
                HandleInstantiatedObject(instantiatedObject);
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
                HandleInstantiatedObject(instantiatedObject);
            };
        }
        protected virtual void PostInstantiate(GameObject instantiatedObject) { }
    }
}