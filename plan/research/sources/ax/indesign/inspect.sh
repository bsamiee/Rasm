#!/usr/bin/env bash
# Adobe InDesign 2026 (Beta). Uses "Preferences" with no ellipsis.
APP_PROCESS="Adobe InDesign 2026 (Beta)"
APP_SLUG="indesign"
APP_DIALOGS=$(printf '%s\n' \
  "preferences-general	InDesign (Beta)	Preferences	General..." \
  "preferences-interface	InDesign (Beta)	Preferences	Interface..." \
  "color-settings	Edit	Color Settings..." \
  "keyboard-shortcuts	Edit	Keyboard Shortcuts...")
source "$(dirname "${BASH_SOURCE[0]}")/../lib/inspect-app.sh"
