# [RASM_RHINO_COMMIT]

`Rasm.Rhino.Document` owns the host-mutation commit envelope. `DocumentCommit.Sealed` opens typed undo custody inside redraw compensation, projects the serial only into results that own it, and converges every fault before releasing the bracket.

The bracket's custody is a closed union, never a bool triple: a program either opened no record, owns the record it opened, enlisted in the command's record, or was refused a seat — four states the prior five booleans spelled as eight unreachable corners and two hand-kept lifecycle flags. The seal folds the execution outcome and the bounded close directly on their own carriers; the two private record families that re-wrapped those carriers as hand unions delete with the booleans.

## [01]-[INDEX]

- [02]-[REDRAW]: `RedrawAxis`, `RedrawPolicy`, `RedrawScope` — the repaint trait vocabulary, the closed posture roster, and the suppress/restore/success-gated-flush bracket.
- [03]-[COMMIT]: `HostInteraction`, `DocumentCommit` — the host-dialogue axis and the sealed commit entry with its compensation algebra.
- [04]-[BRACKET]: `UndoSerial`, `BracketCustody`, `BracketPhase`, `UndoBracket` — the admitted record identity, custody union, monotone lifecycle, and document transaction capsule.

## [02]-[REDRAW]

- Owner: `RedrawAxis` `ICapability` vocabulary — the five repaint traits the host's own `EnableRedraw(enable, redrawDocument, redrawLayers)` and `Redraw(deferred)` members read; `RedrawPolicy` `[SmartEnum<int>]` — the five closed postures, each carrying its traits as ONE `CapabilitySet<RedrawAxis>` column; `RedrawScope.Within` — the one suppress/restore/success-gated-flush bracket.
- Law: the posture roster is CLOSED and the traits are a SET — five bool columns on five rows spelled twenty-five cells a reader audited one at a time, where a set column prints its own wire and a new trait is one vocabulary row every posture answers. The two repaint columns the host's `EnableRedraw` takes are trait rows, not literals inside the bracket: a suppressing policy that hardcoded `false` on both silently forbade the terminal repaint some pipelines need on the restore edge.
- Law: the flush fires only after the prior redraw state is restored, so a suppressing policy still lands its terminal repaint; restore settles beside the primary through the one aggregation fold — a cleanup refusal never rides a discard.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rhino;
using Thinktecture;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RedrawAxis : ICapability<RedrawAxis> {
    public static readonly RedrawAxis Enabled = new(key: "enabled");
    public static readonly RedrawAxis Defers = new(key: "defers");
    public static readonly RedrawAxis Suppress = new(key: "suppress");
    public static readonly RedrawAxis RepaintsDocument = new(key: "repaints-document");
    public static readonly RedrawAxis RepaintsLayers = new(key: "repaints-layers");
}

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

// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class RedrawScope {
    internal static Fin<TOut> Within<TOut>(RhinoDoc document, RedrawPolicy redraw, Func<Fin<TOut>> body) =>
        from prior in Try.lift(() => document.Views.RedrawEnabled).Run()
        let suppress = redraw.Traits.Admits(capability: RedrawAxis.Suppress)
        let outcome = Try.lift(() => {
                if (suppress) {
                    document.Views.EnableRedraw(
                        enable: false,
                        redrawDocument: redraw.Traits.Admits(capability: RedrawAxis.RepaintsDocument),
                        redrawLayers: redraw.Traits.Admits(capability: RedrawAxis.RepaintsLayers));
                }
                return body();
            }).Run().Bind(static inner => inner)
        let restored = Try.lift(() => suppress
            ? HostEdge.Side(() => document.Views.EnableRedraw(
                enable: prior,
                redrawDocument: redraw.Traits.Admits(capability: RedrawAxis.RepaintsDocument),
                redrawLayers: redraw.Traits.Admits(capability: RedrawAxis.RepaintsLayers)))
            : unit).Run()
        from value in outcome.Settled(release: () => restored)
        from _ in Try.lift(() => redraw.Traits.Admits(capability: RedrawAxis.Enabled)
            ? HostEdge.Side(() => document.Views.Redraw(deferred: redraw.Traits.Admits(capability: RedrawAxis.Defers)))
            : unit).Run()
        select value;
}
```

## [03]-[COMMIT]

- Owner: `HostInteraction` `[SmartEnum<int>]` — the corpus-wide host-dialogue axis every folder's `quiet` argument reads; `DocumentCommit` — the sealed commit entry and the compensation algebra.
- Law: `HostInteraction` carries THREE rows because two different facts project the same host bool. `Quiet` and `Interactive` are a CALLER's election on a surface that offers the choice; `Silent` is design-mandated silence — a rollback or compensation leg whose surface offers no choice at all — so a reader tells "the caller chose quiet" from "no choice existed" at the row, and the prior law's per-site comment ("always quiet by design") becomes a row read. A folder minting its own two-row notice vocabulary beside this axis is the forked form, and a bare `quiet:` literal is the unnamed one.
- Law: `DocumentCommit.Sealed` brackets the body, opens the undo record, runs the program, applies an optional serial projection, runs the carried result projection inside the bracket, and seals. Callers omit `stamp` when the serial is not part of their canonical result.
- Law: `DocumentCommit.Compensated` owns the whole compensation algebra: land each element, roll back every landed key on the first refusal, and settle source custody through its release policy — every source releases once the fold's fate is decided, a release refusal after success rolls the landed keys back, and rollback then release faults append in that order onto the initiating fault. The identity release is the default modality riding the `Option` seat, so the release-free arity twin is deleted too; a suffix-only cleanup inside a rollback lambda or a `.Match` ladder re-spelling release beside the fold is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class HostInteraction {
    public static readonly HostInteraction Quiet = new(key: 0, isQuiet: true);
    public static readonly HostInteraction Interactive = new(key: 1, isQuiet: false);
    public static readonly HostInteraction Silent = new(key: 2, isQuiet: true);

    public bool IsQuiet { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DocumentCommit {
    internal static Fin<TOut> Sealed<TResult, TOut>(
        RhinoDoc document,
        string name,
        bool recordsUndo,
        RedrawPolicy redraw,
        Func<Fin<TResult>> run,
        Func<TResult, Fin<TOut>> project,
        Option<Func<TResult, uint, TResult>> stamp = default) =>
        RedrawScope.Within(document: document, redraw: redraw, body: () => Try.lift(() => {
            using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recordsUndo);
            Func<TResult, Fin<TResult>> stamped = undo.Stamper(
                stamp: stamp.IfNone(static (result, _) => result));
            Fin<TOut> executed = guard(undo.Admitted, new KernelFault.InvalidResult()).ToFin()
                .Bind(_ => Try.lift(run).Run().Bind(static inner => inner))
                .Bind(stamped)
                .Bind(result => Try.lift(() => project(result)).Run().Bind(static inner => inner));
            return undo.Seal(outcome: executed);
        }).Run().Bind(static inner => inner));

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

- Owner: `UndoSerial` admits the host's positive record identity; `BracketCustody` `[Union]` closes who owns the record; `BracketPhase` `[SmartEnum<int>]` carries the monotone open/closed/sealed lifecycle; `UndoBracket` owns the result-agnostic document transaction capsule.
- Law: custody is a CLOSED UNION derived once at `Begin`, never a bool triple re-tested per member. The five prior booleans (`required`, `owned`, `enlisted`, `closed`, `terminal`) spelled twenty-four unreachable corners; the union's four cases are the four reachable states, the serial rides its case as an admitted `UndoSerial` — so the `serial > 0u` guard deletes by construction — and an active non-command record lands `Refused`, which the admission gate reads before any mutation runs.
- Law: the lifecycle is MONOTONE — `Open` → `Closed` → `Sealed` — so "closed" and "seal-attempted" are one ordered axis rather than two independent flags: `Close` advances to `Closed`, `Seal` and `Dispose` terminate at `Sealed`, and a second seal reads the phase and refuses. `Dispose` cannot re-enter close after any seal attempt.
- Law: `Seal` owns bounded close recovery and the terminal rollback decision, folding the execution outcome and the bounded close DIRECTLY on their own carriers — success requires a fault-free close, recovered close faults fail successful execution, failed execution rolls back after recovered close, and an unrecoverable close reports rollback as unexecuted. The two private record families that re-wrapped `Fin<TResult>` and `Fin<Option<Error>>` as hand unions for one tuple switch are the deleted form: each was a re-mint of the carrier it copied, and the fold now reads the carriers it already holds.
- Law: `UndoBracket` is result-agnostic — every folder commit pipeline folds the sealed serial into its own result through `DocumentCommit.Sealed` without a foreign-shape hop; `Stamper` stamps only through an owned or enlisted custody, whose serial the union already proved positive, and an unrecorded program bypasses stamping.
- Law: rollback is custody-total — an owned record undoes and clears redo, an enlisted record propagates the failure to the command boundary that owns the record, and an unrecorded or refused seat has nothing to roll; every rollback fault appends onto the primary.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<uint>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UndoSerial {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value is 0u ? new ValidationError(message: "Undo serial must be positive.") : null;

    internal static Option<UndoSerial> Maybe(uint value) =>
        value is 0u ? Option<UndoSerial>.None : Some(Create(value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record BracketCustody {
    private BracketCustody() { }

    internal sealed record UnrecordedCase : BracketCustody;
    internal sealed record OwnedCase(UndoSerial Serial) : BracketCustody;
    internal sealed record EnlistedCase(UndoSerial Serial) : BracketCustody;
    internal sealed record RefusedCase : BracketCustody;
}

[SmartEnum<int>]
internal sealed partial class BracketPhase {
    internal static readonly BracketPhase Open = new(key: 0);
    internal static readonly BracketPhase Closed = new(key: 1);
    internal static readonly BracketPhase Sealed = new(key: 2);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
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

    public Func<TResult, Fin<TResult>> Stamper<TResult>(Func<TResult, uint, TResult> stamp) {
        BracketCustody seat = custody;
        return result => seat.Switch(
            state: (Result: result, Stamp: stamp),
            unrecordedCase: static (held, _) => Fin.Succ(value: held.Result),
            ownedCase: static (held, owned) => Stamped(held: held, serial: owned.Serial),
            enlistedCase: static (held, enlisted) => Stamped(held: held, serial: enlisted.Serial),
            refusedCase: static (held, _) => Fin.Fail<TResult>(error: new KernelFault.InvalidResult()));

        static Fin<TResult> Stamped(
            (TResult Result, Func<TResult, uint, TResult> Stamp) held, UndoSerial serial) =>
            from fold in Admit.Need(held.Stamp)
            from stamped in Try.lift(() => fold(held.Result, serial.Value)).Run()
            select stamped;
    }

    public Fin<TResult> Seal<TResult>(Fin<TResult> outcome) {
        if (phase == BracketPhase.Sealed) {
            return Fin.Fail<TResult>(error: new KernelFault.InvalidResult());
        }
        Fin<Option<Error>> closure = CloseBounded();
        phase = BracketPhase.Sealed;
        RhinoDoc owner = document;
        BracketCustody seat = custody;
        return outcome.Match(
            Succ: result => closure.Match(
                Succ: recovered => recovered.Match(
                    Some: static fault => Fin.Fail<TResult>(error: fault),
                    None: () => Fin.Succ(value: result)),
                Fail: open => Fin.Fail<TResult>(error: open + new DraftFault.HostRefused(Member: nameof(UndoBracket.Close),
                    Detail: "undo record remains open after bounded close recovery"))),
            Fail: primary => closure.Match(
                Succ: recovered => Fin.Fail<TResult>(error: recovered.Map(error => primary + error).IfNone(primary))
                    .Rollback(() => Reversed(document: owner, custody: seat)),
                Fail: open => Fin.Fail<TResult>(error: primary
                    + open
                    + new DraftFault.HostRefused(Member: nameof(UndoBracket.Close),
                        Detail: "undo record could not close, so rollback was not executed"))));
    }

    public void Dispose() {
        if (phase == BracketPhase.Sealed) {
            return;
        }
        _ = CloseBounded();
        phase = BracketPhase.Sealed;
    }

    private Fin<Option<Error>> CloseBounded() => Close().BiBind(
        Succ: static _ => Fin.Succ(Option<Error>.None),
        Fail: first => Close()
            .Map(_ => Some(first))
            .BindFail(second => Fin.Fail<Option<Error>>(error: first + second)));

    private Fin<Unit> Close() {
        if (phase != BracketPhase.Open) {
            return Fin.Succ(value: unit);
        }
        RhinoDoc owner = document;
        Fin<Unit> outcome = custody.Switch(
            state: owner,
            unrecordedCase: static (_, _) => Fin.Succ(value: unit),
            ownedCase: static (held, owned) => Try.lift(() => Admit.Confirm(
                success: held.EndUndoRecord(undoRecordSerialNumber: owned.Serial.Value))).Run().Bind(static inner => inner),
            enlistedCase: static (_, _) => Fin.Succ(value: unit),
            refusedCase: static (_, _) => Fin.Succ(value: unit));
        if (outcome.IsSucc) {
            phase = BracketPhase.Closed;
        }
        return outcome;
    }

    private static Fin<Unit> Reversed(RhinoDoc document, BracketCustody custody) =>
        custody.Switch(
            state: document,
            unrecordedCase: static (_, _) => Fin.Succ(value: unit),
            ownedCase: static (held, _) => Try.lift(() =>
                Admit.Confirm(success: held.Undo()).Map(_ => {
                    held.ClearRedoRecords();
                    return unit;
                })).Run().Bind(static inner => inner),
            enlistedCase: static (held, _) => Fin.Fail<Unit>(error: new DraftFault.HostRefused(Member: nameof(UndoBracket.Reversed),
                Detail: "command-owned undo record requires boundary failure propagation")),
            refusedCase: static (_, _) => Fin.Succ(value: unit));
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-document.md` — undo-record bracketing, redraw suspension members); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` redraw/interaction rows, the `BracketCustody` `[Union]`); kernel `Domain/results` (`Fin`, `Lease`, `Custody.Rollback`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
