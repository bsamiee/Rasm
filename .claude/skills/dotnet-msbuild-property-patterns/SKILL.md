---
name: dotnet-msbuild-property-patterns
description: "MSBuild property definition patterns: conditional defaults, composition/concatenation, path normalization, trailing-slash handling, TFM detection helpers, and evaluation order. USE FOR: diagnosing and fixing property definition issues and shared-property anti-patterns in .props/.csproj; DefineConstants or NoWarn overwritten instead of appended; unconditional assignments that block project-level overrides; unquoted conditions that fail on empty properties; hardcoded paths that break cross-platform builds; setting overridable defaults; property evaluation order and last-write-wins semantics."
---

# [MSBUILD_PROPERTY_PATTERNS]

Canonical property definition and manipulation patterns from the MSBuild repository.

## [01]-[CONDITIONAL_DEFAULTS]-[FOUNDATIONAL_PATTERN]

Set a property only if not already set, allowing callers to override:

```xml
<PropertyGroup>
  <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
  <Platform Condition="'$(Platform)' == ''">AnyCPU</Platform>
  <BuildInParallel Condition="'$(BuildInParallel)' == ''">true</BuildInParallel>
</PropertyGroup>
```

RULES:
- Always quote both sides: `'$(Prop)' == ''`
- In `.props`: creates overridable defaults. In `.targets`: creates fallbacks.
- Properties without the condition cannot be overridden by earlier imports.

## [02]-[NESTED_CONDITIONAL_GROUPS]

> [WARNING]: `$(TargetFramework)` is empty in `.props` files for single-targeting projects until the project body is evaluated. Place `TargetFramework`-conditioned property groups in `.targets` files (or the project file itself), where the value is always available.

Group related properties under a shared condition:

```xml
<PropertyGroup Condition="$(TargetFramework.StartsWith('net4'))">
  <DefineConstants>$(DefineConstants);FEATURE_APARTMENT_STATE</DefineConstants>
  <DefineConstants>$(DefineConstants);FEATURE_APM</DefineConstants>
  <FeatureAppDomain>true</FeatureAppDomain>
</PropertyGroup>

<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)'))' == '.NETCoreApp'">
  <NetCoreBuild>true</NetCoreBuild>
  <DefineConstants>$(DefineConstants);RUNTIME_TYPE_NETCORE</DefineConstants>
</PropertyGroup>
```

Use the outer `Condition` on `PropertyGroup` to avoid repeating the same condition on every property.

## [03]-[COMPOSITION]-[SEMICOLON_CONCATENATION]

Properties that hold lists use semicolons. Always include the existing value when appending:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
  <NoWarn>$(NoWarn);NU5131;IDE0005</NoWarn>
  <LibraryTargetFrameworks>$(FullFrameworkTFM);$(LatestDotNetCoreForMSBuild);netstandard2.0</LibraryTargetFrameworks>
</PropertyGroup>
```

## [04]-[PATH_NORMALIZATON]-[TRAILING_SLASHES]

```xml
<!-- Ensure trailing slash on directories -->
<PropertyGroup>
  <OutDir Condition="'$(OutDir)' != '' and !HasTrailingSlash('$(OutDir)')">$(OutDir)\</OutDir>
</PropertyGroup>

<!-- Normalize paths for cross-platform -->
<PropertyGroup>
  <TargetRefPath>$([MSBuild]::NormalizePath('$(TargetDir)', 'ref', '$(TargetFileName)'))</TargetRefPath>
</PropertyGroup>

<!-- Make relative path absolute -->
<PropertyGroup>
  <MSBuildProjectExtensionsPath
      Condition="'$([System.IO.Path]::IsPathRooted('$(MSBuildProjectExtensionsPath)'))' == 'false'">
    $([System.IO.Path]::Combine('$(MSBuildProjectDirectory)', '$(MSBuildProjectExtensionsPath)'))
  </MSBuildProjectExtensionsPath>
</PropertyGroup>
```

Preferred path functions:

| [INDEX] | [FUNCTION]                                       | [PURPOSE]                              |
| :-----: | :----------------------------------------------- | :------------------------------------- |
|  [01]   | `$([MSBuild]::NormalizePath(...))`               | Combine and normalize (cross-platform) |
|  [02]   | `$([System.IO.Path]::Combine(...))`              | Combine path segments                  |
|  [03]   | `$([System.IO.Path]::IsPathRooted(...))`         | Check if absolute                      |
|  [04]   | `HasTrailingSlash(...)`                          | Check for trailing slash               |
|  [05]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(...))` | Walk up directory tree                 |
|  [06]   | `$(MSBuildThisFileDirectory)`                    | Directory of current file              |

## [05]-[TARGET_FRAMEWORK_DETECTION_HELPERS]

```xml
<!-- Get TFM identifier -->
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)'))' == '.NETCoreApp'">
  <NetCoreBuild>true</NetCoreBuild>
</PropertyGroup>

<!-- Check TFM compatibility -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net472'))">
  <UseFrozenVersions>true</UseFrozenVersions>
</PropertyGroup>

<!-- OS detection -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <DefineConstants>$(DefineConstants);TEST_ISWINDOWS</DefineConstants>
</PropertyGroup>
```

## [06]-[GUARD_PROPERTIES]

Mark that a file has been imported to prevent double-imports:

```xml
<!-- At the end of MySDK.props -->
<PropertyGroup>
  <MySDKPropsImported>true</MySDKPropsImported>
</PropertyGroup>

<!-- At the top of MySDK.targets -->
<Import Project="MySDK.props" Condition="'$(MySDKPropsImported)' != 'true'" />
```

## [07]-[FEATURE_GATING_BY_MSBUILD_VERSION]

```xml
<PropertyGroup Condition="$([MSBuild]::AreFeaturesEnabled('17.10'))">
  <UseNewBehavior>true</UseNewBehavior>
</PropertyGroup>
```

## [08]-[FALLBACK_CHAINS]

Set via primary source first, then fall back:

```xml
<PropertyGroup>
  <TlbExpPath>$([Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToDotNetFrameworkSdkFile('tlbexp.exe'))</TlbExpPath>
  <TlbExpPath Condition="'$(TlbExpPath)' == ''">$(_NetFxToolsDir)TlbExp.exe</TlbExpPath>
</PropertyGroup>
```

## [09]-[EVALUATION_ORDER]-[LAST_WRITE_WINS]

MSBuild evaluates properties top-to-bottom. The last assignment wins:

```xml
<!-- File 1 (imported first) -->
<MyProp>value1</MyProp>                                <!-- set to value1 -->
<!-- File 2 (imported second) -->
<MyProp>value2</MyProp>                                <!-- overwritten to value2 -->
<!-- File 3 (imported third) -->
<MyProp Condition="'$(MyProp)' == ''">value3</MyProp>  <!-- NOT set — already value2 -->
```

Properties in `.targets` (imported late) override properties in `.props` (imported early) and the project file.

## [10]-[COMMON_MISTAKES]

- Unquoted conditions (`$(X)==true`) fail when the property is empty. Always quote both sides.
- Overwriting DefineConstants (`<DefineConstants>MY_CONST</DefineConstants>`) drops all prior constants. Always append with `$(DefineConstants);`.
- Hardcoded absolute paths break portability. Use `$(MSBuildThisFileDirectory)` or `$([MSBuild]::NormalizePath(...))`.
- Missing `Condition` on defaults makes properties non-overridable. Add `Condition="'$(Prop)' == ''"` for values meant to be defaults.