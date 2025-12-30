# Arquitectura y Plan de Implementación

This document defines the technical architecture of the **Local Secure Vault** project, as well as the design decisions and implementation stages.

It is designed to allow following the project step by step, even without prior experience in applied cryptography.

---

## 1. Design Principles

### 1.1 Security First

- All data is stored encrypted
- The master password is never saved
- Data integrity must be verifiable
- The file must be useless to an attacker without the password

### 1.2 Portability

- The vault must be restorable on another device
- System-tied keys are not relied on by default (optional DPAPI)

### 1.3 Separation of Concerns

- The UI never touches cryptography directly
- Encryption and storage are isolated
- The domain is independent of infrastructure

---

## 2. General Architecture

The project follows a layered architecture inspired by Clean Architecture.

### Project Structure

LocalSecureVault.sln
│
|-- Vault.Domain
│ |- Entities and business rules
│
|-- Vault.Application
│ |- Use cases and application logic
│
|-- Vault.Crypto
│ |- Cryptography (Argon2id + AES-GCM)
│
|-- Vault.Storage
│ |- File format and disk access
│
|-- Vault.Ui
│ |- Blazor Hybrid (MAUI)

---

## 3. Security Flow (conceptual view)

### 3.1 Vault Creation

1. The user enters a master password
2. A random `salt` is generated
3. Keys are derived using **Argon2id**
4. The initial (empty) content is serialized to JSON
5. The payload is encrypted with **AES-GCM**
6. A single encrypted file is written

---

### 3.2 Vault Opening

1. The file header is read
2. Keys are derived using the stored parameters
3. The payload is decrypted
4. If it fails → incorrect password or corrupted file

---

## 4. Cryptography (explained without jargon)

### 4.1 Argon2id (key derivation)

- Converts a human password into a strong key
- It is intentionally slow (≈800 ms)
- Uses a lot of memory to resist GPU attacks
- Parameters are stored in the file

**Reason:** protect against offline attacks if someone steals the file.

---

### 4.2 AES-GCM (authenticated encryption)

- Encrypts the data
- Detects if someone has modified the file
- Prevents silent tampering attacks

**Result:** confidentiality + integrity in a single step.

---

## 5. Encrypted File Format (high level)

[ HEADER | CIPHERTEXT | AUTH TAG ]

### Header (not secret, but protected)

Includes:

- File identifier
- Version
- Argon2id parameters
- Salt
- Encryption nonce
- Payload type (JSON)
- Future flags

The header is used as **AAD** (Authenticated Associated Data),
so any modification invalidates decryption.

---

## 6. Internal Payload

JSON format (MVP):

- Vault metadata
- Entry list:
  - Id
  - Name
  - Username
  - Password
  - URL
  - Notes
  - Tags

---

## 7. Auto-lock and Memory Safety

- The vault locks after X minutes of inactivity
- Keys live in memory only while it is unlocked
- Sensitive buffers are cleared when possible (best effort)

---

## 8. Implementation Roadmap

### Phase 0 – Design (current)

- [x] Security decisions
- [x] Defined architecture
- [x] Initial documentation

---

### Phase 1 – Domain and Use Cases

- [x] Domain entities
- [x] Validations
- [x] Entry CRUD
- [x] Search
- [x] Business tests

---

### Phase 2 – Cryptography

- [x] Argon2id implementation
- [x] AES-GCM implementation
- [x] Encryption/decryption tests
- [x] Corruption tests

---

### Phase 3 – Storage

- [x] File format
- [x] Atomic write
- [x] File locks
- [x] Persistence tests

---

### Phase 4 – UI (Blazor Hybrid)

- [x] Unlock screen
- [x] List and search
- [x] Entry editing
- [x] Copy password
- [x] Lock management

### Phase 4.5 – Importers

- [x] Markdown importer
- [ ] CSV importer (optional)
- [x] Import tests

---

### Phase 5 – Sensitive Services

- [x] Clipboard with auto-clear
- [x] Auto-lock on inactivity

---

## 9. Development Philosophy

- No "copy crypto code from StackOverflow"
- Everything critical is understood before implementation
- The project serves both as a real tool and solid learning resource