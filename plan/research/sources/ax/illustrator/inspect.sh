#!/usr/bin/env bash
# Adobe Illustrator 2026 (Beta). Settings is a single item, not a submenu.
APP_PROCESS="Adobe Illustrator"
APP_SLUG="illustrator"
APP_DIALOGS=$(printf '%s\n' \
  "settings	Illustrator (Beta)	Settings…" \
  "color-settings	Edit	Color Settings..." \
  "keyboard-shortcuts	Edit	Keyboard Shortcuts..." \
  "manage-workspaces	Window	Workspace	Manage Workspaces..." \
  "manage-toolbars	Window	Toolbars	Manage Toolbars...")
source "$(dirname "${BASH_SOURCE[0]}")/../lib/inspect-app.sh"
