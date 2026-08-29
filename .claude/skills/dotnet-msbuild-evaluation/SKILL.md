---
name: dotnet-msbuild-evaluation
description: "enter a description here"
---

# [DOTNET_MSBUILD_EVALUATION]

## [01]-[COMPOSITION]-[SEMICOLON_CONCATENATION]

List-valued properties append through the existing value; an assignment without `$(Self);` drops every prior entry:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
  <NoWarn>$(NoWarn);NU5131;IDE0005</NoWarn>
</PropertyGroup>
```

## [02]-[CONDITION_SYNTAX]

The `Condition` attribute runs a separate parser from the property body; quoting rules differ between the two.

- Quote both sides of `==` and `!=` with single quotes: `'$(Prop)' == ''`.
- Single quotes do not nest inside a `Condition`: `'$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)'))' == '.NETCoreApp'` raises `MSB4092`. Quote a nested function argument with backticks — `` `$(TargetFramework)` `` — as `Microsoft.Common.CurrentVersion.targets` does. A property body has no condition parser, so single quotes nest there freely.
- `Exists(...)` and `HasTrailingSlash(...)` are condition functions, called bare: `Condition="!HasTrailingSlash('$(OutDir)')"`. Spelling either as `$([MSBuild]::HasTrailingSlash(...))` raises `MSB4186`.

## [03]-[CONDITIONAL_DEFAULTS]

Set a property only when it is still empty, so every earlier import and the command line keep the override:

```xml
<PropertyGroup>
  <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
  <Platform Condition="'$(Platform)' == ''">AnyCPU</Platform>
  <BuildInParallel Condition="'$(BuildInParallel)' == ''">true</BuildInParallel>
</PropertyGroup>
```

- In `.props` the guard creates an overridable default; in `.targets` it creates a fallback.
- An unguarded assignment overwrites every earlier value.

## [04]-[NESTED_CONDITIONAL_GROUPS]

> [WARNING]: `$(TargetFramework)` is empty in `.props` files for single-targeting projects until the project body is evaluated. Place `TargetFramework`-conditioned property groups in `.targets` files or the project file itself, where the value is always available.

Group related properties under one `PropertyGroup` condition instead of repeating it per property:

```xml
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETFramework'">
  <DefineConstants>$(DefineConstants);FEATURE_APARTMENT_STATE</DefineConstants>
  <DefineConstants>$(DefineConstants);FEATURE_APM</DefineConstants>
  <FeatureAppDomain>true</FeatureAppDomain>
</PropertyGroup>
```

## [05]-[PATH_NORMALIZATION]-[TRAILING_SLASHES]

```xml
<PropertyGroup>
  <!-- Directory properties carry a trailing slash -->
  <OutDir Condition="'$(OutDir)' != ''">$([MSBuild]::EnsureTrailingSlash('$(OutDir)'))</OutDir>

  <!-- Combine and normalize; the result is absolute whether or not the last segment is rooted -->
  <TargetRefPath>$([MSBuild]::NormalizePath('$(TargetDir)', 'ref', '$(TargetFileName)'))</TargetRefPath>
  <TargetRefDir>$([MSBuild]::NormalizeDirectory('$(TargetDir)', 'ref'))</TargetRefDir>
  <MSBuildProjectExtensionsPath>$([MSBuild]::NormalizeDirectory('$(MSBuildProjectDirectory)', '$(MSBuildProjectExtensionsPath)'))</MSBuildProjectExtensionsPath>
</PropertyGroup>
```

| [INDEX] | [FUNCTION]                                       | [PURPOSE]                                       |
| :-----: | :----------------------------------------------- | :---------------------------------------------- |
|  [01]   | `$([MSBuild]::NormalizePath(...))`               | Combine and normalize a file path               |
|  [02]   | `$([MSBuild]::NormalizeDirectory(...))`          | Combine, normalize, and force a trailing slash  |
|  [03]   | `$([MSBuild]::EnsureTrailingSlash(...))`         | Append a trailing slash; leaves empty untouched |
|  [04]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(...))` | Walk up directory tree                          |
|  [05]   | `$(MSBuildThisFileDirectory)`                    | Directory of current file, slash-terminated     |

## [06]-[GUARD_PROPERTIES]

Mark a file imported so a `.targets` file can pull its `.props` sibling exactly once:

```xml
<!-- At the end of MySDK.props -->
<PropertyGroup>
  <MySDKPropsImported>true</MySDKPropsImported>
</PropertyGroup>

<!-- At the top of MySDK.targets -->
<Import Project="MySDK.props" Condition="'$(MySDKPropsImported)' != 'true'" />
```

## [07]-[TARGET_FRAMEWORK_DETECTION_HELPERS]

`IsTargetFrameworkCompatible` is the default TFM test: it spans identifier and version in one call, where a hand-parsed version misses frameworks that differ in identifier. `GetTargetFrameworkIdentifier` returns the moniker identifier — `net10.0` yields `.NETCoreApp`, never `net`.

```xml
<!-- Compatibility spans identifier and version -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net472'))">
  <UseFrozenVersions>true</UseFrozenVersions>
</PropertyGroup>

<!-- Identifier only, when the version does not matter -->
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'">
  <NetCoreBuild>true</NetCoreBuild>
</PropertyGroup>

<!-- OS detection -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <DefineConstants>$(DefineConstants);TEST_ISWINDOWS</DefineConstants>
</PropertyGroup>
```

## [08]-[FALLBACK_CHAINS]

`ValueOrDefault` resolves a primary source with a fallback in one expression:

```xml
<PropertyGroup>
  <ToolPath>$([MSBuild]::ValueOrDefault('$(MyToolPathOverride)', '$(MSBuildThisFileDirectory)tools/mytool'))</ToolPath>
</PropertyGroup>
```

## [09]-[EVALUATION_ORDER]-[LAST_WRITE_WINS]

MSBuild evaluates properties top-to-bottom across the whole import chain; the last assignment wins:

```xml
<!-- File 1 (imported first) -->
<MyProp>value1</MyProp>                                <!-- set to value1 -->
<!-- File 2 (imported second) -->
<MyProp>value2</MyProp>                                <!-- overwritten to value2 -->
<!-- File 3 (imported third) -->
<MyProp Condition="'$(MyProp)' == ''">value3</MyProp>  <!-- NOT set — already value2 -->
```

`Directory.Build.props` is imported early inside `Microsoft.Common.props`, so it sets defaults and reads only what the SDK has already defined. `Directory.Build.targets` is imported after the project body and after every NuGet package's `.targets`, so it is the last workspace-owned window that overrides package build logic.
