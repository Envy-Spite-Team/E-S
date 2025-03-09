using System;
using EnvyLevelLoader;
using UnityEngine;

namespace DoomahLevelLoader.UnityComponents
{
    public class PlayerStart : MonoBehaviour
    {
        [Header("THIS IS A LEGACY COMPONENT DO NOT USE")]
        public string a;
        
        private void Awake()
        {
            bool flag = base.transform != null;
            if (flag)
            {
                MonoSingleton<NewMovement>.Instance.transform.position = base.transform.position;
                TeleportPlayer tp = gameObject.AddComponent<TeleportPlayer>();
                tp.notRelative = true;
                tp.affectPosition = true;
                tp.objectivePosition = base.transform.position;
                Invoke(nameof(Tp), 0.125f);
                UnityEngine.Object.Destroy(base.gameObject, 0.5f);
            }
            else
            {
                Debugger.LogError("NewMovement script instance or transform is null.");
            }
        }

        private void Tp()
        {
            MonoSingleton<NewMovement>.Instance.transform.position = base.transform.position;
            TeleportPlayer tp = gameObject.AddComponent<TeleportPlayer>();
            tp.notRelative = true;
            tp.affectPosition = true;
            tp.objectivePosition = base.transform.position;
            GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prim.GetComponent<BoxCollider>().isTrigger = true;
            prim.GetComponent<MeshRenderer>().enabled = false;
            prim.AddComponent<PlayerActivator>();
            prim.transform.position = base.transform.position;
        }
    }
}