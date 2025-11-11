using UnityEngine;

/// <summary>
/// Helper script for easy app configuration in Unity Editor
/// Attach to ARTranslatorApp GameObject for quick setup
/// </summary>
public class ConfigurationHelper : MonoBehaviour
{
    [Header("Quick Configuration")]
    [Tooltip("Select your preferred translation provider")]
    public TranslationService.TranslationProvider translationProvider = TranslationService.TranslationProvider.LibreTranslate;

    [Tooltip("Your translation API key (leave empty for LibreTranslate or Mock)")]
    public string apiKey = "";

    [Header("Detection Settings")]
    [Tooltip("Enable automatic text detection")]
    public bool autoDetection = true;

    [Tooltip("How often to detect text (seconds). Lower = faster but more battery usage")]
    [Range(0.1f, 5f)]
    public float detectionInterval = 1.0f;

    [Tooltip("Enable battery optimization mode")]
    public bool batteryOptimization = false;

    [Header("OCR Settings")]
    [Tooltip("OCR processing resolution width")]
    public int ocrWidth = 1280;

    [Tooltip("OCR processing resolution height")]
    public int ocrHeight = 720;

    [Tooltip("Enable image enhancement for better OCR")]
    public bool enhanceImages = true;

    [Header("UI Settings")]
    [Tooltip("How long translations stay visible (seconds)")]
    [Range(1f, 30f)]
    public float overlayLifetime = 5f;

    [Tooltip("Show original Japanese text above translation")]
    public bool showOriginalText = true;

    [Tooltip("Scale of translation overlays")]
    [Range(0.5f, 2f)]
    public float textScale = 1.0f;

    [Header("Performance")]
    [Tooltip("Maximum simultaneous translations")]
    [Range(1, 10)]
    public int maxSimultaneousTranslations = 5;

    [Tooltip("Enable translation caching")]
    public bool useCache = true;

    [Tooltip("Maximum cache size")]
    [Range(10, 500)]
    public int maxCacheSize = 100;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = false;

    [Tooltip("Show debug UI")]
    public bool showDebugUI = true;

    private ARTranslatorController controller;
    private TranslationService translationService;
    private JapaneseOCRDetector ocrDetector;
    private TranslationUIManager uiManager;

    void Start()
    {
        ApplyConfiguration();
    }

    /// <summary>
    /// Apply all configuration settings to components
    /// </summary>
    [ContextMenu("Apply Configuration")]
    public void ApplyConfiguration()
    {
        // Get or create components
        controller = GetComponent<ARTranslatorController>();
        if (controller == null)
        {
            Debug.LogError("[ConfigHelper] ARTranslatorController not found!");
            return;
        }

        // Get child components
        translationService = GetComponentInChildren<TranslationService>();
        ocrDetector = GetComponentInChildren<JapaneseOCRDetector>();
        uiManager = GetComponentInChildren<TranslationUIManager>();

        // Apply translation settings
        if (translationService != null)
        {
            // Use reflection or public methods to set provider and API key
            translationService.SetAPIKey(apiKey);
            Debug.Log($"[ConfigHelper] Translation provider: {translationProvider}");
        }

        // Apply UI settings
        if (uiManager != null)
        {
            uiManager.SetOverlayLifetime(overlayLifetime);
            Debug.Log($"[ConfigHelper] UI settings applied");
        }

        Debug.Log("[ConfigHelper] Configuration applied successfully!");
    }

    /// <summary>
    /// Reset to default settings
    /// </summary>
    [ContextMenu("Reset to Defaults")]
    public void ResetToDefaults()
    {
        translationProvider = TranslationService.TranslationProvider.LibreTranslate;
        apiKey = "";
        autoDetection = true;
        detectionInterval = 1.0f;
        batteryOptimization = false;
        ocrWidth = 1280;
        ocrHeight = 720;
        enhanceImages = true;
        overlayLifetime = 5f;
        showOriginalText = true;
        textScale = 1.0f;
        maxSimultaneousTranslations = 5;
        useCache = true;
        maxCacheSize = 100;
        debugMode = false;
        showDebugUI = true;

        Debug.Log("[ConfigHelper] Reset to default settings");
    }

    /// <summary>
    /// Load saved configuration from PlayerPrefs
    /// </summary>
    [ContextMenu("Load Saved Config")]
    public void LoadSavedConfiguration()
    {
        if (PlayerPrefs.HasKey("Config_Provider"))
        {
            translationProvider = (TranslationService.TranslationProvider)PlayerPrefs.GetInt("Config_Provider");
            apiKey = PlayerPrefs.GetString("Config_APIKey", "");
            detectionInterval = PlayerPrefs.GetFloat("Config_DetectionInterval", 1.0f);
            overlayLifetime = PlayerPrefs.GetFloat("Config_OverlayLifetime", 5.0f);
            textScale = PlayerPrefs.GetFloat("Config_TextScale", 1.0f);

            Debug.Log("[ConfigHelper] Configuration loaded from PlayerPrefs");
            ApplyConfiguration();
        }
        else
        {
            Debug.LogWarning("[ConfigHelper] No saved configuration found");
        }
    }

    /// <summary>
    /// Save current configuration to PlayerPrefs
    /// </summary>
    [ContextMenu("Save Current Config")]
    public void SaveConfiguration()
    {
        PlayerPrefs.SetInt("Config_Provider", (int)translationProvider);
        PlayerPrefs.SetString("Config_APIKey", apiKey);
        PlayerPrefs.SetFloat("Config_DetectionInterval", detectionInterval);
        PlayerPrefs.SetFloat("Config_OverlayLifetime", overlayLifetime);
        PlayerPrefs.SetFloat("Config_TextScale", textScale);
        PlayerPrefs.Save();

        Debug.Log("[ConfigHelper] Configuration saved to PlayerPrefs");
    }

    /// <summary>
    /// Print current configuration summary
    /// </summary>
    [ContextMenu("Print Configuration")]
    public void PrintConfiguration()
    {
        Debug.Log("=== Current Configuration ===");
        Debug.Log($"Translation Provider: {translationProvider}");
        Debug.Log($"API Key: {(string.IsNullOrEmpty(apiKey) ? "Not Set" : "***" + apiKey.Substring(apiKey.Length - 4))}");
        Debug.Log($"Auto Detection: {autoDetection}");
        Debug.Log($"Detection Interval: {detectionInterval}s");
        Debug.Log($"OCR Resolution: {ocrWidth}x{ocrHeight}");
        Debug.Log($"Overlay Lifetime: {overlayLifetime}s");
        Debug.Log($"Text Scale: {textScale}");
        Debug.Log($"Cache: {(useCache ? $"Enabled ({maxCacheSize} items)" : "Disabled")}");
        Debug.Log($"Debug Mode: {debugMode}");
        Debug.Log("=============================");
    }

    void OnValidate()
    {
        // Auto-apply when values change in inspector during play mode
        if (Application.isPlaying)
        {
            ApplyConfiguration();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Add helpful menu items in Unity Editor
    /// </summary>
    [UnityEditor.MenuItem("Quest AR Translator/Apply Configuration")]
    static void MenuApplyConfig()
    {
        var helper = FindObjectOfType<ConfigurationHelper>();
        if (helper != null)
        {
            helper.ApplyConfiguration();
        }
        else
        {
            Debug.LogWarning("ConfigurationHelper not found in scene");
        }
    }

    [UnityEditor.MenuItem("Quest AR Translator/Print Current Config")]
    static void MenuPrintConfig()
    {
        var helper = FindObjectOfType<ConfigurationHelper>();
        if (helper != null)
        {
            helper.PrintConfiguration();
        }
    }
#endif
}
