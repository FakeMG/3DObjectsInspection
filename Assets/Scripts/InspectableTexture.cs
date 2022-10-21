using UnityEngine;
using UnityEngine.UI;

public class InspectableTexture : MonoBehaviour, IInteractable {
    [SerializeField] private Transform inspectionUI;
    [SerializeField] private RawImage inspectedObjectUI;
    
    [SerializeField] private Movement playerMovement;
    public Texture texture;

    private Material _material;
    private void Start() {
        _material = GetComponent<Material>();
        
        inspectionUI.gameObject.SetActive(false);
        inspectedObjectUI.gameObject.SetActive(false);
    }

    public void Interact(PlayerInteraction player) {
        playerMovement.ToggleFreeze();
        
        inspectionUI.gameObject.SetActive(!inspectionUI.gameObject.activeSelf);
        
        inspectedObjectUI.gameObject.SetActive(!inspectedObjectUI.gameObject.activeSelf);
        inspectedObjectUI.texture = texture;
    }
}