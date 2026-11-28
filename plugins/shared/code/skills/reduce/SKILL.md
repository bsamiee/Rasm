---
name: reduce
description: "Use when reducing code in focused passes without losing capability, covering targets, units, source reads, consumer maps, categories, nesting, and counts"
argument-hint: "<target> [<concern>]"
arguments: [target, concern]
---

# [CODE_REDUCE]

Reduction rebuilds logic until each fact sits once, at the source stating it. Models hold valid states alone, so no invalid state can be built and no guard checks for one; dependent steps bind and independent steps combine; one parameterized form replaces a family differing by a value; every value derives from a host, package, or upstream declaration that states it. Each pass takes one category over one unit, read whole with its readers, duplicates, and sources, and rebuilds in place with no new declaration standing in for the old. Counts prove a pass and never steer it.

[REFERENCES]:
- [01]-[TYPESCRIPT](references/typescript.md): Skills, sources, catalog, consumers, counts, forms, and checks of TypeScript targets
- [02]-[RUST](references/rust.md): Skills, sources, consumers, counts, forms, and checks of Rust targets

## [01]-[TARGET]

Target `$target` sets what the survey reads, and concern `$concern` names one category of the category list:
- File -> file whole, every importer, and every file it imports
- Folder -> every file whole, ranked by findings
- Project -> counts per folder, then each folder whole in rank order
- Symbol -> declaring file whole and every reference site
- Suite -> suite whole and code under test whole, a unit being one fixture or helper form across the suite
- Commit range -> changed, renamed, and deleted paths, each live file whole, a unit being one category the range introduced
- Empty target -> ask the user, naming candidates from `git status --short` and `git log -5 --stat`
- Empty concern -> every category in list order, one at a time
- Target spanning languages -> one sequence per language, and a contract crossing them rebuilt on every side in one pass
- Language with no reference -> its coding skill, `search:code` for declarations, and its central manifest below

Central manifests list every package a target can adopt, one command per ecosystem (TypeScript, Python per uv root, .NET, Rust, Swift, C++), written to `<scratch>/reduce/catalog.txt` and read whole, .NET `Transitive Pinning` holding pins alone and Swift taking SDK frameworks and repository SPI in place of a catalog:

```bash
yq '.catalog, .catalogs' pnpm-workspace.yaml
sed -n '/^\[dependency-groups\]/,/^\[tool/p' <uv-root>/pyproject.toml
rg -n 'Label=|<PackageVersion Include=' Directory.Packages.props
sed -n '/^\[workspace.dependencies\]/,/^\[workspace.lints/p' Cargo.toml
sed -n '/^    dependencies: \[/,/^    \]/p' Package.swift && ls "$(xcrun --show-sdk-path)/System/Library/Frameworks" libs/swift/macos/SPI
jq -c '.dependencies // []' vcpkg.json
```

## [02]-[SKILLS]

Skills load before the first read of code, every matching line:
- Language: skill table of the language reference, else `<language>:coding` with each package skill the target's manifest names
- Host: skill of the host running the code (`blender:host`, `rhino:host`, `uxp:coding`, `chrome:extensions`)
- Tests: `code:test` and the language testing skill, when the target is a suite or a pass changes one
- Text: `writing:names` and `writing:prose`, for each name, message, and description a pass writes
- Sources: `search:code` for a dependency member at its installed release, `tool:inspect` for a host fact its documentation lacks
- Memory: index lines matching the target's paths or packages, opened before the first read, each body's forbidden forms joining the map

## [03]-[SEQUENCE]

Sessions run one loop, one pass per unit, until the concern holds no finding:
1. Survey: count each file of the target with the reference's count commands, read each whole, and write findings to `<scratch>/reduce/findings.md`
2. Rank: units order by certainty of their source, then by lines removed per line read, so a finding an inventory row proves precedes a probe
3. Pick: first category in list order holding a finding, or the concern given, and its top unit
4. Map: declarations of that unit alone, as a map of a whole target goes stale at its first commit
5. Rebuild: each declaration with every reader and duplicate site in one change, leaving no adapter, alias, or old path
6. Check: the reference's check command green, alone in the background
7. Diff: `git diff` read whole under the close rules, each regression rebuilt before the commit
8. Commit: files staged by name, subject `refactor(<project>): <imperative naming unit and category>`, no scope across projects
9. Re-rank: each touched file reread, finished findings struck, exposed findings added, then step 3
10. Stop: no finding of the concern left, or a decision belongs to the user, asked with each option's cost

Failures name their cause before the next edit:
- Check failing -> cause read from its diagnostic, rebuild corrected at that cause
- Same failure twice -> pass edits restored with the file edit tool, the cause taken as its own unit
- Finding of another category -> added to the findings with its site, fixed in its own pass
- Delegated units -> one agent per unit, its brief holding the unit's map, and units sharing a declaration or manifest run in turn

## [04]-[UNIT]

Units hold one cause and every declaration, reader, and contract that must change to remove it, at a size one whole read keeps in view and one check run proves:
- Size: one file, or one declaration with every reader and duplicate site; a category spanning more splits by file in rank order
- Floor: a unit too small to move a count joins its siblings of one category in one file
- Upstream: a cause sitting upstream moves the unit there, and every consumer is rebuilt in that pass
- Products: duplicates in more than one product merge into a member of their field's library, never into one product importing another
- Forks: a design with more than one valid shape (one type against a field per feature) goes to the user with each option's cost

## [05]-[READ]

Facts come from the source defining them: host and protocol facts from their host API or SDK, domain facts from an upstream value in the tree (enumeration, registry, manifest), operations from a language, compiler, or package at its installed release implementing their semantics, and a declaration in the target holds only a fact no source states, where it removes logic.

Package reads run in order, each ending in a decision row the report cites:
1. Guide: a package's own agent guide (`AGENTS.md`, `llms.txt`, a docs folder for agents), read whole
2. Index: module, namespace, or type list with one summary per row, read whole
3. Groups: every group whose summary names the job's field, read whole, as one search term misses its synonyms
4. Inventory: every export of each chosen group, written to a file and counted
5. Member: its declaration read from the doc comment's first line to the declaration's end, at its inventory row
6. Decision: each hand-written job takes the inventory row of its replacing member, or the groups read with their counts and why none fits

- Version -> release the target's project resolves, never a store or cache glob holding more than one
- Empty search -> index above it reopened, never hand-written code
- Job no core package covers -> central manifest read whole, then each candidate's declarations and docs

Reading reaches every reader, duplicate, and source of a unit, each file read whole:
- Duplicates: code doing a declaration's work without reading it, found by behavior: members it calls, literals it spells, fields it declares, clones
- Comparison: a duplicate and its declaration both read before choosing which form stays
- Sources: producer of each value a unit reads (enumeration, registry, manifest, host listing), where its derivation starts
- History: `git log -S '<literal>' -- <path>` for a guard, flag, or form whose reason no code or reader states

## [06]-[MAP]

Each declaration of a unit takes one row in `<scratch>/reduce/map.md` before any edit: name, kind, exported or file-private, and lines; fact it states that no source states; readers with members and shape each reads; duplicates; sources of its values; and cause of its shape, read from a source, compiler, or history.

Readers decide shape:
- Readers each rebuilding one value (a key list, a lookup, an unwrap) show a declaration holding a wrong shape
- Readers reading one member alone show a shape wider than its use, unless it exposes less authority by design
- Missing readers are never a removal reason: public API, planned capability, and unread code stating its own fact stay, rebuilt to their source

Duplicates take an action by their cause, mapped before any merge:
- Shape too narrow for its second reader -> one shape widened, both readers rebuilt on it
- Shape too wide, driving readers away -> shape narrowed to its contract, both readers rebuilt
- Both shapes missing their domain or host form -> both rebuilt toward that form
- External host or validator rule -> rule named from its source, forms it permits kept
- Type relation a merge loses -> declaration holding it kept, reason recorded
- Duplicate written without a search -> copy deleted, its readers rebuilt on the original
- Duplicate across products -> one general member in their field's library, both products calling it

Constraints divide by their source:
- External validators (plug-in validator, store checker, host loader) and compilers bind: one probe records the refusal, and rebuilds stay inside it
- Repository checker flagging improved code -> 2 agents briefed apart read the skill's goal and both forms, each deciding rule or change
- Rule wrong -> check removed, rule file deleted, or checker rebuilt to flag real defects alone, in its own pass
- Custom checkers (ruff and refurb rule sets, clippy and dylint lints, ast-grep rules, analyzer configs) take that review every time
- Published tool's own rule -> its documentation and source read before its configuration changes
- Probes change one variable against a named hypothesis, run in the real compiler or host, and restore with the file edit tool
- Refusals show a rule for the construct their diagnostic names, never a ban on the design

## [07]-[CATEGORIES]

Categories run in list order, internal declarations before exports, each one pass and one concern value. Each rebuild comes from the unit's map, as one finding's rebuild can differ from the last, and no rebuild keeps an old form moved, wrapped, or aliased, or replaces a literal with another constant, string, or enum renaming it.

### [07.1]-[PASS_THROUGH]

Indirection adds no fact, and its body or value belongs at its caller:
- Helper with one caller -> body at its caller
- Wrapper calling one member unchanged -> direct call to that member
- Local restating a value or a name -> value at its reader
- Parameter the callee derives from another -> derivation inside the callee
- Parameter every caller passes alike -> value folded into the callee
- Helper holding a type parameter that relates its arguments, a host constraint, a recursion, or a value computed once -> kept

### [07.2]-[LITERALS]

Literals with no source chosen take one, or go:
- Constant read once -> value inline at its reader
- Constants one relation joins (a minimum and maximum) -> one value stating that relation
- Literal a host, package, or manifest states -> read from that declaration
- Literal per item (a header per application) -> derived at the earliest point listing the items, every downstream literal deleted
- String compared to choose behavior -> logic over the value it names, a closed set of cases carrying behavior being a tagged union or enum
- String joined into a path, URL, or command -> builder of its module
- Number with a unit -> value of its unit's module
- Parse of human-formatted output -> typed API or structured output
- Wire or host spelling -> decoded once at its boundary into the host's type
- Constant more than one reader shares that no source states -> kept, declared in the group of its readers

### [07.3]-[REDUNDANT_CODE]

Redundant code repeats another declaration, or adds a fact no host, model, tool, or reader receives:
- Declaration repeating another field for field -> readers rebuilt on the original
- Annotation, schema, or description no sink reads -> removed whole
- Decode of fields no consumer reads -> decode of read fields alone
- Guard or re-parse over a state its type excludes -> removed
- Guard repeated across readers of a wide input -> one validation at construction
- Fallback, catch, or retry hiding a cause -> cause fixed at its source, failure passed on
- Error variant restating a source's error -> source's error passed through

Removal follows a read of every sink an annotation, schema, or description reaches (wire, model tool list, generated file, host registration), and code with a sink reading it stays.

### [07.4]-[DUPLICATED_LOGIC]

Logic a standard library, host, package, or the tree already holds goes to that member, rebuilds ordered by lines removed across the unit, largest first:
- Parser, codec, retry, or formatter written by hand -> member doing that job
- Copy of logic in the tree -> one declaration, each copy's readers rebuilt on it
- Weaker version of an existing capability -> existing capability extended in its general form
- Members differing by a value, flag, or case -> one member taking that value
- Member a compiler or generator writes -> attribute, derive, or record form
- Walk repeated over one input -> one walk with keyed lookups

### [07.5]-[TYPES]

Types follow states listed first, independent facts forming a product, exclusive states a sum, and behavior differing by a parameter one parameterized type:
- Struct or interface per variant -> one tagged union, each variant's data as its fields
- Booleans encoding exclusive modes -> one sum over valid states
- Optional fields set together -> one optional record
- Interface with one implementation -> concrete type or function value
- Hand-written type restating a value -> type derived from that value or its builder
- Types of one concept in more than one form -> one type, a narrower form derived at its reader
- Reader narrowing a wide shape each time -> shape narrowed at construction
- Nominal or stated type holding identity, an invariant, a public contract, or a correlation inference loses -> kept

### [07.6]-[NESTING]

Nesting counts levels of logic a reader holds open, in any language, up to the project's limit (3 in CLAUDE.md). Data literals, schema declarations, and markup trees count for none, and logic inside their callbacks counts. Ragged depth, stepping in, back, and in again, is a chain of steps written as a tree and a finding below the limit too. Each level takes the first rebuild that holds:
1. Name each level by its verb (decode, look up, choose, apply)
2. Remove steps changing nothing a later step reads
3. Replace a level with the member performing it
4. Restate dependent steps (unwrap inside unwrap, failure handling per step) as one flat chain, and combine independent steps
5. Turn decisions over more than one value into one tuple match, an inner condition into an arm guard, a loop in a loop into one pipeline
6. Move a decision every caller repeats into the callee's signature, and rebuild each caller

Rebuilds keep evaluation order, errors, cancellation, and resource scope. A body moved to a helper, a closure bound to a name, joined lines, and a raised print width keep their depth.

### [07.7]-[NAMES]

Names state what a thing is in its field's words, one name per concept, and names a host, wire, or tool resolves stay exact:
- Pair differing in case or affix -> one name, a type and its value sharing it where the language keeps them apart
- Pair a host constraint keeps apart -> builder named by its verb, its type derived from it
- Word no field documentation uses -> field term, renamed through language tooling in every reference, file, and folder
- Name restating its module or mechanism -> noun for a value, verb for a function
- One-declaration file, folder named by mechanism -> files merged by concept, folder named by concept
- Product fact inside a library -> product passing it as caller data

### [07.8]-[EXPORTS]

Exports state capability once, under one name:
- Alias, re-export, or forward -> readers on the declaring module
- Readers re-deriving an unexported type -> type exported once, each re-derivation deleted
- Type and value of one concept under different names -> one name under one export
- Export letting readers change module state -> module-private, readers on its operations

## [08]-[CLOSE]

Passes close when the check is green and the diff, read whole, shows the cut:
- Lines, internal members, and helpers fewer, a helper removed by rebuilding its callers and their upstream, never by copying its body
- Exports fewer or held, one more only where it removes a re-derivation in more than one reader
- Nesting deeper nowhere
- No removed form reappearing moved, renamed, wrapped, aliased, or replaced by a constant, string, or new type
- Regression in the diff -> rebuilt in the same pass, never landed for a later one

Scratch files under `<scratch>/reduce/` go once the session's last pass commits.
