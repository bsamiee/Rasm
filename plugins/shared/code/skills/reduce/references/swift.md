# [SWIFT]

Swift targets hold `.swift` files of root `Package.swift` modules, which `xcodebuild` builds through `Rasm.xcworkspace`, and of nested SwiftPM root `products/developer/nx/apple/analyzer`, `<Module>` naming a target as `Package.swift` and `nx show projects` print it, `<paths>` files a pass touches, `<dir>` session scratch folder, `<roots>` `libs products tests`, `$D` this checkout's Xcode build folder, and `$MM` module map arguments a built module's symbol graph reads:

```bash
D=$(dirname "$(rg -l -a -F "$PWD/Rasm.xcworkspace<" ~/Library/Developer/Xcode/DerivedData/Rasm-*/info.plist)")
MM=$(fd -e modulemap . "$D/SourcePackages/checkouts" libs/swift/macos/SPI "$D/Build/Intermediates.noindex/GeneratedModuleMaps" | sed 's/^/-Xcc -fmodule-map-file=/')
```

## [01]-[SKILLS]

| [INDEX] | [TARGET]  | [SKILLS]        | [CONDITION]                                          |
| :-----: | :-------- | :-------------- | :--------------------------------------------------- |
|  [01]   | Any Swift | `swift:coding`  | `.swift` file                                        |
|  [02]   | Tests     | `swift:testing` | Test target under `tests/libs/swift`, analyzer suite |

Packages take no skill of their own, as `swift:coding`'s module table names the field each package serves and its rules name their forms.

## [02]-[SURVEY]

Signals over `<paths>`, one command per step:
1. Size: lines and branching per file, largest first
2. Clones: jscpd AST pairs across `<roots>` touching the target, ranked by lines times similarity
3. Dependents: modules and apps depending on `<Module>`, each module an Nx project of one graph file
4. Levels: declarations per file at `private`, `internal`, `package`, and `public` from outline signatures, type members and extensions included
5. Exports: compiler-exact counts per module from its symbol graph over a current build, top-level declarations apart
6. Private readers: `private` and `fileprivate` declarations with one reader in their file
7. Name pairs: names differing by more than their first letter's case, each group judged
8. Nesting: deepest block per member from the compiler's parse tree, deepest first

```bash
jscpd --config .config/jscpd.json --complexity --reporters json --output <dir>/jscpd <paths> &&
    jq -r --arg r "$PWD/" '.summary.files[] | [.lines, .complexity, (.path | ltrimstr($r))] | @tsv' <dir>/jscpd/jscpd-complexity.json | sort -rn
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format swift --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
nx graph --file=<dir>/graph.json && jq -r --arg p '<Module>' '.graph.dependencies | to_entries[] | select(any(.value[]; .target == $p)) | .key' <dir>/graph.json
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '
    def stated: [.signature // "" | scan("\\b(private|fileprivate|internal|package|public|open)\\b(?!\\()")][0][0] | {fileprivate: "private", open: "public"}[. // ""] // .;
    .path as $f | [.items[] | select(.symbolType != "namespace" and (.isImport | not)) | (stated // "internal") as $t
    | (.symbolType == "interface" or (.symbolType == "object" and stated != null) or $t == "private") as $held
    | $t, (.members[]? | stated // (if $held then $t else "internal" end))] | "\($f)\t" + (group_by(.) | map("\(.[0])=\(length)") | join(" "))'
mkdir -p <dir>/sg && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -I "$D/Build/Products/Debug" -F "$D/Build/Products/Debug" $MM -minimum-access-level private -skip-synthesized-members -output-dir <dir>/sg &&
    jq -rs '[.[].symbols[]] | group_by(.accessLevel)[] | "\(.[0].accessLevel)\t\(length)\ttop \([.[] | select(.pathComponents | length == 1)] | length)"' \
    <dir>/sg/<Module>*.symbols.json
comm -12 <(ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '
    def stated: [.signature // "" | scan("\\b(private|fileprivate|internal|package|public|open)\\b(?!\\()")][0][0];
    .path as $f | .items[] | select(.symbolType != "namespace" and (.isImport | not)) | (stated | IN("private", "fileprivate")) as $p
    | (select($p) | .name), (.members[]? | select($p or (stated | IN("private", "fileprivate"))) | .name) | select(. != "init") | "\($f)|\(.)"' | sort -u) \
    <(rg -How --no-heading '[A-Za-z_]\w*' <paths> | sed 's/:/|/' | sort | uniq -c | awk '$1 == 2 { print $2 }' | sort -u)
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.items[] | select(.symbolType != "namespace" and (.isImport | not)) | .name, (.members[]?.name)' |
    sort -u | awk '{ k = tolower($0); gsub(/_/, "", k); s = $0; sub(/[^.]*$/, "", s); t = substr($0, length(s) + 1); v = s tolower(substr(t, 1, 1)) substr(t, 2)
    if (!((k, v) in seen)) { seen[k, v]; d[k]++ } g[k] = g[k] " " $0 } END { for (k in g) if (d[k] > 1) print g[k] }'
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
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Outline: tree-sitter fails on constructs (`where` on an `associatedtype`, `catch` pattern lists) and prints a member `ERROR`, its span read whole
- Levels: members state their own keyword, else take their container's level in a private type, protocol, or extension stating one, else `internal`
- Exports: `package` is a library's export level, as libraries write no `public` (`0d0b959b5`) and graph `public` rows are macro output
- Graph: symbol graphs list private declarations with no source location, so per-file counts come from the outline and the graph counts per module
- Build: symbol graphs read the last `nx build Rasm-Package`, and JSONRPC aborts the extractor below `public`, counting from the outline alone
- Private readers: `CodingKeys` cases and protocol witnesses (`encode(to:)`) have readers in synthesized or host code, never a one-reader finding
- Name pairs: type and value sharing a word (`kind: Kind`) follow Swift convention and print nothing, so each group is one concept spelled apart
- Nesting: no repository checker measures logic depth (swiftlint `nesting` counts nested declarations alone), so the compiler's parse tree counts it
- Blocks: each `{}` block below a member body is a level (branch, `guard` else, loop, `do`, `catch`, `switch` case), an `else if` chain one
- Closures: bodies holding more than one statement, a branch, or a loop count a level, and single-expression closures with no control flow none
- Parse: `-dump-parse` reads syntax with no imports or build, `-swift-version 6` admits bare regex literals, and output goes to stderr
- Limits: CLAUDE.md sets 3 levels, and root `Package.swift` fails an expression past 200 type-check scopes, so a flattened chain states closure types

## [03]-[SOURCES]

Package reads, one command per step:
1. Version: Xcode, Swift, and SDK the build selects, and each package's resolved pin in root `Package.resolved`, never `Package.swift`'s `from:` range
2. Guide: DocC root article of a package module at its checkout with its topic groups, as no checkout ships `AGENTS.md` or `llms.txt`
3. Package inventory: symbol graph of a built module at `public`, a row per symbol with path, kind, and first doc line into `<dir>/pkg/<Module>.tsv`
4. SDK inventory: symbol graph of an SDK module (`System`, `Synchronization`, `AppKit`) with no build, `<Module>@<Other>` extension files included
5. SPI: `libs/swift/macos/SPI/SPI.h` read whole, its Swift view, then the export set of the framework its module map links, before any new SPI row
6. Workspace: modules `<Module>` depends on with their folders, from the survey's graph file, each read whole
7. Catalog: package outside `Package.resolved` read through `search:code` at its newest tag, as no checkout holds it

```bash
xcodebuild -version && swift --version && xcrun --show-sdk-version && jq -r '.pins[] | "\(.identity)\t\(.state.version // .state.revision[0:8])"' Package.resolved
rg -l --no-ignore '^# ``<Module>``' "$D/SourcePackages/checkouts/<package>" -g '*.md' | xargs rg -n '^#{2,3} '
mkdir -p <dir>/pkg && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -I "$D/Build/Products/Debug" -F "$D/Build/Products/Debug" $MM -minimum-access-level public -skip-synthesized-members -output-dir <dir>/pkg &&
    jq -r '.symbols[] | [(.pathComponents | join(".")), .kind.identifier, (.docComment.lines[0].text // "")] | @tsv' <dir>/pkg/<Module>*.symbols.json > <dir>/pkg/<Module>.tsv
mkdir -p <dir>/sdk && xcrun swift-symbolgraph-extract -module-name <Module> -target "arm64-apple-macos$(xcrun --show-sdk-version)" -sdk "$(xcrun --show-sdk-path)" \
    -minimum-access-level public -skip-synthesized-members -output-dir <dir>/sdk &&
    jq -r '.symbols[] | [(.pathComponents | join(".")), .kind.identifier, (.docComment.lines[0].text // "")] | @tsv' <dir>/sdk/<Module>*.symbols.json > <dir>/sdk/<Module>.tsv
xcrun swift-synthesize-interface -module-name SPI -sdk "$(xcrun --show-sdk-path)" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -I libs/swift/macos/SPI
F=$(rg -oN 'link framework "(\w+)"' -r '$1' libs/swift/macos/SPI/module.modulemap) && dyld_info -exports "/System/Library/PrivateFrameworks/$F.framework/$F"
jq -r --arg p '<Module>' '.graph as $g | $g.dependencies[$p][].target | "\(.)\t\($g.nodes[.].data.root)"' <dir>/graph.json
```

## [04]-[CONSUMERS]

Readers and leads across `<roots>`, one command per step:
1. Readers: sites by symbol identity from sourcekit-lsp over the Xcode index store, which xcode-build-server serves from the last build
2. Unindexed: files the script prints as `unindexed` (manifests, analyzer package, suites not built since their last edit) searched by word
3. Suites: test files reading `internal` members through `@testable import`, which a narrowing to `private` breaks
4. Text: sites spelling a member call, key, host constant, or message the declaration holds
5. Mentions: removed or renamed name across the repository with no type filter, docs, skills, and manifests included
6. Names: top-level type names other than file-private ones declared in more than one module, each pair read whole before a merge
7. Leads: declarations the script prints as `unread`, as no Swift unused-code tool exists, each checked against step 2's files first

```bash
node plugins/shared/code/skills/reduce/scripts/readers.ts <file> [<symbol>...]
rg -nw '<name>' <unindexed files>
rg -l '@testable import <Module>\b' tests
rg -nF '<member or literal>' -g '*.swift' <roots>
rg -nw '<name>' .
rg -oN '^(?:(?:package|public|internal|nonisolated|final|indirect) )*(?:struct|enum|class|actor|protocol|typealias) (\w+)' -r '$1' -g '*.swift' <roots> |
    awk -F: 'NR == FNR { r[$0]; next } { for (p in r) if (index($1, p "/") == 1) print p "\t" $2 }' <(rg -oN 'path: "([^"]+)"' -r '$1' Package.swift) - | sort -u |
    awk -F'\t' '{ n[$2]++; g[$2] = g[$2] " " $1 } END { for (k in n) if (n[k] > 1) print k ":" g[k] }'
```

Readers hold under these conditions:
- Build: the script names `nx build Rasm-Package` for a module the last build lacks, and a suite's file gains index units once its target builds
- Leads: `unread` rows include members synthesized code or a protocol requirement reads (`CodingKeys`, `init(from:)`), never a removal reason

## [05]-[FORMS]

Swift forms of each category that no checker reports, each rebuild holding under its stated condition alone, each source a `swift:coding` rule, a survey, or a past commit:

| [INDEX] | [CATEGORY]   | [FOUND]                                       | [REBUILD]                                      | [SOURCE]           |
| :-----: | :----------- | :-------------------------------------------- | :--------------------------------------------- | :----------------- |
|  [01]   | Redundant    | Hand coder matching synthesized `Codable`     | Synthesis, custom `init` in an extension       | `caa53e739`        |
|  [02]   | Redundant    | Storage mirroring a model's file records      | Model reading and writing its own file         | `4269d7a3a`        |
|  [03]   | Redundant    | Error type per provider, protocol tying them  | One module failure, `@CasePathable` reason     | `4269d7a3a`        |
|  [04]   | Redundant    | Reason cases no caller tells apart            | One case holding host error                    | `caa53e739`        |
|  [05]   | Redundant    | Class wrapping one lock                       | `Mutex`, `OSAllocatedUnfairLock` held directly | `2fd5788fb`        |
|  [06]   | Redundant    | Fixture member no suite asserts               | Removed, cases in one inventory table          | `5f3e9789f`        |
|  [07]   | Duplicated   | Hand `Decoder` or case walk over an enum      | `JSONDecoder` over `JSONEncoder` output        | `09df02f3b`        |
|  [08]   | Duplicated   | Per-case `isX` and `x: T?` accessors          | `@CasePathable`, `.is(\.x)`, `[case: \.x]`     | `swift:coding`     |
|  [09]   | Duplicated   | Near-duplicate initializers of one type       | One initializer taking its differing value     | `caa53e739`        |
|  [10]   | Duplicated   | Value type repeated across modules            | Package type, else one in its field's library  | Name survey        |
|  [11]   | Duplicated   | Hand list of names a host enumerates          | `AXUIElementCopyAttributeNames` at the read    | `swift:coding`     |
|  [12]   | Duplicated   | One host read per attribute of one pass       | `AXUIElementCopyMultipleAttributeValues`       | `swift:coding`     |
|  [13]   | Types        | `Bool` flags with companion fields (`Poll`)   | Case per state, facts as associated values     | `1cefb7039`        |
|  [14]   | Types        | Request struct per message on each wire side  | Few rich recursive enums both sides read       | `25f68a8af`        |
|  [15]   | Types        | Enum field refusing values its producer adds  | `Lenient<Known>` field over any raw value      | `09df02f3b`        |
|  [16]   | Types        | `any P` parameter one type fills per call     | `some P<Assoc>`                                | `swift:coding`     |
|  [17]   | Literals     | Text table per status (`[OSStatus: String]`)  | Status value printed by its constant name      | `25f68a8af`        |
|  [18]   | Literals     | Case list written by hand                     | `allCases`, reverse lookup built once from it  | `swift:coding`     |
|  [19]   | Literals     | Sentence spelled at a throw site              | Reason case fields, text from `ModuleError`    | `25f68a8af`        |
|  [20]   | Literals     | Host role strings compared at consumers       | One failable `init?` over a tuple `switch`     | `swift:coding`     |
|  [21]   | Pass-through | Parameter every caller fills alike            | Callee derives it, `allCases` for every case   | `swift:coding`     |
|  [22]   | Pass-through | Helper generic over its closure's `throws(E)` | Kept, rethrowing members widen `E`             | `swift:coding`     |
|  [23]   | Exports      | `public` across library modules               | `package` for other modules, else `internal`   | `0d0b959b5`        |
|  [24]   | Exports      | `package` member its own module reads alone   | `internal`, suites on `@testable import`       | Level count        |
|  [25]   | Exports      | `@CasePathable` no case path reads            | Attribute removed with its public members      | Symbol graph count |
|  [26]   | Nesting      | `do` per host call in one body                | One `do throws(E)`, pattern `catch` per case   | `swift:coding`     |
|  [27]   | Nesting      | Nested `switch` per subject                   | Tuple `switch`, `where` for one pattern        | `swift:coding`     |
|  [28]   | Nesting      | `if` and `else` branches assigning one value  | `let x: T = switch` or `if` expression         | `09df02f3b`        |
|  [29]   | Names        | Members in coined or mechanism words          | Field terms across every file and suite        | `8e57b8e4f`        |
|  [30]   | Names        | Concept spelled apart (`traceId`, `spanID`)   | Swift spelling, wire key in `CodingKeys`       | Name survey        |

Checked rules guide fixes inside a pass and find no committed site, as the check denies them: swiftlint `only_rules` of `.config/swiftlint.yml`, swift-format rules of `.config/swift-format.json`, ast-grep rules under `tools/ast-grep/rules/swift` (lock types, `@unchecked Sendable`, path text, Subprocess options, raw-value comparison, discarded failures, `@_exported`), and compiler warnings `treatAllWarnings` raises (unused `public import`).

Rows hold under these conditions:
- Row [01] -> every stored type is `Codable` and no field names its case, a custom initializer kept in an extension so the memberwise one stays
- Row [07] -> nesting guard of a recursive decode stays, as `09df02f3b` measured a deeper value overflowing a task's stack
- Row [23] -> suites reading members that leave `public` take `@testable import`, as `0d0b959b5` added it to 15 suite files
- Row [25] -> counts compare symbol graphs before and after, as macro members never reach the outline

## [06]-[CHECK]

Check per pass, each command alone in the background, one Xcode build at a time on this machine:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx run Rasm-Package:test--xcodebuild -- -only-testing:<Target>[/<Suite>]
```

- Autofix: swiftlint's fix rewrites `catch let name` to `catch` and `Type.init(` to `Self.init(`, so each file is read again after `hk fix`
- Build: `check` builds `Rasm-Package` and each app reading the module in `xcodebuild`, as `swiftc -typecheck` misses isolation and ownership errors
- Log: build logs read at "The following build commands failed" and "Please submit a bug report", never at `error:` lines alone
- Stale: failed module leaves its importers on a stale interface, so their errors can be false until its first cause is fixed
- Suites: pass changing a suite or code under test -> test line naming its target, as `nx test` runs more than one tool and refuses words after `--`
- Scope: root project `rasm` runs repository-wide checks (knip, jscpd) that fail on other work in a shared tree, so a pass excludes it
