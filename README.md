# Local Secure Vault

Offline-first encrypted vault for credentials and personal secrets, built with .NET, Clean Architecture, and security-focused defaults.

[![CI](https://github.com/carlos-aisa/LocalSecureVault/actions/workflows/ci.yml/badge.svg)](https://github.com/carlos-aisa/LocalSecureVault/actions/workflows/ci.yml)
[![CodeQL](https://github.com/carlos-aisa/LocalSecureVault/actions/workflows/codeql.yml/badge.svg)](https://github.com/carlos-aisa/LocalSecureVault/actions/workflows/codeql.yml)
[![Release](https://img.shields.io/github/v/release/carlos-aisa/LocalSecureVault)](https://github.com/carlos-aisa/LocalSecureVault/releases)
[![Downloads](https://img.shields.io/github/downloads/carlos-aisa/LocalSecureVault/total)](https://github.com/carlos-aisa/LocalSecureVault/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## TL;DR

- Security-first local vault: credentials are encrypted at rest and unlocked only with a master password.
- No cloud, no backend, no sync service: your vault stays on files you control.
- Production-minded engineering: layered architecture, tests, release artifacts for Windows and Android companion.

## Demo

Typical user flow (desktop + companion):

1. Create a new vault file and define a master password.
2. Unlock the vault and create or edit credential entries.
3. Search and filter entries instantly.
4. Copy a password to clipboard with auto-clear protection.
5. Save the encrypted vault file and lock manually or by inactivity timeout.
6. Optionally transfer the vault file to Android and unlock with password plus biometric authentication.

## Feature Highlights

- Vault creation and unlock with master password
- Full entry CRUD (name, username, password, URL, notes, tags)
- Fast search and filtering
- Clipboard copy with auto-clear policy
- Auto-lock after inactivity
- Manual transfer workflow to companion Android app
- Markdown import support

## Security Model

### Cryptography

- Key derivation: Argon2id
- Encryption: AES-GCM (authenticated encryption)
- Integrity and tamper resistance: authentication tag + protected header usage

### Design Rules

- Master password is never persisted
- Keys live in memory only while unlocked (best effort cleanup)
- Single encrypted file designed for backup and restore

### Out of Scope

- Active malware or keyloggers on the host
- Cloud sync and multi-device conflict resolution
- Enterprise password manager feature parity

## Architecture

The solution follows a layered approach inspired by Clean Architecture:

- `Vault.Domain`: entities and business rules
- `Vault.Application`: use cases and application orchestration
- `Vault.Crypto`: Argon2id and AES-GCM implementations
- `Vault.Storage`: encrypted file format and disk persistence
- `Vault.App`: MAUI + Blazor Hybrid UI

For the full design rationale, see:

- [Architecture](docs/arch.md)
- [File format](docs/file-format.md)
- [Security tests](docs/security-tests.md)

## Quickstart

### Prerequisites

- Windows 10+ for desktop execution
- .NET SDK from `global.json`
- Optional for Android companion build: Android workload and SDK

### Clone and restore

```powershell
git clone https://github.com/carlos-aisa/LocalSecureVault.git
cd LocalSecureVault
dotnet restore LocalSecureVault.sln
```

### Run tests

```powershell
dotnet test tests/Vault.Tests/Vault.Tests.csproj -f net8.0 -c Release
```

### Run desktop app (Windows)

```powershell
dotnet run --project src/Vault.App/Vault.App.csproj --framework net8.0-windows10.0.19041.0
```

More commands are available in [docs/build-and-publish.md](docs/build-and-publish.md).

## Installation (End Users)

1. Open [Releases](https://github.com/carlos-aisa/LocalSecureVault/releases).
2. Download:
   - `LocalSecureVault-windows-vX.Y.Z.zip` for desktop
   - `LocalSecureVault-companion-android-vX.Y.Z.apk` for Android companion
3. Validate the SHA256 checksums from the `SHA256SUMS.txt` asset.
4. Extract and run the Windows app, or sideload the APK on Android.

## Quality and Verification

- Automated CI build and test validation on push and pull requests
- Dedicated security analysis with CodeQL
- Security-focused tests for wrong-password, tamper detection, and corruption paths

Reference docs:

- [Security tests](docs/security-tests.md)
- [Build and publish guide](docs/build-and-publish.md)

## Project Status

- Windows desktop app: feature-complete for MVP
- Android companion: existing vault usage flow implemented

## Roadmap

- [ ] Signed Windows installer (MSIX)
- [ ] Optional secure export/import profile presets
- [ ] UX improvements for large vault browsing
- [ ] Expanded fuzzing and corruption resilience tests

## Contributing and Security

- Contribution guide: [CONTRIBUTING.md](CONTRIBUTING.md)
- Security policy: [SECURITY.md](SECURITY.md)
- Changelog: [CHANGELOG.md](CHANGELOG.md)

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
