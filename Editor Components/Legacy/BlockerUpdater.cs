using System;
using UnityEngine;

namespace DoomahLevelLoader.UnityComponents
{
    public class BlockerUpdater : MonoBehaviour
    {
        [Header("THIS IS A LEGACY COMPONENT DO NOT USE")]
        public string a;

        private void Update()
        {
            this.timer += Time.deltaTime;
            bool flag = this.timer >= this.checkInterval;
            if (flag)
            {
                this.timer = 0f;
                this.CheckEnemies();
            }
        }

        private void CheckEnemies()
        {
            bool flag = true;
            Transform parent = base.transform.parent;
            bool flag2 = parent != null;
            if (flag2)
            {
                foreach (object obj in parent)
                {
                    Transform transform = (Transform)obj;
                    bool flag3 = transform.GetComponent<WaveComponent>() != null;
                    if (!flag3)
                    {
                        EnemyIdentifier component = transform.GetComponent<EnemyIdentifier>();
                        bool flag4 = component != null && !component.dead;
                        if (flag4)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
            bool flag5 = flag && parent != null;
            if (flag5)
            {
                foreach (object obj2 in parent)
                {
                    Transform transform2 = (Transform)obj2;
                    bool flag6 = transform2.parent != null && transform2.parent.GetComponent<WaveComponent>() != null;
                    if (!flag6)
                    {
                        foreach (object obj3 in transform2)
                        {
                            Transform transform3 = (Transform)obj3;
                            EnemyIdentifier component2 = transform3.GetComponent<EnemyIdentifier>();
                            bool flag7 = component2 != null && !component2.dead;
                            if (flag7)
                            {
                                flag = false;
                                break;
                            }
                        }
                        bool flag8 = !flag;
                        if (flag8)
                        {
                            break;
                        }
                    }
                }
            }
            bool flag9 = flag;
            if (flag9)
            {
                base.gameObject.SetActive(false);
            }
        }

        private float timer = 0f;

        private float checkInterval = 0.2f;
    }
}