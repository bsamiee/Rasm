---
type: llm
focus: { source: file, path: .memory/tessel-prism-runtime.md }
---

PASS if the file still states Prism's own facts (its `~/Library/Tessel/Prism/Scripts` folder and the `--headless` flag for batch exports), no longer states the modal-dialog behavior itself, and links the new general memory through a `[[<stem>]]` link inside a line with words naming what that file holds (the modal-dialog blocking or its status check).
FAIL if Prism's own facts are missing, the modal-dialog behavior or its `--modal` check is still stated in this file, or the link appears only as a bare trailing `Related:` list or an opening routing line.
