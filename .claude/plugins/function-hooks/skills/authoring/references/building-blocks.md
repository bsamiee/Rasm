# [BUILDING_BLOCKS]

Compose policies with the local `Option` and `Decision` operations under the loader constraints.

## [01]-[OPTION]

`hooks/composition/option.ts` holds `Option<A>` as `{ match: (cases: { some, none }) => B }`. Callback-taking combinators are data-last, function first and input in the returned call:

| [INDEX] | [OPERATION]                 | [USE]                                                                                            |
| :-----: | :-------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `some(value)`, `none()`     | Construct, `none<T>()` where the arm's type is not inferred                                      |
|  [02]   | `fromPredicate(refinement)` | The one `if`, a value into the Option of its narrowed type, the form of every decoder            |
|  [03]   | `fromNullable(value)`       | Optional fields and array indexes (`argv[0]`, `row.deny`, `.find(...)`) into an Option           |
|  [04]   | `fromBoolean(condition)`    | Booleans into `Option<true>`, a two-way branch,   `fromBoolean(c).match<T>({ some, none })`      |
|  [05]   | `getOrElse(fallback)(o)`    | The value or a fallback evaluated only for none, the end of a chain (`getOrElse(() => '')(...)`) |
|  [06]   | `map(f)(o)`                 | Function applied under the some, the none stays                                                  |
|  [07]   | `flatMap(f)(o)`             | The dependent step, an Option-returning function under the some                                  |
|  [08]   | `toArray(o)`                | One element or none, the shape `flatMap` over a list consumes, `values.flatMap(decode)`          |
|  [09]   | `liftPredicate<T>(p)(x)`    | The value as a some when the predicate holds, the one call for a value under a condition         |

- Independent conditions join in one refinement with `&&`, and a `fromBoolean` inside another `fromBoolean` is one refinement
- Matches that rebuild the Option they read (`some: (v) => some(f(v))`, `none: () => none()`) are `map` or `flatMap`, a rule reports them
- Values mapped over `fromBoolean` (`map(() => x)(fromBoolean(c))`) are `liftPredicate<T>(() => c)(x)` when `x` is a plain value
- Calls, allocations, and reads that stay conditional remain inside the callback
- The type argument on `liftPredicate` is stated at the call, because a nullary predicate infers none
- Chains of two matches are `.match<Option<T>>({ some, none }).match<R>(...)`, never a `some` arm that opens a second match
- Gates in sequence (`fromPredicate` then `fromBoolean`) are one refinement joined with `&&`, the form the nesting rule accepts
- Procedures under a `some` arm nest each callback one level deeper, and the flat form is one step per line with `forEach` over the Option

## [02]-[DECISION]

`hooks/composition/decision.ts` holds `Decision<E, R, D>` as a case record over `rewrite`, `deny`, and `answer`, and `Rule<E, R, D>` is `(e: E) => Decision<E, R, D>`:

| [INDEX] | [OPERATION]              | [USE]                                                                                           |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `rewrite(e, context)`    | The event to run beneath with the context lines, and a pass is `rewrite(e, [])`                 |
|  [02]   | `deny(reason)`           | The refusal, `D` is `string` where the event refuses and `never` where it cannot                |
|  [03]   | `answer(result)`         | The event's result without `next`, the description line of `tool.describe`                      |
|  [04]   | `when(refinement, rule)` | Rule lifted over a narrower input, a non-matching input passes through unchanged                |
|  [05]   | `bind(rule)(decision)`   | The rule under a rewrite with the context accumulated, a deny or an answer stays, the fold step |
|  [06]   | `fold(rules)`            | Table order, a rewrite feeds the next rule and accumulates context, a deny or an answer ends it |
|  [07]   | `absurd(value)`          | The arm a rule's type rules out, `never` in and `never` out, and a case record stays total      |

- Fold order inside one event is the policy: a rewrite later rules read runs first, guards before rows, a rewrite the model must see last
- Rules that consult the person have no pure form, and a `Decision` arm for them lands with its first consumer
- Specs read a decision through one case record into plain data (`{ kind: 'deny', reason }`) and compare values, no fake `next`

## [03]-[REFINEMENTS]

Branches are refinements and case records, read from the declarations' own unions:
- Refinements `(e: ToolCallInput): e is Bash` narrow the event union for `when`, `_isBash`, `_isPath`, `_hasCommand`, `_isTool(tool)` over `Named<T>`
- Case records `Readonly<Record<Class, (…) => T>>` dispatch by computed class (`_LINES[_lineClass(...)]`, `_RECOVERY[replyClass(...)]`)
- The API's result unions tag by `deny?: undefined`, read through `fromPredicate((r) => r.deny === undefined)` and a case record
- Refinements over a store value are decoders, `_isFinding`, `_isSession`, and their `fromPredicate` is the exported `decodeX`
- Type-level guards check a table against the declarations, and `_Missing` in `hooks/policies/tools.ts` fails `typecheck` when a tool family vanishes

## [04]-[TABLES]

Tables are `as const satisfies readonly Row[]` or `Readonly<Record<Key, Row>>`, each policy file's row type states its fields, and the rules over a table compute hits, then a deny, a rewrite, or context lines:
- New cases are rows, a new condition on a case is its `when`, and a new field on the row type is a change every table consumer reads
- `OPTIONS` holds `type` per option, the `Options` type derives from it, and defaults stay in `plugin.json`
- `NAMESPACES` and the name lists (`KIND_NAMES`, `STATUS`, `PART_NAMES`) are each `as const` with their union and refinement
- Rows rewrite to the strong form the call's words decide, and deny a call with nothing to run or a drop that inverts it (a dry run made real)
- `once` keys name the skill or tool a context line routes to, and adapters stamp `injected/<session>/<key>` before the call
- `skill.prompt` stamps `loaded/<session>/<skill>`, and a once line with its key stamped under either namespace is not injected again
- Row reasons and lines are a literal or a template opening with literal text, and the row runner forwards them to `deny`
- The server union `_ServerOf<keyof McpToolInputs>` distributes over the keys and rejects rows for absent servers during typecheck

## [05]-[ADAPTER]

Facts the adapter shape rests on, with `hooks/events/tool-call.ts` as the model:
- `Promise.all` and `.catch` sit in the hook body alone, and each background arm has its own `.catch`, a failed call drops the arm alone
- Post-`next` arms (`toolRecords`, scan rows, the roslyn read after its watcher wait) run in one `Promise.all`
- Each `$.process.run` maps a rejected child through its own `.catch` to an abort `Run`, `exitCode` -1 and the error as the first stderr line
- The row's own lines then report a child that cannot start
- The `$.fs.exists` facts the git refinements need are gathered over `gitPaths(command)` as `{ path, found }` records in one `Promise.all`
- The case record maps `deny` to `{ deny }`, `answer` to `{ result }`, and `rewrite` to `next`, with `$.ui.notice(e.tool_use_id, context.join(' '))`
- The engine's command checks (`Blocked: sleep`) run beneath `next` over the rewritten command, and a dropped leaf never reaches them
- Matched hooks narrow through their matcher, `{ interactive: true }` on `session.start` and `{ tool: 'mcp__function-hooks__close' }` on `tool.call`
- The `ui.render` matcher `{ component: 'AbovePrompt', surface: 'terminal', props: { hasSurvey: false } }` pins the band and yields to a survey
- Matched hooks register beside the plain one when their option is on, `whenEnabled(options.dispatch, () => register(on))` from the options module
- Hooks on a cached answer (`ui.render`, `prompt.context`, `prompt.section`, `tool.describe`) read fixed keys, the `summary` row in place of a scan
- Arms after `next` are one `await` per step with its own `.catch`, and an IIFE with one `.catch` around a whole arm nests every step inside it
- Steps of a timer body that need `$` are named functions at the hook level (`apply`, `settle`, `dispatch`, `tick`), one procedure each

## [06]-[STORE]

`hooks/host/store.ts` holds the keys and the decoders:
- `key(namespace, ...parts)` builds a key, `keys(namespace, ...parts)(all)` filters a key list, and `ids(...)(all)` strips the prefix
- `suffix(namespace, ...parts)(key)` reads one id, the form the `close` handler matches rows by
- `id(now, random)` builds `<iso>-<rand>` from `$.clock.now()` and `crypto.randomUUID()`
- `decodeX: (value: unknown) => Option<X>` is `fromPredicate(_isX)`, and `decodeFindings` and `decodeKeyedFindings` read a list
- `decodeJson` is the one JSON boundary, every text from host or model crosses it, and a throw reads as none
- `secretsOf` and `cleanedOf` read an absent or malformed value as the empty record, the `getOrElse` form for a record rules always need
- `summary` reads through `decodeSummary` alone, because `session.start` writes the row and a reader restates no default

## [07]-[TEXT]

`hooks/text/argv.ts` lexes a shell command into leaves, `hooks/text/path.ts` reads a file path, and `hooks/text/lines.ts` reads output lines:
- `leaves(guarded)(command)` answers `Leaf[]` of `Word { text, start, end }`, split on runs of `;`, `&`, `|`, newlines, and on parentheses
- The lexer recurses into `$(...)`, backticks outside single quotes, `sh -c`, and interpreter `-c` and `-e` bodies to depth 8
- Guarded words inside interpreter code are leaves of their own, and a shell with no script runs nothing
- Leaves stay raw, and `strip(argv)` removes env assignments, wrappers, and runners for the git guard's view
- Wrapped shells and interpreters (`timeout 30 sh -c '<body>'`) and wrappers with no command (`timeout 30`) keep their raw leaf after the body's
- `basename(path)`, `extension(path)`, `under(path, directory)`, and `relative(cwd, path)` are the reads path rows and scan rows share
- `lines(text)` reads non-empty output or reply lines and `first(text)` their first line, shared by the scan and roslyn rows
- Rewrites splice the command bytes by span and never join word texts, a joined form renders `2>&1` as `2 >& 1`, and a leaf after `&&` rewrites too
- `groups(command)` answers each parenthesized group by span with `piped` when one pipe follows its closing paren, the read the sleep rows take
- Secret pairs live in `policies/secrets.ts`, applied in table order through a function replacer that keeps a `$` in a value literal

## [08]-[NEW_BLOCK]

Share an operation when consumers need the same transformation, and the shared form keeps their evaluation order and data distinctions. Keep operations used by one file local under a `_` name. Call the owning operation directly instead of adding a forwarding arrow.

## [09]-[RULES]

Shapes become rules when a block sets them (a call spelling, a table form, a carrier operation) or a fix proved across the code sets them. Rules keep the shape at `lint`:
- The `claude-code` family is named for the package its rules read, with `files` scoped to the plugin and specs ignored by the import rule
- Utils are named `claude-code-<shape>` (hook, engine call, option member), and tests mirror the rule path
- `-by-hand` siblings report forms without a proven automatic fix for a site-specific correction
- `rg -n '^(id|message):' tools/ast-grep/rules/typescript/claude-code/*.yml` lists the kept shapes, beside the `syntax` and `effect` families
