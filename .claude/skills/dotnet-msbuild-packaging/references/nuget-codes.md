# [NUGET_CODES]

Restore reports `NU1xxx` codes and `dotnet pack` reports `NU5xxx` codes, `TreatWarningsAsErrors` promotes both, `NoWarn` on a `PackageReference` silences one code for one reference.

## [01]-[RESTORE_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                      | [FIX]                                                        |
| :-----: | :------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `NU1008` | `PackageReference` has `Version` under CPM                   | `PackageVersion` item, or `VersionOverride` on the reference |
|  [02]   | `NU1009` | `PackageVersion` for a package the SDK references itself     | Delete the item, the SDK owns that version                   |
|  [03]   | `NU1010` | `PackageReference` has no `PackageVersion` item              | Add the item to `Directory.Packages.props`                   |
|  [04]   | `NU1011` | `PackageVersion` with a floating version                     | Write the exact version                                      |
|  [05]   | `NU1013` | `VersionOverride` while overrides are disabled               | Remove the override and change the `PackageVersion` item     |
|  [06]   | `NU1015` | `PackageReference` without `Version` outside CPM             | Turn CPM on, a nested file imports the outer one at its top  |
|  [07]   | `NU1100` | Source mapping matches no source for the id                  | Add a `package pattern` under the source that holds it       |
|  [08]   | `NU1101` | No source has the id                                         | Correct the id, or add the source and its pattern            |
|  [09]   | `NU1102` | Id exists, the version does not                              | Pick a listed version, `dotnet package search` shows them    |
|  [10]   | `NU1103` | Prerelease versions alone satisfy a stable range             | Name the prerelease version in the `PackageVersion` item     |
|  [11]   | `NU1107` | Two dependencies demand incompatible versions of one id      | Reference the package directly at the higher version         |
|  [12]   | `NU1109` | Transitive pinning holds a package below a dependency floor  | Raise the `PackageVersion`, or turn transitive pinning off   |
|  [13]   | `NU1201` | Referenced project targets a newer framework                 | Lower the referenced framework or raise the consumer's       |
|  [14]   | `NU1202` | Package has no asset for the project's framework             | Change the framework, or pick a version that targets it      |
|  [15]   | `NU1301` | Source unreachable or missing                                | Remove the source, or `RestoreIgnoreFailedSources`           |
|  [16]   | `NU1504` | One id appears in two `PackageReference` items               | Keep one item, `Update` changes metadata on it               |
|  [17]   | `NU1506` | One id appears in two `PackageVersion` items                 | Keep one item, `Update` changes it in a nested file          |
|  [18]   | `NU1507` | CPM with two or more HTTP sources and no source mapping      | Add `packageSourceMapping` with `*` on one source            |
|  [19]   | `NU1510` | Direct reference to a package the framework supplies         | Remove the `PackageReference`                                |
|  [20]   | `NU1602` | Dependency declares no lower bound                           | Reference the dependency directly at an exact version        |
|  [21]   | `NU1603` | Declared lower bound absent, a higher version resolved       | Reference the resolved version directly in `PackageVersion`  |
|  [22]   | `NU1605` | Direct reference resolved below a dependency's demand        | Raise the `PackageVersion` to the demanded version           |
|  [23]   | `NU1701` | `AssetTargetFallback` picked assets of an older framework    | Pick a version that targets the project's framework          |
|  [24]   | `NU1702` | `AssetTargetFallback` picked a project of an older framework | Multi-target the referenced project or align the frameworks  |

## [02]-[PACK_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                          | [FIX]                                                     |
| :-----: | :------- | :--------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `NU5017` | No assembly, dependency, or framework reference packed           | Add `lib/<tfm>/_._` and keep the dependency group         |
|  [02]   | `NU5100` | Assembly sits outside `lib/<tfm>/`                               | `PackagePath="lib/<tfm>/"`, or drop `Pack="true"` on it   |
|  [03]   | `NU5104` | Stable package depends on a prerelease package                   | Prerelease `Version`, or a stable dependency version      |
|  [04]   | `NU5110` | `.ps1` file sits outside `tools/`                                | Move it under `tools/` or drop `Pack="true"`              |
|  [05]   | `NU5111` | `.ps1` file under `tools/` is not `init.ps1`                     | Rename it, `init.ps1` alone runs                          |
|  [06]   | `NU5118` | Two items pack to one `PackagePath`                              | One item per `PackagePath`                                |
|  [07]   | `NU5128` | `lib/` or `ref/` file for a framework without a dependency group | `SuppressDependenciesWhenPacking` off, or `lib/<tfm>/_._` |
|  [08]   | `NU5129` | `build/` file is not named `<PackageId>.props` or `.targets`     | Rename the file, another name is never imported           |
