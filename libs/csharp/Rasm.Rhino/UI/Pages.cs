using System.Runtime.InteropServices;
using Eto.Forms;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum PageHost { Options, DocumentProperties, ObjectProperties }

[Union(SwitchMapStateParameterName = "page")]
public abstract partial record PageNav {
    private PageNav() { }
    public sealed record Activate : PageNav;
    public sealed record Reveal(string PageName, bool DocumentProperties = false) : PageNav;
    public sealed record Remove : PageNav;
    public sealed record Dirty(bool Modified = true) : PageNav;
    public sealed record Child(global::Rhino.UI.StackedDialogPage Page) : PageNav;
    public sealed record Appearance(bool Bold, DrawingColor Color) : PageNav;
    public sealed record Sequence(Seq<PageNav> Steps) : PageNav;

    internal Fin<Unit> Apply(global::Rhino.UI.StackedDialogPage page) =>
        RhinoUi.Protect(valid: () => Switch(
            page,
            activate: static (p, _) => Fin.Succ(value: Op.Side(p.MakeActivePage)),
            reveal: static (p, g) => p.SetActivePageTo(pageName: g.PageName, documentPropertiesPage: g.DocumentProperties)   // bool return distinguishes a missing page name from a successful navigation
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: Op.Of(name: nameof(Reveal)).Caution(concern: $"No page named '{g.PageName}' found in stacked dialog.")),
            remove: static (p, _) => Fin.Succ(value: Op.Side(p.RemovePage)),
            dirty: static (p, d) => Fin.Succ(value: Op.Side(() => p.Modified = d.Modified)),
            child: static (p, c) => WindowsOnly(op: Op.Of(name: nameof(Child)), run: () => p.AddChildPage(pageToAdd: c.Page)),
            appearance: static (p, a) => WindowsOnly(op: Op.Of(name: nameof(Appearance)), run: () => { p.NavigationTextIsBold = a.Bold; p.NavigationTextColor = a.Color; }),
            sequence: static (p, g) => g.Steps.TraverseM(step => step.Apply(page: p)).As().Map(static _ => unit)));
    private static Fin<Unit> WindowsOnly(Op op, Action run) =>
        OperatingSystem.IsWindows()
            ? Fin.Succ(value: Op.Side(run))
            : Fin.Fail<Unit>(error: op.Caution(concern: "stacked-dialog navigation member is Windows-only"));
}

[SmartEnum<int>]
public sealed partial class PagePhase {
    public static readonly PagePhase Apply = new(0);
    public static readonly PagePhase Cancel = new(1);
    public static readonly PagePhase Activate = new(2);
    public static readonly PagePhase Script = new(3);
    public static readonly PagePhase Display = new(4);
    public static readonly PagePhase Update = new(5);
    public static readonly PagePhase Modify = new(6);
    public static readonly PagePhase Defaults = new(7);
    public static readonly PagePhase Help = new(8);
    public static readonly PagePhase CreateParent = new(9);
    public static readonly PagePhase SizeParent = new(10);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct PageObjectContext(global::Rhino.UI.ObjectPropertiesPageEventArgs Args);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PageParentContext(IntPtr Handle);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PageScriptContext(RhinoDoc Document, RunMode Mode);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PageSizeContext(int Width, int Height);

[Union]
public abstract partial record PageEvent {
    private PageEvent() { }
    public abstract PagePhase Phase { get; }
    public sealed record Apply(bool Active) : PageEvent { public override PagePhase Phase => PagePhase.Apply; }
    public sealed record Cancel : PageEvent { public override PagePhase Phase => PagePhase.Cancel; }
    public sealed record Activate(bool Active) : PageEvent { public override PagePhase Phase => PagePhase.Activate; }
    public sealed record Script(PageScriptContext Context) : PageEvent { public override PagePhase Phase => PagePhase.Script; }
    public sealed record ObjectEvent(PagePhase Kind, PageObjectContext Context, Option<PageScriptContext> ObjectScript) : PageEvent { public override PagePhase Phase => Kind; }
    public sealed record Defaults : PageEvent { public override PagePhase Phase => PagePhase.Defaults; }
    public sealed record Help : PageEvent { public override PagePhase Phase => PagePhase.Help; }
    public sealed record Parent(PageParentContext Context) : PageEvent { public override PagePhase Phase => PagePhase.CreateParent; }
    public sealed record Size(PageSizeContext Context) : PageEvent { public override PagePhase Phase => PagePhase.SizeParent; }

    public static PageEvent Scripted(RhinoDoc document, RunMode mode) =>
        new Script(Context: new PageScriptContext(Document: document, Mode: mode));

    public static PageEvent ObjectPage(PagePhase phase, global::Rhino.UI.ObjectPropertiesPageEventArgs args, RunMode mode = RunMode.Interactive) {
        ArgumentNullException.ThrowIfNull(args);
        return new ObjectEvent(Kind: phase, Context: new PageObjectContext(Args: args), ObjectScript: Optional(args.Document).Map(document => new PageScriptContext(Document: document, Mode: mode)));
    }

    public Option<global::Rhino.UI.ObjectPropertiesPageEventArgs> Args =>
        this is ObjectEvent { Context.Args: var args } ? Some(args) : Option<global::Rhino.UI.ObjectPropertiesPageEventArgs>.None;

    public Option<PageScriptContext> ScriptContext =>
        this switch {
            Script script => Some(script.Context),
            ObjectEvent obj => obj.ObjectScript,
            _ => Option<PageScriptContext>.None,
        };

    public Option<uint> EventSerialNumber => Args.Map(static args => args.EventRuntimeSerialNumber);
    public Option<uint> DocumentSerialNumber => Args.Map(static args => args.DocRuntimeSerialNumber) | ScriptContext.Map(static context => context.Document.RuntimeSerialNumber);
    public Option<global::Rhino.UI.ObjectPropertiesPage> Page => Args.Bind(static args => Optional(args.Page));
    public Option<RhinoView> View => Args.Bind(static args => Optional(args.View));
    public Option<RhinoViewport> Viewport => Args.Bind(static args => Optional(args.Viewport));
    public int ObjectCount => Args.Map(static args => args.ObjectCount).IfNone(0);
    public ObjectType ObjectTypes => Args.Map(static args => (ObjectType)args.ObjectTypes).IfNone(ObjectType.None);
    public Option<IntPtr> ParentHandle => this is Parent parent ? Some(parent.Context.Handle) : Option<IntPtr>.None;
    public Option<(int Width, int Height)> ParentSize => this is Size size ? Some((size.Context.Width, size.Context.Height)) : Option<(int Width, int Height)>.None;

    public Fin<RhinoDoc> RequireDocument() =>
        (ScriptContext.Map(static context => context.Document).Bind(Optional) | Args.Bind(static args => Optional(args.Document)))
        .ToFin(Fail: Op.Of(name: nameof(RequireDocument)).InvalidInput());

    public Fin<Seq<TObject>> Objects<TObject>() where TObject : RhinoObject => Op.Of(name: nameof(Objects)).Need(Args).Map(static args => toSeq(args.GetObjects<TObject>()));
    public Fin<Seq<RhinoObject>> Objects(ObjectType filter) => Op.Of(name: nameof(Objects)).Need(Args).Map(args => toSeq(args.GetObjects(filter: filter)));
    public Fin<Seq<Guid>> ObjectIds() => Objects<RhinoObject>().Map(static objects => objects.Map(static value => value.Id));
    public Fin<Seq<(Guid Id, ObjectType Type)>> Snapshot(ObjectType filter = ObjectType.AnyObject) =>
        Objects(filter: filter).Map(static objects => objects.Map(static native => (native.Id, native.ObjectType)));
    public Fin<RhinoUi> Ui() {
        RunMode mode = ScriptContext.Map(static context => context.Mode).IfNone(RunMode.Interactive);
        return RequireDocument().Map(document => new RhinoUi(document: document, mode: mode));
    }
}

public readonly record struct PageMetadata(
    Option<string> LocalTitle = default,
    Option<string> IconResource = default,
    int Index = -1,
    Option<global::Rhino.UI.PropertyPageType> PageType = default) {
    public static PageMetadata Replacing(global::Rhino.UI.PropertyPageType pageType, Option<string> localTitle = default, Option<string> iconResource = default, int index = -1) =>
        new(LocalTitle: localTitle, IconResource: iconResource, Index: index, PageType: Some(pageType));
}

public sealed record PageRegistration<TPage>(TPage Page, PageHost Host) where TPage : class {
    public Fin<Unit> Add(ICollection<TPage> pages) =>
        from page in Op.Of(name: nameof(Add)).Need(Page)
        from validPages in Op.Of(name: nameof(Add)).Need(pages)
        from added in Host switch {
            PageHost.Options or PageHost.DocumentProperties => RhinoUi.Protect(valid: () => {
                validPages.Add(item: page);
                return Fin.Succ(value: unit);
            }),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Add)).InvalidInput()),
        }
        select added;

    public Fin<Unit> Add(global::Rhino.UI.ObjectPropertiesPageCollection pages) =>
        (Page, pages, Host) switch {
            (global::Rhino.UI.ObjectPropertiesPage page, global::Rhino.UI.ObjectPropertiesPageCollection collection, PageHost.ObjectProperties) => RhinoUi.Protect(valid: () => {
                collection.Add(page: page);
                return Fin.Succ(value: unit);
            }),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Add)).InvalidInput()),
        };
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
    private readonly Control control;
    private readonly bool showApplyButton;
    private readonly bool showDefaultsButton;

    protected RasmOptionsPage(string englishTitle, Control control, bool showApplyButton = true, bool showDefaultsButton = false) : base(englishTitle) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.control = control;
        this.showApplyButton = showApplyButton;
        this.showDefaultsButton = showDefaultsButton;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override bool ShowApplyButton => showApplyButton;
    public sealed override bool ShowDefaultsButton => showDefaultsButton;
    public sealed override bool OnApply() => ResultOf(pageEvent: new PageEvent.Apply(Active: false)) == Result.Success;
    public sealed override void OnCancel() => _ = ResultOf(pageEvent: new PageEvent.Cancel());
    public sealed override bool OnActivate(bool active) => ResultOf(pageEvent: new PageEvent.Activate(Active: active)) == Result.Success;
    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) => ResultOf(pageEvent: PageEvent.Scripted(document: doc, mode: mode));
    public sealed override void OnDefaults() => _ = ResultOf(pageEvent: new PageEvent.Defaults());
    public sealed override void OnHelp() => _ = ResultOf(pageEvent: new PageEvent.Help());
    public sealed override void OnCreateParent(IntPtr hwndParent) => _ = ResultOf(pageEvent: new PageEvent.Parent(Context: new PageParentContext(Handle: hwndParent)));
    public sealed override void OnSizeParent(int width, int height) => _ = ResultOf(pageEvent: new PageEvent.Size(Context: new PageSizeContext(Width: width, Height: height)));

    public Fin<Unit> Navigate(PageNav nav) => Op.Of(name: nameof(Navigate)).Need(nav).Bind(valid => valid.Apply(page: this));

    protected virtual Fin<Result> Change(PageEvent pageEvent) => Fin.Succ(value: Result.Success);

    private Result ResultOf(PageEvent pageEvent) => RhinoUi.Protect(valid: () => Change(pageEvent: pageEvent)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}

public abstract class RasmPropertiesPage : global::Rhino.UI.ObjectPropertiesPage {
    private readonly string englishTitle;
    private readonly Control control;
    private readonly ObjectType supportedTypes;
    private readonly bool allObjectsMustBeSupported;
    private readonly bool supportsSubObjects;
    private readonly PageMetadata metadata;

    protected RasmPropertiesPage(string englishTitle, Control control, ObjectType supportedTypes = ObjectType.AnyObject, bool allObjectsMustBeSupported = false, bool supportsSubObjects = false, PageMetadata metadata = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.englishTitle = englishTitle;
        this.control = control;
        this.supportedTypes = supportedTypes;
        this.allObjectsMustBeSupported = allObjectsMustBeSupported;
        this.supportsSubObjects = supportsSubObjects;
        this.metadata = metadata;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override string EnglishPageTitle => englishTitle;
    public sealed override string LocalPageTitle => metadata.LocalTitle.IfNone(englishTitle);
    public sealed override int Index => metadata.Index;
    public sealed override global::Rhino.UI.PropertyPageType PageType => metadata.PageType.IfNone(global::Rhino.UI.PropertyPageType.Custom);
    public sealed override string PageIconEmbeddedResourceString => metadata.IconResource.IfNone(string.Empty);
    public sealed override ObjectType SupportedTypes => supportedTypes;
    public sealed override bool AllObjectsMustBeSupported => allObjectsMustBeSupported;
    public sealed override bool SupportsSubObjects => supportsSubObjects;
    public sealed override bool ShouldDisplay(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        ResultOf(pageEvent: PageEvent.ObjectPage(phase: PagePhase.Display, args: e)) == Result.Success;
    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        _ = ResultOf(pageEvent: PageEvent.ObjectPage(phase: PagePhase.Update, args: e));   // native host already gated visibility; re-checking ShouldDisplay fired Change(Display) a second time per selection
    public sealed override bool OnActivate(bool active) => ResultOf(pageEvent: new PageEvent.Activate(Active: active)) == Result.Success;
    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => ResultOf(pageEvent: PageEvent.ObjectPage(phase: PagePhase.Script, args: e, mode: RunMode.Scripted));
    public sealed override void OnHelp() => _ = ResultOf(pageEvent: new PageEvent.Help());
    public sealed override void OnCreateParent(IntPtr hwndParent) => _ = ResultOf(pageEvent: new PageEvent.Parent(Context: new PageParentContext(Handle: hwndParent)));
    public sealed override void OnSizeParent(int width, int height) => _ = ResultOf(pageEvent: new PageEvent.Size(Context: new PageSizeContext(Width: width, Height: height)));

    protected Fin<Unit> Modify(Func<PageEvent, Fin<Unit>> change) =>
        Op.Of(name: nameof(Modify)).Need(change).Bind(valid => RhinoUi.Protect(valid: () => {
            Fin<Unit> result = Fin.Fail<Unit>(error: Op.Of(name: nameof(Modify)).InvalidResult());
            ModifyPage(callbackAction: args => result = valid(arg: PageEvent.ObjectPage(phase: PagePhase.Modify, args: args)));
            return result;
        }));

    protected virtual Fin<Result> Change(PageEvent pageEvent) =>
        from valid in Op.Of(name: nameof(Change)).Need(pageEvent)
        from result in valid.Phase switch {
            var p when p == PagePhase.Display =>
                Op.Of(name: nameof(Change)).Need(valid.Args)
                    .Map(args => args.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported) switch {
                        true => Result.Success,
                        false => Result.Cancel,
                    }),
            var p when p == PagePhase.Update || p == PagePhase.Modify => OnRefresh(pageEvent: valid),
            _ => Fin.Succ(value: Result.Success),
        }
        select result;

    protected virtual Fin<Result> OnRefresh(PageEvent pageEvent) => Fin.Succ(value: Result.Success);

    private Result ResultOf(PageEvent pageEvent) => RhinoUi.Protect(valid: () => Change(pageEvent: pageEvent)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}
