using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using UnityEngine.AddressableAssets;
using System;

namespace DoomahLevelLoader
{
    public class LevelUpdatePopUp : MonoBehaviour
    {
        private static LevelUpdatePopUp instance;
        public static byte[] byteCheck;
        public static bool UpdateReady;
        public static Scene currentScene;

        public static LevelUpdatePopUp Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LevelUpdatePopUp>();
                    if (instance == null)
                        Debugger.LogError("bongbong bepinex log infestiation"); //what the hell? --triggered
                }
                return Instance;
            }
        }

        public static GameObject GetInstanceObject()
        {
            return Instance.gameObject;
        }
    }
}