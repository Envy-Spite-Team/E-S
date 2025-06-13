using System;
using EnvyLevelLoader;
using UnityEngine;

namespace DoomahLevelLoader.UnityComponents
{
    public class Blocker : AddressableReplacer
    {
        [Header("THIS IS A LEGACY COMPONENT DO NOT USE")]
        public string a;

        protected override void PostInstantiate(GameObject instantiatedObject)
        {
            BoxCollider component = base.GetComponent<BoxCollider>();
            bool flag = component != null;
            if (flag)
            {
                BoxCollider boxCollider = instantiatedObject.AddComponent<BoxCollider>();
                boxCollider.center = component.center;
                boxCollider.size = new Vector3(component.size.z, component.size.y, component.size.x);
                Quaternion quaternion = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + 90f, 0f);
                instantiatedObject.transform.rotation = quaternion;
                instantiatedObject.AddComponent<BlockerUpdater>();
            }
            else
            {
                Debugger.LogWarn("Original object does not have a BoxCollider component.");
            }
        }
    }
}