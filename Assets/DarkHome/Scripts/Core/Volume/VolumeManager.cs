// TODO: Làm thêm vài chức năng với Sanity, như Amnesia

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DarkHome
{
    [Serializable]
    public class VolumeEffects
    {
        public LensDistortion Lens;
        public ChromaticAberration Chroma;
        public Vignette Vignette;
        public MotionBlur Blur;
        public ColorAdjustments ColorAdjust;
    }

    public class VolumeManager : MonoBehaviour
    {
        public static VolumeManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Volume _globalVolume;
        [SerializeField] private Volume _sanityVolume;
        [SerializeField] private Volume _damageVolume;
        [SerializeField] private float _lerpSpeed = 5f;

        // Biến cờ: Khóa Update khi đang chuyển cảnh
        private bool _isSceneReady = false;

        public VolumeEffects global = new();
        public VolumeEffects sanity = new();
        public VolumeEffects damage = new();

        // Biến mục tiêu
        private float _targetSanityWeight = 0f;
        private float _targetDamageWeight = 0f;
        private float _targetSanityLensIntensity = 0f;
        private float _targetSanityChromaIntensity = 0f;
        private float _targetSanityVignetteIntensity = 0.2f;
        private float _targetSanityBlurIntensity = 0f;
        private float _targetDamageVignetteIntensity = 0.2f;
        private float _targetDamageChromaIntensity = 0f;

        #region === Init & Life Cycle ===

        // private void Awake()
        // {
        //     if (Instance != null) { Destroy(gameObject); return; }
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }

        private void OnEnable()
        {
            EventManager.AddObserver<float>(GameEvents.Player.OnHealthChanged, HandleHealthChanged);
            EventManager.AddObserver<float>(GameEvents.Player.OnSanityChanged, HandleSanityChanged);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<float>(GameEvents.Player.OnHealthChanged, HandleHealthChanged);
            EventManager.RemoveListener<float>(GameEvents.Player.OnSanityChanged, HandleSanityChanged);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // --- HÀM UPDATE AN TOÀN ---
        private void Update()
        {
            // NẾU CHƯA SẴN SÀNG -> KHÔNG LÀM GÌ CẢ (Chặn lỗi Missing)
            if (!_isSceneReady) return;

            UpdateSanityEffects();
            UpdateDamageEffects();
        }

        #endregion

        #region === Scene Handling (FIXED) ===

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 1. KHÓA UPDATE NGAY LẬP TỨC
            _isSceneReady = false;
            Debug.Log($"[VolumeManager] Scene Loaded: {scene.name}. Đang Reset dữ liệu...");

            // 2. XÓA SẠCH THAM CHIẾU CŨ (Clean Up)
            _globalVolume = null;
            _sanityVolume = null;
            _damageVolume = null;

            ResetVolumeEffects(global);
            ResetVolumeEffects(sanity);
            ResetVolumeEffects(damage);

            // 3. NẾU LÀ MENU/BOOT -> DỪNG LẠI (Vẫn giữ trạng thái sạch)
            if (scene.name == "Main Menu" || scene.name == "Boot")
            {
                return;
            }

            // 4. TÌM KIẾM MỚI (Dùng Coroutine để đợi 1 frame cho object ổn định)
            StartCoroutine(InitializeSceneVolumes());
        }

        private IEnumerator InitializeSceneVolumes()
        {
            // Đợi cuối frame để đảm bảo mọi object trong scene mới đã Spawn xong
            yield return new WaitForSeconds(.5f);

            // Tìm kiếm
            FindAllVolumesByTag();

            // Gán Profile
            InitVolumeEffects(_globalVolume, global);
            InitVolumeEffects(_sanityVolume, sanity);
            InitVolumeEffects(_damageVolume, damage);

            // 5. MỞ KHÓA UPDATE (Lúc này mọi thứ đã an toàn)
            _isSceneReady = true;
            Debug.Log("[VolumeManager] Đã khởi tạo xong Volume cho Scene mới.");
        }

        private void ResetVolumeEffects(VolumeEffects effects)
        {
            effects.Lens = null;
            effects.Chroma = null;
            effects.Vignette = null;
            effects.Blur = null;
            effects.ColorAdjust = null;
        }

        private void FindAllVolumesByTag()
        {
            // Tìm theo Tag (Đảm bảo Tag trong Editor là "Global Volume", "Sanity Volume"...)
            GameObject globalObj = GameObject.FindGameObjectWithTag("GlobalVolume");
            if (globalObj) _globalVolume = globalObj.GetComponent<Volume>();

            GameObject sanityObj = GameObject.FindGameObjectWithTag("SanityVolume");
            if (sanityObj) _sanityVolume = sanityObj.GetComponent<Volume>();

            GameObject damageObj = GameObject.FindGameObjectWithTag("DamageVolume");
            if (damageObj) _damageVolume = damageObj.GetComponent<Volume>();
        }

        private void InitVolumeEffects(Volume volume, VolumeEffects effects)
        {
            // Đã reset ở trên rồi, nhưng check null an toàn
            if (volume == null || volume.profile == null) return;

            volume.profile.TryGet(out effects.Lens);
            volume.profile.TryGet(out effects.Chroma);
            volume.profile.TryGet(out effects.Vignette);
            volume.profile.TryGet(out effects.Blur);
            volume.profile.TryGet(out effects.ColorAdjust);
        }

        #endregion

        #region === Logic Lerp & Effects ===

        private void UpdateSanityEffects()
        {
            if (_sanityVolume == null) return; // Check null kép cho chắc

            _sanityVolume.weight = Mathf.Lerp(_sanityVolume.weight, _targetSanityWeight, _lerpSpeed * Time.deltaTime);

            if (sanity.Lens) sanity.Lens.intensity.value = Mathf.Lerp(sanity.Lens.intensity.value, _targetSanityLensIntensity, _lerpSpeed * Time.deltaTime);
            if (sanity.Chroma) sanity.Chroma.intensity.value = Mathf.Lerp(sanity.Chroma.intensity.value, _targetSanityChromaIntensity, _lerpSpeed * Time.deltaTime);
            if (sanity.Vignette) sanity.Vignette.intensity.value = Mathf.Lerp(sanity.Vignette.intensity.value, _targetSanityVignetteIntensity, _lerpSpeed * Time.deltaTime);
            if (sanity.Blur) sanity.Blur.intensity.value = Mathf.Lerp(sanity.Blur.intensity.value, _targetSanityBlurIntensity, _lerpSpeed * Time.deltaTime);
        }

        private void UpdateDamageEffects()
        {
            if (_damageVolume == null) return;

            _damageVolume.weight = Mathf.Lerp(_damageVolume.weight, _targetDamageWeight, _lerpSpeed * Time.deltaTime);

            if (damage.Vignette) damage.Vignette.intensity.value = Mathf.Lerp(damage.Vignette.intensity.value, _targetDamageVignetteIntensity, _lerpSpeed * Time.deltaTime);
            if (damage.Chroma) damage.Chroma.intensity.value = Mathf.Lerp(damage.Chroma.intensity.value, _targetDamageChromaIntensity, _lerpSpeed * Time.deltaTime);
        }

        private void HandleSanityChanged(float sanityValue)
        {
            if (!_isSceneReady) return; // Không xử lý event khi đang chuyển cảnh

            // if (sanityValue > 75) _targetSanityWeight = 0f;
            // else if (sanityValue > 50) _targetSanityWeight = 0.3f;
            // else if (sanityValue > 25) _targetSanityWeight = 0.6f;
            // else _targetSanityWeight = 1f;
            _targetSanityWeight = 1f - (sanityValue / 100f);

            float t = Mathf.InverseLerp(100f, 0f, sanityValue);
            _targetSanityLensIntensity = t * 0.5f;
            _targetSanityChromaIntensity = t * 1f;
            _targetSanityVignetteIntensity = 0.2f + t * 0.2f;
            _targetSanityBlurIntensity = t * 0.7f;
        }

        private void HandleHealthChanged(float healthValue)
        {
            if (!_isSceneReady) return;

            //  Tính độ đậm (Weight): Máu 100->0 thì Weight 0->1
            _targetDamageWeight = 1f - (healthValue / 100f);

            //  Chỉnh màu đỏ (Color Filter)
            if (damage.ColorAdjust != null)
            {
                // Lerp từ Trắng (bình thường) sang Đỏ (khi máu thấp)
                damage.ColorAdjust.colorFilter.value = Color.Lerp(Color.white, Color.red, _targetDamageWeight);
            }
        }

        public void SetBrightness(float value)
        {
            // Code cũ giữ nguyên
            if (global != null && global.ColorAdjust != null)
            {
                global.ColorAdjust.postExposure.value = value;
                PlayerPrefs.SetFloat("Brightness", value);
            }
        }

        // Giữ lại các hàm Public API khác (ApplyStableEffects...)
        public void ApplyStableEffects() => _targetSanityWeight = 0f;
        public void ApplyDisturbedEffects() => _targetSanityWeight = 0.3f;
        public void ApplyUnstableEffects() => _targetSanityWeight = 0.6f;
        public void ApplyInsaneEffects() => _targetSanityWeight = 1f;

        #endregion
    }
}