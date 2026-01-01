# Build and Publish Guide

This comprehensive guide explains how to build, run, and publish the LocalSecureVault application for both Windows and Android platforms.

---

## Prerequisites

### General Requirements

- .NET 8.0 SDK installed
- Git (for cloning the repository)

### Windows-Specific

- Windows 10 version 1809 or later
- Visual Studio 2022 (optional, for development)

### Android-Specific

1. **Android Workload Installation**

   ```powershell
   dotnet workload install android
   ```

2. **Verify Workload Installation**

   ```powershell
   dotnet workload list
   ```

   You should see `android` in the list of installed workloads.

3. **Android SDK**

   Choose one of these options:
   - **Visual Studio**: Install via Visual Studio Installer → ".NET SDK for Android"
   - **Android Studio**: Download and install Android Studio
   - **Command Line Tools**: Download Android SDK Command Line Tools

4. **Enable Android Build**

   Edit `src/Vault.App/Vault.App.csproj` and set:

   ```xml
   <PropertyGroup>
       <EnableAndroid>true</EnableAndroid>
       ...
   </PropertyGroup>
   ```

---

## Windows

### Development Build (Debug)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Debug
```

**Output location:** `src\Vault.App\bin\Debug\net8.0-windows10.0.19041.0\`

### Production Build (Release)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release
```

### Run Application (Development)

**Option 1: Using dotnet run**

```powershell
dotnet run --project src\Vault.App\Vault.App.csproj --framework net8.0-windows10.0.19041.0
```

**Option 2: Execute the built binary**

```powershell
.\src\Vault.App\bin\Debug\net8.0-windows10.0.19041.0\Vault.App.exe
```

### Publish (Self-Contained Distribution)

For distribution without requiring .NET installed on target machines:

```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained
```

**Output location:** `src\Vault.App\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`

**Executable:** `Vault.App.exe`

**Distribution:** Copy the entire `publish` folder to the target machine. The application is fully self-contained and includes the .NET runtime (~100-150 MB).

**Alternative: Framework-Dependent Build**

If .NET 8 is already installed on target machines, you can create a smaller build:

```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release
```

---

## Android

### Development Build (Debug)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-android -c Debug
```

**Output location:** `src\Vault.App\bin\Debug\net8.0-android\`

### Production Build (Release)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-android -c Release
```

### Run on Emulator or Device

#### Setup Emulator or Physical Device

**Android Emulator:**
1. Open Android Studio → Tools → AVD Manager
2. Create or start a virtual device (API 23 or higher)

**Physical Device:**
1. Enable Developer Mode on your Android device
2. Enable USB Debugging in Developer Options
3. Connect device via USB

#### Verify Device Connection

```powershell
adb devices
```

You should see your emulator or device listed.

#### Run the Application

**From Visual Studio:** Press F5 and select Android target

**From Command Line:**

```powershell
dotnet build -t:Run -f net8.0-android src\Vault.App\Vault.App.csproj
```

### Publish (APK/AAB Generation)

```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-android -c Release
```

**Output location:** `src\Vault.App\bin\Release\net8.0-android\`

**Generated Files:**
- `com.carlosaredo.vault.app-Signed.apk` - For direct installation on devices (sideloading)
- `com.carlosaredo.vault.app-Signed.aab` - For Google Play Store distribution

### Install APK on Device

**Using ADB:**

```powershell
adb install src\Vault.App\bin\Release\net8.0-android\com.companyname.vault.app-Signed.apk
```

**Update existing installation:**

```powershell
adb install -r src\Vault.App\bin\Release\net8.0-android\com.companyname.vault.app-Signed.apk
```

**Manual Installation:**

Copy the APK file to your device and open it to install (requires "Install from Unknown Sources" enabled).

---

## Running Tests

Run all tests:

```powershell
dotnet test
```

Run tests for a specific project:

```powershell
dotnet test tests\Vault.Tests\Vault.Tests.csproj
```

Run tests with verbose output:

```powershell
dotnet test --verbosity detailed
```

---

## Clean Build

Remove all build artifacts:

```powershell
dotnet clean
```

Clean specific project:

```powershell
dotnet clean src\Vault.App\Vault.App.csproj
```

Clean and rebuild:

```powershell
dotnet clean
dotnet build
```

---

### Windows Build Issues

**Missing Windows SDK:**

Install Windows SDK 10.0.19041.0 or later via Visual Studio Installer.

**Runtime errors:**

Ensure Windows 10 version 1809 (build 17763) or later.

---

## Platform-Specific Notes

### Windows

- **Self-Contained Build:** ~100-150 MB, includes .NET runtime, no dependencies
- **Framework-Dependent Build:** ~10-20 MB, requires .NET 8 runtime on target machine
- **Minimum OS:** Windows 10 version 1809 (build 17763)
- **Icon:** Custom application icon included in build

### Android

- **APK Signing:** Debug builds use a debug key. For production release on Google Play, sign with your own release key
- **Minimum Android Version:** API 23 (Android 6.0 Marshmallow)
- **Target Android Version:** API 34 (Android 14)
- **Features:**
  - Biometric authentication (fingerprint/face recognition)
  - Storage Access Framework integration
  - Responsive UI optimized for mobile screens
- **Permissions Required:**
  - `USE_BIOMETRIC` - For fingerprint/face authentication
  - `READ_EXTERNAL_STORAGE` / `WRITE_EXTERNAL_STORAGE` - For vault file access (API ≤ 32)

### Build Configuration

- **Debug:** Includes debugging symbols, larger size, not optimized
- **Release:** Optimized, smaller size, no debug symbols
- **Always use Release configuration for distribution**

---

## Quick Reference

### Most Common Commands

**Windows Development:**
```powershell
dotnet run --project src\Vault.App\Vault.App.csproj --framework net8.0-windows10.0.19041.0
```

**Windows Distribution:**
```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained
```

**Android Development:**
```powershell
dotnet build -t:Run -f net8.0-android src\Vault.App\Vault.App.csproj
```

**Android Distribution:**
```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-android -c Release
```

---

**Last Updated:** January 1, 2026
