---
name: rebuild
description: "Use when building, rebuilding, or restructuring files of code or configuration in any language, covering reading, owner derivation, removal, logic shape, and closing checks"
---

# [CODE_REBUILD]

Rebuilds replace a file's approach, never its spelling: each job written by hand moves to the owner performing it (a language feature or standard member, then a host or package member at its installed release, then a declaration in the tree), each declaration stating no fact of its own goes, each remaining member states each decision once at one level, and every consumer follows in the same change. Agents loading the skill are the adversary of every line they read, their own earlier turns included, run every step themselves from base to commit, and hand no pass to another agent. Code, configuration, build, rule, and checker files take the same steps. Use `<language>:coding` skills for language form.

[REFERENCES]:
- [01]-[OWNERS](references/owners.md): Commands listing each language's owners and ranking them by a job's words

Modes decide the base a pass reads against and what it edits:

| [INDEX] | [MODE]          | [BASE]                                         | [EDITS]                                              |
| :-----: | :-------------- | :--------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Build           | Tree at session start, plan naming consumers   | New files, each consumer the new capability replaces |
|  [02]   | Behind a writer | Commit before the writer's round, from a brief | Added files whole, modified hunks and lines they hit |
|  [03]   | Consumer set    | `HEAD` with uncommitted edits                  | Owner file, then every consumer in one change        |

- Build -> each job a new file does maps to its owner before the file is written
- Build measure -> hand code removed across the tree, as a new module grows while copies in its consumers go
- Behind a writer -> `git diff --name-status <base> -- <paths>` splits files into added and modified rows
- Added rows -> wrong whole, rebuilt from purpose, behaviors, and consumers, never from their own shape
- Modified rows -> read whole, each hunk and each line it reaches rebuilt against base
- Consumer set -> no old path, alias, or adapter stays, and a wire shared across languages changes on both sides in one change
- Folder -> each file takes mode 02 or 03 in turn, owner files before their consumers, largest hand copy first

## [01]-[BASE]

Skills and tree come before the first read of code:
1. Skills: each language's coding skill, package skills its manifest imports, the host skill where code runs inside a host, `writing:names`
2. Memory: index lines matching the paths or packages open their bodies, and each banned form joins the read
3. Tree: `git status --short` and `git log -5 --stat`, uncommitted edits of other agents belong to the base and stay theirs
4. Mixed work: each change since the last clean commit stays for a fact or capability it names, or is undone in place

## [02]-[READING]

Reading precedes the first edit, as a change made from memory of an API, a summary, or an outline restates what an owner holds and misses a consumer's shape:
1. Files: each file to edit from line 1 to its end, `ast-grep outline <path> --view expanded` choosing files and never standing in for one
2. Consumers: every reader of each member a pass changes, found by `ast-grep run` and `rg -F`, read to its use
3. Jobs: each block doing a job by hand (parse, encode, decode, retry, schedule, diff, glob, spawn, format, lay out), named by verb and object
4. Owners: each host and package a file calls, and each job's candidates, listed whole from their own listing per `references/owners.md`
5. Members: each member called and each candidate, read at its installed release through `search:code`
6. History: `git log -S '<literal>'` for a guard, flag, or form whose reason code and readers omit, a form with no cause in history, source, or input goes

Reading ends in statements editing starts from:
- Purpose: what each file is for, who calls it, how it runs, each input it serves, and each declaration's fact
- Behaviors: each case, refusal, fix, output, error, and listed row a file holds, with its line, mapped to a rebuilt site before commit
- Hand copies: each job of step 3 with its owner member, ranked by lines that member removes

Decisions come from facts read: an instruction is followed for the problem it names over its wording, an objection names a fact from the read, a question one read, source, or run answers is decided in the pass with its fact reported, and an owner-held choice (product behavior, a public contract, a design fork with costs on each side) goes to the owner or conductor in one message when first met, the pass continuing on everything it does not decide.

## [03]-[EDITING]

Large removals come from deleting hand copies of what an owner performs, and small edits around a hand copy leave it standing. Editing follows one target shape stated before the first edit, never a run of patches:
1. Owners: each hand copy replaced by its owner member, largest first, as one member call deletes declarations later steps rebuild
2. Removal: each declaration meeting its `[06]` criterion deleted, every consumer rebuilt in the same change
3. Logic: spam and nesting rebuilt by `[07]`
4. Names: each name in its field's term, verbs for functions, nouns for values

Each edit takes its rebuild over its bad form:

| [INDEX] | [FOUND]                                                            | [REBUILD]                                                   |
| :-----: | :----------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Lines trimmed around a hand-written decoder, parser, or codec      | Copy deleted with its tests, owner member called            |
|  [02]   | Hand copy kept as faster or more convenient than its owner         | Owner called, a copy stays for a measured requirement alone |
|  [03]   | Hand copy differing from its owner on some input                   | Owner's behavior, the difference a defect the copy brought  |
|  [04]   | Custom grammar, schema, or format beside a tool's built-in one     | Built-in used, custom files, build steps, and rows deleted  |
|  [05]   | Script, `sed`, or `python3` rewriting code text                    | File write tool for a reshaped file, edit tool per hunk     |
|  [06]   | Refused compact form answered by restoring the spam                | Rule named from its source, next form it accepts            |
|  [07]   | Export, constant, row, helper, type, or table added with no reader | Reader in the same change, code it replaces removed         |
|  [08]   | Helper, private member, constant, or export born of a defect fix   | Fix inside the member holding the defect                    |
|  [09]   | New export where an existing member can take the case              | Better behavior of the existing member                      |
|  [10]   | Edit to a file another agent edits, from an earlier read           | File read again before each edit, own lines changed alone   |
|  [11]   | Probe restored by a path checkout                                  | Edit tool, or `git show HEAD:./<path> > <path>`             |
|  [12]   | Shared member changed for every consumer, consumers named after    | Every consumer read and run before commit, or change scoped |

## [04]-[OWNERS]

Facts and behavior go to the highest owner stating them, and a hand copy of an owner's member is wrong whatever its quality:

| [INDEX] | [OWNER]                          | [STATES]                                                                 |
| :-----: | :------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Language and compiler            | Synthesized equality, hashing, coding, initializers, pattern match       |
|  [02]   | Standard library and runtime     | Collections, text, numbers, time, files, processes, JSON, URLs           |
|  [03]   | Platform SDK and host            | Names, paths, versions, events, dialogs, registries, behavior            |
|  [04]   | Package at its installed version | Parsers, codecs, protocols, schedules, layouts, diffs, globs             |
|  [05]   | Repository library               | Capability another library or product reads, extended in place           |
|  [06]   | Target declaration               | Fact no owner above states, kept when it removes more logic than it adds |

Owner reads run in order, each step deciding what the next reads:
1. Version: the one the project resolves, never a store glob or a home packages folder, as stale and differently patched copies sit beside it
2. Patches: local patches of a package, as they change behavior its documentation describes
3. Guide: a package's own agent guide (`AGENTS.md`, `llms.txt`, `ai-docs/`) read whole where the package includes one
4. Index: each owner's module, namespace, framework, or catalog section list read whole, as a job's owner sits in modules a target never imports
5. Search: each job's words (verb, object, format) searched across every installed declaration of the ecosystem, ranked by owner
6. Inventory: each candidate's whole member set written to a file before any member is read by name
7. Member: one member read from its doc comment's first line to its declaration's end

Package depth counts as much as deletion, and a pass that deletes alone is half a pass:

| [INDEX] | [FOUND]                                                   | [REBUILD]                                                               |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Members recalled from an older release                    | Installed release's declaration, newest per `[03]-[DEPENDENCY_POLICY]`  |
|  [02]   | Candidates limited to imported modules                    | Whole module index, as replacing members sit elsewhere                  |
|  [03]   | Inventory capped, sampled, or listed from symbol names    | Whole member set from a source's own listing, written to a file         |
|  [04]   | Empty search read as leave to hand-write                  | Index above it opened, each group of a job's field read whole           |
|  [05]   | Catalog searched by one word                              | Catalog read whole with group headings, as most rows lack a comment     |
|  [06]   | Package skill standing for an inventory                   | Skill loaded for forms, inventory read for members                      |
|  [07]   | Feature, extra, or companion capability written by hand   | Feature, extra, or companion enabled in its owning manifest row         |
|  [08]   | Input class worked around beside a dependency handling it | Dependency's member or option for that class                            |
|  [09]   | Repository library porting another language's library     | Each language's own ecosystem package for that job                      |
|  [10]   | Weaker version of a capability a repository library holds | Repository library extended in general form, each copy's reader rebuilt |
|  [11]   | Package semantic (concurrency, limit, depth) from memory  | Installed source read and one measured run, no code without both        |
|  [12]   | Package row removed while its capability is wanted        | Row kept, removed with its last import alone                            |
|  [13]   | Private module reached by a hand path                     | Public inference path reaching it                                       |
|  [14]   | File left unchanged with no input or member named         | Advanced input it serves and each package member it uses, named         |

Kept hand-written jobs name the owner groups read and why none fits, and a job naming neither goes back to `[03]` step 1.

## [05]-[HOSTS]

Hosts (applications, runtimes, build systems, checkers, generators) perform behavior and state values. Code calls a host member for each behavior and reads a host declaration or host state for each value, and a hand copy of either diverges at the next host release:

| [INDEX] | [FOUND]                                                          | [REBUILD]                                                    |
| :-----: | :--------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Code opening, saving, quitting, styling, or laying out by hand   | Host member performing that behavior, called                 |
|  [02]   | Code repeating a dialog, save, instance, or focus a host handles | Code removed, host performs it                               |
|  [03]   | Name, path, size, version, key, or dialog kind as a literal      | Read from host declaration or state stating it               |
|  [04]   | List of applications per property (single instance, own dialogs) | Property read from installation or running process           |
|  [05]   | Version-keyed branch, path, or file kind                         | One path on newest release and betas                         |
|  [06]   | Panel, dialog, prompt, or theme drawn from primitives            | Host kind built by host members, diagnostics to logger       |
|  [07]   | Pass hiding, killing, relaunching, or duplicating a host         | Documents opened in running instance behind user windows     |
|  [08]   | Dialog hidden, dismissed by a timer, or waited out; event polled | Dialog read and answered through its host, event awaited     |
|  [09]   | Row, output, or case deleted with its naming literal             | Derivation listing the same row, or value moved to caller    |
|  [10]   | One declaration file of a host read                              | Every declaration file a host writes, augmentations included |

Host facts come from host-owned sources, in order:
- State: registries, receipts, databases, and property lists a host writes (`Info.plist`, LaunchServices, plug-in registries), read at run time
- Declarations: installed typings, scripting dictionaries, object models, and decompiled assemblies at the installed build, through `search:code`
- Reference: the newest release's reference for a member installed declarations lack, then one proving run
- Driving: `rhino:host`, `blender:host`, Atelier for Adobe apps, Rasad for other apps, each against the running instance
- Gaps: `tool:inspect` for a fact host documentation lacks or contradicts

Library code names no application, product, or host, and removing a name keeps everything it served:
- Coupling: a literal naming one application becomes a derivation from host state listing the same row, or caller data at a product boundary
- Listing: rebuilt listing output is compared with the base's on this machine, and each missing row is a lost capability
- Absent value: a value no host source states enters as caller data at a boundary, stated once, and the library derives, defaults, and composes
- Library values: library code holds no product name, style name, palette, sheet size, margin, path, or tuned value
- Absent behavior: a behavior no host member performs composes host kinds and package members over caller data, never a copy of a host member
- Older installs: they load built products alone, and no code targets them
- Checker: a host validator or compiler deciding structure is read from its source, and its accepted forms decide every later site of that form
- Proof: a host behavior claim holds once a run on a real document in a running host showed it, as a run elsewhere proves nothing

## [06]-[REMOVING]

Declared names are every name a reader must know, internal (helpers, fields, locals, nesting, parameters) and external (exports, public parameters, flags, files, entry points, dependency rows), and both reduce by deletion and folding, never by nesting or hiding. Declarations are junk by default per `[04]-[CODE_QUALITY]` of the project instructions, and one stays when no owner states its fact and keeping it removes more logic than it adds.

Each kind goes on its criterion and takes its replacement:

| [INDEX] | [KIND]                 | [GOES_WHEN]                                                            | [REPLACED_BY]                         |
| :-----: | :--------------------- | :--------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Struct, record, class  | Owner states its fields, or keeping it removes no logic                | Owner's type, tuple at the reader     |
|  [02]   | Enum, union            | Variants repeat a type's, one is built, or no caller branches on them  | Repeated type, existing variant       |
|  [03]   | Alias                  | Renames a type, has one reader in its module, or dodges a clash        | Named or qualified type at each site  |
|  [04]   | Constant, string row   | Owner states the value, or one site reads it                           | Read from the owner, inline value     |
|  [05]   | Bool pair, flag        | Selects among cases, or holds a fact the target derives                | One enum, derivation from the target  |
|  [06]   | Wrapper, adapter       | Forwards a call unchanged, or converts as a package member does        | Direct call, package's conversion     |
|  [07]   | Helper                 | Born of this pass, one reader, or dodges a name clash                  | Body at the reader, renamed owner     |
|  [08]   | Interface              | One implementation and no seam a caller fills                          | Concrete type                         |
|  [09]   | Parameter              | Every caller passes it alike, none passes it, or the target derives it | Folded into the owner, derivation     |
|  [10]   | Export                 | Internal member exposed, or no capability named                        | Private declaration                   |
|  [11]   | Re-export              | Forwards a whole crate or a member no public signature exposes         | Own capability, direct reference      |
|  [12]   | Error variant          | No caller branches on it                                               | Owner's error                         |
|  [13]   | Guard, fallback, retry | Proves own state, no entry point reaches it, or hides a cause          | Construction refusing it, cause fixed |
|  [14]   | File                   | Alone in or named for its folder, or one helper                        | Sibling file, fact at its owner       |
|  [15]   | Generator              | Output equals input                                                    | Source read directly                  |
|  [16]   | Dependency row         | No code imports it and no capability of the pass wants it              | Nothing, catalog row stays            |
|  [17]   | CLI flag, subcommand   | Tool derives the value from its target or state                        | Derivation, one name per operation    |

Criteria deciding a row:
- Readers: reader count decides private declarations alone
- Public API: an export, a capability a plan names, a setting row, and a host entry point stay without readers, rebuilt to their owner's shape
- Re-export: each consumer use of a re-export becomes library capability, a type a signature exposes, or a direct import of the dependency
- Library: a library choosing a caller's entry point, runtime, transport, or configuration becomes parts callers compose over dependency types
- Entry: a guard, flag, arm, or default goes once each entry point reaching it (Nx target, CLI switch, host launch, hook, product) ran without it
- Proof: a removed mechanism's cause is proven by source at the installed release or a probe changing that one cause
- Capability: each input a removal stops handling is named, one named input restores the removed code, and a dropped capability fails the pass
- Self-validation: a check proving a program's own code or configuration goes (a re-parse its type guarantees, a cap on terminating recursion)
- External checks: checks over external input and host responses stay
- Cause: a guard, fallback, retry, clamp, lock, or wait names the cause it covers, fixed at its owner in the same change
- Restart rows: a supervisor's restart row is host configuration and no retry
- Divergence: behavior kept apart from its source names source, behavior, and a repository input triggering it, and no input means it goes
- Suppression: each suppression names its reason, and one a catalog or manifest row makes false goes with its finding fixed
- Remedy: a library failure names what failed and why, and the layer owning tools or commands adds the remedy or next call
- Injection: values a build tool or host supplies reach code through its typed channel (an import, a generated module), never a declared global

Removal is total across the tree: every mention, consumer path, manifest row, skill line, and README line goes, found by `rg -F` and `ast-grep run`, nothing stands in, and a removed mode, export, flag, or field takes every consumer in the same change.

Agent-written code repeats these forms in every language, each found by behavior and rebuilt from the owner a read named:

| [INDEX] | [FOUND]                                                                | [REBUILD]                                                      |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | Helper doing a platform member's job (memo map, pair zips, sum loop)   | Platform member (insert-or-compute, n-ary zip, built-in sum)   |
|  [02]   | Encoded-to-domain conversion by hand (try, parse, then construct)      | Package codec stating both directions, construction by schema  |
|  [03]   | JSON Schema, protocol list, or enumeration written beside its package  | Package member generating it, enumeration read from it         |
|  [04]   | Spawn of a tool for values another owner resolved                      | Value read from the owner that resolved it, passed by caller   |
|  [05]   | Parse of a tool's printed text where typed or structured output exists | Typed API, structured output flag, or built-in module          |
|  [06]   | Text test on a path, version, or identifier (prefix and slash, split)  | Relation member of its type, one member every site shares      |
|  [07]   | Hand memberwise initializer, equality, hash, or order                  | Language-synthesized form, hand member deleted                 |
|  [08]   | Helper copied into each sibling project, an exclusion hiding it        | One member in their shared library, each reader rebuilt        |
|  [09]   | Service shape restating its implementation member by member            | Shape derived from its builder's value or success type         |
|  [10]   | Union or key list restating the keys of a record                       | Type derived from that record's keys, cases listed once        |
|  [11]   | Type and builder pair differing in case or prefix                      | One name, the other form derived from it                       |
|  [12]   | Record per error or fault kind holding like fields                     | One error type with a case per kind callers branch on          |
|  [13]   | Tables keyed apart by one discriminator (allowed set, refusal map)     | One table keyed once, each row holding every fact of its key   |
|  [14]   | Decode of fields no reader reads, a schema per request and stored form | Read fields decoded alone, other forms derived from one object |
|  [15]   | Annotation, description, or schema no host, model, or reader receives  | Deleted                                                        |
|  [16]   | Rules or arms each parsing one input again                             | One reader parsing once over one table of its cases            |
|  [17]   | Helper taking as parameters what its caller's scope holds              | Logic inside the scope holding its inputs                      |
|  [18]   | Discard, cast, or declaration added at each site to quiet one rule     | Rule rebuilt or turned off in its row, each forced form gone   |
|  [19]   | Form a deleted rule forced, kept after the rule went                   | Form the language states, at every site                        |
|  [20]   | Fold hiding a use from a checker (names moved into a loop)             | Form the checker reads, every use visible                      |
|  [21]   | Message joined around fragments, one empty in some case                | Choice between complete messages derived from typed fields     |
|  [22]   | Sentence spelled per case, call site, or table row                     | Text derived once from a value's case and fields               |

## [07]-[LOGIC]

Members state each decision once, at one level, as a chain of steps a reader holds one at a time. Flat spam (near-identical statements, arms, or rows), ragged depth, wrappers, and helpers are one defect: a decision written more than once, or a step written where its owner performs it.

Rewrites keep every behavior of the base and change its shape:
- Behavior list -> each row of `[02]` maps to its site in the rebuilt file before commit, and a row with no site takes its base code back
- Refused form -> its rule named from the refusal text or source, then the next form the rule permits, never the spam restored
- Compiler limit (type-check timeout, crash on a packed expression) -> the language skill's form for that construct, never the base shape by default
- Line count -> read as statements and declarations removed, and a line joined or a body packed onto its declaration line is a regression
- Proof -> each rebuilt member runs on inputs it served and yields the values, order, and failures it yielded before
- Wire shape changed on purpose -> old and new outputs compared through one normalizer on repository input, never a gate on old output
- Defect found by a read -> proven by one discriminating input from repository data failing on old code and passing on new

Nesting depth counts levels of logic the formatter prints at project width, each level a step or decision a reader holds open through the next:
- Calls and callbacks sharing one printed line add no level
- Data counts no level: literals and callbacks they hold, schema and table declarations, markup trees, test group bodies, API builder chains
- Limit: the level count of `[04]-[CODE_QUALITY]` of the project instructions, and each checker's own count (callbacks, generators) from its configuration
- Ragged depth: a member whose depth rises, falls, and rises again is a chain written as a tree, a defect under the limit
- Guard forms a language gives an early exit (`guard let`, `let ... else`, `?`, `yield*`) are binding chains and stay

Members nesting past the limit or ragged rebuild deepest first, each step taking the earliest form that holds:
1. Format the file and name each level by its step's verb (decode, look up, choose), a level taking more verbs holding more steps
2. Remove each step changing nothing a later step reads: a conversion the next step undoes, a wrapper built to unwrap, a check its type guarantees
3. Replace code performing a step with a host or package member performing it
4. Restate the remaining steps in order as one flat chain (binding chain, pipeline, statement sequence), one step per line or statement
5. Rebuild an operation's signature where callers handle its result alike (same match arms, a value it knows passed in), with every consumer

Fixes that keep shape and add a name, or change what a member returns or raises, are no rebuild:

| [INDEX] | [FORBIDDEN]                                                                      | [REBUILD]                                         |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Lines joined, body packed onto its declaration, print width raised, packed chain | Level removed from logic, one named step per line |
|  [02]   | Statement made an expression, value lifted into a wrapper to skip a map          | Statement in a block, plain value read in the map |
|  [03]   | Helper or local function holding an inner level                                  | Member performing the step, called in place       |
|  [04]   | Closure bound to a name for an inline callback                                   | Callback inline in one flat chain                 |
|  [05]   | Wrapper or export around a nested call                                           | Direct call                                       |
|  [06]   | Type born of the pass naming an inner value                                      | Value inferred at its reader                      |
|  [07]   | Body moved to another member, private member, or file                            | Sequence restated in place                        |
|  [08]   | Caller locals routing around a callee's signature                                | Callee's signature rebuilt with every consumer    |
|  [09]   | Traversal swapped for one stopping sooner, presented as flattening               | Same items run, same failures raised              |

Each shape replaces one spam or nesting form:

| [INDEX] | [SHAPE]                               | [REPLACES]                                                     |
| :-----: | :------------------------------------ | :------------------------------------------------------------- |
|  [01]   | Match over a tuple of subjects        | Nested conditions over more than one discriminant              |
|  [02]   | Arm guard                             | Inner condition one arm alone reads                            |
|  [03]   | Binding chain                         | Early returns, stacked unwrap guards, binds over a nested call |
|  [04]   | Release by the handle's protocol      | Bracket of acquire, use, and release callbacks                 |
|  [05]   | Shared step hoisted                   | Same statement repeated in every arm                           |
|  [06]   | Match in owner, miss as parameter     | Same match arms at every caller of an absent result            |
|  [07]   | Table or relation                     | Arms or rows differing by one value                            |
|  [08]   | Keyed record typing its own keys      | Row per key beside a hand-written union of the same keys       |
|  [09]   | Iterator pipeline                     | Index loops, side collections, collect then filter or walk     |
|  [10]   | Pipeline over a product, keyed lookup | Loop inside a loop with a filter                               |
|  [11]   | One failure channel                   | Failure handler around each step                               |
|  [12]   | Accumulating collect                  | Loop with a mutable error list                                 |
|  [13]   | Combinator choosing a value           | Decision inside a default-supplying closure                    |
|  [14]   | Builder fold                          | Sequence of mutations on one object                            |
|  [15]   | One parameterized member              | Members differing by a value, flag, or case                    |
|  [16]   | Tagged union, fields per case         | Kind field beside optional fields per kind, type per variant   |

Algebraic forms hold where a relation is real and are never forced:
- Relation: quantities a caller computes from each other are one derivation solvable for any of them, a member or file per quantity fails
- Choice: a caller's choice among methods is an enum parameter, a `bool` holds a computed fact
- Deliberate: an enum, struct, or nesting stays when a caller branches per variant or it names a real relation, a type naming neither goes
- Catalog: a standard set callers extend is the generated enumeration beside one validated custom case, never a hand list beside a constructor
- Helper kept: a generic parameter relating types, a host constraint, or a recursion holds a helper, and every other helper's body sits at its reader

Strings hold text a reader receives or a host's or wire's fixed token, and behavior derives from the value a string names:
- String compared, switched on, or joined to decide behavior -> logic over the value it names, each case derived from its owner (type, member, host)
- Closed set the code branches on -> the language's tagged union or enum, each case holding its behavior or data
- Host or wire string -> decoded once at the boundary into the host's type, consumers never see the string
- Enum whose cases convert back to the same strings at every use -> renames them and goes
- Logic reading text shape, order, or spelling (a message pattern, a split index, a prefix test) -> the typed source or package predicate
- Emitted text -> follows the OUTPUT lines of the global instructions
- Code kept short by an erased error, an untyped read, a discarded message, a dropped case, or a loose decode -> its typed form

Members get faster through fewer passes, allocations, and parses over one input, and speed from a lock, a wait, a narrowed class, a serialized stage, or a dropped case is a regression:
- Pass: each pass, allocation, parse, or collected copy removed is named (a collect before a filter, a second walk zipping by index)
- Level: each level touched is named (grammar, regex class, rule shape, util, file walk, cache key, checker option), as another level hides the cause
- Capability: every input handled before is handled after, and a narrowed grammar, class, or skipped case is a regression with a timing
- Cost: linear-scan lookups, repeated walks or parses, unbounded recursion, per-element copies, and resources held past use are rebuilt
- Concurrency: many agents share the tree, and correctness comes from hashing, caching, and per-task output folders

## [08]-[CLOSING]

Passes close in order, and a pass with a red step stays open:
1. Diff: `git diff <base> -- <paths>` and each untracked file read whole against the regression table below, each hit rebuilt before any check
2. Behaviors: each behavior statement of `[02]` has its site in the rebuilt code, and one with no site is restored
3. Format: `hk fix <paths>`, each file it changed read again, as an autofix can drop a binding
4. Checks: `nx run <project>:check` of each edited and consumer project, each alone in the background, read as counts per file and failing lines
5. Fixes: each finding fixed at its cause and step 4 run again until zero findings, defects present before the pass included
6. Runs: each entry point reaching an edited member once on repository input, served products live
7. Commit: one commit per pass once steps 1 to 6 hold

Each regression a diff read finds takes its rebuild:

| [INDEX] | [KIND]            | [SIGN]                                                                     | [REBUILD]                               |
| :-----: | :---------------- | :------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | Growth            | Lines, declarations, exports, or files above base, no capability named     | Removed, or capability and its reader   |
|  [02]   | Lost behavior     | Case, refusal, fix, input, output, or listed row with no site              | Restored at its site                    |
|  [03]   | Lost nesting      | Capture or restore at the top level, nested levels overwritten             | Capture at every level written          |
|  [04]   | Nearest for exact | Lookup returning a neighbor's entry on a miss                              | Exact match tested, nothing on a miss   |
|  [05]   | Narrowed input    | Predicate, level, class, or grammar narrowed, rows lost                    | Same inputs, or narrowing per consumer  |
|  [06]   | Changed identity  | Resource name, cache key, stored key, or label renamed                     | Identity kept, or outside state moved   |
|  [07]   | Host compiler     | Newer language form in a file a host's own compiler builds                 | Form the host's compiler accepts        |
|  [08]   | Weakened check    | Lint lowered, rule narrowed, suppression, `files` exclusion, lint branch   | Finding fixed at its cause              |
|  [09]   | Snapshot gate     | Check comparing output with a pre-change copy                              | Old and new through one normalizer      |
|  [10]   | Shuffle           | Move, split, rename, alias, forwarder, barrel, re-export, extracted helper | Logic rebuilt in place                  |
|  [11]   | Hidden cause      | Lock, wait, retry, guard, hidden launch, kill, or flag over a cause        | Cause fixed at its owner                |
|  [12]   | Hand-written host | Host behavior in code, host value as literal, app list grown, version arm  | Host member called, value read          |
|  [13]   | Package loss      | Member replaced by hand code, wanted row removed, feature turned off       | Member called, row kept                 |
|  [14]   | Wrong removal     | Guard an entry point reaches, export or row cut for lacking readers        | Restored in its owner's shape           |
|  [15]   | Packed lines      | Lines joined, body on its declaration line, base shape kept to stay under  | Logic level removed, count stands       |
|  [16]   | Naive fix         | Fix another checker refuses, or a fix that does not build                  | Form every checker and compiler accepts |
|  [17]   | False safety      | Test that cannot fail on this machine, guard over an excluded state        | Test turned red by one input, guard cut |
|  [18]   | Product break     | Served product, plumbing, or host integration failing after the change     | Fixed in the pass, product run live     |

Checks cover what the pass reached:
- Type checks: `hk check <paths>` runs file-level checkers and no type checker or compiler, and a file outside any project takes it alone
- Shared build: Swift parts and briefs withholding a build, test, or live run compile nothing, `swiftc -typecheck` and probe programs included
- Shared build close: the file list goes to the conductor in one message, and the pass stays open until each returned diagnostic is fixed at cause
- Diagnostics: a fix keeps the rebuilt form, a type a compiler asks for is stated on the value that lost it, and no line count is paid back by packing
- Others' lines: a red finding in another agent's lines is proven theirs by `git diff` or `git log -L` and its error lines, with the edited project's own task green
- Outside scope: a finding outside the edit scope goes to the conductor or that file's agent in one message with site and proof
- Tests: no test is written, a suite covering an edited member runs with step 4, and a test the rebuild breaks takes the new shape or goes
- Tree: no copy or backup sits in the tree, and scratch files go once their result is recorded

Each report and commit form takes its rebuild:

| [INDEX] | [FOUND]                                                                     | [REBUILD]                                             |
| :-----: | :-------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Fix reported after a file-level check alone, "the next type check confirms" | Project check run, exit code and finding count stated |
|  [02]   | Type error answered with reasoning ("should type as") and no run            | Check run again, its output read                      |
|  [03]   | "For the conductor" or "for you to act on" list in a report                 | Each item fixed, or one message with site and proof   |
|  [04]   | "A consumer needed it", "left as is", "kept for the lint", "to be safe"     | Fact and source naming why it stays, or removed       |
|  [05]   | Commit body narrating forms or reasons the diff lacks                       | Body stating past facts the diff holds                |
|  [06]   | Kept declarations defended in prose                                         | Each kept declaration with the fact it states         |

## [09]-[REPORT]

Reports follow the OUTPUT lines of the global instructions, fields in order:
- `files`: one row per file, lines, declarations, exports, and deepest nesting before and after
- `owners`: each host or package member the rebuild calls, with its source path and line
- `behavior`: each behavior change with every consumer site it reaches
- `added`: each declaration the pass added, with its reader and the code it replaced
- `kept`: each declaration a rebuild left, with the fact it states
- `checks`: each command with exit code and finding count
- `build`: file list sent to the conductor, under a shared build alone
- `unverified`: each host behavior claim no running host proved, with the run deciding it, never a type check or compile within reach
- `commit`: hash and subject

Commits follow the pass:
- Subject: `refactor(<project>): <imperative>` with the scope `nx show projects` prints, no scope across projects
- Body: past facts of the diff (members called, declarations removed, behavior changed, consumers rebuilt)
- Staging: `git add -- <path>` per path
- Briefs withholding commits take the report alone
