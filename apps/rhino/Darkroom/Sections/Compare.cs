using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Rows;

using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
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
                new FramePane(SlotComparison.Sample, [], [SlotComparison.Difference], Picker, Some(ScopesSection.Link)),
                RowRules.Always),
            SlotComparison.Measures(DarkroomSettings.Comparison),
            .. TargetRows.Formation.Map(static row => (Child)row),
            SlotComparison.Export,
            SlotComparison.Settings,
        ],
    };
}
