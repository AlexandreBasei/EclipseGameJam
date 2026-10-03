using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Item : MonoBehaviour, IInteractable
{
    private const int MaxAssemblyItems = 3;

    public ObjectSO itemData;
    private readonly List<Item> _assemblyItems = new List<Item>();
    private IReadOnlyList<Item> _assemblyItemsReadOnly;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    [SerializeField, Min(0.1f)] private float _heldDistance = 2f;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    private Outline _outline;
    private bool _isGrabbed = false;
    private Rigidbody _rb;
    private Camera _heldCamera;
    private Vector3 _heldPositionOffset;

    public IReadOnlyList<Item> AssemblyItems
    {
        get
        {
            Item assemblyRoot = GetAssemblyRoot();
            if (assemblyRoot._assemblyItemsReadOnly == null)
                assemblyRoot._assemblyItemsReadOnly = assemblyRoot._assemblyItems.AsReadOnly();

            return assemblyRoot._assemblyItemsReadOnly;
        }
    }

    public bool CanUnFuse =>
        _isGrabbed &&
        GetAssemblyRoot() == this &&
        GetComponentsInChildren<Item>(true).Length > 1;

    public bool CanSnap
    {
        get
        {
            if (!DaysManager.Instance.isInWorkshop || !_isGrabbed)
                return false;

            foreach (SnapPoint snapPoint in GetComponentsInChildren<SnapPoint>(true))
            {
                if (snapPoint.TryGetContact(out SnapPoint contactingSnapPoint) &&
                    CanMergeWith(contactingSnapPoint.currentItem.GetAssemblyRoot()))
                    return true;
            }

            return false;
        }
    }

    void Start()
    {
        RefreshAssemblyItems();
        _outline = GetComponent<Outline>();
        _rb = GetComponent<Rigidbody>();
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
        print(AssemblyItems.Count);
    }

    private void UpdateHeldPosition()
    {
        Vector3 cameraForward = _heldCamera.transform.forward;
        Vector3 horizontalForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);

        if (horizontalForward.sqrMagnitude < 0.0001f)
            horizontalForward = Vector3.ProjectOnPlane(_heldCamera.transform.root.forward, Vector3.up);

        horizontalForward.Normalize();

        float verticalComponent = Mathf.Clamp(cameraForward.y, -0.25f, 1f);
        Vector3 direction =
            horizontalForward * Mathf.Sqrt(1f - verticalComponent * verticalComponent) +
            Vector3.up * verticalComponent;

        Vector3 heldPosition =
            _heldCamera.transform.position +
            direction * _heldDistance +
            _heldCamera.transform.TransformVector(_heldPositionOffset);

        if (TryGetAssemblyBounds(out Bounds bounds))
            transform.position += heldPosition - bounds.center;
        else
            transform.position = heldPosition;
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
            SetAssemblyCollidersEnabled(false);
            SetAssemblyPresentationVisible(false);
            _heldCamera = playerCamera;
            _heldPositionOffset = Vector3.zero;
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
        _heldPositionOffset = Vector3.zero;
        _rb.position = transform.position;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = false;
        SetAssemblyCollidersEnabled(true);
    }

    public void Rotate()
    {
        if (!Input.GetKey(KeyCode.E))
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rotationSpeed = 100f;

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
    }

    public void TrySnap()
    {
        if (!CanSnap)
            return;

        foreach (SnapPoint snapPoint in GetComponentsInChildren<SnapPoint>(true))
        {
            if (!snapPoint.TryGetContact(out SnapPoint contactingSnapPoint))
                continue;

            Item otherRoot = contactingSnapPoint.currentItem.GetAssemblyRoot();
            if (!CanMergeWith(otherRoot))
                continue;

            Vector3 snapOffset = contactingSnapPoint.transform.position - snapPoint.transform.position;
            transform.position += snapOffset;
            if (_heldCamera != null)
                _heldPositionOffset += _heldCamera.transform.InverseTransformVector(snapOffset);
            otherRoot.MergeInto(this);
            return;
        }
    }

    public void UnFuse()
    {
        if (!CanUnFuse)
            return;

        Item[] assemblyItems = GetComponentsInChildren<Item>(true);
        foreach (Item item in assemblyItems)
        {
            if (item == this)
                continue;

            item.transform.SetParent(null, true);
            item._isGrabbed = false;
            item._heldCamera = null;
            item._heldPositionOffset = Vector3.zero;
            if (item._rb == null)
                item._rb = item.gameObject.AddComponent<Rigidbody>();

            item._rb.detectCollisions = true;
            item._rb.isKinematic = false;
            item._rb.linearVelocity = Vector3.zero;
            item._rb.angularVelocity = Vector3.zero;
            item._rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            item._rb.WakeUp();

            item.SetItemCollidersEnabled(true);

            item.SetOutlineVisible(true);
            item.SetItemNameVisible(false);
        }

        Physics.SyncTransforms();
        RefreshAssemblyItems();
        foreach (Item item in assemblyItems)
        {
            if (item != this)
                item.RefreshAssemblyItems();
        }
    }

    public Item GetAssemblyRoot()
    {
        Item assemblyRoot = this;
        Transform parent = transform.parent;
        while (parent != null)
        {
            Item parentItem = parent.GetComponent<Item>();
            if (parentItem != null)
                assemblyRoot = parentItem;

            parent = parent.parent;
        }

        return assemblyRoot;
    }

    private void MergeInto(Item assemblyRoot)
    {
        Item[] mergedItems = GetComponentsInChildren<Item>(true);
        transform.SetParent(assemblyRoot.transform, true);

        foreach (Item mergedItem in mergedItems)
        {
            mergedItem._isGrabbed = false;
            mergedItem._heldCamera = null;
            mergedItem.SetItemNameVisible(false);
            mergedItem.SetOutlineVisible(false);

            mergedItem.SetItemCollidersEnabled(false);

            if (mergedItem._rb != null)
            {
                mergedItem._rb.linearVelocity = Vector3.zero;
                mergedItem._rb.angularVelocity = Vector3.zero;
                mergedItem._rb.detectCollisions = false;
                mergedItem._rb.isKinematic = true;
                Destroy(mergedItem._rb);
                mergedItem._rb = null;
            }
        }

        assemblyRoot.SetAssemblyCollidersEnabled(false);
        assemblyRoot.SetAssemblyPresentationVisible(false);
        assemblyRoot.RefreshAssemblyItems();
    }

    private bool CanMergeWith(Item otherRoot)
    {
        return otherRoot != null &&
               otherRoot != this &&
               !otherRoot._isGrabbed &&
               GetAssemblyRoot() == this &&
               GetComponentsInChildren<Item>(true).Length +
               otherRoot.GetComponentsInChildren<Item>(true).Length <= MaxAssemblyItems;
    }

    private void RefreshAssemblyItems()
    {
        Item assemblyRoot = GetAssemblyRoot();
        if (assemblyRoot != this)
        {
            assemblyRoot.RefreshAssemblyItems();
            return;
        }

        _assemblyItems.Clear();
        _assemblyItems.AddRange(GetComponentsInChildren<Item>(true));
    }

    private void SetAssemblyCollidersEnabled(bool enabled)
    {
        foreach (Item item in GetComponentsInChildren<Item>(true))
            item.SetItemCollidersEnabled(enabled);
    }

    private void SetItemCollidersEnabled(bool enabled)
    {
        foreach (Collider itemCollider in GetComponents<Collider>())
            itemCollider.enabled = enabled;
    }

    private bool TryGetAssemblyBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }

    private void SetAssemblyPresentationVisible(bool visible)
    {
        foreach (Item item in GetComponentsInChildren<Item>(true))
        {
            item.SetItemNameVisible(visible);
            item.SetOutlineVisible(visible);
        }
    }
}
