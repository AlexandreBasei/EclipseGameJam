using UnityEngine;
using System.Collections.Generic;

public class SnapPoint : MonoBehaviour
{
    public Item currentItem;
    public SnapPoint snapPointInContact;
    public BoxCollider collider;

    private readonly HashSet<SnapPoint> _contactingSnapPoints = new HashSet<SnapPoint>();

    void Awake()
    {
        currentItem = GetComponentInParent<Item>();
        collider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint != null &&
            otherSnapPoint != this &&
            otherSnapPoint.currentItem != null)
        {
            _contactingSnapPoints.Add(otherSnapPoint);
            snapPointInContact = otherSnapPoint;
        }
    }

    void OnTriggerExit(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint == null)
            return;

        _contactingSnapPoints.Remove(otherSnapPoint);
        if (snapPointInContact == otherSnapPoint)
        {
            snapPointInContact = null;
            foreach (SnapPoint contactingSnapPoint in _contactingSnapPoints)
            {
                if (contactingSnapPoint != null)
                {
                    snapPointInContact = contactingSnapPoint;
                    break;
                }
            }
        }
    }

    public bool TryGetContact(out SnapPoint contactingSnapPoint)
    {
        _contactingSnapPoints.RemoveWhere(point => !IsContactValid(point));
        if (currentItem == null || collider == null || !collider.enabled)
        {
            snapPointInContact = null;
            contactingSnapPoint = null;
            return false;
        }

        foreach (SnapPoint point in _contactingSnapPoints)
        {
            if (point.currentItem != null &&
                point.currentItem.GetAssemblyRoot() != currentItem.GetAssemblyRoot())
            {
                snapPointInContact = point;
                contactingSnapPoint = point;
                return true;
            }
        }

        snapPointInContact = null;
        contactingSnapPoint = null;
        return false;
    }

    private bool IsContactValid(SnapPoint point)
    {
        return point != null &&
               point.currentItem != null &&
               point.collider != null &&
               collider != null &&
               point.collider.enabled &&
               collider.enabled &&
               gameObject.activeInHierarchy &&
               point.gameObject.activeInHierarchy &&
               Physics.ComputePenetration(
                   collider,
                   collider.transform.position,
                   collider.transform.rotation,
                   point.collider,
                   point.collider.transform.position,
                   point.collider.transform.rotation,
                   out _,
                   out _);
    }
}
