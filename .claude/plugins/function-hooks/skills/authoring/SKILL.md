---
name: authoring
description: "Use when adding, changing, or reviewing a function-hooks plugin hook, covering runtime, approach, folders, owners, checks, and harness target."
---

# [FUNCTION_HOOKS]

Build hooks in `.claude/plugins/function-hooks/` from pure policy rules and typed event adapters. Module refuses, rewrites, and annotates tool calls and routes skills. Read `.claude/types/claude-code.d.ts` for each event, result, and `$` method in scope, use `plugin-authoring` for the engine orientation.

Plugin skills belong at `skills/<name>/SKILL.md`, agents `agents/<name>.md`, loaded as `function-hooks:<name>`.

[REFERENCES]:
- [01]-[API](references/api.md): Event contracts and runtime proof
- [02]-[BUILDING_BLOCKS](references/building-blocks.md): Policy composition and module design
- [03]-[IDEATION](references/ideation.md): Choosing behavior from the declared capabilities

[HOOK_BUILDER](../../agents/hook-builder.md), one bounded scope with its runtime proof per dispatch.

## [01]-[RUNTIME]

`register(on, options)` in `hooks/register.ts` runs once per load in an environment with no DOM and no Node, `$` is the one way out:
- `on(event, hook)` registers the plain hook of an event, `on(event, matcher, hook)` a matched hook, any number per event
- Hooks are `($, e, next)`, with `e` input frozen to every depth and `next(e)` the hooks beneath, then the engine
- Returns without `next` answer for the engine, `next({ ...e, field })` rewrites what runs beneath, `await next(e)` reads the result
- Registration order is nesting order, the first registration wraps every later one
- Every `$` call is an event, seen by hooks that wrap the caller and by none of the caller's hooks
- Loader refusals: import outside `claude-code` or relative `.ts` path, empty matcher, second plain hook on one event
- Loader refusals: `$` outside `$.noun.verb(...)` call or `$.plugin` read, `e.field = value` throws inside the hook
- Hooks that throw, overrun their budget, or answer a wrong shape are skipped, the chain continues, `claude --debug` names them
- Empty `deny` reasons are fail-open, hooks that return `{}` or `undefined` are skipped, a skipped `next` is no deny
- Settings `command` hooks and `permissions` run inside `next(e)` beneath every function hook, a function hook's deny stops before them
- Managed-settings hooks run before function hooks, their deny is the call's result

## [02]-[APPROACH]

Every hook is an adapter over pure rules, the adapter is where `$` call or `next` call appear:
- Rules are `(e) => Decision`, `rewrite` or `deny(reason)`, `fold` runs a table of rules in order
- `$.store` is the state, read in the hook body of the event that needs it, no module binding changes after load
- Give each fact one owner: a key one writer, a reason one row, a guard one export
- Deny reasons name the correct form (command, tool, skill), the model retries from the reason alone
- Rewrites that change what runs add `context` line naming the change, same-bytes rewrites add none, model reads its written command alone
- Rows replacing a prose rule, deny glob, or settings hook land with runtime proof, the old form leaves in the same change

## [03]-[FOLDERS]

`hooks/` holds one folder per category of concern with each spec beside its module. Imports point from `events/` through `policies/` down to `host/`, `text/`, `composition/`, and `register.ts` at the root imports event files alone:

[COMPOSITION]: `Decision` union of rewrite and deny, `when` over a refinement, `fold` over a rule table
- Absence is `T | undefined` narrowed by language, a branch is a ternary, early return, or `find` over a table
- Specs compare a decision against its literal

[TEXT]: Operations from text to data or to text, with no engine type and no policy row, that a policy calls on a field it reads
- Group operations by text form (command, value list, document), with their types and specs
- Positions survive as spans, policy rewrites by splicing a span and its context line names what changed
- Share text operations when policies need the same interpretation, keep operations used by one policy local under `_` name
- Add operations to the module for their text form, creating a module when a shared form has no owner
- Reasons, skill names, and rows stay in policies, the folder knows no table

[HOST]: Boundary to the engine, store keys, guards, and options narrowed once
- Store facts are namespaces with one key per row, key builder with filters over the key list, and one guard per row type
- Events read every store value through `decode(guard)`, a value outside the guard reads as undefined
- Options derive from a row table that matches the manifest, narrowed once at load with no default restated
- Reuse the namespace and row shape that express a stored fact
- Add a shape when readers need new data, with namespace, interface, guard, and spec row for the valid, wrong, and missing value

[POLICIES]: One file per subject, its table `as const satisfies` a row type, and the rules that compute a decision from the rows
- Rows are data with one move, undefined for an event it leaves as given: a deny with its reason, or a rewrite with its context lines
- Rules read the event and facts its adapter gathered, compute hits, and return the first deny, else a rewrite with every context line
- Files export their table, rule, and keys the adapter stamps, no policy calls `$` or `next`
- Use a row for another case of an existing policy and keep one-off logic in its owning module
- Add a file for a subject with no table, a moved expression alone earns none
- Keep table-driven rules with their table and row type, compare every rule's decisions over literal events in its spec

[EVENTS]: One adapter per engine event, the file that turns a pure decision into the engine's result
- Hook bodies read the session and store keys their rules need, then gather the `$` facts a rule takes as arguments
- Bodies fold their subject's rules over `e`, each lifted by its refinement, in the order that is the policy
- Decisions map by `kind` onto the event's result union, a deny becomes that union's refusal
- Rewrite arms stamp once keys, show the context under the open call, then await `next`
- After `next` the body records what a successful call proves and appends context to a result the union lets hold it

[NEW_FOLDER]: Concerns that fit no folder take a folder of their own under `hooks/` when each criterion holds:
- Its name is an established term for the concern, in the vocabulary of the declarations or of TypeScript
- Two or more modules hold the concern, and a lone module stays in its nearest concern's folder
- It adds no level alone, its modules sit at its root, and no folder nests inside it
- Relative `.ts` imports reach it, and tsconfig `include`, biome override, rule family, and spec glob cover it with no change
- Its modules import no event file, imports keep pointing one way

## [04]-[OWNERS]

Each addition lands in its owning table or module, the plugin's `README.md` names the file that holds each:

| [INDEX] | [ADDITION]                    | [OWNER]                                                      |
| :-----: | :---------------------------- | :----------------------------------------------------------- |
|  [01]   | Shell or git behavior         | `SHELL` or `GIT`                                             |
|  [02]   | File path or content behavior | `PATHS`                                                      |
|  [03]   | Tool routing or description   | `TOOLS`, `SERVERS`, `FETCH`, `FAMILIES`, or `DESCRIBE`       |
|  [04]   | Stored fact                   | `NAMESPACES` and a store guard                               |
|  [05]   | Hook on an unhooked event     | New file under `events/` registered in `register.ts`         |
|  [06]   | User option                   | Manifest `userConfig` and `OPTIONS`                          |
|  [07]   | Shared text operation         | Its module under `text/`                                     |
|  [08]   | Recurring structural defect   | Existing or new rule in `claude-code` family                 |

## [05]-[CHECKS]

Static checks run at zero findings before a runtime proof, the plugin's `README.md` holds their commands:
- Specs sit beside their modules and fold rules over literal events
- Draw hooks are proven in an interactive session, a `-p` run raises `session.start` with no surface

## [06]-[HARNESS]

`pnpm exec nx run rasm:harness` runs `eng/scripts/harness.py` after a Claude Code update, MCP server change, or plugin edit. It is the route to declarations under `.claude/types/` and the installed copy under `~/.claude/plugins/cache/`:
- Target depends on plugin's `lint` and `test`
- Failures name the fix: a configured server absent from `claude-code-mcp.d.ts`, or version line other than `claude --version`
- Installed copies prove their load in a debug file under `.artifacts/`, a copy that fails to load fails the run
- `uv run --only-group eng python -m eng.scripts.harness proof <row> '<prompt>'` proves one row on the plugin tree and logs its reads
- `computer-use` never appears in the declarations a `-p` session writes
- Each regeneration is read for a declared capability a hook hand-rolls, the capability replaces the hand-rolled step
