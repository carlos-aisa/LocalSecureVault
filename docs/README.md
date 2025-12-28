# Local Secure Vault — Documentation

This folder contains the technical documentation for the **Local Secure Vault** project.

The documentation is intended to:

- Follow the development step by step
- Understand security decisions without prior cryptography knowledge
- Serve as a future reference for the design and architecture choices made.

---

## Documents

### Architecture and Planning

- [`arch.md`](arch.md)  
  General architecture, design principles, and implementation roadmap.  

### Flows and Components  

- [`flows.md`](flows.md)  
  Diagram of main components and flows:
  - Create Vault
  - Unlock Vault
  - Save Changes

### File Format

- [`file-format.md`](file-format.md)  
  Exact specification of the encrypted file format:
  - Header
  - Payload
  - Encryption and authentication
  - Versioning

### Security and Tests

- [`security-tests.md`](security-tests.md)  
  List of mandatory security tests for the MVP:
  - KDF (Argon2id)
  - AEAD (AES-GCM)
  - File integrity
  - Clipboard and auto-lock

---

## How to Use This Documentation During Development

- Before implementing a critical part, review the corresponding document.
- Each phase of the roadmap in `arch.md` must be completed **with tests**.
- If something is not clear in the documentation, improve it before writing code.

This project prioritizes **understanding what is being built**, not just making it work. But trying that it works securely from day one. ;)