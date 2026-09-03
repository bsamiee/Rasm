# [IMPORT_CHAIN]-[MICROSOFT_NET_SDK]

Single-targeting `Microsoft.NET.Sdk` projects under .NET SDK 10 import the listed files in order, as `dotnet msbuild <project> -pp:expanded.xml` shows them, with the properties each file assigns and the point at which each repository file runs.

## [01]-[CHAIN]

| [INDEX] | [FILE]                                       | [ASSIGNS OR IMPORTS]                                                                      |
| :-----: | :------------------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Sdk/Sdk.props`                              | `UsingMicrosoftNETSdk`, imports `Microsoft.Common.props`                                  |
|  [02]   | `Current/Microsoft.Common.props`             | `ImportDirectoryBuildProps`, `DirectoryBuildPropsPath` from `GetDirectoryNameOfFileAbove` |
|  [03]   | `$(CustomBeforeDirectoryBuildProps)`         | Repository or caller import, non-empty value only                                         |
|  [04]   | `Directory.Build.props`                      | Repository defaults, project-body values are empty                                        |
|  [05]   | `Sdk/UseArtifactsOutputPath.props`           | `_ArtifactsPathSetEarly`, `BaseIntermediateOutputPath` under the artifacts layout         |
|  [06]   | `obj/<project>.nuget.g.props`                | `NuGetPackageRoot`, each package `build/*.props`, absent during restore                   |
|  [07]   | `Microsoft.Common.props` body                | `BaseIntermediateOutputPath` default `obj/`, `MSBuildProjectExtensionsPath`               |
|  [08]   | `NuGet.props`                                | `ImportDirectoryPackagesProps`, `DirectoryPackagesPropsPath`                              |
|  [09]   | `Directory.Packages.props`                   | Central package versions                                                                  |
|  [10]   | `targets/Microsoft.NET.Sdk.props`            | `Configuration`, `Platform`, `OutputType`, `AssemblyName`, `RootNamespace`                |
|  [11]   | `Microsoft.NET.Sdk.DefaultItems.props`       | The `Compile`, `EmbeddedResource`, and `None` globs                                       |
|  [12]   | `Microsoft.NETCoreSdk.BundledVersions.props` | `NETCoreSdkVersion`, `BundledNETCoreAppPackageVersion`                                    |
|  [13]   | `Microsoft.NET.Sdk.CSharp.props`             | `DefineConstants`, the implicit `Using` items                                             |
|  [14]   | The project body                             | `TargetFramework`, `Nullable`, `ImplicitUsings`, references                               |
|  [15]   | `Sdk/Sdk.targets`                            | Imports `$(BeforeMicrosoftNETSdkTargets)`                                                 |
|  [16]   | `Microsoft.NET.Sdk.BeforeCommon.targets`     | `TargetFrameworkIdentifier`, `TargetFrameworkVersion`                                     |
|  [17]   | `Microsoft.NET.DefaultOutputPaths.targets`   | `BaseOutputPath`, `OutputPath`, `IntermediateOutputPath`, `DefaultItemExcludes`           |
|  [18]   | `Microsoft.CSharp.targets`                   | Imports `Microsoft.Common.targets`                                                        |
|  [19]   | `Microsoft.Common.CurrentVersion.targets`    | `TargetFrameworkMoniker`, `TargetDir`, `TargetPath`, imports `.user`, `NuGet.targets`     |
|  [20]   | `obj/<project>.nuget.g.targets`              | Each package `build/*.targets`, absent during restore                                     |
|  [21]   | `$(CustomBeforeDirectoryBuildTargets)`       | Repository or caller import, non-empty value only                                         |
|  [22]   | `Directory.Build.targets`                    | Values derived from the project body, custom targets                                      |
|  [23]   | `$(CustomAfterDirectoryBuildTargets)`        | Repository or caller import, non-empty value only                                         |
|  [24]   | `targets/Microsoft.NET.Sdk.targets`          | The SDK build targets                                                                     |
|  [25]   | `Microsoft.NET.Sdk.DefaultItems.targets`     | `EnableDefaultItems` and the glob removal targets                                         |
|  [26]   | `NuGet.Build.Tasks.Pack.targets`             | `IsPackable` default, the `Pack` target                                                   |

## [02]-[VISIBILITY]

| [INDEX] | [PROPERTY]                                      | [ASSIGNED IN]                                | [READABLE FROM]                       |
| :-----: | :---------------------------------------------- | :------------------------------------------- | :------------------------------------ |
|  [01]   | `MSBuildProjectName`, `MSBuildProjectDirectory` | Reserved                                     | Every file                            |
|  [02]   | `BaseIntermediateOutputPath`, `ArtifactsPath`   | `Directory.Build.props` or the SDK default   | Later files                           |
|  [03]   | `Configuration`, `Platform`, `OutputType`       | `Microsoft.NET.Sdk.props`                    | The project body and every `.targets` |
|  [04]   | `NETCoreSdkVersion`                             | `Microsoft.NETCoreSdk.BundledVersions.props` | The project body and every `.targets` |
|  [05]   | `TargetFramework`                               | The project body                             | `Directory.Build.targets`, every item |
|  [06]   | `TargetFrameworkIdentifier`, `OutputPath`       | `Sdk.targets` imports                        | `Directory.Build.targets`             |
|  [07]   | `TargetPath`, `TargetFrameworkMoniker`          | `Microsoft.Common.CurrentVersion.targets`    | `Directory.Build.targets`             |
|  [08]   | `IsPackable`, `EnableDefaultItems`              | `Microsoft.NET.Sdk.targets` imports          | Targets, or the project value         |
|  [09]   | `IsTestProject`                                 | Test package `.props` or the project         | `Directory.Build.targets`             |

The outer build of a multi-targeting project imports `Directory.Build.targets` from `Microsoft.Common.CrossTargeting.targets`, which imports neither `CustomBeforeDirectoryBuildTargets` nor `CustomAfterDirectoryBuildTargets`.
