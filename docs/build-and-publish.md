# Build and Publish Guide

This document contains the commands to build and publish the LocalSecureVault application for Windows and Android platforms.

## Prerequisites

- .NET 8.0 SDK installed
- For Android: Android workload installed (`dotnet workload install android`)
- For Android: Android SDK and emulator/device configured

## Windows

### Development Build (Debug)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Debug
```

The output will be in: `src\Vault.App\bin\Debug\net8.0-windows10.0.19041.0\`

### Production Build (Release)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release
```

### Publish (Self-Contained)

For distribution without requiring .NET installed on target machines:

```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained
```

**Output location:** `src\Vault.App\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`

**Executable:** `Vault.App.exe`

**Distribution:** Copy the entire `publish` folder to the target machine. The application is fully self-contained and includes the .NET runtime.

---

## Android

### Development Build (Debug)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-android -c Debug
```

### Production Build (Release)

```powershell
dotnet build src\Vault.App\Vault.App.csproj -f net8.0-android -c Release
```

### Publish (APK/AAB Generation)

```powershell
dotnet publish src\Vault.App\Vault.App.csproj -f net8.0-android -c Release
```

**Output location:** `src\Vault.App\bin\Release\net8.0-android\`

**Generated files:**

- `com.companyname.vault.app-Signed.apk` - For direct installation on devices
- `com.companyname.vault.app-Signed.aab` - For Google Play Store distribution

### Install APK on Device

Using ADB (Android Debug Bridge):

```powershell
adb install src\Vault.App\bin\Release\net8.0-android\com.companyname.vault.app-Signed.apk
```

Or copy the APK to your device and install manually.

---

## Running Tests

```powershell
dotnet test
```

---

## Clean Build

To remove all build artifacts:

```powershell
dotnet clean
```

To clean a specific project:

```powershell
dotnet clean src\Vault.App\Vault.App.csproj
```

---

## Notes

### Windows

- The self-contained build is approximately 100-150 MB but doesn't require .NET to be installed on the target machine.
- For a framework-dependent build (requires .NET 8 on target), omit the `-r win-x64 --self-contained` flags.

### Android

- The APK is signed with a debug key. For production release on Google Play, you need to sign it with your own release key.
- Minimum Android version supported: API 23 (Android 6.0)
- The app includes biometric authentication support (fingerprint/face recognition).

### Build Configuration

- Debug builds include debugging symbols and are larger
- Release builds are optimized and smaller in size
- Always use Release configuration for distribution
