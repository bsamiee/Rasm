# [RASM_RHINO_RENDER_REGISTRY]

`ContentUuidCatalog` owns built-in type, instance, and CCI seed data, `ContentSerializer` owns explicit read transfer and multi-load reporting, and `Registry.Run` closes registration and mutation. `Registry.Read` preserves typed query correlation, icons retain bitmap custody across every verified icon modality, and static content events fold into detached facts with no live `RenderContent` escape. Every bounded journal is the kernel ring, and every refusal codes on the Display page's `RenderFault` family.

## [01]-[INDEX]

- [02]-[FACTORY_REGISTRY]: `ContentTypeInfo`, plug-in registration, the `ContentSerializer` adapter, the `ShellRow`/`RenderShell` render-editor seating, and the `EditorBridge` payload boundary.
- [03]-[OPERATION_FAMILY]: `ContentAdmission`, `ContentMutation`, and identity-discriminated `ContentOp` dispatch.
- [04]-[COMMIT_AND_QUERY]: `ContentTransaction`, typed query programs, and the `Registry` entries.
- [05]-[EVENTS]: `ContentPulse`, `ContentSignal`, `ContentFact`, and the `ContentStream` observation capsule.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[FACTORY_REGISTRY]

- Owner: `ContentUuidCatalog` projects every public static `Guid` property and field on `ContentUuids` into one slot roster, derives kind and role from its fail-closed naming grammar, and refuses an empty census and a duplicate seed id so `Find` only ever reads a validated census; `ContentTypeInfo` detaches registered factory descriptors; `ContentTypeCensus` returns both tiers without confusing type, default-instance, or CCI identifiers.
- Owner: `SerializerProgram` admits a generated extension, content kind, optional single-file programs, typed multi-load reports, and a journal cap; `ContentSerializer` adapts the host, parking every failure in one bounded `Ring<SerializerFailure>`.
- Owner: `RenderShellProgram` admits the panel and side-pane-tab declaration set once; `ShellGate` is the process-static arming state the host's two registration callbacks step through; `RenderShell` drains it and `ShellRow` closes the panel and tab row shapes over their host registrars.
- Law: identity is TYPED at every boundary — a plug-in is a `PluginKey`, a content or object address is a `ResourceId`, and a write target is a `DocumentPath`. Raw `Guid` parameters carried the branch's plug-in identity through four signatures where the ruled owner exists, so an empty guid reached a host registrar and the refusal it produced named no address.
- Law: serializer reads accept only `ContentTransfer` over `Lease<RenderContent>.Owned`; `Take` transfers custody exactly once, and no borrowed lease can masquerade as host-owned output.
- Law: `SerializerDisposition` dispatches to `ReportContentAndFile` or `ReportDeferredContentAndFile`; load policy and kind cross generated correspondence owners before the program runs. A multi-load drains every report — a failed emit never strands later reports undisposed — and a content the host report refuses after `Take` is disposed before the fault leaves.
- Law: the failure journal IS the kernel bounded ring. A cap, oldest-first eviction, and a drop counter were a page-local retention policy, ledger, and overflow triple that three sibling pages composed; `Ring<T>` is that shape once for the solution, its `Park` verdict is COUNTED rather than discarded, and a declined park reads as `Lost`. NAMED LOSS: the accumulated `Error` over every dropped row; that accumulator grew without bound beside a capped roster, so the cap now bounds what it claimed to.
- Law: plug-in classes, serializers, and the render-shell declaration set register through `Registry.Run`; registration returns typed evidence and rejects missing assemblies, serializers, plug-in identities, or an undecorated row type.
- Law: a shell row is keyed by its own `GuidAttribute`-decorated `Type` — the host reads that attribute as the registration key and throws on an undecorated type, so `ShellRow` proves the attribute at admission and the registrar call never sees an unkeyed row.
- Law: shell registration is one-shot and host-driven, never caller-timed — `RenderPlugIn.RegisterRenderPanels`/`RegisterRenderTabs` hand the registrars in and every row registered after those callbacks return is silently ignored, so `Registry.Run` only ARMS the declaration and `RenderShell.Drain` inside each override is what registers.
- Law: arming and draining are ONE cell. Two independent atoms held "a program is armed" and "a drain has happened", so a check-then-act arm seated a program between the check and the swap, and the first surface's drain locked the second surface out of arming while claiming nothing about which surface had been seated. `ShellGate` carries the program beside the `CapabilitySet<ShellSurface>` already drained, one `Cell.Step` decides each transition, and the second surface's callback drains its own rows while a repeat of the same surface refuses.
- Law: a drain CLAIMS its surface before it seats. The host discards every row registered after the callback returns, so a failed seat cannot be retried and a claim released on failure invites only a second attempt the host ignores; the claim standing is the truthful record that this surface's one chance was spent.
- Law: `ShellSeated` names the surface it drained. The prior pair of counts published a zero for the surface this drain never touched, indistinguishable from a surface that drained no rows.
- Law: `RenderPanels.RegisterPanel` and `RenderTabs.RegisterTab` are instance members on host-minted registrars with internal constructors, so no page mints one — `ShellRegistrar` absorbs whichever instance the override was handed and nothing else.
- Law: registration composes only the engine-carrying overloads — the engine-less pair is host-obsolete and forwards the plug-in id, the place-less panel form forwards `Left`, so `ShellRow.Seat` resolves both defaults at admission and issues exactly one registrar call per row.
- Law: a seated row is resolvable — `RenderShell.Resolve<TBody>` folds the two static `FromRenderSessionId` resolvers and the side-pane id behind the armed row that owns `TBody`. The host declares TWO tab-id members and they carry ONE fact: `SessionIdFromTab`'s entire body is `return SidePaneUiIdFromTab(tab);`, so `ShellSeat<TBody>` holds one `Option<Guid>` — a second field mirrors the identical value under a second name — and a panel seat carries none.
- Law: `RhinoSettings` has one public constructor and it takes a native pointer, so the render-editor bridge is never minted here — `EditorBridge` wraps the `IRdkViewModel` the host hands a UI section and vends each payload by its `DataSource.ProviderIds` row, committing or discarding a write by that same id. `Registry.Read` borrows the settings payload for one callback and detaches `EditorFacts`; the live wrapper never crosses out.
- Law: an editor intent's two axes are not independent — the host's auto-change bracket opens only on a write handle — so the intent rows carry `CapabilitySet<EditorTrait>` and the three declared rows ARE the whole legal corner set; a bool pair admitted a bracketed read no host call honours.
- Law: the native behind an editor payload is always the host's; only the managed wrapper's finalizer registration is the borrow's, so release is an `EditorProvider` column and every current row releases. Host truth: `GetData` resolves each provider id through ONE static id-to-type dispatch shared by both managed controller families — the settings row vends `Rhino.Render.DataSources.RhinoSettings`, the selection and display rows vend `Rhino.Render.RenderContentCollection` minted through its non-owning `(nint)` constructor — and each wrapper's `Dispose` clears the managed pointer, suppresses the finalizer, and deletes NO native; an id outside the dispatch answers null, never a foreign carrier. A NEW row proves its own payload's disposal body before its release column is set.
- Law: `ContentUuidCatalog.Census` is built once and memoized — the ids are process-static host constants, so a `Find` reads the built value instead of re-reflecting the type and re-invoking one native getter per member.
- Law: the seed grammar is fail-closed on both sides — no match and multiple matches are kernel validation failures carrying the member and match count; a rename the grammar cannot read fails the whole census.
- Boundary: the host also discovers serializers through `RenderPlugIn.RenderContentSerializers()` and the shell registrars through its two register overrides; the adapter shape is this page's, the plug-in overrides that forward them are the plug-in's.
- Boundary: serializer seating and its evidence both belong to the plug-in load root, and the seat already exists — `PluginBoot.Mounts` declares a `ShellMount.Hooks` row whose `(PluginKey) -> Fin<IDisposable>` arity a `RegistryCommand.RegisterSerializer` fold satisfies, and the same row's release is where `Parked`/`Shed`/`Lost` reach the load report. `Plugin/lifecycle.md` now STATES that row as law — the declared Hooks body runs the `RegisterSerializer` fold and its release drains `Parked`/`Shed`/`Lost` into the load report — so the discovery path is the declared row and a registration outside it is the deleted form.
- Growth: a new serializer route is one `SerializerProgram` column; a new shell surface is one `ShellSurface` row with its `ShellRow` case; a new editor payload is one `EditorProvider` row whose disposal body is proven first.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContentType.GetAllAvailableTypes`, `ContentUuids`, `RenderContent.RegisterContent`, `RenderContentSerializer` and its `Read`/`Write`/`CanLoadMultiple`/`LoadMultiple`/`RegisterSerializer`/`ReportContentAndFile`/`ReportDeferredContentAndFile`, `LoadMultipleFlags`); `api-rhinocommon-render-ui.md` (`RenderPanels.RegisterPanel`, `RenderTabs.RegisterTab`, `RenderPanelType`, `ExtraSidePanePosition`, `FromRenderSessionId`, `SidePaneUiIdFromTab`); `api-rhino-ui-controls.md` (`IRdkViewModel.GetData`/`Commit`/`Discard`, `DataSource.ProviderIds`, `RhinoSettings`); `api-rhinocommon-plugins.md` (`PlugIn`); kernel `Domain/results` (`Try.lift`, `Admit.Confirm`, `HostEdge.Side`, `Lease<T>.Acquire`/`Use`, `Cell.Step`, `Transition`), `Domain/hooks` (`Ring<T>`), `Domain/validation` (`ICapability`, `CapabilitySet`, `FactoryBridge.Row`, `FactoryBridge.Accept`); `Display/render.md` (`RenderFault`); `Document/events.md` (`PluginKey`), `Document/tables.md` (`ResourceId`), kernel `Domain/results` (`Custody`); `Numerics/atoms` (`Dimension`, `Size2i`); LanguageExt.Core; Thinktecture.Runtime.Extensions.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.DataSources;
using Thinktecture;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentTypeInfo(
    ResourceId TypeId, string InternalName, Guid RenderEngineId, PluginKey PlugIn) : IDetachedDocumentResult {
    internal static Fin<Seq<ContentTypeInfo>> Census() =>
        Try.lift(() => toSeq(RenderContentType.GetAllAvailableTypes()).TraverseM(descriptor =>
            Lease<RenderContentType>.Acquire(mint: () => descriptor).Bind(lease => lease.Use(
                body: held =>
                    from type in ResourceId.Admit(value: held.Id)
                    from plugin in FactoryBridge.Accept<PluginKey>(held.PlugInId)
                    from name in Acceptance.Text(value: held.InternalName)
                    select new ContentTypeInfo(
                        TypeId: type, InternalName: name, RenderEngineId: held.RenderEngineId, PlugIn: plugin)))).As()).Run().Bind(static inner => inner);
}

[SmartEnum<string>]
public sealed partial class ContentUuidRole {
    public static readonly ContentUuidRole Type = new("type");
    public static readonly ContentUuidRole DefaultInstance = new("default-instance");
    public static readonly ContentUuidRole Cci = new("cci");
}

public sealed record ContentUuidSeed(string Name, ContentKind Kind, ContentUuidRole Role, ResourceId Id)
    : IDetachedDocumentResult;

public static class ContentUuidCatalog {
    private static readonly Lazy<Fin<Seq<ContentUuidSeed>>> Seeds = new(
        static () => Build(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Fin<Seq<ContentUuidSeed>> Census() => Seeds.Value;

    public static Fin<Option<ContentUuidSeed>> Find(ResourceId id) {
        return from active in Admit.Need(id)
               from seeds in Census()
               select seeds.Find(seed => seed.Id == active);
    }

    private static Fin<Seq<ContentUuidSeed>> Build() =>
        from slots in Try.lift(() => Fin.Succ(toSeq(Slots()))).Run().Bind(static inner => inner)
        from _ in guard(!slots.IsEmpty, (Error)new KernelFault.InvalidValue(nameof(ContentUuids), "at least one content identity"))
        from seeds in slots.TraverseM(slot => Seed(slot)).As()
        from __ in guard(
            seeds.Map(static seed => seed.Id).Distinct().Count == seeds.Count,
            (Error)new KernelFault.InvalidValue(nameof(ContentUuids), "distinct seed identities"))
        select seeds.Strict();

    private static IEnumerable<(string Name, Func<Guid> Read)> Slots() =>
        typeof(ContentUuids).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(static property => property.PropertyType == typeof(Guid) && property.GetMethod is not null)
            .Select(static property => (property.Name, Read: (Func<Guid>)(() => (Guid)property.GetValue(obj: null)!)))
            .Concat(typeof(ContentUuids).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(static field => field.FieldType == typeof(Guid))
                .Select(static field => (field.Name, Read: (Func<Guid>)(() => (Guid)field.GetValue(obj: null)!))))
            .OrderBy(static slot => slot.Name, StringComparer.Ordinal);

    private static Fin<ContentUuidSeed> Seed((string Name, Func<Guid> Read) slot) =>
        from role in Role(slot.Name)
        from kind in Kind(slot.Name)
        from raw in Try.lift(() => Fin.Succ(value: slot.Read())).Run().Bind(static inner => inner)
        from id in ResourceId.Admit(value: raw)
        select new ContentUuidSeed(Name: slot.Name, Kind: kind, Role: role, Id: id);

    private static readonly Seq<(string Suffix, ContentUuidRole Role)> RoleSuffixes = Seq(
        ("CCI", ContentUuidRole.Cci),
        ("Instance", ContentUuidRole.DefaultInstance),
        ("Type", ContentUuidRole.Type),
        ("Texture", ContentUuidRole.Type));

    private static readonly Seq<(string Token, ContentKind Kind)> KindTokens = Seq(
        ("Material", ContentKind.Material),
        ("Environment", ContentKind.Environment),
        ("Texture", ContentKind.Texture));

    private static Fin<ContentUuidRole> Role(string name) =>
        RoleSuffixes
            .Filter(row => name.EndsWith(row.Suffix, StringComparison.Ordinal))
            .Map(static row => row.Role)
            .Distinct() switch {
            [var only] => Fin.Succ(only),
            var matched => Fin.Fail<ContentUuidRole>(new KernelFault.InvalidValue(name, $"exactly one role suffix, matched {matched.Count}")),
        };

    private static Fin<ContentKind> Kind(string name) =>
        KindTokens.Filter(row => name.Contains(row.Token, StringComparison.Ordinal)) switch {
            [var only] => Fin.Succ(only.Kind),
            var matched => Fin.Fail<ContentKind>(new KernelFault.InvalidValue(name, $"exactly one kind token, matched {matched.Count}")),
        };
}

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public sealed partial class ContentExtension {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value switch {
            "" => new ValidationError(string.Join(" | ", new object?[] { nameof(ContentExtension) })),
            var text when !text.StartsWith('.', StringComparison.Ordinal) || text.Length <= 1 =>
                new ValidationError(string.Join(" | ", new object?[] { nameof(ContentExtension), "a dotted extension" })),
            var text when text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 =>
                new ValidationError(string.Join(" | ", new object?[] { nameof(ContentExtension), "file-name-legal characters" })),
            _ => null,
        };
    }

    internal static Fin<ContentExtension> Of(string value) =>
        FactoryBridge.Accept<ContentExtension>(value);
}

[SmartEnum<int>]
public sealed partial class SerializerStage {
    public static readonly SerializerStage Read = new(0);
    public static readonly SerializerStage Write = new(1);
    public static readonly SerializerStage Load = new(2);
    public static readonly SerializerStage Register = new(3);
}

[SmartEnum<int>]
public sealed partial class LoadPolicy {
    public static readonly LoadPolicy Normal = new(key: (int)RenderContentSerializer.LoadMultipleFlags.Normal);
    public static readonly LoadPolicy Preload = new(key: (int)RenderContentSerializer.LoadMultipleFlags.Preload);

    internal RenderContentSerializer.LoadMultipleFlags Native => (RenderContentSerializer.LoadMultipleFlags)Key;

    internal static Fin<LoadPolicy> Of(RenderContentSerializer.LoadMultipleFlags native) =>
        FactoryBridge.Row<RenderContentSerializer.LoadMultipleFlags, LoadPolicy>(native, static value => (int)value);
}

[SmartEnum<bool>]
public sealed partial class SerializerDisposition {
    public static readonly SerializerDisposition Loaded = new(false, static (loaded, _) => loaded());
    public static readonly SerializerDisposition Deferred = new(true, static (_, deferred) => deferred());

    [UseDelegateFromConstructor]
    internal partial Unit Fold(Func<Unit> loaded, Func<Unit> deferred);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EditorTrait : ICapability<EditorTrait> {
    public static readonly EditorTrait Writes = new(key: "writes");
    public static readonly EditorTrait Brackets = new(key: "brackets");
}

[SmartEnum<string>]
public sealed partial class EditorIntent {
    public static readonly EditorIntent Read = new("read", traits: CapabilitySet<EditorTrait>.None);
    public static readonly EditorIntent Write = new("write",
        traits: CapabilitySet<EditorTrait>.Of(EditorTrait.Writes, EditorTrait.Brackets));
    public static readonly EditorIntent RawWrite = new("raw-write",
        traits: CapabilitySet<EditorTrait>.Of(EditorTrait.Writes));

    internal CapabilitySet<EditorTrait> Traits { get; }

    internal bool Writes => Traits.Admits(capability: EditorTrait.Writes);
    internal bool Brackets => Traits.Admits(capability: EditorTrait.Brackets);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShellSurface : ICapability<ShellSurface> {
    public static readonly ShellSurface Panel = new(key: "panel");
    public static readonly ShellSurface Tab = new(key: "tab");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelTrait : ICapability<PanelTrait> {
    public static readonly PanelTrait AlwaysShow = new(key: "always-show");
    public static readonly PanelTrait InitialShow = new(key: "initial-show");
}

[SmartEnum<int>]
public sealed partial class SidePanePlace {
    public static readonly SidePanePlace Left = new(key: (int)RenderPanels.ExtraSidePanePosition.Left);
    public static readonly SidePanePlace Top = new(key: (int)RenderPanels.ExtraSidePanePosition.Top);
    public static readonly SidePanePlace Right = new(key: (int)RenderPanels.ExtraSidePanePosition.Right);
    public static readonly SidePanePlace Bottom = new(key: (int)RenderPanels.ExtraSidePanePosition.Bottom);

    internal RenderPanels.ExtraSidePanePosition Native => (RenderPanels.ExtraSidePanePosition)Key;
}

[SmartEnum<int>]
public sealed partial class ShellPanelKind {
    public static readonly ShellPanelKind RenderWindow = new(key: (int)RenderPanelType.RenderWindow);

    internal RenderPanelType Native => (RenderPanelType)Key;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class ContentTransfer : IDisposable, IDetachedDocumentResult {
    private Lease<RenderContent>.Owned? owned;

    public ContentTransfer(Lease<RenderContent>.Owned owned) => this.owned = owned;

    internal Fin<RenderContent> Take() =>
        Optional(Interlocked.Exchange(ref owned, null)).ToFin(Fail: new KernelFault.MissingContext())
            .Map(static lease => lease.Value);

    public void Dispose() => Interlocked.Exchange(ref owned, null)?.Dispose();
}

[ComplexValueObject]
[ValidationError]
public sealed partial class SerializerReport : IDisposable, IDetachedDocumentResult {
    public SerializerDisposition Disposition { get; }
    public ContentTransfer Content { get; }
    public string Path { get; }
    public int Index { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SerializerDisposition disposition,
        ref ContentTransfer content,
        ref string path,
        ref int index) {
        path = path?.Trim() ?? string.Empty;
        validationError = (disposition, content, path, index) switch {
            (null, _, _, _) => new ValidationError(string.Join(" | ", new object?[] { nameof(Disposition) })),
            (_, null, _, _) => new ValidationError(string.Join(" | ", new object?[] { nameof(Content) })),
            (_, _, "", _) => new ValidationError(string.Join(" | ", new object?[] { nameof(Path) })),
            (_, _, _, < 0) => new ValidationError(string.Join(" | ", new object?[] { nameof(Index), "a non-negative report index" })),
            _ => null,
        };
    }

    public void Dispose() => Content.Dispose();
}

public sealed record SerializerFailure(SerializerStage Stage, string Path, Error Fault) : IDetachedDocumentResult;

public sealed record SerializerProgram(
    ContentExtension FileExtension,
    ContentKind Kind,
    Option<Func<string, Fin<ContentTransfer>>> Read,
    Option<Func<string, RenderContent, CreatePreviewEventArgs, Fin<Unit>>> Write,
    Option<Func<RhinoDoc, Seq<string>, ContentKind, LoadPolicy, Fin<Seq<SerializerReport>>>> LoadMultiple,
    Rasm.Numerics.Dimension JournalCap,
    string EnglishDescription,
    string LocalDescription);

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShellRow {
    private ShellRow(ShellSurface surface, Type body, string caption, Option<Guid> engine) =>
        (Surface, Body, Caption, Engine) = (surface, body, caption, engine);

    internal ShellSurface Surface { get; }
    internal Type Body { get; }
    internal string Caption { get; }
    internal Option<Guid> Engine { get; }

    private sealed record PanelCase(
        ShellPanelKind Kind, CapabilitySet<PanelTrait> Visibility, SidePanePlace Place,
        Type Seat, string Label, Option<Guid> Renderer)
        : ShellRow(ShellSurface.Panel, Seat, Label, Renderer);

    private sealed record TabCase(System.Drawing.Icon Icon, Type Seat, string Label, Option<Guid> Renderer)
        : ShellRow(ShellSurface.Tab, Seat, Label, Renderer);

    public static Fin<ShellRow> Panel(
        Type body, string caption, ShellPanelKind? kind = null, CapabilitySet<PanelTrait> visibility = default,
        SidePanePlace? place = null, Option<Guid> engine = default) {
        return from keyed in Keyed(body: body)
               from label in Acceptance.Text(value: caption)
               from _ in guard(
                   engine.ForAll(static id => id != Guid.Empty),
                   (Error)new KernelFault.InvalidValue(nameof(engine), "a non-empty render engine identity"))
               select (ShellRow)new PanelCase(
                   Kind: kind ?? ShellPanelKind.RenderWindow, Visibility: visibility,
                   Place: place ?? SidePanePlace.Left, Seat: keyed, Label: label, Renderer: engine);
    }

    public static Fin<ShellRow> Tab(
        Type body, string caption, System.Drawing.Icon icon, Option<Guid> engine = default) {
        return from keyed in Keyed(body: body)
               from label in Acceptance.Text(value: caption)
               from art in Admit.Need(icon)
               from _ in guard(
                   engine.ForAll(static id => id != Guid.Empty),
                   (Error)new KernelFault.InvalidValue(nameof(engine), "a non-empty render engine identity"))
               select (ShellRow)new TabCase(Icon: art, Seat: keyed, Label: label, Renderer: engine);
    }

    internal Fin<Unit> Seat(ShellRegistrar registrar, PlugIn owner) => Switch(
        (Registrar: registrar, Owner: owner),
        panelCase: static (context, row) =>
            from panels in context.Registrar.IsPanels
                ? Admit.Need(context.Registrar.AsPanels)
                : Fin.Fail<RenderPanels>(error: new RenderFault.Unbound(Member: nameof(RenderPanels)))
            from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(() => panels.RegisterPanel(
                plugin: context.Owner,
                renderPanelType: row.Kind.Native,
                panelType: row.Body,
                renderEngineId: row.Engine.IfNone(context.Owner.Id),
                caption: row.Caption,
                alwaysShow: row.Visibility.Admits(capability: PanelTrait.AlwaysShow),
                initialShow: row.Visibility.Admits(capability: PanelTrait.InitialShow),
                pos: row.Place.Native)))).Run().Bind(static inner => inner)
            select unit,
        tabCase: static (context, row) =>
            from tabs in context.Registrar.IsTabs
                ? Admit.Need(context.Registrar.AsTabs)
                : Fin.Fail<RenderTabs>(error: new RenderFault.Unbound(Member: nameof(RenderTabs)))
            from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(() => tabs.RegisterTab(
                plugin: context.Owner,
                tabType: row.Body,
                renderEngineId: row.Engine.IfNone(context.Owner.Id),
                caption: row.Caption,
                icon: row.Icon)))).Run().Bind(static inner => inner)
            select unit);

    private static Fin<Type> Keyed(Type body) =>
        from active in Admit.Need(body)
        from _ in guard(
            active is { IsClass: true, IsPublic: true },
            (Error)new KernelFault.InvalidValue(nameof(body), "a public class"))
        from __ in guard(
            active.GetConstructor(Type.EmptyTypes) is not null,
            (Error)new KernelFault.InvalidValue(active.Name, "a parameterless constructor"))
        from ___ in guard(
            active.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false).Length == 1,
            (Error)new KernelFault.InvalidValue(active.Name, "exactly one GuidAttribute"))
        select active;
}

[Union<RenderPanels, RenderTabs>(T1Name = "Panels", T2Name = "Tabs")]
public readonly partial struct ShellRegistrar {
    internal ShellSurface Surface => IsPanels ? ShellSurface.Panel : ShellSurface.Tab;
}

public sealed record RenderShellProgram(PlugIn Owner, Seq<ShellRow> Rows) {
    public static Fin<RenderShellProgram> Of(PlugIn owner, Seq<ShellRow> rows) {
        return from active in Admit.Need(owner)
               from _ in guard(
                   !rows.IsEmpty && rows.ForAll(static row => row is not null),
                   (Error)new KernelFault.InvalidValue(nameof(RenderShellProgram), "a non-empty row set"))
               select new RenderShellProgram(Owner: active, Rows: rows.Strict());
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record ShellGate {
    private ShellGate() { }
    internal sealed record Idle : ShellGate;
    internal sealed record Armed(RenderShellProgram Program) : ShellGate;
    internal sealed record Draining(RenderShellProgram Program, CapabilitySet<ShellSurface> Spent) : ShellGate;

    internal Option<RenderShellProgram> Declared => Switch(
        idle: static _ => Option<RenderShellProgram>.None,
        armed: static state => Some(state.Program),
        draining: static state => Some(state.Program));
}

public sealed record ShellSeated(ShellSurface Surface, int Rows) : IDetachedDocumentResult;

public sealed record EditorFacts(
    Guid CurrentRenderer,
    Option<Guid> RenderingViewport,
    Seq<Size2i> CustomSizes,
    bool CustomSizeIsPreset) : IDetachedDocumentResult;

[SmartEnum<Guid>]
public sealed partial class EditorProvider {
    public static readonly EditorProvider Settings = new(
        key: global::Rhino.UI.Controls.DataSource.ProviderIds.RhinoSettings, releases: true);
    public static readonly EditorProvider Selection = new(
        key: global::Rhino.UI.Controls.DataSource.ProviderIds.ContentSelection, releases: true);
    public static readonly EditorProvider Display = new(
        key: global::Rhino.UI.Controls.DataSource.ProviderIds.ContentDisplayCollection, releases: true);

    internal bool Releases { get; }
}

public sealed record EditorBridge {
    private EditorBridge(global::Rhino.UI.Controls.IRdkViewModel model) => Model = model;

    private global::Rhino.UI.Controls.IRdkViewModel Model { get; }

    public static Fin<EditorBridge> Of(global::Rhino.UI.Controls.IRdkViewModel model) {
        return Admit.Need(model).Map(static active => new EditorBridge(model: active));
    }

    internal Fin<TOut> Use<TPayload, TOut>(EditorProvider provider, EditorIntent intent, Func<TPayload, Fin<TOut>> borrow)
        where TPayload : class {
        EditorBridge self = this;
        Fin<TOut> outcome =
            from activeProvider in Admit.Need(provider)
            from activeIntent in Admit.Need(intent)
            from activeBorrow in Admit.Need(borrow)
            from payload in Try.lift(() => Optional(self.Model.GetData(
                    uuidDataType: activeProvider.Key,
                    bForWrite: activeIntent.Writes,
                    bAutoChangeBracket: activeIntent.Brackets) as TPayload)
                .ToFin(Fail: new RenderFault.Unbound(Member: activeProvider.ToString()))).Run().Bind(static inner => inner)
            from result in Borrowed(provider: activeProvider, payload: payload, borrow: activeBorrow)
            select result;
        return intent is { Writes: true } ? self.Settle(provider: provider, outcome: outcome) : outcome;
    }

    private static Fin<TOut> Borrowed<TPayload, TOut>(
        EditorProvider provider, TPayload payload, Func<TPayload, Fin<TOut>> borrow) where TPayload : class =>
        Try.lift(() => borrow(payload)).Run().Bind(static inner => inner)
            .Settled(
                held: provider.Releases && payload is IDisposable ? Seq((IDisposable)payload) : Seq<IDisposable>(),
                release: static held => Fin.Succ(value: HostEdge.Side(held.Dispose)));

    private Fin<TOut> Settle<TOut>(EditorProvider provider, Fin<TOut> outcome) {
        EditorBridge self = this;
        return outcome.Match(
            Succ: value => Try.lift(() => Fin.Succ(value: (HostEdge.Side(() => self.Model.Commit(uuidDataType: provider.Key)), value).Item2)).Run().Bind(static inner => inner),
            Fail: primary => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => self.Model.Discard(uuidDataType: provider.Key)))).Run().Bind(static inner => inner).Match(
                Succ: static _ => Fin.Fail<TOut>(error: primary),
                Fail: discard => Fin.Fail<TOut>(error: primary + discard)));
    }
}

public sealed record ShellSeat<TBody>(TBody Body, Option<Guid> SidePaneUi) where TBody : class;

// --- [SERVICES] ------------------------------------------------------------------------
public static class RenderShell {
    private static readonly Atom<ShellGate> Gate = Atom<ShellGate>(new ShellGate.Idle());

    internal static Fin<Unit> Arm(RenderShellProgram program) =>
        Cell.Step(
                cell: Gate,
                step: state => state is ShellGate.Idle ? Some<ShellGate>(new ShellGate.Armed(Program: program)) : None,
                declined: new KernelFault.InvalidValue(nameof(RenderShell), "an unarmed shell gate"))
            .Switch(
                state: op,
                committed: static (_, _) => Fin.Succ(value: unit),
                ceded: static (_) => Fin.Fail<Unit>(error: new RenderFault.SeatTaken(Engine: Guid.Empty)),
                refused: static (_, row) => Fin.Fail<Unit>(error: row.Cause),
                contended: static (_) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()));

    public static Fin<ShellSeated> Drain(ShellRegistrar registrar) {
        return from claimed in Claim(surface: registrar.Surface)
               let rows = claimed.Rows.Filter(row => row.Surface == registrar.Surface)
               from _ in rows.TraverseM(row => row.Seat(registrar: registrar, owner: claimed.Owner)).As()
               select new ShellSeated(Surface: registrar.Surface, Rows: rows.Count);
    }

    public static Fin<Option<ShellSeat<TBody>>> Resolve<TBody>(PlugIn owner, Guid session)
        where TBody : class {
        return from active in Admit.Need(owner)
               from _ in guard(session != Guid.Empty, (Error)new KernelFault.InvalidValue(nameof(session), "a non-empty render session identity"))
               from program in Gate.Value.Declared.ToFin(Fail: new KernelFault.MissingContext())
               from row in program.Rows.Find(candidate => candidate.Body == typeof(TBody))
                   .ToFin(Fail: new RenderFault.SeatAbsent(Engine: Guid.Empty))
               from found in Try.lift(() => Fin.Succ(value: Optional(row.Surface == ShellSurface.Panel
                   ? RenderPanels.FromRenderSessionId(plugIn: active, panelType: typeof(TBody), renderSessionId: session)
                   : RenderTabs.FromRenderSessionId(plugIn: active, tabType: typeof(TBody), renderSessionId: session)))).Run().Bind(static inner => inner)
               from seat in found.Traverse(body => Try.lift(() =>
                   Optional(body as TBody).ToFin(Fail: new RenderFault.Unbound(Member: typeof(TBody).Name))
                       .Map(typed => new ShellSeat<TBody>(
                           Body: typed,
                           SidePaneUi: row.Surface == ShellSurface.Panel ? Option<Guid>.None : SidePaneUi(tab: body)))).Run().Bind(static inner => inner)).As()
               select seat;
    }

    private static Fin<RenderShellProgram> Claim(ShellSurface surface) =>
        Cell.Step(
                cell: Gate,
                step: state => state switch {
                    ShellGate.Armed armed => Some<ShellGate>(new ShellGate.Draining(
                        Program: armed.Program, Spent: CapabilitySet<ShellSurface>.Of(surface))),
                    ShellGate.Draining draining when !draining.Spent.Admits(capability: surface) =>
                        Some<ShellGate>(draining with { Spent = draining.Spent.With(capability: surface) }),
                    _ => None,
                },
                declined: new RenderFault.SeatTaken(Engine: Guid.Empty))
            .Switch(
                state: op,
                committed: static (row) => row.State.Declared.ToFin(Fail: new KernelFault.MissingContext()),
                ceded: static (_) => Fin.Fail<RenderShellProgram>(error: new RenderFault.SeatTaken(Engine: Guid.Empty)),
                refused: static (_, row) => Fin.Fail<RenderShellProgram>(error: row.Cause),
                contended: static (_) => Fin.Fail<RenderShellProgram>(error: new KernelFault.InvalidResult()));

    private static Option<Guid> SidePaneUi(object tab) =>
        Optional(RenderTabs.SidePaneUiIdFromTab(tab: tab)).Filter(static id => id != Guid.Empty);
}

public sealed class ContentSerializer : RenderContentSerializer {
    private readonly SerializerProgram program;
    private readonly Ring<SerializerFailure> failures;

    private ContentSerializer(SerializerProgram program)
        : base(fileExtension: program.FileExtension.Value, contentKind: (RenderContentKind)program.Kind.Key,
               canRead: program.Read.IsSome, canWrite: program.Write.IsSome) {
        this.program = program;
        failures = new Ring<SerializerFailure>(cap: program.JournalCap);
    }

    public static Fin<ContentSerializer> Of(SerializerProgram program) {
        return from active in Admit.Need(program)
               from extension in Admit.Need(active.FileExtension)
               from kind in Admit.Need(active.Kind)
               from english in Acceptance.Text(active.EnglishDescription)
               from local in Acceptance.Text(active.LocalDescription)
               from _ in guard(
                   active.Read.IsSome || active.Write.IsSome || active.LoadMultiple.IsSome,
                   (Error)new KernelFault.InvalidValue(nameof(SerializerProgram), "at least one serializer route"))
               select new ContentSerializer(active with {
                   FileExtension = extension,
                   Kind = kind,
                   EnglishDescription = english,
                   LocalDescription = local,
               });
    }

    public override string EnglishDescription => program.EnglishDescription;
    public override string LocalDescription => program.LocalDescription;
    public Seq<SerializerFailure> Parked => failures.Parked;
    public long Shed => failures.Shed;
    public long Lost => failures.Lost;

    [return: MaybeNull]
    public override RenderContent Read(string pathToFile) {
        return (from path in Acceptance.Text(pathToFile)
                from read in program.Read.ToFin(Fail: new RenderFault.Unbound(Member: nameof(Read)))
                from transfer in Try.lift(() => read(path)).Run().Bind(static inner => inner)
                from active in Optional(transfer).ToFin(Fail: new KernelFault.InvalidResult())
                from content in active.Take()
                select content).Match(
                    Succ: static content => content,
                    Fail: fault => Reject<RenderContent>(SerializerStage.Read, pathToFile, fault));
    }

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs previewArgs) {
        return (from path in Acceptance.Text(pathToFile)
                from content in Admit.Need(renderContent)
                from preview in Admit.Need(previewArgs)
                from write in program.Write.ToFin(Fail: new RenderFault.Unbound(Member: nameof(Write)))
                from _ in Try.lift(() => write(path, content, preview)).Run().Bind(static inner => inner)
                select unit).Match(
                    Succ: static _ => true,
                    Fail: fault => Reject(SerializerStage.Write, pathToFile, fault));
    }

    public override bool CanLoadMultiple() => program.LoadMultiple.IsSome;

    public override bool LoadMultiple(
        RhinoDoc document, IEnumerable<string> paths, RenderContentKind kind, RenderContentSerializer.LoadMultipleFlags flags) {
        return (from activeDocument in Admit.Need(document)
                from activePaths in Admit.Need(paths)
                from files in Try.lift(() => Fin.Succ(toSeq(activePaths))).Run().Bind(static inner => inner)
                from _0 in guard(
                    !files.IsEmpty && files.ForAll(static path => !string.IsNullOrWhiteSpace(path)),
                    (Error)new KernelFault.InvalidValue(nameof(paths), "a non-empty path set"))
                from load in program.LoadMultiple.ToFin(Fail: new RenderFault.Unbound(Member: nameof(LoadMultiple)))
                from admittedKind in ContentKind.Of(kind, op)
                from policy in LoadPolicy.Of(flags, op)
                from reports in Try.lift(() => load(activeDocument, files, admittedKind, policy)).Run().Bind(static inner => inner)
                from _ in reports.Map(report => Emit(report, op)).Strict()
                    .Fold(Fin.Succ(value: unit), static (state, outcome) => state.Bind(_ => outcome))
                select unit).Match(
                    Succ: static _ => true,
                    Fail: fault => Reject(SerializerStage.Load, string.Empty, fault));
    }

    internal Fin<Unit> Register(PluginKey plugin) {
        return (from _ in plugin.Admit()
                from result in Try.lift(() => Admit.Confirm(success: RegisterSerializer(id: plugin.ToValue()))).Run().Bind(static inner => inner)
                select result)
            .MapFail(fault => (HostEdge.Side(() => Retain(stage: SerializerStage.Register, path: string.Empty, fault: fault)), fault).Item2);
    }

    private Fin<Unit> Emit(SerializerReport report) =>
        (from active in Optional(report).ToFin(Fail: new KernelFault.InvalidResult())
         from transfer in Optional(active.Content).ToFin(Fail: new KernelFault.InvalidResult())
         from path in Acceptance.Text(active.Path)
         from content in transfer.Take()
         from _ in Try.lift(() => Fin.Succ(value: active.Disposition.Fold(
                 loaded: () => HostEdge.Side(() => ReportContentAndFile(content, path, active.Index)),
                 deferred: () => HostEdge.Side(() => ReportDeferredContentAndFile(content, path, active.Index))))).Run().Bind(static inner => inner)
             .Rollback(release: () => Try.lift(() => Fin.Succ(value: HostEdge.Side(content.Dispose))).Run().Bind(static inner => inner))
         select unit)
        .Settled(
            held: Seq(report),
            release: static row => Fin.Succ(value: HostEdge.Side(row.Dispose)));

    private bool Reject(SerializerStage stage, string path, Error error) {
        Retain(stage: stage, path: path, fault: error);
        return false;
    }

    private Unit Retain(SerializerStage stage, string path, Error fault) =>
        ignore(failures.Park(item: new SerializerFailure(Stage: stage, Path: path, Fault: fault)));

    [return: MaybeNull]
    private T Reject<T>(SerializerStage stage, string path, Error error) where T : class {
        _ = Reject(stage, path, error);
        return default;
    }
}
```

## [03]-[OPERATION_FAMILY]

- Owner: `ContentOp` `[Union]` derives from target identity: `Admit(ContentAdmission)` has no existing target, and `Mutate(ContentRef, ContentMutation)` resolves one target once. `ContentAdmission` closes each mint path behind one owned-lease bracket. `ContentMutation` carries catalogued host concerns; `TreeMutation` and `Grouping` close their bounded subspaces without boolean modes.
- Law: admission internalizes custody — every factory, IO, material, texture, and environment mint becomes an owned lease; top-level results transfer through the expected kind table, parented factory results transfer through the parent slot, and every refused transfer disposes the lease.
- Law: transaction kind is a verified table-scope key — each admission exposes its expected kind, each target mutation derives its live kind, and either must equal the plan kind before mutation.
- Law: undo participation is a ROW on the mutation family, not a derived negation. `RecordsUndo` read `this is not Export` on one union and re-derived the same fact through `Optional(...).Map(...).IfNone(false)` on another, so a second non-recording case had to be added in two places and the outer derivation answered `false` for a null change it owed a refusal. Each case declares its `UndoPolicy` and the plan reads the column.
- Law: graph surgery is one target mutation — `TreeMutation` discriminates graft, prune, and slot state under its own `ChangeReason`; graft and parented admission prove `IsContentTypeAcceptableAsChild` before `SetChild`, and slot-state admission rejects an empty patch.
- Law: an object-reference roster acquires on the carrier, not in a counter loop. The assignment arm minted a fixed array, tracked a seat counter, and released in a `finally` that discarded its own refusals; each `ObjRef` is now a `Lease` acquired through a fold that rolls the already-held set back on the first refusal, and the assignment body releases through the package's both-arms fold so a release fault appends to the assignment's.
- Law: field, parameter, and texture writes compose their owners; material assignment resolves `TableTarget`, contains every `ObjRef` lifetime, and carries native assignment choices.
- Law: every address input takes its spine owner — a content or object identity is `ResourceId` and a write target is a `DocumentPath` — so an empty guid or relative path refuses before host mutation.
- Growth: a new admission path is one `ContentAdmission` case; a new target concern is one `ContentMutation` case with its `UndoPolicy` column; `ContentOp` keeps its identity-derived cases.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.Create`, `SetChild`, `DeleteChild`, `DeleteAllChildren`, `SetChildSlotOn`, `SetChildSlotAmount`, `IsContentTypeAcceptableAsChild`, `SetName`, `Replace`, `SaveToFile`, `MakeGroupInstance`, `Ungroup`, `UngroupRecursive`, `SmartUngroupRecursive`, `ExtraRequirementsSetContexts`, `EmbedFilesChoice`); `api-rhinocommon-render.md` (`RenderMaterial.AssignTo`, `AssignToSubFaceChoices`, `AssignToBlockChoices`); `api-rhinocommon-objects.md` (`ObjRef`); kernel `Domain/results` (`Lease<T>.Acquire`, `Try.lift`, `Admit.Confirm`), `Domain/validation` (`FactoryBridge.Row`); `Render/content.md` (`ContentKind`, `ContentRef`, `ChangeReason`, `ChangeScope`, `ContentIo`), `Render/fields.md` (`ContentValue`, `ParamScope`), `Render/kinds.md` (`TextureConfig`, `MaterialMint`, `TextureMint`, `EnvironmentState`, `TextureExport`); `Document/tables.md` (`ResourceId`, `TableTarget`), `Document/session.md` (`DocumentPath`), kernel `Domain/results` (`Custody`); LanguageExt.Core; Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class UndoPolicy {
    public static readonly UndoPolicy Skip = new(false);
    public static readonly UndoPolicy Record = new(true);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentAdmission {
    private ContentAdmission() { }
    public sealed record Factory(ContentKind Kind, ResourceId TypeId, Option<(ContentRef Parent, string Slot)> Into) : ContentAdmission;
    public sealed record Serialized(ContentKind Kind, ContentIo Source) : ContentAdmission;
    public sealed record Material(MaterialMint Source) : ContentAdmission;
    public sealed record Texture(TextureMint Source) : ContentAdmission;
    public sealed record Environment(EnvironmentState State) : ContentAdmission;

    internal ContentKind Expected => Switch(
        factory: static row => row.Kind,
        serialized: static row => row.Kind,
        material: static _ => ContentKind.Material,
        texture: static _ => ContentKind.Texture,
        environment: static _ => ContentKind.Environment);

    internal Fin<Unit> Apply(RhinoDoc document, ChangeReason reason) =>
        Switch(
            context: (Document: document, Reason: reason),
            factory: static (context, source) =>
                from kind in Admit.Need(source.Kind)
                from type in Admit.Need(source.TypeId)
                from parent in source.Into.Traverse(into =>
                    from slot in Acceptance.Text(value: into.Slot)
                    from target in Admit.Need(into.Parent)
                    from live in target.Resolve(document: context.Document)
                    select (Content: live, Slot: slot)).As()
                from lease in Lease<RenderContent>.Acquire(
                    mint: () => RenderContent.Create(context.Document, type.ToValue()))
                from transferred in Transfer(
                    expected: kind,
                    lease: lease,
                    document: context.Document,
                    parent: parent,
                    reason: context.Reason)
                select transferred,
            serialized: static (context, source) =>
                from kind in Admit.Need(source.Kind)
                from adopted in Adopted(kind, source.Source, static (io, ctx) => io.Mint(document: ctx.Document), context)
                select adopted,
            material: static (context, source) =>
                Adopted(ContentKind.Material, source.Source, static (mint, ctx) => mint.Mint(document: ctx.Document), context),
            texture: static (context, source) =>
                Adopted(ContentKind.Texture, source.Source, static (mint, ctx) => mint.Mint(document: ctx.Document), context),
            environment: static (context, source) =>
                Adopted(ContentKind.Environment, source.State, static (state, ctx) => state.Mint(document: ctx.Document), context));

    private static Fin<Unit> Adopted<TSource>(
        ContentKind expected,
        TSource? source,
        Func<TSource, (RhinoDoc Document, ChangeReason Reason), Fin<Lease<RenderContent>>> mint,
        (RhinoDoc Document, ChangeReason Reason) context) where TSource : class =>
        from active in Admit.Need(source)
        from lease in mint(active, context)
        from transferred in Transfer(
            expected: expected, lease: lease, document: context.Document,
            parent: Option<(RenderContent, string)>.None, reason: context.Reason)
        select transferred;

    private static Fin<Unit> Transfer(
        ContentKind expected, Lease<RenderContent> lease, RhinoDoc document,
        Option<(RenderContent Content, string Slot)> parent, ChangeReason reason) =>
        (from actual in ContentKind.Of(lease.Resource)
         from _ in guard(actual == expected, (Error)new KernelFault.InvalidValue(nameof(ContentKind), expected.ToString()))
         from transferred in parent.Case switch {
             (RenderContent content, string slot) =>
                 from _acceptable in TreeMutation.Accepts(
                     parent: content, child: lease.Resource, slot: slot)
                 from _written in ChangeScope.Write(
                     content: content, reason: reason,
                     body: live => Try.lift(() => Admit.Confirm(success: live.SetChild(renderContent: lease.Resource, childSlotName: slot))).Run().Bind(static inner => inner))
                 select unit,
             _ => expected.Attach(document: document, content: lease.Resource),
         }
         select transferred)
        .Rollback(release: () => Try.lift(() => Fin.Succ(value: lease.Dispose())).Run().Bind(static inner => inner));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TreeMutation {
    private TreeMutation() { }
    public sealed record Graft(string Slot, ContentRef Child, ChangeReason Reason) : TreeMutation;
    public sealed record Prune(Option<string> Slot, ChangeReason Reason) : TreeMutation;
    public sealed record Slot(string Name, Option<bool> On, Option<double> Amount, ChangeReason Reason) : TreeMutation;

    internal Fin<Unit> Apply(RenderContent parent, RhinoDoc document) =>
        Switch(
            context: (Parent: parent, Document: document),
            graft: static (ctx, edit) =>
                from slot in Acceptance.Text(value: edit.Slot)
                from target in Admit.Need(edit.Child)
                from reason in Admit.Need(edit.Reason)
                from child in target.Resolve(document: ctx.Document)
                from _acceptable in Accepts(parent: ctx.Parent, child: child, slot: slot)
                from _ in ChangeScope.Write(content: ctx.Parent, reason: reason,
                    body: live => Try.lift(() => Admit.Confirm(success: live.SetChild(renderContent: child, childSlotName: slot))).Run().Bind(static inner => inner))
                select unit,
            prune: static (ctx, edit) =>
                from reason in Admit.Need(edit.Reason)
                from slot in edit.Slot.Traverse(value => Acceptance.Text(value: value)).As()
                from _ in ChangeScope.Write(content: ctx.Parent, reason: reason,
                    body: live => slot.Case switch {
                        string name => Try.lift(() => Admit.Confirm(success: live.DeleteChild(name, reason.Native))).Run().Bind(static inner => inner),
                        _ => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => live.DeleteAllChildren(reason.Native)))).Run().Bind(static inner => inner),
                    })
                select unit,
            slot: static (ctx, edit) =>
                from name in Acceptance.Text(value: edit.Name)
                from reason in Admit.Need(edit.Reason)
                from _ in guard(
                    (edit.On.IsSome || edit.Amount.IsSome)
                    && edit.Amount.Map(static amount => (bool)ValidityClaim.Finite(value: amount)).IfNone(true),
                    (Error)new KernelFault.InvalidValue(nameof(TreeMutation.Slot), "at least one finite slot patch"))
                from __ in ChangeScope.Write(content: ctx.Parent, reason: reason, body: live => Try.lift(() =>
                    Fin.Succ(value: HostEdge.Side(() => {
                        ignore(edit.On.Iter(on => live.SetChildSlotOn(name, on, reason.Native)));
                        ignore(edit.Amount.Iter(amount => live.SetChildSlotAmount(name, amount, reason.Native)));
                    }))).Run().Bind(static inner => inner))
                select unit);

    internal static Fin<Unit> Accepts(RenderContent parent, RenderContent child, string slot) =>
        Try.lift(() => Admit.Confirm(success: parent.IsContentTypeAcceptableAsChild(
            type: child.TypeId,
            childSlotName: slot))).Run().Bind(static inner => inner);
}

[SmartEnum<string>]
public sealed partial class Grouping {
    public static readonly Grouping Make = new("make", static (content, op) =>
        from _ in Try.lift(() => Optional(content.MakeGroupInstance()).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
        select unit);
    public static readonly Grouping Ungroup = new("ungroup", Undone(static content => content.Ungroup()));
    public static readonly Grouping Recursive = new("recursive", Undone(static content => content.UngroupRecursive()));
    public static readonly Grouping Smart = new("smart", Undone(static content => content.SmartUngroupRecursive()));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Apply(RenderContent content);

    private static Func<RenderContent, Fin<Unit>> Undone(Func<RenderContent, bool> route) =>
        (content, op) =>
            from _ in Try.lift(() => Admit.Confirm(success: route(content))).Run().Bind(static inner => inner)
            select unit;
}

[SmartEnum<bool>]
public sealed partial class RenamePolicy {
    public static readonly RenamePolicy Exact = new(false);
    public static readonly RenamePolicy Unique = new(true);
}

[SmartEnum<int>]
public sealed partial class ExtraRequirementReason {
    public static readonly ExtraRequirementReason Ui = new(key: (int)RenderContent.ExtraRequirementsSetContexts.UI);
    public static readonly ExtraRequirementReason Drop = new(key: (int)RenderContent.ExtraRequirementsSetContexts.Drop);
    public static readonly ExtraRequirementReason Program = new(key: (int)RenderContent.ExtraRequirementsSetContexts.Program);

    internal RenderContent.ExtraRequirementsSetContexts Native => (RenderContent.ExtraRequirementsSetContexts)Key;
}

[SmartEnum<int>]
public sealed partial class SubFaceAssignment {
    public static readonly SubFaceAssignment Keep = new(key: (int)RenderMaterial.AssignToSubFaceChoices.Keep);
    public static readonly SubFaceAssignment Remove = new(key: (int)RenderMaterial.AssignToSubFaceChoices.Remove);
    public static readonly SubFaceAssignment Ask = new(key: (int)RenderMaterial.AssignToSubFaceChoices.Ask);

    internal RenderMaterial.AssignToSubFaceChoices Native => (RenderMaterial.AssignToSubFaceChoices)Key;
}

[SmartEnum<int>]
public sealed partial class BlockAssignment {
    public static readonly BlockAssignment Always = new(key: (int)RenderMaterial.AssignToBlockChoices.Always);
    public static readonly BlockAssignment Never = new(key: (int)RenderMaterial.AssignToBlockChoices.Never);
    public static readonly BlockAssignment Ask = new(key: (int)RenderMaterial.AssignToBlockChoices.Ask);

    internal RenderMaterial.AssignToBlockChoices Native => (RenderMaterial.AssignToBlockChoices)Key;
}

[SmartEnum<int>]
public sealed partial class EmbedPolicy {
    public static readonly EmbedPolicy Never = new(key: (int)RenderContent.EmbedFilesChoice.NeverEmbed);
    public static readonly EmbedPolicy Always = new(key: (int)RenderContent.EmbedFilesChoice.AlwaysEmbed);
    public static readonly EmbedPolicy Ask = new(key: (int)RenderContent.EmbedFilesChoice.AskUser);

    internal RenderContent.EmbedFilesChoice Native => (RenderContent.EmbedFilesChoice)Key;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentExport {
    private ContentExport() { }
    public sealed record Archive(DocumentPath Path, EmbedPolicy Embed) : ContentExport;
    public sealed record TextureImage(DocumentPath Path, Size2i Extent, int Depth) : ContentExport;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentMutation {
    private ContentMutation(UndoPolicy undo) => Undo = undo;

    internal UndoPolicy Undo { get; }

    public sealed record Detach : ContentMutation { public Detach() : base(UndoPolicy.Record) { } }
    public sealed record Rename(string Name, ChangeReason Reason, RenamePolicy Policy)
        : ContentMutation(UndoPolicy.Record);
    public sealed record Tree(TreeMutation Edit) : ContentMutation(UndoPolicy.Record);
    public sealed record Field(string Name, ContentValue Value, ChangeReason Reason)
        : ContentMutation(UndoPolicy.Record);
    public sealed record Param(
        ParamScope Scope, ContentValue Value, ChangeReason Reason,
        ExtraRequirementReason Context) : ContentMutation(UndoPolicy.Record);
    public sealed record Texture(TextureConfig Config, ChangeReason Reason) : ContentMutation(UndoPolicy.Record);
    public sealed record Assign(TableTarget Objects, SubFaceAssignment SubFaces, BlockAssignment Blocks)
        : ContentMutation(UndoPolicy.Record);
    public sealed record Replace(ContentIo Source) : ContentMutation(UndoPolicy.Record);
    public sealed record Group(Grouping Mode) : ContentMutation(UndoPolicy.Record);
    public sealed record Export(ContentExport Output) : ContentMutation(UndoPolicy.Skip);

    internal Fin<Unit> Apply(RenderContent content, RhinoDoc document) =>
        Switch(
            context: (Content: content, Document: document),
            detach: static (ctx, _) =>
                from kind in ContentKind.Of(ctx.Content)
                from _ in kind.Detach(document: ctx.Document, content: ctx.Content)
                select unit,
            rename: static (ctx, edit) =>
                from name in Acceptance.Text(value: edit.Name)
                from reason in Admit.Need(edit.Reason)
                from policy in Admit.Need(edit.Policy)
                from _ in ChangeScope.Write(ctx.Content, reason, live => Try.lift(() =>
                    Fin.Succ(value: HostEdge.Side(() => live.SetName(name, renameEvents: true, ensureNameUnique: policy.Key)))).Run().Bind(static inner => inner))
                select unit,
            tree: static (ctx, edit) =>
                from change in Admit.Need(edit.Edit)
                from changed in change.Apply(parent: ctx.Content, document: ctx.Document)
                select changed,
            field: static (ctx, edit) =>
                from name in Acceptance.Text(value: edit.Name)
                from value in Admit.Need(edit.Value)
                from reason in Admit.Need(edit.Reason)
                from _ in ChangeScope.Write(ctx.Content, reason, live => value.Write(live.Fields, name))
                select unit,
            param: static (ctx, edit) =>
                from scope in Admit.Need(edit.Scope)
                from value in Admit.Need(edit.Value)
                from reason in Admit.Need(edit.Reason)
                from context in Admit.Need(edit.Context)
                from _ in scope.Write(ctx.Content, value, reason, context.Native)
                select unit,
            texture: static (ctx, edit) =>
                from texture in Admit.Need(ctx.Content as RenderTexture)
                from config in Admit.Need(edit.Config)
                from reason in Admit.Need(edit.Reason)
                from _ in config.Apply(texture, reason)
                select unit,
            assign: static (ctx, edit) =>
                from material in Admit.Need(ctx.Content as RenderMaterial)
                from objects in Admit.Need(edit.Objects)
                from subFaces in Admit.Need(edit.SubFaces)
                from blocks in Admit.Need(edit.Blocks)
                from ids in objects.Resolve(document: ctx.Document)
                from references in ids.FoldM<Fin, Seq<Lease<ObjRef>>>(
                    Seq<Lease<ObjRef>>(),
                    (held, id) => Lease<ObjRef>.Acquire(mint: () => new ObjRef(ctx.Document, id))
                        .Map(held.Add)
                        .Rollback(held: held, release: Release))
                from _ in Try.lift(() => Admit.Confirm(success: material.AssignTo(
                        references.Map(static held => held.Resource).ToArray(),
                        subFaces.Native, blocks.Native, bInteractive: false))).Run().Bind(static inner => inner)
                    .Settled(held: references, release: Release)
                select unit,
            replace: static (ctx, edit) =>
                from source in Admit.Need(edit.Source)
                from lease in source.Mint(document: ctx.Document)
                from replaced in ReplaceWith(target: ctx.Content, lease: lease)
                select replaced,
            group: static (ctx, edit) =>
                from mode in Admit.Need(edit.Mode)
                from grouped in mode.Apply(content: ctx.Content)
                select grouped,
            export: static (ctx, edit) =>
                from output in Admit.Need(edit.Output)
                from exported in output.Switch(
                    context: ctx,
                    archive: static (state, archive) =>
                        from embed in Admit.Need(archive.Embed)
                        from path in Admit.Need(archive.Path)
                        from _ in Try.lift(() => Admit.Confirm(
                            success: state.SaveToFile(path.ToValue(), embed.Native))).Run().Bind(static inner => inner)
                        select unit,
                    textureImage: static (state, image) =>
                        from texture in Admit.Need(state as RenderTexture)
                        from path in Admit.Need(image.Path)
                        from _ in TextureExport.Export(
                            texture: texture, path: path.ToValue(),
                            width: image.Extent.Width, height: image.Extent.Height, depth: image.Depth)
                        select unit)
                select exported);

    private static Fin<Unit> Release(Lease<ObjRef> held) => Fin.Succ(value: held.Dispose());

    private static Fin<Unit> ReplaceWith(RenderContent target, Lease<RenderContent> lease) =>
        (from targetKind in ContentKind.Of(target)
         from replacementKind in ContentKind.Of(lease.Resource)
         from _ in guard(targetKind == replacementKind, (Error)new KernelFault.InvalidValue(nameof(ContentKind), targetKind.ToString()))
         from __ in Try.lift(() => Admit.Confirm(success: target.Replace(newcontent: lease.Resource))).Run().Bind(static inner => inner)
         select unit)
        .Rollback(release: () => Try.lift(() => Fin.Succ(value: lease.Dispose())).Run().Bind(static inner => inner));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentOp {
    private ContentOp() { }
    public sealed record Admit(ContentAdmission Source) : ContentOp;
    public sealed record Mutate(ContentRef Target, ContentMutation Change) : ContentOp;

    internal UndoPolicy Undo => Switch(
        admit: static _ => UndoPolicy.Record,
        mutate: static edit => edit.Change.Undo);

    internal Fin<Unit> Apply(RhinoDoc document, ContentKind scope, ChangeReason reason) =>
        Switch(
            context: (Document: document, Scope: scope, Reason: reason),
            admit: static (ctx, edit) =>
                from source in Admit.Need(edit.Source)
                from _ in guard(source.Expected == ctx.Scope, (Error)new KernelFault.InvalidValue(nameof(ContentKind), ctx.Scope.ToString()))
                from admitted in source.Apply(document: ctx.Document, reason: ctx.Reason)
                select admitted,
            mutate: static (ctx, edit) =>
                from target in Admit.Need(edit.Target)
                from change in Admit.Need(edit.Change)
                from content in target.Resolve(document: ctx.Document)
                from kind in ContentKind.Of(content)
                from _ in guard(kind == ctx.Scope, (Error)new KernelFault.InvalidValue(nameof(ContentKind), ctx.Scope.ToString()))
                from changed in change.Apply(content: content, document: ctx.Document)
                select changed);
}
```

## [04]-[COMMIT_AND_QUERY]

- Owner: `RegistryCommand` closes content registration, serializer registration, shell arming, and document mutation; `RegistryResult` keeps each outcome distinct; `Registry.Run` is the sole change entry.
- Owner: `RegistryQuery<T>` closes target reads, rosters, current environments, and the two-tier factory census; `Registry.Read<T>` preserves result correlation through `IDetachedDocumentResult`.
- Owner: `IconModality` carries each verified host icon route as one bind row and `IconRequest` pairs it with an extent; `KindScope` closes what a kind list holds and `CollectionTrait` carries the collection's switches.
- Law: the spine is the one bracket owner — the whole mutation runs inside one `Demand` window, the undo record opens through the document `UndoBracket` only when the plan records, the plan's kind opens its table change scope around the fold through `ContentKind.Table`, redraw suppression restores prior state, and the bracket's `Seal` rolls a failed owned record back before the fault leaves. `ContentKind.Table` owns that window, never this page — it carries the `Lease<TableScope>` custody and the aggregating close, so an open/body/close triple spelled here is a second window whose close refusal REPLACES the body's fault instead of appending to it.
- Law: grants are proven per plan shape against one snapshot — `Mutate` always, `Undo` when the plan records, `Redraw` when the plan redraws — and the session is the only document ingress; the redraw vocabulary is the document `RedrawPolicy` rows, shared with the table and block pipelines.
- Law: an icon route is a ROW, not a case. Three request cases and a four-row policy enum described SIX host calls between them, each re-spelling the same "did the host draw, and who owns the bitmap it handed back" custody hop; `IconModality` names the six routes once, the request is one record, and the custody funnel exists in one place — the bitmap the host hands back on a false answer releases through `Lease<T>.Use` instead of a hand-written dispose beside the failure return.
- Law: kind evidence is a SCOPE, not three columns. A count, a membership bool, and an optional single kind re-derived one another, so a reader held `KindCount == 1` beside `SingleKind = None`; `KindScope` states empty, single, or many once and the membership probe stays its own trait.
- Law: reads never open an undo record; every answer is a detached fact or a capsule with result-typed release.
- Law: `ContentCollectionProbe.Of` admits collection and kind-list leases once and `Mint` is the corpus producer over the two host constructors; `Release()` delegates both leases to kernel custody, so owned cases retire and borrowed cases remain host-owned without a disposer fault being discarded.
- Law: the workflow-corrected hash resolves the document's own `LinearWorkflow` inside the query window and hands it to `HashProbe.Read` as the argument that SELECTS the host route, through the settings page's `SubOwners` bracket so the read is one coherent wrapper set; a live sub-owner never enters or leaves a query value, and the witness records the scope the read took.
- Boundary: the current-environment triple reads through `RhinoDoc.CurrentEnvironment`; the settings-side per-usage binding is the settings page's edit pipeline, and the two never merge.
- Growth: a new icon route is one `IconModality` row; a new query is one `ContentQuery<T>` or `RegistryQuery<T>` value with no entry change.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.Icon`, `VirtualIcon`, `DynamicIcon`, `DynamicIconUsage`, `MatchData`, `MatchDataResult`, `IsCompatible`, `Xml`, `GetEmbeddedFilesList`); `api-rhinocommon-render.md` (`RenderContentCollection` and its `GetFilterContentByUsage`/`Count`/`ContentAt`/`GetForcedVaries`/`GetSearchPattern`/`ContentNeedsPreviewThumbnail`, `RenderContentKindList` and its `Add`/`Count`/`Contains`/`SingleKind`, `FilterContentByUsage`, `RenderTexture.TextureGeneration`); `api-rhinocommon-document.md` (`RhinoDoc.CurrentEnvironment`, `ICurrentEnvironment`); kernel `Domain/results` (`Lease<T>.Acquire`/`Use`, `Try.lift`), `Domain/validation` (`CapabilitySet`, `FactoryBridge.Row`); `Render/content.md` (`HashProbe`, `HashWitness`, `ContentSnapshot`, `ContentKind`), `Render/settings.md` (`SubOwners`), `Render/kinds.md` (`BakeScope`, `MaterialBridge`, `MaterialScent`, `TextureConfig`, `TextureTraits`, `EnvironmentState`), `Render/fields.md` (`FieldCensus`, `ContentValue`, `ParamScope`); `Document/session.md` (`DocumentSession`, `SessionNeed`), `Document/commit.md` (`DocumentCommit.Sealed`, `RedrawPolicy`), kernel `Domain/results` (`Custody`); LanguageExt.Core; Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CollectionTrait : ICapability<CollectionTrait> {
    public static readonly CollectionTrait ForcedVaries = new(key: "forced-varies");
    public static readonly CollectionTrait NeedsPreview = new(key: "needs-preview");
    public static readonly CollectionTrait ContainsOwnKind = new(key: "contains-own-kind");
}

[SmartEnum<int>]
public sealed partial class ContentUsageFilter {
    public static readonly ContentUsageFilter None = new(key: (int)FilterContentByUsage.None);
    public static readonly ContentUsageFilter Used = new(key: (int)FilterContentByUsage.Used);
    public static readonly ContentUsageFilter Unused = new(key: (int)FilterContentByUsage.Unused);
    public static readonly ContentUsageFilter UsedSelected = new(key: (int)FilterContentByUsage.UsedSelected);

    internal FilterContentByUsage Native => (FilterContentByUsage)Key;

    internal static Fin<ContentUsageFilter> Of(FilterContentByUsage native) =>
        FactoryBridge.Row<FilterContentByUsage, ContentUsageFilter>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class MatchVerdict {
    public static readonly MatchVerdict None = new(key: (int)RenderContent.MatchDataResult.None);
    public static readonly MatchVerdict Some = new(key: (int)RenderContent.MatchDataResult.Some);
    public static readonly MatchVerdict All = new(key: (int)RenderContent.MatchDataResult.All);

    internal RenderContent.MatchDataResult Native => (RenderContent.MatchDataResult)Key;

    internal static Fin<MatchVerdict> Of(RenderContent.MatchDataResult native) =>
        FactoryBridge.Row<RenderContent.MatchDataResult, MatchVerdict>(native, static value => (int)value);
}

[SmartEnum<string>]
public sealed partial class IconModality {
    public static readonly IconModality Standard = new("standard", static (content, extent) =>
        (content.Icon(extent, out System.Drawing.Bitmap rendered), rendered));
    public static readonly IconModality Virtual = new("virtual", static (content, extent) =>
        (content.VirtualIcon(extent, out System.Drawing.Bitmap rendered), rendered));
    public static readonly IconModality Tree = new("tree", Dynamic(DynamicIconUsage.TreeControl));
    public static readonly IconModality Subnode = new("subnode", Dynamic(DynamicIconUsage.SubnodeControl));
    public static readonly IconModality Control = new("control", Dynamic(DynamicIconUsage.ContentControl));
    public static readonly IconModality General = new("general", Dynamic(DynamicIconUsage.General));

    [UseDelegateFromConstructor]
    internal partial (bool Drawn, System.Drawing.Bitmap? Image) Draw(RenderContent content, System.Drawing.Size extent);

    private static Func<RenderContent, System.Drawing.Size, (bool, System.Drawing.Bitmap?)> Dynamic(DynamicIconUsage usage) =>
        (content, extent) => (content.DynamicIcon(extent, out System.Drawing.Bitmap rendered, usage), rendered);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KindScope {
    private KindScope() { }
    public sealed record Empty : KindScope;
    public sealed record Single(ContentKind Kind) : KindScope;
    public sealed record Many(int Count) : KindScope;

    internal static Fin<KindScope> Of(RenderContentKindList kinds) =>
        Try.lift(() => kinds.Count() switch {
            0 => Fin.Succ<KindScope>(new Empty()),
            1 => ContentKind.Of(kinds.SingleKind()).Map(static kind => (KindScope)new Single(Kind: kind)),
            var count => Fin.Succ<KindScope>(new Many(Count: count)),
        }).Run().Bind(static inner => inner);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentTransaction(
    string Name,
    ContentKind Kind,
    Seq<ContentOp> Operations,
    ChangeReason Reason,
    RedrawPolicy Redraw,
    UndoPolicy Undo) {
    public static ContentTransaction Batch(
        string name, ContentKind kind, ChangeReason reason, params ReadOnlySpan<ContentOp> operations) =>
        new(
            Name: name,
            Kind: kind,
            Operations: toSeq(operations.ToArray()),
            Reason: reason,
            Redraw: RedrawPolicy.Deferred,
            Undo: UndoPolicy.Record);

    internal bool Records => Undo.Key && Operations.Exists(static operation => operation.Undo.Key);
}

public sealed record EnvironmentBindings(
    Option<ResourceId> Background,
    Option<ResourceId> Reflection,
    Option<ResourceId> Lighting) : IDetachedDocumentResult;

public sealed record ContentArchive(string Xml, Seq<string> EmbeddedFiles) : IDetachedDocumentResult;

public sealed record ContentRoster(ContentKind Kind, Seq<ResourceId> Ids) : IDetachedDocumentResult;

public sealed record ContentCollectionProbe {
    private ContentCollectionProbe(Lease<RenderContentCollection> collection, Lease<RenderContentKindList> kinds) =>
        (Collection, Kinds) = (collection, kinds);

    internal Lease<RenderContentCollection> Collection { get; }
    internal Lease<RenderContentKindList> Kinds { get; }

    public Fin<Unit> Release() => Custody.Release(
        releases: Seq<Func<Fin<Unit>>>(
            () => Try.lift(() => Fin.Succ(value: Kinds.Dispose())).Run().Bind(static inner => inner),
            () => Try.lift(() => Fin.Succ(value: Collection.Dispose())).Run().Bind(static inner => inner)));

    public static Fin<ContentCollectionProbe> Of(
        Lease<RenderContentCollection> collection, Lease<RenderContentKindList> kinds) {
        return from activeCollection in Admit.Need(collection)
               from activeKinds in Admit.Need(kinds)
               select new ContentCollectionProbe(collection: activeCollection, kinds: activeKinds);
    }

    public static Fin<ContentCollectionProbe> Mint(Seq<ContentKind> kinds) {
        return from collection in Lease<RenderContentCollection>.Acquire(
                   mint: static () => new RenderContentCollection())
               from list in Lease<RenderContentKindList>.Acquire(
                       mint: static () => new RenderContentKindList())
                   .Rollback(release: () => Fin.Succ(value: collection.Dispose()))
               from probe in (from _ in Try.lift(() => Fin.Succ(value: kinds.Iter(
                                      kind => list.Resource.Add(kind: (RenderContentKind)kind.Key)))).Run().Bind(static inner => inner)
                              from admitted in Of(collection: collection, kinds: list)
                              select admitted)
                   .Rollback(
                       release: () => Custody.Release(
                           releases: Seq<Func<Fin<Unit>>>(
                               () => Fin.Succ(value: list.Dispose()),
                               () => Fin.Succ(value: collection.Dispose()))))
               select probe;
    }
}

public sealed record ContentCollectionEvidence(
    ContentUsageFilter Usage,
    Seq<ResourceId> Members,
    Option<string> SearchPattern,
    KindScope Kinds,
    CapabilitySet<CollectionTrait> Traits) : IDetachedDocumentResult;

public readonly record struct MatchEvidence(MatchVerdict Verdict) : IDetachedDocumentResult;

public readonly record struct CompatibilityEvidence(Guid RenderEngineId, bool Compatible) : IDetachedDocumentResult;

public sealed record ContentIcon(Lease<System.Drawing.Bitmap> Image) : IDetachedDocumentResult, IDisposable {
    public void Dispose() => ignore(Image.Dispose());
}

public sealed record IconRequest(Size2i Extent, IconModality Modality) {
    internal Fin<ContentIcon> Render(RenderContent content) =>
        Try.lift(() => Modality.Draw(content: content, extent: Extent.Native) switch {
            (true, System.Drawing.Bitmap drawn) =>
                Fin.Succ(value: new ContentIcon(Image: new Lease<System.Drawing.Bitmap>.Owned(Value: drawn))),
            (false, System.Drawing.Bitmap stray) => new Lease<System.Drawing.Bitmap>.Owned(Value: stray).Use(
                body: _ => Fin.Fail<ContentIcon>(new RenderFault.HostRefused(Member: nameof(IconModality), Detail: Modality.Key))),
            _ => Fin.Fail<ContentIcon>(new RenderFault.HostRefused(Member: nameof(IconModality), Detail: Modality.Key)),
        }).Run().Bind(static inner => inner);
}

public sealed class ContentQuery<T> where T : IDetachedDocumentResult {
    private readonly Func<RhinoDoc, RenderContent, Fin<T>> read;

    internal ContentQuery(Func<RhinoDoc, RenderContent, Fin<T>> read) => this.read = read;

    internal Fin<T> Run(RhinoDoc document, RenderContent content) => read(document, content);
}

public static class ContentQuery {
    public static ContentQuery<ContentSnapshot> Snapshot { get; } =
        new(read: static (_, content, op) => ContentSnapshot.Of(content: content));

    public static ContentQuery<ContentArchive> Archive { get; } =
        new(read: static (_, content) =>
            from xml in Acceptance.Text(value: content.Xml)
            from embedded in Try.lift(() => Fin.Succ(value: toSeq(content.GetEmbeddedFilesList()))).Run().Bind(static inner => inner)
            select new ContentArchive(Xml: xml, EmbeddedFiles: embedded));

    public static ContentQuery<FieldCensus> Fields { get; } =
        new(read: static (_, content, op) => FieldCensus.Of(fields: content.Fields));

    public static ContentQuery<ScentCensus> Scents(Seq<MaterialScent> wanted = default) =>
        As<RenderMaterial, ScentCensus>((material, _) =>
            Fin.Succ(value: MaterialScent.CensusOf(material: material, wanted: wanted)));

    public static ContentQuery<TextureConfig> Config { get; } =
        As<RenderTexture, TextureConfig>(static (texture, op) => TextureConfig.Of(texture: texture));

    public static ContentQuery<TextureTraits> Traits { get; } =
        As<RenderTexture, TextureTraits>(static (texture, op) => TextureTraits.Of(texture: texture));

    public static ContentQuery<HashWitness> Hash(HashProbe probe, HashScope scope) =>
        new(read: (document, content, op) =>
            from active in Admit.Need(probe)
            from row in Admit.Need(scope)
            from witness in row == HashScope.Documented
                ? Lease<RenderSettings>.Acquire(mint: () => document.RenderSettings).Bind(lease => lease.Use(
                    body: settings => SubOwners.Within(
                        settings: settings,
                        borrow: owners => active.Read(content: content, workflow: Some(owners.Workflow)))))
                : active.Read(content: content, workflow: None)
            select witness);

    public static ContentQuery<ContentValue> Param(ParamScope scope) =>
        new(read: (_, content, op) =>
            from active in Admit.Need(scope)
            from value in active.Read(content: content)
            select value);

    public static ContentQuery<SlotUsage> Usage(RenderMaterial.StandardChildSlots slot) =>
        As<RenderMaterial, SlotUsage>((material, op) =>
            from _ in guard(Enum.IsDefined(slot), (Error)new KernelFault.InvalidValue(nameof(RenderMaterial.StandardChildSlots), "a defined slot")).ToFin()
            from usage in MaterialBridge.Usage(material: material, slot: slot)
            select usage);

    public static ContentQuery<TOut> Bake<TOut>(
        RenderTexture.TextureGeneration generation, Func<Material, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        As<RenderMaterial, TOut>((material, op) =>
            from activeBorrow in Admit.Need(borrow)
            from _ in guard(Enum.IsDefined(generation), (Error)new KernelFault.InvalidValue(nameof(RenderTexture.TextureGeneration), "a defined generation"))
            from result in MaterialBridge.Bake(
                material: material, generation: generation, borrow: activeBorrow)
            select result);

    public static ContentQuery<TOut> Pbr<TOut>(
        RenderTexture.TextureGeneration generation, Func<global::Rhino.DocObjects.PhysicallyBasedMaterial, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        As<RenderMaterial, TOut>((material, op) =>
            from activeBorrow in Admit.Need(borrow)
            from _ in guard(Enum.IsDefined(generation), (Error)new KernelFault.InvalidValue(nameof(RenderTexture.TextureGeneration), "a defined generation"))
            from result in MaterialBridge.Pbr(
                material: material, generation: generation, borrow: activeBorrow)
            select result);

    public static ContentQuery<EnvironmentState> Environment(BakeScope scope) =>
        As<RenderEnvironment, EnvironmentState>((environment, op) =>
            EnvironmentState.Bake(environment: environment, scope: scope));

    public static ContentQuery<ContentIcon> Icon(IconRequest request) =>
        new(read: (_, content, op) =>
            from active in Admit.Need(request)
            from icon in active.Render(content: content)
            select icon);

    public static ContentQuery<MatchEvidence> Match(ContentRef old) =>
        new(read: (document, content, op) =>
            from reference in Admit.Need(old)
            from prior in reference.Resolve(document: document)
            from native in Try.lift(() => Fin.Succ(content.MatchData(oldContent: prior))).Run().Bind(static inner => inner)
            from verdict in MatchVerdict.Of(native, op)
            select new MatchEvidence(Verdict: verdict));

    public static ContentQuery<CompatibilityEvidence> Compatible(Guid renderEngineId) =>
        new(read: (_, content, op) =>
            from _ in guard(renderEngineId != Guid.Empty, (Error)new KernelFault.InvalidValue(nameof(renderEngineId), "a non-empty render engine identity")).ToFin()
            from compatible in Try.lift(() => Fin.Succ(value: content.IsCompatible(renderEngineId))).Run().Bind(static inner => inner)
            select new CompatibilityEvidence(RenderEngineId: renderEngineId, Compatible: compatible));

    public static ContentQuery<ContentCollectionEvidence> Collection(ContentCollectionProbe probe) =>
        new(read: (_, content, op) =>
            from active in Admit.Need(probe)
            from collectionLease in Admit.Need(active.Collection)
            from kindLease in Admit.Need(active.Kinds)
            from result in collectionLease.Use(collection => kindLease.Use(kinds =>
                from usage in Try.lift(() => ContentUsageFilter.Of(collection.GetFilterContentByUsage())).Run().Bind(static inner => inner)
                from count in Try.lift(() => Fin.Succ(collection.Count())).Run().Bind(static inner => inner)
                from members in toSeq(Enumerable.Range(0, count)).TraverseM(index => Try.lift(() =>
                    Optional(collection.ContentAt(index)).ToFin(Fail: new KernelFault.InvalidResult())
                        .Bind(row => ResourceId.Admit(value: row.Id))).Run().Bind(static inner => inner)).As()
                from kind in ContentKind.Of(content, op)
                from scope in KindScope.Of(kinds: kinds)
                from traits in Try.lift(() => Fin.Succ(value: CapabilitySet<CollectionTrait>.Of(
                    Seq((Trait: CollectionTrait.ForcedVaries, Held: collection.GetForcedVaries()),
                        (Trait: CollectionTrait.NeedsPreview, Held: collection.ContentNeedsPreviewThumbnail(c: content, includeChildren: false)),
                        (Trait: CollectionTrait.ContainsOwnKind, Held: kinds.Contains((RenderContentKind)kind.Key)))
                        .Filter(static row => row.Held)
                        .Map(static row => row.Trait)
                        .ToArray()))).Run().Bind(static inner => inner)
                from evidence in Try.lift(() => Fin.Succ(new ContentCollectionEvidence(
                    Usage: usage,
                    Members: toSeq(members),
                    SearchPattern: HostEdge.Text(collection.GetSearchPattern()),
                    Kinds: scope,
                    Traits: traits))).Run().Bind(static inner => inner)
                select evidence))
            select result);

    private static ContentQuery<TOut> As<TContent, TOut>(Func<TContent, Fin<TOut>> project)
        where TContent : RenderContent where TOut : IDetachedDocumentResult =>
        new(read: (_, content, op) => Admit.Need(content as TContent).Bind(typed => project(typed)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RegistryCommand {
    private RegistryCommand() { }
    public sealed record RegisterContent(Assembly Assembly, PluginKey PlugIn) : RegistryCommand;
    public sealed record RegisterSerializer(ContentSerializer Serializer, PluginKey PlugIn) : RegistryCommand;
    public sealed record ArmShell(RenderShellProgram Program) : RegistryCommand;
    public sealed record Change(DocumentSession Session, ContentTransaction Transaction) : RegistryCommand;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RegistryResult : IDetachedDocumentResult {
    private RegistryResult() { }
    public sealed record Registered(Seq<Type> Types) : RegistryResult;
    public sealed record SerializerRegistered : RegistryResult;
    public sealed record ShellArmed(int Rows) : RegistryResult;
    public sealed record Changed : RegistryResult;
}

public sealed record ContentTypeCensus(
    Seq<ContentUuidSeed> BuiltInUuids,
    Seq<ContentTypeInfo> RegisteredFactories) : IDetachedDocumentResult;

public sealed class RegistryQuery<T> where T : IDetachedDocumentResult {
    private readonly Func< Fin<T>> run;

    internal RegistryQuery(Func< Fin<T>> run) => this.run = run;

    internal Fin<T> Run() => run();
}

public static class RegistryQuery {
    public static RegistryQuery<ContentTypeCensus> Factories { get; } =
        new(op =>
            from builtIn in ContentUuidCatalog.Census()
            from registered in ContentTypeInfo.Census()
            select new ContentTypeCensus(BuiltInUuids: builtIn, RegisteredFactories: registered));

    public static RegistryQuery<T> Content<T>(DocumentSession session, ContentRef target, ContentQuery<T> query)
        where T : IDetachedDocumentResult =>
        new(op => Registry.Query(session, target, query));

    public static RegistryQuery<ContentRoster> Roster(DocumentSession session, ContentKind kind) =>
        new(op => Registry.Roster(session, kind));

    public static RegistryQuery<EnvironmentBindings> CurrentEnvironments(DocumentSession session) =>
        new(op => Registry.CurrentEnvironments(session));

    public static RegistryQuery<EditorFacts> Editor(EditorBridge bridge) =>
        new(op => Registry.Editor(bridge));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Registry {
    public static Fin<RegistryResult> Run(RegistryCommand command) {
        return from active in Admit.Need(command)
               from result in active.Switch(
                   context: op,
                   registerContent: static (state, request) => Register(request.Assembly, request.PlugIn, state)
                       .Map(static types => (RegistryResult)new RegistryResult.Registered(types)),
                   registerSerializer: static (state, request) =>
                       from serializer in Admit.Need(request.Serializer)
                       from registered in serializer.Register(request.PlugIn)
                       select (RegistryResult)new RegistryResult.SerializerRegistered(),
                   armShell: static (state, request) =>
                       from program in Admit.Need(request.Program)
                       from _ in RenderShell.Arm(program: program, op: state)
                       select (RegistryResult)new RegistryResult.ShellArmed(Rows: program.Rows.Count),
                   change: static (state, request) => Commit(request.Session, request.Transaction, state)
                       .Map(static _ => (RegistryResult)new RegistryResult.Changed()))
               select result;
    }

    public static Fin<T> Read<T>(RegistryQuery<T> query) where T : IDetachedDocumentResult {
        return from active in Admit.Need(query)
               from result in active.Run()
               select result;
    }

    internal static Fin<EditorFacts> Editor(EditorBridge bridge) =>
        from active in Admit.Need(bridge)
        from facts in active.Use<RhinoSettings, EditorFacts>(
            provider: EditorProvider.Settings,
            intent: EditorIntent.Read,
            borrow: settings => Facts(settings: settings))
        select facts;

    private static Fin<EditorFacts> Facts(RhinoSettings settings) =>
        from renderer in Try.lift(() => Fin.Succ(value: settings.GetCurrentRenderer())).Run().Bind(static inner => inner)
        from viewport in Try.lift(() => Fin.Succ(value: Optional(settings.RenderingView()).Map(static view => view.Viewport.Id))).Run().Bind(static inner => inner)
        from sizes in Try.lift(() => Fin.Succ(value: toSeq(settings.GetCustomRenderSizes()))).Run().Bind(static inner => inner)
        from admitted in sizes.TraverseM(size => Size2i.Of(width: size.Width, height: size.Height)).As()
        select new EditorFacts(
            CurrentRenderer: renderer,
            RenderingViewport: viewport,
            CustomSizes: admitted.Strict(),
            CustomSizeIsPreset: settings.CustomImageSizeIsPreset);

    private static Fin<Seq<Type>> Register(Assembly assembly, PluginKey plugin) =>
        from active in Admit.Need(assembly)
        from _ in plugin.Admit()
        from registered in Try.lift(() =>
            Optional(RenderContent.RegisterContent(assembly: active, pluginId: plugin.ToValue()))
                .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(RenderContent.RegisterContent), Detail: active.FullName ?? string.Empty))
                .Map(static types => toSeq(types))).Run().Bind(static inner => inner)
        select registered;

    private static Fin<Unit> Commit(DocumentSession session, ContentTransaction plan) =>
        from activeSession in Admit.Need(session)
        from active in Admit.Need(plan)
        from kind in Admit.Need(active.Kind)
        from reason in Admit.Need(active.Reason)
        from redraw in Admit.Need(active.Redraw)
        from undo in Admit.Need(active.Undo)
        from name in Acceptance.Text(value: active.Name)
        from _ in guard(
            !active.Operations.IsEmpty && active.Operations.ForAll(static operation => operation is not null),
            (Error)new KernelFault.InvalidValue(nameof(ContentTransaction), "a non-empty operation set"))
        let admitted = active with { Kind = kind, Reason = reason, Redraw = redraw, Undo = undo, Name = name }
        from changed in activeSession.Demand(
            use: document => Change(document: document, plan: admitted),
            needs: SessionNeed.Mutation(undo: admitted.Records, redraw: admitted.Redraw).ToArray())
        select changed;

    private static Fin<Unit> Change(RhinoDoc document, ContentTransaction plan) =>
        DocumentCommit.Sealed(
            document: document,
            name: plan.Name,
            recordsUndo: plan.Records,
            redraw: plan.Redraw,
            run: () => plan.Kind.Table(
                document: document,
                reason: plan.Reason,
                body: scoped => plan.Operations.TraverseM(operation => operation.Apply(
                        document: scoped, scope: plan.Kind, reason: plan.Reason)).As()
                    .Map(static _ => unit)),
            project: Fin.Succ);

    internal static Fin<T> Query<T>(DocumentSession session, ContentRef target, ContentQuery<T> query)
        where T : IDetachedDocumentResult =>
        from activeSession in Admit.Need(session)
        from activeTarget in Admit.Need(target)
        from active in Admit.Need(query)
        from result in activeSession.Demand(
            use: document =>
                from content in activeTarget.Resolve(document: document)
                from answer in active.Run(document: document, content: content)
                select answer,
            needs: [SessionNeed.Read])
        select result;

    internal static Fin<ContentRoster> Roster(DocumentSession session, ContentKind kind) =>
        from activeSession in Admit.Need(session)
        from activeKind in Admit.Need(kind)
        from result in activeSession.Demand(
            use: document => activeKind.Roster(document)
                .Traverse(content => ResourceId.Admit(value: content.Id).ToValidation())
                .As()
                .ToFin()
                .Map(ids => new ContentRoster(Kind: activeKind, Ids: ids)),
            needs: [SessionNeed.Read])
        select result;

    internal static Fin<EnvironmentBindings> CurrentEnvironments(DocumentSession session) =>
        from activeSession in Admit.Need(session)
        from result in activeSession.Demand(
            use: document => Try.lift(() => {
                ICurrentEnvironment current = document.CurrentEnvironment;
                return Fin.Succ(value: new EnvironmentBindings(
                    Background: Optional(current.ForBackground).Bind(static content => ResourceId.Maybe(content.Id)),
                    Reflection: Optional(current.ForReflectionAndRefraction).Bind(static content => ResourceId.Maybe(content.Id)),
                    Lighting: Optional(current.ForLighting).Bind(static content => ResourceId.Maybe(content.Id))));
            }).Run().Bind(static inner => inner),
            needs: [SessionNeed.Read])
        select result;
}
```

## [05]-[EVENTS]

- Owner: `ContentPulse` carries each catalogued static event as one bind row beside its `ScopeAffinity` column; `ContentSignal` closes detached payloads; `ContentObservation` is the ask a binder hands in; `ContentStream` owns transactional attach, document gating, symmetric release, and a bounded `Ring<ContentStreamFailure>`.
- Entry: `ContentStream.Of(observation)` is the ONE mint — the observation IS the parameter set, so the six-argument entry that re-spelled its columns is gone and the hook binding forwards the ask untouched.
- Law: every reference-like host member projects inside the callback — content becomes its `ResourceId`, the document becomes `DocKey`, the preview bitmap clones into an owned lease; a live `RenderContent` never rides a fact.
- Law: the stream and the table family split by granularity — the Document events page's `RenderContent` payload reports table transitions and material assignment; this stream reports per-content lifecycle, change context, and field mutation the table family cannot; a consumer needing both composes two watches.
- Law: reason filtering occurs at the bind — `PulseFilter` drops changed and field facts whose reason the filter names; filtering never claims debounce or coalescing semantics the host event stream does not provide.
- Law: a pulse row carries its `ScopeAffinity` and admission refuses the pairing its rows cannot honour — `PreviewReady` alone is `AnyDocumentOnly`, because `PreviewRenderedEventArgs` publishes no document to gate on, so a `Document`-scoped stream naming it fails admission rather than seating a subscription that can never deliver.
- Law: callback delivery transfers the original fact to the sink and prepares a detached ledger copy first. Success releases the spare copy; failure parks it with the fault and releases the delivered original before the host delegate returns.
- Law: the failure journal IS the kernel bounded ring; a versioned park lands before its winner releases evicted facts, and `Landed.Cleanup` keeps that release fault distinct from contention so the parked item is never retried or released twice.
- Law: the stream composes the Document spine's `LifecycleGate` — the package's ONE claims, bounded-settle, and one-owner close capsule — and owns only its subscription cell and bounded ring; a hand-rolled `lock`/`Monitor` lifecycle machine beside it is the collapsed form. `Within` admits or refuses each delivery, `Close` drains every admitted claim inside the gate's bounded settle before the stop and settle callbacks release the subscription and the parked facts, and a close issued from a thread already inside a delivery claim refuses typed rather than waiting on its own release.
- Law: releasing a parked fact is IDEMPOTENT, because a refused settle leaves the gate reopenable and a later close re-drains the same ring — the underlying bitmap disposal is idempotent and the lease forwards exactly one call per release, so the second drain retires nothing twice.
- Law: `ContentStream.Close` is the exit that answers; `Dispose` forwards to it and drops the answer, so a caller needing the close verdict names `Close`.
- Law: `ContentHooks.Mount` registers the `rasm.rhino.render.content` point through the TYPED kernel binding — ask `ContentObservation`, grant `ContentStream` — so the registry compares the declared type pair rather than casting an erased `object`, and the point stays observe-only per the registry census.
- Growth: a new host content event is one `ContentPulse` row with its bind column; a new evidence axis is one `ContentSignal` case.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.ContentAdded`/`ContentRenamed`/`ContentDeleting`/`ContentDeleted`/`ContentReplacing`/`ContentReplaced`/`ContentUpdatePreview`/`CurrentEnvironmentChanged`/`ContentChanged`/`ContentFieldChanged`/`PreviewRendered`, `RenderContentChangeReason`, `PreviewRenderedEventArgs`, `Utilities.PreviewQuality`); kernel `Domain/hooks` (`Ring<T>`, `HookBinding`), `Domain/results` (`Cell.Seat`/`Take`, `Transition`, `Lease<T>`); `Document/events.md` (`RhinoPoint`, `MountRegistry`, `PluginKey`, `EventScope`), `Document/lifetime.md` (`LifecycleGate`, `Subscription`), kernel `Domain/results` (`Custody`), `Document/session.md` (`DocKey`), `Document/tables.md` (`ResourceId`); `Render/content.md` (`ChangeReason`), `Render/settings.md` (`EnvironmentRole`); `Numerics/atoms` (`Dimension`); LanguageExt.Core; Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LifecycleReason {
    public static readonly LifecycleReason None = new(key: (int)RenderContentChangeReason.None);
    public static readonly LifecycleReason Attach = new(key: (int)RenderContentChangeReason.Attach);
    public static readonly LifecycleReason Detach = new(key: (int)RenderContentChangeReason.Detach);
    public static readonly LifecycleReason ChangeAttach = new(key: (int)RenderContentChangeReason.ChangeAttach);
    public static readonly LifecycleReason ChangeDetach = new(key: (int)RenderContentChangeReason.ChangeDetach);
    public static readonly LifecycleReason AttachUndo = new(key: (int)RenderContentChangeReason.AttachUndo);
    public static readonly LifecycleReason DetachUndo = new(key: (int)RenderContentChangeReason.DetachUndo);
    public static readonly LifecycleReason Open = new(key: (int)RenderContentChangeReason.Open);
    public static readonly LifecycleReason Delete = new(key: (int)RenderContentChangeReason.Delete);

    internal RenderContentChangeReason Native => (RenderContentChangeReason)Key;

    internal static Fin<LifecycleReason> Of(RenderContentChangeReason native) =>
        FactoryBridge.Row<RenderContentChangeReason, LifecycleReason>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class PreviewQuality {
    public static readonly PreviewQuality None = new(key: (int)global::Rhino.Render.Utilities.PreviewQuality.None);
    public static readonly PreviewQuality Low = new(key: (int)global::Rhino.Render.Utilities.PreviewQuality.Low);
    public static readonly PreviewQuality Medium = new(key: (int)global::Rhino.Render.Utilities.PreviewQuality.Medium);
    public static readonly PreviewQuality Progressive = new(
        key: (int)global::Rhino.Render.Utilities.PreviewQuality.IntermediateProgressive);
    public static readonly PreviewQuality Full = new(key: (int)global::Rhino.Render.Utilities.PreviewQuality.Full);

    internal global::Rhino.Render.Utilities.PreviewQuality Native => (global::Rhino.Render.Utilities.PreviewQuality)Key;

    internal static Fin<PreviewQuality> Of(global::Rhino.Render.Utilities.PreviewQuality native) =>
        FactoryBridge.Row<global::Rhino.Render.Utilities.PreviewQuality, PreviewQuality>(native, static value => (int)value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentSignal : IDisposable {
    private ContentSignal() { }
    public sealed record Lifecycle(ResourceId Content, LifecycleReason Reason) : ContentSignal;
    public sealed record Changed(ResourceId Content, ChangeReason Reason, Option<ResourceId> Old) : ContentSignal;
    public sealed record FieldChanged(ResourceId Content, string Field, ChangeReason Reason) : ContentSignal;
    public sealed record EnvironmentFlip(EnvironmentRole Usage) : ContentSignal;
    public sealed record PreviewReady(
        Lease<System.Drawing.Bitmap> Image,
        Option<Size2i> Signature,
        PreviewQuality Quality) : ContentSignal;

    public void Dispose() => ignore(Switch(
        lifecycle: static _ => unit,
        changed: static _ => unit,
        fieldChanged: static _ => unit,
        environmentFlip: static _ => unit,
        previewReady: static signal => Optional(signal.Image).Map(static image => image.Dispose()).IfNone(unit)));

    internal Fin<ContentSignal> Detached() => Switch(
        lifecycle: static signal => Fin.Succ<ContentSignal>(signal),
        changed: static signal => Fin.Succ<ContentSignal>(signal),
        fieldChanged: static signal => Fin.Succ<ContentSignal>(signal),
        environmentFlip: static signal => Fin.Succ<ContentSignal>(signal),
        previewReady: static (signal) =>
            from image in Admit.Need(signal.Image)
            from clone in Lease<System.Drawing.Bitmap>.Acquire(
                mint: () => (System.Drawing.Bitmap)image.Resource.Clone())
            select (ContentSignal)new PreviewReady(
                Image: clone, Signature: signal.Signature, Quality: signal.Quality));
}

public readonly record struct ContentFact(ContentPulse Pulse, Option<DocKey> Key, ContentSignal Signal)
    : IDisposable, IDetachedDocumentResult {
    public void Dispose() => Optional(Signal).Iter(static signal => signal.Dispose());

    internal Fin<ContentFact> Detached() =>
        Optional(Signal).ToFin(Fail: new KernelFault.InvalidResult())
            .Bind(signal => signal.Detached())
            .Map(signal => this with { Signal = signal });
}

public sealed record PulseFilter(Seq<ChangeReason> DroppedReasons) {
    public static readonly PulseFilter None = new(DroppedReasons: Seq<ChangeReason>());
    public static readonly PulseFilter WithoutRealTimeUi = new(DroppedReasons: Seq(ChangeReason.RealTimeUi));

    internal bool Admits(Option<ChangeReason> reason) =>
        reason.Map(row => !DroppedReasons.Contains(row)).IfNone(true);
}

[SmartEnum<string>]
public sealed partial class ScopeAffinity {
    public static readonly ScopeAffinity EitherScope = new("either-scope", static _ => true);
    public static readonly ScopeAffinity AnyDocumentOnly = new(
        "any-document-only", static scope => scope is EventScope.AnyDocument);

    [UseDelegateFromConstructor]
    internal partial bool Admits(EventScope scope);
}

[SmartEnum<int>]
public sealed partial class ContentPulse {
    public static readonly ContentPulse Added = new(key: 0, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentAdded += h, unsubscribe: static h => RenderContent.ContentAdded -= h));
    public static readonly ContentPulse Renamed = new(key: 1, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentRenamed += h, unsubscribe: static h => RenderContent.ContentRenamed -= h));
    public static readonly ContentPulse Deleting = new(key: 2, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleting += h, unsubscribe: static h => RenderContent.ContentDeleting -= h));
    public static readonly ContentPulse Deleted = new(key: 3, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleted += h, unsubscribe: static h => RenderContent.ContentDeleted -= h));
    public static readonly ContentPulse Replacing = new(key: 4, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentReplacing += h, unsubscribe: static h => RenderContent.ContentReplacing -= h));
    public static readonly ContentPulse Replaced = new(key: 5, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentReplaced += h, unsubscribe: static h => RenderContent.ContentReplaced -= h));
    public static readonly ContentPulse UpdatePreview = new(key: 6, affinity: ScopeAffinity.EitherScope, bind: Plain(
        subscribe: static h => RenderContent.ContentUpdatePreview += h, unsubscribe: static h => RenderContent.ContentUpdatePreview -= h));
    public static readonly ContentPulse EnvironmentFlip = new(key: 7, affinity: ScopeAffinity.EitherScope, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentEventArgs>>(
            subscribe: static h => RenderContent.CurrentEnvironmentChanged += h,
            unsubscribe: static h => RenderContent.CurrentEnvironmentChanged -= h,
            handler: (_, args) => ignore(
                EnvironmentRole.Of(args.EnvironmentUsageEx).ToOption()
                    .Bind(role => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.EnvironmentFlip(Usage: role)))
                    .TraverseM(deliver).As().Map(static _ => unit))));
    public static readonly ContentPulse Changed = new(key: 8, affinity: ScopeAffinity.EitherScope, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentChanged += h,
            unsubscribe: static h => RenderContent.ContentChanged -= h,
            handler: (_, args) => ignore(
                (from reason in ChangeReason.Of(native: args.ChangeContext).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                 from content in ResourceId.Maybe(args.Content.Id)
                 from fact in Gate(pulse: pulse, scope: scope, document: args.Document,
                     signal: new ContentSignal.Changed(
                         Content: content, Reason: reason,
                         Old: Optional(args.OldContent).Bind(static old => ResourceId.Maybe(old.Id))))
                 select fact)
                .TraverseM(deliver).As().Map(static _ => unit))));
    public static readonly ContentPulse FieldChanged = new(key: 9, affinity: ScopeAffinity.EitherScope, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentFieldChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentFieldChanged += h,
            unsubscribe: static h => RenderContent.ContentFieldChanged -= h,
            handler: (_, args) => ignore(
                (from reason in ChangeReason.Of(native: args.ChangeContext).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                 from content in ResourceId.Maybe(args.Content.Id)
                 from fact in Gate(pulse: pulse, scope: scope, document: args.Document,
                     signal: new ContentSignal.FieldChanged(Content: content, Field: args.FieldName, Reason: reason))
                 select fact)
                .TraverseM(deliver).As().Map(static _ => unit))));
    public static readonly ContentPulse PreviewReady = new(key: 10, affinity: ScopeAffinity.AnyDocumentOnly, bind: (pulse, _, _, deliver) =>
        Subscription.Attach<EventHandler<PreviewRenderedEventArgs>>(
            subscribe: static h => RenderContent.PreviewRendered += h,
            unsubscribe: static h => RenderContent.PreviewRendered -= h,
            handler: (_, args) => ignore(
                (from image in Optional(args.Bitmap)
                 from quality in PreviewQuality.Of(args.Quality).ToOption()
                 select new ContentFact(
                      Pulse: pulse,
                      Key: Option<DocKey>.None,
                      Signal: new ContentSignal.PreviewReady(
                          Image: new Lease<System.Drawing.Bitmap>.Owned(Value: (System.Drawing.Bitmap)image.Clone()),
                          Signature: Optional(args.PreviewJobSignature)
                              .Bind(signature => Size2i.Of(
                                  width: signature.ImageWidth(), height: signature.ImageHeight()).ToOption()),
                          Quality: quality)))
                .TraverseM(deliver).As().Map(static _ => unit))));

    internal ScopeAffinity Affinity { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(
        ContentPulse pulse, EventScope scope, PulseFilter filter, Func<ContentFact, Fin<Unit>> deliver);

    private static Func<ContentPulse, EventScope, PulseFilter, Func<ContentFact, Fin<Unit>>, Fin<Subscription>> Plain(
        Action<EventHandler<RenderContentEventArgs>> subscribe,
        Action<EventHandler<RenderContentEventArgs>> unsubscribe) =>
        (pulse, scope, _, deliver) => Subscription.Attach(
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            handler: (EventHandler<RenderContentEventArgs>)((_, args) => ignore(
                (from reason in LifecycleReason.Of(args.Reason).ToOption()
                 from content in ResourceId.Maybe(args.Content.Id)
                 from fact in Gate(pulse: pulse, scope: scope, document: args.Document,
                     signal: new ContentSignal.Lifecycle(Content: content, Reason: reason))
                 select fact)
                .TraverseM(deliver).As().Map(static _ => unit))));

    private static Option<ContentFact> Gate(ContentPulse pulse, EventScope scope, RhinoDoc? document, ContentSignal signal) =>
        Optional(document)
            .Bind(static active => DocKey.Of(document: active).ToOption())
            .Match(
                Some: key => scope.Switch(
                    (Pulse: pulse, Signal: signal),
                    document: static (state, watched) => watched.Key == state.Key
                        ? Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))
                        : Option<ContentFact>.None,
                    anyDocument: static (state, _) => Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))),
                None: () => scope is EventScope.AnyDocument
                    ? Some(new ContentFact(Pulse: pulse, Key: Option<DocKey>.None, Signal: signal))
                    : Option<ContentFact>.None);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentStreamFailure(ContentFact Fact, Error Fault) : IDisposable, IDetachedDocumentResult {
    public void Dispose() => Fact.Dispose();
}

public sealed record ContentObservation {
    private ContentObservation(
        EventScope scope, Seq<ContentPulse> pulses, PulseFilter filter, Rasm.Numerics.Dimension journalCap,
        TimeSpan settleWithin, Func<ContentFact, Fin<Unit>> sink) =>
        (Scope, Pulses, Filter, JournalCap, SettleWithin, Sink) =
            (scope, pulses, filter, journalCap, settleWithin, sink);

    internal EventScope Scope { get; }
    internal Seq<ContentPulse> Pulses { get; }
    internal PulseFilter Filter { get; }
    internal Rasm.Numerics.Dimension JournalCap { get; }
    internal TimeSpan SettleWithin { get; }
    internal Func<ContentFact, Fin<Unit>> Sink { get; }

    public static Fin<ContentObservation> Of(
        EventScope scope,
        Seq<ContentPulse> pulses,
        PulseFilter filter,
        Rasm.Numerics.Dimension journalCap,
        TimeSpan settleWithin,
        Func<ContentFact, Fin<Unit>> sink) {
        return from activeScope in Admit.Need(scope)
               from activeFilter in Admit.Need(filter)
               from activeSink in Admit.Need(sink)
               from _ in guard(
                   !pulses.IsEmpty && pulses.ForAll(static pulse => pulse is not null),
                   (Error)new KernelFault.InvalidValue(nameof(ContentObservation), "a non-empty pulse set"))
               from __ in guard(
                   pulses.ForAll(pulse => pulse.Affinity.Admits(scope: activeScope)),
                   (Error)new KernelFault.InvalidValue(nameof(ScopeAffinity), "pulses this scope can deliver"))
               select new ContentObservation(
                   scope: activeScope, pulses: pulses.Distinct().Strict(), filter: activeFilter,
                   journalCap: journalCap, settleWithin: settleWithin, sink: activeSink);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class ContentHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin) =>
        MountRegistry.Mount(
            binding: new HookBinding<RhinoPoint, PluginKey, ContentObservation, ContentStream>(
                Point: RhinoPoint.RenderContent,
                Owner: plugin,
                Bind: static ask => ContentStream.Of(observation: ask)));
}

public sealed class ContentStream : IDisposable {
    private readonly ContentStreamState state;

    private ContentStream(ContentStreamState state) => this.state = state;

    public Seq<ContentStreamFailure> Parked => state.Parked;
    public long Shed => state.Shed;
    public long Lost => state.Lost;

    public Fin<Unit> Close() => state.Close();

    public void Dispose() => ignore(Close());

    public static Fin<ContentStream> Of(ContentObservation observation) {
        return from ask in Admit.Need(observation)
               from lifecycle in LifecycleGate.Of(settleWithin: ask.SettleWithin)
               let state = new ContentStreamState(gate: lifecycle, cap: ask.JournalCap)
               from attached in Subscription.AttachAll(ask.Pulses.Map(pulse =>
                   (Func<Fin<Subscription>>)(() => pulse.Bind(
                       pulse: pulse,
                       scope: ask.Scope,
                       filter: ask.Filter,
                       deliver: fact => state.Deliver(fact, ask.Sink)))))
               from _ in state.Attach(attached: attached)
               select new ContentStream(state: state);
    }

    private sealed class ContentStreamState {
        private readonly LifecycleGate gate;
        private readonly Atom<Option<Subscription>> subscription = Atom(Option<Subscription>.None);
        private readonly Ring<ContentStreamFailure> failures;

        internal ContentStreamState(LifecycleGate gate, Rasm.Numerics.Dimension cap) {
            this.gate = gate;
            failures = new Ring<ContentStreamFailure>(cap: cap);
        }

        internal Seq<ContentStreamFailure> Parked => failures.Parked;

        internal long Shed => failures.Shed;

        internal long Lost => failures.Lost;

        internal Fin<Unit> Attach(Subscription attached) =>
            gate.Within(
                body: () => Cell.Seat(cell: subscription, mint: () => attached).Switch(
                    state: op,
                    committed: static (_, _) => Fin.Succ(value: unit),
                    ceded: static (key, _) => Fin.Fail<Unit>(error: new RenderFault.SeatTaken(Engine: Guid.Empty)),
                    refused: static (_, row) => Fin.Fail<Unit>(error: row.Cause),
                    contended: static (key, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult())),
                refused: () => Fin.Fail<Unit>(error: new KernelFault.InvalidContext())
                    .Rollback(release: () => Release(fact: attached)));

        internal Fin<Unit> Deliver(ContentFact fact, Func<ContentFact, Fin<Unit>> sink) =>
            gate.Within(
                body: () => Delivered(fact: fact, sink: sink),
                refused: () => Fin.Fail<Unit>(error: new KernelFault.InvalidContext())
                    .Rollback(release: () => Release(fact: fact)));

        internal Fin<Unit> Close() =>
            gate.Close(
                stop: () => Try.lift(() => Fin.Succ(
                    value: Cell.Take(cell: subscription).Current.Iter(static held => held.Dispose()))).Run().Bind(static inner => inner),
                settle: () => Custody.Release(
                    held: failures.Parked,
                    release: static failure => Fin.Succ(value: HostEdge.Side(failure.Dispose))));

        private Fin<Unit> Delivered(ContentFact fact, Func<ContentFact, Fin<Unit>> sink) =>
            fact.Detached().Match(
                Succ: detached => Try.lift(() => sink(fact)).Run().Bind(static inner => inner).Match(
                    Succ: value => Release(fact: detached).Map(_ => value),
                    Fail: fault => Park(original: fact, detached: detached, fault: fault)),
                Fail: fault => Fin.Fail<Unit>(error: fault).Rollback(
                    release: () => Release(fact: fact)));

        private Fin<Unit> Park(ContentFact original, ContentFact detached, Error fault) =>
            failures.Park(
                item: new ContentStreamFailure(Fact: detached, Fault: fault),
                release: dropped => Release(fact: dropped.Fact)).Switch(
                state: (Original: original, Detached: detached, Fault: fault),
                landed: static (ctx, row) => Fin.Fail<Unit>(error: ctx.Fault)
                    .Rollback(
                        release: () => row.Cleanup.Settled(
                            release: () => Release(fact: ctx.Original))),
                ceded: static (ctx, _) => Fin.Fail<Unit>(error: ctx.Fault + new KernelFault.InvalidResult())
                    .Rollback(held: Seq(ctx.Original, ctx.Detached), release: row => Release(fact: row)),
                refused: static (ctx, row) => Fin.Fail<Unit>(error: ctx.Fault + row.Cause)
                    .Rollback(held: Seq(ctx.Original, ctx.Detached), release: r => Release(fact: r)));

        private static Fin<Unit> Release(ContentFact fact) =>
            Try.lift(() => Fin.Succ(value: HostEdge.Side(fact.Dispose))).Run().Bind(static inner => inner);

        private static Fin<Unit> Release(Subscription held) =>
            Try.lift(() => Fin.Succ(value: HostEdge.Side(held.Dispose))).Run().Bind(static inner => inner);
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]       | [OWNER]                            | [FORM]                            | [ENTRY]                 |
| :-----: | :-------------- | :--------------------------------- | :-------------------------------- | :---------------------- |
|  [01]   | UUID seeds      | `ContentUuidCatalog`               | generated kind and role data      | `Census` / `Find`       |
|  [02]   | factory census  | `ContentTypeCensus`                | UUIDs plus registered factories   | `Registry.Read`         |
|  [03]   | custom format   | `ContentSerializer`                | transfer, reports, bounded ring   | `Registry.Run`          |
|  [04]   | mutation        | `ContentOp` / `ContentTransaction` | admission or target mutation      | `Registry.Run`          |
|  [05]   | undo posture    | `UndoPolicy`                       | a column per mutation case        | `ContentTransaction`    |
|  [06]   | typed reads     | `RegistryQuery<T>`                 | result-correlated programs        | `Registry.Read<T>`      |
|  [07]   | collection read | `ContentCollectionEvidence`        | leased set, kind scope, traits    | `Collection`            |
|  [08]   | icon routes     | `IconModality` / `IconRequest`     | six host routes, one custody hop  | `ContentQuery.Icon`     |
|  [09]   | content events  | `ContentPulse`                     | verified event rows               | `ContentStream.Of`      |
|  [10]   | event evidence  | `ContentSignal`                    | detached payload family           | `ContentStream.Of`      |
|  [11]   | observation ask | `ContentObservation`               | the one admitted parameter set    | `ContentObservation.Of` |
|  [12]   | hook point      | `ContentHooks`                     | `rasm.rhino.render.content` mount | `ContentHooks.Mount`    |
|  [13]   | shell rows      | `RenderShellProgram` / `ShellRow`  | keyed panel and side-pane rows    | `Registry.Run`          |
|  [14]   | shell gate      | `ShellGate` / `RenderShell`        | one-shot host-callback seating    | `RenderShell.Drain`     |
|  [15]   | shell resolve   | `ShellSeat<TBody>`                 | seated body plus side-pane id     | `RenderShell.Resolve`   |
|  [16]   | editor payloads | `EditorBridge` / `EditorProvider`  | provider-keyed borrow and commit  | `EditorBridge.Of`       |
|  [17]   | editor facts    | `EditorFacts`                      | renderer, view, and size facts    | `Registry.Read`         |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
