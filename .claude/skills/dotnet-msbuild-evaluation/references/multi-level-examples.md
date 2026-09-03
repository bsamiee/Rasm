# [MULTI_LEVEL_EXAMPLES]-[SHARED_BUILD_FILES]

The files serve a repository with a root `Directory.Build.props`, a nested `tests/Directory.Build.props`, and a `Directory.Build.targets`, and each project file keeps only what differs.

## [01]-[LAYOUT]

```text
<repo>/
├── Directory.Build.props         # Repository roots, artifacts layout, classification, overridable defaults
├── Directory.Build.targets       # Values derived from the project body, shared items, targets
├── Directory.Packages.props      # Every package version
├── libs/
│   └── Library/Library.csproj
└── tests/
    ├── Directory.Build.props     # Imports the root file, then test defaults
    └── Library.Tests/Library.Tests.csproj
```

## [02]-[ROOT_DIRECTORY_BUILD_PROPS]

```xml
<Project>

  <PropertyGroup>
    <RepositoryRoot>$(MSBuildThisFileDirectory)</RepositoryRoot>
    <ArtifactsPath>$([MSBuild]::NormalizePath('$(RepositoryRoot)', '.artifacts'))</ArtifactsPath>
    <Stage Condition="'$(Stage)' == ''">library</Stage>
    <Role Condition="$(MSBuildProjectDirectory.StartsWith('$(RepositoryRoot)tests'))">tests</Role>
    <Role Condition="'$(Role)' == ''">library</Role>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

</Project>
```

## [03]-[NESTED_DIRECTORY_BUILD_PROPS]

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

`dotnet msbuild tests/Library.Tests/Library.Tests.csproj -getProperty:_OuterDirectoryBuildProps,Stage,Role,IsTestProject` prints the root file path, `library`, `tests`, and `true`.

## [04]-[ROOT_DIRECTORY_BUILD_TARGETS]

```xml
<Project>

  <PropertyGroup Condition="'$(OutputType)' == 'Exe'">
    <SelfContained>false</SelfContained>
  </PropertyGroup>

  <ItemGroup>
    <Using Include="Microsoft.Extensions.Logging" Condition="'@(PackageReference->WithMetadataValue('Identity', 'Microsoft.Extensions.Logging'))' != ''" />
    <Compile Update="Generated/*.cs" AutoGen="true" />
  </ItemGroup>

</Project>
```

## [05]-[CENTRALIZED_SETTINGS]

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

[AFTER]: the root files hold the shared settings, `Directory.Packages.props` holds the version, and each project keeps only what differs

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

Use `dotnet-msbuild-packaging` for `Directory.Packages.props` and the nested package file rule.
