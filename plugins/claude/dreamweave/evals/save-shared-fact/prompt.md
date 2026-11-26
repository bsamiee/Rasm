---
description: Fact true of every subject an existing general memory spans lands in that general memory
expected_outcome: tessel-runtime.md gains one marked App Nap line naming tessel-cli keepawake; the Prism and Facet memories stay unchanged and no file is created
max_turns: 40
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill, Write, Edit]
disallowed_tools: [AskUserQuestion, Agent]
plugins: ["../.."]
---

Remember this for all the Tessel apps: when macOS puts one into App Nap, script calls queued for it are dropped without any error. Running `tessel-cli keepawake --app <app>` before a long batch prevents it.
