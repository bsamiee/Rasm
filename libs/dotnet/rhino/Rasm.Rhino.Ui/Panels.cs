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
public abstract partial record PanelIcon {
    public sealed record FromIcon(Icon Icon) : PanelIcon;

    public sealed record FromResource(Assembly Assembly, string ResourceId) : PanelIcon;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelVisibility {
    public sealed record Hidden() : PanelVisibility;

    public sealed record Visible() : PanelVisibility;

    public sealed record SelectedTab() : PanelVisibility;
}

public sealed record PanelState(PanelVisibility Visibility, Seq<Guid> DockBars);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackPanel : Panel {
    private readonly Option<IDisposable> held;

    protected CallbackPanel(IO<(Control Content, IDisposable Held)> view) {
        (Content, held) = view.RunSafe().Match(
            Succ: static shown => (shown.Content, Some(shown.Held)),
            Fail: static error => ((Control)new Label { Text = ErrorOps.Localize(error), Wrap = WrapMode.Word }, Option<IDisposable>.None));
        Content.UseRhinoStyle();
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            _ = held.Iter(static resource => resource.Dispose());
        base.Dispose(disposing);
    }
}

public abstract class CallbackCollapsibleSection : EtoCollapsibleSection3 {
    protected CallbackCollapsibleSection(Control content) => Content = content;

    public sealed override int SectionHeight => (int)Math.Ceiling(Content.GetPreferredSize().Height);

    public override void OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) { }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPanels {
    // --- [REGISTRATION]
    public static IO<Unit> Register<TPanel>(PlugIn plugIn, string caption, PanelIcon icon, PanelType type) where TPanel : CallbackPanel =>
        IO.lift(() => icon.Switch(
            (PlugIn: plugIn, Caption: caption, Type: type),
            fromIcon: static (state, fromIcon) => Panels.RegisterPanel(state.PlugIn, typeof(TPanel), state.Caption, fromIcon.Icon, state.Type),
            fromResource: static (state, fromResource) => Panels.RegisterPanel(state.PlugIn, typeof(TPanel), state.Caption, fromResource.Assembly, fromResource.ResourceId, state.Type)));

    // --- [VISIBILITY]
    public static IO<Guid> OpenOnDockBar(Type panelType, Guid dockBarId, bool select) =>
        IO.lift(() => Answers.Required(Panels.OpenPanel(dockBarId, panelType, select), nameof(Panels.OpenPanel)));

    public static IO<Guid> OpenAsSibling(Type panelType, Guid siblingId, bool select) =>
        IO.lift(() => Answers.Required(Panels.PanelDockBar(siblingId), nameof(Panels.PanelDockBar)))
            .Bind(dockBarId => OpenOnDockBar(panelType, dockBarId, select));

    public static IO<PanelState> Snapshot(RhinoDoc doc, Type panelType) =>
        from selected in TabVisible(doc, panelType, isSelectedTab: true)
        from visible in TabVisible(doc, panelType, isSelectedTab: false)
        from dockBars in IO.lift(() => toSeq(Panels.PanelDockBars(panelType.GUID)))
        select new PanelState(
            selected ? new PanelVisibility.SelectedTab() : visible ? new PanelVisibility.Visible() : new PanelVisibility.Hidden(),
            dockBars);

    private static IO<bool> TabVisible(RhinoDoc doc, Type panelType, bool isSelectedTab) =>
        HostInterop.Execute(
            "Rhino.UI.Internal.NamedCallbacks.RhinoUiIsTabVisible",
            args => {
                args.Set("factoryId", panelType.GUID);
                args.Set("isSelectedTab", isSelectedTab);
                args.Set("documentSerialNumber", doc.RuntimeSerialNumber);
            },
            static args => Answers.Found(args.TryGetBool("isVisible", out bool visible), visible));

    // --- [SECTIONS]
    public static IO<EtoCollapsibleSectionHolder2> RegisterSections(Seq<EtoCollapsibleSection3> above, Option<EtoCollapsibleSection3> fullHeight, Seq<EtoCollapsibleSection3> below, bool scrollbars, bool checkboxes) =>
        from holder in IO.lift(() => new EtoCollapsibleSectionHolder2 { UseScrollbars = scrollbars, UseCheckBoxes = checkboxes })
        from registered in DisposalOps.OnFailure(
            IO.lift(() => (above + fullHeight.ToSeq() + below).Iter(holder.Add)).Bind(_ => IO.lift(() => fullHeight.Iter(holder.SetFullHeightSection))),
            IO.lift(holder.Dispose))
        select holder;
}
