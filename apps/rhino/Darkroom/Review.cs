using System.Runtime.InteropServices;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("B5194C2A-A4C8-4FDA-B1BB-5A1AD9224F6F")]
public sealed class ScopesSection() : DefinedSection(Row) {
    public static readonly FrameLink Link = new();

    public static readonly (ControlRow Row, RowSource<ScopeState> Source) Scope =
        EntryKey.TypeOwner(typeof(ScopesSection).GUID) switch {
            var owner => ChartRows.Scope(
                "Scope",
                "Histogram, waveform, parade, vectorscope, or chromaticity of the newest rendering",
                IterableNE.create(new ScopeSource("Current", RenderingComparison.Scoped)),
                owner,
                scope => PlugInSettings.Record<ScopeState>(((IPlugInViews)scope.Sink).Settings.Child(owner), Applied.Live),
                Some(Link),
                RowRules.Always),
        };

    public static readonly View.Section Row = new() {
        Identity = typeof(ScopesSection),
        Caption = "Scopes",
        Children = [Scope.Row],
    };
}

[Guid("2F07A9A9-F8A2-4B52-86EE-73085EAF6A1F")]
public sealed class CompareSection() : DefinedSection(Row) {
    public static readonly FramePicker Picker = new(Some(ScopesSection.Scope.Source));

    public static readonly View.Section Row = new() {
        Identity = typeof(CompareSection),
        Caption = "Compare",
        Children = [
            PreviewRows.Frame(
                "Frame",
                "Newest rendering wiped against the history slot chosen for comparison",
                new FramePane(RenderingComparison.Sample, [], [RenderingComparison.Difference], Picker, Some(ScopesSection.Link)),
                RowRules.Always),
            RenderingComparison.Measures(ComparisonSettingRows.Default),
            .. TargetRows.Formation,
            RenderingComparison.Export,
            RenderingComparison.Settings,
        ],
    };
}

[Guid("F9092DA1-C8A1-4935-A3D8-0E800F9EDC45")]
public sealed class HistorySection() : DefinedSection(Row) {
    public static readonly View.Section Row = new() {
        Identity = typeof(HistorySection),
        Caption = "Render History",
        Children = [RenderHistory.Browser],
    };
}
