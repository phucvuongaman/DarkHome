using System;
using UnityEngine;

namespace DarkHome
{
    [Serializable]
    public class PatrolZone
    {
        public string zoneName;
        public Transform centerPoint;
        public float patrolRadius; // Bán kính từ điểm giữa (centerPoint)
    }
}