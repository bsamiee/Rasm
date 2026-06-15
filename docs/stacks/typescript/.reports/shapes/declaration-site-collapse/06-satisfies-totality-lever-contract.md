# Satisfies totality-lever contract

[TOTALITY_THESIS]:
- `satisfies T` is the only lever that proves THREE properties of a literal in one clause where each alternative proves at most two: COVERAGE (every required member of `T` is present, a missing one a compile error), EXACT-TYPING (each member is assignable to its `T` slot at the member's own narrow type), and EXACTNESS (no member beyond `T`'s contract). An annotation `: T` proves coverage and exactness but pays for them by WIDENING the value to exactly `T`, erasing every per-member literal; `as const` proves narrowness but proves NO coverage — a frozen literal missing a contract member is silently incomplete. `satisfies` is the unique clause that adds the coverage proof on top of the preserved literal, so a vocabulary table whose key set must be total carries `satisfies`, never an annotation that would lose the rows the dispatch reads.
- The coverage proof is what makes the collapse complete rather than merely narrow: `as const` alone collapses `value -> type` (the literal IS the type, `keyof typeof` and indexed access lift the family), but it leaves the table free to omit a required member, so the dispatch that reads it can hit an absent row at runtime with no compile signal. `as const satisfies Record<K, Row>` closes that gap — the table is the type AND is proven complete against `K` — so the one declaration carries both the chain-collapse and the totality guarantee a separate completeness check would otherwise spell beside it.
- The totality the lever proves is CONDITIONAL on the contract's key being a finite literal union: `satisfies Record<FiniteUnion, Row>` errors on a missing member, but `satisfies Record<string, Row>` proves nothing about coverage — any subset is a valid `Record<string, Row>` — so the lever's coverage power is exactly the closedness of `K`, and a totality intent written against an open `string` key is the silent non-proof, the missing-member compile error that never fires.
- The closed `K` is itself derived from an owner, never restated: `keyof typeof OtherTable`, a `_tag` union lifted off a tagged family, or a `Schema.Literal` member set supplies the finite key, so the totality contract `Record<K, Row>` is one more projection off the single source — the table proves itself complete against a key union the program already holds, and a parallel literal-union type written beside the contract to spell `K` by hand is the restatement the derived key deletes.

```typescript
type Phase = 'idle' | 'busy' | 'done'
const handlers = {
    idle: { weight: 0, retains: false },
    busy: { weight: 2, retains: true },
    done: { weight: 1, retains: false },
} as const satisfies Record<Phase, { readonly weight: number; readonly retains: boolean }>
const cost = <const P extends Phase>(p: P): (typeof handlers)[P]['weight'] => handlers[p].weight
```

[COVERAGE_PROOF_DIAGNOSTIC]:
- The coverage failure is surfaced at the literal site naming the absent member by name: an `as const satisfies Record<K, Row>` table omitting one member of a finite `K` fails on the object literal with the missing key spelled in the diagnostic (`Property '<k>' is missing ... but required`), so the totality proof points at exactly the source line and the exact key the author forgot — the completeness violation is read where the table is authored, never deferred to a runtime traversal that hits the absent row.
- The diagnostic is the table's executable specification of completeness: adding a member to the finite `K` (a new color, a new phase, a new fault) without adding its row breaks the `satisfies` clause loudly, so the next requirement lands as a compile error on the table until its row exists — the totality contract is the anticipatory-collapse proof that the family grows by one row and every incomplete consumer breaks at the source, never as a silent gap discovered downstream.
- The exactness failure is a SEPARATE diagnostic on the stray member (`Object literal may only specify known properties, and '<k>' does not exist`), so a table over-covering `K` and a table under-covering `K` fail with two distinct errors at two distinct positions — the lever distinguishes "you wrote a row the contract forbids" from "you omitted a row the contract requires," the two halves of the totality proof reported independently so the repair is unambiguous.

[ANNOTATION_WIDENS_AND_LOSES]:
- The annotation `: T` is the rejected form precisely because it proves coverage by the same mechanism that destroys the literals: `const t: Record<K, Row> = { ... }` checks every key of `K` is present AND admits the value as exactly `Record<K, Row>`, so each row's `cap: 1` widens to `cap: number`, the indexed read `t['<k>']['cap']` yields `number` not `1`, and a downstream `const x: 1 = t.a.cap` fails — the coverage came at the cost of the narrowness the dispatch table's value-column needs. The annotation and `satisfies` both prove totality; only `satisfies` keeps the rows readable as their literal types.
- The widening is non-recoverable from the annotated binding: once `: Record<K, Row>` has erased `cap: 1` to `number`, no later `as const` or `typeof` read restores it — the literal was lost at the binding, not at a read, so the chain `const t: T = { ... }` followed by a downstream attempt to read a row's literal is the widened-head propagating a widened value the whole length of the consumer, the annotation at the declaration the single cause of every lost-literal symptom far from it.
- The annotation's one legitimate use is where the value-column carries no literal worth preserving — a table of `Schedule`, `Layer`, or function values whose type is already its full information — but a table whose rows carry caps, thresholds, flags, or discriminants the dispatch reads as literals takes `as const satisfies T`, never `: T`; reaching for the annotation on a literal-bearing table is the widen-then-lose defect that trades the rows for a coverage proof `satisfies` provides without the trade.

```typescript
type Channel = 'wire' | 'cache' | 'mem'
const budgets = {
    wire: { ceiling: 8, urgent: true },
    cache: { ceiling: 64, urgent: false },
    mem: { ceiling: 256, urgent: false },
} as const satisfies Record<Channel, { readonly ceiling: number; readonly urgent: boolean }>
const ceilingOf = <const C extends Channel>(c: C): (typeof budgets)[C]['ceiling'] => budgets[c].ceiling
const reject: Record<Channel, { readonly ceiling: number; readonly urgent: boolean }> = budgets
```

[CORRELATED_VOCABULARY_TOTALITY]:
- The totality proof composes across two tables when the second is keyed by the FIRST's key union, so a derived vocabulary is proven complete against its base in one clause: `as const satisfies Record<keyof typeof Base, Row>` forces a row for every key `Base` declares, and a key added to `Base` breaks the derived table until its correlated row exists — the two vocabularies stay in lockstep at the source, no parallel literal-union restated to bridge them.
- This is the totality lever's deepest collapse: a base table and a label table, a code table and a message table, an enum and its per-case policy are the naive author's two parallel structures kept in sync by hope, while `Record<keyof typeof Base, Row>` makes the synchronization a compile obligation — the derived table cannot drift from its base because every base key is a required member of the derived contract, and the diagnostic names the exact key that fell out of sync.
- The correlated key is read, never restated: `keyof typeof Base` is the finite union the base already fixes, so the derived contract spends no second declaration naming the keys — the chain that would write `type BaseKey = '<a>' | '<b>'` beside both tables is collapsed to the indexed read, and the totality proof rides the same one-source key the dispatch reads, the completeness guarantee and the key union being one projection.

```typescript
const codes = { ok: 200, gone: 410, teapot: 418 } as const
const explain = {
    ok: '<accepted>',
    gone: '<vanished>',
    teapot: '<refused>',
} as const satisfies Record<keyof typeof codes, string>
const render = <const K extends keyof typeof codes>(k: K): readonly [number, string] => [codes[k], explain[k]]
```

[REACH_IS_THE_AUTHORED_LITERAL]:
- The coverage the lever proves is a statement about the AUTHORED key set, never the arriving one: `satisfies Record<K, Row>` proves the source table covers every member of `K` and then erases entirely, so the proof is a closed fact over the literal as written and carries nothing about whether a key materializing at runtime — a column name from a request, a tag deserialized elsewhere, a string read from configuration — is even a member of `K`. The completeness guarantee terminates at the source seam: the table is total, the lookup key is unproven.
- The runtime totality question the lever leaves open — is this arriving value one of `K`'s members — is the complementary half a literal-union codec owns: `Schema.keyof(Owner): SchemaClass<keyof A>` and `Schema.Literal(...members)` admit a foreign key from `unknown` as a validated codec rejecting a non-member at the boundary, so the authored-coverage proof and the arriving-membership proof are two HALVES of one totality, not two spellings of one — a foreign key `satisfies`-asserted is membership-unchecked, a source table decoded is a category error.
- The two halves compose into total indexed access BY CONSTRUCTION: a boundary that decodes a foreign value into the table's own literal key union, fed into a table the source proved covers that union, yields an indexed read with no `undefined` arm and no runtime default — coverage proven at the source plus membership proven at the boundary leaves no missing-row case for the lookup to handle. Either half alone is incomplete (a covered table indexed by an unproven key can miss, a proven key into a sparse table can miss), and only their composition discharges the missing-row branch the naive lookup guards with a fallback.

```typescript
import { Effect, Schema } from 'effect'
const metrics = {
    p50: { window: 60, alarm: 0.2 },
    p99: { window: 300, alarm: 0.05 },
} as const satisfies Record<string, { readonly window: number; readonly alarm: number }>
const admitKey = Schema.decodeUnknown(Schema.Literal('p50', 'p99'))
const windowOf = (raw: unknown) => admitKey(raw).pipe(Effect.map((k) => metrics[k].window))
```

[PARTIAL_IS_TOTALITY_DECLARED_ABSENT]:
- The opt-out of totality is itself a typed declaration, not a silent omission: `as const satisfies Partial<Record<K, Row>>` admits a sparse table AND surfaces the sparseness in the read type — indexed access yields `Row | undefined`, so a consumer of a deliberately-incomplete table is forced to handle the absent row, the missing member moved from a silent runtime gap into a compile-visible `undefined` arm. A table that is genuinely partial declares it with `Partial<Record<...>>`, never by omitting members from a total `Record<K, Row>` and hoping no key misses.
- The choice between `Record<K, Row>` and `Partial<Record<K, Row>>` is the choice between two totality stances the lever makes explicit: the total contract proves every key present and lets indexed access return `Row` unconditionally, the partial contract proves no coverage and forces every read to confront `Row | undefined` — so the table's completeness is a property the reader can trust from the contract alone, and a `Record<K, Row>` read that defensively guards against `undefined` is the dead check the totality proof already eliminated.
- The partial stance reaches a third seam the totality stance cannot: an evolving vocabulary where new keys land before their rows do uses `Partial<Record<K, Row>>` so the table compiles through the gap with the `undefined` arm naming the not-yet-covered keys, then tightens to `Record<K, Row>` once every key has a row — the lever spells the transitional incompleteness as a type rather than a comment, and the tightening to the total contract is the compile event that proves the gap closed.

```typescript
type Op = 'create' | 'update' | 'delete' | 'archive'
const overrides = {
    delete: { confirm: true },
    archive: { confirm: true },
} as const satisfies Partial<Record<Op, { readonly confirm: boolean }>>
const needsConfirm = (op: Op): boolean => overrides[op as 'delete']?.confirm ?? false
```

[GENERATED_FENCE_VS_AUTHORED_SATISFIES]:
- A tagged family's matcher carries its own generated coverage proof as a handler-record parameter type, so the hand-authored `satisfies Record<K, Row>` and that generated proof are two carriers of one property whose selection is decided by which surface owns the key union and whether the rows carry data the lookup reads. Where the keys are a `_tag` union and the consumer is a fold over handlers, the generated proof closes coverage and a trailing `satisfies` is the redundant clause against a constraint the parameter already enforces; where the keys are an `as const` table's `keyof typeof` or a `Schema.Literal` member set and the consumer indexes into row VALUES, no generated parameter exists and the hand-authored `satisfies` is the only available coverage proof.
- The deciding distinction is what the proof's consumer reads: a generated matcher proves its handlers cover a tag union, but the handlers are FUNCTIONS the library folds, while a `satisfies`-complete `Record<K, Row>` is read by indexed access into row LITERALS — caps, thresholds, flags — the library never inspects. The hand-authored lever therefore proves a property the generated fence cannot: that every covered row additionally KEEPS its literal value, the conjunction of coverage and per-row literal preservation that only `as const satisfies T` carries and that a behavior-column dispatch reading data rows depends on.
- The selection is total and the misplacement is bidirectional: a redundant `satisfies` trailing a generated-complete handler record proves nothing the parameter type did not, and a missing `satisfies` on a hand-built data table leaves coverage unproven entirely — so the lever lands exactly on the decode-free vocabulary owner whose key union it derives off a single source and whose rows it must keep as literals, and is absent precisely where the library already generated the coverage the matcher consumes.

[TOTALITY_LEVER_LEDGER]:
- `as const satisfies Record<FiniteUnion, Row>`: proves coverage (missing member is a compile error), exact-typing, and exactness while preserving every row literal; the totality contract for the decode-free behavior-column vocabulary owner; the proof holds only for the literal as written.
- `: Record<K, Row>` annotation: proves coverage and exactness but WIDENS the value to exactly the contract, erasing every per-row literal; admitted only for a value-column whose type is already its full information; the widen-then-lose form a literal-bearing table rejects.
- `as const` alone: proves narrowness, no coverage; a finite-`K`-incomplete frozen table compiles silently; the missing-totality form `satisfies Record<K, Row>` completes.
- `satisfies Record<string, Row>`: proves exact-typing and exactness but NO coverage — any subset of an open key is valid; the silent non-proof when a finite-key totality was intended.
- `as const satisfies Partial<Record<K, Row>>`: declares incompleteness as a type — indexed access yields `Row | undefined`; the totality opt-out that surfaces the absent row at the read rather than hiding it; the transitional contract for a vocabulary whose keys outpace its rows.
- `keyof typeof Base` / `_tag` union / `Schema.Literal` members as `K`: the finite key derived off a single source; the totality contract reads its closed key union rather than restating a parallel literal-union type beside the table.
- `Schema.keyof(Owner)` / `Schema.Literal(...members)`: the runtime-totality codec admitting a foreign key from `unknown` against the table's key union; the boundary seam the authoring-time `satisfies` cannot reach, composed so a decoded key indexes a `satisfies`-complete row by construction.

[TOTALITY_ANTIPATTERNS]:
- A finite-key vocabulary table asserted `as const` with no `satisfies` clause is the silent-incompleteness link: the frozen table is the type but is unproven against its key union, so an omitted member is discovered only when the dispatch indexes the absent row at runtime — the repair is `satisfies Record<K, Row>` with `K` the finite union, turning the runtime gap into a named compile error at the source.
- A literal-bearing dispatch table annotated `: Record<K, Row>` to prove coverage is the widen-then-lose link: the annotation proves totality but erases every row literal the indexed read needs, so the table is complete-but-widened — the repair is `as const satisfies Record<K, Row>`, the clause that proves the same coverage while keeping the rows readable as their literal types.
- A parallel `type Key = '<a>' | '<b>' | '<c>'` written beside a table to spell the totality contract's `K` is the restated-key link: the base table or tagged family already fixes the finite union, so the contract reads `keyof typeof Base` or the `_tag` union directly — the standalone alias is the second statement the derived key deletes, and worse it can drift from the source it duplicates.
- A second table keyed by a hand-restated literal union rather than `keyof typeof Base` is the desynchronized-vocabulary link: a key added to the base does not break the derived table, so the two drift silently — the repair is `Record<keyof typeof Base, Row>`, making every base key a required member of the derived contract so the addition breaks the derived table loudly until its row exists.
- A `satisfies`-asserted table fed a runtime key as if the source proof guaranteed the arriving value is one of `K` is the false-runtime-totality link: `satisfies` is erased and proves nothing about a foreign value, so the arriving key is unchecked — the repair pairs the source `satisfies` with a `Schema.keyof`/`Schema.Literal` decode at the boundary, the two seams composing so a decoded key indexes a proven-complete row.
- A defensive `?? fallback` or `undefined` guard on an indexed read into a `Record<K, Row>` table proven total is the dead-totality-check link: the `satisfies Record<K, Row>` clause already proved every key present, so indexed access returns `Row` unconditionally and the fallback is unreachable — the repair is deleting the guard, or, where the table is genuinely sparse, declaring it `Partial<Record<K, Row>>` so the `Row | undefined` arm the guard handles is the contract's own typed admission of incompleteness.
