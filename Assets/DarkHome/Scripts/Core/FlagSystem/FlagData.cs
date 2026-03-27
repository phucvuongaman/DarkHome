using System;
using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Scope của flag - Local (runtime only) hoặc Global (persistent)
    /// </summary>
    public enum EFlagScope
    {
        Local,   // Runtime only, không save
        Global   // Persistent, được save vào file
    }

    /// <summary>
    /// Flag data structure - Stores flag ID and scope
    /// </summary>
    [Serializable]
    public class FlagData
    {
        public string FlagID;
        public EFlagScope Scope;

        public FlagData(string id, EFlagScope scope)
        {
            FlagID = id;
            Scope = scope;
        }

        public override bool Equals(object obj)
        {
            if (obj is FlagData other)
            {
                return this.FlagID == other.FlagID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return FlagID != null ? FlagID.GetHashCode() : 0;
        }
    }
}