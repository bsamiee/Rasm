using Darkroom.Effects;
using Rasm.Imaging.Filters.Optics;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino.PlugIns;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    Settings = QueueSettingRows.Default.Rows + HistorySettingRows.Default.Rows + ComparisonSettingRows.Default.Rows + TileLimitRows.Default.Rows,
    History = HistorySettingRows.Default.Limits,
    Clock = TimeProvider.System,
    Logging = [Queues.Logging],
    Commands = [
        static sink => new PanelCommand(sink, new Guid(0xd19b5cfb, 0x5262, 0x4f60, 0x86, 0xeb, 0xdd, 0x8e, 0x34, 0x98, 0x8d, 0xed), DarkroomPanel.Row),
        static sink => new QueueJobCommand(sink, new Guid(0x4a60fdca, 0x6daf, 0x4fef, 0x8d, 0x5e, 0xfd, 0xc7, 0x71, 0x63, 0x9f, 0xee), "DarkroomRenderQueue"),
        static sink => new QueueFarmJobCommand(sink, new Guid(0x3857668a, 0x1b07, 0x4ae9, 0x97, 0x13, 0x63, 0x33, 0xb3, 0xca, 0x46, 0xee), "DarkroomRenderFarm"),
        static sink => new FrameExportCommand(sink, new Guid(0x66a517fb, 0x68c5, 0x42c9, 0x86, 0xe4, 0x30, 0x01, 0x03, 0x63, 0xa4, 0x57), "DarkroomExportStMap",
            static (settings, extent) => EffectRegistry.State<RadialDistortion, LensDistortion>(settings)
                .Map(state => state.Placed.Bake(extent))),
        static sink => new LutExportCommand(sink, new Guid(0x09625191, 0xac85, 0x44eb, 0xa7, 0x67, 0x23, 0x07, 0x6c, 0x4c, 0x64, 0xcf), "DarkroomExportLut"),
    ],
    Views = [DarkroomPanel.Row, DarkroomPage.Row, OutputPage.Row],
    Acquisitions = [
        EffectRegistry.Register(Seq<EffectKind>()
            .Add<GuidedDenoise>(BuiltinEffect.Denoise)
            .Add<NightSky>()
            .Add<Atmosphere>()
            .Add<LensBlur>()
            .Add<NoiseLayer>()
            .Add<Flames>()
            .Add<Particles>()
            .Add<LightningBolt>()
            .Add<SunBeams>()
            .Add<SourceFlare>()
            .Add<ViewReprojection>()
            .Add<Swirl>()
            .Add<Spherize>()
            .Add<WaveWarp>()
            .Add<TurbulentDisplace>()
            .Add<Displace>()
            .Add<Glass>()
            .Add<Kaleida>()
            .Add<PolarWarp>()
            .Add<ImageTransform>()
            .Add<CornerPin2D>()
            .Add<CameraShake>()
            .Add<RadialDistortion>()
            .Add<AxialFringing>()
            .Add<AdaptiveSharpening>()
            .Add<LensBloom>()
            .Add<VeilingFlare>()
            .Add<ApertureDiffraction>()
            .Add<LensReflections>()
            .Add<NaturalVignetting>()
            .Add<FilmHalation>()
            .Add<SceneExposure>()
            .Add<ProjectorFlicker>()
            .Add<EmulsionGrain>()
            .Add<ImageNoise>()
            .Add<WhiteBalanceCorrection>()
            .Add<ChannelMixer>()
            .Add<ColorWheels>()
            .Add<ColorDecisionList>()
            .Add<ContrastSaturation>()
            .Add<SceneLevels>()
            .Add<RgbCurves>()
            .Add<HueCorrection>()
            .Add<ColorQualifier>()
            .Add<ColorIsolation>()
            .Add<GradientMapping>()
            .Add<ReferenceGamutCompression>()
            .Add<SceneLook>()
            .Add<LocalExposure>()
            .Add<ImageFormation>()
            .Add<DisplayLook>()
            .Add<Pixelation>()
            .Add<Posterization>()
            .Add<AdaptivePalette>()
            .Add<CustomPalette>()
            .Add<HalftoneScreen>()
            .Add<GlyphGrid>()
            .Add<PixelSorting>()
            .Add<PaintFilter>()
            .Add<EdgeFilter>()
            .Add<SceneOutline>()
            .Add<FilmWear>()
            .Add<JpegArtifacts>()
            .Add<DigitalGlitch>()
            .Add<VideoSignal>()
            .Add<TubeDisplay>()
            .Add<GradientLayer>()
            .Add<FourCornerGradient>()
            .Add<Bands>()
            .Add<GridLines>()
            .Add<ShapeLayer>()
            .Add<AspectBars>()
            .Add<OutputDither>(BuiltinEffect.Dithering)),
        Framing.Extent.Attach,
        TargetRows.Shown.Attach,
        RenderHistory.Register(HistorySection.Row, static history => [RenderingComparison.Register(history), .. RenderSnapshots.Clients(history)]),
        PreviewTiles.Register(Some(TileLimitRows.Default), static _ => []),
        Queues.Register(QueueSettingRows.Default, DarkroomPanel.Row),
    ],
});
