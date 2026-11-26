---
description: Fact about one subject no memory covers becomes a specific memory under its general
expected_outcome: New tessel-lattice-runtime.md holding the Lattice scripts folder with a [[tessel-runtime]] link clause and none of the general memory's facts
max_turns: 40
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill, Write, Edit]
disallowed_tools: [AskUserQuestion, Agent]
plugins: ["../.."]
---

Remember this about Tessel Lattice: it only runs scripts saved in `~/Library/Tessel/Lattice/Scripts`, anything elsewhere fails with `E_SCRIPT_PATH`.
