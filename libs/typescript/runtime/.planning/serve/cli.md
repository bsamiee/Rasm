# [RUNTIME_CLI]

The terminal entry family under the one front-door assembly law: a verb family is a `Command` VALUE a domain contributes as data, the APP folds selected families through `Command.withSubcommands` into exactly one root — the same posture as the assembled `HttpApi`, so the god-CLI has no lib-side existence — and `Verb.main` is the run rail that folds `--help`/`--version` to clean exits instead of failures. The flag-config bridge is law: a flag and its environment variable are one declaration (`Options.withFallbackConfig`), a flag decodes into a branded value at the terminal boundary (`Options.withSchema`) exactly as the wire and route boundaries decode once, a missing interactive input prompts instead of failing (`Options.withFallbackPrompt`), and shell completion is a derivation of the root, never a maintained script. Output is one algebra: every verb's output is a composed `Doc<Ansi>` — structure through the printer's layout combinators, semantic markup through one role table, decoded values through the Schema-derived `Pretty` printer — folded to a string at exactly one render seam whose ambient mode row decides styled, plain, or machine form, with live redraw as directive rows over the same seam. `Ops` ships the lib runbook family — doctor, replay, inspect — as code executing over `proc` and `net` owners, never documents. The module ships on the `./server` exports subpath as `runtime/src/serve/cli.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         | [PUBLIC] |
| :-----: | :--------------- | :----------------------------------------------------------------------------- | :------- |
|  [01]   | `ASSEMBLY_LAW`   | the contribution shape, the clean-exit run rail, bridge rows, completion table | `Verb`   |
|  [02]   | `OPS_FAMILY`     | the doctor/replay/inspect runbooks over their capability sources               | `Ops`    |
|  [03]   | `ROLE_TABLE`     | the semantic-role directive rows, the role annotator, the theming seam         | `Print`  |
|  [04]   | `STRUCTURE_ROWS` | kv, table, verdicts, banner, prose, pretty composition rows                    | `Print`  |
|  [05]   | `PRINT_SEAM`     | the ambient mode row, the fold to string, the display effect, live redraw      | `Print`  |

## [02]-[ASSEMBLY_LAW]

[ASSEMBLY_LAW]:
- Owner: `Verb.main` — the run rail the lib genuinely adds: `ValidationError.isHelpRequested` folds to a clean exit (help and version are outcomes, not faults), every other `ValidationError` propagates for the boot edge to report. The FOLD itself is app code by law — `Command.make(name).pipe(Command.withSubcommands([familyA, familyB, Ops.family(sources)]), Command.run({ name, version }))` — because the package's own combinators ARE the assembly surface and a lib member re-wrapping them is the one-hop forward this corpus deletes; an app's CLI entry is `row.main(Verb.main(built)(argv))` under `exec#ROOT_SELECT`'s one-`main` law, and `Command.run` demands the platform `Environment` the runtime row's `context` satisfies — one runtime choice covers server and CLI.
- Law: the bridge rows are the boundary decode — `Options.withFallbackConfig(config)` unifies a flag with its `config#SETTING_OWNER` provider value in one declaration (flag wins, config fills, so an env var and a flag are never two sources), `Options.withSchema(schema)` decodes a flag into a core-branded value at parse time, `Args.fileSchema(schema)` admits a file's content through a schema, and `Options.withFallbackPrompt(prompt)` prompts exactly when the flag is absent on an interactive terminal — the terminal boundary decodes once, a handler never re-validates its inputs, and `ConfigFile.layer(name)` mounts file-resolved flags into the same provider chain at the root.
- Law: completion and wizard are derivation rows — `Verb.completions(root)` folds the shell literal through the `_shells` table (`getBashCompletions` | `getFishCompletions` | `getZshCompletions` over the built root) and prints the lines; `Verb.wizard(root)` surfaces `Command.wizard`, walking the parse tree interactively and printing the assembled invocation — both derive from the one assembled value, so neither can drift from the parse tree.
- Law: subtree capability is scoped provision — `Command.provide(family, layer)` scopes a Layer to one verb family so the ops family carries its exec runtime without leaking it to app verbs; parser policy is one root `CliConfig.layer` value.
- Growth: a new app verb family is contributed data (zero lib edits); a new bridge axis is one `Options` combinator row.
- Packages: `@effect/cli` (`Command`, `Options`, `Args`, `Prompt`, `ValidationError`, `CliConfig`, `ConfigFile`); `effect` (`Effect`, `Config`).

```typescript
import { Args, Command, Options, Prompt, ValidationError } from "@effect/cli"
import { FileSystem, type PlatformError, Terminal } from "@effect/platform"
import { Doc, Optimize } from "@effect/printer"
import { Ansi, AnsiDoc } from "@effect/printer-ansi"
import { Array, Config, Context, Data, Effect, Option, Pretty, Record, Schema, Struct } from "effect"
import { Envelope, Fanout } from "../net/pubsub.ts"
import { Life } from "../proc/life.ts"

const _main = <E, R>(
  built: (args: ReadonlyArray<string>) => Effect.Effect<void, E | ValidationError.ValidationError, R>,
) =>
  (args: ReadonlyArray<string>): Effect.Effect<void, E | ValidationError.ValidationError, R> =>
    built(args).pipe(
      Effect.catchIf(
        (fault): fault is ValidationError.HelpRequested =>
          ValidationError.isValidationError(fault) && ValidationError.isHelpRequested(fault),
        () => Effect.void,
      ),
    )

const _shells = {
  bash: Command.getBashCompletions,
  fish: Command.getFishCompletions,
  zsh: Command.getZshCompletions,
} as const

const _completions = <Name extends string, R, E, A>(root: Command.Command<Name, R, E, A>) =>
  Command.make("completions", { shell: Args.choice(Struct.keys(_shells), { name: "shell" }) }, ({ shell }) =>
    Effect.gen(function* () {
      const lines = yield* _shells[shell](root)
      yield* _out(Doc.vsep(Array.map(lines, Doc.string)))
    }))

const _wizard = <Name extends string, R, E, A>(root: Command.Command<Name, R, E, A>) =>
  Command.make("wizard", {}, () =>
    Effect.flatMap(Command.wizard(root), (line) => _out(Doc.vsep(Array.map(line, Doc.string)))))

const Verb = { completions: _completions, main: _main, wizard: _wizard } as const
```

## [03]-[OPS_FAMILY]

[OPS_FAMILY]:
- Owner: `Ops.family(sources)` — the lib runbook family built over app-supplied capability sources so the verbs stay composition-free: `doctor` folds the health anchor and the app's check rows, `replay` re-publishes a captured fanout envelope, `inspect` emits the canonical spec artifact — one record, three verbs, every handler rendering through the role and structure rows.
- Law: `doctor` accumulates, never aborts — the shipped floor probes are the `life#PROBE_ROUTES` report per kind plus the app's `checks` rows (each a named `Effect<string, OpsCheckFault>` verdict — config resolution, engine reachability, dependency versions through `Proc.run`), folded with `Effect.partition` so every probe runs and the rendered table shows the whole verdict surface in one pass; the three independent life reports run concurrently, and the exit is non-zero when any probe failed, which makes the command a CI gate. A primitive string error would reopen an ungoverned fault rail at the public contribution boundary.
- Law: `replay` re-drives a captured delivery — `Args.fileSchema(Schema.parseJson(Envelope))` admits the capture file through the `Envelope` schema at the argv boundary and the handler publishes straight through `Fanout.publish`, the receipt's `duplicate` flag rendered as the idempotent-noop evidence; a missing topic prompts through `Prompt.select` over the app's declared topic roster, and the mutation gates on `Prompt.confirm` through the same fallback bridge — a `--yes`/`-y` flag pre-answers both for CI, so interactive safety costs scripts nothing.
- Law: `inspect` emits derivations — the `api#EMIT` artifact to a path or stdout — so the served contract's canonical bytes are one verb away for diffing; the `--out` flag falls back to the `INSPECT_OUT` config row through the bridge, this page's own demonstration of the flag-config law.
- Law: runbooks are code — a new runbook is one `Command` row in this family with its probe or effect, never a document; the family is `Command.provide`-scoped with its exec Layer by the app when it needs elevated capability.
- Boundary: process execution mechanics are `exec#COMMAND_SPEC`'s; fanout semantics are `pubsub#PORT_SHAPE`'s; what checks exist beyond the shipped floor is app data through `sources.checks`.
- Packages: `@effect/cli` (`Command`, `Options`, `Args`, `Prompt`); `../proc/life.ts` (`Life`); `../net/pubsub.ts` (`Fanout`, `Envelope`); `effect` (`Effect`, `Option`, `Schema`); app check rows compose `exec#COMMAND_SPEC`'s `Proc.run` at their own seam.

```typescript
class OpsFault extends Data.TaggedError("OpsFault")<{ readonly verb: string; readonly failed: number }> {
  get class(): "breached" {
    return "breached"
  }
}

class OpsCheckFault extends Data.TaggedError("OpsCheckFault")<{ readonly detail: string }> {}

declare namespace Ops {
  type Check = { readonly name: string; readonly run: Effect.Effect<string, OpsCheckFault> }
  type Sources = {
    readonly artifact: Effect.Effect<string>
    readonly checks: ReadonlyArray<Check>
    readonly topics: ReadonlyArray<string>
  }
}

const _target = Options.text("out").pipe(
  Options.withAlias("o"),
  Options.withFallbackConfig(Config.string("INSPECT_OUT")),
  Options.optional,
)

const _topicFlag = (topics: ReadonlyArray<string>) =>
  Options.text("topic").pipe(
    Options.withFallbackPrompt(
      Prompt.select({
        message: "fanout topic to replay onto",
        choices: Array.map(topics, (topic) => ({ title: topic, value: topic })),
      }),
    ),
  )

const _confirmFlag = Options.boolean("yes").pipe(
  Options.withAlias("y"),
  Options.withFallbackPrompt(Prompt.confirm({ message: "re-publish the captured envelope?" })),
)

const _doctor = (sources: Ops.Sources) =>
  Command.make("doctor", {}, () =>
    Effect.gen(function* () {
      const kinds = ["started", "ready", "live"] as const
      const reports = yield* Effect.forEach(kinds, (kind) => Life.report(kind), { concurrency: "unbounded" })
      const anchor = Array.flatMap(reports, (report) =>
        Array.map(report.rows, (row): Ops.Check => ({
          name: `${report.kind}:${row.label}`,
          run: row.grade === "fail"
            ? Effect.fail(new OpsCheckFault({ detail: Option.getOrElse(row.detail, () => "fail") }))
            : Effect.succeed(row.grade),
        })))
      const [failed, passed] = yield* Effect.partition([...anchor, ...sources.checks], (check) =>
        check.run.pipe(
          Effect.map((detail) => [check.name, detail] as const),
          Effect.mapError((fault) => [check.name, fault.detail] as const),
        ))
      yield* _out(_verdicts({ passed, failed }))
      return yield* Effect.when(
        Effect.fail(new OpsFault({ verb: "doctor", failed: failed.length })),
        () => failed.length > 0,
      )
    }))

const _replay = (sources: Ops.Sources) =>
  Command.make(
    "replay",
    {
      capture: Args.fileSchema(Schema.parseJson(Envelope), { name: "capture" }),
      topic: _topicFlag(sources.topics),
      yes: _confirmFlag,
    },
    ({ capture, topic, yes }) =>
      Effect.gen(function* () {
        const fanout = yield* Fanout
        const receipt = yield* Effect.when(fanout.publish(topic, capture), () => yes)
        yield* Option.match(receipt, {
          onNone: () => _out(_prose("replay declined")),
          onSome: (ack) => _out(_kv([["seq", String(ack.seq)], ["duplicate", String(ack.duplicate)]])),
        })
      }),
  )

const _inspect = (sources: Ops.Sources) =>
  Command.make("inspect", { out: _target }, ({ out }) =>
    Effect.gen(function* () {
      const artifact = yield* sources.artifact
      const fs = yield* FileSystem.FileSystem
      yield* Option.match(out, {
        onNone: () => _out(_raw(artifact)),
        onSome: (target) => fs.writeFileString(target, artifact),
      })
    }))

const _family = (sources: Ops.Sources) =>
  Command.make("ops").pipe(
    Command.withDescription("lib runbooks: doctor | replay | inspect"),
    Command.withSubcommands([_doctor(sources), _replay(sources), _inspect(sources)]),
  )

const Ops = { family: _family } as const
```

## [04]-[ROLE_TABLE]

[ROLE_TABLE]:
- Owner: `_roles` — six composed `Ansi` values under one `as const satisfies Record<string, Ansi.Ansi>` anchor: `fault` (`bold` ⊕ `red`), `warn` (`yellow`), `ok` (`green`), `path` (`cyan`), `emph` (`bold`), `faint` (`blackBright`) — each a monoid composition attached by ONE `Doc.annotate` through `Print.role(kind, doc)`, never one annotate per style bit.
- Law: verbs speak roles, never colors — a verb marks a value `path` or a verdict `fault` and the table decides what that means on a terminal; a theme change is a row edit with zero verb diffs, and `Print.themed(palette)` is the theming seam — one `Doc.reAnnotate` mapping every role annotation through a caller-supplied palette record, so a second palette is a value, never a second render path.
- Growth: a new semantic intent is one row; a verb needing a color that is not an intent is the smell — name the intent.
- Packages: `@effect/printer-ansi` (`Ansi`); `@effect/printer` (`Doc`).

```typescript
const _roles = {
  fault: Ansi.combine(Ansi.bold, Ansi.red),
  warn: Ansi.yellow,
  ok: Ansi.green,
  path: Ansi.cyan,
  emph: Ansi.bold,
  faint: Ansi.blackBright,
} as const satisfies Record<string, Ansi.Ansi>

const _role = (kind: keyof typeof _roles, doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc => Doc.annotate(doc, _roles[kind])

const _themed = (palette: Partial<Record<keyof typeof _roles, Ansi.Ansi>>) =>
  (doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
    Doc.reAnnotate(doc, (held) => {
      const entry = Array.findFirst(
        Record.toEntries(_roles),
        ([, ansi]) => ansi === held,
      )
      return Option.match(entry, {
        onNone: () => held,
        onSome: ([kind]) => palette[kind] ?? held,
      })
    })
```

## [05]-[STRUCTURE_ROWS]

[STRUCTURE_ROWS]:
- Owner: the composition rows, each a fold over the printer's own algebra — `kv(pairs)` aligns each value from the current cursor through `Doc.align`; `table(columns, rows)` reads widths from column policy values and applies `Doc.fill`, marks the head `emph`, and stacks with `Doc.vsep`; `seq(items, shape)` renders a delimited collection through `Doc.list`/`Doc.tupled`; `verdicts({ passed, failed })` renders the doctor shape; `banner(title)` is the `emph` section head; `prose(text)` wraps through `Doc.reflow`; `raw(text)` admits pre-formed text through `Doc.string`; and `pretty(schema)` derives a canonical decoded-value renderer through `Pretty.make(schema)`.
- Law: rows return `AnsiDoc` values, never strings — composition stays open (a verb nests a `table` under a `banner` with `Doc.vsep`) and the fold to text happens once at the seam; a string-returning row re-closes the algebra per call site and is the rejected form.
- Law: width is policy, not JavaScript string arithmetic — a table's column declarations carry widths, `Doc.fill` pads without truncation, and wide values remain visible instead of corrupting alignment through UTF-16 `.length` guesses.
- Growth: a new output shape is one row composing the existing algebra; a shape needing a new layout primitive reaches for the printer's own (`align`, `hang`, `encloseSep`) before any local invention.
- Packages: `@effect/printer` (`Doc`); `effect` (`Array`, `Pretty`).

```typescript
const _kv = (pairs: ReadonlyArray<readonly [string, string]>): AnsiDoc.AnsiDoc =>
  Doc.vsep(Array.map(pairs, ([label, value]) =>
    Doc.hsep([_role("faint", Doc.text(label)), Doc.align(Doc.text(value))])))

const _table = (
  columns: ReadonlyArray<{ readonly header: string; readonly width: number }>,
  rows: ReadonlyArray<ReadonlyArray<string>>,
): AnsiDoc.AnsiDoc => {
  const lined = (cells: ReadonlyArray<string>, mark: (doc: AnsiDoc.AnsiDoc) => AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
    Doc.hsep(Array.map(columns, (column, index) => Doc.fill(mark(Doc.text(cells[index] ?? "")), column.width)))
  return Doc.vsep([
    lined(Array.map(columns, (column) => column.header), (doc) => _role("emph", doc)),
    ...Array.map(rows, (row) => lined(row, (doc) => doc)),
  ])
}

const _verdicts = (report: {
  readonly passed: ReadonlyArray<readonly [string, string]>
  readonly failed: ReadonlyArray<readonly [string, string]>
}): AnsiDoc.AnsiDoc =>
  Doc.vsep([
    ...Array.map(report.passed, ([name, detail]) =>
      Doc.hsep([_role("ok", Doc.text("pass")), Doc.text(name), _role("faint", Doc.text(detail))])),
    ...Array.map(report.failed, ([name, detail]) =>
      Doc.hsep([_role("fault", Doc.text("fail")), Doc.text(name), Doc.text(detail)])),
  ])

const _seq = (items: ReadonlyArray<string>, shape: "list" | "tuple" = "list"): AnsiDoc.AnsiDoc =>
  (shape === "list" ? Doc.list : Doc.tupled)(Array.map(items, Doc.text))

const _banner = (title: string): AnsiDoc.AnsiDoc => _role("emph", Doc.text(title))

const _prose = (text: string): AnsiDoc.AnsiDoc => Doc.reflow(text)

const _raw = (text: string): AnsiDoc.AnsiDoc => Doc.string(text)

const _pretty = <A, I, R>(schema: Schema.Schema<A, I, R>): ((value: A) => AnsiDoc.AnsiDoc) => {
  const show = Pretty.make(schema)
  return (value) => Doc.string(show(value))
}
```

## [06]-[PRINT_SEAM]

[PRINT_SEAM]:
- Owner: the one fold from document to terminal — `_MODES` is the render-policy vocabulary: each mode row CARRIES its fold (`tty` renders escape codes through `AnsiDoc.render({ style: "pretty" })`, `plain` strips annotations with `Doc.unAnnotate` then renders pretty, `wire` strips and renders `compact` for single-line machine form), so `Print.text(doc, mode)` is one keyed lookup and a new mode is one row whose missing fold fails at the vocabulary declaration, never a conditional arm; `Print.Mode` is a `Context.Reference` row (`tty` default; `plain` for `--no-color` and non-TTY pipes; `wire` for machine emission) the app root or a global flag overrides once; `Print.out(doc)` reads the ambient mode and writes through the platform `Terminal.display` — the only print site, so output is testable as data everywhere above it.
- Law: mode is ambient, never a parameter — verbs call `Print.out(doc)` with zero knowledge of the egress form, `--no-color` is one root-level `Effect.provideService(Print.Mode, "plain")`, and CI inherits `plain` by the same provision; a per-call mode argument smuggles the knob back into every verb and is the rejected form.
- Law: live redraw is a directive row over the same seam — `Print.sweep(rows)` writes `Ansi.stringify(Ansi.eraseLines(rows))` through the terminal before the next `out`, so a progress loop is erase-then-render with zero cursor arithmetic in verbs, and the directive short-circuits to a plain newline outside `tty` mode so piped output stays append-only.
- Law: deeply nested structures compose through `Print.deep` — `Optimize.optimize(doc, FusionDepth.Deep)` fuses associativity while preserving `AnsiDoc`; the ambient mode row still performs the only render, so optimization never opens a second terminal seam.
- Boundary: `@effect/cli`'s own `HelpDoc` lowers onto this same `AnsiDoc` rail, so parse-error help and verb output share one render seam; the `Terminal` binding is the runtime row's.
- Packages: `@effect/printer-ansi` (`AnsiDoc`, `Ansi`); `@effect/printer` (`Doc`); `@effect/platform` (`Terminal`); `effect` (`Context`, `Effect`).

```typescript
const _MODES = {
  tty: (doc: AnsiDoc.AnsiDoc): string => AnsiDoc.render(doc, { style: "pretty" }),
  plain: (doc: AnsiDoc.AnsiDoc): string => Doc.render(Doc.unAnnotate(doc), { style: "pretty" }),
  wire: (doc: AnsiDoc.AnsiDoc): string => Doc.render(Doc.unAnnotate(doc), { style: "compact" }),
} as const satisfies Record<string, (doc: AnsiDoc.AnsiDoc) => string>

class _Mode extends Context.Reference<_Mode>()("runtime/serve/Print/Mode", {
  defaultValue: (): keyof typeof _MODES => "tty",
}) {}

const _text = (doc: AnsiDoc.AnsiDoc, mode: keyof typeof _MODES): string => _MODES[mode](doc)

const _out = (doc: AnsiDoc.AnsiDoc): Effect.Effect<void, PlatformError.PlatformError, Terminal.Terminal> =>
  Effect.gen(function* () {
    const mode = yield* _Mode
    const terminal = yield* Terminal.Terminal
    yield* terminal.display(`${_text(doc, mode)}\n`)
  })

const _sweep = (rows: number): Effect.Effect<void, PlatformError.PlatformError, Terminal.Terminal> =>
  Effect.gen(function* () {
    const mode = yield* _Mode
    const terminal = yield* Terminal.Terminal
    yield* terminal.display(mode === "tty" ? Ansi.stringify(Ansi.eraseLines(rows)) : "\n")
  })

const _deep = (doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
  Optimize.optimize(doc, Optimize.FusionDepth.Deep)

const Print = {
  Mode: _Mode,
  modes: Struct.keys(_MODES),
  roles: _roles,
  role: _role,
  themed: _themed,
  kv: _kv,
  table: _table,
  seq: _seq,
  verdicts: _verdicts,
  banner: _banner,
  prose: _prose,
  raw: _raw,
  pretty: _pretty,
  text: _text,
  deep: _deep,
  out: _out,
  sweep: _sweep,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ops, OpsCheckFault, OpsFault, Print, Verb }
```
