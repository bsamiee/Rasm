# [ELEMENT_FAULTS]

`ElementFault` is the contract's one closed `[Union]` fault family over structural-graph, value-admission, projection, and content-verification failures. Its seven direct `Fault` cases derive numeric identity from `FaultBand.Element` and `[FaultCase]`; no category registry or conversion layer exists.

`Detail` is self-sufficient presentation evidence. Recovery and telemetry read the generated numeric identity; no consumer parses or hashes rendered text into a second fault taxonomy.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: the direct `ElementFault` union and generated per-case numeric identity.

## [02]-[FAULT_BAND]

- Owner: `ElementFault` is the closed `[Union]` family on `FaultBand.Element`; each leaf carries one `[FaultCase]` ordinal and inherits the generated `Code` directly from `Fault`.
- Cases: `NodeAbsent` (an edge endpoint, a `Bake` root, or a replayed-`GraphDelta` reference naming an undeclared `NodeId`) · `RelationshipInvalid` (an edge whose endpoint node-kinds violate the structural edge law over `Relations/relation#EDGE_ALGEBRA` `Endpoints`, or a cyclic `Compose` ancestry) · `DeltaConflict` (a put-existing / drop-absent / duplicate-link conflicting with the working-graph state) · `ValueRejected` (an irreducible Element semantic admission refusal; generated and scalar admission stay on `KernelFault`) · `ProjectionFailed` (the contract-authored structural verdict on a projector's own delta) · `ProjectorFaulted` (a documented terminal projector-contract refusal retaining the captured `Error` as cause; unknown throws stay exceptional and provider-specific retryable refusals stay on their owner fault) · `AddressUnstable` (a content-verification mismatch — a persisted node whose recomputed content id no longer equals its stored `NodeId`; the mint path stays total and raises nothing) (7). IFC-semantic legality routes the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` → `BimFault.ModelRejected`, never re-cased here.
- Law: `[FaultCase]` ordinals are the sole per-case numeric identity; band membership derives through `FaultBand.OwnerOf(BandKind.Fault, code)`, never a category string or literal range test.
- Entry: callers construct the typed leaf directly and lift it bare onto `Fin<T>` or `Validation<Error,T>`; generated owner admission crosses through the kernel bridge unchanged.
- Auto: `Bake` routes `NodeAbsent` on an absent root and `RelationshipInvalid` on a cyclic `Compose` ancestry; `ElementGraph.Apply`/`WorkingGraph.Apply` route `NodeAbsent`, `RelationshipInvalid`, and `DeltaConflict` per the structural law; irreducible Element semantic gates route `ValueRejected`; each projector implementation returns documented refusals as its typed owner fault or an explicit `ProjectorFaulted`, while `Assemble` preserves unknown throws as their exact exceptional `Error`; `ElementHooks.Live` preserves a kernel composition refusal unchanged, while a captured subscriber failure parks as `IsolatedFault` on the hooks' `FaultCell`; `ContentAddress.Verify` routes `AddressUnstable` on a re-derived id drifting from its stored `NodeId`, the snapshot sweep accumulating every drift over `Validation<Error,Unit>`.
- Output: recovery reads the typed leaf or numeric band owner; telemetry projects `Code` and its derived owner. Retryable provider refusals retain the owning package fault that classified them.
- Packages: `Rasm` (`Fault`, `FaultBand`, `FaultCaseAttribute`), Thinktecture.Runtime.Extensions (`[Union]`, generated validation), LanguageExt.Core (`Error`/`Fin`/`Validation`), NodaTime.
- Growth: a genuinely new arm is one leaf with the next justified `[FaultCase]` ordinal; no parallel registry, category, factory, or string code is added.
- Boundary: the typed leaf lifts bare; `.ToError()`, `Error.New(code, message)`, category mirrors, and compatibility factories are deleted forms. Foreign exceptions enter only through the capture funnel and retain their opaque message as evidence.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Element.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ElementFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Element;
    private ElementFault(string detail) { Key = key; Detail = detail; }

    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)] public sealed partial record NodeAbsent(string Detail) : ElementFault(Key, Detail);
    [FaultCase(1)] public sealed partial record RelationshipInvalid(string Detail) : ElementFault(Key, Detail);
    [FaultCase(2)] public sealed partial record DeltaConflict(string Detail) : ElementFault(Key, Detail);
    [FaultCase(3)] public sealed partial record ValueRejected(string Detail) : ElementFault(Key, Detail);
    [FaultCase(4)] public sealed partial record ProjectionFailed(string Detail) : ElementFault(Key, Detail);
    [FaultCase(5)] public sealed partial record ProjectorFaulted(Error Cause)
        : ElementFault(Key, $"projector failed: {Cause.Message}"), ICausedFault;
    [FaultCase(6)] public sealed partial record AddressUnstable(string Detail) : ElementFault(Key, Detail);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
