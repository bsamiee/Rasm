using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
    private readonly Control control;
    private readonly bool showApplyButton;

    protected RasmOptionsPage(string englishTitle, Control control, bool showApplyButton = true) : base(englishTitle) {
        ArgumentNullException.ThrowIfNull(argument: control);
        this.control = control;
        this.showApplyButton = showApplyButton;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;

    public sealed override bool ShowApplyButton => showApplyButton;

    public sealed override bool OnApply() =>
        Apply().Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override void OnCancel() =>
        _ = Cancel();

    public sealed override bool OnActivate(bool active) =>
        Activate(active: active).Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) =>
        Script(document: doc, mode: mode).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    protected virtual Fin<Unit> Apply() =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Cancel() =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Activate(bool active) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Result> Script(RhinoDoc document, RunMode mode) =>
        Fin.Succ(value: Result.Success);
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
        Display(args: e).Match(Succ: static value => value, Fail: static _ => false);

    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        _ = Update(args: e);

    public sealed override bool OnActivate(bool active) =>
        Activate(active: active).Match(Succ: static _ => true, Fail: static _ => false);

    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        Script(args: e).Match(Succ: static result => result, Fail: static _ => Result.Failure);

    public Fin<Unit> Modify(Action<global::Rhino.UI.ObjectPropertiesPageEventArgs> change) =>
        Optional(change)
            .ToFin(Fail: Op.Of(name: nameof(Modify)).InvalidInput())
            .Map(valid => {
                ModifyPage(valid);
                return unit;
            });

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
