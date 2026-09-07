# [BUILDING_BLOCKS]

The plugin composes in the style of Effect without Effect, because the loader refuses every package import.

## [01]-[OPTION]

`hooks/composition/option.ts` holds `Option<A>` as `{ match: (cases: { some, none }) => B }`, and every operation is data-last, the function first and the option in the second call:

| [INDEX] | [OPERATION]                 | [USE]                                                                                       |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `some(value)`, `none()`     | Construct, `none<T>()` where the arm's type is not inferred                                 |
|  [02]   | `fromPredicate(refinement)` | The one `if`, a value into the Option of its narrowed type, the form of every decoder       |
|  [03]   | `fromNullable(value)`       | Optional fields and array indexes (`argv[0]`, `row.deny`, `.find(...)`) into an Option      |
|  [04]   | `fromBoolean(condition)`    | Booleans into `Option<true>`, the two-way branch, `fromBoolean(c).match<T>({ some, none })` |
|  [05]   | `getOrElse(fallback)(o)`    | The value or a literal fallback, the end of a chain (`getOrElse(() => '')(...)`)            |
|  [06]   | `map(f)(o)`                 | Function applied under the some, the none stays                                             |
|  [07]   | `flatMap(f)(o)`             | The dependent step, an Option-returning function under the some                             |
|  [08]   | `toArray(o)`                | One element or none, the shape `flatMap` over a list consumes, `values.flatMap(decode)`     |
|  [09]   | `liftPredicate<T>(p)(x)`    | The value as a some when the predicate holds, the one call for a value under a condition    |

- Independent conditions join in one refinement with `&&`, and a `fromBoolean` inside another `fromBoolean` is one refinement
- Matches that rebuild the Option they read (`some: (v) => some(f(v))`, `none: () => none()`) are `map` or `flatMap`, a rule reports them
- Values mapped over `fromBoolean` (`map(() => x)(fromBoolean(c))`) are `liftPredicate<T>(() => c)(x)`, and a rule reports the two-call form
- The type argument on `liftPredicate` is stated at the call, because a nullary predicate infers none
- Chains of two matches are `.match<Option<T>>({ some, none }).match<R>(...)`, never a `some` arm that opens a second match

## [02]-[DECISION]

`hooks/composition/decision.ts` holds `Decision<E, R, D>` as a case record over `rewrite`, `deny`, and `answer`, and `Rule<E, R, D>` is `(e: E) => Decision<E, R, D>`:

| [INDEX] | [OPERATION]              | [USE]                                                                                           |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `rewrite(e, context)`    | The event to run beneath with the context lines, and a pass is `rewrite(e, [])`                 |
|  [02]   | `deny(reason)`           | The refusal, `D` is `string` where the event refuses and `never` where it cannot                |
|  [03]   | `answer(result)`         | The event's result without `next`, the description line of `tool.describe`                      |
|  [04]   | `when(refinement, rule)` | Rule lifted over a narrower input, a non-matching input passes through unchanged                |
|  [05]   | `fold(rules)`            | Table order, a rewrite feeds the next rule and accumulates context, a deny or an answer ends it |
|  [06]   | `absurd(value)`          | The arm a rule's type rules out, `never` in and `never` out, so a case record stays total       |

- Fold order inside one event is the policy: a rewrite later rules read runs first, guards before rows, a rewrite the model must see last
- Rules that consult the person have no pure form, and a `Decision` arm for them lands with its first consumer
- Specs read a decision through one case record into plain data (`{ kind: 'deny', reason }`) and compare values, no fake `next`

## [03]-[REFINEMENTS]

Branches are refinements and case records, read from the declarations' own unions:
- Refinements `(e: ToolCallInput): e is Bash` narrow the event union for `when`, `_isBash`, `_isPath`, `_hasCommand`, `_isTool(tool)` over `Named<T>`
- Case records `Readonly<Record<Class, (…) => T>>` indexed by a computed class replace `switch`, `_SCAN_LINES[_scanClass(...)]`, `_LINES[_lineClass(...)]`, `_RECOVERY[replyClass(...)]`
- The API's result unions tag by `deny?: undefined`, read through `fromPredicate((r) => r.deny === undefined)` and a case record
- Refinements over a store value are decoders, `_isFinding`, `_isSession`, and their `fromPredicate` is the exported `decodeX`
- Type-level guards check a table against the generated file, `_Missing` in `hooks/policies/tools.ts` fails `tsc` when a tool family vanishes

## [04]-[TABLES]

Tables are `as const satisfies readonly Row[]` or `Readonly<Record<Key, Row>>`, and the rules over each compute the hits, then a deny, a rewrite, or the context lines:

| [INDEX] | [TABLE]                           | [ROW]                                                                                                                  |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SHELL`                           | `word`, `when(leaf, command)`, one of `deny`, `rewrite`, `context` with `once`                                         |
|  [02]   | `GIT`                             | `why`, `flags`, `starts`, `safe`, `refine(args, existing)` answering the reason as Option                              |
|  [03]   | `PATHS`                           | `match(path, text)`, `tools`, one of `deny`, `once`, `each`, and `guidance` for markdown                               |
|  [04]   | `TOOLS`                           | `tool` over declared names, `deny` a rule, `rewrite` Option of changed event over the event, `once`, `each`, `records` |
|  [05]   | `FETCH`                           | `host` to `route`                                                                                                      |
|  [06]   | `SERVERS`, `FAMILIES`, `DESCRIBE` | `skill`, `line` per server, `prefix`, `requires` per family, a `ToolName` with its line                                |
|  [07]   | `AGENTS`, `OFFERS`                | `subagentType`, `brief` lines, `dispatch`, and `agent`, `isOffered`                                                    |
|  [08]   | `KINDS`                           | `criterion`, `evidence`, `move`, keyed by `Kind` from `KIND_NAMES`                                                     |
|  [09]   | `OPTIONS`                         | `type` per option, the `Options` type derives from it, the defaults stay in `plugin.json`                              |
|  [10]   | `NAMESPACES` and the name lists   | `KIND_NAMES`, `STATUS`, `PART_NAMES` beside it, each `as const` with its union and refinement                          |
|  [11]   | `SCAN`                            | `match`, `argv`, and `lines` over the path and the rule-id facts, and `timeoutMs` for a slow command                   |

- New cases are rows, a new condition on a case is its `when`, and a new field on the row type is a change every consumer of the table reads
- `once` keys name the skill or tool a context line routes to, and the adapter stamps `injected/<session>/<key>` before the call
- `skill.prompt` stamps `loaded/<session>/<skill>`, and a once line with its key stamped under either namespace is not injected again
- Row reasons and lines are a literal or a template opening with literal text, and the row runner forwards them to `deny`
- The server union is `_ServerOf<keyof McpToolInputs>`, distributive over the keys, and a row over an absent server fails `tsc`

## [05]-[ADAPTER]

Facts the adapter shape rests on, with `hooks/events/tool-call.ts` as the model:
- `Promise.all` and `.catch` sit in the hook body alone, and each background arm has its own `.catch`, a failed call drops the arm alone
- The post-`next` arms (`toolRecords`, the scan rows, the roslyn read after its watcher wait) run in one `Promise.all`
- Each `$.process.run` maps a rejected child through its own `.catch` to an abort `Run`, `exitCode` -1 and the error as the first stderr line
- The row's own lines then report a child that cannot start
- The `$.fs.exists` facts the git refinements need are gathered over `gitPaths(command)` as `{ path, found }` records in one `Promise.all`
- The edited file's text is read in the hook body as `PathFacts.file`, a markdown row numbers an `Edit`'s lines from the file
- The case record maps `deny` to `{ deny }`, `answer` to `{ result }`, and `rewrite` to `next`, with `$.ui.notice(e.tool_use_id, context.join(' '))`
- Matched hooks narrow through their matcher, `{ interactive: true }` on `session.start` and `{ tool: 'mcp__function-hooks__close' }` on `tool.call`
- The `ui.render` matcher `{ component: 'AbovePrompt', surface: 'terminal', props: { hasSurvey: false } }` pins the band and yields to a survey
- Matched hooks register beside the plain one when their option is on, `whenEnabled(options.dispatch, () => register(on))` from the options module
- The audit hook tests `next.event` against its audit table before any `$` call, because `$` is empty at `engine.create`

## [06]-[STORE]

`hooks/host/store.ts` holds the keys and the decoders:
- `key(namespace, ...parts)` builds a key, `keys(namespace, ...parts)(all)` filters a key list, and `ids(...)(all)` strips the prefix
- `suffix(namespace, ...parts)(key)` reads one id, the form the `close` handler matches rows by
- `id(now, random)` builds `<iso>-<rand>` from `$.clock.now()` and `crypto.randomUUID()`
- `decodeX: (value: unknown) => Option<X>` is `fromPredicate(_isX)`, and `decodeFindings` and `decodeKeyedFindings` read a list
- `decodeJson` is the one JSON boundary, every text from the host or the model crosses it, and a throw reads as none
- `secretsOf` and `cleanedOf` read an absent or malformed value as the empty record, the `getOrElse` form for a record the rules always need

## [07]-[TEXT]

`hooks/text/argv.ts` lexes a shell command into leaves, `hooks/text/path.ts` reads a file path, `hooks/text/lines.ts` reads output lines, and `hooks/text/replace.ts` applies value pairs:
- `leaves(guarded)(command)` answers `Leaf[]` of `Word { text, start, end }`, split on runs of `;`, `&`, `|`, newlines, and on parentheses
- The lexer recurses into `$(...)`, backticks, `sh -c`, and interpreter `-c` and `-e` bodies to depth 8
- Guarded words inside interpreter code are leaves of their own, and a shell with no script runs nothing
- Leaves stay raw, and `strip(argv)` removes env assignments, wrappers, and runners for the git guard's view
- `basename(path)`, `extension(path)`, `under(path, directory)`, and `relative(cwd, path)` are the reads the path rows and the scan rows share
- `lines(text)` answers the non-empty lines of a child's output or a reply and `first(text)` the first of them, the reads the scan and roslyn rows share
- Spans let a rewrite splice a word in place, the form `packageManager` and the grep rewrite use, and a leaf after `&&` rewrites too
- `replace(pairs, direction)(text)` answers `{ text, applied }`, a string pair both ways and a RegExp pair forward
- Function replacers apply each pair, and a `$` in a replacement value stays literal
- `notes(replacement)` answers the distinct non-empty notes of the applied pairs, the context lines of a redaction

## [08]-[NEW_BLOCK]

Blocks earn their place with a second copy in a second file. The first copy stays local under a `_` name, and the second moves both into the owning module with one export. Blocks one file calls stay in that file, and a forwarding arrow that renames a call is the call.

## [09]-[RULES]

A shape becomes a rule when a block sets it (a call spelling, a table form, a carrier operation) or a fix proved across the code sets it, and the rule keeps the shape at `lint`:
- The `claude-code` family is named for the package the rules read, `files` scoped to the plugin, the specs ignored by the import rule
- Utils are named `claude-code-<shape>` (the hook, the engine call, the option member), and tests mirror the rule path
- `ls tools/ast-grep/rules/typescript/claude-code` lists the family, and the `syntax` and `effect` families read the plugin too
- Use the `ast-grep` skill for the rule sequence

| [INDEX] | [SHAPE_KEPT]                     | [REPORTS]                                                                                          |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | Loader refusals made static      | `$` as a value, an import outside the plugin, a registration outside `register`, a mutation of `e` |
|  [02]   | Fail-open forms                  | Empty or non-text `deny` or `drop`, `catch` around `next`, rewrite with no `context`               |
|  [03]   | Awaited engine reads             | `$` call in a condition slot without `await`, always truthy as a promise                           |
|  [04]   | Cache discipline                 | Clock, random, or turn read inside a cached answer                                                 |
|  [05]   | Invalidation discipline          | Literal invalidate in a loop over data or a render body, a map over the owning view list accepted  |
|  [06]   | Timer discipline                 | Timer outside `session.start`                                                                      |
|  [07]   | Matchers on data                 | Prose matcher beside a refusal                                                                     |
|  [08]   | Options as the manifest declares | Option default or type restated with `??`, `\|\|`, or a boolean comparison                         |
|  [09]   | Carrier discipline               | Value mapped over `fromBoolean`, re-lifted Option, return by branch, fold by loop                  |
|  [10]   | One owner per fact               | Forwarding arrow, module state, fourth callback level                                              |
|  [11]   | `-by-hand` siblings              | The forms of a rule no fix types, reported for the site to correct                                 |
