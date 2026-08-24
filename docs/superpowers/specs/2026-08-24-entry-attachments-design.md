# Entry attachments design

## Purpose

Allow a vault entry to contain portable attachments so a user can manage them on
both desktop and mobile. The first supported attachment types are images and
PDF documents.

## Scope

- Attachments are embedded in the vault payload, never referenced by external
  paths.
- The existing vault encryption, authentication, persistence, transfer, and
  restore flow protects attachments automatically because they form part of the
  encrypted payload.
- Each entry accepts at most five attachments in total.
- Each attachment has a maximum source size of 5 MiB.
- Supported types are JPEG, PNG, GIF, WebP, and PDF.
- Existing vault files and entries without attachments remain readable without
  a mandatory migration.

## Domain model

`VaultEntry` owns an ordered collection of attachment value objects. Every
attachment contains:

- a stable GUID;
- the original filename;
- a validated MIME type;
- the binary content;
- its creation timestamp.

The domain enforces the maximum count, maximum size, accepted MIME types, and
non-empty content. Adding, replacing, or deleting an attachment updates the
entry and document timestamps. Attachments cannot be shared by entries.

The application layer exposes focused use cases to add, replace, and delete an
attachment by entry and attachment identifier. It returns the existing
application error result when the entry or attachment is absent, or when input
is invalid. UI and platform file-picker details remain outside the domain and
application layers.

## Persistence and compatibility

The serialized entry DTO gains an `attachments` array. Binary content is
represented as Base64 in the JSON payload. This keeps the file format's single
encrypted payload model intact and works unchanged on Windows and Android.

Base64 increases stored content size, but the five-by-5-MiB limit bounds that
cost. No separate filesystem, encryption keys, header changes, or sidecar
files are introduced. A missing `attachments` property deserializes to an
empty collection, preserving backwards compatibility with existing vaults.

## User experience

The entry editor shows an Attachments section integrated into the existing
Bootstrap-based form. It reuses the application's established form controls,
button hierarchy, spacing, badges, validation-message placement, neutral
colours, and English UI copy. The section must feel like another field group in
the current editor, rather than a separate document manager or a custom visual
pattern. No new UI framework, bespoke visual language, or unrelated navigation
is introduced.

The section provides:

- **Add** opens the platform file picker, accepts the supported types, then
  validates size and type before adding the attachment.
- Each attachment displays its filename and type. Images additionally show a
  thumbnail and can be opened at a larger size.
- PDFs are opened using the native platform viewer.
- **Replace** selects another valid file for the same attachment.
- **Delete** removes the selected attachment after an explicit confirmation.
- User-facing errors explain unsupported type, excessive size, five-item limit,
  or a missing attachment without exposing sensitive attachment content.

No raster editing, PDF editing, OCR, document search, or attachment sharing is
included in this feature.

## Security and reliability

- Input type is validated from the picker result and detected content where
  platform APIs make that available; file extensions alone are not trusted.
- Attachment bytes are never written to a plaintext app-managed directory and
  are not logged.
- Image thumbnails and previews exist only while the vault is unlocked and are
  released when the editing view or vault session ends.
- Saving remains atomic through the existing vault storage workflow.
- Invalid or corrupt attachment data fails safely when the vault is loaded;
  it must not produce an unhandled UI exception.

## Verification

- Domain tests cover type, size, count, add, replace, delete, timestamps, and
  invalid identifiers.
- Application tests cover successful and failed CRUD cases.
- Serialization tests cover a round trip with image and PDF bytes, omission of
  the property for legacy data, and malformed data handling.
- UI/service tests cover picker validation and attachment-list behaviour where
  existing test seams support it.
- The affected .NET test suite and build must run without warnings.

## Documentation updates during implementation

Update `docs/arch.md`, `docs/file-format.md`, and the README feature list to
describe encrypted embedded attachments and the supported types and limits.
