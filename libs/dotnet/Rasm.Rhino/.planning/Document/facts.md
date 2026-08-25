# [RASM_RHINO_FACTS]

`Rasm.Rhino.Document` owns the parameterized evidence machines every mutation folder composes and none re-mints: the slot contract with its readable kind gate and the fact stream that accumulates commit-scoped consequences under an undo stamp. Eighteen `*Slot` vocabularies live across this folder and two parameterized owners already carried most of them badly — each folder that re-minted a receipt, a fact, a gate, or a projection beside these owners was executing the deleted form the stream's own law names. A third mutation folder joins by declaring a `[SmartEnum<int>]` slot vocabulary and a `[Union]` body family and inherits the accumulation, the gate, and every projection with zero new surface.

The owners are RULED PLURAL by timing, not by accident, and the discriminant is stated here once: `FactStream<TSlot, TBody>` accumulates consequence facts across every operation inside ONE commit and is sealed by the undo stamp; Modeling's `BuildReceipt<TSlot>`/`Built<TSlot>` carries build-product evidence bound to one produced value, minted OUTSIDE `DocumentCommit.Sealed` and read by the builder that produced it — "merging them would put a commit-scoped undo column on a value that never enters a commit" (the tables rail's own law, carried here with the owner); Exchange’s `BatchProgram<TReceipt>` is the third regime — independent ordered rows each minting its own receipt, file-scoped and envelope-spanning, which is why neither sibling can absorb it (`Exchange/operations#[04]-[BATCH_PROGRAM]`).

## [01]-[INDEX]

- [02]-[SLOT]: `IFactSlot<TBody>`, `IFactBody<TKind>`, `IFactSlot<TBody, TKind>`, `UndoSerial` — the slot contract, the body-kind floor, the readable kind gate, and the stamp scalar.
- [03]-[FACT]: `Fact<TSlot, TBody>` — one slot-addressed consequence.
- [04]-[STREAM]: `FactStream<TSlot, TBody>` — the commit-scoped accumulation, the cross-product gate, the undo-stamp projection, and the slot-keyed readers.

## [02]-[SLOT]

- Owner: `IFactSlot<TBody>` — the floor a mutation folder's slot vocabulary realizes: a generated key and the body-admission gate; `IFactBody<TKind>` — the kind floor a body union answers through one total fold; `IFactSlot<TBody, TKind>` — the readable refinement whose `Admits` DERIVES from a declared `CapabilitySet<TKind>` column; `UndoSerial` — the positive stamp scalar the commit envelope mints and every folder receipt carries.
- Entry: a folder's `[SmartEnum<int>]` slot vocabulary satisfies the kinded contract with one `CapabilitySet<TKind> Bodies` column — the key is already generated and `Admits` is a default interface member reading that column — so joining the stream is two declarations and no body.
- Law: the kind gate makes admission READABLE. The bare contract admitted through an opaque `Func<TBody, bool>` predicate per row — fourteen type-test lambdas across the boundary slot vocabularies, none of which a reader, a receipt printer, or a census could enumerate — where the kinded refinement states the emitted kinds as a set whose `Wire` prints and whose rows a census greps. A slot vocabulary whose admission is genuinely not kind-shaped (a value-dependent gate) keeps the bare contract and states why; kind-shaped admission on the bare contract is now the deleted form.
- Law: the body family answers its OWN kind through one total generated fold — a slot re-deriving a body's kind by type test holds a second authority over a fact the union already states, and the fold breaks loudly when a body case lands.
- Law: `UndoSerial` refuses zero, because `UndoBracket` answers `0u` for a program that opened no record — the value object's refusal is what keeps "no record" out of a receipt as a fact claiming record zero. `Maybe` is the one projector of that host sentinel.
- Growth: a folder joins with a slot vocabulary and a body union; a new body kind is one vocabulary row plus the union case that answers it; the contracts never widen per folder.
- Boundary: the `*Slot` SUFFIX claims nothing — conformance is claimed by the `IFactSlot` declaration alone, and a `*Slot` name outside a conformer names its own concern (`SlotPresence`/`NamedSlot` seat a named host-callback parameter; `PickSlot`, `AcceptSlot`, `PointSlot`, `ObjectSlot`, `ArchiveSlot`, `SwatchSlot`, and `PrinterSlot` each name an acquisition, archive, or output seat with no fact timing), so a reader tests the declaration, never the word.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IFactSlot<in TBody> where TBody : class {
    int Key { get; }
    bool Admits(TBody body);
}

public interface IFactBody<out TKind> where TKind : class, ICapability<TKind> {
    TKind Kind { get; }
}

public interface IFactSlot<in TBody, TKind> : IFactSlot<TBody>
    where TBody : class, IFactBody<TKind>
    where TKind : class, ICapability<TKind> {
    CapabilitySet<TKind> Bodies { get; }

    bool IFactSlot<TBody>.Admits(TBody body) => Bodies.Admits(capability: body.Kind);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<uint>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UndoSerial {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value is 0u ? new ValidationError(message: "Undo serial must be positive.") : null;

    internal static Option<UndoSerial> Maybe(uint value) =>
        value is 0u ? Option<UndoSerial>.None : Some(Create(value));
}
```

## [03]-[FACT]

- Owner: `Fact<TSlot, TBody>` — one slot-addressed consequence, the pairing the stream accumulates and every projection filters on.
- Law: a fact is DETACHED evidence — admitted values, runtime pairs, stamps — never a live host handle, because the stream outlives the commit window that minted it.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Fact<TSlot, TBody>(TSlot Slot, TBody Body)
    where TSlot : class, IFactSlot<TBody>
    where TBody : class;
```

## [04]-[STREAM]

- Owner: `FactStream<TSlot, TBody>` — the per-op fact accumulation two mutation folders had built twice, verbatim: only the vocabularies were folder-specific, and a fix to the shared machinery (the gate naming the offending slot, the zero-serial projection) landed on one copy and not the other.
- Entry: `Of` admits one fact through the cross-product gate; `All` accumulates a body roster under one slot with every refusal reported; `Stamped` is the undo projection; `Project` and `FactCount` are the slot-keyed readers; `+` is the fold.
- Law: `Admits` is THE gate and total in both directions — a slot cannot exist without declaring the bodies it emits, a body cannot enter under a slot that does not name it, and the refusal carries the slot's key, because "this receipt rejected a body" is unactionable where "slot 7 does not emit a path" is not.
- Law: the undo stamp is a PROJECTION, never a rail. `DocumentCommit.Sealed` stamps every sealed receipt including a program that opened no record; `UndoSerial.Maybe` refuses that zero, so an unrecorded program contributes no fact rather than one claiming record zero, and the total `(receipt, serial) -> receipt` shape the envelope demands holds. The gate still runs inside the stamp, so a folder whose undo slot does not declare its record body stamps nothing instead of smuggling a body past the cross product.
- Law: a folder re-minting a receipt, a fact, a gate, or a projection beside this owner is the deleted form — its own mint factories ride an extension block over the closed instantiation (`global using XReceipt = FactStream<XSlot, XBody>` plus one `extension` block), so its call sites read as its own and it gains every projection for two declarations.
- Growth: a third mutation folder joins by declaring a slot vocabulary and a body union; the stream itself never widens per folder.
- Boundary: Modeling's `BuildReceipt<TSlot>`/`Built<TSlot>` is the build-product timing class and stays where it is; Exchange's `BatchProgram<TReceipt>` (`Exchange/operations#[04]-[BATCH_PROGRAM]`) is the FILE-scoped, envelope-spanning class that lawfully refuses `FactStream` — the page charter states the three-way timing discriminant once and each owner's card points here.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FactStream<TSlot, TBody> : IDetachedDocumentResult
    where TSlot : class, IFactSlot<TBody>
    where TBody : class {
    private readonly Seq<Fact<TSlot, TBody>> facts;

    private FactStream(Seq<Fact<TSlot, TBody>> facts) => this.facts = facts;

    public static FactStream<TSlot, TBody> Empty { get; } = new(facts: Seq<Fact<TSlot, TBody>>());

    public Seq<Fact<TSlot, TBody>> Facts => facts;

    public static FactStream<TSlot, TBody> operator +(FactStream<TSlot, TBody> left, FactStream<TSlot, TBody> right) =>
        new(facts: left.facts + right.facts);

    public static Fin<FactStream<TSlot, TBody>> Of(TSlot slot, TBody body, Op key) =>
        from row in key.Need(value: slot)
        from payload in key.Need(value: body)
        from _ in guard(
            row.Admits(body: payload),
            key.InvalidResult(detail: row.Key.ToString(CultureInfo.InvariantCulture))).ToFin()
        select new FactStream<TSlot, TBody>(facts: Seq(new Fact<TSlot, TBody>(Slot: row, Body: payload)));

    public static Fin<FactStream<TSlot, TBody>> All(TSlot slot, Seq<TBody> bodies, Op key) =>
        bodies
            .Traverse(body => Of(slot: slot, body: body, key: key).ToValidation())
            .As()
            .ToFin()
            .Map(static streams => streams.Fold(Empty, static (state, next) => state + next));

    public FactStream<TSlot, TBody> Stamped(TSlot slot, Func<UndoSerial, TBody> record, uint serial) =>
        UndoSerial.Maybe(value: serial)
            .Map(record)
            .Filter(slot.Admits)
            .Map(body => this + new FactStream<TSlot, TBody>(
                facts: Seq(new Fact<TSlot, TBody>(Slot: slot, Body: body))))
            .IfNone(noneValue: this);

    public Seq<T> Project<T>(TSlot slot, Func<TBody, Option<T>> select) =>
        facts.Filter(fact => fact.Slot == slot).Choose(fact => select(fact.Body));

    public int FactCount(TSlot slot) => facts.Count(fact => fact.Slot == slot);
}
```

- Packages: `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` slot vocabularies, `[Union]` bodies, `[ValueObject]` `UndoSerial`); `LanguageExt.Core` (`libs/dotnet/.api/api-languageext.md` — `Seq` fact runs, monoid append); kernel `Domain/rails` (`Op`, `Fin`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
