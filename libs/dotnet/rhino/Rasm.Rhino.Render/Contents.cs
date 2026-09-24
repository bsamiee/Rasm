using System.Diagnostics;
using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]
[assembly: UseStaticMapper(typeof(DocumentHandles))]

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
public enum UngroupMode { Ungroup = 0, UngroupRecursive = 1, SmartUngroupRecursive = 2 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentSource {
    public sealed record FromXml(string Text) : ContentSource;

    public sealed record FromFile(string Path) : ContentSource;

    public sealed record GroupInstance(RenderContent Source) : ContentSource;

    public sealed record FromTypeId(Guid TypeId) : ContentSource;
}

public sealed record SlotState(string Name, Guid Child, string DisplayName, bool On, double Amount);

public sealed record ContentState(
    Guid Id,
    Guid TypeId,
    Guid GroupId,
    RenderContentKind Kind,
    Option<string> Name,
    Option<string> DisplayName,
    Option<string> TypeName,
    Option<string> TypeDescription,
    Option<string> Notes,
    Option<string> Tags,
    Option<string> Category,
    RenderContentStyles Styles,
    ProxyTypes ProxyType,
    LengthUnit ModelUnits,
    bool TopLevel,
    bool Hidden,
    bool Private,
    bool IsLocked,
    bool CanBeEdited,
    bool IsDefaultInstance,
    bool IsHiddenByAutoDelete,
    bool IsReference,
    int UseCount,
    Option<uint> DocumentOwner,
    Option<uint> DocumentAssoc,
    Option<Guid> Parent,
    Option<string> ChildSlotName,
    Seq<SlotState> Slots);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentOp {
    public sealed record Rename(string Name, bool RenameEvents, bool EnsureUnique) : ContentOp;

    public sealed record SetChild(string ChildSlotName, RenderContent Child) : ContentOp;

    public sealed record DeleteChild(Option<string> ChildSlotName) : ContentOp;

    public sealed record ChildSlot(string Name, Option<bool> On, Option<double> Amount) : ContentOp;

    public sealed record Replace(ContentSource Source) : ContentOp;

    public sealed record Ungroup(UngroupMode Mode) : ContentOp;

    public sealed record SaveToFile(string Path, RenderContent.EmbedFilesChoice Embed) : ContentOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IconKind {
    public sealed record Standard() : IconKind;

    public sealed record Virtual() : IconKind;

    public sealed record DynamicIcon(DynamicIconUsage Usage) : IconKind;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contents {
    // --- [RESOLUTION]
    public static RenderContentKind KindOf(RenderContent content) =>
        content switch {
            RenderMaterial => RenderContentKind.Material,
            RenderEnvironment => RenderContentKind.Environment,
            RenderTexture => RenderContentKind.Texture,
            _ => throw new UnreachableException(),
        };

    public static IO<Option<RenderContent>> Resolve(RhinoDoc doc, RenderContentKind kind, ComponentRef row, Seq<string> slots) =>
        from table in IO.lift(() => Table(doc, kind))
        from root in TableOps.Find(
            row,
            id => Optional(RenderContent.FromId(doc, id)).Traverse(content => Kinded(content, kind)).As(),
            index => toSeq(table.Rows).At(index),
            name => Named(toSeq(table.Rows).Filter(content => string.Equals(content.Name, name, StringComparison.Ordinal)).Strict()))
        from child in IO.lift(() => slots.Fold(root, static (parent, slot) => parent.Bind(content => Optional(content.FindChild(slot)))))
        select child;

    public static IO<Seq<RenderContent>> Rows(RhinoDoc doc, RenderContentKind kind) =>
        IO.lift(() => Table(doc, kind).Map(static table => toSeq(table.Rows).Strict()));

    public static IO<ContentState> Snapshot(RenderContent content) =>
        IO.lift(() => ContentMapper.ToState(content));

    public static IO<uint> Hash(RenderContent content, CrcRenderHashFlags flags, Seq<string> excludedParameters, Option<LinearWorkflow> workflow) =>
        from supported in IO.lift(() => Invalid.Unless(workflow.IsNone || flags.HasFlag(CrcRenderHashFlags.ExcludeLinearWorkflow), nameof(CrcRenderHashFlags.ExcludeLinearWorkflow)))
        let names = string.Join(';', excludedParameters)
        from hash in IO.lift(() => workflow.Match(
            Some: lw => content.RenderHashExclude(flags, names, lw),
            None: () => (flags == CrcRenderHashFlags.Normal) && excludedParameters.IsEmpty ? content.RenderHash : content.RenderHashExclude(flags, names)))
        select hash;

    private static Fin<RenderContent> Kinded(RenderContent content, RenderContentKind kind) =>
        KindOf(content) == kind ? content : new WrongKind(kind, KindOf(content));

    private static Fin<Option<RenderContent>> Named(Seq<RenderContent> rows) =>
        rows.Count > 1 ? new Ambiguous(nameof(RenderContent.Name), rows.Count) : rows.Head;

    internal static Seq<RenderContent> Children(RenderContent parent) =>
        toSeq(LanguageExt.List.unfold(parent.FirstChild, static child => Optional(child).Map(static c => (c, c.NextSibling))));

    private static Fin<ContentTable> Table(RhinoDoc doc, RenderContentKind kind) =>
        kind switch {
            RenderContentKind.Material => new ContentTable(
                nameof(RhinoDoc.RenderMaterials),
                doc.RenderMaterials,
                content => doc.RenderMaterials.Add((RenderMaterial)content),
                content => doc.RenderMaterials.Remove((RenderMaterial)content),
                doc.RenderMaterials.BeginChange,
                doc.RenderMaterials.EndChange),
            RenderContentKind.Environment => new ContentTable(
                nameof(RhinoDoc.RenderEnvironments),
                doc.RenderEnvironments,
                content => doc.RenderEnvironments.Add((RenderEnvironment)content),
                content => doc.RenderEnvironments.Remove((RenderEnvironment)content),
                doc.RenderEnvironments.BeginChange,
                doc.RenderEnvironments.EndChange),
            RenderContentKind.Texture => new ContentTable(
                nameof(RhinoDoc.RenderTextures),
                doc.RenderTextures,
                content => doc.RenderTextures.Add((RenderTexture)content),
                content => doc.RenderTextures.Remove((RenderTexture)content),
                doc.RenderTextures.BeginChange,
                doc.RenderTextures.EndChange),
            _ => new Invalid(nameof(RenderContentKind)),
        };

    private sealed record ContentTable(
        string Member,
        IEnumerable<RenderContent> Rows,
        Func<RenderContent, bool> Add,
        Func<RenderContent, bool> Remove,
        Action<RenderContent.ChangeContexts> BeginChange,
        Action EndChange);

    // --- [HANDLES]
    public static IO<TValue> WithCreated<TValue>(RhinoDoc doc, ContentSource source, Func<RenderContent, IO<TValue>> body) =>
        Disposal.Using(source.Switch(
            doc,
            fromXml: static (document, xml) => IO.lift(() => Missing.Unless(RenderContent.FromXml(xml.Text, document), nameof(RenderContent.FromXml))),
            fromFile: static (_, file) =>
                from path in Answers.ExistingPath(file.Path)
                from content in IO.lift(() => Missing.Unless(RenderContent.LoadFromFile(path), nameof(RenderContent.LoadFromFile)))
                select content,
            groupInstance: static (_, grouped) => IO.lift(() => Missing.Unless(grouped.Source.MakeGroupInstance(), nameof(RenderContent.MakeGroupInstance))),
            fromTypeId: static (document, fromTypeId) =>
                from id in IO.lift(() => Answers.NonEmpty(fromTypeId.TypeId, nameof(RenderContentType.NewContentFromTypeId)))
                from content in IO.lift(() => Missing.Unless(RenderContentType.NewContentFromTypeId(id, document), nameof(RenderContentType.NewContentFromTypeId)))
                select content), body);

    public static IO<Unit> Attach(RhinoDoc doc, RenderContent content) =>
        IO.lift(() => Table(doc, KindOf(content)).Bind(table => Refused.Unless(table.Add(content), table.Member)));

    public static IO<TValue> WithDetached<TValue>(RhinoDoc doc, RenderContent content, Func<RenderContent, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Table(doc, KindOf(content)).Bind(table => Refused.Unless(table.Remove(content), table.Member)).Map(_ => content)), body);

    // --- [CHANGES]
    public static IO<TValue> WithinTableChange<TValue>(RhinoDoc doc, RenderContentKind kind, RenderContent.ChangeContexts cc, IO<TValue> body) =>
        IO.lift(() => Table(doc, kind)).Bind(table => Disposal.Bracketed(() => table.BeginChange(cc), table.EndChange, body));

    public static IO<TValue> WithinContentChange<TValue>(RenderContent content, RenderContent.ChangeContexts cc, IO<TValue> body) =>
        Disposal.Bracketed(() => content.BeginChange(cc), content.EndChange, body);

    public static IO<Unit> Apply(RenderContent target, RenderContent.ChangeContexts cc, ContentOp op) =>
        op.Switch(
            (Target: target, Context: cc),
            rename: static (state, rename) =>
                WithinContentChange(state.Target, state.Context, IO.lift(() => state.Target.SetName(rename.Name, rename.RenameEvents, rename.EnsureUnique))),
            setChild: static (state, set) =>
                from acceptable in IO.lift(() => SlotRejected.Unless(state.Target.IsContentTypeAcceptableAsChild(set.Child.TypeId, set.ChildSlotName), set.Child.TypeId, set.ChildSlotName))
                from child in WithinContentChange(state.Target, state.Context, IO.lift(() => Refused.Unless(state.Target.SetChild(set.Child, set.ChildSlotName), nameof(RenderContent.SetChild))))
                select child,
            deleteChild: static (state, delete) => delete.ChildSlotName.Match(
                Some: slot => IO.lift(() => Refused.Unless(state.Target.DeleteChild(slot, state.Context), nameof(RenderContent.DeleteChild))),
                None: () => IO.lift(() => state.Target.DeleteAllChildren(state.Context))),
            childSlot: static (state, slot) => WithinContentChange(state.Target, state.Context, Slotted(state.Target, state.Context, slot)),
            replace: static (state, replace) =>
                from owner in IO.lift(() => Optional(state.Target.DocumentOwner).ToFin(new Unattached(state.Target.Id)))
                from replaced in WithCreated(owner, replace.Source, replacement => Replaced(state.Target, replacement))
                select replaced,
            ungroup: static (state, ungroup) => IO.lift(() => ungroup.Mode switch {
                UngroupMode.Ungroup => Refused.Unless(state.Target.Ungroup(), nameof(RenderContent.Ungroup)),
                UngroupMode.UngroupRecursive => Refused.Unless(state.Target.UngroupRecursive(), nameof(RenderContent.UngroupRecursive)),
                UngroupMode.SmartUngroupRecursive => Refused.Unless(state.Target.SmartUngroupRecursive(), nameof(RenderContent.SmartUngroupRecursive)),
            }),
            saveToFile: static (state, save) =>
                from path in Answers.QualifiedPath(save.Path)
                from saved in IO.lift(() => Refused.Unless(state.Target.SaveToFile(path, save.Embed), nameof(RenderContent.SaveToFile)))
                select saved);

    private static IO<Unit> Slotted(RenderContent target, RenderContent.ChangeContexts cc, ContentOp.ChildSlot slot) =>
        IO.lift(() => {
            _ = slot.On.Iter(on => target.SetChildSlotOn(slot.Name, on, cc));
            _ = slot.Amount.Iter(amount => target.SetChildSlotAmount(slot.Name, amount, cc));
        });

    private static IO<Unit> Replaced(RenderContent target, RenderContent replacement) =>
        IO.lift(() => Refused.Unless(target.Replace(replacement), nameof(RenderContent.Replace)));

    // --- [ICONS]
    public static IO<TValue> WithIcon<TValue>(RenderContent content, Size size, IconKind kind, Func<Option<Bitmap>, IO<TValue>> body) =>
        Disposal.Bracketed(IO.lift(() => Rendered(content, size, kind)), static icon => Disposal.Release(icon.ToSeq()), body);

    private static Option<Bitmap> Rendered(RenderContent content, Size size, IconKind kind) =>
        kind.Switch(
            (Content: content, Size: size),
            standard: static (state, _) => Answers.Found(state.Content.Icon(state.Size, out Bitmap bitmap), bitmap),
            @virtual: static (state, _) => Answers.Found(state.Content.VirtualIcon(state.Size, out Bitmap bitmap), bitmap),
            dynamicIcon: static (state, dynamic) => Answers.Found(state.Content.DynamicIcon(state.Size, out Bitmap bitmap, dynamic.Usage), bitmap));
}

[Mapper]
internal static partial class ContentMapper {
    [MapPropertyFromSource(nameof(ContentState.Kind), Use = nameof(@Contents.KindOf))]
    [MapPropertyFromSource(nameof(ContentState.IsReference), Use = nameof(IsReference))]
    [MapPropertyFromSource(nameof(ContentState.UseCount), Use = nameof(UseCount))]
    [MapPropertyFromSource(nameof(ContentState.Slots), Use = nameof(Slots))]
    internal static partial ContentState ToState(RenderContent content);

    [MapProperty(nameof(RenderContentType.Id), nameof(ContentTypeState.TypeId))]
    internal static partial ContentTypeState ToState(RenderContentType type);

    [UserMapping]
    private static Option<Guid> Parent(RenderContent? parent) => Optional(parent).Map(static present => present.Id);

    private static bool IsReference(RenderContent content) => content.IsReference();

    private static int UseCount(RenderContent content) => content.UseCount();

    private static Seq<SlotState> Slots(RenderContent content) =>
        Contents.Children(content)
            .Map(child => new SlotState(child.ChildSlotName, child.Id, child.ChildSlotDisplayName, content.ChildSlotOn(child.ChildSlotName), content.ChildSlotAmount(child.ChildSlotName)))
            .Strict();
}
