# [APPUI_CONTRACT_WIRE_PROOF]

AppUI wire snapshots assert deterministic branch-owned producer output outside the production package. The subject is the generated `Ui.V1.LayoutProgram` sequence emitted by `Shell/solver#TS_PROJECTION`; the shared AppHost `WireJson` registry supplies the sole ProtoJSON spelling.

## [01]-[LAYOUT_PROTOJSON]

- Owner: `LayoutWireGolden` — the one snapshot assertion over `LayoutWireCases.ProtoJson`; `ContractEnumProof` — generated-enum roster equality for every smart-enum row consumed by the control and layout projections; `VerifyRegistration` — the assembly's sole diff registration.
- Law: production owns constraint expansion and generated-message projection; this test owns only the committed artifact and assertion. No second layout builder, schema, formatter, or case roster exists here.
- Entry: `LayoutWireGolden.Canonical()` snapshots the ordered canonical programs already rendered through `WireJson.Formatter`; `ContractEnumProof.RowsAreComplete()` rejects a missing or duplicated smart-enum wire coordinate.
- Packages: Rasm.AppUi (project), Verify.XunitV3, Verify.DiffPlex, Avalonia.Headless.XUnit
- Growth: a canonical-serialization change moves the reviewed snapshot; a new generated enum value breaks the row proof until the behavioral owner seats it; zero production test dependency.
- Boundary: this is a branch-local serialization golden, not a claim that the current TypeScript app executes the program or agrees on solver output. Cross-language schema and binding proof remains in `tests/contracts/`; this page does not duplicate it. The smart-enum proof compares the behavioral rows to generated nonzero values and never re-declares either roster.

```csharp signature
// --- [PROOF] --------------------------------------------------------------------------------
public static class VerifyRegistration {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize() => VerifyDiffPlex.Initialize();
}

public static class LayoutWireGolden {
    [Xunit.Fact]
    public static Task Canonical() =>
        VerifyXunit.Verifier.Verify(LayoutWireCases.ProtoJson);
}

public static class ContractEnumProof {
    [Xunit.Fact]
    public static void RowsAreComplete() {
        Exact(ControlEmphasis.Items.Select(static row => row.Wire));
        Exact(ControlTrigger.Items.Select(static row => row.Wire));
        Exact(SelectPosture.Items.Select(static row => row.Wire));
        Exact(MultiPosture.Items.Select(static row => row.Wire));
        Exact(SegmentPosture.Items.Select(static row => row.Wire));
        Exact(ChipPosture.Items.Select(static row => row.Wire));
        Exact(ColorPosture.Items.Select(static row => row.Wire));
        Exact(BannerSeverity.Items.Select(static row => row.Wire));
        Exact(BannerPlacement.Items.Select(static row => row.Wire));
        Exact(ProgressForm.Items.Select(static row => row.Wire));
        Exact(TemporalKind.Items.Select(static row => row.Wire));
        Exact(NumericKind.Items.Select(static row => row.Wire));
        Exact(MenuPosture.Items.Select(static row => row.Wire));
        Exact(TypographyRole.Items.Select(static row => row.Wire));
        Exact(ExtentMode.Items.Select(static row => row.Wire));
        Exact(OverviewAxis.Items.Select(static row => row.Wire));
        Exact(LayoutRelation.Items.Select(static row => row.Wire));
        Exact(LayoutStrength.Items.Select(static row => row.Wire));
    }

    private static void Exact<TWire>(IEnumerable<TWire> rows) where TWire : struct, Enum {
        TWire[] values = rows.ToArray();
        HashSet<TWire> actual = values.ToHashSet();
        HashSet<TWire> expected = Enum.GetValues<TWire>()
            .Where(static value => Convert.ToUInt64(value) != 0UL)
            .ToHashSet();

        Xunit.Assert.True(
            actual.Count == values.Length && expected.SetEquals(actual),
            $"{typeof(TWire).Name}: expected [{string.Join(", ", expected)}], rows [{string.Join(", ", values)}]");
    }
}
```
