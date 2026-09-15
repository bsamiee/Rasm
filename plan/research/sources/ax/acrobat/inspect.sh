#!/usr/bin/env bash
# Acrobat Pro. Uses the older "Preferences..." wording.
APP_PROCESS="AdobeAcrobat"
APP_SLUG="acrobat"
APP_DIALOGS=$(printf '%s\n' \
  "preferences	Acrobat	Preferences...")
source "$(dirname "${BASH_SOURCE[0]}")/../lib/inspect-app.sh"
