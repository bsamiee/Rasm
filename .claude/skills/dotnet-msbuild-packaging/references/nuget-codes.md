# [NUGET_CODES]

Restore reports `NU1xxx` codes and `dotnet pack` reports `NU5xxx` codes, `TreatWarningsAsErrors` promotes both, and `NoWarn` on a `PackageReference` silences one code for one reference.

## [01]-[RESTORE_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                      | [FIX]                                                        |
| :-----: | :------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `NU1004` | Locked mode found a graph unlike the lock file               | `dotnet restore --force-evaluate`, then commit the lock file |
|  [02]   | `NU1008` | `PackageReference` has `Version` under CPM                   | `PackageVersion` item, or `VersionOverride` on the reference |
|  [03]   | `NU1009` | `PackageVersion` for a package the SDK references itself     | Delete the item, the SDK owns that version                   |
|  [04]   | `NU1010` | `PackageReference` has no `PackageVersion` item              | Add the item to `Directory.Packages.props`                   |
|  [05]   | `NU1011` | `PackageVersion` with a floating version                     | Write the exact version                                      |
|  [06]   | `NU1013` | `VersionOverride` while overrides are disabled               | Remove the override and change the `PackageVersion` item     |
|  [07]   | `NU1015` | `PackageReference` without `Version` outside CPM             | Turn CPM on, a nested file imports the outer one at its top  |
|  [08]   | `NU1100` | Source mapping matches no source for the id                  | Add a `package pattern` under the source that holds it       |
|  [09]   | `NU1101` | No source has the id                                         | Correct the id, or add the source and its pattern            |
|  [10]   | `NU1102` | The id exists, the version does not                          | Pick a listed version, `dotnet package search` shows them    |
|  [11]   | `NU1103` | Only prerelease versions satisfy a stable range              | Name the prerelease version in the `PackageVersion` item     |
|  [12]   | `NU1107` | Two dependencies demand incompatible versions of one id      | Reference the package directly at the higher version         |
|  [13]   | `NU1109` | Transitive pinning holds a package below a dependency floor  | Raise the `PackageVersion`, or turn transitive pinning off   |
|  [14]   | `NU1201` | The referenced project targets a newer framework             | Lower the referenced framework or raise the consumer's       |
|  [15]   | `NU1202` | The package has no asset for the project's framework         | Change the framework, or pick a version that targets it      |
|  [16]   | `NU1301` | The source did not answer                                    | Remove the source, or `RestoreIgnoreFailedSources`           |
|  [17]   | `NU1504` | One id appears in two `PackageReference` items               | Keep one item, `Update` changes metadata on it               |
|  [18]   | `NU1506` | One id appears in two `PackageVersion` items                 | Keep one item, `Update` changes it in a nested file          |
|  [19]   | `NU1507` | CPM with two or more HTTP sources and no source mapping      | Add `packageSourceMapping` with `*` on one source            |
|  [20]   | `NU1510` | Direct reference to a package the framework supplies         | Remove the reference and its `PackageVersion` item           |
|  [21]   | `NU1512` | `RestoreLockedMode` and `RestoreForceEvaluate` both `true`   | Pass `--force-evaluate` without locked mode, locally         |
|  [22]   | `NU1602` | The dependency declares no lower bound                       | Reference the dependency directly at an exact version        |
|  [23]   | `NU1603` | Declared lower bound absent, a higher version resolved       | Reference the resolved version directly in `PackageVersion`  |
|  [24]   | `NU1605` | The direct reference resolved below a dependency's demand    | Raise the `PackageVersion` to the demanded version           |
|  [25]   | `NU1701` | `AssetTargetFallback` picked assets of an older framework    | Pick a version that targets the project's framework          |
|  [26]   | `NU1702` | `AssetTargetFallback` picked a project of an older framework | Multi-target the referenced project or align the frameworks  |

## [02]-[PACK_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                          | [FIX]                                                   |
| :-----: | :------- | :--------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `NU5017` | No assembly, dependency, or framework reference packed           | Add `lib/<tfm>/_._` and keep the dependency group       |
|  [02]   | `NU5100` | The assembly sits outside `lib/<tfm>/`                           | `PackagePath="lib/<tfm>/"`, or drop `Pack="true"` on it |
|  [03]   | `NU5104` | The stable package depends on a prerelease package               | Prerelease `Version`, or a stable dependency version    |
|  [04]   | `NU5105` | The version uses SemVer 2.0.0 parts an old client cannot read    | Keep the version, only clients before 4.3 are affected  |
|  [05]   | `NU5110` | The `.ps1` file sits outside `tools/`                            | Move it under `tools/` or drop `Pack="true"`            |
|  [06]   | `NU5111` | The `.ps1` file under `tools/` is not `init.ps1`                 | Rename it, only `init.ps1` runs                         |
|  [07]   | `NU5118` | Two items pack to one `PackagePath`                              | One item per `PackagePath`                              |
|  [08]   | `NU5128` | `lib/<tfm>/` has a file and no dependency group for it           | Turn `SuppressDependenciesWhenPacking` off              |
|  [09]   | `NU5129` | The `build/` file is not named `<PackageId>.props` or `.targets` | Rename the file, another name is never imported         |
