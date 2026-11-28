---
name: reduce
description: "Use when reducing code in focused passes by deriving values, consolidating logic, and keeping every capability, covering targets, units, sources, readers, and categories"
argument-hint: "<target> [<concern>]"
arguments: [target, concern]
---

# [CODE_REDUCE]

Reduction rebuilds logic until each fact and operation sits once, at the source stating it: language, standard library, host, package, or upstream declaration. Models hold valid states alone: construction excludes invalid ones, no guard rechecks them, and foreign input is validated once at its boundary. Domain logic stays pure with effects composed at system boundaries, dependent steps bind and independent steps combine, one parameterized form replaces a family differing by a value, and logic walks its input once with keyed lookups. Each pass takes one category over one unit, read whole with its readers, duplicates, and sources, and rebuilds its logic in place.

Targets are files, folders, or packages named for reduction, and every pass leaves them with fewer lines, internal members, and exports: stringly code becomes logic over typed values, loose constants go to their source, low-quality shapes become fewer richer objects, dead and redundant code goes, indirection folds into its callers, and exports narrow to the capability readers need. Language references add each language's mechanisms for those cuts, and a finding needs no row of theirs.

[REFERENCES]:
- [01]-[TYPESCRIPT](references/typescript.md): Skills, sources, readers, survey signals, forms, and checks of TypeScript and JavaScript targets
- [02]-[RUST](references/rust.md): Skills, sources, consumers, counts, forms, and checks of Rust targets
- [03]-[CSHARP](references/csharp.md): Skills, survey signals, sources, consumers, forms, and checks of C# targets
- [04]-[PYTHON](references/python.md): Skills, survey signals, sources, consumers, forms, and checks of Python targets
- [05]-[SWIFT](references/swift.md): Skills, survey signals, sources, consumers, forms, and checks of Swift targets

## [01]-[TARGET]

Target `$target` (first argument, else a path the request names) sets survey scope, and concern `$concern` (second argument, else empty) names one category by its heading in lowercase (`redundant-code`, `pass-through`):
- File -> file whole, every importer, and every file it imports
- Folder or project -> survey signals per folder, then each file read whole in rank order
- Folder of projects -> each project a target of its own, in order of survey signals
- Symbol -> declaring file whole and every reader site
- Suite -> suite and code under test whole, a unit being one fixture or helper form across the suite
- Commit range -> changed, renamed, and deleted paths at both revisions, each live file whole, a unit being one category the range introduced
- Empty target -> `sazandeh:ask` with candidates from `git status --short` and `git log -5 --stat`
- Empty concern -> every category in list order, one at a time
- Target spanning languages -> one sequence per language, a contract crossing them rebuilt on every side in one pass
- Language with no reference -> its coding skill, `search:code` for declarations, and its ecosystem's tools named below

## [02]-[SKILLS]

Skills load before the first read of code, every matching line:
- Language: skill table of its language reference, else `<language>:coding` with each package skill the target's manifest names
- Host: coding skill of the host running code (`blender:coding` with a Blender skill per part it writes, `uxp:coding`, `chrome:extensions`)
- Tests: `code:test` and language testing skill, for a suite target or a pass changing one
- Text: `writing:names` and `writing:prose`, for each name, message, and description a pass writes
- Sources: `search:code` for a dependency member at its installed release, `tool:inspect` for a host fact its documentation lacks
- Memory: index lines matching target paths or packages, opened before the first read, each body's forbidden forms joining a map

## [03]-[SEQUENCE]

Sessions run one loop, one pass per unit, scratch files under `<scratch>/reduce-<target>/` (session scratch folder, target path with `/` spelled `-`):
1. Survey: signals over the target's paths rank files, then each file read whole in rank order, findings to `findings.md` with site and category
2. Sources: package reads for each core package the target calls, once per session, inventories written to its scratch folder
3. Pick: first category in list order holding a finding (or `$concern`), then its unit a listing or inventory proves, largest cut first
4. Map: declarations of the picked unit alone, as a map of a whole target goes stale at its first commit
5. Rebuild: each declaration with every reader and duplicate site in one change, leaving no adapter, alias, old path, or mention
6. Check: reference check commands over pass paths, each alone in the background, green
7. Diff: `git add -N <new paths>`, then `git diff -- <paths>` read whole under close rules, each regression rebuilt before the commit
8. Commit: `git add -- <paths>`, own hunks of a shared file by `git apply --cached`, subject `refactor(<nx-project>): <unit and category>`
9. Re-rank: each touched file reread, finished findings struck, exposed findings added, then step 3
10. Stop: no finding of `$concern` left in `$target`, a decision belongs to the user, or each finding left needs a source no read reached

Survey signals rank before any whole read, each tool run once per question over the target's paths from `git ls-files`, never per file:
- Outline: `ast-grep outline` declarations and exports, stderr checked for ignored custom languages, `namespace` dividers dropped, ERROR items read
- Size and clones: jscpd lines and branching per file deciding whole-read order, AST pairs touching the target ranked by lines times similarity
- Dependents: projects depending on the target's project from `nx graph`, files from a language graph (`ruff analyze graph`)
- Joins: outline names against token counts, giving file-private declarations with one reader and name pairs differing by case
- Leads: exports and dependency rows no reader reaches from a language's unused-code tool (knip, codelens `find_unused_symbols`, `cargo shear`)

Failures name their cause before the next edit:
- Repository checker refusing a rebuild -> checker review under map constraints first, the pass resuming after a wrong rule's own pass
- Check failing -> cause read from its diagnostic, rebuild corrected at its cause
- Same failure twice -> pass edits restored with the file edit tool, cause and evidence a new design needs recorded, retry only on new evidence
- Finding of another category -> added to findings with its site, fixed in its own pass
- Delegated units -> one agent and map file per unit, units sharing a declaration or manifest in turn, one conductor check for Swift

## [04]-[UNIT]

Units hold one cause and every declaration, reader, and contract that must change to remove it, at a size one whole read keeps in view and one check proves:
- Size: one file, or one declaration with every reader and duplicate site, files of one contract staying in one unit
- Floor: findings of one category in one file joining one unit
- Upstream: cause sitting upstream moves the unit there, every consumer rebuilt in one pass before any downstream inlining
- Forks: representation decided by intended capability, policy, or preference no source states goes to `sazandeh:ask` with each option's cost

## [05]-[READ]

Facts come from their defining source: host and protocol facts from host API or SDK, domain facts from an upstream value in the tree (enumeration, registry, manifest), operations from a language, compiler, or package implementing their semantics at its installed release, and a target declaration holds only a fact no source states, where it removes logic.

Package reads run in order, each ending in a decision row of the map:
1. Guide: package's own agent guide (`AGENTS.md`, `llms.txt`, agent docs folder), read whole, else its entry-point index
2. Index: every entry point and module with member counts and summaries, read whole
3. Groups: every group with a name or summary naming the job's field read whole, and every group with no summary opened at its members
4. Inventory: every member of each chosen group, written to a file and counted
5. Member: declaration from its doc comment's first line to its end, inputs, outputs, failures, and effects compared against the job
6. Decision: each hand-written job takes the inventory row of its replacing member, or groups read with their counts and why none fits

Package reads end in a case:
- Version -> release the target's project resolves, never a store or cache glob holding more than one
- Empty search -> index above it reopened, never hand-written code
- Job no core package covers -> central manifest read whole, then each candidate's declarations and docs at its catalog release
- Adoption -> package added through its owning manifest and catalog row, never reached as a transitive import

Central manifests list every package a target can adopt, one command per ecosystem (TypeScript, Python per uv root, .NET, Rust, Swift, C++):

```bash
yq '.catalog, .catalogs' pnpm-workspace.yaml
git ls-files ':!:.config/**' '*uv.lock' && yq -p toml -oy '."dependency-groups"' <uv-root>/pyproject.toml
rg -n 'Label=|<PackageVersion (Include|Update)=' Directory.Packages.props
yq -p toml -oy '.workspace.dependencies' Cargo.toml
sed -n '/^    dependencies: \[/,/^    \]/p' Package.swift
ls "$(xcrun --show-sdk-path)/System/Library/Frameworks" "$(xcrun --show-sdk-path)/usr/lib/swift" "$(xcrun --show-sdk-platform-path)/Developer/Library/Frameworks" libs/swift/macos/SPI
jq '{dependencies, features}' vcpkg.json
```

Rows of .NET `Transitive Pinning` hold pins alone and `Update` rows versions a host condition selects, and Swift reads SDK frameworks, Swift-only SDK modules (`Synchronization`), platform test frameworks, and repository SPI beside `Package.swift`.

Reading reaches every reader, duplicate, and source of a unit, each file read whole:
- Duplicates: code doing a declaration's work without reading it, found by behavior (calls, literals, fields, clones), sibling projects first
- Comparison: duplicate and declaration both read with their own readers, inputs, outputs, and constraints before choosing which form stays
- Sources: declarations listing the items a unit spells (enumeration, registry, host listing), and producers of each value it reads
- History: `git log -S '<literal>' -- <path>` for a guard, flag, or form with a reason no code or reader states

## [06]-[MAP]

Each declaration of a unit takes one row in `map.md` (one file per delegated agent) before any edit: name, kind, visibility, and lines; fact it states that no source states; readers with members and shape each reads; duplicates; sources of its values; and cause of its shape, read from a source, compiler, or history.

Visibility levels narrow without removing capability:
- File-private: declaration its own file reads alone
- Package-internal: TypeScript export no package entry lists, C# `internal`, Rust `pub(crate)`, Swift `internal`
- Public: package entry or exported API, C# `public`, Rust items the crate root reaches, Swift `public`

Readers resolve by symbol identity, never by file name or text:
- C#: roslyn-codelens `find_references` and `find_callers`
- Text: `rg -nw` for literals, comments, strings, and non-code mentions (docs, skills, manifests) alone
- Roots: `libs products plugins tools tests infra`, package export maps, subpath imports, and dynamic imports included

Readers decide shape:
- Readers each rebuilding one value (key list, lookup, unwrap) -> declaration holding a wrong shape
- Readers reading one member alone -> shape wider than its use, unless it exposes less authority by design
- No readers -> never a removal reason, as public API, planned capability, and unread code stating its own fact stay, rebuilt to their source

Duplicates take an action by their cause, mapped before any merge:
- Shape too narrow for its second reader, or too wide and driving readers away -> one shape at its contract's width, both readers rebuilt
- Both shapes missing their domain or host form -> both rebuilt toward it
- External host or validator rule -> rule named from its source, forms it permits kept
- Type relation a merge loses -> declaration holding it kept, reason recorded
- Duplicate written without a search -> copy deleted, its readers rebuilt on the original
- Duplicate across products -> one general member in their field's library, never one product importing another

Constraints divide by their source:
- External validator, store checker, host loader, or compiler refusal -> probe records its rule for the named construct, rebuilds kept inside it
- Repository checker flagging improved code, lint fixes included -> 2 agents briefed apart judge rule or change from goal and both forms
- Agents disagreeing -> rule stays, and the user decides through `sazandeh:ask`
- Rule wrong -> check removed, rule file deleted, or checker rebuilt to flag real defects alone, in its own pass
- Custom checker (ruff and refurb rule sets, clippy and dylint lints, ast-grep rules, analyzer configs) -> review every time
- Published tool's own rule -> its documentation and source read before its configuration changes
- Probe -> one variable against a named hypothesis in the real compiler or host, restored with a file edit tool, a tool failure proving nothing

## [07]-[CATEGORIES]

Categories run in list order, most code deleted first, as a later pass deletes code an earlier one would polish. Each category is one concern value with one unit per pass, and a finding belongs to the category naming its cause, other symptoms joining its unit's map. Each rebuild comes from its unit's map, as one finding's rebuild can differ from the last. Each category covers the whole target, and lists below name its forms, never its limit.

### [07.1]-[REDUNDANT_CODE]

Redundant code repeats another declaration, or states a fact no sink receives (annotation no encoder, tool list, generator, or host registration reads), traced through every sink:
- Declaration repeating another field for field -> readers rebuilt on the original
- Annotation, schema, or description no sink reads -> removed whole
- Decode of fields no consumer reads -> decode of read fields alone, read-modify-write data staying whole
- Guard or re-parse over a state its type excludes -> removed, a guard each reader of a wide input repeats becoming one validation at construction
- Fallback, catch, or retry hiding a cause -> cause fixed at its source, recovery a host or protocol requires (timeout, compensation) kept
- Error variant restating a source's error -> source's error passed through

### [07.2]-[DUPLICATED_LOGIC]

Logic a standard library, host, package, or the tree holds goes to its member, rebuilds ordered by lines removed, largest first:
- Parser, codec, retry, formatter, or host operation (process, path, dialog) written by hand -> package or host member doing the job
- Copy or weaker version of logic in the tree -> one declaration in its general form, each copy's readers rebuilt on it
- Sibling adapters of one contract shaped apart (field in one, per-call value in another, module constant in a third) -> one shape the contract states
- Members differing by a value, flag, or case -> one member taking the value, distinct operations kept apart
- Member a compiler or generator writes -> attribute, derive, or record form with identical serialization
- Walk repeated over one input, or parallel tables keyed by one discriminator -> one walk with keyed lookups, one keyed table

### [07.3]-[TYPES]

Types follow states listed first, independent facts forming a product, exclusive states a sum, and behavior differing by a parameter one parameterized type:
- Struct or interface per variant, or booleans encoding exclusive modes -> one tagged union over valid states, each case's data as its fields
- Optional fields set together -> one optional record
- Hand-written type restating a value, builder, host, or package declaration -> type derived from its source, a boundary decode where none exists
- Types of one concept in more than one form -> one type, a narrower form derived at its reader
- Reader narrowing a wide shape each time -> shape narrowed at construction
- Interface or trait with one implementation -> concrete type, inherent members, or plain functions, method syntax alone being no reason
- Implementations beyond one, public contract a caller elsewhere fills, or public blanket `<Trait>Ext` continuing a chain -> interface kept
- Nominal or stated type holding identity, an invariant, a public contract, or a correlation inference loses -> kept

### [07.4]-[LITERALS]

Literals and facts with no source chosen take one or go, never a constant, string, or enum renaming them:
- Constant read once, renaming its value alone -> value inline at its reader
- Constants one relation joins (minimum and maximum) -> one value stating the relation
- Literal a host, package, or manifest states -> read from its declaration
- Literal per item (header per application) -> derived at the earliest point listing its items, every downstream literal deleted
- String compared to choose behavior -> logic over the value it names, a closed set of cases carrying behavior being a tagged union or enum
- String joined into a path, URL, or command, or number with a unit -> builder or value of its module
- Parse of human-formatted output -> typed API or structured output
- Wire or host spelling -> decoded once at its boundary into the host or domain type, spelled exact there alone
- Product fact inside a library (rule table, name, path) -> product passing it as caller data, library taking its general form
- Constant shared by readers with no source stating it, or value compiled once at module scope (pattern, schema, formatter) -> kept

### [07.5]-[PASS_THROUGH]

Indirection adds no fact, and its body or value belongs at its caller:
- Helper with one caller -> body moved into its caller, parameters bound to the caller's values, no copy left
- Helper with more than one caller -> removed by rebuilding callers and their upstream, never by a copy of its body in each
- Helper restating a host, package, or tree member -> duplicated-logic finding, never inlined first
- Wrapper calling one member unchanged -> direct call to the member
- Local read once restating a value or a name -> value at its reader
- Parameter the callee derives, or every caller passes alike with no public contract varying it -> value derived or folded inside it
- Helper holding a type parameter relating its arguments, a host constraint, a recursion, or a value computed once -> kept

### [07.6]-[EXPORTS]

Exports state capability once, under one name, at the narrowest visibility level its readers need:
- Alias, re-export, or forward -> readers on the declaring module
- Export no package entry lists -> package-internal, or file-private where its own file reads it alone
- Readers re-deriving an unexported type -> type exported once, each re-derivation deleted
- Type and value of one concept under different names -> one name under one export
- Export letting readers change module state -> module-private, readers on its operations
- Dependency row after its last import goes -> removed, catalog row kept

### [07.7]-[NESTING]

Nesting counts levels of logic a reader holds open, in any language, up to the project's limit (3 in CLAUDE.md). Data literals, schema declarations, and markup trees count for none, and logic inside their callbacks counts. Ragged depth (stepping in, back, and in again) is a chain of steps written as a tree and a finding below the limit. Each level takes the first rebuild that holds:
1. Name each level by its verb (decode, look up, choose, apply)
2. Remove steps changing no value a later step reads and no observable effect
3. Replace a level with the member performing it
4. Restate dependent steps (unwrap inside unwrap, failure handling per step) as one flat chain, and combine independent steps
5. Turn decisions over more than one evaluated value into one tuple match, an inner condition into an arm guard, a loop in a loop into one pipeline
6. Move a decision every caller repeats into the callee's signature, and rebuild each caller

Rebuilds keep evaluation order, short-circuiting, laziness, errors, cancellation, and resource scope. A body moved to a helper, a closure bound to a name, joined lines, and a raised print width keep their depth.

### [07.8]-[NAMES]

Names state what a thing is in its field's words, one name per concept, and names a host, wire, or tool resolves stay exact:
- Pair differing in case, affix, or joined word (`makeX`, `XOf`) for one fact -> one name, type and value sharing it where the language allows
- Pair a host constraint keeps apart, proved by a probe of the host's check -> builder named by its verb, its type derived from it
- Word no field documentation uses -> field term, renamed through language tooling in every reference, file, folder, and doc
- Name restating its module or mechanism -> noun for a value, verb for a function
- One-declaration file, folder named by mechanism -> files merged by concept, folder named by concept

## [08]-[CLOSE]

Passes close on a green check and a whole read of the pass's own diff:
- Counts: lines, internal members, helpers, exports, and nesting rise nowhere across pass paths, and the category's own count falls
- Exceptions: names pass holding counts and removing its coined or paired name, nesting pass lowering depth with lines free to rise
- Exports: one more only where it deletes a re-derivation in more than one reader
- Shuffles: no removed form back under another name, file, wrapper, alias, constant, string, or new type, erased to an untyped read, or missing a case
- Regression in the diff -> rebuilt before its pass commits, never left for a later one

Folder `<scratch>/reduce-<target>/` goes once the session's last pass commits, and no other scratch file goes with it.
