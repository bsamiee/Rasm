# [UI_CONTENT]

Content owns the prose plane and the document editor as one derivation: `Content.Block` rows are the single authority a compiled class derives its ProseMirror `Schema`, effect `Schema` codec, DOM rule sets, and wire form from, so a document grammar is a table edit, never four hand-kept mirrors. `Content.mount` seats one imperative `EditorView` behind a container the React tree never reconciles into, every mutation rides `state.tr` through one dispatch fold into the atom bridge, and `prosemirror-collab` rebases against the authority behind the `Sequencer` port. Module: `ui/src/view/content.ts`.

Composition facts arrive settled: appearance keys resolve through `system/token#TONE_VOCABULARY` and emit through `Theme.css`; recipes ride `system/primitive#STYLED_SPINE`; every HTML-bearing string passes `Primitive.sanitize` before a DOM sink; untrusted document trees decode through core `Shape.Ingress.bounded`; the draft atom persists on `system/atom#STORE_ROOT`'s `hold` residue disposition, so a refused parcel holds beside the seeded default; faults derive through core `Fault.Class.family`; dirty navigation binds the route plane's `Guard.hold` at composition.

## [01]-[INDEX]

- [02]-[BLOCK_ROSTER]: `Content.roster` derives all four artifacts from the `Content.Block` row contract, with the generation join and the stored-doc quarantine; `Content`, `ContentFault`, `ContentCensus`.
- [03]-[PROSE_PLANE]: `Content.registers` bridges the typography plugin, with the `not-prose` boundary and the type-hierarchy recipe; `Content`.
- [04]-[EDITOR_HOST]: `Content.mount` scopes the `EditorView` acquisition, the plugin value roster, the paste gate, and the command roster; `Content`.
- [05]-[COLLAB_LANE]: `Content.collab` drives the `Sequencer` port — send/receive trip, generation pinning, draft persistence; `Content`, `Sequencer`.
- [06]-[COMMIT_SEAM]: `Content` crosses the committed document out — form-field codec, render projection, observe weave; `Content`.

## [02]-[BLOCK_ROSTER]

[BLOCK_ROSTER]:
- Owner: `Content.Block` — the one row contract for everything a document may hold, split into its two PM planes: a `Node` row carries `kind`, the `attrs` effect `Schema` struct (the shape authority — the PM `AttributeSpec` bag and the codec field both derive from it), the `content` expression, `group`/`inline`/`atom` flags, `parse` rules, a `render` arm (`toDOM` spec for inert kinds, a node-view constructor for live embeds), and the `steps` policy; a `Mark` row carries `kind`, `attrs`, `inclusive`/`excludes`, `parse`, `render`, and `steps`. `Content.roster(rows)` is the compile gate: it refuses duplicate kinds and content expressions naming absent kinds or groups, then derives `Content.Compiled` whole: the PM `Schema` (spec rows assembled over `addListNodes` for the list family), the document codec (`Schema.suspend` recursive union — the one sanctioned hand-stated encoded twin), `DOMParser.fromSchema`/`DOMSerializer.fromSchema`, and the roster `generation` — the canonical `kind@<attrs identity>` join in declared order, compared as declared data and never a schema hash.
- Cases: embeddable surfaces register by VALUE — `view/media`'s figure row, `view/canvas`'s graph row, and the chart panel row each export a `Content.Block`-shaped value from their own page, and the composition root hands `Content.roster` the assembled set at editor boot, exactly as `Overlay.commands` admits a command table; a `declare module` augmentation between view siblings is the lateral import the strata law forbids, so the open-interface form is deliberately not used here.
- Law: quarantine is a STORED-DOC decode posture alone — `Content.Compiled.stored` maps a node kind this roster lacks onto the interior quarantine atom (kind and raw payload held whole, re-admitted when the roster gains the kind) and folds an unknown mark into the same recorded residue, while `Content.Compiled.wire` refuses the whole payload as `decode-refused`; live collab never quarantines, because a quarantine atom occupies different position arithmetic than the node it stands for and silently corrupts every subsequent step's coordinates — the sequencer pins the generation instead (`[05]`).
- Law: the stored envelope carries the generation it was written under beside the document, and a decode under a different generation re-admits every node the CURRENT roster names — an unrecognized node holds whole as residue and an unrecognized attribute folds into that residue — so one roster edit re-admits every persisted draft and no chain of steps stands beside the table; that collapse loses rename carry, so a renamed attribute's stored value lands in residue rather than reaching its new name.
- Law: the `steps` policy gates authoring, never rendering — a `decode` row renders wherever a document carries it while the command and input-rule derivations skip it, so read-only embeds (a chart inside collaborative prose) ship as one column value and the sequencer never sees an insert step for them.
- Law: the seed rows are data like every later row — paragraph, heading, blockquote, code block, rule, the list family through `addListNodes`, hard break, and the mark rows emphasis, strong, link, code, and `thread` — the annotation mark whose `id` attribute is the durable text anchor `view/presence`'s thread plane reads, riding PM's own position mapping so a comment anchor survives every edit with no bespoke carry rule.
- Packages: `prosemirror-model` (`Schema`, `NodeSpec`/`MarkSpec`, `DOMParser`/`DOMSerializer`, `Node.fromJSON`/`toJSON`, `Node.check`); `prosemirror-schema-list` (`addListNodes` — list structure and the `itemContent` shape its verbs assume); `effect` (`Schema`, `Either`, `Option`, `Array`, `Record`); `@rasm/core` (`Shape.vocabulary`, `Shape.Json`, `Fault.Class`).
- Boundary: the quarantine atom renders as an inert chip through its own `toDOM` and counts onto the `[06]` metric — self-healing but never silent; which rows a deployment registers is the composition root's, and this page owns the contract, the gate, and the prose-core seed alone.
- Growth: a new block is one row (its four derivations arrive compiled); a new attribute is one field on the row's `attrs` schema, which moves the generation join by construction; a new embeddable surface is one exported row value at its owning page — never a schema edit here, a codec fork, or a parser rule written by hand.

```typescript signature
import { Fault, Shape } from "@rasm/core"
import { Array, Either, Option, Record, Schema } from "effect"
import { DOMParser, DOMSerializer, type MarkSpec, Node as PmNode, type NodeSpec, Schema as PmSchema } from "prosemirror-model"
import { addListNodes } from "prosemirror-schema-list"

const _planes = ["node", "mark"] as const
const _steps = ["authored", "decoded"] as const

// Four legs partition the plane and each reason renders its OWN subject, because the operator act differs per leg:
// a roster refusal names the plane and the row it condemns, a generation skew states both joins side by side, and
// an ingress, decode, or sequencer refusal names the payload's own cause. One free `detail` string spelled every
// reason alike, so a duplicate kind and an unreachable authority read the same on a board.
const _family = Fault.Class.family(
  ["roster-invalid", "generation-skew", "ingress-refused", "decode-refused", "sequencer-lost"] as const,
  {
    "roster-invalid": Fault.Class.row({
      class: "invalid",
      leg: "roster",
      detail: Schema.Struct({ plane: Schema.Literal(..._planes), kind: Schema.String, cause: Schema.String }),
      render: ({ cause, kind, plane }) => `${plane} row ${kind} refused: ${cause}`,
    }),
    "generation-skew": Fault.Class.row({
      class: "conflicted",
      leg: "generation",
      detail: Schema.Struct({ compiled: Schema.String, pinned: Schema.String }),
      render: ({ compiled, pinned }) => `sequencer pins generation ${pinned}, this roster compiles ${compiled}`,
    }),
    "ingress-refused": Fault.Class.row({
      class: "malformed",
      leg: "ingress",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `document exceeded the ingress ceiling: ${cause}`,
    }),
    "decode-refused": Fault.Class.row({
      class: "malformed",
      leg: "decode",
      detail: Schema.Struct({ kind: Schema.String }),
      render: ({ kind }) => `wire document names block ${kind}, which this roster declares no row for`,
    }),
    "sequencer-lost": Fault.Class.row({
      class: "unavailable",
      leg: "sequencer",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `sequencer port did not answer: ${cause}`,
    }),
  },
)

declare namespace ContentFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class ContentFault extends Schema.TaggedError<ContentFault>()("ContentFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

// `_roster` admits every row INDEPENDENTLY — a duplicate kind decides nothing about a sibling's content
// expression — so it censuses every offending row in one refusal and an author repairs the whole registration in
// one pass rather than one round trip per row. Every other reason is a single verdict and keeps the plain carrier.
const ContentCensus = _family.census("ContentCensus")
type ContentCensus = InstanceType<typeof ContentCensus>

declare namespace Content {
  type Plane = (typeof _planes)[number]
  type Steps = (typeof _steps)[number]
  type Render =
    | { readonly _tag: "Spec"; readonly toDOM: NonNullable<NodeSpec["toDOM"]> }
    | { readonly _tag: "Live"; readonly view: string } // names the nodeViews key the host binds; the constructor itself is host material
  type Block =
    | {
        readonly _tag: "Node"
        readonly kind: string
        readonly attrs: Schema.Schema.AnyNoContext // any struct schema a row supplies: the roster derives the AttributeSpec bag and the codec field from it, and a context-requiring schema cannot enter
        readonly content: Option.Option<string>
        readonly group: Option.Option<string>
        readonly inline: boolean
        readonly atom: boolean
        readonly parse: ReadonlyArray<NonNullable<NodeSpec["parseDOM"]>[number]>
        readonly render: Content.Render
        readonly steps: Content.Steps
      }
    | {
        readonly _tag: "Mark"
        readonly kind: string
        readonly attrs: Schema.Schema.AnyNoContext
        readonly inclusive: boolean
        readonly excludes: Option.Option<string>
        readonly parse: ReadonlyArray<NonNullable<MarkSpec["parseDOM"]>[number]>
        readonly render: Content.Render
        readonly steps: Content.Steps
      }
  type Doc = { readonly kind: string; readonly attrs: Record.ReadonlyRecord<string, unknown>; readonly marks: ReadonlyArray<{ readonly kind: string; readonly attrs: Record.ReadonlyRecord<string, unknown> }>; readonly text: Option.Option<string>; readonly children: ReadonlyArray<Content.Doc> }
  type Envelope = { readonly generation: string; readonly doc: Shape.Json } // the stored form: decode judges the generation, then re-admits each node against the live roster
  type Compiled = {
    readonly schema: PmSchema
    readonly codec: Schema.Schema<Content.Doc, Shape.Json>
    readonly parser: DOMParser
    readonly serializer: DOMSerializer
    readonly generation: string
    readonly rows: Record.ReadonlyRecord<string, Content.Block>
    readonly stored: (envelope: Content.Envelope) => Either.Either<PmNode, ContentFault> // quarantines unknown node kinds and folds unknown marks and attributes into residue
    readonly wire: (raw: unknown) => Either.Either<PmNode, ContentFault> // refuses whole on any unknown kind: the live plane never quarantines
  }
}

// this interior row is an inert atom leaf holding the foreign kind and its raw payload byte-preserved, so a
// roster that later gains the kind re-admits the payload whole instead of losing it
const _QUARANTINE = "quarantine"

// presence's thread plane reads this durable text-anchor mark; PM's own StepMap carries it across every edit
const _THREAD = "thread"

// `_attrsIdentity` reads the row's own declared field names in declared order, so adding, dropping, or renaming
// an attribute moves the join while an annotation or comment edit leaves it still
declare const _attrsIdentity: (row: Content.Block) => string

const _generation = (rows: ReadonlyArray<Content.Block>): string =>
  Array.map(rows, (row) => `${row.kind}@${_attrsIdentity(row)}`).join(",") // declared data in declared order — never a schema hash

declare const _roster: (
  rows: Array.NonEmptyReadonlyArray<Content.Block>,
) => Either.Either<Content.Compiled, ContentCensus>

declare const _core: Array.NonEmptyReadonlyArray<Content.Block> // paragraph, heading, blockquote, codeBlock, rule, hardBreak + addListNodes fold + emphasis/strong/link/code/thread marks
```

## [03]-[PROSE_PLANE]

[PROSE_PLANE]:
- Owner: the register bridge riding `Content` — `_REGISTERS` maps each `--tw-prose-*` color register onto the palette key it reads, and `Content.registers()` folds the table into `var(--color-…)` references emitted through `Theme.css` under the `tw-prose` namespace, so the prose plane re-inks on a theme flip with zero second emission and the plugin's shipped neutral ramps go unreferenced; `prose-invert` stays unused because the `data-theme` override plane already flips the palette variables the registers point at, and a second dark mechanism beside it forks the one theme stamp.
- Packages: `@tailwindcss/typography` (`@plugin` in the token stylesheet — build-time only, nothing imports it from TypeScript; the `prose` base class, size modifiers, the thirty-three element variants, `not-prose`); `system/token` (`Theme.css`, `Theme.Palette` keys, `cn`).
- Law: `not-prose` bounds the plane structurally — every embedded component, node-view chrome, rendered widget, and quarantine chip carries it, so the plane styles exactly the unclassed document HTML and a component's own recipe never fights an element rule; the plugin's `:where()` selectors carry zero specificity, so a utility on any child wins without ceremony and an `!important` against the plane is the named defect.
- Law: overrides are element variants, never descendant selectors — `prose-headings:`/`prose-a:`/`prose-code:` compose through `cn` with every other variant, and the `prose` size and color modifier groups register on the one merge instance at `system/token#CLASS_RAIL` so competing size classes resolve last-wins (the group rows land there — one table, no second instance).
- Law: the measure is layout's to release — `prose` sets its own reading measure and a container that owns width states `max-w-none`; the editor's content DOM mounts as a `prose` container, so authoring and reading render off one vocabulary.
- Law: the type hierarchy is one recipe with a `type` discriminant — `_text` maps `h1`–`h6`, `body`, `lead`, `caption`, and `code` onto `Theme.Scale` steps as a single `cva` recipe under `Primitive.styled`, so a heading outside the prose plane and one inside it read the same scale rows and a sibling per-level component never exists.
- Boundary: sanitization is upstream of this plane — the plugin styles whatever tags arrive and enforces nothing, so `[04]`'s paste gate and the sanitize gate decide which element variants can ever fire on foreign HTML.
- Growth: a new register assignment is one `_REGISTERS` row; a new prose size posture is a modifier the recipe names — never a hand-authored stylesheet for unclassed content.

```typescript signature
import { cva } from "class-variance-authority"
import { Record } from "effect"
import { cn, Theme } from "../system/token.ts"

// each register reads a palette variable the token authority already emits, so one emission serves every theme plane
// and the plugin's own ramps never load-bear; kbd-shadows carries an RGB triple by the plugin's contract, so it reads
// from the posture row's near-neutral surface rather than a toned slot
const _REGISTERS = {
  body: "neutral-text",
  headings: "neutral-on",
  lead: "neutral-text",
  links: "accent-text",
  bold: "neutral-on",
  counters: "neutral-border",
  bullets: "neutral-border",
  hr: "neutral-border",
  quotes: "neutral-text",
  "quote-borders": "accent-border",
  captions: "neutral-border",
  kbd: "neutral-on",
  "kbd-shadows": "well",
  code: "accent-text",
  "pre-code": "neutral-text",
  "pre-bg": "well",
  "th-borders": "neutral-border",
  "td-borders": "neutral-border",
} as const satisfies Record.ReadonlyRecord<string, string>

const _registers = (): string =>
  Theme.css(
    Record.map(_REGISTERS, (key) => `var(--color-${key})`),
    { head: "theme", namespace: "tw-prose" },
  )

const _text = cva("text-neutral-text", {
  variants: {
    type: {
      h1: "text-4xl font-semibold text-neutral-on",
      h2: "text-3xl font-semibold text-neutral-on",
      h3: "text-2xl font-medium text-neutral-on",
      h4: "text-xl font-medium text-neutral-on",
      h5: "text-lg font-medium text-neutral-on",
      h6: "text-base font-medium text-neutral-on",
      body: "text-base",
      lead: "text-lg",
      caption: "text-sm text-neutral-border",
      code: "font-mono text-sm text-accent-text",
    },
  },
  defaultVariants: { type: "body" },
})
```

## [04]-[EDITOR_HOST]

[EDITOR_HOST]:
- Owner: `Content.mount(container, options)` — the one scoped acquisition: `new EditorView({ mount: container }, { state, dispatchTransaction, nodeViews, transformPastedHTML, attributes })` acquires against the stable element the React tree renders and never reconciles into, `destroy()` releases in the same bracket, and `dispatchTransaction` is the single write path — it folds `state.apply(tr)` into the owning atom and calls `updateState` with the result, so the document lives in the registry with one writer and the view stays the imperative DOM owner. `Content.mount`'s hook carries the `"use no memo"` directive: the compiler must not memoize the imperative view holder, and the opt-out is the recorded trap, never an accident.
- Owner: `Content.plugins(compiled, policy)` — the plugin VALUE roster in precedence order: the document class's own `keymap` (list verbs chained ahead of the base ones — `chainCommands(splitListItem(itemType), splitBlock)` on Enter, `liftListItem`/`sinkListItem` on Shift-Tab/Tab, `chainCommands(undoInputRule, undo)` on Backspace), `keymap(baseKeymap)` at the floor, `history({ depth, newGroupDelay })` off the supplied policy row (never a module literal), `inputRules({ rules })` from `Content.rules`, `gapCursor()`, `dropCursor({ color: false, class })` styled on the token scale, and `collab` when `[05]` arms it.
- Owner: `Content.rules(compiled)` — the autoformat roster derived from the rows: `textblockTypeInputRule` retypes headings off the capture-group level, `wrappingInputRule` wraps blockquotes and both list families (the ordered form deriving `order` from its match), and `smartQuotes` spreads in beside `emDash` and `ellipsis`; a rule the roster's content expressions forbid declines by construction, and a keystroke branch in `handleTextInput` is the rejected shape.
- Owner: `Content.commands(compiled)` — the command roster keyed by verb: each value is the `Command` alias with both arms honoured, so a toolbar control's `isDisabled` reads `!command(view.state)` and its `onPress` runs `command(view.state, view.dispatch, view)` off the SAME value; marks toggle through `toggleMark(schema.marks[kind])`, blocks retype through `setBlockType`, structure lifts and wraps through the admissibility-checked verbs, and history exposes `undo`/`redo` beside `undoDepth`/`redoDepth` for enablement.
- Law: the paste boundary is three gates in one prop — `transformPastedHTML` runs `Primitive.sanitize` over the foreign clipboard HTML before the parser's own `parseDOM` rules map it onto the schema, and every stored or wire document decodes through `Shape.Ingress.bounded(codec)` so a hostile-depth tree refuses at the ceiling before any node constructs; a raw string reaching the parser, or a decode outside the bounded gate, is the named defect.
- Law: the view stylesheets are load-bearing — `prosemirror-view/style/prosemirror.css` carries the whitespace, hidden-selection, and node-selection rules editing correctness depends on, and `prosemirror-gapcursor/style/gapcursor.css` makes the gap cursor visible; both import once beside the token stylesheet, and a build omitting either renders collapsed whitespace or an invisible cursor.
- Law: live embeds are node-view portals — a `render: Live` row's constructor returns `dom` (and no `contentDOM` where a foreign renderer owns the children whole), declares `stopEvent`/`ignoreMutation` so widget interiors never read as edits, and mounts its own React root inside the returned element, torn down in `destroy`; React children rendered inside `view.dom` are the rejected shape.
- Law: presentation that must not persist is a decoration — plugin-field `DecorationSet`s mapped through `tr.mapping` carry selection echoes and collaboration carets, while anything that must survive serialization is a mark or attribute row; a floating toolbar anchors outside `view.dom` on a virtual element built from `coordsAtPos`, through the overlay plane's anchored host.
- Packages: `prosemirror-view` (`EditorView`, `Decoration`/`DecorationSet`, `NodeView`, the props system, the stylesheet subpath); `prosemirror-state` (`EditorState`, `Plugin`, `Command`); `prosemirror-keymap` (`keymap`); `prosemirror-commands` (the verb roster, `chainCommands`, `baseKeymap`); `prosemirror-history` (`history`, `undo`/`redo`, depth readers); `prosemirror-inputrules` (the rule builders, `smartQuotes`, `emDash`, `ellipsis`, `undoInputRule`); `prosemirror-gapcursor` + `prosemirror-dropcursor` (the two cursor plugins); `system/primitive` (`Primitive.sanitize`); `system/atom` (the dispatch fold's write law).
- Boundary: the editor's key plane is its own `contenteditable` matcher — the global palette and its `Overlay.Command` scopes stay outside `view.dom`, and a toolbar rendered beside the editor is RAC composition reading `Content.commands` values; which document classes an app mounts, and their node-view constructors, are composition material.
- Growth: a new editing verb is one command row composing the admitted verbs; a new autoformat is one rule row; a new plugin concern is one value in the roster — never a second view, a raw DOM listener, or a hand-rolled key ladder.

```typescript signature
import { Effect, type Scope } from "effect"
import { baseKeymap, chainCommands, splitBlock, toggleMark } from "prosemirror-commands"
import { dropCursor } from "prosemirror-dropcursor"
import { gapCursor } from "prosemirror-gapcursor"
import { history, redo, redoDepth, undo, undoDepth } from "prosemirror-history"
import { ellipsis, emDash, InputRule, inputRules, smartQuotes, textblockTypeInputRule, undoInputRule, wrappingInputRule } from "prosemirror-inputrules"
import { keymap } from "prosemirror-keymap"
import { liftListItem, sinkListItem, splitListItem } from "prosemirror-schema-list"
import { type Command, EditorState, type Plugin, type Transaction } from "prosemirror-state"
import { EditorView, type NodeViewConstructor } from "prosemirror-view"
import { Primitive } from "../system/primitive.ts"

declare namespace Content {
  type HistoryPolicy = { readonly depth: number; readonly newGroupDelay: number } // structural: prosemirror-history declares but never exports its options type
  type Host = {
    readonly compiled: Content.Compiled
    readonly doc: PmNode
    readonly history: Content.HistoryPolicy
    readonly cursor: string // the drop-cursor class on the token scale; color: false cedes appearance to it
    readonly views: Record.ReadonlyRecord<string, NodeViewConstructor> // one entry per Live render row; the gate proves the key set matches
    readonly fold: (state: EditorState) => void // the atom write: dispatchTransaction applies, folds, then updates the view
  }
}

const _plugins = (compiled: Content.Compiled, policy: Content.HistoryPolicy, cursor: string): ReadonlyArray<Plugin> => [
  keymap(_bindings(compiled)), // the document class's own verbs, list rows chained ahead
  keymap(baseKeymap),
  history(policy),
  inputRules({ rules: _rules(compiled) }),
  gapCursor(),
  dropCursor({ color: false, class: cursor }),
]

const _mount = (container: HTMLElement, host: Content.Host): Effect.Effect<EditorView, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.sync(() =>
      new EditorView({ mount: container }, {
        state: EditorState.create({ doc: host.doc, plugins: [..._plugins(host.compiled, host.history, host.cursor)] }),
        dispatchTransaction(tr: Transaction) {
          // BOUNDARY ADAPTER: the view hands intent here and renders what the fold returns — one writer, in order
          const next = this.state.apply(tr)
          host.fold(next)
          this.updateState(next)
        },
        nodeViews: { ...host.views },
        transformPastedHTML: (html) => Primitive.sanitize(html),
        attributes: { class: cn("prose max-w-none") }, // the content DOM is a prose container: one vocabulary for authoring and reading
      }),
    ),
    (view) => Effect.sync(() => view.destroy()),
  )

declare const _bindings: (compiled: Content.Compiled) => Record.ReadonlyRecord<string, Command>
declare const _rules: (compiled: Content.Compiled) => ReadonlyArray<InputRule>
declare const _commands: (compiled: Content.Compiled) => Record.ReadonlyRecord<string, Command>
```

## [05]-[COLLAB_LANE]

[COLLAB_LANE]:
- Owner: `Sequencer` — the folder-declared authority port: `frames(since)` pulls every accepted step frame after a version, `append(batch)` submits this client's unconfirmed steps and answers whether the authority's version admitted them, `live` streams arrivals as they land, and `generation` answers the roster generation the authority pinned at document birth — declared HERE and satisfied at the composition root exactly as `Egress` and `GlbViewport` are, so the server half (the linear version, the append-only step log, the rebase order) lives behind the port and this folder imports no transport.
- Owner: `Content.collab(view, compiled, sequencer, client)` — the closed loop: boot compares `compiled.generation` against the port's pinned generation and refuses `generation-skew` before any step flows (the standing law: wires pinning divergent generations refuse at the consumer — a quarantine posture here corrupts position arithmetic, so refusal is the only honest arm); the `collab({ version, clientID: client })` plugin mounts in the roster; after every dispatch `sendableSteps(state)` reports unconfirmed work and the loop ships `{ version, steps: steps.map(Step.toJSON), clientID }` through `append`; a stale offer pulls the missing frames and folds them — the client's own confirmations included — through `receiveTransaction(state, steps, clientIDs)` before re-offering the rebased batch; reconnection resumes from `getVersion(state)`.
- Law: `clientID` derives from the estate's identity owner, never the plugin's per-boot random — a durable actor identity is what lets the authority's log read as evidence, and a per-reload random id forks one author across sessions.
- Law: dirty navigation reads the loop's own state — unconfirmed steps (`sendableSteps !== null`) hold the route plane's `Guard.hold` token, and the token releases when the authority confirms; a `beforeunload` listener beside it restates the guard the runtime already owns.
- Law: carets are presence facts, never document steps — `Content.caret(state)` projects the selection's `{ anchor, head }` integer pair for the presence plane's caret axis (`core/state/presence`'s landed `_AXES` row), remote carets render as a plugin-field `DecorationSet` mapped through `tr.mapping`, and a cursor smuggled in as a document step is the rejected shape the collab catalogue names.
- Law: the anchor-space row is this page's export — `Content.anchors(surface, view, transactions)` answers the structural space value the presence registry admits at the composition root: `resolve` reads `coordsAtPos` off the live view, `carry` maps a position pair through each transaction's `mapping` (raw positions go stale on every edit, so the arm is load-bearing, never optional), and `epoch` is the dispatch fold's own transaction stream; a stale-position anchor rendered without this arm is the drift the column exists to close.
- Law: the draft persists on a STABLE key under the `hold` residue disposition — one `Atom.kvs` row holds the `Content.Envelope` (generation beside document JSON) sealed by `Store.sealed`, and a generation miss hands the raw stored document back as residue beside the seeded default, so the precious draft stays recoverable by hand; the persisted-grain law is `system/atom#STORE_ROOT`'s, and this page owns the envelope shape and the re-admission walk alone.
- Packages: `prosemirror-collab` (`collab`, `sendableSteps`, `receiveTransaction`, `getVersion` — never the undeclared `rebaseSteps`); `prosemirror-transform` (`Step.toJSON`/`Step.fromJSON` — the wire round trip); `effect` (`Context`, `Effect`, `Stream`, `Schema`); `@rasm/core` (`Fault.Class`).
- Boundary: the authority's storage substrate (the step log, snapshots, its recovery) is server material behind the port; history composes with no coordination — remote steps rebase its branches and undo never reverts another client's work; presence transport, rosters, and leases are `view/presence`'s plane.
- Growth: a new authority engine is one Layer satisfying the Tag; a new frame policy (batch width, pull cadence) is a field on the port's own row — never a second loop, a peer-to-peer arm, or a CRDT bridge beside the ordered lane.

```typescript signature
import { Fault } from "@rasm/core"
import { Context, type Effect, Schema, type Stream } from "effect"
import { collab, getVersion, receiveTransaction, sendableSteps } from "prosemirror-collab"
import { Step } from "prosemirror-transform"

declare namespace Sequencer {
  type Frame = {
    readonly version: number
    readonly steps: ReadonlyArray<Shape.Json>
    readonly clients: ReadonlyArray<string> // index-aligned with steps: the client's own id marks a confirmation
  }
  type Batch = { readonly version: number; readonly steps: ReadonlyArray<Shape.Json>; readonly client: string }
}

class Sequencer extends Context.Tag("ui/Sequencer")<Sequencer, {
  readonly generation: Effect.Effect<string, ContentFault>
  readonly frames: (since: number) => Effect.Effect<Sequencer.Frame, ContentFault>
  readonly append: (batch: Sequencer.Batch) => Effect.Effect<boolean, ContentFault> // false: the version went stale — pull, rebase, re-offer
  readonly live: Stream.Stream<Sequencer.Frame, ContentFault>
}>() {}

// this stored draft envelope seats generation beside document, so a decode judges meaning before it admits a node
const _Envelope = Schema.Struct({
  generation: Schema.NonEmptyString,
  doc: Shape.Json,
})

const _caret = (state: EditorState): { readonly anchor: number; readonly head: number } => ({
  anchor: state.selection.anchor,
  head: state.selection.head,
})

// carets and durable pins alike ride this position-pair locator the anchor plane holds for this surface
const _Pos = Schema.Struct({
  anchor: Schema.Int.pipe(Schema.nonNegative()),
  head: Schema.Int.pipe(Schema.nonNegative()),
})

// this exported anchor-space row spells structurally field-for-field against view/presence#ANCHOR_PLANE's
// Anchor.Space<L, C>: resolve reads the live view's coordsAtPos, carry maps positions through each transaction's
// own StepMap (a deleted range answers None and the anchor parks), and epoch is the dispatch fold's transaction
// stream — the composition root hands this value to the registry, and no view sibling imports another
const _anchors = (
  surface: string,
  view: EditorView,
  transactions: Stream.Stream<Transaction>,
): {
  readonly kind: "content"
  readonly surface: string
  readonly locator: typeof _Pos
  readonly resolve: (locator: typeof _Pos.Type) => Option.Option<{ readonly x: number; readonly y: number; readonly width: number; readonly height: number }>
  readonly carry: Option.Option<(locator: typeof _Pos.Type, change: Transaction) => Option.Option<typeof _Pos.Type>>
  readonly epoch: Stream.Stream<Transaction>
} => ({
  kind: "content",
  surface,
  locator: _Pos,
  resolve: (locator) => {
    if (locator.head > view.state.doc.content.size) return Option.none() // past the live extent: parked, never a guess
    const at = view.coordsAtPos(locator.head)
    return Option.some({ x: at.left, y: at.top, width: 0, height: at.bottom - at.top })
  },
  carry: Option.some((locator, change) => {
    const anchor = change.mapping.mapResult(locator.anchor)
    const head = change.mapping.mapResult(locator.head)
    return anchor.deleted || head.deleted ? Option.none() : Option.some({ anchor: anchor.pos, head: head.pos })
  }),
  epoch: transactions,
})

declare const _collab: (
  view: EditorView,
  compiled: Content.Compiled,
  sequencer: Context.Tag.Service<Sequencer>,
  client: string,
) => Effect.Effect<void, ContentFault, Scope.Scope>
```

## [06]-[COMMIT_SEAM]

[COMMIT_SEAM]:
- Owner: the crossing members riding `Content` — `Content.field(compiled)` is the form-field commit codec: the compiled document codec IS the `Schema` the form plane's `Form.standard` validates and the wire decodes, so a document field row binds `value`/`onChange` at the controlled boundary with the committed document crossing as the codec's kernel value and the editor's interior state never mirroring into the draft atom (the same two-owner grammar the token field settled — live state widget-interior, committed value one codec); `Content.rendered(compiled, doc, document)` projects a document through the derived `DOMSerializer` into a `DocumentFragment` against an EXPLICIT document, so server rendering and export serialization share one projection; `Content.committed(write, registry, editor)` is the observed commit trip.
- Law: the commit trip is woven at the effect — the veto-free observe point `rasm.ui.content.commit` publishes each settled outcome tagged by the bounded stage vocabulary (`committed`/`quarantined`), `Effect.withSpan("rasm.ui.content.commit")` carries the editor id as span attribute and log annotation, and quarantine counts feed `_QUARANTINED` tagged by the foreign kind — roster drift across deployments reads on a board before a user reports a grey chip; one stage value drives the hook fact and the metric tag, so the two cannot disagree.
- Law: export is the landed matrix's row, not a member here — the `Document` source case, its admitted formats, and the serializer cell land at `view/export`'s matrix (`json` from the codec's encoded side, `text` from `textContent`, `svg`/`png` per embedded kind through each row's own owner), and this page supplies only the projections those cells compose.
- Packages: `@rasm/core` (`Convention` — the metric mount; the `contentQuarantine` row's convention counterpart lands at `core/observe/convention` per the instrument-admission coupling); `effect` (`Effect`, `Metric`, `Option`); `react-dom` (nothing — the form action seam is `view/form`'s).
- Boundary: which flows commit, and where committed documents persist, are app material; the quarantine chip's tone keys `Theme.Tone` through the status plane's vocabulary; the thread mark's anchor semantics are `view/presence`'s to read.
- Growth: a new commit outcome is one stage value on the bounded vocabulary with its arm in the trip; a new projection is one member reading the compiled artifacts — never a parallel serializer or a second commit path.

```typescript signature
import { Convention } from "@rasm/core"
import { Effect, Metric } from "effect"
import { Hook } from "../system/hook.ts"

declare module "../system/hook.ts" {
  interface Points {
    readonly "rasm.ui.content.commit": { readonly modality: "observe"; readonly payload: Commit.Fact }
  }
}

declare namespace Commit {
  type Stage = "committed" | "quarantined"
  type Fact = { readonly editor: string; readonly stage: Commit.Stage; readonly kind: Option.Option<string> }
}

const _commitHook: Hook.Row<"rasm.ui.content.commit"> = {
  modality: "observe",
  depth: 16,
  source: Option.none(),
}

const _QUARANTINED = Convention.mount(Convention.metric.contentQuarantine)

// on the trip's quarantine leg one bounded kind word rides the metric tag while the full foreign kind stays on the
// hook fact — _committed composes this per quarantined node
const _quarantined = (kind: string): Effect.Effect<void> =>
  Effect.asVoid(Effect.withMetric(Effect.succeed(1), Metric.tagged(_QUARANTINED, Convention.rasm.contentKind, kind)))

const _rendered = (compiled: Content.Compiled, doc: PmNode, document: Document): DocumentFragment =>
  compiled.serializer.serializeFragment(doc.content, { document }) // explicit document: the projection renders identically off-browser

declare const _field: (compiled: Content.Compiled) => Schema.Schema<Content.Doc, Shape.Json>
declare const _committed: <A, E, R>(
  write: Effect.Effect<A, E, R>,
  registry: Hook.Registry,
  editor: string,
) => Effect.Effect<A, E, R>

declare namespace Content {
  type Shape = {
    readonly quarantine: typeof _QUARANTINE
    readonly thread: typeof _THREAD
    readonly core: typeof _core
    readonly roster: typeof _roster
    readonly generation: typeof _generation
    readonly registers: typeof _registers
    readonly text: typeof _text
    readonly plugins: typeof _plugins
    readonly mount: typeof _mount
    readonly bindings: typeof _bindings
    readonly rules: typeof _rules
    readonly commands: typeof _commands
    readonly collab: typeof _collab
    readonly caret: typeof _caret
    readonly anchors: typeof _anchors
    readonly Envelope: typeof _Envelope
    readonly field: typeof _field
    readonly rendered: typeof _rendered
    readonly committed: typeof _committed
    readonly hook: typeof _commitHook
  }
}

const Content: Content.Shape = {
  quarantine: _QUARANTINE,
  thread: _THREAD,
  core: _core,
  roster: _roster,
  generation: _generation,
  registers: _registers,
  text: _text,
  plugins: _plugins,
  mount: _mount,
  bindings: _bindings,
  rules: _rules,
  commands: _commands,
  collab: _collab,
  caret: _caret,
  anchors: _anchors,
  Envelope: _Envelope,
  field: _field,
  rendered: _rendered,
  committed: _committed,
  hook: _commitHook,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Content, ContentCensus, ContentFault, Sequencer }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
