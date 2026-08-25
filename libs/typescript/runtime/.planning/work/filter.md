# [RUNTIME_FILTER]

Subscription filtering is ONE owner: the seven specification dialects compile into a single predicate shape, so a delivery decision folds one AND-set regardless of which dialect declared it, and a transport that can push a dialect to its broker reads that verdict off the dialect's own row rather than re-deriving it per binding. Six dialects are attribute-comparison rows that reach no parser; the seventh is CESQL, whose expression grammar no comparison row can express.

`core:interchange/carrier#EVENT_ENVELOPE` settles the message envelope, its attribute grammar, and its extension roster; this page resolves identifiers against them and mints no envelope vocabulary. CESQL evaluation is TOTAL — every operator, cast, and function answers a value beside an accumulated fault list — so a filter meeting several defects reports all of them and still settles a verdict. Its module ships on the `./server` subpath as `runtime/src/work/filter.ts`.

## [01]-[INDEX]

- [02]-[DIALECT_ROSTER]: seven dialect rows, untagged wire declaration, compiled predicate, AND-set fold; `Filter`.
- [03]-[VALUE_ALGEBRA]: three CESQL types, seven-reason fault vocabulary, total readings, 32-bit saturation, implicit-cast matrix; `Cesql`.
- [04]-[OPERATOR_TABLES]: arithmetic, ordering, equality, the short-circuit logic carve, the built-in function table; `Cesql`.
- [05]-[EXPRESSION_FOLD]: attribute resolution, the pattern compile, and the total evaluation over the owned expression family; `Cesql`.
- [06]-[GRAMMAR_OWNER]: token vocabulary, recorded LL(k) grammar, visitor lowering, compiled service; `Cesql`.

## [02]-[DIALECT_ROSTER]

- Owner: `Filter` — the seven dialect rows, the untagged wire declaration each subscription carries, and the compile-and-fold pair every delivery seam composes.
- Law: every dialect compiles into ONE predicate shape, so the delivery fold reads a verdict and never a dialect; a consumer branching on which dialect declared a filter re-derives the dispatch the compile already settled.
- Law: `filters` is an AND-set — any false expression withholds delivery — so nesting is `all`/`any`/`not` rows rather than a combining dialect beside the roster, and a subscription carrying several filters needs no join grammar.
- Law: the wire ships each case as a single-key object, so the discriminant attaches at the declaration through `Schema.attachPropertySignature` and the encoded side stays exactly what the specification publishes; a re-tagged provider shape reaching the union is the boundary defect this declaration forecloses.
- Law: `pushdown` is a column the BINDING reads, never a capability this page realizes — a transport resolving a dialect at its broker consults the row and a transport that cannot filters consumer-side; `sql` reads consumer-side on every transport, since no broker parses a CESQL expression.
- Law: recursion closes through `Schema.suspend` inside the union, and the nesting ceiling refuses at admission, so the compiled fold walks a data depth without a frontier.
- Law: an unparseable declaration refuses the SUBSCRIPTION at admission and never a delivery — a filter that cannot compile has no verdict to withhold, and admitting it delivers everything the operator meant to exclude.
- Entry: `Filter.compile(spec)` at subscription admission answers `Either<Compiled, CesqlFault>`; `Filter.admits(compiled, envelope)` at delivery answers the verdict beside the faults its evaluation accumulated.
- Growth: a dialect is one union member, one row, and one compile arm; the delivery fold never widens.
- Boundary: which subscription holds which filters, and where a subscription persists, are the consuming binding's; this page owns the dialect vocabulary and its evaluation alone.
- Packages: `effect` (`Array`, `Either`, `Option`, `Record`, `Schema`, `pipe`); `cloudevents` (`CloudEventV1`); `@rasm/core` (`Event`, `Fault.Class`, `Shape.Record`).

```typescript
import {
  createToken, CstParser, Lexer, type CstChildrenDictionary, type CstElement, type CstNode, type IToken, type TokenType,
} from "chevrotain"
import type { CloudEventV1 } from "cloudevents"
import { Array, Data, Effect, Either, Number, Option, Order, Predicate, Record, RegExp, Schema, String, pipe } from "effect"
import { Event, Fault, Shape } from "@rasm/core"

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace Filter {
  type Dialect = keyof typeof _DIALECTS
  type Row = { readonly pushdown: string; readonly arity: "attributes" | "nested" | "single" | "text" }
  type Spec =
    | { readonly _tag: "all"; readonly all: ReadonlyArray<Spec> }
    | { readonly _tag: "any"; readonly any: ReadonlyArray<Spec> }
    | { readonly _tag: "exact"; readonly exact: Record.ReadonlyRecord<string, string> }
    | { readonly _tag: "not"; readonly not: Spec }
    | { readonly _tag: "prefix"; readonly prefix: Record.ReadonlyRecord<string, string> }
    | { readonly _tag: "sql"; readonly sql: string }
    | { readonly _tag: "suffix"; readonly suffix: Record.ReadonlyRecord<string, string> }
  type Compiled = { readonly dialect: Dialect; readonly evaluate: (envelope: CloudEventV1<unknown>) => Cesql.Reading }
  type Verdict = { readonly admitted: boolean; readonly faults: ReadonlyArray<CesqlFault> }
  type _Rows<T extends Record.ReadonlyRecord<Dialect, Row> = typeof _DIALECTS> = T
}

// --- [MODELS] --------------------------------------------------------------------------

const _Attributes = Shape.Record(Schema.NonEmptyString, Schema.String)

const _Spec: Schema.Schema<Filter.Spec> = Schema.Union(
  Schema.Struct({ exact: _Attributes }).pipe(Schema.attachPropertySignature("_tag", "exact")),
  Schema.Struct({ prefix: _Attributes }).pipe(Schema.attachPropertySignature("_tag", "prefix")),
  Schema.Struct({ suffix: _Attributes }).pipe(Schema.attachPropertySignature("_tag", "suffix")),
  Schema.Struct({ sql: Schema.NonEmptyString }).pipe(Schema.attachPropertySignature("_tag", "sql")),
  Schema.Struct({ all: Schema.Array(Schema.suspend((): Schema.Schema<Filter.Spec> => _Spec)) })
    .pipe(Schema.attachPropertySignature("_tag", "all")),
  Schema.Struct({ any: Schema.Array(Schema.suspend((): Schema.Schema<Filter.Spec> => _Spec)) })
    .pipe(Schema.attachPropertySignature("_tag", "any")),
  Schema.Struct({ not: Schema.suspend((): Schema.Schema<Filter.Spec> => _Spec) })
    .pipe(Schema.attachPropertySignature("_tag", "not")),
)

// --- [OPERATIONS] ----------------------------------------------------------------------

const _DIALECTS = {
  all: { arity: "nested", pushdown: "<broker-only-when-every-child-pushes-down>" },
  any: { arity: "nested", pushdown: "<broker-only-when-every-child-pushes-down>" },
  exact: { arity: "attributes", pushdown: "<mqtt-and-nats-on-the-routing-attribute>" },
  not: { arity: "single", pushdown: "<consumer-side-on-every-transport>" },
  prefix: { arity: "attributes", pushdown: "<mqtt-and-nats-wildcard-where-the-attribute-is-the-routing-key>" },
  sql: { arity: "text", pushdown: "<consumer-side-always:-no-broker-parses-a-cesql-expression>" },
  suffix: { arity: "attributes", pushdown: "<consumer-side-on-every-transport>" },
} as const satisfies Record.ReadonlyRecord<string, Filter.Row>

const _TEXT = {
  exact: (held: string, expected: string) => held === expected,
  prefix: (held: string, expected: string) => String.startsWith(expected)(held),
  suffix: (held: string, expected: string) => String.endsWith(expected)(held),
} as const

const _textual = (
  dialect: keyof typeof _TEXT,
  attributes: Record.ReadonlyRecord<string, string>,
): Filter.Compiled => ({
  dialect,
  evaluate: (envelope) =>
    pipe(
      Array.map(Record.toEntries(attributes), ([name, expected]) =>
        pipe(_cesqlCast(_cesqlAttribute(envelope, name), "String"), (held) =>
          _cesqlRead(CesqlValue.Boolean({ value: _TEXT[dialect](_cesqlText(held), expected) }), held))),
      (readings) =>
        _cesqlRead(
          CesqlValue.Boolean({ value: Array.every(readings, (reading) => _cesqlFlag(reading)) }),
          ...readings,
        ),
    ),
})

const _nested = (
  dialect: "all" | "any",
  children: ReadonlyArray<Filter.Compiled>,
): Filter.Compiled => ({
  dialect,
  evaluate: (envelope) =>
    pipe(Array.map(children, (child) => child.evaluate(envelope)), (readings) =>
      _cesqlRead(
        CesqlValue.Boolean({
          value: dialect === "all"
            ? Array.every(readings, (reading) => _cesqlFlag(reading))
            : Array.some(readings, (reading) => _cesqlFlag(reading)),
        }),
        ...readings,
      )),
})

const _compiled = (
  spec: Filter.Spec,
  grammar: _CesqlGrammar,
  lower: { readonly visit: (node: CstNode) => unknown },
  depth: number,
): Either.Either<Filter.Compiled, CesqlFault> =>
  depth > _CESQL_CEILING.depth
    ? Either.left(new CesqlFault({ case: { reason: "parse", stage: "nesting", detail: `spec nests past depth ${_CESQL_CEILING.depth}` } }))
    : _Compile[spec._tag](spec as never, grammar, lower, depth)

const _Compile: {
  readonly [K in Filter.Dialect]: (
    spec: Extract<Filter.Spec, { readonly _tag: K }>,
    grammar: _CesqlGrammar,
    lower: { readonly visit: (node: CstNode) => unknown },
    depth: number,
  ) => Either.Either<Filter.Compiled, CesqlFault>
} = {
  all: (spec, grammar, lower, depth) =>
    Either.map(Either.all(Array.map(spec.all, (child) => _compiled(child, grammar, lower, depth + 1))), (children) =>
      _nested("all", children)),
  any: (spec, grammar, lower, depth) =>
    Either.map(Either.all(Array.map(spec.any, (child) => _compiled(child, grammar, lower, depth + 1))), (children) =>
      _nested("any", children)),
  exact: (spec) => Either.right(_textual("exact", spec.exact)),
  not: (spec, grammar, lower, depth) =>
    Either.map(_compiled(spec.not, grammar, lower, depth + 1), (child) => ({
      dialect: "not" as const,
      evaluate: (envelope: CloudEventV1<unknown>) =>
        pipe(child.evaluate(envelope), (held) => _cesqlRead(CesqlValue.Boolean({ value: !_cesqlFlag(held) }), held)),
    })),
  prefix: (spec) => Either.right(_textual("prefix", spec.prefix)),
  sql: (spec, grammar, lower) =>
    Either.map(_cesqlCompiled(spec.sql, grammar, lower), (expr) => ({
      dialect: "sql" as const,
      evaluate: (envelope: CloudEventV1<unknown>) => _cesqlEvaluate(expr, envelope),
    })),
  suffix: (spec) => Either.right(_textual("suffix", spec.suffix)),
}
```

## [03]-[VALUE_ALGEBRA]

- Owner: `Cesql` — the three-type value family, the seven-reason fault vocabulary, the total reading pair, the 32-bit integer band, and the implicit-cast matrix every operator and function reads.
- Law: evaluation is TOTAL — every arm answers a value beside an accumulated fault list — so an expression meeting several defects reports all of them and still settles; a rail aborting on the first refusal loses every later reason and turns a diagnosable filter into one opaque miss.
- Law: Integer is 32-bit and SATURATES — a result past the band answers the nearest bound beside `math` rather than wrapping, which is why the negation of the floor answers the ceiling and never its own operand; division and remainder truncate toward zero and a zero divisor answers the type's zero beside `math`.
- Law: the implicit-cast matrix is the whole three-by-three as data — an operand crossing into an operator's declared type reads one row, a refused crossing answers the target's zero beside `cast`, and no arm re-derives a conversion the table already answers.
- Law: a string admits to Boolean on the two specification words alone and to Integer on a strict decimal alone, so `'1.5'` and `''` both refuse rather than rounding or reading as zero.
- Law: the seven reasons carry the conformance corpus's own spellings and declare alphabetically, so two branches transcribing the roster publish one sequence; each grades through the core class table rather than minting a second taxonomy.
- Law: each reason declares its OWN subject and renders it — the crossing pair a cast refused, the operand and extent a slice fell outside, the site and value the 32-bit band rejected, whether an absent identifier is a declared extension, the offered arity beside a name, the stage a text refusal reached — so a diagnostic is columns a consumer folds rather than a hand-templated word pair only a human could read.
- Growth: a type is one case on the value family with its own cast column; a reason is one row on the family carrying its own subject and renderer.
- Packages: `effect` (`Array`, `Data`, `Number`, `Option`, `Record`, `Schema`, `String`, `pipe`); `@rasm/core` (`Fault.Class`).

```typescript
type CesqlValue = Data.TaggedEnum<{
  Boolean: { readonly value: boolean }
  Integer: { readonly value: number }
  String: { readonly value: string }
}>
const CesqlValue = Data.taggedEnum<CesqlValue>()

declare namespace Cesql {
  type Type = CesqlValue["_tag"]
  type Slot = Type | "Any"
  type Issue = typeof _cesqlFamily.payload.Type
  type Reason = (typeof _cesqlFamily.kinds)[number]
  type Reading = { readonly value: CesqlValue; readonly faults: ReadonlyArray<CesqlFault> }
  type Binary = keyof typeof _CESQL_ARITH | keyof typeof _CESQL_EQUALITY | keyof typeof _CESQL_ORDER
  type Logic = keyof typeof _CESQL_LOGIC
  type FunctionRow = {
    readonly params: ReadonlyArray<Slot>
    readonly optional: ReadonlyArray<Slot>
    readonly rest: Option.Option<Slot>
    readonly returns: Type
    readonly kernel: (operands: ReadonlyArray<CesqlValue>) => Reading
  }
  type _Casts<T extends { readonly [From in Type]: Record.ReadonlyRecord<Type, unknown> } = typeof _CESQL_CAST> = T
  type _Functions<T extends Record.ReadonlyRecord<string, FunctionRow> = typeof _CESQL_FUNCTIONS> = T
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CESQL_I32 = { max: 2147483647, min: -2147483648 } as const
const _CESQL_CEILING = { depth: 32, text: 4096 } as const
const _CESQL_DECIMAL = /^[+-]?(?:0|[1-9][0-9]*)$/
const _CESQL_TRUTH = { false: false, true: true } as const

const _CESQL_ZERO = {
  Boolean: CesqlValue.Boolean({ value: false }),
  Integer: CesqlValue.Integer({ value: 0 }),
  String: CesqlValue.String({ value: "" }),
} as const satisfies { readonly [K in Cesql.Type]: Extract<CesqlValue, { readonly _tag: K }> }

// --- [ERRORS] --------------------------------------------------------------------------

const _CESQL_TYPES = ["Boolean", "Integer", "String"] as const satisfies ReadonlyArray<Cesql.Type>
const _CesqlType = Schema.Literal(..._CESQL_TYPES)

const _CESQL_STAGES = ["lex", "nesting", "parse", "text"] as const

const _cesqlFamily = Fault.Class.family(
  ["cast", "functionEvaluation", "generic", "math", "missingAttribute", "missingFunction", "parse"] as const,
  {
    cast: Fault.Class.row({
      class: "invalid",
      leg: "value",
      detail: Schema.Struct({ from: _CesqlType, text: Schema.String, to: _CesqlType }),
      render: ({ from, text, to }) => `${from} value ${text} does not cross into ${to}`,
    }),
    functionEvaluation: Fault.Class.row({
      class: "invalid",
      leg: "builtin",
      detail: Schema.Struct({ extent: Schema.Int, operand: Schema.Int }),
      render: ({ extent, operand }) => `operand ${operand} falls outside the ${extent}-character subject`,
    }),
    generic: Fault.Class.row({
      class: "defect",
      leg: "grammar",
      detail: Schema.Struct({ detail: Schema.String }),
      render: ({ detail }) => `the lowering visitor threw — ${detail}`,
    }),
    math: Fault.Class.row({
      class: "invalid",
      leg: "value",
      detail: Schema.Struct({ site: Schema.NonEmptyString, value: Schema.Number }),
      render: ({ site, value }) => `${site} could not answer over ${value} inside the 32-bit band`,
    }),
    missingAttribute: Fault.Class.row({
      class: "absent",
      leg: "envelope",
      detail: Schema.Struct({ name: Schema.NonEmptyString, rostered: Schema.Boolean }),
      render: ({ name, rostered }) =>
        rostered ? `${name} is a declared extension this envelope omitted` : `${name} names no attribute this roster declares`,
    }),
    missingFunction: Fault.Class.row({
      class: "malformed",
      leg: "builtin",
      detail: Schema.Struct({ arity: Schema.Int, name: Schema.NonEmptyString, rostered: Schema.Boolean }),
      render: ({ arity, name, rostered }) =>
        rostered ? `${name} rows no arity ${arity}` : `${name} names no builtin this table rows`,
    }),
    parse: Fault.Class.row({
      class: "malformed",
      leg: "grammar",
      detail: Schema.Struct({ detail: Schema.String, stage: Schema.Literal(..._CESQL_STAGES) }),
      render: ({ detail, stage }) => `filter text refused at ${stage} — ${detail}`,
    }),
  },
)

class CesqlFault extends Schema.TaggedError<CesqlFault>()("CesqlFault", {
  case: _cesqlFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _cesqlFamily.classOf(this.case.reason)
  }
  override get message(): string {
    return _cesqlFamily.render(this.case)
  }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _cesqlCarried = (carried: ReadonlyArray<Cesql.Reading>): ReadonlyArray<CesqlFault> =>
  Array.flatMap(carried, (reading) => reading.faults)

const _cesqlRead = (value: CesqlValue, ...carried: ReadonlyArray<Cesql.Reading>): Cesql.Reading => ({
  value,
  faults: _cesqlCarried(carried),
})

const _cesqlRaise = (
  issue: Cesql.Issue,
  value: CesqlValue,
  ...carried: ReadonlyArray<Cesql.Reading>
): Cesql.Reading => ({ value, faults: [..._cesqlCarried(carried), new CesqlFault({ case: issue })] })

const _cesqlFlag = (reading: Cesql.Reading): boolean =>
  reading.value._tag === "Boolean" ? reading.value.value : false
const _cesqlNumber = (reading: Cesql.Reading): number =>
  reading.value._tag === "Integer" ? reading.value.value : 0
const _cesqlText = (reading: Cesql.Reading): string =>
  reading.value._tag === "String" ? reading.value.value : ""

const _cesqlInt = (raw: number, site: string, ...carried: ReadonlyArray<Cesql.Reading>): Cesql.Reading =>
  globalThis.Number.isInteger(raw) && raw >= _CESQL_I32.min && raw <= _CESQL_I32.max
    ? _cesqlRead(CesqlValue.Integer({ value: raw }), ...carried)
    : _cesqlRaise(
      { reason: "math", site, value: raw },
      CesqlValue.Integer({
        value: globalThis.Number.isNaN(raw)
          ? 0
          : Number.min(_CESQL_I32.max, Number.max(_CESQL_I32.min, Math.trunc(raw))),
      }),
      ...carried,
    )

const _CESQL_CAST: {
  readonly [From in Cesql.Type]: {
    readonly [To in Cesql.Type]: (held: Extract<CesqlValue, { readonly _tag: From }>) => Cesql.Reading
  }
} = {
  Boolean: {
    Boolean: (held) => _cesqlRead(held),
    Integer: (held) => _cesqlRead(CesqlValue.Integer({ value: held.value ? 1 : 0 })),
    String: (held) => _cesqlRead(CesqlValue.String({ value: held.value ? "true" : "false" })),
  },
  Integer: {
    Boolean: (held) => _cesqlRead(CesqlValue.Boolean({ value: held.value !== 0 })),
    Integer: (held) => _cesqlRead(held),
    String: (held) => _cesqlRead(CesqlValue.String({ value: `${held.value}` })),
  },
  String: {
    Boolean: (held) =>
      Option.match(Record.get(_CESQL_TRUTH, String.toLowerCase(held.value)), {
        onNone: () => _cesqlRaise({ reason: "cast", from: "String", text: held.value, to: "Boolean" }, _CESQL_ZERO.Boolean),
        onSome: (value) => _cesqlRead(CesqlValue.Boolean({ value })),
      }),
    Integer: (held) =>
      Option.match(
        Option.flatMap(Option.liftPredicate(held.value, (text) => _CESQL_DECIMAL.test(text)), Number.parse),
        {
          onNone: () => _cesqlRaise({ reason: "cast", from: "String", text: held.value, to: "Integer" }, _CESQL_ZERO.Integer),
          onSome: (parsed) => _cesqlInt(parsed, "cast"),
        },
      ),
    String: (held) => _cesqlRead(held),
  },
}

const _cesqlCast = (reading: Cesql.Reading, target: Cesql.Type): Cesql.Reading =>
  pipe(
    (_CESQL_CAST[reading.value._tag] as Record.ReadonlyRecord<Cesql.Type, (held: CesqlValue) => Cesql.Reading>)[target](
      reading.value,
    ),
    (crossed) => ({ value: crossed.value, faults: [...reading.faults, ...crossed.faults] }),
  )
```

## [04]-[OPERATOR_TABLES]

- Owner: the arithmetic, ordering, equality, logic, and built-in function tables — every operator and function is a row, so dispatch is a keyed read and a new operator adds no arm anywhere.
- Law: `AND` and `OR` carry the specification's ONE short-circuit carve — an absorbing left operand settles the expression and the right operand never evaluates, so its faults never reach the report; `XOR` names no absorbing value and evaluates both sides. That carve rides the row's own `absorbing` column, never a branch beside the table.
- Law: equality casts the RIGHT operand into the LEFT operand's type, so the pair always shares a tag and one primitive comparison serves all three types; ordering and arithmetic cast both sides into Integer, which is why two decimal-text attributes compare numerically rather than lexically.
- Law: arithmetic is partial at the divisor alone — the row answers `Option.none` and the caller folds it into `math`, so no arm answers a non-finite value a later comparison reads as a number.
- Law: arity is part of a function name's identity — a resolved name at an arity its row does not admit refuses exactly as an unknown name does, which is why the separator-joining concatenation refuses at zero operands while the plain one admits it.
- Law: the two casting predicates answer whether a crossing lands and DISCARD the probe's faults by rule, since a predicate asking that question cannot itself report the refusal it exists to detect.
- Growth: an operator is one row on its own table; a function is one row carrying its slot list, its return type, and one total kernel.
- Packages: `effect` (`Array`, `Number`, `Option`, `Record`, `String`, `pipe`).

```typescript
const _CESQL_ARITH = {
  "%": (left: number, right: number) => (right === 0 ? Option.none() : Option.some(left % right)),
  "*": (left: number, right: number) => Option.some(left * right),
  "+": (left: number, right: number) => Option.some(left + right),
  "-": (left: number, right: number) => Option.some(left - right),
  "/": (left: number, right: number) => (right === 0 ? Option.none() : Option.some(Math.trunc(left / right))),
} as const

const _CESQL_ORDER = {
  "<": (left: number, right: number) => left < right,
  "<=": (left: number, right: number) => left <= right,
  ">": (left: number, right: number) => left > right,
  ">=": (left: number, right: number) => left >= right,
} as const

const _CESQL_EQUALITY = { "!=": true, "<>": true, "=": false } as const

const _CESQL_LOGIC = {
  AND: { absorbing: Option.some(false), fold: (left: boolean, right: boolean) => left && right },
  OR: { absorbing: Option.some(true), fold: (left: boolean, right: boolean) => left || right },
  XOR: { absorbing: Option.none<boolean>(), fold: (left: boolean, right: boolean) => left !== right },
} as const

const _CESQL_FUNCTIONS = {
  ABS: {
    params: ["Integer"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Integer",
    kernel: (operands) => _cesqlInt(Math.abs(_cesqlSlot(operands, 0, _cesqlNumber)), "ABS"),
  },
  BOOL: {
    params: ["Any"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Boolean",
    kernel: (operands) => _cesqlCross(operands, "Boolean"),
  },
  CONCAT: {
    params: [], optional: [], rest: Option.some<Cesql.Slot>("String"), returns: "String",
    kernel: (operands) => _cesqlJoined(operands, ""),
  },
  CONCAT_WS: {
    params: ["String"], optional: [], rest: Option.some<Cesql.Slot>("String"), returns: "String",
    kernel: (operands) => _cesqlJoined(Array.drop(operands, 1), _cesqlSlot(operands, 0, _cesqlText)),
  },
  INT: {
    params: ["Any"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Integer",
    kernel: (operands) => _cesqlCross(operands, "Integer"),
  },
  IS_BOOL: {
    params: ["Any"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Boolean",
    kernel: (operands) => _cesqlProbe(operands, "Boolean"),
  },
  IS_INT: {
    params: ["Any"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Boolean",
    kernel: (operands) => _cesqlProbe(operands, "Integer"),
  },
  LEFT: {
    params: ["String", "Integer"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlSliced(operands, (text, take) => text.slice(0, take)),
  },
  LENGTH: {
    params: ["String"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "Integer",
    kernel: (operands) => _cesqlInt(_cesqlSlot(operands, 0, _cesqlText).length, "LENGTH"),
  },
  LOWER: {
    params: ["String"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlWord(operands, String.toLowerCase),
  },
  RIGHT: {
    params: ["String", "Integer"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlSliced(operands, (text, take) => text.slice(text.length - Number.min(take, text.length))),
  },
  STRING: {
    params: ["Any"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlCross(operands, "String"),
  },
  SUBSTRING: {
    params: ["String", "Integer"], optional: ["Integer"], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlSubstring(operands),
  },
  TRIM: {
    params: ["String"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlWord(operands, String.trim),
  },
  UPPER: {
    params: ["String"], optional: [], rest: Option.none<Cesql.Slot>(), returns: "String",
    kernel: (operands) => _cesqlWord(operands, String.toUpperCase),
  },
} as const satisfies Record.ReadonlyRecord<string, Cesql.FunctionRow>

const _cesqlSlot = <A>(operands: ReadonlyArray<CesqlValue>, slot: number, read: (reading: Cesql.Reading) => A): A =>
  read(_cesqlRead(operands[slot] ?? _CESQL_ZERO.String))

const _cesqlJoined = (operands: ReadonlyArray<CesqlValue>, separator: string): Cesql.Reading =>
  _cesqlRead(
    CesqlValue.String({ value: Array.join(Array.map(operands, (held) => _cesqlText(_cesqlRead(held))), separator) }),
  )

const _cesqlWord = (operands: ReadonlyArray<CesqlValue>, shape: (text: string) => string): Cesql.Reading =>
  _cesqlRead(CesqlValue.String({ value: shape(_cesqlSlot(operands, 0, _cesqlText)) }))

const _cesqlCross = (operands: ReadonlyArray<CesqlValue>, target: Cesql.Type): Cesql.Reading =>
  _cesqlCast(_cesqlRead(operands[0] ?? _CESQL_ZERO[target]), target)

const _cesqlProbe = (operands: ReadonlyArray<CesqlValue>, target: Cesql.Type): Cesql.Reading =>
  _cesqlRead(CesqlValue.Boolean({ value: Array.isEmptyReadonlyArray(_cesqlCross(operands, target).faults) }))

const _cesqlSliced = (
  operands: ReadonlyArray<CesqlValue>,
  take: (text: string, count: number) => string,
): Cesql.Reading =>
  pipe(
    { count: _cesqlSlot(operands, 1, _cesqlNumber), text: _cesqlSlot(operands, 0, _cesqlText) },
    ({ count, text }) =>
      count < 0
        ? _cesqlRaise({ reason: "functionEvaluation", extent: text.length, operand: count }, CesqlValue.String({ value: text }))
        : _cesqlRead(CesqlValue.String({ value: take(text, count) })),
  )

const _cesqlSubstring = (operands: ReadonlyArray<CesqlValue>): Cesql.Reading =>
  pipe(
    { start: _cesqlSlot(operands, 1, _cesqlNumber), text: _cesqlSlot(operands, 0, _cesqlText) },
    ({ start, text }) => ({
      from: start > 0 ? start - 1 : text.length + start,
      span: operands.length > 2 ? _cesqlSlot(operands, 2, _cesqlNumber) : text.length,
      start,
      text,
    }),
    ({ from, span, start, text }) =>
      start === 0
        ? _cesqlRead(_CESQL_ZERO.String)
        : from < 0 || from >= text.length
        ? _cesqlRaise({ reason: "functionEvaluation", extent: text.length, operand: start }, _CESQL_ZERO.String)
        : _cesqlRead(CesqlValue.String({ value: text.slice(from, from + Number.max(span, 0)) })),
  )
```

## [05]-[EXPRESSION_FOLD]

- Owner: the owned expression family, the attribute reader, the pattern compile, and the one total fold every dialect's `sql` arm evaluates.
- Law: an absent attribute answers `Boolean(false)` beside `missingAttribute`, and its peer operand never evaluates — a missing left operand reports the absence ALONE, so a division whose dividend does not exist carries no zero-divisor refusal beside it.
- Law: the generated roster decides the diagnostic through `Event.rasm.extensions`; no filter owns extension names.
- Law: attribute values cross as the wire forms the envelope carries, so a URI reference and an RFC-3339 instant both read as String and only an integral number reads as Integer; a decoded domain value never enters the fold, because the specification compares what crosses.
- Law: `EXISTS` is total over absence by construction and mints no fault, so a filter probing an optional attribute never reports the absence it exists to test.
- Law: a pattern's backslash escapes the two wildcards and itself and stands literal before every other glyph, and every literal glyph splices through `RegExp.escape` OUTSIDE any character class, so an escaped hyphen cannot mint a range.
- Law: the fold recurses natively because admission already bounded the depth — the compile refuses past the nesting ceiling, which is what converts a caller-supplied expression into the data depth this form requires.
- Growth: an expression case is one family member with its fold arm; the missing arm is a compile error at the exhaustive dispatch.
- Packages: `effect` (`Array`, `Data`, `Option`, `Predicate`, `Record`, `RegExp`, `String`, `pipe`); `cloudevents` (`CloudEventV1`); `@rasm/core` (`Event`).

```typescript
type CesqlExpr = Data.TaggedEnum<{
  Attribute: { readonly name: string }
  Binary: { readonly op: Cesql.Binary; readonly left: CesqlExpr; readonly right: CesqlExpr }
  Call: { readonly name: string; readonly operands: ReadonlyArray<CesqlExpr> }
  Exists: { readonly name: string }
  In: { readonly operand: CesqlExpr; readonly set: ReadonlyArray<CesqlExpr>; readonly negated: boolean }
  Like: { readonly operand: CesqlExpr; readonly pattern: globalThis.RegExp; readonly negated: boolean }
  Literal: { readonly value: CesqlValue }
  Logic: { readonly op: Cesql.Logic; readonly left: CesqlExpr; readonly right: CesqlExpr }
  Negate: { readonly operand: CesqlExpr }
  Not: { readonly operand: CesqlExpr }
}>
const CesqlExpr = Data.taggedEnum<CesqlExpr>()

const _CESQL_WILD = { "%": ".*", _: "." } as const

const _cesqlPattern = (pattern: string): globalThis.RegExp => {
  const glyphs = Array.fromIterable(pattern)
  let source = ""
  let index = 0
  while (index < glyphs.length) {
    const glyph = glyphs[index]!
    const next = glyphs[index + 1]
    const escapes = glyph === "\\" && next !== undefined && (next === "\\" || Record.has(_CESQL_WILD, next))
    source += escapes
      ? RegExp.escape(next!)
      : Option.getOrElse(Record.get(_CESQL_WILD, glyph), () => RegExp.escape(glyph))
    index += escapes ? 2 : 1
  }
  return new globalThis.RegExp(`^${source}$`, "su")
}

const _cesqlAttribute = (envelope: CloudEventV1<unknown>, name: string): Cesql.Reading =>
  Option.match(Option.fromNullable(envelope[name]), {
    onNone: () =>
      _cesqlRaise({ reason: "missingAttribute", name, rostered: Event.rasm.extensions.is(name) }, _CESQL_ZERO.Boolean),
    onSome: (held) =>
      _cesqlRead(
        Predicate.isBoolean(held)
          ? CesqlValue.Boolean({ value: held })
          : Predicate.isNumber(held) && globalThis.Number.isInteger(held)
          ? CesqlValue.Integer({ value: held })
          : CesqlValue.String({ value: `${held}` }),
      ),
  })

const _cesqlBinary = (op: Cesql.Binary, left: Cesql.Reading, right: Cesql.Reading): Cesql.Reading =>
  Option.match(Record.get(_CESQL_EQUALITY, op), {
    onSome: (negate) =>
      pipe(_cesqlCast(right, left.value._tag), (crossed) =>
        _cesqlRead(CesqlValue.Boolean({ value: (left.value.value === crossed.value.value) !== negate }), left, crossed)),
    onNone: () =>
      pipe(
        { left: _cesqlCast(left, "Integer"), right: _cesqlCast(right, "Integer") },
        ({ left: lhs, right: rhs }) =>
          Option.match(Record.get(_CESQL_ORDER, op), {
            onSome: (compare) =>
              _cesqlRead(CesqlValue.Boolean({ value: compare(_cesqlNumber(lhs), _cesqlNumber(rhs)) }), lhs, rhs),
            onNone: () =>
              Option.match(
                Option.flatMap(Record.get(_CESQL_ARITH, op), (kernel) =>
                  kernel(_cesqlNumber(lhs), _cesqlNumber(rhs))),
                {
                  onNone: () => _cesqlRaise({ reason: "math", site: op, value: _cesqlNumber(rhs) }, _CESQL_ZERO.Integer, lhs, rhs),
                  onSome: (raw) => _cesqlInt(raw, op, lhs, rhs),
                },
              ),
          }),
      ),
  })

const _cesqlAbsent = (reading: Cesql.Reading): boolean =>
  Array.some(reading.faults, (fault) => fault.case.reason === "missingAttribute")

const _cesqlEvaluate = (expr: CesqlExpr, envelope: CloudEventV1<unknown>): Cesql.Reading =>
  CesqlExpr.$match(expr, {
    Attribute: ({ name }) => _cesqlAttribute(envelope, name),
    Binary: ({ left, op, right }) =>
      pipe(_cesqlEvaluate(left, envelope), (held) =>
        _cesqlAbsent(held) ? _cesqlRead(_CESQL_ZERO.Integer, held) : _cesqlBinary(op, held, _cesqlEvaluate(right, envelope))),
    Call: ({ name, operands }) => _cesqlCall(name, operands, envelope),
    Exists: ({ name }) => _cesqlRead(CesqlValue.Boolean({ value: envelope[name] !== undefined })),
    In: ({ negated, operand, set }) =>
      pipe(_cesqlEvaluate(operand, envelope), (held) =>
        pipe(Array.map(set, (member) => _cesqlCast(_cesqlEvaluate(member, envelope), held.value._tag)), (members) =>
          _cesqlRead(
            CesqlValue.Boolean({
              value: Array.some(members, (member) => member.value.value === held.value.value) !== negated,
            }),
            held,
            ...members,
          ))),
    Like: ({ negated, operand, pattern }) =>
      pipe(_cesqlCast(_cesqlEvaluate(operand, envelope), "String"), (held) =>
        _cesqlRead(CesqlValue.Boolean({ value: pattern.test(_cesqlText(held)) !== negated }), held)),
    Literal: ({ value }) => _cesqlRead(value),
    Logic: ({ left, op, right }) =>
      pipe({ held: _cesqlCast(_cesqlEvaluate(left, envelope), "Boolean"), row: _CESQL_LOGIC[op] }, ({ held, row }) =>
        Option.match(Option.filter(row.absorbing, (absorbing) => absorbing === _cesqlFlag(held)), {
          onSome: (absorbing) => _cesqlRead(CesqlValue.Boolean({ value: absorbing }), held),
          onNone: () =>
            pipe(_cesqlCast(_cesqlEvaluate(right, envelope), "Boolean"), (peer) =>
              _cesqlRead(CesqlValue.Boolean({ value: row.fold(_cesqlFlag(held), _cesqlFlag(peer)) }), held, peer)),
        })),
    Negate: ({ operand }) =>
      pipe(_cesqlCast(_cesqlEvaluate(operand, envelope), "Integer"), (held) =>
        _cesqlInt(-_cesqlNumber(held), "NEGATE", held)),
    Not: ({ operand }) =>
      pipe(_cesqlCast(_cesqlEvaluate(operand, envelope), "Boolean"), (held) =>
        _cesqlRead(CesqlValue.Boolean({ value: !_cesqlFlag(held) }), held)),
  })

const _cesqlCall = (
  name: string,
  operands: ReadonlyArray<CesqlExpr>,
  envelope: CloudEventV1<unknown>,
): Cesql.Reading =>
  Option.match(Record.get(_CESQL_FUNCTIONS, String.toUpperCase(name)), {
    onNone: () => _cesqlRaise({ reason: "missingFunction", arity: operands.length, name, rostered: false }, _CESQL_ZERO.Boolean),
    onSome: (row) =>
      Option.match(_cesqlSlots(row, operands.length), {
        onNone: () => _cesqlRaise({ reason: "missingFunction", arity: operands.length, name, rostered: true }, _CESQL_ZERO[row.returns]),
        onSome: (slots) =>
          pipe(
            Array.map(operands, (operand, index) =>
              pipe(_cesqlEvaluate(operand, envelope), (held) =>
                pipe(slots[index], (slot) => (slot === undefined || slot === "Any" ? held : _cesqlCast(held, slot))))),
            (admitted) =>
              pipe(row.kernel(Array.map(admitted, (held) => held.value)), (answered) => ({
                value: answered.value,
                faults: [..._cesqlCarried(admitted), ...answered.faults],
              })),
          ),
      }),
  })

const _cesqlSlots = (row: Cesql.FunctionRow, arity: number): Option.Option<ReadonlyArray<Cesql.Slot>> =>
  Option.map(
    Option.liftPredicate(
      arity - row.params.length,
      (extra) => extra >= 0 && (Option.isSome(row.rest) || extra <= row.optional.length),
    ),
    (extra) =>
      Option.match(row.rest, {
        onNone: () => [...row.params, ...Array.take(row.optional, extra)],
        onSome: (rest) => [...row.params, ...Array.makeBy(extra, () => rest)],
      }),
  )
```

## [06]-[GRAMMAR_OWNER]

- Owner: the token vocabulary, the recorded LL(k) grammar, the visitor lowering, and the `Cesql` service every subscription admission reaches for compilation.
- Law: grammar declaration is a RECORDING phase and `performSelfAnalysis()` closes the constructor, so an ambiguity, a left recursion, or a lookahead miss fails at module initialization rather than at the first admission.
- Law: recovery stays off and validations stay on — a synthesized token admits a filter the producer never wrote, and a grammar defect surfaces at construction.
- Law: declaration ORDER is the lexer's match order, so every keyword row precedes the identifier row and each two-glyph comparison spelling precedes its one-glyph prefix; a keyword prefixing a longer identifier loses to it through `longer_alt`, never through reordering.
- Law: keywords, boolean literals, function names, and identifiers are case-INSENSITIVE while a string literal preserves its own case, so the keyword patterns carry the ignore-case flag and the literal rule carries none.
- Law: one grammar instance serves every fiber because `compile` is synchronous end to end — the input binds, the rule runs, the errors read, and the instance resets with no suspension point between, so the library's carried mutable state cannot interleave two admissions.
- Law: the lowering rides the visitor whose completeness `validateVisitor()` proves, so a rule added without its arm fails at construction, and no concrete-syntax node, token, or token type escapes that seam.
- Law: `getBaseCstVisitorConstructor` returns a class the module extends once at initialization, so the visitor is built beside the grammar it validates against and never per admission.
- Entry: `Cesql.compile` is the admission seam; `Filter.compile` composes it for the `sql` dialect alone and the six comparison dialects reach no parser.
- Growth: a terminal is one token row, a production is one rule, and the visitor's own proof demands the matching arm.
- Packages: `chevrotain` (`createToken`, `CstParser`, `Lexer`, `CstChildrenDictionary`, `CstElement`, `CstNode`, `IToken`, `TokenType`); `effect` (`Array`, `Effect`, `Either`, `Option`, `Order`, `Predicate`, `Record`, `String`, `pipe`).

```typescript
// --- [COMPOSITION] ---------------------------------------------------------------------

const _Identifier = createToken({ name: "Identifier", pattern: /[a-zA-Z_][a-zA-Z0-9_]*/ })
const _cesqlKeyword = (name: string, word: string): TokenType =>
  createToken({ name, pattern: new globalThis.RegExp(word, "i"), longer_alt: _Identifier })

const _CESQL_TOKENS = {
  Whitespace: createToken({ name: "Whitespace", pattern: /\s+/, group: Lexer.SKIPPED }),
  And: _cesqlKeyword("And", "AND"),
  Or: _cesqlKeyword("Or", "OR"),
  Xor: _cesqlKeyword("Xor", "XOR"),
  Not: _cesqlKeyword("Not", "NOT"),
  Like: _cesqlKeyword("Like", "LIKE"),
  Exists: _cesqlKeyword("Exists", "EXISTS"),
  In: _cesqlKeyword("In", "IN"),
  True: _cesqlKeyword("True", "TRUE"),
  False: _cesqlKeyword("False", "FALSE"),
  Identifier: _Identifier,
  Integer: createToken({ name: "Integer", pattern: /0|[1-9][0-9]*/ }),
  Text: createToken({ name: "Text", pattern: /'(?:\\'|[^'])*'|"(?:\\"|[^"])*"/ }),
  LParen: createToken({ name: "LParen", pattern: /\(/ }),
  RParen: createToken({ name: "RParen", pattern: /\)/ }),
  Comma: createToken({ name: "Comma", pattern: /,/ }),
  Compare: createToken({ name: "Compare", pattern: /<=|>=|<>|!=|=|<|>/ }),
  Additive: createToken({ name: "Additive", pattern: /[+-]/ }),
  Multiplicative: createToken({ name: "Multiplicative", pattern: /[*/%]/ }),
} as const

const _cesqlVocabulary = Record.values(_CESQL_TOKENS)
const _cesqlLexer = new Lexer(_cesqlVocabulary, { ensureOptimizations: true, positionTracking: "onlyOffset" })

class _CesqlGrammar extends CstParser {
  constructor() {
    super(_cesqlVocabulary, { maxLookahead: 2, recoveryEnabled: false })
    this.performSelfAnalysis()
  }
  readonly expression = this.RULE("expression", () => {
    this.SUBRULE(this.conjunction)
    this.MANY(() => {
      this.OR([{ ALT: () => this.CONSUME(_CESQL_TOKENS.Or) }, { ALT: () => this.CONSUME(_CESQL_TOKENS.Xor) }])
      this.SUBRULE2(this.conjunction)
    })
  })
  readonly conjunction = this.RULE("conjunction", () => {
    this.SUBRULE(this.comparison)
    this.MANY(() => {
      this.CONSUME(_CESQL_TOKENS.And)
      this.SUBRULE2(this.comparison)
    })
  })
  readonly comparison = this.RULE("comparison", () => {
    this.SUBRULE(this.additive)
    this.OPTION(() =>
      this.OR([
        {
          ALT: () => {
            this.CONSUME(_CESQL_TOKENS.Compare)
            this.SUBRULE2(this.additive)
          },
        },
        {
          ALT: () => {
            this.OPTION2(() => this.CONSUME(_CESQL_TOKENS.Not))
            this.CONSUME(_CESQL_TOKENS.Like)
            this.CONSUME(_CESQL_TOKENS.Text)
          },
        },
        {
          ALT: () => {
            this.OPTION3(() => this.CONSUME2(_CESQL_TOKENS.Not))
            this.CONSUME(_CESQL_TOKENS.In)
            this.SUBRULE(this.set)
          },
        },
      ])
    )
  })
  readonly set = this.RULE("set", () => {
    this.CONSUME(_CESQL_TOKENS.LParen)
    this.MANY_SEP({ SEP: _CESQL_TOKENS.Comma, DEF: () => this.SUBRULE(this.expression) })
    this.CONSUME(_CESQL_TOKENS.RParen)
  })
  readonly additive = this.RULE("additive", () => {
    this.SUBRULE(this.multiplicative)
    this.MANY(() => {
      this.CONSUME(_CESQL_TOKENS.Additive)
      this.SUBRULE2(this.multiplicative)
    })
  })
  readonly multiplicative = this.RULE("multiplicative", () => {
    this.SUBRULE(this.unary)
    this.MANY(() => {
      this.CONSUME(_CESQL_TOKENS.Multiplicative)
      this.SUBRULE2(this.unary)
    })
  })
  readonly unary = this.RULE("unary", () => {
    this.OR([
      {
        ALT: () => {
          this.CONSUME(_CESQL_TOKENS.Additive)
          this.SUBRULE(this.unary)
        },
      },
      {
        ALT: () => {
          this.CONSUME(_CESQL_TOKENS.Not)
          this.SUBRULE2(this.unary)
        },
      },
      {
        ALT: () => {
          this.CONSUME(_CESQL_TOKENS.Exists)
          this.CONSUME(_CESQL_TOKENS.Identifier)
        },
      },
      { ALT: () => this.SUBRULE(this.atom) },
    ])
  })
  readonly atom = this.RULE("atom", () => {
    this.OR([
      {
        ALT: () => {
          this.CONSUME(_CESQL_TOKENS.LParen)
          this.SUBRULE(this.expression)
          this.CONSUME(_CESQL_TOKENS.RParen)
        },
      },
      { ALT: () => this.SUBRULE(this.call) },
      { ALT: () => this.CONSUME2(_CESQL_TOKENS.Identifier) },
      { ALT: () => this.CONSUME(_CESQL_TOKENS.Integer) },
      { ALT: () => this.CONSUME2(_CESQL_TOKENS.Text) },
      { ALT: () => this.CONSUME(_CESQL_TOKENS.True) },
      { ALT: () => this.CONSUME(_CESQL_TOKENS.False) },
    ])
  })
  readonly call = this.RULE("call", () => {
    this.CONSUME3(_CESQL_TOKENS.Identifier)
    this.CONSUME2(_CESQL_TOKENS.LParen)
    this.MANY_SEP({ SEP: _CESQL_TOKENS.Comma, DEF: () => this.SUBRULE2(this.expression) })
    this.CONSUME2(_CESQL_TOKENS.RParen)
  })
}

const _cesqlParsed = (grammar: _CesqlGrammar, tokens: ReadonlyArray<IToken>): Either.Either<CstNode, CesqlFault> => {
  grammar.input = [...tokens]
  const tree = grammar.expression()
  const errors = [...grammar.errors]
  grammar.reset()
  return Array.isNonEmptyReadonlyArray(errors)
    ? Either.left(new CesqlFault({ case: { reason: "parse", stage: "parse", detail: Array.headNonEmpty(errors).message } }))
    : Either.right(tree)
}

const _cesqlCompiled = (
  source: string,
  grammar: _CesqlGrammar,
  lower: { readonly visit: (node: CstNode) => unknown },
): Either.Either<CesqlExpr, CesqlFault> =>
  source.length > _CESQL_CEILING.text
    ? Either.left(new CesqlFault({ case: { reason: "parse", stage: "text", detail: `${source.length} characters past ${_CESQL_CEILING.text}` } }))
    : pipe(_cesqlLexer.tokenize(source), (lexed) =>
      Array.isNonEmptyReadonlyArray(lexed.errors)
        ? Either.left(new CesqlFault({ case: { reason: "parse", stage: "lex", detail: Array.headNonEmpty(lexed.errors).message } }))
        : Either.flatMap(_cesqlParsed(grammar, lexed.tokens), (tree) =>
          Either.flatMap(
            Either.try({
              try: () => lower.visit(tree) as CesqlExpr,
              catch: (caught) => new CesqlFault({ case: { reason: "generic", detail: String(caught) } }),
            }),
            (expr) =>
              _cesqlDepth(expr) > _CESQL_CEILING.depth
                ? Either.left(new CesqlFault({ case: { reason: "parse", stage: "nesting", detail: `expression nests past depth ${_CESQL_CEILING.depth}` } }))
                : Either.right(expr),
          )))

const _cesqlDepth = (expr: CesqlExpr): number =>
  1 + CesqlExpr.$match(expr, {
    Attribute: () => 0,
    Binary: ({ left, right }) => Number.max(_cesqlDepth(left), _cesqlDepth(right)),
    Call: ({ operands }) => Number.max(0, ...Array.map(operands, _cesqlDepth)),
    Exists: () => 0,
    In: ({ operand, set }) => Number.max(_cesqlDepth(operand), ...Array.map(set, _cesqlDepth)),
    Like: ({ operand }) => _cesqlDepth(operand),
    Literal: () => 0,
    Logic: ({ left, right }) => Number.max(_cesqlDepth(left), _cesqlDepth(right)),
    Negate: ({ operand }) => _cesqlDepth(operand),
    Not: ({ operand }) => _cesqlDepth(operand),
  })

const _cesqlNodes = (children: CstChildrenDictionary, key: string): ReadonlyArray<CstNode> =>
  Array.filter(
    (children[key] ?? []) as ReadonlyArray<CstElement>,
    (element): element is CstNode => Predicate.hasProperty(element, "children"),
  )

const _cesqlTokens = (children: CstChildrenDictionary, ...keys: ReadonlyArray<string>): ReadonlyArray<IToken> =>
  Array.sort(
    Array.flatMap(keys, (key) =>
      Array.filter(
        (children[key] ?? []) as ReadonlyArray<CstElement>,
        (element): element is IToken => Predicate.hasProperty(element, "image"),
      )),
    Order.mapInput(Order.number, (token: IToken) => token.startOffset),
  )

const _cesqlInfix = (
  operands: ReadonlyArray<CesqlExpr>,
  operators: ReadonlyArray<IToken>,
  join: (op: string, left: CesqlExpr, right: CesqlExpr) => CesqlExpr,
): CesqlExpr =>
  Array.reduce(
    Array.drop(operands, 1),
    operands[0] ?? CesqlExpr.Literal({ value: _CESQL_ZERO.Boolean }),
    (left, right, index) => join(String.toUpperCase(operators[index]?.image ?? ""), left, right),
  )

const _cesqlLiteral = (token: IToken | undefined): string =>
  pipe(token?.image ?? "''", (image) => image.slice(1, -1).replaceAll(`\\${image[0] ?? "'"}`, image[0] ?? "'"))

const _cesqlLowering = (grammar: _CesqlGrammar): { readonly visit: (node: CstNode) => unknown } => {
  class _Lowering extends grammar.getBaseCstVisitorConstructor<never, CesqlExpr>() {
    constructor() {
      super()
      this.validateVisitor()
    }
    expression(children: CstChildrenDictionary): CesqlExpr {
      return _cesqlInfix(
        Array.map(_cesqlNodes(children, "conjunction"), (node) => this.visit(node)),
        _cesqlTokens(children, "Or", "Xor"),
        (op, left, right) => CesqlExpr.Logic({ op: op as Cesql.Logic, left, right }),
      )
    }
    conjunction(children: CstChildrenDictionary): CesqlExpr {
      return _cesqlInfix(
        Array.map(_cesqlNodes(children, "comparison"), (node) => this.visit(node)),
        _cesqlTokens(children, "And"),
        (_op, left, right) => CesqlExpr.Logic({ op: "AND", left, right }),
      )
    }
    comparison(children: CstChildrenDictionary): CesqlExpr {
      const operands = Array.map(_cesqlNodes(children, "additive"), (node) => this.visit(node))
      const operand = operands[0] ?? CesqlExpr.Literal({ value: _CESQL_ZERO.Boolean })
      const negated = Array.isNonEmptyReadonlyArray(_cesqlTokens(children, "Not"))
      const sets = _cesqlNodes(children, "set")
      const pattern = _cesqlTokens(children, "Text")
      const compare = _cesqlTokens(children, "Compare")
      return Array.isNonEmptyReadonlyArray(sets)
        ? CesqlExpr.In({
          operand,
          negated,
          set: Array.map(_cesqlNodes(Array.headNonEmpty(sets).children, "expression"), (node) => this.visit(node)),
        })
        : Array.isNonEmptyReadonlyArray(pattern)
        ? CesqlExpr.Like({ operand, negated, pattern: _cesqlPattern(_cesqlLiteral(Array.headNonEmpty(pattern))) })
        : Array.isNonEmptyReadonlyArray(compare)
        ? CesqlExpr.Binary({
          op: Array.headNonEmpty(compare).image as Cesql.Binary,
          left: operand,
          right: operands[1] ?? operand,
        })
        : operand
    }
    set(children: CstChildrenDictionary): CesqlExpr {
      return pipe(_cesqlNodes(children, "expression"), (members) =>
        Array.isNonEmptyReadonlyArray(members)
          ? this.visit(Array.headNonEmpty(members))
          : CesqlExpr.Literal({ value: _CESQL_ZERO.Boolean }))
    }
    additive(children: CstChildrenDictionary): CesqlExpr {
      return _cesqlInfix(
        Array.map(_cesqlNodes(children, "multiplicative"), (node) => this.visit(node)),
        _cesqlTokens(children, "Additive"),
        (op, left, right) => CesqlExpr.Binary({ op: op as Cesql.Binary, left, right }),
      )
    }
    multiplicative(children: CstChildrenDictionary): CesqlExpr {
      return _cesqlInfix(
        Array.map(_cesqlNodes(children, "unary"), (node) => this.visit(node)),
        _cesqlTokens(children, "Multiplicative"),
        (op, left, right) => CesqlExpr.Binary({ op: op as Cesql.Binary, left, right }),
      )
    }
    unary(children: CstChildrenDictionary): CesqlExpr {
      const inner = _cesqlNodes(children, "unary")
      const identifier = _cesqlTokens(children, "Identifier")
      return Array.isNonEmptyReadonlyArray(identifier)
        ? CesqlExpr.Exists({ name: String.toLowerCase(Array.headNonEmpty(identifier).image) })
        : Array.isNonEmptyReadonlyArray(inner)
        ? pipe(this.visit(Array.headNonEmpty(inner)), (operand) =>
          Array.isNonEmptyReadonlyArray(_cesqlTokens(children, "Not"))
            ? CesqlExpr.Not({ operand })
            : Array.headNonEmpty(_cesqlTokens(children, "Additive")).image === "-"
            ? CesqlExpr.Negate({ operand })
            : operand)
        : pipe(_cesqlNodes(children, "atom"), (atoms) =>
          Array.isNonEmptyReadonlyArray(atoms)
            ? this.visit(Array.headNonEmpty(atoms))
            : CesqlExpr.Literal({ value: _CESQL_ZERO.Boolean }))
    }
    atom(children: CstChildrenDictionary): CesqlExpr {
      const nested = _cesqlNodes(children, "expression")
      const called = _cesqlNodes(children, "call")
      const identifier = _cesqlTokens(children, "Identifier")
      const integer = _cesqlTokens(children, "Integer")
      const text = _cesqlTokens(children, "Text")
      const truth = _cesqlTokens(children, "True")
      const falsity = _cesqlTokens(children, "False")
      return Array.isNonEmptyReadonlyArray(nested)
        ? this.visit(Array.headNonEmpty(nested))
        : Array.isNonEmptyReadonlyArray(called)
        ? this.visit(Array.headNonEmpty(called))
        : Array.isNonEmptyReadonlyArray(identifier)
        ? CesqlExpr.Attribute({ name: String.toLowerCase(Array.headNonEmpty(identifier).image) })
        : Array.isNonEmptyReadonlyArray(integer)
        ? CesqlExpr.Literal({
          value: _cesqlCast(_cesqlRead(CesqlValue.String({ value: Array.headNonEmpty(integer).image })), "Integer").value,
        })
        : Array.isNonEmptyReadonlyArray(text)
        ? CesqlExpr.Literal({ value: CesqlValue.String({ value: _cesqlLiteral(Array.headNonEmpty(text)) }) })
        : CesqlExpr.Literal({
          value: CesqlValue.Boolean({ value: Array.isNonEmptyReadonlyArray(truth) && Array.isEmptyReadonlyArray(falsity) }),
        })
    }
    call(children: CstChildrenDictionary): CesqlExpr {
      return CesqlExpr.Call({
        name: _cesqlTokens(children, "Identifier")[0]?.image ?? "",
        operands: Array.map(_cesqlNodes(children, "expression"), (node) => this.visit(node)),
      })
    }
  }
  return new _Lowering()
}

class Cesql extends Effect.Service<Cesql>()("runtime/work/Cesql", {
  sync: () => {
    const grammar = new _CesqlGrammar()
    const lower = _cesqlLowering(grammar)
    return {
      compile: (spec: Filter.Spec): Either.Either<Filter.Compiled, CesqlFault> => _compiled(spec, grammar, lower, 1),
      admits: (filters: ReadonlyArray<Filter.Compiled>, envelope: CloudEventV1<unknown>): Filter.Verdict =>
        pipe(Array.map(filters, (filter) => filter.evaluate(envelope)), (readings) => ({
          admitted: Array.every(readings, (reading) => _cesqlFlag(reading)),
          faults: _cesqlCarried(readings),
        })),
    }
  },
  accessors: true,
}) {
  static readonly Fault = CesqlFault
  static readonly Spec = _Spec
  static readonly Value = CesqlValue
  static readonly dialects = _DIALECTS
  static readonly functions = _CESQL_FUNCTIONS
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Cesql, CesqlFault, CesqlValue }
export type { CesqlExpr, Filter }
```

## [07]-[RESEARCH]

(none)
