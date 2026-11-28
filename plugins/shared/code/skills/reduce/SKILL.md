---
name: reduce
description: "Use when cutting a target's lines, declarations, exports, and nesting in focused passes that rebuild logic on its owners, in any language"
argument-hint: "<target> [<concern>]"
---

# [CODE_REDUCE]

Code grows by addition: each change adds a constant, a wrapper, a helper, or a type, and nothing takes the old form away. Reduction reverses it in passes. Each pass takes one concern over one unit, reads every file and consumer whole, maps who reads each declaration and who does like work without it, rebuilds in place on the owner that already states each fact (a host member, a package member, an upstream value, one shape in the tree), and closes with fewer lines, declarations, exports, and nesting levels and every capability kept. A count that rose names the capability it added, or the pass failed.

[REFERENCES]:
- [01]-[TYPESCRIPT](references/typescript.md): Skills, packages, inventories, forms, and checks of TypeScript targets

## [01]-[TARGET]

`<target>` decides what the first read covers and what one unit holds, `<concern>` names one category of `[07]` for the session:

| [INDEX] | [TARGET]          | [FIRST_READ]                                       | [UNIT]                                      |
| :-----: | :---------------- | :------------------------------------------------- | :------------------------------------------ |
|  [01]   | File              | File whole, every importer, every file it imports  | One concern across the file and its readers |
|  [02]   | Folder or project | Every file listed and read, ranked by findings     | One file, or one concern across files       |
|  [03]   | Symbol            | Declaring file whole, every reference site         | Symbol, its readers, its like work          |
|  [04]   | Test or suite     | Suite whole, code under test whole                 | One fixture or helper form across the suite |
|  [05]   | Commit range      | Each changed file whole at its new revision        | One concern the range introduced            |
|  [06]   | Smell category    | Every match of its detection across the named path | Matches of that category alone              |

- Empty arguments -> the owner names a target before any read
- Targets larger than one unit become an ordered unit list before any edit, run one unit per pass
- Targets spanning languages run one sequence per language, each with its reference file
- Languages with no reference file run this sequence with their coding skill and central package manifest

## [02]-[SKILLS]

Skills load before the first read of code, chosen by what the target holds:
1. Reference file of each language the target holds, whose skill table names that language's skills
2. Every coding skill that table matches, each language holding its own set (`typescript:coding` with `typescript:effect`)
3. Package skills the target's manifest imports (`dotnet:languageext`, `python:msgspec`)
4. `code:test` and the language testing skill when the target is a test or a pass breaks one
5. Host skill when target code runs inside a host (`blender:host`, `rhino:host`, `uxp:coding`, `sazandeh:mod-ui`)
6. `writing:names` and `writing:prose` for every name, message, and description a pass writes
7. `code:rebuild` as a checklist of removal kinds, its order of work yielding to this sequence

Memory lines matching the target's paths or packages open before the first read, and each body's banned forms join the map.

## [03]-[READ]

Reading ends in a statement of purpose per file, and no edit precedes it:
1. Tree: `git status --short` and `git log -5 --stat`, the live tree with uncommitted edits is the base
2. Inventory: list each file's declarations and exports with the reference file's inventory steps, counted
3. Read: each file in scope line by line whole, as an outline, excerpt, or summary standing in for a file misses shapes
4. Owners: read each package and host member the file calls at its installed version, with the reference file's package steps
5. History: `git log -S '<literal>'` for each guard, flag, or form whose reason code and readers do not state
6. Purpose: state per file what it is for, who calls it, how it runs, and each behavior with the member performing it

Facts go to the highest owner stating them:

| [INDEX] | [OWNER]                          | [STATES]                                           |
| :-----: | :------------------------------- | :------------------------------------------------- |
|  [01]   | Language and standard library    | Operators, collections, text, numbers, time        |
|  [02]   | Host API or SDK                  | Names, paths, versions, events, behavior of a host |
|  [03]   | Package at its installed version | Parsers, codecs, protocols, schemas, layouts       |
|  [04]   | Upstream value in the tree       | Enumeration, registry, reflection read once        |
|  [05]   | Declaration in the target        | Fact no owner above states, that removes logic     |

Reading stays inside the target and its consumers, and files outside them join the unit list by name.

## [04]-[MAP]

Each declaration in scope takes one map row before any edit, and the map decides the unit:

| [INDEX] | [COLUMN]    | [RECORDS]                                                                  |
| :-----: | :---------- | :------------------------------------------------------------------------- |
|  [01]   | Declaration | Name, kind, exported or file-private, line count                           |
|  [02]   | Fact        | Fact it states that no owner of `[03]` states                              |
|  [03]   | Readers     | Each reference site with the members and shape that site reads             |
|  [04]   | Like work   | Code doing the same work without reading it, found by behavior, never name |
|  [05]   | Cause       | Reason for its shape: host rule, type relation, history read               |

Readers decide shape:
- Members no reader touches stay when they state a capability, as a missing reader is never a removal reason
- Readers that each rebuild one value (a key list, a lookup, an unwrap) show the declaration holds the wrong shape
- Readers reading one member alone show a declaration wider than its use, narrowed by rebuilding the readers

Like work without a reader is a finding, and its cause is mapped before any merge:

| [INDEX] | [CAUSE]                                      | [ACTION]                                                |
| :-----: | :------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Shape too narrow for its second reader       | Widen the one shape, rebuild both readers on it         |
|  [02]   | Host or checker rule forbids the shared form | Name the rule from its source, keep forms it permits    |
|  [03]   | Type relation lost by merging                | Keep the declaration holding it, record why             |
|  [04]   | Duplicate written without a search           | Delete the copy, rebuild its readers on the original    |
|  [05]   | Upstream shape wrong for every reader        | Rebuild upstream first, then every consumer in one pass |

Causes come from the host, compiler, or checker before a rebuild relies on them:
- Host checkers decide structure: a plug-in validator following a value only into named function declarations forces a function per site
- Type checkers decide shape: a generic helper correlating a key with its value type stays, as inlining it fails to compile
- Inferred types can lose a fact a stated type held: a derived shape keeps one stated type on the value that lost it

## [05]-[UNIT]

Units hold one concern across every site it reaches, at a size one pass finishes with its checks:
- Concern: one category of `[07]`, one per pass, and findings of another category go to the unit list with their site
- Size: a unit too small to change a count joins its siblings, a unit reaching past one concern splits
- Sites: a unit includes every reader and like-work site of what it changes, a declaration with an old reader left is unfinished
- Design forks with more than one valid shape (one type or fields per feature, a name pattern) go to the owner with each option's cost

Passes run cheapest and widest first, so later passes read code earlier ones already reduced:

| [INDEX] | [PASS]               | [REMOVES]                                                               |
| :-----: | :------------------- | :---------------------------------------------------------------------- |
|  [01]   | Junk                 | Code meeting the proof bar of `[08]`                                    |
|  [02]   | Owners               | Hand-written code a host member, package member, or declaration states  |
|  [03]   | Private declarations | Forwarders, one-reader helpers, locals restating a value, dead branches |
|  [04]   | Values               | Loose constants, deciding strings, values a target derives              |
|  [05]   | Shapes               | Types, schemas, and objects no owner needs, like shapes held apart      |
|  [06]   | Exports and names    | Exports restating a declaration, case-only name pairs, coined names     |
|  [07]   | Nesting              | Ragged depth, decisions inside arms, binds over nested calls            |
|  [08]   | Consumers            | Second shapes of one concept across files, upstream causes              |

Owner passes come early, as one package member often deletes declarations later passes would rebuild, and export passes follow private ones, their readers rebuilt against declarations already reduced.

## [06]-[PASS]

Each pass runs one sequence:
1. Count: lines, declarations, exports, deepest nesting, and coined names of every file the unit touches
2. Decide: the rebuild, and the owner each removed fact moves to, with the inventory line proving the owner states it
3. Probe: one discriminating run where the rebuild depends on a host, checker, or compiler behavior no source settled
4. Edit: the declaration and every reader and like-work site in one change, no adapter, alias, or old path left
5. Check: the reference file's verify steps once, over touched projects and every consumer project
6. Recount: compare with step 1, a count that rose names the capability it added
7. Commit: one atomic commit per pass, its message naming category and unit

Probes decide before code depends on them:
- Probes change one variable and restore the tree with the file edit tool after, never with a checkout that drops other edits
- Probe refusal ends that design: revert it, record the rule it showed, and choose again from the map
- Repeated failure of one check for one cause ends the pass, and the cause joins the unit list as its own unit

Failures name their cause before the next edit:
- Type error after deriving a shape -> state the lost fact on the one value that lost it, the derivation kept
- Inference failing for a variadic or intersected parameter -> the form that inferred before stays, its type tightened
- Checker refusal of a rebuilt form -> its rule decides every later site of the same form
- Compile failure after inlining -> the declaration restored, its reason recorded

## [07]-[CATALOG]

Categories name a smell, how a read finds it, and the larger problem it signals:

| [INDEX] | [CATEGORY]       | [DETECT]                                                       | [SIGNALS]                             |
| :-----: | :--------------- | :------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Junk             | Declaration no path reads, wrapper adding no fact              | Code written for its look             |
|  [02]   | Loose constants  | Module-level literal read once, or restating a host value      | Value with no owner chosen            |
|  [03]   | Strings          | Literal compared, switched on, or joined to decide behavior    | Name standing in for a value          |
|  [04]   | Owned values     | Literal a host, SDK, package, or upstream listing states       | Upstream reflection not used          |
|  [05]   | Name pairs       | Exports differing in case or suffix alone (`Engine`, `engine`) | One concept split by a constraint     |
|  [06]   | Coined names     | Word no documentation of the field uses for the thing          | Concept not understood                |
|  [07]   | Divergent shapes | Like things built, typed, or called differently                | One owner missing, or one shape wrong |
|  [08]   | Type spread      | Struct, interface, alias, or string type per variant           | Cases no union holds                  |
|  [09]   | Hand-rolled      | Code doing what a package, host, or standard member does       | Dependency read shallowly             |
|  [10]   | Copies           | Same logic, or a weaker version, elsewhere in the tree         | Owner not found before writing        |
|  [11]   | Nesting          | Staircase of indentation deeper than the project limit         | Chain of steps written as a tree      |
|  [12]   | Guards           | Check, error, or fallback over a state construction excludes   | Type not carrying its proof           |
|  [13]   | Forwarders       | Helper with one caller, wrapper calling one member unchanged   | Indirection kept for its look         |

Rebuilds replace a string or constant with a derivation, never another constant, string, or enum renaming them. Each row is a category, never a pattern to match: the cleanest rebuild of one finding can differ from the last, so each is chosen from the owners `[03]` ranked and the readers `[04]` mapped.

## [08]-[JUNK]

Junk is code stating no fact a reader needs, and it goes whole:
- Annotations, schemas, and descriptions no host, model, or reader receives (22 schemas removed from one library carried annotations nothing read)
- Wrappers re-declaring a package type field for field, decodes of fields no consumer reads, types naming a value used once
- Proof bar: name every entry point reaching the code, run each once, and show no output, type, or behavior changes without it
- Absent consumers alone never meet the bar: public API, a capability a plan names, and a host entry point stay without readers
- Unread public members stay as capability and are rebuilt to their owner's shape, never deleted

## [09]-[VALUES]

Values come from their owner, and each literal names its owner or goes:

| [INDEX] | [FOUND]                                    | [REBUILD]                                              | [NOT_THIS]                       |
| :-----: | :----------------------------------------- | :----------------------------------------------------- | :------------------------------- |
|  [01]   | Constant read once                         | Value inline at its reader                             | Constant moved to another file   |
|  [02]   | Constants one relation joins               | One value holding the relation (`{ min, max }`)        | Prefix-named constant family     |
|  [03]   | Literal a host or package declares         | Read from the declaration (manifest, enum, SDK member) | Literal with a comment naming it |
|  [04]   | Literal per item (header per application)  | Derive each where items are listed                     | Table of literals per item       |
|  [05]   | String compared to choose behavior         | Logic over the value the string names                  | Enum renaming the strings        |
|  [06]   | String joined into a path, key, or command | Package builder (path join, URL, argv)                 | Template literal with a helper   |
|  [07]   | Number with a unit                         | Value of its module (`Duration`, byte size, instant)   | Number with a unit in its name   |

Derivation runs at the earliest point holding the source: where applications are enumerated, a header for each application derives there, and every downstream literal goes with its table.

## [10]-[NAMES]

Names state what a thing is in its field's words, one name per concept:
- Pairs differing in case or suffix fold into one name, where the language keeps types and values apart a type and its constant share it
- Pairs a host constraint keeps apart (a builder that must stay a function) take a verb-noun builder, its type derived from it
- Coined words take the term the field's documentation uses, and every reference, file, and folder follows through language tooling
- Names restating their module, a mechanism, or a past participle (`tallied`) take a noun for a value and a verb for a function
- Names a host, wire, or tool resolves stay exact, and the report names the coupling

## [11]-[SHAPES]

Like things share one shape, and different shapes for like things signal a missing owner:

| [INDEX] | [FOUND]                                  | [REBUILD]                                               | [NOT_THIS]                        |
| :-----: | :--------------------------------------- | :------------------------------------------------------ | :-------------------------------- |
|  [01]   | Types for one concept (part and module)  | One type, the narrower form derived from it             | Adapter converting between them   |
|  [02]   | Struct or interface per variant          | One tagged union, each variant's data as its fields     | Base class with a kind field      |
|  [03]   | Hand-written type restating code         | Type derived from its owning value or builder           | Duplicate kept in sync by hand    |
|  [04]   | String type standing for a closed set    | Union or enum the host declares, else one literal union | Constants named after each string |
|  [05]   | Optional fields set together             | One optional record, or a variant per valid state       | Boolean field beside each         |
|  [06]   | Reader narrowing a wide shape every time | Shape narrowed at construction                          | Guard repeated per reader         |

Divergence points one way or both: compare each shape with its readers and its siblings, and rebuild the wrong one, or both toward a third the field names.

## [12]-[LOGIC]

Logic calls the member doing the job and states each step once:

| [INDEX] | [FOUND]                                      | [REBUILD]                                         | [NOT_THIS]                         |
| :-----: | :------------------------------------------- | :------------------------------------------------ | :--------------------------------- |
|  [01]   | Parser, codec, or formatter by hand          | Package, host, or standard member doing it        | Helper wrapping the same hand code |
|  [02]   | Copy of logic already in the tree            | One owner, every copy's reader rebuilt to call it | Shared helper beside both copies   |
|  [03]   | Weaker version of an existing capability     | Existing capability extended in its general form  | Third version                      |
|  [04]   | Members differing by a value, flag, or case  | One member taking that value                      | Member per value                   |
|  [05]   | Guard over a state the type excludes         | Construction refusing the state, guard removed    | Lowered check level                |
|  [06]   | Error variant no caller branches on          | Owner's error                                     | Generic error string               |
|  [07]   | Helper with one caller, forwarder, re-export | Body at its caller, direct call                   | Rename of the helper               |
|  [08]   | Walk repeated over one input                 | One walk, keyed lookups                           | Cache in front of the second walk  |

Helpers stay when they carry a fact their caller cannot hold: a generic parameter relating types, a host constraint, or a recursion.

Package depth decides the largest reductions:
- List every member of each dependency the unit imports from its installed source before rebuilding hand code
- Search members by the job a block does (parse, retry, schedule, diff, encode), as a search by name misses the rest
- Order rebuilds by lines a member removes, largest first, each its own pass, as large files halve through passes and never through one import

## [13]-[NESTING]

Nesting counts levels of logic a reader holds open, in any language, as the project formatter prints them, and data literals, schema declarations, and markup trees count for none:

```text
step
    decide
        step
            decide
                step
        step
    step
        decide
```

Ragged depth is a chain of steps written as a tree, and it flattens by rebuilding the chain in place, each step taking the first form that holds:
1. Name each level by its verb (decode, look up, choose, apply)
2. Remove steps changing nothing a later step reads: a conversion the next undoes, a check its type guarantees
3. Replace a level with the package or host member performing it
4. Restate remaining steps as one flat chain (pipeline, binding chain, statement sequence), one step per line
5. Turn nested decisions over more than one value into one match over a tuple of them
6. Move a decision repeated in every caller into its owner's signature, and rebuild each caller

| [INDEX] | [FOUND]                                    | [REBUILD]                                | [NOT_THIS]                      |
| :-----: | :----------------------------------------- | :--------------------------------------- | :------------------------------ |
|  [01]   | Early returns and stacked unwraps          | Binding chain                            | Helper holding the inner level  |
|  [02]   | Nested conditions over more than one value | Match over a tuple                       | Lines joined, width raised      |
|  [03]   | Inner condition one arm alone reads        | Arm guard                                | Closure bound to a name         |
|  [04]   | Acquire, use, release brackets             | Release by the handle's own protocol     | Bracket moved to another file   |
|  [05]   | Collection built, then walked again        | One pipeline                             | Second loop beside the first    |
|  [06]   | Same nesting at every caller of one member | Member's signature rebuilt, callers flat | Caller locals routing around it |

Same depth across a member's readers signals an upstream shape problem: the owner returns a form every reader unwraps, and its rebuild reaches all readers in one pass.

## [14]-[CLOSE]

Passes close on counts and runs, never on a reading of the diff:

| [INDEX] | [COUNT]          | [PASSES_WHEN]                                                    |
| :-----: | :--------------- | :--------------------------------------------------------------- |
|  [01]   | Lines            | Fell, or rose by a capability the pass names                     |
|  [02]   | Declarations     | Fell, each kept one listed with the fact it states               |
|  [03]   | Exports          | Fell or held, each kept export read by a consumer or a named API |
|  [04]   | Nesting          | Deepest level fell or held at the language's limit               |
|  [05]   | Coined names     | None left in touched files                                       |
|  [06]   | Checks and build | Each touched project and each consumer project passes            |
|  [07]   | Entry points     | Each entry point ran on repository input with its normal output  |

Counts kept low by an erased type, an untyped read, or a dropped case are a regression.

Report each pass in this form:
- Subject: one line naming target, unit, and category
- Counts: before and after per file, rows in one field order
- Kept: each declaration a rebuild left, with the fact it states and the cause that kept it
- Rules: each host, checker, or compiler rule a probe showed, with the command that showed it
- Units left: each finding outside the pass, with its site and category
