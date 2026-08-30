# [MULTI_LEVEL_EXAMPLES]-[DIRECTORY_BUILD]

Full file examples for a typical multi-level repo layout.

## [01]-[ROOT]-[DIRECTORY_BUILD_PROPS]

```xml
<Project>

  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

## [02]-[INNER_FOLDER]-[DIRECTORY_BUILD_PROPS]

`<folder>/Directory.Build.props`:

```xml
<Project>

  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))"
         Condition="Exists('$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))')" />

  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

</Project>
```

## [03]-[INNER_FOLDER]-[TESTS]

`tests/dotnet/Directory.Build.props`:

```xml
<Project>

  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))"
         Condition="Exists('$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))')" />

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NSubstitute" />
  </ItemGroup>

</Project>
```

## [04]-[BEFORE_AFTER]-[CENTRALIZING_DUPLICATED_SETTINGS]

[BEFORE]: the same settings in every project file

```xml
<!-- libs/dotnet/LibA/LibA.csproj -->
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

<!-- libs/dotnet/LibB/LibB.csproj -->
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

<!-- libs/dotnet/LibA/LibA.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
  </ItemGroup>

</Project>

<!-- libs/dotnet/LibB/LibB.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

</Project>
```
