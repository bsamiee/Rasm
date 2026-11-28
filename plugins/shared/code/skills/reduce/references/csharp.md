# [CSHARP]

C# targets hold `.cs` files of the projects `Workspace.slnx` lists, `<project>` naming the folder holding a `.csproj`, `<name>` its project name as `nx show projects` prints it, `<paths>` files a pass touches, `<dir>` session scratch folder, and `<roots>` `libs products tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET]       | [SKILLS]                                      | [CONDITION]                                         |
| :-----: | :------------- | :-------------------------------------------- | :-------------------------------------------------- |
|  [01]   | Any C#         | `dotnet:coding`, `dotnet:codelens`            | `.cs` file of a `Workspace.slnx` project            |
|  [02]   | LanguageExt    | `dotnet:languageext`                          | `LanguageExt.Core` in `packages.lock.json`          |
|  [03]   | Thinktecture   | `dotnet:thinktecture`                         | `[SmartEnum]`, `[ValueObject]`, `[Union]`, `TTRESG` |
|  [04]   | Mapperly       | `dotnet:mapperly`                             | `[Mapper]`, or a mapping a pass writes              |
|  [05]   | Spans, kernels | `dotnet:memory`                               | `Span<T>`, pooled owners, `TensorPrimitives`        |
|  [06]   | Drawn UI       | `dotnet:ui`                                   | `libs/dotnet/ui`, imports `SkiaSharp` or `Eto`      |
|  [07]   | Tests          | `code:test`, `rhino:testing` for Rhino suites | `*.Tests.csproj`                                    |
|  [08]   | MSBuild        | `msbuild:antipatterns`, `msbuild:evaluation`  | `.csproj`, `.props`, `.targets`                     |
|  [09]   | Packages       | `msbuild:packaging`                           | `Directory.Packages.props`, `.slnx`                 |

Rhino and Grasshopper code takes rows [01] to [06] alone, as its host SDK is a package read under [03]-[SOURCES]. `Directory.Build.targets` adds global usings per `PackageReference` (`LanguageExt`, `Thinktecture`), so a file's `using` lines never decide a skill row.

## [02]-[SURVEY]

Signals over `<paths>`, one command per step:
1. Size: lines and branching per file, largest first
2. Clones: jscpd C# pairs across `<roots>` touching the target, ranked by lines times similarity
3. Dependents: projects depending on the target's Nx project
4. Exports: declarations per file at each level, a member public only where its type is exported, `implied` holding enum members and positional parameters
5. Private readers: private and `file` members whose name appears twice in their file, leads past what `RASM3001` and `RASM3015` refuse
6. Name pairs: type and member names differing by case, each group judged, as `Color Color` names a property by its type by design
7. Nesting: sites past the limit from the rule hk runs, then members ranked by codelens `maxNesting` and `cognitive` across `<name>`

```bash
jscpd --config .config/jscpd.json --complexity --reporters json --output <dir>/jscpd <paths> &&
    jq -r --arg r "$PWD/" '.summary.files[] | [.lines, .complexity, (.path | ltrimstr($r))] | @tsv' <dir>/jscpd/jscpd-complexity.json | sort -rn
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format csharp --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
nx graph --file=<dir>/graph.json &&
    jq -r --arg p '<name>' '.graph.dependencies | to_entries[] | select(any(.value[]; .target == $p)) | .key' <dir>/graph.json
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | [.items[] | select(.symbolType != "namespace") | . as $t
    | ([$t] + [($t.members // [])[] | select(.astKind != "local_function_statement")])[]
    | ((.signature // "") | sub("^(\\[[^]]*\\]\\s*)+"; "") | sub("^extension(<[^>]*>)?\\([^)]*\\)\\s*"; "") | capture("^(?<v>public|internal|protected|private|file)\\b").v // "implied") as $v
    | if $t.isExported and (. == $t or .isPublic) then "public" elif $v == "private" or $v == "file" then "private" elif $v == "implied" then "implied" else "internal" end]
    | group_by(.) | map("\(.[0])=\(length)") | "\($p)\t\(join(" "))"'
comm -12 <(ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $f | .items[] | (.members // [])[]
    | select((.signature // "") | test("^(\\[[^]]*\\]\\s*)*(private|file)\\b")) | "\($f)|\(.name)"' | sort -u) \
    <(rg -How --no-heading '[A-Za-z_]\w*' <paths> | sed 's/:/|/' | sort | uniq -c | awk '$1 == 2 { print $2 }' | sort -u)
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.items[] | select(.symbolType != "namespace") | .name, ((.members // [])[] | select(.astKind != "parameter") | .name)' | sort -u |
    awk '{ k = tolower($0); gsub(/[-_]/, "", k); g[k] = g[k] " " $0; n[k]++ } END { for (k in g) if (n[k] > 1) print g[k] }'
ast-grep scan --filter no-fourth-nesting-level-csharp --report-style short <paths>
```

```text
get_complexity_metrics {"project": "<name>", "metric": "cognitive", "threshold": 0, "limit": 500}
```

Signals read by these rules:
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Private readers: both rules skip block bodies, attributes, type parameters, `ref`, `params`, or default parameters, and inlines dropping a name word
- Nesting levels: `if` (an `else if` chain one level), loops, `switch` statements and expressions, and conditionals, a one-arm `x switch { var y => ... }` binding and conditions counting none
- Closures: lambda, anonymous method, and local function bodies of 2 or more statements, a branch, or a loop count a level, others none
- Limit: the rule refuses a fourth level, and hk's `ast-grep` step runs it under `hk check` and `hk fix`
- Ranking: codelens rows whose `file` sits in `<paths>` order whole reads, `maxNesting` counting a chained conditional as nested where the rule counts one level
- Generated rows: `isGenerated` marks a generator's output, which moves with its attribute and is never a finding

## [03]-[SOURCES]

Package reads, one command per step:
1. Version: package version the project's lock resolves, as `Directory.Packages.props` pins and `packages.lock.json` records what compiles
2. Guide: `README.md` or `README.nuget.md` in the package folder, as no core package publishes `AGENTS.md` or `llms.txt`
3. Index: codelens namespace tree of an assembly the solution references, one namespace's members per call
4. Inventory: XML documentation ids of an assembly into the scratch folder, counted per kind, `ilspycmd` types where no XML file ships (`Rhino.UI`)
5. Generated API: generators per project and their output, the source of `Get`, `TryGet`, `Switch`, `Map`, `Validate`, comparers, and `[LoggerMessage]` bodies a hand-written member restates
6. Host SDK: RhinoCommon, Eto, Grasshopper2, and GrasshopperIO XML ids at `$(RhinoVersion)` through step 4, a runtime behavior of the installed Rhino read through `tool:inspect`
7. Catalog: each `Directory.Packages.props` candidate no project references, downloaded at its pinned version, then steps 4 and 2 over its folder
8. Member: declaration and doc comment of one type through `search:code`'s `ilspycmd -t` and XML id commands

```bash
jq -r --arg id '<Id>' '[.dependencies[][$id]?.resolved] | unique[]' <project>/packages.lock.json
fd -d 1 -i '^(agents\.md|llms.*\.txt|readme(\.nuget)?\.md|docs)$' .cache/nuget/packages/<id>/<version>
rg -o 'name="([TMPFE]):([^"(]+)' -r '$1 $2' .cache/nuget/packages/<id>/<version>/lib/<tfm>/<Assembly>.xml > <dir>/<id>.members.txt && cut -d' ' -f1 <dir>/<id>.members.txt | sort | uniq -c
ilspycmd -l cisde .cache/nuget/packages/<id>/<version>/lib/<tfm>/<Assembly>.dll
fd -e xml . .cache/nuget/packages/<id>/<version>
yq -p xml -oy '[.Project.ItemGroup[].PackageVersion[]? | select(."+@Include" == "<Id>") | ."+@Version"][0]' Directory.Packages.props
dotnet package download <Id>@<version> --prerelease --output <dir>/pkg
```

```text
inspect_external_assembly {"assemblyName": "<Assembly>"}
inspect_external_assembly {"assemblyName": "<Assembly>", "mode": "namespace", "namespaceFilter": "<Namespace>"}
get_source_generators {"project": "<name>"}
get_generated_code {"file": "<Type>.<Facet>.g.cs"}
```

Reads hold under these conditions:
- Store: `NuGet.config` sets `.cache/nuget/packages`, folder names lowercase, `<tfm>` the key of the lock's `dependencies`, host SDKs under `lib/` or `ref/`
- Generated: generator output exists in the compilation alone, so `get_generated_code` reads it and no file under `.artifacts/dotnet/obj` does
- Unreferenced: codelens inspects referenced assemblies alone, so a catalog candidate reads from its download
- Prerelease: `dotnet package download` refuses a prerelease pin without `--prerelease`
- Compile and host: projects compile against the package version, and the installed Rhino can run a newer build

## [04]-[CONSUMERS]

Readers and leads across `<roots>`, one command per step:
1. Readers: codelens references by symbol, the full name from `search_symbols`, `kinds` naming the reads a shape decision needs (`write`, `invocation`, `object_creation`)
2. Callers and implementors: method call sites, interface implementors, generator attribute holders, and reflection reads
3. Text: sites spelling a member, key, setting name, or message the declaration holds
4. Mentions: removed or renamed name across the repository with no type filter, docs, skills, and manifests included
5. Leads: members nothing references, a host-loaded type (`PlugIn`, GH2 `Component`, generated `PlugInEntry`) read as its host's reader first

```text
search_symbols {"query": "<Name>"}
find_references {"symbol": "<Namespace.Type.Member>", "kinds": ["<kind>"]}
find_callers {"symbol": "<Namespace.Type.Method>"}
find_implementations {"symbol": "<Namespace.Interface>"}
find_attribute_usages {"attribute": "<Attribute>"}
find_reflection_usage {"symbol": "<Type>"}
find_unused_symbols {"project": "<name>", "includeInternal": true}
```

```bash
rg -nF '<member or literal>' -g '*.cs' <roots>
rg -nw '<name>' .
```

Readers resolve under these conditions:
- Names: `find_references` takes a fully qualified member, and a bare name resolves only where it is unique in the solution
- Generated: sites with `isGenerated` are a generator's reads of its own attribute, apart from the code a pass rebuilds
- Leads: `find_unused_symbols` rows enter the map's reader column alone, as a public library member kept for its capability stays

## [05]-[FORMS]

C# forms of each category, each rebuild holding under its stated condition alone, each source a rule the build or hk reports, a skill rule, or the commit of a past pass. The largest cuts deleted a library layer the host SDK holds (`001592869`, `371df8c84`) and a family of records differing by one type (`e9a82a6e7`), so host-native layers and per-package records rank first:

| [INDEX] | [CATEGORY]   | [FOUND]                                  | [REBUILD]                                | [SOURCE]                      |
| :-----: | :----------- | :--------------------------------------- | :--------------------------------------- | :---------------------------- |
|  [01]   | Redundant    | Null guard on a non-nullable parameter   | Removed, its type stating the contract   | `CA1062` off                  |
|  [02]   | Redundant    | `_ =` discard a style rule forced        | Expression statement, rule gone          | `2fd5788fb`                   |
|  [03]   | Redundant    | Record restating a host property bag     | Host copy as snapshot, sets in one `IO`  | `5e69ca609`                   |
|  [04]   | Redundant    | Hand `Equals`, `GetHashCode`, `ToString` | `record`, `[ValueObject]` members        | `dotnet:thinktecture`         |
|  [05]   | Redundant    | `Map` then `Flatten`, `Validation` trip  | `Bind`, direct form                      | `RASM1012`, `RASM1004`        |
|  [06]   | Duplicated   | Layer over host events, store, or undo   | Host members at their use                | `001592869`                   |
|  [07]   | Duplicated   | Error record per package and reason      | Record generic over its reason           | `e9a82a6e7`                   |
|  [08]   | Duplicated   | `Items` scanned for a key, hand `Parse`  | Generated `Get`, `TryGet`, factory       | `dotnet:thinktecture`         |
|  [09]   | Duplicated   | `ILogger.Log*` call with hand state      | `[LoggerMessage]` partial                | `RASM1202`                    |
|  [10]   | Duplicated   | Loop summing, converging, or indexing    | MathNet, `TensorPrimitives`, `Span2D<T>` | `dotnet:memory`               |
|  [11]   | Duplicated   | Members differing by numeric type        | One member under `INumber<T>`            | `dotnet:coding`               |
|  [12]   | Duplicated   | Static helper class over a foreign type  | `extension(<receiver>)` block            | `dotnet:coding`               |
|  [13]   | Duplicated   | Union cases each calling one host member | `Func<T, IO<Unit>>` folded in order      | `e2355817b`                   |
|  [14]   | Duplicated   | Sibling host-table adapters of one shape | One member over the table contract       | `jscpd` pair                  |
|  [15]   | Types        | `is` tests, flags for exclusive modes    | `[Union]` record with `sealed` cases     | `dotnet:thinktecture`         |
|  [16]   | Types        | Cases differing by a tag alone           | Record keyed by a smart enum item        | `dotnet:coding`               |
|  [17]   | Types        | Data-free smart enum, preset union       | Items holding data, preset type          | `RASM1105`, `RASM1106`        |
|  [18]   | Types        | Value object holding no domain rule      | Key type or plain record                 | `dotnet:thinktecture`         |
|  [19]   | Types        | Delegate equal to `Func` or `Action`     | `Func` or `Action`                       | `no-func-equivalent-delegate` |
|  [20]   | Types        | Interface with one implementation        | Concrete type or `Func` value            | `dotnet:coding`               |
|  [21]   | Types        | `struct` outside native call or kernel   | `record` or `sealed` class               | `dotnet:coding`               |
|  [22]   | Types        | `partial` on a type no generator writes  | `partial` dropped                        | `get_source_generators`       |
|  [23]   | Types        | `switch` over one enum in many members   | Smart enum items holding each value      | `dotnet:thinktecture`         |
|  [24]   | Literals     | `switch` over text keys of a closed set  | `[SmartEnum<string>]` item per key       | `dotnet:thinktecture`         |
|  [25]   | Literals     | Key or setting name spelled as text      | `nameof(<Member>)`                       | `dotnet:coding`               |
|  [26]   | Literals     | Logger category spelled as text          | `CreateLogger<T>()`                      | `RASM1205`                    |
|  [27]   | Literals     | Type name as text, sentinel for absence  | `Type` value, `Option`                   | `RASM3101`, `RASM3102`        |
|  [28]   | Literals     | Fault text written at each call site     | Reason item stating its wording once     | `e9a82a6e7`                   |
|  [29]   | Literals     | Milliseconds or seconds as a number      | `Duration`, `UnitsNet` quantity          | `dotnet:coding`               |
|  [30]   | Pass-through | Private member read once, rule exempt    | Body at its reader                       | Private readers survey        |
|  [31]   | Pass-through | Mapping method forwarding its source     | `Use = nameof(...)`, `[UseStaticMapper]` | `dotnet:mapperly`             |
|  [32]   | Pass-through | `IO.lift` of a continuation value        | Value itself                             | `RASM1011`                    |
|  [33]   | Pass-through | `Pure` where conversion is implicit      | Value itself                             | `RASM1001`                    |
|  [34]   | Exports      | `public` declaration in a product        | `internal`                               | Visibility count              |
|  [35]   | Exports      | Library member no other assembly reads   | `internal` or `private`, member kept     | `fbad4128a`                   |
|  [36]   | Exports      | File `global using` or `using` alias     | `Using` row on its package reference     | `Directory.Build.targets`     |
|  [37]   | Nesting      | `if` chain over a type or shape          | Switch expression, patterns, `when`      | `0ae37df1e`                   |
|  [38]   | Nesting      | `Bind` or `Match` nested per step        | LINQ query, tuple `Apply`                | `0ae37df1e`                   |
|  [39]   | Nesting      | Loop in a loop over an accumulator       | Query of 2 `from` clauses, `Fold`        | `0ae37df1e`                   |
|  [40]   | Nesting      | Decision over 2 values                   | Tuple switch `(a, b) switch`             | `dotnet:coding`               |
|  [41]   | Names        | Type clashing with an imported type      | Declaration renamed                      | `RASM3006`                    |
|  [42]   | Names        | Qualified name with no clash, alias      | Simple name, namespace imported          | `RASM3010`, `RASM3014`        |
|  [43]   | Names        | Mechanism name (`*Ops`, `*Helpers`)      | Field noun through `rename_symbol`       | `writing:names`               |
|  [44]   | Names        | Private field beside its property        | `field` keyword in the accessor          | `dotnet:coding`               |

Rows hold under these conditions:
- Row [07] -> aligning the fields of each record and keeping one per package leaves the family (`e3ec52163`), so the type parameter goes in the same pass
- Row [13] -> a case holding data its consumer reads stays a union
- Row [14] -> both host types read whole first, as the host's own table interface decides the shared member
- Row [20] -> host-loaded contracts (`PlugIn`, GH2 `Component`, Eto `IHandler` platform seams) stay, as `RASM2205` states
- Row [22] -> generator attributes (`[SmartEnum]`, `[Union]`, `[LoggerMessage]`, `[Mapper]`) need `partial` on the type and every enclosing type
- Row [30] -> helpers holding `[CallerMemberName]`, `[CallerArgumentExpression]`, or a type parameter relating their arguments (`Fault.Of<TReason>`) stay
- Row [35] -> a library member `find_unused_symbols` lists stays as API, as `7f77262d2` removed 35 Cycles settings `fbad4128a` restored
- Rows [02], [19] -> a checker forcing a form goes to the 2-agent review, and its forced forms leave in bulk once its rule goes

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
```

- Build: `check--dotnet-build` compiles with warnings as errors under every project analyzer (Meziantou, Roslynator, ErrorProne.NET, VS Threading, `Rasm.Analyzers`), and `check--dotnet-format` checks style
- Diagnostics: codelens `get_diagnostics` runs analyzers with no option values and is no check, the build decides
- Manifest: `<paths>` holding a `.csproj`, `.props`, `.targets`, or `packages.lock.json` -> same `nx affected` line without `--exclude=rasm`
- Host: Rhino and Grasshopper plug-ins have no validator, and a host-loaded change proves itself through its `rhino:testing` suite
