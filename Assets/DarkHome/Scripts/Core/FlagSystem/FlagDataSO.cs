using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "FlagSO", menuName = "SO/Flag/Flags")]
    public class FlagDataSO : ScriptableObject
    {
        public List<FlagData> Flags;
    }
}