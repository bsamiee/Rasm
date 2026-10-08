using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRegistry {
    public static IO<Option<TBody>> Panel<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        IO.lift(() => MissingGuid.Unless(typeof(TBody)).Map(_ => Optional(RenderPanels.FromRenderSessionId(owner, typeof(TBody), renderSessionId) as TBody)));

    public static IO<Option<TBody>> Tab<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        IO.lift(() => MissingGuid.Unless(typeof(TBody)).Map(_ => Optional(RenderTabs.FromRenderSessionId(owner, typeof(TBody), renderSessionId) as TBody)));

    public static IO<Guid> SidePaneUiIdFromTab(object tab) =>
        IO.lift(() =>
            from keyed in MissingGuid.Unless(tab.GetType())
            from id in Answers.Required(RenderTabs.SidePaneUiIdFromTab(tab), nameof(RenderTabs.SidePaneUiIdFromTab))
            select id);
}
