# Contributing to Local Secure Vault

Thanks for helping improve Local Secure Vault.

## Ground Rules

- Keep changes focused and reviewable.
- Follow existing architecture boundaries (Domain, Application, Crypto, Storage, App).
- Do not introduce logging of secrets, passwords, plaintext payloads, or key material.
- Add or update tests for every behavior change.

## Development Setup

1. Install .NET SDK from global.json.
2. Clone the repository.
3. Restore dependencies:

```powershell
dotnet restore LocalSecureVault.sln
```

## Build and Test

Run tests (portable target):

```powershell
dotnet test tests\Vault.Tests\Vault.Tests.csproj -f net8.0 -c Release
```

Run all tests:

```powershell
dotnet test LocalSecureVault.sln -c Release
```

## Coding Standards

- Nullable reference types must remain enabled.
- Warnings are treated as errors; keep builds warning-free.
- Prefer explicit names and small methods for security-critical code.
- Document security decisions in docs/ when behavior changes.

## Commit and Pull Request Guidelines

- Use conventional-style prefixes when possible: feat, fix, docs, refactor, test, chore.
- One pull request should solve one problem.
- Include:
  - What changed
  - Why it changed
  - Risks and mitigation
  - Test evidence

## Security-Sensitive Changes

For changes touching cryptography, file format, or vault unlock flow:

- Update docs/file-format.md or docs/arch.md when needed.
- Add negative tests for tampering/corruption paths.
- Verify behavior for wrong-password and corrupted-header scenarios.

## Reporting Vulnerabilities

Please do not open public issues for security vulnerabilities.
Use the process in SECURITY.md.
