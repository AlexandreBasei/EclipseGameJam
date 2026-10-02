using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : Singleton<PlayerInteract>
{
    private const float TargetLossGracePeriod = 0.15f;

    private readonly HashSet<Collider> _collidersInRange = new HashSet<Collider>();
    private Camera _playerCamera;
    private float _lastTimePickUpTargetWasValid = float.NegativeInfinity;

    protected override void Awake()
    {
        base.Awake();
        _playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        UpdateInputTip();
    }

    private void UpdateInputTip()
    {
        if (_playerCamera == null)
        {
            Debug.LogError("PlayerInteract could not find a Camera in its children.", this);
            enabled = false;
            return;
        }

        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hasValidTarget = false;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Outline hitOutline = hit.collider.GetComponentInParent<Outline>();
            hasValidTarget = hitOutline != null && IsOutlineInRange(hitOutline);
        }

        if (hasValidTarget)
        {
            _lastTimePickUpTargetWasValid = Time.time;
        }

        bool shouldShowPickUpTip =
            hasValidTarget || Time.time - _lastTimePickUpTargetWasValid <= TargetLossGracePeriod;
        PlayerHUD.Instance.SetInputTipVisible(PlayerHUD.InputTip.PickUp, shouldShowPickUpTip);
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
        _collidersInRange.Clear();
        _lastTimePickUpTargetWasValid = float.NegativeInfinity;
        if (PlayerHUD.TryGetInstance != null)
        {
            PlayerHUD.TryGetInstance.HideInputTip(PlayerHUD.InputTip.PickUp);
        }
    }
}
