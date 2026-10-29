using Rasm.Rhino.Persistence;
using Rhino.PlugIns;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CyclesSetting<T>(string Key, bool Document) where T : notnull {
    internal Option<IO<Option<T>>> Derived { get; init; }

    public IO<Option<T>> Default => Derived.IfNone(() => Cycles.Node.Bind(node => node.Default<T>(Key)));

    public IO<Option<T>> Effective(RenderSettingsSource source) =>
        from stored in Document
            ? RenderSettingsSources.Read(source, scope => IO.lift(() => ArchivableDictionaries.Find<T>(scope.Settings.UserDictionary, Key)))
            : IO.pure(Option<T>.None)
        from value in stored.Match(Some: static held => IO.pure(Some(held)), None: () => Applied.Bind(applied => applied.Match(Some: static held => IO.pure(Some(held)), None: () => Default)))
        select value;

    public Preference<T> Stored(RenderSettingsSource source, T fallback) => Preferences.Dictionary(RenderSettingsSources.Dictionary(source), Key, fallback);

    public IO<Preference<T>> Application(T fallback) => Cycles.Node.Map(node => Preferences.Setting(node, Key, fallback));

    private IO<Option<T>> Applied => Cycles.Node.Bind(static node => node.Existing).Map(held => held.Bind(settings => SettingOverloads<T>.Read(settings, Key, [])));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cycles {
    // --- [NODE]
    public static IO<Guid> PlugInId => IO.lift(static () => Conversions.Present(PlugIn.IdFromName("Rhino Render")).ToFin(new RhinoFault("Rhino Render", RhinoReason.Missing)));

    public static IO<SettingsNode> Node => PlugInId.Map(static id => new SettingsNode(new SettingsRoot.PlugInNode(id)));

    // --- [DOCUMENT]
    public static readonly CyclesSetting<int> RenderPreset = new(nameof(RenderPreset), Document: true);
    public static readonly CyclesSetting<int> Seed = new(nameof(Seed), Document: true);
    public static readonly CyclesSetting<int> Samples = new(nameof(Samples), Document: true);
    public static readonly CyclesSetting<bool> UseDocumentSamples = new(nameof(UseDocumentSamples), Document: true);
    public static readonly CyclesSetting<int> MaxBounce = new(nameof(MaxBounce), Document: true);
    public static readonly CyclesSetting<int> MaxDiffuseBounce = new(nameof(MaxDiffuseBounce), Document: true);
    public static readonly CyclesSetting<int> MaxGlossyBounce = new(nameof(MaxGlossyBounce), Document: true);
    public static readonly CyclesSetting<int> MaxTransmissionBounce = new(nameof(MaxTransmissionBounce), Document: true);
    public static readonly CyclesSetting<int> MaxVolumeBounce = new(nameof(MaxVolumeBounce), Document: true);
    public static readonly CyclesSetting<int> TransparentMaxBounce = new(nameof(TransparentMaxBounce), Document: true);
    public static readonly CyclesSetting<int> TextureBakeQuality = new(nameof(TextureBakeQuality), Document: true);
    public static readonly CyclesSetting<bool> UseAdaptiveSampling = new(nameof(UseAdaptiveSampling), Document: true);
    public static readonly CyclesSetting<double> AdaptiveThreshold = new(nameof(AdaptiveThreshold), Document: true);
    public static readonly CyclesSetting<int> AdaptiveMinSamples = new(nameof(AdaptiveMinSamples), Document: true);
    public static readonly CyclesSetting<int> AoBounces = new(nameof(AoBounces), Document: true);
    public static readonly CyclesSetting<double> SampleClampDirect = new(nameof(SampleClampDirect), Document: true);
    public static readonly CyclesSetting<double> SampleClampIndirect = new(nameof(SampleClampIndirect), Document: true);
    public static readonly CyclesSetting<double> FilterGlossy = new(nameof(FilterGlossy), Document: true);
    public static readonly CyclesSetting<bool> CausticsReflective = new(nameof(CausticsReflective), Document: true);
    public static readonly CyclesSetting<bool> CausticsRefractive = new(nameof(CausticsRefractive), Document: true);
    public static readonly CyclesSetting<bool> UseDirectLight = new(nameof(UseDirectLight), Document: true);
    public static readonly CyclesSetting<bool> UseIndirectLight = new(nameof(UseIndirectLight), Document: true);
    public static readonly CyclesSetting<bool> MaxPasses = new(nameof(MaxPasses), Document: true);
    public static readonly CyclesSetting<int> Threads = new(nameof(Threads), Document: true);
    public static readonly CyclesSetting<double> SunLightFactor = new(nameof(SunLightFactor), Document: true);
    public static readonly CyclesSetting<double> AreaLightFactor = new(nameof(AreaLightFactor), Document: true);
    public static readonly CyclesSetting<double> LinearLightFactor = new(nameof(LinearLightFactor), Document: true);
    public static readonly CyclesSetting<double> PointLightFactor = new(nameof(PointLightFactor), Document: true);
    public static readonly CyclesSetting<double> SpotLightFactor = new(nameof(SpotLightFactor), Document: true);
    public static readonly CyclesSetting<string> IntermediateSelectedDeviceStr = new(nameof(IntermediateSelectedDeviceStr), Document: true);

    // --- [APPLICATION]
    public static readonly CyclesSetting<int> ThrottleMs = new(nameof(ThrottleMs), Document: false);
    public static readonly CyclesSetting<string> SelectedDeviceStr = new(nameof(SelectedDeviceStr), Document: false);
    public static readonly CyclesSetting<bool> StartGpuKernelCompiler = new(nameof(StartGpuKernelCompiler), Document: false);
    public static readonly CyclesSetting<bool> UseLightTree = new(nameof(UseLightTree), Document: false);
    public static readonly CyclesSetting<int> PixelSize = new(nameof(PixelSize), Document: false) {
        Derived = Some(IO.lift(static () => ((long)Eto.Forms.Screen.PrimaryScreen.Bounds.Width * (long)Eto.Forms.Screen.PrimaryScreen.Bounds.Height) switch {
            >= 33_177_600 => Some(4),
            >= 8_294_400 => Some(2),
            _ => Some(1),
        })),
    };
}
