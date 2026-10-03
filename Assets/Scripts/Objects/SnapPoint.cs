using UnityEngine;

public class SnapPoint : MonoBehaviour
{
     public Item currentItem;
     public SnapPoint snapPointInContact;
     public BoxCollider collider;

    void Start()
    {
        currentItem = GetComponentInParent<Item>();
        collider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint != null && otherSnapPoint.currentItem != null)
        {
            snapPointInContact = otherSnapPoint;
            print("Snap point contacted!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint != null && snapPointInContact == otherSnapPoint)
        {
            snapPointInContact = null;
            print("Snap point exited!");
        }
    }
}
