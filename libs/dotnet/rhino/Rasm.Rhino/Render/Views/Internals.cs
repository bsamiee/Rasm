using System.Runtime.CompilerServices;
using Rhino.Render;

namespace Rasm.Rhino.Render.Views;

// --- [SERVICES] ------------------------------------------------------------------------
internal static class Internals {
    // --- [RENDER_PANES]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FindExistingPanelData")]
    [return: UnsafeAccessorType("Rhino.Render.RenderPanelData, RhinoCommon")]
    internal static extern object? FindExistingPanelData(RenderPanels? owner, Guid pluginId, Guid tabId);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SidePaneUiIdFromPanelObject")]
    internal static extern Guid SidePaneUiIdFromPanelObject([UnsafeAccessorType("Rhino.Render.RenderPanelInstances, RhinoCommon")] object instances, object panelObject);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PanelObject")]
    internal static extern object PanelObject([UnsafeAccessorType("Rhino.UI.PanelInstance, RhinoCommon")] object instance);
}
