using UnityEngine;
using DarkHome;

namespace DarkHome
{
    public class LocalizationTest : MonoBehaviour
    {
        void Start()
        {
            // Test 1: Get existing key
            string greeting = LocalizationManager.Instance.GetText("DIALOGUE_KAI_GREET_START");
            Debug.Log($"[TEST 1] Greeting: {greeting}");

            // Test 2: Get item name
            string itemName = LocalizationManager.Instance.GetText("ITEM_C1_FAMILYPHOTO_name");
            Debug.Log($"[TEST 2] Item: {itemName}");

            // Test 3: Missing key (should show warning)
            string missing = LocalizationManager.Instance.GetText("FAKE_KEY");
            Debug.Log($"[TEST 3] Missing: {missing}");
        }
    }
}