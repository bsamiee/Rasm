using System.Runtime.InteropServices;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rhino.UI.Controls;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("B5194C2A-A4C8-4FDA-B1BB-5A1AD9224F6F")]
internal static class ScopesSection {
    public static readonly (Func<IterableNE<ScopeSource>, ControlRow> Row, RowSource<ScopeState> Source) Scope =
        EntryKey.TypeOwner(typeof(ScopesSection).GUID) switch {
            var owner => ChartRows.Scope(
                "Scope",
                "Chart of stage input formed for display",
                owner,
                scope => PlugInSettings.Record<ScopeState>(((IPlugInViews)scope.Sink).Settings.Child(owner), Applied.Live),
                RowRules.Always),
        };

    public static readonly Func<EtoPostEffectCollapsibleSection, IO<Option<ScopeFrames>>, View.Section> Row = static (_, frames) => new() {
        Identity = typeof(ScopesSection),
        Caption = "Scopes",
        Children = [Scope.Row(IterableNE.create(new ScopeSource("Formed", _ => frames)))],
    };
}

[Guid("2F07A9A9-F8A2-4B52-86EE-73085EAF6A1F")]
public sealed class CompareSection(IPlugInViews owner) : DefinedSection(Row, owner) {
    // --- [SETTINGS]
    public static readonly ComparisonSettingRows Settings = new(
        PsnrAdvisory: new(ValueKey.Numeric("ComparePsnrAdvisory", "PSNR advisory", "PSNR in decibels below which compared renders grade advisory", BarLevel.PsnrAdvisory,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 })), SettingType.Double),
        PsnrRefuse: new(ValueKey.Numeric("ComparePsnrRefuse", "PSNR refuse", "PSNR in decibels below which compared renders are refused", BarLevel.PsnrRefuse,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 })), SettingType.Double),
        NrmseAdvisory: new(ValueKey.Numeric("CompareNrmseAdvisory", "NRMSE advisory", "Normalized error above which compared renders grade advisory", BarLevel.NrmseAdvisory,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Decimals = 2 })), SettingType.Double),
        NrmseRefuse: new(ValueKey.Numeric("CompareNrmseRefuse", "NRMSE refuse", "Normalized error above which compared renders are refused", BarLevel.NrmseRefuse,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Decimals = 2 })), SettingType.Double));

    public static Seq<Child> Page => [
        new ControlRow.Group("Render comparison", RowRules.Always),
        .. Seq(Settings.PsnrAdvisory, Settings.PsnrRefuse, Settings.NrmseAdvisory, Settings.NrmseRefuse).Bind(static row => SectionRows.Setting(row, RowRules.Always)),
    ];

    // --- [PANEL]
    public static readonly FramePicker Picker = new(Some(ScopesSection.Scope.Source));

    private static readonly (RowSource<bool> Source, RowField<bool> Field) Difference = RowSource.Opaque(nameof(RenderingComparison), nameof(Difference), @default: false);

    public static readonly View.Section Row = new() {
        Identity = typeof(CompareSection),
        Caption = "Compare",
        Children = [
            PreviewRows.Frame(
                "Frame",
                "Newest rendering wiped against compared rendering",
                new FramePane(static scope => RenderingComparison.Sample(scope, scope.Read(Difference.Source)), [], Framed, Picker, "Newest rendering appears here"),
                RowRules.Always),
            TextRows.Readout("Measure", "PSNR and NRMSE against newest rendering, graded by comparison settings",
                new ReadoutSource<Option<(FrameComparison Measured, Verdict Verdict)>>.Scoped(static scope => RenderingComparison.Graded(scope, Settings)),
                static graded => graded.Match(Some: Measured, None: static () => ""),
                RowRules.Always),
            SectionRows.Pushed(new Command<RowRule, Invoke>.Run(
                new CommandFace("comparison-export", "Export displayed frame", Some("Write displayed callback or history frame to output targets"), None), None, new Invoke.Effect(RenderingComparison.Export))),
            ListRows.List("Render Settings", "Compared rendering's saved render settings against current document values", new ListRow<SettingsRow, EntryKey>(
                    IterableNE.create<ListColumn<SettingsRow>>(
                        new ListColumn<SettingsRow>.Label(Some("Setting"), static row => row.Setting, None, None),
                        new ListColumn<SettingsRow>.Label(Some("Saved"), static row => row.A.IfNone(""), None, None),
                        new ListColumn<SettingsRow>.Label(Some("Current"), static row => row.B.IfNone(""), None, None)),
                    static row => row.Key,
                    RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Items), Seq<SettingsRow>(), RenderingComparison.Items),
                    RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Selected), LanguageExt.HashSet<EntryKey>.Empty)),
                RowRules.Always),
        ],
    };

    private static Seq<Seq<Command<RowRule, Invoke>>> Framed(FrameCommands frame) => [
        [
            CommandRows.Toggle(new CommandFace("comparison-difference", "Difference", Some("Tints frame where compared rendering differs from newest rendering"), None), None,
                RowRule.When(Difference.Source, static on => on, Difference.Field.Parameter), static (scope, on) => scope.Store(Difference.Source).Put(Some(on)).Map(unit)),
        ],
        [
            new Command<RowRule, Invoke>.Menu(new CommandFace("frame-compare-modes", "Compare modes", None, None), None, [
                [
                    new Command<RowRule, Invoke>.Choice(new CommandFace("frame-compare-mode", "Compare mode", None, None), None, toSeq(Rasm.UI.Imaging.CompareMode.Items).Map(mode => frame.Mode(mode).Faced(
                        new CommandFace($"frame-{mode.Key}", ((ICaptioned)mode).Caption, None,
                            Some(mode.Map(verticalWipe: DarkroomGlyphs.VerticalWipe, horizontalWipe: DarkroomGlyphs.HorizontalWipe, sideBySide: DarkroomGlyphs.SideBySide))),
                        Some(mode.Map(verticalWipe: Eto.Forms.Keys.Shift | Eto.Forms.Keys.Y, horizontalWipe: Eto.Forms.Keys.W, sideBySide: Eto.Forms.Keys.Y))))),
                ],
                [frame.Swap.Faced(new CommandFace("frame-invert-wipe", "Invert wipe", None, Some(DarkroomGlyphs.Flip)), Some(Eto.Forms.Keys.Alt | Eto.Forms.Keys.W))],
            ]) { Primary = frame.Compare.Faced(new CommandFace("frame-compare", "Compare", None, None), Some(Eto.Forms.Keys.Backslash)) },
            frame.Swap.Faced(new CommandFace("frame-swap", "Swap", None, Some(DarkroomGlyphs.Swap)), Some(Eto.Forms.Keys.Enter)),
        ],
        [
            frame.Fit.Faced(new CommandFace("frame-fit", "Fit", None, Some(DarkroomGlyphs.Fit)), None),
            frame.ActualSize.Faced(new CommandFace("frame-actual-size", "Actual Size", None, Some(DarkroomGlyphs.ActualSize)), None),
        ],
    ];

    private static string Measured((FrameComparison Measured, Verdict Verdict) graded) =>
        RowText.Localize("{0}, {1}, {2}", arguments: [
            RowText.Localize("PSNR {0:F1} dB", arguments: [graded.Measured.Psnr]).Local,
            graded.Measured.Nrmse.Match(Some: static nrmse => RowText.Localize("NRMSE {0:F3}", arguments: [nrmse]).Local, None: static () => RowText.Localize("NRMSE unmeasured").Local),
            graded.Verdict.Map(pass: RowText.Localize("Pass"), advisory: RowText.Localize("Advisory"), unmeasured: RowText.Localize("Unmeasured"), refuse: RowText.Localize("Refused")).Local,
        ]).Local;
}

[Guid("F9092DA1-C8A1-4935-A3D8-0E800F9EDC45")]
public sealed class HistorySection(IPlugInViews owner) : DefinedSection(Row, owner) {
    // --- [SETTINGS]
    public static readonly HistorySettingRows Settings = new(
        Capacity: new(ValueKey.Numeric("HistorySteps", "Undo steps", "Edits per section for undo, redo, and restore, 0 disables history", HistoryLimits.Default.Capacity,
            new NumericDomain<int, int>(Fin.Succ, identity, (0, int.MaxValue), new() { Form = NumberForm.Field, Soft = Some((1, 100)) })), SettingType.Integer),
        Window: new(ValueKey.Numeric("HistoryMergeWindow", "Merge edits within", "Joins consecutive section edits into one undo step",
            (int)HistoryLimits.Default.Window.TotalSeconds,
            new NumericDomain<int, int>(Fin.Succ, identity, (0, int.MaxValue), new() { Form = NumberForm.Field, Soft = Some((0, 600)), Unit = Quantity.GetUnitInfo(DurationUnit.Second) })),
            SettingType.Integer));

    public static Seq<Child> Page => [
        new ControlRow.Group("Section history", RowRules.Always),
        .. Seq(Settings.Capacity, Settings.Window).Bind(static row => SectionRows.Setting(row, RowRules.Always)),
    ];

    // --- [PANEL]
    public static readonly View.Section Row = new() {
        Identity = typeof(HistorySection),
        Caption = "Render History",
        Store = Some(RenderHistory.Store),
        Actions = [ListRows.Arrangement(BrowserMode.List), SectionCommands.Values([RenderHistory.Browser]), SectionCommands.Reset],
        Children = [RenderHistory.Browser],
    };
}
