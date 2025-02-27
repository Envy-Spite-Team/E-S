using System;
using System.Collections;
using UnityEngine;

namespace EnvyLevelLoader.UI
{
    public class Mover : MonoBehaviour
    {
        public Vector3 dir;
        public float speed;

        public void Update()
        {
            transform.position += dir * (speed * Time.unscaledDeltaTime);
        }
    }
}