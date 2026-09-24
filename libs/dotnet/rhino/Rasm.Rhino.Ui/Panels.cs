using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelEvent {
    public abstract uint DocSerial { get; init; }

    public sealed record Shown(uint DocSerial, ShowPanelReason Reason) : PanelEvent;

    public sealed record Hidden(uint DocSerial, ShowPanelReason Reason) : PanelEvent;

    public sealed record ClosingPanel(uint DocSerial) : PanelEvent;

    public sealed record ClosingDocument(uint DocSerial) : PanelEvent;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelIcon {
    public sealed record FromIcon(Icon Icon) : PanelIcon;

    public sealed record FromFile(string Path) : PanelIcon;

    public sealed record FromResource(Assembly Assembly, string ResourceId) : PanelIcon;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelPlacement {
    public sealed record Default(bool Select) : PanelPlacement;

    public sealed record DockBar(Guid Id, bool Select) : PanelPlacement;

    public sealed record Sibling(Guid Id, bool Select) : PanelPlacement;

    public sealed record Float(Panels.FloatPanelMode Mode) : PanelPlacement;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelVisibility {
    public sealed record Hidden() : PanelVisibility;

    public sealed record Visible() : PanelVisibility;

    public sealed record SelectedTab() : PanelVisibility;
}

public sealed record PanelState(PanelVisibility Visibility, Seq<Guid> DockBars, Seq<Guid> OpenPanels);

public sealed record SectionSpec(
    LocalizeStringPair Caption,
    int Height,
    bool Collapsible,
    bool Hidden,
    bool InitiallyExpanded,
    bool FullHeight,
    Option<LocalizeStringPair> CommandOptionName,
    Option<Func<IHeaderButtonHandler>> HeaderButtons);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionEvent {
    public sealed record AttachingToHolder(ICollapsibleSectionHolder2 Holder) : SectionEvent;

    public sealed record AttachedToHolder(ICollapsibleSectionHolder2 Holder) : SectionEvent;

    public sealed record DetachingFromHolder(ICollapsibleSectionHolder2 Holder) : SectionEvent;

    public sealed record DetachedFromHolder(ICollapsibleSectionHolder2 Holder) : SectionEvent;

    public sealed record HolderVisible(bool Visible) : SectionEvent;

    public sealed record UpdateView(uint Flags) : SectionEvent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackPanel : Panel, IPanel {
    private readonly Func<PanelEvent, IO<Unit>> onEvent;
    private readonly Action<Error> reject;

    protected CallbackPanel(Control content, Func<PanelEvent, IO<Unit>> onEvent, Action<Error> reject) {
        content.UseRhinoStyle();
        Content = content;
        this.onEvent = onEvent;
        this.reject = reject;
    }

    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => _ = Answers.Answer(onEvent(new PanelEvent.Shown(documentSerialNumber, reason)), reject, unit);

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => _ = Answers.Answer(onEvent(new PanelEvent.Hidden(documentSerialNumber, reason)), reject, unit);

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) =>
        _ = Answers.Answer(onEvent(onCloseDocument ? new PanelEvent.ClosingDocument(documentSerialNumber) : new PanelEvent.ClosingPanel(documentSerialNumber)), reject, unit);
}

public abstract class CallbackCollapsibleSection : EtoCollapsibleSection3 {
    private readonly Func<SectionEvent, IO<Unit>> onEvent;
    private readonly Action<Error> reject;

    protected CallbackCollapsibleSection(SectionSpec spec, Control content, Func<SectionEvent, IO<Unit>> onEvent, Action<Error> reject) {
        Spec = spec;
        Content = content;
        this.onEvent = onEvent;
        this.reject = reject;
    }

    public SectionSpec Spec { get; }

    public sealed override LocalizeStringPair Caption => Spec.Caption;

    public sealed override int SectionHeight => Spec.Height;

    public sealed override bool Collapsible => Spec.Collapsible;

    public sealed override bool Hidden => Spec.Hidden;

    public sealed override bool InitiallyExpanded => Spec.InitiallyExpanded;

    public sealed override LocalizeStringPair CommandOptionName => Spec.CommandOptionName.IfNone(() => base.CommandOptionName);

    public sealed override IHeaderButtonHandler NewHeaderButtonHandler() => Spec.HeaderButtons.Match(Some: static create => create(), None: base.NewHeaderButtonHandler);

    public sealed override void OnAttachingToHolder(ICollapsibleSectionHolder2 holder) => _ = Answers.Answer(onEvent(new SectionEvent.AttachingToHolder(holder)), reject, unit);

    public sealed override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        _ = Answers.Answer(onEvent(new SectionEvent.AttachedToHolder(holder)), reject, unit);
    }

    public sealed override void OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) => _ = Answers.Answer(onEvent(new SectionEvent.DetachingFromHolder(holder)), reject, unit);

    public sealed override void OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) => _ = Answers.Answer(onEvent(new SectionEvent.DetachedFromHolder(holder)), reject, unit);

    public sealed override void HolderVisible(bool visible) => _ = Answers.Answer(onEvent(new SectionEvent.HolderVisible(visible)), reject, unit);

    public sealed override void UpdateView(uint flags) => _ = Answers.Answer(onEvent(new SectionEvent.UpdateView(flags)), reject, unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPanels {
    // --- [REGISTRATION]
    public static IO<Unit> Register(PlugIn plugIn, Type panelType, LocalizeStringPair caption, PanelIcon icon, PanelType kind) =>
        IO.lift(() => icon.Switch(
            (PlugIn: plugIn, Panel: panelType, Caption: caption.Local, Kind: kind),
            fromIcon: static (state, fromIcon) => Panels.RegisterPanel(state.PlugIn, state.Panel, state.Caption, fromIcon.Icon, state.Kind),
            fromFile: static (state, fromFile) => Panels.RegisterPanel(state.PlugIn, state.Panel, state.Caption, new Icon(fromFile.Path), state.Kind),
            fromResource: static (state, fromResource) => Panels.RegisterPanel(state.PlugIn, state.Panel, state.Caption, fromResource.Assembly, fromResource.ResourceId, state.Kind)));

    // --- [VISIBILITY]
    public static IO<Option<Guid>> Open(Type panelType, PanelPlacement placement) =>
        placement.Switch(
            panelType,
            @default: static (type, placed) => IO.lift(() => Panels.OpenPanel(type, placed.Select)).Map(static _ => Option<Guid>.None),
            dockBar: static (type, dockBar) => IO.lift(() => Answers.NonEmpty(Panels.OpenPanel(dockBar.Id, type, dockBar.Select), nameof(Panels.OpenPanel))).Map(static id => Some(id)),
            sibling: static (type, sibling) => IO.lift(() => Refused.Unless(Panels.OpenPanelAsSibling(type.GUID, sibling.Id, sibling.Select), nameof(Panels.OpenPanelAsSibling))).Map(static _ => Option<Guid>.None),
            @float: static (type, floating) => IO.lift(() => Panels.FloatPanel(type, floating.Mode)).Map(static _ => Option<Guid>.None));

    public static IO<PanelState> Snapshot(Type panelType) =>
        IO.lift(() => new PanelState(
            Panels.IsPanelVisible(panelType, isSelectedTab: true) ? new PanelVisibility.SelectedTab()
                : Panels.IsPanelVisible(panelType, isSelectedTab: false) ? new PanelVisibility.Visible()
                : new PanelVisibility.Hidden(),
            toSeq(Panels.PanelDockBars(panelType.GUID)),
            toSeq(Panels.GetOpenPanelIds())));

    public static IO<Seq<T>> Instances<T>(RhinoDoc doc) where T : class =>
        IO.lift(() => toSeq(Panels.GetPanels<T>(doc)));

    // --- [SECTIONS]
    public static IO<EtoCollapsibleSectionHolder2> RegisterSections(Seq<CallbackCollapsibleSection> sections, bool scrollbars, bool checkboxes) =>
        from full in IO.lift(() => sections.Filter(static section => section.Spec.FullHeight))
        from single in IO.lift(() => ManyFullHeightSections.Unless(full.Count <= 1, full.Count))
        from holder in IO.lift(() => new EtoCollapsibleSectionHolder2 { UseScrollbars = scrollbars, UseCheckBoxes = checkboxes })
        from registered in GeometryOps.OnFailure(
            IO.lift(() => sections.Iter(holder.Add)).Bind(_ => IO.lift(() => full.Head.Iter(holder.SetFullHeightSection))),
            IO.lift(holder.Dispose))
        select holder;
}
