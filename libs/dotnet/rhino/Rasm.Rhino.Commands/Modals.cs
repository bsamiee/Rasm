using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FileNameMethod {
    public sealed record GetFileName(Option<string> Title) : FileNameMethod;

    public sealed record GetFileNameScripted() : FileNameMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Modals {
    // --- [PICKS]
    public static IO<ViewportIdentity> GetView(string prompt) =>
        IO.lift(() => Answers.FromResult(RhinoGet.GetView(prompt, out RhinoView view), nameof(RhinoGet.GetView))
            .Bind(_ => Missing.Unless(view, nameof(RhinoGet.GetView)))
            .Map(static found => Viewports.Identity(found, found.MainViewport)));

    public static IO<Seq<ViewportIdentity>> GetViewports(string prompt) =>
        IO.lift(() => Answered(RhinoGet.GetViewports(prompt, out RhinoViewport[] found), toSeq(found), nameof(RhinoGet.GetViewports))
            .Bind(static viewports => viewports
                .Traverse(static viewport => from view in Optional(viewport.ParentView) select Viewports.Identity(view, viewport))
                .As()
                .ToFin(new InvalidAnswer(nameof(RhinoViewport.ParentView)))));

    // --- [VALUES]
    public static IO<Color> GetColor(string prompt, bool acceptNothing, Color defaultValue) =>
        IO.lift(() => {
            Color value = defaultValue;
            return Answered(RhinoGet.GetColor(prompt, acceptNothing, ref value), value, nameof(RhinoGet.GetColor));
        });

    public static IO<bool> GetBool(string prompt, bool acceptNothing, LocalizeStringPair off, LocalizeStringPair on, bool defaultValue) =>
        Getters.GetOption(new GetterRequest<GetOption, bool>(prompt) {
            Accept = new() { Nothing = Answers.Found(acceptNothing, IO.pure(defaultValue)) },
            Options = [new OptionSpec<bool>.Simple(on, None, Hidden: false, IO.pure(value: true)), new OptionSpec<bool>.Simple(off, None, Hidden: false, IO.pure(value: false))],
            Configure = getter => when(acceptNothing, IO.lift(() => getter.SetDefaultString(defaultValue ? on.Local : off.Local))).As(),
        });

    public static IO<double> GetAngle(string prompt, Point3d basePoint, Point3d reference, double defaultRadians) =>
        IO.lift(() => Answered(RhinoGet.GetAngle(prompt, basePoint, reference, defaultRadians, out double angle), angle, nameof(RhinoGet.GetAngle)));

    public static IO<double> GetDistance(string prompt, double defaultDistance) =>
        IO.lift(() => Answered(RhinoGet.GetDistance(prompt, defaultDistance, out double distance), distance, nameof(RhinoGet.GetDistance)));

    public static IO<string> GetFileName(GetFileNameMode mode, string defaultName, FileNameMethod method) =>
        IO.lift(() => Answers.Present(method.Switch(
                (Mode: mode, DefaultName: defaultName),
                getFileName: static (state, dialog) => RhinoGet.GetFileName(state.Mode, state.DefaultName, dialog.Title.ValueUnsafe(), parent: null),
                getFileNameScripted: static (state, _) => RhinoGet.GetFileNameScripted(state.Mode, state.DefaultName)))
            .ToFin(new Canceled()));

    // --- [SHAPES]
    public static IO<Plane> GetPlane() =>
        IO.lift(static () => Answered(RhinoGet.GetPlane(out Plane plane), plane, nameof(RhinoGet.GetPlane)));

    public static IO<Seq<Point3d>> GetRectangle(Option<string> firstPrompt) =>
        IO.lift(() => firstPrompt.Match(
            Some: static prompt => Answered(RhinoGet.GetRectangle(prompt, out Point3d[] corners), toSeq(corners), nameof(RhinoGet.GetRectangle)),
            None: static () => Answered(RhinoGet.GetRectangle(out Point3d[] corners), toSeq(corners), nameof(RhinoGet.GetRectangle))));

    public static IO<Box> GetBox() =>
        IO.lift(static () => Answered(RhinoGet.GetBox(out Box box), box, nameof(RhinoGet.GetBox)));

    public static IO<Line> GetLine() =>
        IO.lift(static () => Answered(RhinoGet.GetLine(out Line line), line, nameof(RhinoGet.GetLine)));

    public static IO<Polyline> GetPolyline() =>
        IO.lift(static () => Answered(RhinoGet.GetPolyline(out Polyline polyline), polyline, nameof(RhinoGet.GetPolyline)));

    public static IO<Arc> GetArc() =>
        IO.lift(static () => Answered(RhinoGet.GetArc(out Arc arc), arc, nameof(RhinoGet.GetArc)));

    public static IO<Circle> GetCircle() =>
        IO.lift(static () => Answered(RhinoGet.GetCircle(out Circle circle), circle, nameof(RhinoGet.GetCircle)));

    // --- [ANSWERS]
    private static Fin<T> Answered<T>(Result result, T value, string member) =>
        Answers.FromResult(result, member).Map(_ => value);
}
