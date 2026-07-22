# [CORE_QUANTITY]

The SI quantity law: a physical measure is one `Quantity` — SI-coherent magnitude plus a seven-axis `Dimension` exponent vector — canonicalized exactly once at the C# admission and carried dimension-checked everywhere in TS. A `{value, unit}` shape never exists in this branch: unit conversion happened before the wire, so the interchange codec decodes the C# `QuantityFamily` SI scalar straight into this owner, and every downstream fold reads magnitudes it can lawfully combine. Dimension algebra is total, magnitude algebra is honestly partial — a dimension mismatch, a non-finite result, or a power the JavaScript scalar plane cannot represent exactly is a typed `QuantityFault` on the `Either` rail, never a `NaN` or silently rounded exponent escaping into a report. The module is `core/src/value/quantity.ts`; a new named dimension is one static row, a new operation is one member on the owning class.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                         | [PUBLIC]                    |
| :-----: | :----------------- | :------------------------------------------------------------- | :-------------------------- |
|  [01]   | `DIMENSION_VECTOR` | the SI base-exponent vector, its algebra, the named rows       | `Dimension`                 |
|  [02]   | `QUANTITY_ALGEBRA` | magnitude+dimension carrier and its `Either`-railed arithmetic | `Quantity`, `QuantityFault` |

## [02]-[DIMENSION_VECTOR]

[DIMENSION_VECTOR]:
- Owner: `Dimension`, a `Schema.Class` over the seven SI base exponents — `length` (m), `mass` (kg), `time` (s), `current` (A), `temperature` (K), `amount` (mol), `luminous` (cd) — with structural `Equal` from the declaration, so dimension matching is `Equal.equals` and a dimension keys containers with zero ceremony.
- Law: exponents are decoded by `Schema.BigIntFromNumber` and carried as `bigint` — the C# numeric wire stays unchanged, while `product`, `quotient`, and `pow` remain total under arbitrary composition with no JavaScript-number overflow, `NaN`, or fractional exponent path; exponent bounds belong to the wire shape, never the interior algebra.
- Law: the axis family has ONE authority — the `_axes` key tuple anchors order and membership, the `_rows` table carries each axis's symbol, and everything else derives: the class field record computes as `Record.map(_rows, () => Schema.BigIntFromNumber)` under a mapped annotation, `_cells` generates a full exponent record from one axis projection, the class-carried `zipWith`/`mapWith` kernels and the `basis` unit-vector mint are its one-line specializations, and the guard pair closes tuple and table on each other — so vector membership, symbols, zero construction, admission, and both algebraic projections read one declaration and no parallel axis enumeration exists to drift.
- Law: the kernels ride the class, not the module — a derived static (`Area`, `Force`) constructs during class evaluation, when only the inner class binding is initialized, so a module-scope kernel closing over the outer `Dimension` binding is a load-time `ReferenceError` waiting on the first composed row; `zipWith`/`mapWith`/`basis` capture the inner binding and are themselves public algebra members.
- Law: the named rows ride the class as statics in derivation order — seven `basis` rows plus `Scalar`, then rows composed from earlier rows (`Area = pow(Length, 2)`, `Force = product(Mass, Acceleration)`) — so the vocabulary is provably consistent with the algebra that derives it and a new named dimension is one static line.
- Law: `pow` takes `bigint`, so fractional powers are absent from the signature; a root (`m² → m`) is an `Option`-returning row on this owner admitted only with an exact divisibility proof across every axis.
- Law: `alike` is the class-derived `Schema.equivalence` — dimension keys, quantity deduplication, and arithmetic guards consume the same structural relations as decode.
- Law: `symbol` renders diagnostics only — `m·s^-1`, `1` for scalar — a display-unit rendering (`km/h`, `psi`) is a consumer-side projection over the SI value and never a floor concern.
- Growth: a new named dimension is one static row composed from existing rows; a new base axis is a physical-constants change and lands as one `_axes` entry plus one `_rows` symbol row — fields, kernels, zero, admission, and rendering all derive.
- Boundary: unit families, conversion factors, and non-SI admission live in C# (`Rasm.Compute/Symbolic`); TS receives SI-coherent values and this owner never converts.
- Packages: `effect` (`Schema`, `Record`).

```typescript
import { Data, Either, Equal, pipe, Record, Schema } from "effect"

const _axes = ["length", "mass", "time", "current", "temperature", "amount", "luminous"] as const // SI base-axis order: rendering and every generated vector read this ONE anchor
const _rows = {
  length: { symbol: "m" },
  mass: { symbol: "kg" },
  time: { symbol: "s" },
  current: { symbol: "A" },
  temperature: { symbol: "K" },
  amount: { symbol: "mol" },
  luminous: { symbol: "cd" },
} as const

const _cells = (project: (axis: Dimension.Axis) => bigint): Dimension.Cells => Record.map(_rows, (_row, axis) => project(axis))

const _fields: { readonly [A in Dimension.Axis]: typeof Schema.BigIntFromNumber } = Record.map(_rows, () => Schema.BigIntFromNumber) // the field record derives: a new axis admits without an eighth field spelling

class Dimension extends Schema.Class<Dimension>("Dimension")(_fields) {
  static readonly alike = Schema.equivalence(Dimension)
  // the vector kernels live on the class so derived statics construct through the inner class binding during evaluation, never the outer TDZ binding
  static readonly basis = (axis: Dimension.Axis): Dimension => new Dimension(_cells((held) => (held === axis ? 1n : 0n)))
  static readonly zipWith = (combine: (left: bigint, right: bigint) => bigint) => (left: Dimension, right: Dimension): Dimension =>
    new Dimension(_cells((axis) => combine(left[axis], right[axis])))
  static readonly mapWith = (map: (exponent: bigint) => bigint) => (self: Dimension): Dimension =>
    new Dimension(_cells((axis) => map(self[axis])))
  static readonly product = Dimension.zipWith((left, right) => left + right)
  static readonly quotient = Dimension.zipWith((left, right) => left - right)
  static readonly pow = (self: Dimension, power: bigint): Dimension => Dimension.mapWith((exponent) => exponent * power)(self)
  static readonly Scalar = new Dimension(_cells(() => 0n))
  static readonly Length = Dimension.basis("length")
  static readonly Mass = Dimension.basis("mass")
  static readonly Time = Dimension.basis("time")
  static readonly Current = Dimension.basis("current")
  static readonly Temperature = Dimension.basis("temperature")
  static readonly Amount = Dimension.basis("amount")
  static readonly Luminous = Dimension.basis("luminous")
  static readonly Area = Dimension.pow(Dimension.Length, 2n)
  static readonly Volume = Dimension.pow(Dimension.Length, 3n)
  static readonly Velocity = Dimension.quotient(Dimension.Length, Dimension.Time)
  static readonly Acceleration = Dimension.quotient(Dimension.Velocity, Dimension.Time)
  static readonly Force = Dimension.product(Dimension.Mass, Dimension.Acceleration)
  static readonly Pressure = Dimension.quotient(Dimension.Force, Dimension.Area)
  static readonly Energy = Dimension.product(Dimension.Force, Dimension.Length)
  static readonly Power = Dimension.quotient(Dimension.Energy, Dimension.Time)
  get scalar(): boolean {
    return _axes.every((axis) => this[axis] === 0n)
  }
  get symbol(): string {
    const parts = _axes.flatMap((axis) =>
      this[axis] === 0n ? [] : [this[axis] === 1n ? _rows[axis].symbol : `${_rows[axis].symbol}^${this[axis]}`],
    )
    return parts.length === 0 ? "1" : parts.join("·")
  }
}

declare namespace Dimension {
  type Axes = typeof _axes
  type Axis = keyof typeof _rows // anchored on the row table; the guard pair below closes tuple and table on each other
  type Cells = { readonly [A in Axis]: bigint }
  type Row = { readonly symbol: string }
  type Contract = { readonly [A in Axes[number]]: Row }
  type _Rows<T extends Contract = typeof _rows> = T // row guard: a tuple axis missing its symbol row fails at declaration
  type _Keys<K extends keyof Contract = Axis> = K // key guard: an excess table row fails here — closure in both directions
}
```

## [03]-[QUANTITY_ALGEBRA]

[QUANTITY_ALGEBRA]:
- Owner: `Quantity`, a `Schema.Class` of finite `magnitude` plus `Dimension` — the class is the decoded interior value, the validator, the constructor, and the derivation root under one name; the interchange codec decodes the C# SI scalar into it and compute-adjacent folds combine it lawfully.
- Law: arithmetic rides one `Either<Quantity, QuantityFault>` rail — `of` refuses a non-finite magnitude, `sum`/`difference` refuse a dimension mismatch, `product`/`quotient`/`scale` refuse overflow through the same `of` gate, `pow` refuses a `bigint` exponent unless `Number(power)` round-trips exactly through `BigInt`, and `ratio` refuses a non-scalar quotient — so partiality folds once at the consumer and no `NaN`, `Infinity`, silently rounded exponent, or unit error travels the interior.
- Law: `QuantityFault` carries evidence as data — `reason` (`"dimension"` | `"exponent"` | `"range"`) plus both operand dimensions — and derives `message` from fields; it is the in-process `Data.TaggedError` altitude, and a quantity fault crossing a wire is re-spelled by the owning surface's fault family.
- Law: `negate` is total and `difference` derives as `sum` with a negated operand — one guard kernel, zero duplicated dimension checks.
- Law: `new Quantity`/`Quantity.make` are the trusted-construction channel and re-prove finiteness; `of` is the domain entry every operation routes through, so the throwing constructor is never reached with an unproven magnitude.
- Law: ordering never crosses dimensions — consumers prove dimension alignment (`ratio`, `Equal.equals` on `dimension`) and then compare magnitudes; a total `Order<Quantity>` forges cross-dimension comparability and is the rejected instance.
- Growth: a new operation is one static composing `of` and the `Dimension` algebra; a new fault cause is one `reason` row, never a second error class.
- Boundary: SI canonicalization happens once at C# admission; display conversion, unit symbols beyond SI diagnostics, and locale-aware formatting are `ui` projections over the SI value.
- Packages: `effect` (`Schema`, `Either`, `Data`, `Equal`, `pipe`).

```typescript
class QuantityFault extends Data.TaggedError("QuantityFault")<{
  readonly reason: "dimension" | "exponent" | "range"
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
  static readonly alike = Schema.equivalence(Quantity)
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
  static readonly pow = (self: Quantity, power: bigint): Either.Either<Quantity, QuantityFault> =>
    pipe(Number(power), (exponent) =>
      Number.isFinite(exponent) && BigInt(exponent) === power
        ? Quantity.of(self.magnitude ** exponent, Dimension.pow(self.dimension, power))
        : Either.left(new QuantityFault({ reason: "exponent", left: self.dimension, right: self.dimension })))
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
