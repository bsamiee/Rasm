using System.Buffers;
using System.Drawing;
using System.Reflection;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ProcessSetting<T>(IO<T> Read, Func<T, IO<Unit>> Write) where T : notnull {
    public IO<Unit> Edit(Func<T, T> change) => Read.Map(change).Bind(Write);

    public IO<TResult> Within<TResult>(Func<T, T> change, IO<TResult> body) =>
        DisposalOps.Transacted(Read, prior => Write(change(prior)).Bind(_ => body), Write, (prior, result) => Write(prior).Map(_ => result));
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
            Store = ValueStore.Defaulted(Setting.Read, Default, Setting.Write, Same, Applied.Live, AppSettings.Changed);
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommandName {
    private static readonly SearchValues<char> Separators = SearchValues.Create(" ,;\b\v\r\n\t");

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } name && !name.AsSpan().ContainsAny(Separators) ? null : new ValidationError("command name is blank or holds space, comma, semicolon, or control character");
        value = trimmed ?? value;
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PackageSource {
    public const char Separator = ';';

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } source && !source.Contains(Separator, StringComparison.Ordinal) ? null : new ValidationError($"package source is blank or holds {Separator}");
        value = trimmed ?? value;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SettingsMapper {
    internal static partial ShortcutRow ToRow(KeyboardShortcut shortcut);
}

public static class AppSettings {
    // --- [CHANGES]
    internal static readonly Option<HostEvent<Unit>> Changed = Some(ValueStore.Signal(EventKind.AppSettingsChanged, static _ => true));

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
                return unit;
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
            static (name, _) => CommandName.Validated(name))),
        names => IO.lift(() => write([.. names])));

    // --- [FILES]
    public static readonly ProcessSetting<Option<InputPath>> TemplateFile = Location(static () => FileSettings.TemplateFile, static path => FileSettings.TemplateFile = path, InputPath.Of);
    public static readonly ProcessSetting<Option<FolderPath>> TemplateFolder = Location(static () => FileSettings.TemplateFolder, static path => FileSettings.TemplateFolder = path, FolderPath.Of);
    public static readonly ProcessSetting<Option<FolderPath>> BackupFileFolder = Location(static () => FileSettings.BackupFileFolder, static path => FileSettings.BackupFileFolder = path, FolderPath.Of);

    public static readonly ProcessSetting<Seq<PackageSource>> PackageSources = new(
        IO.lift(static () => Callbacks.Each(
            Conversions.Rows(PackageManagerSettings.Sources?.Split(PackageSource.Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)),
            static (source, _) => PackageSource.Validated(source))),
        static sources => IO.lift(() => PackageManagerSettings.Sources = string.Join(PackageSource.Separator, sources.Map(static source => (string)source))).Map(unit));

    public static ProcessSetting<bool> SearchPath(string folder) => new(
        IO.lift(() => toSeq(FileSettings.GetSearchPaths()).Exists(path => string.Equals(path, folder, StringComparison.OrdinalIgnoreCase))),
        listed => IO.lift(() => listed
            ? FolderPath.Of(folder)
                .Bind(static existing => Conversions.Required(FileSettings.AddSearchPath(existing, Conversions.Unset(Option<int>.None)), nameof(FileSettings.AddSearchPath)))
                .Map(static _ => unit)
            : Refused.Unless(FileSettings.DeleteSearchPath(folder), nameof(FileSettings.DeleteSearchPath))));

    private static ProcessSetting<Option<TPath>> Location<TPath>(Func<string?> read, Action<string> write, Func<string, Fin<TPath>> located) where TPath : IConvertible<string> => new(
        IO.lift(() => Conversions.Present(read()).Traverse(located).As()),
        path => IO.lift(() => write(Conversions.Unset(path.Map(static held => held.ToValue())))));

    // --- [NUDGE]
    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> NudgeFollowsActiveDocument(Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        (sink, retain) => Subscriptions.Host(typeof(AppSettings), static () => Seq(
                EventKind.DocumentArrived.Choose(static _ => Some((unit, unit))),
                EventKind.ActiveDocumentChanged.Choose(static _ => Some((unit, unit))),
                EventKind.DocumentPropertiesChanged.Choose(static _ => Some((unit, unit)))))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => NudgeActive(steps)), sink, retain);

    private static IO<Unit> NudgeActive(Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        IO.lift(static () => Optional(RhinoDoc.ActiveDoc)).Bind(active => active.Match(Some: doc => Nudge(doc, steps), None: static () => IO.pure(unit)));

    private static IO<Unit> Nudge(RhinoDoc doc, Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        from display in DistanceDisplay.Read(doc, modelUnits: true)
        from grid in IO.lift(doc.GetGridDefaults)
        let modelUnit = doc.ModelUnits
        let wanted = steps(display.Resolution(modelUnit), Quantities.From(grid.SnapSpacing, modelUnit), Quantities.From(grid.GridSpacing, modelUnit))
        let target = (Quantities.As(wanted.Nudge, modelUnit), Quantities.As(wanted.Ctrl, modelUnit), Quantities.As(wanted.Shift, modelUnit))
        from written in IO.lift(() => {
            (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep) = target;
        })
        select written;

    // --- [LIFETIME]
    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Hold<T>(ProcessSetting<T> setting, T value) where T : notnull, IEquatable<T> =>
        (_, retain) =>
            setting.Read.Bind(prior => DisposalOps.Acquire(DisposalOps.OnFailure(when(!prior.Equals(value), setting.Write(value)).As(), setting.Write(prior)),
                _ => setting.Read.Bind(current => when(current.Equals(value) && !prior.Equals(value), setting.Write(prior)).As()), retain));
}
