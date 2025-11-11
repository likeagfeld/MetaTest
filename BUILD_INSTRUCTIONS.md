# Build Instructions - Quest 3 AR Translator

Complete build guide for creating a production APK.

## Build Types

### Development Build (Recommended for Testing)
Fast build with debugging enabled.

### Release Build
Optimized for distribution and performance.

## Development Build

### 1. Configure Build Settings

```
File > Build Settings
```

Settings:
- ✅ **Scenes In Build**: MainScene
- **Platform**: Android (switch if needed)
- ✅ **Development Build**: Checked
- ✅ **Script Debugging**: Checked (optional)
- **Compression Method**: LZ4 (faster build)

### 2. Player Settings

```
Edit > Project Settings > Player > Android
```

**Identification:**
- Company Name: Your Company
- Product Name: Quest3 AR Translator
- Package Name: com.yourcompany.jptranslator (must be unique)
- Version: 1.0.0
- Bundle Version Code: 1

**Resolution and Presentation:**
- Default Orientation: Landscape Left
- Render Outside Safe Area: ✅ On

**Other Settings:**
- Scripting Backend: **IL2CPP**
- Target Architectures: **ARM64** (only)
- Minimum API Level: **Android 10.0 (API 29)**
- Target API Level: **Android 12.0 (API 32)**

**XR Settings:**
- Stereo Rendering Mode: **Multiview**

### 3. Build

```
File > Build Settings > Build And Run
```

- Choose output location
- Name: `Quest3Translator_Dev.apk`
- Wait for build (10-20 minutes first time)
- App automatically installs and launches

### 4. Verify

- App launches on Quest 3
- Check Unity console for errors
- Test all features
- Monitor performance

## Release Build

### 1. Pre-Build Checklist

- [ ] All features tested in dev build
- [ ] No console errors/warnings
- [ ] API keys configured correctly
- [ ] All assets optimized
- [ ] Tested on actual Quest 3 device

### 2. Configure for Release

**Build Settings:**
```
File > Build Settings
```
- ✅ MainScene in build
- ❌ Development Build: **Unchecked**
- ❌ Script Debugging: **Unchecked**
- Compression: **LZ4HC** (smaller size)

**Player Settings - Publishing:**
```
Edit > Project Settings > Player > Publishing Settings
```

Create Keystore (first time only):
1. **Create New Keystore**
   - Path: Choose secure location
   - Password: Create strong password
   - **SAVE PASSWORD SECURELY - YOU'LL NEED IT FOR UPDATES**

2. **Create New Key**
   - Alias: Your app name
   - Password: Create strong password
   - Validity: 25 years
   - Organization info (fill all fields)
   - **SAVE ALIAS PASSWORD SECURELY**

Or use existing keystore:
- Browse to keystore file
- Enter keystore password
- Select key alias
- Enter key password

**Optimization Settings:**
```
Edit > Project Settings > Player > Other Settings
```
- Managed Stripping Level: **High**
- Strip Engine Code: ✅
- Optimize Mesh Data: ✅

### 3. Build APK

```
File > Build Settings > Build
```

- Name: `Quest3Translator_v1.0.0.apk`
- Wait for build (15-30 minutes)

### 4. Build AAB (For Meta Store Distribution)

For publishing to Meta Quest Store:

1. **Enable AAB format:**
   ```
   Edit > Project Settings > Player > Publishing Settings
   ```
   - Build App Bundle (Google Play): ✅

2. **Build:**
   ```
   File > Build Settings > Build
   ```
   - Name: `Quest3Translator_v1.0.0.aab`

### 5. Test Release Build

```bash
# Install on Quest 3
adb install -r Quest3Translator_v1.0.0.apk

# Verify it works
# Launch from Quest 3 library
# Test all major features
```

## Build Optimization

### Reduce APK Size

1. **Texture Compression:**
   - Select textures in Project
   - Android tab > Format: ASTC 6x6

2. **Audio Compression:**
   - Select audio files
   - Compression Format: Vorbis
   - Quality: 70%

3. **Code Stripping:**
   - Already configured in release build
   - Managed Stripping Level: High

4. **Remove Unused Assets:**
   - Delete unused textures, models, scripts
   - Clean up StreamingAssets

### Improve Performance

1. **Graphics Settings:**
   ```
   Edit > Project Settings > Quality
   ```
   - Select "Medium" quality for Android
   - Pixel Light Count: 1
   - Texture Quality: Full Res
   - Anti Aliasing: 2x Multi Sampling

2. **Script Optimization:**
   - Already optimized in code
   - No reflection used
   - Minimal allocations

3. **Occlusion Culling:**
   - Not needed for this app (AR)

## Build Errors & Solutions

### "Unable to list target platforms"
**Solution:**
- Install Android Build Support in Unity Hub
- Verify Android SDK installed

### "Gradle build failed"
**Solution:**
- Check `mainTemplate.gradle` is valid
- Verify internet connection (downloads dependencies)
- Update Gradle: Edit > Preferences > External Tools > Gradle

### "UnityLinker error"
**Solution:**
- Reduce Managed Stripping Level to "Medium"
- Or add link.xml to preserve specific types

### "Unsupported Architecture"
**Solution:**
- Player Settings > Other > ARM64 checked only
- ARMv7 should be unchecked

### "Keystore error"
**Solution:**
- Verify keystore password correct
- Check keystore file exists and accessible
- Ensure key alias password correct

## Build Variants

### Debug Logging Enabled
```csharp
// In scripts, add #define DEBUG at top
#define DEBUG
```

### Battery Optimization Build
```
In ARTranslatorController:
- optimizeForBattery: true
- detectionInterval: 2.0f
```

### High Performance Build
```
In ARTranslatorController:
- detectionInterval: 0.5f
- maxSimultaneousTranslations: 10

In OCR:
- processWidth: 1920
- processHeight: 1080
```

## Version Management

### Incrementing Version

Before each release build:

1. **Version String:**
   ```
   Player Settings > Version: 1.0.0 → 1.0.1
   ```

2. **Bundle Version Code:**
   ```
   Player Settings > Bundle Version Code: 1 → 2
   ```
   - Must increase for each update
   - Never reuse a version code

### Version Naming Scheme

- **Major.Minor.Patch** (e.g., 1.0.0)
- **Major**: Breaking changes
- **Minor**: New features
- **Patch**: Bug fixes

## Automated Building (Advanced)

### Unity Command Line Build

```bash
# Windows
"C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe" \
  -quit -batchmode -projectPath "C:\Path\To\MetaTest" \
  -executeMethod BuildScript.Build

# Mac/Linux
/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -projectPath "/Path/To/MetaTest" \
  -executeMethod BuildScript.Build
```

Create `Assets/Editor/BuildScript.cs`:
```csharp
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void Build()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/Quest3Translator.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
```

## Distribution

### Internal Testing
- Share APK directly
- Install via `adb install`
- Use SideQuest for easier installation

### Meta Quest Store
1. Create Meta developer account
2. Submit AAB file
3. Fill store listing
4. Submit for review

### App Lab (Easier Alternative)
1. Join App Lab program
2. Submit APK/AAB
3. Less strict requirements
4. Faster approval

## Post-Build Checklist

After successful build:

- [ ] APK/AAB file created
- [ ] Version numbers incremented
- [ ] Tested on actual Quest 3 device
- [ ] All features working
- [ ] No crashes or errors
- [ ] Performance acceptable
- [ ] API keys working
- [ ] Backup keystore and passwords
- [ ] Tag git commit with version

## Build Artifacts

Typical build output:
```
Quest3Translator_v1.0.0.apk          # ~150-250 MB
Quest3Translator_v1.0.0.aab          # ~140-230 MB (if built)
```

## Next Steps

After successful build:
1. Distribute to testers
2. Gather feedback
3. Fix bugs
4. Increment version
5. Rebuild
6. Submit to store (optional)

---

**Average Build Times:**
- First build: 15-30 minutes
- Incremental builds: 5-15 minutes
- Release builds: 20-40 minutes

**Typical APK Size:** 150-250 MB (includes OCR data)
