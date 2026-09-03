# [WORKED_EXAMPLES]

Full files for the catalog entries with a fix that spans more than one element.

## [01]-[DUPLICATE_PUBLISH_INSTANCE]

The `DependsOnTargets="Publish"` target reads `_IsPublishing`, which nothing sets in a plain build, and `-check` reports `BC0201` for that read. Set `build_check.BC0201.severity = none` under a `[*.csproj]` section in `.editorconfig`. BuildCheck ignores a key outside a section, and `AllowUninitializedPropertiesInConditions` does not cover the read.

The consumer form calls `Publish` on a tool project from another project:

```xml
<!-- BAD: the consumer creates a publish instance of the tool that races the tool's own build in the graph -->
<Target Name="PublishTool" BeforeTargets="Build">
  <MSBuild Projects="../Tool/Tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- GOOD: the tool publishes its own build through its DependsOnTargets="Publish" target, the consumer orders the build -->
<ItemGroup>
  <ProjectReference Include="../Tool/Tool.csproj" ReferenceOutputAssembly="false" UndefineProperties="_IsPublishing" />
</ItemGroup>
```

- `UndefineProperties="_IsPublishing"` is required because `dotnet publish` on the consumer passes `_IsPublishing=true` into the referenced build, where it satisfies the tool's condition and the tool publishes nothing
- The consumer derives the tool publish directory from `$(Configuration)` and the tool's `PublishDir` convention
- Extra global properties are safe only when the effective `OutputPath` and `IntermediateOutputPath` contain their values, `Platform` is never a pivot under the artifacts layout, and a build that needs a property outside the path gets its own `BaseIntermediateOutputPath` and output path

## [02]-[SETTARGETFRAMEWORK_FORMS]

The multi-targeting form and the other-framework form are correct only when the effective output paths contain the framework. Incompatible frameworks, for example `.NETFramework` and `.NETCoreApp`, need `SkipGetTargetFrameworkProperties="true"` because the framework negotiation fails for them, and `ReferenceOutputAssembly="false"` because the consumer cannot load the assembly:

```xml
<!-- OK: a .NETFramework project builds a .NETCoreApp tool without a reference to its assembly -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" ReferenceOutputAssembly="false" />
```

`SkipGetTargetFrameworkProperties="true"` skips the step that removes the inherited `TargetFramework`. Every multi-targeting inner build of the consumer then passes its `TargetFramework` into the referenced project, which builds once per consumer framework and fails with `NETSDK1005` naming an assets file with no target for that framework. Guard with one of:
- `SetTargetFramework="TargetFramework=<tfm>"` pins the referenced build, which a multi-targeting reference requires
- `UndefineProperties="TargetFramework"` removes the inherited global property, and a single-targeting project then builds as declared

```xml
<!-- OK: the multi-targeting consumer's TargetFramework never reaches the single-targeting tool -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" UndefineProperties="TargetFramework" ReferenceOutputAssembly="false" />
```

`UndefineProperties` removes the property that `SetTargetFramework` sets, and a reference with both loses its pin with no message.

## [03]-[HOST_SUPPLIED_REFERENCE]

The path comes from a property with a default, the `Reference` items derive from one item list, and a target before `ResolveAssemblyReferences` turns a missing installation into one error in place of a warning per missing assembly.

```xml
<!-- Directory.Build.props: the host location, a global property or an environment variable overrides the default -->
<PropertyGroup>
  <HostAppPath Condition="'$(HostAppPath)' == '' and '$(HOST_APP_PATH)' != ''">$(HOST_APP_PATH)</HostAppPath>
  <HostAppPath Condition="'$(HostAppPath)' == ''">/Applications/Host.app</HostAppPath>
  <!-- Derived and never assigned back, a -p:HostAppPath global property cannot be rewritten -->
  <_HostAppDir>$([MSBuild]::NormalizeDirectory('$(HostAppPath)'))</_HostAppDir>
  <HostAssemblyDir>$(_HostAppDir)Contents/Resources/</HostAssemblyDir>
</PropertyGroup>

<!-- Directory.Build.targets: HostRole is a project-body property, and the items derive here -->
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

## [04]-[LAYER_VALIDATION_TARGET]

The role derives from the project directory in `Directory.Build.props`, where `$(MSBuildProjectDirectory)` is already set.

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

`ProjectReference` items with `ReferenceOutputAssembly="false"` only order the build, and an analyzer project reached that way with `OutputItemType="Analyzer"` stays exempt through `Condition="'%(ProjectReference.ReferenceOutputAssembly)' != 'false'"` on the `_UpwardReference` item.

## [05]-[BACKSLASH_CASES]

ERROR, no conversion or a conversion the consumer must not get:
- `Exec` commands that start with a program name (`cat`, `git`, `dotnet`)
- Backslashes outside a path, `<Exec Command="echo a\b\c" />` prints `abc`
- Backslashes the consumer must keep, `Lines` on `<WriteLinesToFile>` is an item list, every item converts, and the file receives `a/b` where it needs `a\b`
- Paths that a custom task passes to file APIs without the MSBuild path utilities

STYLE, `$(MSBuildThisFileDirectory)` ends with the separator of the current operating system, and `$(MSBuildThisFileDirectory)tools/mytool` works on every one.
