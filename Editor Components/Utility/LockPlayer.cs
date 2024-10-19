using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnvyLevelLoader
{
    public class LockPlayer : MonoBehaviour
    {
        public bool includeScale = false;
        public bool inverse = false;
        public void Update()
        {
            if (inverse)
            {
                try
                {
                    transform.position = NewMovement.Instance.transform.position;
                    transform.rotation = NewMovement.Instance.transform.rotation;
                    if (includeScale)
                        transform.localScale = NewMovement.Instance.transform.localScale;
                }
                catch (Exception e) { }
            }
            try
            {
                NewMovement.Instance.transform.position = transform.position;
                NewMovement.Instance.transform.rotation = transform.rotation;
                if (includeScale)
                    NewMovement.Instance.transform.localScale = transform.localScale;
            }
            catch (Exception e) { }
        }
    }
}
