using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Handles input from Meta Quest 3 controllers
/// Provides button mappings for app control
/// </summary>
public class QuestInputHandler : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private bool enableHandTracking = false;
    [SerializeField] private float vibrationIntensity = 0.3f;
    [SerializeField] private float vibrationDuration = 0.1f;

    [Header("Button Mappings")]
    [SerializeField] private bool rightTriggerCapture = true;
    [SerializeField] private bool leftTriggerClear = true;
    [SerializeField] private bool aButtonToggleAuto = true;
    [SerializeField] private bool bButtonToggleUI = true;

    private ARTranslatorController appController;
    private InputDevice rightController;
    private InputDevice leftController;
    private bool controllersInitialized = false;

    // Button states for edge detection
    private bool previousRightTrigger = false;
    private bool previousLeftTrigger = false;
    private bool previousAButton = false;
    private bool previousBButton = false;
    private bool previousXButton = false;
    private bool previousYButton = false;

    void Start()
    {
        appController = GetComponent<ARTranslatorController>();

        if (appController == null)
        {
            appController = FindObjectOfType<ARTranslatorController>();
        }

        InitializeControllers();
    }

    /// <summary>
    /// Initialize Quest controllers
    /// </summary>
    private void InitializeControllers()
    {
        // Get right controller
        List<InputDevice> rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightDevices);

        if (rightDevices.Count > 0)
        {
            rightController = rightDevices[0];
            Debug.Log($"[QuestInput] Right controller found: {rightController.name}");
        }

        // Get left controller
        List<InputDevice> leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftDevices);

        if (leftDevices.Count > 0)
        {
            leftController = leftDevices[0];
            Debug.Log($"[QuestInput] Left controller found: {leftController.name}");
        }

        controllersInitialized = (rightController.isValid || leftController.isValid);

        if (!controllersInitialized)
        {
            Debug.LogWarning("[QuestInput] No controllers found, will retry...");
        }
    }

    void Update()
    {
        // Retry controller initialization if needed
        if (!controllersInitialized)
        {
            InitializeControllers();
            return;
        }

        // Handle controller input
        HandleRightController();
        HandleLeftController();
    }

    /// <summary>
    /// Handle right controller input
    /// </summary>
    private void HandleRightController()
    {
        if (!rightController.isValid) return;

        // Right Trigger - Capture/Translate
        bool rightTrigger = false;
        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger))
        {
            if (rightTrigger && !previousRightTrigger && rightTriggerCapture)
            {
                OnRightTriggerPressed();
            }
            previousRightTrigger = rightTrigger;
        }

        // A Button - Toggle Auto-detection
        bool aButton = false;
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out aButton))
        {
            if (aButton && !previousAButton && aButtonToggleAuto)
            {
                OnAButtonPressed();
            }
            previousAButton = aButton;
        }

        // B Button - Toggle UI
        bool bButton = false;
        if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bButton))
        {
            if (bButton && !previousBButton && bButtonToggleUI)
            {
                OnBButtonPressed();
            }
            previousBButton = bButton;
        }

        // Right Thumbstick - Could be used for overlay scaling
        Vector2 thumbstick;
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstick))
        {
            if (thumbstick.magnitude > 0.5f)
            {
                // Future feature: scale overlays
            }
        }
    }

    /// <summary>
    /// Handle left controller input
    /// </summary>
    private void HandleLeftController()
    {
        if (!leftController.isValid) return;

        // Left Trigger - Clear translations
        bool leftTrigger = false;
        if (leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger))
        {
            if (leftTrigger && !previousLeftTrigger && leftTriggerClear)
            {
                OnLeftTriggerPressed();
            }
            previousLeftTrigger = leftTrigger;
        }

        // X Button - Additional function
        bool xButton = false;
        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out xButton))
        {
            if (xButton && !previousXButton)
            {
                OnXButtonPressed();
            }
            previousXButton = xButton;
        }

        // Y Button - Additional function
        bool yButton = false;
        if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out yButton))
        {
            if (yButton && !previousYButton)
            {
                OnYButtonPressed();
            }
            previousYButton = yButton;
        }
    }

    /// <summary>
    /// Right trigger pressed - Manual capture/translate
    /// </summary>
    private void OnRightTriggerPressed()
    {
        Debug.Log("[QuestInput] Right trigger pressed - Manual capture");

        if (appController != null)
        {
            appController.TriggerManualDetection();
        }

        VibrateController(rightController);
    }

    /// <summary>
    /// Left trigger pressed - Clear all translations
    /// </summary>
    private void OnLeftTriggerPressed()
    {
        Debug.Log("[QuestInput] Left trigger pressed - Clear translations");

        if (appController != null)
        {
            appController.ClearAllTranslations();
        }

        VibrateController(leftController);
    }

    /// <summary>
    /// A button pressed - Toggle auto-detection
    /// </summary>
    private void OnAButtonPressed()
    {
        Debug.Log("[QuestInput] A button pressed - Toggle auto-detection");

        if (appController != null)
        {
            appController.ToggleAutoDetection();
        }

        VibrateController(rightController);
    }

    /// <summary>
    /// B button pressed - Toggle UI visibility
    /// </summary>
    private void OnBButtonPressed()
    {
        Debug.Log("[QuestInput] B button pressed - Toggle UI");

        // Future feature: Toggle UI visibility

        VibrateController(rightController);
    }

    /// <summary>
    /// X button pressed - Additional function
    /// </summary>
    private void OnXButtonPressed()
    {
        Debug.Log("[QuestInput] X button pressed");

        // Future feature: Could be used for settings menu

        VibrateController(leftController);
    }

    /// <summary>
    /// Y button pressed - Additional function
    /// </summary>
    private void OnYButtonPressed()
    {
        Debug.Log("[QuestInput] Y button pressed");

        // Future feature: Could be used for language selection

        VibrateController(leftController);
    }

    /// <summary>
    /// Provide haptic feedback to controller
    /// </summary>
    private void VibrateController(InputDevice controller)
    {
        if (controller.isValid)
        {
            HapticCapabilities capabilities;
            if (controller.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    controller.SendHapticImpulse(0, vibrationIntensity, vibrationDuration);
                }
            }
        }
    }

    /// <summary>
    /// Get controller status string
    /// </summary>
    public string GetControllerStatus()
    {
        return $"Right: {(rightController.isValid ? "Connected" : "Disconnected")}\n" +
               $"Left: {(leftController.isValid ? "Connected" : "Disconnected")}";
    }

    /// <summary>
    /// Check if hand tracking is available
    /// </summary>
    private bool IsHandTrackingAvailable()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Check for hand tracking support
        try
        {
            using (AndroidJavaClass ovrPlugin = new AndroidJavaClass("com.oculus.plugin.OVRPlugin"))
            {
                return ovrPlugin.CallStatic<bool>("GetHandTrackingEnabled");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[QuestInput] Hand tracking check failed: {e.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }

    /// <summary>
    /// Handle device connection
    /// </summary>
    private void OnDeviceConnected(InputDevice device)
    {
        Debug.Log($"[QuestInput] Device connected: {device.name}");
        InitializeControllers();
    }

    /// <summary>
    /// Handle device disconnection
    /// </summary>
    private void OnDeviceDisconnected(InputDevice device)
    {
        Debug.Log($"[QuestInput] Device disconnected: {device.name}");
        controllersInitialized = false;
    }
}
