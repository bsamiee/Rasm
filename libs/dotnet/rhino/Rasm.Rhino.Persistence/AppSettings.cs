using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AliasRow(Option<string> Alias, Option<string> Macro, bool Instant);

public sealed record ShortcutRow(KeyboardKey Key, ModifierKey Modifier, Option<string> Macro);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppSettings {
    // --- [SEPARATORS]
    private const char CommandNameSeparator = ' ';

    private const char PackageSourceSeparator = ';';

    // --- [APPEARANCE]
    public static IO<Unit> SetTheme(bool darkMode) =>
        IO.lift(() => darkMode
            ? Refused.Unless(AppearanceSettings.SetToDarkMode(), nameof(AppearanceSettings.SetToDarkMode))
            : Refused.Unless(AppearanceSettings.SetToLightMode(), nameof(AppearanceSettings.SetToLightMode)));

    public static IO<Option<Rectangle>> InitialMainWindowPosition() =>
        IO.lift(static () => Answers.Found(AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds), bounds));

    // --- [ANALYSIS]
    public static IO<CurvatureAnalysisSettingsState> CurvatureAutoRange(CurvatureAnalysisSettingsState initial, Seq<Mesh> meshes) =>
        from populated in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange)))
        from computed in IO.lift(() => {
            CurvatureAnalysisSettingsState state = initial;
            return Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange));
        })
        select computed;

    // --- [ALIASES]
    public static IO<Seq<AliasRow>> Aliases() =>
        IO.lift(static () => toSeq(Range(0, CommandAliasList.Count))
            .TraverseM(static index => Missing.Unless(CommandAliasList.GetAlias(index), nameof(CommandAliasList.GetAlias)))
            .As()
            .Map(static rows => rows.Map(static row => SettingsMapper.ToRow(row)).Strict()));

    public static IO<Option<string>> AliasMacro(string alias) =>
        from named in IO.lift(() => Invalid.Unless(alias.Length > 0, nameof(CommandAliasList.IsAlias)))
        from macro in IO.lift(() => CommandAliasList.IsAlias(alias) ? Some(CommandAliasList.GetMacro(alias)) : Option<string>.None)
        select macro;

    public static IO<Unit> DeleteAlias(string alias) =>
        from named in IO.lift(() => Invalid.Unless(alias.Length > 0, nameof(CommandAliasList.Delete)))
        from deleted in IO.lift(() => Refused.Unless(CommandAliasList.Delete(alias), nameof(CommandAliasList.Delete)))
        select deleted;

    public static IO<Unit> UpdateAliases(Seq<AliasRow> rows, bool replaceAll) =>
        from named in IO.lift(() => Invalid.Unless(rows.ForAll(static row => row.Alias.IsSome), nameof(CommandAliasList.Update)))
        from updated in IO.lift(() => CommandAliasList.Update(rows.Map(static row => new CommandAlias(row.Alias.ValueUnsafe(), row.Macro.ValueUnsafe(), row.Instant)), replaceAll))
        select updated;

    // --- [SHORTCUTS]
    public static IO<Seq<ShortcutRow>> Shortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetShortcuts()));

    public static IO<Seq<ShortcutRow>> DefaultShortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetDefaults()));

    public static IO<Unit> SetShortcut(KeyboardKey key, ModifierKey modifier, string macro) =>
        from acceptable in IO.lift(() => Refused.Unless(ShortcutKeySettings.IsAcceptableKeyCombo(key, modifier), nameof(ShortcutKeySettings.IsAcceptableKeyCombo)))
        from written in IO.lift(() => ShortcutKeySettings.SetMacro(key, modifier, macro))
        select written;

    public static IO<Unit> UpdateShortcuts(Seq<ShortcutRow> rows, bool replaceAll) =>
        from populated in IO.lift(() => Invalid.Unless(!rows.IsEmpty, nameof(ShortcutKeySettings.Update)))
        from updated in IO.lift(() => ShortcutKeySettings.Update(rows.Map(static row => new KeyboardShortcut { Key = row.Key, Modifier = row.Modifier, Macro = row.Macro.ValueUnsafe() }), replaceAll))
        select updated;

    private static Seq<ShortcutRow> Rows(KeyboardShortcut[] shortcuts) =>
        toSeq(shortcuts).Map(static row => SettingsMapper.ToRow(row)).Strict();

    // --- [NEVER_REPEAT]
    public static IO<(bool Enabled, Seq<string> Names)> NeverRepeat() =>
        IO.lift(static () => (Enabled: NeverRepeatList.UseNeverRepeatList, Names: toSeq(NeverRepeatList.CommandNames()).Filter(static name => name.Length > 0).Strict()));

    public static IO<int> SetNeverRepeat(Seq<string> names) =>
        from tokens in IO.lift(() => Tokens(names, CommandNameSeparator, nameof(NeverRepeatList.SetList)))
        from count in IO.lift(() => NeverRepeatList.SetList([.. names]))
        select count;

    // --- [FILES]
    public static IO<int> AddSearchPath(string folder, int index) =>
        from placed in IO.lift(() => unless(index == -1, IndexOutOfRange.Unless(index, FileSettings.SearchPathCount + 1, nameof(FileSettings.AddSearchPath))).As())
        from qualified in Answers.QualifiedPath(folder)
        from inserted in IO.lift(() => Answers.NonNegative(FileSettings.AddSearchPath(qualified, index), nameof(FileSettings.AddSearchPath)))
        select inserted;

    public static IO<Unit> DeleteSearchPath(string folder) =>
        IO.lift(() => Refused.Unless(FileSettings.DeleteSearchPath(folder), nameof(FileSettings.DeleteSearchPath)));

    public static IO<Option<string>> FindFile(string fileName) =>
        from named in IO.lift(() => Invalid.Unless(fileName.Length > 0, nameof(FileSettings.FindFile)))
        from found in IO.lift(() => Optional(FileSettings.FindFile(fileName)).Filter(static path => !string.IsNullOrWhiteSpace(path)))
        select found;

    public static IO<Seq<string>> AutoSaveBeforeCommands() =>
        IO.lift(static () => toSeq(FileSettings.AutoSaveBeforeCommands()));

    public static IO<Unit> SetAutoSaveBeforeCommands(Seq<string> commands) =>
        from tokens in IO.lift(() => Tokens(commands, CommandNameSeparator, nameof(FileSettings.SetAutoSaveBeforeCommands)))
        from written in IO.lift(() => FileSettings.SetAutoSaveBeforeCommands([.. commands]))
        select written;

    // --- [PACKAGES]
    public static IO<Seq<string>> PackageSources() =>
        IO.lift(static () => toSeq(PackageManagerSettings.Sources.Split(PackageSourceSeparator, StringSplitOptions.RemoveEmptyEntries)));

    public static IO<Unit> SetPackageSources(Seq<string> sources) =>
        from tokens in IO.lift(() => Tokens(sources, PackageSourceSeparator, nameof(PackageManagerSettings.Sources)))
        let joined = string.Join(PackageSourceSeparator, sources)
        from written in IO.lift(() => { PackageManagerSettings.Sources = joined; })
        from same in IO.lift(() => Mismatch.Unless(string.Equals(PackageManagerSettings.Sources, joined, StringComparison.Ordinal), nameof(PackageManagerSettings.Sources)))
        select same;

    private static Fin<Unit> Tokens(Seq<string> names, char separator, string member) =>
        Invalid.Unless(names.ForAll(name => (name.Length > 0) && !name.Contains(separator, StringComparison.Ordinal)), member);
}

[Mapper]
internal static partial class SettingsMapper {
    internal static partial AliasRow ToRow(CommandAlias alias);

    internal static partial ShortcutRow ToRow(KeyboardShortcut shortcut);

    internal static partial SettingsState ToState(PersistentSettings node);
}
