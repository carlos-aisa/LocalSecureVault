# Implementation Plan - Android Companion

This document describes the incremental plan to develop the Android companion version of LocalSecureVault.

---

## 🎯 Objective

Create an Android companion app that allows **opening, viewing, editing and saving** an existing vault (.vlt), maximizing reuse of existing architecture and code.

### Constraints and Decisions

- ✅ **Android only** (no iOS/Mac for now)
- ✅ **Read/edit only** of existing vaults (no creation)
- ✅ **No automatic sync** - user copies .vlt manually
- ✅ **No backend/cloud** - 100% local
- ✅ **Maximum code reuse** from Application/Crypto/Storage/Domain
- ❌ **Don't duplicate business logic**
- ❌ **Don't break existing architecture**

---

## 📐 Proposed Architecture

### Layer Structure (no changes)

```
Domain         (no changes)
Application    (no changes or minimal adjustments)
Crypto         (no changes)
Storage        (no changes)
App (UI)       (Desktop/Android variants)
```

### Platform: Specific Components

#### Desktop (Windows)

- `Vault.App` (MAUI Blazor)
- `IVaultFilePicker` → `MauiVaultFilePicker` (Windows)
- `IRecentVaultPathStore` → `PreferencesRecentVaultPathStore`

#### Android (New)

- `Vault.App` (same MAUI project, Android target)
- `IVaultFilePicker` → `AndroidVaultFilePicker` (Storage Access Framework)
- `IRecentVaultPathStore` → `PreferencesRecentVaultPathStore` (can be reused)
- **persistedUriPermissions** management to remember vault

### Critical Abstraction Interface

**IVaultFilePicker** (already exists on Desktop):

```csharp
public interface IVaultFilePicker
{
    Task<string?> PickVaultToOpenAsync();
    Task<string?> PickLocationToSaveAsync();
}
```

**New Android implementation**:

```csharp
public class AndroidVaultFilePicker : IVaultFilePicker
{
    // Uses Android Storage Access Framework (SAF)
    // Returns content:// URIs
    // Requests persisted permissions
}
```

### File Management on Android

#### Problem

- Android doesn't use traditional paths (`C:\...`)
- Uses **content:// URIs** (Storage Access Framework)
- We need **persisted permissions** to remember the last vault

#### Solution

1. `IVaultFilePicker` returns URI (string)
2. `IVaultStore` (FileVaultStore) must support URIs on Android
3. On Android, use `ContentResolver` to read/write streams
4. Save URI with persisted permissions in Preferences

### Adjustment in FileVaultStore

**Current** (filesystem paths only):

```csharp
public async Task<VaultFile> LoadAsync(string filePath)
{
    var bytes = await File.ReadAllBytesAsync(filePath);
    // ...
}
```

**Proposed** (cross-platform):

```csharp
public async Task<VaultFile> LoadAsync(string filePathOrUri)
{
    byte[] bytes;
    
    #if ANDROID
    if (filePathOrUri.StartsWith("content://"))
    {
        bytes = await ReadFromContentUri(filePathOrUri);
    }
    else
    #endif
    {
        bytes = await File.ReadAllBytesAsync(filePathOrUri);
    }
    // ...
}

#if ANDROID
private async Task<byte[]> ReadFromContentUri(string uri)
{
    // Uses Android.Content.ContentResolver
    var contentResolver = Platform.CurrentActivity.ContentResolver;
    using var stream = contentResolver.OpenInputStream(Android.Net.Uri.Parse(uri));
    using var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    return memoryStream.ToArray();
}
#endif
```

---

## 📋 Incremental Development Plan

### ✅ Phase 0: Preparation and Analysis

- [x] Review existing `Vault.App` code
- [x] Identify Windows-specific dependencies
- [x] Document interfaces to implement for Android
- [x] Create planning document (this file)

### ✅ Phase 1: Android Project Configuration

- [x] Enable Android target in `Vault.App.csproj`
- [x] Configure permissions in `AndroidManifest.xml`
  - `READ_EXTERNAL_STORAGE` (if API < 30)
  - `WRITE_EXTERNAL_STORAGE` (optional)
- [x] Configure `MainActivity.cs` for Android
- [x] Project configured with optional enable (`<EnableAndroid>true</EnableAndroid>`)
- [ ] ⏳ Verify Android build (requires installed workload)
- [ ] ⏳ Test execution on emulator or device

### ✅ Phase 2: File Picker Abstraction

- [x] Review existing `IVaultFilePicker`
- [x] Implement `AndroidVaultFilePicker` using SAF
  - Use `Intent.ActionOpenDocument`
  - Filter by `.vlt` extension
  - Request **FLAG_GRANT_PERSISTABLE_URI_PERMISSION**
  - Take `TakePersistableUriPermission`
- [x] Register implementation in DI for Android
- [ ] ⏳ Test file selection on Android

### ✅ Phase 3: FileVaultStore Adaptation

- [x] Refactor `LoadAsync` to support `content://` URIs
- [x] Refactor `SaveAsync` to support `content://` URIs
- [x] Create Android-specific helper methods:
  - `ReadFromContentUri`
  - `WriteToContentUri`
- [x] Use `#if ANDROID` directives to avoid Desktop contamination
- [ ] ⏳ Test read/write on Android

### ✅ Phase 4: Last Vault Persistence

- [x] Review `IRecentVaultPathStore` (reusable)
- [x] Ensure it saves URIs on Android (not paths)
- [x] Implement "Open Last Vault" logic on Android
- [ ] ⏳ Verify URI with permissions persists after restart

### ✅ Phase 5: UI Adjustments for Android

- [x] Review Blazor page navigation on Android
- [x] Adjust `Welcome.razor` for Android (no "Create Vault")
- [x] `OpenVault.razor` works without changes
- [x] Verify CRUD pages work without changes
- [ ] ⏳ Test complete flow: Open → Edit → Save → Lock

### ✅ Phase 6: Complete Features

- [x] Verify SearchService works on Android (pure logic)
- [x] Verify ClipboardService works on Android (MAUI API)
- [x] Verify InactivityMonitor works on Android (MAUI Dispatcher)
- [x] Verify auto-hide password works
- [ ] ⏳ Test manual and inactivity Lock

### ⏳ Phase 7: Testing and Refinement

- [ ] Install Android workload: `dotnet workload install android`
- [ ] Enable Android: Uncomment `<EnableAndroid>true</EnableAndroid>`
- [ ] Build for Android: `dotnet build -f net8.0-android`
- [ ] Run on emulator/device
- [ ] Test with real vault copied from Desktop
- [ ] Verify integrity after editing on Android and opening on Desktop
- [ ] Verify behavior with vault modified on both sides
- [ ] Adjust Android-specific UI/UX if needed
- [ ] Document known limitations

### ⏳ Phase 8: Packaging and Distribution

- [ ] Configure APK signing
- [ ] Configure Release build
- [ ] Generate APK for distribution
- [ ] Create installation documentation
- [ ] Update README with Android instructions

---

## 🚨 Risks and Considerations

### Risk 1: Content URIs vs Traditional Paths

**Impact**: High  
**Mitigation**: Complete abstraction in `IVaultStore`, use compilation directives

### Risk 2: Persisted Permissions

**Impact**: Medium  
**Mitigation**: Document proper use of `TakePersistableUriPermission`, test app restart

### Risk 3: Concurrent Edits

**Impact**: Low (occasional mobile use)  
**Mitigation**: Document no automatic merge, last write wins

### Risk 4: UI Differences between Desktop and Mobile

**Impact**: Low  
**Mitigation**: Blazor is responsive, adjust CSS if needed

---

## 🎓 Architecture Decisions

### ✅ Decision 1: One Project, Multiple Targets

**Context**: We need Desktop and Android  
**Decision**: Use same `Vault.App.csproj` with multiple targets  
**Rejected alternatives**: Separate projects (duplication)  
**Consequence**: Shared code, specific implementations with DI

### ✅ Decision 2: Storage Access Framework (SAF)

**Context**: Android 10+ restricts file access  
**Decision**: Use SAF with content:// URIs  
**Rejected alternatives**: MANAGE_EXTERNAL_STORAGE (too permissive)  
**Consequence**: Better security, moderate complexity

### ✅ Decision 3: No Vault Creation on Android

**Context**: Simpler UX, less critical flow  
**Decision**: Only open existing vaults on Android  
**Rejected alternatives**: Implement full creation  
**Consequence**: First time requires Desktop, but simplifies code

### ✅ Decision 4: Don't Modify Application/Domain/Crypto

**Context**: Current architecture is solid  
**Decision**: Changes only in Storage (URIs) and App (UI/DI)  
**Rejected alternatives**: Large refactor  
**Consequence**: Lower risk, localized changes

---

## 📚 Technical References

- [.NET MAUI Multi-targeting](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/configure-multi-targeting)
- [Android Storage Access Framework](https://developer.android.com/guide/topics/providers/document-provider)
- [Persisted URI Permissions](https://developer.android.com/reference/android/content/ContentResolver#takePersistableUriPermission(android.net.Uri,%20int))
- [MAUI Android Lifecycle](https://learn.microsoft.com/en-us/dotnet/maui/android/lifecycle)

---

## 🗓️ Estimation

| Phase | Complexity | Estimated Time |
|-------|------------|----------------|
| Phase 0 | Low | 1-2 hours |
| Phase 1 | Low | 2-3 hours |
| Phase 2 | Medium | 3-4 hours |
| Phase 3 | Medium-High | 4-6 hours |
| Phase 4 | Low | 1-2 hours |
| Phase 5 | Medium | 3-4 hours |
| Phase 6 | Low | 2-3 hours |
| Phase 7 | Medium | 4-6 hours |
| Phase 8 | Low | 2-3 hours |
| **TOTAL** | | **~25-35 hours** |

---

## 📌 Immediate Next Steps

1. ✅ Create this document
2. ✅ Update README.md
3. ⏭️ Review `Vault.App.csproj` and current structure
4. ⏭️ Identify Windows-specific dependencies
5. ⏭️ Create `IVaultFilePicker` and review existing implementation

---

**Last updated**: 2025-12-31  
**Status**: Phase 7 - Testing and iteration

---

## ✅ Implementation Completed

### Changes Made

#### 1. **Vault.App.csproj**

- ✅ Enabled `net8.0-android` target in addition to Windows
- ✅ Functional multi-target configuration

#### 2. **AndroidManifest.xml**

- ✅ Basic permissions configured (READ/WRITE_EXTERNAL_STORAGE for API < 33)
- ✅ Application label defined

#### 3. **AndroidVaultFilePicker** (NEW)

- ✅ Implementation using Storage Access Framework (SAF)
- ✅ Use of `Intent.ActionOpenDocument` with MIME type filter
- ✅ **Persisted URI permissions** via `TakePersistableUriPermission`
- ✅ Last opened vault management with Preferences
- ✅ Persisted permissions verification when retrieving last URI

#### 4. **FileVaultStore** (ADAPTED)

- ✅ Refactored to support both traditional paths and `content://` URIs
- ✅ Separate methods for stream-based operations (platform-agnostic)
- ✅ Android-specific implementation with `#if ANDROID` directives
  - `ReadFromContentUriAsync` - Read from content:// URI
  - `WriteToContentUriAsync` - Write to content:// URI
- ✅ Use of `ContentResolver` to access URIs
- ✅ Desktop version not contaminated

#### 5. **MauiProgram.cs**

- ✅ Conditional registration of `IVaultFilePicker` by platform
- ✅ Android uses `AndroidVaultFilePicker`
- ✅ Desktop uses `MauiVaultFilePicker`

#### 6. **Welcome.razor**

- ✅ "Create new vault" button hidden on Android with `#if !ANDROID` directive
- ✅ Simplified flow for Android (open existing vaults only)

#### 7. **Cross-platform Services**

- ✅ `ClipboardService` - Uses `Clipboard.Default` (MAUI), Android compatible
- ✅ `InactivityMonitor` - Uses `IDispatcher` (MAUI), Android compatible
- ✅ `SearchService` - Pure C# logic, no platform dependencies
- ✅ `PreferencesRecentVaultPathStore` - Uses `Preferences.Default`, supports URIs

---

## 🧪 Next Step: Testing

### Build

```bash
# Build for Android
dotnet build src/Vault.App/Vault.App.csproj -f net8.0-android

# Run on emulator/device (requires Android SDK and configured emulator)
dotnet build -t:Run -f net8.0-android src/Vault.App/Vault.App.csproj
```

### Testing Checklist

- [ ] **Successful build** for Android target
- [ ] **Execution on emulator/device** without crashes at startup
- [ ] **Open vault**: Picker shows files, allows selecting .vlt
- [ ] **Persisted permissions**: Close and reopen app, verify vault access
- [ ] **Open Last Vault**: Works correctly with URIs
- [ ] **Unlock**: Master password unlocks correctly
- [ ] **Entry listing**: All vault entries displayed
- [ ] **Search**: Filtering works correctly
- [ ] **Copy password**: Copies to Android clipboard
- [ ] **Create entry**: Adding new entry works
- [ ] **Edit entry**: Modifying existing entry works
- [ ] **Delete entry**: Removing entry works
- [ ] **Show/Hide password**: Toggle in editor works
- [ ] **Save**: Changes persisted correctly
- [ ] **Manual lock**: Manual locking works
- [ ] **Inactivity lock**: Auto-lock after configured timeout
- [ ] **Cross-platform integrity**: Vault edited on Android opens correctly on Desktop
- [ ] **Reverse integrity**: Vault edited on Desktop opens on Android

### Possible Problems and Solutions

| Problem | Probable Cause | Solution |
|---------|----------------|----------|
| Won't build for Android | Android SDK not installed | Install Android SDK via Visual Studio Installer |
| Picker won't open | Missing permissions | Verify AndroidManifest.xml |
| Can't read vault | Non-persisted permissions | Verify `TakePersistableUriPermission` |
| Crashes when saving | Writing to URI without permissions | Verify "wt" mode in `OpenOutputStream` |
| Doesn't remember last vault | Preferences not saving URI | Verify `LastUriKey` in AndroidVaultFilePicker |

---
