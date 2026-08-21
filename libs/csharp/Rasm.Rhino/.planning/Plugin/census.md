# [RASM_RHINO_PLUGIN_CENSUS]

`PluginCensus.Ask` answers every read of the host's installed-plug-in registry — identity resolution, the registry descriptor, presence, load protection, command rosters, and the installed roll — behind one polymorphic entry over a request union. `PluginRegistry.Commit` owns the two registry mutations, load and protection, so a read never loads a plug-in as a side effect of asking about it; `lifecycle#LOAD_ROOT` is its one caller, folding a program's declared prerequisites at plug-in load.

`PluginKey` (`Document/events#HOOK_REGISTRY`) keys every row; a bare `Guid` never crosses a signature here. The identity hexad the host publishes as six free-standing statics is not six cases and not six entrypoints — it is ROWS on two lookup vocabularies, each row owning the host static it reads and the answer shape it produces, because the whole family is one resolution keyed by which coordinate the caller holds and which coordinate it wants. Every combinable host flag rides kernel `CapabilitySet<T>` over an `ICapability` vocabulary: the kind filter, the descriptor's traits, and the presence triple are one carrier apiece and no set carrier is minted here.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `PluginKind`, `PluginTrait`, `PluginState`, `PluginSchedule`, `PluginRoster`, `NameSource`, `PluginNaming`, `LoadNotice`, `LoadForce`, and `LoadProtection` close every registry axis as keyed rows.
- [03]-[DESCRIPTOR]: `PluginContact`, `PluginInfo`, `PluginRollRow`, `PluginPresence`, and `PluginProtection` detach the registry record before it leaves the boundary.
- [04]-[QUERY]: `PluginRead` and `PluginLookup` own the host reads as rows; `PluginQuery` and `PluginAnswer` close the read family and its admission.
- [05]-[CENSUS]: `PluginCensus.Ask` dispatches every read arm, folds the installed roll, and answers the registry icon as an asset origin.
- [06]-[ADMISSION]: `PathLoadVerdict`, `PluginAct`, `PluginReceipt`, and `PluginRegistry.Commit` own load and load-protection mutation.
- [07]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across the read entry, the mutation entry, and the vocabularies.

## [02]-[VOCABULARY]

- Owner: `PluginKind` `[SmartEnum<PlugInType>]` is the plug-in kind vocabulary and realizes `ICapability<PluginKind>` so a kind SET is the kernel `CapabilitySet<PluginKind>`; the host flag IS the capability, so its member name is the canonical text and its bit is the rank, and no hand-minted key or rank column can drift from the enum the registry publishes.
- Law: the `None` row exists so a host read is total — `PlugInInfo.PlugInType` answers `None` for a record whose kind the manager never resolved (`.api/api-rhinocommon-plugins.md:70`), and a vocabulary missing that row would refuse a row the registry genuinely holds. Its bit is zero, so it contributes nothing to a mask and cannot survive a mask ingress.
- Law: `PluginKind.Law` BARS the empty set — `GetInstalledPlugInNames` reads the OR-fold as its filter and an empty selection asks the host for nothing while looking like a request; the corner states as the complement it is, not as an enumeration of the legal rest.
- Owner: `PluginTrait` and `PluginState` are the descriptor's and the presence probe's own capability vocabularies, each row carrying the host read it answers — `PluginTrait` reads the registry record, `PluginState` reads the two `PlugInExists` out-slots — so each set is TOTAL over its producer's knowledge and neither can answer a row its host member never reported.
- Owner: `PluginSchedule` `[SmartEnum<PlugInLoadTime>]` mirrors the host load schedule completely, including the two composite rows whose ordinals are not powers of two; it is single-valued and stays a row, never a set.
- Law: `PluginRoster` collapses the host's `loaded`/`unloaded` boolean pair into three rows, so an all-false pair that can only answer an empty roster is unrepresentable; both columns are exactly the two arguments `GetInstalledPlugInNames(loaded:, unloaded:)` takes, and the three-row closure IS the legal-corner law a capability set would otherwise state.
- Law: `PluginNaming`, `LoadNotice`, `LoadForce`, and `LoadProtection` each replace one host boolean argument and convert IMPLICITLY to their key, so no call site passes a bare `true` whose meaning lives in the parameter name and no call site spells `.Key` to erase the row back.
- Packages: Thinktecture.Runtime.Extensions (`libs/csharp/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<THostEnum>]`, `[SmartEnum<bool>]` with `ConversionToKeyMemberType = Implicit`, `[UseDelegateFromConstructor]`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:51-53` — `PlugInType`, `PlugInLoadTime`, `LoadPlugInResult`; `:70` — the `PlugInInfo` default-on-undefined reads).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Rhino.Document;
using Rhino.PlugIns;
using GdiBitmap = System.Drawing.Bitmap;
using GdiSize = System.Drawing.Size;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<PlugInType>]
public sealed partial class PluginKind : ICapability<PluginKind> {
    public static readonly PluginKind None = new(key: PlugInType.None);
    public static readonly PluginKind Render = new(key: PlugInType.Render);
    public static readonly PluginKind FileImport = new(key: PlugInType.FileImport);
    public static readonly PluginKind FileExport = new(key: PlugInType.FileExport);
    public static readonly PluginKind Digitizer = new(key: PlugInType.Digitizer);
    public static readonly PluginKind Utility = new(key: PlugInType.Utility);
    public static readonly PluginKind DisplayPipeline = new(key: PlugInType.DisplayPipeline);
    public static readonly PluginKind DisplayEngine = new(key: PlugInType.DisplayEngine);
    public static readonly PluginKind Any = new(key: PlugInType.Any);

    // The capability text and rank DERIVE from the host flag: the enum member name is the canonical token and the
    // bit is the canonical order, so the vocabulary cannot drift from the enum the registry publishes.
    string ICapability<PluginKind>.Key => Key.ToString();
    int ICapability<PluginKind>.Rank => (int)Key;

    // The one illegal corner stated as a complement: an empty filter asks the host for nothing.
    public static CapabilityLaw<PluginKind> Law { get; } =
        CapabilityLaw<PluginKind>.Forbidden(barred: Seq(CapabilitySet<PluginKind>.None));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PluginTrait : ICapability<PluginTrait> {
    public static readonly PluginTrait Shipped = new(key: "shipped", reads: static row => row.ShipsWithRhino);
    public static readonly PluginTrait Managed = new(key: "managed", reads: static row => row.IsDotNet);

    [UseDelegateFromConstructor] internal partial bool Reads(PlugInInfo value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PluginState : ICapability<PluginState> {
    public static readonly PluginState Loaded = new(key: "loaded", holds: static (loaded, _) => loaded);
    public static readonly PluginState Protected = new(key: "protected", holds: static (_, guarded) => guarded);

    [UseDelegateFromConstructor] internal partial bool Holds(bool loaded, bool guarded);
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NameSource {
    public static readonly NameSource Commands = new("commands");
    public static readonly NameSource Installed = new("installed");
    public static readonly NameSource Folders = new("folders");
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class PluginNaming {
    public static readonly PluginNaming English = new(false);
    public static readonly PluginNaming Localized = new(true);
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class LoadNotice {
    public static readonly LoadNotice Announced = new(false);
    public static readonly LoadNotice Quiet = new(true);
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class LoadForce {
    public static readonly LoadForce Lazy = new(false);
    public static readonly LoadForce Forced = new(true);
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class LoadProtection {
    public static readonly LoadProtection Prompted = new(false);
    public static readonly LoadProtection Silent = new(true);
}
```

## [03]-[DESCRIPTOR]

- Owner: `PluginInfo` is the detached registry record; the live `PlugInInfo` handle wraps a native pointer and never leaves the resolving call.
- Law: every registry string reads through a native accessor that answers null on absence, so each optional coordinate crosses as `Option<string>` and only `Name` refuses typed — a nameless registry row is not addressable.
- Law: loaded-ness has ONE authority. `PluginInfo` carries the `PluginPresence` the descriptor fold resolved rather than a second boolean off `PlugInInfo.IsLoaded`, so a holder of a descriptor reads presence without a second census call and the two host members can never disagree on the same page.
- Law: `FileTypeDescriptions` and `FileTypeExtensions` are two INDEPENDENT native rosters, so the record carries them apart; pairing them positionally invents a correspondence the host never guarantees. `[Equatable]` with `[OrderedEquality]` on all three sequences, because a positional record compares a `Seq` member by reference and the registry's own order is meaningful.
- Owner: `PluginPresence` splits absence from state — a plug-in the registry does not hold has no loaded-ness and no protection, so `Absent` carries the key alone and `Present` carries the capability set the `PlugInExists` out-slots filled.
- Owner: `PluginProtection` carries only a resolved state; a registry record the host cannot read refuses typed rather than reporting a fabricated default.
- Boundary: the plug-in icon is a registry read — `PlugIn.Icon(Size)` is a non-virtual forward to `PlugInInfo.Icon` — so the icon leaves as a kernel `AssetOrigin.Raster` over the GDI raster arm and no lifecycle hook produces it.
- Packages: Generator.Equals (`libs/csharp/.api/api-generator-equals.md` — `[Equatable]`, `[OrderedEquality]`); LanguageExt.Core (`Option`, `Seq`); kernel `Domain/validation` (`CapabilitySet`); `Document/events` (`PluginKey`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:70` — the `PlugInInfo` descriptor reads).

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

[Equatable]
public sealed partial record PluginInfo(
    PluginKey Plugin,
    string Name,
    Option<string> Description,
    Option<string> Version,
    Option<string> FileName,
    Option<string> RegistryPath,
    PluginKind Kind,
    PluginSchedule Schedule,
    PluginPresence Presence,
    CapabilitySet<PluginTrait> Traits,
    PluginContact Contact,
    [property: OrderedEquality] Seq<string> CommandNames,
    [property: OrderedEquality] Seq<string> FileTypeDescriptions,
    [property: OrderedEquality] Seq<string> FileTypeExtensions);

// The kernel `PackageIdentity.PluginSlot` const owns the word `slot` as a telemetry dimension key, so the
// installed-roll row carries the name the roll gives it and no reader resolves two senses of one word.
public sealed record PluginRollRow(PluginKey Plugin, Option<string> Name);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginPresence {
    private PluginPresence() { }
    public sealed record Absent(PluginKey Plugin) : PluginPresence;
    public sealed record Present(PluginKey Plugin, CapabilitySet<PluginState> States) : PluginPresence;
}

public sealed record PluginProtection(PluginKey Plugin, LoadProtection Behavior);
```

## [04]-[QUERY]

- Owner: `PluginRead` and `PluginLookup` are the host reads AS DATA — one row per free-standing registry static, each row owning the call it makes and the `PluginAnswer` case it produces, so a new registry read is one row and no arm, no case, and no admission leg moves.
- Law: the two tables split on PAYLOAD, not on answer — `PluginRead` rows take an admitted `PluginKey` and `PluginLookup` rows take admitted text, which is exactly the admission each needs; a single table would carry a payload half its rows cannot use.
- Owner: `PluginQuery` closes every registry read in six cases where fourteen stood, because eleven of them differed only in which host static an arm called.
- Law: admission runs before any host call — text coordinates pass `Op.AcceptText`, identity coordinates pass `PluginKey.Admit`, and the kind filter passes `PluginKind.Law.Admit`, so a query that cannot resolve never reaches the native manager. The roster case's three independent columns ACCUMULATE, so a caller learns every absent column at once.
- Law: an unresolved identity or path is `None`, not a fault — the host answers `Guid.Empty` or an empty string for an unknown coordinate, and those sentinels are projected away by `PluginKey.Maybe` and `Op.Text` at the row rather than surfacing as a value.
- Law: an answer names the request it answers where the payload alone cannot — `PluginAnswer.Names` carries its `NameSource`, because a command roster, an installed-name roster, and a folder roster are three questions with one payload shape (folder RULINGS `[02]`).
- Boundary: the descriptor row is the ONE read that touches `PlugInInfo`; every other row reads a free-standing static, so the native record's lifetime never spans two reads.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[UseDelegateFromConstructor]`, `[Union]` with the generated total `Switch`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Traverse`, `Validation` tuple `.Apply`); kernel `Domain/rails` (`Op.AcceptText`, `Op.Text`, `Op.Need`, `Op.Probe`, `Op.Row`), `Domain/validation` (`CapabilitySet`, `CapabilityLaw`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:60-63,70` — `IdFromName`, `IdFromPath`, `IdFromFileName`, `NameFromPath`, `PathFromId`, `PathFromName`, `GetPlugInInfo`, `PlugInExists`, `GetLoadProtection`, `GetEnglishCommandNames`, `GetInstalledPlugIns`, `GetInstalledPlugInNames`, `GetInstalledPlugInFolders`, `InstalledPlugInCount`).

```csharp signature
// --- [TABLES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PluginRead {
    public static readonly PluginRead Path = new(key: "path", read: static (plugin, op) =>
        Named(read: () => PlugIn.PathFromId(pluginId: plugin.ToValue()), op: op));
    public static readonly PluginRead Descriptor = new(key: "descriptor", read: Detached);
    public static readonly PluginRead Presence = new(key: "presence", read: static (plugin, op) =>
        Probed(plugin: plugin, op: op).Map<PluginAnswer>(static row => new PluginAnswer.Presence(Value: row)));
    public static readonly PluginRead Protection = new(key: "protection", read: Guarded);
    public static readonly PluginRead Commands = new(key: "commands", read: static (plugin, op) =>
        op.Catch(() => Fin.Succ<PluginAnswer>(value: new PluginAnswer.Names(
            Source: NameSource.Commands,
            Value: toSeq(PlugIn.GetEnglishCommandNames(pluginId: plugin.ToValue())).Strict()))));

    [UseDelegateFromConstructor] internal partial Fin<PluginAnswer> Read(PluginKey plugin, Op op);

    internal static Fin<PluginAnswer> Named(Func<string> read, Op op) => op.Catch(() =>
        Fin.Succ<PluginAnswer>(value: new PluginAnswer.Text(Value: Op.Text(read()))));

    // The two `out` slots are ROWS, so the state set is built by the vocabulary's own filter and a third state
    // lands as a row instead of as a third conditional append. The multi-`out` probe lifts once on the kernel
    // arm, so absence — a key the registry does not hold — is the `Absent` case, never a triple of false.
    internal static Fin<PluginPresence> Probed(PluginKey plugin, Op op) => op.Catch(() => Fin.Succ(
        value: Op.Probe(() => {
            bool installed = PlugIn.PlugInExists(
                id: plugin.ToValue(), loaded: out bool loaded, loadProtected: out bool guarded);
            return (Ok: installed, Value: (Loaded: loaded, Guarded: guarded));
        }).Match(
            Some: slots => (PluginPresence)new PluginPresence.Present(
                Plugin: plugin,
                States: CapabilitySet<PluginState>.Of(toSeq(PluginState.Items)
                    .Filter(row => row.Holds(loaded: slots.Loaded, guarded: slots.Guarded))
                    .ToArray())),
            None: () => new PluginPresence.Absent(Plugin: plugin))));

    private static Fin<PluginAnswer> Guarded(PluginKey plugin, Op op) => op.Catch(() =>
        Op.Probe<bool>(probe: (out bool silent) =>
                PlugIn.GetLoadProtection(pluginId: plugin.ToValue(), loadSilently: out silent))
            .Map(static silent => silent ? LoadProtection.Silent : LoadProtection.Prompted)
            .ToFin(Fail: new PluginFault.HostRefused(
                Key: op, Member: nameof(PlugIn.GetLoadProtection), Detail: plugin.ToValue().ToString()))
            .Map<PluginAnswer>(behavior => new PluginAnswer.Protection(
                Value: new PluginProtection(Plugin: plugin, Behavior: behavior))));

    internal static Fin<PlugInInfo> Handle(PluginKey plugin, Op op) => op.Catch(() =>
        Optional(PlugIn.GetPlugInInfo(pluginId: plugin.ToValue())).ToFin(
            Fail: new PluginFault.Unbound(Key: op, Member: nameof(PlugIn.GetPlugInInfo))));

    // The whole record folds in one pass, so a caller holding a `PluginInfo` never re-enters the census for a
    // coordinate the record already carries — presence included.
    private static Fin<PluginAnswer> Detached(PluginKey plugin, Op op) =>
        from record in Handle(plugin: plugin, op: op)
        from presence in Probed(plugin: plugin, op: op)
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
                Presence: presence,
                Traits: CapabilitySet<PluginTrait>.Of(toSeq(PluginTrait.Items)
                    .Filter(row => row.Reads(value: record))
                    .ToArray()),
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PluginLookup {
    public static readonly PluginLookup IdOfName = new(key: "id-of-name", read: static (value, op) =>
        Resolved(read: () => PlugIn.IdFromName(pluginName: value), op: op));
    public static readonly PluginLookup IdOfPath = new(key: "id-of-path", read: static (value, op) =>
        Resolved(read: () => PlugIn.IdFromPath(pluginPath: value), op: op));
    public static readonly PluginLookup IdOfFile = new(key: "id-of-file", read: static (value, op) =>
        Resolved(read: () => PlugIn.IdFromFileName(filename: value), op: op));
    public static readonly PluginLookup NameOfPath = new(key: "name-of-path", read: static (value, op) =>
        PluginRead.Named(read: () => PlugIn.NameFromPath(pluginPath: value), op: op));
    public static readonly PluginLookup PathOfName = new(key: "path-of-name", read: static (value, op) =>
        PluginRead.Named(read: () => PlugIn.PathFromName(pluginName: value), op: op));

    [UseDelegateFromConstructor] internal partial Fin<PluginAnswer> Read(string value, Op op);

    private static Fin<PluginAnswer> Resolved(Func<Guid> read, Op op) => op.Catch(() =>
        Fin.Succ<PluginAnswer>(value: new PluginAnswer.Identity(Value: PluginKey.Maybe(read()))));
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginQuery {
    private PluginQuery() { }
    public sealed record Keyed(PluginRead Read, PluginKey Plugin) : PluginQuery;
    public sealed record Text(PluginLookup Read, string Value) : PluginQuery;
    public sealed record Installed(PluginNaming Naming) : PluginQuery;
    public sealed record InstalledNames(
        CapabilitySet<PluginKind> Kinds, PluginRoster Roster, PluginNaming Naming) : PluginQuery;
    public sealed record Folders : PluginQuery;
    public sealed record Tally : PluginQuery;

    internal Fin<PluginQuery> Admit(Op op) => Switch(
        op,
        keyed: static (key, row) => key.Need(row.Read).Bind(_ => row.Plugin.Admit(key)).Map<PluginQuery>(_ => row),
        text: static (key, row) => key.Need(row.Read)
            .Bind(_ => key.AcceptText(value: row.Value))
            .Map<PluginQuery>(value => new Text(Read: row.Read, Value: value)),
        installed: static (key, row) => key.Need(row.Naming).Map<PluginQuery>(_ => row),
        installedNames: static (key, row) => (
                key.Need(row.Roster).ToValidation(),
                key.Need(row.Naming).ToValidation(),
                PluginKind.Law.Admit(held: row.Kinds).ToValidation())
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin()
            .Map<PluginQuery>(_ => row),
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
    public sealed record Roll(Seq<PluginRollRow> Value) : PluginAnswer;
    public sealed record Names(NameSource Source, Seq<string> Value) : PluginAnswer;
    public sealed record Tally(int Value) : PluginAnswer;

    // The typed accessor for the one product two rows both answer: `Presence` reads the probe and `Descriptor`
    // reads the presence it already folded, so a consumer asks either row and reads its answer in one hop instead
    // of probing a closed union it already decided.
    public Fin<PluginPresence> Presence(Op key) => Switch(
        key,
        identity: static (held, _) => Elsewhere(held),
        text: static (held, _) => Elsewhere(held),
        descriptor: static (_, row) => Fin.Succ(value: row.Value.Presence),
        presence: static (_, row) => Fin.Succ(value: row.Value),
        protection: static (held, _) => Elsewhere(held),
        roll: static (held, _) => Elsewhere(held),
        names: static (held, _) => Elsewhere(held),
        tally: static (held, _) => Elsewhere(held));

    private static Fin<PluginPresence> Elsewhere(Op key) => Fin.Fail<PluginPresence>(
        error: new KernelFault.InvalidValue(nameof(PluginPresence), "a presence or descriptor read"));
}
```

## [05]-[CENSUS]

- Entry: `PluginCensus.Ask(PluginQuery, Op?)` is the single read entry; a new registry read is one ROW on `PluginRead` or `PluginLookup`, and the entry itself does not move.
- Law: the descriptor's kind and schedule project through `Op.Row` against the host value; an ordinal the vocabulary does not carry refuses typed rather than defaulting to a row the registry never reported.
- Law: the kind filter leaves the capability owner ONCE, at the one host member that takes the raw flag word, so the OR-fold has a single spelling and no caller re-derives it.
- Law: `Icon` is a separate leased entry, not a `PluginInfo` field, because a raster is caller-disposed custody and a record field would make its lifetime ambient; it answers the kernel `AssetOrigin.Raster` over an `AssetRaster.Gdi` scale row, so a consumer receives extent and scale rather than an unlabelled bitmap and composes `AssetOrigin.Resolve` for any other product shape.
- Boundary: every native string crosses through `Op.Text` and every registry identity through `PluginKey.Maybe`, so a host null, an empty string, and a `Guid.Empty` are each the same typed absence.
- Packages: LanguageExt.Core (`Fin`, `Option`, `Seq`, `Traverse`, `.Strict()`); kernel `Domain/rails` (`Op`, `Op.Text`, `Op.Need`, `Lease<T>`), `Interaction/asset` (`AssetExtent`, `AssetRaster.Gdi`, `AssetOrigin.Raster`), `Numerics/atoms` (`PositiveMagnitude`, `Dimension`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:63` — `GetInstalledPlugIns`, `GetInstalledPlugInNames`, `GetInstalledPlugInFolders`, `InstalledPlugInCount`; `PlugInInfo.Icon(Size)`).

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PluginCensus {
    public static Fin<PluginAnswer> Ask(PluginQuery query, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(query)
            .Bind(request => request.Admit(op))
            .Bind(request => request.Switch(
                op,
                keyed: static (held, row) => row.Read.Read(plugin: row.Plugin, op: held),
                text: static (held, row) => row.Read.Read(value: row.Value, op: held),
                installed: static (held, row) => held.Catch(() =>
                    toSeq(PlugIn.GetInstalledPlugIns(localizedPlugInName: row.Naming))
                        .Traverse(entry => PluginKey.Maybe(entry.Key)
                            .ToFin(Fail: new PluginFault.HostRefused(
                                Key: held, Member: nameof(PlugIn.GetInstalledPlugIns), Detail: nameof(PluginRollRow)))
                            .Map(plugin => new PluginRollRow(Plugin: plugin, Name: Op.Text(entry.Value))))
                        .As()
                        .Map<PluginAnswer>(static rows => new PluginAnswer.Roll(Value: rows.Strict()))),
                installedNames: static (held, row) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Names(
                        Source: NameSource.Installed,
                        Value: toSeq(PlugIn.GetInstalledPlugInNames(
                            typeFilter: (PlugInType)row.Kinds.Mask(bit: static kind => (int)kind.Key),
                            loaded: row.Roster.IsLoaded,
                            unloaded: row.Roster.IsUnloaded,
                            localizedPlugInName: row.Naming)).Strict()))),
                folders: static (held, _) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Names(
                        Source: NameSource.Folders,
                        Value: toSeq(PlugIn.GetInstalledPlugInFolders()).Strict()))),
                tally: static (held, _) => held.Catch(() => Fin.Succ<PluginAnswer>(
                    value: new PluginAnswer.Tally(Value: PlugIn.InstalledPlugInCount)))));
    }

    // The bitmap is a fresh host allocation on every call, so it crosses owned inside the asset owner's own scale
    // row; a null answer is an absent icon, never an empty image the caller would then have to test.
    public static Fin<AssetOrigin> Icon(PluginKey plugin, AssetExtent extent, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in plugin.Admit(op)
               from record in PluginRead.Handle(plugin: plugin, op: op)
               from bitmap in op.Catch(() => Optional(record.Icon(
                       size: new GdiSize(width: extent.PixelWidth, height: extent.PixelHeight)))
                   .ToFin(Fail: new PluginFault.Unbound(Key: op, Member: nameof(PlugInInfo.Icon))))
               select (AssetOrigin)new AssetOrigin.Raster(Scales: Seq<AssetRaster>(
                   new AssetRaster.Gdi(Scale: extent.Scale, Bitmap: new Lease<GdiBitmap>.Owned(Value: bitmap))));
    }
}
```

## [06]-[ADMISSION]

- Owner: `PluginAct` closes registry mutation — load by path, load by identity, and load-protection assignment.
- Entry: `PluginRegistry.Commit(PluginAct, Op?)` is the one mutation entry; the two host load overloads are two cases on it, never two entrypoints. `lifecycle#LOAD_ROOT` folds a program's declared `PluginBoot.Prerequisites` through it at plug-in load, which is the only moment a package may load its own dependencies.
- Law: `PathLoadVerdict` mirrors `LoadPlugInResult` whole — the host publishes exactly `Success`, `SuccessAlreadyLoaded`, and `ErrorUnknown` (`.api/api-rhinocommon-plugins.md:53`) — so "already loaded" stays a distinct success rather than collapsing into the plain success arm.
- Law: identity-keyed load answers a bare `bool`, so a false answer refuses typed with the requested key as detail; the host publishes no richer reason on that overload.
- Law: `SetLoadProtection` returns nothing and the host publishes no failure signal, so the receipt reports the assignment the boundary made and a caller wanting the settled state re-reads `PluginRead.Protection`.
- Boundary: loading a plug-in runs its `OnLoad` inside this call, so a `Commit` is a host lifecycle event, never a query — this is exactly why the read family carries no load flag.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<LoadPlugInResult>]`, `[Union]`); LanguageExt.Core (`Fin`, `Option`); kernel `Domain/rails` (`Op.Need`, `Op.Catch`, `Op.Side`), `Domain/validation` (`Op.Row`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md:53,62` — `LoadPlugInResult`, `LoadPlugIn` both overloads, `SetLoadProtection`).

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
            .Bind(_ => key.Need(row.Notice))
            .Bind(_ => key.Need(row.Force))
            .Map<PluginAct>(_ => row),
        protect: static (key, row) => row.Plugin.Admit(key)
            .Bind(_ => key.Need(row.Behavior))
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
                        loadQuietly: row.Notice,
                        forceLoad: row.Force)
                    ? Fin.Succ<PluginReceipt>(value: new PluginReceipt.KeyLoaded(Plugin: row.Plugin))
                    : Fin.Fail<PluginReceipt>(error: new PluginFault.HostRefused(
                        Key: held, Member: nameof(PlugIn.LoadPlugIn), Detail: row.Plugin.ToValue().ToString()))),
                protect: static (held, row) => held
                    .Catch(() => PlugIn.SetLoadProtection(
                        pluginId: row.Plugin.ToValue(), loadSilently: row.Behavior))
                    .Map<PluginReceipt>(_ => new PluginReceipt.Protected(
                        Plugin: row.Plugin, Behavior: row.Behavior))));
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]          | [INGRESS]                    | [STATE]                             | [EGRESS]                             |
| :-----: | :--------------- | :--------------------------- | :---------------------------------- | :----------------------------------- |
|  [01]   | `PluginCensus`   | `Ask` · `Icon(key, extent)`  | none — every read is a host static  | `PluginAnswer` · `AssetOrigin`       |
|  [02]   | `PluginRegistry` | `Commit(PluginAct)`          | the host registry itself            | `PluginReceipt`                      |
|  [03]   | `PluginRead`     | admitted `PluginKey`         | row-owned host read                 | `PluginAnswer` per row               |
|  [04]   | `PluginLookup`   | admitted text                | row-owned host read                 | `PluginAnswer.Identity`/`.Text`      |
|  [05]   | `PluginInfo`     | `PluginRead.Descriptor` fold | detached registry record            | traits · presence · rosters          |
|  [06]   | `PluginKind`     | host flag word               | `CapabilityLaw` barred-empty corner | `CapabilitySet` · mask at one member |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
