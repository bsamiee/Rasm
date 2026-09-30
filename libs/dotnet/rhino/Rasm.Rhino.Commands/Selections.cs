using Rasm.Rhino.Document;
using Rhino;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Selections {
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, ViewportTarget target, Func<PickContext, IO<Unit>> configure) =>
        from view in DisposalOps.Using(Viewports.ResolveViewport(doc, target), static row => IO.pure(row.View))
        from captures in DisposalOps.Using(static () => new PickContext(), context =>
            from viewed in IO.lift(() => context.View = view)
            from configured in configure(context)
            from captures in DisposalOps.Using(IO.lift(() => toSeq(doc.Objects.PickObjects(context))), static references => references.TraverseM(Captures.Capture).As())
            select captures)
        select captures;
}
