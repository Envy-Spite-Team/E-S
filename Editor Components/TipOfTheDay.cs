using System;
using System.Threading.Tasks;
using EnvyLevelLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    public class TipOfTheDay : MonoBehaviour
    {
        public string Tip;

        [HideInInspector]
        public GameObject TipBox;

        public void Awake()
        {
            this.gameObject.AddComponent<DoomahLevelLoader.UnityComponents.TipOfTheDay>().Tip = this.Tip;
        }
    }
}

namespace DoomahLevelLoader.UnityComponents
{
    public class TipOfTheDay : MonoBehaviour
    {
        public string Tip;

        [HideInInspector]
        public GameObject TipBox;

        public void Awake()
        {
            Debugger.Log("TipOfTheDay Awake");
            StockMapInfo mapInfo = StockMapInfo.Instance;
            if (mapInfo?.tipOfTheDay == null)
            {
                mapInfo!.tipOfTheDay = ScriptableObject.CreateInstance<ScriptableObjects.TipOfTheDay>();
                mapInfo!.tipOfTheDay.tip = this.Tip;
            }
            
            EnvyUtility.RunOnMainThread(() =>
            {
                try
                {
                    TipBox = EnvyUtility.FindObjectEvenIfDisabled("FirstRoom(Clone)", "Room/Shop/Canvas/TipBox/Panel/TipText");
                }
                catch(Exception)
                {
                    Debugger.LogError("This level does not have Tip of the Day setup correctly, please make sure you are using the addressable replacer version of the FirstRoom (the one with no children inside of it, do not take that out of context please) Attempting to deploy temporary fix.");
                    TipBox = EnvyUtility.FindObjectEvenIfDisabled("FirstRoom", "Room/Shop/Canvas/TipBox/Panel/TipText");
                }

                if (TipBox == null)
                {
                    ShopZone[] shopZones = GameObject.FindObjectsOfType<ShopZone>();
                    foreach (ShopZone shopZone in shopZones)
                    {
                        if (shopZone.tipOfTheDay != null)
                        {
                            shopZone.tipOfTheDay.text = Tip;
                        }
                    }
                
                    Debugger.LogWarn("(TOTD) Temporary fix failed.");
                }
                else
                {
                    TipBox.GetComponent<TextMeshProUGUI>().text = Tip;
                }
            }, 0.25f);
        }
    }
}