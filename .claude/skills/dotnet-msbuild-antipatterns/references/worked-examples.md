# [WORKED_EXAMPLES]

Full files for the catalog entries with a fix spanning more than one element.

## [01]-[DUPLICATE_PUBLISH_INSTANCE]

The `DependsOnTargets="Publish"` target reads `_IsPublishing`, which nothing sets in a plain build, and `-check` reports `BC0201` for that read. Set `build_check.BC0201.severity = none` under a `[*.csproj]` section in `.editorconfig`. BuildCheck ignores a key outside a section. `AllowUninitializedPropertiesInConditions` covers no read outside a condition.

A consumer calling `Publish` on a tool project:

```xml
<!-- BAD -->
<Target Name="PublishTool" BeforeTargets="Build">
  <MSBuild Projects="../Tool/Tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- GOOD -->
<ItemGroup>
  <ProjectReference Include="../Tool/Tool.csproj" ReferenceOutputAssembly="false" UndefineProperties="_IsPublishing" />
</ItemGroup>
```

- The tool publishes its own build through its `DependsOnTargets="Publish"` target, the consumer orders the build
- `dotnet publish` on the consumer passes `_IsPublishing=true` into the referenced build, `UndefineProperties="_IsPublishing"` keeps the tool's condition false
- The consumer derives the tool publish directory from `$(Configuration)` and the tool's `PublishDir` convention
- An extra global property is safe when the effective `OutputPath` and `IntermediateOutputPath` contain its value, `Platform` is no pivot under the artifacts layout
- A build that needs a property outside the path takes its own `BaseIntermediateOutputPath` and output path

## [02]-[SETTARGETFRAMEWORK_FORMS]

The multi-targeting form and the other-framework form hold when the effective output paths contain the framework. Incompatible frameworks, `.NETFramework` against `.NETCoreApp`, take `SkipGetTargetFrameworkProperties="true"` for the framework negotiation that fails across the reference and `ReferenceOutputAssembly="false"` for the assembly the consumer cannot load:

```xml
<!-- OK -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" ReferenceOutputAssembly="false" />
```

`SkipGetTargetFrameworkProperties="true"` skips the step that removes the inherited `TargetFramework`. Every inner build of a multi-targeting consumer then passes its `TargetFramework` into the referenced project, which fails `NETSDK1005`. One guard applies:
- `SetTargetFramework="TargetFramework=<tfm>"` pins the referenced build, the form a multi-targeting reference takes
- `UndefineProperties="TargetFramework"` removes the inherited global property, a single-targeting project builds as declared

```xml
<!-- OK -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" UndefineProperties="TargetFramework" ReferenceOutputAssembly="false" />
```

`UndefineProperties` removes the property `SetTargetFramework` sets, a reference with both loses its pin silently.

## [03]-[HOST_SUPPLIED_REFERENCE]

The path comes from a property with a default, the `Reference` items derive from one item list, and a target before `ResolveAssemblyReferences` turns a missing installation into one error.

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <HostAppPath Condition="'$(HostAppPath)' == '' and '$(HOST_APP_PATH)' != ''">$(HOST_APP_PATH)</HostAppPath>
  <HostAppPath Condition="'$(HostAppPath)' == ''">/Applications/Host.app</HostAppPath>
  <_HostAppDir>$([MSBuild]::NormalizeDirectory('$(HostAppPath)'))</_HostAppDir>
  <HostAssemblyDir>$(_HostAppDir)Contents/Resources/</HostAssemblyDir>
</PropertyGroup>

<!-- Directory.Build.targets -->
<ItemGroup Condition="'$(HostRole)' != ''">
  <_HostAssembly Include="HostCore" />
  <_HostAssembly Include="HostUi" Condition="'$(HostRole)' == 'ui'" />
  <Reference Include="@(_HostAssembly)" HintPath="$(HostAssemblyDir)%(Identity).dll" Private="false" />
</ItemGroup>

<Target Name="VerifyHostInstallation" BeforeTargets="ResolveAssemblyReferences" Condition="'$(HostRole)' != ''">
  <ItemGroup>
    <_MissingHostFile Include="@(Reference->'%(HintPath)')" Condition="'%(Reference.HintPath)' != '' and !Exists('%(Reference.HintPath)')" />
  </ItemGroup>
  <Error Condition="'@(_MissingHostFile)' != ''" Text="Host installation '$(HostAppPath)' lacks @(_MissingHostFile->'%(Filename)%(Extension)', ', '), set HOST_APP_PATH" />
</Target>
```

`HostRole` is a project-body property, the items derive in `Directory.Build.targets`. `_HostAppDir` is derived and never assigned back, a `-p:HostAppPath` global property cannot be rewritten.

## [04]-[LAYER_VALIDATION_TARGET]

The role derives from the project directory in `Directory.Build.props`.

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <LibrariesRoot>$([MSBuild]::NormalizeDirectory('$(MSBuildThisFileDirectory)', 'libs'))</LibrariesRoot>
  <IsLibraryProject Condition="$(MSBuildProjectDirectory.StartsWith('$(LibrariesRoot)'))">true</IsLibraryProject>
</PropertyGroup>

<!-- Directory.Build.targets -->
<Target Name="ValidateReferenceLayer" BeforeTargets="PrepareForBuild" Condition="'$(IsLibraryProject)' == 'true'">
  <ItemGroup>
    <_UpwardReference Include="@(ProjectReference->'%(FullPath)')" Condition="!$([System.String]::Copy('%(FullPath)').StartsWith('$(LibrariesRoot)'))" />
  </ItemGroup>
  <Error Condition="'@(_UpwardReference)' != ''" Text="Library '$(MSBuildProjectName)' references outside libs/: @(_UpwardReference, ', ')" />
</Target>
```

A `ProjectReference` with `ReferenceOutputAssembly="false"` orders the build alone, `Condition="'%(ProjectReference.ReferenceOutputAssembly)' != 'false'"` on the `_UpwardReference` item exempts an analyzer project reached with `OutputItemType="Analyzer"`.

## [05]-[BACKSLASH_CASES]

ERROR, no conversion or a conversion the consumer must not get:
- `Exec` commands starting with a program name (`cat`, `git`, `dotnet`)
- Backslashes outside a path, `<Exec Command="echo a\b\c" />` prints `abc`
- Backslashes the consumer keeps, `Lines` on `<WriteLinesToFile>` is an item list, every item converts, the file receives `a/b` where it needs `a\b`
- Paths a custom task passes to file APIs without the MSBuild path utilities

STYLE, `$(MSBuildThisFileDirectory)` ends with the separator of the current operating system, `$(MSBuildThisFileDirectory)tools/mytool` works on every one.
