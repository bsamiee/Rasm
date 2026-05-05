# Handoff

Temporary handoff for next session. Delete after absorbing.

## Current State

- Repo targets RhinoWIP on macOS only.
- Current C# target is `net10.0` because local RhinoWIP was verified as running .NET 10.
- Rhino/GH SDK references come from installed `/Applications/RhinoWIP.app`, not stale NuGet assumptions.
- Grasshopper folder/product name stays `grasshopper`, but implementation uses GH2 APIs.
- Python/RhinoCode publishing remains deferred.

## Session Changes

- Initialized Git and committed progress, then rewrote history to remove `.vscode/settings.json`.
- `.vscode/settings.json` is local-only and ignored; local file nests `LICENSE` under `README.md`.
- Flattened Rhino/Grasshopper app folders:
  - `apps/rhino`
  - `apps/grasshopper`
- Moved shared C# library from `libs/Rasm.Kernel` to `libs/csharp/core`.
- Renamed shared C# project from `Rasm.Kernel.csproj` to `Core.csproj`.
- Updated app references to `../../libs/csharp/core/Core.csproj`.
- Moved analyzer tooling out of `apps`:
  - `tools/cs-analyzer`
  - `tools/py_analyzer`
- Moved Yak metadata from `yak/rasm` to `tools/yak`.
- `scripts/rhino.sh` now copies `tools/yak/manifest.yml` and `tools/yak/icon.png`.
- README now documents `apps`, `libs/csharp`, future `libs/python`, and `tools`.

## Naming Standard

- Do not prefix new folders, shared libraries, or project names with `Rasm.` by default.
- Use short role names:
  - `core`
  - `analysis`
  - `geometry`
  - `io`
- Use language buckets under `libs`:
  - `libs/csharp/<name>`
  - `libs/python/<name>`
- Keep `Rasm.*` only where product/runtime identity truly requires it, such as existing Rhino or Grasshopper plugin outputs, until those are deliberately reviewed.
- Do not rename public/plugin artifacts casually. Verify host/plugin metadata implications first.

## Verified Commands

- `dotnet restore Workspace.slnx`
- `dotnet build Workspace.slnx --configuration Release --no-restore`
- `pnpm check:cs`
- `bash -n scripts/rhino.sh`
- `shellcheck scripts/rhino.sh`
- `scripts/rhino.sh package 0.1.0-wip.6`

## Git Checkpoint Note

- Latest committed checkpoint includes:
  - `libs/csharp/core`
  - `tools/yak`
  - updated app project references
  - updated README/script/solution/lock files
- Before new work, verify:
  - `git status --short --untracked-files=no`
  - `git diff --stat`

## Research Discipline

- Do not guess RhinoWIP runtime, SDK, Yak, or GH2 behavior.
- Re-verify current facts with local app assemblies, local commands, or current official McNeel docs.
- Use exact dates and exact versions when discussing WIP behavior.
- Avoid old Rhino 8, GH1 `.gha`, Windows, or legacy assumptions unless user explicitly changes scope.

## Next Session Focus

Start with a full tree review:

```bash
eza -la --tree --group-directories-first --git-ignore
```

Then inspect actual content before proposing moves:

- List every tracked project file and folder.
- Identify stale names, redundant nesting, and inconsistent schemas.
- Treat `libs/` as a primary architecture question, not a settled decision.
- Understand `apps/rhino` and `apps/grasshopper` before changing them.
- Do not assume each app folder can only contain one app forever.
- Consider whether future multiple Rhino or Grasshopper outputs need subfolders.
- Check if `apps/rhino` and `apps/grasshopper` are product buckets, output roots, or current single-output shortcuts.
- Find opportunities to centralize shared logic in `libs/csharp/core` only when it is real shared logic.
- Avoid duplicating boundary/plugin code unless Rhino/GH APIs require it.
- Prefer fewer files and less code when behavior remains clear and scalable.
- Preserve official RhinoWIP/GH2/Yak conventions over invented structure.

## Libs Architecture Focus

Investigate `libs/` before adding code:

- Decide whether `libs/csharp/core/Core.csproj` is the right granularity.
- Compare one C# project at `libs/csharp/Core.csproj` with folders under it:
  - `core`
  - `analysis`
  - `geometry`
- Compare that against one `.csproj` per library folder.
- Avoid `.csproj` spam unless separate assembly/package boundaries are useful.
- Use separate C# projects only for real reasons:
  - different dependencies
  - different target frameworks
  - independent versioning
  - plugin loading boundaries
  - sharply different analyzer/build settings
- Prefer one shared C# assembly when folders are only organizational modules.
- Verify how SDK-style `.csproj` includes source recursively before restructuring.
- For Python, decide whether `libs/python` should be one package root or multiple packages.
- Avoid creating many Python package folders unless imports, packaging, or runtime ownership requires them.
- Keep `tools/py_analyzer` separate from runtime Python libs.
- The target is less infrastructure, fewer project/package files, and clearer module ownership.

## Review Questions

- Should app projects keep `Rasm.Rhino.csproj` and `Rasm.Grasshopper.csproj`, or should project names lose `Rasm.` too?
- Are `apps/rhino` and `apps/grasshopper` final app roots or language/product buckets?
- Should `libs/csharp` have one higher-level `.csproj` with module folders beneath it?
- Should `Core` be a folder/module inside a broader C# shared project instead of a project root?
- Should future Python runtime code use one `libs/python` package first, then split only when needed?
- Can Rhino and Grasshopper plugin metadata be centralized without hiding required SDK-specific declarations?

## Guardrails

- Make surgical changes only.
- Keep root configs authoritative.
- Do not add placeholder directories/files just to reserve structure.
- Do not add broad abstractions before a second real use appears.
- Re-run impacted gates after any path or project rename.
