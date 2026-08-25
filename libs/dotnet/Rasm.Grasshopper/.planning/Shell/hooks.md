# [RASM_GRASSHOPPER_SHELL_HOOKS]

Folder's hook estate is the kernel mechanism composed, never re-rolled: `GrasshopperPoint` realizes `IHookRoster<GrasshopperPoint>` and the rail is one `HookRail<GrasshopperPoint, HookSignal, HookScope>` per plugin composition, minted at `Platform/composition.md`'s mount roster and handed to every raise site as a required parameter. Veto admission, observe isolation, replay retention, bounded fault custody, and scoped release are all the kernel capsule's (`Rasm/Domain/hooks.md`); this page owns the point census with its host-ruled modalities, the closed `HookSignal` fact union with its seating fan, and the per-plugin `HookScope` key — and nothing else, per the branch one-mechanism ruling.

Every declared point lands its FIRE SITE in the same estate (branch RULINGS `[02]`): the census below names the raising owner per row, and a row no raise reaches has no seat here. `Shell/events.md` `UiEvents` remains the raw host-event gate — the rail is governance (veto, replay, plugin taps), never a second event wire.

## [01]-[INDEX]

- [02]-[POINTS]: `GrasshopperPoint` — the host-truthful roster realizing the kernel floor, each row carrying its modality set, plane, and fire site.
- [03]-[RAIL]: `HookScope` + `HookSignal` — the owner key, the closed fact union, and the composition law over the kernel rail.

## [02]-[POINTS]

- Owner: `GrasshopperPoint` `[SmartEnum<string>]` realizing `IHookRoster<GrasshopperPoint>` — the closed `rasm.grasshopper.<domain>.<point>` roster whose `Modalities` column is the kernel `CapabilitySet<HookModality>` and whose `Plane` is the folder's one `TraceScope`. Veto capability is ruled per row from the host's actual cancellation surface, never wished into existence: the document transaction gate admits refusal pre-commit, the background paint raise carries `CanvasBackgroundPaintEventArgs.OverrideDefaultPainting`, `Window.Closing` and `Application.Terminating` carry `CancelEventArgs`, and interaction verdicts refuse at the responder gate — every other host stream is post-facto and rides the kernel evidence drain, not this rail.

| [INDEX] | [POINT]                                | [MODALITIES] | [HOST_TRUTH]                            | [FIRE_SITE]                          |
| :-----: | :------------------------------------- | :----------- | :-------------------------------------- | :----------------------------------- |
|  [01]   | `rasm.grasshopper.document.mutate`     | `Veto`       | undo-sealed gates refuse pre-commit     | `Transact` + `GraphScope.Mutate`     |
|  [02]   | `rasm.grasshopper.solution.lifecycle`  | `Observe`    | host args carry no cancellation         | `SolutionControl.Drive`              |
|  [03]   | `rasm.grasshopper.interaction.verdict` | `Veto`       | verdicts refuse at this gate            | `SpecResponder.Governed`             |
|  [04]   | `rasm.grasshopper.paint.background`    | `Veto`       | `OverrideDefaultPainting` suppresses    | `PaintAnchor.Herald` background arm  |
|  [05]   | `rasm.grasshopper.history.replay`      | `Replay`     | `HistoryLedger` replays sealed actions  | `HistoryLedger.Commit` replay arms   |
|  [06]   | `rasm.grasshopper.window.close`        | `Veto`       | `Closing` carries `CancelEventArgs`     | `HookBridge.Closing` consult arm     |
|  [07]   | `rasm.grasshopper.shell.terminate`     | `Veto`       | `Terminating` carries `CancelEventArgs` | `HookBridge.Terminating` consult arm |

- Law: a point's modality set is admission — the kernel capsule's own gates read `CanVeto` off the held rows, so a veto gate on an observe-only point refuses typed at the kernel and no per-point identity probe exists here.
- Law: NAMED LOSS — the former `document.state`, `graph.membership`, and `paint.layer` observe rows DELETE: all three were post-facto host streams with no veto, replay, or grant semantics to earn a rail seat — the first two ride the kernel evidence drain in total order, and layer-paint cadence rides the drain's `CanvasSignal.Draw` row or the plugin's own `PaintAnchor` mount. Plugin wanting those facts folds `EvidenceDrain.Reader`.
- Packages: Thinktecture.Runtime.Extensions, `Rasm.Domain` (`HookId`, `TraceScope`, `HookModality`, `CapabilitySet`, `IHookRoster`).
- Growth: a new hook point is one row with its ruled modality set and its fire site landed in the same change; a mis-ruled modality is a defect against the host surface, never a configuration choice.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GrasshopperPoint : IHookRoster<GrasshopperPoint> {
    public static readonly GrasshopperPoint DocumentMutate = new(key: "rasm.grasshopper.document.mutate", modalities: Veto);
    public static readonly GrasshopperPoint SolutionLifecycle = new(key: "rasm.grasshopper.solution.lifecycle", modalities: Observe);
    public static readonly GrasshopperPoint InteractionVerdict = new(key: "rasm.grasshopper.interaction.verdict", modalities: Veto);
    public static readonly GrasshopperPoint PaintBackground = new(key: "rasm.grasshopper.paint.background", modalities: Veto);
    public static readonly GrasshopperPoint HistoryReplay = new(key: "rasm.grasshopper.history.replay", modalities: Replay);
    public static readonly GrasshopperPoint WindowClose = new(key: "rasm.grasshopper.window.close", modalities: Veto);
    public static readonly GrasshopperPoint ShellTerminate = new(key: "rasm.grasshopper.shell.terminate", modalities: Veto);

    public HookId Id => HookId.Create(value: Key);
    public CapabilitySet<HookModality> Modalities { get; }
    public Option<TraceScope> Plane => Some(TraceScope.Create(value: "rasm.grasshopper.host"));

    private static CapabilitySet<HookModality> Veto => CapabilitySet<HookModality>.Of(HookModality.Veto);
    private static CapabilitySet<HookModality> Observe => CapabilitySet<HookModality>.Of(HookModality.Observe);
    private static CapabilitySet<HookModality> Replay => CapabilitySet<HookModality>.Of(HookModality.Replay);
}
```

## [03]-[RAIL]

- Owner: `HookScope` `[ValueObject<string>]` — the per-plugin namespace and the rail's `TOwner`: the kernel signature keys every subscription and release by it, and because per-ALC isolation bounds each rail to ONE plugin, `rail.Release(scope, key)` and whole-rail teardown coincide here — the scope is the typed owner handle the kernel demands, never a multi-plugin filter, and a collectible plugin ALC drops its whole rail with the root's lease. `HookSignal` `[Union]` realizing the kernel `IHookFact<GrasshopperPoint>` floor — the roster's closed `TFact` carrying its own seating fan: `EventCase` carries a kernel-ordered `UiEvent<GhFact>` (`Shell/events.md`), `IntentCase` the pre-commit `Op` beside the host-published document identity a veto point judges.
- Entry: the rail is `HookRail<GrasshopperPoint, HookSignal, HookScope>.Of(key, gates, taps, span, cell)` — minted ONCE by `Platform/composition.md`'s mount roster with the composition's own `FaultCell`, never a page static. Raise site takes the rail as a REQUIRED parameter and calls `rail.Fire(at, fact, key)` — the kernel's guarded transforming arity has no lawful site on this roster (the refusal-only law below); replay is `rail.Replay(at, captured, key)` over a retaining point; scoped teardown is `rail.Release(scope, key)`.
- Law: fan-out and ordering are the kernel's — `Fire` delivers to every subscriber of the point's one seat, vetoes fold in ATTACH order and the mount census is mount-ordered (kernel S1-26), so the veto left-fold is deterministic without a rank column here. NAMED LOSS: the scoped-delivery raise arm (`Raise(point, signal, Some(scope))`) — per-ALC isolation already bounded every rail to one plugin, so the arm filtered a single-scope table; a raise is now always the rail's whole seat.
- Law: veto verdicts ride the rail — `Fire` answers `Fin<HookSignal>` and the raise site consults it before committing its host mutation; the two `CancelEventArgs` points write `args.Cancel = true` on the `Fail` leg at their `Shell/events.md` bridge, which is the one host readback the fact projection cannot carry.
- Law: seating is the FACT's own declaration and the kernel gates on it twice, at fire entry and on the veto fold's product — `IntentCase` is what every `[02]` fire site raises, so it seats at the whole roster; `EventCase` enters only through `rail.Replay` and seats where the row retains, which is `history.replay` alone today. Firing a journal case at a live veto point refuses before any gate runs, and a gate rewriting an intent into a journal case refuses before the raise site reads its verdict.
- Law: every veto on this roster is REFUSAL-ONLY — the host surfaces judge commit-or-cancel and carry no mutable payload for a subscriber to rewrite, so the kernel's guarded transforming `Fire<T>` arity has no lawful site here; a future transformable seam earns it as a new point row, never by widening `IntentCase`.
- Law: fault custody is the composition's `FaultCell` — bounded ring, oldest-out, `Shed` and `Lost` counted — handed whole at the mint; the cell's tap writes each parked `IsolatedFault` through `Shell/telemetry.md`'s `GhInstruments.Hooked` and no page-local `Atom<Seq<IsolatedFault>>`, cap const, or trim fold exists.
- Law: replay capture comes from `Shell/journal.md` `SessionJournal.Export` or the `HistoryLedger` action stream, re-fired in captured order through the kernel's `TraverseM` verdict rail, so `Ok` certifies the whole window re-fired and a late-mounted panel reads the recent path without a second recording surface.
- Boundary: raise sites are the owning pages named in the `[02]` census — this page owns the roster and the fact union, never a raise; fire is synchronous, so an effect-rail raise site lifts at its own composition seam (`IO.lift(() => rail.Fire(...))`).
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, `Rasm.Domain` (`HookRail`, `IHookFact`, `HookMounts`, `FaultCell`, `Op`), `Shell/events.md` (`GhFact`, `UiEvent<GhFact>`).
- Growth: zero on the mechanism — new capability lands as `GrasshopperPoint` rows and `HookSignal` cases, a case declaring the arm of the seating fan it answers to; the kernel rail never widens per folder.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Thinktecture;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct HookScope {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "HookScope requires a nonblank plugin namespace.");
    }
}

[Union]
public abstract partial record HookSignal : IHookFact<GrasshopperPoint> {
    private HookSignal() { }
    public sealed record EventCase(UiEvent<GhFact> Fact) : HookSignal;
    public sealed record IntentCase(Op Operation, Option<Guid> DocumentId) : HookSignal;

    public bool Seats(GrasshopperPoint at) => Switch(
        state: at,
        eventCase: static (row, _) => Replayable(at: row),
        intentCase: static (_, _) => true);

    private static bool Replayable(GrasshopperPoint at) => at.Modalities.Held.Exists(static row => row.Retains);
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]    | [OWNER]                                           | [RAIL]                                         | [CASES] |
| :-----: | :----------- | :------------------------------------------------ | :--------------------------------------------- | :-----: |
|  [01]   | point census | `GrasshopperPoint : IHookRoster`                  | keyed rows, kernel modality sets, fire sites   |    7    |
|  [02]   | payload      | `HookSignal : IHookFact`                          | closed union + seating fan → the raise fold    |    2    |
|  [03]   | rail         | kernel `HookRail<GrasshopperPoint, …, HookScope>` | `Fire`/`Replay`/`Release` — zero local members |    1    |

137-line `GhHooks` registry — seats, ranks, trim fold, fault cap, detacher tracking, release folds — deleted whole onto the kernel rail; a new governance capability lands as a point row or a signal case.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
