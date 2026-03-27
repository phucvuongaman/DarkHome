using UnityEngine;
using UnityEngine.EventSystems; // Cần cái này để bắt sự kiện chuột

namespace DarkHome
{
    public class UISound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private AudioClip _hoverSound;
        [SerializeField] private AudioClip _clickSound;

        // Khi di chuột vào
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(_hoverSound, 0.5f); // 0.5 là volume nhỏ bớt
        }

        // Khi bấm chuột
        public void OnPointerClick(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(_clickSound);
        }
    }
}