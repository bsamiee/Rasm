# [UI_FORM]

The Schema-driven form plane: one kernel `Schema` owns wire decode AND live field validity — projected through `Schema.standardSchemaV1` (the package surface used directly; a forwarding wrapper is the named defect) into the standard-schema validator RAC fields consume — decode failures land in `FormValidationContext` keyed by field path so server faults and live validation share one error shape, and the submit round-trip awaits the store. The field roster is the full RAC family set bound as rows: text/number, date/time over `@internationalized/date`, color over RAC color state, gauges and toggles — each row one schema field bound to one RAC field, with the tw-rac `invalid:`/`required:` variants styling validity with zero JS branching. No form library, no per-field `useState`, no parallel validator. The module is `ui/src/view/form.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              | [PUBLIC] |
| :-----: | :--------------- | :--------------------------------------------------------------------- | :------- |
|  [01]   | `SCHEMA_BINDING` | the Schema→aria validation seam and the one error-shape fold            | `Form`   |
|  [02]   | `FIELD_ROSTER`   | the field-family rows and their kernel-scalar commit seams              | —        |
|  [03]   | `SUBMIT_TRIP`    | the store-awaited action, pending state, reset, refusal reconciliation  | `Form`   |
|  [04]   | `DRAFT_CURSORS`  | field-grain re-render over one draft atom                               | —        |

## [2]-[SCHEMA_BINDING]

[SCHEMA_BINDING]:
- Owner: `Form` — the Schema→aria binding: `Form.standard(schema)` projects the owning kernel field Schema through `Schema.standardSchemaV1` into the validator RAC fields consume; `Form.errors(schema)` folds a full-payload decode (`errors: "all"`, `ArrayFormatter` at the terminal reporting edge only) into the path-keyed error record `FormValidationContext` injects; `validationBehavior: "aria"` marks invalid via ARIA without blocking native submit, and `FieldError` renders the `ValidationResult`.
- Packages: `effect` (`Schema.standardSchemaV1`, `Schema.decodeUnknownEither`, `ParseResult.ArrayFormatter`, `Array`, `Either`, `Record`); `react-aria-components` (`Form`, `FieldError`, `FormValidationContext`).
- Law: one Schema, both duties — the same owner that decodes the wire payload validates the live field; a parallel validator, a regex beside a brand, or a hand `errors` record is the named defect.
- Law: the error shape is one fold — live per-field validation and server-refusal projection both land as `Readonly<Record<path, ReadonlyArray<string>>>` keyed by the `ParseError` tree's dotted path, so a refusal from the wire and a local decode render through the same `FieldError` rows.
- Growth: a new form is a schema plus rows; a new constraint is a Schema refinement on the owning field — never a validation prop ladder.

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

## [3]-[FIELD_ROSTER]

[FIELD_ROSTER]:
- Law: a field kind is one RAC field row bound to its schema field — `TextField`/`NumberField` for scalars, `SearchField` for query drafts, `Checkbox`/`Switch`/`RadioGroup` for toggles and choices, `Slider` for bounded magnitudes, `Select`/`ComboBox` for vocabularies (option matching through `system/intl`'s `useFilter`), `DateField`/`TimeField`/`DatePicker`/`DateRangePicker` for temporal input, `ColorField`/`ColorPicker` for color input; every row styles through `system/primitive` recipes and the `invalid:`/`required:`/`disabled:` variants.
- Law: foreign field interiors commit as kernel scalars — a date row's `DateValue` crosses to the domain as `DateTime.Utc` through `system/intl`'s epoch seam at the controlled-prop boundary; a color row's committed value decodes through `Theme.Color`; the draft atom never stores a widget-interior currency.
- Law: controlled rows bind the draft atom — `value`/`onChange` pairs read `useAtomValue` and write `useAtomSet` per `system/atom` law; uncommitted segment state (a half-typed date) stays widget-interior in react-stately and never mirrors.
- Law: gauges are output, fields are input — `Meter`/`ProgressBar` render atom-derived readings and take no schema field; a disabled field standing in for a reading is the named defect.
- Boundary: the roster composition pattern (`Xxx`/`XxxContext`/`XxxStateContext`, `Provider` values) is `system/primitive#ROSTER_LAW`'s; this page owns only the schema-field-to-row binding.

## [4]-[SUBMIT_TRIP]

[SUBMIT_TRIP]:
- Owner: the submit round-trip riding `Form` — the action writes through `useAtomSet(mutation, { mode: "promise" })` inside `startTransition`; pending state reads `useFormStatus` (the row's submit affordance disables and spins from it, never from a local flag); a successful action resets through `requestFormReset`; refusal reconciles the optimistic write, and the fault set projects into `FormValidationContext` by field path through `Form.errors`' shape so a server refusal renders exactly like a live validation failure.
- Packages: `react-dom` (`useFormStatus`, `requestFormReset`); `react` (`startTransition`); `effect` (`Exit`); `@effect-atom/atom-react` (write modality, `system/atom` law).
- Law: submit awaits the store — the mutation's `Result` is the completion evidence; polling an atom to detect completion marks a missing write mode, and a `try`/`catch` around the awaited promise restates the boundary rail.
- Law: a blocking submit failure lands in the form's error rows; a non-blocking outcome (a saved draft, a queued write) lands as a `Primitive.toasts` note — the two sinks never swap.
- Boundary: the fence below is the app-side action shape this page legislates — the `promiseExit` write and the form element arrive from the consuming row; the refusal arm folds the fault into `Form.Errors` through the same path-keyed shape `[2]` owns.

```typescript
import { Exit } from "effect"
import { startTransition } from "react"
import { requestFormReset } from "react-dom"

declare namespace Submit {
  type Draft = Readonly<Record<string, unknown>>
  type Write = (draft: Draft) => Promise<Exit.Exit<void, { readonly _tag: "DraftRefused"; readonly errors: Form.Errors }>>
}

const _submit = (write: Submit.Write, form: HTMLFormElement, sink: (errors: Form.Errors) => void) =>
  (draft: Submit.Draft): void =>
    startTransition(async () => {
      const outcome = await write(draft)
      Exit.match(outcome, {
        onSuccess: () => {
          requestFormReset(form)
          sink({})
        },
        onFailure: (cause) =>
          sink(
            cause._tag === "Fail" && cause.error._tag === "DraftRefused"
              ? cause.error.errors
              : { "": ["<submit-failed>"] },
          ),
      })
    })
```

## [5]-[DRAFT_CURSORS]

[DRAFT_CURSORS]:
- Law: a large form draft is one atom, each field a cursor — `useAtomRefProp(ref, key)` scopes each field's subscription to its own property so an edited field re-renders alone; a per-field atom family for one draft, or a whole-draft subscription per field, restates the cursor law.
- Law: draft persistence is one `Atom.kvs` row over the draft schema — an abandoned session restores the decoded draft or the default, never a raw JSON parse.
- Law: dirty-navigation guarding reads the draft — the route guard's dirty predicate is a derived atom comparing draft to committed (`Equal.equals`), consumed by the browser navigation plane through the atom bridge; a `beforeunload` listener beside it is the named defect.

```typescript
const Form: {
  readonly standard: typeof _standard
  readonly errors: typeof _errors
} = {
  standard: _standard,
  errors: _errors,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Form }
```
