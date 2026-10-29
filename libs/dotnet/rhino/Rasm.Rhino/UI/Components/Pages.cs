using Rasm.Rhino.UI.Rows;
using Rhino.DocObjects;

namespace Rasm.Rhino.UI.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Pages {
    // --- [PREFERENCES]
    public static View.Page Preferences(Type key, string caption, PageDialog dialog, params Seq<Row> rows) =>
        new() { Identity = key, Caption = caption, Kind = dialog, Commit = new CommitMode.Immediate(), Children = [ControlRows.Fixed(rows)] };

    // --- [PROPERTIES]
    public static View.PropertiesPage Properties<TObject>(Type key, string caption, ObjectType supported, Func<Seq<TObject>, Seq<Row>> rows) where TObject : RhinoObject =>
        new() {
            Identity = key,
            Caption = caption,
            Subject = new PageSubject.Objects { SupportedTypes = supported },
            Children = [new ControlRows(scope => Selected<TObject>(scope).Map(rows), static _ => IO.pure(Seq<HostEvent<Unit>>()))],
        };

    private static IO<Seq<TObject>> Selected<TObject>(RowScope scope) where TObject : RhinoObject =>
        scope.Mode.Map(static mode => Optional(mode as CommitMode.Targets).ToSeq().Bind(static targets => toSeq(targets.Page.GetSelectedObjects<TObject>())).Strict());
}
