# [ARCHES]

Architectural arch geometry and interactive drawing commands for Rhino.

Imported from [OmkarBhagwat29/Arches](https://github.com/OmkarBhagwat29/Arches) at [`d00b64c6a6e2133ee0fa5b8bd34e728676541524`](https://github.com/OmkarBhagwat29/Arches/commit/d00b64c6a6e2133ee0fa5b8bd34e728676541524), the `master` snapshot dated September 3, 2026.

`Arches` defines the host-independent interfaces. `Rhino.Arches.Core` implements the geometry, `Rhino.Arches.Interaction` supplies point picking and drawing workflows, and `Rhino.Arches.Plugin` exposes Rhino commands. Both upstream directories belong to this dependency chain.

The projects sit directly under this app directory, with the upstream C# source and icon preserved. Project files inherit Rasm's .NET defaults and RhinoCommon catalog through `RhinoHost`; the plugin retains its `.rhp` output. `Workspace.slnx` includes all four projects, and the existing Nx plugins discover their project files.

App guidance is in [CLAUDE.md](CLAUDE.md). Git configuration is owned by Rasm's root files. The upstream solution and machine-specific Rhino 8 debugger templates remain in the source repository.
