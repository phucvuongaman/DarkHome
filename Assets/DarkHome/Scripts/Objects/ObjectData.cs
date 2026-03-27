using System;
using UnityEngine;

namespace DarkHome
{
    [Serializable]
    public class ObjectData
    {
        public string objId;
        public Vector3 position;
        public Quaternion rotation;
        public bool isActive;
    }
}