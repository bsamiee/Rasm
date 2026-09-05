# [RULE_BUILDING]

Rule building starts from a fix to real code and ends with the pattern statement, the sibling list, and the near-miss list the rule sequence consumes. Use `rule-hardening` for the widening the first rule receives.

## [01]-[SOURCES]

Read in order before the first edit, and stop at the first step with a failing criterion:
1. The package manifests and lock files of the scope, the resolved versions decide which capability is available
2. The installed sources of each external package the scope imports, whole for the modules it uses
3. The exported functions and types of the internal packages the scope depends on
4. The rules of every checker the scope runs, a pattern a checker reports takes no rule
5. The existing logic, whole from its entry point

Every documented default, combinator, and option of a package is a candidate replacement for a hand-written equivalent, and a capability an internal package exports once replaces a local copy. Compose the capabilities of packages into one form (a package default read from the environment, a memoizing combinator over a shared read, a config default keyed by a tag), the form replaces every hand-written layer between them, and a capability found in one package is checked against the rest of the scope for other uses. Read the logic for what it computes twice, what it derives from an input it holds, and what it branches on that the type decides.

## [02]-[SMELLS]

Smells are code the checkers accept and the standards reject, and each category states the criterion an agent applies to the next unseen case:

| [INDEX] | [CATEGORY]                 | [CRITERION]                                                                              |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | Deep nesting               | Callback or block nesting past three levels, each level a scope the reader holds         |
|  [02]   | Elements for one fact      | Constant, enum, string, class, type, or object that names what one value states          |
|  [03]   | Repeated logic             | Same read, decode, or computation at two sites over one input                            |
|  [04]   | Wrapper                    | Function with a body of one call on the same arguments and no domain type added          |
|  [05]   | Forwarder, service locator | Layer that reaches a dependency the runtime or the function supplies                     |
|  [06]   | Threaded value             | Value curried or passed through every call that one runtime or one scope owns            |
|  [07]   | Constant column            | Flag or field that holds one value on every row of a collection                          |
|  [08]   | Restated tool fact         | Table or option the code emits that the tool's own configuration owns                    |
|  [09]   | Element order              | Declarations ordered against dependency, or a specific rewrite run after a general one   |
|  [10]   | Missed capability          | Hand-written logic where the package documents the same operation                        |
|  [11]   | Presence by branch         | Branch with one arm that stands for absence or failure and one arm that passes the value |
|  [12]   | Re-lifted carrier          | Value matched into the type it is, an option, an either, or an exit into an effect       |
|  [13]   | Known discriminator        | Match inside a callee on a value every call site knows                                   |
|  [14]   | Deprecated member          | Member the package marks deprecated for a replacement it names                           |

Flag each category with `find_code_by_rule` over a `kind` and a relational rule:

| [INDEX] | [CATEGORY]                 | [FLAG]                                                                             |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | Deep nesting               | Callback kind `inside` three callback kinds, `stopBy: end` on each                 |
|  [02]   | Elements for one fact      | Declaration with one use site, or a table with values that collapse to one rule    |
|  [03]   | Repeated logic             | Two calls on one callee with the same argument in one scope                        |
|  [04]   | Wrapper                    | Function body that is one call forwarding every parameter                          |
|  [05]   | Forwarder, service locator | Tag or container holding other services beside a scalar                            |
|  [06]   | Threaded value             | Parameter every callee passes unchanged to the next call                           |
|  [07]   | Constant column            | Pair bound on the first row and repeated on every row                              |
|  [08]   | Restated tool fact         | Literal that names a tool's default or a tool's configuration key                  |
|  [09]   | Element order              | Use that precedes its declaration, a general pattern before its specific form      |
|  [10]   | Missed capability          | Loop, probe, or conversion beside an import of the package that owns the operation |
|  [11]   | Presence by branch         | Two-arm match with an empty arm, or with a fail arm beside a pass arm              |
|  [12]   | Re-lifted carrier          | Match on a carrier with arms that rebuild the carrier's own cases                  |
|  [13]   | Known discriminator        | Parameter matched inside the callee and known at every caller                      |
|  [14]   | Deprecated member          | Member's name, read from the deprecation in the package source                     |

The categories a package member decides (missed capability, presence by branch, re-lifted carrier, known discriminator, deprecated member) need the package source open beside the code, and the search flags the call sites once the member is named. Categories the scope's own gates already report yield a finding and no rule. Read the rule list of each linter, analyzer, and plugin the scope runs before searching a category, and leave the pattern with the gate that reports it.

## [03]-[FINDING]

Smells across a codebase come from fresh Opus general-purpose agents, one per package or directory, and the dispatching agent judges every finding:
- Each brief pastes the language standards from `CLAUDE.md`, the smell table, the fix criteria, and the reading order of the sources
- Each brief scopes the agent to one directory or one package and asks for at most 15 rows
- Each row reads `file:line | category | the code | the correction | the reason` with the package source line that documents the correction
- Each agent reads the whole scope and the installed package sources before it reports, and reports no finding a checker in the scope reports
- Findings without the documenting source line are guesses and go back to the agent with the line to read

Findings are one higher-order pattern when the same correction applies for the same reason across them, and they differ when the correction or the reason diverges:
- Two findings with one correction and one reason are one pattern, and their instances become the sibling list of one rule
- Two findings with one shape and two corrections are two patterns, because a rule's `note` states one correction
- Two findings with one correction and two reasons are two patterns when the reason changes which near miss is valid
- Findings with a correction no run has proven are candidates, and the fix promotes or ends them

## [04]-[FIX]

Fixes are the direct form the owning package documents, landed in place, and proven by a run before any rule is derived:
- The scope ends with fewer constants, types, schemas, classes, and objects, at most three callback levels, and fewer lines
- Callback levels are functions directly under an argument list that declare a parameter
- Thunks, initializers, curried returns, and pair values add no callback level
- The correction lands at the site it corrects, and a fix that names a new function, type, file, or alias to hold what the site held is rejected
- Each library in the fix is used for a capability its source documents, in the direct form, with no wrapper, helper, forwarder, or alias added
- No error the code handled is thrown, dropped, or deferred, and the result type of the scope stays the one its boundary chose
- Every checker passes at zero warnings, and the artifact the scope emits (a graph, a file, an exit code, a response) matches the baseline
- The element count and the nesting count are measured before and after under the same commands, each against the baseline the scope already held

Up to 25 added lines are acceptable for a capability the scope lacked, and the element count still falls.

BEFORE, a branch with a second arm that yields an empty object, and the key disappears:

```ts
const entry = (hidden: boolean, label: string): Record<string, string> => ({
  name,
  ...Boolean.match(hidden, { onFalse: () => ({ label }), onTrue: () => ({}) }),
});
```

AFTER, the present key is lifted under its condition and spread once:

```ts
const entry = (hidden: boolean, label: string): Record<string, string> => ({
  name,
  ...Record.getSomes({ label: Option.liftPredicate(label, () => !hidden) }),
});
```

The branch, both arms, and the empty object leave, because `Record.getSomes` writes a key for a `Some` alone and absence becomes a case of the value.

## [05]-[DERIVATION]

Enumerate siblings per module function with the same meaning, per carrier module that exports the function (`Option`, `Either`, `Effect`), per overload of a `dual` export (data-first and data-last), per container kind (object, array, argument list), per spelling of the same operation, and per position the shape occupies (a spread, an argument, a local, a return). Siblings are real when the correction produces the same after form from them, proven by writing the after form once per sibling. Near misses are real when the after form adds an element or changes behavior, and every near miss becomes a `valid` test case. Patterns with one form and no sibling are instances, and their rule waits for the second instance that proves the category.

Write the shape the siblings share as a util with a `kind` at its rule root and each narrower sibling as a refinement of it, the rule references one util and the family is enumerated as util variants before the rule widens.

The BEFORE and AFTER pair yields the pattern statement: a two-arm branch over a boolean, an option, or an either with one arm that yields an empty container stands for absence, the present value is lifted as an option and spread, because absence is a case of the value and not of the control flow. Its siblings are each carrier's match, the matcher chain of two steps, and the conjunction spread, in an object, an array, a local, and a return. Its near misses are a match with two value arms, a dispatch of more than two arms, and a nullish default spread.
