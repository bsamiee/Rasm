---
name: tessel-runtime
description: "Running a script in any Tessel app, or E_SCRIPT_PATH and hung calls: tessel-cli run, the scripts folder rule, modal dialogs blocking calls"
metadata:
  type: reference
---

Tessel apps run scripts through `tessel-cli run --app <app> <script>`, which reaches the running app over `~/Library/Tessel/<app>/ipc.sock` (tried 2026-11-18)
- Scripts folder: each app runs scripts only from its own folder `<scripts-dir>`, and a script anywhere else fails with `E_SCRIPT_PATH` (tried 2026-11-18)
- Modal dialogs: an open modal dialog blocks every script call until it closes, and `tessel-cli status --modal` reports one (tried 2026-11-19)
- Launch: `tessel-cli run` starts a closed app in the foreground, so check `tessel-cli status` first (stated 2026-11-19)
