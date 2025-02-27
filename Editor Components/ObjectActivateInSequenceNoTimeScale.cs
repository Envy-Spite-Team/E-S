using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnvyLevelLoader.UnityComponents
{
    // literally just the games version but it can work in pause menus
    public class ObjectActivateInSequenceNoTimeScale : MonoBehaviour
    {
        public GameObject[] objectsToActivate;
        public Coroutine coroutine;
        public float delay;
        public AudioSource aud;

        public void Awake() => this.aud = this.GetComponent<AudioSource>();

        public void OnEnable()
        {
            foreach (GameObject gameObject in this.objectsToActivate)
                gameObject.SetActive(false);
            this.coroutine = this.StartCoroutine(this.activationCoroutine());
        }

        public void OnDisable()
        {
            if (this.coroutine == null)
                return;
            this.StopCoroutine(this.coroutine);
        }

        public IEnumerator activationCoroutine()
        {
            int i = 0;
            while (i < this.objectsToActivate.Length)
            {
                this.objectsToActivate[i].SetActive(true);
                ++i;
                if (this.aud != null)
                    this.aud.Play();
                yield return new WaitForSecondsRealtime(this.delay);
            }
        }
    }
}