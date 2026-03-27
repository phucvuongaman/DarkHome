///////////////////////////////
//// Script chua chát nhất ////
///////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


namespace DarkHome
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance;

        [Header("Transition Settings")]
        // [SerializeField] private SceneTransitionDataSO _transitionData;
        // [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private CanvasGroup _fadeCanvas;

        private GameObject _playerInstance;

        private bool _isTransitioning = false;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            EventManager.AddObserver<SceneChangeData>(GameEvents.SceneTransition.OnSceneChangeRequested, HandleSceneChangeRequest);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SceneChangeData>(GameEvents.SceneTransition.OnSceneChangeRequested, HandleSceneChangeRequest);
        }

        private void HandleSceneChangeRequest(SceneChangeData data)
        {
            TransitionTo(data.SceneName, data.TargetSpawnID);
        }

        // Hàm TransitionTo cũ của bạn không cần thay đổi
        public void TransitionTo(string nextSceneName, string spawnID)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Đang chuyển cảnh, từ từ thôi bạn ơi!");
                return;
            }
            StartCoroutine(TransitionRoutine(nextSceneName, spawnID));
        }

        #region TransitionRoutine
        //////////////////////////////////////// Đã cũ nhưng lười sửa comment, về cơ bản logic vẫn đúng 03/2026
        ///// Khó nhớ comment nhiều một chút
        // Quy trình (Coroutine) chuyển cảnh
        /// <summary>
        /// Các viêc mà tôi cần làm khi chuyển scene
        /// Fade In Out mỗi khi chuyển cảnh: 🔹 1 và 10
        /// LoadScene mới rồi set active cho nó, mode sẽ là Additive để giữa cho SceneTransitionManager chạy không bị cắt tiết giữa chừng rồi dùng
        /// Lấy dữ liệu player cũ hoặc tạo mới player mới: 
        ///  BỎ + 🔹 2 và 3 (NHƯNG TÔI ĐÃ BỎ ĐI VÌ TÔI CẦN LOADSCENE, LÁY SPAWNPOINT,.. TRƯỚC KHI SET MỘT PLAYER)
        ///     + 🔹 6 và 7 sẽ gánh vác vấn đề đó
        ///  BỎ + 🔹 8 sẽ lắp cam vào player (TÔI SẼ ĐÃ GÃ CAMERACONTROLLER LÀM)
        /// Hủy đi scene cũ (trừ Persitent scene)
        /// </summary>
        /// <param name="nextSceneName"> Gã này ghi đúng string Scene name là được </param>
        /// <param name="spawnID"> phải tạo 1 GO chứa SpawnPoint, SpawnPoint.SpawnID sẽ giống với string spawnID là được</param>
        private IEnumerator TransitionRoutine(string nextSceneName, string spawnID)
        {
            Debug.Log($"🚀 Bắt đầu chuyển cảnh: {nextSceneName} | Spawn: {spawnID}");
            _isTransitioning = true;

            // Nếu cùng scene hoặc dùng keyword "SAME_SCENE" thì chỉ Fade + Teleport, KHÔNG reload!
            string currentSceneName = SceneManager.GetActiveScene().name;

            // "SAME_SCENE" là keyword từ Events.csv, thay bằng tên scene thật
            if (nextSceneName == "SAME_SCENE")
                nextSceneName = currentSceneName;

            bool isSameScene = (nextSceneName == currentSceneName);

            if (isSameScene)
            {
                // Debug.Log($"⚡ SAME SCENE detected! Fade + Teleport only (NO reload)");
                yield return SameSceneTeleport(spawnID);
                _isTransitioning = false;
                yield break; // Exit early
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 1: SAVE & CLEAR
            // ---------------------------------------------------------------------------
            if (SaveLoadManager.Instance != null)
            {
                try
                {
                    // Chỉ Save khi không phải load game/menu
                    if (spawnID != "LOAD_FROM_SAVE" &&
                        SceneManager.GetActiveScene().name != "Main Menu" &&
                        SceneManager.GetActiveScene().name != "Boot")
                    {
                        SaveLoadManager.Instance.SaveSceneToMemory();
                    }
                }
                catch (System.Exception ex) { Debug.LogError($"❌ Lỗi Save: {ex.Message}"); }

                if (ObjectManager.Instance != null) ObjectManager.Instance.ClearSceneObjects();
                if (NpcManager.Instance != null) NpcManager.Instance.Clear();
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 2: LOAD SCENE & GIẢI CỨU PLAYER
            // ---------------------------------------------------------------------------
            yield return Fade(1f); // Fade Out

            var loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            Scene newScene = SceneManager.GetSceneByName(nextSceneName);
            SceneManager.SetActiveScene(newScene);

            // Destroy duplicate Player baked vào scene mới
            // Scene có thể có Player prefab đặt sẵn (để test). Phải xóa trước khi rescue player cũ.
            if (_playerInstance != null)
            {
                // Tìm TẤT CẢ Player trong scene mới (không phải player instance cũ)
                GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
                foreach (var p in allPlayers)
                {
                    if (p != _playerInstance)
                    {
                        // Debug.Log($"🗑️ Destroy duplicate Player từ scene: {p.name}");
                        Destroy(p);
                    }
                }
            }

            // Chuyển hộ khẩu Player sang Scene mới TRƯỚC KHI Unload scene cũ
            GameObject playerToRescue = _playerInstance != null ? _playerInstance : GameObject.FindWithTag("Player");
            if (playerToRescue != null)
            {
                SceneManager.MoveGameObjectToScene(playerToRescue, newScene);
            }

            // Unload Scene cũ
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.name != newScene.name && s.name != this.gameObject.scene.name)
                {
                    var unload = SceneManager.UnloadSceneAsync(s);
                    if (unload != null) while (!unload.isDone) yield return null;
                }
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 3: TÌM SPAWN POINT
            // ---------------------------------------------------------------------------
            Transform spawnTransform = null;
            if (spawnID == "LOAD_FROM_SAVE")
            {
                spawnTransform = new GameObject("TempSpawn").transform; // Tạo tạm
            }
            else
            {
                // Tìm SpawnPoint xịn trong Scene mới
                SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                foreach (var sp in spawns) { if (sp.SpawnID == spawnID) { spawnTransform = sp.transform; break; } }
                if (spawnTransform == null) spawnTransform = new GameObject("FallbackSpawn").transform;
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 4: LOAD DATA (ĐỂ RESTORE ITEM/INVENTORY)
            // ---------------------------------------------------------------------------

            // Disable NavMeshAgent TRƯỚC khi unload để tránh race condition ============= Nyaf quan trọng =============
            // Khi load additive, NavMesh của scene cũ bị destroy trước khi agent kịp detach
            NavMeshAgent[] agents = GameObject.FindObjectsByType<NavMeshAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var agent in agents)
            {
                if (agent != null && agent.isActiveAndEnabled)
                    agent.enabled = false;
            }

            // Chờ 1 Frame cho các Object (Cửa, Item) kịp Start()
            yield return null;
            yield return new WaitForEndOfFrame();

            // Re-enable NavMeshAgents sau khi NavMesh của scene mới đã active
            // Dùng Warp() để đặt agent đúng vị trí trên NavMesh thay vì để nó tự tìm
            agents = GameObject.FindObjectsByType<NavMeshAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var agent in agents)
            {
                if (agent != null && agent.gameObject.activeInHierarchy)
                {
                    agent.enabled = true;
                    // Warp đặt agent đúng trên NavMesh, không bị rơi xuống đất
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(agent.transform.position, out hit, 5f, NavMesh.AllAreas))
                        agent.Warp(hit.position);
                }
            }

            //  LOAD DATA TRƯỚC KHI TELEPORT 
            // Việc này sẽ cập nhật Inventory, Máu, Trạng thái Cửa...
            // Nó cũng sẽ set vị trí Player về vị trí cũ (SAI), nhưng đừng lo, bước 5 sẽ sửa lại ngay.
            if (SaveLoadManager.Instance != null)
            {
                Debug.Log("📥 Đang Apply Data (Item, Trạng thái)...");
                SaveLoadManager.Instance.ApplyLoadedDataToScene();
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 5: XỬ LÝ PLAYER (CHỐT HẠ VỊ TRÍ CUỐI CÙNG)
            // ---------------------------------------------------------------------------

            var existingPlayer = GameObject.FindWithTag("Player");

            if (existingPlayer == null)
            {
                // ==> SPAWN MỚI (New Game / Load Game từ Menu)
                _playerInstance = Instantiate(_playerPrefab, spawnTransform.position, spawnTransform.rotation);
                EventManager.Notify(GameEvents.SceneTransition.OnPlayerSpawned, _playerInstance.transform);

                // Nếu là Load Game thì Data đã được Apply ở Bước 4 rồi, không cần làm gì thêm
                if (spawnID == "LOAD_FROM_SAVE" && SaveLoadManager.Instance != null)
                {
                    // Debug.Log(" Đang nạp dữ liệu Load Game...");
                    // Chờ 1 frame cho PlayerStats kịp Awake/Start
                    yield return null;
                    SaveLoadManager.Instance.ApplyLoadedDataToScene();
                }
            }
            else
            {
                // ==> DỊCH CHUYỂN (Transition)
                // Chỉ thực hiện khi KHÔNG PHẢI LÀ LOAD GAME (Vì Load Game thì vị trí trong Save là chuẩn rồi)
                if (spawnID != "LOAD_FROM_SAVE")
                {
                    // Debug.Log($" Force Teleport Player đến SpawnPoint: {spawnID}");
                    _playerInstance = existingPlayer;

                    // Tắt CharacterController để tránh xung đột vật lý
                    // var cc = _playerInstance.GetComponent<CharacterController>();
                    // if (cc) cc.enabled = false;

                    // GHI ĐÈ VỊ TRÍ
                    _playerInstance.transform.position = spawnTransform.position;
                    _playerInstance.transform.rotation = spawnTransform.rotation;

                    // Đồng bộ vật lý (quan trọng)
                    Physics.SyncTransforms();

                    // if (cc) cc.enabled = true;

                    EventManager.Notify(GameEvents.SceneTransition.OnPlayerSpawned, _playerInstance.transform);
                }
            }

            // ---------------------------------------------------------------------------
            // 🔹 GIAI ĐOẠN 6: KẾT THÚC
            // ---------------------------------------------------------------------------
            if (spawnID == "LOAD_FROM_SAVE" && spawnTransform != null) Destroy(spawnTransform.gameObject);

            yield return Fade(0f); // Fade In
            _isTransitioning = false;
            Debug.Log("✅ DONE!");
        }

        #endregion


        /// <summary>
        /// Same-scene teleport (for Bed sleep, etc.) - Fade + Move player + Fade in
        /// </summary>
        private IEnumerator SameSceneTeleport(string spawnID)
        {
            // Debug.Log($" Same-scene teleport to: {spawnID}");

            // Fade out
            yield return Fade(1f);

            // Find spawn point
            Transform spawnTransform = null;
            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawns)
            {
                if (sp.SpawnID == spawnID)
                {
                    spawnTransform = sp.transform;
                    break;
                }
            }

            if (spawnTransform == null)
            {
                // Debug.LogWarning($" SpawnPoint '{spawnID}' not found! Using Player current position.");
                yield return Fade(0f);
                yield break;
            }

            // Teleport player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // var cc = player.GetComponent<CharacterController>();
                // if (cc) cc.enabled = false;

                player.transform.position = spawnTransform.position;
                player.transform.rotation = spawnTransform.rotation;

                Physics.SyncTransforms();

                // if (cc) cc.enabled = true;
                // _playerInstance.transform.position = spawnTransform.position;
                // _playerInstance.transform.rotation = spawnTransform.rotation;

                // Debug.Log($" Player teleported to {spawnID}");
            }

            // Fade in
            yield return Fade(0f);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float speed = 2f;
            while (!Mathf.Approximately(_fadeCanvas.alpha, targetAlpha))
            {
                _fadeCanvas.alpha = Mathf.MoveTowards(_fadeCanvas.alpha, targetAlpha, Time.deltaTime * speed);
                yield return null;
            }
        }

    }
}
