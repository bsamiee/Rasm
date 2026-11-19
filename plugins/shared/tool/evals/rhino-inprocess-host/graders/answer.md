---
type: llm
weight: 3
---

PASS if the reply says in-process Rhino 9 on macOS differs from the Windows `RhinoCore` pattern and backs that with at least two of these facts read from the installed Rhino (decompiled RhinoCommon, `nm` exports, disassembly, or equivalent): `RhinoCore` ignores its arguments, headless option, or window style on macOS; `Run`, `DoEvents`, or `DoIdle` call native entry points no bundled binary exports, so they throw `EntryPointNotFoundException`; .NET startup goes through `RhSetDotNetStartProc` or `dotnetstart`; the boot must run on the process main thread; the harness must resolve Rhino's native libraries itself.
FAIL if the reply says the Windows `RhinoCore` harness works unchanged on macOS, says in-process hosting is impossible, or states its constraints without evidence read from the installed Rhino.
