# [MULTI_LEVEL_EXAMPLES]-[SHARED_BUILD_FILES]

Full files for a repository with a root `Directory.Build.props`, a nested `tests/Directory.Build.props`, and a `Directory.Build.targets`, followed by the settings that move out of project files into them.

## [01]-[LAYOUT]

```text
<repo>/
├── Directory.Build.props         # repository roots, artifacts layout, overridable defaults
├── Directory.Build.targets       # values derived from the project body, shared items, targets
├── Directory.Packages.props      # every package version
├── libs/
│   └── Library/Library.csproj
└── tests/
    ├── Directory.Build.props     # imports the root file, then test defaults
    └── Library.Tests/Library.Tests.csproj
```

MSBuild imports the nearest `Directory.Build.props` above the project and stops, and the nested file opens with an import of the outer one. A file with nothing above it gets an empty path, and the condition skips the import instead of failing with `MSB4020`.

## [02]-[ROOT]-[DIRECTORY_BUILD_PROPS]

```xml
<Project>

  <PropertyGroup>
    <RepositoryRoot>$(MSBuildThisFileDirectory)</RepositoryRoot>
    <ArtifactsPath>$(RepositoryRoot).artifacts</ArtifactsPath>
    <Stage Condition="'$(Stage)' == ''">library</Stage>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

</Project>
```

## [03]-[NESTED]-[DIRECTORY_BUILD_PROPS]

`tests/Directory.Build.props`:

```xml
<Project>

  <PropertyGroup>
    <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />

  <PropertyGroup>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

</Project>
```

`dotnet msbuild tests/Library.Tests/Library.Tests.csproj -getProperty:_OuterDirectoryBuildProps,Stage,IsTestProject` prints the root file path, `library`, and `true`.

## [04]-[ROOT]-[DIRECTORY_BUILD_TARGETS]

```xml
<Project>

  <PropertyGroup>
    <Role Condition="$(MSBuildProjectDirectory.StartsWith('$(RepositoryRoot)tests'))">tests</Role>
    <Role Condition="'$(Role)' == ''">library</Role>
  </PropertyGroup>

  <PropertyGroup Condition="'$(OutputType)' == 'Exe'">
    <SelfContained>false</SelfContained>
  </PropertyGroup>

  <ItemGroup>
    <Using Include="Microsoft.Extensions.Logging" Condition="'@(PackageReference->WithMetadataValue('Identity', 'Microsoft.Extensions.Logging'))' != ''" />
    <Compile Update="Generated/*.cs" AutoGen="true" />
  </ItemGroup>

</Project>
```

`Role` reads a reserved property and fits `Directory.Build.props` too, the `OutputType` group and the `Using` condition read the project body and fit only `Directory.Build.targets`, and the `Compile` `Update` runs after the SDK glob that creates the items.

## [05]-[BEFORE_AFTER]-[CENTRALIZING_DUPLICATED_SETTINGS]

[BEFORE]: the same settings in every project file

```xml
<!-- libs/Library/Library.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
    <Using Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

</Project>

<!-- tests/Library.Tests/Library.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

</Project>
```

[AFTER]: the root `Directory.Build.props` and `Directory.Build.targets` above, `Directory.Packages.props` holds the version, and each project keeps only what differs

```xml
<!-- libs/Library/Library.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

</Project>

<!-- tests/Library.Tests/Library.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

</Project>
```

See `dotnet-msbuild-packaging` for `Directory.Packages.props` and the nested package file rule.
