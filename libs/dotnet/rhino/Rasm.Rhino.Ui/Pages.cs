using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PageSelection(uint DocSerial, uint EventSerial, int ObjectCount, Seq<Guid> ObjectIds, Option<uint> ViewSerial, Option<Guid> ViewportId);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionsPageEvent {
    public sealed record Activate(bool Active) : OptionsPageEvent;

    public sealed record Apply() : OptionsPageEvent;

    public sealed record Cancel() : OptionsPageEvent;

    public sealed record RunScript(Option<RhinoDoc> Doc, RunMode Mode) : OptionsPageEvent;

    public sealed record Defaults() : OptionsPageEvent;

    public sealed record Help() : OptionsPageEvent;

    public sealed record CreateParent(nint Handle) : OptionsPageEvent;

    public sealed record SizeParent(int Width, int Height) : OptionsPageEvent;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PropertiesPageEvent {
    public sealed record Activate(bool Active) : PropertiesPageEvent;

    public sealed record Help() : PropertiesPageEvent;

    public sealed record CreateParent(nint Handle) : PropertiesPageEvent;

    public sealed record SizeParent(int Width, int Height) : PropertiesPageEvent;

    public sealed record ShouldDisplay(PageSelection Selection) : PropertiesPageEvent;

    public sealed record UpdatePage(PageSelection Selection) : PropertiesPageEvent;

    public sealed record RunScript(PageSelection Selection) : PropertiesPageEvent;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageOp {
    public sealed record MakeActivePage() : PageOp;

    public sealed record SetActivePageTo(string Name, bool DocumentProperties) : PageOp;

    public sealed record RemovePage() : PageOp;

    public sealed record Modified(bool Value) : PageOp;

    public sealed record SetEnglishPageTitle(string Title) : PageOp;

    public sealed record NavigationText(bool Bold, Option<Color> Color) : PageOp;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CallbackOptionsDialogPage : OptionsDialogPage {
    private readonly LocalizeStringPair title;
    private readonly Option<Image> image;
    private readonly Func<OptionsPageEvent, IO<Unit>> onEvent;
    private readonly Action<Error> reject;

    public CallbackOptionsDialogPage(
        LocalizeStringPair title,
        Control content,
        Option<Image> image,
        bool showApply,
        bool showDefaults,
        Seq<OptionsDialogPage> children,
        Func<OptionsPageEvent, IO<Unit>> onEvent,
        Action<Error> reject) : base(title.English) {
        content.UseRhinoStyle();
        Children.AddRange(children);
        this.title = title;
        PageControl = content;
        this.image = image;
        ShowApplyButton = showApply;
        ShowDefaultsButton = showDefaults;
        this.onEvent = onEvent;
        this.reject = reject;
    }

    public override object PageControl { get; }

    public override string LocalPageTitle => title.Local;

    public override Image PageImage => image.IfNone(() => base.PageImage);

    public override bool ShowApplyButton { get; }

    public override bool ShowDefaultsButton { get; }

    public override bool OnApply() => Answers.Succeeded(onEvent(new OptionsPageEvent.Apply()), reject);

    public override void OnCancel() => _ = Answers.Answer(onEvent(new OptionsPageEvent.Cancel()), reject, unit);

    public override bool OnActivate(bool active) => Answers.Succeeded(onEvent(new OptionsPageEvent.Activate(active)), reject);

    public override void OnDefaults() => _ = Answers.Answer(onEvent(new OptionsPageEvent.Defaults()), reject, unit);

    public override void OnHelp() => _ = Answers.Answer(onEvent(new OptionsPageEvent.Help()), reject, unit);

    public override void OnCreateParent(nint hwndParent) => _ = Answers.Answer(onEvent(new OptionsPageEvent.CreateParent(hwndParent)), reject, unit);

    public override void OnSizeParent(int width, int height) => _ = Answers.Answer(onEvent(new OptionsPageEvent.SizeParent(width, height)), reject, unit);

    public override Result RunScript(RhinoDoc doc, RunMode mode) => Answers.ToResult(onEvent(new OptionsPageEvent.RunScript(Optional(doc), mode)).RunSafe(), reject);
}

public abstract class CallbackObjectPropertiesPage : ObjectPropertiesPage {
    private readonly LocalizeStringPair title;
    private readonly Func<PageSelection, bool> display;
    private readonly Func<PropertiesPageEvent, IO<Unit>> onEvent;
    private readonly Action<Error> reject;

    protected CallbackObjectPropertiesPage(
        LocalizeStringPair title,
        int index,
        PropertyPageType pageType,
        ObjectType supportedTypes,
        bool allObjectsMustBeSupported,
        bool supportsSubObjects,
        Control content,
        Func<PageSelection, bool> display,
        Func<PropertiesPageEvent, IO<Unit>> onEvent,
        Action<Error> reject) {
        content.UseRhinoStyle();
        this.title = title;
        Index = index;
        PageType = pageType;
        SupportedTypes = supportedTypes;
        AllObjectsMustBeSupported = allObjectsMustBeSupported;
        SupportsSubObjects = supportsSubObjects;
        PageControl = content;
        this.display = display;
        this.onEvent = onEvent;
        this.reject = reject;
    }

    public sealed override object PageControl { get; }

    public sealed override string EnglishPageTitle => title.English;

    public sealed override string LocalPageTitle => title.Local;

    public sealed override int Index { get; }

    public sealed override PropertyPageType PageType { get; }

    public sealed override ObjectType SupportedTypes { get; }

    public sealed override bool AllObjectsMustBeSupported { get; }

    public sealed override bool SupportsSubObjects { get; }

    public sealed override bool OnActivate(bool active) => Answers.Succeeded(onEvent(new PropertiesPageEvent.Activate(active)), reject);

    public sealed override void OnHelp() => _ = Answers.Answer(onEvent(new PropertiesPageEvent.Help()), reject, unit);

    public sealed override void OnCreateParent(nint hwndParent) => _ = Answers.Answer(onEvent(new PropertiesPageEvent.CreateParent(hwndParent)), reject, unit);

    public sealed override void OnSizeParent(int width, int height) => _ = Answers.Answer(onEvent(new PropertiesPageEvent.SizeParent(width, height)), reject, unit);

    public sealed override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        base.ShouldDisplay(e)
        && Some(Read(e)).Filter(display).Do(selection => _ = Answers.Answer(onEvent(new PropertiesPageEvent.ShouldDisplay(selection)), reject, unit)).IsSome;

    public sealed override void UpdatePage(ObjectPropertiesPageEventArgs e) => _ = Answers.Answer(onEvent(new PropertiesPageEvent.UpdatePage(Read(e))), reject, unit);

    public sealed override Result RunScript(ObjectPropertiesPageEventArgs e) => Answers.ToResult(onEvent(new PropertiesPageEvent.RunScript(Read(e))).RunSafe(), reject);

    public IO<Seq<Guid>> Selection(ObjectType filter) =>
        IO.lift(() => toSeq(GetSelectedObjects(filter)).Map(static row => row.Id).Strict());

    public IO<Unit> Modify(Func<PageSelection, Fin<Unit>> change) =>
        IO.lift(() => Answers.Captured<Unit>(capture => ModifyPage(e => capture(change(Read(e)))), nameof(ModifyPage)));

    private PageSelection Read(ObjectPropertiesPageEventArgs e) =>
        new(
            e.DocRuntimeSerialNumber,
            e.EventRuntimeSerialNumber,
            e.ObjectCount,
            toSeq(e.GetObjects(SupportedTypes)).Map(static row => row.Id).Strict(),
            Optional(e.View).Map(static view => view.RuntimeSerialNumber),
            Optional(e.Viewport).Map(static viewport => viewport.Id));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPages {
    public static IO<Unit> Apply(StackedDialogPage page, PageOp op) =>
        op.Switch(
            page,
            makeActivePage: static (target, _) => IO.lift(target.MakeActivePage),
            setActivePageTo: static (target, active) => IO.lift(() => Refused.Unless(target.SetActivePageTo(active.Name, active.DocumentProperties), nameof(StackedDialogPage.SetActivePageTo))),
            removePage: static (target, _) => IO.lift(target.RemovePage),
            modified: static (target, modified) => IO.lift(() => { target.Modified = modified.Value; }),
            setEnglishPageTitle: static (target, retitled) => IO.lift(() => target.SetEnglishPageTitle(retitled.Title)),
            navigationText: static (target, text) =>
                from windows in IO.lift(static () => WindowsOnly.Unless(HostUtils.RunningOnWindows, nameof(StackedDialogPage.NavigationTextIsBold)))
                from styled in IO.lift(() => {
                    target.NavigationTextIsBold = text.Bold;
                    target.NavigationTextColor = text.Color.IfNone(Color.Empty);
                })
                select styled);

    public static IO<Option<Window>> WindowFor(OptionsDialogPage page, bool preferences) =>
        IO.lift(() => Optional(preferences ? RhinoEtoApp.ApplicationPreferencesWindowForPage(page) : RhinoEtoApp.DocumentPropertiesWindowForPage(page)));
}
