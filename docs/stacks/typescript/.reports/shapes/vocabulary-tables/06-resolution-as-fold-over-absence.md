# Resolution As Fold Over Absence

[RESOLUTION_WRITES_A_SLOT_VECTOR_ADMISSION_READS_IT]:
- Admission folds absence onto an external rail — a `ParseError`, an `Option.None`, a residual type — and the fold's codomain is OUTSIDE the vocabulary; resolution folds the same absence back to a designated member and its codomain is the closed key union itself, so the resolved value re-enters every projection, order, and dispatch with no second gate. The two folds share one input (the missing key) and differ only in where they land, which is why a vocabulary owner carries both without a second source.
- The resolution fold is not a runtime branch but a write to the property signature's state vector: `PropertySignature<TypeToken, Type, Key, EncodedToken, Encoded, HasDefault, R>` carries `TypeToken` and `EncodedToken` over `"?:" | ":"`, the `HasDefault` boolean, and the requirement `R`, and each resolution combinator writes a specific orthogonal subset of those four slots — the fold's effect IS the slot transition, recoverable from the signature type alone.
- `withConstructorDefault(() => member)` writes ONLY `HasDefault: false → true` and leaves both tokens untouched (`PropertySignature<TT, Type, K, ET, Enc, true, R>`): the fallback fires at the validating constructor's `make` path, so `Struct.Constructor` marks the field optional-on-make (`RequiredKeys<...> extends never ? void | ...`) while the decode side still demands the wire key — the constructor-omittable property is recovered from the `HasDefault` slot, never a token flip.
- `withDecodingDefault(() => member)` writes ONLY the `TypeToken: "?:" → ":"` slot and applies `Exclude<Type, undefined>`, requiring the source signature to already be `("?:", "?:", false)`: the fallback fires at decode, so a missing wire key resolves to the member while `make` still demands it — the dual slot of the constructor default, the two folds writing two disjoint coordinates of one vector.
- `withDefaults({ constructor, decoding })` writes BOTH slots in one declaration (`(":", Exclude<Type, undefined>, ..., true)`) and the two thunks stay separate because the make-time omitted-field meaning and the wire-time missing-key meaning are distinct events: a builder may default to one member and a wire to another, and the slot vector is the only place that distinction is recorded.

```typescript
import { Schema } from 'effect'

const Vocab = Schema.Literal('<a>', '<b>', '<c>')

// each combinator writes a disjoint slot of the signature; the resolved fold is recoverable from the field type
const onMake = Schema.optional(Vocab).pipe(Schema.withConstructorDefault((): '<a>' => '<a>')) // PropertySignature<"?:", _, _, "?:", _, true, never>: make omits, decode demands
const onWire = Schema.optional(Vocab).pipe(Schema.withDecodingDefault((): '<a>' => '<a>')) // PropertySignature<":", '<a>'|'<b>'|'<c>', _, "?:", _, false, never>: decode resolves, make demands
const onBoth = Schema.optional(Vocab).pipe(Schema.withDefaults({ constructor: (): '<a>' => '<a>', decoding: (): '<b>' => '<b>' })) // both slots; the two events resolve to different members
// REJECT Schema.withDecodingDefault over a required signature: the source must be ("?:","?:",false); the optional token is the precondition the fold consumes
```

[THE_UNDEFINED_STRIP_IS_THE_SAME_EXCLUDE_THAT_SUBTRACTS_A_MEMBER]:
- The Type a defaulted decode produces is `Exclude<Type, undefined>`, written in the combinator signature, not a coincidence of inference: the resolution fold over absence and the residual subtraction a partial dispatch performs are one `Exclude` operator under two readings — the dispatch subtracts handled members toward `never`, the decode default subtracts the `undefined` arm toward the closed union, both proving the absence case discharged at the type level.
- The strip is total exactly when the source `Type` is `Vocab | undefined` and the fold removes the `undefined` arm, leaving the bare vocabulary union: `optionalWith(Vocab, { default })`'s Type adds `undefined` only when `Types.Has<Options, "as" | "default" | "exact">` is false, so the presence of `default` is precisely the condition that the `Exclude<…, undefined>` fires and the decoded field is a closed member with no `undefined` arm — the consumer reads the union, never widens to admit the absence it cannot receive.
- `exact: true` removes a value FROM the trigger set rather than from the result: without it an explicit `undefined` on the wire is treated as missing and resolves to the default, with it only an absent key resolves and an explicit `undefined` fails decode — the fold's domain (what counts as absence) is a decode knob, where the strip (what leaves the result) is the combinator's fixed `Exclude`.
- A `Schema.optional(Vocab)` whose Type is `Vocab | undefined` reaching a downstream `Match.exhaustive` keyed on the vocabulary forces an `undefined` arm the alphabet does not own; the decoding default is the fold that deletes that arm before dispatch, so the resolution and the dispatch totality are the same `Exclude`-toward-closure read at two stages — resolve the absence to a member, then the exhaustive matcher subtracts members to `never` over a value with no `undefined` left to handle.

[THE_FALLBACK_IS_CHECKED_AGAINST_THE_ALPHABET_AT_THE_DECLARATION]:
- The fold's codomain is constrained to the vocabulary's own type so the designated member cannot escape the alphabet: every default thunk is typed `() => NoInfer<Exclude<Type, undefined>>`, the `NoInfer` forbidding the thunk's literal from WIDENING the field's inferred type and the `Exclude` forbidding it from resolving back to `undefined` — a fallback off the alphabet fails at the field declaration, the same site the `satisfies` table rejects a forgotten column.
- The `.annotations({ default })` form is METADATA, not an active fold, and conflating it with `optionalWith({ default })` is the trap: `DefaultAnnotation<A>` is the bare type `A` read back through `getDefaultAnnotation` as `Option<unknown>`, so it neither flips a token, strips `undefined`, nor resolves a missing key at decode or make — the annotated default feeds the JSON-schema and arbitrary projections only, while the field stays required and a missing key still fails. The decode-active default lives on the property signature's `defaultValue`, which `withConstructorDefault`/`withDecodingDefault` write, not on the schema's annotation map.
- A resolution needing a member NOT in the alphabet is the signal the alphabet is wrong, never that the default should widen: the member is added as a vocabulary row first — one literal in the `Schema.Literal` tuple or one row in the `as const` table — then named as the default, so the alphabet and its designated fallback grow from the one owner and the next fallback lands as a literal the checker already admits.
- The `NoInfer` guard is what makes the fold composable across many fields: a struct of defaulted vocabulary fields each names its own member, and no field's thunk leaks its literal into a sibling's inferred type, so the resolution writes per-field with zero cross-field widening — the struct is N independent folds onto N closed unions, each proved at its own declaration.

[CONSTANT_FALLBACK_VS_CONTEXT_READING_FOLD]:
- The default thunk is a `LazyArg` of NO arguments — it cannot read the surrounding decoded value, a sibling field, or a service — so `optionalWith({ default })` is the resolution whose fallback is a CONSTANT member of the alphabet; the moment the resolved member depends on other decoded fields, a policy value, or a requirement, the thunk's nullary shape is the ceiling and the fold lifts to `optionalToRequired`.
- `optionalToRequired(From, To, { decode, encode })` is the deepest resolution: `decode` receives `Option<FA>` where `Option.none` IS the missing key, so the body is `Option.match({ onNone: () => member, onSome: (v) => transform(v) })` — a true fold over absence whose `onNone` arm computes the fallback rather than naming a constant, and whose `onSome` arm may rewrite a present value, both folding into the resolved `To` member.
- The fold's `onNone` arm reads context the thunk cannot because the transform runs inside the schema's decode, so a fallback derived from another field, a `Config`, or a service surfaces through the body's closure — the resolution that reads context is the fold, the resolution that does not is the thunk, and the choice between them is whether `onNone` needs an argument, not a flag on one combinator.
- The token result is fixed `PropertySignature<":", TA, never, "?:", FI, false, FR | TR>`: required Type, optional Encoded, `HasDefault` false, and `R` unioning BOTH schemas' requirements — so a fold whose `onNone` reaches a service surfaces that requirement in the field's `R`, the same `R`-accumulation a struct extension performs, where the constant thunk's `FR | TR` collapses to `never` because no transform reaches a context.

```typescript
import { Option, Schema } from 'effect'

const Wire = Schema.Literal('<a>', '<b>') // narrow wire alphabet
const Domain = Schema.Literal('<a>', '<b>', '<fallback>') // wider domain alphabet carrying a member the wire never sends

// the fold's onNone computes a domain-only member from absence; onSome lifts a present wire member into the domain union
const resolved = Schema.optionalToRequired(Wire, Domain, {
    decode: Option.match({ onNone: (): '<fallback>' => '<fallback>', onSome: (v): '<a>' | '<b>' => v }),
    encode: (m): Option.Option<'<a>' | '<b>'> => (m === '<fallback>' ? Option.none() : Option.some(m)), // the seam default round-trips to a dropped key
})
// REJECT Schema.optionalWith(Domain, { default: () => '<fallback>' }) when the fallback must read a sibling field or service: the nullary thunk has no argument to read it from
```

[OPTIONAL_TO_OPTIONAL_RE_EMITS_THE_ABSENCE]:
- The fold family is three combinators discriminated by where absence enters and exits: `optionalToRequired` is absence-in/member-out (`(Option<FA>) => TI`, total resolution), `requiredToOptional` is member-in/absence-out (`(FA) => Option<TI>`, conditional drop), and `optionalToOptional` is absence-in/absence-out (`(Option<FA>) => Option<TI>`, the fold that may resolve a present key, drop a present key, or leave a missing key missing) — one owner, three absence dispositions selected by the transform's `Option` arity, not three parallel surfaces.
- `optionalToOptional` is the resolution that re-emits absence as a decision: its `decode` returning `Option.none` for a present input DROPS the key from the decoded value, and returning `Option.some(member)` for a `none` input RESOLVES it — so a vocabulary that promotes some members and elides others (a deprecated alias dropped, a sentinel resolved to a member) folds in one transform where a constant default could only resolve and never drop.
- The tokens partition the three folds exactly: `optionalToRequired` is `(":", "?:")` — the resolved key is required in the domain, optional on the wire; `requiredToOptional` is `("?:", ":")` — present on the wire, droppable in the domain; `optionalToOptional` is `("?:", "?:")` — optional on both sides, the only fold whose domain field may still be absent, so a downstream read of it stays an `Option` rather than a resolved member.
- The fold staying in `Option` on both sides is the form that COMPOSES with the carry disposition rather than the resolve disposition: where `optionalToRequired` lands a member every consumer reads unconditionally, `optionalToOptional` lands an `Option` the consumer must still match, so the absence is threaded one more hop rather than discharged at the field — the choice is whether the field IS the resolution point or only a transform in a longer absence chain.

[THE_AS_OPTION_DISPOSITION_CARRIES_RATHER_THAN_RESOLVES]:
- `optionalWith(Vocab, { as: "Option" })` is the SAME owner as the defaulting form selecting the opposite disposition: `default` resolves absence to a member and writes `(":", member, HasDefault: true)`, `as: "Option"` lifts the field into `Option<Type>` and writes `(":", Option<Type>, HasDefault: false)` — the field's Type becomes `Option.Option<Schema.Type<S>>`, so the missing key is CARRIED as `Option.None` into the domain rather than folded to a member, the carry-vs-resolve choice expressed as one mutually-exclusive option key on one combinator.
- The two dispositions are exclusive in the option type itself: `OptionalOptions` is a union where the `default` arm forbids `as` (`as?: never`) and every `as: "Option"` arm forbids `default` (`default?: never`), so a single field is either a resolution point or a carry point and the checker rejects requesting both — the absence has one destination per field, written into the option shape, not reconciled at runtime.
- `as: "Option"` carries `onNoneEncoding` — `LazyArg<Option<undefined>>` (or `Option<null | undefined>` under `nullable`) — to control the ENCODE-side absence: a domain `Option.None` either omits the key or writes an explicit `null`/`undefined`, so the carry disposition still owns its wire round-trip where the resolve disposition's encode simply re-emits the resolved member; the encoding fold is the dual of the decoding fold, and `onNoneEncoding` is its `onNone` arm.
- The disposition selects the downstream algebra: a `default` field flows a closed member into the rank-order, the group-equivalence, and the function-column dispatch with no further match, while an `as: "Option"` field flows an `Option` that each consumer threads through `Option.map`/`Option.match` — so a vocabulary whose absence is genuinely meaningful (a not-yet-set tier) carries, and one whose absence has a canonical meaning (an implied default tier) resolves, the same owner serving both by the option key.

```typescript
import { Option, Schema } from 'effect'

const Tier = Schema.Literal('<a>', '<b>', '<c>')
const Row = Schema.Struct({
    resolved: Schema.optionalWith(Tier, { default: (): '<a>' => '<a>', nullable: true }), // Type '<a>'|'<b>'|'<c>': absence folds to a member, no Option downstream
    carried: Schema.optionalWith(Tier, { as: 'Option', onNoneEncoding: () => Option.some(undefined) }), // Type Option<'<a>'|'<b>'|'<c>'>: absence carried; onNoneEncoding writes the key explicitly on encode
})
// the option type forbids both keys at once; a field is a resolution point OR a carry point, never both, the exclusion proved at the declaration
```

[GET_OR_ELSE_WIDENS_BECAUSE_THE_FALLBACK_IS_THE_SECOND_OPERAND_OF_A_UNION]:
- The widening trap is precisely that `B`, the fallback type, is a FREE type variable solved against the `onNone` thunk in ISOLATION from `A`: `getOrElse` declares `<B>(onNone: LazyArg<B>): <A>(self) => B | A` with no constraint tying `B` to `A`, so the checker infers `B` from the thunk's body alone — a fallback expression the checker widens to `string` (any non-`const` string operation, a parameter, a non-literal) solves `B = string`, and `B | A = string | Key = string`, collapsing the resolved value to `string` and silently escaping the closed union the `Some` arm preserved.
- The widening is structural, not a quirk of `getOrElse`: any resolution whose fallback type is inferred separately from the present type unions the two, so `B | A` collapses to `A` only when `B` is constrained to be `A` — annotating the thunk `(): Key => '<member>'` forces `B = Key`, and `Key | Key = Key` re-closes the union, the annotation doing the work the codec's `NoInfer<Type>` thunk does automatically on the schema path.
- Resolving to the fallback ROW instead of a member avoids the trap by construction: `Option.getOrElse(() => Table[fallbackKey])` reads a row of type `(typeof Table)[keyof typeof Table]`, the same type the `Some` arm carries, so `B | A` is `Row | Row = Row` with no annotation needed — reading the closed source on the `onNone` arm keeps both arms at one type, where a fresh literal on the `onNone` arm is the operand that escapes.
- The schema fold cannot exhibit this trap because its fallback is checked against the alphabet by the combinator's typed thunk, while the structural read's fallback is an untyped value the inference widens — so the codec-backed resolution is widening-safe by the owner's constraint and the structural resolution is widening-safe only by annotating the thunk to the key type or reading the fallback row from the table, the two paths reaching closure by different levers over the one alphabet.

[STAYING_IN_OPTION_DEFERS_THE_WIDENING_TO_ONE_TERMINAL]:
- The widening fires at the boundary where `Option<A>` becomes `A`, so a resolution that stays inside `Option` across many candidate fallbacks defers the single widening point to one terminal: `Option.orElse(onNone: LazyArg<Option<B>>): Option<A | B>` chains alternative resolutions while keeping the carrier, and only the final `getOrElse` collapses `Option<Key>` to `Key` — the intermediate `orElse` steps union inside the `Option`, the closure proved once at the exit.
- A cascade of vocabulary-key fallbacks is `Option.firstSomeOf(collection)`: `<T>(collection: Iterable<Option<T>>) => Option<T>` folds a sequence of candidate resolutions to the FIRST present member, so a key resolved through a primary lookup, then a secondary, then a designated default is one fold over an `Iterable<Option<Key>>` whose element type pins `T = Key` — the multi-candidate resolution is a single combinator, not a nested `orElse` ladder, and its result stays `Option<Key>` until the terminal closes it.
- The element type of the `firstSomeOf` collection is the union of every candidate's `Some` type, so every candidate `Option<Key>` keeps the fold's result `Option<Key>` while one candidate `Option<WiderThing>` widens the whole fold — the cascade is closure-safe exactly when every candidate resolves within the same alphabet, the multi-candidate analogue of the single `getOrElse` thunk's annotation.
- The deferred-widening form is what lets the structural read compose with the codec gate without two absence rails: a foreign key admitted through `Schema.decodeUnknownOption(Schema.keyof(Table))` is already `Option<Key>`, so it chains directly into `Option.orElse(() => Record.get(Table, fallbackRaw))` and `firstSomeOf` over further candidates, the admission `Option` and the resolution `Option` being one carrier the terminal `getOrElse` closes — the gate and the resolution share the rail rather than each owning a separate one.

```typescript
import { Option, Record, pipe } from 'effect'

const Table = { '<a>': { rank: 0 }, '<b>': { rank: 1 }, '<c>': { rank: 2 } } as const
type Key = keyof typeof Table

// each candidate keeps Option<Key> via Option.as replacing the matched row with the requested literal key; firstSomeOf pins T = Key, so the cascade stays closed until one terminal getOrElse
const candidate = (k: Key): Option.Option<Key> => Option.as(Record.get(Table, k), k)
const resolveKey = (primary: Key, secondary: Key): Key =>
    pipe(Option.firstSomeOf([candidate(primary), candidate(secondary)]), Option.getOrElse((): Key => '<a>'))
const resolveRow = (gated: Option.Option<(typeof Table)[Key]>) => pipe(gated, Option.getOrElse(() => Table['<a>'])) // B | A = Row | Row = Row; reading the fallback row from the table needs no annotation
// REJECT Option.getOrElse(() => '<a>') on Option<Key> without the (): Key annotation: a fallback the checker widens infers B = string, and the result string | Key = string, defeating every downstream exhaustiveness on the resolved key
```
