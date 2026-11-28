---
name: rebuild
description: "Use when building, rebuilding, or restructuring files of code or configuration in any language, covering reading, owner derivation, removal, logic shape, and closing checks"
---

# [CODE_REBUILD]

Rebuilds replace a file's approach, never its spelling: each job written by hand moves to the member performing it, each declaration stating no fact of its own goes, each remaining member states each decision once at one level, and every consumer follows in one change. Code, configuration, build, rule, and checker files take the same steps.

[REFERENCES]:
- [01]-[OWNERS](references/owners.md): Read order and commands listing each language's owners and ranking them by a job's words

Agents loading the skill are the adversary of every line they read, their own earlier turns included, and run every step themselves from base to commit. Modes decide the base a pass reads against:

| [INDEX] | [MODE]          | [BASE]                                                    | [EDITS]                                              |
| :-----: | :-------------- | :-------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Build           | Tree at session start, plan naming consumers              | New files, each consumer the new capability replaces |
|  [02]   | Behind a writer | Base a brief names, else commit before the file was added | Added files whole, modified hunks and lines they hit |
|  [03]   | File or folder  | `HEAD` with uncommitted edits                             | Each file whole, owner files before their consumers  |

- Build -> each job a new file does maps to its owner before the file is written, and the measure is hand code removed across the tree
- Behind a writer -> `git diff --name-status <base> -- <paths>` splits added from modified, a file with no brief taking the commit before `git log --follow --diff-filter=A` added it
- Added files -> wrong whole, rebuilt from purpose, behaviors, and consumers, never from their own shape
- Modified files -> read whole, each hunk and each line it reaches rebuilt against base
- Folder -> files in turn, largest hand copy first
- Shared wires -> a wire read in more than one language changes on every side in one change

## [01]-[BASE]

Base facts come before the first read of code:
1. Skills: each language's coding skill, package skills its manifest imports, `writing:names`, and the host skill of the runtime its consumers run it in
2. Memory: index lines naming the work (rebuild, schema, naming, nesting, a package, a host) open their bodies, each banned form joining the read
3. Tree: `git status --short` and `git log --follow --stat -- <path>` per file, uncommitted edits belonging to the base
4. Decisions: a brief's or memory's recorded decision binds, and a pass reverses one only with a new fact it names

## [02]-[READING]

Reading precedes the first edit, as a change made from memory of an API, a summary, or an outline restates what an owner holds and misses a consumer's shape:
1. Files: each file of the unit from line 1 to its end, sibling modules it imports included, `ast-grep outline <path> --view expanded` choosing files alone
2. Consumers: every reader of each member a pass changes, found by `ast-grep run` and `rg -F`, read to its use
3. Jobs: each block doing a job by hand (parse, encode, decode, retry, schedule, diff, glob, spawn, format, lay out), named by verb and object
4. Owners: each job's candidates listed whole per `references/owners.md`, and members a kept decision rests on read at their installed release
5. History: `git log -S '<literal>'` for a guard, flag, or form whose reason code and readers omit, and each `fix:` subject of the file's history

Reading ends in statements editing starts from:
- Purpose: what each file is for, who calls it, how it runs, each input class it serves, and each declaration's fact
- Behaviors: each case, refusal, fix, output, error, and listed row of every file the pass edits, deletes, or replaces, with its line
- Hand copies: each job of step 3 with its owner member, ranked by lines that member removes

Questions take one route each:
- Instructions -> followed for the problem they name over their wording
- Objections -> each names a fact from the read
- Questions a read or source answers -> decided in the pass, the fact reported
- Choices a user holds (product behavior seen by a user, a design fork with costs on each side) -> asked through `sazandeh:ask` when first met

## [03]-[EDITING]

Large removals come from deleting hand copies of what an owner performs, and small edits around a hand copy leave it standing:
1. Owners: each hand copy replaced by its owner member, largest first, as one member call deletes declarations later steps rebuild
2. Removal: each declaration meeting its `[05]` criterion deleted, every consumer rebuilt, a file whose deletions reshape it written whole
3. Findings: each defect a removed guard or fold reveals fixed at its site in the same change, wherever the site sits
4. Logic: repetition and nesting rebuilt by `[06]`
5. Names: each name in its field's term, verbs for functions, nouns for values

Each edit takes its rebuild over its bad form:

| [INDEX] | [FOUND]                                                              | [REBUILD]                                                     |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Lines trimmed around a hand-written decoder, parser, or codec        | Copy deleted with its tests, owner member called              |
|  [02]   | Hand copy kept as faster or more convenient than its owner           | Owner called, a copy staying for a measured requirement       |
|  [03]   | Hand copy differing from its owner on some input                     | Owner's behavior, difference a defect the copy brought        |
|  [04]   | Custom grammar, schema, or format beside a tool's built-in one       | Built-in used, custom files, build steps, and rows deleted    |
|  [05]   | `sd` or a script reshaping code a pass rebuilds                      | Write of a reshaped file, Edit per hunk                       |
|  [06]   | `sd -F` over sites no read covered                                   | `sd -F` for one literal after each site was read              |
|  [07]   | Refused compact form answered by restoring the repetition            | Rule named from its source, next form it accepts              |
|  [08]   | Export, constant, row, helper, type, or table added with no reader   | Reader in the same change, code it replaces removed           |
|  [09]   | Helper, member, constant, or export added for defect fix or new case | Fix or case inside existing member holding it                 |
|  [10]   | Shared member changed for every consumer, consumers named after      | Every consumer read and rebuilt in the diff, or change scoped |
|  [11]   | Probe file or lowered severity in a tracked folder                   | Probe in scratch, severity left at its row                    |

Files are read again right before each edit, and no line of the tree is fenced from a fix.

## [04]-[OWNERS]

Facts and behavior go to the highest owner stating them, and a hand copy of an owner's member is wrong whatever its quality:

| [INDEX] | [OWNER]                          | [STATES]                                                           |
| :-----: | :------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Language and compiler            | Synthesized equality, hashing, coding, initializers, pattern match |
|  [02]   | Standard library and runtime     | Collections, text, numbers, time, files, processes, JSON, URLs     |
|  [03]   | Platform SDK and host            | Names, paths, versions, events, dialogs, registries, behavior      |
|  [04]   | Package at its installed version | Parsers, codecs, protocols, schedules, layouts, diffs, globs       |
|  [05]   | Repository library               | Capability another library or product reads, extended in place     |
|  [06]   | Target declaration               | Fact no owner above states                                         |

Package depth counts as much as deletion:

| [INDEX] | [FOUND]                                                   | [REBUILD]                                                               |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Members recalled from an older release                    | Installed release's declaration, newest per `[03]-[DEPENDENCY_POLICY]`  |
|  [02]   | Candidates limited to imported modules                    | Whole module index, as replacing members sit elsewhere                  |
|  [03]   | Inventory capped, sampled, or listed from symbol names    | Whole member set from a source's own listing, written to a file         |
|  [04]   | Empty search read as leave to hand-write                  | Index above it opened, each group of a job's field read whole           |
|  [05]   | Catalog searched by one word                              | Catalog read whole with group headings                                  |
|  [06]   | Package skill standing for an inventory                   | Skill loaded for forms, inventory read for members                      |
|  [07]   | Feature, extra, or companion capability written by hand   | Feature, extra, or companion enabled in its owning manifest row         |
|  [08]   | Input class worked around beside a dependency handling it | Dependency's member or option for that class                            |
|  [09]   | Repository library porting another language's library     | Each language's own ecosystem package for that job                      |
|  [10]   | Weaker version of a capability a repository library holds | Repository library extended in general form, each copy's reader rebuilt |
|  [11]   | Package semantic (concurrency, limit, depth) from memory  | Installed source read, code waiting until a source states it            |
|  [12]   | Package row removed while its capability is wanted        | Row kept, removed with its last import alone                            |
|  [13]   | Private module reached by a hand path                     | Public inference path reaching it                                       |
|  [14]   | File left unchanged with no input class or member named   | Largest input class it serves and each package member it uses, named    |

Kept hand-written jobs name the owner groups read and why none fits, and a job naming neither goes back to `[03]` step 1.

Hosts (applications, runtimes, build systems, checkers, generators) perform behavior and state values, and code calls a host member for each behavior and reads host state or a host declaration for each value:

| [INDEX] | [FOUND]                                                          | [REBUILD]                                                    |
| :-----: | :--------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Code opening, saving, quitting, styling, or laying out by hand   | Host member performing that behavior, called                 |
|  [02]   | Code repeating a dialog, save, instance, or focus a host handles | Code removed, host performs it                               |
|  [03]   | Name, path, size, version, key, or dialog kind as a literal      | Read from host state or declaration stating it               |
|  [04]   | List of applications per property (single instance, own dialogs) | Property read from installation or running process           |
|  [05]   | Version-keyed branch, path, or file kind                         | One path on newest release and betas                         |
|  [06]   | Panel, dialog, prompt, or theme drawn from primitives            | Host kind built by host members, diagnostics to logger       |
|  [07]   | Pass hiding, killing, relaunching, or duplicating a host         | Documents opened in running instance behind user windows     |
|  [08]   | Dialog hidden, dismissed by a timer, or waited out; event polled | Dialog read and answered through its host, event awaited     |
|  [09]   | Row, output, or case deleted with its naming literal             | Derivation listing that row, or value moved to caller data   |
|  [10]   | One declaration file of a host read                              | Every declaration file a host writes, augmentations included |

Host facts come from host-owned sources, in order:
- State: registries, receipts, databases, and property lists a host writes (`Info.plist`, LaunchServices, plug-in registries), read at run time
- Declarations: installed typings, scripting dictionaries, object models, and decompiled assemblies at the installed build, through `search:code`
- Reference: the newest release's reference for a member installed declarations lack
- Gaps: `tool:inspect` for a fact host documentation lacks or contradicts, one probe deciding it before code depends on it

Library code names no application, product, or host, and a removed name keeps everything it served:
- Coupling: a literal naming one application becomes a derivation from host state listing its row, or caller data at a product boundary
- Absent value: a value no host source states enters as caller data at a boundary, stated once, and the library derives, defaults, and composes
- Library values: library code holds no product name, style name, palette, sheet size, margin, path, or tuned value
- Absent behavior: a behavior no host member performs composes host kinds and package members over caller data, never a copy of a host member
- Older installs: they load built products alone, and no code targets them
- Checker: a host validator or compiler deciding structure is read from its source, its accepted forms deciding every later site of that form

## [05]-[REMOVING]

Declared names are every name a reader must know, internal (helpers, fields, locals, nesting, parameters) and external (exports, public parameters, flags, files, entry points, dependency rows), and both reduce by deletion and folding, never by nesting or hiding. A declaration stays when no owner states its fact and keeping it removes more logic than it adds.

Each kind goes on its criterion and takes its replacement:

| [INDEX] | [KIND]                 | [GOES_WHEN]                                                           | [REPLACED_BY]                          |
| :-----: | :--------------------- | :-------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | Struct, record, class  | Owner states its fields, or keeping it removes no logic               | Owner's type, named record at the read |
|  [02]   | Enum, union            | Variants repeat a type's, one is built, or no caller branches on them | Repeated type, existing variant        |
|  [03]   | Alias                  | Renames a type, has one reader in its module, or dodges a clash       | Named or qualified type at each site   |
|  [04]   | Constant, string row   | Owner states the value, or one site reads it                          | Read from the owner, inline value      |
|  [05]   | Bool pair, flag        | Selects among cases, or holds a fact the target derives               | One enum, derivation from target       |
|  [06]   | Wrapper, adapter       | Forwards a call unchanged, or converts as a package member does       | Direct call, package's conversion      |
|  [07]   | Helper                 | Born of the pass, one reader, or dodges a name clash                  | Body at the reader, renamed owner      |
|  [08]   | Interface              | One implementation and no seam a caller fills                         | Concrete type                          |
|  [09]   | Parameter              | Private and passed alike or never, or target derives it               | Folded into owner, derivation          |
|  [10]   | Export                 | Exposes a member only its package reads, never on reader count        | Private declaration                    |
|  [11]   | Re-export              | Forwards a whole crate or a member no public signature exposes        | Own capability, direct reference       |
|  [12]   | Error variant          | No caller branches on it                                              | Owner's error                          |
|  [13]   | Guard, fallback, retry | Proves own state, no reader reaches it, or hides a cause              | Construction refusing it, cause fixed  |
|  [14]   | File                   | Alone in or named for its folder, or one helper                       | Sibling file, fact at its owner        |
|  [15]   | Generator              | Output equals input                                                   | Source read directly                   |
|  [16]   | Dependency row         | No code imports it and no capability of the pass wants it             | Nothing, catalog row stays             |
|  [17]   | CLI flag, subcommand   | Tool derives the value from its target or state                       | Derivation, one name per operation     |

Criteria deciding a row:
- Readers: reader count decides private declarations alone
- Public API: exports, capabilities a plan names, setting rows, suite exports, and host entry points stay without readers, in their owner's shape
- Tuples: a tuple holds a value built and unpacked in one scope, and fields and signatures keep a named record
- Re-export: each consumer use becomes library capability, a type a signature exposes, or a direct import of the dependency
- Library: a library choosing a caller's entry point, runtime, transport, or configuration becomes parts callers compose over dependency types
- Entry: a guard, flag, arm, or default goes once its readers and `git log -S` show no input reaching it, a named input keeping it
- Cause: a guard, fallback, retry, clamp, lock, or wait names its cause from source at the installed release, fixed at its owner in the same change
- Self-validation: a check proving a program's own code or configuration goes (a re-parse its type guarantees, a cap on recursion over its own data)
- External checks: checks over external input, host responses, and caller-supplied code stay, a loop over caller functions keeping its bound
- Limits: a split respecting a limit a source states stays
- Restart rows: a supervisor's restart row is host configuration and no retry
- Divergence: behavior kept apart from its source names source, behavior, and a repository input triggering it, and no input means it goes
- Suppression: each suppression names its reason, and one a catalog or manifest row makes false goes with its finding fixed
- Capability: each input class a removal stops handling is named, one named input restores the removed code, and a dropped capability fails the pass

Agent-written code repeats these forms in every language, each found by behavior and rebuilt from the owner a read named.

### [05.1]-[HAND_CODE]

Hand code performs a job a platform member, package, or tool performs:

| [INDEX] | [FOUND]                                                               | [REBUILD]                                                     |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Helper doing platform member's job (memo map, pair zip, sum, merge)   | Platform member (insert-or-compute, n-ary zip, sum, combiner) |
|  [02]   | Character loop doing text member's job (case map, quote-aware scan)   | Standard text member or one pattern                           |
|  [03]   | Encoded-to-domain conversion by hand (try, parse, then construct)     | Package codec stating both directions, construction by schema |
|  [04]   | JSON Schema, protocol list, or enumeration written beside its package | Package member generating it, enumeration read from it        |
|  [05]   | Spawn of tool for values another owner resolved                       | Value read from its resolving owner, passed by caller         |
|  [06]   | Parse of tool's printed text where typed or structured output exists  | Typed API, structured output flag, or built-in module         |
|  [07]   | Text test on path, version, file name, or identifier (prefix, split)  | Relation member or glob of its type, shared by every site     |
|  [08]   | String joined into path, URL, key, or command line                    | Package builder (path join, URL, argument vector)             |
|  [09]   | Build-time file read and decoded at run time (manifest, bundled data) | Build-time import or embed the build tracks                   |
|  [10]   | Helper copied into each sibling project, exclusion hiding it          | One member in their shared library, each reader rebuilt       |
|  [11]   | State cells and lock kept consistent by hand                          | One record in one transactional or atomic cell                |

### [05.2]-[RESTATED_SHAPES]

Declarations restate a fact another value holds:

| [INDEX] | [FOUND]                                                                   | [REBUILD]                                               |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | Second type for one value (service shape and builder, request and stored) | One owning value, other forms derived from it           |
|  [02]   | Record copying fields out of value it could hold whole                    | Value held whole, readers deriving each field           |
|  [03]   | Tables keyed by one key held apart (allowed set, refusal map, labels)     | One table keyed once, row holding every fact of its key |
|  [04]   | Record per error or fault kind holding like fields                        | One error type with case per kind callers branch on     |
|  [05]   | Field, case data, or decoded key no reader reads                          | Read fields decoded alone, unread data deleted          |
|  [06]   | Annotation, description, or schema no host, model, or reader receives     | Deleted                                                 |
|  [07]   | Constant family sharing prefix (`MAX_`, `MIN_`, `DEFAULT_`)               | One value holding relation (`{ min, max }`)             |
|  [08]   | Literal per item in table (header per application)                        | Value derived where items are enumerated                |
|  [09]   | Guard repeated by every reader of wide shape                              | Shape narrowed once at construction                     |

### [05.3]-[INDIRECTION]

Members route a step through a copy, a parameter, or a second pass:

| [INDEX] | [FOUND]                                                                  | [REBUILD]                                                 |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Helper taking as parameters what its caller's scope holds                | Logic inside scope holding its inputs                     |
|  [02]   | Copy of general member fixed to one argument (combinator per condition)  | General member taking argument, each copy deleted         |
|  [03]   | Input parsed or walked again (check then split, pass per rule or output) | One read yielding parts, row per item serving each output |
|  [04]   | Value fixed at construction converted or built again on each call        | Converted once where it is constructed                    |

### [05.4]-[CHECKER_FORCED]

Checker rules force forms that outlive their cause:

| [INDEX] | [FOUND]                                                                        | [REBUILD]                                             |
| :-----: | :----------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Discard, cast, or declaration per site quieting one rule, kept after rule went | Rule rebuilt or off in its row, each forced form gone |
|  [02]   | Fold hiding use from checker (names moved into loop)                           | Form checker reads, every use visible                 |

### [05.5]-[VALUES]

Values and messages lose the type or case they come from:

| [INDEX] | [FOUND]                                                                  | [REBUILD]                                                    |
| :-----: | :----------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Message joined around fragments, sentence spelled per case, site, or row | Complete message per case, derived once from typed fields    |
|  [02]   | Library failure naming no subject or reason                              | Failure naming what failed and why, remedy added by its tool |
|  [03]   | Failure caught as default (zero, empty, unknown kind), filtered later    | Expected failure alone caught as absence, others raised      |
|  [04]   | Number with unit in name or formatted text (`timeoutMs`, `${n} bytes`)   | Duration or size type value, printed by its formatter        |
|  [05]   | Global declared for value build tool or host supplies                    | Typed channel of that tool (import, generated module)        |

## [06]-[LOGIC]

Members state each decision once, at one level, as a chain of steps a reader holds one at a time. Near-identical statements, arms, or rows, depth that rises after it fell, wrappers, and helpers are one defect: a decision written more than once, or a step written where its owner performs it.

Rewrites keep every behavior of the base and change its shape:
- Behavior list -> each row of `[02]` maps to a site in the rebuilt diff, and a row with no site takes its base code back
- Compiler limit (type-check timeout, crash on a packed expression) -> the language skill's form for that construct, never the base shape by default
- Checker limit (callback count, generator count) -> the form the language skill names at the limit, a curried member or a generator statement
- Defect found by a read -> named with its line and the source fact showing it, fixed at its cause

Nesting counts levels of logic, each a scope a reader holds open through the next:
- Levels: each nested callback, branch, arm body, and loop scope, read from syntax at any print width
- Data: value literals, schema and table declarations, markup, test group bodies, and API builder chains count none, their callbacks counted
- Limit: `[04]-[CODE_QUALITY]` of the project instructions, and each checker's own count from its configuration
- Ragged: depth rising, falling, and rising again in one member is a chain written as a tree, a defect under the limit
- Guards: a language's early-exit forms (`guard let`, `let ... else`, `?`, `yield*`) are binding chains and stay

Members past the limit or ragged rebuild deepest first, each step taking the earliest form that holds:
1. Name each level by its step's verb (decode, look up, choose), a level taking more verbs holding more steps
2. Remove each step changing nothing a later step reads: a conversion the next step undoes, a wrapper built to unwrap, a check its type guarantees
3. Replace code performing a step with a host or package member performing it
4. Restate the remaining steps in order as one flat chain (binding chain, pipeline, statement sequence), one step per line or statement
5. Rebuild an operation's signature where callers handle its result alike (same match arms, a value it knows passed in), with every consumer

Fixes that keep shape and add a name, or change what a member returns or raises, are no rebuild:

| [INDEX] | [FORBIDDEN]                                                                 | [REBUILD]                                               |
| :-----: | :-------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Lines joined, packed body or chain, tuple binding, print width raised       | Level removed from logic, one step and binding per line |
|  [02]   | Statement made an expression, value lifted into a wrapper to skip a map     | Statement in a block, plain value read in the map       |
|  [03]   | Helper, local function, private member, or file holding moved level or body | Sequence restated in place, owner member called         |
|  [04]   | Closure bound to a name for an inline callback                              | Callback inline in one flat chain                       |
|  [05]   | Wrapper or export around a nested call                                      | Direct call                                             |
|  [06]   | Type born of the pass naming an inner value                                 | Value inferred at its reader                            |
|  [07]   | Caller locals routing around a callee's signature                           | Callee's signature rebuilt with every consumer          |
|  [08]   | Traversal swapped for one stopping sooner, presented as flattening          | Same items run, same failures raised                    |

Each shape replaces one repeated or nested form:

| [INDEX] | [SHAPE]                                   | [REPLACES]                                                                           |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | Match over a tuple of subjects            | Nested conditions over more than one discriminant                                    |
|  [02]   | Arm guard                                 | Inner condition one arm alone reads                                                  |
|  [03]   | Binding chain                             | Early returns, stacked unwrap guards, binds over a nested call                       |
|  [04]   | Release by the handle's protocol          | Bracket of acquire, use, and release callbacks                                       |
|  [05]   | Shared step hoisted                       | Same statement repeated in every arm                                                 |
|  [06]   | Match in owner, miss as parameter         | Same match arms at every caller of an absent result                                  |
|  [07]   | Table or relation                         | Arms or rows differing by one value                                                  |
|  [08]   | Key type derived from record or host type | Union of keys a record or host type lists, written by hand                           |
|  [09]   | Iterator pipeline                         | Index loops, side collections, collect then filter or walk                           |
|  [10]   | Pipeline over a product, keyed lookup     | Loop inside a loop with a filter                                                     |
|  [11]   | One failure channel                       | Failure handler around each step                                                     |
|  [12]   | Accumulating collect                      | Loop with a mutable error list                                                       |
|  [13]   | Combinator choosing a value               | Decision inside a default-supplying closure                                          |
|  [14]   | Builder fold                              | Sequence of mutations on one object                                                  |
|  [15]   | One parameterized member                  | Members differing by a value, flag, or case                                          |
|  [16]   | Tagged union, fields per case             | Kind field or flag beside optional fields, type per variant, flag read after a match |

Algebraic forms hold where a relation is real and are never forced:
- Relation: quantities a caller computes from each other are one derivation solvable for any of them, a member or file per quantity fails
- Choice: a caller's choice among methods is an enum parameter, a `bool` holds a computed fact
- Deliberate: an enum, struct, or nesting stays when a caller branches per variant or it names a real relation, a type naming neither goes
- Catalog: a standard set callers extend is the generated enumeration beside one validated custom case, never a hand list beside a constructor
- Helper kept: a generic parameter relating types, a host constraint, a recursion, or a checker limit holds a helper, any other body at its reader

Strings hold text a reader receives or a host's or wire's fixed token, and behavior derives from the value a string names:
- String compared, switched on, or joined to decide behavior -> logic over the value it names, each case derived from its owner (type, member, host)
- Closed set the code branches on -> the language's tagged union or enum, each case holding its behavior or data
- Host or wire string -> decoded once at its boundary into the host's type, consumers never seeing the string
- Enum whose cases convert back to the same strings at every use -> renames them and goes
- Logic reading text shape, order, or spelling (a message pattern, a split index, a prefix test) -> the typed source or package predicate
- Code kept short by an erased error, an untyped read, a discarded message, a dropped case, or a loose decode -> its typed form

Members get faster through fewer passes, allocations, and parses over one input, and speed from a lock, a wait, a narrowed class, a serialized stage, or a dropped case is a regression:
- Pass: each pass, allocation, parse, or collected copy removed (a collect before a filter, a second walk zipping by index) goes in the commit body
- Level: each change names its level (grammar, regex class, rule shape, util, file walk, cache key, checker option), as another level hides the cause
- Capability: every input class handled before is handled after, and a narrowed grammar, class, or skipped case is a regression
- Cost: linear-scan lookups, repeated walks or parses, unbounded recursion, per-element copies, and resources held past use are rebuilt

## [07]-[CLOSING]

Passes close on the repository's own checks and a read of the diff, and a pass with a red step stays open:
1. Diff: `git diff <base> -- <paths>` and each untracked file read whole against the tables of `[03]` to `[06]` and the rows below, each hit rebuilt
2. Behaviors: each behavior statement of `[02]` has its site in the diff, and one with no site is restored
3. Shape: each fall in lines or declarations traces to a deleted declaration or owner call, never to a form moved, split, wrapped, aliased, or joined
4. Format: `hk fix <paths>`, each file it changed read again, as an autofix can drop a binding
5. Check: `nx run-many -t check -p <edited and consumer projects>` in the background, or a file outside any project its language's type checker, run again after each fix until green
6. Commit: `git commit -- <paths>` naming each path of the pass, as a commit with no paths takes every staged path in the tree

Rows no edit table holds, each found in the diff read:

| [INDEX] | [KIND]            | [SIGN]                                                            | [REBUILD]                               |
| :-----: | :---------------- | :---------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | Lost nesting      | Capture or restore at top level, nested levels overwritten        | Capture at every level written          |
|  [02]   | Nearest for exact | Lookup returning a neighbor's entry on a miss                     | Exact match tested, nothing on a miss   |
|  [03]   | Narrowed input    | Predicate, level, class, or grammar narrowed, rows lost           | Same inputs, or narrowing per consumer  |
|  [04]   | Changed identity  | Resource name, cache key, stored key, or label renamed            | Identity kept, or outside state moved   |
|  [05]   | Host compiler     | Newer language form in a file a host's own compiler builds        | Form host's compiler accepts            |
|  [06]   | Weakened check    | Lint lowered, rule narrowed, suppression, `files` exclusion       | Finding fixed at its cause              |
|  [07]   | Hidden cause      | Lock, wait, retry, guard, hidden launch, kill, or flag over cause | Cause fixed at its owner                |
|  [08]   | Snapshot gate     | Check comparing output with a pre-change copy                     | Gate deleted                            |
|  [09]   | False safety      | Test that cannot fail on this machine, guard over excluded state  | Test deleted, guard cut                 |
|  [10]   | Naive fix         | Fix another checker refuses, or one that fails to build           | Form every checker and compiler accepts |
|  [11]   | Stale read        | Edit made from a read taken before another write to the file      | File read again, edit made on it        |

Checks and their findings take one form each:
- Findings: each finding in an edited or consumer project is fixed at its cause after a fresh read, whoever wrote the line
- Diagnostics: a fix keeps the rebuilt form, and a type a compiler asks for is stated on the value that lost it
- Tree: checks run in the tree through nx, never in a scratch copy or through a tool outside the lock
- Tests: none is written unless the request names one, and a test the rebuild breaks takes the new shape or goes

Reports and commits take their rebuild over their bad form:

| [INDEX] | [FOUND]                                                                 | [REBUILD]                                          |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | Fix reported after `hk check` alone, "the next type check confirms"     | Check command run green                            |
|  [02]   | Type error answered with reasoning ("should type as") and no run        | Check run again, its output read                   |
|  [03]   | List of follow-ups for someone else in a report                         | Each item fixed in the pass                        |
|  [04]   | "A consumer needed it", "left as is", "kept for the lint", "to be safe" | Fact and source naming why it stays, or removed    |
|  [05]   | Commit body narrating forms or reasons the diff lacks                   | Body stating past facts the diff holds             |
|  [06]   | Input class the new code misses reported as unverified                  | Handled in the pass, or a refusal the commit names |

Commits take their type and scope from the commit lines of `writing:prose`, and the body states what the diff removed, what owner replaced it, and each behavior change with the consumers it reaches.
