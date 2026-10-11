using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Slots;

using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("B5194C2A-A4C8-4FDA-B1BB-5A1AD9224F6F")]
public sealed class ScopesSection() : DefinedSection(Row) {
    public static readonly FrameLink Link = new();

    public static readonly (ControlRow Row, RowSource<ScopeState> Source) Scope =
        EntryKey.TypeOwner(typeof(ScopesSection).GUID) switch {
            var owner => ChartRows.Scope(
                "Scope",
                "Histogram, waveform, parade, vectorscope, or chromaticity of the newest rendering",
                IterableNE.create(new ScopeSource("Current", SlotComparison.Scoped)),
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
