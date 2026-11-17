# [MULTI_LEVEL_EXAMPLES]

Repository with a root `Directory.Build.props`, a nested `tests/Directory.Build.props`, and a `Directory.Build.targets`, each project file keeping what differs.

## [01]-[LAYOUT]

```text
<repo>/
├── Directory.Build.props         # Repository roots, artifacts layout, overridable defaults
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
        <Stage>library</Stage>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
        <IsPackable>false</IsPackable>
    </PropertyGroup>
</Project>
```

## [03]-[NESTED_DIRECTORY_BUILD_PROPS]

```xml
<!-- tests/Directory.Build.props -->
<Project>
    <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

    <PropertyGroup>
        <OutputType>Exe</OutputType>
    </PropertyGroup>
</Project>
```

`Library.Tests` evaluates `Stage` to `library` and `OutputType` to `Exe`.

## [04]-[ROOT_DIRECTORY_BUILD_TARGETS]

```xml
<Project>
    <PropertyGroup Condition="'$(OutputType)' == 'Exe'">
        <SelfContained>false</SelfContained>
    </PropertyGroup>

    <ItemGroup>
        <Using Include="Microsoft.Extensions.Logging" Condition="@(PackageReference->AnyHaveMetadataValue('Identity', 'Microsoft.Extensions.Logging'))" />
        <Compile Update="Generated/*.cs" AutoGen="true" />
    </ItemGroup>
</Project>
```

## [05]-[CENTRALIZED_SETTINGS]

[BEFORE]:
- Same settings in every project file

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
    </PropertyGroup>
</Project>
```

[AFTER]:
- Root files hold the shared settings, `Directory.Packages.props` holds the version

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
