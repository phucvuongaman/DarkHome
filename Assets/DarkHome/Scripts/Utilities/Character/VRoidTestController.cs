using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Test script for VRoid facial expressions and look system
    /// Press keyboard keys to test different expressions and targets
    /// </summary>
    public class VRoidTestController : MonoBehaviour
    {
        [Header("Test Target")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterLookController _lookController;

        [Header("Test Targets for Looking")]
        [SerializeField] private Transform _testTarget1;
        [SerializeField] private Transform _testTarget2;

        [Header("Current State")]
        [SerializeField] private string _currentExpression = "Neutral";
        [SerializeField] private string _currentLookTarget = "None";

        private void Start()
        {
            // Auto-find components if not assigned
            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (_lookController == null)
                _lookController = GetComponent<CharacterLookController>();

            // Auto-find camera as test target 1
            if (_testTarget1 == null && Camera.main != null)
                _testTarget1 = Camera.main.transform;

            Debug.Log("=== VROID TEST CONTROLLER ===");
            Debug.Log("Facial Expressions:");
            Debug.Log("  1 = Neutral");
            Debug.Log("  2 = Happy");
            Debug.Log("  3 = Sad");
            Debug.Log("  4 = Angry");
            Debug.Log("  5 = Surprised");
            Debug.Log("\nLook Targets:");
            Debug.Log("  Q = Look at Target 1 (Camera)");
            Debug.Log("  W = Look at Target 2");
            Debug.Log("  E = Clear target (stop looking)");
            Debug.Log("\nToggle Components:");
            Debug.Log("  H = Toggle Head Tracking");
            Debug.Log("  Y = Toggle Eye Tracking");
            Debug.Log("  C = Simulate Cutscene (disable all)");
            Debug.Log("  V = End Cutscene (re-enable)");
        }

        private void Update()
        {
            HandleFacialExpressionInput();
            HandleLookTargetInput();
            HandleComponentToggleInput();
        }

        private void HandleFacialExpressionInput()
        {
            if (_animator == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetExpression(0, "Neutral");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetExpression(1, "Happy");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetExpression(2, "Sad");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetExpression(3, "Angry");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetExpression(4, "Surprised");
            }
        }

        private void HandleLookTargetInput()
        {
            if (_lookController == null) return;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (_testTarget1 != null)
                {
                    _lookController.SetTarget(_testTarget1);
                    _currentLookTarget = _testTarget1.name;
                    // Debug.Log($"Looking at: {_testTarget1.name}");
                }
                else
                {
                    Debug.LogWarning("Test Target 1 not assigned!");
                }
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                if (_testTarget2 != null)
                {
                    _lookController.SetTarget(_testTarget2);
                    _currentLookTarget = _testTarget2.name;
                    Debug.Log($"Looking at: {_testTarget2.name}");
                }
                else
                {
                    Debug.LogWarning("Test Target 2 not assigned!");
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _lookController.ClearTarget();
                _currentLookTarget = "None";
                // Debug.Log("Cleared look target");
            }
        }

        private void HandleComponentToggleInput()
        {
            if (_lookController == null) return;

            if (Input.GetKeyDown(KeyCode.H))
            {
                // Toggle head tracking
                bool current = _lookController.GetComponent<HeadLook>()?.enabled ?? false;
                _lookController.SetHeadTrackingEnabled(!current);
                // Debug.Log($"Head Tracking: {!current}");
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                // Toggle eye tracking
                bool current = _lookController.GetComponent<VRoidEyeTracker>()?.enabled ?? false;
                _lookController.SetEyeTrackingEnabled(!current);
                // Debug.Log($"Eye Tracking: {!current}");
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                _lookController.OnCutsceneStart();
                // Debug.Log("Cutscene Started - All tracking disabled");
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                _lookController.OnCutsceneEnd();
                // Debug.Log("Cutscene Ended - Tracking re-enabled");
            }
        }

        private void SetExpression(int value, string name)
        {
            if (_animator == null) return;

            _animator.SetInteger("Expression", value);
            _currentExpression = name;
            Debug.Log($"Expression : {name}");
        }

        private void OnGUI()
        {
            // Display current state on screen
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, 10, 300, 30), $"Expression: {_currentExpression}", style);
            GUI.Label(new Rect(10, 40, 300, 30), $"Look Target: {_currentLookTarget}", style);
        }
    }
}
