# Constructor-Type Widening Law

[ACCRUAL_ALGEBRA]:
- The constructor-parameter type `C` is the fifth `Class<Self, Fields, I, R, C, Inherited, Proto>` slot, and every widening verb accrues it by the SAME rewrite `C & Struct.Constructor<NewFields>` — `.extend`, `.transformOrFail`, and `.transformOrFailFrom` all return `Class<…, C & Struct.Constructor<NewFields>, Self, Proto>`, so the constructor obligation is a left-fold of struct-constructor intersections down the inheritance chain, never a re-derivation from the merged `Fields & NewFields` record.
- The accrual is intersection, not the field-merge `Struct.Constructor<Fields & NewFields>` — and the two are NOT interchangeable: `Struct.Constructor<F>` is itself a `Types.UnionToIntersection<{ [K in keyof F]: … }[keyof F]>`, so `Struct.Constructor<A> & Struct.Constructor<B>` and `Struct.Constructor<A & B>` coincide as a type yet the implementation folds the per-step intersection because each `.extend` only HAS `NewFields` in scope, not the historical `Fields`; the chain accrues what each step contributes, the parent's `C` already carrying the prior steps.
- The intersection is the reason a parent's optionality survives widening untouched: an optional-or-defaulted parent key contributes `{ readonly K?: T }` to `C`, the subclass contributes `Struct.Constructor<NewFields>`, and `{ readonly K?: T } & X` keeps `K` optional — so a subclass never silently re-requires a key the parent had relieved, the optionality riding the parent factor of the intersection.

[REQUIRED_SET_MONOTONICITY]:
- The void-ability of every owner is read off one type predicate, `RequiredKeys<C> extends never`, where `RequiredKeys<T> = { [K in keyof T]-?: {} extends Pick<T, K> ? never : K }[keyof T]` — a key counts as required exactly when `{}` is NOT assignable to its single-key pick, which holds precisely for a key that is neither `?:`-optional nor `_HasDefault`-backed; the props argument is `void`-able when this set is `never` and a hard `Simplify<C>` otherwise.
- The required set is monotone-up under accrual but recomputed per owner: intersecting `Struct.Constructor<NewFields>` can only ADD keys to `C`, so `RequiredKeys` either stays `never` (every accrued key optional-or-defaulted) or gains the new required keys — a widening can flip an owner from void-able to props-demanding but never the reverse, because no `.extend` step removes a key from the intersection.
- The flip is local to the subclass type and invisible to the parent: the parent constructor signature is still `new (props: void | Simplify<C_parent>)`, only the subclass's `new (props: Simplify<C_parent & Struct.Constructor<{ required }>>)` demands the object — so adding a required field to an all-defaulted owner is a loud compile break at every `new Sub()` / `Sub.make()` call site while every `new Parent()` keeps type-checking, the widening surfacing as a leaf-local error, never a silent admission and never a parent-wide cascade.

```typescript
import { Schema } from 'effect';

class Anchor extends Schema.Class<Anchor>('Anchor')({
    rank: Schema.Number.pipe(Schema.propertySignature, Schema.withConstructorDefault(() => 0)),
}) {}
const a = new Anchor(); // void-able: RequiredKeys<Struct.Constructor<{ rank }>> extends never

// a defaulted addition PRESERVES void-ability — the next case lands with zero call-site churn
class Stamped extends Anchor.extend<Stamped>('Stamped')({
    label: Schema.optionalWith(Schema.String, { default: () => '<auto>' }),
}) {}
const s = new Stamped(); // still void-able: C accrued only optional-or-defaulted factors

// a REQUIRED addition flips THIS owner to props-demanding; Anchor and Stamped stay void-able
class Keyed extends Stamped.extend<Keyed>('Keyed')({
    id: Schema.String,
}) {}
const k = new Keyed({ id: '<x>' }); // every `new Keyed()` site breaks loudly, none silently admits
const parentUntouched = new Stamped(); // the widening never reaches up the chain
```

[DUPLICATE_THROW_EAGERNESS]:
- A parent-versus-new key collision is a hard `throw new Error("Duplicate property signature" / "Duplicate key \"<k>\"")` raised inside `extendFields(a, b)` — `for (const key of Reflect.ownKeys(b)) if (key in a) throw …` — and `extendFields` runs BEFORE `makeClass`, so the throw fires at class-DEFINITION (module-evaluation) time, never at instantiation: an `class Sub extends Parent.extend<Sub>("Sub")({ collidingKey })` that overlaps a parent field cannot be loaded, the offending module crashes on import rather than on first `new Sub`.
- The eager throw covers every widening verb identically because all three route through `extendFields(fields, newFields)`: `.extend`, `.transformOrFail`, and `.transformOrFailFrom` each compute `extendFields` first and only then build the schema — so a field-name collision is a uniform definition-time crash whether the widening is a plain struct merge or an effectful transform, the throw deciding before any decode/encode function is even wired.
- The collision check is `Reflect.ownKeys`, covering string AND symbol keys, so a symbol-keyed field colliding across the chain throws the same as a string key — the total-or-fail rule is over the full own-key set, not the enumerable string subset.

```typescript
import { Effect, Schema } from 'effect';

class Base extends Schema.Class<Base>('Base')({ slot: Schema.Number }) {}

// reject: a collision is NOT a silent parent override and NOT a lazy per-instance fault — it crashes
// at module load. extendFields throws on `"slot" in a` before makeClass is ever called:
//   class Dup extends Base.extend<Dup>('Dup')({ slot: Schema.String }) {}  // throws at import
//   class TDup extends Base.transformOrFail<TDup>('TDup')(
//       { slot: Schema.String }, { decode, encode },                       // same throw, same eagerness
//   ) {}
// widening is total or it fails loudly at definition time; the loose alternative — a runtime
// `assertNoOverlap(parentFields, newFields)` guard at each subclass — re-derives this eager check by hand.
class Widened extends Base.extend<Widened>('Widened')({ note: Schema.String }) {}
const w = new Widened({ slot: 1, note: '<x>' });
void Effect;
```

[REPRESENTATION_DIVERGENCE_ON_SHARED_ACCRUAL]:
- The three widening verbs share one `C` accrual yet diverge on the encoded face `I`, and the divergence is the choice of which representation the new fields are derived from: `.extend` widens `I & Struct.Encoded<NewFields>` (the new fields gain a wire shape), `.transformOrFail` keeps `I` UNCHANGED (the new fields are computed from the parent's decoded type and never serialize), and `.transformOrFailFrom` also keeps the outer `I` but feeds its `decode` the encoded side `Simplify<I>` yielding `I & Struct.Encoded<NewFields>` internally — so a computed field added by `transformOrFail` accrues into `C` (it is constructible) yet stays absent from the owner's encoded record.
- This is the asymmetry the constructor type cannot show alone: `Struct.Constructor<NewFields>` joins `C` for all three, but a `transformOrFail`-added field is constructor-supplied-or-defaulted while being wire-invisible, so `Schema.Schema.Encoded<typeof Sub>` does not gain it — the field participates in `new`/`make` but is reconstructed by the transform's `decode` on the admission path, never read from input bytes; reading the widening law off `C` alone reports constructibility, reading it off `I` reports serializability, and the two diverge exactly at the effectful-transform verb.
- `transformOrFail` and `transformOrFailFrom` additionally accrue `R | Struct.Context<NewFields> | R2 | R3` — the two transform-function requirements join the owner's `R`, so a context-dependent widening surfaces its requirement on the owner's own channel and is provided once at the composition root; `.extend` accrues only `R | Struct.Context<NewFields>`, having no transform functions, so the requirement growth is itself a discriminant of which verb widened.

```typescript
import { Effect, Schema } from 'effect';

class Raw extends Schema.Class<Raw>('Raw')({ text: Schema.String }) {}

// transformOrFail: `length` accrues into C (constructible) but NOT into I (wire-invisible) —
// one widening row, the computed field reconstructed at decode, every Raw consumer untouched.
class Sized extends Raw.transformOrFail<Sized>('Sized')(
    { length: Schema.Number },
    {
        decode: (parent) => Effect.succeed({ text: parent.text, length: parent.text.length }),
        encode: (self) => Effect.succeed({ text: self.text }),
    },
) {
    get isEmpty(): boolean {
        return this.length === 0;
    }
}
type SizedWire = Schema.Schema.Encoded<typeof Sized>; // { text: string } — `length` never serializes
const built = new Sized({ text: '<x>', length: 1 }); // C accrued `length`: constructible
const admit = Schema.decodeUnknown(Sized); // reconstructs `length` from `text`, not from input bytes
```

[MAKE_INDEPENDENCE_PER_SUBCLASS]:
- `make` is a `this`-polymorphic static `make<C extends new (...args) => any>(this: C, ...args: ConstructorParameters<C>)`, and the load-bearing subtlety is that `ConstructorParameters<C>` resolves against the STATIC receiver type `C`, not the base where `make` is physically defined — so the single inherited `make` is NOT frozen at the base's parameter list: `Sub.make` binds `C = typeof Sub` and reads `Sub`'s own widened constructor parameters, while `Parent.make` binds `C = typeof Parent`, the same physical member typing differently per call expression.
- This is why widening needs no per-subclass `make` re-declaration: the constructor type `RequiredKeys<C>` gate is the ONE source the `this`-polymorphic `make` reflects through `ConstructorParameters`, so an accrual flip breaks `new Sub` and `Sub.make` in lockstep with zero duplicated parameter spelling — the rejected shape is a hand-written `static makeSub(props): Sub` per subclass, which re-states the constructor parameters the polymorphic static already derives and drifts the moment a field's modifier changes.

```typescript
import { Schema } from 'effect';

class Loose extends Schema.Class<Loose>('Loose')({
    flag: Schema.optionalWith(Schema.Boolean, { default: () => false }),
}) {}
const lm = Loose.make(); // `this = typeof Loose`: ConstructorParameters void-able

class Tight extends Loose.extend<Tight>('Tight')({ key: Schema.String }) {}
const tm = Tight.make({ key: '<x>' }); // SAME inherited `make`, `this = typeof Tight`: now props-demanding
const lm2 = Loose.make(); // Loose.make stays void-able — the polymorphic static re-resolves per receiver
```

[ANTICIPATORY_PROOF_FROM_ONE_TYPE]:
- The correct-shape proof is the diff of the next field, read entirely off `C`: a field added as `withConstructorDefault`/`optional`/`optionalWith({default})` contributes an OPTIONAL factor to the accrued intersection, leaving `RequiredKeys<C>` and thus every call site untouched; a field added bare contributes a REQUIRED factor, flipping `RequiredKeys<C>` non-empty and breaking every `new`/`make` site at compile time — so the modifier algebra on the new field, not a separate migration pass, decides whether the widening is silent-compatible or a loud break, and the decision is recoverable from the one type.
- Sizing an owner all-defaulted is the anticipatory shape: a void-able base absorbs defaulted additions with zero call-site churn and converts the first required addition into a checker-chased break at exactly the sites that must now supply it — the rejected order is sizing the base with a required field, then relieving it later, because relieving a required key after call sites exist leaves every existing `new Self(props)` over-supplying a now-optional key while the type permits omission, drifting the call sites apart from the owner; widening before call sites exist is one intersection factor, widening after is N call-site reconciliations.
- The `_tag` factor is permanently optional in the accrual: `TaggedClass` seeds `C = Struct.Constructor<Omit<Fields, "_tag">>` and `_tag` is `withConstructorDefault`-backed (`PropertySignatureWithDefault`), so even the generic `.extend` return — which intersects `Struct.Constructor<NewFields>` onto that already-`_tag`-stripped `C` — never re-admits `_tag` into `RequiredKeys`; supplying `_tag` to a subclass constructor is a type error at the subclass exactly as at the base, the tag input-forbidden through every widening step without the `extend` signature needing to re-thread the `Omit`.
