using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

/// <summary>
/// Detects Japanese text from images using OCR
/// Supports Hiragana, Katakana, and Kanji character recognition
/// </summary>
public class JapaneseOCRDetector : MonoBehaviour
{
    [Header("OCR Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float confidenceThreshold = 0.6f;
    [SerializeField] private int maxConcurrentDetections = 3;

    [Header("Image Processing")]
    [SerializeField] private int processWidth = 1280;
    [SerializeField] private int processHeight = 720;
    [SerializeField] private bool enhanceContrast = true;

    private Queue<OCRRequest> requestQueue = new Queue<OCRRequest>();
    private bool isProcessing = false;
    private int activeDetections = 0;

    public delegate void OnTextDetected(List<DetectedText> detectedTexts);
    public event OnTextDetected OnTextRecognized;

    [System.Serializable]
    public class DetectedText
    {
        public string text;
        public Rect boundingBox;
        public float confidence;
        public Vector2 worldPosition;

        public DetectedText(string text, Rect box, float conf)
        {
            this.text = text;
            this.boundingBox = box;
            this.confidence = conf;
        }
    }

    private class OCRRequest
    {
        public Texture2D image;
        public Action<List<DetectedText>> callback;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Native Android plugin interface for Tesseract OCR
    private AndroidJavaObject tesseractInstance;
    private bool isTesseractInitialized = false;
#endif

    void Start()
    {
        InitializeOCR();
    }

    /// <summary>
    /// Initializes the OCR engine with Japanese language support
    /// </summary>
    private void InitializeOCR()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Initialize Tesseract for Japanese
            using (AndroidJavaClass tesseractClass = new AndroidJavaClass("com.googlecode.tesseract.android.TessBaseAPI"))
            {
                tesseractInstance = new AndroidJavaObject("com.googlecode.tesseract.android.TessBaseAPI");

                // Set data path for Japanese trained data
                string dataPath = Application.persistentDataPath + "/tessdata/";

                // Initialize with Japanese + English
                bool initSuccess = tesseractInstance.Call<bool>("init", dataPath, "jpn+eng");

                if (initSuccess)
                {
                    isTesseractInitialized = true;
                    Debug.Log("[JapaneseOCR] Tesseract initialized successfully");
                }
                else
                {
                    Debug.LogError("[JapaneseOCR] Tesseract initialization failed");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JapaneseOCR] Failed to initialize Tesseract: {e.Message}");
            // Fall back to simulated detection for testing
            isTesseractInitialized = false;
        }
#else
        Debug.Log("[JapaneseOCR] Running in editor mode - using simulated detection");
#endif
        StartCoroutine(ProcessRequestQueue());
    }

    /// <summary>
    /// Detect Japanese text from an image
    /// </summary>
    public void DetectText(Texture2D image, Action<List<DetectedText>> callback = null)
    {
        if (image == null)
        {
            Debug.LogWarning("[JapaneseOCR] Null image provided");
            return;
        }

        OCRRequest request = new OCRRequest
        {
            image = image,
            callback = callback ?? ((texts) => OnTextRecognized?.Invoke(texts))
        };

        requestQueue.Enqueue(request);
    }

    /// <summary>
    /// Process queued OCR requests
    /// </summary>
    private IEnumerator ProcessRequestQueue()
    {
        while (true)
        {
            if (requestQueue.Count > 0 && activeDetections < maxConcurrentDetections)
            {
                OCRRequest request = requestQueue.Dequeue();
                StartCoroutine(ProcessOCRRequest(request));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Process a single OCR request
    /// </summary>
    private IEnumerator ProcessOCRRequest(OCRRequest request)
    {
        activeDetections++;
        isProcessing = true;

        // Preprocess image
        Texture2D processedImage = PreprocessImage(request.image);

        // Perform OCR
        List<DetectedText> results = new List<DetectedText>();

#if UNITY_ANDROID && !UNITY_EDITOR
        if (isTesseractInitialized)
        {
            results = PerformNativeOCR(processedImage);
        }
        else
        {
            results = PerformSimulatedOCR(processedImage);
        }
#else
        // Editor mode - use simulated detection
        results = PerformSimulatedOCR(processedImage);
#endif

        // Clean up
        if (processedImage != request.image)
        {
            Destroy(processedImage);
        }

        // Invoke callback
        if (request.callback != null)
        {
            request.callback.Invoke(results);
        }

        activeDetections--;
        isProcessing = false;

        yield return null;
    }

    /// <summary>
    /// Preprocess image for better OCR accuracy
    /// </summary>
    private Texture2D PreprocessImage(Texture2D original)
    {
        // Resize if needed
        Texture2D processed = original;

        if (original.width != processWidth || original.height != processHeight)
        {
            processed = ResizeTexture(original, processWidth, processHeight);
        }

        if (enhanceContrast)
        {
            processed = EnhanceContrast(processed);
        }

        return processed;
    }

    /// <summary>
    /// Resize texture to target dimensions
    /// </summary>
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    /// <summary>
    /// Enhance image contrast for better text detection
    /// </summary>
    private Texture2D EnhanceContrast(Texture2D source)
    {
        Color[] pixels = source.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            // Convert to grayscale
            float gray = pixels[i].grayscale;

            // Apply contrast enhancement
            gray = Mathf.Clamp01((gray - 0.5f) * 1.5f + 0.5f);

            pixels[i] = new Color(gray, gray, gray, 1f);
        }

        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        result.SetPixels(pixels);
        result.Apply();

        return result;
    }

    /// <summary>
    /// Perform OCR using native Tesseract on Android
    /// </summary>
    private List<DetectedText> PerformNativeOCR(Texture2D image)
    {
        List<DetectedText> results = new List<DetectedText>();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Convert Unity texture to Android Bitmap
            byte[] imageBytes = image.EncodeToJPG();

            using (AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory"))
            {
                AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>(
                    "decodeByteArray", imageBytes, 0, imageBytes.Length);

                // Set image in Tesseract
                tesseractInstance.Call("setImage", bitmap);

                // Get recognized text
                string recognizedText = tesseractInstance.Call<string>("getUTF8Text");

                if (!string.IsNullOrEmpty(recognizedText))
                {
                    // Parse results (in production, you'd get bounding boxes too)
                    results.Add(new DetectedText(
                        recognizedText.Trim(),
                        new Rect(0, 0, image.width, image.height),
                        0.85f
                    ));

                    if (debugMode)
                    {
                        Debug.Log($"[JapaneseOCR] Detected: {recognizedText}");
                    }
                }

                bitmap.Call("recycle");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JapaneseOCR] Native OCR failed: {e.Message}");
        }
#endif

        return results;
    }

    /// <summary>
    /// Simulated OCR for testing in editor
    /// </summary>
    private List<DetectedText> PerformSimulatedOCR(Texture2D image)
    {
        List<DetectedText> results = new List<DetectedText>();

        // Simulate detection with sample Japanese text
        string[] sampleTexts = new string[]
        {
            "こんにちは", // Hello
            "ありがとう", // Thank you
            "さようなら", // Goodbye
            "お願いします", // Please
            "すみません"  // Excuse me
        };

        // Randomly detect 1-2 texts
        int numDetections = UnityEngine.Random.Range(1, 3);

        for (int i = 0; i < numDetections; i++)
        {
            string text = sampleTexts[UnityEngine.Random.Range(0, sampleTexts.Length)];
            float x = UnityEngine.Random.Range(0.1f, 0.7f);
            float y = UnityEngine.Random.Range(0.1f, 0.7f);

            results.Add(new DetectedText(
                text,
                new Rect(x * image.width, y * image.height, 200, 50),
                UnityEngine.Random.Range(0.7f, 0.95f)
            ));
        }

        if (debugMode && results.Count > 0)
        {
            Debug.Log($"[JapaneseOCR] Simulated detection: {results.Count} texts found");
        }

        return results;
    }

    /// <summary>
    /// Check if OCR is currently processing
    /// </summary>
    public bool IsProcessing()
    {
        return isProcessing || activeDetections > 0;
    }

    /// <summary>
    /// Get number of queued requests
    /// </summary>
    public int GetQueueLength()
    {
        return requestQueue.Count;
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (tesseractInstance != null)
        {
            tesseractInstance.Call("end");
            tesseractInstance.Dispose();
        }
#endif
    }
}
