using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnvyLevelLoader
{
    [System.Serializable]
    public struct MaterialProperty
    {
        public string name;
        public int int_value;
        public float float_value;
        public Color color_value;
        public Vector4 vector4_value;
        public Texture2D texture_value;
    }
    [RequireComponent(typeof(MeshRenderer))]
    public class LocalMaterialProperties : MonoBehaviour
    {
        public List<MaterialProperty> properties;
        public bool auto = true;
        public bool remove_ondisable = true;
        void Start()
        {
            if (auto) SetValues();
        }
        public void SetValues()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            foreach (MaterialProperty p in properties) {
                mpb.SetInt(p.name, p.int_value);
                mpb.SetFloat(p.name, p.float_value);
                mpb.SetColor(p.name, p.color_value);
                mpb.SetVector(p.name, p.vector4_value);
                mpb.SetTexture(p.name, p.texture_value);
            }
            mr.SetPropertyBlock(mpb);
        }
        public void ResetValues()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(null);
        }
        void OnDisable()
        {
            if(remove_ondisable) ResetValues();
        }
    }
}
