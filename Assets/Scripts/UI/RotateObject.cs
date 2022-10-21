using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class RotateObject : MonoBehaviour, IDragHandler {
        [SerializeField] private Transform inspectedObject;

        public void OnDrag(PointerEventData eventData) {
            Vector3 axis = Quaternion.AngleAxis(-90, Vector3.forward) * eventData.delta;
            inspectedObject.rotation = Quaternion.AngleAxis(eventData.delta.magnitude, axis) * inspectedObject.rotation;
        }
    }
}