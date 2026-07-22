# [UI_FORM]

Form owns Schema-driven input, submission, and resumable upload. One kernel `Schema` projects through `standardSchemaV1`; live and server faults share field-path rows. RAC fields bind schema rows, Form's observed submit trip awaits the store, and large byte payloads ride one tus session. No parallel validator or field store exists. Module: `ui/src/view/form.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                 | [PUBLIC] |
| :-----: | :--------------- | :--------------------------------------------------------------------- | :------- |
|  [01]   | `SCHEMA_BINDING` | the Schema→aria validation seam and the one error-shape fold           | `Form`   |
|  [02]   | `FIELD_ROSTER`   | the field-family rows and their kernel-scalar commit seams             | —        |
|  [03]   | `SUBMIT_TRIP`    | the store-awaited action, pending state, reset, refusal reconciliation | `Form`   |
|  [04]   | `DRAFT_CURSORS`  | field-grain re-render over one draft atom                              | —        |
|  [05]   | `UPLOAD_LANE`    | the resumable tus session — resume proof, progress taps, typed refusal | `Form`   |

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
- Law: a field kind is one RAC field row bound to its schema field — `TextField`/`NumberField` for scalars, `SearchField` for query drafts, `Checkbox`/`Switch`/`RadioGroup` for toggles and choices, `Slider` for bounded magnitudes, `Select`/`ComboBox` for vocabularies (option matching through `system/intl`'s `useFilter`), `DateField`/`TimeField`/`DatePicker`/`DateRangePicker` for temporal input, `ColorField`/`ColorPicker` for color input; every row styles through `system/primitive` recipes and the `invalid:`/`required:`/`disabled:` variants.
- Law: foreign field interiors commit as kernel scalars — a date row's `DateValue` crosses to the domain as `DateTime.Utc` through `system/intl`'s epoch seam at the controlled-prop boundary; a color row's committed value decodes through `Theme.Color`; the draft atom never stores a widget-interior currency.
- Law: controlled rows bind the draft atom — `value`/`onChange` pairs read `useAtomValue` and write `useAtomSet` per `system/atom` law; uncommitted segment state (a half-typed date) stays widget-interior in react-stately and never mirrors.
- Law: gauges are output, fields are input — `Meter`/`ProgressBar` render atom-derived readings and take no schema field; a disabled field standing in for a reading is the named defect.
- Boundary: the roster composition pattern (`Xxx`/`XxxContext`/`XxxStateContext`, `Provider` values) is `system/primitive#ROSTER_LAW`'s; this page owns only the schema-field-to-row binding.

## [04]-[SUBMIT_TRIP]

[SUBMIT_TRIP]:
- Owner: the submit round-trip riding `Form` — `Form.submit` IS the nearest form's `action`: React brackets the async action in its own transition, so `useFormStatus` reflects the trip (the row's submit affordance disables and spins from it, never from a local flag); the action writes through `useAtomSet(mutation, { mode: "promiseExit" })`; a successful action resets through `requestFormReset`; refusal reconciles the optimistic write, and the fault set projects into `FormValidationContext` by field path through `Form.errors`' shape so a server refusal renders exactly like a live validation failure.
- Packages: `react-dom` (`useFormStatus`, `requestFormReset`); `effect` (`Exit`); `@effect-atom/atom-react` (write modality, `system/atom` law).
- Law: submit awaits the store — the mutation's `Result` is the completion evidence; polling an atom to detect completion marks a missing write mode, and a `try`/`catch` around the awaited promise restates the boundary rail.
- Law: pre-flight rides the hook rail — `Form.observed` consults the `rasm.ui.form.submit` point (`system/hook`, `veto` modality; the contributed `Points` and runtime rows are this page's) before the mutation write, a veto refusal fails the trip as `DraftRefused` and folds into the same error sink a validation failure feeds, and the settled outcome publishes on the same point tagged by the bounded stage vocabulary. `Form.hook` carries `consult: stage === "preflight"`, so arbiters cannot refuse settled facts and history and telemetry consume one rail.
- Law: the refusal fold reads the Cause tree through `Cause.failureOption` — the tagged `DraftRefused` arm projects its path-keyed errors, and a `Die`/`Interrupt`/composite cause preserves its evidence through `Cause.pretty` on the form-level row instead of collapsing to a blind sentinel; probing `cause._tag` by hand is the named defect.
- Law: a blocking submit failure lands in the form's error rows; a non-blocking outcome (a saved draft, a queued write) lands as a `Primitive.toasts` note — the two sinks never swap.
- Law: the trip is woven at the mutation effect — `Form.observed(write, registry, form)` is the composed trip the promiseExit write awaits: the veto consult leads, `Effect.withSpan("rasm.ui.form.submit")` carries the form id as span attribute and log annotation, and `Effect.onExit` both publishes the settled stage and feeds `1` through `Effect.withMetric` into `_SUBMITTED` tagged by the same bounded vocabulary (`resolved`/`refused`/`torn`) — so hook facts, metrics, and error rows cannot disagree.
- Boundary: the async action body is the React-19 form-action platform seam — React runs it inside its own transition (`useFormStatus`/`requestFormReset` are Promise-shaped); `Effect.promise` lifts the non-rejecting `Promise<Exit>` from `promiseExit`, `Exit.match` restores its Cause rail, and `Effect.runPromiseExit(Form.observed(...))` returns the one settled outcome the form folds; the write, hook registry, form id, form element, draft reader, and error sink arrive from the consuming row.

```typescript
import { Cause, Effect, Exit, Metric, Option, pipe } from "effect"
import { requestFormReset } from "react-dom"
import { Hook } from "../system/hook.ts"

declare module "../system/hook.ts" {
  interface Points {
    readonly "rasm.ui.form.submit": { readonly modality: "veto"; readonly payload: Submit.Fact } // this page's contributed row: the veto modality restated at this end of the seam
  }
}

declare namespace Submit {
  type Draft = Readonly<Record<string, unknown>>
  type Stage = "preflight" | "resolved" | "refused" | "torn"
  type Fact = { readonly form: string; readonly stage: Submit.Stage }
  type Refusal = { readonly _tag: "DraftRefused"; readonly errors: Form.Errors }
  type Write = (draft: Draft) => Promise<Exit.Exit<void, Submit.Refusal>>
}

const _SUBMITTED = Metric.counter("rasm.ui.form.submit", { description: "settled submit trips", incremental: true })

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
    Effect.filterOrFail(
      (admitted) => admitted,
      (): Submit.Refusal => ({ _tag: "DraftRefused", errors: { "": ["<vetoed>"] } }), // a veto refusal folds into the same sink a validation failure feeds
    ),
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
            Effect.asVoid(Effect.withMetric(Effect.succeed(1), Metric.tagged(_SUBMITTED, "outcome", stage))), // one stage value drives the hook fact and metric tag
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
    // nearest-form action seam: this function IS the <form action={...}> binding — React brackets
    // it in its own transition and useFormStatus reads the trip; the draft reads from the atom cursor
    // root at submit time, never from the platform FormData
    const outcome = await Effect.runPromiseExit(
      Form.observed(
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
- Law: draft persistence is one `Atom.kvs` row over the draft schema — an abandoned session restores the decoded draft or the default, never a raw JSON parse.
- Law: dirty-navigation guarding reads the draft — the route guard's dirty predicate is a derived atom comparing draft to committed (`Equal.equals`), consumed by the browser navigation plane through the atom bridge; a `beforeunload` listener beside it is the named defect.
- Boundary: a multi-step wizard whose stage graph answers requests and survives remounts is a `Machine` actor bound through `system/atom#LIVE_BRIDGE` — the draft cursors ride inside each stage, and the stage machine never mirrors field state.

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
- Packages: `tus-js-client` (`Upload`, `UploadOptions`, `PreviousUpload`, `OnSuccessPayload`, `DetailedError`, the event and policy hooks); `effect` (`Data`, `Effect`, `Option`).
- Law: progress is a tap parameter — `onProgress` folds into the app-composed sink (an atom write the `Meter`/`ProgressBar` gauges read), never component state; `onChunkComplete` rides the same sink where chunk grain matters.
- Law: refusal classes by status — `DetailedError.originalResponse.getStatus()` and the `onShouldRetry` hook own retry refusal; string-matching an error message is the named defect.
- Law: the server owns finalization — content-address folding and object-store writes land server-side on the data folder's tus lane; this session is a protocol driver, and the finished object's identity returns on the wire.
- Growth: a new transfer policy (chunk size, fingerprint store, signing hook) is one options row on the policy shape — never a second session mechanism.

```typescript
import { Data, Effect, Option } from "effect"
import { DetailedError, Upload, type OnSuccessPayload, type PreviousUpload } from "tus-js-client"

declare namespace Form {
  type UploadPolicy = {
    readonly endpoint: string
    readonly chunkSize: number
    readonly metadata: Readonly<Record<string, string>>
  }
  type Progress = { readonly sent: number; readonly total: number }
}

class UploadFault extends Data.TaggedError("UploadFault")<{
  readonly status: Option.Option<number>
  readonly detail: string
}> {}

const _upload = (
  file: File,
  policy: Form.UploadPolicy,
  progress: (step: Form.Progress) => void,
): Effect.Effect<OnSuccessPayload, UploadFault> =>
  Effect.async<OnSuccessPayload, UploadFault>((resume) => {
    // BOUNDARY ADAPTER: tus hooks are the platform's push seam — success and fault resume the effect, interruption aborts
    const session = new Upload(file, {
      endpoint: policy.endpoint,
      chunkSize: policy.chunkSize,
      metadata: { ...policy.metadata },
      onProgress: (sent, total) => progress({ sent, total }),
      onSuccess: (payload) => resume(Effect.succeed(payload)),
      onError: (fault) =>
        resume(Effect.fail(
          fault instanceof DetailedError
            ? new UploadFault({ status: Option.fromNullable(fault.originalResponse?.getStatus()), detail: fault.message })
            : new UploadFault({ status: Option.none(), detail: String(fault) }),
        )),
    })
    void session.findPreviousUploads().then((held: ReadonlyArray<PreviousUpload>) => {
      const prior = held[0]
      if (prior !== undefined) session.resumeFromPreviousUpload(prior)
      session.start()
    })
    return Effect.promise(() => session.abort())
  })

declare namespace Form {
  type Shape = {
    readonly standard: typeof _standard
    readonly errors: typeof _errors
    readonly observed: typeof _observed
    readonly hook: typeof _submitHook
    readonly submit: typeof _submit
    readonly upload: typeof _upload
  }
}

const Form: Form.Shape = {
  standard: _standard,
  errors: _errors,
  observed: _observed,
  hook: _submitHook,
  submit: _submit,
  upload: _upload,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Form, UploadFault }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
