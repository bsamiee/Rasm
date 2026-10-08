using System.Diagnostics;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.Collections;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SampleCount : System.Numerics.IMinMaxValue<SampleCount> {
    public static SampleCount MinValue { get; } = new(1);
    public static SampleCount MaxValue { get; } = new(int.MaxValue);
    public static SampleCount Default { get; } = new(1000);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Automatic", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MinimumSamples : System.Numerics.IMinMaxValue<MinimumSamples> {
    public static MinimumSamples MinValue { get; } = new(0);
    public static MinimumSamples MaxValue { get; } = new(int.MaxValue);
    public static MinimumSamples Default { get; } = new(16);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct BounceLimit : System.Numerics.IMinMaxValue<BounceLimit> {
    public static BounceLimit MinValue { get; } = new(0);
    public static BounceLimit MaxValue { get; } = new(int.MaxValue);
    public static BounceLimit Diffuse { get; } = new(4);
    public static BounceLimit Glossy { get; } = new(16);
    public static BounceLimit Path { get; } = new(32);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SamplingSeed : System.Numerics.IMinMaxValue<SamplingSeed> {
    public static SamplingSeed MinValue { get; } = new(0);
    public static SamplingSeed MaxValue { get; } = new(int.MaxValue);
    public static SamplingSeed Default { get; } = new(128);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct NoiseThreshold : System.Numerics.IMinMaxValue<NoiseThreshold> {
    public static NoiseThreshold MinValue { get; } = new(double.BitIncrement(0d));
    public static NoiseThreshold MaxValue { get; } = new(1d);
    public static NoiseThreshold Default { get; } = new(0.01d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SampleClamp : System.Numerics.IMinMaxValue<SampleClamp> {
    public static SampleClamp MinValue { get; } = new(0d);
    public static SampleClamp MaxValue { get; } = new(double.MaxValue);
    public static SampleClamp Default { get; } = new(3d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct GlossyFilter : System.Numerics.IMinMaxValue<GlossyFilter> {
    public static GlossyFilter MinValue { get; } = new(0d);
    public static GlossyFilter MaxValue { get; } = new(double.MaxValue);
    public static GlossyFilter Default { get; } = new(0.5d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum<int>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class TextureBake {
    public static readonly TextureBake Low = new(0);
    public static readonly TextureBake Standard = new(1);
    public static readonly TextureBake High = new(2);
    public static readonly TextureBake Ultra = new(3);
    public static readonly TextureBake Disable = new(4);
}

[SmartEnum<int>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class CyclesPreset {
    public static readonly CyclesPreset Architecture = new(0, 0.5d, 3d, 16, 1);
    public static readonly CyclesPreset Product = new(1, 0d, 20d, 256, 2);

    public double FilterGlossy { get; }
    public double SampleClampIndirect { get; }
    public int AdaptiveMinSamples { get; }
    public int SampleFactor { get; }

    public int QualitySamples(AntialiasLevel level) =>
        SampleFactor * level switch {
            AntialiasLevel.None => 15,
            AntialiasLevel.Draft => 50,
            AntialiasLevel.Good => 500,
            AntialiasLevel.High => 1500,
            _ => throw new UnreachableException(),
        };

    public IO<Unit> Apply(SceneSource source) =>
        Sources.Dictionary(source).Edit(target =>
            (ArchivableDictionaries.Set(target, CyclesKey.RenderPreset.Key, Key).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.FilterGlossy.Key, FilterGlossy).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.SampleClampIndirect.Key, SampleClampIndirect).ToValidation(),
             ArchivableDictionaries.Set(target, CyclesKey.AdaptiveMinSamples.Key, AdaptiveMinSamples).ToValidation())
                .Apply(static (_, _, _, _) => unit)
                .As()
                .ToFin());

    public static CyclesPreset Inferred(GlossyFilter filter, SampleClamp indirect, MinimumSamples minimum) =>
        (filter.ToValue(), indirect.ToValue(), minimum.ToValue()) switch {
            (0d, 0d, _) => Product,
            var signature when signature == (Product.FilterGlossy, Product.SampleClampIndirect, Product.AdaptiveMinSamples) => Product,
            _ => Architecture,
        };

    public static IO<CyclesPreset> Held(SceneSource source) =>
        Sources.Dictionary(source).Read.Bind(static dictionary => IO.lift(() =>
            (Stored<CyclesPreset, int>(dictionary, CyclesKey.RenderPreset.Key), Stored<GlossyFilter, double>(dictionary, CyclesKey.FilterGlossy.Key),
             Stored<SampleClamp, double>(dictionary, CyclesKey.SampleClampIndirect.Key), Stored<MinimumSamples, int>(dictionary, CyclesKey.AdaptiveMinSamples.Key))
                .Apply(static (preset, filter, indirect, minimum) =>
                    preset.IfNone(() => Inferred(filter.IfNone(GlossyFilter.Default), indirect.IfNone(SampleClamp.Default), minimum.IfNone(MinimumSamples.Default))))
                .As()
                .ToFin()));

    private static Validation<Error, Option<T>> Stored<T, TRaw>(ArchivableDictionary dictionary, string key)
        where T : IObjectFactory<T, TRaw, InvalidRhinoValue>
        where TRaw : notnull =>
        ArchivableDictionaries.Find<TRaw>(dictionary, key).Bind(static raw => raw.Traverse(static held => Conversions.Validated<T, TRaw, InvalidRhinoValue>(held)).As()).ToValidation();
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class CyclesKey {
    public static readonly CyclesKey RenderPreset = new Row<CyclesPreset, int>(
        ValueKey.Of<CyclesPreset, int, InvalidRhinoValue>("RenderPreset", "Presets", "Setting to control caustics.", CyclesPreset.Architecture));
    public static readonly CyclesKey Seed = new Row<SamplingSeed, int>(
        ValueKey.Of<SamplingSeed, int, InvalidRhinoValue>("Seed", "Seed", "Set the seed for the random number generator", SamplingSeed.Default));
    public static readonly CyclesKey Samples = new Row<SampleCount, int>(
        ValueKey.Of<SampleCount, int, InvalidRhinoValue>("Samples", "Samples", "Settings for controlling Cycles in a session", SampleCount.Default));
    public static readonly CyclesKey UseDocumentSamples = new Row<bool, bool>(
        ValueKey.Raw("UseDocumentSamples", "Override Production Render Quality", "If unchecked affects only viewport sample count", false));
    public static readonly CyclesKey MaxBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("MaxBounce", "Maximum", "Settings controlling the bounce limits for different types of rays", BounceLimit.Path));
    public static readonly CyclesKey MaxDiffuseBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("MaxDiffuseBounce", "Diffuse", "Settings controlling the bounce limits for different types of rays", BounceLimit.Diffuse));
    public static readonly CyclesKey MaxGlossyBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("MaxGlossyBounce", "Glossy", "Settings controlling the bounce limits for different types of rays", BounceLimit.Glossy));
    public static readonly CyclesKey MaxTransmissionBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("MaxTransmissionBounce", "Transmission", "Settings controlling the bounce limits for different types of rays", BounceLimit.Path));
    public static readonly CyclesKey MaxVolumeBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("MaxVolumeBounce", "Volume", "Settings controlling the bounce limits for different types of rays", BounceLimit.Path));
    public static readonly CyclesKey TransparentMaxBounce = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("TransparentMaxBounce", "Transparency", "Settings controlling the bounce limits for different types of rays", BounceLimit.Path));
    public static readonly CyclesKey TextureBakeQuality = new Row<TextureBake, int>(
        ValueKey.Of<TextureBake, int, InvalidRhinoValue>("TextureBakeQuality", "Texture Bake Quality", "Setting for controlling texture bake resolution", TextureBake.Low));
    public static readonly CyclesKey UseAdaptiveSampling = new Row<bool, bool>(
        ValueKey.Raw("UseAdaptiveSampling", "Adaptive sampling", "Stop sampling pixels that reach the noise threshold", true));
    public static readonly CyclesKey AdaptiveThreshold = new Row<NoiseThreshold, double>(
        ValueKey.Of<NoiseThreshold, double, InvalidRhinoValue>("AdaptiveThreshold", "Adaptive threshold", "Noise level a pixel stops sampling at", NoiseThreshold.Default));
    public static readonly CyclesKey AdaptiveMinSamples = new Row<MinimumSamples, int>(
        ValueKey.Of<MinimumSamples, int, InvalidRhinoValue>("AdaptiveMinSamples", "Adaptive minimum samples", "Samples before adaptive stopping, 0 derives them from the threshold", MinimumSamples.Default));
    public static readonly CyclesKey AoBounces = new Row<BounceLimit, int>(
        ValueKey.Of<BounceLimit, int, InvalidRhinoValue>("AoBounces", "AO bounces", "Bounces before ambient occlusion replaces global illumination", BounceLimit.Off));
    public static readonly CyclesKey SampleClampDirect = new Row<SampleClamp, double>(
        ValueKey.Of<SampleClamp, double, InvalidRhinoValue>("SampleClampDirect", "Direct sample clamp", "Limit on direct light per sample, 0 off", SampleClamp.Default));
    public static readonly CyclesKey SampleClampIndirect = new Row<SampleClamp, double>(
        ValueKey.Of<SampleClamp, double, InvalidRhinoValue>("SampleClampIndirect", "Indirect sample clamp", "Limit on indirect light per sample, 0 off", SampleClamp.Default));
    public static readonly CyclesKey FilterGlossy = new Row<GlossyFilter, double>(
        ValueKey.Of<GlossyFilter, double, InvalidRhinoValue>("FilterGlossy", "Filter glossy", "Blur of glossy paths that suppresses caustic fireflies, 0 off", GlossyFilter.Default));
    public static readonly CyclesKey CausticsReflective = new Row<bool, bool>(
        ValueKey.Raw("CausticsReflective", "Reflective caustics", "Light paths reflected onto diffuse surfaces", true));
    public static readonly CyclesKey CausticsRefractive = new Row<bool, bool>(
        ValueKey.Raw("CausticsRefractive", "Refractive caustics", "Light paths refracted onto diffuse surfaces", true));
    public static readonly CyclesKey UseDirectLight = new Row<bool, bool>(
        ValueKey.Raw("UseDirectLight", "Sample all lights", "Direct light from every light at each bounce", true));
    public static readonly CyclesKey UseIndirectLight = new Row<bool, bool>(
        ValueKey.Raw("UseIndirectLight", "Sample all lights indirect", "Indirect light from every light at each bounce", true));
    public static readonly CyclesKey MaxPasses = new Row<bool, bool>(
        ValueKey.Raw("MaxPasses", "Show max passes", "Raytraced heads-up display shows the pass limit", true));

    public static string Owner => "rhino-render";

    internal abstract (ValueBinding Binding, IO<Unit> Reset) Entry(DictionaryOwner owner);

    private sealed class Row<TValue, TRaw> : CyclesKey where TValue : notnull where TRaw : notnull {
        private readonly ValueKey<TValue, TRaw, InvalidRhinoValue> key;

        public Row(ValueKey<TValue, TRaw, InvalidRhinoValue> key) : base(key.Name) => this.key = key;

        internal override (ValueBinding Binding, IO<Unit> Reset) Entry(DictionaryOwner owner) =>
            ArchivableDictionaries.Store(owner, key, None) switch {
                var store => (ValueBinding.Key(Owner, key, store), store.Write(None).Map(static _ => unit)),
            };
    }
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ThrottleInterval : System.Numerics.IMinMaxValue<ThrottleInterval> {
    public static ThrottleInterval MinValue { get; } = new(0);
    public static ThrottleInterval MaxValue { get; } = new(int.MaxValue);
    public static ThrottleInterval Default { get; } = new(100);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ViewportPixelSize : System.Numerics.IMinMaxValue<ViewportPixelSize> {
    public static ViewportPixelSize MinValue { get; } = new(1);
    public static ViewportPixelSize MaxValue { get; } = new(10);
    public static ViewportPixelSize Full => MinValue;

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LightFactor : System.Numerics.IMinMaxValue<LightFactor> {
    public static LightFactor MinValue { get; } = new(0d);
    public static LightFactor MaxValue { get; } = new(double.MaxValue);
    public static LightFactor Sun { get; } = new(3.2d);
    public static LightFactor Area { get; } = new(17.2d);
    public static LightFactor Linear { get; } = new(10d);
    public static LightFactor Point { get; } = new(40d);
    public static LightFactor Spot { get; } = new(40d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class CyclesSetting {
    public static readonly CyclesSetting ThrottleMs = new Row<ThrottleInterval, int>(
        ValueKey.Of<ThrottleInterval, int, InvalidRhinoValue>("ThrottleMs", "Throttle (in ms)", "Pause between Raytraced viewport updates", ThrottleInterval.Default), SettingType.Integer, Applied.Live);
    public static readonly CyclesSetting SelectedDeviceStr = new Row<string, string>(
        ValueKey.Raw("SelectedDeviceStr", "Device", "Devices Rhino Render runs on, -1 picks the first GPU", "-1"), SettingType.String, Applied.Live);
    public static readonly CyclesSetting IntermediateSelectedDeviceStr = new Row<string, string>(
        ValueKey.Raw("IntermediateSelectedDeviceStr", "Device selection", "Device choice the device section writes with Device", "-1"), SettingType.String, Applied.Live);
    public static readonly CyclesSetting PixelSize = new Row<ViewportPixelSize, int>(
        ValueKey.Of<ViewportPixelSize, int, InvalidRhinoValue>("PixelSize", "Pixel size", "Raytraced pixel size, Sharpness writes 11 minus it", ViewportPixelSize.Full), SettingType.Integer, Applied.Live);
    public static readonly CyclesSetting StartGpuKernelCompiler = new Row<bool, bool>(
        ValueKey.Raw("StartGpuKernelCompiler", "Compile GPU kernels", "Compile GPU kernels when Rhino Render loads", true), SettingType.Bool, Applied.Relaunch);
    public static readonly CyclesSetting UseLightTree = new Row<bool, bool>(
        ValueKey.Raw("UseLightTree", "Light tree", "Sample lights through the light tree", true), SettingType.Bool, Applied.Live);
    public static readonly CyclesSetting SunLightFactor = new Row<LightFactor, double>(
        ValueKey.Of<LightFactor, double, InvalidRhinoValue>("SunLightFactor", "Sun light factor", "Multiplier Rhino Render applies to the sun's intensity", LightFactor.Sun), SettingType.Double, Applied.Live);
    public static readonly CyclesSetting AreaLightFactor = new Row<LightFactor, double>(
        ValueKey.Of<LightFactor, double, InvalidRhinoValue>("AreaLightFactor", "Area light factor", "Multiplier on rectangular light intensity", LightFactor.Area), SettingType.Double, Applied.Live);
    public static readonly CyclesSetting LinearLightFactor = new Row<LightFactor, double>(
        ValueKey.Of<LightFactor, double, InvalidRhinoValue>("LinearLightFactor", "Linear light factor", "Multiplier on linear light intensity", LightFactor.Linear), SettingType.Double, Applied.Live);
    public static readonly CyclesSetting PointLightFactor = new Row<LightFactor, double>(
        ValueKey.Of<LightFactor, double, InvalidRhinoValue>("PointLightFactor", "Point light factor", "Multiplier on point light intensity", LightFactor.Point), SettingType.Double, Applied.Live);
    public static readonly CyclesSetting SpotLightFactor = new Row<LightFactor, double>(
        ValueKey.Of<LightFactor, double, InvalidRhinoValue>("SpotLightFactor", "Spot light factor", "Multiplier on spot light intensity", LightFactor.Spot), SettingType.Double, Applied.Live);

    public static Guid PlugInId { get; } = new(0x9bc28e9e, 0x7a6c, 0x4b8f, 0xa0, 0xc6, 0x3d, 0x05, 0xe0, 0x2d, 0x1b, 0x97);

    public static SettingsNode Node { get; } = new(PlugInId);

    public static string Owner => "rhino-render-options";

    public static Validation<Error, BindingGroup> Bindings => BindingGroup.Of(toSeq(Items).Map(static item => item.Bind()));

    internal abstract ValueBinding Bind();

    private sealed class Row<TValue, TRaw> : CyclesSetting where TValue : notnull where TRaw : notnull {
        private readonly PlugInSetting<TValue, TRaw, InvalidRhinoValue> row;

        public Row(ValueKey<TValue, TRaw, InvalidRhinoValue> key, SettingType<TRaw> kind, Applied applied) : base(key.Name) =>
            row = new(key, kind, applied, Seq<string>(), Hidden: false);

        internal override ValueBinding Bind() => ValueBinding.Key(Owner, row.Value, PlugInSettings.Store(Node, row));
    }
}
