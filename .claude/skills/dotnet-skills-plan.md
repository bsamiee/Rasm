# [DOTNET_SKILLS_PLAN]

Four skills form the C# coding standard: `dotnet-coding`, `dotnet-coding-languageext`, `dotnet-coding-thinktecture`, and `dotnet-coding-mapperly`. Their material is the research under `docs/research/dotnet/`, and the plan records where each piece of that material belongs, how the skills divide the ground, the process that moves content from research into a skill, and the findings from the first review of `dotnet-coding/SKILL.md`.

## [01]-[SOURCES]

The research folders were extracted from instructional sources (2 textbooks, an article series, and package documentation) and then corrected and compacted over many sessions. They are complete in facts and wrong in framing: a textbook narrates, motivates, and repeats, and a skill instructs. The work restructures that content, one section at a time, into instructions.

| [INDEX] | [FOLDER]            | [CONTENT]                                                       | [OWNER]                                     |
| :-----: | :------------------ | :-------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `functional-csharp` | Files 01 to 20 from the textbooks, 21 to 24 the library catalog | `dotnet-coding`, 21 to 24 seed languageext  |
|  [02]   | `languageext`       | 10 articles deriving traits and transformers on toy types, API  | `dotnet-coding-languageext`, ideas only     |
|  [03]   | `thinktecture`      | Value objects, smart enums, unions, factories, Serilog, settings | `dotnet-coding-thinktecture`                |
|  [04]   | `mapperly`          | Boundary mapping                                                | `dotnet-coding-mapperly`                    |

The `languageext` folder is not higher-order-function material. Its toy `Maybe`, `List`, and `MaybeT` code must appear in no skill, because a hand-rolled option type beside `Option<A>` is the mixed approach the standard forbids. The ideas survive, rewritten on `Option`, `Seq`, and `IO`: applicative versus monadic evaluation structure, traversal semantics per applicative, transformer layer order under `Run`, domain-monad wrappers with `Deriving`, forked `StateT` state that does not merge back, and `Writer` output combined in `tell`. The higher-order-function material for `dotnet-coding` is files 06, 07, and 09.

## [02]-[OWNERSHIP]

Each skill owns one kind of fact, and a fact appears in the skill that owns it and nowhere else:
- `dotnet-coding` owns decisions: which type, which operator, where the boundary sits, what stays pure, how a signature is shaped
- `dotnet-coding-languageext` owns the library's types and their operations: conversions, `Catch` overloads, schedule algebra, traits and `K<F, A>`, transformers, collections, `Atom` and `Ref`, `Source` and `Conduit`, runtimes and `Has`
- `dotnet-coding-thinktecture` owns declaring value objects, smart enums, and unions, their generated API, settings, and framework integration
- `dotnet-coding-mapperly` owns mapping at the host boundary

The standard treats the 3 packages as part of the language, and that creates a matrix: the core files had to be written with each package in mind, and each package file had to be written with the core and the other packages in mind. The rule that keeps the matrix manageable is that a snippet stays on its own subject and does not extend to show another package, but when the functionality it shows belongs to a package, it uses that package. Both failures are visible and both are wrong: a package forced into a snippet where it does not belong, and a snippet that hand-rolls what a package owns (a bool flag where a union belongs, a `List<T>` where `Seq<A>` belongs, an exception where `Fin<A>` belongs). File 04 showed the first failure, its EXPLICIT_ALTERNATIVES section explained `[Union]` mechanics and listed the runtime shape of every result type inside a language-features file.

Three rules apply the ownership split inside `dotnet-coding`:
- The intro alone names each sibling skill, in one line with what it owns
- Every example uses the sibling types as vocabulary (`Seq`, `Fin`, `Validation`, `IO`, `Age.From`, `[Union]`) with no BCL or hand-rolled alternative beside them
- The skill holds no mechanics of a sibling (attribute settings, generated-API listings, conversion catalogs)

CLAUDE.md already names the principles as TOTALITY, FLOW, INDEPENDENCE, PURITY, and BOUNDARY, and the skills are their C# realization. No skill restates a principle, and no skill has a "core principles" section. Pattern matching and expressions are mechanisms that serve TOTALITY and FLOW, immutability, purity, and higher-order functions are properties of code, and each gets a section named for the thing.

## [03]-[PROCESS]

The work proceeds by small moves with a read between each: read the target section and its research source, move or rewrite that one section, comment out the integrated research, read again. One pass over everything loses facts and produces the duplication the first review found.

- Write every section as if the `references/` files and the 3 sibling skills exist, so no fact is added twice while they are being built
- Comment out research content in place with `<!-- -->` once a skill or reference holds it, a later session then sees what is done and what remains
- Move content removed from a skill to its research file or to a stub reference, so nothing is lost or forgotten
- Choose the strongest research section for a fact when files cover it at different depths, integrate that one, and comment out the weaker ones where they duplicate it
- Fix prose first and structure second, and keep each move inside one section
- Test each fact against one question, and place it in `SKILL.md` when leaving it out lets an agent violate a standard, or in a reference when leaving it out only slows the agent down
- Defer to a reference from the section that owns the topic, as a one-line pointer

A skill drives behavior, it is instructions to an agent. Framing decides what the agent writes:
- State what to do, and confine "do not" to an anti-pattern table with the correct form beside each row
- Keep every rule at the abstraction level the workspace uses, so an agent does not invent result flow in the wrong layer or a type per snippet
- Keep snippets in `SKILL.md` small and complete, one shape per rule
- Keep reference snippets non-anchoring: a category of language functionality shown in one coupled way becomes the only way an agent writes it
- Write every snippet with placeholder names from the start (`Command`, `State`, `Item`, `Quantity`), translated to a coherent shape rather than copied from the research, name every local by its type rather than `var`, and keep it rich enough to show the rule without hardening one shape into the only way an agent writes that construct. A placeholder keeps one shape within a file, each file's snippets stand on their own, and the same name (`Entry`, `Snapshot`) can take another shape in another file

## [04]-[SKILL_SHAPE]

`dotnet-coding/SKILL.md` holds about 330 lines in 6 sections, each mapped to the standard it serves:

| [INDEX] | [SECTION]    | [STANDARD]                   | [CONTENT]                                                                                 |
| :-----: | :----------- | :--------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | Intro        | All                          | Scope, 3 sibling pointers, `Prelude` import, vocabulary rule                              |
|  [02]   | FUNCTIONS    | TOTALITY, PURITY             | Arrow-to-delegate table, honest signatures, purity, closures, currying, argument order    |
|  [03]   | EXPRESSIONS  | FLOW                         | Expression versus statement, patterns, expression-bodied construction, tuples, loop table |
|  [04]   | IMMUTABILITY | PURITY                       | `record`, `init`, `readonly`, deep-immutability rules, transitions, collection table      |
|  [05]   | RESULTS      | TOTALITY, FLOW, INDEPENDENCE | Type-per-concern table, boundary rule, operator selection, `Expected`, unions, exceptions |
|  [06]   | EFFECTS      | PURITY, BOUNDARY             | `IO` as the effect type, injection rule, `use` and `Bracket`, retry, traversal policy     |

Decision tables and rule lists pass the placement test, worked flows fail it. File 08 fails it: its operations table merges into the operator table of `SKILL.md`, and its tutorial is `dotnet-coding-languageext` material. Each reference starts as a stub with an HTML comment naming the research sections it draws from, and the stub fills when its research is restructured.

## [05]-[DISPOSITION]

Each `functional-csharp` file splits between `SKILL.md` and one of 8 references: functions, sequences, results, immutable-data, effects, state, streams, event-sourcing.

| [INDEX] | [FILE]        | [SKILL]                                                            | [REFERENCE]                                        |
| :-----: | :------------ | :----------------------------------------------------------------- | :------------------------------------------------- |
|   [01]  | 01 model      | Drained                                                            | None                                               |
|   [02]  | 02 purity     | Purity definition, injection rule, no mutable statics              | effects: `Zip` counter, testability shape          |
|   [03]  | 03 signatures | Arrow-to-delegate table, honest signatures, `Unit`, nullable rule  | None, `Option` internals go to languageext         |
|   [04]  | 04 features   | Drained after dropping the 2 remaining sections                    | None                                               |
|   [05]  | 05 sequences  | Expression-bodied construction, `Fold`, indexed `Map`              | sequences: `Iterable`, median, CSV pipeline        |
|   [06]  | 06 higher     | Behavior parameterization, factories, `compose` versus chaining    | functions: `Pipe`, `Fork`, `Do`, predicates, rules |
|   [07]  | 07 currying   | Currying versus `par`, stable-first order, function dependencies   | functions: parser families, `Query<RT, T>`, `Book` |
|   [08]  | 08 patterns   | Operations table, values-in-context table                          | languageext skill                                  |
|   [09]  | 09 compose    | Composable-function properties, operator table, layering rule      | functions: `OptionT<IO>` repository flow, limits   |
|   [10]  | 10 unions     | Design rules, flags-to-cases example, classify once at the source  | results: recursive `Json` fold, `UserInput`        |
|   [11]  | 11 errors     | Type choice, `Expected` with `Codes`, classify by code, exceptions | results: fail-fast workflow, `MapFail`, HTTP       |
|   [12]  | 12 applicat.  | Selection guide, `Traverse` versus `TraverseM`, laws in 1 line     | results: lift-then-apply, validator folds, tests   |
|   [13]  | 13 immutable  | Deep-immutability rules, decision rules, `With` and lenses         | immutable-data: persistent list and tree cost      |
|   [14]  | 14 events     | Nothing                                                            | event-sourcing: whole file                         |
|   [15]  | 15 lazy       | Call-by-value cost, lazy `\|\|` fallback, `use` and `Bracket`      | effects: `Try`, `Reader`, scope ordering           |
|   [16]  | 16 state      | `S -> (A, S)` in 1 line                                            | state: rate cache, generators, tree numbering      |
|   [17]  | 17 loops      | Approach table                                                     | state: trampolines, `Monad.recur`, `unfold`        |
|   [18]  | 18 async      | `IO` as the effect type, failure policies, traversal policy        | effects: stacked effects, host exits               |
|   [19]  | 19 observ.    | Fit-and-limits list                                                | streams: operators, per-item `Fin`, backpressure   |
|   [20]  | 20 agents     | Synchronization strategy table                                     | streams: agent, entity process, registry           |
|   [21]  | 21 to 24      | Type table, boundary rule, anti-pattern table, collection table    | Seed of `dotnet-coding-languageext`                |

Nothing in the remaining content of file 04 is needed. Its first EXPLICIT_ALTERNATIVES sentence is the "invalid states unrepresentable" bullet of CLAUDE.md and design rule 1 of file 10, its `[Union]` sentence is thinktecture, and its shape sentence is the SHAPE column of the table in file 21. C# has no active patterns, the only user code that runs inside a pattern is `Deconstruct`, and it cannot fail, so the C# idiom for that intent is a classifier that returns `Option<A>` or a closed union and is matched afterward, which file 10 shows with `Classify` returning `UserInput`.

## [06]-[SEQUENCE]

The work runs as a sequence of forked agents, each with one slice, a commit between forks, and a report of at most 12 lines (files touched, facts moved, plan adjustments, open questions). The sibling skills come before the `dotnet-coding` references, because the references must defer library semantics to `dotnet-coding-languageext` and generator mechanics to `dotnet-coding-thinktecture`, and that boundary is visible only once the skills exist:

| [INDEX] | [FORK]                    | [SOURCES]                              | [OUTPUT]                                               |
| :-----: | :------------------------ | :------------------------------------- | :----------------------------------------------------- |
|   [01]  | languageext skill         | 21 to 24, 08                           | `SKILL.md`, stubs only where a topic demands           |
|   [02]  | thinktecture skill        | `thinktecture` folder                  | `SKILL.md`, settings, factory-path, Serilog references |
|   [03]  | mapperly skill            | `mapperly` folder                      | `SKILL.md`                                             |
|   [04]  | results reference         | 10, 11, 12, 21 [04] to [07]            | `results.md`, skill adjustments                        |
|   [05]  | functions reference       | 06, 07, 09                             | `functions.md`, skill adjustments                      |
|   [06]  | effects reference         | 02, 15, 18, 22 [02] [03]               | `effects.md`, skill adjustments                        |
|   [07]  | sequences, immutable data | 05, 13, 24 [02] to [05]                | 2 references, skill adjustments                        |
|   [08]  | state, streams            | 16, 17, 19, 20, 24 [06]                | 2 references, skill adjustments                        |
|   [09]  | event sourcing            | 14                                     | `event-sourcing.md`                                    |
|   [10]  | languageext references    | `languageext` folder on the real types | references under `dotnet-coding-languageext`                  |
|   [11]  | consistency pass          | All 4 skills                           | Duplicates removed, pointers aligned, descriptions set |

Each fork reads the plan, the current state of its target and of `dotnet-coding/SKILL.md`, `git diff --stat HEAD~1`, and its research sections, then works in small moves: one section, comment the research out, read again. A fork that finds a rule in its slice that belongs in a skill moves it there, and a fork that finds the plan wrong adjusts the plan and says so in its report.

## [07]-[REPAIRS]

The first review of `SKILL.md` found defects, and each is a pattern to check for after every move:
- Duplicated facts (tuples at 3 places, immutability, purity, and higher-order functions each as an intro bullet and a section, the `with` example in 2 sections)
- Referents cut with the narration around them ("This distinction matters under concurrency" with no distinction stated, an orphaned "Modularity" sentence)
- Sibling mechanics in the wrong skill (the traits and `K<F, A>` paragraph under language features)
- Textbook narration with no rule ("Dictionaries directly store arbitrary mappings", the LINQ operator list)
- Rules that stop short of the C# answer (recursion with no reply to the missing tail-call optimization)
- Typos and numbering gaps ("tarnsparency", "bheaviopr", "functions receives", [04] then [6] then [XXXX], 18 blank lines)

## [08]-[REVIEW]

After fork [11], a second sequence of fresh agents (not forks) reviews the 4 skills in the same order, one skill or reference set per agent. Each agent reads the plan, its target, the research files its target absorbed (including the commented-out sections), and the other 3 skills, then checks that every fact moved rather than vanished, that no narration or meta text survived, that the complexity of the topic was carried and not trimmed away, that each snippet uses placeholders and shows one rule without hardening a shape, and that nothing is restated across skills. Corrections are surgical integrations.

The thinktecture reviewers also read `.claude/skills/.tmp/thinktecture-runtime-extensions/SKILL.md` with its `references/` (equality-and-comparers, generic-types among them) and `.claude/skills/.tmp/thinktecture-entityframeworkcore`. They are the official skills from the package repositories, written to a lower standard, and serve only as a coverage check: a topic they hold and the research omitted is either placed in the owning skill or recorded as omitted because it conflicts with the workspace approach.
