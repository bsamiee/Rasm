using Rasm.Rhino.Persistence.Stores;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Imaging.Filters.Optics;
using Rasm.Rhino;
using Rasm.Rhino.UI.Rows;

using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render;
using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;
using Darkroom.Effects;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rhino.PlugIns;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("1EA59DFD-DF4C-4D26-8A78-8E17DEC6826E")]
public sealed class RenderSetsSection() : DefinedSection(Row) {
    public static readonly View.Section Row = new() {
        Identity = typeof(RenderSetsSection),
        Caption = "Render Sets and Presets",
        Store = Some(RenderSets.Store),
        Children = RenderSets.Rows().Add(PresetBinding.Browser(static scope => {
            const float ntscChromaDelay = 0.003556f;
            const float vhsJitter = 0.003556f;
            const float vhsNoiseLength = 0.07111f;
            ValueSet formed = ValueSet.Fields(EntryKey.TypeOwner(typeof(ImageFormation).GUID),
                ToneMapping.Default with { Selected = Formation.HablePiecewise, Application = CurveApplication.Default with { PreserveHue = Factor.MinValue } },
                [ToneMappingParameter.Formation, ToneMappingParameter.PreserveHue]);
            AnalogVideo television = AnalogVideo.Default with {
                LumaBandwidth = ShortSideLength.Create(0.001363f),
                ChromaBandwidth = ShortSideLength.Create(0.004326f),
                Ringing = EdgeGain.Create(0.4132f),
                RingingDelay = ShortSideOffset.Create(0.004089f),
                LumaNoise = NoiseLevel.Create(0.01155f),
                ChromaNoise = NoiseLevel.Create(0.01732f),
            };
            string videoOwner = EntryKey.TypeOwner(typeof(VideoSignal).GUID);
            string displayOwner = EntryKey.TypeOwner(typeof(TubeDisplay).GUID);
            string posterOwner = EntryKey.TypeOwner(typeof(Posterization).GUID);
            string contrastOwner = EntryKey.TypeOwner(typeof(ContrastSaturation).GUID);
            Seq<IStateParameter<AnalogVideo>> televisionFields = [
                AnalogVideoParameter.LumaBandwidth, AnalogVideoParameter.ChromaBandwidth, AnalogVideoParameter.Ringing,
                AnalogVideoParameter.RingingDelay, AnalogVideoParameter.LumaNoise, AnalogVideoParameter.ChromaNoise,
            ];
            ValueSet tube = ValueSet.Fields(displayOwner, CrtDisplay.Scanlines with { Curvature = Distortion.Create(0.03f) },
                [CrtDisplayParameter.MaskStrength, CrtDisplayParameter.ScanlineDepth, CrtDisplayParameter.Curvature]);
            Seq<Preset> stock = [.. from preset in Seq<(string Name, Exposure Exposure, Contrast Contrast)>(
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
                            ChromaDelay = ShortSideOffset.Create(ntscChromaDelay), Ringing = EdgeGain.Create(1.033f), CrossColor = Mix.Create(0.5f),
                            LumaNoise = NoiseLevel.Create(0.005774f), ChromaNoise = NoiseLevel.Create(0.01905f),
                        }, [.. televisionFields, AnalogVideoParameter.ChromaDelay, AnalogVideoParameter.CrossColor]), tube,
                    ]),
                    Preset.Of(PresetName.Create("VHS"), [
                        ValueSet.Fields(videoOwner, AnalogVideo.Default with {
                            LumaBandwidth = ShortSideLength.Create(0.001896f), ChromaBandwidth = ShortSideLength.Create(0.01126f),
                            Ringing = EdgeGain.Create(0.8264f), RingingDelay = ShortSideOffset.Create(0.005689f), ChromaDelay = ShortSideOffset.Create(0.007111f),
                            LumaNoise = NoiseLevel.Create(0.02309f), ChromaNoise = NoiseLevel.Create(0.04619f), Jitter = ShortSideLength.Create(vhsJitter),
                            HeadSwitch = ShortSideOffset.Create(0.08889f), Snow = Mix.Full, SnowDensity = Likelihood.Create(0.02f),
                            NoiseLength = ShortSideExtent.Create(vhsNoiseLength), SnowLength = ShortSideExtent.Create(vhsNoiseLength),
                        }, [.. televisionFields,
                            AnalogVideoParameter.ChromaDelay, AnalogVideoParameter.Jitter, AnalogVideoParameter.HeadSwitch,
                            AnalogVideoParameter.Snow, AnalogVideoParameter.SnowDensity, AnalogVideoParameter.NoiseLength, AnalogVideoParameter.SnowLength,
                        ]),
                        ValueSet.Fields(displayOwner, CrtDisplay.Scanlines with { ScanlineDepth = Mix.Create(0.2f) }, [CrtDisplayParameter.MaskStrength, CrtDisplayParameter.ScanlineDepth]),
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
                        ValueSet.Fields(contrastOwner, ContrastGrade.Default with { Saturation = Saturation.MinValue }, [ContrastGradeParameter.Saturation]),
                        ValueSet.Fields(posterOwner, Posterize.Default with { Levels = LevelCount.Create(2), DitherStrength = Mix.Full }, [PosterizeParameter.Levels, PosterizeParameter.DitherStrength]),
                    ]),
                ];

            return from history in IO.lift(scope.History.ToFin(new Missing(nameof(RowScope.History))))
                   select new PresetBinding(scope, history.Group,
                       new DirectoryInfo(Path.Combine(((PlugIn)scope.Sink).SettingsDirectory, "Presets", EntryKey.TypeOwner(typeof(RenderSetsSection).GUID))), stock);
        })),
    };
}

[Guid("5D74DDAF-123D-4EBA-963B-9602927998E6")]
public sealed class RenderSetsSnapshots(RenderSets sets)
    : DefinedSnapshotClient(new SnapshotDefinition("Render Sets", RenderingCategory, Document: Some(sets.Snapshot)));
