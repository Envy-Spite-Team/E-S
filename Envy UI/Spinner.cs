using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnvyLevelLoader.UI
{
    public class Spinner : MonoBehaviour
    {
        public Vector3 axis = new Vector3(0,0,1);
        public float speed = 64f;
        public void Update()
        {
            this.transform.Rotate(axis, speed * Time.deltaTime);
        }
    }
}