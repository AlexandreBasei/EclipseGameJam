using UnityEngine;
using System.Collections.Generic;

public interface IInteractable
{
    void PickUp(Camera playerCamera = null);
    void Rotate();
    void Snap();
}

public class PlayerInteract : Singleton<PlayerInteract>
{
    private const float TargetLossGracePeriod = 0.15f;

    private readonly HashSet<Collider> _collidersInRange = new HashSet<Collider>();
    private Camera _playerCamera;
    private Outline _currentTarget;
    private Outline _lookedAtTarget;
    private Outline _highlightedOutline;
    private float _lastTimePickUpTargetWasValid = float.NegativeInfinity;
    private bool _isInteracting = false;
    private Item _grabbedItem;

    protected override void Awake()
    {
        base.Awake();
        _playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        UpdateCurrentTarget();
        UpdateOutlineAndName();
        UpdateInputTip(_currentTarget);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PickupCurrentTarget();
        }
    }

    private void UpdateCurrentTarget()
    {
        Outline lookedAtTarget = GetLookedAtTarget();
        _lookedAtTarget = lookedAtTarget;

        if (lookedAtTarget != null)
        {
            if (_currentTarget != null &&
                _currentTarget != lookedAtTarget &&
                !IsOutlineInRange(_currentTarget))
            {
                _currentTarget.enabled = false;
            }

            _currentTarget = lookedAtTarget;
            _lastTimePickUpTargetWasValid = Time.time;
            return;
        }

        if (_currentTarget == null ||
            Time.time - _lastTimePickUpTargetWasValid <= TargetLossGracePeriod)
        {
            return;
        }

        if (!IsOutlineInRange(_currentTarget))
            _currentTarget.enabled = false;

        _currentTarget = null;
    }

    private Outline GetLookedAtTarget()
    {
        if (_playerCamera == null)
        {
            Debug.LogError("PlayerInteract could not find a Camera in its children.", this);
            enabled = false;
            return null;
        }

        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return null;
        }

        Outline target = hit.collider.GetComponentInParent<Outline>();

        return target != null && IsOutlineInRange(target) ? target : null;
    }

    private void UpdateInputTip(Outline target)
    {
        bool hasValidTarget = target != null;
        bool shouldShowPickUpTip =
            hasValidTarget || Time.time - _lastTimePickUpTargetWasValid <= TargetLossGracePeriod;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.PickUp, shouldShowPickUpTip);
    }

    private void UpdateOutlineAndName()
    {
        Outline targetToHighlight =
            _currentTarget != null && _currentTarget.enabled
                ? _currentTarget
                : null;

        if (_highlightedOutline != targetToHighlight)
        {
            RestoreOutlineColor();

            if (_highlightedOutline != null)
            {
                Item previousItem = _highlightedOutline.GetComponent<Item>();
                if (previousItem != null)
                    previousItem.SetItemNameVisible(false);
            }

            _highlightedOutline = targetToHighlight;

            if (_highlightedOutline != null)
            {
                Item highlightedItem = _highlightedOutline.GetComponent<Item>();
                if (highlightedItem != null && !_isInteracting)
                    _highlightedOutline.OutlineColor = highlightedItem.HighlightedOutlineColor;
            }
        }

        if (_highlightedOutline == null)
            return;

        Item item = _highlightedOutline.GetComponent<Item>();
        if (item != null)
        {
            bool shouldShowItemName =
                _currentTarget == _highlightedOutline &&
                Time.time - _lastTimePickUpTargetWasValid <= TargetLossGracePeriod &&
                item.itemData != null && !_isInteracting;

            item.SetItemNameVisible(shouldShowItemName);
        }
    }

    private void RestoreOutlineColor()
    {
        if (_highlightedOutline == null)
            return;

        Item item = _highlightedOutline.GetComponent<Item>();
        if (item != null)
            _highlightedOutline.OutlineColor = item.OutlineColor;
    }

    public void PickupCurrentTarget()
    {
        if (_grabbedItem != null)
        {
            _grabbedItem.Drop();
            _grabbedItem = null;
            _isInteracting = false;
            return;
        }

        if (_currentTarget == null || !IsOutlineInRange(_currentTarget))
        {
            _isInteracting = false;
            return;
        }

        IInteractable interactable = _currentTarget.GetComponent<IInteractable>();
        if (interactable != null)
        {
            _isInteracting = true;
            interactable.PickUp(_playerCamera);

            if (!DaysManager.Instance.isInWorkshop)
            {
                Invoke(nameof(StopInteracting), 0.2f);
            }
            else
            {
                _grabbedItem = _currentTarget.GetComponent<Item>();
            }
        }
    }

    private void StopInteracting()
    {
        _isInteracting = false;
    }

    private bool IsOutlineInRange(Outline outline)
    {
        foreach (Collider rangeCollider in _collidersInRange)
        {
            if (rangeCollider != null && rangeCollider.GetComponentInParent<Outline>() == outline)
            {
                return true;
            }
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        _collidersInRange.Add(other);

        Outline outline = other.GetComponentInParent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        _collidersInRange.Remove(other);

        Outline outline = other.GetComponentInParent<Outline>();
        if (outline != null &&
            !IsOutlineInRange(outline) &&
            outline != _currentTarget)
        {
            outline.enabled = false;
        }
    }

    void OnDisable()
    {
        if (_highlightedOutline != null)
        {
            RestoreOutlineColor();
            _highlightedOutline = null;
        }

        _collidersInRange.Clear();
        _currentTarget = null;
        _lastTimePickUpTargetWasValid = float.NegativeInfinity;
        if (PlayerHUD.TryGetInstance != null)
        {
            PlayerHUD.TryGetInstance.HideInputTip(PlayerHUD.InputTip.PickUp);
        }
    }
}
