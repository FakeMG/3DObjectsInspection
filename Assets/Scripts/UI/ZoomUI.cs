using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {
    public class ZoomUI : MonoBehaviour, IScrollHandler {
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float minZoom = 0.3f;
        [SerializeField] private float zoomSpeed = 10f;

        private AspectRatioFitter _aspectRatioFitter;
        private RawImage _image;
        private RectTransform _rectTransform;

        private void Start() {
            _aspectRatioFitter = GetComponent<AspectRatioFitter>();
            _image = GetComponent<RawImage>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update() {
            KeepAspectRatio();
        }

        private void KeepAspectRatio() {
            if (_image != null) {
                float aspect = (_image.texture.width * 1.0f) / _image.texture.height;

                if (_aspectRatioFitter != null) _aspectRatioFitter.aspectRatio = aspect;
            }
        }

        public void OnScroll(PointerEventData eventData) {
            Zoom(eventData);
        }

        private void Zoom(PointerEventData eventData) {
            float mouseScrollWheelValue = Input.GetAxis("Mouse ScrollWheel");
            Vector2 sizeDelta = _rectTransform.sizeDelta;

            if (mouseScrollWheelValue != 0f) {
                sizeDelta.x += mouseScrollWheelValue * zoomSpeed;
                sizeDelta.y += mouseScrollWheelValue * zoomSpeed;
            }

            if (sizeDelta.x >= _rectTransform.sizeDelta.x * maxZoom ||
                sizeDelta.y >= _rectTransform.sizeDelta.y * maxZoom) {
                var delta = _rectTransform.sizeDelta;
                sizeDelta.x = delta.x * maxZoom;
                sizeDelta.y = delta.y * maxZoom;
            }

            if (sizeDelta.x <= _rectTransform.sizeDelta.x * minZoom ||
                sizeDelta.y <= _rectTransform.sizeDelta.y * minZoom) {
                var delta = _rectTransform.sizeDelta;
                sizeDelta.x = delta.x * minZoom;
                sizeDelta.y = delta.y * minZoom;
            }

            _rectTransform.sizeDelta = sizeDelta;
        }
    }
}