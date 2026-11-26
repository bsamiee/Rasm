---
description: Correction to one memory replaces its value in place with Edit, keeping no history
expected_outcome: tessel-facet-runtime.md names ~/Library/Tessel/Facet/Automations with a fresh mark, the old Facet/Scripts path is gone with no history wording, and no file is created or rewritten
max_turns: 40
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill, Write, Edit]
disallowed_tools: [AskUserQuestion, Agent]
plugins: ["../.."]
---

Facet 4 moved its scripts folder, it's `~/Library/Tessel/Facet/Automations` now, so whatever your memory says about where Facet scripts go is out of date. Fix it.
