# [BIM_FAULTS]

`BimFault` is the BIM package's direct kernel fault family. A refusal carries a closed scope and reason; a failed foreign boundary carries its original `Error` whole. Consumers accumulate independent failures as `Validation<Error, T>` and `Error.Many`, never through a package-local aggregate arm.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `BimFault`, `BimScope`, `BimReason`, and `BimBoundary`.

## [02]-[FAULT_BAND]

- Owner: `BimFault` the direct `FaultBand.Bim` union.
- Cases: `Refused` for typed BIM contract failures; `BoundaryFailed` for an exact foreign `Error`.
- Entry: construct the case at the raising site and lift it bare onto `Fin` or `Validation<Error, T>`.
- Auto: `[FaultCase]` generates the codes 2600 and 2601 from the family band.
- Law: scope and reason drive routing; `Detail` is evidence only. Independent failures accumulate through `Error.Many`.
- Receipt: the typed fault itself.
- Packages: Rasm kernel fault substrate, LanguageExt.Core, and Thinktecture.Runtime.Extensions.
- Growth: add a scope, reason, or boundary row only when the axis is closed; do not add token rosters, category mirrors, factories, or aggregate cases.
- Boundary: geometry and structural failures retain their own families; native and codec errors remain the exact `Cause`.

```csharp signature
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Bim.Model;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimScope {
    public static readonly BimScope Format         = new("format");
    public static readonly BimScope Import         = new("import");
    public static readonly BimScope Export         = new("export");
    public static readonly BimScope Wire           = new("wire");
    public static readonly BimScope Events         = new("events");
    public static readonly BimScope Tessellation   = new("tessellation");
    public static readonly BimScope Reconstruct    = new("reconstruct");
    public static readonly BimScope Energy         = new("energy");
    public static readonly BimScope Model          = new("model");
    public static readonly BimScope Projection     = new("projection");
    public static readonly BimScope Semantics      = new("semantics");
    public static readonly BimScope Planning       = new("planning");
    public static readonly BimScope Review         = new("review");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimReason {
    public static readonly BimReason Codec              = new("codec");
    public static readonly BimReason Rejected           = new("rejected");
    public static readonly BimReason Capability         = new("capability");
    public static readonly BimReason Unmapped           = new("unmapped");
    public static readonly BimReason DanglingReference = new("dangling-reference");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimBoundary {
    public static readonly BimBoundary HoneybeeJson             = new("honeybee-json", Retriability.Terminal);
    public static readonly BimBoundary DragonflyJson            = new("dragonfly-json", Retriability.Terminal);
    public static readonly BimBoundary OpenStudioRaise          = new("openstudio-raise", Retriability.Terminal);
    public static readonly BimBoundary OpenStudioInMemoryDecode = new("openstudio-in-memory-decode", Retriability.Terminal);
    public static readonly BimBoundary HostScratchWrite         = new("host-scratch-write", Retriability.Transient);
    public static readonly BimBoundary OpenStudioPathDecode     = new("openstudio-path-decode", Retriability.Terminal);
    public static readonly BimBoundary TessellationCompanion    = new("tessellation-companion", Retriability.Transient);

    private BimBoundary(string key, Retriability posture) : this(key) => Posture = posture;

    public Retriability Posture { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Bim;
    private BimFault() { }

    [FaultCase(0)]
    public sealed partial record Refused(Op Key, BimScope Scope, BimReason Reason, string Detail) : BimFault {
        public override string Message => Detail;
    }

    [FaultCase(1)]
    public sealed partial record BoundaryFailed(BimBoundary Boundary, Error Cause) : BimFault, ICausedFault {
        public override string Message => $"BIM boundary failed: {Boundary.Key}: {Cause.Message}";
        public override Retriability Retriability => Boundary.Posture;
    }
}
```

## [03]-[RESEARCH]

(none)
