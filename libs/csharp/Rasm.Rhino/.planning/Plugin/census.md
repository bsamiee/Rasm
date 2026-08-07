# [RASM_RHINO_PLUGIN_CENSUS]

`PluginCensus.Ask` answers every read of the host's installed-plug-in registry — identity resolution, the registry descriptor, presence, load protection, command rosters, and the installed roll — behind one polymorphic entry over a request union. `PluginRegistry.Commit` owns the two registry mutations, load and protection, so a read never loads a plug-in as a side effect of asking about it.

`PluginKey` (`Document/events#HOOK_REGISTRY`) keys every row; a bare `Guid` never crosses a signature here. The identity hexad the host publishes as six free-standing statics — `IdFromName`, `IdFromPath`, `IdFromFileName`, `NameFromPath`, `PathFromId`, `PathFromName` — is six cases on one query union, never six entrypoints, because the whole family is one resolution keyed by which coordinate the caller holds. `PluginKinds` mirrors `Document/tables#TARGET_ALGEBRA`'s `ObjectKind`/`ObjectKinds` shape: a keyed row per host flag, an admitted set, and one OR-fold read only where a host member takes the raw flag.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `PluginKind`, `PluginKinds`, `PluginSchedule`, `PluginRoster`, `PluginNaming`, and `LoadProtection` close every registry axis as keyed rows.
- [03]-[DESCRIPTOR]: `PluginInfo`, `PluginContact`, `PluginSlot`, `PluginPresence`, and `PluginProtection` detach the registry record before it leaves the boundary.
- [04]-[QUERY]: `PluginQuery` and `PluginAnswer` close the read family and its admission.
- [05]-[CENSUS]: `PluginCensus.Ask` dispatches every read arm and folds the installed roll.
- [06]-[ADMISSION]: `PluginAct`, `PathLoadVerdict`, `PluginReceipt`, and `PluginRegistry.Commit` own load and load-protection mutation.

## [02]-[VOCABULARY]

- Owner: `PluginKind` `[SmartEnum<PlugInType>]` is the plug-in kind vocabulary and `PluginKinds` its admitted set; `Mask` is the one OR-fold and the only place a raw `PlugInType` is spelled.
- Law: the `None` row exists so a host read is total — `PlugInInfo.PlugInType` answers `None` for a record whose kind the manager never resolved, and a vocabulary missing that row would refuse a row the registry genuinely holds.
- Owner: `PluginSchedule` `[SmartEnum<PlugInLoadTime>]` mirrors the host load schedule completely, including the two composite rows whose ordinals are not powers of two.
- Law: `PluginRoster` collapses the host's `loaded`/`unloaded` boolean pair into three rows, so an all-false pair that can only answer an empty roster is unrepresentable.
- Law: `PluginNaming`, `LoadNotice`, `LoadForce`, and `LoadProtection` each replace one host boolean argument, so no call site passes a bare `true` whose meaning lives in the parameter name.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.PlugIns;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<PlugInType>]
public sealed partial class PluginKind {
    public static readonly PluginKind None = new(key: PlugInType.None);
    public static readonly PluginKind Render = new(key: PlugInType.Render);
    public static readonly PluginKind FileImport = new(key: PlugInType.FileImport);
    public static readonly PluginKind FileExport = new(key: PlugInType.FileExport);
    public static readonly PluginKind Digitizer = new(key: PlugInType.Digitizer);
    public static readonly PluginKind Utility = new(key: PlugInType.Utility);
    public static readonly PluginKind DisplayPipeline = new(key: PlugInType.DisplayPipeline);
    public static readonly PluginKind DisplayEngine = new(key: PlugInType.DisplayEngine);
    public static readonly PluginKind Any = new(key: PlugInType.Any);
}

[ComplexValueObject]
public sealed partial class PluginKinds {
    public FrozenSet<PluginKind> Values { get; }

    // `PlugInType` is a flag enum, so a kind set IS its OR-fold; the mask leaves this owner only at a host member
    // that takes the raw flag, and no caller re-derives the fold.
    internal PlugInType Mask => toSeq(Values).Fold(PlugInType.None, static (mask, kind) => mask | kind.Key);

    public static PluginKinds Every { get; } = Create(values: FrozenSet.ToFrozenSet([PluginKind.Any]));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<PluginKind> values) =>
        validationError = values is null || values.Count is 0 || values.Any(static kind => kind is null)
            ? new ValidationError(message: "Plugin kind set is empty.")
            : null;

    public static Fin<PluginKinds> Of(Op? key, params ReadOnlySpan<PluginKind> values) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PluginKinds>(
            fault: Validate(toSeq(values.ToArray()).ToFrozenSet(), out PluginKinds? admitted),
            admitted: admitted);
    }
}

[SmartEnum<PlugInLoadTime>]
public sealed partial class PluginSchedule {
    public static readonly PluginSchedule Disabled = new(key: PlugInLoadTime.Disabled);
    public static readonly PluginSchedule AtStartup = new(key: PlugInLoadTime.AtStartup);
    public static readonly PluginSchedule WhenNeeded = new(key: PlugInLoadTime.WhenNeeded);
    public static readonly PluginSchedule WhenNeededIgnoringDockingBars = new(key: PlugInLoadTime.WhenNeededIgnoreDockingBars);
    public static readonly PluginSchedule WhenNeededOrOptionsDialog = new(key: PlugInLoadTime.WhenNeededOrOptionsDialog);
    public static readonly PluginSchedule WhenNeededOrTabbedDockBar = new(key: PlugInLoadTime.WhenNeededOrTabbedDockBar);
}

[SmartEnum]
public sealed partial class PluginRoster {
    public static readonly PluginRoster Loaded = new(loaded: true, unloaded: false);
    public static readonly PluginRoster Unloaded = new(loaded: false, unloaded: true);
    public static readonly PluginRoster Every = new(loaded: true, unloaded: true);

    internal bool IsLoaded { get; }
    internal bool IsUnloaded { get; }
}

[SmartEnum<bool>]
public sealed partial class PluginNaming {
    public static readonly PluginNaming English = new(false);
    public static readonly PluginNaming Localized = new(true);
}

[SmartEnum<bool>]
public sealed partial class LoadNotice {
    public static readonly LoadNotice Announced = new(false);
    public static readonly LoadNotice Quiet = new(true);
}

[SmartEnum<bool>]
public sealed partial class LoadForce {
    public static readonly LoadForce Lazy = new(false);
    public static readonly LoadForce Forced = new(true);
}

[SmartEnum<bool>]
public sealed partial class LoadProtection {
    public static readonly LoadProtection Prompted = new(false);
    public static readonly LoadProtection Silent = new(true);
}
```

## [03]-[DESCRIPTOR]

- Owner: `PluginInfo` is the detached registry record; the live `PlugInInfo` handle wraps a native pointer and never leaves the resolving call.
- Law: every registry string reads through a native accessor that answers null on absence, so each optional coordinate crosses as `Option<string>` and only `Name` refuses typed when blank — a nameless registry row is not addressable.
- Law: `FileTypeDescriptions` and `FileTypeExtensions` are two INDEPENDENT native rosters, so the record carries them apart; pairing them positionally invents a correspondence the host never guarantees.
- Owner: `PluginPresence` carries the `PlugInExists` triple whole — installed, loaded, load-protected — because absence is an answer, not a fault.
- Owner: `PluginProtection` carries only a resolved state; a registry record the host cannot read refuses typed rather than reporting a fabricated default.
- Boundary: the plug-in icon is a registry read — `PlugIn.Icon(Size)` is a non-virtual forward to `PlugInInfo.Icon`, so the icon crosses as `Lease<DrawingBitmap>.Owned` under caller custody and no lifecycle hook produces it.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PluginContact(
    Option<string> Organization,
    Option<string> Address,
    Option<string> Country,
    Option<string> Email,
    Option<string> Phone,
    Option<string> Fax,
    Option<string> WebSite,
    Option<string> UpdateUrl);

public sealed record PluginInfo(
    PluginKey Plugin,
    string Name,
    Option<string> Description,
    Option<string> Version,
    Option<string> FileName,
    Option<string> RegistryPath,
    PluginKind Kind,
    PluginSchedule Schedule,
    bool Loaded,
    bool ShipsWithRhino,
    bool Managed,
    PluginContact Contact,
    Seq<string> CommandNames,
    Seq<string> FileTypeDescriptions,
    Seq<string> FileTypeExtensions);

public sealed record PluginSlot(PluginKey Plugin, string Name);

public sealed record PluginPresence(PluginKey Plugin, bool Installed, bool Loaded, bool LoadProtected);

public sealed record PluginProtection(PluginKey Plugin, LoadProtection Behavior);
```

## [04]-[QUERY]

- Owner: `PluginQuery` closes every registry read; the six identity cases are the host's resolution hexad seated as data.
- Law: admission runs before any host call — text coordinates pass `Op.AcceptText`, identity coordinates pass `PluginKey.Admit`, and set coordinates prove non-null, so a query that cannot resolve never reaches the native manager.
- Law: an unresolved identity or path is `None`, not a fault — the host answers `Guid.Empty` or an empty string for an unknown coordinate, and those sentinels are projected away at the arm rather than surfacing as a value.
- Boundary: `Descriptor` is the ONE arm that touches `PlugInInfo`; every other arm reads a free-standing static, so the native record's lifetime never spans two arms.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginQuery {
    private PluginQuery() { }
    public sealed record ByName(string Name) : PluginQuery;
    public sealed record ByPath(string Path) : PluginQuery;
    public sealed record ByFileName(string FileName) : PluginQuery;
    public sealed record NameOfPath(string Path) : PluginQuery;
    public sealed record PathOfKey(PluginKey Plugin) : PluginQuery;
    public sealed record PathOfName(string Name) : PluginQuery;
    public sealed record Descriptor(PluginKey Plugin) : PluginQuery;
    public sealed record Presence(PluginKey Plugin) : PluginQuery;
    public sealed record Protection(PluginKey Plugin) : PluginQuery;
    public sealed record CommandNames(PluginKey Plugin) : PluginQuery;
    public sealed record Installed(PluginNaming Naming) : PluginQuery;
    public sealed record InstalledNames(PluginKinds Kinds, PluginRoster Roster, PluginNaming Naming) : PluginQuery;
    public sealed record Folders : PluginQuery;
    public sealed record Tally : PluginQuery;

    internal Fin<PluginQuery> Admit(Op op) => Switch(
        op,
        byName: static (key, row) => key.AcceptText(value: row.Name).Map<PluginQuery>(name => new ByName(Name: name)),
        byPath: static (key, row) => key.AcceptText(value: row.Path).Map<PluginQuery>(path => new ByPath(Path: path)),
        byFileName: static (key, row) => key.AcceptText(value: row.FileName)
            .Map<PluginQuery>(file => new ByFileName(FileName: file)),
        nameOfPath: static (key, row) => key.AcceptText(value: row.Path).Map<PluginQuery>(path => new NameOfPath(Path: path)),
        pathOfKey: static (key, row) => row.Plugin.Admit(key).Map<PluginQuery>(_ => row),
        pathOfName: static (key, row) => key.AcceptText(value: row.Name).Map<PluginQuery>(name => new PathOfName(Name: name)),
        descriptor: static (key, row) => row.Plugin.Admit(key).Map<PluginQuery>(_ => row),
        presence: static (key, row) => row.Plugin.Admit(key).Map<PluginQuery>(_ => row),
        protection: static (key, row) => row.Plugin.Admit(key).Map<PluginQuery>(_ => row),
        commandNames: static (key, row) => row.Plugin.Admit(key).Map<PluginQuery>(_ => row),
        installed: static (key, row) => guard(row.Naming is not null, key.InvalidInput()).ToFin().Map<PluginQuery>(_ => row),
        installedNames: static (key, row) => guard(
            row.Kinds is not null && row.Roster is not null && row.Naming is not null,
            key.InvalidInput()).ToFin().Map<PluginQuery>(_ => row),
        folders: static (_, row) => Fin.Succ<PluginQuery>(value: row),
        tally: static (_, row) => Fin.Succ<PluginQuery>(value: row));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginAnswer {
    private PluginAnswer() { }
    public sealed record Identity(Option<PluginKey> Value) : PluginAnswer;
    public sealed record Text(Option<string> Value) : PluginAnswer;
    public sealed record Descriptor(PluginInfo Value) : PluginAnswer;
    public sealed record Presence(PluginPresence Value) : PluginAnswer;
    public sealed record Protection(PluginProtection Value) : PluginAnswer;
    public sealed record Roster(Seq<PluginSlot> Value) : PluginAnswer;
    public sealed record Names(Seq<string> Value) : PluginAnswer;
    public sealed record Tally(int Value) : PluginAnswer;
}
```

## [05]-[CENSUS]

- Entry: `PluginCensus.Ask(PluginQuery, Op?)` is the single read entry; a new registry read is one union case and one arm.
- Law: `Descriptor` folds the whole registry record in one pass — a caller holding a `PluginInfo` never re-enters the census for a coordinate the record already carries.
- Law: the descriptor's kind and schedule project through `TryGet` against the host value; an ordinal the vocabulary does not carry refuses typed rather than defaulting to a row the registry never reported.
- Law: `Icon` is a separate leased entry, not a `PluginInfo` field, because a bitmap is caller-disposed custody and a record field would make its lifetime ambient.
- Boundary: every native string crosses through `Op.Text` and every registry identity through `PluginKey.Maybe`, so a host null, an empty string, and a `Guid.Empty` are each the same typed absence.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PluginCensus {
    public static Fin<PluginAnswer> Ask(PluginQuery query, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(query)
            .Bind(request => request.Admit(op))
            .Bind(request => request.Switch(
                op,
                byName: static (held, row) => Resolved(PlugIn.IdFromName(pluginName: row.Name), held),
                byPath: static (held, row) => Resolved(PlugIn.IdFromPath(pluginPath: row.Path), held),
                byFileName: static (held, row) => Resolved(PlugIn.IdFromFileName(filename: row.FileName), held),
                nameOfPath: static (held, row) => Named(() => PlugIn.NameFromPath(pluginPath: row.Path), held),
                pathOfKey: static (held, row) => Named(() => PlugIn.PathFromId(pluginId: row.Plugin.ToValue()), held),
                pathOfName: static (held, row) => Named(() => PlugIn.PathFromName(pluginName: row.Name), held),
                descriptor: static (held, row) => Descriptor(row.Plugin, held),
                presence: static (held, row) => held.Catch(() => {
                    bool installed = PlugIn.PlugInExists(
                        id: row.Plugin.ToValue(),
                        loaded: out bool loaded,
                        loadProtected: out bool loadProtected);
                    return Fin.Succ<PluginAnswer>(value: new PluginAnswer.Presence(Value: new PluginPresence(
                        Plugin: row.Plugin,
                        Installed: installed,
                        Loaded: loaded,
                        LoadProtected: loadProtected)));
                }),
                protection: static (held, row) => held.Catch(() =>
                    PlugIn.GetLoadProtection(pluginId: row.Plugin.ToValue(), loadSilently: out bool silent)
                        ? Fin.Succ<PluginAnswer>(value: new PluginAnswer.Protection(Value: new PluginProtection(
                            Plugin: row.Plugin,
                            Behavior: silent ? LoadProtection.Silent : LoadProtection.Prompted)))
                        : Fin.Fail<PluginAnswer>(error: held.MissingContext())),
                commandNames: static (held, row) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Names(Value: toSeq(PlugIn.GetEnglishCommandNames(pluginId: row.Plugin.ToValue())).Strict()))),
                installed: static (held, row) => held.Catch(() => toSeq(PlugIn.GetInstalledPlugIns(localizedPlugInName: row.Naming.Key))
                    .Traverse(entry => PluginKey.Maybe(entry.Key)
                        .ToFin(Fail: held.InvalidResult(detail: nameof(PluginSlot)))
                        .Map(plugin => new PluginSlot(Plugin: plugin, Name: entry.Value ?? string.Empty)))
                    .As()
                    .Map<PluginAnswer>(static rows => new PluginAnswer.Roster(Value: rows.Strict()))),
                installedNames: static (held, row) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Names(Value: toSeq(PlugIn.GetInstalledPlugInNames(
                        typeFilter: row.Kinds.Mask,
                        loaded: row.Roster.IsLoaded,
                        unloaded: row.Roster.IsUnloaded,
                        localizedPlugInName: row.Naming.Key)).Strict()))),
                folders: static (held, _) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Names(Value: toSeq(PlugIn.GetInstalledPlugInFolders()).Strict()))),
                tally: static (held, _) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Tally(Value: PlugIn.InstalledPlugInCount)))));
    }

    // The bitmap is a fresh host allocation on every call, so it crosses owned; a null answer is an absent icon,
    // never an empty image the caller would then have to test.
    public static Fin<Lease<DrawingBitmap>> Icon(PluginKey plugin, DrawingSize size, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in plugin.Admit(op)
               from __ in guard(size.Width > 0 && size.Height > 0, op.InvalidInput()).ToFin()
               from record in Record(plugin, op)
               from bitmap in op.Catch(() => Optional(record.Icon(size: size)).ToFin(Fail: op.MissingContext()))
               select (Lease<DrawingBitmap>)new Lease<DrawingBitmap>.Owned(Value: bitmap);
    }

    private static Fin<PluginAnswer> Resolved(Guid native, Op op) => op.Catch(() => Fin.Succ<PluginAnswer>(
        value: new PluginAnswer.Identity(Value: PluginKey.Maybe(native))));

    private static Fin<PluginAnswer> Named(Func<string> read, Op op) => op.Catch(() => Fin.Succ<PluginAnswer>(
        value: new PluginAnswer.Text(Value: Op.Text(read()))));

    private static Fin<PlugInInfo> Record(PluginKey plugin, Op op) => op.Catch(() =>
        Optional(PlugIn.GetPlugInInfo(pluginId: plugin.ToValue())).ToFin(Fail: op.MissingContext()));

    private static Fin<PluginAnswer> Descriptor(PluginKey plugin, Op op) =>
        from record in Record(plugin, op)
        from info in op.Catch(() =>
            from name in op.AcceptText(value: record.Name)
            from kind in op.Row<PlugInType, PluginKind>(record.PlugInType)
            from schedule in op.Row<PlugInLoadTime, PluginSchedule>(record.PlugInLoadTime)
            select new PluginInfo(
                Plugin: plugin,
                Name: name,
                Description: Op.Text(record.Description),
                Version: Op.Text(record.Version),
                FileName: Op.Text(record.FileName),
                RegistryPath: Op.Text(record.RegistryPath),
                Kind: kind,
                Schedule: schedule,
                Loaded: record.IsLoaded,
                ShipsWithRhino: record.ShipsWithRhino,
                Managed: record.IsDotNet,
                Contact: new PluginContact(
                    Organization: Op.Text(record.Organization),
                    Address: Op.Text(record.Address),
                    Country: Op.Text(record.Country),
                    Email: Op.Text(record.Email),
                    Phone: Op.Text(record.Phone),
                    Fax: Op.Text(record.Fax),
                    WebSite: Op.Text(record.WebSite),
                    UpdateUrl: Op.Text(record.UpdateUrl)),
                CommandNames: toSeq(record.CommandNames).Strict(),
                FileTypeDescriptions: toSeq(record.FileTypeDescriptions).Strict(),
                FileTypeExtensions: toSeq(record.FileTypeExtensions).Strict()))
        select (PluginAnswer)new PluginAnswer.Descriptor(Value: info);
}
```

## [06]-[ADMISSION]

- Owner: `PluginAct` closes registry mutation — load by path, load by identity, and load-protection assignment.
- Entry: `PluginRegistry.Commit(PluginAct, Op?)` is the one mutation entry; the two host load overloads are two cases on it, never two entrypoints.
- Law: `PathLoadVerdict` mirrors `LoadPlugInResult` whole, so "already loaded" stays a distinct success rather than collapsing into the plain success arm.
- Law: identity-keyed load answers a bare `bool`, so a false answer refuses typed with the requested key as detail; the host publishes no richer reason on that overload.
- Law: `SetLoadProtection` returns nothing and the host publishes no failure signal, so the receipt reports the assignment the boundary made and a caller wanting the settled state re-reads `PluginQuery.Protection`.
- Boundary: loading a plug-in runs its `OnLoad` inside this call, so a `Commit` is a host lifecycle event, never a query — this is exactly why the read family carries no load flag.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<LoadPlugInResult>]
public sealed partial class PathLoadVerdict {
    public static readonly PathLoadVerdict Loaded = new(key: LoadPlugInResult.Success);
    public static readonly PathLoadVerdict AlreadyLoaded = new(key: LoadPlugInResult.SuccessAlreadyLoaded);
    public static readonly PathLoadVerdict Refused = new(key: LoadPlugInResult.ErrorUnknown);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginAct {
    private PluginAct() { }
    public sealed record LoadPath(string Path) : PluginAct;
    public sealed record LoadKey(PluginKey Plugin, LoadNotice Notice, LoadForce Force) : PluginAct;
    public sealed record Protect(PluginKey Plugin, LoadProtection Behavior) : PluginAct;

    internal Fin<PluginAct> Admit(Op op) => Switch(
        op,
        loadPath: static (key, row) => key.AcceptText(value: row.Path).Map<PluginAct>(path => new LoadPath(Path: path)),
        loadKey: static (key, row) => row.Plugin.Admit(key)
            .Bind(_ => guard(row.Notice is not null && row.Force is not null, key.InvalidInput()).ToFin())
            .Map<PluginAct>(_ => row),
        protect: static (key, row) => row.Plugin.Admit(key)
            .Bind(_ => guard(row.Behavior is not null, key.InvalidInput()).ToFin())
            .Map<PluginAct>(_ => row));
}

// --- [MODELS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginReceipt {
    private PluginReceipt() { }
    public sealed record PathLoaded(PathLoadVerdict Verdict, Option<PluginKey> Plugin) : PluginReceipt;
    public sealed record KeyLoaded(PluginKey Plugin) : PluginReceipt;
    public sealed record Protected(PluginKey Plugin, LoadProtection Behavior) : PluginReceipt;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PluginRegistry {
    public static Fin<PluginReceipt> Commit(PluginAct act, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(act)
            .Bind(request => request.Admit(op))
            .Bind(request => request.Switch(
                op,
                // A path load reports its identity only on success; the host writes `Guid.Empty` into the out slot
                // on refusal, so the identity rides `Option` rather than a sentinel a caller could carry forward.
                loadPath: static (held, row) => held.Catch(() => {
                    LoadPlugInResult native = PlugIn.LoadPlugIn(path: row.Path, plugInId: out Guid loaded);
                    return held.Row<LoadPlugInResult, PathLoadVerdict>(native).Map<PluginReceipt>(verdict =>
                        new PluginReceipt.PathLoaded(Verdict: verdict, Plugin: PluginKey.Maybe(loaded)));
                }),
                loadKey: static (held, row) => held.Catch(() => PlugIn.LoadPlugIn(
                        pluginId: row.Plugin.ToValue(),
                        loadQuietly: row.Notice.Key,
                        forceLoad: row.Force.Key)
                    ? Fin.Succ<PluginReceipt>(value: new PluginReceipt.KeyLoaded(Plugin: row.Plugin))
                    : Fin.Fail<PluginReceipt>(error: held.InvalidResult(detail: row.Plugin.ToValue().ToString()))),
                protect: static (held, row) => held.Catch(() => {
                    PlugIn.SetLoadProtection(pluginId: row.Plugin.ToValue(), loadSilently: row.Behavior.Key);
                    return Fin.Succ<PluginReceipt>(value: new PluginReceipt.Protected(
                        Plugin: row.Plugin,
                        Behavior: row.Behavior));
                })));
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
