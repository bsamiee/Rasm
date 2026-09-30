using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>]
[ValidationError<ValidationFailure>]
public readonly partial struct SampleCount {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) =>
        validationError = Limits.AtLeast(1).Violated(value, nameof(SampleCount));
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Automatic")]
[ValidationError<ValidationFailure>]
public readonly partial struct MinimumSamples {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) =>
        validationError = Limits.AtLeast(0).Violated(value, nameof(MinimumSamples));
}

[ValueObject<int>]
[ValidationError<ValidationFailure>]
public readonly partial struct BounceLimit {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) =>
        validationError = Limits.AtLeast(0).Violated(value, nameof(BounceLimit));
}

[ValueObject<int>]
[ValidationError<ValidationFailure>]
public readonly partial struct Seed {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) =>
        validationError = Limits.AtLeast(0).Violated(value, nameof(Seed));
}

[ValueObject<double>]
[ValidationError<ValidationFailure>]
public readonly partial struct NoiseThreshold {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref double value) =>
        validationError = Limits.Above(0.0).AtMost(1.0).Violated(value, nameof(NoiseThreshold));
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off")]
[ValidationError<ValidationFailure>]
public readonly partial struct SampleClamp {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref double value) =>
        validationError = Limits.AtLeast(0.0).Violated(value, nameof(SampleClamp));
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off")]
[ValidationError<ValidationFailure>]
public readonly partial struct GlossyFilter {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref double value) =>
        validationError = Limits.AtLeast(0.0).Violated(value, nameof(GlossyFilter));
}

[SmartEnum<int>]
[ValidationError<ValidationFailure>]
public sealed partial class TextureBake {
    public static readonly TextureBake Low = new(0);

    public static readonly TextureBake Standard = new(1);

    public static readonly TextureBake High = new(2);

    public static readonly TextureBake Ultra = new(3);

    public static readonly TextureBake Disable = new(4);
}

[SmartEnum<int>]
[ValidationError<ValidationFailure>]
public sealed partial class CyclesPreset {
    public static readonly CyclesPreset Architecture = new(0, GlossyFilter.Create(0.5), SampleClamp.Create(3.0), MinimumSamples.Create(16));

    public static readonly CyclesPreset Product = new(1, GlossyFilter.Off, SampleClamp.Create(20.0), MinimumSamples.Create(256));

    public GlossyFilter FilterGlossy { get; }

    public SampleClamp SampleClampIndirect { get; }

    public MinimumSamples AdaptiveMinSamples { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cycles {
    // --- [DOCUMENT]
    public static readonly SceneSetting<Option<CyclesPreset>> RenderPreset = Key<CyclesPreset, int>("RenderPreset");

    public static readonly SceneSetting<Option<SampleCount>> Samples = Key<SampleCount, int>("Samples");

    public static readonly SceneSetting<Option<bool>> UseDocumentSamples = Key<bool, bool>("UseDocumentSamples", Fin.Succ, identity);

    public static readonly SceneSetting<Option<bool>> UseAdaptiveSampling = Key<bool, bool>("UseAdaptiveSampling", Fin.Succ, identity);

    public static readonly SceneSetting<Option<NoiseThreshold>> AdaptiveThreshold = Key<NoiseThreshold, double>("AdaptiveThreshold");

    public static readonly SceneSetting<Option<MinimumSamples>> AdaptiveMinSamples = Key<MinimumSamples, int>("AdaptiveMinSamples");

    public static readonly SceneSetting<Option<Seed>> Seed = Key<Seed, int>("Seed");

    public static readonly SceneSetting<Option<BounceLimit>> MaxBounce = Key<BounceLimit, int>("MaxBounce");

    public static readonly SceneSetting<Option<BounceLimit>> MaxDiffuseBounce = Key<BounceLimit, int>("MaxDiffuseBounce");

    public static readonly SceneSetting<Option<BounceLimit>> MaxGlossyBounce = Key<BounceLimit, int>("MaxGlossyBounce");

    public static readonly SceneSetting<Option<BounceLimit>> MaxTransmissionBounce = Key<BounceLimit, int>("MaxTransmissionBounce");

    public static readonly SceneSetting<Option<BounceLimit>> MaxVolumeBounce = Key<BounceLimit, int>("MaxVolumeBounce");

    public static readonly SceneSetting<Option<BounceLimit>> TransparentMaxBounce = Key<BounceLimit, int>("TransparentMaxBounce");

    public static readonly SceneSetting<Option<BounceLimit>> AoBounces = Key<BounceLimit, int>("AoBounces");

    public static readonly SceneSetting<Option<SampleClamp>> SampleClampDirect = Key<SampleClamp, double>("SampleClampDirect");

    public static readonly SceneSetting<Option<SampleClamp>> SampleClampIndirect = Key<SampleClamp, double>("SampleClampIndirect");

    public static readonly SceneSetting<Option<GlossyFilter>> FilterGlossy = Key<GlossyFilter, double>("FilterGlossy");

    public static readonly SceneSetting<Option<bool>> CausticsReflective = Key<bool, bool>("CausticsReflective", Fin.Succ, identity);

    public static readonly SceneSetting<Option<bool>> CausticsRefractive = Key<bool, bool>("CausticsRefractive", Fin.Succ, identity);

    public static readonly SceneSetting<Option<bool>> UseDirectLight = Key<bool, bool>("UseDirectLight", Fin.Succ, identity);

    public static readonly SceneSetting<Option<bool>> UseIndirectLight = Key<bool, bool>("UseIndirectLight", Fin.Succ, identity);

    public static readonly SceneSetting<Option<TextureBake>> TextureBakeQuality = Key<TextureBake, int>("TextureBakeQuality");

    public static readonly SceneSetting<Option<bool>> MaxPasses = Key<bool, bool>("MaxPasses", Fin.Succ, identity);

    public static Seq<Func<RenderSettings, IO<Unit>>> Preset(CyclesPreset preset) =>
        Seq(
            RenderPreset.Set(Some(preset)),
            FilterGlossy.Set(Some(preset.FilterGlossy)),
            SampleClampIndirect.Set(Some(preset.SampleClampIndirect)),
            AdaptiveMinSamples.Set(Some(preset.AdaptiveMinSamples)));

    private static SceneSetting<Option<TValue>> Key<TValue, TStored>(string name) where TValue : IObjectFactory<TValue, TStored, ValidationFailure>, IConvertible<TStored> where TStored : struct =>
        Key<TValue, TStored>(name, Answers.Validated<TValue, TStored>, static value => value.ToValue());

    private static SceneSetting<Option<TValue>> Key<TValue, TStored>(string name, Func<TStored, Fin<TValue>> parse, Func<TValue, TStored> store) where TStored : struct =>
        new(
            settings => ArchivableDictionaries.Find<TStored>(settings.UserDictionary, name).RunSafe().Bind(stored => stored.Traverse(parse).As()),
            (settings, value) => _ = value.Match(Some: held => settings.UserDictionary[name] = store(held), None: () => settings.UserDictionary.Remove(name)));

    // --- [PLUG_IN]
    public static readonly SettingKey<int> ThrottleMs = new("ThrottleMs", SettingType.Integer);

    public static readonly SettingKey<int> Threads = new("Threads", SettingType.Integer);

    public static readonly SettingKey<int> PixelSize = new("PixelSize", SettingType.Integer);

    public static readonly SettingKey<string> SelectedDeviceStr = new("SelectedDeviceStr", SettingType.String);

    public static readonly SettingKey<string> IntermediateSelectedDeviceStr = new("IntermediateSelectedDeviceStr", SettingType.String);

    public static readonly SettingKey<bool> StartGpuKernelCompiler = new("StartGpuKernelCompiler", SettingType.Bool);

    public static readonly SettingKey<bool> UseLightTree = new("UseLightTree", SettingType.Bool);

    public static readonly SettingKey<double> SunLightFactor = new("SunLightFactor", SettingType.Double);

    public static readonly SettingKey<double> AreaLightFactor = new("AreaLightFactor", SettingType.Double);

    public static readonly SettingKey<double> LinearLightFactor = new("LinearLightFactor", SettingType.Double);

    public static readonly SettingKey<double> PointLightFactor = new("PointLightFactor", SettingType.Double);

    public static readonly SettingKey<double> SpotLightFactor = new("SpotLightFactor", SettingType.Double);

    public static IO<Option<T>> Read<T>(SettingKey<T> key) =>
        Settings.Bind(node => PlugInSettings.Find(node, key));

    public static IO<Unit> Write<T>(SettingKey<T> key, T value) =>
        Settings.Bind(node => PlugInSettings.Set(node, key, value));

    private static IO<PersistentSettings> Settings { get; } =
        IO.lift(static () => PersistentSettings.FromPlugInId(new Guid(0x9bc28e9e, 0x7a6c, 0x4b8f, 0xa0, 0xc6, 0x3d, 0x05, 0xe0, 0x2d, 0x1b, 0x97)));
}
