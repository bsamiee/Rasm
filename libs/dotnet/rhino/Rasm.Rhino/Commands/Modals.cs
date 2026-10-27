using System.Drawing;
using Rasm.RhinoCommon.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.PlugIns;

namespace Rasm.RhinoCommon.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ClientRectangle(Rectangle Client, ViewportTarget Viewport);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Modals {
    // --- [VIEWS]
    public static IO<ViewportTarget> GetView(string prompt) =>
        IO.lift(() => Results.Ended(RhinoGet.GetView(prompt, out RhinoView view)).Bind(_ => Target(view)));

    public static IO<Seq<ViewportTarget>> GetViewports(string prompt) =>
        (from viewports in IO.lift(() => Results.Ended(RhinoGet.GetViewports(prompt, out RhinoViewport[] chosen)).Map(_ => chosen))
         from held in Products.Made(viewports)
         select held.Map(static viewport => (ViewportTarget)new ViewportTarget.Id(viewport.Id)).Strict()).Bracket();

    public static IO<ClientRectangle> Get2dRectangle(bool solidPen) =>
        IO.lift(() => Results.Ended(RhinoGet.Get2dRectangle(solidPen, out Rectangle rectangle, out RhinoView view)).Bind(_ => Target(view)).Map(target => new ClientRectangle(rectangle, target)));

    private static Fin<ViewportTarget> Target(RhinoView? view) =>
        Conversions.Found(view).Map(static found => (ViewportTarget)new ViewportTarget.Id(found.ActiveViewportID));

    // --- [VALUES]
    public static IO<string> GetFileName(RunMode runMode, GetFileNameMode mode, string defaultName, string title, Option<RhinoGet.BitmapFileTypes> fileTypes, Option<object> parent = default) =>
        IO.lift(() => Conversions.Present(runMode switch {
            RunMode.Interactive => RhinoGet.GetFileName(mode, defaultName, title, parent.ValueUnsafe(), fileTypes.IfNone(static () => RhinoGet.AllBitmapFileTypes)),
            RunMode.Scripted => RhinoGet.GetFileNameScripted(mode, defaultName),
        }).ToFin(Errors.Cancelled));

    public static IO<bool> GetBool(RhinoDoc doc, PlugIn plugIn, string prompt, LabelPair labels, Option<bool> defaultValue) =>
        Getters.Choice(doc, new GetterRequest<GetOption, bool>(prompt, plugIn) {
            Accept = new() { Nothing = defaultValue.Map(value => new NothingEntry<bool>(Some(value ? labels.On : labels.Off), IO.pure(value))) },
            Options = Seq(a: true, b: false).Map(value => OptionSpecs.Plain(value ? labels.On : labels.Off, None, hidden: false, _ => IO.pure(value))),
        });
}
