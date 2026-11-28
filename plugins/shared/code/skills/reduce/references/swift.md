# [SWIFT]

Swift targets hold `.swift` files of root `Package.swift` modules, which `xcodebuild` builds through `Rasm.xcworkspace`, and of nested SwiftPM root `products/developer/nx/apple/analyzer`, `<Module>` naming a target as `Package.swift` and `nx show projects` print it, `<package>` its checkout folder, `<paths>` files a pass touches, `<dir>` session scratch folder, `<roots>` `libs products tests`, `$D` this checkout's Xcode build folder, and `$MM` module map arguments a built module's symbol graph reads:

```bash
D=$(dirname "$(rg -l -a -F "$PWD/Rasm.xcworkspace<" ~/Library/Developer/Xcode/DerivedData/Rasm-*/info.plist)")
MM=$(fd -e modulemap . "$D/SourcePackages/checkouts" libs/swift/macos/SPI "$D/Build/Intermediates.noindex/GeneratedModuleMaps" | sed 's/^/-Xcc -fmodule-map-file=/')
```

## [01]-[SKILLS]

| [INDEX] | [TARGET]          | [SKILLS]                      | [CONDITION]                                                       |
| :-----: | :---------------- | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | Any Swift         | `swift:coding`                | `.swift` file                                                     |
|  [02]   | Tests             | `swift:testing`, `code:test`  | Test target under `tests/libs/swift`, analyzer `Tests/`           |
|  [03]   | SDK or SPI member | `tool:inspect`, `search:code` | Imports `SPI`, or a framework member its documentation leaves out |

Packages take no skill of their own, as the module table of `swift:coding` names the field each package serves and its rules name their forms.

## [02]-[SOURCES]

Swift targets read the standard library, the SDK, and their package checkouts at the release the build resolves, as most hand-written code in this tree restates one of them. Package reads, one command per step:
1. Version: Xcode, Swift, and SDK the build selects, and each pin of root `Package.resolved`, never a `from:` range of `Package.swift`
2. Guide: agent guide of a package checkout, else its DocC root article with topic groups, as a checkout ships DocC catalogs and no `AGENTS.md`
3. Language: symbol graph of `Swift`, `_Concurrency`, `Synchronization`, or `Observation`, a row per symbol with kind and first doc line
4. SDK: symbol graph of an SDK module (`System`, `Foundation`, `AppKit`) through the same command, `<Module>@<Other>` extension files included
5. Package: symbol graph of a built package module at `public` from `$D`, one `.tsv` per module
6. SPI: `libs/swift/macos/SPI/SPI.h` read whole, its Swift view, then the export set of the framework its module map links
7. Workspace: modules `<Module>` depends on with their folders, from the Nx graph, each read whole
8. Catalog: package outside `Package.resolved` read through `search:code` at its newest tag, as no checkout holds it

```bash
xcodebuild -version && swift --version && xcrun --show-sdk-version && jq -r '.pins[] | "\(.identity)\t\(.state.version // .state.revision[0:8])"' Package.resolved
fd -d 2 -i '^(agents\.md|llms.*\.txt)$' "$D/SourcePackages/checkouts/<package>"; rg -l --no-ignore '^# ``<Module>``' "$D/SourcePackages/checkouts/<package>" -g '*.md' | xargs rg -n '^#{2,3} '
mkdir -p <dir>/sdk && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -minimum-access-level public -skip-synthesized-members -output-dir <dir>/sdk &&
    jq -r '.symbols[] | [(.pathComponents | join(".")), .kind.identifier, (.docComment.lines[0].text // "")] | @tsv' <dir>/sdk/<Module>*.symbols.json > <dir>/sdk/<Module>.tsv
mkdir -p <dir>/pkg && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -I "$D/Build/Products/Debug" -F "$D/Build/Products/Debug" $MM -minimum-access-level public -skip-synthesized-members -output-dir <dir>/pkg &&
    jq -r '.symbols[] | [(.pathComponents | join(".")), .kind.identifier, (.docComment.lines[0].text // "")] | @tsv' <dir>/pkg/<Module>*.symbols.json > <dir>/pkg/<Module>.tsv
xcrun swift-synthesize-interface -module-name SPI -sdk "$(xcrun --show-sdk-path)" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -I libs/swift/macos/SPI
F=$(rg -oN 'link framework "(\w+)"' -r '$1' libs/swift/macos/SPI/module.modulemap) && dyld_info -exports "/System/Library/PrivateFrameworks/$F.framework/$F"
nx graph --file=<dir>/graph.json && jq -r --arg p '<Module>' '.graph as $g | $g.dependencies[$p][].target | "\(.)\t\($g.nodes[.].data.root)"' <dir>/graph.json
```

Synthesized module text (the SPI line with `-module-name <Module> -include-submodules` in place of `-I`) reads an SDK module as Swift declarations where a symbol graph row lacks the signature, and an overlay's `.swiftinterface` under its framework holds the Swift-only members synthesis omits.

## [03]-[MAPPING]

Maps list what each module of the target declares, at which access level, and who reads it, one command per step, each run once:
1. Levels: declarations per module at each access level, top-level apart, from the symbol graph of the last `nx build Rasm-Package`
2. Wide access: `package` declarations no file outside their module names, each an `internal` candidate the build confirms
3. Unread: declarations whose name appears once across `<roots>`, the declaration itself
4. Name families: names differing by a builder word, an affix, a plural, or an initialism's case, each family read whole
5. Suites: test files reading `internal` members through `@testable import`
6. Dependents: modules and apps depending on `<Module>`, from `<dir>/graph.json` that `nx graph` writes
7. Text: sites spelling a member, key, host constant, or message a declaration holds, and mentions in docs, skills, and manifests
8. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
9. Nesting: deepest block per member from the compiler's parse tree, deepest first

```bash
mkdir -p <dir>/sg && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -I "$D/Build/Products/Debug" -F "$D/Build/Products/Debug" $MM -minimum-access-level private -skip-synthesized-members -output-dir <dir>/sg &&
    jq -rs '[.[].symbols[]] | group_by(.accessLevel)[] | "\(.[0].accessLevel)\t\(length)\ttop \([.[] | select(.pathComponents | length == 1)] | length)"' <dir>/sg/<Module>*.symbols.json
P=$(nx show project <Module> --json | jq -r .root) && awk -F'\t' 'NR == FNR { w[$0]; next } !($1 in w) { print $2 "\t" $3 }' \
    <(rg -ow --no-filename -g '*.swift' -g "!$P/**" '[A-Za-z_]\w*' <roots> | sort -u) <(jq -r --arg r "file://$PWD/" '.symbols[] | select(.accessLevel == "package" and .location != null)
    | [(.pathComponents[-1] | sub("\\(.*"; "")), (.pathComponents | join(".")), "\(.location.uri | ltrimstr($r)):\(.location.position.line + 1)"] | @tsv' <dir>/sg/<Module>*.symbols.json)
awk -F'\t' 'NR == FNR { c[$2] = $1; next } c[$1] < 2' <(rg -ow --no-filename -g '*.swift' '[A-Za-z_]\w*' <roots> | sort | uniq -c | awk '{ print $1 "\t" $2 }') \
    <(jq -r '.symbols[] | select(.accessLevel != "public" and (.kind.identifier | test("init|subscript|deinit|op$") | not) and (.pathComponents[-1] | startswith("$") | not))
    | [(.pathComponents[-1] | sub("\\(.*"; "")), .accessLevel, (.pathComponents | join("."))] | @tsv' <dir>/sg/<Module>*.symbols.json) | sort -u
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
    /^ *\((if|guard|for_each|while|repeat_while|switch|do|do_catch)_stmt / { for (j = 1; j <= n; j++) w[j] = 1 }
    /^ *\(brace_stmt / { b[++n] = i; w[n] = !e; e = 0; t = 0; for (j = 1; j <= n; j++) t += w[j]
        if (t > 2 && match($0, /range=\[[^:]+:[0-9]+/)) { r = substr($0, RSTART + 7, RLENGTH - 7); k = f[m] "\t" r; sub(/:[0-9]+$/, "", k)
            if (t - 1 > x[k]) { x[k] = t - 1; s[k] = r } } }
    END { for (k in x) print x[k] "\t" s[k] "\t" substr(k, 1, index(k, "\t") - 1) }' | sort -rn
```

Commands read by their own rules:
- Levels: libraries write no `public`, so `package` is their export level and graph `public` rows are macro output (`@CasePathable`'s `AllCasePaths`)
- Graph: private declarations carry no source location, so a per-file count of them comes from the outline and the graph counts per module
- Build: symbol graphs read the last build, and an extractor aborting below `public` leaves that module to outline signatures alone
- Wide access: the compiler refuses a narrowing a wider signature exposes, so each printed row is a lead the build of the narrowed form proves
- Unread: stored fields a synthesized coder reads, cases a raw value decodes, and witnesses a host calls (delegate methods) have readers no text shows
- Families: type and value sharing a word (`kind: Kind`) follow Swift convention and print nothing, so each group is one concept spelled apart
- Outline: tree-sitter fails on constructs (`where` on an `associatedtype`, `catch` pattern lists) and prints an `ERROR` name, its span read whole
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: no repository checker measures logic depth, as swiftlint `nesting` counts nested declarations alone
- Blocks: each `{}` block below a member body is a level (branch, `guard` else, loop, `do`, `catch`, `switch` case), an `else if` chain one
- Parse: `-dump-parse` reads syntax with no imports or build, `-swift-version 6` admits bare regex literals, and output goes to stderr

## [04]-[MECHANISMS]

Swift states a concept once through its type: synthesis writes the members a declaration implies, enums hold exclusive states, extensions attach operations to the type they act on, and access levels narrow each name to its readers. Most reductions here delete a member the compiler or a macro writes, or move a free form onto its type:
- One name: a type is its own constructor, so `init` overloads and failable `init?` replace `makeX`, `XFrom`, and `parseX` builders
- Labels: argument labels and overloads distinguish forms of one operation (`events(of:)`, `events(of:eventMask:)`), never a suffix in the base name
- Nested types: a type's parts nest under it (`FileFailure.Reason`, `InstanceState.Activity`), one exported name carrying them, never a prefixed name
- Implicit members: static members and cases read as `.name` where the type is known, so a type needs no global constant beside it
- Synthesis: memberwise `init`, `Codable` over `CodingKeys`, `Equatable`, `Hashable`, `Comparable` for enums, and `CaseIterable.allCases`
- Raw values: a `String` enum's raw value defaults to its case name, and `CodingKeys` raw values hold wire names apart from field names
- Sums: enums with associated values hold exclusive states, `indirect` cases hold recursion, and an exhaustive `switch` fails to compile on a new case
- Macros: `@CasePathable` writes case reads (`.is(\.x)`, `[case: \.x]`), `@Observable` writes observation in place of `@Published` members
- Protocol extensions: a default member written once serves every conformer (`ModuleError.errorDescription`, `CaseTagged.encode(to:)`)
- Constraints: `extension Sequence<UInt8>` and `where` clauses add members to every matching type, retroactive conformance adapting a host type
- Extensions: members on the type they act on, in the file of their concept, in place of free functions taking the type first
- Generics: `some P<Assoc>` for one concrete type per call, `any P<Assoc>` for mixed values, parameter packs for many distinct types
- Access: `private` reaches its declaration and same-file extensions, `fileprivate` its file, `internal` its module, `package` its package
- Imports: `InternalImportsByDefault` keeps imports internal, and `package import` names a module only where a `package` signature states its type
- Failure: `throws(E)` states one module error, `throws(Never)` none, and a generic `E` lets a higher-order function rethrow its closure's error
- Values: structs and enums copy, a class holds identity alone, and `~Copyable` holds a single-owner handle
- Isolation: `actor` holds state its updates suspend over, `Mutex` and `Atomic` synchronous state, `async let` and task groups child work
- Expressions: `if` and `switch` expressions bind one value, a tuple subject decides over many, key paths act as functions (`map(\.name)`)
- Builders: `@resultBuilder` types (`ViewBuilder`) state a declarative tree as statements, in place of arrays built by hand

Host rules bind where the compiler checks the code, each proved by a probe or a build of root `Package.swift`:
- Coders: a hand `init(from:)` keeps the synthesized `encode(to:)` over the same `CodingKeys`, so a hand encoder matching it goes alone
- Enums: synthesized `Codable` of a case with associated values writes `{"<case>": {<labels>}}`, a shape apart from `CaseTagged`'s `_tag` field
- Chains: root `Package.swift` fails an expression past 200 type-check scopes, so a flattened chain over `Result` and optionals states closure types
- Case paths: a `[case:]` subscript on a `Result` with a tuple success type crashes IR generation with no `error:` line, a non-tuple success passing
- Isolation: `swiftc -typecheck` passes region isolation and noncopyable move errors that `xcodebuild` reports, so only a build proves a pass

## [05]-[PATTERNS]

Patterns common in Swift, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]             | [LOOK_FOR]                                                     | [REBUILD]                                    |
| :-----: | :-------------------- | :------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Flag records          | `Bool` fields beside optionals one mode sets                   | Case per state, facts as its values          |
|  [02]   | Case accessors        | `isX` and `x: T?` per case, `if case` repeated                 | `@CasePathable`, `.is(\.x)`, `[case: \.x]`   |
|  [03]   | Hand coders           | `CodingKeys` and a hand coder matching synthesis               | Synthesis, wire names in `CodingKeys`        |
|  [04]   | Protocol per type     | Protocol with one conformer, `any P` field holding one type    | Concrete type, `some P<Assoc>` where generic |
|  [05]   | Function namespaces   | Free functions taking a type first, caseless `enum` of statics | Members in an extension of that type         |
|  [06]   | Host constant renames | `let` renaming a host value, `String` for a typed host name    | Host constant at its reader                  |
|  [07]   | Hand synchronization  | Class around a lock, `@unchecked Sendable`, queue sync         | `actor`, `Mutex`, `async let`, task group    |
|  [08]   | Untyped failure       | `throws`, `any Error`, or `try?` beside a module error type    | `throws(E)` over the module error            |
|  [09]   | Wide access           | `package` member or import its module alone needs              | `internal`, plain import                     |
|  [10]   | Unread declarations   | Module constant or member no other line names                  | Removed                                      |
|  [11]   | Name spellings        | `makeX`, `XFrom`, `fooForBar`, `accountId` beside `traceID`    | `init`, labels on one name, `ID` initialism  |
|  [12]   | Statement towers      | `if let` in `if let`, `do` per host call, `switch` in `switch` | Condition list, `do throws(E)`, tuple arm    |

Patterns hold under these conditions:
- Row [01] -> a `Bool` a wire or host format states stays a field of that boundary type, decoded into the case once
- Row [03] -> hand coders stay where a stored type lacks `Codable` or a field derives from a host value (`InstanceSnapshot`)
- Row [03] -> wire shapes apart from synthesis (`_tag` fields) take `CaseTagged`, every reader of a changed wire rebuilt in one pass
- Row [04] -> a protocol stays where another module fills it (`LockFailure`), generic code constrains on it, or its extension serves every conformer
- Row [05] -> generic functions over closures (`race`, `withTimeout`, `combine`) stay free, as no type owns them
- Row [06] -> a value the SDK headers lack (`"AXFrame"`) stays one constant, file-private where one file reads it
- Row [07] -> `Mutex` fields inside an actor stay where host callbacks read them off the actor, `withCheckedContinuation` at one callback boundary
- Row [09] -> a member a `package` signature exposes keeps its level, and suites reading a narrowed member take `@testable import`
- Row [10] -> the compiler warns of unused locals alone, so module constants and members nobody reads compile clean until the unread map finds them
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
