---
description: In-process Rhino host on macOS differs from the Windows RhinoCore pattern
expected_outcome: Names macOS constraints read from installed files, such as missing exports behind Run, DoEvents, and DoIdle
max_turns: 60
timeout_seconds: 900
allowed_tools: [Read, Glob, Grep, Skill]
plugins: ["../../skills/inspect"]
---

We want our .NET unit tests on this Mac to spin up Rhino 9 in-process with RhinoCommon's `RhinoCore`, headless, so tests can call RhinoCommon directly. Rhino 9 is installed. Can we do that, and what approach should the test harness take?
