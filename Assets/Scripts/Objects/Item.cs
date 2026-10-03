using UnityEngine;
using TMPro;

public class Item : MonoBehaviour, IInteractable
{
    public ObjectSO itemData;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    [SerializeField, Min(0.1f)] private float _heldDistance = 2f;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    private Outline _outline;
    private bool _isGrabbed = false;
    private Rigidbody _rb;
    private BoxCollider _collider;
    private Camera _heldCamera;

    void Start()
    {
        _outline = GetComponent<Outline>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        if (_itemNameText != null && itemData != null)
        {
            _itemNameText.text = itemData.objectName;
        }

        _outline.OutlineColor = _outlineColor;
    }

    void LateUpdate()
    {
        if (!_isGrabbed || _heldCamera == null)
            return;

        UpdateHeldPosition();
    }

    private void UpdateHeldPosition()
    {
        Vector3 cameraForward = _heldCamera.transform.forward;
        Vector3 horizontalForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);

        if (horizontalForward.sqrMagnitude < 0.0001f)
            horizontalForward = Vector3.ProjectOnPlane(_heldCamera.transform.root.forward, Vector3.up);

        horizontalForward.Normalize();

        float verticalComponent = Mathf.Clamp01(cameraForward.y);
        Vector3 direction =
            horizontalForward * Mathf.Sqrt(1f - verticalComponent * verticalComponent) +
            Vector3.up * verticalComponent;

        transform.position = _heldCamera.transform.position + direction * _heldDistance;
    }

    public void SetItemNameVisible(bool visible)
    {
        if (_itemNameText != null)
            _itemNameText.gameObject.SetActive(visible);
    }

    public void SetOutlineVisible(bool visible)
    {
        if (_outline != null)
            _outline.enabled = visible;
    }

    public void PickUp(Camera playerCamera = null)
    {
        if (DaysManager.Instance.isInWorkshop)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            _collider.enabled = false;
            SetItemNameVisible(false);
            SetOutlineVisible(false);
            _heldCamera = playerCamera;
            _isGrabbed = true;
            transform.SetParent(playerCamera.transform, true);
            UpdateHeldPosition();
        }
        else
        {
            int weight = itemData.tags == Tag.Heavy ? 2 : 1;
            
            if (WeightManager.Instance.SliderBar.value + weight <= WeightManager.Instance.SliderBar.maxValue)
            {
                WeightManager.Instance.AddSliderValue(weight);
                Destroy(gameObject);
            }
        }
    }

    public void Drop()
    {
        if (!_isGrabbed)
            return;

        transform.SetParent(null, true);
        _isGrabbed = false;
        _heldCamera = null;
        _rb.position = transform.position;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = false;
        _collider.enabled = true;
    }

    public void Rotate()
    {
        // WIP
    }

    public void Snap()
    {
        // WIP
    }
}
