using Steamworks.Ugc;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace EnvyLevelLoader
{
    [System.Serializable]
    public enum CompareTypes
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }
    public class Compare : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent onSuccess;
        public UnityEvent onFailure;

        [Header("Comparison")]
        public Object a;
        public Object b;
        public string property_a;
        public string property_b;
        public CompareTypes operation;

        public void CompareValues()
        {
            object val_a = a.GetType().GetProperty(property_a).GetValue(a);
            object val_b = b.GetType().GetProperty(property_b).GetValue(b);

            Debug.Log(val_a);
            Debug.Log(val_b);
            Debug.Log(val_a == val_b);

            bool state = false;
            switch (operation) {
                case CompareTypes.Equal:
                    state = val_a.Equals(val_b);
                    break;
                case CompareTypes.NotEqual:
                    state = !val_a.Equals(val_b);
                    break;
                case CompareTypes.Greater:
                    state = (double)val_a > (double)val_b;
                    break;
                case CompareTypes.GreaterOrEqual:
                    state = (double)val_a >= (double)val_b;
                    break;
                case CompareTypes.Less:
                    state = (double)val_a < (double)val_b;
                    break;
                case CompareTypes.LessOrEqual:
                    state = (double)val_a <= (double)val_b;
                    break;
            }

            if (state)
                onSuccess.Invoke();
            else
                onFailure.Invoke();
        }
    }
}
