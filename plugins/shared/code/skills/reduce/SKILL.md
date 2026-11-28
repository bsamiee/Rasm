---
name: reduce
description: "Use when reducing a file, folder, or package in focused passes that rebuild logic in place, covering reading, units, readers, sources, categories, and nesting"
argument-hint: "<target> [<concern>]"
arguments: [target, concern]
---

# [CODE_REDUCE]

Reduction rebuilds logic until each fact and operation sits once, at the source stating it: language, standard library, host, package, or upstream declaration. Models hold valid states alone: construction excludes invalid ones, no guard rechecks them, and boundaries validate foreign input once. Domain logic stays pure with effects composed at system boundaries, dependent steps bind and independent steps combine, one parameterized form replaces families differing by one value, values derive from variables already holding them, and logic walks its input once with keyed lookups.

Targets are files, folders, or packages, and every pass leaves them with fewer lines, internal members, and exports without losing capability. Strings deciding behavior become logic over typed values, loose constants go to their source, hand-rolled logic goes to the package or host member doing its job, many weak shapes become fewer richer objects, dead code goes, wrappers and helpers fold into rebuilt callers, scattered exports collapse to one name per concept, and ragged nesting becomes one flat chain. Removed forms never return as new helpers, wrappers, constants, or types standing in for them. Language references add each language's reduction mechanisms with its package and host reads, and findings need no reference row.

[REFERENCES]:
- [01]-[TYPESCRIPT](references/typescript.md): Skills, sources, mapping, mechanisms, patterns, and checks of TypeScript and JavaScript targets
- [02]-[RUST](references/rust.md): Skills, sources, mapping, mechanisms, patterns, and checks of Rust targets
- [03]-[CSHARP](references/csharp.md): Skills, sources, mapping, mechanisms, patterns, and checks of C# targets
- [04]-[PYTHON](references/python.md): Skills, sources, mapping, mechanisms, patterns, and checks of Python targets
- [05]-[SWIFT](references/swift.md): Skills, sources, mapping, mechanisms, patterns, and checks of Swift targets

[TEMPLATES]:
- [01]-[REFERENCE](templates/reference.md): Sections and order of each language reference

## [01]-[TARGET]

Target `$target` (first argument, else path named in request) sets what sessions read, and concern `$concern` (second argument, else empty) names one category by its lowercase heading (`redundant-code`, `exports`):
- File -> file whole, every reader, and every file it imports
- Folder or project -> every file whole, then units in category order across them
- Folder of projects -> each project as its own target
- Symbol -> declaring file whole and every reader site
- Suite -> suite and code under test whole, each unit one fixture or helper form across suite
- Commit range -> changed paths at both revisions, each live file whole, each unit one category the range introduced
- Empty target -> `sazandeh:ask` with candidates from `git status --short` and `git log -5 --stat`
- Empty concern -> every category in list order, one per pass
- Target spanning languages -> one sequence per language, contracts crossing them rebuilt on every side in one pass
- Language with no reference -> its coding skill, `search:code` for declarations, and its central manifest

## [02]-[SKILLS]

Skills load before first read of code, every matching line:
- Language: coding skill and core package skills of its language reference, always, whatever the target imports today
- Packages: skill of each package replacing hand-written target code, imported or not, as passes add it
- Host: coding skill of host running code (`blender:coding` with Blender skills for each part it writes, `uxp:coding`, `chrome:extensions`)
- Tests: `code:test` and language testing skill, for suite targets and passes changing suites
- Text: `writing:names` and `writing:prose`, for each name, message, and description passes write
- Sources: `search:code` for dependency members at their installed release, `tool:inspect` for host facts documentation lacks
- Memory: index lines matching target paths or packages, opened before first read, each body's forbidden forms joining each unit map

## [03]-[SEQUENCE]

Sessions run one loop, one pass per unit, notes kept in one session scratch folder:
1. Read: each target file whole, purpose per file (job, readers, how it runs), findings by category with site
2. Sources: core packages, standard library, and host of the language reference, then its central manifest whole, once per session
3. Pick: first category in list order holding findings (or `$concern`), then its clearest unit, internal cuts before exports
4. Map: declarations of picked unit alone, with readers, like work, and sources, as whole-target maps go stale at their first commit
5. Rebuild: each declaration with every reader, like-work site, and upstream cause in one change, no adapter, alias, old path, or mention left
6. Check: reference check commands over pass paths, each alone in background, green
7. Diff: `git diff -- <paths>` read whole under close rules, each regression rebuilt before commit
8. Commit: pass paths alone staged, subject `refactor(<project>): <unit and category>`
9. Re-rank: each touched file reread, finished findings struck, exposed findings added, then step 3
10. Stop: no finding of `$concern` left in `$target`, or next decision belongs to user

Passes stay focused, one category over one unit, and large files reach their smaller form across many passes, each committing working code:
- Order: clearest and largest cuts first, hand-rolled blocks packages replace going before polish on code inside them
- Scope: findings of other categories join session findings with their site and wait for their own pass
- Failure: cause read from its diagnostic before next edit, and same failure twice restores pass edits and records evidence new designs need

## [04]-[UNIT]

Units hold one cause and every declaration, reader, and contract that must change to remove it, at sizes one whole read keeps in view and one check proves:
- Size: one file, or one declaration with every reader and like-work site, files of one contract staying in one unit
- Floor: findings of one category in one file joining one unit, never passes too small to move counts
- Upstream: causes upstream move their unit there, every consumer rebuilt in one pass before any downstream change
- Products: like work in more than one product goes to one member of their field's library, never one product importing another
- Forks: representations decided by intended capability, policy, or preference no source states go to `sazandeh:ask` with each option's cost
- Delegation: one agent and map per unit, units sharing one declaration or manifest running in turn

## [05]-[READ]

Facts come from their defining source: host and protocol facts from host API or SDK, domain facts from upstream tree values (enumeration, registry, manifest), operations from languages, compilers, or packages implementing their semantics at installed release, and target declarations hold only facts no source states, where they remove logic. Unit reads reach every reader, like-work site, and source, each file read whole, never with outlines, excerpts, or summaries in its place.

Sessions read packages of the language's central manifest, imported or not, beside core packages its reference names, and use each one fitting hand-written target work deeply through its API, added to target project where it fits. Package reads run in order, each step ending with its decision on unit map:
1. Guide: package's own agent guide (`AGENTS.md`, `llms.txt`, agent docs folder), read whole, else its entry-point index
2. Index: every entry point and module with member counts and summaries, read whole
3. Groups: every group with name or summary naming job's field read whole, groups with no summary opened at their members
4. Inventory: every member of each chosen group, written to session scratch folder and counted
5. Member: declaration from its doc comment's first line to its end, inputs, outputs, failures, and effects compared against the job
6. Decision: each hand-written job takes its replacing member, or groups read with their counts and the reason none fits

Package reads end in one case:
- Version -> release the target's project resolves, never store or cache globs holding more than one
- Empty search -> index above it reopened, never hand-written code
- Adoption -> package added through its owning manifest and catalog row, never through transitive import

History reads `git log -S '<literal>' -- <path>` for guards, flags, or forms with reasons no code or reader states.

## [06]-[MAP]

Unit declarations each take one row before any edit: name, kind, visibility, and lines; fact it states that no source states; readers with members and shape each reads; like work; sources of its values; and cause of its shape, read from source, compiler, or history.

Visibility levels narrow without removing capability:
- File-private: declaration its own file reads alone
- Package-internal: TypeScript export no package entry lists, C# `internal`, Rust `pub(crate)`, Swift `internal`
- Public: package entry or exported API, C# `public`, Rust items the crate root reaches, Swift `public`

Readers resolve by symbol identity through each language's compiler tools its reference names, and `rg -nw` finds literals, comments, strings, and non-code mentions (docs, skills, manifests) alone. Readers decide shape:
- Readers each rebuilding one value (key list, lookup, unwrap) -> declaration holding wrong shape
- Readers reading one member alone -> shape wider than its use, unless it exposes less authority by design
- No readers -> no removal reason, as public API, planned capability, and unread code stating its own fact stay, rebuilt to their source

Like work is code doing another declaration's job without reading it, found by behavior (members it calls, literals it spells, fields it declares, clones), sibling projects first. Reasons it skips the declaration are findings of their own, and sessions read both forms with their readers before either stays:
- Shape too narrow for its second reader, or too wide and driving readers away -> one shape at its contract's width, both readers rebuilt
- Both shapes missing their domain or host form -> both rebuilt toward it
- External host or validator rule -> rule named from its source, forms it permits kept
- Type relation lost in merge -> declaration holding it kept, reason recorded
- Copy written without search -> copy deleted, its readers rebuilt on the original

Constraints divide by their source:
- External validator, store checker, host loader, or compiler error -> one probe records its rule for named construct, rebuilds kept inside rule
- Repository checker flagging improved code -> 2 agents briefed separately judge rule or change from the skill's goal and both forms
- Agents disagreeing -> rule stays, and user decides through `sazandeh:ask`
- Rule wrong -> check removed, rule file deleted, or checker rebuilt to flag real defects alone, in its own pass
- Custom checker (ruff and refurb rule sets, clippy and dylint lints, ast-grep rules, analyzer configs) -> reviewed every time
- Published tool's own rule -> its documentation and source read before its configuration changes

## [07]-[CATEGORIES]

Categories run in list order, most code deleted first, and no pass polishes code later passes delete. Each category is one concern value covering whole target, and its list names its forms, never its limit. Findings belong to the category naming their cause, and rebuilds come from their unit's map, as one finding's rebuild can differ from previous ones.

### [07.1]-[REDUNDANT_CODE]

Redundant code repeats another declaration, or states facts no sink receives (annotation no encoder, tool list, generator, or host registration reads), traced through every sink:
- Declaration repeating another field for field -> readers rebuilt on the original
- Annotation, schema, or description no sink reads -> removed whole, however needed its wrapping looks
- Decode of fields no consumer reads -> decode of read fields alone, read-modify-write data staying whole
- Guard or re-parse over state its type excludes -> removed, guards each reader of wide input repeats becoming one validation at construction
- Read before call with host side effect (create, refresh, notify) -> kept, as it guards host writes
- Fallback, catch, or retry hiding its cause -> cause fixed at its source, recovery hosts or protocols require (timeout, compensation) kept
- Error variant restating source error -> source error passed through
- Attribute, macro, or derive with generated members no reader reads -> removed with them, as generated members are declarations

### [07.2]-[DUPLICATED_LOGIC]

Logic standard libraries, hosts, packages, or the tree hold goes to their member, rebuilds ordered by lines removed, largest first:
- Parser, codec, retry, formatter, or host operation (process, path, dialog) written by hand -> package or host member doing the job
- Copy or weaker version of logic in the tree -> one declaration in its general form, each copy's readers rebuilt on it
- Adapters of one contract shaped apart (field in one, per-call value in another, module constant in third) -> one shape the contract states
- Members differing by value, flag, or case -> one member taking it, distinct operations kept apart
- Member compilers or generators write -> attribute, derive, or record form with identical serialization
- Walk repeated over one input, or parallel tables keyed by one discriminator -> one walk with keyed lookups, one keyed table

### [07.3]-[TYPES]

Types follow states listed first, independent facts forming product types, exclusive states sum types, and behavior differing by parameter one parameterized type. Fewer, richer objects replace many weak ones, and passes rebuild consumers to their shape:
- Struct or interface per variant, or booleans encoding exclusive modes -> one tagged union over valid states, each case's data as its fields
- Optional fields set together -> one optional record
- Hand-written type restating value, builder, host, or package declaration -> type derived from its source, boundary decode where none exists
- Types of one concept in more than one form -> one type, narrower forms derived at their reader
- Wrapper type holding no rule, string type standing for closed set, alias read once -> its value type, union of set, type inline
- Readers narrowing wide shapes each time -> shape narrowed at construction
- Interface or trait with one implementation -> concrete type, inherent members, or plain functions, method syntax alone being no reason
- Implementations beyond one, public contract callers elsewhere fill, or public blanket `<Trait>Ext` continuing method chains -> interface kept
- Nominal or stated type holding identity, invariant, public contract, or correlation inference loses -> kept

### [07.4]-[LITERALS]

Literals and facts with no source chosen take one or go, never through constants, strings, or enums renaming them:
- Constant read once, renaming its value alone -> value inline at its reader
- Constants one relation joins (minimum and maximum) -> one value stating the relation
- Literal stated by host, package, or manifest -> read from its declaration
- Literal per item (header per application) -> derived at the earliest point listing its items, every downstream literal deleted
- String compared to choose behavior -> logic over the value it names, closed sets of cases carrying behavior becoming tagged unions or enums
- String joined into path, URL, or command, or number with unit -> builder or value of its module
- Parse of human-formatted output -> typed API or structured output
- Wire or host spelling -> decoded once at its boundary into host or domain type, spelled exact there alone
- Product fact inside library (rule table, name, path) -> product passing it as caller data, library taking its general form
- Constant shared by readers with no source stating it, or value compiled once at module scope (pattern, schema, formatter) -> kept

### [07.5]-[PASS_THROUGH]

Indirection adds no fact, and its body or value belongs at its caller:
- Helper with one caller -> body moved into its caller, parameters bound to the caller's values, no copy left
- Helper with more than one caller -> removed by rebuilding callers and their upstream, never by copying its body into each
- Helper restating host, package, or tree member -> duplicated-logic finding, never inlined first
- Wrapper calling one member unchanged -> direct call to the member
- Local read once restating value or name -> value at its reader
- Parameter the callee derives, or every caller passes alike with no public contract varying it -> value derived or folded inside it
- Helper holding type parameter relating its arguments, host constraint, recursion, or value computed once -> kept

### [07.6]-[EXPORTS]

Exports state capability once, under one name per concept, at the narrowest visibility level its readers need:
- Alias, re-export, or forward -> readers on the declaring module
- Export no package entry lists -> package-internal, or file-private where its own file reads it alone
- Operations exported one by one beside their concept's object -> members of the object, or file-private where one reader holds them
- Exported constant or string constant -> derived at each reader from its source, or file-private
- Readers re-deriving unexported type -> type exported once, each re-derivation deleted
- Type and value of one concept under different names -> one name under one export
- Export letting readers change module state -> module-private, readers on its operations
- Dependency row after its last import goes -> removed, catalog row kept

### [07.7]-[NESTING]

Nesting counts levels of logic readers hold open, in any language, up to the project's limit (3 in CLAUDE.md). Data literals, schema declarations, and markup trees count for none, and logic inside their callbacks counts, single-expression closures with no branch or loop counting none. Ragged depth (stepping in, back, and in again) writes step chains as trees, and passes rebuild it below the limit. Each level takes the first rebuild that holds:
1. Name each level by its verb (decode, look up, choose, apply)
2. Remove steps changing no value later steps read and no observable effect
3. Replace level with member performing it
4. Restate dependent steps (unwrap inside unwrap, failure handling per step) as one flat chain, and combine independent steps
5. Turn decisions over more than one evaluated value into one tuple match, inner conditions into arm guards, loops in loops into one pipeline
6. Move decisions every caller repeats into callee signature, and rebuild each caller

Rebuilds keep evaluation order, short-circuiting, laziness, errors, cancellation, and resource scope. Bodies moved to helpers, closures bound to names, joined lines, and raised print widths keep their depth.

### [07.8]-[NAMES]

Names state what things are in their field's words, one name per concept, and names hosts, wire formats, or tools resolve stay exact:
- Pair differing in case, affix, or joined word (`X` and `x`, `makeX`, `XOf`) for one fact -> one name, type and value sharing it where allowed
- Pair host constraints keep apart, proved by probe of host check -> builder named by its verb, its type derived from it
- Joined word naming job other than its pair's (`runHook` beside `Hook`, running effects and no hook) -> name of its own job
- Word no field documentation uses -> field term, renamed through language tooling in every reference, file, folder, and doc
- Name restating its module or mechanism -> noun for value, verb for function
- One-declaration file, folder named by mechanism -> files merged by concept, folder named by concept

## [08]-[CLOSE]

Passes close on green check and whole read of their own diff:
- Counts: lines, internal members, helpers, exports, and nesting rise nowhere across pass paths, and the category's own count falls
- Exceptions: names pass holding counts and removing its coined or paired name, nesting pass lowering depth with lines free to rise
- Exports: one more only where it deletes re-derivations in more than one reader
- Shuffles: no removed form back under another name, file, wrapper, alias, constant, string, or new type, erased to untyped read, or missing cases
- Regression in diff -> rebuilt before its pass commits, never left for later passes

Scratch notes go once the session's last pass commits.
