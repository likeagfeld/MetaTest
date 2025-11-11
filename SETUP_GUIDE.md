# Quick Setup Guide - Quest 3 AR Translator

This guide will get you from zero to a working build in ~30 minutes.

## Prerequisites Checklist

- [ ] Unity Hub installed
- [ ] Unity 2022.3.10f1+ with Android module
- [ ] Meta Quest 3 headset
- [ ] USB-C cable
- [ ] Translation API key (or use free LibreTranslate)

## Step-by-Step Setup

### Part 1: Unity Installation (10 minutes)

1. **Download Unity Hub**
   - Visit: https://unity.com/download
   - Install Unity Hub

2. **Install Unity Editor**
   - Open Unity Hub
   - Go to "Installs" tab
   - Click "Install Editor"
   - Select version **2022.3.10f1** (or newer 2022.3.x)
   - Add modules:
     - ✅ Android Build Support
     - ✅ Android SDK & NDK Tools
     - ✅ OpenJDK

3. **Open Project**
   - In Unity Hub, click "Add"
   - Navigate to the MetaTest folder
   - Click "Select Folder"
   - Project will open and import packages (5-10 min first time)

### Part 2: Project Configuration (5 minutes)

1. **Enable XR**
   - `Edit > Project Settings > XR Plug-in Management`
   - Click Android tab (robot icon)
   - Check ✅ **OpenXR**
   - Under OpenXR > Interaction Profiles:
     - Add **Oculus Touch Controller Profile**

2. **Switch to Android**
   - `File > Build Settings`
   - Select **Android**
   - Click **Switch Platform** (takes 2-5 minutes)

3. **Configure Android Settings**
   - `Edit > Project Settings > Player`
   - Click Android tab
   - Under **Other Settings**:
     - Minimum API Level: **Android 10.0 (API 29)**
     - Scripting Backend: **IL2CPP**
     - Target Architectures: ✅ **ARM64 only**

### Part 3: Translation API Setup (5 minutes)

**Choose ONE option:**

#### Option A: LibreTranslate (FREE - No Key Needed) ⭐ Recommended for Testing
1. Open scene: `Assets/Scenes/MainScene.unity`
2. Select `ARTranslatorApp` GameObject
3. Find `TranslationService` component
4. Set Provider: **LibreTranslate**
5. Leave API Key empty
6. **Done!**

#### Option B: Google Translate (Most Accurate)
1. Go to: https://console.cloud.google.com/
2. Create new project (or select existing)
3. Enable "Cloud Translation API"
4. Create credentials > API Key
5. Copy the API key
6. In Unity:
   - Select `ARTranslatorApp` GameObject
   - Find `TranslationService` component
   - Set Provider: **GoogleTranslate**
   - Paste your API key

#### Option C: DeepL (Best Quality)
1. Go to: https://www.deepl.com/pro-api
2. Sign up for free account
3. Copy your API key from account settings
4. In Unity:
   - Select `ARTranslatorApp` GameObject
   - Find `TranslationService` component
   - Set Provider: **DeepL**
   - Paste your API key

### Part 4: OCR Data Setup (5 minutes)

1. **Download Japanese Language Data**
   - Visit: https://github.com/tesseract-ocr/tessdata/raw/main/jpn.traineddata
   - Download `jpn.traineddata` file (~14 MB)

2. **Add to Project**
   - Create folder: `Assets/StreamingAssets/tessdata/`
   - Move `jpn.traineddata` into this folder
   - Wait for Unity to import

### Part 5: Quest 3 Preparation (5 minutes)

1. **Enable Developer Mode**
   - Install Meta Quest mobile app on phone
   - Pair with your Quest 3
   - Open app > Menu > Devices
   - Select your Quest 3
   - Developer Mode > Toggle ON
   - Put on headset, accept developer mode

2. **Connect to PC**
   - Connect Quest 3 to PC via USB-C
   - Put on headset
   - Allow USB debugging when prompted
   - Allow file access when prompted

3. **Verify Connection**
   - Open command prompt/terminal
   - Run: `adb devices`
   - You should see your Quest 3 listed

### Part 6: Build & Deploy (5 minutes)

1. **Final Build Settings**
   - `File > Build Settings`
   - Verify **MainScene** is checked in "Scenes In Build"
   - Platform shows **Android** with Unity icon
   - Click **Build And Run**

2. **Save APK**
   - Choose save location (e.g., Desktop/Quest3Translator.apk)
   - Click **Save**
   - Wait for build (5-15 minutes first time)

3. **App Launches Automatically**
   - App installs and launches on Quest 3
   - Grant camera permissions when prompted
   - Grant storage permissions when prompted

## First Use

1. **Put on your Quest 3**
2. **Launch "Quest3 AR Translator"** from Apps library
3. **Point at Japanese text** (try a book, sign, or printed text)
4. **Watch for green translated text** to appear!

## Testing Without Japanese Text

For testing the app without Japanese text:

1. Change to Mock mode:
   - In Unity, select `ARTranslatorApp`
   - Set Provider: **Mock**
   - This will show simulated translations

2. Or print test page:
   - Print this Japanese text on paper:
   ```
   こんにちは
   ありがとう
   さようなら
   ```

## Controls Quick Reference

- **Right Trigger** = Manually translate what you're looking at
- **Left Trigger** = Clear all translations from view
- **A Button** = Turn auto-detect on/off
- **B Button** = (Reserved for future features)

## Troubleshooting

### "Build Failed"
- Check Unity version is 2022.3.10f1 or newer
- Verify Android module installed
- Ensure IL2CPP selected (not Mono)

### "No devices found"
- Enable Developer Mode on Quest 3
- Allow USB debugging
- Try different USB cable/port
- Run `adb devices` to verify connection

### "App crashes on launch"
- Grant camera permissions in Quest settings
- Check API key is correct (or use LibreTranslate)
- Verify jpn.traineddata file exists

### "No translation appears"
- Check internet connection
- Verify API key/provider settings
- Try switching to Mock mode to test
- Check Unity console for errors

### "OCR not working"
- Verify jpn.traineddata in StreamingAssets/tessdata/
- Check camera permissions granted
- Ensure good lighting on text
- Try larger, clearer text

## Next Steps

Once it's working:

1. **Optimize Settings**
   - Adjust detection interval for battery life
   - Configure overlay lifetime
   - Customize colors/sizes

2. **Try Different APIs**
   - Compare translation quality
   - Monitor API usage
   - Set up caching preferences

3. **Advanced Features**
   - Explore the full README.md
   - Customize UI appearance
   - Add new languages

## Getting Help

1. Check full README.md for detailed docs
2. Review Unity console logs for errors
3. Test with Mock mode to isolate issues
4. Verify all setup steps completed

---

**Estimated Total Time: 30-40 minutes**

**You're now ready to translate Japanese text in AR!** 🎉
