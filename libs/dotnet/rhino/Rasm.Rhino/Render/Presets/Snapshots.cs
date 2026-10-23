using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects.SnapShots;
using UUIDNext;

namespace Rasm.Rhino.Render.Presets;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotCategory {
    public static readonly SnapshotCategory Resolution = new("resolution", [], static _ => [ImageOutputState.Owner, FrameState.Owner]);
    public static readonly SnapshotCategory Sampling = new(
        "sampling",
        [CyclesKey.UseDocumentSamples, CyclesKey.Samples, CyclesKey.UseAdaptiveSampling, CyclesKey.AdaptiveThreshold, CyclesKey.AdaptiveMinSamples, CyclesKey.Seed, CyclesKey.TextureBakeQuality],
        static _ => []);
    public static readonly SnapshotCategory LightPaths = new(
        "light-paths",
        [
            CyclesKey.RenderPreset, CyclesKey.MaxBounce, CyclesKey.MaxDiffuseBounce, CyclesKey.MaxGlossyBounce, CyclesKey.MaxTransmissionBounce,
            CyclesKey.MaxVolumeBounce, CyclesKey.TransparentMaxBounce, CyclesKey.AoBounces, CyclesKey.SampleClampDirect, CyclesKey.SampleClampIndirect,
            CyclesKey.FilterGlossy, CyclesKey.CausticsReflective, CyclesKey.CausticsRefractive, CyclesKey.UseDirectLight, CyclesKey.UseIndirectLight,
        ],
        static _ => []);
    public static readonly SnapshotCategory Channels = new("channels", [], static _ => [RenderChannelsState.Owner]);
    public static readonly SnapshotCategory Grading = new("grading", [], static effects => effects.Bindings.Map(static binding => binding.Owner));

    public Seq<CyclesKey> CyclesKeys { get; }

    public string Caption =>
        RowText.Localize(Map(resolution: "Resolution and Quality", sampling: "Sampling", lightPaths: "Light Paths", channels: "Render Channels", grading: "Grading")).Local;

    [UseDelegateFromConstructor]
    public partial Seq<string> Owners(BindingGroup effects);

    public LanguageExt.HashSet<EntryKey> Keys(BindingGroup bindings, BindingGroup effects) =>
        toHashSet(Owners(effects)) switch {
            var owners => [.. bindings.Keys().Filter(key => owners.Contains(key.Owner)), .. CyclesKeys.Map(static item => new EntryKey(CyclesKey.Owner, [item.Key]))],
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderSnapshots {
    // --- [CLIENTS]
    public static Seq<Func<IPlugInViews, Action<IDisposable>, IO<Unit>>> Clients(RenderHistory history) =>
        toSeq(SnapshotCategory.Items).Map(category => fun((IPlugInViews views, Action<IDisposable> retain) => DefinedSnapshotClient.Register(new SnapshotDefinition(
            Uuid.NewNameBased(views.PlugIn.Id, category.Key), category.Caption, SnapShotsClient.RenderingCategory,
            Document: Some<SnapshotState<RhinoDoc>>(new SnapshotState<RhinoDoc, ValueSet>(
                0x5244_5353u, ValueSet.Codec,
                doc => Captured(history, doc, category),
                (doc, values) => history.Settings(doc).Bind(binding => binding.Commit(values, [])).Map(unit)))))(views, retain)));

    private static IO<ValueSet> Captured(RenderHistory history, RhinoDoc doc, SnapshotCategory category) =>
        from binding in history.Settings(doc)
        from grading in history.Grading(doc)
        from values in binding.Group.Capture
        select new ValueSet(values.Entries.Intersect(category.Keys(binding.Group, grading)));
}
