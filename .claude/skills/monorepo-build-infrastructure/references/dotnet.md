# [DOTNET]

The .NET build, packaging, and test configuration of a language area is one chain of directory files, central versions, and project files that the task runner infers targets from and the pipeline runs unchanged. Every .NET file type is scanned for scattering, duplication, and misplaced declarations, a manifest exists only where one belongs, and the root holds the central versions.

## [01]-[INFERENCE]

The .NET plugin globs every project file and every ancestor `Directory.Build.*` and `Directory.Packages.props`, and its registration options and the tag-filtered defaults decide what each inferred target runs:

| [INDEX] | [PLUGIN_FACT]                                   | [CONSEQUENCE]                                                                        |
| :-----: | :---------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `restore: false` and `build --no-restore`       | `build` depends on the root `restore` target, the one restore of the solution        |
|  [02]   | `build` depends on `^build`                     | `ProjectReference` edges order builds, `--no-dependencies` skips referenced ones     |
|  [03]   | Outputs derive from `ArtifactsPath`             | One `ArtifactsPath` under `.artifacts/` makes every output cacheable                 |
|  [04]   | `pack: false`, `publish: false`, `clean: false` | Inferred `pack` over a shared feed caches every sibling, the publish script packs    |
|  [05]   | `exclude` is a registration property            | Packaging subtrees leave the inferred graph without `project.json`                   |
|  [06]   | Directory files are per-target inputs           | Named input `dotnet` lists them, and the filtered defaults add it per target         |

`nx affected` is correct when every edge exists in the graph:
- `ProjectReference` edges come from the .NET plugin
- `PackageReference` edges from a consumer to a packaging project come from the local plugin's `createDependencies`, one static edge per reference
- `implicitDependencies` from a managed binding to its native package come from the local plugin, which pairs the projects by library name

## [02]-[PACKAGING]

Each packaging project derives `Version` from its library's manifest under `eng/native/<library>/`, checked before `GenerateNuspec`:
- `Version` is the manifest's `version-string`, and `VersionManifestFileName` names a manifest other than `vcpkg.json`
- An `Error` task fails a pack with no manifest version
- Projects locked to one central version name it as `CentralPackageId`, and an `Error` task fails a `Version` that differs from its entry

The asset-only package holds `runtimes/`, `contentFiles/any/any/` with `PackageCopyToOutput`, and a `lib/<tfm>/_._` placeholder with `IncludeBuildOutput` false, and a pinned `DeterministicTimestamp` makes the package bytes a function of content and version:
- `contentFiles` with copy-to-output keeps the staged tree in consumer output where a runtime-specific publish flattens `runtimes/` native assets
- `EnableDefaultItems` false keeps the project directory out of the package, and an `Error` task fails a pack with nothing staged

Give a library with a generated binding a managed packaging project, `Item` beside `Native.Item`, that compiles `stage/managed/*.cs` with the support sources beside its project file under `IncludeBuildOutput` true, shares the manifest version, and depends on the native `stage` target. Separate package ids keep the native assets loadable from any binding and the binding free of platform-specific content.

The local feed is a folder source in `NuGet.config` under `.artifacts/`, package source mapping pins every workspace id pattern to that source and every other id to the registry, and `globalPackagesFolder` places the one restore folder every client shares under `.cache/`.

Reference the native package beside the binding, and an `Error` task in the root `Directory.Build.targets` fails a project that references one without the other, because the binding package holds no native asset. The same target fails a binding referenced without the companion project that holds its runtime initialization.

Local feeds need no `nx release` configuration. The local plugin emits `nx-release-publish` for each library with a release tag, the .NET default runs the publish script, and the script pushes with `--skip-duplicate`.

## [03]-[SUBTREE]

Take a packaging subtree out of the root `Directory.Build.props` chain when the root enforces rules the packaging projects break: `Version` in the project file, `IsPackable` false, analyzer references under central package management, and an `ArtifactsPath` for the language area:

| [INDEX] | [FORM] | [FILE]                                   | [WHEN]                                              |
| :-----: | :----- | :--------------------------------------- | :-------------------------------------------------- |
|  [01]   | Chain  | `Import` through `GetPathOfFileAbove`    | Subtree needs the root defaults and overrides a few |
|  [02]   | Stop   | `Directory.Build.props` with no `Import` | Subtree breaks root rules and shares no default     |

```xml
<!-- Chain: the root file evaluates first and the subtree overrides after it -->
<Project>
    <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />
    <PropertyGroup>
        <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '..', '.artifacts', 'packaging'))</ArtifactsPath>
    </PropertyGroup>
</Project>
```

Pair the stop form with a minimal `Directory.Build.targets` and a `Directory.Packages.props` that sets `ManagePackageVersionsCentrally` to `false`, because MSBuild finds every directory file in the subtree and searches no further up. The native packaging subtree takes the stop form, with its own `ArtifactsPath` and the `PackageOutputPath` set to the local feed. `dotnet msbuild <project> -getProperty:<RootProperty>` proves the choice: the chain returns the root value and the stop returns empty.

Packaging projects stay out of the solution file and out of the inferred graph:
- The solution lists the projects a developer builds and tests, and a packaging project builds through its own target
- The .NET plugin registration excludes the subtree, and the local plugin infers it with its own target names

## [04]-[BUILD_FILES]

The root directory files derive every project-level fact from the project's role and its package references, and a project file states the facts the directory files cannot derive:
- The root `Directory.Build.targets` gives each project role an allowed set of reference roles and fails a reference outside it
- Compose a path from `NuGetPackageRoot` through `EnsureTrailingSlash`, the property lacks the slash under a `NUGET_PACKAGES` override
- Derive `IsTestProject` from `IsTestingPlatformApplication`, which the test framework package sets, and the plugin infers the `test` target from it
- Keep no `packages.lock.json`, central versions with transitive pinning resolve the same set, and the lock added files and drift failures
- Set `ContinuousIntegrationBuild` from the `CI` variable every hosted runner exports, and the pipeline passes its binlog switch after `--`

## [05]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                     | [CORRECT_FORM]                                                      |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Packaging projects inside the solution file                 | Project excluded from the solution and the plugin, packed by target |
|  [02]   | Subtree `Directory.Build.props` files repeating root values | `Import` through `GetPathOfFileAbove` and the overrides alone       |
|  [03]   | `ArtifactsPath` from `NormalizeDirectory`                   | `NormalizePath`, the SDK appends the separator itself               |
|  [04]   | Tool manifest that pins a one-shot tool package             | `dotnet dnx <tool>` on the command                                  |

Use `dotnet-msbuild-packaging` for the package layout, the source list and its mapping, the dotnet-outdated command line, and the MSBuild switches the pipeline passes.
