using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnvyLevelLoader
{
    public class LockCamera : MonoBehaviour
    {
        public bool inverse = false;
        public void Update()
        {
            if (inverse)
            {
                try
                {
                    transform.position = Camera.main.transform.position;
                    transform.rotation = Camera.main.transform.rotation;
                }
                catch (Exception e)
                {
                    try
                    {
                        transform.position = Camera.current.transform.position;
                        transform.rotation = Camera.current.transform.rotation;
                    }
                    catch (Exception ee)
                    {
                        transform.position = CameraController.Instance.cam.transform.position;
                        transform.rotation = CameraController.Instance.cam.transform.rotation;
                    }
                }
                return;
            }
            try
            {
                Camera.main.transform.position = transform.position;
                Camera.main.transform.rotation = transform.rotation;
            }
            catch (Exception e)
            {
                try
                {
                    Camera.current.transform.position = transform.position;
                    Camera.current.transform.rotation = transform.rotation;
                }
                catch (Exception ee)
                {
                    CameraController.Instance.cam.transform.position = transform.position;
                    CameraController.Instance.cam.transform.rotation = transform.rotation;
                }
            }
        }
    }
}
