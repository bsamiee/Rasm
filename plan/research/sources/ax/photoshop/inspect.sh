#!/usr/bin/env bash
# Adobe Photoshop 2026 (Beta). Settings is a submenu; each pane is its own item.
APP_PROCESS="Adobe Photoshop 2026"
APP_SLUG="photoshop"
APP_DIALOGS=$(printf '%s\n' \
  "settings-general	Photoshop (Beta)	Settings…	General..." \
  "settings-interface	Photoshop (Beta)	Settings…	Interface..." \
  "settings-tools	Photoshop (Beta)	Settings…	Tools..." \
  "color-settings	Edit	Color Settings..." \
  "keyboard-shortcuts	Edit	Keyboard Shortcuts..." \
  "toolbar	Edit	Toolbar...")
source "$(dirname "${BASH_SOURCE[0]}")/../lib/inspect-app.sh"
