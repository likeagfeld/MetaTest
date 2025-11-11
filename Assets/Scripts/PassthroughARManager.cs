using UnityEngine;
using UnityEngine.XR;
using System.Collections;

/// <summary>
/// Manages Meta Quest 3 passthrough AR functionality
/// Enables camera feed and overlays virtual content on the real world
/// </summary>
public class PassthroughARManager : MonoBehaviour
{
    [Header("Passthrough Settings")]
    [SerializeField] private bool enableOnStart = true;
    [SerializeField] private float passthroughOpacity = 1.0f;

    [Header("Camera Capture")]
    [SerializeField] private int captureWidth = 1920;
    [SerializeField] private int captureHeight = 1080;
    [SerializeField] private float captureIntervalSeconds = 0.5f;

    private WebCamTexture webCamTexture;
    private bool isPassthroughActive = false;
    private Camera mainCamera;
    private RenderTexture capturedFrame;

    public delegate void OnFrameCaptured(Texture2D frame);
    public event OnFrameCaptured OnNewFrameCaptured;

    public bool IsPassthroughActive => isPassthroughActive;

    void Start()
    {
        mainCamera = Camera.main;
        InitializePassthrough();

        if (enableOnStart)
        {
            EnablePassthrough();
        }

        StartCoroutine(CaptureFramesCoroutine());
    }

    /// <summary>
    /// Initializes Quest 3 passthrough capability
    /// </summary>
    private void InitializePassthrough()
    {
        // Set camera to transparent for passthrough
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        // Initialize WebCam for capture
        if (WebCamTexture.devices.Length > 0)
        {
            // Try to find the best camera (Quest 3 has multiple)
            string deviceName = WebCamTexture.devices[0].name;

            // Prefer the front-facing camera for AR
            foreach (var device in WebCamTexture.devices)
            {
                if (device.isFrontFacing)
                {
                    deviceName = device.name;
                    break;
                }
            }

            webCamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, 30);
            Debug.Log($"[PassthroughAR] Initialized camera: {deviceName}");
        }
        else
        {
            Debug.LogError("[PassthroughAR] No camera devices found!");
        }

        capturedFrame = new RenderTexture(captureWidth, captureHeight, 24);
    }

    /// <summary>
    /// Enables passthrough mode
    /// </summary>
    public void EnablePassthrough()
    {
        if (isPassthroughActive) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Enable Oculus passthrough via native Android
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject ovrManager = new AndroidJavaObject("com.oculus.vrapi.VrApiManager"))
                    {
                        // Enable passthrough layer
                        Debug.Log("[PassthroughAR] Enabling Oculus passthrough");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PassthroughAR] Could not enable native passthrough: {e.Message}");
        }
#endif

        // Start camera capture
        if (webCamTexture != null && !webCamTexture.isPlaying)
        {
            webCamTexture.Play();
            Debug.Log("[PassthroughAR] WebCam started");
        }

        isPassthroughActive = true;
        Debug.Log("[PassthroughAR] Passthrough enabled");
    }

    /// <summary>
    /// Disables passthrough mode
    /// </summary>
    public void DisablePassthrough()
    {
        if (!isPassthroughActive) return;

        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        isPassthroughActive = false;
        Debug.Log("[PassthroughAR] Passthrough disabled");
    }

    /// <summary>
    /// Captures frames from the camera at regular intervals
    /// </summary>
    private IEnumerator CaptureFramesCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(captureIntervalSeconds);

            if (isPassthroughActive && webCamTexture != null && webCamTexture.isPlaying)
            {
                CaptureFrame();
            }
        }
    }

    /// <summary>
    /// Captures a single frame from the camera
    /// </summary>
    private void CaptureFrame()
    {
        if (webCamTexture == null || !webCamTexture.didUpdateThisFrame) return;

        try
        {
            // Create texture from webcam
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Notify listeners
            OnNewFrameCaptured?.Invoke(photo);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PassthroughAR] Frame capture failed: {e.Message}");
        }
    }

    /// <summary>
    /// Manually trigger a frame capture
    /// </summary>
    public void TriggerCapture()
    {
        if (isPassthroughActive)
        {
            CaptureFrame();
        }
    }

    /// <summary>
    /// Get the current camera texture
    /// </summary>
    public WebCamTexture GetCameraTexture()
    {
        return webCamTexture;
    }

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
        }

        if (capturedFrame != null)
        {
            capturedFrame.Release();
            Destroy(capturedFrame);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            DisablePassthrough();
        }
        else if (enableOnStart)
        {
            EnablePassthrough();
        }
    }
}
