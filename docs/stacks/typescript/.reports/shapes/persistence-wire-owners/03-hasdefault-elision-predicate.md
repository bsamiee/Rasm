# HasDefault Elision Predicate: One Terminal Combinator Mints Every Constructor-Optional Key

[ONE_COMBINATOR_MINTS_THE_PREDICATE]:
- The discriminant slot and every generative slot are not two constructs that happen to share a flag — they are two call sites of ONE terminal pipe step. `tag` is `Literal(t).pipe(propertySignature, withConstructorDefault(() => t))` and the auto-stamp slot is `transformOrFail(from, Union(Undefined, to)).pipe(propertySignature, withConstructorDefault(constructorDefault ?? constUndefined))` — both end in `withConstructorDefault`, and that combinator is typed `<…boolean, R>(self) => <…true, R>`: it takes a `PropertySignature` with ANY `HasDefault` and returns one whose `HasDefault` is the literal `true`, regardless of every other type parameter. The predicate is not declared on the slot; it is the output type of the function that builds the slot.
- `withConstructorDefault` is the sole producer of a `HasDefault=true` signature reachable by a field author — `tag`, `withDefaults`, the class-tag stamp `getClassTag`, and `VariantSchema.Overrideable` all route through it, none reimplementing the flip. A bespoke constructor-elidable column is therefore not a new marker but the same pipe: `Schema.propertySignature(leaf).pipe(Schema.withConstructorDefault(() => seed))` yields a slot the construct surface treats identically to `tag` and `Overrideable`, with zero new machinery.

```typescript
import { Schema } from 'effect';

class Owner extends Schema.Class<Owner>('Owner')({
    kind: Schema.tag('Owner'),
    seq: Schema.propertySignature(Schema.Number).pipe(Schema.withConstructorDefault(() => 0)),
    label: Schema.NonEmptyTrimmedString,
}) {}

const built = Owner.make({ label: '<value-a>' });
type Ctor = ConstructorParameters<typeof Owner>[0];
```

- `Owner.make({ label })` omits BOTH `kind` and `seq`: the `tag` slot and the hand-built `withConstructorDefault` slot are byte-identical to the construct surface — both `HasDefault=true` — so the one author-reachable combinator collapses "discriminant" and "generative default" to the same constructor-optionality, and `label` (a bare schema) is the lone required key. Reject the parallel spelling `kind: Schema.optional(Schema.Literal('Owner'))`: an `optional` slot is a `"?:"` signature with `HasDefault=false`, elidable for a different reason and absent on the wire — the tag must be present-on-encode, which only `withConstructorDefault` delivers.

[THE_HASDEFAULT_PREDICATE_IS_THE_ONLY_CATCH_FOR_A_COLON_TOKEN_SLOT]:
- The two elision predicates partition by ORTHOGONAL axes of the same signature: `OptionalTypePropertySignature` reads the TypeToken (`"?:"`, `HasDefault` free) and `PropertySignatureWithDefault` reads `HasDefault` (`true`, Token free). Because `withConstructorDefault` returns a `":"` TypeToken signature, every slot it builds is INVISIBLE to the TypeToken predicate — `tag` and `Overrideable` both carry `TypeToken=":"`, so the HasDefault predicate is the ONLY arm that elides them. A `"?:"` slot is constructor-optional AND encoded-optional; a `":"`-with-default slot is constructor-optional but encoded-MANDATORY — the divergence in arity between the two surfaces traces to which of the two orthogonal tokens each mapped type reads.
- This is why a defaulted column cannot be expressed as `Schema.optional` and vice versa: `optional` flips TypeToken to `"?:"` and drops the column from the encoded body, fine for a truly absent value; `withConstructorDefault` keeps TypeToken `":"` so the value is encoded-required and present-on-encode, the only shape a discriminant or an auto-stamp can take. Choosing `optional` for a tag would erase the literal from the wire; choosing `withConstructorDefault` for a genuinely absent field would force a default no encode wants — the two predicates are not interchangeable spellings of "optional."

[THE_PREDICATE_IS_FIXED_THE_SLOT_SET_IS_NOT]:
- The per-construct asymmetry the owner exhibits is one predicate applied over DIFFERENT field maps, never different predicates per surface. The class construct param `C` is `Struct.Constructor<ExtractFields<"select", Fields, true>>` — the SELECT projection alone — while each write `.make` carries `Struct.Constructor<ExtractFields<V, Fields, false>>` over its own variant slots. The HasDefault predicate is identical in all three; what differs is which slot holds a `HasDefault=true` signature in that variant's extraction.
- The class constructor elides ONLY the tag, because `select` is the one variant where the discriminant carries `withConstructorDefault` and every other column is a bare schema or a plain transform — a `Generated` id sits in `select` as its raw branded leaf (mandatory), a `DateTimeUpdate` column sits in `select` as plain `Schema.DateTimeUtc` (mandatory). The same column's INSERT slot is the `Overrideable` (`HasDefault=true`, elided) — so the identical predicate flips the identical column from mandatory-at-`new` to optional-at-`insert.make`, the variant deciding which slot the generative signature occupies.

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    kind: Schema.tag('Owner'),
    revisedAt: Model.DateTimeUpdate,
    label: Schema.NonEmptyTrimmedString,
}) {}

type ReadCtor = ConstructorParameters<typeof Owner>[0];
type InsertCtor = Parameters<typeof Owner.insert.make>[0];
type UpdateCtor = Parameters<typeof Owner.update.make>[0];
```

- `ReadCtor` requires `id`, `revisedAt`, `label` and elides `kind` alone — `id`'s select slot is the bare `Schema.Number`, `revisedAt`'s select slot is the plain `DateTimeUtc`, both falling to the mandatory arm; only `kind` carries `HasDefault=true` in `select`. `InsertCtor` elides `kind` AND `revisedAt` (whose insert slot is the `Overrideable`) AND drops `id` outright (membership, not elision). `UpdateCtor` re-requires `id` (membership rejoins) yet still elides `revisedAt` (its update slot is also the `Overrideable`) — three constructor shapes, one predicate, the slot population alone driving the divergence with no per-variant rule authored.

[THE_DEFAULT_THUNK_SHAPES_THE_ELIDED_KEY_TYPE]:
- The predicate decides ELISION; the default THUNK decides the elided key's TYPE, and the two are independent. `Constructor` reads `Schema.Type<F[H]>` for the optional arm, which is the signature's first variance parameter — for `tag` that is `Tag` (the literal, no `undefined`), for `Overrideable` that is `(To & Brand<"Override">) | undefined`. The tag's default thunk is `() => t` (always the literal), so its key is `kind?: Tag`; the auto-stamp's default is `constUndefined` (`() => undefined`), and because the slot schema is `Union(Undefined, to)`, its key is `stamp?: (To & Brand<"Override">) | undefined`. Same predicate elides both; the discriminant cannot be set to `undefined` while the stamp explicitly can.
- `constUndefined` is the load-bearing default precisely because it makes `generate(none)` fire at encode: an omitted key constructs as `undefined`, the `Undefined` arm of the union decodes through `generate` to compute the persisted value against `R`. A `tag` whose thunk is the literal never reaches a `generate` — its encode emits the literal directly. The same elidable slot therefore carries two distinct encode behaviors: a constant fill versus an effectful mint, the thunk identity being the entire difference.

[HASDEFAULT_ELIDES_THE_CONSTRUCTOR_NEVER_THE_ENCODED_KEY]:
- The construct-optional key is a REQUIRED encoded key — the value is present on the wire and in the store, the constructor merely supplies it by default. `Struct.Encoded<F>` computes its optional keys from `EncodedOptionalKeys`, which matches `OptionalEncodedPropertySignature` = a `"?:"` ENCODED token; both `tag` (`EncodedToken=":"`) and `Overrideable` (`EncodedToken=":"`) are mandatory encoded keys. So `Schema.Schema.Encoded<typeof Owner.insert>` carries the stamped column as a present key while `Parameters<typeof Owner.insert.make>[0]` elides it — the construct surface and the encoded surface diverge because one mapped type reads `HasDefault` and the other reads `EncodedToken`, three orthogonal token slots (`TypeToken`, `EncodedToken`, `HasDefault`) on one `PropertySignature` each driving a different projection.
- This is the proof that elision is a CONSTRUCTION convenience, not a representational drop: a membership-erased column is gone from the encoded type entirely (no key), whereas a HasDefault-elided column is absent from the constructor payload yet present-and-required on the wire — the default fires to fill it at encode. A consumer cannot infer from a missing constructor key that the column is missing on the store; it infers only that the value is auto-supplied unless forced.

[REQUIREDKEYS_NEVER_COLLAPSES_THE_PAYLOAD_TO_VOID]:
- `RequiredKeys<T> = { [K in keyof T]-?: {} extends Pick<T, K> ? never : K }[keyof T]` is the same predicate inverted to count what survives: an optional key `[H in K]?:` makes `{}` assignable to `Pick<T, K>`, resolving that key to `never`; only a mandatory key keeps `{}` non-assignable and survives. `make`'s signature is `RequiredKeys<C> extends never ? void | Simplify<C> : Simplify<C>` — when EVERY constructor key is HasDefault-elided or `"?:"`-optional, `RequiredKeys` is `never` and the payload collapses to `void | C`, an argument-free `.make()`.
- ONE bare-schema slot flips the branch back to the mandatory payload — a single non-defaulted column makes `RequiredKeys` non-`never`, demanding the full object. This is the boundary of the elision law: a write variant whose every column is generative, optional, or tagged constructs with no argument, but the first plain leaf re-imposes a required payload, the predicate over the slot set toggling the entire `make` arity with no flag.

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

const NodeId = Schema.Uint8ArrayFromSelf.pipe(Schema.brand('NodeId'));

class Stamp extends Model.Class<Stamp>('Stamp')({
    id: Model.UuidV4Insert(NodeId),
    kind: Schema.tag('Stamp'),
    createdAt: Model.DateTimeInsert,
}) {}

const minted = Stamp.insert.make();
type InsertRequired = Parameters<typeof Stamp.insert.make>[0];
```

- `Stamp.insert.make()` takes NO argument: every insert-variant slot is HasDefault-elided — `id`'s insert overrideable mints the UUID, `kind`'s tag fills the literal, `createdAt`'s insert overrideable stamps the time — so `RequiredKeys<C>` resolves `never` and `InsertRequired` is `void | …`. Adding one `label: Schema.NonEmptyTrimmedString` column flips `RequiredKeys` non-`never` and breaks the bare `.make()` at compile time, the absorbed-growth proof: the predicate over the widened slot set re-derives the arity, no call site respelled by hand.

[THE_NESTED_STRUCT_CROSSES_FROM_CONSTRUCTOR_TO_TYPE]:
- The elision predicate is consulted by `Struct.Constructor` and IGNORED by `Struct.Type` — and a nested struct crosses precisely that boundary, which is the true mechanism behind the construct trap, not a generic "non-recursion." A nested fragment projects as a `Schema.Struct` field whose value is a plain `Schema.Schema` (neither `"?:"` nor `HasDefault=true`), so the parent's `Constructor` routes it to the MANDATORY arm and embeds `Schema.Type<nested>`, NOT `Struct.Constructor<nested>`. `Struct.Type` routes a `PropertySignatureWithDefault` slot to its OWN mandatory arm — only the `"?:"` TypeToken goes optional in `Type` — so the nested overrideable, elidable in a `Constructor`, is a REQUIRED key typed `(To & Brand<"Override">) | undefined` inside the parent's view of the fragment.
- The fix is the inverse of the top-level elision: where a parent slot is OMITTED to fire its default, a nested slot must be EXPLICITLY passed `undefined` to fire the same default, because the parent embeds the nested `Type` (HasDefault unread) rather than the nested `Constructor` (HasDefault elides). The predicate never threads into the fragment's constructor; it stops at the parent keyset, the nested fragment arriving as a `Type` whose defaulted slots are mandatory-but-`| undefined`. This is the membership/construct asymmetry sharpened: membership recurses through `extract`, the constructor-elision does not, because nesting swaps the `Constructor` projection for the `Type` projection.

```typescript
import { Model } from '@effect/sql';
import { Option, Schema } from 'effect';

const Audit = Model.Struct({
    createdAt: Model.DateTimeInsert,
    revision: Model.Generated(Schema.Number),
});

class Owner extends Model.Class<Owner>('Owner')({
    label: Schema.NonEmptyTrimmedString,
    note: Model.FieldOption(Schema.String),
    audit: Audit,
}) {}

const stored = Owner.insert.make({ label: '<value-a>', note: Option.none(), audit: { createdAt: undefined } });
```

- `Owner.insert.make` omits no top-level overrideable — there is none at the parent — yet must spell `audit: { createdAt: undefined }`: `audit` is a mandatory parent key (a `Schema.Struct` field, neither `"?:"` nor defaulted), and the parent embeds the fragment's `Schema.Type`, where the nested `DateTimeInsert` overrideable is a REQUIRED key typed `(DateTime.Utc & Brand<"Override">) | undefined` because `Type` does not read `HasDefault`. Reject `audit: {}` — it fails the required nested `createdAt`; the explicit `undefined` is what reaches the overrideable's `Undefined` arm to auto-stamp, the elision living in the `Constructor` projection the parent never invokes for a nested struct.

[BRAND_OVERRIDE_IS_THE_TYPE_ONLY_OPT_OUT_OF_THE_OPT_OUT]:
- `Override = value => value` is the runtime identity; its `& Brand<"Override">` is erased at runtime and exists solely to make the elidable key's type `(To & Brand<"Override">) | undefined` admit a forced value. The HasDefault predicate makes the key optional (auto-supply); the brand on the key's TYPE makes a NON-default value rejected unless it carries the phantom brand — so a write `.make` has exactly three spellings for an overrideable slot: omit (default fires), pass `undefined` (default fires, the explicit form needed only nested), or pass `Model.Override(x)` (the branded forced value). An unbranded `x` is a type error at the key — the only construct in the system where the optionality and the value-shape are two independent gates on one slot.
- The brand is type-only because the runtime needs no marker: `generate` receives `Option.some(value)` when ANY non-`undefined` reaches the slot and `Option.none()` when `undefined` does, so the runtime discriminates by presence, not by brand. The brand exists only to stop a caller passing a raw value that would silently bypass the generative path's intent — it makes the forced-value path a deliberate, compiler-checked opt-out of the auto-default, never an accident. Forcing the value does NOT discharge the slot's `R`: the codec still declares the `generate` effect's requirement whether the default fires or not, so `Override` bypasses the elidable key but never the requirement the slot's schema carries.
