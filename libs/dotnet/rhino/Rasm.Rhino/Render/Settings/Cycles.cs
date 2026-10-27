using System.Runtime.CompilerServices;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CyclesKey<T>(string Key) where T : notnull {
    public ValueStore<T> Store(RenderSettingsSource source, object owner) =>
        ArchivableDictionaries.Store<T>(RenderSettingsSources.Dictionary(source, owner), Key, None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CyclesSetting {
    public static Guid PlugInId { get; } = new(0x9bc28e9e, 0x7a6c, 0x4b8f, 0xa0, 0xc6, 0x3d, 0x05, 0xe0, 0x2d, 0x1b, 0x97);

    public static SettingsNode Node { get; } = new(PlugInId);

    // --- [DOCUMENT]
    public static readonly CyclesKey<int> RenderPreset = new(nameof(RenderPreset));
    public static readonly CyclesKey<int> Seed = new(nameof(Seed));
    public static readonly CyclesKey<int> Samples = new(nameof(Samples));
    public static readonly CyclesKey<bool> UseDocumentSamples = new(nameof(UseDocumentSamples));
    public static readonly CyclesKey<int> MaxBounce = new(nameof(MaxBounce));
    public static readonly CyclesKey<int> MaxDiffuseBounce = new(nameof(MaxDiffuseBounce));
    public static readonly CyclesKey<int> MaxGlossyBounce = new(nameof(MaxGlossyBounce));
    public static readonly CyclesKey<int> MaxTransmissionBounce = new(nameof(MaxTransmissionBounce));
    public static readonly CyclesKey<int> MaxVolumeBounce = new(nameof(MaxVolumeBounce));
    public static readonly CyclesKey<int> TransparentMaxBounce = new(nameof(TransparentMaxBounce));
    public static readonly CyclesKey<int> TextureBakeQuality = new(nameof(TextureBakeQuality));
    public static readonly CyclesKey<bool> UseAdaptiveSampling = new(nameof(UseAdaptiveSampling));
    public static readonly CyclesKey<double> AdaptiveThreshold = new(nameof(AdaptiveThreshold));
    public static readonly CyclesKey<int> AdaptiveMinSamples = new(nameof(AdaptiveMinSamples));
    public static readonly CyclesKey<int> AoBounces = new(nameof(AoBounces));
    public static readonly CyclesKey<double> SampleClampDirect = new(nameof(SampleClampDirect));
    public static readonly CyclesKey<double> SampleClampIndirect = new(nameof(SampleClampIndirect));
    public static readonly CyclesKey<double> FilterGlossy = new(nameof(FilterGlossy));
    public static readonly CyclesKey<bool> CausticsReflective = new(nameof(CausticsReflective));
    public static readonly CyclesKey<bool> CausticsRefractive = new(nameof(CausticsRefractive));
    public static readonly CyclesKey<bool> UseDirectLight = new(nameof(UseDirectLight));
    public static readonly CyclesKey<bool> UseIndirectLight = new(nameof(UseIndirectLight));
    public static readonly CyclesKey<bool> MaxPasses = new(nameof(MaxPasses));

    // --- [APPLICATION]
    public static readonly PlugInSetting<int> ThrottleMs =
        new(nameof(ThrottleMs), 100, static (settings, key, out value, legacy) => settings.TryGetInteger(key, out value, legacy), static (settings, key, value) => settings.SetInteger(key, value));
    public static readonly PlugInSetting<string> SelectedDeviceStr =
        new(nameof(SelectedDeviceStr), "-1", static (settings, key, out value, legacy) => settings.TryGetString(key, out value, legacy), static (settings, key, value) => settings.SetString(key, value));
    public static readonly PlugInSetting<string> IntermediateSelectedDeviceStr =
        new(nameof(IntermediateSelectedDeviceStr), "-1", static (settings, key, out value, legacy) => settings.TryGetString(key, out value, legacy), static (settings, key, value) => settings.SetString(key, value));
    public static readonly PlugInSetting<int> PixelSize =
        new(nameof(PixelSize), 1, static (settings, key, out value, legacy) => settings.TryGetInteger(key, out value, legacy), static (settings, key, value) => settings.SetInteger(key, value));
    public static readonly PlugInSetting<bool> StartGpuKernelCompiler =
        new(nameof(StartGpuKernelCompiler), Default: true, static (settings, key, out value, legacy) => settings.TryGetBool(key, out value, legacy), static (settings, key, value) => settings.SetBool(key, value)) {
            Applied = Applied.IdleSave,
        };
    public static readonly PlugInSetting<bool> UseLightTree =
        new(nameof(UseLightTree), Default: true, static (settings, key, out value, legacy) => settings.TryGetBool(key, out value, legacy), static (settings, key, value) => settings.SetBool(key, value));
    public static readonly PlugInSetting<double> SunLightFactor =
        new(nameof(SunLightFactor), 3.2d, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));
    public static readonly PlugInSetting<double> AreaLightFactor =
        new(nameof(AreaLightFactor), 17.2d, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));
    public static readonly PlugInSetting<double> LinearLightFactor =
        new(nameof(LinearLightFactor), 10d, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));
    public static readonly PlugInSetting<double> PointLightFactor =
        new(nameof(PointLightFactor), 40d, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));
    public static readonly PlugInSetting<double> SpotLightFactor =
        new(nameof(SpotLightFactor), 40d, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));

    public static IO<Option<PlugInSetting<int>>> Threads => Defaulted();

    private static IO<Option<PlugInSetting<int>>> Defaulted([CallerMemberName] string key = "") =>
        from node in SettingRoots.TryGetChild(Node)
        select node
            .Bind(settings => Conversions.Found(settings.TryGetDefault(key, out int factory), factory))
            .Map(factory => new PlugInSetting<int>(key, factory,
                static (settings, key, out value, legacy) => settings.TryGetInteger(key, out value, legacy), static (settings, key, value) => settings.SetInteger(key, value)));
}
