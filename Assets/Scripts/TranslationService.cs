using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Translation service for Japanese to English translation
/// Supports multiple translation APIs with fallback options
/// </summary>
public class TranslationService : MonoBehaviour
{
    [Header("Translation API Settings")]
    [SerializeField] private TranslationProvider provider = TranslationProvider.GoogleTranslate;
    [SerializeField] private string apiKey = ""; // Set in Unity Inspector or via code
    [SerializeField] private bool useCache = true;
    [SerializeField] private int maxCacheSize = 100;

    [Header("Request Settings")]
    [SerializeField] private float requestTimeout = 10f;
    [SerializeField] private int maxRetries = 3;
    [SerializeField] private float retryDelay = 1f;

    public enum TranslationProvider
    {
        GoogleTranslate,
        DeepL,
        LibreTranslate,  // Free, self-hosted option
        Mock             // For testing without API
    }

    [System.Serializable]
    public class TranslationResult
    {
        public string originalText;
        public string translatedText;
        public string sourceLanguage;
        public string targetLanguage;
        public float confidence;
        public bool fromCache;

        public TranslationResult(string original, string translated)
        {
            this.originalText = original;
            this.translatedText = translated;
            this.sourceLanguage = "ja";
            this.targetLanguage = "en";
            this.confidence = 1.0f;
            this.fromCache = false;
        }
    }

    private Dictionary<string, string> translationCache = new Dictionary<string, string>();
    private Queue<string> cacheOrder = new Queue<string>();

    public delegate void OnTranslationComplete(TranslationResult result);
    public event OnTranslationComplete OnTranslated;

    void Start()
    {
        // Load API key from PlayerPrefs if not set
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = PlayerPrefs.GetString("TranslationAPIKey", "");
        }

        Debug.Log($"[Translation] Service initialized with provider: {provider}");
    }

    /// <summary>
    /// Translate Japanese text to English
    /// </summary>
    public void TranslateText(string japaneseText, Action<TranslationResult> callback = null)
    {
        if (string.IsNullOrEmpty(japaneseText))
        {
            Debug.LogWarning("[Translation] Empty text provided");
            return;
        }

        // Check cache first
        if (useCache && translationCache.ContainsKey(japaneseText))
        {
            TranslationResult cachedResult = new TranslationResult(japaneseText, translationCache[japaneseText])
            {
                fromCache = true
            };

            callback?.Invoke(cachedResult);
            OnTranslated?.Invoke(cachedResult);
            return;
        }

        // Start translation coroutine
        StartCoroutine(TranslateCoroutine(japaneseText, callback));
    }

    /// <summary>
    /// Translate multiple texts in batch
    /// </summary>
    public void TranslateBatch(List<string> texts, Action<List<TranslationResult>> callback)
    {
        StartCoroutine(TranslateBatchCoroutine(texts, callback));
    }

    /// <summary>
    /// Translation coroutine
    /// </summary>
    private IEnumerator TranslateCoroutine(string text, Action<TranslationResult> callback)
    {
        TranslationResult result = null;
        int retries = 0;

        while (retries < maxRetries)
        {
            UnityWebRequest request = null;

            switch (provider)
            {
                case TranslationProvider.GoogleTranslate:
                    request = CreateGoogleTranslateRequest(text);
                    break;

                case TranslationProvider.DeepL:
                    request = CreateDeepLRequest(text);
                    break;

                case TranslationProvider.LibreTranslate:
                    request = CreateLibreTranslateRequest(text);
                    break;

                case TranslationProvider.Mock:
                    result = CreateMockTranslation(text);
                    break;
            }

            if (provider == TranslationProvider.Mock)
            {
                break;
            }

            if (request != null)
            {
                request.timeout = (int)requestTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    result = ParseTranslationResponse(text, request.downloadHandler.text);
                    break;
                }
                else
                {
                    Debug.LogWarning($"[Translation] Request failed (attempt {retries + 1}/{maxRetries}): {request.error}");
                    retries++;

                    if (retries < maxRetries)
                    {
                        yield return new WaitForSeconds(retryDelay);
                    }
                }

                request.Dispose();
            }
            else
            {
                break;
            }
        }

        // Fallback to mock if all retries failed
        if (result == null)
        {
            Debug.LogWarning("[Translation] All retries failed, using mock translation");
            result = CreateMockTranslation(text);
        }

        // Cache the result
        if (useCache && result != null)
        {
            AddToCache(text, result.translatedText);
        }

        // Invoke callbacks
        callback?.Invoke(result);
        OnTranslated?.Invoke(result);
    }

    /// <summary>
    /// Batch translation coroutine
    /// </summary>
    private IEnumerator TranslateBatchCoroutine(List<string> texts, Action<List<TranslationResult>> callback)
    {
        List<TranslationResult> results = new List<TranslationResult>();

        foreach (string text in texts)
        {
            bool completed = false;
            TranslationResult result = null;

            TranslateText(text, (r) =>
            {
                result = r;
                completed = true;
            });

            // Wait for completion
            while (!completed)
            {
                yield return null;
            }

            results.Add(result);
        }

        callback?.Invoke(results);
    }

    /// <summary>
    /// Create Google Translate API request
    /// </summary>
    private UnityWebRequest CreateGoogleTranslateRequest(string text)
    {
        string url = "https://translation.googleapis.com/language/translate/v2";

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("[Translation] Google Translate API key not set");
            return null;
        }

        url += $"?key={apiKey}&q={UnityWebRequest.EscapeURL(text)}&source=ja&target=en";

        UnityWebRequest request = UnityWebRequest.Get(url);
        return request;
    }

    /// <summary>
    /// Create DeepL API request
    /// </summary>
    private UnityWebRequest CreateDeepLRequest(string text)
    {
        string url = "https://api-free.deepl.com/v2/translate";

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("[Translation] DeepL API key not set");
            return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("auth_key", apiKey);
        form.AddField("text", text);
        form.AddField("source_lang", "JA");
        form.AddField("target_lang", "EN");

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        return request;
    }

    /// <summary>
    /// Create LibreTranslate API request (free, open-source option)
    /// </summary>
    private UnityWebRequest CreateLibreTranslateRequest(string text)
    {
        // Using public LibreTranslate instance
        string url = "https://libretranslate.de/translate";

        string jsonData = $"{{\"q\":\"{text}\",\"source\":\"ja\",\"target\":\"en\",\"format\":\"text\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    /// <summary>
    /// Parse translation API response
    /// </summary>
    private TranslationResult ParseTranslationResponse(string originalText, string jsonResponse)
    {
        try
        {
            // Simple JSON parsing (in production, use JsonUtility or Newtonsoft.Json)
            string translatedText = ExtractTranslatedText(jsonResponse);

            return new TranslationResult(originalText, translatedText);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Translation] Failed to parse response: {e.Message}");
            return CreateMockTranslation(originalText);
        }
    }

    /// <summary>
    /// Extract translated text from JSON response
    /// </summary>
    private string ExtractTranslatedText(string json)
    {
        // Simple extraction - looks for common translation response patterns
        if (json.Contains("\"translatedText\""))
        {
            int start = json.IndexOf("\"translatedText\":\"") + 18;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start);
        }
        else if (json.Contains("\"text\":\""))
        {
            int start = json.IndexOf("\"text\":\"") + 8;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start);
        }

        return json;
    }

    /// <summary>
    /// Create mock translation for testing
    /// </summary>
    private TranslationResult CreateMockTranslation(string japaneseText)
    {
        // Simple mock translations for common phrases
        Dictionary<string, string> mockTranslations = new Dictionary<string, string>()
        {
            { "こんにちは", "Hello" },
            { "ありがとう", "Thank you" },
            { "さようなら", "Goodbye" },
            { "お願いします", "Please" },
            { "すみません", "Excuse me" },
            { "はい", "Yes" },
            { "いいえ", "No" },
            { "おはよう", "Good morning" },
            { "こんばんは", "Good evening" },
            { "おやすみ", "Good night" }
        };

        string translated = mockTranslations.ContainsKey(japaneseText)
            ? mockTranslations[japaneseText]
            : $"[Translation of: {japaneseText}]";

        return new TranslationResult(japaneseText, translated)
        {
            confidence = 0.7f
        };
    }

    /// <summary>
    /// Add translation to cache
    /// </summary>
    private void AddToCache(string original, string translated)
    {
        if (translationCache.Count >= maxCacheSize)
        {
            string oldest = cacheOrder.Dequeue();
            translationCache.Remove(oldest);
        }

        translationCache[original] = translated;
        cacheOrder.Enqueue(original);
    }

    /// <summary>
    /// Set API key
    /// </summary>
    public void SetAPIKey(string key)
    {
        apiKey = key;
        PlayerPrefs.SetString("TranslationAPIKey", key);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Clear translation cache
    /// </summary>
    public void ClearCache()
    {
        translationCache.Clear();
        cacheOrder.Clear();
        Debug.Log("[Translation] Cache cleared");
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public string GetCacheStats()
    {
        return $"Cache: {translationCache.Count}/{maxCacheSize} entries";
    }
}
