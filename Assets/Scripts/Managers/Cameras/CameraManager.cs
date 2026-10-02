using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    static List<CinemachineCamera> cameras = new List<CinemachineCamera>();

    public static CinemachineCamera ActiveCamera = null;

    public CinemachineCamera hourglassBotCam = null;
    public CinemachineCamera hourglassTopCam = null;
    public CinemachineCamera deathCamera = null;
    public CinemachineCamera playerCamera = null;
    public CinemachineCamera turnCamera = null;
    [SerializeField] private CinemachineBrain cameraBrain;
    private CinemachineImpulseSource screenShake;
    [HideInInspector] public Camera mainCamera;

    private void Awake()
    {
        base.Awake();
        cameras = new List<CinemachineCamera>();
        ActiveCamera = null;
    }
    private void Start()
    {
        mainCamera = cameraBrain.GetComponent<Camera>();
        screenShake = GetComponent<CinemachineImpulseSource>();
    }

    public static bool IsCameraActive(CinemachineCamera camera)
    {
        return ActiveCamera == camera;
    }

    public void SwitchCamera(CinemachineCamera newCamera, float transitionDuration = 0.5f)
    {
        if (newCamera == null || newCamera == ActiveCamera)
            return;

        if (cameraBrain != null)
        {
            cameraBrain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseInOut,
                Mathf.Max(0f, transitionDuration));
        }

        newCamera.Priority = 10;
        ActiveCamera = newCamera;

        foreach (CinemachineCamera camera in cameras)
        {
            if (camera != newCamera)
            {
                camera.Priority = 0;
            }
        }
    }

    public void changeMainCameraCullingMask(int newMask)
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = newMask;
        }
    }

    public void resetMainCameraCullingMask()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = -1;
        }
    }

    public void changeMainCameraBackgroundColor(Color newColor)
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = newColor;
        }
    }

    public void PlayScreenShake(float shakeIntensity)
    {
        screenShake.GenerateImpulse(shakeIntensity);
    }

    public static void Register(CinemachineCamera camera)
    {
        cameras.Add(camera);
    }

    public static void Unregister(CinemachineCamera camera)
    {
        cameras.Remove(camera);
    }
}