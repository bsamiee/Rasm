# [IMPORT_CHAIN]

Single-targeting `Microsoft.NET.Sdk` projects import the listed files in order, as `dotnet msbuild <project> -pp:expanded.xml` shows them, with the properties each file assigns.

## [01]-[CHAIN]

| [INDEX] | [FILE]                                       | [ASSIGNS_OR_IMPORTS]                                                                      |
| :-----: | :------------------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Sdk/Sdk.props`                              | `UsingMicrosoftNETSdk`, imports `Microsoft.Common.props`                                  |
|  [02]   | `Microsoft.NET.Sdk.BeforeCommon.props`       | Imports `Microsoft.NETCoreSdk.BundledVersions.props`                                      |
|  [03]   | `Microsoft.NETCoreSdk.BundledVersions.props` | `NETCoreSdkVersion`, `NETCoreSdkPortableRuntimeIdentifier`, bundled package versions      |
|  [04]   | `Current/Microsoft.Common.props`             | `ImportDirectoryBuildProps`, `DirectoryBuildPropsPath` from `GetDirectoryNameOfFileAbove` |
|  [05]   | `$(CustomBeforeDirectoryBuildProps)`         | Repository or caller import, non-empty value                                              |
|  [06]   | `Directory.Build.props`                      | Repository defaults, project-body values are empty                                        |
|  [07]   | `Sdk/UseArtifactsOutputPath.props`           | `_ArtifactsPathSetEarly`, `BaseIntermediateOutputPath` under the artifacts layout         |
|  [08]   | `Microsoft.Common.props` body                | `BaseIntermediateOutputPath` default `obj/`, `MSBuildProjectExtensionsPath`               |
|  [09]   | `obj/<project>.nuget.g.props`                | `NuGetPackageRoot`, each package `build/*.props`, absent during restore                   |
|  [10]   | `NuGet.props`                                | `ImportDirectoryPackagesProps`, `DirectoryPackagesPropsPath`                              |
|  [11]   | `Directory.Packages.props`                   | Central package versions                                                                  |
|  [12]   | `targets/Microsoft.NET.Sdk.props`            | `Configuration`, `Platform`, `OutputType`, `AssemblyName`, `RootNamespace`                |
|  [13]   | `Microsoft.NET.Sdk.DefaultItems.props`       | `Compile`, `EmbeddedResource`, and `None` globs                                           |
|  [14]   | `Microsoft.NET.Sdk.CSharp.props`             | `DefineConstants`, implicit `Using` items                                                 |
|  [15]   | Project body                                 | `TargetFramework`, `Nullable`, `ImplicitUsings`, references                               |
|  [16]   | `Sdk/Sdk.targets`                            | Imports `$(BeforeMicrosoftNETSdkTargets)`                                                 |
|  [17]   | `Microsoft.NET.Sdk.BeforeCommon.targets`     | `TargetFrameworkIdentifier`, `TargetFrameworkVersion`                                     |
|  [18]   | `Microsoft.NET.DefaultOutputPaths.targets`   | `BaseOutputPath`, `OutputPath`, `IntermediateOutputPath`, `DefaultItemExcludes`           |
|  [19]   | `Microsoft.CSharp.targets`                   | Imports `Microsoft.Common.targets`                                                        |
|  [20]   | `Microsoft.Common.CurrentVersion.targets`    | `TargetFrameworkMoniker`, `TargetDir`, `TargetPath`, imports `.user`, `NuGet.targets`     |
|  [21]   | `obj/<project>.nuget.g.targets`              | Each package `build/*.targets`, absent during restore                                     |
|  [22]   | `$(CustomBeforeDirectoryBuildTargets)`       | Repository or caller import, non-empty value                                              |
|  [23]   | `Directory.Build.targets`                    | Values derived from the project body, custom targets                                      |
|  [24]   | `$(CustomAfterDirectoryBuildTargets)`        | Repository or caller import, non-empty value                                              |
|  [25]   | `targets/Microsoft.NET.Sdk.targets`          | SDK build targets                                                                         |
|  [26]   | `Microsoft.NET.Sdk.DefaultItems.targets`     | `EnableDefaultItems` and glob removal targets                                             |
|  [27]   | `NuGet.Build.Tasks.Pack.targets`             | `IsPackable` default, `Pack` target                                                       |

## [02]-[VISIBILITY]

| [INDEX] | [PROPERTY]                                      | [ASSIGNED_IN]                                | [READABLE_FROM]                       |
| :-----: | :---------------------------------------------- | :------------------------------------------- | :------------------------------------ |
|  [01]   | `MSBuildProjectName`, `MSBuildProjectDirectory` | Reserved                                     | Every file                            |
|  [02]   | `BaseIntermediateOutputPath`, `ArtifactsPath`   | `Directory.Build.props` or SDK default       | Later files                           |
|  [03]   | `Configuration`, `Platform`, `OutputType`       | `Microsoft.NET.Sdk.props`                    | Project body and every `.targets`     |
|  [04]   | `NETCoreSdkVersion`                             | `Microsoft.NETCoreSdk.BundledVersions.props` | `Directory.Build.props` onward        |
|  [05]   | `TargetFramework`                               | Project body                                 | `Directory.Build.targets`, every item |
|  [06]   | `TargetFrameworkIdentifier`, `OutputPath`       | `Sdk.targets` imports                        | `Directory.Build.targets`             |
|  [07]   | `TargetPath`, `TargetFrameworkMoniker`          | `Microsoft.Common.CurrentVersion.targets`    | `Directory.Build.targets`             |
|  [08]   | `IsPackable`, `EnableDefaultItems`              | `Microsoft.NET.Sdk.targets` imports          | Targets, or project value             |
|  [09]   | `IsTestProject`, `IsTestingPlatformApplication` | Test package `.props` or `.targets`, project | `Directory.Build.targets`             |

Outer builds of a multi-targeting project import `Directory.Build.targets` from `Microsoft.Common.CrossTargeting.targets`, with no `CustomBeforeDirectoryBuildTargets` or `CustomAfterDirectoryBuildTargets` import.
