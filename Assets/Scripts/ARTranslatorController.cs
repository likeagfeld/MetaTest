using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main controller for the AR Japanese-to-English translator app
/// Coordinates passthrough AR, OCR detection, translation, and UI display
/// </summary>
public class ARTranslatorController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PassthroughARManager passthroughManager;
    [SerializeField] private JapaneseOCRDetector ocrDetector;
    [SerializeField] private TranslationService translationService;
    [SerializeField] private TranslationUIManager uiManager;

    [Header("Detection Settings")]
    [SerializeField] private bool autoDetectEnabled = true;
    [SerializeField] private float detectionInterval = 1.0f;
    [SerializeField] private bool continuousMode = true;

    [Header("Performance")]
    [SerializeField] private int maxSimultaneousTranslations = 5;
    [SerializeField] private bool optimizeForBattery = false;

    private float lastDetectionTime = 0f;
    private int activeTranslations = 0;
    private bool isInitialized = false;

    private List<string> recentDetections = new List<string>();
    private int maxRecentDetections = 10;

    void Start()
    {
        InitializeComponents();
        StartApp();
    }

    /// <summary>
    /// Initialize all components
    /// </summary>
    private void InitializeComponents()
    {
        // Create components if not assigned
        if (passthroughManager == null)
        {
            GameObject pmObj = new GameObject("PassthroughARManager");
            pmObj.transform.SetParent(transform);
            passthroughManager = pmObj.AddComponent<PassthroughARManager>();
        }

        if (ocrDetector == null)
        {
            GameObject ocrObj = new GameObject("JapaneseOCRDetector");
            ocrObj.transform.SetParent(transform);
            ocrDetector = ocrObj.AddComponent<JapaneseOCRDetector>();
        }

        if (translationService == null)
        {
            GameObject tsObj = new GameObject("TranslationService");
            tsObj.transform.SetParent(transform);
            translationService = tsObj.AddComponent<TranslationService>();
        }

        if (uiManager == null)
        {
            GameObject uiObj = new GameObject("TranslationUIManager");
            uiObj.transform.SetParent(transform);
            uiManager = uiObj.AddComponent<TranslationUIManager>();
        }

        // Subscribe to events
        if (passthroughManager != null)
        {
            passthroughManager.OnNewFrameCaptured += HandleFrameCaptured;
        }

        if (ocrDetector != null)
        {
            ocrDetector.OnTextRecognized += HandleTextDetected;
        }

        if (translationService != null)
        {
            translationService.OnTranslated += HandleTranslationComplete;
        }

        isInitialized = true;
        Debug.Log("[ARTranslator] All components initialized");
    }

    /// <summary>
    /// Start the application
    /// </summary>
    private void StartApp()
    {
        if (!isInitialized)
        {
            Debug.LogError("[ARTranslator] Components not initialized!");
            return;
        }

        // Enable passthrough
        if (passthroughManager != null)
        {
            passthroughManager.EnablePassthrough();
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateStatus("AR Translator Active - Point at Japanese text");
        }

        Debug.Log("[ARTranslator] Application started successfully");
    }

    /// <summary>
    /// Handle new camera frame captured
    /// </summary>
    private void HandleFrameCaptured(Texture2D frame)
    {
        if (!autoDetectEnabled || !continuousMode) return;

        // Throttle detection rate
        if (Time.time - lastDetectionTime < detectionInterval) return;

        // Check if we can process more
        if (ocrDetector.GetQueueLength() > 3) return;

        lastDetectionTime = Time.time;

        // Perform OCR detection
        ocrDetector.DetectText(frame);

        if (uiManager != null)
        {
            uiManager.UpdateStatus($"Scanning... (Queue: {ocrDetector.GetQueueLength()})");
        }
    }

    /// <summary>
    /// Handle text detected from OCR
    /// </summary>
    private void HandleTextDetected(List<JapaneseOCRDetector.DetectedText> detectedTexts)
    {
        if (detectedTexts == null || detectedTexts.Count == 0)
        {
            if (uiManager != null)
            {
                uiManager.UpdateStatus("No text detected");
            }
            return;
        }

        Debug.Log($"[ARTranslator] Detected {detectedTexts.Count} text regions");

        // Process each detected text
        foreach (var detection in detectedTexts)
        {
            // Skip if already processing too many
            if (activeTranslations >= maxSimultaneousTranslations)
            {
                Debug.Log("[ARTranslator] Max simultaneous translations reached, queuing...");
                break;
            }

            // Skip if recently detected (avoid duplicates)
            if (recentDetections.Contains(detection.text))
            {
                continue;
            }

            // Add to recent detections
            AddToRecentDetections(detection.text);

            // Calculate world position for overlay
            Vector3 worldPos = CalculateWorldPosition(detection.boundingBox);

            // Translate the text
            TranslateDetectedText(detection, worldPos);
        }

        if (uiManager != null)
        {
            uiManager.UpdateStatus($"Processing {detectedTexts.Count} text regions...");
        }
    }

    /// <summary>
    /// Translate detected text
    /// </summary>
    private void TranslateDetectedText(JapaneseOCRDetector.DetectedText detection, Vector3 worldPosition)
    {
        activeTranslations++;

        translationService.TranslateText(detection.text, (result) =>
        {
            activeTranslations--;

            // Display translation in UI
            if (uiManager != null && result != null)
            {
                uiManager.ShowTranslation(
                    result.originalText,
                    result.translatedText,
                    worldPosition
                );
            }

            Debug.Log($"[ARTranslator] Translated: '{detection.text}' -> '{result.translatedText}'");
        });
    }

    /// <summary>
    /// Handle translation completion
    /// </summary>
    private void HandleTranslationComplete(TranslationService.TranslationResult result)
    {
        if (uiManager != null)
        {
            string cacheInfo = result.fromCache ? " (cached)" : "";
            uiManager.UpdateStatus($"Translated{cacheInfo}: {result.translatedText}");
        }
    }

    /// <summary>
    /// Calculate world position from bounding box
    /// </summary>
    private Vector3 CalculateWorldPosition(Rect boundingBox)
    {
        // Calculate center of bounding box
        float centerX = boundingBox.center.x;
        float centerY = boundingBox.center.y;

        // Convert to world space (simplified - assumes text is at fixed distance)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 screenPoint = new Vector3(centerX, centerY, 2.0f);
            return mainCam.ScreenToWorldPoint(screenPoint);
        }

        return Vector3.forward * 2f;
    }

    /// <summary>
    /// Add text to recent detections list
    /// </summary>
    private void AddToRecentDetections(string text)
    {
        recentDetections.Add(text);

        if (recentDetections.Count > maxRecentDetections)
        {
            recentDetections.RemoveAt(0);
        }
    }

    /// <summary>
    /// Manually trigger detection
    /// </summary>
    public void TriggerManualDetection()
    {
        if (passthroughManager != null)
        {
            passthroughManager.TriggerCapture();
        }

        if (uiManager != null)
        {
            uiManager.UpdateStatus("Manual detection triggered");
        }
    }

    /// <summary>
    /// Toggle auto-detection
    /// </summary>
    public void ToggleAutoDetection()
    {
        autoDetectEnabled = !autoDetectEnabled;

        if (uiManager != null)
        {
            uiManager.UpdateStatus($"Auto-detection: {(autoDetectEnabled ? "ON" : "OFF")}");
        }

        Debug.Log($"[ARTranslator] Auto-detection: {autoDetectEnabled}");
    }

    /// <summary>
    /// Clear all translations
    /// </summary>
    public void ClearAllTranslations()
    {
        if (uiManager != null)
        {
            uiManager.ClearAllOverlays();
        }

        recentDetections.Clear();

        Debug.Log("[ARTranslator] All translations cleared");
    }

    /// <summary>
    /// Set translation API key
    /// </summary>
    public void SetTranslationAPIKey(string apiKey)
    {
        if (translationService != null)
        {
            translationService.SetAPIKey(apiKey);
            Debug.Log("[ARTranslator] API key updated");
        }
    }

    /// <summary>
    /// Get application status
    /// </summary>
    public string GetStatus()
    {
        return $"Passthrough: {(passthroughManager != null && passthroughManager.IsPassthroughActive ? "ON" : "OFF")}\n" +
               $"Auto-detect: {(autoDetectEnabled ? "ON" : "OFF")}\n" +
               $"OCR Queue: {(ocrDetector != null ? ocrDetector.GetQueueLength() : 0)}\n" +
               $"Active Translations: {activeTranslations}\n" +
               $"Translation Cache: {(translationService != null ? translationService.GetCacheStats() : "N/A")}";
    }

    void Update()
    {
        // Performance optimization
        if (optimizeForBattery)
        {
            // Reduce detection rate when battery saving is on
            detectionInterval = 2.0f;
        }

        // Handle input (will be enhanced by QuestInputHandler)
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (passthroughManager != null)
        {
            passthroughManager.OnNewFrameCaptured -= HandleFrameCaptured;
        }

        if (ocrDetector != null)
        {
            ocrDetector.OnTextRecognized -= HandleTextDetected;
        }

        if (translationService != null)
        {
            translationService.OnTranslated -= HandleTranslationComplete;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // App is going to background
            autoDetectEnabled = false;

            if (uiManager != null)
            {
                uiManager.UpdateStatus("App paused");
            }
        }
        else
        {
            // App is resuming
            autoDetectEnabled = true;

            if (uiManager != null)
            {
                uiManager.UpdateStatus("App resumed");
            }
        }
    }
}
