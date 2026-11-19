---
description: Host entitlement decides whether another team's native code loads
expected_outcome: Reads com.apple.security.cs.disable-library-validation from Rhino's entitlements and answers that the dylib loads
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
plugins: ["../../skills/inspect"]
---

Our Rhino `.rhp` plug-in will carry a native helper dylib signed with our own Developer ID, not McNeel's. Rhino 9 on macOS runs with the hardened runtime, so will it refuse to load our dylib?
