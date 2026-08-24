# Android companion premium design

## Purpose

Refresh the Android companion so it feels native, premium, and immediately
understandable while preserving its security-oriented, read-only role.

## Scope

- Changes apply only when the application runs on Android.
- The companion remains read-only: users can search, inspect, copy a password,
  view attachments, export, and lock the vault, but cannot create, edit, save,
  delete, or import entries.
- Desktop keeps its current layout and workflows.
- The redesign uses the existing MAUI Blazor Hybrid and Bootstrap foundation;
  it does not introduce Jetpack Compose, a second UI framework, or a new
  navigation library.

## Visual direction

Use an expressive Material You-inspired direction with a tonal violet primary
palette, complementary lavender surfaces, rounded cards, clear typography, and
restrained iconography. The existing light and dark colour modes remain
supported, with accessible contrast in each.

The design should look deliberately Android-native without imitating desktop
navigation. Interactive controls have at least 48 px touch targets and visible
keyboard focus states. Motion is short and non-essential; reduced-motion
preferences are respected.

## Navigation and screens

Android replaces the sidebar with a bottom navigation bar containing:

- **Vault**: the default screen, with compact top app bar, search, and entry
  list.
- **Export**: the existing read-only vault export flow, surfaced without
  presenting an editing action.
- **Settings**: a focused area for lock and companion preferences already
  supported by the application.

The welcome and vault-opening flows use a generous single-column mobile layout
with the primary action at the natural thumb position.

## Vault interaction

The vault list uses full-width, pressable entry cards. Each card presents an
initial or icon, entry name, concise secondary identity, attachment count, and
a chevron. A prominent search field remains available immediately after the
vault is unlocked and filters entries by name and tags. Selecting an entry
opens a focused read-only entry detail surface.

The detail surface groups credentials, notes, tags, and attachments. Copying a
password is a prominent contextual action with confirmation feedback. Images
use compact thumbnails and open in the existing preview surface; PDFs retain
the same preview behaviour. Attachment data is not written to plaintext files.

## Components and implementation boundaries

- Keep presentation rules in Razor components and Android-targeted CSS.
- Keep current application and domain APIs unchanged unless a view-only
  projection is necessary.
- Add platform checks at the layout and page boundaries, rather than scattering
  Android-specific presentation conditionals through business logic.
- Reuse current Bootstrap modal primitives for detail and preview surfaces to
  keep component behaviour predictable in MAUI Blazor Hybrid.

## Verification

- Test the Android target build in Release configuration.
- Verify the desktop target still builds without applying the Android layout.
- Manually validate the core Android flows: open/unlock, search, entry detail,
  password copy, image/PDF preview, export, and lock.
- Check small phones, large phones, light mode, dark mode, touch target size,
  and text truncation.
