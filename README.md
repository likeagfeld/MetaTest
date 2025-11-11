# Quest 3 AR Japanese-to-English Translator

A fully-featured augmented reality application for Meta Quest 3 that translates Japanese text to English in real-time using the headset's passthrough cameras.

## Features

- **Real-time AR Translation**: Point your Quest 3 at Japanese text and see instant English translations
- **Passthrough AR**: Uses Quest 3's high-quality passthrough cameras for clear real-world view
- **OCR Text Detection**: Advanced Japanese character recognition (Hiragana, Katakana, Kanji)
- **Multiple Translation APIs**: Supports Google Translate, DeepL, and LibreTranslate
- **Intuitive Controls**: Simple controller-based interface
- **Smart Caching**: Reduces API calls and improves performance
- **Floating Overlays**: Translations appear as AR overlays positioned near detected text

## Requirements

### Hardware
- Meta Quest 3 headset
- Stable internet connection (for cloud translation APIs)

### Software
- Unity 2022.3.10f1 or later
- Android SDK (API Level 29+)
- Meta XR SDK / OpenXR Plugin

### API Keys (Optional)
- Google Translate API key OR
- DeepL API key OR
- Use the free LibreTranslate option (no key required)

## Installation & Setup

### 1. Unity Setup

1. **Install Unity Hub** and **Unity 2022.3.10f1** (or later)
   - Include Android Build Support module
   - Include Android SDK & NDK Tools

2. **Open the Project**
   ```bash
   # Clone or download this repository
   git clone <repository-url>

   # Open in Unity Hub
   # Add Project -> Select the MetaTest folder
   ```

3. **Install Required Packages**
   - Unity will automatically install dependencies from `Packages/manifest.json`
   - Required packages:
     - XR Plugin Management
     - OpenXR Plugin
     - TextMeshPro
     - Android support modules

4. **Configure XR Settings**
   - Go to `Edit > Project Settings > XR Plug-in Management`
   - Enable **OpenXR** for Android platform
   - Under OpenXR settings, add **Oculus Touch Controller Profile**

### 2. Android Build Setup

1. **Switch to Android Platform**
   - `File > Build Settings`
   - Select **Android**
   - Click **Switch Platform**

2. **Configure Player Settings**
   - `Edit > Project Settings > Player > Android tab`
   - Set **Company Name** and **Product Name**
   - **Other Settings**:
     - Minimum API Level: **Android 10.0 (API Level 29)**
     - Target API Level: **Android 12.0 (API Level 32)**
     - Scripting Backend: **IL2CPP**
     - Target Architectures: **ARM64** (check only)
   - **XR Settings**:
     - Stereo Rendering Mode: **Multiview**

3. **Configure Android Manifest**
   - The AndroidManifest.xml is already configured at:
     `Assets/Plugins/Android/AndroidManifest.xml`
   - Verify permissions for Camera and Internet are present

### 3. Translation API Configuration

Choose one of the following options:

#### Option A: Google Translate API (Recommended for accuracy)
1. Get API key from [Google Cloud Console](https://console.cloud.google.com/)
2. Enable Cloud Translation API
3. In Unity, select the **ARTranslatorApp** GameObject
4. In **TranslationService** component, set:
   - Provider: `GoogleTranslate`
   - API Key: `<your-google-api-key>`

#### Option B: DeepL API (Best for natural translations)
1. Get free API key from [DeepL](https://www.deepl.com/pro-api)
2. In Unity, configure:
   - Provider: `DeepL`
   - API Key: `<your-deepl-api-key>`

#### Option C: LibreTranslate (Free, no key required)
1. In Unity, configure:
   - Provider: `LibreTranslate`
   - API Key: (leave empty)
2. Uses public LibreTranslate instance

#### Option D: Mock Mode (For testing without API)
1. In Unity, configure:
   - Provider: `Mock`
2. Returns simulated translations

### 4. OCR Setup

The app uses Tesseract OCR for Japanese text recognition.

1. **Download Japanese Trained Data**
   - Download `jpn.traineddata` from [Tesseract GitHub](https://github.com/tesseract-ocr/tessdata)
   - Place in: `Assets/StreamingAssets/tessdata/jpn.traineddata`

2. **The gradle build will automatically include tess-two library** (specified in mainTemplate.gradle)

### 5. Build & Deploy

1. **Build APK**
   ```
   File > Build Settings
   - Ensure MainScene is in "Scenes in Build"
   - Click "Build" or "Build and Run"
   - Choose output location
   ```

2. **Install on Quest 3**
   - Connect Quest 3 via USB-C cable
   - Enable Developer Mode on Quest 3:
     - Open Meta Quest mobile app
     - Go to Headset Settings > Developer Mode > Enable
   - In Unity, click **Build and Run**

   OR manually install:
   ```bash
   adb install -r YourApp.apk
   ```

3. **Grant Permissions**
   - First launch will request camera permissions
   - Grant all permissions for full functionality

## Usage

### Controls

**Right Controller:**
- **Trigger**: Manually capture and translate text
- **A Button**: Toggle auto-detection on/off
- **B Button**: Reserved for future features

**Left Controller:**
- **Trigger**: Clear all translation overlays
- **X Button**: Reserved for future features
- **Y Button**: Reserved for future features

### Basic Operation

1. **Launch the App** from your Quest 3 library
2. **Point at Japanese Text** - The app will automatically detect and translate
3. **View Translations** - English translations appear as floating overlays
4. **Manual Capture** - Press right trigger to force a translation
5. **Clear Screen** - Press left trigger to remove all overlays

### Tips for Best Results

- **Lighting**: Ensure good lighting on the text
- **Distance**: Stay 1-3 feet from the text
- **Stability**: Hold your head still for a moment while detecting
- **Clear Text**: Works best with printed text (books, signs, menus)
- **Font Size**: Larger text (12pt+) works better

## Project Structure

```
MetaTest/
├── Assets/
│   ├── Scenes/
│   │   └── MainScene.unity          # Main AR scene
│   ├── Scripts/
│   │   ├── ARTranslatorController.cs    # Main app controller
│   │   ├── PassthroughARManager.cs      # AR camera management
│   │   ├── JapaneseOCRDetector.cs       # Text detection
│   │   ├── TranslationService.cs        # Translation API integration
│   │   ├── TranslationUIManager.cs      # UI overlay management
│   │   ├── QuestInputHandler.cs         # Controller input
│   │   └── AssemblyInfo.asmdef          # Assembly definition
│   └── Plugins/
│       └── Android/
│           ├── AndroidManifest.xml      # Android permissions & config
│           ├── mainTemplate.gradle      # Gradle build config
│           └── gradleTemplate.properties
├── Packages/
│   └── manifest.json                 # Unity package dependencies
├── ProjectSettings/
│   ├── ProjectVersion.txt
│   ├── ProjectSettings.asset
│   └── XRSettings.asset
└── README.md
```

## Architecture

### Component Overview

1. **ARTranslatorController**: Main orchestrator
   - Coordinates all components
   - Manages app lifecycle
   - Handles events between components

2. **PassthroughARManager**: Camera & AR
   - Manages Quest 3 passthrough
   - Captures camera frames
   - Provides textures for OCR

3. **JapaneseOCRDetector**: Text Recognition
   - Processes images with Tesseract
   - Detects Japanese text regions
   - Returns text with bounding boxes

4. **TranslationService**: Translation
   - Supports multiple APIs
   - Handles caching
   - Manages network requests

5. **TranslationUIManager**: Display
   - Creates AR overlays
   - Manages UI canvas
   - Handles text positioning

6. **QuestInputHandler**: Input
   - Processes controller input
   - Provides haptic feedback
   - Maps buttons to actions

### Data Flow

```
Camera Frame → OCR Detection → Text Extraction → Translation API → UI Overlay
     ↑                                                                  ↓
     └──────────────── User Input (Controllers) ─────────────────────┘
```

## Performance Optimization

- **Detection Interval**: Adjust `detectionInterval` to balance responsiveness vs battery
- **Batch Processing**: Multiple texts translated in parallel
- **Smart Caching**: Recently translated text is cached
- **Queue Management**: Prevents OCR overload
- **Battery Optimization**: Enable `optimizeForBattery` mode

## Troubleshooting

### No Text Detected
- Check lighting conditions
- Ensure camera permissions granted
- Verify text is clear and in focus
- Try manual capture (right trigger)

### Translations Not Appearing
- Verify internet connection
- Check API key configuration
- Review Unity console for errors
- Try switching to LibreTranslate or Mock mode

### App Won't Build
- Verify Unity version (2022.3.10f1+)
- Check Android SDK installation
- Ensure IL2CPP backend selected
- Verify OpenXR plugin installed

### Performance Issues
- Reduce `captureIntervalSeconds`
- Decrease `processWidth/Height` in OCR settings
- Enable battery optimization mode
- Clear translation cache periodically

### Camera Not Working
- Grant camera permissions in Quest settings
- Restart the app
- Check AndroidManifest.xml permissions
- Verify passthrough is enabled in script

## API Costs & Limits

### Google Translate
- First 500,000 characters/month: Free
- After: $20 per million characters
- [Pricing Details](https://cloud.google.com/translate/pricing)

### DeepL
- Free tier: 500,000 characters/month
- Pro tier: From $5.49/month
- [Pricing Details](https://www.deepl.com/pro-api)

### LibreTranslate
- Public instance: Free, rate-limited
- Self-hosted: Unlimited, free
- [Documentation](https://libretranslate.com/)

## Development

### Adding New Features

1. **Custom Translation Provider**
   - Edit `TranslationService.cs`
   - Add new enum value to `TranslationProvider`
   - Implement request creation method

2. **Additional Languages**
   - Download Tesseract trained data for target language
   - Update `JapaneseOCRDetector.cs` initialization
   - Modify `TranslationService.cs` language codes

3. **UI Customization**
   - Modify `TranslationUIManager.cs`
   - Adjust colors, fonts, sizes
   - Customize overlay behavior

### Testing in Editor

The app includes mock/simulated modes for editor testing:
- OCR uses simulated Japanese text detection
- Translation uses mock dictionary
- Passthrough simulation (limited)

### Building for Quest Pro

The app is compatible with Quest Pro:
- Same build process
- Enhanced passthrough quality
- Hand tracking support (partial)

## Known Limitations

- OCR accuracy depends on text clarity
- Internet required for cloud translation
- Handwriting recognition limited
- Performance varies with text density
- Best with horizontal text orientation

## Future Enhancements

- [ ] Offline translation mode
- [ ] History/favorites system
- [ ] Voice pronunciation
- [ ] Photo capture & save
- [ ] Multi-language support
- [ ] Hand tracking gestures
- [ ] Spatial anchors for persistent overlays
- [ ] Settings menu UI

## Credits

### Technologies Used
- **Unity**: Game engine and XR framework
- **Meta XR SDK**: Quest platform integration
- **OpenXR**: Cross-platform VR/AR standard
- **Tesseract OCR**: Text recognition engine
- **TextMeshPro**: Advanced text rendering

### Translation APIs
- Google Cloud Translation
- DeepL Translator
- LibreTranslate (Open source)

## License

This project is provided as-is for educational and personal use.

Translation API usage subject to respective provider terms of service.

## Support

For issues, questions, or contributions:
1. Check the Troubleshooting section
2. Review Unity console logs
3. Verify all setup steps completed
4. Test with mock mode to isolate issues

## Version History

### v1.0.0 (2025-01-11)
- Initial release
- Core AR translation functionality
- Multi-API support
- Quest 3 controller integration
- Caching system
- UI overlay system

---

**Built for Meta Quest 3** | **Requires Unity 2022.3+** | **Android 10+**
