using Eto.Drawing;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Canvas;
using Rasm.Rhino;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Policies {
    // --- [CANVASES]
    public static IO<Unit> MakeReadOnly(Canvas canvas) =>
        IO.lift(() => {
            _ = toSeq(typeof(CanvasActions).GetProperties())
                .Filter(static property => property.PropertyType == typeof(bool))
                .Iter(property => property.SetValue(canvas.AllowedActions, value: false));
            (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups) = (false, false, false);
        });

    public static IO<Unit> RestrictWires(Canvas canvas, Func<IParameter, IParameter, bool> permits, IPlugInSink sink) =>
        IO.lift(() => {
            CallbackSite site = new(sink, typeof(CanvasActions), nameof(CanvasActions.MakeWireFilter));
            canvas.AllowedActions.MakeWireFilter = wire => Callbacks.Answer(IO.lift(() => permits(wire.source, wire.target)), static () => false, site);
            canvas.AllowedActions.AllowModifyWire = false;
        });

    // --- [EDITING]
    public static IO<Unit> ShowInlineEditor<T, TKey>(
        Canvas canvas, RectangleF frame, T current, ITypeAssistant<TKey> assistant, Func<T, IO<Unit>> assign, IPlugInSink sink)
        where T : IObjectFactory<T, TKey, ValidationError>, IConvertible<TKey>
        where TKey : notnull {
        CallbackSite site = new(sink, typeof(Canvas), nameof(Canvas.ShowInlineEditor));
        return IO.lift(() => {
            Pear<TKey> existing = Garden.Pear(current.ToValue());
            canvas.ShowInlineEditor(
                frame,
                assistant.Format(existing.Item),
                text => Callbacks.Answer(text, Entered, () => Result<TKey>.Fail("", text), site),
                () => _ = Callbacks.Answer(current, assign, static () => unit, site));

            IO<IResult> Entered(string text) =>
                from parsed in IO.lift(() => assistant.Parse(constructor: null, text, existing))
                from answer in parsed.Success
                    ? Admission.Validated<T, TKey>(parsed.Value).Match(
                        Succ: value => assign(value).Map((IResult)parsed),
                        Fail: error => IO.pure<IResult>(Result<TKey>.Fail(ErrorOps.Localize(error), text)))
                    : IO.pure<IResult>(parsed)
                select answer;
        });
    }
}
