using System.Collections.Generic;
using System.Drawing;
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

    public sealed record LoadFromFile(InputPath FileName) : ContentSource;

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
    public sealed record Set(Action<RenderContent, RenderContent.ChangeContexts> Write) : ContentEdit;

    public sealed record SetChild(ContentSource Source, string ChildSlotName) : ContentEdit;

    public sealed record DeleteChild(string ChildSlotName) : ContentEdit;
}

public sealed record ContentSlot(RenderContent Parent, string ChildSlotName);

public sealed record ContentChoice(Utilities.ContentNewExistingResults Choice, Seq<Guid> Contents);

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
                byId: static (state, byId) => IO.lift(() => Optional(RenderContent.FromId(state.Doc, byId.Id)).Filter(found => state.Kinds.HasFlag(ContentKinds.KindOf(found)))),
                byIndex: static (state, byIndex) => Rows(state.Doc, state.Kinds).Map(rows => rows.At(byIndex.Index)),
                byName: static (state, byName) => Rows(state.Doc, state.Kinds)
                    .Map(rows => rows.Filter(content => string.Equals(content.Name, byName.Name, StringComparison.Ordinal)))
                    .Bind(named => named.Count > 1
                        ? IO.fail<Option<RenderContent>>(new RhinoFault($"render content name {byName.Name}", RhinoReason.Repeated))
                        : IO.pure(named.Head)))
            .Bind(found => IO.lift(found.ToFin(new RhinoFault($"render content {row}", RhinoReason.Missing))))
        from child in IO.lift(() => slots.Fold(Fin.Succ(top), static (parent, slot) => parent.Bind(content =>
            Optional(content.FindChild(slot)).ToFin(new RhinoFault($"slot {slot} of {content.Id}", RhinoReason.Missing)))))
        select child;

    // --- [LIFETIME]
    public static IO<T> Handed<T>(Option<RhinoDoc> doc, ContentSource source, Func<RenderContent, IO<T>> hand) =>
        (from content in Created(doc.ValueUnsafe(), source)
         from handed in hand(content).Catch(error => IO.lift(content.Dispose) >> IO.fail<T>(error))
         select handed).Bracket();

    private static IO<RenderContent> Created(RhinoDoc? doc, ContentSource source) =>
        source.Switch(
            doc,
            fromXml: static (state, xml) => IO.lift(() => Conversions.Found(RenderContent.FromXml(xml.Xml, state)).Map(static content => {
                AutoDelete(content, autoDelete: true);
                return content;
            })),
            loadFromFile: static (_, file) => IO.lift(() => Conversions.Found(RenderContent.LoadFromFile(file.FileName))),
            newContentFromTypeId: static (state, type) => IO.lift(() => Conversions.Found(RenderContentType.NewContentFromTypeId(type.TypeId, state))),
            makeGroupInstance: static (_, group) => IO.lift(() => Conversions.Found(group.Content.MakeGroupInstance())),
            makeCopy: static (_, copy) => IO.lift(() => Conversions.Found(copy.Content.MakeCopy())),
            edit: static (_, edit) => IO.lift(() => Optional(edit.Content.Edit()).ToFin(Errors.Cancelled)),
            fromMaterial: static (state, material) => IO.lift(() => Conversions.Found<RenderContent>(RenderMaterial.FromMaterial(material.Material, state))),
            createBasicMaterial: static (state, basic) => IO.lift(() => Conversions.Found<RenderContent>(RenderMaterial.CreateBasicMaterial(basic.Material, state))),
            newBitmapTexture: static (state, bitmap) => IO.lift(() => Conversions.Found<RenderContent>(RenderTexture.NewBitmapTexture(bitmap.Image, state))),
            fromSimulation: static (state, simulated) =>
                from simulation in use(() => new SimulatedTexture(state))
                from configured in simulated.Configure(simulation)
                from content in IO.lift(() => Conversions.Found<RenderContent>(RenderTexture.NewBitmapTexture(simulation, state)))
                select content,
            newBasicEnvironment: static (state, environment) =>
                from simulation in use(static () => new SimulatedEnvironment())
                from configured in environment.Configure(simulation)
                from content in IO.lift(() => Conversions.Found<RenderContent>(RenderEnvironment.NewBasicEnvironment(simulation, state)))
                select content,
            nativeFactory: static (_, factory) => IO.lift(factory.Create));

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_AutoDelete")]
    internal static extern void AutoDelete(RenderContent content, bool autoDelete);

    public static IO<RenderMaterial> Import(RhinoDoc doc, Material material, bool reference) =>
        IO.lift(() => Conversions.Found(RenderMaterial.CreateImportedMaterial(material, doc, reference)));

    public static IO<RenderContent> Create(RhinoDoc doc, Guid typeId, Option<ContentSlot> slot) =>
        IO.lift(() => slot.Match(
            Some: into => Conversions.Found(RenderContent.Create(doc, typeId, into.Parent, into.ChildSlotName)),
            None: () => Conversions.Found(RenderContent.Create(doc, typeId))));

    public static IO<Unit> Attach(RhinoDoc doc, RenderContent content) =>
        IO.lift(() => RhinoFault.Unless(ContentKinds.ByKind(content, doc.RenderMaterials.Add, doc.RenderEnvironments.Add, doc.RenderTextures.Add)));

    public static IO<RenderContent> Detach(RhinoDoc doc, RenderContent content) =>
        IO.lift(() => RhinoFault.Unless(ContentKinds.ByKind(content, doc.RenderMaterials.Remove, doc.RenderEnvironments.Remove, doc.RenderTextures.Remove)))
            .Bind(_ => use(() => content));

    public static IO<Unit> Replace(RenderContent target, RenderContent replacement) =>
        IO.lift(() => Conversions.Found(target.DocumentOwner).Bind(_ => RhinoFault.Unless(target.Replace(replacement))));

    public static IO<Seq<Guid>> Purge(RhinoDoc doc, RenderContentKind kinds) =>
        Rows(doc, kinds).Bind(rows => Callbacks.Each(rows
            .Filter(static content => content.UseCount() is 0 && !content.IsDefaultInstance && !content.IsHiddenByAutoDelete)
            .Map(content => Detach(doc, content).Map(static detached => detached.Id).Bracket())));

    // --- [CHANGES]
    public static IO<T> WithinContentChange<T>(RenderContent content, RenderContent.ChangeContexts cc, IO<T> body) =>
        IO.lift(() => content.BeginChange(cc)) >> body.Finally(IO.lift(content.EndChange));

    public static IO<T> WithinTableChange<T>(RhinoDoc doc, RenderContent.ChangeContexts cc, IO<T> body) =>
        IO.lift(() => doc.RenderMaterials.BeginChange(cc)) >> body.Finally(IO.lift(doc.RenderMaterials.EndChange));

    public static IO<Unit> Apply(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, Seq<ContentEdit> edits) =>
        WithinContentChange(target, cc, edits.TraverseM(edit => Edited(doc, target, cc, edit)).As().Map(unit));

    private static IO<Unit> Edited(RhinoDoc doc, RenderContent target, RenderContent.ChangeContexts cc, ContentEdit edit) =>
        edit.Switch(
            (Doc: doc, Target: target, Context: cc),
            set: static (state, set) => IO.lift(() => set.Write(state.Target, state.Context)),
            setChild: static (state, set) => Handed(state.Doc, set.Source, child => IO.lift(() => RhinoFault.Unless(state.Target.SetChild(child, set.ChildSlotName)))),
            deleteChild: static (state, delete) => IO.lift(() => RhinoFault.Unless(state.Target.DeleteChild(delete.ChildSlotName, state.Context))));

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
                doc, Conversions.Unset(defaultType), Conversions.Unset(defaultInstance), kinds, flags, Conversions.Unset(presetCategory), categories, types, out Guid[] contents)
            is not Utilities.ContentNewExistingResults.None and var choice ? Fin.Succ(new ContentChoice(choice, toSeq(contents))) : Errors.Cancelled);

    // --- [READS]
    public static IO<uint> Hash(RenderContent content, CrcRenderHashFlags flags, Seq<string> excluded, Option<LinearWorkflow> workflow) =>
        IO.lift(() => (workflow.Case, string.Join(';', excluded)) switch {
            (LinearWorkflow linear, var names) => content.RenderHashExclude(flags | CrcRenderHashFlags.ExcludeLinearWorkflow, names, linear),
            (_, "") when flags is CrcRenderHashFlags.Normal => content.RenderHash,
            (_, var names) => content.RenderHashExclude(flags, names),
        });

    public static IO<Bitmap> Icon(RenderContent content, Size size, Option<DynamicIconUsage> usage) =>
        IO.lift(() => usage.Match(
                Some: dynamic => RhinoFault.Unless(content.DynamicIcon(size, out Bitmap bitmap, dynamic)).Map(_ => bitmap),
                None: () => RhinoFault.Unless(content.Icon(size, out Bitmap bitmap)).Map(_ => bitmap)))
            .Bind(static bitmap => use(() => bitmap));

    // --- [FILES]
    public static IO<Unit> SaveToFile(RenderContent content, string fileName, RenderContent.EmbedFilesChoice embed) =>
        IO.lift(() => OutputPath.Of(Path.ChangeExtension(fileName, ContentKinds.ByKind(content, static _ => ".rmtl", static _ => ".renv", static _ => ".rtex")))
            .Bind(path => RhinoFault.Unless(content.SaveToFile(path, embed))));

    public static IO<Unit> SaveAsImage(RenderTexture texture, OutputPath path, int width, int height, int depth) =>
        IO.lift(() => RhinoFault.Unless(texture.SaveAsImage(path, width, height, depth)));
}
