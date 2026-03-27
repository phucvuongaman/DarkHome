using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace DarkHome
{
    public class EnemyRage : MonoBehaviour
    {
        [SerializeField] private GameObject _currentWeapon;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private LayerMask targetLayer;
        // Biến thời gian chờ (bằng độ dài animation tấn công, ví dụ 1.5 giây)
        [SerializeField] private float _attackCooldown = 1.5f;


        private Collider _weaponCollider;
        private Animator _animator;


        private bool _isAttacking = false; // Cờ này để xem animation chém đã xong chưa

        void Awake()
        {
            if (_currentWeapon != null)
            {
                _weaponCollider = _currentWeapon.GetComponent<Collider>();
            }
            _animator = GetComponent<Animator>();
        }

        void Start()
        {
            Assert.IsNotNull(_currentWeapon, "Weapon contraint is not assigned");

        }


        // --- CÁC HÀM CÔNG CỤ (PUBLIC) CHO STATE GỌI ---
        public void EnableDamage()
        {
            if (_weaponCollider != null) _weaponCollider.enabled = true;
        }

        public void DisableDamage()
        {
            if (_weaponCollider != null) _weaponCollider.enabled = false;
        }

        // Hàm kiểm 
        public void StartAttack()
        {
            // Nếu đang tấn công rồi thì DỪNG LẠI NGAY, không làm gì cả
            if (_isAttacking) return;

            // Bắt đầu tấn công
            _isAttacking = true;

            // Random kiểu chém (nhớ dùng 0, 2 để random int ra 0 hoặc 1)
            _animator.SetFloat("AttackStyle", Random.Range(0, 2));
            _animator.SetTrigger("Attack");

            // Đặt hẹn giờ để reset trạng thái (Sau khi chém xong)
            Invoke(nameof(ResetAttackFlag), _attackCooldown);
        }


        private void ResetAttackFlag()
        {
            _isAttacking = false;
        }

        public bool ScanAttackRange(out Transform target)
        {
            target = null;

            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, targetLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    target = hit.transform;
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif



        private void OnEnable()
        {
            EventManager.AddObserver(GameEvents.NpcWeaponAttack.StartAttack, StartAttack);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(GameEvents.NpcWeaponAttack.StartAttack, StartAttack);
        }

    }
}