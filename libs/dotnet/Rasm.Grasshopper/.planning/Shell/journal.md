# [RASM_GRASSHOPPER_SHELL_JOURNAL]

`SessionJournal` is the boundary's analytics egress — one monotone-stamped, per-document journal folding `UiEvent<GhFact>` envelopes and `GhEvidence` receipts into bounded partitions, and one export projection turning a session into a detached record for post-mortems, support bundles, analytics, and hook replay. Rows are evidence values only; a live host object, lease, or delegate never enters a partition, so an export is serializable by construction at whichever wire the app root chooses.

Consumption stays off the UI thread: one single-reader loop drains the kernel `EvidenceDrain<GhFact>.Reader`, appends each envelope under its owning document identity, and the drain's own `Shed`/`Refused` account the publication losses — the journal accounts only what ITS ring sheds. Kernel envelope's `Ordinal` is the sink-serialized total order the replay law rests on; the journal's `MonotonicStamp` is the cross-family ordering authority inside a partition, both minted off the session's one injected timeline.

## [01]-[INDEX]

- [02]-[ROWS]: `JournalPolicy` + `JournalFact` + `JournalRow` — the bounded partition policy, the fact union, and the stamped row evidence.
- [03]-[FOLD]: `JournalLedger` + `JournalExport` + `SessionJournal` — the committed fold state, the append gate, the drain mount, and the export projection.

## [02]-[ROWS]

- Owner: `JournalPolicy` sealed record — the per-partition ring bound; `Default` keeps the newest rows per document and sheds the head with accounting. It stays DISTINCT from the kernel `DrainPolicy` on a named discriminant: the drain bounds a channel's admission (drop mode), this bounds a partition's retention (head shed) — two losses, two counters, two policies. `JournalFact` `[Union]` — `EventCase` carries one `UiEvent<GhFact>`, `EvidenceCase` one `GhEvidence` receipt; every payload the boundary witnesses is one of these two families, so the journal adds no third truth.
- Owner: `JournalRow` readonly record struct — one appended fact under its `Sequence` ordinal, optional owning document, and the `MonotonicStamp` the session timeline captured at append; a partition's rows are monotone by construction because ONE timeline stamps every append.
- Law: stamps are layered, never merged — the kernel envelope's `Ordinal` is the drain's total publication order, the journal's `Sequence` is its own append order, and the `MonotonicStamp` orders across fact families; each answers a different question and none substitutes.
- Law: document attribution derives from the fact — a `DocumentCase` fact keys its partition through the host-published `Document.Identity`, a `GhEvidence` receipt keys the document its projector named, and an unattributable fact lands in the session partition rather than being dropped; a `GraphCase` subject id is object-instance identity and never keys a partition.
- Packages: LanguageExt.Core, `Rasm.Domain` (`Op`), `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`), `Rasm.Interaction` (`UiEvent<TFact>`), `Shell/events.md` (`GhFact`), `Shell/telemetry.md` (`GhEvidence`).
- Growth: a new journalable family is one `JournalFact` case; the row shape never widens per family.

## [03]-[FOLD]

- Owner: `JournalLedger` readonly record struct — the committed fold state: partitions keyed by document identity (the session partition rides `Guid.Empty`) beside the next sequence and the appended and shed tallies, advanced by one pure `Folded` transition. Ordinal lives INSIDE the ledger, so one `Cell.Commit` settles the row, the sequence, and the tallies as one committed value — the split commit (an interlocked counter beside a CAS) tears an export into three figures that disagree.
- Owner: `JournalExport` `[Equatable]` sealed record — the export projection: the selected rows in sequence order (`[OrderedEquality]`), the whole `JournalLedger` tally snapshot, and the capture stamp, detached from every live cell. `Signals` is the replay grounding this page promises: each row projects to its `Shell/hooks.md` `HookSignal`, so replay capture and analytics export are ONE record, never two recordings.
- Entry: `SessionJournal.Of(MonotonicTimeline clock, FaultCell faults, Option<JournalPolicy> policy = default, Op? key = null)` → `Fin<SessionJournal>` — the clock is the session's one injected timeline (folder RULINGS `[02]`; no mint here) and the fault cell is the composition's; `Append(JournalFact fact, Option<Guid> document = default, Op? key = null)` → `Fin<JournalRow>`; `Export(Option<Guid> document = default, Op? key = null)` → `Fin<JournalExport>` — `Some` exports one partition, `None` merges every partition ordered by sequence; `Mount(EvidenceDrain<GhFact> drain, …)` → `Fin<Lease<SessionJournal>>` — the off-thread drain consumer the composition root's roster names.
- Law: `Mount` owns the single-reader contract — one retained consumer task drains `ReadAllAsync` under the journal's cancellation source, its whole loop inside the kernel's ASYNC `Op.Catch` arm, so a cancelled drain keeps the `KernelFault.Cancelled` case and an unknown raise keeps its original exceptional `Error` on the composition's fault cell. Consumer stays a deliberately off-UI-thread `Task.Run`; disposal cancels, joins the task, then releases, so no unowned consumer survives its lease.
- Law: every journal fault PARKS on the injected `FaultCell` — bounded ring, `Shed` and `Lost` counted — never a newest-only `Atom<Option<Error>>`; the release one-shot is the kernel `Atom<bool>` latch through `Cell.Step`.
- Boundary: serialization, upload, and bundle formats are app-root concerns over the detached export; the journal never names a serializer or a wire.
- Packages: LanguageExt.Core (`Fin`, `Seq`, `HashMap`, `Atom`, `Cell`), .NET (`CancellationTokenSource`, `Task`), Generator.Equals, Microsoft.Extensions.Logging.Abstractions (`JournalLog`), `Rasm.Domain` (`Op`, `Lease<T>`, `FaultCell`), `Rasm.Parametric` (`MonotonicTimeline`), `Rasm.Interaction` (`EvidenceDrain<TFact>`).
- Growth: a new export slice is one filter over the one fold; a new retention posture is one `JournalPolicy` field.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Generator.Equals;
using Microsoft.Extensions.Logging;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record JournalFact {
    private JournalFact() { }
    public sealed record EventCase(UiEvent<GhFact> Fact) : JournalFact;
    public sealed record EvidenceCase(GhEvidence Evidence) : JournalFact;
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record JournalPolicy(int Capacity) {
    public static readonly JournalPolicy Default = new(Capacity: 2048);
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct JournalRow(long Sequence, Option<Guid> Document, MonotonicStamp Stamp, JournalFact Fact) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Sequence >= 0L,
        Stamp.IsValid);
}

[Equatable]
public sealed partial record JournalExport(
    [property: OrderedEquality] Seq<JournalRow> Rows,
    JournalLedger Tallies,
    MonotonicStamp Captured) {
    public Seq<HookSignal> Signals => Rows.Map(static row => row.Fact.Switch<HookSignal>(
        eventCase: static fact => new HookSignal.EventCase(Fact: fact.Fact),
        evidenceCase: static fact => new HookSignal.EvidenceCase(Evidence: fact.Evidence)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct JournalLedger(HashMap<Guid, Seq<JournalRow>> Partitions, long Next, long Appended, long Shed) {
    public static readonly JournalLedger Empty = new(Partitions: HashMap<Guid, Seq<JournalRow>>(), Next: 0L, Appended: 0L, Shed: 0L);

    internal (JournalLedger Ledger, JournalRow Row) Folded(Option<Guid> document, MonotonicStamp stamp, JournalFact fact, int capacity) {
        Guid partition = document.IfNone(Guid.Empty);
        JournalRow row = new(Sequence: Next, Document: document, Stamp: stamp, Fact: fact);
        Seq<JournalRow> grown = Partitions.Find(partition).IfNone(Seq<JournalRow>()).Add(row);
        return (new JournalLedger(
            Partitions: Partitions.AddOrUpdate(partition, grown.Count > capacity ? grown.Tail.Strict() : grown),
            Next: Next + 1L,
            Appended: Appended + 1L,
            Shed: grown.Count > capacity ? Shed + 1L : Shed), row);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class JournalLog {
    internal const int ConsumerFault = 4711;
    static JournalLog() => Op.SideWhen(
        condition: ConsumerFault != FaultBand.GrasshopperLog.Code(offset: 11),
        action: static () => throw new InvalidOperationException("JournalLog.ConsumerFault drifted from FaultBand.GrasshopperLog."));

    [LoggerMessage(EventId = ConsumerFault, Level = LogLevel.Error, Message = "Journal consumer faulted: {Detail}")]
    internal static partial void ConsumerFaulted(ILogger logger, [UserContent] string detail);
}

public sealed class SessionJournal : IDisposable {
    private readonly JournalPolicy policy;
    private readonly MonotonicTimeline clock;
    private readonly FaultCell faults;
    private readonly Atom<JournalLedger> ledger = Atom(JournalLedger.Empty);
    private readonly Atom<bool> released = Atom(false);
    private readonly CancellationTokenSource drain = new();
    private Task consuming = Task.CompletedTask;

    public JournalLedger Tallies => ledger.Value;
    public Seq<IsolatedFault> Faults => faults.Parked.Filter(static fault => fault.Point == Rail);

    public static Fin<SessionJournal> Of(
        MonotonicTimeline clock, FaultCell faults, Option<JournalPolicy> policy = default, Op? key = null);

    public static Fin<Lease<SessionJournal>> Mount(
        EvidenceDrain<GhFact> drain, MonotonicTimeline clock, FaultCell faults,
        Option<JournalPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        return from journal in Of(clock: clock, faults: faults, policy: policy, key: op)
               from mounted in op.Catch(body: () => Fin.Succ(Op.Side(action: () => journal.consuming = Task.Run(
                   async () => (await op.Catch(async token => {
                       await foreach (UiEvent<GhFact> fact in drain.Reader.ReadAllAsync(cancellationToken: token)) {
                           journal.Append(fact: new JournalFact.EventCase(Fact: fact), document: DocumentOf(fact: fact), key: op)
                               .IfFail(journal.Park);
                       }
                       return Fin.Succ(unit);
                   }, token: journal.drain.Token)).IfFail(journal.Park)))))
               select (Lease<SessionJournal>)new Lease<SessionJournal>.Owned(Value: journal);
    }

    public Fin<JournalRow> Append(JournalFact fact, Option<Guid> document = default, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in op.Need(fact)
               from live in guard(!released.Value, op.InvalidResult()).ToFin()
               from stamp in clock.Capture(key: op)
               from committed in Cell.Commit(ledger, held => held.Folded(
                       document: document, stamp: stamp, fact: valid, capacity: policy.Capacity).Ledger)
                   .Switch(
                       state: op,
                       committed: (o, row) => row.State.Partitions
                           .Find(document.IfNone(Guid.Empty))
                           .Bind(static rows => rows.Last)
                           .ToFin(o.InvalidResult()),
                       ceded: static (o, _) => Fin.Fail<JournalRow>(o.InvalidResult()),
                       refused: static (_, row) => Fin.Fail<JournalRow>(row.Cause),
                       contended: static (o, _) => Fin.Fail<JournalRow>(o.InvalidResult()))
               select committed;
    }

    public Fin<JournalExport> Export(Option<Guid> document = default, Op? key = null) {
        Op op = key.OrDefault();
        return from stamp in clock.Capture(key: op)
               let held = ledger.Value
               let rows = document.Match(
                   Some: partition => held.Partitions.Find(partition).IfNone(Seq<JournalRow>()),
                   None: () => toSeq(held.Partitions.Values.Bind(static rows => rows).OrderBy(static row => row.Sequence)))
               select new JournalExport(Rows: rows.Strict(), Tallies: held, Captured: stamp);
    }

    public void Dispose();

    private Unit Park(Error cause) {
        JournalLog.ConsumerFaulted(GhLog.For(category: nameof(SessionJournal)), cause.Message);
        return ignore(faults.Park(point: Rail, cause: cause));
    }

    private static readonly HookId Rail = HookId.Create(value: "rasm.grasshopper.shell.journal");

    private static Option<Guid> DocumentOf(UiEvent<GhFact> fact) => fact.Fact switch {
        GhFact.DocumentCase document => document.DocumentId,
        _ => Option<Guid>.None,
    };
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]         | [OWNER]                      | [RAIL]                                          | [CASES] |
| :-----: | :---------------- | :--------------------------- | :---------------------------------------------- | :-----: |
|  [01]   | fact admission    | `JournalFact` + `JournalRow` | closed union → stamped row evidence             |    2    |
|  [02]   | bounded fold      | `JournalLedger`              | one `Cell.Commit` — row, ordinal, tallies whole |    1    |
|  [03]   | drain consumption | `SessionJournal.Mount`       | async `Op.Catch` loop → `FaultCell` parks       |    1    |
|  [04]   | export + replay   | `JournalExport`              | one record — bundle rows AND `Signals` window   |    1    |

`Op`, `Lease<T>`, `FaultCell`, `MonotonicTimeline`, `EvidenceDrain<GhFact>`, `UiEvent<GhFact>`, and `GhEvidence` are composed upstream owners; retention, serialization, and upload policy compose at the app root over the detached export.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
