# [UI_FORM]

Form owns Schema-driven input, submission, resumable upload, the multi-step wizard, and the auth ceremony faces. One kernel `Schema` projects through `standardSchemaV1`; live and server faults share field-path rows. RAC fields bind schema rows, Form's observed submit trip awaits the store, and large byte payloads ride one tus session. `Wizard` binds the stage graph as one core `Transition` spec through the atom bridge, and `Ceremony` renders the runtime session plane's phases while security owns every ceremony's logic. Module: `ui/src/view/form.ts`.

## [01]-[INDEX]

- [02]-[SCHEMA_BINDING]: Form projects one kernel Schema into aria validation and folds one error shape; `Form`.
- [03]-[FIELD_ROSTER]: Form binds field-family rows to kernel-scalar commit seams under the two-owner token grammar; `Form`.
- [04]-[SUBMIT_TRIP]: Form awaits the store on submit — pending state, reset, refusal reconciliation; `Form`.
- [05]-[DRAFT_CURSORS]: field-grain re-render over one draft atom; —.
- [06]-[UPLOAD_LANE]: Form drives one resumable tus session — resume proof, progress taps, typed refusal; `Form`.
- [07]-[WIZARD]: `Wizard` derives the multi-step stage graph as one core `Transition` spec, with its cursor folds and stepper face; `Wizard`.
- [08]-[CEREMONY]: `Ceremony` renders the auth faces — session phases, departure and landing invocation, passkey legs; `Ceremony`.

## [02]-[SCHEMA_BINDING]

[SCHEMA_BINDING]:
- Owner: `Form` — the Schema→aria binding: `Form.standard(schema)` projects the owning kernel field Schema through `Schema.standardSchemaV1` into the validator RAC fields consume; `Form.errors(schema)` folds a full-payload decode (`errors: "all"`, `ArrayFormatter` at the terminal reporting edge only) into the path-keyed error record `FormValidationContext` injects; `validationBehavior: "aria"` marks invalid via ARIA without blocking native submit, and `FieldError` renders the `ValidationResult`.
- Packages: `effect` (`Schema.standardSchemaV1`, `Schema.decodeUnknownEither`, `ParseResult.ArrayFormatter`, `Array`, `Either`, `Record`); `react-aria-components` (`Form`, `FieldError`, `FormValidationContext`).
- Law: one Schema, both duties — the same owner that decodes the wire payload validates the live field; a parallel validator, a regex beside a brand, or a hand `errors` record is the named defect.
- Law: the error shape is one fold — live per-field validation and server-refusal projection both land as `Readonly<Record<path, ReadonlyArray<string>>>` keyed by the `ParseError` tree's dotted path, so a refusal from the wire and a local decode render through the same `FieldError` rows.
- Growth: a new form is a schema and its rows; a new constraint is a Schema refinement on the owning field — never a validation prop ladder.

```typescript
import { Array, Either, ParseResult, Record, Schema } from "effect"

declare namespace Form {
  type Errors = Readonly<Record<string, ReadonlyArray<string>>>
}

const _standard = <A, I>(schema: Schema.Schema<A, I>) => Schema.standardSchemaV1(schema)

const _errors = <A, I>(schema: Schema.Schema<A, I>) =>
  (raw: I): Form.Errors =>
    Either.match(Schema.decodeUnknownEither(schema, { errors: "all" })(raw), {
      onRight: () => Record.empty<string, ReadonlyArray<string>>(),
      onLeft: (fault) =>
        Record.map(
          Array.groupBy(ParseResult.ArrayFormatter.formatErrorSync(fault), (issue) => issue.path.join(".")),
          Array.map((issue) => issue.message),
        ),
    })
```

## [03]-[FIELD_ROSTER]

[FIELD_ROSTER]:
- Law: Form binds each field kind to one RAC field row over its schema field — `TextField`/`NumberField` for scalars, `SearchField` for query drafts, `Checkbox`/`Switch`/`RadioGroup` for toggles and choices, `Slider` for bounded magnitudes, `Select`/`ComboBox` for vocabularies (option matching through `system/intl`'s `useFilter`), `DateField`/`TimeField`/`DatePicker`/`DateRangePicker` for temporal input, `Calendar`/`RangeCalendar` standing alone where the selection IS the surface, `ColorField`/`ColorPicker` for color input, `TagGroup`/`TagList`/`Tag` for a committed multi-value vocabulary, `TokenField`/`TokenInput`/`Token` for an expression the user writes as prose and reads back as chips, `FileTrigger`/`DropZone` for byte intake, and the document field for prose the content plane compiles; every row styles through `system/primitive` recipes and the `invalid:`/`required:`/`disabled:` variants. Every RAC input member stacks above or declines with its reason on this card, so the roster is CLOSED against the admitted input surface and a missing row is a census defect, never an open question.
- Law: a tag field commits a branded array — `TagGroup` renders and removes, the committed value decodes through the owning field Schema (a `Schema.Array` of the vocabulary brand), and reordering rides `useDragAndDrop` per `system/primitive#ROSTER_LAW`; the chip recipe is the token field's, so an expression chip and a vocabulary chip read one visual grammar.
- Law: a standalone calendar is still a COMMIT seam — `Calendar`/`RangeCalendar` outside a picker popover carry the same controlled-prop boundary, the same `DateValue` → `DateTime.Utc` epoch crossing, and the same draft-atom write as their fielded siblings; standing alone changes the chrome, never the seam.
- Law: bytes never enter the draft — `FileTrigger` opens the platform picker and `DropZone` admits drops through the `isFileDropItem` refinement (text and directory items decline at the seam; `isTextDropItem` stays the collection-reorder refinement `system/primitive` names); the admitted `File` hands straight to `[06]`'s upload lane and the DRAFT field holds only the returned receipt identity, so an abandoned session restores a reference, never a stranded blob.
- Law: the document field is the two-owner grammar at document grain — live editing is the mounted `EditorView`'s own state (`view/content#EDITOR_HOST`), and the committed value crosses ONCE through `Content.field(compiled)` — the same compiled codec that decodes the wire — at the controlled boundary, mirroring the `TokenField` split exactly: interior currency stays widget-interior, one codec owns the crossing, and a parallel document parser at either end is the named defect.
- Law: a token field has TWO grammar owners and they never trade jobs — the subclassed value owns LIVE segmentation and the `Schema` codec owns the COMMITTED decode. `TokenFieldValue` is an immutable persistent sequence whose every mutator answers a new instance of its own type, carrying its caret `Position`, its range replacement and boundary search, and its undo history with the coalescing window, so a grammar is a `tokenize` override on the value — presentation-state grain, the same grain a motion value holds, outside the atom entirely — and the state hook stays the thin `{ value, setValue, isComposing, setComposing }` cell it ships as. Only what the field COMMITS crosses to the domain, decoded by the one Schema that decodes the wire payload, so a saved filter round-trips through a single codec and a parallel parser at either end is the named defect.
- Law: the subclass seam is two overrides or none — `tokenize` states the grammar and `createFieldValue` keeps the type, so an override of the first without the second widens every edit back to the base value and the grammar is lost on the first keystroke.
- Law: foreign field interiors commit as kernel scalars — a date row's `DateValue` crosses to the domain as `DateTime.Utc` through `system/intl`'s epoch seam at the controlled-prop boundary; a color row's committed value decodes through `Theme.Color`; the draft atom never stores a widget-interior currency.
- Law: controlled rows bind the draft atom — `value`/`onChange` pairs read `useAtomValue` and write `useAtomSet` per `system/atom` law; uncommitted segment state (a half-typed date) stays widget-interior in react-stately and never mirrors.
- Law: gauges are output, fields are input — `Meter`/`ProgressBar` render atom-derived readings and take no schema field; a disabled field standing in for a reading is the named defect.
- Boundary: the roster composition pattern (`Xxx`/`XxxContext`/`XxxStateContext`, `Provider` values) is `system/primitive#ROSTER_LAW`'s; this page owns only the schema-field-to-row binding.

```typescript
import { Schema } from "effect"
import { TokenFieldValue, type TokenFieldSegment } from "react-stately"

declare class Tokenized<T> extends TokenFieldValue<T> {
  protected tokenize(text: string): Array<TokenFieldSegment<T>>
  protected createFieldValue(segments: ReadonlyArray<TokenFieldSegment<T>>): this
}

declare namespace Form {
  type TokenCodec<A, T> = Schema.Schema<A, ReadonlyArray<TokenFieldSegment<T>>>
}
```

## [04]-[SUBMIT_TRIP]

[SUBMIT_TRIP]:
- Owner: the submit round-trip riding `Form` — `Form.submit` IS the nearest form's `action`: React brackets the async action in its own transition, so `useFormStatus` reflects the trip (the row's submit affordance disables and spins from it, never from a local flag); the action writes through `useAtomSet(mutation, { mode: "promiseExit" })`; a successful action resets through `requestFormReset`; refusal reconciles the optimistic write, and the fault set projects into `FormValidationContext` by field path through `Form.errors`' shape so a server refusal renders exactly like a live validation failure.
- Packages: `react-dom` (`useFormStatus`, `requestFormReset`); `effect` (`Exit`); `@rasm/core` (`Tap.Verdict`); `@effect-atom/atom-react` (write modality, `system/atom` law).
- Law: submit awaits the store — the mutation's `Result` is the completion evidence; polling an atom to detect completion marks a missing write mode, and a `try`/`catch` around the awaited promise restates the boundary rail.
- Law: pre-flight rides the hook rail — `Form.observed` consults the `rasm.ui.form.submit` point (`system/hook`, `veto` modality; the contributed `Points` and runtime rows are this page's) before the mutation write, a `vetoed` verdict fails the trip as `DraftRefused` carrying the arbiter's own reason and folds into the same error sink a validation failure feeds, and the settled outcome publishes on the same point tagged by the bounded stage vocabulary. `Form.hook` carries `consult: stage === "preflight"`, so arbiters cannot refuse settled facts and history and telemetry consume one rail.
- Law: the refusal fold reads the Cause tree through `Cause.failureOption` — the tagged `DraftRefused` arm projects its path-keyed errors, and a `Die`/`Interrupt`/composite cause preserves its evidence through `Cause.pretty` on the form-level row instead of collapsing to a blind sentinel; probing `cause._tag` by hand is the named defect.
- Law: a blocking submit failure lands in the form's error rows; a non-blocking outcome (a saved draft, a queued write) lands as a `Primitive.toasts` note — the two sinks never swap.
- Law: the trip is woven at the mutation effect — `Form.observed(write, registry, form)` is the composed trip the promiseExit write awaits: the veto consult leads, `Effect.withSpan("rasm.ui.form.submit")` carries the form id as span attribute and log annotation, and `Effect.onExit` both publishes the settled stage and feeds `1` through `Effect.withMetric` into `_SUBMITTED` tagged by the same bounded vocabulary (`resolved`/`refused`/`torn`) — so hook facts, metrics, and error rows cannot disagree.
- Boundary: the async action body is the React-19 form-action platform seam — React runs it inside its own transition (`useFormStatus`/`requestFormReset` are Promise-shaped); `Effect.promise` lifts the non-rejecting `Promise<Exit>` from `promiseExit`, `Exit.match` restores its Cause rail, and `Effect.runPromiseExit(Form.observed(...))` returns the one settled outcome the form folds; the write, hook registry, form id, form element, draft reader, and error sink arrive from the consuming row.

```typescript
import { Convention, Tap } from "@rasm/core"
import { Cause, Effect, Exit, Metric, Option, pipe } from "effect"
import { requestFormReset } from "react-dom"
import { Hook } from "../system/hook.ts"

declare module "../system/hook.ts" {
  interface Points {
    readonly "rasm.ui.form.submit": { readonly modality: "veto"; readonly payload: Submit.Fact }
  }
}

declare namespace Submit {
  type Draft = Readonly<Record<string, unknown>>
  type Stage = "preflight" | "resolved" | "refused" | "torn"
  type Fact = { readonly form: string; readonly stage: Submit.Stage }
  type Refusal = { readonly _tag: "DraftRefused"; readonly errors: Form.Errors }
  type Write = (draft: Draft) => Promise<Exit.Exit<void, Submit.Refusal>>
}

const _SUBMITTED = Convention.mount(Convention.metric.formSubmit)

const _submitHook: Hook.Row<"rasm.ui.form.submit"> = {
  modality: "veto",
  depth: 16,
  source: Option.none(),
  consult: (fact) => fact.stage === "preflight",
}

const _observed = Effect.fn("Form.observed")(function* <A, E, R>(
  write: Effect.Effect<A, E, R>,
  registry: Hook.Registry,
  form: string,
) {
  return yield* Hook.publish(registry, "rasm.ui.form.submit", { form, stage: "preflight" }).pipe(
    Effect.flatMap((verdict) =>
      Tap.Verdict.$match(verdict, {
        fanned: () => Effect.void,
        unrostered: () => Effect.void,
        vetoed: ({ veto }): Effect.Effect<void, Submit.Refusal> =>
          Effect.fail({ _tag: "DraftRefused", errors: { "": [veto.reason] } }),
      })),
    Effect.zipRight(write),
    Effect.onExit((exit) =>
      pipe(
        Exit.match(exit, {
          onFailure: (cause) => (Option.isSome(Cause.failureOption(cause)) ? "refused" : "torn") as const,
          onSuccess: () => "resolved" as const,
        }),
        (stage) =>
          Effect.zipRight(
            Effect.asVoid(Hook.publish(registry, "rasm.ui.form.submit", { form, stage })),
            Effect.asVoid(Effect.withMetric(Effect.succeed(1), Metric.tagged(_SUBMITTED, Convention.rasm.formOutcome, stage))),
          ),
      )),
    Effect.annotateLogs({ form }),
    Effect.withSpan("rasm.ui.form.submit", { attributes: { "form.id": form } }),
  )
})

const _submit = (
  write: Submit.Write,
  registry: Hook.Registry,
  id: string,
  form: HTMLFormElement,
  draft: () => Submit.Draft,
  sink: (errors: Form.Errors) => void,
) =>
  async (_formData: FormData): Promise<void> => {
    const outcome = await Effect.runPromiseExit(
      _observed(
        Effect.flatMap(Effect.promise(() => write(draft())), (exit) =>
          Exit.match(exit, { onFailure: Effect.failCause, onSuccess: Effect.succeed })),
        registry,
        id,
      ),
    )
    Exit.match(outcome, {
      onSuccess: () => {
        requestFormReset(form)
        sink({})
      },
      onFailure: (cause) =>
        sink(
          Option.match(Cause.failureOption(cause), {
            onSome: (refusal) => refusal.errors,
            onNone: () => ({ "": [Cause.pretty(cause)] }),
          }),
        ),
    })
  }
```

## [05]-[DRAFT_CURSORS]

[DRAFT_CURSORS]:
- Law: a large form draft is one `AtomRef` root, each field a cursor — `AtomRef.make(seed)` mints the draft, `useAtomRefProp(ref, key)` derives the per-field child so an edited field re-renders alone, and `useAtomRefPropValue(ref, key)` is the read-only projection for summary rows; a per-field atom family for one draft, or a whole-draft subscription per field, restates the cursor law (`system/atom#SELECTOR_RAIL` owns the cursor primitive).
- Law: a search-driving draft field defers — the field's committed value feeds heavy consumers (a filtered collection) through `useDeferredValue(value)` so typing stays responsive while the derived view lags one beat; `Atom.debounce` shapes the store-side rate, `useDeferredValue` shapes the render-side lag, and the two compose without a hand-rolled timer.
- Law: draft persistence is one `Atom.kvs` row on a STABLE grain sealed by `Store.sealed` — the key derives through `system/atom#STORE_ROOT`'s one mint member and holds for the draft's whole life, and a draft is precious, so the seal takes the `hold` residue disposition: a parcel written under another generation refuses on content and hands its raw stored value back as residue beside the seeded default. Abandoned sessions restore the decoded draft, its residue, or the default, and a hand-spelled key literal or a raw JSON parse beside the mint is the named defect.
- Law: dirty-navigation guarding reads the draft — the route guard's dirty predicate is a derived atom comparing draft to committed (`Equal.equals`), consumed by the browser navigation plane through the atom bridge; a `beforeunload` listener beside it is the named defect.
- Boundary: the multi-step stage graph is `[07]`'s `Transition` spec bound through `system/atom#LIVE_BRIDGE` — the draft cursors ride inside each stage, and the stage machine never mirrors field state.

```typescript
import { AtomRef, useAtomRefProp, useAtomRefPropValue } from "@effect-atom/atom-react"
import { useDeferredValue } from "react"

declare const _seed: { readonly title: string; readonly quantity: number; readonly note: string }

const _draft = AtomRef.make(_seed)

const _useField = <K extends keyof typeof _seed>(key: K): AtomRef.AtomRef<(typeof _seed)[K]> =>
  useAtomRefProp(_draft, key)

const _useQuery = (): string => useDeferredValue(useAtomRefPropValue(_draft, "title"))
```

## [06]-[UPLOAD_LANE]

[UPLOAD_LANE]:
- Owner: `Form.upload(file, policy, progress)` — one resumable session per source: the session constructs over the endpoint policy row, proves prior progress through `findPreviousUploads` and binds the first candidate through `resumeFromPreviousUpload` before `start()`, and completion resolves with the `OnSuccessPayload` receipt; interruption runs `abort()` — the stored URL survives, so the next session resumes at the proven offset — and an explicit cancel escalates `Upload.terminate(url)`.
- Law: progress is a tap parameter — `onProgress` folds into the app-composed sink (an atom write the `Meter`/`ProgressBar` gauges read), never component state; `onChunkComplete` rides the same sink where chunk grain matters.
- Law: the received status stays inbound evidence beside the reason — the reason carries the routing and the status carries what the endpoint said, so a report names `409` rather than the band it fell into; the status never becomes the discriminant a handler switches on.
- Law: the server owns finalization — content-address folding and object-store writes land server-side on the data folder's tus lane; this session is a protocol driver, and the finished object's identity returns on the wire.
- Growth: a new transfer policy (chunk size, fingerprint store, signing hook) is one options row on the policy shape — never a second session mechanism.

```typescript
import { Fault } from "@rasm/core"
import { Effect, Option, Schema } from "effect"
import { DetailedError, Upload, type OnSuccessPayload, type PreviousUpload } from "tus-js-client"

declare namespace Form {
  type UploadPolicy = {
    readonly endpoint: string
    readonly chunkSize: number
    readonly metadata: Readonly<Record<string, string>>
  }
  type Progress = { readonly sent: number; readonly total: number }
}

const _Status = Schema.optionalWith(Schema.Int, { as: "Option" })
const _coded = (status: Option.Option<number>): string =>
  Option.getOrElse(Option.map(status, (code) => `${code}`), () => "no response")

const _family = Fault.Class.family(["endpoint-denied", "payload-rejected", "transfer-lost"] as const, {
  "endpoint-denied": Fault.Class.row({
    class: "denied",
    leg: "endpoint",
    detail: Schema.Struct({ status: _Status, cause: Schema.String }),
    render: ({ cause, status }) => `upload endpoint refused the session (${_coded(status)}): ${cause}`,
  }),
  "payload-rejected": Fault.Class.row({
    class: "invalid",
    leg: "payload",
    detail: Schema.Struct({ status: _Status, cause: Schema.String }),
    render: ({ cause, status }) => `upload endpoint rejected the payload (${_coded(status)}): ${cause}`,
  }),
  "transfer-lost": Fault.Class.row({
    class: "unavailable",
    leg: "transfer",
    detail: Schema.Struct({ status: _Status, cause: Schema.String }),
    render: ({ cause, status }) => `upload transfer did not complete (${_coded(status)}): ${cause}`,
  }),
})

const _refusal = (status: Option.Option<number>): UploadFault.Reason =>
  Option.match(status, {
    onNone: () => "transfer-lost" as const,
    onSome: (code) => (code === 401 || code === 403 ? "endpoint-denied" : code >= 400 && code < 500 ? "payload-rejected" : "transfer-lost"),
  })

declare namespace UploadFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class UploadFault extends Schema.TaggedError<UploadFault>()("UploadFault", {
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

const _upload = (
  file: File,
  policy: Form.UploadPolicy,
  progress: (step: Form.Progress) => void,
): Effect.Effect<OnSuccessPayload, UploadFault> =>
  Effect.async<OnSuccessPayload, UploadFault>((resume) => {
    const session = new Upload(file, {
      endpoint: policy.endpoint,
      chunkSize: policy.chunkSize,
      metadata: { ...policy.metadata },
      onProgress: (sent, total) => progress({ sent, total }),
      onSuccess: (payload) => resume(Effect.succeed(payload)),
      onShouldRetry: (fault) => _refusal(Option.fromNullable(fault.originalResponse?.getStatus())) === "transfer-lost",
      onError: (fault) => {
        const status = fault instanceof DetailedError ? Option.fromNullable(fault.originalResponse?.getStatus()) : Option.none<number>()
        return resume(Effect.fail(new UploadFault({ case: { reason: _refusal(status), status, cause: fault.message } })))
      },
    })
    void session.findPreviousUploads()
      .then((held: ReadonlyArray<PreviousUpload>) => {
        const prior = held[0]
        if (prior !== undefined) session.resumeFromPreviousUpload(prior)
      })
      .catch(() => undefined)
      .finally(() => session.start())
    return Effect.promise(() => session.abort())
  })

declare namespace Form {
  type Shape = {
    readonly standard: typeof _standard
    readonly errors: typeof _errors
    readonly observed: typeof _observed
    readonly hook: typeof _submitHook
    readonly refusal: typeof _refusal
    readonly submit: typeof _submit
    readonly upload: typeof _upload
  }
}

const Form: Form.Shape = {
  standard: _standard,
  errors: _errors,
  observed: _observed,
  hook: _submitHook,
  refusal: _refusal,
  submit: _submit,
  upload: _upload,
}
```

## [07]-[WIZARD]

[WIZARD]:
- Owner: `Wizard` — the multi-step stage graph as one core `Transition` spec the builder derives from data: `Wizard.spec(options)` maps each stage onto an atomic node (with one final node), and generates the row set from the policy — a guarded advance row per stage (`when` reads the consumer's `valid` predicate over the extended state), a `skip` row guarded by `skippable`, an unguarded `back` row, and — only under `linear: false` — one seek row per OTHER stage (a self-seek row exits and re-enters the standing stage, re-running its entry program over live draft state), the seek vocabulary deriving from the stage roster itself so an unknown TARGET is unspellable at the type plane. Linearity is therefore enforced IN the machine: a linear wizard generates no seek row, so a spelled jump lands `Unrouted` on the macrostep's own refusal column, and a disabled-button veneer over an unguarded machine is the named defect.
- Cases: the cursor is a FOLD over the actor's config, never a second cell — `Wizard.cursor(stages, config)` reads the active atomic node's rank, and `Wizard.standing(cursor, rank)` derives the tri-state (`completed` below, `current` at, `incomplete` above), so step chrome renders off one derivation and no per-step completion flag exists to drift.
- Law: refusal is a rendered verdict, never a silent no-op — a refused press arrives on the actor's fact stream as `Transition`'s own `Guarded` refusal (`Macro.refused`, naming the advance row whose guard closed it), and the consuming face folds it into the SAME error sink `[02]`'s validation failures feed; the next-trigger stays enabled, because a clickable refusal that explains beats a disabled button that cannot, and a same-signal fallback row minted to manufacture that verdict re-derives the guard the owner already folds.
- Law: the actor binds through the one bridge and survives remounts — `Atom.subscribable(actor.state)` is the whole machine→view seam (`system/atom#LIVE_BRIDGE`), `actor.freeze` snapshots the configuration and `compiled.restore(frozen)` resumes it, so a reloaded session reopens on the stage it left; stage draft state is `[05]`'s cursors riding inside each stage, and the machine's extended state carries only what guards read.
- Law: the face is recipe rows over the derivation — the step list renders `standing` as `data-standing` the recipe's variants read, the action grid is three fixed columns (`prev | skip | next-or-submit`) so button placement never shifts as arms appear, `skip` is a first-class sibling rendered only where `skippable` holds, and a hidden stage keeps its mount through the `<Activity mode>` row `system/act#DOCUMENT_RAIL` owns.
- Packages: `@rasm/core` (`Transition` — `spec`/`Node`/`Row`/`Config`/`Actor`, the `when` guard column, the `Macro.refused` column); `@effect-atom/atom-react` (`Atom.subscribable`); `effect` (`Array`, `Option`, `Schema`); `class-variance-authority` (the step recipe).
- Boundary: stage CONTENT is ordinary form rows — schema binding, cursors, and the submit trip compose unchanged inside each stage; the final stage's submit is `[04]`'s trip, so the wizard adds navigation and never a second commit path; watchdog deadlines and invoked activities are `Transition`'s own `watches`/`invokes` rows when a wizard earns them.
- Growth: a new stage is one roster entry (its rows arrive generated); a new navigation arm is one row kind in the builder; a new face posture is one recipe variant; a stage graph past the builder's family — parallel regions, history re-entry — authors its `Transition` spec directly, since the builder instantiates the linear family and never ceilings the machine — never a bespoke stepper state, a completion set, or a second machine binding.

```typescript
import { Transition } from "@rasm/core"
import { cva } from "class-variance-authority"
import { Array, Option, Schema } from "effect"

declare namespace Wizard {
  type Node<Stage extends string> = Stage | "done"
  type Signal<Stage extends string> = "next" | "back" | "skip" | `seek.${Stage}`
  type Standing = "completed" | "current" | "incomplete"
  type Options<Stage extends string, X> = {
    readonly name: string
    readonly stages: Array.NonEmptyReadonlyArray<Stage>
    readonly extended: Schema.Schema<X>
    readonly seed: X
    readonly valid: (stage: Stage) => (extended: X) => boolean
    readonly skippable: (stage: Stage) => (extended: X) => boolean
    readonly linear: boolean
  }
}

const _rows = <const Stage extends string, X>(
  options: Wizard.Options<Stage, X>,
): ReadonlyArray<Transition.Row<Wizard.Node<Stage>, Wizard.Signal<Stage>, never, X>> =>
  Array.flatMap(options.stages, (stage, rank) => {
    const target = Option.getOrElse<Wizard.Node<Stage>>(Array.get(options.stages, rank + 1), () => "done")
    return [
      { source: stage, on: "next" as const, when: options.valid(stage), to: [target] as const },
      { source: stage, on: "skip" as const, when: options.skippable(stage), to: [target] as const },
      ...Option.match(Array.get(options.stages, rank - 1), {
        onNone: () => [],
        onSome: (prior) => [{ source: stage, on: "back" as const, to: [prior] as const }],
      }),
      ...(options.linear
        ? []
        : Array.filterMap(options.stages, (seek) =>
          seek === stage
            ? Option.none()
            : Option.some({ source: stage, on: `seek.${seek}` as const, to: [seek] as const }))),
    ]
  })

const _cursor = <Stage extends string>(
  stages: ReadonlyArray<Stage>,
  config: Transition.Config<Wizard.Node<Stage>, unknown>,
): number =>
  Array.findFirstIndex(stages, (stage) => Array.contains(config.active, stage)).pipe(Option.getOrElse(() => stages.length))

const _standing = (cursor: number, rank: number): Wizard.Standing =>
  rank < cursor ? "completed" : rank === cursor ? "current" : "incomplete"

declare const _spec: <const Stage extends string, X>(
  options: Wizard.Options<Stage, X>,
) => ReturnType<typeof Transition.spec<Wizard.Node<Stage>, Wizard.Signal<Stage>, never, X>>

const _step = cva("flex items-center gap-2 text-sm", {
  variants: {
    standing: {
      completed: "text-neutral-text",
      current: "font-medium text-accent-text",
      incomplete: "text-neutral-border",
    },
  },
  defaultVariants: { standing: "incomplete" },
})

declare namespace Wizard {
  type Shape = {
    readonly spec: typeof _spec
    readonly rows: typeof _rows
    readonly cursor: typeof _cursor
    readonly standing: typeof _standing
    readonly step: typeof _step
  }
}

const Wizard: Wizard.Shape = {
  spec: _spec,
  rows: _rows,
  cursor: _cursor,
  standing: _standing,
  step: _step,
}
```

## [08]-[CEREMONY]

[CEREMONY]:
- Owner: `Ceremony` — the auth faces rendering the runtime session plane: `_PHASES` maps every `SessionStatus` tag onto its tone and motion posture (total by `satisfies`, so a new phase breaks here rather than rendering untinted), the sign-in and sign-out affordances invoke `Vault.depart(plan)` and the app's clear leg, the landing route's face folds `Vault.land(url, exchange)`'s outcome into `[02]`'s error shape, and the whole ceremony form is `[04]`'s submit trip over `[02]`'s schema binding — rows, never new machinery.
- Law: security owns every ceremony's LOGIC and this page owns its FACE — `security/authn` models issuers, sessions, and the passkey ceremony; the runtime route plane holds the session cell, continuity, and CSRF; these faces render `Vault.status` phases through the atom bridge and hold zero token bytes, zero cookie reads, and zero ceremony state of their own.
- Law: the passkey legs are two POSTs through the app's contract binding — the face requests the ceremony options and posts the credential response through `AtomHttpApi`/`AtomRpc` mutation rows (`system/atom#REMOTE_BINDING`), while `security/authn/webauthn`'s browser subpath owns the `navigator.credentials` invocation BETWEEN them; a fetch beside the binding or a credentials call in this folder is the named defect.
- Law: a phase is rendered evidence, never a guess — `Authenticating` renders its hold posture (the boot probe or a refresh is in flight and the cell settles it), `Expired` renders the re-entry affordance with the danger tone, and no face branches on a token's presence because the browser never holds one.
- Law: the phase vocabulary TRANSCRIBES — ui imports core alone, so the four `SessionStatus` tags spell here field-for-field as a deliberate non-import mirror (the cache `Budget` and vital grade precedent), the subscribed session cell arrives through the atom bridge as data, and `Vault.depart`/`Vault.land` are invoked through the app-composed binding, never an imported member; a tag landing on either end sweeps the other in the same change.
- Packages: `system/token` (`Theme.Tone`); `system/act` (`Motion.holds`); `effect` (`Option`); the runtime session plane arrives composed — no runtime import exists in this folder.
- Boundary: which identity providers exist, the exchange leg `land` receives, and the post-sign-out destination are app composition; the login/signup FIELD content is ordinary `[03]` rows; OAuth's full-page departure means this face renders a leaving state and never sequences after it.
- Growth: a new phase presentation is one `_PHASES` row; a new ceremony kind (a second factor's prompt, a recovery flow) is a form composed of landed rows — never a ceremony engine, a token cell, or a session mirror in this folder.

```typescript
import { Option } from "effect"
import type { Motion } from "../system/act.ts"
import type { Theme } from "../system/token.ts"

const _phases = ["Anonymous", "Authenticating", "Authenticated", "Expired"] as const

declare namespace Ceremony {
  type Phase = (typeof _phases)[number]
  type Posture = { readonly tone: Theme.Tone; readonly hold: Option.Option<Motion.Hold> }
}

const _PHASES = {
  Anonymous: { tone: "neutral", hold: Option.none() },
  Authenticating: { tone: "accent", hold: Option.some("spin" as const) },
  Authenticated: { tone: "success", hold: Option.none() },
  Expired: { tone: "danger", hold: Option.none() },
} as const satisfies Record<Ceremony.Phase, Ceremony.Posture>

declare namespace Ceremony {
  type Shape = {
    readonly phases: typeof _phases
    readonly postures: typeof _PHASES
  }
}

const Ceremony: Ceremony.Shape = {
  phases: _phases,
  postures: _PHASES,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Ceremony, Form, UploadFault, Wizard }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
