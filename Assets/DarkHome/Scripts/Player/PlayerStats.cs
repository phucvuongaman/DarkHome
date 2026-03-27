using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

namespace DarkHome
{
    public class PlayerStats : MonoBehaviour, IDataPersistence
    {
        [SerializeField]
        [Range(0, 100)]
        private float _sanity = 100f;

        [SerializeField]
        [Range(0, 100)]
        private float _health = 100f;


        // Dùng Property để có thể kiểm soát logic khi giá trị thay đổi
        public float Sanity
        {
            get => _sanity;
            set
            {
                float newSanity = Mathf.Clamp(value, 0, 100);
                if (!Mathf.Approximately(_sanity, newSanity))
                {
                    _sanity = newSanity;
                    Debug.Log($"[PlayerStats] Sanity changed to: {_sanity}. Firing event!");
                    // PHÁT TÍN HIỆU khi sanity thay đổi
                    EventManager.Notify(GameEvents.Player.OnSanityChanged, _sanity);
                }
            }
        }

        public float Health
        {
            get => _health;
            set
            {
                float newHealth = Mathf.Clamp(value, 0, 100);
                if (!Mathf.Approximately(_health, newHealth))
                {
                    _health = newHealth;
                    Debug.Log($"[PlayerStats] Health changed to: {_health}. Firing event!");
                    // PHÁT TÍN HIỆU khi máu thay đổi
                    EventManager.Notify(GameEvents.Player.OnHealthChanged, _health);
                }
            }
        }


        private void OnValidate()
        {
            // Chỉ chạy logic này khi Game đang Play (để tránh lỗi khi Edit mode)
            if (Application.isPlaying)
            {
                // Ép buộc bắn Event thủ công khi giá trị _health/_sanity bị đổi từ Inspector
                // Lưu ý: Chúng ta gọi thẳng EventManager vì Property Setter sẽ từ chối chạy 
                // do _health đã bằng value mới rồi (Mathf.Approximately sẽ trả về true)

                EventManager.Notify(GameEvents.Player.OnHealthChanged, _health);
                EventManager.Notify(GameEvents.Player.OnSanityChanged, _sanity);
            }
        }
        // private PlayerContext context;
        // void Awake()
        // {
        // }
        // void OnEnable()
        // {
        // }
        // void OnDisable()
        // {
        // }
        // void JumpPressedSnity()
        // {
        //     Sanity -= 10;
        // }
        // public SanityStateMachine.ESanityState GetCurrentSanityState()
        // {
        //     if (_sanity > 75) return SanityStateMachine.ESanityState.Stable;
        //     if (_sanity > 50) return SanityStateMachine.ESanityState.Disturbed;
        //     if (_sanity > 25) return SanityStateMachine.ESanityState.Unstable;
        //     return SanityStateMachine.ESanityState.Insane;
        // }

        public void ApplyDamage(float amount)
        {
            Health -= amount;

            // Nếu muốn chắc chắn máu không âm
            if (Health < 0) Health = 0;

            if (Health <= 0)
            {
                Debug.Log("💀 Player đã chết! Phát tín hiệu Game Over...");

                // 👇 GỌI SỰ KIỆN Ở ĐÂY 👇
                EventManager.Notify(GameEvents.Player.OnPlayerDied);
            }
        }

        public void ApplySanityChange(float amount)
        {
            Sanity += amount;
        }

        public void LoadData(SaveData data)
        {
            this.Health = data.playerHealth;
            this.Sanity = data.playerSanity;
        }

        public void SaveData(ref SaveData data)
        {
            data.playerHealth = Health;
            data.playerSanity = Sanity;
        }
    }
}