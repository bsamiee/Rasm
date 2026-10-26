using System.Globalization;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedEffectSection<TState> : EtoPostEffectCollapsibleSection, ICollapsibleSection, ICollapsibleSection2, ICollapsibleSection3
    where TState : IStateRecord<TState> {
    private readonly Guid effect;
    private readonly PostEffectType stage;
    private readonly Guid id;
    private readonly View.Section row;
    private readonly IPlugInViews views;
    private readonly SectionLifetime lifetime;

    public DefinedEffectSection(IPlugInViews owner, PostEffect hosted, Func<ValueStore<TState>, View.Section> build) {
        (views, effect, stage) = (owner, hosted.Id, hosted.PostEffectType);
        row = build(Store);
        (id, Caption) = (row.Identity.GUID, RowText.Localize(row.Caption, views));
        lifetime = new SectionLifetime(this, views, Some(ProviderIds.RdkRenderingPostEffects), () => Excluded, visible: true, retain =>
            from document in IO.lift(() => (Optional(ViewModel?.GetData(ProviderIds.RdkRendering, bForWrite: false, bAutoChangeBracket: false))
                    .Bind(static rendering => Optional(RhinoDoc.FromRuntimeSerialNumber(EffectCollection.RhinoDocumentId(rendering)))) | Optional(RhinoDoc.ActiveDoc))
                .ToFin(new Refused(nameof(RhinoDoc.ActiveDoc), Refused.Absent)))
            from scope in RowScope.Open(views, Some(document), new CommitMode.Immediate())
            from held in ViewBody.Realize(views, row, scope, retain)
            select held);
    }

    // --- [STORE]
    private ValueStore<TState> Store =>
        new EffectSource.Section(this) switch {
            var source => ValueStore.Defaulted(
                EffectCollection.State<TState>(source, effect),
                IO.pure(TState.Default),
                state => IO.lift(() => Callbacks.Each(EffectRequest.Tuning.Of(effect, state), tuning => SetParameter(tuning.Key, tuning.Text), nameof(SetParameter))),
                EqualityComparer<TState>.Default.Equals,
                Applied.Live,
                Some(source.Changed)),
        };

    // --- [IDENTITY]
    public override Guid PostEffectId => effect;
    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);
    Guid ICollapsibleSection.Id => id;
    public override LocalizeStringPair Caption { get; }
    public override int SectionHeight => lifetime.Height;
    public override bool Hidden => Excluded || lifetime.Hidden;

    private bool Excluded =>
        !toSeq(GetPostEffects(stage)).Exists(listed => listed.Id == effect && listed.IsSelected && (stage is PostEffectType.ToneMapping || listed.Shown));

    // --- [HEADER]
    IHeaderButtonHandler ICollapsibleSection2.NewHeaderButtonHandler() => lifetime.Header(row).ValueUnsafe()!;

    void ICollapsibleSection2.OnAttachingToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnAttachedToHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) { }

    void ICollapsibleSection2.OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) { }

    bool ICollapsibleSection2.EnableHeaderButton(int index, bool bEnable) => false;

    bool ICollapsibleSection2.ShowHeaderButton(int index, bool bShow) => false;

    void ICollapsibleSection3.UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, CallbackSite.Of(views, this, nameof(ICollapsibleSection3.UpdateView)));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing)
                lifetime.Dispose();
        } finally {
            base.Dispose(disposing);
        }
    }
}
