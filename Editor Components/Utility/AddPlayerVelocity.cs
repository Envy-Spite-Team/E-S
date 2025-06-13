using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnvyLevelLoader
{
    public class AddPlayerVelocity : MonoBehaviour
    {
        public bool FireOnce = true;
        public bool ManualOnly = false;
        public Vector3 force = Vector3.zero;
        public void Update()
        {
            if (ManualOnly) return;

            if (!FireOnce)
                NewMovement.Instance.rb.AddForce(force);
        }

        public void OnEnable()
        {
            if (ManualOnly) return;

            NewMovement.Instance.rb.AddForce(force);
        }

        public void ManualFire()
        {
            NewMovement.Instance.rb.AddForce(force);
        }
    }
}