using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Logic;
using System.Reflection;
using System.IO;
using System.Net.Http;
using TMPro;
using JetBrains.Annotations;

namespace DoomahLevelLoader
{
    public class ShowDebugInfo : ICheat
    {
        public string LongName => "Show Debug Info";
        public string Identifier => "envyandspite.showdebuginfo";
        public string ButtonEnabledOverride => "SHOWN";
        public string ButtonDisabledOverride => "HIDDEN";
        public string Icon => "debug";
        public bool IsActive { get; private set; } = false;
        public bool DefaultState => false;
        public StatePersistenceMode PersistenceMode => (StatePersistenceMode)1;

        //public GameObject Plugin.Instance.instantiatedDebug = Plugin.FindObjectEvenIfDisabled("/Canvas/DebugInfo");

        public ShowDebugInfo()
        {

        }

        public void Disable()
        {
            if (Plugin.Instance.instantiatedDebug != null)
                Plugin.Instance.instantiatedDebug.SetActive(false);
            IsActive = false;
        }


        public TextMeshProUGUI BundleText;
        public TextMeshProUGUI FPSText;
        public TextMeshProUGUI EnvyText;
        public TextMeshProUGUI EnemyText;
        public TextMeshProUGUI CheckPointText;
        public TextMeshProUGUI PolyText;
        public TextMeshProUGUI CombatText;
        public TextMeshProUGUI TimeScaleText;
        public GameObject Scripts;

        public int polyTick { get; private set; }
        public int polyCount { get; private set; }

        public void Update()
        {
            if (Plugin.Instance.instantiatedDebug != null)
            {
                if (Plugin.Instance.instantiatedDebug.activeSelf == true)
                {
                    // arcade mode flashbacks --thebluenebula
                    if (Loaderscene.lastUsedBundle)
                        BundleText.text = "BUNDLE: " + Loaderscene.lastUsedBundle.name;

                    FPSText.text = "FPS: " + Mathf.Floor((1.0f / Time.unscaledDeltaTime * 10)) / 10;
                    EnvyText.text = "ENVY: V2.0";

                    TimeScaleText.text = $"TIMESCALE: {Time.timeScale}";

                    Scripts.SetActive(Loaderscene.lastLevelUsesScripts);

                    foreach (CheckPoint checkPoint in UnityEngine.Object.FindObjectsOfType<CheckPoint>())
                        if (checkPoint.activated)
                            CheckPointText.text = checkPoint.gameObject.name;

                    // this code gets the current rendered objects and counts their polys
                    // because of how resource intensive this is i have made it run every 30 frames
                    // --triggered
                    polyTick++;
                    if (polyTick >= 30)
                    {

                        // ok so turns out this lags alot too so its going here :D --triggered
#pragma warning disable CS0618 // shut the fuck up thanks --triggered
                        EnemyIdentifier[] eids = UnityEngine.Object.FindObjectsOfTypeAll(typeof(EnemyIdentifier)) as EnemyIdentifier[];
#pragma warning restore CS0618

                        int enemyCount = 0;
                        int enemyCountMax = 0;
                        Scene s = SceneManager.GetActiveScene();
                        foreach (EnemyIdentifier enemy in eids)
                            if (enemy.gameObject.scene == s)
                            {
                                if (!enemy.dead && enemy.gameObject.activeSelf)
                                    enemyCount++;
                                enemyCountMax++;
                            }

                        EnemyText.text = $"ENEMIES: {enemyCount}/{enemyCountMax}";

                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        polyTick = 0;
                        polyCount = 0;
                        foreach (MeshFilter mf in UnityEngine.Object.FindObjectsOfType<MeshFilter>())
                        {
                            MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
                            if (mr == null) continue;

                            if (mr.isVisible && mf.sharedMesh != null && mf.sharedMesh.isReadable)
                                polyCount += mf.sharedMesh.triangles.Length;
                        }
                    }
                    // turns out polys can be anything made of triangles so triangle count it is :P --triggered
                    PolyText.text = $"TRI COUNT: {polyCount}";
                }
            }
        }

        public void Enable()
        {
            polyTick = 0;
            polyCount = 0;
            if (Plugin.Instance.instantiatedDebug != null)
            {
                Plugin.Instance.instantiatedDebug.SetActive(true);

                // gotta love how nebula put this in update --triggered
                Transform information = Plugin.Instance.instantiatedDebug.transform.Find("Information").transform;
                BundleText = information.Find("Bundle").GetComponent<TextMeshProUGUI>();
                FPSText = information.Find("FPS").GetComponent<TextMeshProUGUI>();
                EnvyText = information.Find("EnvyVersion").GetComponent<TextMeshProUGUI>();
                EnemyText = information.Find("Enemies").GetComponent<TextMeshProUGUI>();
                CheckPointText = information.Find("Checkpoint").GetComponent<TextMeshProUGUI>();
                PolyText = information.Find("PolyCount").GetComponent<TextMeshProUGUI>();
                CombatText = information.Find("Wave").GetComponent<TextMeshProUGUI>();
                TimeScaleText = information.Find("Timescale").GetComponent<TextMeshProUGUI>();
                Scripts = information.Find("Scripts").gameObject;
            }
            IsActive = true;
        }
    }
}
