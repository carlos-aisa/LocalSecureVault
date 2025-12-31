# Local Secure Vault (LSV)

## Description

**Local Secure Vault** is a local Windows application designed for the secure management of credentials and secrets (users, passwords, URLs, notes, etc.).

The main goal of the project is to replace the use of plain text files (for example Markdown) with a **secure, encrypted, and offline** solution that allows backups and restores even on other devices.

There is no server, backend, or cloud service:  
everything runs **exclusively on the user's local machine**.

---

## Main Objectives  

- Store all information **always encrypted at rest**
- Access protected by a **master password** (never stored)
- Modern design based on good security practices:
  - Key derivation with Argon2id
  - Symmetric encryption with AES-GCM
  - Data integrity and authentication protection
- A single encrypted file easy to back up
- Restore possible on other devices
- 100% local application for Windows
- Modern interface based on **Blazor Hybrid**

---

## What this project is NOT

- It is not a cloud password manager
- It does not sync data
- It does not aim to compete with Bitwarden, 1Password, etc.
- It does not protect against active malware on the system

It is a **local personal vault**, secure and controlled by the user.

---

## Functionality (MVP)  

- Create a new vault (encrypted file)
- Open and unlock an existing vault
- CRUD entries:
  - Name
  - Username
  - Password
  - URL
  - Notes
  - Tags
- Quick search
- Copy password to clipboard with auto-clear
- Auto-lock after inactivity

---

## Main Technologies

- .NET (>= 8)
- Blazor Hybrid (MAUI)
- Argon2id (key derivation)
- AES-GCM (authenticated encryption)
- JSON (internal payload, MVP)

---

## Project Status

✅ **Desktop version (Windows)** - Fully functional  
🚧 **Android companion** - In development

### Desktop (Windows) - Implemented
- Create/open vaults
- Full CRUD operations on entries
- Markdown import
- Search functionality
- Copy password to clipboard
- Manual save
- Manual and auto-lock by inactivity
- Show/hide password in editor

### Android Companion - Planned
- Open existing vault (manual file selection)
- Full CRUD operations on entries
- Search functionality
- Copy password to clipboard
- Manual save
- Manual and auto-lock by inactivity
- **NOT included**: Vault creation (desktop only)
- **Transfer method**: Manual (USB, cloud storage, etc.)
- **Sync strategy**: None - occasional mobile use expected

The architecture documentation and implementation plan are located in `docs/`.