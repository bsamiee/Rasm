# [RASM_RHINO_COMMIT]

`Rasm.Rhino.Document` owns the ONE host-mutation commit envelope. Every folder commit rail — table, layer, session-regime, annotation draft, block, object, render content, render settings, exchange, sheet, persistence preset, user-text, capture-adopt — frames its change through `DocumentCommit.Sealed`: the undo bracket opens under a typed custody, the redraw scope suppresses and restores around the body, the sealed serial stamps the folder's own receipt, and every mid-stage fault converges on the one rail that still releases the bracket. A hand-spelled `UndoBracket.Begin` or redraw triple beside this envelope is the deleted form, and twenty-three out-of-page consumers compose it.

The bracket's custody is a closed union, never a bool triple: a program either opened no record, owns the record it opened, enlisted in the command's record, or was refused a seat — four states the prior five booleans spelled as eight unreachable corners and two hand-kept lifecycle flags. The seal folds the execution outcome and the bounded close directly on their own rails; the two private record families that re-wrapped those rails as hand unions delete with the booleans.

## [01]-[INDEX]

- [02]-[REDRAW]: `RedrawAxis`, `RedrawPolicy`, `RedrawScope` — the repaint trait vocabulary, the closed posture roster, and the suppress/restore/success-gated-flush bracket.
- [03]-[COMMIT]: `HostInteraction`, `DocumentCommit` — the host-dialogue axis and the sealed commit entry with its compensation algebra.
- [04]-[BRACKET]: `BracketCustody`, `BracketPhase`, `UndoBracket` — the record custody union, the monotone lifecycle, and the document transaction capsule.

## [02]-[REDRAW]

- Owner: `RedrawAxis` `ICapability` vocabulary — the five repaint traits the host's own `EnableRedraw(enable, redrawDocument, redrawLayers)` and `Redraw(deferred)` members read; `RedrawPolicy` `[SmartEnum<int>]` — the five closed postures, each carrying its traits as ONE `CapabilitySet<RedrawAxis>` column; `RedrawScope.Within` — the one suppress/restore/success-gated-flush bracket.
- Law: the posture roster is CLOSED and the traits are a SET — five bool columns on five rows spelled twenty-five cells a reader audited one at a time, where a set column prints its own wire and a new trait is one vocabulary row every posture answers. The two repaint columns the host's `EnableRedraw` takes are trait rows, not literals inside the bracket: a suppressing policy that hardcoded `false` on both silently forbade the terminal repaint some rails need on the restore edge.
- Law: the flush fires only after the prior redraw state is restored, so a suppressing policy still lands its terminal repaint; restore settles beside the primary through the one aggregation fold — a cleanup refusal never rides a discard.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Rasm.Domain;
using Rhino;
using Thinktecture;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RedrawAxis : ICapability<RedrawAxis> {
    public static readonly RedrawAxis Enabled = new(key: "enabled");
    public static readonly RedrawAxis Defers = new(key: "defers");
    public static readonly RedrawAxis Suppress = new(key: "suppress");
    public static readonly RedrawAxis RepaintsDocument = new(key: "repaints-document");
    public static readonly RedrawAxis RepaintsLayers = new(key: "repaints-layers");
}

// The five postures a commit names; each row's traits are ONE set column the bracket reads, so the twenty-five
// boolean cells the five-column roster carried collapse and a new trait is one vocabulary row.
[SmartEnum<int>]
public sealed partial class RedrawPolicy {
    public static readonly RedrawPolicy None = new(key: 0, traits: CapabilitySet<RedrawAxis>.Of());
    public static readonly RedrawPolicy Continuous = new(key: 1, traits: CapabilitySet<RedrawAxis>.Of(RedrawAxis.Enabled));
    public static readonly RedrawPolicy Immediate = new(key: 2, traits: CapabilitySet<RedrawAxis>.Of(RedrawAxis.Enabled, RedrawAxis.Suppress));
    public static readonly RedrawPolicy Deferred = new(key: 3, traits: CapabilitySet<RedrawAxis>.Of(RedrawAxis.Enabled, RedrawAxis.Defers, RedrawAxis.Suppress));
    public static readonly RedrawPolicy Repainting = new(key: 4, traits: CapabilitySet<RedrawAxis>.Of(
        RedrawAxis.Enabled, RedrawAxis.Suppress, RedrawAxis.RepaintsDocument, RedrawAxis.RepaintsLayers));

    public CapabilitySet<RedrawAxis> Traits { get; }
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal static class RedrawScope {
    internal static Fin<TOut> Within<TOut>(RhinoDoc document, RedrawPolicy redraw, Func<Fin<TOut>> body, Op key) =>
        from prior in key.Catch(() => Fin.Succ(value: document.Views.RedrawEnabled))
        let suppress = redraw.Traits.Admits(capability: RedrawAxis.Suppress)
        let outcome = key.Catch(() =>
            Op.SideWhen(suppress, () => document.Views.EnableRedraw(
                        enable: false,
                        redrawDocument: redraw.Traits.Admits(capability: RedrawAxis.RepaintsDocument),
                        redrawLayers: redraw.Traits.Admits(capability: RedrawAxis.RepaintsLayers)))
                .Bind(_ => key.Catch(body)))
        let restored = key.Catch(() => Fin.Succ(value: Op.SideWhen(suppress, () => document.Views.EnableRedraw(
            enable: prior,
            redrawDocument: redraw.Traits.Admits(capability: RedrawAxis.RepaintsDocument),
            redrawLayers: redraw.Traits.Admits(capability: RedrawAxis.RepaintsLayers)))))
        from value in Append(primary: outcome, side: restored)
        from _ in key.Catch(() => Fin.Succ(value: Op.SideWhen(
            redraw.Traits.Admits(capability: RedrawAxis.Enabled),
            () => document.Views.Redraw(deferred: redraw.Traits.Admits(capability: RedrawAxis.Defers)))))
        select value;

    // A restore fault APPENDS to the primary rather than replacing or vanishing — the ruled cleanup posture.
    private static Fin<T> Append<T>(Fin<T> primary, Fin<Unit> side) => primary.BiBind(
        Succ: value => side.Map(_ => value),
        Fail: error => side.Match(
            Succ: _ => Fin.Fail<T>(error: error),
            Fail: fault => Fin.Fail<T>(error: error + fault)));
}
```

## [03]-[COMMIT]

- Owner: `HostInteraction` `[SmartEnum<int>]` — the corpus-wide host-dialogue axis every folder's `quiet` argument reads; `DocumentCommit` — the sealed commit entry and the compensation algebra.
- Law: `HostInteraction` carries THREE rows because two different facts project the same host bool. `Quiet` and `Interactive` are a CALLER's election on a surface that offers the choice; `Silent` is design-mandated silence — a rollback or compensation leg whose surface offers no choice at all — so a reader tells "the caller chose quiet" from "no choice existed" at the row, and the prior law's per-site comment ("always quiet by design") becomes a row read. A folder minting its own two-row notice vocabulary beside this axis is the forked form, and a bare `quiet:` literal is the unnamed one.
- Law: `DocumentCommit.Sealed` is ONE entry: it brackets the body in the redraw scope, opens the undo bracket, runs the program, stamps the sealed serial through the bracket's custody, runs the railed receipt projection INSIDE the bracket — so a stamp or projection fault remains rollback-capable — and seals. The identity projection is the default MODALITY, spelled `project: Fin.Succ` at receipt-shaped call sites; the non-projecting arity twin is deleted, so one declaration carries every consumer. NAMED LOSS: the two-argument convenience signature; witness — `Tables.Commit`'s receipt-shaped entry composes `Sealed(..., project: Fin.Succ, ...)` and compiles unchanged.
- Law: `DocumentCommit.Compensated` owns the whole compensation algebra: land each element, roll back every landed key on the first refusal, and settle source custody through its release policy — every source releases once the fold's fate is decided, a release refusal after success rolls the landed keys back, and rollback then release faults append in that order onto the initiating fault. The identity release is the default modality riding the `Option` seat, so the release-free arity twin is deleted too; a suffix-only cleanup inside a rollback lambda or a `.Match` ladder re-spelling release beside the fold is the deleted form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Three rows over one host bool: `Quiet`/`Interactive` are a caller's ELECTION, `Silent` is design-mandated
// silence on a surface that offers no choice — a rollback leg reads `Silent` and no site carries a comment
// asserting what the row now states.
[SmartEnum<int>]
public sealed partial class HostInteraction {
    public static readonly HostInteraction Quiet = new(key: 0, isQuiet: true);
    public static readonly HostInteraction Interactive = new(key: 1, isQuiet: false);
    public static readonly HostInteraction Silent = new(key: 2, isQuiet: true);

    public bool IsQuiet { get; }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class DocumentCommit {
    // ONE sealed entry: redraw scope outside, bracket inside, stamp and projection INSIDE the bracket so their
    // refusals roll the owned record back like any operation fault. Identity projection spells `Fin.Succ`.
    internal static Fin<TOut> Sealed<TReceipt, TOut>(
        RhinoDoc document,
        string name,
        bool recordsUndo,
        RedrawPolicy redraw,
        Func<Fin<TReceipt>> run,
        Func<TReceipt, uint, TReceipt> stamp,
        Func<TReceipt, Fin<TOut>> project,
        Op op) =>
        RedrawScope.Within(document: document, redraw: redraw, key: op, body: () => op.Catch(() => {
            using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recordsUndo);
            Func<TReceipt, Fin<TReceipt>> stamped = undo.Stamper<TReceipt>(stamp: stamp, key: op);
            Fin<TOut> executed = guard(undo.Admitted, op.InvalidResult()).ToFin()
                .Bind(_ => op.Catch(run))
                .Bind(stamped)
                .Bind(receipt => op.Catch(() => project(receipt)));
            return undo.Seal(outcome: executed, key: op);
        }));

    // Land, roll back on first refusal, settle custody — with rollback then release faults appending in that
    // order onto the initiating fault. The identity release rides the `Option` seat as the default modality.
    internal static Fin<Seq<TKey>> Compensated<TSource, TKey>(
        Seq<TSource> source,
        Func<TSource, Fin<TKey>> land,
        Func<Seq<TKey>, Fin<Unit>> rollback,
        Option<Func<Seq<TSource>, Fin<Unit>>> release = default) {
        Func<Seq<TSource>, Fin<Unit>> settle = release.IfNone(static _ => Fin.Succ(value: unit));
        (Seq<TKey> Landed, Option<Error> Fault) outcome = source.Fold(
            (Landed: Seq<TKey>(), Fault: default(Option<Error>)),
            (state, value) => state.Fault.IsSome ? state : land(value).Match(
                Succ: key => (state.Landed.Add(key), default(Option<Error>)),
                Fail: error => (state.Landed, Some(error))));
        return outcome.Fault.Match(
            Some: cause => Unwound<TKey>(primary: cause, rollback(outcome.Landed), settle(source)),
            None: () => settle(source).Match(
                Succ: _ => Fin.Succ(value: outcome.Landed),
                Fail: cause => Unwound<TKey>(primary: cause, rollback(outcome.Landed))));
    }

    private static Fin<Seq<TKey>> Unwound<TKey>(Error primary, params ReadOnlySpan<Fin<Unit>> compensation) =>
        Fin.Fail<Seq<TKey>>(error: toSeq(compensation.ToArray()).Fold(primary, static (fault, step) => step.Match(
            Succ: _ => fault,
            Fail: error => fault + error)));
}
```

## [04]-[BRACKET]

- Owner: `BracketCustody` `[Union]` — who owns the undo record: no record, an owned record, the command's enlisted record, or a refused seat; `BracketPhase` `[SmartEnum<int>]` — the monotone open/closed/sealed lifecycle; `UndoBracket` — the receipt-agnostic document transaction capsule.
- Law: custody is a CLOSED UNION derived once at `Begin`, never a bool triple re-tested per member. The five prior booleans (`required`, `owned`, `enlisted`, `closed`, `terminal`) spelled twenty-four unreachable corners; the union's four cases are the four reachable states, the serial rides its case as an admitted `UndoSerial` — so the `serial > 0u` guard deletes by construction — and an active non-command record lands `Refused`, which the admission gate reads before any mutation runs.
- Law: the lifecycle is MONOTONE — `Open` → `Closed` → `Sealed` — so "closed" and "seal-attempted" are one ordered axis rather than two independent flags: `Close` advances to `Closed`, `Seal` and `Dispose` terminate at `Sealed`, and a second seal reads the phase and refuses. `Dispose` cannot re-enter close after any seal attempt.
- Law: `Seal` owns bounded close recovery and the terminal rollback decision, folding the execution outcome and the bounded close DIRECTLY on their own rails — success requires a fault-free close, recovered close faults fail successful execution, failed execution rolls back after recovered close, and an unrecoverable close reports rollback as unexecuted. The two private record families that re-wrapped `Fin<TReceipt>` and `Fin<Option<Error>>` as hand unions for one tuple switch are the deleted form: each was a re-mint of the rail it copied, and the fold now reads the rails it already holds.
- Law: `UndoBracket` is receipt-agnostic — every folder commit rail folds the sealed serial into its own receipt through `DocumentCommit.Sealed` without a foreign-receipt hop; `Stamper` stamps only through an owned or enlisted custody, whose serial the union already proved positive, and an unrecorded program bypasses stamping.
- Law: rollback is custody-total — an owned record undoes and clears redo, an enlisted record propagates the failure to the command boundary that owns the record, and an unrecorded or refused seat has nothing to roll; every rollback fault appends onto the primary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Derived ONCE at `Begin` from the host's own state — recording demand, command scope, active record — and read
// as a case everywhere the five booleans were re-tested: the serial rides its case as an admitted `UndoSerial`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record BracketCustody {
    private BracketCustody() { }

    internal sealed record UnrecordedCase : BracketCustody;
    internal sealed record OwnedCase(UndoSerial Serial) : BracketCustody;
    internal sealed record EnlistedCase(UndoSerial Serial) : BracketCustody;
    internal sealed record RefusedCase : BracketCustody;
}

// Monotone: `Open` → `Closed` → `Sealed`. One ordered axis carries what two independent flags spelled, so a
// re-entrant close and a double seal are phase reads rather than flag arithmetic.
[SmartEnum<int>]
internal sealed partial class BracketPhase {
    internal static readonly BracketPhase Open = new(key: 0);
    internal static readonly BracketPhase Closed = new(key: 1);
    internal static readonly BracketPhase Sealed = new(key: 2);
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal ref struct UndoBracket {
    private readonly RhinoDoc document;
    private readonly BracketCustody custody;
    private BracketPhase phase;

    private UndoBracket(RhinoDoc document, BracketCustody custody) {
        this.document = document;
        this.custody = custody;
        phase = BracketPhase.Open;
    }

    public bool Admitted => custody is not BracketCustody.RefusedCase;

    // Custody derives from the host's own three facts: outside a command with no active record the bracket OWNS
    // the record it opens; inside a command with an active record it ENLISTS; a recording program that can seat
    // neither — an active non-command record — is REFUSED before any mutation runs.
    public static UndoBracket Begin(RhinoDoc document, string name, bool recordsUndo) {
        bool active = document.UndoRecordingIsActive;
        bool inCommand = global::Rhino.Commands.Command.InCommand();
        BracketCustody custody = !recordsUndo
            ? new BracketCustody.UnrecordedCase()
            : !inCommand && !active
                ? UndoSerial.Maybe(value: document.BeginUndoRecord(description: name))
                    .Map(static serial => (BracketCustody)new BracketCustody.OwnedCase(Serial: serial))
                    .IfNone(static () => new BracketCustody.RefusedCase())
                : inCommand && active
                    ? UndoSerial.Maybe(value: document.CurrentUndoRecordSerialNumber)
                        .Map(static serial => (BracketCustody)new BracketCustody.EnlistedCase(Serial: serial))
                        .IfNone(static () => new BracketCustody.RefusedCase())
                    : new BracketCustody.RefusedCase();
        return new UndoBracket(document: document, custody: custody);
    }

    // Stamps only through a custody that HOLDS a record — the union already proved the serial positive, so no
    // `serial > 0u` guard survives — and an unrecorded program bypasses stamping entirely.
    public Func<TReceipt, Fin<TReceipt>> Stamper<TReceipt>(Func<TReceipt, uint, TReceipt> stamp, Op key) {
        BracketCustody seat = custody;
        return receipt => seat.Switch(
            state: (Receipt: receipt, Stamp: stamp, Key: key),
            unrecordedCase: static (held, _) => Fin.Succ(value: held.Receipt),
            ownedCase: static (held, owned) => Stamped(held: held, serial: owned.Serial),
            enlistedCase: static (held, enlisted) => Stamped(held: held, serial: enlisted.Serial),
            refusedCase: static (held, _) => Fin.Fail<TReceipt>(error: held.Key.InvalidResult()));

        static Fin<TReceipt> Stamped(
            (TReceipt Receipt, Func<TReceipt, uint, TReceipt> Stamp, Op Key) held, UndoSerial serial) =>
            from fold in held.Key.Need(held.Stamp)
            from stamped in held.Key.Catch(() => Fin.Succ(value: fold(held.Receipt, serial.Value)))
            select stamped;
    }

    // The 2×2 fold reads the two rails it already holds — execution `Fin<TReceipt>` and bounded close
    // `Fin<Option<Error>>` — where two private record families re-wrapped both for one tuple switch.
    public Fin<TReceipt> Seal<TReceipt>(Fin<TReceipt> outcome, Op key) {
        if (phase == BracketPhase.Sealed) {
            return Fin.Fail<TReceipt>(error: key.InvalidResult());
        }
        Fin<Option<Error>> closure = CloseBounded(key: key);
        phase = BracketPhase.Sealed;
        RhinoDoc owner = document;
        BracketCustody seat = custody;
        return outcome.Match(
            Succ: receipt => closure.Match(
                Succ: recovered => recovered.Match(
                    Some: static fault => Fin.Fail<TReceipt>(error: fault),
                    None: () => Fin.Succ(value: receipt)),
                Fail: open => Fin.Fail<TReceipt>(error: open + new DraftFault.HostRefused(
                    Key: key,
                    Member: nameof(UndoBracket.Close),
                    Detail: "undo record remains open after bounded close recovery"))),
            Fail: primary => closure.Match(
                Succ: recovered => Fin.Fail<TReceipt>(error: recovered.Map(error => primary + error).IfNone(primary))
                    .Rollback(() => Reversed(document: owner, custody: seat, key: key)),
                Fail: open => Fin.Fail<TReceipt>(error: primary
                    + open
                    + new DraftFault.HostRefused(
                        Key: key,
                        Member: nameof(UndoBracket.Close),
                        Detail: "undo record could not close, so rollback was not executed"))));
    }

    public void Dispose() {
        if (phase == BracketPhase.Sealed) {
            return;
        }
        _ = CloseBounded(key: Op.Of());
        phase = BracketPhase.Sealed;
    }

    private Fin<Option<Error>> CloseBounded(Op key) => Close(key: key).BiBind(
        Succ: static _ => Fin.Succ(Option<Error>.None),
        Fail: first => Close(key: key)
            .Map(_ => Some(first))
            .BindFail(second => Fin.Fail<Option<Error>>(error: first + second)));

    // Only an OWNED record closes; the phase advances on success so a re-entrant close is a phase read.
    private Fin<Unit> Close(Op key) {
        if (phase != BracketPhase.Open) {
            return Fin.Succ(value: unit);
        }
        RhinoDoc owner = document;
        Fin<Unit> outcome = custody.Switch(
            state: (Owner: owner, Key: key),
            unrecordedCase: static (_, _) => Fin.Succ(value: unit),
            ownedCase: static (held, owned) => held.Key.Catch(() => held.Key.Confirm(
                success: held.Owner.EndUndoRecord(undoRecordSerialNumber: owned.Serial.Value))),
            enlistedCase: static (_, _) => Fin.Succ(value: unit),
            refusedCase: static (_, _) => Fin.Succ(value: unit));
        if (outcome.IsSucc) {
            phase = BracketPhase.Closed;
        }
        return outcome;
    }

    // Custody-total compensation the kernel `Custody.Rollback` delegate arm consumes — its faults append onto the
    // primary at that owner: owned rolls back and clears redo, enlisted refuses with the boundary-propagation
    // refusal the command boundary that owns the record must carry, unrecorded and refused have nothing to roll.
    private static Fin<Unit> Reversed(RhinoDoc document, BracketCustody custody, Op key) =>
        custody.Switch(
            state: (Document: document, Key: key),
            unrecordedCase: static (_, _) => Fin.Succ(value: unit),
            ownedCase: static (held, _) => held.Key.Catch(() =>
                held.Key.Confirm(success: held.Document.Undo()).Map(_ => {
                    held.Document.ClearRedoRecords();
                    return unit;
                })),
            enlistedCase: static (held, _) => Fin.Fail<Unit>(error: new DraftFault.HostRefused(
                Key: held.Key,
                Member: nameof(UndoBracket.Reversed),
                Detail: "command-owned undo record requires boundary failure propagation")),
            refusedCase: static (_, _) => Fin.Succ(value: unit));
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-document.md` — undo-record bracketing, redraw suspension members); `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` redraw/interaction rows, the `BracketCustody` `[Union]`); kernel `Domain/rails` (`Op`, `Fin`, `Lease`, `Custody.Rollback`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
