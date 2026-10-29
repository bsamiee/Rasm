using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using UUIDNext;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayModeDefinition(string Name, Guid Base) {
    public Seq<Action<DisplayModeDescription>> Edits { get; init; }
    public Seq<ModeKey> Keys { get; init; }
}

public sealed record DisplayModeSpec(IO<Seq<DisplayModeDefinition>> Modes) {
    public HashMap<string, DisplayModeDefinition> Successors { get; init; }
    public Seq<HostEvent<Unit>> Refreshed { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayModes {
    // --- [IDENTITY]
    public static Guid Id(Guid owner, DisplayModeDefinition mode) => Uuid.NewNameBased(owner, mode.Name);

    // --- [ACQUISITION]
    public static IO<Unit> Declared(DisplayModeSpec spec) =>
        from package in IO.lift(static () => FolderPath.Installed(Session.Assembly))
        let owner = Session.PlugIn.Id
        let reconciled = Reconcile(owner, package, spec).Map(unit)
        from loaded in reconciled
        from opened in when(!spec.Successors.IsEmpty, Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.EndOpenDocument += h).Documents()
            .Attach(args => IO.lift(() => Conversions.Required(args.Document)).Bind(doc => Reassigned(doc, Successors(owner, spec))))).As()
        from refreshed in spec.Refreshed.TraverseM(signal => signal.Attach(_ => reconciled)).As()
        select unit;

    private static HashMap<Guid, Guid> Successors(Guid owner, DisplayModeSpec spec) =>
        toHashMap(spec.Successors.AsIterable().Map(pair => (Uuid.NewNameBased(owner, pair.Key), Id(owner, pair.Value))));

    // --- [RECONCILE]
    private const string Order = "Order";
    private const string DerivedFrom = "DerivedFrom";

    public static IO<Seq<Guid>> Reconcile(Guid owner, FolderPath package, DisplayModeSpec spec) =>
        from declared in spec.Modes
        let rows = declared.Map(mode => (Id: Id(owner, mode), Mode: mode)).Strict()
        from valid in IO.lift(() => Checked(owner, rows, spec))
        from present in Products.Many(static () => DisplayModeDescription.GetDisplayModes()).Map(static modes => modes.Map(static mode => (mode.Id, mode.EnglishName)).Strict()).Bracket()
        let ids = toHashSet(rows.Map(static row => row.Id))
        let retired = present.Filter(mode => mode.Id == Uuid.NewNameBased(owner, mode.EnglishName) && !ids.Contains(mode.Id))
            .Map(mode => (mode.Id, Successor: spec.Successors.Find(mode.EnglishName).Map(successor => Id(owner, successor)))).Strict()
        from bases in Products.Each(rows.Map(static row => row.Mode.Base).Distinct().Map(static id => Keys(id).Map(table => (Id: id, Table: table))))
        let tables = toHashMap(bases)
        from intended in Products.Each(rows.Map(row => Intended(row.Mode, tables[row.Mode.Base], package).Map(planned => (row.Id, row.Mode, planned.Table, planned.Owned))))
        from stale in Products.Each(intended.Map(row => Stale(owner, row.Id, row.Owned, row.Mode.Keys, present.Exists(mode => mode.Id == row.Id), package)
            .Map(changed => (row.Id, row.Table, row.Owned, Changed: changed))))
        let imports = stale.Filter(static row => row.Changed == Some(value: true))
        from imported in Products.Each(imports.Map(static row => Imported(row.Id, row.Table)))
            .Finally(IO.lift(DisplayModeDescription.SaveDisplayModes))
        from recorded in Products.Each(stale.Filter(static row => row.Changed.IsSome).Map(row => Record(owner, row.Id, toSeq(row.Owned.Keys))))
        from moved in Retired(owner, rows.Map(static row => row.Id), retired)
        from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
        from raised in IO.lift(() => Raise(imports.Map(static row => row.Id) + moved))
        select imports.Map(static row => row.Id);

    public static IO<Unit> Update(Seq<Guid> ids, Seq<Action<DisplayModeDescription>> edits, Seq<ModeKey> keys) =>
        from formatted in IO.lift(() => Ini.Merged(keys).Traverse(static key => key.Value.Formatted(None).Map(text => ((key.Section, key.Key), text))).As())
        let owner = Session.PlugIn.Id
        from patched in Products.Each(ids.Map(id =>
            from table in Keys(id)
            from touched in Touched(id, edits)
            from recorded in Recorded(owner, id).Read
            let owned = touched.AddOrUpdateRange(toHashMap(formatted).AsIterable())
            from written in when(!owned.IsEmpty, Imported(id, table.AddOrUpdateRange(owned.AsIterable())).Map(unit)).As()
            from marked in Record(owner, id, toSeq(Ini.Table(recorded).Keys) + toSeq(owned.Keys))
            select unit))
        from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
        from raised in IO.lift(() => Raise(ids))
        select unit;

    private static Fin<Unit> Checked(Guid owner, Seq<(Guid Id, DisplayModeDefinition Mode)> rows, DisplayModeSpec spec) =>
        toHashSet(rows.Map(static row => row.Id)) switch {
            var ids => (
                    Conversions.Unique(rows.Map(static row => row.Mode.Name) + toSeq(spec.Successors.Keys), identity, StringComparer.OrdinalIgnoreCase),
                    Products.Each(rows, (row, index) => RhinoFault.Unless(!ids.Contains(row.Mode.Base), RhinoReason.Invalid, row.Mode.Base, index)),
                    Products.Each(toSeq(spec.Successors.Values), (successor, index) => RhinoFault.Unless(ids.Contains(Id(owner, successor)), RhinoReason.Missing, successor.Name, index)))
                .Apply(static (_, _, _) => unit)
                .As(),
        };

    private static IO<(HashMap<(string Section, string Key), string> Table, HashMap<(string Section, string Key), string> Owned)> Intended(
        DisplayModeDefinition mode, HashMap<(string Section, string Key), string> source, FolderPath package) =>
        from formatted in IO.lift(() => Ini.Merged(Root(mode, source) + mode.Keys).Traverse(key => key.Value.Formatted(Some(package)).Map(text => ((key.Section, key.Key), text))).As())
        from touched in Touched(mode.Base, mode.Edits)
        let owned = touched.AddOrUpdateRange(toHashMap(formatted).AsIterable())
        select (source.AddOrUpdateRange(owned.AsIterable()), owned.Filter(static (key, _) => Volatile(key).IsNone));

    private static Seq<ModeKey> Root(DisplayModeDefinition mode, HashMap<(string Section, string Key), string> source) =>
        Seq(new ModeKey("", "Name", new ModeValue.Text(mode.Name)), new ModeKey("", Order, new ModeValue.Whole(0)), new ModeKey("", "AddToMenu", new ModeValue.Flag(Value: true)))
        + (source.Find(("", Order)).Bind(Ini.Integral).Exists(static order => order < 0)
            ? Seq(new ModeKey("", DerivedFrom, new ModeValue.Id(mode.Base)))
            : Seq<ModeKey>());

    private static IO<HashMap<(string Section, string Key), string>> Touched(Guid source, Seq<Action<DisplayModeDescription>> edits) =>
        edits.IsEmpty
            ? IO.pure(HashMap<(string Section, string Key), string>())
            : IO.lift(() => DisplayModeDescription.CopyDisplayMode(source, $"{nameof(DisplayModes)} {Guid.NewGuid():N}")).Bind(scratch =>
                (from copy in IO.lift(() => Conversions.Required(Conversions.Present(scratch)))
                 from before in Keys(copy)
                 from updated in With(copy, staged =>
                     from edited in IO.lift(() => edits.Iter(edit => edit(staged)))
                     from written in Products.Accepted(() => DisplayModeDescription.UpdateDisplayMode(staged, bSave: false))
                     select written)
                 from after in Keys(copy)
                 select after.Filter((key, text) => Volatile(key).IsNone && key != ("", "Name") && before.Find(key) != Some(text)))
                .Finally(IO.lift(() => ignore(DisplayModeDescription.DeleteDisplayMode(scratch)))));

    private static IO<Option<bool>> Stale(Guid owner, Guid id, HashMap<(string Section, string Key), string> owned, Seq<ModeKey> keys, bool present, FolderPath package) =>
        present
            ? from live in Keys(id)
              from recorded in Recorded(owner, id).Read
              let differs = owned.Exists((key, text) => !live.Find(key).Exists(held => keys.Find(declared => (declared.Section, declared.Key) == key).Match(
                  Some: declared => declared.Value.Holds(held, Some(package)),
                  None: () => string.Equals(held, text, StringComparison.Ordinal))))
              select Conversions.Present(!differs || Unedited(recorded, live), differs)
            : IO.pure(Some(value: true));

    private static Preference<string> Recorded(Guid owner, Guid id) =>
        Preferences.Setting(new SettingsNode(new SettingsRoot.PlugInNode(owner)).Child(nameof(DisplayModes)), id.ToString("D"), fallback: "", hidden: true);

    private static IO<Unit> Record(Guid owner, Guid id, Seq<(string Section, string Key)> owned) =>
        Keys(id).Bind(live => Recorded(owner, id).Write(Ini.Rendered(id, toHashMap(owned.Distinct().Filter(static key => Volatile(key).IsNone).Choose(key => live.Find(key).Map(text => (key, text)))))));

    private static bool Unedited(string recorded, HashMap<(string Section, string Key), string> live) =>
        Ini.Table(recorded) switch {
            var held => !held.IsEmpty && held.ForAll((key, text) => live.Find(key) == Some(text)),
        };

    private static Option<string> Volatile((string Section, string Key) key) =>
        key switch {
            ("", Order) or ("", DerivedFrom) => Some(key.Key),
            _ => None,
        };

    private static IO<Guid> Imported(Guid id, HashMap<(string Section, string Key), string> table) =>
        from deleted in IO.lift(() => ignore(DisplayModeDescription.DeleteDisplayMode(id)))
        from imported in Import(Ini.Rendered(id, table))
        from same in IO.lift(() => RhinoFault.Unless(imported == id, RhinoReason.Mismatched, imported))
        select imported;

    private static IO<Seq<Guid>> Retired(Guid owner, Seq<Guid> written, Seq<(Guid Id, Option<Guid> Successor)> candidates) =>
        from owned in Products.Each(candidates.Map(mode =>
            from live in Keys(mode.Id)
            from recorded in Recorded(owner, mode.Id).Read
            select Conversions.Present(Unedited(recorded, live), mode)))
        let retired = owned.Somes()
        from moves in Products.Each(retired.Map(static mode => mode.Successor.Match(
            Some: successor => IO.pure((mode.Id, successor)),
            None: () => Keys(mode.Id).Bind(table => IO.lift(() => Conversions.Required(table.Find(("", DerivedFrom)).Bind(parseGuid)).Map(source => (mode.Id, source)))))))
        from moved in Reassigned(toHashMap(written.Map(static id => (id, id))).AddOrUpdateRange(moves))
        from deleted in Products.Each(retired.Map(mode =>
            from removed in Products.Accepted(() => DisplayModeDescription.DeleteDisplayMode(mode.Id))
            from forgotten in SettingsNode.Options.Child("DisplayAttributesManager").Delete(mode.Id.ToString("D"))
            from unrecorded in Recorded(owner, mode.Id).Write("")
            select unit))
        select retired.Map(static mode => mode.Id);

    // --- [FILES]
    public static IO<HashMap<(string Section, string Key), string>> Keys(Guid id) => With(id, Exported);

    private static IO<HashMap<(string Section, string Key), string>> Exported(DisplayModeDescription mode) =>
        Scoped(file =>
            from exported in Products.Accepted(() => DisplayModeDescription.ExportToFile(mode, file))
            from text in IO.lift(() => File.ReadAllText(file))
            select Ini.Table(text));

    private static IO<Guid> Import(string text) =>
        Scoped(file =>
            from written in IO.lift(() => File.WriteAllText(file, text))
            from imported in IO.lift(() => Conversions.Required(Conversions.Present(DisplayModeDescription.ImportFromFile(file, interactive: false))))
            select imported);

    private static IO<T> Scoped<T>(Func<OutputPath, IO<T>> body) =>
        IO.lift(static () => Directory.CreateTempSubdirectory(nameof(DisplayModes))).Bind(folder =>
            IO.lift(() => OutputPath.Of(Path.Join(folder.FullName, $"{nameof(DisplayModes)}.ini"))).Bind(body)
                .Finally(IO.lift(() => folder.Delete(recursive: true))));

    // --- [HOLDERS]
    private static IO<Unit> Reassigned(HashMap<Guid, Guid> moves) =>
        IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments())).Bind(documents => Products.Each(documents.Map(doc => Reassigned(doc, moves)))).Map(unit);

    private static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        moves.Filter(static (source, target) => source != target) switch {
            var moved => Products.Each(Seq(
                Viewports.Edit(
                    doc,
                    new ViewportSet.Every(ViewTypeFilter.All, Details: true),
                    viewport => Shown(viewport).Bind(id => moves.Find(id)).Match(
                        Some: target => With(target, mode => IO.lift(() => { viewport.DisplayMode = mode; })),
                        None: static () => IO.pure(unit)),
                    new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
                when(!moved.IsEmpty, Products.Each(Seq(NamedViews.Reassigned(doc, moved), Overrides(doc, moved), Animations.Reassigned(doc, moved))).Map(unit)).As())).Map(unit),
        };

    private static IO<Unit> Overrides(RhinoDoc doc, HashMap<Guid, Guid> moved) =>
        from slots in Viewports.Resolve(doc, new ViewportSet.Every(ViewTypeFilter.All, Details: true))
            .Map(static rows => (Seq(Guid.Empty) + rows.Map(static row => row.Viewport.Id)).Strict()).Bracket()
        from targets in new ObjectTarget.Lookup(table => table.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true, LockedObjects = true, IncludeLights = true })
                .Where(found => !Overridden(found.Attributes, slots, moved).IsEmpty))
            .Objects(doc)
        from rewritten in AttributeEdits.Modify(doc, targets, _ => [Rows.Write<ObjectAttributes>(staged =>
                Products.Each(Overridden(staged, slots, moved).Map(held => DisplayModeRules.DisplayModeOverride(staged, held.Slot, Some(held.Target)))).Map(unit)).Write],
            quiet: true)
        select unit;

    private static Seq<(Guid Slot, Guid Target)> Overridden(ObjectAttributes attributes, Seq<Guid> slots, HashMap<Guid, Guid> moved) =>
        slots.Choose(slot => DisplayMaterialRefs.DisplayMaterialRef(attributes, slot).Bind(held => moved.Find(held)).Map(target => (slot, target))).Strict();

    private static Option<Guid> Shown(RhinoViewport viewport) {
        using DisplayModeDescription? mode = viewport.DisplayMode;
        return Optional(mode).Map(static held => held.Id);
    }

    // --- [SIGNALS]
    private static readonly Atom<Option<Guid>> Raised = Atom(Option<Guid>.None);

    private static readonly HostEvent<DisplayModeChangedEventArgs> DisplayModeChanged =
        Subscriptions.Inline<DisplayModeChangedEventArgs>(static h => DisplayPipeline.DisplayModeChanged += h, static h => DisplayPipeline.DisplayModeChanged -= h);

    public static HostEvent<Guid> Changed { get; } = Subscriptions.Merged(
        Subscriptions.Changed(Raised).Choose(identity),
        DisplayModeChanged.Choose(static args => Conversions.Present(!Switched(args), args.ChangedDisplayModeId)));

    public static DocumentEvent<DisplayModeChangedEventArgs> Switches { get; } =
        DisplayModeChanged.Choose(static args => Conversions.Present(Switched(args), args)).Documents(static args => args.RhinoDoc);

    private static bool Switched(DisplayModeChangedEventArgs args) =>
        Optional(args.Viewport).Exists(viewport => args.OldDisplayModeId != args.ChangedDisplayModeId && Shown(viewport) == Some(args.ChangedDisplayModeId));

    private static Unit Raise(Seq<Guid> ids) => ids.Iter(static id => ignore(Raised.Swap(_ => Some(id))));

    // --- [TABLE]
    public static IO<TValue> With<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        Products.Of(() => DisplayModeDescription.GetDisplayMode(id)).Bind(body).Bracket();

    // --- [MATERIALS]
    private static readonly Seq<TextureType> Slotted = [TextureType.Bitmap, TextureType.Emap, TextureType.Transparency];

    public static Fin<Unit> CopyFrom(DisplayAttributeMaterial slot, Material material) =>
        Conversions.Rows(material.GetTextures()) switch {
            var textures => Products.Each(textures, static (texture, index) => RhinoFault.Unless(Slotted.Exists(type => type == texture.TextureType), RhinoReason.Unsupported, texture.TextureType, index))
                .Bind(_ => Copied(slot, material, textures)),
        };

    private static readonly Seq<Action<DisplayAttributeMaterial, Material>> Shading = [
        static (slot, material) => slot.Diffuse = material.DiffuseColor,
        static (slot, material) => slot.Emission = material.EmissionColor,
        static (slot, material) => slot.Shine = material.Shine,
        static (slot, material) => slot.Transparency = material.Transparency,
        static (slot, material) => slot.IndexOfRefraction = material.IndexOfRefraction,
        static (slot, material) => slot.Reflectivity = material.Reflectivity,
        static (slot, material) => slot.DisableLighting = material.DisableLighting,
    ];

    private static Fin<Unit> Copied(DisplayAttributeMaterial slot, Material material, Seq<Texture> textures) {
        _ = Shading.Iter(copy => copy(slot, material));
        return Products.Each(Slotted, (type, index) => textures.Find(texture => texture.TextureType == type).Match(
            Some: texture => RhinoFault.Unless(slot.SetTexture(texture, type), RhinoReason.Declined, type, index),
            None: () => Optional(slot.GetTexture(type)).Match(
                Some: _ => RhinoFault.Unless(slot.RemoveTexture(type), RhinoReason.Declined, type, index),
                None: static () => unit))).Map(static _ => unit);
    }
}
