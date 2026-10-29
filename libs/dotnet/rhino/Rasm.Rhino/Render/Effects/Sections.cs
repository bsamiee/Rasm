using System.Globalization;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedEffectSection<TState> : EtoPostEffectCollapsibleSection, ICollapsibleSection, ICollapsibleSection2, ICollapsibleSection3
    where TState : notnull {
    // --- [STATE]
    private readonly PostEffectType stage;
    private readonly Guid id;
    private readonly View.Section row;
    private readonly HostedSection hosted;

    public DefinedEffectSection(PostEffect effect, TState initial, Func<Preference<TState>, View.Section> build) {
        (PostEffectId, stage) = (effect.Id, effect.PostEffectType);
        row = build(Preferences.Of(
            IO.lift(() => Optional(GetParameter(EffectList.StateKey, defaultValue: null) as string)).Bind(text => IO.lift(text.Match(Some: EffectList.Decoded<TState>, None: () => initial))),
            state => Products.Accepted(() => SetParameter(EffectList.StateKey, EffectList.Encoded(state)))));
        id = row.Identity.GUID;
        hosted = new HostedSection(this, row.Actions, Some(ProviderIds.RdkRenderingPostEffects), () => Excluded, visible: true,
            from document in IO.lift(() => Optional(ViewModel?.GetData(ProviderIds.RdkRendering, bForWrite: false, bAutoChangeBracket: false))
                .Bind(static rendering => Optional(RhinoDoc.FromRuntimeSerialNumber(Internals.RhinoDocumentId(rendering))))
                .ToFin(new RhinoFault($"document of post effect section {row.Caption}", RhinoReason.Missing)))
            from scope in RowScope.Open(Some(document), new CommitMode.Immediate())
            from body in ViewBody.Realize(row, row.Actions, scope)
            select body);
    }

    // --- [IDENTITY]
    public override Guid PostEffectId { get; }
    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);
    Guid ICollapsibleSection.Id => id;
    public override Guid PlugInId => Session.PlugIn.Id;
    public override LocalizeStringPair Caption => LocalText.Of(row.Caption);
    public override int SectionHeight => hosted.Height;
    public override bool Hidden => Excluded || hosted.Hidden;

    private bool Excluded =>
        !toSeq(GetPostEffects(stage)).Exists(listed => listed.Id == PostEffectId && listed.IsSelected && (stage is PostEffectType.ToneMapping || listed.Shown));

    // --- [HEADER]
    IHeaderButtonHandler ICollapsibleSection2.NewHeaderButtonHandler() => hosted.Header.ValueUnsafe()!;

    void ICollapsibleSection2.OnAttachingToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnAttachedToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) { }

    bool ICollapsibleSection2.EnableHeaderButton(int index, bool bEnable) => false;

    bool ICollapsibleSection2.ShowHeaderButton(int index, bool bShow) => false;

    void ICollapsibleSection3.UpdateView(uint flags) => _ = hosted.Reread;

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) hosted.Dispose();
        base.Dispose(disposing);
    }
}
