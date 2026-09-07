---
name: authoring
description: "Use when adding, changing, or judging a hook of the function-hooks Claude Code plugin, covering runtime, approach, folders, owners, checks, and the harness target."
---

# [FUNCTION_HOOKS]

Build hooks in `.claude/plugins/function-hooks/` from pure policy rules and typed event adapters. The module refuses, rewrites, and annotates tool calls, routes skills, briefs agents, and records findings.

Read `.claude/types/claude-code.d.ts` for each event, result, and `$` method in scope, and use `plugin-authoring` for the engine orientation.

Plugin skills belong at `skills/<name>/SKILL.md` and agents at `agents/<name>.md`, loaded as `function-hooks:<name>`.

[REFERENCES]:
- [01]-[API](references/api.md): Event contracts and runtime proof
- [02]-[BUILDING_BLOCKS](references/building-blocks.md): Policy composition and module design
- [03]-[IDEATION](references/ideation.md): Choosing behavior from the declared capabilities

Delegate every edit under the folder to [HOOK_BUILDER](../../agents/hook-builder.md), one bounded scope with its runtime proof per dispatch.

## [01]-[RUNTIME]

`register(on, options)` in `hooks/register.ts` runs once per load in an environment with no DOM and no Node, and `$` is the one way out:
- `on(event, hook)` registers the one plain hook of an event, and `on(event, matcher, hook)` a matched hook, any number per event
- Hooks are `($, e, next)`, with `e` the input frozen to every depth and `next(e)` the hooks beneath and then the engine
- Returns without `next` answer for the engine, `next({ ...e, field })` rewrites what runs beneath, and `await next(e)` reads the result
- Registration order is nesting order, and the first registration wraps every later one
- Every `$` call is itself an event, seen by the hooks that wrap the caller and by none of the caller's own hooks
- The loader refuses an import outside `claude-code` and a relative `.ts` path, an empty matcher, and a second plain hook on one event
- The loader refuses `$` outside a `$.noun.verb(...)` call or a `$.plugin` read, and `e.field = value` throws inside the hook
- Hooks that throw, overrun their budget, or answer a wrong shape are skipped, the chain continues, and `claude --debug` names them
- Empty `deny` reasons are fail-open, hooks that return `{}` or `undefined` are skipped, and a skipped `next` is no deny
- Settings `command` hooks and `permissions` run inside `next(e)` beneath every function hook, and a function hook's deny stops before them
- Managed-settings hooks run before every function hook, and their deny is the call's result

## [02]-[APPROACH]

Every hook is an adapter over pure rules, and the adapter is the one place a `$` call or a `next` call appears:
- Rules are `(e) => Decision`, a `rewrite`, a `deny(reason)`, or an `answer(result)`, and `fold` runs a table of rules in order
- `$.store` is the one state, read in the hook body of the event that needs it, and no module binding changes after load
- Give each fact one owner: a key one writer, a reason one row, and a decoder one export
- Deny reasons name the correct form (the command, the tool, the skill), and the model retries from the reason alone
- Rewrites add a `context` line naming what changed, because the model reads the command it wrote and not the rewritten one
- Rows replacing a prose rule, a deny glob, or a settings hook land with their runtime proof, and the old form leaves in the same change

## [03]-[FOLDERS]

`hooks/` holds one folder per category of concern with each spec beside its module, imports point from `events/` through `policies/` down to `host/`, `text/`, and `composition/`, and `register.ts` at the root imports the event files alone:

[COMPOSITION]: Carriers for a value with cases, each a record of one `match` over a case record, the dispatch every file uses in place of a branch
- Constructors build one case each, operations are data-last functions over a carrier, and the one two-way `if` lifts a refinement
- Every other branch is a `match` with its result type written, because the compiler binds the type argument to the first arm
- Reach for a carrier when a value is absent, refined, or decided, and for an operation when a `match` rebuilds the carrier it read
- Share operations that express the same carrier transformation across consumers, and add an arm with its first consumer
- Specs fold the operations over literal values and read the result through one case record into plain data

[TEXT]: Operations from text to data or to text, with no engine type and no policy row, that a policy calls on a field it reads
- Group operations by text form (a command, a value list, a document), with their types and specs beside the module
- Positions survive as spans, a policy rewrites by splicing a span and its context line names what changed
- Share text operations when policies need the same interpretation, and keep operations used by one policy local under a `_` name
- Add operations to the module for their text form, creating a module when a shared form has no owner
- Reasons, skill names, and rows stay in the policies, the folder knows no table

[HOST]: The boundary to the engine, the store's keys and decoders, the options narrowed once, and the input of every served tool
- Store facts are namespaces with one key per row, a key builder with filters over the key list, and one decoder per row type
- Decoders lift a refinement over an unknown value into the carrier, and the events read every store value through them
- Options derive from a row table that matches the manifest, narrowed once at load with no default restated
- Reuse the namespace and row shape that express a stored fact
- Add a shape when readers need new data, with its namespace, interface, refinement, decoder, and spec cases for the valid, wrong, and missing row

[POLICIES]: One file per subject, its table `as const satisfies` a row type, and the rules that compute a decision from the rows
- Rows are data, the predicate that selects them and one move, a deny with its reason, a rewrite with its context, a context line, or an answer
- Rules read the event and the facts the adapter gathered, compute the hits, and return the first deny, else the rewrite with every context line
- Files export the table, the rule, and the once keys the adapter stamps, and no policy calls `$` or `next`
- Use a row for another case of an existing policy and keep one-off logic in its owning module
- Add a file for a subject with no table, and a moved expression alone earns none
- Keep table-driven rules with their table and row type, and compare every rule's decisions over literal events in its spec

[EVENTS]: One adapter per engine event, the file that turns a pure decision into the engine's result
- The hook body reads the session and the store keys its rules need, then gathers the `$` facts a rule takes as arguments
- The body folds the rules of its subject over `e`, each lifted by its refinement, in the order that is the policy
- The decision maps through one case record with the result type written onto the event's result union, and a deny becomes the union's refusal
- In the rewrite arm the body stamps the once keys, shows the context under the open call, then awaits `next`
- After `next` the body records what a successful call proves and appends the context to a result the union lets carry it
- Files with no rows export a registration that registers nothing, and they gain `on(event, hook)` with their first row
- The first row brings the event's input and result types from the declarations, the store read, the fold, and the case record

[NEW_FOLDER]: Concerns that fit no folder take a folder of their own under `hooks/` when each criterion holds:
- The name is an established term of the field for the concern, in the vocabulary of the declarations or of the language
- Two or more modules hold the one concern, each with its spec beside it, and a lone module stays in the folder of its nearest concern
- The folder adds no level alone, its modules sit at its root, and no folder nests inside it
- Relative `.ts` imports reach it, and the tsconfig `include`, the biome override, the rule family, and the spec glob cover it with no change
- Its modules import the carriers and no event file, and imports keep pointing one way

## [04]-[OWNERS]

Each addition lands in its owning table or module, and the plugin's `README.md` names the file that holds each:

| [INDEX] | [ADDITION]                    | [OWNER]                                                          |
| :-----: | :---------------------------- | :--------------------------------------------------------------- |
|  [01]   | Shell or git behavior         | `SHELL` or `GIT`                                                 |
|  [02]   | File path or content behavior | `PATHS`                                                          |
|  [03]   | Tool routing or description   | `TOOLS`, `SERVERS`, `FETCH`, `FAMILIES`, or `DESCRIBE`           |
|  [04]   | Agent brief or availability   | `AGENTS` or `OFFERS`                                             |
|  [05]   | Classifier kind               | `KINDS`                                                          |
|  [06]   | Stored fact                   | `NAMESPACES` and the store decoder                               |
|  [07]   | Hook on an event with no rows | Registration in the event's file                                 |
|  [08]   | User option                   | Manifest `userConfig` and `OPTIONS`                              |
|  [09]   | Served tool                   | Registration, matched handler, and input declaration             |
|  [10]   | Shared text operation         | Its module under `text/`                                         |
|  [11]   | Recurring structural defect   | Existing or new rule in the `claude-code` family                 |
|  [12]   | Command after an edit         | `SCAN`                                                           |
|  [13]   | Incorrect Roslyn diagnostic   | `WRONG_DIAGNOSTICS`, with its retirement condition in the README |

## [05]-[CHECKS]

Static checks run at zero findings before a runtime proof, and the plugin's `README.md` holds the commands and the debug lines:
- Specs sit beside their modules and fold rules over literal events
- Timers and served tools are proven in an interactive session, because a `-p` run exits before either matters

## [06]-[HARNESS]

`nx run rasm:harness` runs `eng/scripts/harness.py`, the one route to the declarations under `.claude/types/` and to the installed copy under `~/.claude/plugins/cache/`, after a Claude Code update, an MCP server change, or a plugin edit:
- The target depends on the plugin's `lint` and `test`
- Failures name the fix: a configured server absent from `claude-code-mcp.d.ts`, or a version line other than `claude --version`
- The installed copy proves its load in a debug file under `.artifacts/`, and a copy that fails to load fails the run
- `computer-use` never appears in the declarations a `-p` session writes
- Each regeneration is read for a declared capability a hook hand-rolls, and the capability replaces the hand-rolled step
