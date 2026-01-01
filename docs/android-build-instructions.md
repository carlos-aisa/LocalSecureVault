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

**Last updated**: 2025-12-31
