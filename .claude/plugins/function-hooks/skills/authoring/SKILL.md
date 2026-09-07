---
name: authoring
description: "Use when adding, changing, or judging a hook of the function-hooks Claude Code plugin, covering runtime, approach, folders, owners, checks, and the harness target."
---

# [FUNCTION_HOOKS]

The `function-hooks` plugin at `.claude/plugins/function-hooks/` is one hooks module that refuses, rewrites, and annotates tool calls, redacts prompts, routes skills, briefs agents, and records findings from policy tables. Dispatch the plugin's own `function-hooks:hook-builder` agent for every edit under the folder, one scope per dispatch, and read `.claude/types/claude-code.d.ts` for every event input, result, and `$` method before judging a hook, since the declarations are the API and the harness regenerates them, and the bundled `plugin-authoring` skill is the engine's own orientation to such a plugin, preloaded by the agent. The plugin's own skills sit at `skills/<name>/SKILL.md` and its agents at `agents/<name>.md`, each loaded as `function-hooks:<name>`, and a new component of the plugin takes one such file.

[REFERENCES]:
- [01]-[API](references/api.md): The declarations read by category, with the runtime fact and the proof form of each category
- [02]-[BUILDING_BLOCKS](references/building-blocks.md): Carriers, tables, adapter, store, text operations, when a block or a rule earns its place
- [03]-[IDEATION](references/ideation.md): The moves, the potentialities the declarations open, the compositions, the questions that settle a design

[AGENTS]: Each agent is this skill's worker with `plugin-authoring` preloaded, and proves its result by the checks and a `-p` run:
- [01]-[HOOK_BUILDER](../../agents/hook-builder.md): One scope of the plugin from design to proof, the sources, the ownership, the procedure, and the gate

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
- Each fact has one owner: a key one writer, a reason one row, a decoder one export, and a second copy moves into the owning module
- Deny reasons name the correct form (the command, the tool, the skill), and the model retries from the reason alone
- Rewrites add a `context` line naming what changed, because the model reads the command it wrote and not the rewritten one
- Rows replacing a prose rule, a deny glob, or a settings hook land with their runtime proof, and the old form leaves in the same change

## [03]-[FOLDERS]

`hooks/` holds one folder per category of concern with each spec beside its module, imports point from `events/` through `policies/` down to `host/`, `text/`, and `composition/`, and `register.ts` at the root imports the event files alone:

[COMPOSITION]: Carriers for a value with cases, each a record of one `match` over a case record, the dispatch every file uses in place of a branch
- Constructors build one case each, operations are data-last functions over a carrier, and the one two-way `if` lifts a refinement
- Every other branch is a `match` with its result type written, because the compiler binds the type argument to the first arm
- Reach for a carrier when a value is absent, refined, or decided, and for an operation when a `match` rebuilds the carrier it read
- Operations join the folder when two files repeat one `match` shape, and an arm joins a carrier with its first consumer
- Specs fold the operations over literal values and read the result through one case record into plain data

[TEXT]: Operations from text to data or to text, with no engine type and no policy row, that a policy calls on a field it reads
- Modules export one operation over one text form (a command, a value list, a document), with its types and its spec beside it
- Positions survive as spans, a policy rewrites by splicing a span and its context line names what changed
- Reach for the folder when two policies read one text form, and keep an operation one policy uses inside that policy under a `_` name
- Text forms no module reads take a new module, and operations over a read form join that form's module
- Reasons, skill names, and rows stay in the policies, the folder knows no table

[HOST]: The boundary to the engine, the store's keys and decoders, the options narrowed once, and the input of every served tool
- Store facts are namespaces with one key per row, a key builder with filters over the key list, and one decoder per row type
- Decoders lift a refinement over an unknown value into the carrier, and the events read every store value through them
- Options derive from a row table that matches the manifest, narrowed once at load with no default restated
- Facts that outlive a call take a namespace, an interface, a refinement, a decoder, and a spec case for the valid, wrong, and missing row
- Options take a manifest row and a table row, and served tools take an input declaration, a handler in a policy file, and a spec

[POLICIES]: One file per subject, its table `as const satisfies` a row type, and the rules that compute a decision from the rows
- Rows are data, the predicate that selects them and one move, a deny with its reason, a rewrite with its context, a context line, or an answer
- Rules read the event and the facts the adapter gathered, compute the hits, and return the first deny, else the rewrite with every context line
- Files export the table, the rule, and the once keys the adapter stamps, and no policy calls `$` or `next`
- Behavior is a row before it is a branch, a fact every row needs is a field on the row type, and a subject with no table is a new file
- New files hold the table, its row type, the rule, and a spec that folds the rule over literal events and compares decisions as data

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

| [INDEX] | [ADDITION]                                      | [OWNER]                                                                                      |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | Bash leaf refusal, rewrite, or context          | Row in `SHELL`                                                                               |
|  [02]   | Destructive git form                            | Row in `GIT`                                                                                 |
|  [03]   | File tool path or content rule                  | Row in `PATHS`                                                                               |
|  [04]   | Tool deny, route, guard, or description         | Row in `TOOLS`, `SERVERS`, `FETCH`, `FAMILIES`, or `DESCRIBE`                                |
|  [05]   | Agent brief, or a withheld agent type           | Row in `AGENTS` or `OFFERS`                                                                  |
|  [06]   | Weakness kind the classifier names              | Row in `KINDS`                                                                               |
|  [07]   | Fact that outlives a call                       | Namespace in `NAMESPACES` with its decoder in the store module                               |
|  [08]   | Hook on an event with no rows                   | Registration in the event's file                                                             |
|  [09]   | Option the person sets                          | Row in `userConfig` and in `OPTIONS`                                                         |
|  [10]   | Tool the model calls                            | `$.tool.register` in `session.start`, a matched `tool.call` hook, the tool input declaration |
|  [11]   | Text operation two policies share               | Module under `text/` with its spec                                                           |
|  [12]   | Shape the plugin code keeps                     | Rule in the `claude-code` family with its test                                               |
|  [13]   | Command run after an edit, its lines as context | Row in `SCAN`                                                                                |
|  [14]   | Diagnostic id the roslyn server reports wrongly | Row in `WRONG_DIAGNOSTICS` with its retirement in the README                                 |

## [05]-[CHECKS]

Static checks run at zero findings before a runtime proof, and the plugin's `README.md` holds the commands and the debug lines:
- Specs sit beside their modules and fold rules over literal events
- Runtime proof is one `-p` call per row, read in the transcript's result block and in the debug line the move prints
- Timers and served tools are proven in an interactive session, because a `-p` run exits before either matters

## [06]-[HARNESS]

`nx run rasm:harness` runs `eng/scripts/harness.py`, the one route to the declarations under `.claude/types/` and to the installed copy under `~/.claude/plugins/cache/`, after a Claude Code update, an MCP server change, or a plugin edit:
- The target depends on the plugin's `lint`, `format`, and `test`
- Failures name the fix: a configured server absent from `claude-code-mcp.d.ts`, or a version line other than `claude --version`
- The installed copy proves its load in a debug file under `.artifacts/`, and a copy that fails to load fails the run
- `computer-use` never appears in the declarations a `-p` session writes
- Each regeneration is read for a declared capability a hook hand-rolls, and the capability replaces the hand-rolled step
