using System;
using UnityEngine;


namespace DarkHome
{
    [Serializable]
    public class NpcData
    {
        public string npcId;
        public Vector3 position;
        public Quaternion rotation;
        public bool isActive;
    }
}