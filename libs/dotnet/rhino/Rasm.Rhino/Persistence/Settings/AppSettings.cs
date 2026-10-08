using System.Buffers;
using System.Drawing;
using System.Reflection;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.PlugIns;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ProcessSetting<T>(IO<T> Read, Func<T, IO<Unit>> Write) where T : notnull {
    public IO<Unit> Edit(Func<T, T> change) => Read.Map(change).Bind(Write);

    public IO<TResult> Within<TResult>(Func<T, T> change, IO<TResult> body) =>
        from prior in Read
        from result in DisposalOps.OnFailure(Edit(change).Bind(_ => body), Write(prior))
        from restored in Write(prior)
        select result;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppSettingsFamily<TSettings> where TSettings : class {
    private static readonly Seq<PropertyInfo> Members = toSeq(typeof(TSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance)).Strict();

    private AppSettingsFamily(Func<TSettings> factory) => Default = IO.lift(factory);

    public IO<TSettings> Default { get; }

    public abstract IO<TSettings> Current { get; }

    private static bool Same(TSettings held, TSettings wanted) =>
        Members.ForAll(member => (member.GetValue(held), member.GetValue(wanted)) switch {
            (Color left, Color right) => Conversions.Same(left, right),
            var (left, right) => Equals(left, right),
        });

    public sealed record ReadOnly : AppSettingsFamily<TSettings> {
        public ReadOnly(Func<TSettings> current, Func<TSettings> factory) : base(factory) => Current = IO.lift(current);

        public override IO<TSettings> Current { get; }
    }

    public sealed record Writable : AppSettingsFamily<TSettings> {
        public Writable(Func<TSettings> current, Func<TSettings> factory, Action<TSettings> update) : base(factory) {
            Setting = new ProcessSetting<TSettings>(IO.lift(current), state => IO.lift(() => update(state)));
            Store = new ValueStore<TSettings>(
                from held in Setting.Read
                from initial in Default
                select Same(held, initial) ? Option<TSettings>.None : Some(held),
                value => value.Match(Some: static state => IO.pure(state), None: () => Default).Bind(Setting.Write),
                Same,
                Applied.Live,
                Some(ValueStore.Signal(EventKind.AppSettingsChanged, static _ => true)));
        }

        public ProcessSetting<TSettings> Setting { get; }

        public ValueStore<TSettings> Store { get; }

        public override IO<TSettings> Current => Setting.Read;
    }
}

public sealed record HistorySwitches(bool RecordingEnabled, bool RecordNextCommand, bool UpdateEnabled, bool ObjectLockingEnabled, bool BrokenRecordWarningEnabled);

public sealed record DefinedViewSettings(bool DefinedViewSetCPlane, bool DefinedViewSetProjection, bool DefinedViewSetClippingPlanes, bool DefinedViewSetDisplayMode);

public sealed record ShortcutRow(KeyboardKey Key, ModifierKey Modifier, Option<string> Macro);

[ValueObject<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommandName {
    private static readonly SearchValues<char> Separators = SearchValues.Create(" ,;\b\v\r\n\t");

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } name && !name.AsSpan().ContainsAny(Separators) ? null : new InvalidRhinoValue();
        value = trimmed ?? value;
    }
}

[ValueObject<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PackageSource {
    public const char Separator = ';';

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } source && !source.Contains(Separator, StringComparison.Ordinal) ? null : new InvalidRhinoValue();
        value = trimmed ?? value;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SettingsMapper {
    internal static partial ShortcutRow ToRow(KeyboardShortcut shortcut);
}

public static class AppSettings {
    // --- [FAMILIES]
    public static readonly AppSettingsFamily<AppearanceSettingsState>.Writable Appearance = new(AppearanceSettings.GetCurrentState, AppearanceSettings.GetDefaultState, AppearanceSettings.UpdateFromState);
    public static readonly AppSettingsFamily<ChooseOneObjectSettingsState>.Writable ChooseOneObject = new(ChooseOneObjectSettings.GetCurrentState, ChooseOneObjectSettings.GetDefaultState, ChooseOneObjectSettings.UpdateFromState);
    public static readonly AppSettingsFamily<CursorTooltipSettingsState>.Writable CursorTooltip = new(CursorTooltipSettings.GetCurrentState, CursorTooltipSettings.GetDefaultState, CursorTooltipSettings.UpdateFromState);
    public static readonly AppSettingsFamily<CurvatureAnalysisSettingsState>.Writable CurvatureAnalysis = new(CurvatureAnalysisSettings.GetCurrentState, CurvatureAnalysisSettings.GetDefaultState, CurvatureAnalysisSettings.UpdateFromState);
    public static readonly AppSettingsFamily<CurvatureGraphSettingsState>.Writable CurvatureGraph = new(CurvatureGraphSettings.GetCurrentState, CurvatureGraphSettings.GetDefaultState, CurvatureGraphSettings.UpdateFromState);
    public static readonly AppSettingsFamily<DraftAngleAnalysisSettingsState>.Writable DraftAngleAnalysis = new(DraftAngleAnalysisSettings.GetCurrentState, DraftAngleAnalysisSettings.GetDefaultState, DraftAngleAnalysisSettings.UpdateFromState);
    public static readonly AppSettingsFamily<EdgeAnalysisSettingsState>.Writable EdgeAnalysis = new(EdgeAnalysisSettings.GetCurrentState, EdgeAnalysisSettings.GetDefaultState, EdgeAnalysisSettings.UpdateFromState);
    public static readonly AppSettingsFamily<FileSettingsState>.Writable File = new(FileSettings.GetCurrentState, FileSettings.GetDefaultState, FileSettings.UpdateFromState);
    public static readonly AppSettingsFamily<GumballSettingsState>.Writable Gumball = new(GumballSettings.GetCurrentState, GumballSettings.GetDefaultState, GumballSettings.UpdateFromState);
    public static readonly AppSettingsFamily<ModelAidSettingsState>.Writable ModelAid = new(ModelAidSettings.GetCurrentState, ModelAidSettings.GetDefaultState, ModelAidSettings.UpdateFromState);
    public static readonly AppSettingsFamily<OpenGLSettingsState>.Writable OpenGL = new(OpenGLSettings.GetCurrentState, OpenGLSettings.GetDefaultState, OpenGLSettings.UpdateFromState);
    public static readonly AppSettingsFamily<SelectionFilterSettingsState>.Writable SelectionFilter = new(SelectionFilterSettings.GetCurrentState, SelectionFilterSettings.GetDefaultState, SelectionFilterSettings.UpdateFromState);
    public static readonly AppSettingsFamily<SoftTransformSettingsState>.Writable SoftTransform = new(SoftTransformSettings.GetCurrentState, SoftTransformSettings.GetDefaultState, SoftTransformSettings.UpdateFromState);
    public static readonly AppSettingsFamily<ViewSettingsState>.Writable View = new(ViewSettings.GetCurrentState, ViewSettings.GetDefaultState, ViewSettings.UpdateFromState);
    public static readonly AppSettingsFamily<ZebraAnalysisSettingsState>.Writable ZebraAnalysis = new(ZebraAnalysisSettings.GetCurrentState, ZebraAnalysisSettings.GetDefaultState, ZebraAnalysisSettings.UpdateFromState);
    public static readonly AppSettingsFamily<GeneralSettingsState>.ReadOnly General = new(GeneralSettings.GetCurrentState, GeneralSettings.GetDefaultState);
    public static readonly AppSettingsFamily<DirectionAnalysisSettingsState>.ReadOnly DirectionAnalysis = new(DirectionAnalysisSettings.GetCurrentState, DirectionAnalysisSettings.GetDefaultState);
    public static readonly AppSettingsFamily<EmapAnalysisSettingsState>.ReadOnly EmapAnalysis = new(EmapAnalysisSettings.GetCurrentState, EmapAnalysisSettings.GetDefaultState);
    public static readonly AppSettingsFamily<EndAnalysisSettingsState>.ReadOnly EndAnalysis = new(EndAnalysisSettings.GetCurrentState, EndAnalysisSettings.GetDefaultState);

    // --- [SWITCHES]
    public static readonly ProcessSetting<HistorySwitches> History = new(
        IO.lift(static () => new HistorySwitches(
            HistorySettings.RecordingEnabled, HistorySettings.RecordNextCommand, HistorySettings.UpdateEnabled,
            HistorySettings.ObjectLockingEnabled, HistorySettings.BrokenRecordWarningEnabled)),
        static switches => IO.lift(() => {
            (HistorySettings.RecordingEnabled, HistorySettings.RecordNextCommand, HistorySettings.UpdateEnabled) =
                (switches.RecordingEnabled, switches.RecordNextCommand, switches.UpdateEnabled);
            (HistorySettings.ObjectLockingEnabled, HistorySettings.BrokenRecordWarningEnabled) = (switches.ObjectLockingEnabled, switches.BrokenRecordWarningEnabled);
        }));

    public static readonly ProcessSetting<DefinedViewSettings> DefinedViews = new(
        IO.lift(static () => new DefinedViewSettings(
            ViewSettings.DefinedViewSetCPlane, ViewSettings.DefinedViewSetProjection, ViewSettings.DefinedViewSetClippingPlanes, ViewSettings.DefinedViewSetDisplayMode)),
        static facets => IO.lift(() => {
            (ViewSettings.DefinedViewSetCPlane, ViewSettings.DefinedViewSetProjection) = (facets.DefinedViewSetCPlane, facets.DefinedViewSetProjection);
            (ViewSettings.DefinedViewSetClippingPlanes, ViewSettings.DefinedViewSetDisplayMode) = (facets.DefinedViewSetClippingPlanes, facets.DefinedViewSetDisplayMode);
        }));

    // --- [COMMANDS]
    public static readonly IO<Seq<CommandAlias>> Aliases =
        IO.lift(static () => toSeq(Range(0, CommandAliasList.Count)).Map(CommandAliasList.GetAlias).Strict());

    public static readonly ProcessSetting<Duration> InstantAliasDelay = new(
        IO.lift(static () => Duration.FromMilliseconds(CommandAliasList.InstantAliasDelayMilliseconds)),
        static delay => IO.lift(Conversions.Whole(delay, Duration.FromMilliseconds(1)))
            .Bind(static milliseconds => IO.lift(() => { CommandAliasList.InstantAliasDelayMilliseconds = milliseconds; })));

    public static readonly IO<Seq<ShortcutRow>> Shortcuts =
        IO.lift(static () => toSeq(ShortcutKeySettings.GetShortcuts()).Map(SettingsMapper.ToRow).Strict());

    public static readonly IO<Seq<ShortcutRow>> DefaultShortcuts =
        IO.lift(static () => toSeq(ShortcutKeySettings.GetDefaults()).Map(SettingsMapper.ToRow).Strict());

    public static readonly ProcessSetting<Seq<CommandName>> NeverRepeat = CommandList(NeverRepeatList.CommandNames, static names => _ = NeverRepeatList.SetList(names));

    public static readonly ProcessSetting<Seq<CommandName>> AutoSaveBeforeCommands = CommandList(FileSettings.AutoSaveBeforeCommands, FileSettings.SetAutoSaveBeforeCommands);

    public static ProcessSetting<Option<(string Macro, bool Instant)>> Alias(string name) => new(
        IO.lift(() => Optional(CommandAliasList.FindAlias(name)).Map(static alias => (alias.Macro, alias.Instant))),
        entry => IO.lift(() => entry.Match(
            Some: row => {
                CommandAliasList.Update([new CommandAlias(name, row.Macro, row.Instant)], replaceAll: false);
                return Fin.Succ(unit);
            },
            None: () => Refused.Unless(CommandAliasList.Delete(name), nameof(CommandAliasList.Delete)))));

    public static ProcessSetting<Option<string>> Shortcut(KeyboardKey key, ModifierKey modifier) => new(
        Shortcuts.Map(rows => rows.Find(row => row.Key == key && row.Modifier == modifier).Bind(static row => row.Macro)),
        macro =>
            from accepted in IO.lift(() => Refused.Unless(macro.IsNone || ShortcutKeySettings.IsAcceptableKeyCombo(key, modifier), nameof(ShortcutKeySettings.IsAcceptableKeyCombo)))
            from written in IO.lift(() => ShortcutKeySettings.SetMacro(key, modifier, Conversions.Unset(macro)))
            select written);

    private static ProcessSetting<Seq<CommandName>> CommandList(Func<string[]?> read, Action<string[]> write) => new(
        IO.lift(() => Callbacks.Each(
            Conversions.Rows(read()).Choose(static name => Conversions.Present(name)),
            static (name, _) => Conversions.Validated<CommandName, string, InvalidRhinoValue>(name))),
        names => IO.lift(() => write([.. names])));

    // --- [FILES]
    public static readonly ProcessSetting<Option<string>> TemplateFile = Location(static () => FileSettings.TemplateFile, static path => FileSettings.TemplateFile = path, Exchange.ExistingPath);
    public static readonly ProcessSetting<Option<string>> TemplateFolder = Location(static () => FileSettings.TemplateFolder, static path => FileSettings.TemplateFolder = path, Exchange.ExistingFolder);
    public static readonly ProcessSetting<Option<string>> BackupFileFolder = Location(static () => FileSettings.BackupFileFolder, static path => FileSettings.BackupFileFolder = path, Exchange.ExistingFolder);

    public static readonly ProcessSetting<Seq<PackageSource>> PackageSources = new(
        IO.lift(static () => Callbacks.Each(
            Conversions.Rows(PackageManagerSettings.Sources?.Split(PackageSource.Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)),
            static (source, _) => Conversions.Validated<PackageSource, string, InvalidRhinoValue>(source))),
        static sources => IO.lift(() => PackageManagerSettings.Sources = string.Join(PackageSource.Separator, sources.Map(static source => (string)source))).Map(static _ => unit));

    public static ProcessSetting<bool> SearchPath(string folder) => new(
        IO.lift(() => toSeq(FileSettings.GetSearchPaths()).Exists(path => string.Equals(path, folder, StringComparison.OrdinalIgnoreCase))),
        listed => IO.lift(() => listed
            ? Exchange.QualifiedPath(folder)
                .Bind(static qualified => Conversions.Required(FileSettings.AddSearchPath(qualified, Conversions.Unset(Option<int>.None)), nameof(FileSettings.AddSearchPath)))
                .Map(static _ => unit)
            : Refused.Unless(FileSettings.DeleteSearchPath(folder), nameof(FileSettings.DeleteSearchPath))));

    private static ProcessSetting<Option<string>> Location(Func<string?> read, Action<string> write, Func<string, Fin<string>> existing) => new(
        IO.lift(() => Conversions.Present(read())),
        path => IO.lift(() => path.Traverse(existing).As()).Bind(checkedPath => IO.lift(() => write(Conversions.Unset(checkedPath)))));

    // --- [NUDGE]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> NudgeFollowsActiveDocument(Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        (_, sink) => new HostEvent<(Unit, Unit)>(typeof(AppSettings), nameof(NudgeFollowsActiveDocument), static (deliver, site) =>
                DisposalOps.AcquireAll(
                        Seq(
                            EventKind.DocumentArrived.Inline(_ => deliver((unit, unit)), site.Sink),
                            EventKind.ActiveDocumentChanged.Inline(_ => deliver((unit, unit)), site.Sink),
                            EventKind.DocumentPropertiesChanged.Inline(_ => deliver((unit, unit)), site.Sink)),
                        DisposalOps.Release)
                    .Map(held => DisposalOps.Composite(held, site)))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => NudgeActive(steps)), sink);

    private static IO<Unit> NudgeActive(Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        IO.lift(static () => Optional(RhinoDoc.ActiveDoc)).Bind(active => active.Match(Some: doc => Nudge(doc, steps), None: static () => IO.pure(unit)));

    private static IO<Unit> Nudge(RhinoDoc doc, Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        from display in DistanceDisplay.Read(doc, modelUnits: true)
        from grid in IO.lift(doc.GetGridDefaults)
        let modelUnit = doc.ModelUnits
        let wanted = steps(display.Resolution(modelUnit), Quantities.From(grid.SnapSpacing, modelUnit), Quantities.From(grid.GridSpacing, modelUnit))
        let target = (Quantities.As(wanted.Nudge, modelUnit), Quantities.As(wanted.Ctrl, modelUnit), Quantities.As(wanted.Shift, modelUnit))
        from held in IO.lift(static () => (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep))
        from written in when(held != target, IO.lift(() => {
            (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep) = target;
        })).As()
        select written;

    // --- [LIFETIME]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Hold<T>(ProcessSetting<T> setting, T value) where T : notnull, IEquatable<T> =>
        (_, sink) =>
            from prior in setting.Read
            from written in when(!prior.Equals(value), setting.Write(value)).As()
            select (IDisposable)new Disposal<T>(prior, held => Callbacks.Succeeded(
                setting.Read.Bind(current => when(current.Equals(value) && !held.Equals(value), setting.Write(held)).As()),
                new CallbackSite(sink, typeof(AppSettings), nameof(Hold))));
}
