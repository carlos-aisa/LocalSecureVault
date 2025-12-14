# Component Diagram + Flows (Create / Unlock / Save)

## Component diagram (high level)

```mermaid
flowchart LR
  UI[Vault.Ui\nBlazor Hybrid] --> APP[Vault.Application\nUse-cases]
  APP -->|ports| STORE[IVaultStore]
  APP -->|ports| CRYPTO[ICryptoProvider]
  APP -->|ports| CLIP[IClipboardService]
  APP -->|ports| IDLE[IIdleTimeProvider]
  STORE --> STORAGE[Vault.Storage\nFile I/O + format]
  CRYPTO --> VC[Vault.Crypto\nArgon2id + AES-GCM]
  STORAGE --> FS[(Windows File System)]

#Flow: Create Vault
sequenceDiagram
  autonumber
  actor U as User
  participant UI as Vault.Ui
  participant APP as Vault.Application
  participant CR as ICryptoProvider
  participant ST as IVaultStore
  participant FS as File System

  U->>UI: Create new vault (path + master password)
  UI->>APP: CreateVault(path, masterPassword)
  APP->>CR: GenerateSalt(16)
  APP->>CR: DeriveKey(masterPassword, salt, kdfParams)
  APP->>APP: Build empty payload (JSON bytes)
  APP->>CR: Encrypt(plaintext, aad=HeaderBytes, key)
  APP->>ST: WriteAtomic(path, Header + Ciphertext + Tag)
  ST->>FS: Write temp file, fsync, replace
  APP-->>UI: Success (vault created)

  #Flow: Unlock Vault
  sequenceDiagram
  autonumber
  actor U as User
  participant UI as Vault.Ui
  participant APP as Vault.Application
  participant ST as IVaultStore
  participant CR as ICryptoProvider

  U->>UI: Unlock (master password)
  UI->>APP: UnlockVault(path, masterPassword)
  APP->>ST: Read(path) -> (Header, Ciphertext, Tag)
  APP->>CR: DeriveKey(masterPassword, header.Salt, header.KdfParams)
  APP->>CR: Decrypt(blob, aad=HeaderBytes, key)
  alt Decrypt OK
    APP->>APP: Parse JSON payload -> Domain objects
    APP-->>UI: Unlocked session
  else Decrypt FAIL
    APP-->>UI: Invalid password or corrupted/tampered file
  end

  #Flow: Save Changes
  sequenceDiagram
  autonumber
  participant UI as Vault.Ui
  participant APP as Vault.Application
  participant CR as ICryptoProvider
  participant ST as IVaultStore

  UI->>APP: UpdateEntry/AddEntry/DeleteEntry(...)
  APP->>APP: Update in-memory vault state
  APP->>APP: Serialize payload to JSON bytes
  APP->>CR: Encrypt(plaintext, aad=HeaderBytes, key)
  APP->>ST: WriteAtomic(path, Header + Ciphertext + Tag)
  APP-->>UI: Saved OK
```

