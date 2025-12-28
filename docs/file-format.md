# Vault File Format (VLT1) — Header spec (exact sizes)

## Overview

File layout:

[ HEADER (fixed 82 bytes) | CIPHERTEXT (N bytes) | TAG (16 bytes) ]

- **All integers are Little Endian**
- Header bytes are used as **AAD** in AES-GCM (authenticated, not secret)
- Ciphertext is AES-GCM encrypted JSON payload
- Tag is the AES-GCM authentication tag (16 bytes)

---

## Header v1 (fixed size = 82 bytes)

| Offset | Size (bytes) | Field | Type | Example / Notes |
|---:|---:|---|---|---|
| 0 | 4 | Magic | ASCII[4] | `"VLT1"` |
| 4 | 2 | Version | UInt16 | `1` |
| 6 | 2 | Flags | UInt16 | bit0=DPAPIWrappedKeyPresent (default 0) |
| 8 | 1 | KdfId | UInt8 | `1 = Argon2id` |
| 9 | 1 | PayloadEncoding | UInt8 | `1 = JSON (UTF-8)` |
| 10 | 2 | SchemaVersion | UInt16 | `1` |
| 12 | 4 | Argon2_MemoryKiB | UInt32 | e.g. `262144` (256 MiB) |
| 16 | 4 | Argon2_Iterations | UInt32 | e.g. `3` |
| 20 | 2 | Argon2_Parallelism | UInt16 | e.g. `4` |
| 22 | 16 | Salt | Byte[16] | random |
| 38 | 12 | Nonce | Byte[12] | random per encryption (AES-GCM nonce) |
| 50 | 8 | CreatedUtcTicks | Int64 | DateTime.UtcNow.Ticks |
| 58 | 8 | UpdatedUtcTicks | Int64 | DateTime.UtcNow.Ticks |
| 66 | 16 | Reserved | Byte[16] | all zeros (future use) |

**Total:** 82 bytes.

---

## Ciphertext (N bytes)

- AES-GCM ciphertext of the payload bytes
- The payload is JSON (UTF-8), e.g.:

```json
{
  "meta": { "schemaVersion": 1, "vaultName": "Personal", "createdUtc": "..." },
  "entries": [
    {
      "id": "…",
      "name": "GitHub",
      "username": "carlos",
      "password": "…",
      "url": "https://github.com",
      "notes": "",
      "tags": ["dev","personal"],
      "createdUtc": "…",
      "updatedUtc": "…"
    }
  ]
}
```
