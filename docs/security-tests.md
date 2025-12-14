
---

## `docs/security-tests.md`

```md
# Security Test Suite (must-have)

This is the minimum set of tests that should exist before shipping a usable MVP.
We treat crypto + file format as a “security-critical subsystem”.

## A. KDF (Argon2id) tests

1) **Determinism with same inputs**
- Given same master password + same salt + same params => derived key must be identical.

2) **Different salt => different key**
- Same password/params, different salt => derived key must differ.

3) **Parameter persistence**
- Create vault with params A, unlock must use params A stored in header (not defaults).
- Create vault with params B, unlock must use params B.

4) **Timing sanity (non-functional)**
- A benchmark-style test (not unit test if flaky) to verify the default profile is within expected range on target machine (e.g. 500–1200ms).

## B. AEAD (AES-GCM) tests

5) **Roundtrip encryption**
- Encrypt payload -> decrypt -> bytes must match exactly.

6) **Wrong password fails**
- Use wrong master password (=> wrong derived key) => decrypt must fail.

7) **Tamper detection — ciphertext**
- Flip 1 random byte in ciphertext => decrypt must fail.

8) **Tamper detection — tag**
- Flip 1 byte in tag => decrypt must fail.

9) **Tamper detection — header**
- Flip 1 byte in header (e.g. iterations, nonce, schema) => decrypt must fail (because header is AAD).

10) **Nonce uniqueness policy**
- Ensure encryption always generates a new random nonce per save
- Ensure the saved nonce is the one used for decryption
- (You don’t need to test collision probability; test “we never reuse nonce intentionally”)

## C. File format & storage tests

11) **Magic/version validation**
- Wrong magic => reject.
- Unsupported version => reject.

12) **Header size enforcement**
- File shorter than header+tag => reject.
- Header read must be exactly 82 bytes for v1.

13) **Atomic write**
- Simulate partial write (write temp only) => original vault remains readable.
- After successful replace => new file readable.

14) **Concurrent access**
- Two writers => one must fail or serialize; no corruption.
- Reader during write => either reads old valid file or fails cleanly, never returns garbage.

15) **Cross-process lock**
- Locking strategy works across processes (Windows) to reduce corruption risk.

## D. Sensitive data handling (best-effort)

16) **No secret in logs**
- Tests or code review checks that logging never prints password, key material, plaintext payload.

17) **Clipboard auto-clear**
- Copy password -> clipboard has value.
- After timeout -> clipboard cleared (or restored if you implement “restore previous clipboard”).
- If user overwrites clipboard before timeout -> do not clear user’s new content (policy decision; test accordingly).

18) **Auto-lock**
- With idle time > threshold => vault state transitions to locked.
- After lock => operations requiring unlocked state fail fast.

## E. Negative tests / robustness

19) **Random corruption fuzz**
- Mutate random bytes in a copy of a vault file; unlock must fail safely (no crashes).

20) **JSON parse failure**
- If decrypted bytes are not valid JSON => fail gracefully (treat as corruption).

---

## Acceptance criteria for MVP

- All tests A–C must be green.
- Clipboard + auto-lock tests (D) green.
- No known path returns decrypted content without successful AEAD verification.
