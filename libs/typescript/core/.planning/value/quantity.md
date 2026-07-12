# [CORE_QUANTITY]

The SI quantity law: a physical measure is one `Quantity` — SI-coherent magnitude plus a seven-axis `Dimension` exponent vector — canonicalized exactly once at the C# admission and carried dimension-checked everywhere in TS. A `{value, unit}` shape never exists in this branch: unit conversion happened before the wire, so the interchange codec decodes the C# `QuantityFamily` SI scalar straight into this owner, and every downstream fold reads magnitudes it can lawfully combine. Dimension algebra is total, magnitude algebra is honestly partial — a dimension mismatch or a non-finite product is a typed `QuantityFault` on the `Either` rail, never a `NaN` escaping into a report. The module is `core/src/value/quantity.ts`; a new named dimension is one static row, a new operation is one member on the owning class.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                         | [PUBLIC]                    |
| :-----: | :----------------- | :------------------------------------------------------------- | :-------------------------- |
|  [01]   | `DIMENSION_VECTOR` | the SI base-exponent vector, its algebra, the named rows       | `Dimension`                 |
|  [02]   | `QUANTITY_ALGEBRA` | magnitude+dimension carrier and its `Either`-railed arithmetic | `Quantity`, `QuantityFault` |

## [02]-[DIMENSION_VECTOR]

[DIMENSION_VECTOR]:
- Owner: `Dimension`, a `Schema.Class` over the seven SI base exponents — `length` (m), `mass` (kg), `time` (s), `current` (A), `temperature` (K), `amount` (mol), `luminous` (cd) — with structural `Equal` from the declaration, so dimension matching is `Equal.equals` and a dimension keys containers with zero ceremony.
- Law: exponents are unbounded `Schema.Int` — `product`, `quotient`, and `pow` stay total under composition, and no mid-computation construction can trip a range filter; the wire's own bounds are the wire shape's business.
- Law: the algebra is two interior kernels — `_folded` zips two vectors under one combine, `_mapped` rescales one vector under one map — and every public operation is a one-line projection of them, so a new operation is one member, never an eighth field spelling.
- Law: the named rows ride the class as statics in derivation order — seven base rows plus `Scalar`, then rows composed from earlier rows (`Area = pow(Length, 2)`, `Force = product(Mass, Acceleration)`) — so the vocabulary is provably consistent with the algebra that derives it and a new named dimension is one static line.
- Law: `pow` truncates its exponent to an integer — fractional powers produce non-integer exponents the field type refuses, and a root (`m² → m`) is a future `Option`-returning row on this owner, admitted only with an all-even-exponent proof.
- Law: `symbol` renders diagnostics only — `m·s^-1`, `1` for scalar — a display-unit rendering (`km/h`, `psi`) is a consumer-side projection over the SI value and never a floor concern.
- Growth: a new named dimension is one static row composed from existing rows; a new base axis is a physical-constants change and lands as one field plus one `_folded`/`_mapped` line each.
- Boundary: unit families, conversion factors, and non-SI admission live in C# (`Rasm.Compute/Symbolic`); TS receives SI-coherent values and this owner never converts.
- Packages: `effect` (`Schema`).

```typescript
import { Data, Either, Equal, Schema } from "effect"

const _AXES = ["length", "mass", "time", "current", "temperature", "amount", "luminous"] as const
const _SYMBOLS = { length: "m", mass: "kg", time: "s", current: "A", temperature: "K", amount: "mol", luminous: "cd" } as const
const _BASE = { length: 0, mass: 0, time: 0, current: 0, temperature: 0, amount: 0, luminous: 0 } as const

const _folded = (combine: (left: number, right: number) => number) => (left: Dimension, right: Dimension): Dimension =>
  new Dimension({
    length: combine(left.length, right.length),
    mass: combine(left.mass, right.mass),
    time: combine(left.time, right.time),
    current: combine(left.current, right.current),
    temperature: combine(left.temperature, right.temperature),
    amount: combine(left.amount, right.amount),
    luminous: combine(left.luminous, right.luminous),
  })

const _mapped = (map: (exponent: number) => number) => (self: Dimension): Dimension =>
  new Dimension({
    length: map(self.length),
    mass: map(self.mass),
    time: map(self.time),
    current: map(self.current),
    temperature: map(self.temperature),
    amount: map(self.amount),
    luminous: map(self.luminous),
  })

class Dimension extends Schema.Class<Dimension>("Dimension")({
  length: Schema.Int,
  mass: Schema.Int,
  time: Schema.Int,
  current: Schema.Int,
  temperature: Schema.Int,
  amount: Schema.Int,
  luminous: Schema.Int,
}) {
  static readonly product = _folded((left, right) => left + right)
  static readonly quotient = _folded((left, right) => left - right)
  static readonly pow = (self: Dimension, power: number): Dimension => _mapped((exponent) => exponent * Math.trunc(power))(self)
  static readonly Scalar = new Dimension({ ..._BASE })
  static readonly Length = new Dimension({ ..._BASE, length: 1 })
  static readonly Mass = new Dimension({ ..._BASE, mass: 1 })
  static readonly Time = new Dimension({ ..._BASE, time: 1 })
  static readonly Current = new Dimension({ ..._BASE, current: 1 })
  static readonly Temperature = new Dimension({ ..._BASE, temperature: 1 })
  static readonly Amount = new Dimension({ ..._BASE, amount: 1 })
  static readonly Luminous = new Dimension({ ..._BASE, luminous: 1 })
  static readonly Area = Dimension.pow(Dimension.Length, 2)
  static readonly Volume = Dimension.pow(Dimension.Length, 3)
  static readonly Velocity = Dimension.quotient(Dimension.Length, Dimension.Time)
  static readonly Acceleration = Dimension.quotient(Dimension.Velocity, Dimension.Time)
  static readonly Force = Dimension.product(Dimension.Mass, Dimension.Acceleration)
  static readonly Pressure = Dimension.quotient(Dimension.Force, Dimension.Area)
  static readonly Energy = Dimension.product(Dimension.Force, Dimension.Length)
  static readonly Power = Dimension.quotient(Dimension.Energy, Dimension.Time)
  get scalar(): boolean {
    return _AXES.every((axis) => this[axis] === 0)
  }
  get symbol(): string {
    const parts = _AXES.flatMap((axis) =>
      this[axis] === 0 ? [] : [this[axis] === 1 ? _SYMBOLS[axis] : `${_SYMBOLS[axis]}^${this[axis]}`],
    )
    return parts.length === 0 ? "1" : parts.join("·")
  }
}
```

## [03]-[QUANTITY_ALGEBRA]

[QUANTITY_ALGEBRA]:
- Owner: `Quantity`, a `Schema.Class` of finite `magnitude` plus `Dimension` — the class is the decoded interior value, the validator, the constructor, and the derivation root under one name; the interchange codec decodes the C# SI scalar into it and compute-adjacent folds combine it lawfully.
- Law: arithmetic rides one `Either<Quantity, QuantityFault>` rail — `of` refuses a non-finite magnitude, `sum`/`difference` refuse a dimension mismatch, `product`/`quotient`/`scale`/`pow` refuse overflow through the same `of` gate, and `ratio` refuses a non-scalar quotient — so partiality folds once at the consumer and no `NaN`, `Infinity`, or silent unit error travels the interior.
- Law: `QuantityFault` carries evidence as data — `reason` (`"dimension"` | `"range"`) plus both operand dimensions — and derives `message` from fields; it is the in-process `Data.TaggedError` altitude, and a quantity fault crossing a wire is re-spelled by the owning surface's fault family.
- Law: `negate` is total and `difference` derives as `sum` with a negated operand — one guard kernel, zero duplicated dimension checks.
- Law: `new Quantity`/`Quantity.make` are the trusted-construction channel and re-prove finiteness; `of` is the domain entry every operation routes through, so the throwing constructor is never reached with an unproven magnitude.
- Law: ordering never crosses dimensions — consumers prove dimension alignment (`ratio`, `Equal.equals` on `dimension`) and then compare magnitudes; a total `Order<Quantity>` forges cross-dimension comparability and is the rejected instance.
- Growth: a new operation is one static composing `of` and the `Dimension` algebra; a new fault cause is one `reason` row, never a second error class.
- Boundary: SI canonicalization happens once at C# admission; display conversion, unit symbols beyond SI diagnostics, and locale-aware formatting are `ui` projections over the SI value.
- Packages: `effect` (`Schema`, `Either`, `Data`, `Equal`).

```typescript
class QuantityFault extends Data.TaggedError("QuantityFault")<{
  readonly reason: "dimension" | "range"
  readonly left: Dimension
  readonly right: Dimension
}> {
  override get message(): string {
    return `<${this.reason}> ${this.left.symbol} vs ${this.right.symbol}`
  }
}

class Quantity extends Schema.Class<Quantity>("Quantity")({
  magnitude: Schema.Number.pipe(Schema.finite()),
  dimension: Dimension,
}) {
  static readonly of = (magnitude: number, dimension: Dimension): Either.Either<Quantity, QuantityFault> =>
    Number.isFinite(magnitude)
      ? Either.right(new Quantity({ magnitude, dimension }))
      : Either.left(new QuantityFault({ reason: "range", left: dimension, right: dimension }))
  static readonly negate = (self: Quantity): Quantity =>
    new Quantity({ magnitude: -self.magnitude, dimension: self.dimension })
  static readonly sum = (left: Quantity, right: Quantity): Either.Either<Quantity, QuantityFault> =>
    Equal.equals(left.dimension, right.dimension)
      ? Quantity.of(left.magnitude + right.magnitude, left.dimension)
      : Either.left(new QuantityFault({ reason: "dimension", left: left.dimension, right: right.dimension }))
  static readonly difference = (left: Quantity, right: Quantity): Either.Either<Quantity, QuantityFault> =>
    Quantity.sum(left, Quantity.negate(right))
  static readonly product = (left: Quantity, right: Quantity): Either.Either<Quantity, QuantityFault> =>
    Quantity.of(left.magnitude * right.magnitude, Dimension.product(left.dimension, right.dimension))
  static readonly quotient = (left: Quantity, right: Quantity): Either.Either<Quantity, QuantityFault> =>
    Quantity.of(left.magnitude / right.magnitude, Dimension.quotient(left.dimension, right.dimension))
  static readonly scale = (self: Quantity, factor: number): Either.Either<Quantity, QuantityFault> =>
    Quantity.of(self.magnitude * factor, self.dimension)
  static readonly pow = (self: Quantity, power: number): Either.Either<Quantity, QuantityFault> =>
    Quantity.of(self.magnitude ** Math.trunc(power), Dimension.pow(self.dimension, power))
  static readonly ratio = (left: Quantity, right: Quantity): Either.Either<number, QuantityFault> =>
    Either.flatMap(Quantity.quotient(left, right), (measure) =>
      measure.scalar
        ? Either.right(measure.magnitude)
        : Either.left(new QuantityFault({ reason: "dimension", left: left.dimension, right: right.dimension })),
    )
  get scalar(): boolean {
    return this.dimension.scalar
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Dimension, Quantity, QuantityFault }
```
