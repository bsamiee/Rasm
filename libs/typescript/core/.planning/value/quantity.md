# [CORE_QUANTITY]

`Quantity` is the sole SI value owner: its nested dimension algebra is total, and every partial magnitude operation lands on `Either<Quantity, Quantity.Fault>`. Module: `core/src/value/quantity.ts`.

## [01]-[QUANTITY_OWNER]

- `Quantity.Dimension` derives fields, symbols, basis vectors, and named dimensions from one ordered SI-axis vocabulary.
- `Quantity.Fault.family` closes `dimension`, `exponent`, and `range`; every reason classifies as `invalid` and retains the operands that produced it.
- SI admission happens before this owner; no display unit, non-finite magnitude, or rounded exponent enters the interior.

```typescript
import { Data, Either, Equal, pipe, Record, Schema } from "effect"
import { Fault } from "./fault.ts"
import { Shape } from "./schema.ts"

const _axisKinds = ["length", "mass", "time", "current", "temperature", "amount", "luminous"] as const
const _axisRows = {
  length: { symbol: "m" },
  mass: { symbol: "kg" },
  time: { symbol: "s" },
  current: { symbol: "A" },
  temperature: { symbol: "K" },
  amount: { symbol: "mol" },
  luminous: { symbol: "cd" },
} as const
const _axes = Shape.vocabulary(_axisKinds, _axisRows)

type _Axis = (typeof _axisKinds)[number]
type _Cells = { readonly [Axis in _Axis]: bigint }

const _cells = (project: (axis: _Axis) => bigint): _Cells =>
  Record.map(_axisRows, (_row, axis) => project(axis))
const _fields: { readonly [Axis in _Axis]: typeof Schema.BigIntFromNumber } =
  Record.map(_axisRows, () => Schema.BigIntFromNumber)

class _Dimension extends Schema.Class<_Dimension>("Quantity.Dimension")(_fields) {
  static readonly alike = Schema.equivalence(_Dimension)
  static readonly axes = _axes
  static readonly basis = (axis: _Axis): _Dimension => new _Dimension(_cells((held) => held === axis ? 1n : 0n))
  static readonly zipWith = (combine: (left: bigint, right: bigint) => bigint) =>
    (left: _Dimension, right: _Dimension): _Dimension =>
      new _Dimension(_cells((axis) => combine(left[axis], right[axis])))
  static readonly mapWith = (map: (exponent: bigint) => bigint) => (self: _Dimension): _Dimension =>
    new _Dimension(_cells((axis) => map(self[axis])))
  static readonly product = _Dimension.zipWith((left, right) => left + right)
  static readonly quotient = _Dimension.zipWith((left, right) => left - right)
  static readonly pow = (self: _Dimension, power: bigint): _Dimension =>
    _Dimension.mapWith((exponent) => exponent * power)(self)
  static readonly Scalar = new _Dimension(_cells(() => 0n))
  static readonly Length = _Dimension.basis("length")
  static readonly Mass = _Dimension.basis("mass")
  static readonly Time = _Dimension.basis("time")
  static readonly Current = _Dimension.basis("current")
  static readonly Temperature = _Dimension.basis("temperature")
  static readonly Amount = _Dimension.basis("amount")
  static readonly Luminous = _Dimension.basis("luminous")
  static readonly Area = _Dimension.pow(_Dimension.Length, 2n)
  static readonly Volume = _Dimension.pow(_Dimension.Length, 3n)
  static readonly Velocity = _Dimension.quotient(_Dimension.Length, _Dimension.Time)
  static readonly Acceleration = _Dimension.quotient(_Dimension.Velocity, _Dimension.Time)
  static readonly Force = _Dimension.product(_Dimension.Mass, _Dimension.Acceleration)
  static readonly Pressure = _Dimension.quotient(_Dimension.Force, _Dimension.Area)
  static readonly Energy = _Dimension.product(_Dimension.Force, _Dimension.Length)
  static readonly Power = _Dimension.quotient(_Dimension.Energy, _Dimension.Time)
  get scalar(): boolean {
    return _axes.kinds.every((axis) => this[axis] === 0n)
  }
  get symbol(): string {
    const terms = _axes.kinds.flatMap((axis) => this[axis] === 0n
      ? []
      : [this[axis] === 1n ? _axes.at(axis).symbol : `${_axes.at(axis).symbol}^${this[axis]}`])
    return terms.length === 0 ? "1" : terms.join("·")
  }
}

type _Operand = { readonly magnitude: number; readonly dimension: _Dimension }
type _Evidence = readonly [_Operand, ...ReadonlyArray<_Operand | bigint | number>]

const _reasonKinds = ["dimension", "exponent", "range"] as const
const _reasonRows = {
  dimension: { class: "invalid" },
  exponent: { class: "invalid" },
  range: { class: "invalid" },
} as const
const _family = Fault.Class.family(_reasonKinds, _reasonRows)

class _QuantityFault extends Data.TaggedError("QuantityFault")<{
  readonly reason: (typeof _reasonKinds)[number]
  readonly operands: _Evidence
}> {
  static readonly family = _family
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<quantity:${this.reason}> ${this.operands.map((operand) =>
      typeof operand === "object" ? `${operand.magnitude}:${operand.dimension.symbol}` : operand).join(" vs ")}`
  }
}

const _operand = (magnitude: number, dimension: _Dimension): _Operand => ({ magnitude, dimension })
const _fault = (reason: (typeof _reasonKinds)[number], head: _Operand, ...tail: ReadonlyArray<_Operand | bigint | number>) =>
  new _QuantityFault({ reason, operands: [head, ...tail] })
const _admit = (magnitude: number, dimension: _Dimension, evidence: _Evidence): Either.Either<Quantity, _QuantityFault> =>
  Number.isFinite(magnitude)
    ? Either.right(new Quantity({ magnitude, dimension }))
    : Either.left(new _QuantityFault({ reason: "range", operands: evidence }))

class Quantity extends Schema.Class<Quantity>("Quantity")({
  magnitude: Schema.Number.pipe(Schema.finite()),
  dimension: _Dimension,
}) {
  static readonly Dimension = _Dimension
  static readonly Fault = _QuantityFault
  static readonly alike = Schema.equivalence(Quantity)
  static readonly of = (magnitude: number, dimension: _Dimension): Either.Either<Quantity, _QuantityFault> =>
    _admit(magnitude, dimension, [_operand(magnitude, dimension)])
  static readonly negate = (self: Quantity): Quantity =>
    new Quantity({ magnitude: -self.magnitude, dimension: self.dimension })
  static readonly sum = (left: Quantity, right: Quantity): Either.Either<Quantity, _QuantityFault> =>
    Equal.equals(left.dimension, right.dimension)
      ? _admit(left.magnitude + right.magnitude, left.dimension, [
          _operand(left.magnitude, left.dimension),
          _operand(right.magnitude, right.dimension),
        ])
      : Either.left(_fault(
          "dimension",
          _operand(left.magnitude, left.dimension),
          _operand(right.magnitude, right.dimension),
        ))
  static readonly difference = (left: Quantity, right: Quantity): Either.Either<Quantity, _QuantityFault> =>
    Equal.equals(left.dimension, right.dimension)
      ? _admit(left.magnitude - right.magnitude, left.dimension, [
          _operand(left.magnitude, left.dimension),
          _operand(right.magnitude, right.dimension),
        ])
      : Either.left(_fault(
          "dimension",
          _operand(left.magnitude, left.dimension),
          _operand(right.magnitude, right.dimension),
        ))
  static readonly product = (left: Quantity, right: Quantity): Either.Either<Quantity, _QuantityFault> =>
    _admit(left.magnitude * right.magnitude, _Dimension.product(left.dimension, right.dimension), [
      _operand(left.magnitude, left.dimension),
      _operand(right.magnitude, right.dimension),
    ])
  static readonly quotient = (left: Quantity, right: Quantity): Either.Either<Quantity, _QuantityFault> =>
    _admit(left.magnitude / right.magnitude, _Dimension.quotient(left.dimension, right.dimension), [
      _operand(left.magnitude, left.dimension),
      _operand(right.magnitude, right.dimension),
    ])
  static readonly scale = (self: Quantity, factor: number): Either.Either<Quantity, _QuantityFault> =>
    _admit(self.magnitude * factor, self.dimension, [_operand(self.magnitude, self.dimension), factor])
  static readonly pow = (self: Quantity, power: bigint): Either.Either<Quantity, _QuantityFault> =>
    pipe(Number(power), (exponent) => Number.isFinite(exponent) && BigInt(exponent) === power
      ? _admit(self.magnitude ** exponent, _Dimension.pow(self.dimension, power), [
          _operand(self.magnitude, self.dimension),
          power,
        ])
      : Either.left(_fault("exponent", _operand(self.magnitude, self.dimension), power)))
  static readonly ratio = (left: Quantity, right: Quantity): Either.Either<number, _QuantityFault> =>
    Either.flatMap(Quantity.quotient(left, right), (measure) => measure.scalar
      ? Either.right(measure.magnitude)
      : Either.left(_fault(
          "dimension",
          _operand(left.magnitude, left.dimension),
          _operand(right.magnitude, right.dimension),
        )))
  get scalar(): boolean {
    return this.dimension.scalar
  }
}

namespace Quantity {
  export type Axis = _Axis
  export type Cells = _Cells
  export type Dimension = _Dimension
  export type Fault = _QuantityFault
  export type Operand = _Operand
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Quantity }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
