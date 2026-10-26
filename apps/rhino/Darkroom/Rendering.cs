using System.Runtime.InteropServices;
using Darkroom.Effects;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.UI.Imaging;
using Rhino.UI.Controls;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("1EA59DFD-DF4C-4D26-8A78-8E17DEC6826E")]
internal static class LooksSection {
    // --- [SETTINGS]
    public static readonly TileLimitRows Settings = new(
        Batch: new(ValueKey.Numeric("PreviewBatch", "Tiles per tick", "Preview tiles formed in one scheduling tick", TileLimits.Default.Batch,
            new NumericDomain<int, int>(Fin.Succ, identity, (1, 8), new() { Form = NumberForm.Field })), SettingType.Integer),
        Budget: new(ValueKey.Numeric("PreviewBudget", "Time budget", "Time limit for preview tiles per scheduling tick", (int)TileLimits.Default.Budget.TotalMilliseconds,
            new NumericDomain<int, int>(Fin.Succ, identity, (20, 2000), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Millisecond) })), SettingType.Integer),
        Capacity: new(ValueKey.Numeric("PreviewCapacity", "Cached tiles", "Preview tiles kept while offscreen", TileLimits.Default.Capacity,
            new NumericDomain<int, int>(Fin.Succ, identity, (16, 4096), new() { Form = NumberForm.Field })), SettingType.Integer));

    public static Seq<Child> Page => [
        new ControlRow.Group("Preview tiles", RowRules.Always),
        .. Seq(Settings.Batch, Settings.Budget, Settings.Capacity).Bind(static row => SectionRows.Setting(row, RowRules.Always)),
    ];

    // --- [PANEL]
    public static readonly Func<EtoPostEffectCollapsibleSection, IO<Option<ScopeFrames>>, View.Section> Row = static (section, _) => new() {
        Identity = typeof(LooksSection),
        Caption = "Looks",
        Actions = [ListRows.Arrangement(BrowserMode.Grid)],
        Children = [PresetBinding.Browser(new EffectSource.Section(section), EntryKey.TypeOwner(typeof(LooksSection).GUID), Stock)],
    };

    private static readonly Seq<Preset> Stock = Stocked();

    private static Seq<Preset> Stocked() {
        const float ntscChromaDelay = 0.003556f;
        const float vhsJitter = 0.003556f;
        const float vhsNoiseLength = 0.07111f;
        ValueSet formed = ValueSet.Fields(EntryKey.TypeOwner(typeof(ImageFormation).GUID),
            ToneMapping.Default with { Selected = Formation.HablePiecewise, Application = CurveApplication.Default with { PreserveHue = AxisFraction.MinValue } },
            [ToneMappingParameter.Formation, ToneMappingParameter.PreserveHue]);
        AnalogVideo television = AnalogVideo.Default with {
            LumaBandwidth = ShortSideLength.Create(0.001363f),
            ChromaBandwidth = ShortSideLength.Create(0.004326f),
            Ringing = Magnitude.Create(0.4132f),
            RingingDelay = ShortSideOffset.Create(0.004089f),
            LumaNoise = Magnitude.Create(0.01155f),
            ChromaNoise = Magnitude.Create(0.01732f),
        };
        string videoOwner = EntryKey.TypeOwner(typeof(VideoSignal).GUID);
        string displayOwner = EntryKey.TypeOwner(typeof(TubeDisplay).GUID);
        string posterOwner = EntryKey.TypeOwner(typeof(Posterization).GUID);
        string contrastOwner = EntryKey.TypeOwner(typeof(ContrastSaturation).GUID);
        Seq<IStateParameter<AnalogVideo>> televisionFields = [
            AnalogVideoParameter.LumaBandwidth, AnalogVideoParameter.ChromaBandwidth, AnalogVideoParameter.Ringing,
            AnalogVideoParameter.RingingDelay, AnalogVideoParameter.LumaNoise, AnalogVideoParameter.ChromaNoise,
        ];
        ValueSet tube = ValueSet.Fields(displayOwner, TubeDisplay.Scanlines with { Curvature = Distortion.Create(0.03f) },
            [CrtDisplayParameter.MaskStrength, CrtDisplayParameter.ScanlineDepth, CrtDisplayParameter.Curvature]);
        return [
            .. from preset in Seq<(string Name, Exposure Exposure, Contrast Contrast)>(
                ("Rhino Filmic, Low Contrast", Exposure.Create(4f), Contrast.Create(0.5f)),
                ("Rhino Filmic, Medium Contrast", Exposure.Create(1f), Contrast.Neutral),
                ("Rhino Filmic, High Contrast", Exposure.Create(-1f), Contrast.Create(2f)))
               select Preset.Of(PresetName.Create(preset.Name), [
                   ValueSet.Fields(EntryKey.TypeOwner(typeof(SceneExposure).GUID), ExposureState.Default with { Exposure = preset.Exposure }, [ExposureParameter.Exposure]),
                   formed,
                   ValueSet.Fields(contrastOwner, ContrastGrade.Default with { Contrast = preset.Contrast }, [ContrastGradeParameter.Contrast]),
               ]),
            Preset.Of(PresetName.Create("Television"), [ValueSet.Fields(videoOwner, television, televisionFields), tube]),
            Preset.Of(PresetName.Create("NTSC"), [
                ValueSet.Fields(videoOwner, television with {
                    ChromaDelay = ShortSideOffset.Create(ntscChromaDelay), Ringing = Magnitude.Create(1.033f), CrossColor = AxisFraction.Create(0.5f),
                    LumaNoise = Magnitude.Create(0.005774f), ChromaNoise = Magnitude.Create(0.01905f),
                }, [.. televisionFields, AnalogVideoParameter.ChromaDelay, AnalogVideoParameter.CrossColor]), tube,
            ]),
            Preset.Of(PresetName.Create("VHS"), [
                ValueSet.Fields(videoOwner, AnalogVideo.Default with {
                    LumaBandwidth = ShortSideLength.Create(0.001896f), ChromaBandwidth = ShortSideLength.Create(0.01126f),
                    Ringing = Magnitude.Create(0.8264f), RingingDelay = ShortSideOffset.Create(0.005689f), ChromaDelay = ShortSideOffset.Create(0.007111f),
                    LumaNoise = Magnitude.Create(0.02309f), ChromaNoise = Magnitude.Create(0.04619f), Jitter = ShortSideLength.Create(vhsJitter),
                    HeadSwitch = ShortSideOffset.Create(0.08889f), Snow = AxisFraction.MaxValue, SnowDensity = AxisFraction.Create(0.02f),
                    NoiseLength = ShortSideExtent.Create(vhsNoiseLength), SnowLength = ShortSideExtent.Create(vhsNoiseLength),
                }, [.. televisionFields,
                    AnalogVideoParameter.ChromaDelay, AnalogVideoParameter.Jitter, AnalogVideoParameter.HeadSwitch,
                    AnalogVideoParameter.Snow, AnalogVideoParameter.SnowDensity, AnalogVideoParameter.NoiseLength, AnalogVideoParameter.SnowLength,
                ]),
                ValueSet.Fields(displayOwner, TubeDisplay.Scanlines with { ScanlineDepth = AxisFraction.Create(0.2f) }, [CrtDisplayParameter.MaskStrength, CrtDisplayParameter.ScanlineDepth]),
            ]),
            Preset.Of(PresetName.Create("Comic"), [
                ValueSet.Fields(posterOwner, Posterize.Default with { Levels = LevelCount.Create(3) }, [PosterizeParameter.Levels]),
                ValueSet.Fields(EntryKey.TypeOwner(typeof(HalftoneScreen).GUID), Halftone.Default with { Mode = BlendingMode.Multiply }, [HalftoneParameter.Mode]),
            ]),
            Preset.Of(PresetName.Create("Game Kid"), [
                ValueSet.Fields(EntryKey.TypeOwner(typeof(Pixelation).GUID), Mosaic.Default, [MosaicParameter.Size]),
                ValueSet.Fields(EntryKey.TypeOwner(typeof(CustomPalette).GUID), PaletteMapping.Default with {
                    Palette = Palette.Create(Seq(Swatch.Create(0x081820), Swatch.Create(0x346856), Swatch.Create(0x88C070), Swatch.Create(0xE0F8D0))),
                    Dithering = PaletteDither.Bayer8,
                }, [PaletteMappingParameter.Palette, PaletteMappingParameter.Dithering]),
            ]),
            Preset.Of(PresetName.Create("Bayer Dither"), [
                ValueSet.Fields(contrastOwner, ContrastGrade.Default with { Saturation = Magnitude.Zero }, [ContrastGradeParameter.Saturation]),
                ValueSet.Fields(posterOwner, Posterize.Default with { Levels = LevelCount.Create(2), DitherStrength = AxisFraction.MaxValue }, [PosterizeParameter.Levels, PosterizeParameter.DitherStrength]),
            ]),
        ];
    }
}
