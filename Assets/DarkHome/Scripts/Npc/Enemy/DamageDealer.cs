using UnityEngine;

namespace DarkHome
{
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] private float _damageAmount = 40f;

        private void OnTriggerEnter(Collider other)
        {
            // Chỉ gây damage nếu chạm vào Player
            if (other.CompareTag("Player"))
            {
                // Gọi hàm trừ máu (nhớ là số dương, hàm ApplyDamage tự trừ)
                PlayerStats player = other.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.ApplyDamage(_damageAmount);
                    // Debug.Log($"[Weapon] Chém trúng Player! Gây {_damageAmount} sát thương.");

                    // Tắt collider ngay sau khi trúng để tránh gây damage 2 lần trong 1 cú chém (Optional)
                    // GetComponent<Collider>().enabled = false; 
                }
            }
        }
    }
}