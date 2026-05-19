using System.Runtime.InteropServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;

namespace Rasm.Grasshopper.UI;

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputModifierSnapshot(bool Shift, bool Command, bool Option);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputSelectionSnapshot(SelectionMode Mode, InputModifierSnapshot Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputPanelSnapshot(int Count, string Category);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ToolbarSnapshot(int Count, bool Enabled, float MinimumWidth, float MaximumWidth, float Height);

// --- [INTENTS] --------------------------------------------------------------------------
public static class InputIntent {
    public static GrasshopperUiIntent<InputSelectionSnapshot> Selection(Control control) =>
        new(run: _ => Fin.Succ(value: new InputSelectionSnapshot(Mode: control.SelectionMode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))), policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<InputSelectionSnapshot> Selection(MouseEventArgs mouse) =>
        new(run: _ => Fin.Succ(value: new InputSelectionSnapshot(Mode: mouse.SelectionMode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))), policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<InputSelectionSnapshot> Selection(WindowSelectionEventArgs window) =>
        new(run: _ => Fin.Succ(value: new InputSelectionSnapshot(Mode: window.SelectionMode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))), policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<InputModifierSnapshot> Modifiers(Keys keys) =>
        new(run: _ => Fin.Succ(value: ModifierOf(keys: keys)), policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) =>
        new(
            run: _ => Optional(populate)
                .ToFin(Fail: Op.Of(name: nameof(Panel)).InvalidInput())
                .Bind(valid => {
                    InputPanel panel = new();
                    return valid(arg: panel).Map(_ => new InputPanelSnapshot(Count: panel.Count, Category: panel.Category));
                }),
            policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(Func<Bar, Fin<Unit>> populate) =>
        new(
            run: _ => Optional(populate)
                .ToFin(Fail: Op.Of(name: nameof(Toolbar)).InvalidInput())
                .Bind(valid => {
                    Bar bar = new();
                    return valid(arg: bar).Map(_ => {
                        bar.Layout();
                        return new ToolbarSnapshot(Count: bar.Count, Enabled: bar.Enabled, MinimumWidth: bar.MinimumWidth, MaximumWidth: bar.MaximumWidth, Height: bar.Height);
                    });
                }),
            policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<T> ContextMenu<T>(Func<ContextMenu, Fin<T>> populate) =>
        new(
            run: _ => Optional(populate)
                .ToFin(Fail: Op.Of(name: nameof(ContextMenu)).InvalidInput())
                .Bind(valid => {
                    using ContextMenu menu = new();
                    return valid(arg: menu);
                }),
            policy: GrasshopperUiPolicy.Read);

    public static GrasshopperUiIntent<T> ContextMenu<T>(ContextMenu menu, Func<ContextMenu, Fin<T>> populate) =>
        new(
            run: _ => Optional((Menu: menu, Populate: populate))
                .Filter(static item => item.Menu is not null && item.Populate is not null)
                .ToFin(Fail: Op.Of(name: nameof(ContextMenu)).InvalidInput())
                .Bind(valid => valid.Populate(arg: valid.Menu)),
            policy: GrasshopperUiPolicy.Read);

    private static InputModifierSnapshot ModifierOf(Keys keys) =>
        new(Shift: keys.HasShift(), Command: keys.HasCommand(), Option: keys.HasOption());
}
