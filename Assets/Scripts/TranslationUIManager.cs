using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the UI for displaying translations in AR
/// Creates floating text overlays at detected text positions
/// </summary>
public class TranslationUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject translationPanelPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Overlay Settings")]
    [SerializeField] private Color originalTextColor = new Color(1f, 1f, 0f, 0.8f); // Yellow
    [SerializeField] private Color translatedTextColor = new Color(0f, 1f, 0f, 1f); // Green
    [SerializeField] private float overlayLifetime = 5f;
    [SerializeField] private bool showOriginalText = true;
    [SerializeField] private float textScale = 1.0f;

    [Header("Animation")]
    [SerializeField] private bool animateAppearance = true;
    [SerializeField] private float fadeInDuration = 0.3f;

    private Dictionary<string, TranslationOverlay> activeOverlays = new Dictionary<string, TranslationOverlay>();
    private List<string> overlaysToRemove = new List<string>();
    private Camera mainCamera;

    private class TranslationOverlay
    {
        public GameObject gameObject;
        public TextMeshProUGUI originalText;
        public TextMeshProUGUI translatedText;
        public Image background;
        public float creationTime;
        public Vector3 worldPosition;
        public CanvasGroup canvasGroup;
    }

    void Start()
    {
        mainCamera = Camera.main;

        // Create main canvas if not assigned
        if (mainCanvas == null)
        {
            CreateMainCanvas();
        }

        // Create translation panel prefab if not assigned
        if (translationPanelPrefab == null)
        {
            CreateTranslationPanelPrefab();
        }

        // Create status UI
        CreateStatusUI();
    }

    /// <summary>
    /// Create the main canvas for AR overlay
    /// </summary>
    private void CreateMainCanvas()
    {
        GameObject canvasObj = new GameObject("TranslationCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("[TranslationUI] Main canvas created");
    }

    /// <summary>
    /// Create prefab for translation panels
    /// </summary>
    private void CreateTranslationPanelPrefab()
    {
        // Create panel
        GameObject panel = new GameObject("TranslationPanel");
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 120);

        // Add background
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);

        // Add canvas group for fading
        panel.AddComponent<CanvasGroup>();

        // Create original text
        GameObject originalTextObj = new GameObject("OriginalText");
        originalTextObj.transform.SetParent(panel.transform);
        RectTransform originalRect = originalTextObj.AddComponent<RectTransform>();
        originalRect.anchorMin = new Vector2(0, 0.5f);
        originalRect.anchorMax = new Vector2(1, 1);
        originalRect.offsetMin = new Vector2(10, 0);
        originalRect.offsetMax = new Vector2(-10, -5);

        TextMeshProUGUI originalTMP = originalTextObj.AddComponent<TextMeshProUGUI>();
        originalTMP.fontSize = 24;
        originalTMP.color = originalTextColor;
        originalTMP.alignment = TextAlignmentOptions.Center;
        originalTMP.fontStyle = FontStyles.Bold;

        // Create translated text
        GameObject translatedTextObj = new GameObject("TranslatedText");
        translatedTextObj.transform.SetParent(panel.transform);
        RectTransform translatedRect = translatedTextObj.AddComponent<RectTransform>();
        translatedRect.anchorMin = new Vector2(0, 0);
        translatedRect.anchorMax = new Vector2(1, 0.5f);
        translatedRect.offsetMin = new Vector2(10, 5);
        translatedRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI translatedTMP = translatedTextObj.AddComponent<TextMeshProUGUI>();
        translatedTMP.fontSize = 28;
        translatedTMP.color = translatedTextColor;
        translatedTMP.alignment = TextAlignmentOptions.Center;
        translatedTMP.fontStyle = FontStyles.Bold;

        translationPanelPrefab = panel;
        translationPanelPrefab.SetActive(false);

        Debug.Log("[TranslationUI] Translation panel prefab created");
    }

    /// <summary>
    /// Create status UI elements
    /// </summary>
    private void CreateStatusUI()
    {
        // Status text (top of screen)
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(mainCanvas.transform);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.anchoredPosition = new Vector2(0, -20);
        statusRect.sizeDelta = new Vector2(-40, 60);

        statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 24;
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Top;
        statusText.text = "AR Translator Ready";

        // Debug text (bottom of screen)
        GameObject debugObj = new GameObject("DebugText");
        debugObj.transform.SetParent(mainCanvas.transform);
        RectTransform debugRect = debugObj.AddComponent<RectTransform>();
        debugRect.anchorMin = new Vector2(0, 0);
        debugRect.anchorMax = new Vector2(1, 0);
        debugRect.pivot = new Vector2(0.5f, 0);
        debugRect.anchoredPosition = new Vector2(0, 20);
        debugRect.sizeDelta = new Vector2(-40, 100);

        debugText = debugObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 18;
        debugText.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
        debugText.alignment = TextAlignmentOptions.BottomLeft;
        debugText.text = "";
    }

    /// <summary>
    /// Display a translation overlay
    /// </summary>
    public void ShowTranslation(string originalText, string translatedText, Vector3 worldPosition)
    {
        string key = $"{originalText}_{worldPosition.GetHashCode()}";

        // Remove existing overlay with same key
        if (activeOverlays.ContainsKey(key))
        {
            RemoveOverlay(key);
        }

        // Create new overlay
        GameObject overlayObj = Instantiate(translationPanelPrefab, mainCanvas.transform);
        overlayObj.SetActive(true);

        TranslationOverlay overlay = new TranslationOverlay
        {
            gameObject = overlayObj,
            originalText = overlayObj.transform.Find("OriginalText").GetComponent<TextMeshProUGUI>(),
            translatedText = overlayObj.transform.Find("TranslatedText").GetComponent<TextMeshProUGUI>(),
            background = overlayObj.GetComponent<Image>(),
            canvasGroup = overlayObj.GetComponent<CanvasGroup>(),
            creationTime = Time.time,
            worldPosition = worldPosition
        };

        // Set text
        overlay.originalText.text = showOriginalText ? originalText : "";
        overlay.translatedText.text = translatedText;

        // Set scale
        overlayObj.transform.localScale = Vector3.one * textScale;

        // Position overlay
        UpdateOverlayPosition(overlay);

        // Animate appearance
        if (animateAppearance)
        {
            overlay.canvasGroup.alpha = 0;
            StartCoroutine(FadeInOverlay(overlay));
        }

        activeOverlays[key] = overlay;

        Debug.Log($"[TranslationUI] Showing: '{originalText}' -> '{translatedText}'");
    }

    /// <summary>
    /// Fade in overlay animation
    /// </summary>
    private System.Collections.IEnumerator FadeInOverlay(TranslationOverlay overlay)
    {
        float elapsed = 0;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            overlay.canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            yield return null;
        }

        overlay.canvasGroup.alpha = 1;
    }

    /// <summary>
    /// Update overlay position based on world position
    /// </summary>
    private void UpdateOverlayPosition(TranslationOverlay overlay)
    {
        if (mainCamera == null) return;

        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(overlay.worldPosition);

        // If behind camera, don't show
        if (screenPos.z < 0)
        {
            overlay.gameObject.SetActive(false);
            return;
        }

        overlay.gameObject.SetActive(true);

        // Set position
        RectTransform rect = overlay.gameObject.GetComponent<RectTransform>();
        rect.position = screenPos;
    }

    /// <summary>
    /// Remove an overlay
    /// </summary>
    private void RemoveOverlay(string key)
    {
        if (activeOverlays.ContainsKey(key))
        {
            Destroy(activeOverlays[key].gameObject);
            activeOverlays.Remove(key);
        }
    }

    /// <summary>
    /// Update status text
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }

    /// <summary>
    /// Update debug text
    /// </summary>
    public void UpdateDebug(string debug)
    {
        if (debugText != null)
        {
            debugText.text = debug;
        }
    }

    /// <summary>
    /// Clear all overlays
    /// </summary>
    public void ClearAllOverlays()
    {
        foreach (var overlay in activeOverlays.Values)
        {
            Destroy(overlay.gameObject);
        }

        activeOverlays.Clear();
        Debug.Log("[TranslationUI] All overlays cleared");
    }

    /// <summary>
    /// Set overlay lifetime
    /// </summary>
    public void SetOverlayLifetime(float lifetime)
    {
        overlayLifetime = lifetime;
    }

    void Update()
    {
        // Update overlay positions
        foreach (var overlay in activeOverlays.Values)
        {
            UpdateOverlayPosition(overlay);
        }

        // Remove expired overlays
        overlaysToRemove.Clear();

        foreach (var kvp in activeOverlays)
        {
            if (Time.time - kvp.Value.creationTime > overlayLifetime)
            {
                overlaysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in overlaysToRemove)
        {
            RemoveOverlay(key);
        }

        // Update debug info
        if (debugText != null)
        {
            UpdateDebug($"Active Overlays: {activeOverlays.Count}");
        }
    }

    void OnDestroy()
    {
        ClearAllOverlays();
    }
}
