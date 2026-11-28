# [CSHARP]

C# targets hold `.cs` files of projects `Workspace.slnx` lists. Placeholders: `<project>` names project folder holding `.csproj`, `<name>` its project name as `nx show projects` prints it, `<paths>` files each pass touches, `<dir>` session scratch folder, and `<roots>` `libs products tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET]         | [SKILLS]                                      | [CONDITION]                                      |
| :-----: | :--------------- | :-------------------------------------------- | :----------------------------------------------- |
|  [01]   | Any C#           | `dotnet:coding`, `dotnet:codelens`            | Always                                           |
|  [02]   | Core packages    | `dotnet:languageext`, `dotnet:thinktecture`   | Always, whatever target imports                  |
|  [03]   | Mapping          | `dotnet:mapperly`                             | Members copied between shapes by hand            |
|  [04]   | Buffers, kernels | `dotnet:memory`                               | Span, buffer, or numeric loop written by hand    |
|  [05]   | Drawn UI         | `dotnet:ui`                                   | Drawing, text layout, color, or Eto control code |
|  [06]   | Tests            | `code:test`, `rhino:testing` for Rhino suites | Suite under `tests/`, or pass changing one       |
|  [07]   | MSBuild          | `msbuild:antipatterns`, `msbuild:evaluation`  | `.csproj`, `.props`, `.targets`                  |
|  [08]   | Packages         | `msbuild:packaging`                           | `Directory.Packages.props`, `.slnx`              |

Rhino and Grasshopper code takes table rows alone. File `using` lines never decide rows, as `Directory.Build.targets` adds global usings per `PackageReference` (`LanguageExt.Prelude`, `Thinktecture`, aliases `Duration` and `Length`).

## [02]-[SOURCES]

C# targets read their compiler, BCL, LanguageExt, Thinktecture, generator output, and host SDK every session, whatever they import, then central manifest whole, as most hand-rolled C# here restates one of them. Package reads, commands below in step order:
1. Language: SDK compiler and language versions, features its docs list confirmed by one probe build at project framework
2. Runtime: BCL reference pack at its version in project assets file, XML ids per kind into `<dir>`
3. LanguageExt: each `LanguageExt.*` package the assets file resolves, `README.nuget.md` read whole, XML ids per kind, then codelens namespaces
4. Thinktecture: each `Thinktecture.Runtime.Extensions*` package through step 3's commands, its `README.md` read whole
5. Generated: generators per project and each output file, source of `Items`, `Get`, `Switch`, `Map`, `Validate`, operators, and logger bodies
6. Host: RhinoCommon, Eto, and Grasshopper2 XML ids for host code, `ilspycmd` types for `Rhino.UI`, runtime behavior read through `tool:inspect`
7. Workspace: target project references, each library read at members target files restate
8. Catalog: `Directory.Packages.props` listed whole, each package for a job target code does by hand added to its project and read through step 3

```bash
dotnet --version && dotnet "$(dirname "$(readlink -f "$(command -v dotnet)")")/sdk/$(dotnet --version)/Roslyn/bincore/csc.dll" -langversion:?
jq -r '.project.frameworks[].downloadDependencies[] | select(.name == "Microsoft.NETCore.App.Ref") | .version | ltrimstr("[") | split(",")[0]' \
    .artifacts/dotnet/obj/<name>/project.assets.json
jq -r '.libraries | keys[] | select(test("^(LanguageExt|Thinktecture)"))' .artifacts/dotnet/obj/<name>/project.assets.json
yq -p xml -oy '[.Project.ItemGroup[].PackageVersion[]? | select(."+@Include" == "<Id>") | ."+@Version"][0]' Directory.Packages.props
fd -d 1 -i '^(agents\.md|llms.*\.txt|readme(\.nuget)?\.md|docs)$' .cache/nuget/packages/<id>/<version>
fd -e xml . .cache/nuget/packages/<id>/<version>
rg -o 'name="([TMPFE]):([^"(]+)' -r '$1 $2' <xml> > <dir>/<id>.members.txt && cut -d' ' -f1 <dir>/<id>.members.txt | sort | uniq -c
ilspycmd -l cisde <dll>
rg -n 'Label=|<PackageVersion (Include|Update)=' Directory.Packages.props
```

```text
inspect_external_assembly {"assemblyName": "<Assembly>"}
inspect_external_assembly {"assemblyName": "<Assembly>", "mode": "namespace", "namespaceFilter": "<Namespace>"}
get_source_generators {"project": "<name>"}
get_generated_code {"file": "<Namespace>.<Type>.<Facet>.g.cs", "limit": 1}
get_project_dependencies {"project": "<name>"}
```

Reads hold under conditions below:
- Version: assets row where project references its package, catalog pin through `yq` where it references none yet
- Store: `NuGet.config` sets `.cache/nuget/packages`, folder names lowercase, BCL pack under `microsoft.netcore.app.ref/<version>/ref/<tfm>`
- Folders: packages hold one XML file per `lib/<tfm>` or `ref/<tfm>`, and newest folder at or below project framework decides
- Host: RhinoCommon holds `RhinoCommon.xml`, `Eto.xml`, and `Rhino.UI.dll` with no XML, Grasshopper2 `Grasshopper2.xml` and `GrasshopperIO.xml`
- Generated: output exists in compilation alone, `get_source_generators` names paths no disk file holds, and `get_generated_code` reads them
- Facets: generated files hold one facet of one type each (`SmartEnum`, `EqualityComparisonOperators`), and type name alone returns every facet
- Adoption: catalog packages read once their `PackageReference` joins and `rebuild_solution` runs, as codelens inspects referenced assemblies alone
- Members: one type's declaration, decompiled source, and XML doc ids follow `search:code`

## [03]-[MAPPING]

Maps list what each file of the target declares at which level and who reads it, one command or call per step, each run once over `<paths>`:
1. Visibility: declarations per file with their effective level, `public` members of non-public types counted `internal`
2. Name families: type and nested type names differing by `I` prefix, plural, or mechanism suffix, each family read whole before judging it
3. Constants: non-private `const` and `static readonly` fields other than smart enum items, each traced to its readers and to the source stating it
4. Imports: global usings the project compiles with, as reader files show no `using` for them
5. Readers: codelens references by symbol identity, callers, implementors, attribute holders, reflection reads, and members nothing references
6. Text: sites spelling a member, key, setting name, or message a declaration holds, and mentions across docs, skills, and manifests
7. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
8. Nesting: sites at limit hk's rule sets, then members ranked by codelens `maxNesting` and `cognitive`

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
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | .items[] | select(.symbolType != "namespace") | (.name | sub("<.*"; "")) as $t
    | (.members // [])[] | select((.signature // "") | test("^(\\[[^]]*\\]\\s*)*(public|internal)\\b[^(=]*\\b(const|static readonly)\\b"))
    | (.name | split(".") | if length > 1 then .[-2] else $t end) as $o
    | select(.signature | test("static readonly " + $o + "\\b") | not) | "\($p):\(.range.start.line + 1)\t\(.signature | sub("\\s*=.*"; ""))"'
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

Command outputs read by their own rules:
- Visibility: `implied` rows hold enum members, interface members, and explicit interface implementations, positional record parameters staying out
- Families: plural pairs are collection types beside their element or operations classes beside their type, and only reading both tells them apart
- Constants: nested members print as `<Nested>.<Member>`, and fields typed as their own declaring type (smart enum item, union case) stay out
- Readers: `find_references` takes fully qualified name `search_symbols` prints, and `isGenerated` rows are generators' own reads
- Kinds: `write` and `readwrite` find mutation, `invocation` and `method_group` calls, `object_creation` construction a factory replaces
- Unused: smart enum items read by generated `Items` alone print as unused, as `find_unused_symbols` drops generated reads
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Levels: rule counts `if` (`else if` chain once), loops, switches, and conditionals, one-arm `x switch { var y => ... }` binding none
- Closures: lambda, anonymous method, and local function bodies count one level when holding 2 or more statements or one control statement
- Ranking: codelens rows of `<paths>` files order whole reads, `maxNesting` counting chained conditionals as nested where hk's rule counts one level

## [04]-[MECHANISMS]

C# states behavior on nominal types, and its compiler and source generators write members from one declaration. Most cuts replace hand-written members with a declaration form and narrow each declaration to its readers' level. Root `Directory.Build.props` decides which forms compile: `LangVersion` `preview` over SDK compiler, `net10.0` for every project but Roslyn components on `netstandard2.0`, and warnings as errors under `AnalysisLevel` `latest-all`.

[DECLARATIONS]:
- Records: positional `record` declares constructor, `init` properties, value equality, `Deconstruct`, `ToString`, and `with` in one line
- Equality: records compare members through `EqualityComparer<T>.Default`, `string`, `Seq`, and `HashMap` by content and arrays by reference
- Copies: `with { }` copies a record with named members changed, in place of a constructor call restating every unchanged member
- Options: `required` and `init` members with defaults replace a constructor parameter per option, and compiler refuses initializers missing one
- Primary constructors: class parameters replace fields and `this.x = x` lines, field or property initializers normalizing one parameter once
- Backing fields: `field` in accessors replaces a private field beside its property, normalizing in `init` and caching through `??=` on classes
- Nested names: cases nested in their base read as `Selection.Many` and `Command<TInvoke>.Run`, one name in place of an affix per case
- Arity: static classes beside a generic type of their name (`Fault`, `Fault<TReason>`) hold factories inferring it, as types key by name and arity
- Same name: properties take their type's name (`Gamut Gamut`), and static factories on their type (`X.Of`) replace `XFactory` or `CreateX`

[GENERATORS]:
- Partial: `partial` declarations take members their generator writes (Thinktecture, `[LoggerMessage]`, `[GeneratedRegex]`, `[JsonSerializable]`)
- Value objects: `[ValueObject<TKey>]` writes factories, equality, comparison, operators, `IParsable`, and key conversions from one key and its rule
- Composites: `[ComplexValueObject]` writes equality and factories, `[IgnoreMember]` and `[MemberEqualityComparer]` picking which members compare
- Text forms: `[ObjectFactory<string>]` with one `Validate` and one `ToValue` writes `Parse`, `TryParse`, `IParsable`, and serializer conversions
- Smart enums: `[SmartEnum]` items holding data and `[UseDelegateFromConstructor]` behavior replace an `enum` and a `switch` per reader
- Item sets: `Items` lists a set once, and generic code reaches `Items`, `Get`, and `TryGet` of every set through `ISmartEnum<TKey, T, TError>`
- Unions: `[Union]` abstract partial record with nested `sealed` cases closes a hierarchy under a private constructor and an exhaustive `Switch`
- Language unions: `union X(A, B)` closes a union over existing types, and switch expressions missing a case fail CS8509
- Accessors: `[UnsafeAccessor]` extern methods read non-public host members by signature, in place of reflection over a name string

[RESULTS]:
- Absence: `Option<A>` with `Optional` at a nullable boundary replaces `null` returns, `bool Try*(out T)` pairs, and `IsSome` tests before an unwrap
- Failure: `Fin<A>` with `Fault.Of` and `Fault.Unless` replaces exceptions as outcomes, `(bool, T, string)` tuples, and `default` on failure
- Accumulation: tuple `Apply` and `Traverse` over `Fin` checks replace hand-written error lists, `Bind` and `TraverseM` stopping at first failure
- Effects: `IO.lift` at throwing host calls replaces `try` and `catch` blocks, `use` with `Bracket` replacing `try` and `finally` release
- Collections: `Seq`, `HashMap`, and `Map` hold structural equality and persistent updates, `toSeq` copying a mutable input once at its boundary

[MEMBERS]:
- Patterns: type, property, extended property, relational, list, and tuple patterns in switch expressions replace `if` chains, casts, and `is` tests
- Lists: list patterns bind split parts (`text.Split(':') is [var paper, var model]`), in place of length tests and index reads
- Collections: `[.. head, item, .. option.ToSeq()]` builds a target-typed `Seq<T>`, `ImmutableArray<T>`, or span, in place of builders and `Concat`
- Params: `params ReadOnlySpan<T>` and `params Seq<T>` take lists of items or strategies, in place of an overload per count or array arguments
- Generic math: one member constrained on `INumber<T>` or `IBinaryInteger<T>` replaces a copy per numeric type, its limits read from `T`
- Static interface members: `static abstract` and `static virtual` members move per-type factories and constants onto their type, read as `T.Member`
- Extension members: `extension(<receiver>)` blocks give foreign types properties, operators, and members, in place of `GetX(x)` helpers and wrappers
- Static extensions: blocks with unnamed receiver (`extension(Command<Invoke>)`) add factories read as `Command<Invoke>.Toggle(...)`
- Caller information: `[CallerArgumentExpression]` and `[CallerMemberName]` derive a label from call text, in place of a caption string per call

[VISIBILITY]:
- Levels: `public` for API other assemblies read, `internal` for its own, `file` for types one file reads, `private` for members and nested types
- Modifiers: each level reads from its declaration's own text, as `.editorconfig` requires an access modifier on every declaration
- Sealing: passes decide `sealed` on public types, as CA1852 fails builds on internal types no type extends and no `sealed` closes
- Interfaces: explicit interface implementation keeps interface members off a type's public members

Host rules apply where a compiler or generator refuses a form, each proved by one probe build:
- Extension blocks compile only inside a top-level, non-generic static class (CS9283), never nested in their concept's type or generic over it
- `file` applies to types alone (CS0106), and members their own file alone reads stay `private`
- `union` declarations need `IUnion` and `UnionAttribute` (CS0518, CS0656), held in .NET 11 reference pack and absent from .NET 10
- Frameworks lacking `IUnion` (Rhino's .NET 10 runtime, `netstandard2.0`) take Thinktecture `[Union]`
- Types a validating factory gates keep a private constructor, as primary constructors compile `public`, `protected` on abstract types
- Thinktecture types refuse primary constructors (TTRESG043), as their generator owns a private one

## [05]-[PATTERNS]

C# across the repository repeats patterns below, each a candidate a map confirms before any rebuild:

| [INDEX] | [PATTERN]                 | [LOOK_FOR]                                                 | [REBUILD]                                    |
| :-----: | :------------------------ | :--------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Operations beside types   | `Xs`, `XOps`, `XExtensions`, `XFactory` class over `X`     | Members, static factories, static extensions |
|  [02]   | Hand-rolled equality      | `Equals` and `GetHashCode` over member tuple or comparer   | Record, `[ComplexValueObject]` members       |
|  [03]   | Repeated guards           | `Fault.Unless` rule over one primitive across types        | One `[ValueObject<T>]` holding rule          |
|  [04]   | Hand-rolled results       | `bool Try*(out T)`, `null` lookup, `(bool, T)`, `IsSome ?` | `Option`, `Fin`, `Match`, tuple `Apply`      |
|  [05]   | Hand-rolled text forms    | `Parse` beside `ToString` override on one value type       | `[ObjectFactory<string>]`, `IParsable`       |
|  [06]   | Constructor boilerplate   | Fields set from parameters, field beside its property      | Primary constructor, record, `field`         |
|  [07]   | Enums with switches       | `enum` or key string branched in many members              | Smart enum items holding data, delegates     |
|  [08]   | Flags and kind fields     | Exclusive booleans, `is` tests, kind beside payload fields | `[Union]` record, nested `sealed` cases      |
|  [09]   | Per-type copies           | Overloads differing by numeric type or per-type value      | Generic member, `static abstract` member     |
|  [10]   | Restated copies           | Constructor restating members, `List<T>` filled once       | `with`, collection expression                |
|  [11]   | Interface per implementer | `IX` with one implementer                                  | `sealed` concrete type, `Func` value         |
|  [12]   | Visibility past readers   | `public` in product code, `internal` type one file reads   | `internal`, `file`, `private`                |

Rows hold under conditions below:
- Row [01] -> generic `X<T>` takes static class `X` inferring `T`, and foreign `X` takes static extension block
- Row [01] -> operations a host calls by name (command, component, handler registration) stay where their registration reads them
- Row [01] -> passes cut class count and mechanism names down to one top-level static class, as extension blocks need one
- Row [02] -> records holding reference identity over mutable host graphs (`ReferenceEquals`, `RuntimeHelpers.GetHashCode`) keep their override
- Row [03] -> rules differing per owning type, or ranges an owner's other members decide, stay in their owner's `Validated`
- Row [04] -> host setters taking `null` read `ValueUnsafe` at their call, and host `Try*` calls convert to `Option` at their call alone
- Row [05] -> types no parser, serializer, or binder reads keep `Validated` and their own text member, as `[ObjectFactory]` converts `Fin` back
- Row [06] -> Thinktecture types and types a validating factory gates keep their private constructor
- Row [07] -> facts one consumer owns stay at its `Map`, and `enum` types a host or native API defines stay at their boundary
- Row [09] -> overloads a host registers by reflected signature (Grasshopper 2 `ConversionServer` conversions) stay one per signature
- Row [10] -> host APIs filling a `List<T>` argument keep each list inside the effect passing it
- Row [11] -> host seams (Eto handlers, platform files, `PlugIn`), static-abstract constraints (`IBlend`), and type-erasing interfaces (`ISlot`) stay
- Row [12] -> types a host loads by reflection (plug-in, command, component) keep the level their loader reads

## [06]-[CHECK]

Checks run per pass, each command in its own background call, with `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
```

- Scope: passes exclude root project `rasm`, as its repository-wide checks (jscpd, knip, NuGet restore) fail on other work in a shared tree
- Build: `check--dotnet-build` compiles with warnings as errors under every analyzer, the repository's `RASM` rules included
- Format: `check--dotnet-format` checks style, and codelens `get_diagnostics` runs analyzers with no option values and decides nothing
- Manifest: `<paths>` holding `.csproj`, `.props`, `.targets`, or `packages.lock.json` -> same `nx affected` line without `--exclude=rasm`
- Host: Rhino and Grasshopper plug-ins have no validator, and host-loaded changes prove themselves through their `rhino:testing` suite
