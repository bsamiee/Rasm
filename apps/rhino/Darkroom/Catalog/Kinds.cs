using Darkroom.Chain;
using Rasm.UI;
using Rhino.Render.PostEffects;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Family : ICaptioned {
    public static readonly Family Generators = new("generators");
    public static readonly Family Camera = new("camera");
    public static readonly Family Optics = new("optics");
    public static readonly Family Warp = new("warp");
    public static readonly Family Grade = new("grade");
    public static readonly Family Detail = new("detail");
    public static readonly Family Stylize = new("stylize");
    public static readonly Family Formation = new("formation");
    public static readonly Family Overlays = new("overlays");
    public static readonly Family Quantization = new("quantization");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class Kinds {
    public static Seq<EffectKind> Ordered { get; } = [
        EffectKind.Job<Denoise, DenoiseJob>(new("7465f313-020e-49ce-acd3-58ade34c750a"), PostEffectType.Early, Family.Detail, new() { Mix = None }),
        EffectKind.Of(new("5bfb53d0-6c96-4fc2-bf47-fd7662e4182b"), PostEffectType.Early, Family.Generators, new EffectRow<Starfield>()),
        EffectKind.Of(new("a65107ea-d4f1-4b61-ac09-b74d67ba8935"), PostEffectType.Early, Family.Generators, new EffectRow<Fog> { Looks = Fog.Looks }),
        EffectKind.Of(new("77743df8-86ad-48bc-b60a-a08a5dc51818"), PostEffectType.Early, Family.Generators, new EffectRow<NoiseTexture> { Looks = NoiseTexture.Looks }),
        EffectKind.Of(new("36ac0f92-c557-4d37-a728-cb2249510efe"), PostEffectType.Early, Family.Generators, new EffectRow<NoiseTexture> { Default = NoiseTexture.Fire }),
        EffectKind.Of(new("31555b4a-ff97-4387-946f-e7e9b0a131e8"), PostEffectType.Early, Family.Generators, new EffectRow<ParticleField> { Looks = ParticleField.Looks }),
        EffectKind.Of(new("60ad6377-8592-43dd-acd2-cc3ef03f7b5f"), PostEffectType.Early, Family.Generators, new EffectRow<Lightning>()),
        EffectKind.Of(new("ff29c5af-33af-4817-90a1-910fdbb21909"), PostEffectType.Early, Family.Generators, new EffectRow<LightRays>()),
        EffectKind.Of(new("05985670-b6cb-40da-9e8e-0f6ac2d30a70"), PostEffectType.Early, Family.Generators, new EffectRow<LensFlare> { Looks = LensFlare.Looks }),
        EffectKind.Of(new("3213d575-58cc-4a78-b13a-d130c437f053"), PostEffectType.Early, Family.Camera, new EffectRow<SceneExposure>()),
        EffectKind.Of(new("c32e1536-d129-432d-b5ae-6da715024b02"), PostEffectType.Early, Family.Camera, new EffectRow<WhiteBalance> { Looks = WhiteBalance.Looks }),
        EffectKind.Of(new("7400690e-f17b-4539-a88a-38b5bfa154fd"), PostEffectType.Early, Family.Optics, new EffectRow<Defocus>()),
        EffectKind.Of(new("944522ca-dde2-4e3e-81b0-82565728951b"), PostEffectType.Early, Family.Optics, new EffectRow<LensDistortion>()),
        EffectKind.Of(new("106bf35b-4759-4e08-a0a3-38f02c7be061"), PostEffectType.Early, Family.Optics, new EffectRow<AxialAberration>()),
        EffectKind.Of(new("b8bf3b1d-0d30-4019-b5d5-417dba7936db"), PostEffectType.Early, Family.Optics, new EffectRow<Bloom>()),
        EffectKind.Of(new("134e641a-47aa-4f42-9084-8040f3568e94"), PostEffectType.Early, Family.Optics, new EffectRow<VeilingGlare>()),
        EffectKind.Of(new("166dcfdb-9388-4cde-ac2b-2c5fe76e4604"), PostEffectType.Early, Family.Optics, new EffectRow<StarGlare>()),
        EffectKind.Of(new("d3df8c60-5cc3-4c70-bbf7-2996bd2d7fd0"), PostEffectType.Early, Family.Optics, new EffectRow<Ghosts>()),
        EffectKind.Of(new("f4afa240-2e97-469d-8f23-ce02aab2f499"), PostEffectType.Early, Family.Optics, new EffectRow<Vignette>()),
        EffectKind.Of(new("6a47cbbf-f6a2-4b53-9b6d-6ccd6c5ede48"), PostEffectType.Early, Family.Optics, new EffectRow<Halation>()),
        EffectKind.Of(new("f7d32275-fcbc-4c8a-ad36-8826d09678a5"), PostEffectType.Early, Family.Warp, new EffectRow<Panorama>()),
        EffectKind.Of(new("30219496-aff4-482e-91cc-8059f0a1532e"), PostEffectType.Early, Family.Warp, new EffectRow<Twirl>()),
        EffectKind.Of(new("44c0cd28-8f52-44c7-b508-946cf6cd29cc"), PostEffectType.Early, Family.Warp, new EffectRow<Bulge>()),
        EffectKind.Of(new("651cc9ac-db9d-48b7-935b-142916a94f16"), PostEffectType.Early, Family.Warp, new EffectRow<WaveWarp>()),
        EffectKind.Of(new("3017b4d2-68e3-46f7-984e-daafecefe685"), PostEffectType.Early, Family.Warp, new EffectRow<Turbulence>()),
        EffectKind.Of(new("3833180c-a491-44a2-961d-a875bc57874b"), PostEffectType.Early, Family.Warp, new EffectRow<MapDisplacement>()),
        EffectKind.Of(new("52c0caf8-262f-49d1-90d0-0bac92cedbc8"), PostEffectType.Early, Family.Warp, new EffectRow<Refraction>()),
        EffectKind.Of(new("d21a93e6-91f0-497e-b9f9-21f030037564"), PostEffectType.Early, Family.Warp, new EffectRow<Kaleidoscope>()),
        EffectKind.Of(new("1f0dfd6b-db2d-41e6-8b94-0bc199d18cab"), PostEffectType.Early, Family.Warp, new EffectRow<PolarCoordinates>()),
        EffectKind.Of(new("2840e7bd-5137-4e50-99c2-82efcea276b7"), PostEffectType.Early, Family.Warp, new EffectRow<AffineTransform>()),
        EffectKind.Of(new("106fb69d-480f-430a-95f0-cc9ee4bea176"), PostEffectType.Early, Family.Warp, new EffectRow<CornerPin>()),
        EffectKind.Of(new("4c2a5565-2369-462c-91cf-2b52ce565040"), PostEffectType.Early, Family.Warp, new EffectRow<CameraShake>()),
        EffectKind.Of(new("570b0a76-a94d-4749-b0a7-b9ec2e1fc36f"), PostEffectType.Early, Family.Grade, new EffectRow<ChannelMixer>()),
        EffectKind.Of(new("556b0475-308d-4a3d-b366-f541d6666a00"), PostEffectType.Early, Family.Grade, new EffectRow<ColorWheels>()),
        EffectKind.Of(new("8e3a41fa-43cb-4d27-a488-922a226a0e8a"), PostEffectType.Early, Family.Grade, new EffectRow<ColorDecisionList>()),
        EffectKind.Of(new("c75e00ae-77a6-4a7f-9556-a2aa0d59a883"), PostEffectType.Early, Family.Grade, new EffectRow<ContrastGrade>()),
        EffectKind.Of(new("485d8eb9-7e34-4c31-99c7-8c4aee2714dd"), PostEffectType.Early, Family.Grade, new EffectRow<Levels>()),
        EffectKind.Of(new("b10c1e89-0353-49ca-9e6e-5b9dc0c0c82c"), PostEffectType.Early, Family.Grade, new EffectRow<PointCurveGrade>()),
        EffectKind.Of(new("c619d89b-47ee-4f89-bf6d-9a43711af308"), PostEffectType.Early, Family.Grade, new EffectRow<HueCurveGrade>()),
        EffectKind.Of(new("e330c3e8-bff0-452a-87d7-fb23a5ec7fbe"), PostEffectType.Early, Family.Grade, new EffectRow<SelectiveColor>()),
        EffectKind.Of(new("7d1e5a92-c64b-4f38-9e07-b3a28c51d6f4"), PostEffectType.Early, Family.Grade, new EffectRow<HueIsolation>()),
        EffectKind.Of(new("b278e80b-5018-4dbc-9910-e477e852ff02"), PostEffectType.Early, Family.Grade, new EffectRow<GradientMapping>()),
        EffectKind.Of(new("2f6b8d41-93c7-4e0a-b5d2-7a1c64e9f308"), PostEffectType.Early, Family.Grade, new EffectRow<ReferenceGamutCompression>()),
        EffectKind.Of(new("91b7f223-a7ab-4cec-aa60-99755bdd23d6"), PostEffectType.Early, Family.Grade, new EffectRow<LutLook>()),
        EffectKind.Of(new("ce14bc1b-2e14-41c0-91d5-54e4886e3d1d"), PostEffectType.Early, Family.Detail, new EffectRow<Sharpen>()),
        EffectKind.Of(new("b03f5ab9-d399-4e1e-95ac-ece13e441ced"), PostEffectType.Early, Family.Detail, new EffectRow<FilmGrain>()),
        EffectKind.Of(new("a9cab48c-9612-4712-85f5-eafd71be51b6"), PostEffectType.Early, Family.Detail, new EffectRow<SensorNoise>()),
        EffectKind.Of(new("49dd6485-0396-4b83-8744-516aecbffe53"), PostEffectType.Early, Family.Detail, new EffectRow<LocalContrast>()),
        EffectKind.Of(new("72443a3b-a5f1-4018-8368-7ad55d5f6127"), PostEffectType.Early, Family.Stylize, new EffectRow<FilmFlicker>()),
        EffectKind.Of(new("72433230-65e7-4c28-b1a9-0afa4e6f32e2"), PostEffectType.ToneMapping, Family.Formation, new EffectRow<ImageFormation> { Mix = None, Looks = ImageFormation.Looks, Listed = true }),
        EffectKind.Of(new("d8948345-e067-46c3-9e56-52229c540f9c"), PostEffectType.Late, Family.Quantization, new EffectRow<LutLook>()),
        EffectKind.Of(new("3f5e6947-cf6e-4623-a607-5ddda6e05486"), PostEffectType.Late, Family.Stylize, new EffectRow<Mosaic>()),
        EffectKind.Of(new("6666a316-3ff8-49ce-a566-34d9531372ea"), PostEffectType.Late, Family.Stylize, new EffectRow<Posterize>()),
        EffectKind.Of(new("b0fd305b-80fb-4726-ab16-ca4223c7d61c"), PostEffectType.Late, Family.Stylize, new EffectRow<PaletteReduction>()),
        EffectKind.Of(new("fc33be5b-eb04-4bb7-b890-89e5778451c9"), PostEffectType.Late, Family.Stylize, new EffectRow<PaletteMapping>()),
        EffectKind.Of(new("d0635c79-6508-4cc8-85fc-4fbc0af55405"), PostEffectType.Late, Family.Stylize, new EffectRow<Halftone>()),
        EffectKind.Of(new("1aa387a7-1493-4d9f-aeb2-0c9a1b5773b8"), PostEffectType.Late, Family.Stylize, new EffectRow<GlyphCells> { Looks = GlyphCells.Looks }),
        EffectKind.Of(new("d9419256-acc5-4728-b874-d9b529d5cab4"), PostEffectType.Late, Family.Stylize, new EffectRow<PixelSort>()),
        EffectKind.Of(new("dbf0553d-7010-4f70-b0c8-1df82c2b722b"), PostEffectType.Late, Family.Stylize, new EffectRow<Painterly>()),
        EffectKind.Of(new("965adbbd-2586-4504-8fc7-efff82b7983c"), PostEffectType.Late, Family.Stylize, new EffectRow<EdgeDetect>()),
        EffectKind.Of(new("8a1307d8-a9b1-4d95-b44c-64e8ab72b3d4"), PostEffectType.Late, Family.Stylize, new EffectRow<GeometryOutline>()),
        EffectKind.Of(new("2c3dfe8b-89b8-42d4-9bab-7c1f920b02e5"), PostEffectType.Late, Family.Stylize, new EffectRow<FilmDamage>()),
        EffectKind.Of(new("de4f9efc-1a2c-4668-814f-0f280ded7917"), PostEffectType.Late, Family.Stylize, new EffectRow<CodecDamage> { Looks = CodecDamage.Looks }),
        EffectKind.Of(new("50657d74-77ee-4dc0-99c9-01b94b4ff0d0"), PostEffectType.Late, Family.Stylize, new EffectRow<Glitch> { Looks = Glitch.Looks }),
        EffectKind.Of(new("c33c6983-8c08-4ace-b768-dd7988d96500"), PostEffectType.Late, Family.Stylize, new EffectRow<AnalogVideo> { Looks = AnalogVideo.Looks }),
        EffectKind.Of(new("0bfccb12-f88d-4843-8836-e91acfd439a2"), PostEffectType.Late, Family.Stylize, new EffectRow<CrtDisplay> { Looks = CrtDisplay.Looks }),
        EffectKind.Of(new("99007c9c-22fe-4569-a242-ae2392a3a08d"), PostEffectType.Late, Family.Overlays, new EffectRow<Gradient>()),
        EffectKind.Of(new("cd4ad9d9-44cd-49d1-ad8f-a332aadd6d8d"), PostEffectType.Late, Family.Overlays, new EffectRow<CornerGradient>()),
        EffectKind.Of(new("45ae93a8-0400-486a-af24-b70e9f05598d"), PostEffectType.Late, Family.Overlays, new EffectRow<Stripes>()),
        EffectKind.Of(new("fc4fd80c-73d9-4184-bad1-a759b4182f60"), PostEffectType.Late, Family.Overlays, new EffectRow<Grid>()),
        EffectKind.Of(new("e7c2ca0d-f654-4980-b540-2314d6c58de2"), PostEffectType.Late, Family.Overlays, new EffectRow<Shapes>()),
        EffectKind.Of(new("1f4355d5-07b5-417c-8ff7-4373e3efc668"), PostEffectType.Late, Family.Overlays, new EffectRow<Letterbox> { Looks = Letterbox.Looks }),
        EffectKind.Of(new("f2d92349-c684-4276-86dd-587c71634aff"), PostEffectType.Late, Family.Quantization, new EffectRow<OutputDither> { Listed = true }),
    ];
}
