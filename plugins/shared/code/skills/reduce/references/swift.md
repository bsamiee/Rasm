# [SWIFT]

Swift targets hold `.swift` files of root `Package.swift` modules, which `xcodebuild` builds through `Rasm.xcworkspace`, and of nested SwiftPM root `products/developer/nx/apple/analyzer`, `<Module>` naming a target as `Package.swift` and `nx show projects` print it, `<package>` its checkout folder, `<paths>` files a pass touches, `<dir>` session scratch folder, `<roots>` `libs products tests`, `$D` this checkout's Xcode build folder, `$B` its built products, `$MM` module map arguments a built module's symbol graph reads, and `sg <Module> <level>` the symbol graph of one module at one access level, written to `<dir>/sg/<Module>/` with a row per symbol (path, kind, level, first doc line) in `<dir>/sg/<Module>.tsv`:

```bash
D=$(dirname "$(rg -l -a -F "$PWD/Rasm.xcworkspace<" ~/Library/Developer/Xcode/DerivedData/Rasm-*/info.plist)") && B="$D/Build/Products/Debug"
MM=$(fd -e modulemap . "$D/SourcePackages/checkouts" libs/swift/macos/SPI "$D/Build/Intermediates.noindex/GeneratedModuleMaps" | sed 's/^/-Xcc -fmodule-map-file=/')
sg() { mkdir -p <dir>/sg/$1 && xcrun swift-symbolgraph-extract -module-name "$1" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -I "$B" -F "$B" $MM -minimum-access-level "$2" -skip-synthesized-members -output-dir <dir>/sg/$1 &&
    jq -r '.symbols[] | [(.pathComponents | join(".")), .kind.identifier, .accessLevel, (.docComment.lines[0].text // "")] | @tsv' <dir>/sg/$1/*.symbols.json > <dir>/sg/$1.tsv; }
```

Analyzer targets call `B=products/developer/nx/apple/analyzer/.build/out/Products/Debug MM= sg <Module> <level>`, as that root builds apart.

## [01]-[SKILLS]

| [INDEX] | [TARGET]          | [SKILLS]                      | [CONDITION]                                                       |
| :-----: | :---------------- | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | Any Swift         | `swift:coding`                | Always, whatever the target imports                               |
|  [02]   | Tests             | `swift:testing`, `code:test`  | Test target under `tests/libs/swift`, analyzer `Tests/`           |
|  [03]   | SDK or SPI member | `tool:inspect`, `search:code` | Imports `SPI`, or a framework member its documentation leaves out |

Core modules take no skill of their own, as the module table of `swift:coding` names the field each module serves and its references name their forms.

## [02]-[SOURCES]

Swift targets read the standard library, each core module family of `swift:coding`, the SDK, and the repository SPI every session whatever they import, at the release the build resolves, as most hand-written Swift in this tree restates one of them. Package reads, one command per step:
1. Version: Xcode, Swift, and SDK the build selects, and each pin of root `Package.resolved`, never a `from:` range of `Package.swift`
2. Guide: agent guide of a package checkout, else its DocC root article with topic groups, as a checkout ships DocC catalogs and no `AGENTS.md`
3. Language: `Swift`, `_Concurrency`, `_StringProcessing`, `RegexBuilder`, `Synchronization`, and `Observation`
4. Collections: `OrderedCollections` and `DequeModule` from `$B`, then `HeapModule`, `HashTreeCollections`, and `BitCollections` from checkout source
5. Async sequences: `AsyncAlgorithms`
6. Enum cases: `CasePaths` and `CasePathsCore`
7. Files and processes: SDK `System` and `Subprocess`
8. Settings and lifecycle: `Configuration`, `ServiceLifecycle`, and `UnixSignals`
9. Observability: `Logging`, `Tracing`, `Instrumentation`, `OTel`, `OTelSemanticConventions`, and `SystemMetrics`
10. Updates: `Sparkle`, a binary framework the extractor reads from `$B`
11. Command lines: `ArgumentParser` for analyzer targets, from the analyzer's own build
12. SDK: `Foundation`, `AppKit`, `Darwin`, and each further framework the target imports
13. SPI: `libs/swift/macos/SPI/SPI.h` read whole, its Swift view, then the export set of the framework its module map links
14. Workspace: modules `<Module>` depends on with their folders from the Nx graph, each read whole and at its `package` level
15. Candidates: dependencies, pin products, SDK frameworks, Swift modules, and SPI listed whole, each module doing a target job read deeply and added

```bash
xcodebuild -version && swift --version && xcrun --show-sdk-version && jq -r '.pins[] | "\(.identity)\t\(.state.version // .state.revision[0:8])"' Package.resolved
fd -d 2 -i '^(agents\.md|llms.*\.txt)$' "$D/SourcePackages/checkouts/<package>"; rg -l --no-ignore '^# ``<Module>``' "$D/SourcePackages/checkouts/<package>" -g '*.md' | xargs rg -n '^#{2,3} '
for m in Swift _Concurrency _StringProcessing RegexBuilder Synchronization Observation; do sg $m public; done
for m in OrderedCollections DequeModule; do sg $m public; done
rg -n '\bpublic (\w+ )*(func|var|let|init|subscript|struct|enum|class|protocol|typealias|case)\b' "$D/SourcePackages/checkouts/swift-collections/Sources/<Module>"
sg AsyncAlgorithms public
for m in CasePaths CasePathsCore; do sg $m public; done
for m in System Subprocess; do sg $m public; done
for m in Configuration ServiceLifecycle UnixSignals; do sg $m public; done
for m in Logging Tracing Instrumentation OTel OTelSemanticConventions SystemMetrics; do sg $m public; done
sg Sparkle public
B=products/developer/nx/apple/analyzer/.build/out/Products/Debug MM= sg ArgumentParser public
for m in Foundation AppKit Darwin; do sg $m public; done
xcrun swift-synthesize-interface -module-name SPI -sdk "$(xcrun --show-sdk-path)" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -I libs/swift/macos/SPI
F=$(rg -oN 'link framework "(\w+)"' -r '$1' libs/swift/macos/SPI/module.modulemap) && dyld_info -exports "/System/Library/PrivateFrameworks/$F.framework/$F"
nx graph --file=<dir>/graph.json && jq -r --arg p '<Module>' '.graph as $g | $g.dependencies[$p][].target | "\(.)\t\($g.nodes[.].data.root)"' <dir>/graph.json
sed -n '/^    dependencies: \[/,/^    \]/p' Package.swift
for p in $(jq -r '.pins[].identity' Package.resolved); do swift package --package-path "$D/SourcePackages/checkouts/$p" --scratch-path <dir>/spm dump-package |
    jq -r --arg p "$p" '.products[] | "\($p)\t\(.name)\t\(.targets | join(","))"'; done
ls "$(xcrun --show-sdk-path)/System/Library/Frameworks" "$(xcrun --show-sdk-path)/usr/lib/swift" "$(xcrun --show-sdk-platform-path)/Developer/Library/Frameworks" libs/swift/macos/SPI
```

Reads hold under these conditions:
- Built: `sg` reads modules a scheme target builds, so a collections module no target depends on reads from its `public` source lines and DocC
- Synthesis: `swift-synthesize-interface` with `-module-name <Module> -include-submodules` prints an SDK module as Swift where a row lacks a signature
- Overlays: overlay `.swiftinterface` files under a framework hold Swift-only members synthesis omits
- Products: `dump-package` takes `--scratch-path`, as a run without it writes `.build` into the checkout
- Adoption: module from a pin no target names joins through its `Package.swift` product row, then reads through `sg` after the next build

## [03]-[MAPPING]

Maps list what each module of the target declares, at which access level, and who reads it, one command per step, each run once:
1. Levels: declarations per module at each access level, top-level apart, from the symbol graph of the last `nx build Rasm-Package`
2. Wide access: `package` declarations no file outside their module names, each an `internal` candidate the build confirms
3. Unread: declarations whose name the tree spells once, at the declaration itself
4. Protocols: conformance sites of each repository protocol, a count of 1 being a protocol with one conformer
5. Constants: module-scope and `static let` values, each traced to its reader and the source that should state it
6. Name families: names differing by a builder word, an affix, a plural, or an initialism's case, each family read whole
7. Suites: test files reading `internal` members through `@testable import`
8. Dependents: modules and apps depending on `<Module>`, from `<dir>/graph.json` that `nx graph` writes
9. Text: sites spelling a member, key, host constant, or message a declaration holds, and mentions in docs, skills, and manifests
10. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
11. Nesting: deepest block per member from the compiler's parse tree, deepest first

```bash
sg <Module> private && awk -F'\t' '{ n[$3]++; if ($1 !~ /\./) t[$3]++ } END { for (a in n) print a "\t" n[a] "\ttop " t[a] + 0 }' <dir>/sg/<Module>.tsv
P=$(nx show project <Module> --json | jq -r .root) && awk -F'\t' 'NR == FNR { w[$0]; next } !($1 in w) { print $2 "\t" $3 }' \
    <(rg -ow --no-filename -g '*.swift' -g "!$P/**" '[A-Za-z_]\w*' <roots> | sort -u) <(jq -r --arg r "file://$PWD/" '.symbols[] | select(.accessLevel == "package" and .location != null)
    | [(.pathComponents[-1] | sub("\\(.*"; "")), (.pathComponents | join(".")), "\(.location.uri | ltrimstr($r)):\(.location.position.line + 1)"] | @tsv' <dir>/sg/<Module>/*.symbols.json)
awk -F'\t' 'NR == FNR { c[$2] = $1; next } c[$1] == 1' <(rg -ow --no-filename -g '*.swift' '[A-Za-z_]\w*' <roots> | sort | uniq -c | awk '{ print $1 "\t" $2 }') \
    <(jq -r '.symbols[] | select(.accessLevel != "public" and (.kind.identifier | test("init|subscript|deinit|op$") | not))
    | [(.pathComponents[-1] | sub("\\(.*"; "")), .accessLevel, (.pathComponents | join("."))] | @tsv' <dir>/sg/<Module>/*.symbols.json) | sort -u
ast-grep outline $(git ls-files <roots> | rg '\.swift$') --items structure --json=stream | jq -r '.items[] | select(.symbolType == "interface") | .name' | sort -u | while read -r p; do
    printf '%s\t%s\n' "$(rg -c --no-filename "^\s*(\w+ )*(struct|enum|class|actor|extension) [\w.]+(<[^>]*>)?:[^{]*\b$p\b" -g '*.swift' <roots> | awk '{ s += $1 } END { print s + 0 }')" "$p"; done | sort -n
rg -n '^(\w+ )*let \w+|^\s+(@\w+ )*(\w+ )*static let \w+' <paths>
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r 'select(.path | endswith(".swift")) | .items[] | select(.isImport | not) | .name, (.members[]?.name | sub("^.*\\."; ""))' |
    sort -u | awk '{ k = $1; if (k ~ /^(make|create|build|new|run|to|from|get|with)[A-Z]/) sub(/^(make|create|build|new|run|to|from|get|with)/, "", k); k = tolower(k); sub(/(of|for|with|from|s|es)$/, "", k)
    v = tolower(substr($1, 1, 1)) substr($1, 2); sub(/s$/, "", v); if (!((k, v) in seen)) { seen[k, v]; d[k]++ } g[k] = g[k] " " $1 } END { for (k in g) if (d[k] > 1) print g[k] }'
rg -l '@testable import <Module>\b' tests
jq -r --arg p '<Module>' '.graph.dependencies | to_entries[] | select(any(.value[]; .target == $p)) | .key' <dir>/graph.json
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format swift --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
xcrun swiftc -dump-parse -swift-version 6 <paths> 2>&1 | awk '{ match($0, /^ */); i = RLENGTH; while (n && b[n] >= i) n--; while (m && d[m] >= i) m-- }
    /^ *\((func|constructor|accessor)_decl / { d[++m] = i; match($0, /"[^"]*"|for="[^"]*"/); f[m] = substr($0, RSTART, RLENGTH) }
    /^ *\(closure_expr .*single_expression/ { e = 1 }
    /^ *\((if|guard|for_each|while|repeat_while|switch|do|do_catch)_stmt / { w[n] = 1 }
    /^ *\(brace_stmt / { b[++n] = i; w[n] = !e; e = 0; t = 0; for (j = 1; j <= n; j++) t += w[j]
        if (t > 2 && match($0, /range=\[[^:]+:[0-9]+/)) { r = substr($0, RSTART + 7, RLENGTH - 7); k = f[m] "\t" r; sub(/:[0-9]+$/, "", k)
            if (t - 1 > x[k]) { x[k] = t - 1; s[k] = r } } }
    END { for (k in x) print x[k] "\t" s[k] "\t" substr(k, 1, index(k, "\t") - 1) }' | sort -rn
```

Commands read by their own rules:
- Levels: libraries write no `public`, so `package` is their export level and graph `public` rows are macro output (`@CasePathable`'s `AllCasePaths`)
- Graph: private declarations carry no source location, so a per-file count of them comes from the outline and the graph counts per module
- Dropped: symbol graphs leave out a `package` protocol refining a protocol of a `package import` module (`ModuleError`), so step 4 reads source
- Build: symbol graphs read the last build, and a module the extractor aborts on below `public` (`JSONRPC`) leaves its map to the outline
- Wide access: compiler refuses a narrowing a wider signature exposes, so each printed row is a lead the build of the narrowed form proves
- Unread: macro and wrapper storage (`_name`, `_$observationRegistrar`, `$name`) has no source text, so a count of 1 keeps hand declarations alone
- Witnesses: stored fields a synthesized coder reads, cases a raw value decodes, and members a host calls by protocol have readers no text shows
- Readers: no reference index runs outside SourceKit, so `rg -nw` finds candidate sites and a build of the narrowed or removed form names each reader
- Families: type and value sharing a word (`kind: Kind`) follow Swift convention and print nothing, so each group is one concept spelled apart
- Outline: tree-sitter fails on constructs (`where` on an `associatedtype`, `catch` pattern lists) and prints an `ERROR` name, its span read whole
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: no repository checker measures logic depth, as swiftlint `nesting` counts nested declarations alone
- Blocks: each `{}` block below a member body is a level (branch, `guard` else, loop, `do`, `catch`, `switch` case), an `else if` chain one
- Closures: single-expression closure counts no level unless its own body holds a branch, as a branch in a closure nested inside it counts there
- Parse: `-dump-parse` reads syntax with no imports or build, `-swift-version 6` admits bare regex literals, and output goes to stderr

## [04]-[MECHANISMS]

Swift states a concept once through its type: synthesis and attached macros write the members a declaration implies, enums hold exclusive states, extensions attach operations to the type they act on, and access levels narrow each name to its readers. Root `Package.swift` decides which forms compile: `ExistentialAny` requires `any` on every existential, `InternalImportsByDefault` keeps imports internal, `MemberImportVisibility` resolves a member only through a module the file imports, warnings are errors, and app module `Relay` takes `defaultIsolation(MainActor.self)`. Most reductions here delete a member the compiler or a macro writes, or move a free form onto its type.

[NAMES]:
- Initializers: type is its own constructor, so `init` overloads, failable `init?`, and `init(_:) throws(E)` replace builders returning `Self`
- Labels: argument labels state each parameter's role (`events(of:)`, `events(of:eventMask:)`), so a base name joins no preposition or noun
- Nested types: parts of a type nest under it (`FileFailure.Reason`, `InstanceState.Activity`), one exported name holding them
- Implicit members: static members and cases read as `.name` where the type is known, so a type needs no global constant beside it

[SYNTHESIS]:
- Memberwise: structs take an `init` over stored fields at `internal` or narrower, and an `init` in an extension keeps it
- Codable: synthesized coders read `CodingKeys`, whose raw values hold wire names apart from field names and whose subset leaves fields out
- Strategies: `keyEncodingStrategy` and `keyDecodingStrategy` convert every key a coder writes, in place of a raw value per key
- Conformances: `Equatable`, `Hashable`, `Comparable` for enums, and `CaseIterable.allCases`, also in an extension inside the type's own file
- Raw values: `String` enum's raw value defaults to its case name, and `RawRepresentable` over a `Codable` raw value codes through it
- Conditional: `extension X: P where T: P {}` synthesizes `P` for each generic argument that holds it (`Lenient: Hashable`)

[CASES]:
- Sums: enums with associated values hold exclusive states, `indirect` cases hold recursion, and a `switch` missing a new case fails to compile
- Uninhabited: `Never` removes a case at its type (`Result<T, Never>`, `throws(Never)`), in place of a case no value reaches
- Case paths: `@CasePathable` writes case reads (`.is(\.x)`, `[case: \.x]`), and `@dynamicMemberLookup` reads a case as a property
- Matching: tuple subjects decide over many values, `case let` binds in a condition list, and `where` holds a test one arm reads

[EXTENSIONS]:
- Protocol extensions: default member written once serves every conformer (`ModuleError.errorDescription`, `CaseTagged.encode(to:)`)
- Constraints: `extension Sequence<UInt8>` and `where` clauses add members to every matching type, a conformance adapting a host type
- Placement: members sit on the type they act on, in the file of their concept, in place of free functions taking the type first
- Markers: protocol with a defaulted `associatedtype RawValue = Never where RawValue == Never` refuses raw-value enums (`CaseTagged`)

[ACCESS]:
- Levels: `private` reaches its declaration and same-file extensions, `fileprivate` its file, `internal` its module, `package` its package
- Setters: `private(set)` and `package private(set)` keep a read wide and its write narrow, in place of a computed property over private storage
- Imports: `package import` names a module only where a `package` signature states its type, and `@testable import` reaches `internal` from suites

[EFFECTS]:
- Failure: `throws(E)` states one module error, `throws(Never)` none, and a generic `E` lets a higher-order function rethrow its closure's error
- Isolation: `actor` holds state its updates suspend over, `Mutex` and `Atomic` synchronous state, `async let` and task groups child work
- Default isolation: `Relay` declarations run on the main actor, and `nonisolated` on a type covers its members
- Ownership: structs and enums copy, a class holds identity alone, `~Copyable` holds a single-owner handle, `sending` moves a value across regions

[TYPES]:
- Generics: `some P<Assoc>` for one concrete type per call or result, `any P<Assoc>` for mixed values, parameter packs for many distinct types
- Key paths: act as functions (`map(\.name)`), `KeyPathComparator` states a sort, and a `WritableKeyPath` parameter replaces a setter per field
- Wrappers: `@propertyWrapper` types and accessor macros state one get and set pair for every property using them, `$name` reading its projection
- Builders: `@resultBuilder` types (`ViewBuilder`) state a declarative tree as statements, in place of arrays built by hand

Host rules bind where the compiler checks the code, each proved by a probe or a build of root `Package.swift`:
- Coders: hand `init(from:)` keeps the synthesized `encode(to:)` over the same `CodingKeys`, so a hand encoder matching it goes alone
- Enums: synthesized `Codable` writes `{"<case>": {"<label>": ..., "_1": ...}}` and `{"<case>": {}}`, a shape apart from `CaseTagged`'s `_tag` field
- Observable: synthesized `Codable` of an `@Observable` class writes `_<field>` keys and warns on `_$observationRegistrar`, failing the build
- Observable keys: `CodingKeys` of `case _<field> = "<field>"` rows keep synthesis for an `@Observable` class with wire names and no warning
- Snake case: `.convertToSnakeCase` writes `traceID` as `trace_id` and leaves `[String: V]` keys as written
- Chains: root `Package.swift` fails an expression past 200 type-check scopes, so a flattened chain over `Result` and optionals states closure types
- Case paths: `[case:]` subscript on a `Result` with a tuple success type crashes IR generation with no `error:` line, a non-tuple success passing
- Isolation: `swiftc -typecheck` passes region isolation and noncopyable move errors that `xcodebuild` reports, so only a build proves a pass

## [05]-[PATTERNS]

Patterns common in Swift, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]            | [LOOK_FOR]                                                     | [REBUILD]                                    |
| :-----: | :------------------- | :------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Flag records         | `Bool` fields beside optionals one mode sets                   | Case per state, facts as its values          |
|  [02]   | Case accessors       | `isX` and `x: T?` per case, `if case` repeated                 | `@CasePathable`, `.is(\.x)`, `[case: \.x]`   |
|  [03]   | Hand coders          | Coder matching synthesis, raw value per key restating one rule | Synthesis, `CodingKeys` names, key strategy  |
|  [04]   | Memberwise copies    | `init` assigning each stored field of an `internal` struct     | Memberwise `init`, others in an extension    |
|  [05]   | Protocol per type    | Protocol with one conformer, `any P` field holding one type    | Concrete type, `some P<Assoc>` where generic |
|  [06]   | Function namespaces  | Free functions taking a type first, caseless `enum` of statics | Members in an extension of that type         |
|  [07]   | Host tokens          | `let` renaming a host value, `Set<String>` of `kAX…` constants | Host constant, enum classifying once         |
|  [08]   | Hand synchronization | Class around a lock, `@unchecked Sendable`, queue sync         | `actor`, `Mutex`, `async let`, task group    |
|  [09]   | Untyped failure      | `throws`, `any Error`, or `try?` beside a module error type    | `throws(E)` over the module error            |
|  [10]   | Unread declarations  | Name spelled once, macro output no reader reads                | Lead, removed where it restates a source     |
|  [11]   | Name spellings       | `makeX` returning `Self`, `processOfSerial`, `accountId`       | `init`, one base name with labels, `ID`      |
|  [12]   | Statement towers     | `if let` in `if let`, `do` per host call, `switch` in `switch` | Condition list, `do throws(E)`, tuple arm    |

Patterns hold under these conditions:
- Row [01] -> `Bool` a wire or host format states stays a field of that boundary type, decoded into the case once
- Row [03] -> hand coders stay where a stored type lacks `Codable`, a field derives from a host value, or another type decodes from its container
- Row [03] -> wire shapes apart from synthesis (`_tag` fields) take `CaseTagged`, every reader of a changed wire rebuilt in one pass
- Row [03] -> key strategy holds where one coder serves every type of that wire, and a shared coder keeps per-key raw values
- Row [04] -> `package` struct another module constructs, a class, an actor, or a label or default memberwise lacks keeps its `init`
- Row [05] -> protocol stays where another module fills it (`LockFailure`), generic code constrains on it, or its extension serves every conformer
- Row [06] -> generic functions over closures (`race`, `withTimeout`, `combine`) stay free, as no type owns them
- Row [07] -> value the SDK headers lack (`"AXFrame"`) stays one constant, file-private where one file reads it
- Row [08] -> `Mutex` fields inside an actor stay where host callbacks read them off the actor, `withCheckedContinuation` at one callback boundary
- Row [10] -> declaration renaming a host value (`sectionsAttribute`), repeating another, or generated with no reader goes with its readers moved
- Row [10] -> declaration stating its own fact stays, rebuilt to its source: wire case a host decodes, witness a host calls, planned capability
- Row [11] -> `make` a protocol requirement names (`makeIterator`, `makeAsyncIterator`, `makeNSView`) stays, as its host calls it by name
- Row [11] -> wire keys stay exact as `CodingKeys` raw values, and a host's own spelling stays in the call reading it

## [06]-[CHECK]

Check per pass, each command alone in the background, one Xcode build at a time on this machine:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx test Rasm-Package -- -only-testing:<Target>[/<Suite>]
```

- Autofix: swiftlint's fix rewrites `catch let name` to `catch` and `Type.init(` to `Self.init(`, so each file is read again after `hk fix`
- Build: `check` builds `Rasm-Package` and each app reading the module in `xcodebuild`
- Log: build logs read at "The following build commands failed" and "Please submit a bug report", never at `error:` lines alone
- Stale: failed module leaves its importers on a stale interface, so their errors can be false until its first cause is fixed
- Suites: pass changing a suite or code under test -> test line naming its target, its other forms in `swift:testing`
- Scope: root project `rasm` runs repository-wide checks (knip, jscpd) that fail on other work in a shared tree, so a pass excludes it
- Analyzer: `products/developer/nx/apple/analyzer` is a SwiftPM root outside `Rasm-Package`, checked as `@rasm/nx-apple-darwin-arm64`
