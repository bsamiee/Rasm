# [UI_FORM]

The headless form-validity owner distinct in kind from the field-control behavior. `FormBinding` folds one `effect` `Schema` over the raw `FormData` through `Schema.decodeUnknownEither`, projects the `ParseError` into a per-path field-error map, and feeds the `react-aria-components` `Form` `validationBehavior: "aria"` surface so a form is one schema and the accessibility error region is the decoded validity. The page composes the `role-behavior.md` `inputs` role family for the field controls and holds no domain state; a valid decode crosses the `interchange` `CommandGateway` as one intent, never a transport directly.

## [1]-[INDEX]

- [1]-[FORM_BINDING]: the `Schema.decodeUnknownEither` form-validity fold over one `effect` `Schema`, and the per-field ARIA error projection onto the `react-aria-components` `Form` surface.

## [2]-[FORM_BINDING]

- Owner: `FormBinding<A>`, the headless form owner over one `Schema.Schema<A, Record<string, string>>` field shape — `validate` decodes the raw `FormData` through `Schema.decodeUnknownEither` into either the typed value or a `ParseResult.ParseError`, `errorMap` projects the `ParseError` into a per-path `Record<string, string>` the `react-aria-components` `Form` `validationBehavior: "aria"` resolves onto each `FieldError` slot, and `submit` crosses the decoded value into the `interchange` `CommandGateway` as one intent. The schema is the single source of field validity; the react-stately field-state hooks own the headless control state.
- Cases: `validate` reads the submitted `FormData` as a `Record<string, string>` and runs `Schema.decodeUnknownEither(schema)`, returning `Either.right(value)` on a clean decode or `Either.left(parseError)` otherwise; `errorMap` walks the `ParseError` into the `{ [path]: message }` map keyed by the field name so `Form`'s `validationBehavior: "aria"` writes each message into the matching field's `aria-describedby` error region and no field re-implements a validator; `submit` short-circuits on `Either.left` (rendering the error map) and dials the `CommandGateway` on `Either.right`. The `inputs` `RoleBehavior` family (field/radio/select/slider) owns the control state through the react-stately `useNumberFieldState`/`useSelectState`/`useComboBoxState`/`useRadioGroupState`/`useSliderState` hooks; the schema owns the validity, the role owns the behavior.
- Entry: a form composes one `FormBinding<A>` over its `Schema.Struct`; the field controls compose the `role-behavior.md` `inputs` rows; the submit decodes once and either renders the projected error map or crosses the `interchange` `CommandGateway`; the form holds no domain state and reads any prefill default through the `binding/atom.md` `AtomBinding`.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `effect`.
- Growth: a new field lands as one `Schema.Struct` property and one `inputs` control row, never a parallel validator; a new validity rule lands as one `Schema` refinement/filter on the field property, never a second validation pass; a new form lands as one `FormBinding<A>` over its own schema, never a shared mutable form-state object.
- Boundary: a parallel validation library (a hand-rolled per-field validator, a second schema for the same shape the wire owns, an imperative `onChange` validity check) beside the `Schema.decodeUnknownEither` fold is the named defect the schema deletes; a field-error written outside the `Form` `validationBehavior: "aria"` projection is the named defect; a submit that dials a transport directly instead of the `interchange` `CommandGateway` is the named defect; a `useState` form-state object holding the field values instead of the react-stately field-state hooks and the schema decode is the named defect.

```ts contract
import type { CommandGateway } from "@rasm/ts/interchange";
import { Effect, Either, ParseResult, Schema } from "effect";

interface FormBinding<A> {
  readonly schema: Schema.Schema<A, Record<string, string>>;
  readonly validate: (data: FormData) => Either.Either<A, ParseResult.ParseError>;
  readonly errorMap: (error: ParseResult.ParseError) => Record<string, string>;
  readonly submit: (data: FormData) => Effect.Effect<void, ParseResult.ParseError, CommandGateway>;
}

const formDataRecord = (data: FormData): Record<string, string> =>
  Object.fromEntries(Array.from(data.entries(), ([k, v]) => [k, String(v)]));

const errorMap = (error: ParseResult.ParseError): Record<string, string> =>
  ParseResult.ArrayFormatter.formatErrorSync(error).reduce<Record<string, string>>(
    (acc, issue) => (issue.path.length === 0 ? acc : { ...acc, [String(issue.path[0])]: issue.message }),
    {},
  );

const makeFormBinding = <A>(
  schema: Schema.Schema<A, Record<string, string>>,
  dispatch: (value: A) => Effect.Effect<void, never, CommandGateway>,
): FormBinding<A> => {
  const validate = (data: FormData): Either.Either<A, ParseResult.ParseError> =>
    Schema.decodeUnknownEither(schema)(formDataRecord(data));
  return {
    schema,
    validate,
    errorMap,
    submit: (data) =>
      Either.match(validate(data), {
        onLeft: (error) => Effect.fail(error),
        onRight: (value) => dispatch(value),
      }),
  };
};
```

The `errorMap` folds `ParseResult.ArrayFormatter.formatErrorSync` per-path issues into the `{ [field]: message }` map keyed by the first path segment — the field name — that the `Form` `validationBehavior: "aria"` resolves onto each `FieldError` slot's `aria-describedby` region. `Schema.decodeUnknownEither`, `ParseResult.ArrayFormatter`, `ParseResult.ParseError`, and `Either.match` are catalogued and transcribed above.
