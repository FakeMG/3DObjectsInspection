using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour {
    [SerializeField] private float radius;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private RectTransform interactionUI;

    private readonly Collider[] _colliders = new Collider[10];
    private RaycastHit _hitInfo;
    private bool _isPointingToInteractableObject;

    private RectTransform _parentUIElement;
    private Text _interactionUIText;
    private Camera _mainCamera;
    private IInteractable _targetInteractableComponent;

    private void Start() {
        interactionUI.gameObject.SetActive(false);

        _interactionUIText = interactionUI.GetComponent<Text>();
        _parentUIElement = interactionUI.parent.GetComponent<RectTransform>();
        _mainCamera = GetComponentInChildren<Camera>();
    }

    private void Update() {
        _isPointingToInteractableObject = IsPointingToInteractableObject();
        
        if (_isPointingToInteractableObject) {
            EnableInteractionUI();

            if (Input.GetKeyDown(KeyCode.E)) {

                _targetInteractableComponent.Interact(this);
            }
        } else {
            interactionUI.gameObject.SetActive(false);
        }
    }

    private void EnableInteractionUI() {
        interactionUI.gameObject.SetActive(true);

        _interactionUIText.text = "E Examine";
        
        var target = _hitInfo.transform;
        Vector3 targetScreenPosition = _mainCamera.WorldToScreenPoint(target.transform.position);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentUIElement, targetScreenPosition, null, out Vector2 finalPos);
        interactionUI.anchoredPosition = finalPos;
    }

    private bool IsPointingToInteractableObject() {
        int size = Physics.OverlapSphereNonAlloc(transform.position, radius, _colliders, interactableLayer);

        if (size <= 0) {
            return false;
        }

        foreach (Collider target in _colliders) {
            if (target != null) {
                Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, directionToTarget) < 80f / 2) {
                    if (Physics.Raycast(transform.position, transform.forward, out _hitInfo, radius,
                            interactableLayer)) {
                        _targetInteractableComponent = _hitInfo.transform.GetComponent<IInteractable>();
                        if (_targetInteractableComponent != null) {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}