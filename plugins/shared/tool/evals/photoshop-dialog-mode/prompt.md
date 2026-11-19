---
description: Installed UXP runtime contradicts documented dialog constant meaning
expected_outcome: Reads app.js mapping ALL=display, ERROR=silent, NONE=dontDisplay and recommends ERROR
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
plugins: ["../../skills/inspect"]
---

Our Photoshop UXP plugin's `batchPlay` calls pass `DialogModes.NONE`, since Adobe's constants page says that hides dialogs, yet users still get an "Invalid reference for multiGet" alert that pulls Photoshop to the front. We develop against Photoshop (Beta). What should we change?
