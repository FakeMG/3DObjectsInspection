using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable {
    [SerializeField] private Transform inspectionUI;
    [SerializeField] private RectTransform inspectedObjectUI;
    
    [SerializeField] private MeshFilter inspectedObject;
    [SerializeField] private Movement playerMovement;

    private Mesh _mesh;

    private void Start() {
        _mesh = GetComponent<MeshFilter>().mesh;
        
        inspectionUI.gameObject.SetActive(false);
        inspectedObjectUI.gameObject.SetActive(false);
    }

    public void Interact(PlayerInteraction player) {
        playerMovement.ToggleFreeze();
        
        
        inspectionUI.gameObject.SetActive(!inspectionUI.gameObject.activeSelf);
        
        inspectedObjectUI.gameObject.SetActive(!inspectedObjectUI.gameObject.activeSelf);
        inspectedObject.mesh = _mesh;
    }
}