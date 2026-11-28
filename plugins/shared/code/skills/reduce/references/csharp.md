# [CSHARP]

C# targets hold `.cs` files of the projects `Workspace.slnx` lists, `<project>` naming the folder holding a `.csproj`, `<name>` its project name as `nx show projects` prints it, `<paths>` files a pass touches, `<dir>` session scratch folder, and `<roots>` `libs products tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET]         | [SKILLS]                                      | [CONDITION]                                      |
| :-----: | :--------------- | :-------------------------------------------- | :----------------------------------------------- |
|  [01]   | Any C#           | `dotnet:coding`, `dotnet:codelens`            | Always                                           |
|  [02]   | Core packages    | `dotnet:languageext`, `dotnet:thinktecture`   | Always, whatever the target imports              |
|  [03]   | Mapping          | `dotnet:mapperly`                             | Mapping written by hand, or a `[Mapper]`         |
|  [04]   | Buffers, kernels | `dotnet:memory`                               | Span, buffer, or numeric loop written by hand    |
|  [05]   | Drawn UI         | `dotnet:ui`                                   | Drawing, text layout, color, or Eto control code |
|  [06]   | Tests            | `code:test`, `rhino:testing` for Rhino suites | `*.Tests.csproj`                                 |
|  [07]   | MSBuild          | `msbuild:antipatterns`, `msbuild:evaluation`  | `.csproj`, `.props`, `.targets`                  |
|  [08]   | Packages         | `msbuild:packaging`                           | `Directory.Packages.props`, `.slnx`              |

Rhino and Grasshopper code takes these rows alone, its host SDK a core source [02]-[SOURCES] reads. `Directory.Build.targets` adds global usings per `PackageReference` (`LanguageExt.Prelude`, `Thinktecture`, aliases `Duration` and `Length`), so a file's `using` lines never decide a row.

## [02]-[SOURCES]

C# targets read their compiler, BCL, LanguageExt, Thinktecture, generator output, and host SDK every session whatever they import, then the central manifest whole, as most hand-rolled C# in this tree restates one of them. Package reads, their commands below in step order:
1. Language: compiler and language versions of the SDK, a feature its docs list confirmed by one probe build at the project's framework
2. Runtime: BCL reference pack at the version the project's assets file records, its XML ids per kind into `<dir>`
3. LanguageExt: `LanguageExt.Core` at the version the lock resolves, `README.nuget.md` read whole, XML ids per kind, then codelens groups
4. Thinktecture: `Thinktecture.Runtime.Extensions` through step 3's commands, its `README.md` read whole
5. Generated: generators per project and each output file, the source of `Items`, `Get`, `TryGet`, `Switch`, `Map`, `Validate`, and logger bodies
6. Host: RhinoCommon, Eto, and Grasshopper2 XML ids for host code, `ilspycmd` types for `Rhino.UI`, a runtime behavior read through `tool:inspect`
7. Catalog: `Directory.Packages.props` listed whole, each candidate judged by its declarations and docs against a job the target does by hand
8. Candidate: catalog package no project references, downloaded at its pinned version, its guide and XML ids read through step 3's commands

```bash
dotnet --version && dotnet "$(dirname "$(readlink -f "$(command -v dotnet)")")/sdk/$(dotnet --version)/Roslyn/bincore/csc.dll" -langversion:?
jq -r '.project.frameworks[].downloadDependencies[] | select(.name == "Microsoft.NETCore.App.Ref") | .version | ltrimstr("[") | split(",")[0]' \
    .artifacts/dotnet/obj/<name>/project.assets.json
jq -r --arg id '<Id>' '[.dependencies[][$id]?.resolved] | unique[]' <project>/packages.lock.json
yq -p xml -oy '[.Project.ItemGroup[].PackageVersion[]? | select(."+@Include" == "<Id>") | ."+@Version"][0]' Directory.Packages.props
fd -d 1 -i '^(agents\.md|llms.*\.txt|readme(\.nuget)?\.md|docs)$' .cache/nuget/packages/<id>/<version>
fd -e xml . .cache/nuget/packages/<id>/<version>
rg -o 'name="([TMPFE]):([^"(]+)' -r '$1 $2' <xml> > <dir>/<id>.members.txt && cut -d' ' -f1 <dir>/<id>.members.txt | sort | uniq -c
ilspycmd -l cisde <dll>
rg -n 'Label=|<PackageVersion (Include|Update)=' Directory.Packages.props
dotnet package download <Id>@<version> --prerelease --output <dir>/pkg
```

```text
inspect_external_assembly {"assemblyName": "<Assembly>"}
inspect_external_assembly {"assemblyName": "<Assembly>", "mode": "namespace", "namespaceFilter": "<Namespace>"}
get_source_generators {"project": "<name>"}
get_generated_code {"file": "<Namespace>.<Type>.<Facet>.g.cs", "limit": 1}
```

Reads hold under these conditions:
- Version: lock row where the project references the package, catalog pin through `yq` where it references none yet
- Store: `NuGet.config` sets `.cache/nuget/packages`, folder names lowercase, the BCL pack under `microsoft.netcore.app.ref/<version>/ref/<tfm>`
- Folders: packages ship one XML file per `lib/<tfm>` or `ref/<tfm>`, the newest folder at or below the project's framework deciding
- Host: RhinoCommon holds `RhinoCommon.xml`, `Eto.xml`, and `Rhino.UI.dll` with no XML, Grasshopper2 `Grasshopper2.xml` and `GrasshopperIO.xml`
- Generated: output exists in the compilation alone, `get_source_generators` naming paths no disk file holds and `get_generated_code` reading them
- Facets: each generated file is one facet of one type (`SmartEnum`, `EqualityComparisonOperators`), and a type name alone returns every facet whole
- Unreferenced: codelens inspects referenced assemblies alone, so a candidate reads from its download, a prerelease pin needing `--prerelease`
- Members: one type's declaration, decompiled source, and XML doc ids follow `search:code`

## [03]-[MAPPING]

Maps list what each file of the target declares at which level and who reads it, one command or call per step, each run once over `<paths>`:
1. Visibility: declarations per file with their effective level, a `public` member of a non-public type counted `internal`
2. Name families: type and nested type names differing by an `I` prefix, a plural, or a mechanism suffix, each family read whole before judging it
3. Constants: non-private `const` and `static readonly` fields other than smart enum items, each traced to its reader and the source that states it
4. Imports: global usings the project compiles with, as a reader's file shows no `using` for them
5. Readers: codelens references by symbol identity, callers, implementors, attribute holders, reflection reads, and members nothing references
6. Text: sites spelling a member, key, setting name, or message a declaration holds, and mentions across docs, skills, and manifests
7. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
8. Nesting: sites at the limit from the rule hk runs, then members ranked by codelens `maxNesting` and `cognitive`

```bash
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | .items[] | select(.symbolType != "namespace") | . as $t
    | ([$t] + [($t.members // [])[] | select(.astKind != "parameter" and .astKind != "local_function_statement")])[]
    | ((.signature // "") | sub("^(\\[[^]]*\\]\\s*)+"; "") | sub("^extension(<[^>]*>)?\\([^)]*\\)\\s*"; "")
        | capture("^(?<v>public|internal|protected|private|file)\\b").v // "implied") as $v
    | [$p, .range.start.line + 1, (if $v == "public" and ($t.isExported | not) then "internal" else $v end), .symbolType, .name] | @tsv' > <dir>/visibility.tsv
cut -f1,3 <dir>/visibility.tsv | sort | uniq -c
ast-grep outline <paths> --items structure --view expanded --json=stream |
    jq -r '.items[] | select(.symbolType != "namespace") | .name, ((.members // [])[] | select(.symbolType | test("class|struct|interface|enum")) | .name)' | sort -u |
    awk '{ k = $1; if (k ~ /^I[A-Z][a-z]/) k = substr(k, 2); sub(/(Extensions|Helpers|Utilities|Factory|Builder|Ops|s)$/, "", k)
        g[k] = g[k] " " $1; n[k]++ } END { for (k in g) if (n[k] > 1) print g[k] }'
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | .items[] | select(.symbolType != "namespace") | .name as $t
    | (.members // [])[] | select((.signature // "") | test("^(\\[[^]]*\\]\\s*)*(public|internal)\\b[^(=]*\\b(const|static readonly)\\b"))
    | select(.signature | test("static readonly " + ($t | sub("<.*"; "")) + "\\b") | not) | "\($p):\(.range.start.line + 1)\t\(.signature | sub("\\s*=.*"; ""))"'
cat .artifacts/dotnet/obj/<name>/release/<name>.GlobalUsings.g.cs
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format csharp --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
ast-grep scan --filter no-fourth-nesting-level-csharp --report-style short <paths>
```

```text
search_symbols {"query": "<Name>"}
find_references {"symbol": "<Namespace.Type.Member>", "kinds": ["<kind>"]}
find_callers {"symbol": "<Namespace.Type.Method>"}
find_implementations {"symbol": "<Namespace.Interface>"}
find_attribute_usages {"attribute": "<Attribute>"}
find_reflection_usage {"symbol": "<Type>"}
find_unused_symbols {"project": "<name>", "includeInternal": true}
get_complexity_metrics {"project": "<name>", "metric": "cognitive", "threshold": 0, "limit": 500}
```

Commands read by their own rules:
- Visibility: `implied` rows hold enum members, interface members, and explicit interface implementations, positional record parameters staying out
- Families: a plural pair is a collection type beside its element or an operations class beside its type, and only a read of both tells them apart
- Readers: `find_references` takes the fully qualified name `search_symbols` prints, and `isGenerated` rows are a generator's own reads
- Kinds: `write` and `readwrite` find mutation, `invocation` and `method_group` calls, `object_creation` construction a factory replaces
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Levels: the rule counts `if` (an `else if` chain once), loops, switches, and conditionals, a one-arm `x switch { var y => ... }` binding none
- Closures: lambda, anonymous method, and local function bodies count a level at 2 or more statements or a control inside
- Ranking: codelens rows of `<paths>` files order whole reads, `maxNesting` counting a chained conditional as nested where the rule counts one

## [04]-[MECHANISMS]

C# states behavior on nominal types and lets its compiler and source generators write members from one declaration, most reductions here replace hand members with a declaration form and narrow each declaration to the level its readers need:
- Records: a positional `record` declares constructor, properties, equality, `Deconstruct`, and `with` in one line, `init` members holding options
- Copies: `with { }` derives a successor from a record, in place of a constructor call restating every unchanged member
- Primary constructors: class parameters replace fields and `this.x = x` lines, a field initializer (`same = same.IfNone(Equal)`) normalizing one
- Backing fields: `field` in an accessor replaces a private field beside its property, normalizing in `init => field = value.Trim()`, caching in `??=`
- Generators: a `partial` declaration takes members its attribute's generator writes (Thinktecture, `[LoggerMessage]`, `[GeneratedRegex]`, `[Mapper]`)
- Smart enums: one `[SmartEnum]` whose items hold data and `[UseDelegateFromConstructor]` behavior replaces an `enum` and a `switch` per reader
- Unions: an `[Union]` abstract partial record with nested `sealed` cases closes a hierarchy under a private constructor and an exhaustive `Switch`
- Nested names: cases nested in their base read as `Selection.Many` and `Command<TInvoke>.Run`, one name in place of an affix per case
- Collections: `[.. head, item, .. option.ToSeq()]` builds a target-typed `Seq<T>`, `ImmutableArray<T>`, or span, in place of builders and `Concat`
- Patterns: switch expressions over type, property, extended property, relational, list, and tuple patterns replace `if` chains, casts, and `is` tests
- Generic math: one member constrained on `INumber<T>` or `IBinaryInteger<T>` replaces a copy per numeric type, its limits read from `T`
- Static interface members: `static abstract` and `static virtual` members move a per-type factory or constant onto its type, read as `T.Member`
- Extension members: `extension(<receiver>)` blocks give a foreign type properties, operators, and members, in place of `GetX(x)` helpers and wrappers
- Static extensions: a block with an unnamed receiver adds static members to a foreign type, `Command<Invoke>.Picked(...)` needing no operations class
- Arity: types key by name and arity, a static class beside a generic type of its name (`Fault`, `Fault<TReason>`) holds factories inferring it
- Same name: a property can take its type's name (`Gamut Gamut`), and a static factory on the type (`X.Of`) replaces an `XFactory` or `CreateX`
- Visibility: `public` for API other assemblies read, `internal` for its own, `file` for a type one file reads, `private` for members and nested types
- Closure: `sealed` on every type no case extends, and explicit interface implementation keeps an interface's members off a type's public surface

Host rules bind where a compiler or generator refuses a form, each proved by one probe build:
- Extension blocks compile only inside a top-level, non-generic static class (CS9283), never nested in their concept's type or generic over it
- `file` applies to types alone (CS0106), a member its own file reads alone staying `private`
- `union` declarations need `IUnion` and `UnionAttribute` from their framework (CS0518, CS0656 without them), lowering to a struct over `object?`
- Frameworks lacking `IUnion` (Rhino's runtime, `netstandard2.0`) take Thinktecture `[Union]`, and one holding it checks `union` arms (CS8509)

## [05]-[PATTERNS]

Patterns common in this tree's C#, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]                 | [LOOK_FOR]                                                  | [REBUILD]                                     |
| :-----: | :------------------------ | :---------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Hand generated members    | `Equals`, `GetHashCode`, `ToString`, `Parse`, `Items` scans | Record, generator attribute, generated member |
|  [02]   | Constructor boilerplate   | Fields set from parameters, field beside its property       | Primary constructor, record, `field`          |
|  [03]   | Operations beside a type  | `Xs` or `XHelpers` class of functions over `X`              | Members, static factories, static extensions  |
|  [04]   | Extension sprawl          | `XExtensions` class per receiver, `GetX(x)` helpers         | One `extension` class named by its field      |
|  [05]   | Interface per implementer | `IX` with one implementer                                   | `sealed` concrete type, `Func` value          |
|  [06]   | Per-type copies           | Overloads differing by numeric type or a per-type value     | Generic member, `static abstract` member      |
|  [07]   | Enums with switches       | `enum` branched in many members, `Map` repeating item facts | Smart enum items holding data, delegates      |
|  [08]   | Flags and kind fields     | Exclusive booleans, `is` tests, kind beside payload fields  | `[Union]` record, nested `sealed` cases       |
|  [09]   | Builders and copies       | `List<T>` with `Add`, `Concat`, `ToArray`, hand copies      | Collection expression, `with`                 |
|  [10]   | Visibility past readers   | `public` in a product, `internal` type one file reads       | `internal`, `file`, `private`                 |

Patterns hold under these conditions:
- Row [01] -> equality a comparer decides (ordinal text, sequence content) takes a generated comparer attribute or accessor, never a hand override
- Row [02] -> Thinktecture types refuse a primary constructor (TTRESG043) and keep the one their generator writes
- Row [03] -> an operation a host calls by name (command, component, handler registration) stays where its registration reads it
- Row [04] -> blocks need their top-level static class, so a pass cuts the class count and its mechanism name, never the class itself
- Row [05] -> host seams (Eto handlers, platform files, `PlugIn`) and interfaces serving as a static-abstract constraint (`IBlend`, `IWireRoute`) stay
- Row [06] -> overloads a host registers by reflected signature (Grasshopper 2 `ConversionServer` conversions) stay one per signature
- Row [07] -> a fact one consumer owns stays at its `Map`, and an `enum` a host or native API defines stays at that boundary
- Row [09] -> host APIs filling a `List<T>` argument keep the list inside the effect that passes it

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
```

- Scope: root project `rasm` runs repository-wide knip and jscpd, which fail on other work in a shared tree, and a pass excludes it
- Build: `check--dotnet-build` compiles with warnings as errors under every project analyzer, and `check--dotnet-format` checks style
- Diagnostics: codelens `get_diagnostics` runs analyzers with no option values and is no check, the build decides
- Manifest: `<paths>` holding a `.csproj`, `.props`, `.targets`, or `packages.lock.json` -> same `nx affected` line without `--exclude=rasm`
- Host: Rhino and Grasshopper plug-ins have no validator, and a host-loaded change proves itself through its `rhino:testing` suite
