using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI {
    public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler {
        [SerializeField] private VideoPlayer videoPlayer;

        private Slider _slider;
        private bool _isSkipping;

        private void Start() {
            _slider = GetComponent<Slider>();
        }

        private void Update() {
            if (Input.GetMouseButtonUp(0)) {
                _isSkipping = false;
            }
            
            if (videoPlayer.frame > 0 && !_isSkipping) {
                _slider.value = videoPlayer.frame / (float)videoPlayer.frameCount;
            }

            if (_isSkipping) {
                videoPlayer.frame = (long)(videoPlayer.frameCount * _slider.value);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            _isSkipping = true;
        }

        public void OnPointerDown(PointerEventData eventData) {
            _isSkipping = true;
        }
    }
}