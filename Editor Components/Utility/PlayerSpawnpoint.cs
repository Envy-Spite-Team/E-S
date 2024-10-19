using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Events;

namespace EnvyLevelLoader
{
    public class PlayerSpawnpoint : MonoBehaviour
    {
        Transform plr;
        public UnityEvent onSpawn;
        public bool overrideStats = false;
        public float walkSpeed = 750;
        public float jumpPower = 90;
        public float airAccel = 6000;
        public float wallJumpPower = 150;
        public int hp = 100;
        public float antiHP = 0;
        void Awake()
        {
            GameObject targetObject = Addressables.LoadAssetAsync<GameObject>("FirstRoom").WaitForCompletion();
            targetObject = Instantiate(targetObject, transform.position, transform.rotation, transform);
            GameObject.Destroy(targetObject.transform.Find("Room").gameObject);
            plr = targetObject.transform.Find("Player");
            if (plr == null)
                plr = GameObject.Find("Player").transform;

            plr.transform.position = this.transform.position;
            plr.transform.rotation = this.transform.rotation;

            onSpawn.Invoke();

            if (!overrideStats) return;

            NewMovement nm = plr.GetComponent<NewMovement>();
            nm.walkSpeed = walkSpeed;
            nm.jumpPower = jumpPower;
            nm.wallJumpPower = wallJumpPower;
            nm.hp = hp;
            nm.antiHp = antiHP;
        }
        void Update()
        {
            NewMovement nm = plr.GetComponent<NewMovement>();
            nm.walkSpeed = walkSpeed;
            nm.jumpPower = jumpPower;
            nm.wallJumpPower = wallJumpPower;
            nm.hp = hp;
            nm.antiHp = antiHP;
            Destroy(this);
        }
    }
}
