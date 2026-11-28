---
name: rebuild
description: "Use when building, rebuilding, or restructuring files of code or configuration in any language, covering reading, owner derivation, removal, logic shape, and closing checks"
---

# [CODE_REBUILD]

Rebuilds replace a file's approach, never its spelling: each job written by hand moves to the member performing it, each declaration stating no fact of its own goes, each remaining member states each decision once at one level, and every consumer follows in one change. Code, configuration, build, rule, and checker files take the same steps.

[REFERENCES]:
- [01]-[OWNERS](references/owners.md): Read order and commands listing each language's owners and ranking them by a job's words

Agents loading the skill are the adversary of every line they read, their own earlier turns included, and run every step themselves from base to commit. Modes decide the base a pass reads against:

| [INDEX] | [MODE]          | [BASE]                                                   | [EDITS]                                              |
| :-----: | :-------------- | :------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Build           | Tree at session start, plan naming consumers             | New files, each consumer the new capability replaces |
|  [02]   | Behind a writer | Base a brief names, else parent of writer's first commit | Added files whole, modified hunks and lines they hit |
|  [03]   | File or folder  | `HEAD` with uncommitted edits                            | Each file whole, owner files before their consumers  |

- Build -> each job of a new file maps to its owner before any line is written, and hand code removed across the tree is its measure
- Behind a writer -> `git diff --name-status <base> -- <paths>` splits added from modified, and with no brief `git log --follow -- <path>` names the writer's first commit per file
- Added files -> wrong whole, rebuilt from purpose, consumers, and behaviors callers receive, never from their own shape or mechanism
- File sets -> each file read before the first edit, a form repeated across files rebuilt once at the module owning its concept, then files in turn
- Shared wires -> a wire read in more than one language changes on every side in one change

## [01]-[BASE]

Base facts come before the first read of code:
1. Skills: package skills its manifest imports, `writing:names`, and the host skill of the runtime its consumers run it in
2. Memory: index lines naming the work (rebuild, schema, naming, nesting, a package, a host) open their bodies, each banned form joining the read
3. Tree: `git status --short`, uncommitted edits belonging to the base
4. Decisions: a brief's or memory's recorded decision binds, and a pass reverses one only with a new fact it names

## [02]-[READING]

Reading precedes the first edit, as a change made from memory of an API, a summary, or an outline restates what an owner holds and misses a consumer's shape:
1. Files: each file of the unit from line 1 to its end, sibling modules it imports included, an outline choosing files alone
2. Package: `ast-grep outline <package> --view expanded` over the unit's package, as an owner sits in a sibling module the file never imports
3. Consumers: every reader of each member a pass changes, found by `ast-grep run` and `rg -F`, read to its use
4. Jobs: each block doing a job by hand (parse, encode, retry, schedule, diff, spawn, format, intercept, correlate, connect), named by verb and object
5. Owners: step 2 members first, then candidates read per `references/owners.md`
6. History: `git log --follow --format='%h %s' -- <path>` with each `fix:` subject, and `git log -S '<literal>'` for a form code and readers leave unexplained

Reading ends in statements editing starts from:
- Purpose: what each file is for, who calls it, how it runs, each input class it serves, and each declaration's fact
- Behaviors: each case, refusal, fix, output, error, and listed row a caller, user, or host receives from an edited or replaced file, with its line
- Mechanism: each lock, shared state, correlation, and failure a hand copy needs only to run, going with that copy and named in the commit body
- Hand copies: each job of step 4 with its owner member, ranked by lines that member removes

## [03]-[EDITING]

Large removals come from deleting hand copies of what an owner performs, and small edits around a hand copy leave it standing:
1. Owners: each hand copy replaced by its owner member, largest first, as one member call deletes declarations later steps rebuild
2. Removal: each declaration meeting its `[05]` criterion deleted, every consumer rebuilt, a file whose deletions reshape it written whole
3. Findings: each defect a removed guard or fold reveals fixed at its site in the same change, wherever the site sits
4. Logic: each member a pass writes or keeps taken through `[06]` steps 2 to 4 at any depth, members past the limit or ragged through all five
5. Names: each wrong name renamed in the same change, verbs for functions and nouns for values

Each edit takes its rebuild over its bad form:

| [INDEX] | [FOUND]                                                              | [REBUILD]                                                     |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Lines trimmed around a hand-written decoder, parser, or codec        | Copy deleted with its tests, owner member called              |
|  [02]   | Hand copy kept as faster or more convenient than its owner           | Owner called, a copy staying for a measured requirement       |
|  [03]   | Hand copy differing from its owner on some input                     | Owner's behavior, kept difference naming source and input     |
|  [04]   | Grammar, schema, protocol list, or enumeration beside its owner's    | Owner member generating it, custom files and steps deleted    |
|  [05]   | `sd` or a script reshaping code a pass rebuilds                      | Write of a reshaped file, Edit per hunk                       |
|  [06]   | `sd -F` over sites no read covered                                   | `sd -F` for one literal after each site was read              |
|  [07]   | Form a validator, checker limit, or compiler refused, base restored  | Language skill's form for it, else next form the rule accepts |
|  [08]   | Export, constant, row, helper, type, or table added with no reader   | Reader in the same change, code it replaces removed           |
|  [09]   | Helper, member, constant, or export added for defect fix or new case | Fix or case inside existing member holding it                 |
|  [10]   | Shared member changed for every consumer, consumers named after      | Every consumer read and rebuilt in the diff, or change scoped |
|  [11]   | Probe file in a tracked folder                                       | Probe in scratch                                              |
|  [12]   | Hook patching a host call the code itself made with short input      | Call made once with the full input its member takes           |
|  [13]   | Lock, shared reference, or text match tying a call to its own hook   | Result read from the call, the state deleted                  |
|  [14]   | Private symbol or key bound where read (`@_silgen_name`, literal)    | One declaration in the module stating private host API        |
|  [15]   | Connection, watch, or handle opened beside a sibling owning one      | Sibling's member extended and called                          |

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
|  [01]   | Feature, extra, or companion capability written by hand   | Feature, extra, or companion enabled in its owning manifest row         |
|  [02]   | Input class worked around beside a dependency handling it | Dependency's member or option for that class                            |
|  [03]   | Repository library porting another language's library     | Each language's own ecosystem package for that job                      |
|  [04]   | Weaker version of a capability a repository library holds | Repository library extended in general form, each copy's reader rebuilt |
|  [05]   | Private module reached by a hand path                     | Public inference path reaching it                                       |

Kept hand-written jobs name the owner groups read and why none fits, and a job naming neither goes back to `[03]` step 1.

Hosts (applications, runtimes, build systems, checkers, generators) perform behavior and state values, and code calls a host member for each behavior and reads host state or a host declaration for each value:

| [INDEX] | [FOUND]                                                          | [REBUILD]                                                    |
| :-----: | :--------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Code opening, saving, styling, laying out, or focusing by hand   | Host member performing it, code deleted                      |
|  [02]   | Name, path, size, version, key, or dialog kind as a literal      | Read from host state or declaration stating it               |
|  [03]   | List of applications per property (single instance, own dialogs) | Property read from installation or running process           |
|  [04]   | Version-keyed branch, path, or file kind                         | One path on newest release and betas                         |
|  [05]   | Panel, dialog, prompt, or theme drawn from primitives            | Host kind built by host members, diagnostics to logger       |
|  [06]   | Pass hiding, killing, relaunching, or duplicating a host         | Documents opened in running instance behind user windows     |
|  [07]   | Dialog hidden, dismissed by a timer, or waited out; event polled | Dialog read and answered through its host, event awaited     |
|  [08]   | Row, output, or case deleted with its naming literal             | Derivation listing that row, or value moved to caller data   |
|  [09]   | One declaration file of a host read                              | Every declaration file a host writes, augmentations included |

Host facts come from host-owned sources, in order:
- State: registries, receipts, databases, and property lists a host writes (`Info.plist`, LaunchServices, plug-in registries), read at run time
- Declarations: installed typings, scripting dictionaries, object models, and decompiled assemblies at the installed build
- Reference: the newest release's reference for a member installed declarations lack

Library code names no application, product, or host, and a removed name keeps everything it served:
- Absent value: a value no host source states enters as caller data at a boundary, stated once
- Checker: a host validator or compiler deciding structure is read from its source, its accepted forms deciding every later site of that form
- Library values: library code holds no product name, style name, palette, sheet size, margin, path, or tuned value
- Absent behavior: a behavior no host member performs composes host kinds and package members over caller data, never a copy of a host member

## [05]-[REMOVING]

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
|  [10]   | Re-export              | Forwards a whole crate or a member no public signature exposes        | Own capability, exposed type, import   |
|  [11]   | Error variant          | No caller branches on it                                              | Owner's error                          |
|  [12]   | Guard, fallback, retry | Proves own state, no reader reaches it, or hides a cause              | Construction refusing it, cause fixed  |
|  [13]   | File                   | Alone in or named for its folder, or one helper                       | Sibling file, fact at its owner        |
|  [14]   | Generator              | Output equals input                                                   | Source read directly                   |
|  [15]   | Dependency row         | No code imports it and no capability of the pass wants it             | Nothing, catalog row stays             |
|  [16]   | CLI flag, subcommand   | Tool derives the value from its target or state                       | Derivation, one name per operation     |

Criteria deciding a row:
- Public API: exports, capabilities a plan names, setting rows, suite exports, and host entry points stay without readers, in their owner's shape
- Tuples: a tuple holds a value built and unpacked in one scope, and fields and signatures keep a named record
- Entry: a guard, flag, arm, or default goes once its readers and history show no input reaching it, a named input keeping it
- Self-validation: a check proving a program's own code or configuration goes (a re-parse its type guarantees, a cap on recursion over its own data)
- Divergence: behavior kept apart from its source names source, behavior, and a repository input triggering it, and no input means it goes
- Cause: a guard, fallback, retry, clamp, lock, or wait names its cause from source at the installed release, fixed at its owner in the same change
- External checks: checks over external input, host responses, and caller-supplied code stay, a loop over caller functions keeping its bound
- Limits: a split respecting a limit a source states stays
- Restart rows: a supervisor's restart row is host configuration and no retry
- Capability: an input class a removal stops handling fails the pass, and one named input restores its code

Forms below recur in agent-written code of every language, and each is found by behavior and rebuilt from the owner a read named.

### [05.1]-[HAND_CODE]

Hand code performs a job a platform member, package, or tool performs:

| [INDEX] | [FOUND]                                                               | [REBUILD]                                                     |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Helper doing platform member's job (memo map, pair zip, sum, merge)   | Platform member (insert-or-compute, n-ary zip, sum, combiner) |
|  [02]   | Character loop doing text member's job (case map, quote-aware scan)   | Standard text member or one pattern                           |
|  [03]   | Encoded-to-domain conversion by hand (try, parse, then construct)     | Package codec stating both directions, construction by schema |
|  [04]   | Spawn of tool for values another owner resolved                       | Value read from its resolving owner, passed by caller         |
|  [05]   | Parse of tool's printed text where typed or structured output exists  | Typed API, structured output flag, or built-in module         |
|  [06]   | Text test on path, version, file name, or identifier (prefix, split)  | Relation member or glob of its type, shared by every site     |
|  [07]   | String joined into path, URL, key, or command line                    | Package builder (path join, URL, argument vector)             |
|  [08]   | Build-time file read and decoded at run time (manifest, bundled data) | Build-time import or embed the build tracks                   |
|  [09]   | Helper copied into each sibling project, exclusion hiding it          | One member in their shared library, each reader rebuilt       |
|  [10]   | State cells and lock kept consistent by hand                          | One record in one transactional or atomic cell                |

### [05.2]-[RESTATED_SHAPES]

Declarations restate a fact another value holds:

| [INDEX] | [FOUND]                                                                   | [REBUILD]                                               |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | Second type for one value (service shape and builder, request and stored) | One owning value, other forms derived from it           |
|  [02]   | Record copying fields out of value it can hold whole                      | Value held whole, readers deriving each field           |
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
|  [04]   | Global declared for value build tool or host supplies                    | Typed channel of that tool (import, generated module)        |

## [06]-[LOGIC]

Near-identical statements, arms, or rows, depth that rises after it fell, wrappers, and helpers are one defect: a decision written more than once, or a step written where its owner performs it.

Rewrites keep every behavior of the base and change its shape:
- Behavior list -> each row of `[02]` maps to a site in the rebuilt diff, and a row with no site is restored in the rebuilt form, never by base code
- Compiler limit (type-check timeout, crash on a packed expression) -> the language skill's form for that construct, never the base shape by default
- Checker limit (callback count, generator count) -> the form the language skill names at the limit, a curried member or a generator statement

Nesting counts levels of logic, each a scope a reader holds open through the next:
- Levels: each nested callback, branch, arm body, and loop scope, read from syntax at any print width
- Data: value literals, schema and table declarations, markup, test group bodies, and API builder chains count none, their callbacks counted
- Limit: `[04]-[CODE_QUALITY]` of the project instructions, and each checker's own count from its configuration
- Ragged: depth rising, falling, and rising again in one member is a chain written as a tree, a defect under the limit
- Guards: a language's early-exit forms (`guard let`, `let ... else`, `?`, `yield*`) are binding chains and stay

Members rebuild deepest first, each step taking the earliest form that holds:
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
|  [05]   | Type born of the pass naming an inner value                                 | Value inferred at its reader                            |
|  [06]   | Caller locals routing around a callee's signature                           | Callee's signature rebuilt with every consumer          |
|  [07]   | Traversal swapped for one stopping sooner, presented as flattening          | Same items run, same failures raised                    |

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
- Deliberate: an enum, struct, or nesting stays when a caller branches per variant or it names a real relation, a type naming neither goes
- Catalog: a standard set callers extend is the generated enumeration beside one validated custom case, never a hand list beside a constructor
- Helper kept: a generic parameter relating types, a host constraint, a recursion, or a checker limit holds a helper, any other body at its reader

Strings hold text a reader receives or a host's or wire's fixed token:
- Logic reading text shape, order, or spelling (a message pattern, a split index, a prefix test) -> the typed source or package predicate
- Closed set the code branches on -> the language's tagged union or enum, each case holding its behavior or data
- Host or wire string -> decoded once at its boundary into the host's type, consumers never seeing the string
- Code kept short by an erased error, an untyped read, a discarded message, a dropped case, or a loose decode -> its typed form

Members get faster through fewer passes, allocations, and parses over one input, and speed from a lock, a wait, a narrowed class, a serialized stage, or a dropped case is a regression:
- Capability: every input class handled before is handled after, and a narrowed grammar, class, or skipped case is a regression
- Level: each change names its level (grammar, regex class, rule shape, util, file walk, cache key, checker option), as another level hides the cause

## [07]-[CLOSING]

Passes close on the repository's own checks, then a whole read of the diff, and a pass with a red step or a hit in the read stays open:
1. Format: `hk fix <paths>`, each file it changed read again, as an autofix can drop a binding
2. Check: `nx run-many -t check -p <edited and consumer projects>`, or its language's type checker for a file outside any project, until green
3. Diff: `git diff <base> -- <paths>` and each untracked file read whole after the last green check, as a form a check forced reaches no earlier read
4. Rebuild: each hit of the read rebuilt, then steps 1 to 3 again
5. Commit: `git commit -- <paths>` once a read finds no hit, naming each path, as a commit with no paths takes every staged path in the tree

Diff reads hit on:
- Tables: a form a row of `[03]` to `[06]` or of the table below names
- Behaviors: a behavior statement of `[02]` with no site, restored in the rebuilt form and never by base code
- Shape: a removed line gone to no deletion or owner call, an added line with neither an owner call nor a capability the base lacked
- Moves: a form moved, split, wrapped, aliased, renamed in place, or joined onto one line

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
|  [09]   | False safety      | Test that cannot fail on its machine, guard over excluded state   | Test deleted, guard cut                 |
|  [10]   | Naive fix         | Fix another checker refuses, or one that fails to build           | Form every checker and compiler accepts |
|  [11]   | Stale read        | Edit made from a read taken before another write to the file      | File read again, edit made on it        |

Checks and their findings take one form each:
- Findings: each finding in an edited or consumer project is fixed at its cause after a fresh read, whoever wrote the line
- Diagnostics: a fix keeps the rebuilt form, and a type a compiler asks for is stated on the value that lost it
- Tree: checks run in the tree through nx, never in a scratch copy or through a tool outside the lock
- Tests: a test the rebuild breaks takes the new shape or goes

Reports and commits take their rebuild over their bad form:

| [INDEX] | [FOUND]                                                                 | [REBUILD]                                          |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | Fix reported after `hk check` alone, type error answered by reasoning   | Check run again green, its output read             |
|  [02]   | List of follow-ups for someone else in a report                         | Each item fixed in the pass                        |
|  [03]   | "A consumer needed it", "left as is", "kept for the lint", "to be safe" | Fact and source naming why it stays, or removed    |
|  [04]   | Commit body narrating forms or reasons the diff lacks                   | Removals, owners, behavior changes with consumers  |
|  [05]   | Input class the new code misses reported as unverified                  | Handled in the pass, or a refusal the commit names |

Commit type and scope follow `writing:prose` commit lines.
