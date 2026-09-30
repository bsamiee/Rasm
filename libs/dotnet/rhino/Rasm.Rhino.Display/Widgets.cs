using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetScope {
    public sealed record AllDocuments() : WidgetScope;

    public sealed record InDocument(RhinoDoc Doc, Guid Group) : WidgetScope;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Widgets {
    public static IO<Unit> Register(UserInterfaceObjectBase item, WidgetScope scope) =>
        IO.lift(() => scope.Switch(
            item,
            allDocuments: static (widget, _) => Refused.Unless(widget.RegisterForAllDocuments(), nameof(UserInterfaceObjectBase.RegisterForAllDocuments)),
            inDocument: static (widget, bound) => Refused.Unless(bound.Doc.ViewUserInterface.Add(widget, bound.Group), nameof(ViewUserInterfaceTable.Add))));
}
