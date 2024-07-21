using System;
using System.Threading.Tasks;
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

        public async void Awake()
        {
            await Task.Delay(150);
            try
            {
                TipBox = Plugin.FindObjectEvenIfDisabled("FirstRoom(Clone)", "Room/Shop/Canvas/TipBox/Panel/TipText");
            }
            catch(Exception)
            {
                Debug.Log("This level does not have Tip of the Day setup correctly, please make sure you are using the addressable replacer version of the FirstRoom (the one with no children inside of it, do not take that out of context please) Attempting to deploy temporary fix.");
                TipBox = Plugin.FindObjectEvenIfDisabled("FirstRoom", "Room/Shop/Canvas/TipBox/Panel/TipText");
            }

            if (TipBox == null)
            {
                Debug.Log("Temporary fix failed.");
            }
            else
            {
                TipBox.GetComponent<TextMeshProUGUI>().text = Tip;
            }
        }
    }
}