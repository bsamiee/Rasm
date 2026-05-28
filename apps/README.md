# [H1][APPS]
>**Dictum:** *One folder per host platform; one folder per plugin under it.*

<br>

Top-level layout for shippable plugins. The host platform (Rhino-only vs Grasshopper) determines the parent folder; each plugin gets its own subdirectory containing a single `.csproj` that emits a `.rhp` artifact via the Yak packaging pipeline (`uv run python -m tools.quality bridge package`).

```
apps/
├── grasshopper/        # Grasshopper 2 plugins (component libraries)
│   └── <Plugin>/<Plugin>.csproj
└── rhino/              # Rhino-only plugins (commands, UI, no GH components)
    └── <Plugin>/<Plugin>.csproj
```

| [INDEX] | [HOST]        | [WHEN_TO_USE]                                                        | [EXAMPLES]                 |
| :-----: | ------------- | -------------------------------------------------------------------- | -------------------------- |
|   [1]   | `grasshopper` | Plugin exposes Grasshopper2 components, parameter ports, IDataAccess | `apps/grasshopper/Radyab/` |
|   [2]   | `rhino`       | Plugin exposes Rhino commands, panels, overlays — no GH components   | *(to be populated)*        |

---
## [1][CSPROJ_CONVENTIONS]

`Directory.Build.props` auto-classifies projects by path:

- `IsGrasshopperPluginProject = true` when the project is under `apps/grasshopper/`
- `IsRhinoPluginProject = true` when the project is under `apps/rhino/`

Both classifications imply:

- `TargetExt = .rhp` (the Yak artifact)
- `EnableDynamicLoading = true`
- RhinoCommon + Eto + (for Grasshopper) Grasshopper2 / GrasshopperIO references resolved from `RhinoWIP.app`
- `UseWorkspaceLibraries = false` (no LanguageExt prelude auto-import; plugin assemblies stay minimal)

To add a new plugin:

1. Create `apps/<host>/<PluginName>/<PluginName>.csproj` with `<TargetFramework>net10.0</TargetFramework>` and the `RhinoPluginAssemblyGuid` / `RhinoPluginIconResource` properties as needed.
2. Add the project to `Workspace.slnx` under the matching `/apps/<host>/<PluginName>/` folder.
3. Add `<YakPackageSlug>` to the project and `tools/yak/<slug>/manifest.yml` so `uv run python -m tools.quality bridge package <slug> <version>` resolves the artifact through MSBuild.
4. Tests for the plugin live at `tests/csharp/<PluginName>/<PluginName>.Tests.csproj` for static specs. Runtime bridge scenarios live under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/` for library-owned behavior, including plugin-facing GH UI slices. See the `cs-testing` skill.

---
## [2][REFERENCE_PLUGIN]

`apps/grasshopper/Radyab/Radyab.csproj` is the live exemplar of the Grasshopper-host convention. Match its layout when adding new plugins:

```
apps/grasshopper/Radyab/
├── Components/         # IComponentDefinition implementations
├── Icons/              # plugin.ico + .ghicon assets
├── Library.cs          # plugin manifest (IoId, Author, Icon)
└── Radyab.csproj
```
