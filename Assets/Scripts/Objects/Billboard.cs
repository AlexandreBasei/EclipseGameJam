
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (playerCamera != null)
        {
            transform.forward = playerCamera.transform.forward;
        }
    }
}