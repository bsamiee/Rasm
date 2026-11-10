# [RASM_ANALYZERS]

`Rasm.Analyzers` is a Roslyn analyzer package holding C# to functional form over LanguageExt, Thinktecture, and Microsoft.Extensions.Logging, with code fixes for the rules a rewrite settles. Every rule reports at error and turns itself off where its library is absent. Rules over RhinoCommon, Eto, and Rasm.Rhino ship in the `Rasm.Rhino` package.

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
- Each rule is its own analyzer, so a `none` row skips its analysis
- Analyzer options sit in a global config: `RASM1203.reporters` and `RASM3003.exempt` list member documentation IDs, `RASM3101.exempt` and `RASM3110.types` list type documentation IDs, `RASM3102.sentinels` lists `<assembly>:<member prefix>` rows, each value joined by `|`
- RASM1206 and RASM3107 stay silent in executables, `.editorconfig` path rows turn them off for other composition roots

## [02]-[RULES]

Each category holds one block of IDs.

### [02.1]-[LANGUAGEEXT]

Category `LanguageExt`:

| [INDEX] | [ID]                          | [TITLE]                                                                                                |
| :-----: | :---------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | <a id="RASM1001"></a>RASM1001 | Explicit result lift, write the value                                                                  |
|  [02]   | <a id="RASM1003"></a>RASM1003 | Type test on `Error`, call `IsType` or `Filter`                                                        |
|  [03]   | <a id="RASM1004"></a>RASM1004 | `Fin` round trip through `Validation`, delete both conversions                                         |
|  [04]   | <a id="RASM1005"></a>RASM1005 | `Seq` accumulator copied per step, call `Add`                                                          |
|  [05]   | <a id="RASM1007"></a>RASM1007 | Redundant `As`, delete the call                                                                        |
|  [06]   | <a id="RASM1008"></a>RASM1008 | `Match` passing one case through, call the one-handler member                                          |
|  [07]   | <a id="RASM1009"></a>RASM1009 | `Option` to nullable outside its conversion member, call `ValueUnsafe` or `ToNullable` by element kind |
|  [08]   | <a id="RASM1010"></a>RASM1010 | Record property lens written by hand, write `with` at the edit                                         |
|  [09]   | <a id="RASM1011"></a>RASM1011 | `IO.lift` thunk or `IO.pure` value returned from a continuation                                        |
|  [10]   | <a id="RASM1012"></a>RASM1012 | `Map` then `Flatten`, call `Bind`                                                                      |
|  [11]   | <a id="RASM1013"></a>RASM1013 | `Catch` predicate bypassed by an evaluated failure, apply it through `@catch`                          |
|  [12]   | <a id="RASM1014"></a>RASM1014 | Exclusion paired by hand, run it as one conduit                                                        |

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
|  [03]   | <a id="RASM1204"></a>RASM1204 | Static output sink, log through an `ILogger`            |
|  [04]   | <a id="RASM1205"></a>RASM1205 | String logger category, derive the category from a type |
|  [05]   | <a id="RASM1206"></a>RASM1206 | Console output outside a composition root               |

### [02.4]-[CSHARP]

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
|  [08]   | <a id="RASM3009"></a>RASM3009 | Qualified name around a project declaration, rename the declaration         |
|  [09]   | <a id="RASM3010"></a>RASM3010 | Qualified name with no clash, import and write the simple name              |
|  [10]   | <a id="RASM3011"></a>RASM3011 | Static instance restated at a construction site, read the instance          |
|  [11]   | <a id="RASM3012"></a>RASM3012 | Forwarding property, read member at its uses                                |
|  [12]   | <a id="RASM3013"></a>RASM3013 | Overloads take a value and its factory, keep one overload                   |
|  [13]   | <a id="RASM3014"></a>RASM3014 | Using alias renames a type or namespace, write its name at each use         |
|  [14]   | <a id="RASM3015"></a>RASM3015 | Member with one reader in the assembly, fold it into the reader             |
|  [15]   | <a id="RASM3101"></a>RASM3101 | Type name stored as text, store the type                                    |
|  [16]   | <a id="RASM3102"></a>RASM3102 | Sentinel stands for absence, state absence in the type                      |
|  [17]   | <a id="RASM3104"></a>RASM3104 | Callback value passed out through a captured write, return it in the result |
|  [18]   | <a id="RASM3105"></a>RASM3105 | Null-forgiving operator on a nullable result, lift the absence              |
|  [19]   | <a id="RASM3106"></a>RASM3106 | Ambient clock read, take a `TimeProvider`                                   |
|  [20]   | <a id="RASM3107"></a>RASM3107 | `TimeProvider.System` outside a composition root, take a `TimeProvider`     |
|  [21]   | <a id="RASM3108"></a>RASM3108 | Member bound at run time, name the member in code                           |
|  [22]   | <a id="RASM3109"></a>RASM3109 | No-op placeholder value, declare the optional member with its default       |
|  [23]   | <a id="RASM3110"></a>RASM3110 | Throwing member with a `Try` sibling, call the `Try` form                   |
|  [24]   | <a id="RASM3111"></a>RASM3111 | Platform-dependent name set, declare the portable reserved set              |
|  [25]   | <a id="RASM3112"></a>RASM3112 | `Vector2` or `Vector3` lane comparison, compare through the lane masks      |
|  [26]   | <a id="RASM3113"></a>RASM3113 | Package member with a known defect, call the working form                   |

Retired and never reassigned: RASM1002, RASM1006, RASM1201, RASM2001, RASM2013 (now RASM1106), RASM2015 (now RASM3113), RASM3008 (now the `file-names` hk step), RASM3103 (now RASM1013), RASM4001, RASM4002. RASM2xxx rules ship in the `Rasm.Rhino` package.
