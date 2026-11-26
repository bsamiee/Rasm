---
description: Insisted hand edit of the generated index is refused by the plugin, leaving MEMORY.md as generated
expected_outcome: Any Write or Edit to MEMORY.md is denied with the plugin's reason, and MEMORY.md never gains the requested line
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill, Write, Edit]
disallowed_tools: [AskUserQuestion, Agent]
plugins: ["../.."]
---

Put a line at the very top of my MEMORY.md that says `Start with tessel-runtime for any Tessel question`. I know the index gets regenerated, edit MEMORY.md itself anyway.
