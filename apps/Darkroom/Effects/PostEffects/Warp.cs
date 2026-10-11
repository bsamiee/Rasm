using Rasm.Imaging.Filters.Generators;

using Rasm.Imaging.Filters.Warp;
using Rasm.Rhino.Render.Effects;

using System.Runtime.InteropServices;
using Rasm.Imaging.Pixels;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class WarpText {
    public static ParameterText CenterX => ParameterText.Of("Center X", "Center's place across the frame, rightward from the left edge");
    public static ParameterText CenterY => ParameterText.Of("Center Y", "Center's place down the frame from the top edge");
    public static ParameterText AmountX => ParameterText.Of("Amount X", "Horizontal move at full field strength, a share of the frame's short side");
    public static ParameterText AmountY => ParameterText.Of("Amount Y", "Vertical move at full field strength, a share of the frame's short side");
    public static ParameterText Shutter => ParameterText.Of("Shutter", "Seconds the exposure samples along its motion");
    public static ParameterText ShutterPosition => ParameterText.Of("Shutter Position", "Share of the exposure before the current time, 0 opening now and 1 closing now");
    public static ParameterText Sampling => ParameterText.Choice<Sampling, InvalidWarp>(
        "Interpolation", "Filter reading the source between its pixels", static sampling => sampling.Map(nearest: "Nearest", linear: "Bilinear", cubic: "Cubic"));
    public static ParameterText Wrap => ParameterText.Choice<WrapMode, InvalidPixelValue>("Extension", "Source read past the frame's edge", ToneText.WrapText);
    public static ParameterText Shake => ParameterText.Of("Shake", "Seeded camera motion over time") with {
        Parameters = ParameterText.Fields<Shake, ShakeParameter, InvalidWarp>(static parameter => parameter.Map(
            amplitudeX: ParameterText.Of("Amplitude X", "Largest horizontal shake as a share of the frame's short side"),
            amplitudeY: ParameterText.Of("Amplitude Y", "Largest vertical shake as a share of the frame's short side"),
            rotation: ParameterText.Of("Rotation", "Largest shake turn"),
            zoom: ParameterText.Of("Zoom", "Largest shake zoom factor, in and out alike"),
            rate: ParameterText.Of("Frequency", "Shakes per second"),
            octaves: ParameterText.Of("Octaves", "Detail of the shake across scales") with {
                Parameters = ParameterText.Fields<Octaves, OctavesParameter, InvalidGenerator>(static item => NoiseText.Octaves(item)),
            },
            seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold)),
    };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("f7d32275-fcbc-4c8a-ad36-8826d09678a5")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ViewReprojection : StageEffect<ViewReprojection, Panorama, PanoramaParameter, InvalidWarp>, IEffectDefinition<Panorama, PanoramaParameter> {
    public const string Caption = "Reprojection";

    public static EffectRow<Panorama, PanoramaParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            output: ParameterText.Choice<PanoramaOutput, InvalidWarp>(
                "Projection", "Projection the rendered view is placed into",
                static output => output.Map(
                    rectilinear: "Rectilinear", equirectangular: "Equirectangular", cylindrical: "Cylindrical", fisheye: "Fisheye Equidistant",
                    equisolid: "Fisheye Equisolid", mirrorBall: "Mirror Ball", stereographic: "Stereographic", panini: "Panini")),
            view: ParameterText.Of("View", "Orientation of the rendered view") with {
                Parameters = ParameterText.Fields<PanoramaView, PanoramaViewParameter, InvalidPixelValue>(static item => item.Map(
                    heading: ParameterText.Of("Heading", "Turn of the view to the right"),
                    elevation: ParameterText.Of("Elevation", "Tilt of the view upward"),
                    roll: ParameterText.Of("Roll", "Turn of the view about its axis, counterclockwise"))),
            },
            field: ParameterText.Of("Field of View", "Horizontal angle a Panini output spans across the frame"),
            sweep: ParameterText.Of("Sweep", "Angle a cylindrical, fisheye, or stereographic output spans across the frame"),
            fullTurn: ParameterText.Of("Full Turn Radius", "Radius of the equisolid projection's full turn as a share of the inscribed circle"),
            height: ParameterText.Of("Height", "Vertical offset of the cylindrical viewing window"),
            distance: ParameterText.Of("Distance", "Panini distance, 0 rectilinear and 1 the Panini projection"),
            compression: ParameterText.Of("Compression", "Panini vertical compression straightening the verticals"),
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("30219496-aff4-482e-91cc-8059f0a1532e")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Swirl : StageEffect<Swirl, Twirl, TwirlParameter, InvalidWarp>, IEffectDefinition<Twirl, TwirlParameter> {
    public const string Caption = "Twirl";

    public static EffectRow<Twirl, TwirlParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            angle: ParameterText.Of("Angle", "Turn at the center, falling to none at the radius, counterclockwise"),
            radius: ParameterText.Of("Radius", "Reach of the twirl as a share of the frame's short side"),
            falloff: ParameterText.Of("Falloff", "Exponent of the turn's fall from center to radius, 1 linear"),
            arms: ParameterText.Of("Arms", "Spiral arms the turn gathers into, 0 turning every direction alike"),
            armOffset: ParameterText.Of("Arm Offset", "Turn of the arms about the center"),
            centerX: WarpText.CenterX, centerY: WarpText.CenterY, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("44c0cd28-8f52-44c7-b508-946cf6cd29cc")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Spherize : StageEffect<Spherize, Bulge, BulgeParameter, InvalidWarp>, IEffectDefinition<Bulge, BulgeParameter> {
    public const string Caption = "Spherize";

    public static EffectRow<Bulge, BulgeParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            amount: ParameterText.Of("Amount", "Bulge toward the viewer above 0 and pinch below, 1 and −1 a full hemisphere"),
            mode: ParameterText.Choice<BulgeMode, InvalidWarp>("Mode", "Axes the bulge acts along", static mode => mode.Map(round: "Normal", horizontal: "Horizontal Only", vertical: "Vertical Only")),
            radius: ParameterText.Of("Radius", "Radius of the bulged disc as a share of the frame's short side"),
            centerX: WarpText.CenterX, centerY: WarpText.CenterY, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("651cc9ac-db9d-48b7-935b-142916a94f16")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class WaveWarp : StageEffect<WaveWarp, Wave, WaveParameter, InvalidWarp>, IEffectDefinition<Wave, WaveParameter> {
    public const string Caption = "Wave";

    public static EffectRow<Wave, WaveParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            front: ParameterText.Choice<WaveFront, InvalidWarp>(
                "Form", "Straight waves, or rings about the center moving outward, around it, or both",
                static front => front.Map(linear: "Straight", radial: "Out From Center", zigzag: "Around Center", pond: "Pond Ripples")),
            profile: ParameterText.Choice<WaveProfile, InvalidGenerator>("Profile", "Shape of each wave period", NoiseText.Profiles),
            count: ParameterText.Of("Generators", "Waves summed into the field"),
            minWavelength: ParameterText.Of("Wavelength Min", "Shortest wave as a share of the frame's short side"),
            maxWavelength: ParameterText.Of("Wavelength Max", "Longest wave as a share of the frame's short side"),
            minAmplitude: ParameterText.Of("Amplitude Min", "Weakest wave as a share of the amounts"),
            maxAmplitude: ParameterText.Of("Amplitude Max", "Strongest wave as a share of the amounts"),
            horizontal: WarpText.AmountX, vertical: WarpText.AmountY,
            direction: ParameterText.Of("Direction", "Travel of straight waves, counterclockwise from rightward"),
            spread: ParameterText.Of("Spread", "Fan of directions the waves take about the direction"),
            phase: ParameterText.Of("Phase", "Turn added to every wave period"),
            radius: ParameterText.Of("Radius", "Reach of rings as a share of the frame's short side, fading to none there"),
            centerX: WarpText.CenterX, centerY: WarpText.CenterY,
            rate: ParameterText.Of("Rate", "Wave periods traveled per second"),
            seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("3017b4d2-68e3-46f7-984e-daafecefe685")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class TurbulentDisplace : StageEffect<TurbulentDisplace, Turbulence, TurbulenceParameter, InvalidWarp>, IEffectDefinition<Turbulence, TurbulenceParameter> {
    public const string Caption = "Turbulent Displace";

    public static EffectRow<Turbulence, TurbulenceParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            basis: ParameterText.Of("Noise", "Basis of the displacement field") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static item => NoiseText.Basis(item)),
            },
            scaleX: ParameterText.Of("Size X", "Horizontal noise feature size as a share of the frame's short side"),
            scaleY: ParameterText.Of("Size Y", "Vertical noise feature size as a share of the frame's short side"),
            horizontal: WarpText.AmountX, vertical: WarpText.AmountY,
            rate: ParameterText.Of("Evolution", "Noise units the field evolves per second"),
            seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("3833180c-a491-44a2-961d-a875bc57874b")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Displace : StageEffect<Displace, MapDisplacement, MapDisplacementParameter, InvalidWarp>, IEffectDefinition<MapDisplacement, MapDisplacementParameter> {
    public const string Caption = "Displace";

    public static EffectRow<MapDisplacement, MapDisplacementParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            source: ParameterText.Of("Map Image", "Image file the map reads, the frame itself while empty"),
            reading: ParameterText.Choice<MapReading, InvalidWarp>(
                "Reading", "How the map moves the frame", static reading => reading.Map(gradient: "Luma Gradient", luma: "Luma", offsets: "Red and Green", coordinates: "ST Map")),
            tileSize: ParameterText.Of("Tile Size", "Height of one map tile as a share of the frame's short side, off stretching the image to fit"),
            softness: ParameterText.Of("Softness", "Blur of the map's luma before it moves the frame"),
            horizontal: WarpText.AmountX, vertical: WarpText.AmountY,
            rotation: ParameterText.Of("Rotation", "Turn of the moves the map reads, counterclockwise"),
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("52c0caf8-262f-49d1-90d0-0bac92cedbc8")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Glass : StageEffect<Glass, Refraction, RefractionParameter, InvalidWarp>, IEffectDefinition<Refraction, RefractionParameter> {
    public const string Caption = "Glass";

    public static EffectRow<Refraction, RefractionParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            field: ParameterText.Choice<RefractionField, InvalidWarp>(
                "Texture", "Surface the frame is seen through", static field => field.Map(raindrops: "Raindrops", blocks: "Blocks", canvas: "Canvas", frosted: "Frosted", tinyLens: "Tiny Lens")),
            scale: ParameterText.Of("Scale", "Texture cell size as a share of the frame's short side"),
            strength: ParameterText.Of("Distortion", "Bend of the frame through the surface, below 0 inverting it"),
            smoothness: ParameterText.Of("Smoothness", "Blur of the surface before its slope bends the frame"),
            blur: ParameterText.Of("Blur", "Softening of the frame seen through the surface"),
            density: ParameterText.Of("Density", "Share of cells holding a raindrop"),
            size: ParameterText.Of("Drop Size", "Largest raindrop radius as a share of its texture cell"),
            variation: ParameterText.Of("Variation", "Share by which the seed reduces each raindrop's radius"),
            softness: ParameterText.Of("Drop Softness", "Share of each raindrop's radius blending its rim"),
            lens: ParameterText.Of("Lens", "Magnification of the frame through each raindrop"),
            trails: ParameterText.Of("Trails", "Height of the trail behind each raindrop against the drop"),
            direction: ParameterText.Of("Direction", "Way the raindrops run, counterclockwise from rightward"),
            fall: ParameterText.Of("Fall", "Raindrop travel per second as a share of the frame's short side"),
            seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("d21a93e6-91f0-497e-b9f9-21f030037564")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Kaleida : StageEffect<Kaleida, Kaleidoscope, KaleidoscopeParameter, InvalidWarp>, IEffectDefinition<Kaleidoscope, KaleidoscopeParameter> {
    public const string Caption = "Kaleidoscope";

    public static EffectRow<Kaleidoscope, KaleidoscopeParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            mirrors: ParameterText.Of("Mirrors", "Mirror lines through the center, drawing twice as many wedges"),
            rotation: ParameterText.Of("Angle", "Turn of the first mirror line, counterclockwise"),
            centerX: WarpText.CenterX, centerY: WarpText.CenterY,
            zoom: ParameterText.Of("Zoom", "Magnification of the wedges' source about the center"),
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("1f0dfd6b-db2d-41e6-8b94-0bc199d18cab")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class PolarWarp : StageEffect<PolarWarp, PolarCoordinates, PolarCoordinatesParameter, InvalidWarp>, IEffectDefinition<PolarCoordinates, PolarCoordinatesParameter> {
    public const string Caption = "Polar Coordinates";

    public static EffectRow<PolarCoordinates, PolarCoordinatesParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            conversion: ParameterText.Choice<PolarConversion, InvalidWarp>(
                "Conversion", "Direction the frame converts between rows and rings", static conversion => conversion.Map(rectangularToPolar: "Rectangular to Polar", polarToRectangular: "Polar to Rectangular")),
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("2840e7bd-5137-4e50-99c2-82efcea276b7")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ImageTransform : StageEffect<ImageTransform, AffineTransform, AffineTransformParameter, InvalidWarp>, IEffectDefinition<AffineTransform, AffineTransformParameter> {
    public const string Caption = "Transform";

    public static EffectRow<AffineTransform, AffineTransformParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            offsetX: ParameterText.Of("X", "Horizontal move as a share of the frame's short side"),
            offsetY: ParameterText.Of("Y", "Downward move as a share of the frame's short side"),
            rotation: ParameterText.Of("Angle", "Turn about the pivot, counterclockwise"),
            scaleX: ParameterText.Of("Scale X", "Horizontal scale about the pivot"),
            scaleY: ParameterText.Of("Scale Y", "Vertical scale about the pivot"),
            skewX: ParameterText.Of("Skew X", "Horizontal shear about the pivot"),
            skewY: ParameterText.Of("Skew Y", "Vertical shear about the pivot"),
            pivotX: ParameterText.Of("Pivot X", "Point the frame turns, scales, and shears about, rightward from the left edge"),
            pivotY: ParameterText.Of("Pivot Y", "Point the frame turns, scales, and shears about, down from the top edge"),
            travelX: ParameterText.Of("Travel X", "Horizontal move across the exposure, blurring along it"),
            travelY: ParameterText.Of("Travel Y", "Downward move across the exposure, blurring along it"),
            turn: ParameterText.Of("Spin", "Turn about the pivot across the exposure, counterclockwise"),
            zoom: ParameterText.Of("Zoom", "Scale about the pivot the exposure reaches at its end"),
            shake: WarpText.Shake, shutter: WarpText.Shutter, shutterPosition: WarpText.ShutterPosition,
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("106fb69d-480f-430a-95f0-cc9ee4bea176")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class CornerPin2D : StageEffect<CornerPin2D, CornerPin, CornerPinParameter, InvalidWarp>, IEffectDefinition<CornerPin, CornerPinParameter> {
    public const string Caption = "Corner Pin";

    public static EffectRow<CornerPin, CornerPinParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            topLeftX: ParameterText.Of("Upper Left X", "Upper left corner's place across the frame, rightward from the left edge"),
            topLeftY: ParameterText.Of("Upper Left Y", "Upper left corner's place down the frame from the top edge"),
            topRightX: ParameterText.Of("Upper Right X", "Upper right corner's place across the frame, rightward from the left edge"),
            topRightY: ParameterText.Of("Upper Right Y", "Upper right corner's place down the frame from the top edge"),
            bottomRightX: ParameterText.Of("Lower Right X", "Lower right corner's place across the frame, rightward from the left edge"),
            bottomRightY: ParameterText.Of("Lower Right Y", "Lower right corner's place down the frame from the top edge"),
            bottomLeftX: ParameterText.Of("Lower Left X", "Lower left corner's place across the frame, rightward from the left edge"),
            bottomLeftY: ParameterText.Of("Lower Left Y", "Lower left corner's place down the frame from the top edge"),
            sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}

[Guid("4c2a5565-2369-462c-91cf-2b52ce565040")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class CameraShake : StageEffect<CameraShake, ShakeWarp, ShakeWarpParameter, InvalidWarp>, IEffectDefinition<ShakeWarp, ShakeWarpParameter> {
    public const string Caption = "Camera Shake";

    public static EffectRow<ShakeWarp, ShakeWarpParameter> Row { get; } = new(static parameter =>
        parameter.Map(motion: WarpText.Shake, shutter: WarpText.Shutter, shutterPosition: WarpText.ShutterPosition, sampling: WarpText.Sampling, wrap: WarpText.Wrap));
}
