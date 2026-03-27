using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [System.Serializable]
    public class PoolData
    {
        public string key; // hoặc int/enum tùy bạn
        public GameObject prefab;
        public Transform container;
        public List<GameObject> pool = new();
    }
}