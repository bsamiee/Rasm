using System.Drawing;
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

    public sealed record CreateImportedMaterial(Material Material, bool Reference) : ContentSource;

    public sealed record NewBitmapTexture(Bitmap Image) : ContentSource;

    public sealed record FromSimulation(SimulatedTextureState State) : ContentSource;

    public sealed record NewBasicEnvironment(EnvironmentState State) : ContentSource;
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
    public sealed record New(Seq<Guid> TypeIds) : ContentChoice;

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
                    .Bind(static named => named.Count > 1
                        ? IO.fail<Option<RenderContent>>(new CountMismatch(nameof(RenderContent.Name), Required: 1, named.Count))
                        : IO.pure(named.Head)))
            .Bind(found => IO.lift(found.ToFin(new MissingComponent<RenderContent>(row))))
        from child in IO.lift(() => slots.Fold(Fin.Succ(top), static (parent, slot) => parent.Bind(content => EmptySlot.Unless(content.FindChild(slot), content.Id, slot))))
        select child;

    // --- [LIFETIME]
    public static IO<RenderContent> FreeFloating(RhinoDoc doc, ContentSource source) =>
        source.Switch(
            doc,
            fromXml: static (document, xml) => IO.lift(() => Missing.Unless(RenderContent.FromXml(xml.Xml, document), nameof(RenderContent.FromXml))),
            loadFromFile: static (_, file) => IO.lift(() => Exchange.ExistingPath(file.FileName)
                .Bind(static path => Missing.Unless(RenderContent.LoadFromFile(path), nameof(RenderContent.LoadFromFile)))),
            newContentFromTypeId: static (document, type) =>
                IO.lift(() => Missing.Unless(RenderContentType.NewContentFromTypeId(type.TypeId, document), nameof(RenderContentType.NewContentFromTypeId))),
            makeGroupInstance: static (_, group) => IO.lift(() => Missing.Unless(group.Content.MakeGroupInstance(), nameof(RenderContent.MakeGroupInstance))),
            makeCopy: static (_, copy) => IO.lift(() => Missing.Unless(copy.Content.MakeCopy(), nameof(RenderContent.MakeCopy))),
            edit: static (_, edit) => IO.lift(() => Optional(edit.Content.Edit()).ToFin(Errors.Cancelled)),
            fromMaterial: static (document, material) =>
                IO.lift(() => Missing.Unless<RenderContent>(RenderMaterial.FromMaterial(material.Material, document), nameof(RenderMaterial.FromMaterial))),
            createBasicMaterial: static (document, basic) =>
                IO.lift(() => Missing.Unless<RenderContent>(RenderMaterial.CreateBasicMaterial(basic.Material, document), nameof(RenderMaterial.CreateBasicMaterial))),
            createImportedMaterial: static (document, imported) =>
                IO.lift(() => Missing.Unless<RenderContent>(RenderMaterial.CreateImportedMaterial(imported.Material, document, imported.Reference), nameof(RenderMaterial.CreateImportedMaterial))),
            newBitmapTexture: static (document, bitmap) =>
                IO.lift(() => Missing.Unless<RenderContent>(RenderTexture.NewBitmapTexture(bitmap.Image, document), nameof(RenderTexture.NewBitmapTexture))),
            fromSimulation: static (document, simulated) =>
                (from simulation in use(() => new SimulatedTexture(document))
                 from _ in ContentKinds.WriteSimulation(simulated.State, simulation)
                 from texture in IO.lift(() => Missing.Unless<RenderContent>(RenderTexture.NewBitmapTexture(simulation, document), nameof(RenderTexture.NewBitmapTexture)))
                 select texture).Bracket(),
            newBasicEnvironment: static (document, environment) =>
                (from simulation in use(static () => new SimulatedEnvironment())
                 from _ in ContentKinds.WriteEnvironment(environment.State, simulation)
                 from basic in IO.lift(() => Missing.Unless<RenderContent>(RenderEnvironment.NewBasicEnvironment(simulation, document), nameof(RenderEnvironment.NewBasicEnvironment)))
                 select basic).Bracket());

    public static IO<RenderContent> Create(RhinoDoc doc, Guid typeId, Option<(RenderContent Parent, string ChildSlotName)> slot) =>
        IO.lift(() => Missing.Unless(
            slot.Match(Some: into => RenderContent.Create(doc, typeId, into.Parent, into.ChildSlotName), None: () => RenderContent.Create(doc, typeId)),
            nameof(RenderContent.Create)));

    public static IO<Unit> Attach(RhinoDoc doc, RenderContent content) =>
        IO.lift(() => Refused.Unless(Conversions.ByKind(content, doc.RenderMaterials.Add, doc.RenderEnvironments.Add, doc.RenderTextures.Add), nameof(RenderMaterialTable.Add)));

    public static IO<RenderContent> Detach(RhinoDoc doc, RenderContent content) =>
        IO.lift(() => Refused.Unless(Conversions.ByKind(content, doc.RenderMaterials.Remove, doc.RenderEnvironments.Remove, doc.RenderTextures.Remove), content, nameof(RenderMaterialTable.Remove)));

    public static IO<Unit> Replace(RenderContent target, RenderContent replacement) =>
        IO.lift(() => Unattached.Unless(target.DocumentOwner is not null, target.Id).Bind(_ => Refused.Unless(target.Replace(replacement), nameof(RenderContent.Replace))));

    public static IO<Seq<Guid>> Purge(RhinoDoc doc, RenderContentKind kinds) =>
        Rows(doc, kinds).Bind(rows => rows
            .Filter(static content => content.UseCount() == 0 && !content.IsDefaultInstance && !content.IsHiddenByAutoDelete)
            .TraverseM(content => use(Detach(doc, content)).Map(static detached => detached.Id).Bracket())
            .As());

    // --- [CHANGES]
    public static IO<A> WithinContentChange<A>(RenderContent content, RenderContent.ChangeContexts cc, IO<A> body) =>
        IO.lift(() => content.BeginChange(cc)).Bracket(Use: _ => IO.pure(unit).Bind(_ => body), Fin: _ => IO.lift(content.EndChange));

    public static IO<A> WithinTableChange<A>(RhinoDoc doc, RenderContent.ChangeContexts cc, IO<A> body) =>
        IO.lift(() => doc.RenderMaterials.BeginChange(cc)).Bracket(Use: _ => IO.pure(unit).Bind(_ => body), Fin: _ => IO.lift(doc.RenderMaterials.EndChange));

    public static IO<Unit> Apply(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, Seq<ContentEdit> edits) =>
        WithinContentChange(target, cc, edits.TraverseM(edit => Edited(doc, target, cc, edit)).As().Map(static _ => unit));

    private static IO<Unit> Edited(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, ContentEdit edit) =>
        edit.Switch(
            (Doc: doc, Target: target, Context: cc),
            setName: static (state, name) => IO.lift(() => state.Target.SetName(name.Name, name.RenameEvents, name.EnsureNameUnique)),
            set: static (state, set) => IO.lift(() => set.Write(state.Target)),
            setChild: static (state, set) =>
                (from child in use(FreeFloating(state.Doc, set.Source))
                 from _ in IO.lift(() => SlotRejected.Unless(state.Target.IsContentTypeAcceptableAsChild(child.TypeId, set.ChildSlotName), child.TypeId, set.ChildSlotName))
                 from __ in IO.lift(() => Refused.Unless(state.Target.SetChild(child, set.ChildSlotName), nameof(RenderContent.SetChild)))
                 select unit).Bracket(),
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
                    Utilities.ContentNewExistingResults.New => new ContentChoice.New(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Copy => new ContentChoice.Copy(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Instance => new ContentChoice.Instance(toSeq(contents)),
                    Utilities.ContentNewExistingResults.Reference => new ContentChoice.Reference(toSeq(contents)),
                });

    // --- [READS]
    public static IO<uint> Hash(RenderContent content, CrcRenderHashFlags flags, Seq<string> excluded, Option<LinearWorkflow> workflow) =>
        IO.lift(() => (workflow.Case, string.Join(';', excluded)) switch {
            (LinearWorkflow linear, var names) => content.RenderHashExclude(flags | CrcRenderHashFlags.ExcludeLinearWorkflow, names, linear),
            (_, "") when flags == CrcRenderHashFlags.Normal => content.RenderHash,
            (_, var names) => content.RenderHashExclude(flags, names),
        });

    public static IO<Bitmap> Icon(RenderContent content, Size size, Option<DynamicIconUsage> usage) =>
        IO.lift(() => usage.Match(
            Some: dynamic => content.DynamicIcon(size, out Bitmap bitmap, dynamic) ? Fin.Succ(bitmap) : new Missing(nameof(RenderContent.DynamicIcon)),
            None: () => content.Icon(size, out Bitmap bitmap) ? Fin.Succ(bitmap) : new Missing(nameof(RenderContent.Icon))));

    // --- [FILES]
    public static IO<Unit> SaveToFile(RenderContent content, string fileName, RenderContent.EmbedFilesChoice embed) =>
        IO.lift(() => Exchange.QualifiedPath(fileName)
            .Map(qualified => Path.ChangeExtension(qualified, Conversions.ByKind(content, static _ => ".rmtl", static _ => ".renv", static _ => ".rtex")))
            .Bind(path => Refused.Unless(content.SaveToFile(path, embed), nameof(RenderContent.SaveToFile))));

    public static IO<Unit> SaveAsImage(RenderTexture texture, string fullPath, int width, int height, int depth) =>
        IO.lift(() => Exchange.QualifiedPath(fullPath).Bind(target => Refused.Unless(texture.SaveAsImage(target, width, height, depth), nameof(RenderTexture.SaveAsImage))));
}
