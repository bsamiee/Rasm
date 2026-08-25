# [RUNTIME_CLI]

Terminal entry rides one front-door assembly law: a verb family is a `Command` VALUE a domain contributes as data, the APP folds selected families through `Command.withSubcommands` into exactly one root — the same posture as the assembled `HttpApi`, so the god-CLI has no lib-side existence — and `Verb.main` is the run rail that folds `--help`/`--version` to clean exits instead of failures. Flag-config bridging is law: a flag and its environment variable are one declaration (`Options.withFallbackConfig`), a flag decodes into a branded value at the terminal boundary (`Options.withSchema`) exactly as the wire and route boundaries decode once, a missing interactive input prompts instead of failing (`Options.withFallbackPrompt`), and shell completion is a derivation of the root, never a maintained script. Output is one algebra: every verb's output is a composed `Doc<Ansi>` — structure through the printer's layout combinators, semantic markup through one role table, decoded values through the Schema-derived `Pretty` printer — folded to a string at exactly one render seam whose ambient mode row decides styled, plain, or machine form, with live redraw as directive rows over the same seam. `Ops` ships the lib runbook family — doctor, replay, inspect — as code executing over `proc` and `net` owners, never documents. This module ships on the `./server` exports subpath as `runtime/src/serve/cli.ts`.

## [01]-[INDEX]

- [02]-[ASSEMBLY_LAW]: `Verb` — contribution shape, clean-exit run rail, bridge rows, completion table.
- [03]-[OPS_FAMILY]: `Ops` — doctor, replay, and inspect runbooks over their capability sources.
- [04]-[ROLE_TABLE]: `Print` — semantic-role directive rows, the role annotator, the theming seam.
- [05]-[STRUCTURE_ROWS]: kv, table, verdicts, banner, prose, pretty composition rows; `Print`.
- [06]-[PRINT_SEAM]: `Print` — ambient mode row, the fold to string, the display effect, live redraw.

## [02]-[ASSEMBLY_LAW]

[ASSEMBLY_LAW]:
- Owner: `Verb.main` — the run rail the lib genuinely adds: `ValidationError.isHelpRequested` and a prompt's `Terminal.QuitException` both fold to a clean exit (help, version, and an operator aborting an interactive prompt are outcomes, not faults), every other `ValidationError` propagates for the boot edge to report. Assembly itself is app code by law — `Command.make(name).pipe(Command.withSubcommands([familyA, familyB, Ops.family(sources)]), Command.run({ name, version }))` — because the package's own combinators ARE the assembly surface and a lib member re-wrapping them is the one-hop forward this corpus deletes; an app's CLI entry is `row.main(Verb.main(built)(argv))` under `proc/exec#ROOT_SELECT`'s one-`main` law, and `Command.run` demands the platform `Environment` the runtime row's `context` satisfies — one runtime choice covers server and CLI.
- Law: the bridge rows are the boundary decode — `Options.withFallbackConfig(config)` unifies a flag with its `proc/config#SETTING_OWNER` provider value in one declaration (flag wins, config fills, so an env var and a flag are never two sources), `Options.withSchema(schema)` decodes a flag into a core-branded value at parse time, `Args.fileSchema(schema)` admits a file's content through a schema, and `Options.withFallbackPrompt(prompt)` prompts exactly when the flag is absent on an interactive terminal — the terminal boundary decodes once, a handler never re-validates its inputs, and `ConfigFile.layer(name)` mounts file-resolved flags into the same provider chain at the root.
- Law: completion and wizard are derivation rows — `Verb.completions(root)` folds the shell literal through the `_shells` table (`getBashCompletions` | `getFishCompletions` | `getZshCompletions` over the built root) and prints the lines; `Verb.wizard(root)` surfaces `Command.wizard`, walking the parse tree interactively and printing the assembled invocation — both derive from the one assembled value, so neither can drift from the parse tree.
- Law: subtree capability is scoped provision — `Command.provide(family, layer)` scopes a Layer to one verb family so the ops family carries its exec runtime without leaking it to app verbs; parser policy is one root `CliConfig.layer` value.
- Growth: a new app verb family is contributed data (zero lib edits); a new bridge axis is one `Options` combinator row.
- Packages: `@effect/cli` (`Command`, `Options`, `Args`, `Prompt`, `ValidationError`, `CliConfig`, `ConfigFile`); `effect` (`Effect`, `Config`).

```typescript
import { Args, Command, HelpDoc, Options, Prompt, ValidationError } from "@effect/cli"
import { FileSystem, type PlatformError, Terminal } from "@effect/platform"
import { Doc, Optimize } from "@effect/printer"
import { Ansi, AnsiDoc } from "@effect/printer-ansi"
import { Array, Config, Context, Effect, Layer, Option, Predicate, Pretty, Record, Schema, Struct } from "effect"
import { Event, Fault, Format } from "@rasm/core"
import { Fanout } from "../net/pubsub.ts"
import { Life } from "../proc/life.ts"

const _main = <E, R>(
  built: (
    args: ReadonlyArray<string>,
  ) => Effect.Effect<void, E | ValidationError.ValidationError | Terminal.QuitException, R>,
) =>
  (args: ReadonlyArray<string>): Effect.Effect<void, E | ValidationError.ValidationError, R> =>
    built(args).pipe(
      Effect.catchIf(
        (fault): fault is ValidationError.HelpRequested | Terminal.QuitException =>
          (ValidationError.isValidationError(fault) && ValidationError.isHelpRequested(fault)) ||
          Predicate.isTagged(fault, "QuitException"),
        () => Effect.void,
      ),
    )

const _shells = {
  bash: Command.getBashCompletions,
  fish: Command.getFishCompletions,
  zsh: Command.getZshCompletions,
} as const

const _completions = <Name extends string, R, E, A>(root: Command.Command<Name, R, E, A>) =>
  Command.make(
    "completions",
    {
      shell: Args.choice(
        Array.map(Struct.keys(_shells), (shell): [string, keyof typeof _shells] => [shell, shell]),
        { name: "shell" },
      ),
    },
    ({ shell }) =>
      Effect.gen(function* () {
        const lines = yield* _shells[shell](root)
        yield* _out(Doc.vsep(Array.map(lines, Doc.string)))
      }),
  )

const _wizard = <Name extends string, R, E, A>(root: Command.Command<Name, R, E, A>) =>
  Command.make("wizard", {}, () =>
    Effect.flatMap(Command.wizard(root), (line) => _out(Doc.vsep(Array.map(line, Doc.string)))))

const Verb = { completions: _completions, main: _main, wizard: _wizard } as const
```

## [03]-[OPS_FAMILY]

[OPS_FAMILY]:
- Owner: `Ops.family(sources)` — the lib runbook family built over app-supplied capability sources so the verbs stay composition-free: `doctor` folds the health anchor and the app's check rows, `replay` re-publishes a captured fanout envelope, `inspect` emits the canonical spec artifact — one record, three verbs, every handler rendering through the role and structure rows.
- Law: `doctor` accumulates, never aborts — the shipped floor probes are the `proc/life#PROBE_ROUTES` report per kind beside the app's `checks` rows (each a named `Effect<string, OpsFault>` verdict — config resolution, engine reachability, dependency versions through `Proc.run`), folded with `Effect.partition` so every probe runs and the rendered table shows the whole verdict surface in one pass; the three independent life reports run concurrently, and the exit is non-zero when any probe failed, which makes the command a CI gate. Partitioning is refusal against everything else, so an anchor row grading `warn` rides the passing arm carrying its own grade as detail rather than minting a third exit posture. Engine census reads are check rows, never new verbs — a fanout row folds `Fanout.consumers(topic)` (the durable-consumer census with its reap arm withheld from CI), and a coordination row folds `Accord.census(filter)` whose record answers `names` beside `Option<Accord.Health>`, so a name-list render folds the record rather than assuming a bare list. One reason-discriminated `OpsFault` carries both refusal routes off one core family mint — `probe` classifies `unavailable` (the dependency answered no), `gate` classifies `breached` (this process refusing its own run) — because a primitive, class-less, or literal-asserted error reopens an ungoverned rail that folds to `defect` at the public contribution boundary.
- Law: narrowing never costs the gate its default — `--check` repeats to name probes and an EMPTY roster is the whole surface, so a script and CI pass nothing and are never prompted, while `--pick` opens `Prompt.multiSelect` over the same resolved probe names for an operator narrowing at a terminal; an abort at that prompt is a clean exit through `Verb.main`'s quit fold, not a doctor failure.
- Law: `replay` re-drives a captured delivery — a capture file is the announcement in the ONE structured JSON spelling `Event.format` names, so `Args.fileText` reads text at argv and the core codec decodes plus admits its bytes. Fabricating HTTP framing or a second attribute Schema invents grammar this page does not own. Its handler publishes through `Fanout.publish`; topic selection and mutation confirmation stay on the prompt bridge, with `--yes`/`-y` pre-answering both for CI.
- Law: `inspect` emits derivations — the `api#EMIT` artifact to a path or stdout — so the served contract's canonical bytes are one verb away for diffing; the `--out` flag falls back to the `INSPECT_OUT` config row through the bridge, this page's own demonstration of the flag-config law.
- Law: runbooks are code — a new runbook is one `Command` row in this family with its probe or effect, never a document; the family is `Command.provide`-scoped with its exec Layer by the app when it needs elevated capability.
- Boundary: process execution mechanics are `proc/exec#COMMAND_SPEC`'s; fanout semantics are `net/pubsub#PORT_SHAPE`'s; what checks exist beyond the shipped floor is app data through `sources.checks`.

```typescript
const _ops = Fault.Class.family(["probe", "gate"] as const, {
  probe: Fault.Class.row({
    class: "unavailable",
    leg: "probe",
    detail: Schema.Struct({ detail: Schema.String, probe: Schema.NonEmptyString }),
    render: ({ detail, probe }) => `probe ${probe} answered no — ${detail}`,
  }),
  gate: Fault.Class.row({
    class: "breached",
    leg: "gate",
    detail: Schema.Struct({ refused: Schema.Int, verb: Schema.NonEmptyString }),
    render: ({ refused, verb }) => `${verb} closed its gate: ${refused} rows refused`,
  }),
})

declare namespace OpsFault {
  type Issue = typeof _ops.payload.Type
  type Reason = (typeof _ops.kinds)[number]
}

class OpsFault extends Schema.TaggedError<OpsFault>()("OpsFault", {
  case: _ops.payload,
}) {
  get class(): Fault.Class.Kind {
    return _ops.classOf(this.case.reason)
  }
  override get message(): string {
    return _ops.render(this.case)
  }
}

declare namespace Ops {
  type Check = { readonly name: string; readonly run: Effect.Effect<string, OpsFault> }
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

const _checkFlag = Options.text("check").pipe(
  Options.withAlias("c"),
  Options.withDescription("probe name to run; repeat to narrow, omit for the whole surface"),
  Options.repeated,
)

const _pickFlag = Options.boolean("pick").pipe(
  Options.withDescription("choose the probe set interactively; a script narrows with --check instead"),
)

const _doctor = (sources: Ops.Sources) =>
  Command.make("doctor", { check: _checkFlag, pick: _pickFlag }, ({ check, pick }) =>
    Effect.gen(function* () {
      const kinds = ["started", "ready", "live"] as const
      const reports = yield* Effect.forEach(kinds, (kind) => Life.report(kind), { concurrency: "unbounded" })
      const anchor = Array.flatMap(reports, (report) =>
        Array.map(report.rows, (row): Ops.Check => ({
          name: `${report.kind}:${row.label}`,
          run: row.grade === "fail"
            ? Effect.fail(new OpsFault({
                case: {
                  reason: "probe",
                  probe: `${report.kind}:${row.label}`,
                  detail: Option.getOrElse(row.detail, () => "the row graded fail and stated nothing"),
                },
              }))
            : Effect.succeed(row.grade),
        })))
      const every = [...anchor, ...sources.checks]
      const named = pick
        ? yield* Prompt.run(Prompt.multiSelect({
            message: "probes to run",
            choices: Array.map(every, (probe) => ({ title: probe.name, value: probe.name })),
            selectAll: "every probe",
          }))
        : check
      const selected = Array.isEmptyReadonlyArray(named)
        ? every
        : Array.filter(every, (probe) => Array.contains(named, probe.name))
      const [failed, passed] = yield* Effect.partition(selected, (probe) =>
        probe.run.pipe(
          Effect.map((detail) => [probe.name, detail] as const),
          Effect.mapError((fault) => [probe.name, fault.message] as const),
        ))
      yield* _out(_verdicts({ pass: passed, fail: failed }))
      return yield* Effect.when(
        Effect.fail(new OpsFault({ case: { reason: "gate", refused: failed.length, verb: "doctor" } })),
        () => failed.length > 0,
      )
    }))

const _replay = (sources: Ops.Sources) =>
  Command.make(
    "replay",
    {
      capture: Args.fileText({ name: "capture" }),
      topic: _topicFlag(sources.topics),
      yes: _confirmFlag,
    },
    ({ capture, topic, yes }) =>
      Effect.gen(function* () {
        const fanout = yield* Fanout
        const announced = yield* _captured(capture[1])
        const landing = yield* Effect.when(fanout.publish(topic, announced), () => yes)
        yield* Option.match(landing, {
          onNone: () => _out(_prose("replay declined")),
          onSome: (landed) => _out(_kv([["position", _position(landed.position)], ["duplicate", String(landed.duplicate)]])),
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

const _byAnsi: ReadonlyMap<Ansi.Ansi, keyof typeof _roles> = new Map(
  Array.map(Record.toEntries(_roles), ([kind, ansi]) => [ansi, kind] as const),
)

const _themed = (palette: Partial<Record<keyof typeof _roles, Ansi.Ansi>>) =>
  (doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
    Doc.reAnnotate(doc, (held) =>
      Option.match(Option.fromNullable(_byAnsi.get(held)), {
        onNone: () => held,
        onSome: (kind) => palette[kind] ?? held,
      }))
```

## [05]-[STRUCTURE_ROWS]

[STRUCTURE_ROWS]:
- Owner: the composition rows, each a fold over the printer's own algebra — `kv(pairs)` admits each value through `Doc.string` and hangs it from the current cursor with `Doc.align`, so a multi-line value's continuation lines stay under the value column; `table(columns, rows)` reads widths from column policy values and pads with `Doc.fillBreak`, marks the head `emph`, and stacks with `Doc.vsep`; `seq(items, shape)` selects a `_shapes` row over the printer's own fixed `encloseSep` derivations; `verdicts(report)` folds the `_grades` roster; `banner(title)` is the `emph` section head; `prose(text)` wraps through `Doc.reflow`; `raw(text)` admits pre-formed text through `Doc.string`; and `pretty(schema)` derives a canonical decoded-value renderer through `Pretty.make(schema)`.
- Law: rows return `AnsiDoc` values, never strings — composition stays open (a verb nests a `table` under a `banner` with `Doc.vsep`) and the fold to text happens once at the seam; a string-returning row re-closes the algebra per call site and is the rejected form.
- Law: caller text enters through `Doc.string`, never `Doc.text` — `text` is declared newline-free and a value carrying one corrupts the stream it lands in, where `string` splits on `hardLine` and every layout combinator above it keeps working.
- Law: width is policy, not JavaScript string arithmetic — column declarations carry the widths and `Doc.fillBreak` pads to them, nesting a cell that outgrows its column onto its own line so every following column still starts where its header does; `Doc.fill` pads in one direction only and lets one wide cell shove the rest of the row sideways, and UTF-16 `.length` guesses are the deleted spelling.
- Growth: a new output shape is one row composing the existing algebra; a third delimiter is one `_shapes` row and a third verdict grade one `_grades` row; a shape needing a new layout primitive reaches for the printer's own (`align`, `hang`, `encloseSep`) before any local invention.
- Packages: `@effect/printer` (`Doc`); `effect` (`Array`, `Pretty`, `Record`).

```typescript
const _kv = (pairs: ReadonlyArray<readonly [string, string]>): AnsiDoc.AnsiDoc =>
  Doc.vsep(Array.map(pairs, ([label, value]) =>
    Doc.hsep([_role("faint", Doc.string(label)), Doc.align(Doc.string(value))])))

const _table = (
  columns: ReadonlyArray<{ readonly header: string; readonly width: number }>,
  rows: ReadonlyArray<ReadonlyArray<string>>,
): AnsiDoc.AnsiDoc => {
  const lined = (cells: ReadonlyArray<string>, mark: (doc: AnsiDoc.AnsiDoc) => AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
    Doc.hsep(Array.map(columns, (column, index) => Doc.fillBreak(mark(Doc.string(cells[index] ?? "")), column.width)))
  return Doc.vsep([
    lined(Array.map(columns, (column) => column.header), (doc) => _role("emph", doc)),
    ...Array.map(rows, (row) => lined(row, (doc) => doc)),
  ])
}

const _grades = {
  pass: { role: "ok", muted: true },
  fail: { role: "fault", muted: false },
} as const satisfies Record<string, { readonly role: keyof typeof _roles; readonly muted: boolean }>

const _verdicts = (
  report: { readonly [G in keyof typeof _grades]: ReadonlyArray<readonly [string, string]> },
): AnsiDoc.AnsiDoc =>
  Doc.vsep(Array.flatMap(Record.toEntries(_grades), ([grade, row]) =>
    Array.map(report[grade], ([name, detail]) =>
      Doc.hsep([
        _role(row.role, Doc.string(grade)),
        Doc.string(name),
        row.muted ? _role("faint", Doc.string(detail)) : Doc.string(detail),
      ]))))

const _shapes = { list: Doc.list, tuple: Doc.tupled } as const satisfies Record<string, typeof Doc.list>

const _seq = (items: ReadonlyArray<string>, shape: keyof typeof _shapes = "list"): AnsiDoc.AnsiDoc =>
  _shapes[shape](Array.map(items, Doc.string))

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
- Owner: the one fold from document to terminal — `_MODES` is the render-policy vocabulary: each mode row CARRIES its fold over the document AND the measured width (`tty` renders escape codes pretty, `plain` strips annotations with `Doc.unAnnotate` then renders pretty, `wire` strips and renders `compact` for single-line machine form and takes no width because its output is one line), so `Print.text(doc, mode, width)` is one keyed lookup and a new mode is one row whose missing fold fails at the vocabulary declaration, never a conditional arm; `Print.Mode` is a `Context.Reference` row (`tty` default; `plain` for `--no-color` and non-TTY pipes; `wire` for machine emission); `Print.out(doc)` reads the ambient mode, measures the terminal, and writes through the platform `Terminal.display` — the only print site, so output is testable as data everywhere above it.
- Law: the seam renders at the terminal's OWN width — `terminal.columns` feeds `PageWidth.AvailablePerLine` through each mode row's `{ lineWidth, ribbonFraction: 1 }` options, so `Print.table`, `Print.prose`'s `Doc.reflow`, and `@effect/cli`'s `HelpDoc` all break against the real viewport instead of the printer's 80-column default; the page's own width-is-policy law then holds for the page itself, not only for its columns.
- Law: mode is ambient, never a parameter — verbs call `Print.out(doc)` with zero knowledge of the egress form, `--no-color` is one root-level `Effect.provideService(Print.Mode, "plain")`, and `Print.detected` is the Layer deriving the default from `Terminal.isTTY` so a pipe or a CI runner lands `plain` without any root provision at all; a per-call mode argument smuggles the knob back into every verb and is the rejected form.
- Law: live redraw is a directive row over the same seam — `Print.sweep(rows)` renders the published `AnsiDoc.eraseLines(rows)` document through `Print.text` before the next `out`, so a progress loop is erase-then-render with zero cursor arithmetic in verbs and zero raw escape strings; the directive annotates `Doc.empty`, so `plain` and `wire` strip it to nothing and piped output stays append-only without a mode branch at this seam.
- Law: deeply nested structures compose through `Print.deep` — `Optimize.optimize(doc, Optimize.Deep)` fuses associativity while preserving `AnsiDoc`; the ambient mode row still performs the only render, so optimization never opens a second terminal seam.
- Boundary: `@effect/cli`'s own `HelpDoc` lowers onto this same `AnsiDoc` rail, so parse-error help and verb output share one render seam; the `Terminal` binding is the runtime row's.
- Packages: `@effect/printer-ansi` (`AnsiDoc`); `@effect/printer` (`Doc`, `Optimize`); `@effect/platform` (`Terminal`); `effect` (`Context`, `Effect`, `Layer`).

```typescript
const _MODES = {
  tty: (doc: AnsiDoc.AnsiDoc, width: number): string =>
    AnsiDoc.render(doc, { style: "pretty", options: { lineWidth: width, ribbonFraction: 1 } }),
  plain: (doc: AnsiDoc.AnsiDoc, width: number): string =>
    Doc.render(Doc.unAnnotate(doc), { style: "pretty", options: { lineWidth: width, ribbonFraction: 1 } }),
  wire: (doc: AnsiDoc.AnsiDoc): string => Doc.render(Doc.unAnnotate(doc), { style: "compact" }),
} as const satisfies Record<string, (doc: AnsiDoc.AnsiDoc, width: number) => string>

class _Mode extends Context.Reference<_Mode>()("runtime/serve/Print/Mode", {
  defaultValue: (): keyof typeof _MODES => "tty",
}) {}

const _detected: Layer.Layer<_Mode, never, Terminal.Terminal> = Layer.effect(
  _Mode,
  Effect.map(
    Effect.flatMap(Terminal.Terminal, (terminal) => terminal.isTTY),
    (tty): keyof typeof _MODES => tty ? "tty" : "plain",
  ),
)

const _text = (doc: AnsiDoc.AnsiDoc, mode: keyof typeof _MODES, width: number): string => _MODES[mode](doc, width)

const _captureUtf8 = new TextEncoder()

const _captured = (text: string): Effect.Effect<Fanout.Announced, ValidationError.ValidationError> =>
  Schema.decodeUnknown(Event.format.json.single)(_captureUtf8.encode(text)).pipe(
    Effect.mapError((issue) => ValidationError.invalidValue(HelpDoc.p(issue.message))),
  )

const _position = (position: Fanout.Position): string =>
  position._tag === "Sequence" ? String(position.seq) : `${position.partition}@${position.offset}`

const _out = (doc: AnsiDoc.AnsiDoc): Effect.Effect<void, PlatformError.PlatformError, Terminal.Terminal> =>
  Effect.gen(function* () {
    const mode = yield* _Mode
    const terminal = yield* Terminal.Terminal
    const width = yield* terminal.columns
    yield* terminal.display(`${_text(doc, mode, width)}\n`)
  })

const _sweep = (rows: number): Effect.Effect<void, PlatformError.PlatformError, Terminal.Terminal> =>
  Effect.gen(function* () {
    const mode = yield* _Mode
    const terminal = yield* Terminal.Terminal
    const width = yield* terminal.columns
    yield* terminal.display(_text(AnsiDoc.eraseLines(rows), mode, width))
  })

const _deep = (doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc => Optimize.optimize(doc, Optimize.Deep)

const Print = {
  Mode: _Mode,
  detected: _detected,
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Ops, OpsFault, Print, Verb }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
