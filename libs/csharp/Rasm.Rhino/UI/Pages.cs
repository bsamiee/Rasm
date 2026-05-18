using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
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
        RhinoUi.Protect(valid: Apply).Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override void OnCancel() =>
        _ = RhinoUi.Protect(valid: Cancel);

    public sealed override bool OnActivate(bool active) =>
        RhinoUi.Protect(valid: () => Activate(active: active)).Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) =>
        RhinoUi.Protect(valid: () => Script(document: doc, mode: mode)).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    protected virtual Fin<Unit> Apply() =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Cancel() =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Activate(bool active) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Result> Script(RhinoDoc document, RunMode mode) =>
        Fin.Succ(value: Result.Success);

    private static string Title(string englishTitle) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        return englishTitle;
    }
}

public abstract class RasmPropertiesPage : global::Rhino.UI.ObjectPropertiesPage {
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
        RhinoUi.Protect(valid: () => Display(args: e)).Match(Succ: static value => value, Fail: static _ => false);

    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => Update(args: e));

    public sealed override bool OnActivate(bool active) =>
        RhinoUi.Protect(valid: () => Activate(active: active)).Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        RhinoUi.Protect(valid: () => Script(args: e)).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    protected Fin<Unit> Modify(Func<global::Rhino.UI.ObjectPropertiesPageEventArgs, Fin<Unit>> change) =>
        Optional(change)
            .ToFin(Fail: Op.Of(name: nameof(Modify)).InvalidInput())
            .Bind(valid => RhinoUi.Protect(valid: () => {
                Fin<Unit> result = Fin.Fail<Unit>(error: Op.Of(name: nameof(Modify)).InvalidResult());
                ModifyPage(callbackAction: args => result = valid(arg: args));
                return result;
            }));

    protected virtual Fin<bool> Display(global::Rhino.UI.ObjectPropertiesPageEventArgs args) =>
        Optional(args)
            .ToFin(Fail: Op.Of(name: nameof(Display)).InvalidInput())
            .Map(valid => valid.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported));

    protected virtual Fin<Unit> Update(global::Rhino.UI.ObjectPropertiesPageEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Activate(bool active) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Result> Script(global::Rhino.UI.ObjectPropertiesPageEventArgs args) =>
        Fin.Succ(value: Result.Success);
}
