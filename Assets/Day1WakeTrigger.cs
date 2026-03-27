using UnityEngine;
using DarkHome;



namespace Name
{
    public class Day1WakeTrigger : MonoBehaviour
    {
        private bool triggered = false;
        void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;
            FlagManager.Instance?.AddFlag(new FlagData("FLAG_DAY1_STARTED", EFlagScope.Local));
            Debug.Log("✅ FLAG_DAY1_STARTED fired!");
            triggered = true;
        }
    }

}
