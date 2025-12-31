

# Instructions for Building the Android Version

This document explains how to enable and build the Android version of LocalSecureVault.

---

## Prerequisites

### 1. Install Android Workload

The Android workload for .NET MAUI must be installed:

```bash
dotnet workload install android
```

### 2. Verify Installation

```bash
dotnet workload list
```

You should see `android` in the list of installed workloads.

### 3. Android SDK

Make sure you have the Android SDK installed. Options:

- **Visual Studio**: Install via Visual Studio Installer → ".NET SDK for Android"
- **Android Studio**: Download and install Android Studio
- **Command Line Tools**: Download Android SDK Command Line Tools

---

## Enable Android Build

### Option 1: Modify Vault.App.csproj

Edit `src/Vault.App/Vault.App.csproj` and uncomment this line:

```xml
<PropertyGroup>
    <!-- <EnableAndroid>true</EnableAndroid> -->  <!-- UNCOMMENT THIS LINE -->
    ...
</PropertyGroup>
```

Should become:

```xml
<PropertyGroup>
    <EnableAndroid>true</EnableAndroid>
    ...
</PropertyGroup>
```

### Option 2: Pass Property When Building

Without modifying the file, you can enable Android when building:

```bash
dotnet build src/Vault.App/Vault.App.csproj -f net8.0-android -p:EnableAndroid=true
```

---

## Build for Android

### Debug Build

```bash
# Build
dotnet build src/Vault.App/Vault.App.csproj -f net8.0-android -c Debug

# Or run directly (if you have emulator/device connected)
dotnet build -t:Run -f net8.0-android -c Debug src/Vault.App/Vault.App.csproj
```

### Release Build

```bash
dotnet build src/Vault.App/Vault.App.csproj -f net8.0-android -c Release
```

---

## Run on Emulator/Device

### Prerequisite: Emulator or Device

**Emulator**:
1. Open Android Studio → Tools → AVD Manager
2. Create or start a virtual device (API 23+)

**Physical device**:
1. Enable developer mode on device
2. Enable USB debugging
3. Connect via USB

### Verify Connected Devices

```bash
adb devices
```

You should see your emulator or device listed.

### Run the App

```bash
# From Visual Studio: F5 selecting Android target

# From command line:
dotnet build -t:Run -f net8.0-android src/Vault.App/Vault.App.csproj
```

---

## Generate APK for Distribution

### Signed APK (Release)

```bash
dotnet publish src/Vault.App/Vault.App.csproj -f net8.0-android -c Release
```

The APK will be generated in:
```
src/Vault.App/bin/Release/net8.0-android/publish/
```

### Install APK Manually

```bash
adb install -r path/to/com.companyname.vault.app-Signed.apk
```

---

## Troubleshooting

### Error: "workload android not found"

**Solution**: Install Android workload
```bash
dotnet workload install android
```

### Error: "No Android SDK found"

**Solution**: Install Android SDK and configure `ANDROID_HOME`:
```bash
# Windows (example)
setx ANDROID_HOME "C:\Program Files\Android\android-sdk"

# Linux/Mac
export ANDROID_HOME=/path/to/android-sdk
```

### Error: "No devices found"

**Solution**: Verify emulator or device is connected
```bash
adb devices
adb kill-server
adb start-server
```

### Error: "Could not take persistable permission"

**Solution**: Normal on first run. The vault will work in current session. On subsequent uses with the same URI, the permission should persist.

---

## Android vs Desktop Differences

| Feature | Desktop | Android |
|---------|---------|---------|
| Create vault | ✅ Yes | ❌ No (open only) |
| Open vault | ✅ File paths | ✅ content:// URIs |
| CRUD entries | ✅ | ✅ |
| Search | ✅ | ✅ |
| Copy password | ✅ | ✅ |
| Save | ✅ Atomic write | ✅ ContentResolver |
| Manual lock | ✅ | ✅ |
| Auto lock | ✅ | ✅ |
| Import Markdown | ✅ | ✅ (if vault exists) |

---

## Important Notes

1. **First Time**: You need to create the vault on Desktop and copy it to Android device
2. **Transfer**: Use USB, Google Drive, Dropbox, etc. to copy the `.vlt` to device
3. **Location on Android**: Select file from Downloads, Documents, or any accessible folder
4. **No Sync**: Changes don't sync automatically between devices
5. **Last Write Wins**: If you edit the same vault on both sides, last write overwrites
6. **Backup Recommended**: Always keep backups of `.vlt` before editing on multiple devices

---

## Recommended Initial Testing

1. Create vault on Desktop with some test entries
2. Copy `.vlt` to Android device
3. Open vault on Android with master password
4. Verify all entries read correctly
5. Create new entry on Android
6. Save and close
7. Copy `.vlt` back to Desktop
8. Open on Desktop and verify new entry is present
9. Verify integrity (no data corruption)

---

**Last updated**: 2025-12-31
