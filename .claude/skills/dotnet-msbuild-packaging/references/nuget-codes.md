# [NUGET_CODES]

Restore reports `NU1xxx` codes and `dotnet pack` reports `NU5xxx` codes, `TreatWarningsAsErrors` promotes both, `NoWarn` on a `PackageReference` suppresses one code for one reference.

## [01]-[RESTORE_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                       | [FIX]                                                        |
| :-----: | :------- | :------------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `NU1004` | Lock file disagrees with the project's dependencies           | `dotnet restore --force-evaluate`, commit the written lock   |
|  [02]   | `NU1008` | `PackageReference` has `Version` under CPM                    | `PackageVersion` item, or `VersionOverride` on the reference |
|  [03]   | `NU1009` | `PackageVersion` for a package the SDK references itself      | Remove the item, the SDK owns that version                   |
|  [04]   | `NU1010` | `PackageReference` has no `PackageVersion` item               | Add the item to `Directory.Packages.props`                   |
|  [05]   | `NU1011` | `PackageVersion` with a floating version                      | Write the exact version                                      |
|  [06]   | `NU1013` | `VersionOverride` while overrides are disabled                | Remove the override and change the `PackageVersion` item     |
|  [07]   | `NU1015` | `PackageReference` without `Version` outside CPM              | Enable CPM, a nested file imports the outer one at its top   |
|  [08]   | `NU1100` | Source mapping matches no source for the id                   | Add a `package pattern` under the source that holds it       |
|  [09]   | `NU1101` | No source has the id                                          | Correct the id, or add the source and its pattern            |
|  [10]   | `NU1102` | Id exists, the version does not                               | Pick a listed version, `dotnet package search` shows them    |
|  [11]   | `NU1103` | Prerelease versions alone satisfy a stable range              | Name the prerelease version in the `PackageVersion` item     |
|  [12]   | `NU1107` | Dependencies demand incompatible versions of one id           | Reference the package directly at the higher version         |
|  [13]   | `NU1109` | Transitive pinning holds a package below a required version   | Raise the `PackageVersion`, or disable transitive pinning    |
|  [14]   | `NU1201` | Referenced project targets a newer framework                  | Lower the referenced framework or raise the consumer's       |
|  [15]   | `NU1202` | Package has no asset for the project's framework              | Change the framework, or pick a version that targets it      |
|  [16]   | `NU1301` | Source unreachable or missing                                 | Remove the source, or `RestoreIgnoreFailedSources`           |
|  [17]   | `NU1403` | Package content hash differs from its lock entry              | Confirm the source, then `dotnet restore --force-evaluate`   |
|  [18]   | `NU1504` | `PackageReference` items repeat one id                        | Keep one item, `Update` changes metadata on it               |
|  [19]   | `NU1506` | `PackageVersion` items repeat one id                          | Keep one item, `Update` changes it in a nested file          |
|  [20]   | `NU1507` | CPM with more than one HTTP source and no source mapping      | Add `packageSourceMapping` with `*` on one source            |
|  [21]   | `NU1510` | Direct reference to a package the framework supplies          | Remove the `PackageReference`                                |
|  [22]   | `NU1602` | Dependency declares no lower bound                            | Reference the dependency directly at an exact version        |
|  [23]   | `NU1603` | Declared lower bound absent, a higher version resolved        | Reference the resolved version directly in `PackageVersion`  |
|  [24]   | `NU1605` | Direct reference resolved below a dependency's demand         | Raise the `PackageVersion` to the demanded version           |
|  [25]   | `NU1701` | `AssetTargetFallback` picked assets of an older framework     | Framework-built version, else `NoWarn` on the reference      |
|  [26]   | `NU1702` | `AssetTargetFallback` picked a project of an older framework  | Multi-target the referenced project or align the frameworks  |
|  [27]   | `NU1900` | Vulnerability data unreachable from a source                  | `dotnet restore --force`, a no-op restore audits nothing     |
|  [28]   | `NU1901` | Advisory on a resolved package, `NU1902`-`NU1904` by severity | Pin a patched release in `PackageVersion`                    |

## [02]-[PACK_CODES]

| [INDEX] | [CODE]   | [CAUSE]                                                           | [FIX]                                                     |
| :-----: | :------- | :---------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `NU5017` | No assembly, dependency, or framework reference packed            | Add `lib/<tfm>/_._` and keep the dependency group         |
|  [02]   | `NU5100` | Assembly sits outside `lib/<tfm>/`                                | `PackagePath="lib/<tfm>/"`, or remove its `Pack="true"`   |
|  [03]   | `NU5104` | Stable package depends on a prerelease package                    | Prerelease `Version`, or a stable dependency version      |
|  [04]   | `NU5110` | `.ps1` file sits outside `tools/`                                 | Move it under `tools/` or remove `Pack="true"`            |
|  [05]   | `NU5111` | `.ps1` file under `tools/` is not `init.ps1`                      | Rename it, `init.ps1` alone runs                          |
|  [06]   | `NU5118` | Items share one `PackagePath`                                     | One item per `PackagePath`                                |
|  [07]   | `NU5128` | Framework in `lib/` or `ref/` alone or in dependency groups alone | `SuppressDependenciesWhenPacking` off, or `lib/<tfm>/_._` |
|  [08]   | `NU5129` | `build/` file is not named `<PackageId>.props` or `.targets`      | Rename the file, another name is never imported           |
