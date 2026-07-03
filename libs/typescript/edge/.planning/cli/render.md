# [EDGE_RENDER]

`cli/render.ts` is the terminal output surface: every verb's output is a composed `Doc<Ansi>` value — structure through the printer's layout algebra, semantic markup through one role table — folded to a string at exactly one render seam whose mode row decides styled, plain, or machine form. Style is data at two altitudes: `_roles` maps semantic intent (`fault`, `warn`, `ok`, `path`, `emph`, `faint`) to composed `Ansi` directives so no verb ever names a color, and `Render.Mode` is an ambient row the app root sets once (`--no-color`, piped CI, machine emission) so the same document serves interactive, piped, and wire egress by mode selection — `Doc.unAnnotate` strips styling for the plain branch, the compact algorithm single-lines the wire branch, and raw escape codes exist nowhere in application code. The structural rows (`kv`, `table`, `verdicts`, `banner`, `prose`) are the closed vocabulary the ops family and every app verb compose; a hand-joined string with column arithmetic is the named defect this page deletes.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        | [PUBLIC] |
| :-----: | :--------------- | :---------------------------------------------------------------- | :------- |
|  [01]   | [ROLE_TABLE]     | the semantic-role directive rows and the role annotator            | `Render` |
|  [02]   | [STRUCTURE_ROWS] | kv, table, verdicts, banner, prose composition rows                | `Render` |
|  [03]   | [RENDER_SEAM]    | the mode row, the fold to string, the terminal display effect      | `Render` |

## [2]-[ROLE_TABLE]

[ROLE_TABLE]:
- Owner: `_roles` — six composed `Ansi` values under one `as const satisfies Record<string, Ansi.Ansi>` anchor: `fault` (`bold` ⊕ `red`), `warn` (`yellow`), `ok` (`green`), `path` (`cyan`), `emph` (`bold`), `faint` (`blackBright`) — each a monoid composition attached by ONE `Doc.annotate` through `Render.role(kind, doc)`, never one annotate per style bit.
- Law: verbs speak roles, never colors — a verb marks a value `path` or a verdict `fault` and the table decides what that means on a terminal; a theme change is a row edit with zero verb diffs, and `Doc.reAnnotate` over the role table is the theming seam if a second palette ever earns existence.
- Growth: a new semantic intent is one row; a verb needing a color that is not an intent is the smell — name the intent.
- Packages: `@effect/printer-ansi` (`Ansi`); `@effect/printer` (`Doc`).

```typescript
import { type PlatformError, Terminal } from "@effect/platform"
import { Doc } from "@effect/printer"
import { Ansi, AnsiDoc } from "@effect/printer-ansi"
import { Array, Context, Effect } from "effect"

const _roles = {
  fault: Ansi.combine(Ansi.bold, Ansi.red),
  warn: Ansi.yellow,
  ok: Ansi.green,
  path: Ansi.cyan,
  emph: Ansi.bold,
  faint: Ansi.blackBright,
} as const satisfies Record<string, Ansi.Ansi>

const _role = (kind: keyof typeof _roles, doc: AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc => Doc.annotate(doc, _roles[kind])
```

## [3]-[STRUCTURE_ROWS]

[STRUCTURE_ROWS]:
- Owner: the composition rows, each a fold over the printer's own algebra — `kv(pairs)` aligns a label column by `Doc.fill` to the widest label and stacks with `Doc.vsep` (labels `faint`, values plain); `table(head, rows)` fills every column to its measured width, marks the head `emph`, and stacks — layout by combinator, zero column arithmetic in consumers; `verdicts({ passed, failed })` renders the doctor shape — `ok`/`fault` glyph rows over `kv` bodies — one row family for every check-shaped output; `banner(title)` is the `emph` section head; `prose(text)` wraps through `Doc.reflow` at the page width the render seam owns; `raw(text)` admits pre-formed text (an emitted artifact) as a newline-splitting `Doc.string`.
- Law: rows return `AnsiDoc` values, never strings — composition stays open (a verb nests a `table` under a `banner` with `Doc.vsep`) and the fold to text happens once at the seam; a row that returned a string would re-close the algebra per call site.
- Law: width is measured, not guessed — column widths fold from the rows' own content (`Array.reduce` over rendered cell lengths), so a wide value grows its column and truncation is never silent.
- Growth: a new output shape is one row composing the existing algebra; a shape needing a new layout primitive reaches for the printer's own (`align`, `hang`, `encloseSep`) before any local invention.
- Packages: `@effect/printer` (`Doc`); `effect` (`Array`).

```typescript
const _width = (labels: ReadonlyArray<string>): number => Array.reduce(labels, 0, (held, label) => Math.max(held, label.length))

const _kv = (pairs: ReadonlyArray<readonly [string, string]>): AnsiDoc.AnsiDoc => {
  const span = _width(Array.map(pairs, ([label]) => label))
  return Doc.vsep(Array.map(pairs, ([label, value]) =>
    Doc.hsep([Doc.fill(_role("faint", Doc.text(label)), span), Doc.text(value)])))
}

const _table = (head: ReadonlyArray<string>, rows: ReadonlyArray<ReadonlyArray<string>>): AnsiDoc.AnsiDoc => {
  const spans = Array.map(head, (label, column) =>
    _width([label, ...Array.map(rows, (row) => row[column] ?? "")]))
  const lined = (cells: ReadonlyArray<string>, mark: (doc: AnsiDoc.AnsiDoc) => AnsiDoc.AnsiDoc): AnsiDoc.AnsiDoc =>
    Doc.hsep(Array.map(cells, (cell, column) => Doc.fill(mark(Doc.text(cell)), spans[column] ?? cell.length)))
  return Doc.vsep([lined(head, (doc) => _role("emph", doc)), ...Array.map(rows, (row) => lined(row, (doc) => doc))])
}

const _verdicts = (report: {
  readonly passed: ReadonlyArray<readonly [string, string]>
  readonly failed: ReadonlyArray<readonly [string, string]>
}): AnsiDoc.AnsiDoc =>
  Doc.vsep([
    ...Array.map(report.passed, ([name, detail]) => Doc.hsep([_role("ok", Doc.text("pass")), Doc.text(name), _role("faint", Doc.text(detail))])),
    ...Array.map(report.failed, ([name, detail]) => Doc.hsep([_role("fault", Doc.text("fail")), Doc.text(name), Doc.text(detail)])),
  ])

const _banner = (title: string): AnsiDoc.AnsiDoc => _role("emph", Doc.text(title))

const _prose = (text: string): AnsiDoc.AnsiDoc => Doc.reflow(text)

const _raw = (text: string): AnsiDoc.AnsiDoc => Doc.string(text)
```

## [4]-[RENDER_SEAM]

[RENDER_SEAM]:
- Owner: the one fold from document to terminal — `Render.Mode` is a `Context.Reference` row (`tty` default; `plain` for `--no-color` and non-TTY pipes; `wire` for machine emission) the app root or a global flag overrides once; `Render.text(doc, mode)` is the pure fold — `tty` renders escape codes through `AnsiDoc.render({ style: "pretty" })`, `plain` strips annotations with `Doc.unAnnotate` then renders pretty, `wire` strips and renders `compact` for single-line machine form; `Render.out(doc)` reads the ambient mode and writes through the platform `Terminal.display` — the only print site, so output is testable as data everywhere above it.
- Law: mode is ambient, never a parameter — verbs call `Render.out(doc)` with zero knowledge of the egress form, the `--no-color` flag is one root-level `Effect.provideService(Render.Mode, "plain")`, and CI inherits `plain` by the same provision; a per-call mode argument would smuggle the knob back into every verb.
- Law: the pretty width is a policy row — 80-column ribbon as the fixed default (`{ style: "pretty" }` carries the printer's own default options), stated here so a width change is one edit.
- Boundary: `@effect/cli`'s own `HelpDoc` lowers onto this same `AnsiDoc` rail (`toAnsiDoc`), so parse-error help and verb output share one render seam; the `Terminal` binding is the runtime row's; interactive cursor/erase composition (progress redraw) composes `AnsiDoc`'s directive documents when a verb earns it.
- Packages: `@effect/printer-ansi` (`AnsiDoc`); `@effect/printer` (`Doc`); `@effect/platform` (`Terminal`); `effect` (`Context`, `Effect`).

```typescript
const _MODES = ["tty", "plain", "wire"] as const

class _Mode extends Context.Reference<_Mode>()("edge/cli/Render/Mode", {
  defaultValue: (): (typeof _MODES)[number] => "tty",
}) {}

const _text = (doc: AnsiDoc.AnsiDoc, mode: (typeof _MODES)[number]): string =>
  mode === "tty"
    ? AnsiDoc.render(doc, { style: "pretty" })
    : mode === "plain"
      ? Doc.render(Doc.unAnnotate(doc), { style: "pretty" })
      : Doc.render(Doc.unAnnotate(doc), { style: "compact" })

const _out = (doc: AnsiDoc.AnsiDoc): Effect.Effect<void, PlatformError.PlatformError, Terminal.Terminal> =>
  Effect.gen(function* () {
    const mode = yield* _Mode
    const terminal = yield* Terminal.Terminal
    yield* terminal.display(`${_text(doc, mode)}\n`)
  })

const Render: {
  readonly Mode: typeof _Mode
  readonly modes: typeof _MODES
  readonly roles: typeof _roles
  readonly role: typeof _role
  readonly kv: typeof _kv
  readonly table: typeof _table
  readonly verdicts: typeof _verdicts
  readonly banner: typeof _banner
  readonly prose: typeof _prose
  readonly raw: typeof _raw
  readonly text: typeof _text
  readonly out: typeof _out
} = {
  Mode: _Mode,
  modes: _MODES,
  roles: _roles,
  role: _role,
  kv: _kv,
  table: _table,
  verdicts: _verdicts,
  banner: _banner,
  prose: _prose,
  raw: _raw,
  text: _text,
  out: _out,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Render }
```
