---
description: Request to edit an index line directly ends as a description edit the plugin regenerates the line from
expected_outcome: tessel-prism-runtime.md gains the E_LICENSE_SEAT fact and names it in its description, and the regenerated MEMORY.md line carries it
max_turns: 40
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill, Write, Edit]
disallowed_tools: [AskUserQuestion, Agent]
plugins: ["../.."]
---

The Prism line in my MEMORY.md should also say that Prism batch exports fail with `E_LICENSE_SEAT` when no license seat is free. Just edit that line in MEMORY.md.
