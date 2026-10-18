using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ContentSource {
    public sealed record FromXml(string Xml) : ContentSource;

    public sealed record LoadFromFile(string FileName) : ContentSource;

    public sealed record NewContentFromTypeId(Guid TypeId) : ContentSource;

    public sealed record MakeGroupInstance(RenderContent Content) : ContentSource;

    public sealed record MakeCopy(RenderContent Content) : ContentSource;

    public sealed record Edit(RenderContent Content) : ContentSource;

    public sealed record FromMaterial(Material Material) : ContentSource;

    public sealed record CreateBasicMaterial(Material Material) : ContentSource;

    public sealed record NewBitmapTexture(Bitmap Image) : ContentSource;

    public sealed record FromSimulation(Func<SimulatedTexture, IO<Unit>> Configure) : ContentSource;

    public sealed record NewBasicEnvironment(Func<SimulatedEnvironment, IO<Unit>> Configure) : ContentSource;

    public sealed record NativeFactory(Func<Fin<RenderContent>> Create) : ContentSource;
}

[Union]
public abstract partial record ContentEdit {
    public sealed record SetName(string Name, bool RenameEvents, bool EnsureNameUnique) : ContentEdit;

    public sealed record Set(Action<RenderContent> Write) : ContentEdit;

    public sealed record SetChild(ContentSource Source, string ChildSlotName) : ContentEdit;

    public sealed record DeleteChild(string ChildSlotName) : ContentEdit;

    public sealed record DeleteAllChildren() : ContentEdit;

    public sealed record SetChildSlotOn(string ChildSlotName, bool On) : ContentEdit;

    public sealed record SetChildSlotAmount(string ChildSlotName, double Amount) : ContentEdit;

    public sealed record Ungroup() : ContentEdit;

    public sealed record UngroupRecursive() : ContentEdit;

    public sealed record SmartUngroupRecursive() : ContentEdit;
}

[Union]
public abstract partial record ContentChoice {
    public sealed record Created(Seq<Guid> TypeIds) : ContentChoice;

    public sealed record Copy(Seq<Guid> Instances) : ContentChoice;

    public sealed record Instance(Seq<Guid> Instances) : ContentChoice;

    public sealed record Reference(Seq<Guid> Instances) : ContentChoice;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contents {
    // --- [ROWS]
    public static IO<Seq<RenderContent>> Rows(RhinoDoc doc, RenderContentKind kinds) =>
        IO.lift(() => Seq<(RenderContentKind Kind, IEnumerable<RenderContent> Table)>(
                (RenderContentKind.Material, doc.RenderMaterials), (RenderContentKind.Environment, doc.RenderEnvironments), (RenderContentKind.Texture, doc.RenderTextures))
            .Filter(row => kinds.HasFlag(row.Kind))
            .Bind(static row => toSeq(row.Table))
            .Strict());

    public static IO<RenderContent> Resolve(RhinoDoc doc, RenderContentKind kinds, ComponentRef<RenderContent> row, Seq<string> slots) =>
        from top in row.Switch(
                (Doc: doc, Kinds: kinds),
                byId: static (state, byId) => IO.lift(() => Optional(RenderContent.FromId(state.Doc, byId.Id)).Filter(found => state.Kinds.HasFlag(Conversions.KindOf(found)))),
                byIndex: static (state, byIndex) => Rows(state.Doc, state.Kinds).Map(rows => rows.At(byIndex.Index)),
                byName: static (state, byName) => Rows(state.Doc, state.Kinds)
                    .Map(rows => rows.Filter(content => string.Equals(content.Name, byName.Name, StringComparison.Ordinal)))
                    .Bind(named => named.Count > 1
                        ? IO.fail<Option<RenderContent>>(Error.New(string.Create(CultureInfo.InvariantCulture, $"{named.Count} render contents share the name {byName.Name}")))
                        : IO.pure(named.Head)))
            .Bind(found => IO.lift(found.ToFin(new NotFound(nameof(RenderContent), $"{row}"))))
        from child in IO.lift(() => slots.Fold(Fin.Succ(top), static (parent, slot) => parent.Bind(content => Optional(content.FindChild(slot)).ToFin(new NotFound("child content", $"slot {slot} of {content.Id}")))))
        select child;

    // --- [LIFETIME]
    public static IO<(RenderContent Content, Disposal<RenderContent> Ownership)> FreeFloating(Option<RhinoDoc> doc, ContentSource source, Action<IDisposable> retain) {
        return source.Switch(
            (Doc: doc.ValueUnsafe(), Retain: retain),
            fromXml: static (state, xml) => Created(() => Refused.UnlessFound(RenderContent.FromXml(xml.Xml, state.Doc), nameof(RenderContent.FromXml))
                .Map(static content => { AutoDelete(content, autoDelete: true); return content; }), state.Retain),
            loadFromFile: static (state, file) => Created(() => Exchange.ExistingPath(file.FileName)
                .Bind(static path => Refused.UnlessFound(RenderContent.LoadFromFile(path), nameof(RenderContent.LoadFromFile))), state.Retain),
            newContentFromTypeId: static (state, type) =>
                Created(() => Refused.UnlessFound(RenderContentType.NewContentFromTypeId(type.TypeId, state.Doc), nameof(RenderContentType.NewContentFromTypeId)), state.Retain),
            makeGroupInstance: static (state, group) => Created(() => Refused.UnlessFound(group.Content.MakeGroupInstance(), nameof(RenderContent.MakeGroupInstance)), state.Retain),
            makeCopy: static (state, copy) => Created(() => Refused.UnlessFound(copy.Content.MakeCopy(), nameof(RenderContent.MakeCopy)), state.Retain),
            edit: static (state, edit) => Created(() => Optional(edit.Content.Edit()).ToFin(Errors.Cancelled), state.Retain),
            fromMaterial: static (state, material) =>
                Created(() => Refused.UnlessFound<RenderContent>(RenderMaterial.FromMaterial(material.Material, state.Doc), nameof(RenderMaterial.FromMaterial)), state.Retain),
            createBasicMaterial: static (state, basic) =>
                Created(() => Refused.UnlessFound<RenderContent>(RenderMaterial.CreateBasicMaterial(basic.Material, state.Doc), nameof(RenderMaterial.CreateBasicMaterial)), state.Retain),
            newBitmapTexture: static (state, bitmap) =>
                Created(() => Refused.UnlessFound<RenderContent>(RenderTexture.NewBitmapTexture(bitmap.Image, state.Doc), nameof(RenderTexture.NewBitmapTexture)), state.Retain),
            fromSimulation: static (state, simulated) =>
                DisposalOps.Within(() => new SimulatedTexture(state.Doc), simulation =>
                    simulated.Configure(simulation).Bind(_ =>
                        Created(() => Refused.UnlessFound<RenderContent>(RenderTexture.NewBitmapTexture(simulation, state.Doc), nameof(RenderTexture.NewBitmapTexture)), state.Retain))),
            newBasicEnvironment: static (state, environment) =>
                DisposalOps.Within(static () => new SimulatedEnvironment(), simulation =>
                    environment.Configure(simulation).Bind(_ =>
                        Created(() => Refused.UnlessFound<RenderContent>(RenderEnvironment.NewBasicEnvironment(simulation, state.Doc), nameof(RenderEnvironment.NewBasicEnvironment)), state.Retain))),
            nativeFactory: static (state, factory) => Created(factory.Create, state.Retain));

        static IO<(RenderContent Content, Disposal<RenderContent> Ownership)> Created(Func<Fin<RenderContent>> acquire, Action<IDisposable> retain) =>
            DisposalOps.Acquire(IO.lift(() => acquire().Map(static content => (Content: content, Ownership: new Disposal<RenderContent>(content, static held => held.Dispose())))),
                static held => IO.lift(held.Ownership.Dispose), held => retain(held.Ownership));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_AutoDelete")]
    internal static extern void AutoDelete(RenderContent content, bool autoDelete);

    public static IO<RenderMaterial> Import(RhinoDoc doc, Material material, bool reference) =>
        IO.lift(() => Refused.UnlessFound(RenderMaterial.CreateImportedMaterial(material, doc, reference), nameof(RenderMaterial.CreateImportedMaterial)));

    public static IO<RenderContent> Create(RhinoDoc doc, Guid typeId, Option<(RenderContent Parent, string ChildSlotName)> slot) =>
        IO.lift(() => Refused.UnlessFound(
            slot.Match(Some: into => RenderContent.Create(doc, typeId, into.Parent, into.ChildSlotName), None: () => RenderContent.Create(doc, typeId)),
            nameof(RenderContent.Create)));

    public static IO<Unit> Attach(RhinoDoc doc, (RenderContent Content, Disposal<RenderContent> Ownership) content) =>
        IO.lift(() => Refused.Unless(Conversions.ByKind(content.Content, doc.RenderMaterials.Add, doc.RenderEnvironments.Add, doc.RenderTextures.Add), nameof(RenderMaterialTable.Add))
            .Map(_ => { content.Ownership.Relinquish(); return unit; })).Uninterruptible();

    public static IO<RenderContent> Detach(RhinoDoc doc, RenderContent content, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => Refused.Unless(Conversions.ByKind(content, doc.RenderMaterials.Remove, doc.RenderEnvironments.Remove, doc.RenderTextures.Remove), content, nameof(RenderMaterialTable.Remove))), held => retain(new Disposal<RenderContent>(held, static detached => detached.Dispose())));

    public static IO<Unit> Replace(RenderContent target, RenderContent replacement) =>
        IO.lift(() => (target.DocumentOwner is not null ? Fin.Succ(unit) : Error.New($"Content {target.Id} belongs to no document"))
            .Bind(_ => Refused.Unless(target.Replace(replacement), nameof(RenderContent.Replace))));

    public static IO<Seq<Guid>> Purge(RhinoDoc doc, RenderContentKind kinds) =>
        Rows(doc, kinds).Bind(rows => Callbacks.Each(rows
            .Filter(static content => content.UseCount() is 0 && !content.IsDefaultInstance && !content.IsHiddenByAutoDelete)
            .Map(content => DisposalOps.Within<IDisposable, Guid>(retain => Detach(doc, content, retain).Map(static detached => detached.Id), DisposalOps.Release))));

    // --- [CHANGES]
    public static IO<T> WithinContentChange<T>(RenderContent content, RenderContent.ChangeContexts cc, IO<T> body) =>
        DisposalOps.Within(fun(() => content.BeginChange(cc)), _ => body, _ => IO.lift(content.EndChange));

    public static IO<T> WithinTableChange<T>(RhinoDoc doc, RenderContent.ChangeContexts cc, IO<T> body) =>
        DisposalOps.Within(fun(() => doc.RenderMaterials.BeginChange(cc)), _ => body, _ => IO.lift(doc.RenderMaterials.EndChange));

    public static IO<Unit> Apply(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, Seq<ContentEdit> edits) =>
        WithinContentChange(target, cc, edits.TraverseM(edit => Edited(doc, target, cc, edit)).As().Map(unit));

    private static IO<Unit> Edited(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, ContentEdit edit) =>
        edit.Switch(
            (Doc: doc, Target: target, Context: cc),
            setName: static (state, name) => IO.lift(() => state.Target.SetName(name.Name, name.RenameEvents, name.EnsureNameUnique)),
            set: static (state, set) => IO.lift(() => set.Write(state.Target)),
            setChild: static (state, set) =>
                DisposalOps.Within<IDisposable, Unit>(retain =>
                    FreeFloating(state.Doc, set.Source, retain).Bind(child =>
                        IO.lift(() => Refused.Unless(state.Target.SetChild(child.Content, set.ChildSlotName), nameof(RenderContent.SetChild))
                            .Map(_ => { child.Ownership.Relinquish(); return unit; })).Uninterruptible()), DisposalOps.Release),
            deleteChild: static (state, delete) =>
                IO.lift(() => Refused.Unless(state.Target.DeleteChild(delete.ChildSlotName, state.Context), nameof(RenderContent.DeleteChild))),
            deleteAllChildren: static (state, _) => IO.lift(() => state.Target.DeleteAllChildren(state.Context)),
            setChildSlotOn: static (state, on) => IO.lift(() => state.Target.SetChildSlotOn(on.ChildSlotName, on.On, state.Context)),
            setChildSlotAmount: static (state, amount) => IO.lift(() => state.Target.SetChildSlotAmount(amount.ChildSlotName, amount.Amount, state.Context)),
            ungroup: static (state, _) => IO.lift(() => ignore(state.Target.Ungroup())),
            ungroupRecursive: static (state, _) => IO.lift(() => ignore(state.Target.UngroupRecursive())),
            smartUngroupRecursive: static (state, _) => IO.lift(() => ignore(state.Target.SmartUngroupRecursive())));

    // --- [CHOOSERS]
    public static IO<Guid> ShowContentInstanceBrowser(RhinoDoc doc, Option<Guid> selected, RenderContentKind kinds, RenderContent.ContentInstanceBrowserButtons buttons) =>
        IO.lift(() => {
            Guid chosen = Conversions.Unset(selected);
            return RenderContent.ShowContentInstanceBrowser(doc, ref chosen, kinds, buttons) ? Fin.Succ(chosen) : Errors.Cancelled;
        });

    public static IO<ContentChoice> ShowContentNewExistingBrowser(
        RhinoDoc doc, Option<Guid> defaultType, Option<Guid> defaultInstance, RenderContentKind kinds, Utilities.ContentNewExistingFlags flags,
        Option<string> presetCategory, Seq<string> categories, Seq<Guid> types) =>
        IO.lift(() => Utilities.ShowContentNewExistingBrowser(
                doc, Conversions.Unset(defaultType), Conversions.Unset(defaultInstance), kinds, flags, Conversions.Unset(presetCategory), categories, types, out Guid[] contents) switch {
                    Utilities.ContentNewExistingResults.None => Fin.Fail<ContentChoice>(Errors.Cancelled),
                    Utilities.ContentNewExistingResults.New => new ContentChoice.Created(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Copy => new ContentChoice.Copy(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Instance => new ContentChoice.Instance(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Reference => new ContentChoice.Reference(toSeq(contents)),
                });

    // --- [READS]
    public static IO<uint> Hash(RenderContent content, CrcRenderHashFlags flags, Seq<string> excluded, Option<LinearWorkflow> workflow) =>
        IO.lift(() => (workflow.Case, string.Join(';', excluded)) switch {
            (LinearWorkflow linear, var names) => content.RenderHashExclude(flags | CrcRenderHashFlags.ExcludeLinearWorkflow, names, linear),
            (_, "") when flags is CrcRenderHashFlags.Normal => content.RenderHash,
            (_, var names) => content.RenderHashExclude(flags, names),
        });

    public static IO<Bitmap> Icon(RenderContent content, Size size, Option<DynamicIconUsage> usage, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => usage.Match(
            Some: dynamic => content.DynamicIcon(size, out Bitmap bitmap, dynamic) ? Fin.Succ(bitmap) : new Refused(nameof(RenderContent.DynamicIcon), Refused.Absent),
            None: () => content.Icon(size, out Bitmap bitmap) ? bitmap : new Refused(nameof(RenderContent.Icon), Refused.Absent))), bitmap => retain(bitmap));

    // --- [FILES]
    public static IO<Unit> SaveToFile(RenderContent content, string fileName, RenderContent.EmbedFilesChoice embed) =>
        IO.lift(() => Exchange.QualifiedPath(fileName)
            .Map(qualified => Path.ChangeExtension(qualified, Conversions.ByKind(content, static _ => ".rmtl", static _ => ".renv", static _ => ".rtex")))
            .Bind(path => Refused.Unless(content.SaveToFile(path, embed), nameof(RenderContent.SaveToFile))));

    public static IO<Unit> SaveAsImage(RenderTexture texture, string fullPath, int width, int height, int depth) =>
        IO.lift(() => Exchange.QualifiedPath(fullPath).Bind(target => Refused.Unless(texture.SaveAsImage(target, width, height, depth), nameof(RenderTexture.SaveAsImage))));
}
