---
description: Installed binary contradicts documented UXP version
expected_outcome: Reports UXP 9.5.0 read from the Premiere Pro (Beta) binary, over the documented 9.4
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
plugins: ["../../skills/inspect"]
---

I'm filling in the manifest for a Premiere Pro UXP plugin and need the UXP version Premiere Pro (Beta) runs, which is what we have installed. Adobe's docs list 9.4. What should I put?
