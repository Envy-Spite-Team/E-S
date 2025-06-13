using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnvyLevelLoader
{
    public class AddToPauseMenu : MonoBehaviour
    {
        public void Start()
        {
            this.transform.parent = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "PauseMenu").transform;
        }
    }
}
