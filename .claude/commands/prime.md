---
description: Initialize session with Rasm repository topology and code-quality standards context.
---

# [PRIME]

>**Dictum:** *Context initialization enables informed execution.*

Execute in sequence:

1. Run `eza . --tree --git-ignore --level=3` to map repository structure.
2. Read in parallel: `CLAUDE.md`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `Workspace.slnx`, `scripts/check-cs.sh`, `tools/cs-analyzer/Kernel/RuleCatalog.cs`.
3. Identify which library/app the current working directory targets (Rasm core / Rasm.Rhino / Rasm.Grasshopper / cs-analyzer / apps/rhino / apps/grasshopper).
4. Summarize in ≤10 bullets: architecture, target framework (.NET 10 / C# 14), library stack (LanguageExt v5, Thinktecture.Runtime.Extensions, Eto.Forms, RhinoCommon, Grasshopper2), code philosophy (singular OOP boundary, unified Fin/Validation/Eff rails, dense FP internals, greenfield posture, polymorphic collapse over file extraction), active-folder canonical templates (`RasmCommand<TSelf>`, `RhinoUi.Use<T>`, `RasmOverlay<TState>`), quality gate (`bash scripts/check-cs.sh check`).
5. State the current task framing before any edits.
