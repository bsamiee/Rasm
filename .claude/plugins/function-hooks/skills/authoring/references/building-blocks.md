# [BUILDING_BLOCKS]

Compose policies with the `Decision` union, the language's own absence, and the tables, under the loader constraints.

## [01]-[DECISION]

`hooks/composition/decision.ts` holds `Decision<E, R>` as a union tagged by `kind`, and `Rule<E, R>` is `(e: E) => Decision<E, R>`:

| [INDEX] | [OPERATION]              | [USE]                                                                                           |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `rewrite(e, context?)`   | The event to run beneath with the context lines, and a pass is `rewrite(e)`                     |
|  [02]   | `deny(reason)`           | The refusal, the reason naming the correct form                                                 |
|  [03]   | `when(refinement, rule)` | Rule lifted over a narrower input, a non-matching input passes through unchanged                |
|  [04]   | `fold(rules)`            | Table order, a rewrite feeds the next rule and accumulates context, a deny ends it              |

- Fold order inside one event is the policy: rewrites later rules read run first, guards before rows, a rewrite the model must see last
- Adapters read the decision through `decision.kind` and map each case onto the event's result union
- Specs compare a decision against its literal with `toStrictEqual`

## [02]-[ABSENCE]

Absence is `T | undefined`, narrowed by the language: `?.`, `??`, `find`, and a ternary or an early return. A record of guards becomes one guard through `struct`, and `decode(guard)(value)` reads a stored value as `T | undefined`. A `for` loop with local `let` state that publishes an immutable value is the form of a scanner.

## [03]-[TABLES]

Tables are `as const satisfies readonly Row[]` or `Readonly<Record<Key, Row>>`, each policy file's row type states its fields, and the rules over a table compute hits, then a deny, a rewrite, or context lines:
- New cases are rows, a new condition on a case is its `when`, and a new field on the row type is a change every table consumer reads
- Rows rewrite to the strong form the call's words decide, and deny a call with nothing to run, a destructive action, or a drop that inverts it (a dry run made real)
- Context lines are `OnceLine` values, keyed ones inject once per session and the adapter stamps `injected/<session>/<key>` before the call
- `skill.prompt` stamps `loaded/<session>/<skill>`, and a once line with its key stamped under either namespace is not injected again
- Tool rows are built through `_row(tool, rules)`, each rule typed over the tool's own event from the declarations

## [04]-[ADAPTER]

Facts the adapter shape rests on, with `hooks/events/tool-call.ts` as the model:
- `Promise.all` and `.catch` sit in the hook body alone, and each arm after `next` has its own `.catch`, a failed call drops the arm alone
- The `$.fs.exists` facts the git refinements need are gathered over `gitPaths(command)` after the shell rules ran
- Matched hooks register beside the plain one under an `if` on their option
- Hooks on a cached answer (`ui.render`, `prompt.context`, `prompt.section`, `tool.describe`) read the tables or one fixed key

## [05]-[STORE]

`hooks/host/store.ts` holds the keys and the guards:
- `key(namespace, ...parts)` builds a key, `keys(namespace, ...parts)(all)` filters a key list, `ids(...)(all)` strips the prefix, and `suffix(...)(key)` reads one id
- `isX` guards read each row type, and `decodeJson` is the one JSON boundary

## [06]-[TEXT]

`hooks/text/argv.ts` parses a shell command into argvs and `hooks/text/path.ts` reads a file path:
- `parse(guarded)(command)` answers `Argv[]` of `Word { text, start, end }`, split on runs of `;`, `&`, `|`, newlines, and on parentheses
- The parser recurses into `$(...)`, backticks outside single quotes, `sh -c`, and interpreter `-c` and `-e` bodies to depth 8
- Guarded words inside interpreter code are argvs of their own, and a shell with no script runs nothing
- Argvs stay raw, `pastAssignments(argv)` drops env assignments, and `strip(argv)` drops wrappers and runners too for the git guard's view
- Wrapped shells and interpreters (`timeout 30 sh -c '<body>'`) keep their raw argv after the body's, and `hasGroup(command)` reads a subshell
- Rewrites splice the command bytes by span and never join word texts, a joined form renders `2>&1` as `2 >& 1`

## [07]-[RULES]

Shapes become rules when a block sets them or a fix proved across the code sets them. Rules keep the shape at `lint`, the `claude-code` family is named for the package its rules read with `files` scoped to the plugin, utils are named `claude-code-<shape>`, and tests mirror the rule path.
