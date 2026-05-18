using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
    protected enum OptionsPageOperation { Apply, Cancel, Activate, Script }

    protected readonly record struct OptionsPageContext(OptionsPageOperation Operation, bool Active = false, RhinoDoc? Document = null, RunMode Mode = RunMode.Interactive);

    private readonly Control control;
    private readonly bool showApplyButton;

    protected RasmOptionsPage(string englishTitle, Control control, bool showApplyButton = true) : base(Title(englishTitle: englishTitle)) {
        ArgumentNullException.ThrowIfNull(argument: control);
        this.control = control;
        this.showApplyButton = showApplyButton;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;

    public sealed override bool ShowApplyButton => showApplyButton;

    public sealed override bool OnApply() =>
        RhinoUi.Protect(valid: () => Change(context: new OptionsPageContext(Operation: OptionsPageOperation.Apply))).Match(Succ: static result => result == Result.Success, Fail: static _ => false);

    public sealed override void OnCancel() =>
        _ = RhinoUi.Protect(valid: () => Change(context: new OptionsPageContext(Operation: OptionsPageOperation.Cancel)));

    public sealed override bool OnActivate(bool active) =>
        RhinoUi.Protect(valid: () => Change(context: new OptionsPageContext(Operation: OptionsPageOperation.Activate, Active: active))).Match(Succ: static result => result == Result.Success, Fail: static _ => false);

    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) =>
        RhinoUi.Protect(valid: () => Change(context: new OptionsPageContext(Operation: OptionsPageOperation.Script, Document: doc, Mode: mode))).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    protected virtual Fin<Result> Change(OptionsPageContext context) =>
        Fin.Succ(value: Result.Success);

    private static string Title(string englishTitle) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        return englishTitle;
    }
}

public abstract class RasmPropertiesPage : global::Rhino.UI.ObjectPropertiesPage {
    protected enum PropertiesPageOperation { Display, Update, Activate, Script }

    protected readonly record struct PropertiesPageContext(PropertiesPageOperation Operation, global::Rhino.UI.ObjectPropertiesPageEventArgs? Args = null, bool Active = false);

    private readonly Control control;
    private readonly string englishTitle;
    private readonly bool allObjectsMustBeSupported;
    private readonly bool supportsSubObjects;
    private readonly ObjectType supportedTypes;

    protected RasmPropertiesPage(
        string englishTitle,
        Control control,
        ObjectType supportedTypes = ObjectType.AnyObject,
        bool allObjectsMustBeSupported = false,
        bool supportsSubObjects = false) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.englishTitle = englishTitle;
        this.control = control;
        this.supportedTypes = supportedTypes;
        this.allObjectsMustBeSupported = allObjectsMustBeSupported;
        this.supportsSubObjects = supportsSubObjects;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;

    public sealed override string EnglishPageTitle => englishTitle;

    public sealed override ObjectType SupportedTypes => supportedTypes;

    public sealed override bool AllObjectsMustBeSupported => allObjectsMustBeSupported;

    public sealed override bool SupportsSubObjects => supportsSubObjects;

    public sealed override bool ShouldDisplay(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        RhinoUi.Protect(valid: () => Change(context: new PropertiesPageContext(Operation: PropertiesPageOperation.Display, Args: e))).Match(Succ: static result => result == Result.Success, Fail: static _ => false);

    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => Change(context: new PropertiesPageContext(Operation: PropertiesPageOperation.Update, Args: e)));

    public sealed override bool OnActivate(bool active) =>
        RhinoUi.Protect(valid: () => Change(context: new PropertiesPageContext(Operation: PropertiesPageOperation.Activate, Active: active))).Match(Succ: static result => result == Result.Success, Fail: static _ => false);

    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        RhinoUi.Protect(valid: () => Change(context: new PropertiesPageContext(Operation: PropertiesPageOperation.Script, Args: e))).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    protected Fin<Unit> Modify(Func<global::Rhino.UI.ObjectPropertiesPageEventArgs, Fin<Unit>> change) =>
        Optional(change)
            .ToFin(Fail: Op.Of(name: nameof(Modify)).InvalidInput())
            .Bind(valid => RhinoUi.Protect(valid: () => {
                Fin<Unit> result = Fin.Fail<Unit>(error: Op.Of(name: nameof(Modify)).InvalidResult());
                ModifyPage(callbackAction: args => result = valid(arg: args));
                return result;
            }));

    protected virtual Fin<Result> Change(PropertiesPageContext context) =>
        context.Operation switch {
            PropertiesPageOperation.Display => Optional(context.Args)
                .ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput())
                .Map(valid => valid.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported) switch {
                    true => Result.Success,
                    false => Result.Cancel,
                }),
            _ => Fin.Succ(value: Result.Success),
        };
}
