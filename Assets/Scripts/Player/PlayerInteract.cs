using UnityEngine;
using System.Collections.Generic;

public interface IInteractable
{
    Color OutlineColor { get; }
    Color HighlightedOutlineColor { get; }
    void PickUp(Camera playerCamera = null);
    void SetItemNameVisible(bool visible);
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
    [SerializeField] private KeyCode _snapKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode _rotateKey = KeyCode.E;
    [SerializeField] private KeyCode _dropPickUpKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode _unFuseKey = KeyCode.R;
    [SerializeField] private KeyCode _chestKey = KeyCode.F;

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

        if (Input.GetKeyDown(_dropPickUpKey))
        {
            PickupCurrentTarget();
        }

        if (_grabbedItem != null)
        {
            if (Input.GetKey(_rotateKey))
            {
                PlayerController.Instance.CanLook = false;
                _grabbedItem.Rotate();
            }
            else
            {
                PlayerController.Instance.CanLook = true;
            }

            if (Input.GetKeyDown(_snapKey))
                _grabbedItem.TrySnap();

            if (Input.GetKeyDown(_unFuseKey))
                _grabbedItem.UnFuse();

            if (Input.GetKeyDown(_chestKey))
            {
                if (_grabbedItem.isInChest)
                {
                    _grabbedItem.RemoveFromChest();
                }
                else if (_grabbedItem.CanBeAddedToChest)
                {
                    _grabbedItem.AddToChest();
                }
            }
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

        return target != null
            && !IsUnavailableCarDoor(target)
            && IsOutlineInRange(target)
                ? target
                : null;
    }

    private void UpdateInputTip(Outline target)
    {
        bool hasValidTarget = target != null;

        bool shouldShowPickUpTip =
            hasValidTarget && target.GetComponent<Item>() != null;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.PickUp, shouldShowPickUpTip);

        bool shouldShowDropTip =
            _grabbedItem != null;
        bool shouldShowSnapTip =
            _grabbedItem != null && _grabbedItem.CanSnap;
        bool shouldShowDetachTip =
            _grabbedItem != null && _grabbedItem.CanUnFuse;
        bool ShouldShowAddChestTip =
            _grabbedItem != null &&
            !_grabbedItem.isInChest &&
            _grabbedItem.CanBeAddedToChest &&
            DaysManager.Instance.chestLevel != 0 &&
            DaysManager.Instance.itemsInChest < DaysManager.Instance.chestLevel;
        bool ShouldShowRemoveChestTip =
            _grabbedItem != null && _grabbedItem.isInChest;
        if (shouldShowDropTip)
            PlayerHUD.Instance.HideAllInputTips();

        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.Drop, shouldShowDropTip);
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.Rotate, shouldShowDropTip);
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.Snap, shouldShowSnapTip);
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.Detach, shouldShowDetachTip);
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.AddChest, ShouldShowAddChestTip);
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.RemoveChest, ShouldShowRemoveChestTip);

        bool shouldShowCarToWorkShopTip =
            hasValidTarget && target.TryGetComponent<CarDoor>(out CarDoor carDoorWork) && !carDoorWork.isDoorOpen && !DaysManager.Instance.isInWorkshop;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.StartCar, shouldShowCarToWorkShopTip);

        bool shouldShowCarToWarehouseTip =
            hasValidTarget && target.TryGetComponent<CarDoor>(out CarDoor carDoorWare) && !carDoorWare.isDoorOpen && DaysManager.Instance.isInWorkshop && !DaysManager.Instance.hasVisitedWareHouse;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.GoToWareHouse, shouldShowCarToWarehouseTip);

        bool shouldShowSleepTip =
            hasValidTarget && target.GetComponent<SleepDoor>() != null;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.GoToSleep, shouldShowSleepTip);
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
                IInteractable previousItem = _highlightedOutline.GetComponent<IInteractable>();
                if (previousItem != null)
                    previousItem.SetItemNameVisible(false);
            }

            _highlightedOutline = targetToHighlight;

            if (_highlightedOutline != null)
            {
                IInteractable highlightedItem = _highlightedOutline.GetComponent<IInteractable>();
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

        IInteractable item = _highlightedOutline.GetComponent<IInteractable>();
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

        if (_currentTarget == null
            || !IsOutlineInRange(_currentTarget)
            || IsUnavailableCarDoor(_currentTarget)
            || !IsOutlineInRange(_currentTarget))
        {
            _isInteracting = false;
            return;
        }

        Item targetItem = _currentTarget.GetComponent<Item>();
        Item assemblyRoot = targetItem != null ? targetItem.GetAssemblyRoot() : null;
        IInteractable interactable = assemblyRoot != null
            ? assemblyRoot
            : _currentTarget.GetComponent<IInteractable>();
        if (interactable != null)
        {
            _isInteracting = true;
            interactable.PickUp(_playerCamera);

            if (!DaysManager.Instance.isInWorkshop)
            {
                if (targetItem != null)
                {
                    Invoke(nameof(StopInteracting), 0.2f);
                }
            }
            else
            {
                _grabbedItem = assemblyRoot;
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

    private bool IsUnavailableCarDoor(Outline outline)
    {
        if (outline == null || DaysManager.Instance == null)
            return false;

        return outline.GetComponent<CarDoor>() != null
            && DaysManager.Instance.isInWorkshop
            && DaysManager.Instance.hasVisitedWareHouse;
    }

    void OnTriggerEnter(Collider other)
    {
        _collidersInRange.Add(other);

        Outline outline = other.GetComponentInParent<Outline>();
        if (outline != null)
        {
            outline.enabled = !IsUnavailableCarDoor(outline);
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
