# [MULTI_LEVEL_EXAMPLES]-[SHARED_BUILD_FILES]

Full file examples for a typical multi-level repo layout, and the before/after of settings centralized out of project files.

## [01]-[LAYOUT]

```text
<repo>/
├── Directory.Build.props         # repo-wide defaults
├── Directory.Packages.props      # every package version
├── <libs>/Directory.Build.props  # imports the root, adds library settings
└── <tests>/
    ├── Directory.Build.props     # imports the root, adds test settings
    └── Directory.Packages.props  # imports the root, adds test-only packages
```

A nested `Directory.Packages.props` hides the root file, so it imports the root first:

```xml
<!-- <tests>/Directory.Packages.props -->
<Project>
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Packages.props', '$(MSBuildThisFileDirectory)../'))" />

  <ItemGroup>
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Update="xunit.v3" Version="4.0.0" />
  </ItemGroup>
</Project>
```

## [02]-[ROOT]-[DIRECTORY_BUILD_PROPS]

```xml
<Project>

  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

## [03]-[INNER_FOLDER]-[DIRECTORY_BUILD_PROPS]

`<inner>/Directory.Build.props`:

```xml
<Project>

  <PropertyGroup>
    <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />

  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

</Project>
```

## [04]-[INNER_FOLDER]-[TESTS]

`<tests>/Directory.Build.props`:

```xml
<Project>

  <PropertyGroup>
    <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="NSubstitute" />
  </ItemGroup>

</Project>
```

`xunit.v3` carries its own Microsoft.Testing.Platform runner. `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` select VSTest, which `global.json` rejects when it names `Microsoft.Testing.Platform`.

## [05]-[BEFORE_AFTER]-[CENTRALIZING_DUPLICATED_SETTINGS]

[BEFORE]: the same settings in every project file

```xml
<!-- <libs>/LibA/LibA.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Company>Contoso</Company>
    <Authors>Contoso Engineering</Authors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>

</Project>

<!-- <libs>/LibB/LibB.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Company>Contoso</Company>
    <Authors>Contoso Engineering</Authors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
  </ItemGroup>

</Project>
```

[AFTER]: settings moved to `Directory.Build.props` and `Directory.Packages.props`

```xml
<!-- Directory.Build.props -->
<Project>

  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Company>Contoso</Company>
    <Authors>Contoso Engineering</Authors>
  </PropertyGroup>

</Project>

<!-- Directory.Packages.props -->
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
  </ItemGroup>

</Project>

<!-- <libs>/LibA/LibA.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
  </ItemGroup>

</Project>

<!-- <libs>/LibB/LibB.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

</Project>
```
