using UnityEngine;
using System.Collections.Generic;

public interface IInteractable
{
    void Interact();
}

public class PlayerInteract : Singleton<PlayerInteract>
{
    private const float TargetLossGracePeriod = 0.15f;

    private readonly HashSet<Collider> _collidersInRange = new HashSet<Collider>();
    private Camera _playerCamera;
    private Outline _currentTarget;
    private Outline _highlightedOutline;
    private float _lastTimePickUpTargetWasValid = float.NegativeInfinity;
    private Color _originalOutlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;

    protected override void Awake()
    {
        base.Awake();
        _playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        UpdateCurrentTarget();
        UpdateOutlineColor();
        UpdateInputTip(_currentTarget);
    }

    private void UpdateCurrentTarget()
    {
        Outline lookedAtTarget = GetLookedAtTarget();
        if (lookedAtTarget != null)
        {
            _currentTarget = lookedAtTarget;
            _lastTimePickUpTargetWasValid = Time.time;
            return;
        }

        bool currentTargetIsStillInRange =
            _currentTarget != null &&
            _currentTarget.enabled &&
            IsOutlineInRange(_currentTarget);

        if (!currentTargetIsStillInRange ||
            Time.time - _lastTimePickUpTargetWasValid > TargetLossGracePeriod)
        {
            _currentTarget = null;
        }
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

    private void UpdateOutlineColor()
    {
        Outline targetToHighlight =
        _currentTarget != null && _currentTarget.enabled
            ? _currentTarget
            : null;

        if (_highlightedOutline == targetToHighlight)
            return;

        if (_highlightedOutline != null)
            _highlightedOutline.OutlineColor = _originalOutlineColor;

        _highlightedOutline = targetToHighlight;

        if (_highlightedOutline == null)
            return;

        _originalOutlineColor = _highlightedOutline.OutlineColor;
        _highlightedOutline.OutlineColor = _highlightedOutlineColor;
    }

    public void InteractWithCurrentTarget()
    {
        if (_currentTarget == null || !IsOutlineInRange(_currentTarget))
        {
            return;
        }

        IInteractable interactable = _currentTarget.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
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
        if (outline != null && !IsOutlineInRange(outline))
        {
            outline.enabled = false;
        }
    }

    void OnDisable()
    {
        if (_highlightedOutline != null)
        {
            _highlightedOutline.OutlineColor = _originalOutlineColor;
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
