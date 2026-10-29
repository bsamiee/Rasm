using System.Collections;
using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino.Objects;

// --- [SERVICES] ------------------------------------------------------------------------
internal static class Internals {
    // --- [NAMES]
    private const string ModelType = "Commands.ViewModels.NamedSelectionsViewModel, Commands";
    private const string ItemType = "Commands.ViewModels.NsGridItem, Commands";
    private const string SelectionType = "Commands.ViewModels.NamedSelection, Commands";
    private const string StandardType = "Commands.ViewModels.NsStandardObject, Commands";
    private const string PartType = "Commands.ViewModels.NsSubObject, Commands";
    private const string ChildType = "Commands.ViewModels.NsGridChildObject, Commands";
    private const string CollectionType = "Eto.Forms.TreeGridItemCollection, Eto";
    private const string StandardList = "System.Collections.Generic.List`1[[" + StandardType + "]]";
    private const string PartList = "System.Collections.Generic.List`1[[" + PartType + "]]";
    private const string ChildList = "System.Collections.Generic.List`1[[" + ChildType + "]]";
    internal const string Collection = "TreeGridCollection";

    // --- [NAMED_SELECTIONS]
    internal static IO<object> SelectionModel(RhinoDoc doc) =>
        from type in IO.lift(static () => Conversions.Required(Type.GetType(ModelType)))
        from model in IO.lift(() => doc.RuntimeData.GetValue(type, static owner => Created(owner.RuntimeSerialNumber)))
        from read in unless(IsDocumentEventWatchingEnabled(model), IO.lift(() => ReadSavedNamedSelectionArray(model))).As()
        select model;

    internal static Seq<NamedSelection> Selections(object model) =>
        Items(TreeGridCollection(model)).Map(static item => NamedSelectionOf(item)).Map(static selection => new NamedSelection(
            SelectionId(selection),
            SelectionName(selection),
            Items(StandardObjectTypes(selection)).Bind(static standard => Live(Items(StandardObjects(standard)))).Map(ChildId).Distinct().Strict(),
            Items(SubObjects(selection)).Bind(static part => Live(Items(SubObjectChildren(part)))).Map(static child => (ObjectId: ChildId(child), Component: ChildComponent(child))).Strict())).Strict();

    private static Seq<object> Items(object list) => toSeq(((IEnumerable)list).Cast<object>());

    private static Seq<object> Live(Seq<object> children) => children.Filter(static child => !ChildDeleted(child));

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ModelType)]
    private static extern object Created(uint documentRuntimeSerial);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsDocumentEventWatchingEnabled")]
    private static extern bool IsDocumentEventWatchingEnabled([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ReadSavedNamedSelectionArray))]
    private static extern void ReadSavedNamedSelectionArray([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_" + Collection)]
    [return: UnsafeAccessorType(CollectionType)]
    private static extern object TreeGridCollection([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_NamedSelection")]
    [return: UnsafeAccessorType(SelectionType)]
    private static extern object NamedSelectionOf([UnsafeAccessorType(ItemType)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid SelectionId([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Name")]
    private static extern string SelectionName([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StandardObjectTypes")]
    [return: UnsafeAccessorType(StandardList)]
    private static extern object StandardObjectTypes([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SubObjects")]
    [return: UnsafeAccessorType(PartList)]
    private static extern object SubObjects([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Objects")]
    [return: UnsafeAccessorType(ChildList)]
    private static extern object StandardObjects([UnsafeAccessorType(StandardType)] object standard);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SubObjectChildren")]
    [return: UnsafeAccessorType(ChildList)]
    private static extern object SubObjectChildren([UnsafeAccessorType(PartType)] object part);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid ChildId([UnsafeAccessorType(ChildType)] object child);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsDeleted")]
    private static extern bool ChildDeleted([UnsafeAccessorType(ChildType)] object child);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ComponentIndex")]
    private static extern ComponentIndex ChildComponent([UnsafeAccessorType(ChildType)] object child);
}
