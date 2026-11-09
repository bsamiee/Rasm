# [RASM_ANALYZERS]

`Rasm.Analyzers` is a Roslyn analyzer package holding C# to functional form over LanguageExt, Thinktecture, RhinoCommon, Eto, and Microsoft.Extensions.Logging, with code fixes for the rules a rewrite settles. Every rule reports at error and turns itself off where its library is absent.

## [01]-[USE]

```xml
<ItemGroup>
    <PackageReference Include="Rasm.Analyzers" Version="<version>" PrivateAssets="all" />
</ItemGroup>
```

- Development dependency: analyzers and `build/Rasm.Analyzers.props` reach the referencing project alone, never its consumers
- Analyzers load into C# compilers from Roslyn 5.9, the version the package compiles against
- `dotnet_diagnostic.<id>.severity` rows in `.editorconfig` or `.globalconfig` change one rule, `dotnet_analyzer_diagnostic.category-<category>.severity` rows change a category
- `build/Rasm.Analyzers.props` passes the project's referenced project assemblies to the compiler as `ProjectReferenceAssemblies`, which RASM3006 and RASM3007 read
- Rules naming Rasm.Rhino members run only where Rasm.Rhino is referenced: RASM2003, 2006, 2009, 2012, 2102–2105, 2203, 2204

## [02]-[RULES]

Each category holds one block of IDs.

### [02.1]-[LANGUAGEEXT]

Category `LanguageExt`:

| [INDEX] | [ID]                          | [TITLE]                                                                       |
| :-----: | :---------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | <a id="RASM1001"></a>RASM1001 | Explicit result lift, write the value                                         |
|  [02]   | <a id="RASM1003"></a>RASM1003 | Type test on `Error`, call `IsType` or `Filter`                               |
|  [03]   | <a id="RASM1004"></a>RASM1004 | `Fin` round trip through `Validation`, delete both conversions                |
|  [04]   | <a id="RASM1005"></a>RASM1005 | `Seq` accumulator copied per step, call `Add`                                 |
|  [05]   | <a id="RASM1007"></a>RASM1007 | Redundant `As`, delete the call                                               |
|  [06]   | <a id="RASM1008"></a>RASM1008 | `Match` passing one case through, call the one-handler member                 |
|  [07]   | <a id="RASM1009"></a>RASM1009 | `Option` converted to nullable by hand, call `ValueUnsafe` or `ToNullable`    |
|  [08]   | <a id="RASM1010"></a>RASM1010 | Record property lens written by hand, write `with` at the edit                |
|  [09]   | <a id="RASM1011"></a>RASM1011 | `IO.lift` thunk or `IO.pure` value returned from a continuation               |
|  [10]   | <a id="RASM1012"></a>RASM1012 | `Map` then `Flatten`, call `Bind`                                             |
|  [11]   | <a id="RASM1013"></a>RASM1013 | `Catch` predicate bypassed by an evaluated failure, apply it through `@catch` |

### [02.2]-[THINKTECTURE]

Category `Thinktecture`:

| [INDEX] | [ID]                          | [TITLE]                                                              |
| :-----: | :---------------------------- | :------------------------------------------------------------------- |
|  [01]   | <a id="RASM1101"></a>RASM1101 | Catch-all arm over smart enum or union, call `Switch` or `Map`       |
|  [02]   | <a id="RASM1102"></a>RASM1102 | Validation through default `ValidationError`                         |
|  [03]   | <a id="RASM1103"></a>RASM1103 | `ValidationError` converted to `Error` by hand                       |
|  [04]   | <a id="RASM1104"></a>RASM1104 | Throwing factory on caller data, return `Fin` through `Validate`     |
|  [05]   | <a id="RASM1105"></a>RASM1105 | Smart enum with no data, move behavior onto items or declare a union |
|  [06]   | <a id="RASM1106"></a>RASM1106 | Union case per host member, call the member at its caller            |

### [02.3]-[LOGGING]

Category `Microsoft.Extensions.Logging`:

| [INDEX] | [ID]                          | [TITLE]                                                 |
| :-----: | :---------------------------- | :------------------------------------------------------ |
|  [01]   | <a id="RASM1202"></a>RASM1202 | Logger called by hand, declare a generated log member   |
|  [02]   | <a id="RASM1203"></a>RASM1203 | Failure reported twice, report once at host translation |

### [02.4]-[RHINOCOMMON]

Category `RhinoCommon`:

| [INDEX] | [ID]                          | [TITLE]                                                              |
| :-----: | :---------------------------- | :------------------------------------------------------------------- |
|  [01]   | <a id="RASM2002"></a>RASM2002 | Unchecked item count, compare with input count                       |
|  [02]   | <a id="RASM2003"></a>RASM2003 | `RhinoDoc` read off UI thread, read through `OnUiThread()`           |
|  [03]   | <a id="RASM2004"></a>RASM2004 | `ActiveDoc` read with document in scope, read the held document      |
|  [04]   | <a id="RASM2005"></a>RASM2005 | `RunScript` in command without `ScriptRunner` style                  |
|  [05]   | <a id="RASM2006"></a>RASM2006 | Single call marshaled to UI thread, compose one `OnUiThread()` `IO`  |
|  [06]   | <a id="RASM2007"></a>RASM2007 | `SetDisplayModeOverride` result read, discard through `_ =`          |
|  [07]   | <a id="RASM2008"></a>RASM2008 | macOS-breaking member outside macOS guard, guard or replace the call |
|  [08]   | <a id="RASM2009"></a>RASM2009 | `GrayscaleDib` result read without null check                        |
|  [09]   | <a id="RASM2010"></a>RASM2010 | Page view built in `PageControl`, realize in `OnActivate(true)`      |
|  [10]   | <a id="RASM2011"></a>RASM2011 | Floating view docked by assignment, delete the assignment            |
|  [11]   | <a id="RASM2012"></a>RASM2012 | Owned `DisplayModeDescription` left undisposed                       |
|  [12]   | <a id="RASM2101"></a>RASM2101 | `PersistentSettings` read through a default, call `TryGet<Kind>`     |
|  [13]   | <a id="RASM2102"></a>RASM2102 | View redraw outside `Commits`, pass a `RedrawPolicy`                 |
|  [14]   | <a id="RASM2103"></a>RASM2103 | Cast or type test on `GeometryBase` duplicate                        |
|  [15]   | <a id="RASM2104"></a>RASM2104 | Conduit toggled outside `Conduits`, toggle through `Conduits`        |
|  [16]   | <a id="RASM2105"></a>RASM2105 | `RenderSettings` written outside `RenderSettingsSources`             |

### [02.5]-[ETO]

Category `Eto`:

| [INDEX] | [ID]                          | [TITLE]                                                                   |
| :-----: | :---------------------------- | :------------------------------------------------------------------------ |
|  [01]   | <a id="RASM2201"></a>RASM2201 | Cached system font disposed, read without release                         |
|  [02]   | <a id="RASM2202"></a>RASM2202 | Eto container acquired by plain dispose, detach children before `Dispose` |
|  [03]   | <a id="RASM2203"></a>RASM2203 | Retained control transferred, pass it without `Transfer`                  |
|  [04]   | <a id="RASM2204"></a>RASM2204 | Native callback or Eto override reading host outside `Session`            |
|  [05]   | <a id="RASM2205"></a>RASM2205 | Delegate registered as platform handler, declare a handler interface      |

### [02.6]-[CSHARP]

Category `CSharp`:

| [INDEX] | [ID]                          | [TITLE]                                                                     |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | <a id="RASM3001"></a>RASM3001 | Private member with one read, inline at the read                            |
|  [02]   | <a id="RASM3002"></a>RASM3002 | Catch-all arm over covered enum, delete the arm                             |
|  [03]   | <a id="RASM3003"></a>RASM3003 | Discarded awaitable, await or return the value                              |
|  [04]   | <a id="RASM3004"></a>RASM3004 | Catch-all arm over enum, name each member                                   |
|  [05]   | <a id="RASM3005"></a>RASM3005 | Namespace owned by a referenced assembly, declare under project namespace   |
|  [06]   | <a id="RASM3006"></a>RASM3006 | Type name clashes with an imported type, rename the declaration             |
|  [07]   | <a id="RASM3007"></a>RASM3007 | Type test on a type parameter, declare a static virtual member              |
|  [08]   | <a id="RASM3008"></a>RASM3008 | File named after its folder, rename for its concept family                  |
|  [09]   | <a id="RASM3009"></a>RASM3009 | Qualified name around a project declaration, rename the declaration         |
|  [10]   | <a id="RASM3010"></a>RASM3010 | Qualified name with no clash, import and write the simple name              |
|  [11]   | <a id="RASM3011"></a>RASM3011 | Static instance restated at a construction site, read the instance          |
|  [12]   | <a id="RASM3012"></a>RASM3012 | Forwarding property, read member at its uses                                |
|  [13]   | <a id="RASM3013"></a>RASM3013 | Overloads take a value and its factory, keep one overload                   |
|  [14]   | <a id="RASM3101"></a>RASM3101 | Type name stored as text, store the type                                    |
|  [15]   | <a id="RASM3102"></a>RASM3102 | Sentinel for an absent member, state absence in the type                    |
|  [16]   | <a id="RASM3104"></a>RASM3104 | Callback value passed out through a captured write, return it in the result |

Retired and never reassigned: RASM1002, RASM1006, RASM1201, RASM2001, RASM2013 (now RASM1106), RASM3103 (now RASM1013), RASM4001, RASM4002.
