# [FUNCTION_HOOKS]

Function hooks is the plugin `function-hooks@rasm` of the `rasm` marketplace at `.claude/plugins/`, one hooks module (`hooks/register.ts`) loaded under `CLAUDE_CODE_ENABLE_FUNCTION_HOOKS=1` in the `env` block of `.claude/settings.json`. Hooks `($, e, next)` run inside the Claude Code process on a typed event and answer a typed result, in place of a shell hook's stdin JSON and exit code, without spawning a shell to dispatch the hook. `claude --plugin-dir .claude/plugins/function-hooks` loads the directory in place of the installed copy under `~/.claude/plugins/cache/`.

## [01]-[LAYOUT]

```text
function-hooks/
├── .claude-plugin/
│   └── plugin.json           # Manifest with name, description, and userConfig rows with type and default
├── hooks/
│   ├── hooks.json            # Names register.ts as the one module the loader reads
│   ├── register.ts           # Reads the options once and registers every event file in engine lifecycle order
│   ├── composition/          # Carriers as case records, the dispatch every file uses in place of a branch
│   │   ├── option.ts         # Option with its constructors and data-last operations, the one if
│   │   └── decision.ts       # Decision with rewrite, deny, and answer, when over a refinement, fold over rules
│   ├── text/                 # Operations from text to data or text, no engine type and no policy row
│   │   ├── argv.ts           # Argv leaves of a shell command with source spans
│   │   ├── lines.ts          # Non-empty lines of a child's output or a reply, and the first of them
│   │   ├── path.ts           # Reads of a file path or a command word, the base name, the extension, under a directory, relative to cwd
│   │   └── replace.ts        # Table-driven value replacement in both directions with the applied pairs
│   ├── host/                 # Boundary to the engine
│   │   ├── store.ts          # Store namespaces, key builders, row types, and decoders from unknown values
│   │   ├── options.ts        # Options narrowed once from the host-validated values
│   │   └── tools.d.ts        # Input of the served close tool merged into McpToolInputs, and the two ops the generated types omit
│   ├── policies/             # One table per subject as const satisfies a row type, and the rules over it
│   │   ├── git.ts            # GIT rows with their refinements, the guard and the paths it tests
│   │   ├── shell.ts          # SHELL rows over parsed leaves, the shell rule, the timeout, cache, package manager, sleep, and ast-grep rewrites
│   │   ├── paths.ts          # PATHS rows over the file tools by path and content, the path rule
│   │   ├── tools.ts          # TOOLS, FAMILIES, SERVERS, FETCH, and DESCRIBE rows, the tool and describe rules
│   │   ├── secrets.ts        # Secret pairs from the store, the redaction and restoration rules
│   │   ├── agents.ts         # AGENTS and OFFERS rows, the spawn rule
│   │   ├── findings.ts       # Finding rows, the open-question block, the close tool, the status line, the batches
│   │   ├── kinds.ts          # KINDS rows, the evidence rule, the classifier prompt and its field decoder
│   │   ├── roslyn.ts         # Roslyn requests and lines over an edited C# file, the read tools, the watcher window, the filtered reply
│   │   └── scan.ts           # SCAN rows over an edited path, the command each row runs and the lines its output yields
│   ├── events/               # One adapter per engine event, <event>.ts in kebab case
│   └── */<module>.spec.ts    # Vitest spec beside each module that has one
├── skills/
│   └── authoring/            # Skill function-hooks:authoring with its references, the approach main briefs and judges by
├── agents/
│   └── hook-builder.md       # Agent function-hooks:hook-builder, dispatched by main per scope
├── package.json              # Workspace membership, language tag, the lint, format, check, and typecheck targets, the test target's inputs
├── tsconfig.json             # The declarations header's config plus allowImportingTsExtensions for the relative .ts imports
└── vitest.config.ts          # Vitest configuration that gives the project its test target
```

## [02]-[COMPOSITION]

Every event takes one path from the engine to its result:

```mermaid
flowchart LR
    event([Engine event]) --> adapter["events/&lt;event&gt;.ts<br>reads the store, gathers $ facts"]
    store[("$.store<br>host/store.ts decoders")] -.-> adapter
    adapter --> fold["fold(rules)(e)<br>policies/*.ts with text/*.ts"]
    fold --> decision{"Decision<br>composition/decision.ts"}
    decision -->|rewrite| next["$.ui.notice, once stamps<br>await next(input)"]
    next --> result([Result with the context appended])
    decision -->|deny| deny(["{ deny: reason }"])
    decision -->|answer| answer([The answer as the result])
```

## [03]-[MODULES]

The composition, text, and host modules export the carriers, the text operations, and the boundary values the policies and events build on:

| [INDEX] | [MODULE]                  | [GROUP]      | [EXPORTS]                                                                                  |
| :-----: | :------------------------ | :----------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `composition/option.ts`   | Constructors | `some`, `none`, `fromPredicate`, `fromNullable`, `fromBoolean`, `liftPredicate`            |
|  [02]   | `composition/option.ts`   | Refinements  | `struct`, `optional`, `isRecord`                                                           |
|  [03]   | `composition/option.ts`   | Operations   | `map`, `flatMap`, `getOrElse`, `toArray`, `traverse`, `forEach`                            |
|  [04]   | `composition/decision.ts` | Constructors | `rewrite`, `deny`, `answer`                                                                |
|  [05]   | `composition/decision.ts` | Operations   | `when`, `bind`, `fold`, `absurd`                                                           |
|  [06]   | `text/argv.ts`            | Operations   | `leaves`, `strip`, and the `INTERPRETER` sentinel word                                     |
|  [07]   | `text/path.ts`            | Operations   | `basename`, `extension`, `under`, `relative`                                               |
|  [08]   | `text/lines.ts`           | Operations   | `lines`, `first`                                                                           |
|  [09]   | `text/replace.ts`         | Operations   | `replace`, `notes`                                                                         |
|  [10]   | `host/store.ts`           | Tables       | `NAMESPACES`, `KIND_NAMES`, `STATUS`, `PART_NAMES`                                         |
|  [11]   | `host/store.ts`           | Keys         | `key`, `keys`, `ids`, `suffix`, `id`                                                       |
|  [12]   | `host/store.ts`           | Decoders     | `decode<Row>` per row type, `decodeJson`, `secretsOf`, `cleanedOf`, `is<Type>` refinements |
|  [13]   | `host/options.ts`         | Options      | `OPTIONS`, `Options`, `options`, `whenEnabled`                                             |
|  [14]   | `host/tools.d.ts`         | Tool input   | `McpToolInputs['mcp__function-hooks__close']`                                              |
|  [15]   | `host/tools.d.ts`         | Op events    | `UndeclaredOpEventOf`, `UndeclaredOpValueOf`, `SessionAuthorization`, the two omitted ops  |

## [04]-[STORE]

`NAMESPACES` in `host/store.ts` names every key prefix, and each key has one writer. Stamp rows are read by presence, and every other row through its `decode<Row>`:

| [INDEX] | [KEY]                      | [ROW]      | [WRITER]                          | [READER]                                                             |
| :-----: | :------------------------- | :--------- | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `secrets`                  | `Secrets`  | Seeded outside the plugin         | `prompt.submit`, `tool.call`                                         |
|  [02]   | `session/<session>`        | `Session`  | `session.start`                   | `tool.call`, `skill.prompt`, the timer, every `$.process.run`        |
|  [03]   | `injected/<session>/<key>` | `Stamp`    | `tool.call`                       | `tool.call`                                                          |
|  [04]   | `loaded/<session>/<skill>` | `Stamp`    | `skill.prompt`                    | `tool.call`                                                          |
|  [05]   | `snapshot/<session>/<vm>`  | `Stamp`    | `tool.call`, recording tools      | `tool.call`                                                          |
|  [06]   | `dns/<session>/<domain>`   | `Stamp`    | `tool.call`, recording tools      | `tool.call`                                                          |
|  [07]   | `prompt/<session>`         | `Notice`   | `prompt.submit`                   | `tool.call`                                                          |
|  [08]   | `findings/<id>`            | `Finding`  | `turn.complete`, `close`          | `close`, the timer, pruned past the window at start                  |
|  [09]   | `summary`                  | `Summary`  | `turn.complete`, `close`, start   | `prompt.context`, `ui.render`                                        |
|  [10]   | `notice`                   | `Notice`   | The timer, refused spawn          | `ui.render`, its button deletes it                                   |
|  [11]   | `cleaned`                  | `Cleaned`  | `close`                           | `session.start`, the timer, `close`                                  |
|  [12]   | `dispatch/<batchId>`       | `Dispatch` | The timer                         | `agent.spawn`, the timer                                             |
|  [13]   | `skill/<skill>`            | `Skill`    | `skill.prompt`                    | No plugin reader, the cleaning pass over the skills part             |
|  [14]   | `scan/<id>`                | `Scan`     | `tool.call`, a landed edit        | `skill.prompt`, the telemetry block, pruned past the window at start |
|  [15]   | `roslyn/<session>`         | `Stamp`    | `tool.call`, a landed `.cs` write | `tool.call`, the wait before a read of the server                    |

`Session.env` is the answer of `mise env --json` in the session's working directory, PATH with the mise installs first and the `mise.toml` `[env]` rows, and every `$.process.run` passes it as `init.env`, so a child resolves the same binaries and values as the agent shell does through `CLAUDE_ENV_FILE`. Both channels run `mise` from the PATH `claude` was launched with, and a launch without it fails `session.start` with `$.process.run(mise) failed to start: ENOENT` in the debug file beside the settings hook's `mise: command not found`. The `mise` rows of `SHELL` rewrite a `mise x` or `mise exec` leaf to the command after its tool specs, with the context line naming the form replaced, and refuse a leaf that names no command.

## [05]-[OPTIONS]

`userConfig` in `plugin.json` declares each option with its type and default, the host validates the values before the module loads, and `options(raw)` in `host/options.ts` narrows them once. Values sit under `pluginConfigs[<plugin key>].options` in user settings, the `--settings` flag, or managed settings, and Claude Code ignores the key in a project's `.claude/settings.json` and `.claude/settings.local.json`. The key is the plugin id `function-hooks@rasm` for the installed copy and the manifest `name` (or `<name>@inline`) for a `--plugin-dir` load. Refusals, rewrites, redaction, and routing context are always on, and each option turns on one behavior:

| [INDEX] | [OPTION]         | [DEFAULT] | [BEHAVIOR]                                                                    |
| :-----: | :--------------- | :-------- | :---------------------------------------------------------------------------- |
|  [01]   | `packageManager` | `pnpm`    | The command that replaces `npm` in a Bash call                                |
|  [02]   | `speak`          | `false`   | Speaks a summary of each answer at `turn.complete`                            |
|  [03]   | `classify`       | `false`   | Classifies each answer at `turn.complete` and writes one `findings/<id>` row  |
|  [04]   | `dispatch`       | `false`   | Sets the status line, draws the band, serves the `close` tool, runs the timer |

## [06]-[POLICIES]

Each policy file holds its tables and the rules over them, and the events table names the adapter that folds each rule:

| [INDEX] | [FILE]        | [TABLES]                                            | [RULES]                                                                              |
| :-----: | :------------ | :-------------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `git.ts`      | `GIT`                                               | `gitGuard`, `gitPaths`                                                               |
|  [02]   | `shell.ts`    | `SHELL`, `_TIMEOUT`                                 | `shellRule`, `commandTimeout`, `skipNxCache`, `packageManager`, `shellSkills`        |
|  [03]   | `paths.ts`    | `PATHS`                                             | `pathRule`, `pathSkills`, `recordSearches`                                           |
|  [04]   | `tools.ts`    | `TOOLS`, `FAMILIES`, `SERVERS`, `FETCH`, `DESCRIBE` | `toolRule`, `toolRecords`, `toolSkills`, `describeRule`                              |
|  [05]   | `secrets.ts`  | The `secrets` rows as `pairs`                       | `redact`, `restore`                                                                  |
|  [06]   | `agents.ts`   | `AGENTS`, `OFFERS`                                  | `spawnRule`, `spawnLabel`                                                            |
|  [07]   | `findings.ts` | `CLOSE`                                             | `finding`, `summarize`, `withFinding`, `questions`, `close`                          |
|  [08]   | `findings.ts` | `FINDING_VIEWS`, `DUE_MS`                           | `open`, `due`, `status`, `batch`                                                     |
|  [09]   | `kinds.ts`    | `KINDS`, `LABELS`                                   | `hasEvidence`, `CLASSIFIER_PROMPT`, `decodeFields`, `fieldsOf`                       |
|  [10]   | `scan.ts`     | `SCAN`                                              | `scanRows`, `needsRuleIds`, `scanHits`, `abort`, `stem`, `TREE`, `stale`, the blocks |
|  [11]   | `roslyn.ts`   | `SERVER`, `SOLUTION`, `WRONG_DIAGNOSTICS`           | The decoders, the request builders, `replyClass`, `diagnosticLines`                  |
|  [12]   | `roslyn.ts`   | `ROSLYN_READS`, `WATCHER_SETTLE_MS`                 | `isRoslynRead`, `settleWait`, `settleLine`, `filterEnvelope`, `droppedLines`         |

The dependency row of `PATHS` reads the names an Edit drops from a manifest, the catalog, `Directory.Packages.props`, or `pyproject.toml`, and `recordSearches` builds one `rg` search per name over the sibling records of its language and every `README.md`, run in the hook body before the call. A name another record still holds gets the context line naming its holders, and a name held nowhere is a removal with no line.

The `sleep` rows of `SHELL` drop a top-level `sleep <duration>` leaf with its joining operator before the call runs, one leaf per re-read, and refuse a call of sleep leaves alone, and the context line names `run_in_background: true` with its completion notification and the `until` loop under the Monitor tool as the wait forms. A sleep inside an `if`, `do`, `{`, or `case` compound or beside a pipe stays. The `ast-grep` rows rewrite `scan -r <file>` on a rule under `tools/ast-grep/rules/` or `rewrites/` to `--filter '^<id>$'` with the file stem as the id and `--error=<id>` under `rewrites/`, a bare id after `test` to `--filter '^<id>$'`, and drop `-l` on `scan`, because `scan -r` loads no `utilDirs`, `test` takes no positional, and `scan` reads each rule's `language` field.

## [07]-[EVENTS]

`hooks/events/` holds one file per engine event, and a file with no rows is the slot for the event's first hook:

| [INDEX] | [EVENT]            | [HOOK]                                                                                                       |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `*`                | No row                                                                                                       |
|  [02]   | `engine.create`    | No row                                                                                                       |
|  [03]   | `session.start`    | The session row and the prune, then under `dispatch` the status line, the served tool, and the timer         |
|  [04]   | `prompt.submit`    | Redaction of secret values to their ids and the `prompt/<session>` row                                       |
|  [05]   | `prompt.section`   | No row                                                                                                       |
|  [06]   | `prompt.context`   | The open-question block from the `summary` row                                                               |
|  [07]   | `tool.describe`    | The skill's line and the `DESCRIBE` line, one per line, before the tool's own description                    |
|  [08]   | `turn.start`       | No row                                                                                                       |
|  [09]   | `tool.call`        | The rules folded in order and the decision mapped onto the result, then the arms after `next`                |
|  [10]   | `agent.offer`      | One matched hook per `OFFERS` row, and the table holds none                                                  |
|  [11]   | `agent.spawn`      | The brief of the spawned type and the newest batch's lines appended to the prompt                            |
|  [12]   | `skill.prompt`     | The stamps of the loaded skill, under `ast-grep` the tree facts and hit telemetry                            |
|  [13]   | `attribution.text` | No row                                                                                                       |
|  [14]   | `turn.step`        | No row                                                                                                       |
|  [15]   | `turn.complete`    | The summary and the classifier over each answered turn, each under its option, after `next`                  |
|  [16]   | `ui.render`        | Under `dispatch`, the `AbovePrompt` band on the terminal with no survey, the notice and the `summary` count  |
|  [17]   | `ui.resolve`       | No row                                                                                                       |
|  [18]   | `ui.press`         | No row, the hide button's `onPress` sits in `ui-render.ts`                                                   |
|  [19]   | `ui.input`         | No row                                                                                                       |
|  [20]   | `ui.select`        | No row                                                                                                       |

## [08]-[CHECKS]

`function-hooks:check` composes `lint`, `test`, and `typecheck` runs under `harness`, the target that writes the declarations it reads. Unit tests run without them because Vitest erases the type-only `claude-code` imports:

| [INDEX] | [COMMAND]                                               | [CHECK]                                                         |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `pnpm exec nx run function-hooks:check`                 | Read-only lint and test                                         |
|  [02]   | `pnpm exec nx run function-hooks:typecheck`             | Typecheck over `.claude/types/`                                 |
|  [03]   | `claude plugin validate .claude/plugins/function-hooks` | Lists the registered events and every `$` call the module makes |
|  [04]   | `nx run rasm:harness`                                   | Brings the declarations and the installed copy up to date       |

`.claude/types/` is gitignored, and `typecheck` runs uncached because the declarations are no input. The `test` target names the guidance files as inputs and runs uncached, and a guidance edit reruns it. The validator's `version` warning is accepted, because the version resolves from the commit of the source.

## [09]-[PROOFS]

Hooks are proven by a run from the repository root with a transcript and a debug file that show the hook's own decision:

```bash
claude --plugin-dir .claude/plugins/function-hooks --debug -p '<prompt>' --output-format stream-json --verbose
```

The prompt opens with `Run exactly this tool call and report its result verbatim, do not try another route:` and names the call. The transcript's `tool_result` block holds `is_error: true` with `<tool_use_error><reason></tool_use_error>` on a deny and the output on a pass, and a rewrite's `context` line reaches the model and returns restated in its answer. `~/.claude/debug/<session id>.txt`, the id from the transcript's `init` event, holds the engine's lines:

- `hooks module function-hooks loaded (worker, environment 1); events: ...` — the module loaded with the registered set
- `tool.call <Tool> <id>: resolved by a hooks module (deny: <reason>)` — the deny came from a hook and not from a permission rule
- `hooks module function-hooks tool.call settled in <n>ms` — the hook settled, a rewrite's `context` line then sits in the transcript
- `$.store.set (function-hooks@inline): <key>` — a store write with its key, the first call of a once row and no later one
- `$.process.run (function-hooks): <argv[0]> with <n> args in <cwd> (pid <pid>)` — the scan child, or the record search of a dropped dependency row
- `$.mcp.call (function-hooks): mcp__roslyn-codelens__get_diagnostics (project, severity, includeAnalyzers)` — the Roslyn call
- `... answered in <n>ms: <k> block(s)` — the Roslyn reply
- `<argv[0]> exited <code> in <n>ms, <out> + <err> chars` — the child's exit, the second line of the pair
- `prompt.submit: text rewritten by a hook (<n> -> <m> chars)` — a redaction rewrote the prompt
- `hook failed: function-hooks: <error> (<event>; skipped; what is below it ran in its place)` — a failing hook is skipped and never fatal
- `ui.render key=<component> settled in <n>ms` — one draw-path evaluation per component instance and version, absent while the hook is unregistered

The transcript's `tool_use` block holds the input the model wrote, and a rewrite of a field beside `command` (the Bash `timeout` the GNU `timeout` prefixes of a command become, every prefix spliced out by span and the largest duration capped at 600000 ms) is read from the `context` line alone. Scan children run with the session row's environment, and the `$.process.run` and `$.mcp.call` lines spell the origin without `@inline`.

Once-per-session context lines reach every call of one parallel batch, because the calls read the store before any of them stamps its key. Redaction proofs seed `secrets` in the plugin's store file under `~/.claude/plugins/store/`, read the rewrite line in the debug file and the redacted text under `prompt/<session>`, and remove the seed.

## [10]-[KNOWN_ISSUES]

Facts of RoslynCodeLens.Mcp 2.18.0 the roslyn arm works under and of Claude Code 2.1.263 the declarations work under, each issue with the release change that retires it:

| [INDEX] | [KIND] | [FACT]                                                                                                                       |
| :-----: | :----- | :--------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | issue  | `AnalyzerRunner.cs` calls `WithAnalyzers(analyzers, options: null)`, so `.editorconfig` analyzer options do not apply        |
|  [02]   | issue  | `IDE0055` reports the brace style under Roslyn's defaults                                                                    |
|  [03]   | leaves | A release attaches `project.AnalyzerOptions`, then the `IDE0055` row of `WRONG_DIAGNOSTICS` in `policies/roslyn.ts` leaves   |
|  [04]   | issue  | `AnalyzerAllowlist.cs` hardcodes `~/.nuget/packages`, `SECURITY.md` names `NUGET_PACKAGES` and `globalPackagesFolder` unread |
|  [05]   | issue  | The `.cache/nuget/packages` analyzers load under `analyzerPolicy: all` in the trust file alone                               |
|  [06]   | leaves | A release honors `globalPackagesFolder`, then the policy narrows to `strict`                                                 |
|  [07]   | issue  | The load line lists `session.authorize` and `flag.value`, and `/plugin-types` omits both, anthropics/claude-code#92469       |
|  [08]   | issue  | One build constant gates both, `$.session.authorize()` answers `null` and `$.flag` is `undefined`, no hook reads either      |
|  [09]   | leaves | A release declares them, then `UndeclaredOpEventOf` and its value type leave `host/tools.d.ts` for the generated file        |
