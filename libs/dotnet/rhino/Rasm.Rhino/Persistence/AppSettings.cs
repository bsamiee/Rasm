using System.Buffers;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<RhinoFault>]
public sealed partial class CommandName {
    private static readonly SearchValues<char> Separators = SearchValues.Create(" ,;\b\v\r\n\t");

    public static Fin<CommandName> Validated(string value) => TryCreate(value, out CommandName? name, out RhinoFault? fault) ? name : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } name && !name.AsSpan().ContainsAny(Separators) ? null : new RhinoFault(value, RhinoReason.Unqualified);
        value = trimmed ?? value;
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<RhinoFault>]
public sealed partial class PackageSource {
    public const char Separator = ';';

    public static Fin<PackageSource> Validated(string value) => TryCreate(value, out PackageSource? source, out RhinoFault? fault) ? source : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        validationError = trimmed is { } source && !source.Contains(Separator, StringComparison.Ordinal) ? null : new RhinoFault(value, RhinoReason.Unqualified);
        value = trimmed ?? value;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppSettings {
    // --- [COMMANDS]
    public static Preference<Option<(string Macro, bool Instant)>> Alias(string name) =>
        Preferences.Of(
            IO.lift(() => Optional(CommandAliasList.FindAlias(name)).Map(static alias => (alias.Macro, alias.Instant))),
            entry => entry.Match(
                Some: row => IO.lift(() => CommandAliasList.Update([new CommandAlias(name, row.Macro, row.Instant)], replaceAll: false)),
                None: () => Products.Accepted(() => CommandAliasList.Delete(name))),
            Some(Preferences.AppSettingsChanged));

    public static Preference<Duration> InstantAliasDelay { get; } =
        Preferences.Of(
            IO.lift(static () => Duration.FromMilliseconds(CommandAliasList.InstantAliasDelayMilliseconds)),
            static delay => IO.lift(Conversions.Whole(delay, Duration.FromMilliseconds(1))).Bind(static milliseconds => IO.lift(() => { CommandAliasList.InstantAliasDelayMilliseconds = milliseconds; })),
            Some(Preferences.AppSettingsChanged));

    public static Preference<Option<string>> Shortcut(KeyboardKey key, ModifierKey modifier) =>
        Preferences.Of(
            IO.lift(() => toSeq(ShortcutKeySettings.GetShortcuts()).Find(row => row.Key == key && row.Modifier == modifier).Bind(static row => Conversions.Present(row.Macro))),
            macro =>
                from accepted in IO.lift(() => RhinoFault.Unless(macro.IsNone || ShortcutKeySettings.IsAcceptableKeyCombo(key, modifier), RhinoReason.Unsupported, (key, modifier)))
                from written in IO.lift(() => ShortcutKeySettings.SetMacro(key, modifier, Conversions.Unset(macro)))
                select written,
            Some(Preferences.AppSettingsChanged));

    public static Preference<Seq<CommandName>> NeverRepeat { get; } = Commands(NeverRepeatList.CommandNames, static names => _ = NeverRepeatList.SetList(names));

    public static Preference<Seq<CommandName>> AutoSaveBeforeCommands { get; } = Commands(FileSettings.AutoSaveBeforeCommands, FileSettings.SetAutoSaveBeforeCommands);

    private static Preference<Seq<CommandName>> Commands(Func<string[]?> read, Action<string[]> write) =>
        Preferences.Of(
            IO.lift(() => Conversions.Rows(read()).Choose(static name => Conversions.Present(name)).Traverse(CommandName.Validated).As()),
            names => IO.lift(() => write([.. names])),
            Some(Preferences.AppSettingsChanged));

    // --- [FILES]
    public static Preference<Option<InputPath>> TemplateFile { get; } = Location(static () => FileSettings.TemplateFile, static path => FileSettings.TemplateFile = path, InputPath.Of);

    public static Preference<Option<FolderPath>> TemplateFolder { get; } = Location(static () => FileSettings.TemplateFolder, static path => FileSettings.TemplateFolder = path, FolderPath.Of);

    public static Preference<Option<FolderPath>> BackupFileFolder { get; } = Location(static () => FileSettings.BackupFileFolder, static path => FileSettings.BackupFileFolder = path, FolderPath.Of);

    public static Preference<Seq<PackageSource>> PackageSources { get; } =
        Preferences.Of(
            IO.lift(static () => Conversions.Rows(PackageManagerSettings.Sources?.Split(PackageSource.Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Traverse(PackageSource.Validated).As()),
            static sources => IO.lift(() => { PackageManagerSettings.Sources = string.Join(PackageSource.Separator, sources.Map(static source => (string)source)); }),
            Some(Preferences.AppSettingsChanged));

    public static Preference<bool> SearchPath(string folder) =>
        Preferences.Of(
            IO.lift(() => toSeq(FileSettings.GetSearchPaths()).Exists(path => string.Equals(path, folder, StringComparison.OrdinalIgnoreCase))),
            listed => listed
                ? IO.lift(() => FolderPath.Of(folder).Bind(static existing => Conversions.Required(Conversions.Present(FileSettings.AddSearchPath(existing, Conversions.Unset(Option<int>.None)))))).Map(static _ => unit)
                : Products.Accepted(() => FileSettings.DeleteSearchPath(folder)),
            Some(Preferences.AppSettingsChanged));

    private static Preference<Option<TPath>> Location<TPath>(Func<string?> read, Action<string> write, Func<string, Fin<TPath>> located) where TPath : IConvertible<string> =>
        Preferences.Of(
            IO.lift(() => Conversions.Present(read()).Traverse(located).As()),
            path => IO.lift(() => write(Conversions.Unset(path.Map(static held => held.ToValue())))),
            Some(Preferences.AppSettingsChanged));

    // --- [NUDGE]
    public static IO<Unit> NudgeFollowsActiveDocument(Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        (from post in Mailboxes.Idle<Unit, Unit>(_ => IO.lift(static () => Optional(RhinoDoc.ActiveDoc)).Bind(active => active.Match(Some: doc => Nudge(doc, steps), None: static () => IO.pure(unit))))
         from attached in Subscriptions.Merged(
                 Documents.Arrived.Choose(static _ => Some(unit)),
                 Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.ActiveDocumentChanged += h, static h => RhinoDoc.ActiveDocumentChanged -= h).Documents().Choose(static _ => Some(unit)),
                 Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h).Documents().Choose(static _ => Some(unit)))
             .Attach(_ => post(unit, unit))
         select attached).Scoped(Session.Scope);

    private static IO<Unit> Nudge(RhinoDoc doc, Func<Length, Length, Length, (Length Nudge, Length Ctrl, Length Shift)> steps) =>
        from grid in IO.lift(doc.GetGridDefaults)
        let modelUnit = doc.ModelUnits
        let wanted = steps(Quantities.Resolution(doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision, modelUnit), Quantities.From(grid.SnapSpacing, modelUnit), Quantities.From(grid.GridSpacing, modelUnit))
        from written in IO.lift(() => {
            (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep) =
                (Quantities.As(wanted.Nudge, modelUnit), Quantities.As(wanted.Ctrl, modelUnit), Quantities.As(wanted.Shift, modelUnit));
        })
        select written;
}
