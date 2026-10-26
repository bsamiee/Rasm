using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using PeterO.Numbers;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Resampling(
    [property: Display(Name = "Interpolation", Description = "Filter between source pixels")] Sampling Sampling,
    [property: Display(Name = "Extension", Description = "Sampling past frame edges")] WrapMode Wrap) {
    public static Resampling Default { get; } = new(Sampling.Linear, WrapMode.Black);

    public (WrapMode Across, WrapMode Down) Edges => (Wrap, Wrap);
}

public sealed record Twirl(
    [property: Display(Name = "Angle", Description = "Counterclockwise turn at center, none at radius")][property: Range(-double.Tau, double.Tau)] float Angle,
    [property: Display(Name = "Radius", Description = "Twirl reach in frame short sides")][property: Range(0d, 1d)] ShortSideLength Radius,
    [property: Display(Name = "Falloff", Description = "Turn falloff exponent, 1 linear")][property: Range(0.25d, 4d)] float Falloff,
    [property: Display(Name = "Arms", Description = "Spiral arms the turn gathers into, 0 turning every direction alike")][property: Range(0, 8)] int Arms,
    [property: Display(Name = "Arm offset", Description = "Arm turn about center")][property: Range(-double.Pi, double.Pi)] float ArmOffset,
    [property: Display(Name = "Center", Description = "Point the warp centers on")] FramePosition Center,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(0f, ShortSideLength.Create(0.5f), 1f, 0, 0f, FramePosition.Center, Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Option<PixelPass> Pass(Twirl state, PassContext context) =>
        state.Angle is 0f || state.Radius == ShortSideLength.Neutral ? None
            : Some<PixelPass>(new PixelPass.Frame(new Displacement.Twirl(state.Angle, state.Radius, state.Falloff, state.Arms, state.ArmOffset, state.Center)
                .Map(extent: context.Extent, sampling: state.Resampling.Sampling, wrap: state.Resampling.Edges).Apply));
}

public sealed record Bulge(
    [property: Display(Name = "Amount", Description = "Bulge above 0 and pinch below, ±1 a full hemisphere")][property: Range(-1d, 1d)] float Amount,
    [property: Display(Name = "Mode", Description = "Axes the bulge acts along")] BulgeMode Mode,
    [property: Display(Name = "Radius", Description = "Bulged disc radius in frame short sides")][property: Range(0d, 1d)] ShortSideLength Radius,
    [property: Display(Name = "Center", Description = "Point the warp centers on")] FramePosition Center,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Bulge> {
    public static Bulge Default { get; } = new(0f, BulgeMode.Round, ShortSideLength.Create(0.5f), FramePosition.Center, Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Option<PixelPass> Pass(Bulge state, PassContext context) =>
        state.Amount is 0f || state.Radius == ShortSideLength.Neutral ? None
            : Some<PixelPass>(new PixelPass.Frame(new Displacement.Bulge(state.Amount, state.Mode, state.Radius, state.Center).Map(extent: context.Extent, sampling: state.Resampling.Sampling, wrap: state.Resampling.Edges).Apply));
}

public sealed record WaveWarp(
    [property: Display(Name = "Form", Description = "Straight waves or center rings moving outward, around, or both")] WaveFront Front,
    [property: Display(Name = "Profile", Description = "Shape of each wave period")] WaveProfile Profile,
    [property: Display(Name = "Generators", Description = "Waves summed into the field")][property: Range(1, 16)] int Count,
    [property: Display(Name = "Wavelength min", Description = "Shortest wave in frame short sides")][property: Range(0.005d, 1d)] ShortSideExtent MinWavelength,
    [property: Display(Name = "Wavelength max", Description = "Longest wave in frame short sides")][property: Range(0.005d, 1d)] ShortSideExtent MaxWavelength,
    [property: Display(Name = "Amplitude min", Description = "Weakest wave as share of amounts")][property: Range(0d, 1d)] float MinAmplitude,
    [property: Display(Name = "Amplitude max", Description = "Strongest wave as share of amounts")][property: Range(0d, 1d)] float MaxAmplitude,
    [property: Display(Name = "Amount X", Description = "Horizontal move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Horizontal,
    [property: Display(Name = "Amount Y", Description = "Vertical move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Vertical,
    [property: Display(Name = "Direction", Description = "Straight wave angle counterclockwise from right")][property: Range(-double.Pi, double.Pi)] float Direction,
    [property: Display(Name = "Spread", Description = "Wave direction fan about travel direction")][property: Range(0d, double.Pi)] float Spread,
    [property: Display(Name = "Phase", Description = "Turn added to every wave period")][property: Range(-double.Pi, double.Pi)] float Phase,
    [property: Display(Name = "Radius", Description = "Ring reach in frame short sides, fading to none")][property: Range(0d, 1d)] ShortSideLength Radius,
    [property: Display(Name = "Center", Description = "Point the warp centers on")] FramePosition Center,
    [property: Display(Name = "Rate", Description = "Wave periods traveled per second")][property: Range(-2d, 2d)] float Rate,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Stored time and rate of recorded frame time")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<WaveWarp> {
    public static WaveWarp Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, 5, ShortSideExtent.Create(10f / ReferenceFrame.Height), ShortSideExtent.Create(120f / ReferenceFrame.Height), 5f / 35f, 1f,
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, 0f, 0f, 0f, ShortSideLength.Create(0.5f), FramePosition.Center, 0f, Seed.MinValue, Timing.Default, Hold.MinValue, Resampling.Default);

    public static Option<PixelPass> Pass(WaveWarp state, PassContext context) =>
        (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) || state is { MinAmplitude: 0f, MaxAmplitude: 0f }
        || (state.Front.Ring.IsSome && state.Radius == ShortSideLength.Neutral)
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new Displacement.Wave(
                state.Front, state.Profile, state.Count, (state.MinWavelength, state.MaxWavelength), (state.MinAmplitude, state.MaxAmplitude), (state.Horizontal, state.Vertical),
                state.Direction, state.Spread, (state.Phase / float.Tau) - (state.Rate * state.Hold.Held(state.Timing.At(context))), state.Radius, state.Center,
                state.Seed.Field(8u, 0u)).Map(extent: context.Extent, sampling: state.Resampling.Sampling, wrap: state.Resampling.Edges).Apply));
}

public sealed record Turbulence(
    [property: Display(Name = "Noise", Description = "Displacement field basis")] NoiseBasis Basis,
    [property: Display(Name = "Size X", Description = "Horizontal noise feature size in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent ScaleX,
    [property: Display(Name = "Size Y", Description = "Vertical noise feature size in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent ScaleY,
    [property: Display(Name = "Amount X", Description = "Horizontal move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Horizontal,
    [property: Display(Name = "Amount Y", Description = "Vertical move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Vertical,
    [property: Display(Name = "Evolution", Description = "Noise units the field evolves per second")][property: Range(-2d, 2d)] float Rate,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Stored time and rate of recorded frame time")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Turbulence> {
    public static Turbulence Default { get; } = new(
        NoiseBasis.Default, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f), ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f), ShortSideOffset.Neutral, ShortSideOffset.Neutral, 0f,
        Seed.MinValue, Timing.Default, Hold.MinValue, Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Option<PixelPass> Pass(Turbulence state, PassContext context) =>
        (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) ? None
            : state.Basis.Sampler switch {
                var sampler => Some<PixelPass>(new PixelPass.Frame(new Displacement.Turbulence(
                    (at, hash) => sampler(at, hash).Value, (state.ScaleX, state.ScaleY), (state.Horizontal, state.Vertical), state.Rate * state.Hold.Held(state.Timing.At(context)),
                    state.Seed.Field(9u, 0u)).Map(extent: context.Extent, sampling: state.Resampling.Sampling, wrap: state.Resampling.Edges).Apply)),
            };
}

public sealed record MapDisplacement(
    [property: Display(Name = "Map image", Description = "Image file, frame itself when empty")] string? Source,
    [property: Display(Name = "Reading", Description = "Map values moving frame, or map coordinates the frame is sampled at when unset")] MapReading? Reading,
    [property: Display(Name = "Tile size", Description = "Map tile height in frame short sides, off stretching to fit")][property: Range(0.05d, 2d)] ShortSideExtent? TileSize,
    [property: Display(Name = "Softness", Description = "Map luma blur before moving frame")][property: Range(0.001d, 0.1d)] ShortSideExtent Softness,
    [property: Display(Name = "Amount X", Description = "Horizontal move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Horizontal,
    [property: Display(Name = "Amount Y", Description = "Vertical move at full strength in frame short sides")][property: Range(-0.1d, 0.1d)] ShortSideOffset Vertical,
    [property: Display(Name = "Rotation", Description = "Counterclockwise turn of map moves")][property: Range(-double.Pi, double.Pi)] float Rotation,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(
        Source: null, MapReading.Gradient, TileSize: null, ShortSideExtent.Create(0.015f * ReferenceFrame.GreaterSide), ShortSideOffset.Neutral, ShortSideOffset.Neutral, 0f, Resampling.Default);

    public static Option<PixelPass> Pass(MapDisplacement state, PassContext context) =>
        Optional(state.Source).Traverse(path => context.Derivations.Derived(path, static held => ImageFile.Load(held).RunSafe())).As().Match(
            Succ: image => Optional(state.Reading).Match(
                Some: reading => (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) ? None
                    : Some<PixelPass>(new DisplacementMap(reading, Optional(state.TileSize), state.Softness, (state.Horizontal, state.Vertical), state.Rotation)
                        .Pass(image.Map(static file => file.Frame), context.Working.Luminance, context.Exposure, state.Resampling.Sampling, state.Resampling.Wrap)),
                None: () => image.Map(file => (PixelPass)new PixelPass.Frame(new CoordinateMap.Sampled(file.Frame, state.Resampling.Sampling, state.Resampling.Edges).Apply))),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Raindrops), nameof(Raindrops))]
[JsonDerivedType(typeof(Blocks), nameof(Blocks))]
[JsonDerivedType(typeof(Canvas), nameof(Canvas))]
[JsonDerivedType(typeof(Frosted), nameof(Frosted))]
[JsonDerivedType(typeof(TinyLens), nameof(TinyLens))]
public abstract partial record RefractionTexture {
    public abstract RefractionSurface Surface(float held);

    public virtual bool Rests(Refraction state) => (state.Strength, state.Blur) == (ShortSideOffset.Neutral, ShortSideLength.Neutral);

    public sealed record Raindrops(
        [property: Display(Name = "Density", Description = "Share of cells holding a raindrop")][property: Range(0d, 1d)] float Density,
        [property: Display(Name = "Drop size", Description = "Largest raindrop radius as texture cell share")][property: Range(0.01d, 0.5d)] float Size,
        [property: Display(Name = "Variation", Description = "Seeded raindrop radius reduction share")][property: Range(0d, 1d)] float Variation,
        [property: Display(Name = "Drop softness", Description = "Raindrop radius share blending rim")][property: Range(0d, 1d)] float Softness,
        [property: Display(Name = "Lens", Description = "Raindrop magnification")][property: Range(0d, 10d)] float Lens,
        [property: Display(Name = "Trails", Description = "Raindrop trail height relative to drop")][property: Range(0d, 1d)] float Trails,
        [property: Display(Name = "Direction", Description = "Raindrop run angle counterclockwise from right")][property: Range(-double.Pi, double.Pi)] float Direction,
        [property: Display(Name = "Fall", Description = "Raindrop travel per second in frame short sides")][property: Range(-1d, 1d)] float Fall) : RefractionTexture {
        public static Raindrops Default { get; } = new(0.5f, 0.5f, 0.5f, 0.05f, 2f, 0.3f, -float.Pi / 2f, 0f);

        public override RefractionSurface Surface(float held) => new RefractionSurface.Raindrops(Density, Size, Variation, Softness, Lens, Trails, Direction, Fall * held);

        public override bool Rests(Refraction state) => Density is 0f;
    }

    public sealed record Blocks : RefractionTexture {
        public static Blocks Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.Blocks();
    }

    public sealed record Canvas : RefractionTexture {
        public static Canvas Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.Canvas();
    }

    public sealed record Frosted([property: Display(Name = "Octaves", Description = "Frost detail across scales")] Octaves Octaves) : RefractionTexture {
        public static Frosted Default { get; } = new(NoiseBasis.Fbm.Default.Octaves);

        public override RefractionSurface Surface(float held) => new RefractionSurface.Frosted(Octaves);
    }

    public sealed record TinyLens : RefractionTexture {
        public static TinyLens Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.TinyLens();
    }
}

public sealed record Refraction(
    [property: Display(Name = "Texture", Description = "Surface the frame is seen through")] RefractionTexture Texture,
    [property: Display(Name = "Scale", Description = "Texture cell size in frame short sides")][property: Range(0.005d, 1d)] ShortSideExtent Scale,
    [property: Display(Name = "Distortion", Description = "Frame bend through surface, below 0 inverting")][property: Range(-0.1d, 0.1d)] ShortSideOffset Strength,
    [property: Display(Name = "Smoothness", Description = "Surface blur before slope bends frame")][property: Range(0.001d, 0.05d)] ShortSideExtent Smoothness,
    [property: Display(Name = "Blur", Description = "Frame softening through surface")][property: Range(0d, 0.05d)] ShortSideLength Blur,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Stored time and rate of recorded frame time")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Refraction> {
    public static Refraction Default { get; } = new(
        RefractionTexture.Raindrops.Default, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f / 3.8f), ShortSideOffset.Neutral, ShortSideExtent.Create(3f / ReferenceFrame.Height),
        ShortSideLength.Create(14.6f / ReferenceFrame.Height), Seed.MinValue, Timing.Default, Hold.MinValue, Resampling.Default);

    public static Option<PixelPass> Pass(Refraction state, PassContext context) =>
        state.Texture.Rests(state) ? None
            : Some<PixelPass>(new RefractionField(state.Texture.Surface(state.Hold.Held(state.Timing.At(context))), state.Scale, state.Strength, state.Smoothness, state.Blur,
                state.Seed.Field(10u, 0u)).Pass(state.Resampling.Sampling, state.Resampling.Wrap));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Rectilinear), nameof(Rectilinear))]
[JsonDerivedType(typeof(Equirectangular), nameof(Equirectangular))]
[JsonDerivedType(typeof(Cylindrical), nameof(Cylindrical))]
[JsonDerivedType(typeof(Fisheye), nameof(Fisheye))]
[JsonDerivedType(typeof(Equisolid), nameof(Equisolid))]
[JsonDerivedType(typeof(MirrorBall), nameof(MirrorBall))]
[JsonDerivedType(typeof(Stereographic), nameof(Stereographic))]
[JsonDerivedType(typeof(Panini), nameof(Panini))]
public abstract partial record PanoramaProjection {
    public abstract Projection Of(Projection.Rectilinear rendering);

    public sealed record Rectilinear : PanoramaProjection {
        public static Rectilinear Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => rendering;
    }

    public sealed record Equirectangular : PanoramaProjection {
        public static Equirectangular Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Equirectangular(rendering.Extent);
    }

    public sealed record Cylindrical(
        [property: Display(Name = "Sweep", Description = "Angle the output spans across frame")][property: Range(0d, double.Tau)] float Sweep,
        [property: Display(Name = "Height", Description = "Viewing window vertical offset")][property: Range(-10d, 10d)] float Height) : PanoramaProjection {
        public static Cylindrical Default { get; } = new(float.Pi, 0f);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Cylindrical(rendering.Extent, ViewWindow.Centered(Angle.FromRadians(Sweep), rendering.Extent).Raised(Height));
    }

    public sealed record Fisheye([property: Display(Name = "Sweep", Description = "Angle the output spans across frame")][property: Range(0d, double.Tau)] float Sweep) : PanoramaProjection {
        public static Fisheye Default { get; } = new(float.Pi);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Fisheye(rendering.Extent, Sweep);
    }

    public sealed record Equisolid(
        [property: Display(Name = "Sweep", Description = "Angle the output spans across frame")][property: Range(0d, double.Tau)] float Sweep,
        [property: Display(Name = "Full turn radius", Description = "Full-turn radius as a share of the inscribed circle")][property: Range(2d * 0.01d / 12d, 2d * 15d / 12d)] float FullTurn) : PanoramaProjection {
        public static Equisolid Default { get; } = new(float.Pi, 2f * 10.5f / 12f);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Equisolid(rendering.Extent, Sweep, FullTurn);
    }

    public sealed record MirrorBall : PanoramaProjection {
        public static MirrorBall Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.MirrorBall(rendering.Extent);
    }

    public sealed record Stereographic([property: Display(Name = "Sweep", Description = "Angle the output spans across frame")][property: Range(0d, double.Tau)] float Sweep) : PanoramaProjection {
        public static Stereographic Default { get; } = new(float.Pi);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Stereographic(rendering.Extent, Sweep);
    }

    public sealed record Panini(
        [property: Display(Name = "Field of view", Description = "Horizontal angle the output spans across frame")][property: Range(0d, double.Pi)] float Field,
        [property: Display(Name = "Distance", Description = "Projection distance, 0 rectilinear and 1 Panini")][property: Range(0d, 1d)] float Distance,
        [property: Display(Name = "Compression", Description = "Vertical compression straightening verticals")][property: Range(0d, 1d)] float Compression) : PanoramaProjection {
        public static Panini Default { get; } = new(float.Pi / 3f, 1f, 0f);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Panini(rendering.Extent, Field, Distance, Compression);
    }
}

public sealed record Panorama(
    [property: Display(Name = "Projection", Description = "Projection the rendered view is placed into")] PanoramaProjection Output,
    [property: Display(Name = "Heading", Description = "Rightward turn of view")][property: Range(-double.Pi, double.Pi)] float Heading,
    [property: Display(Name = "Elevation", Description = "View upward tilt")][property: Range(-double.Pi / 2d, double.Pi / 2d)] float Elevation,
    [property: Display(Name = "Roll", Description = "Counterclockwise turn about view axis")][property: Range(-double.Pi, double.Pi)] float Roll,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(PanoramaProjection.Rectilinear.Default, 0f, 0f, 0f, Resampling.Default);

    public static Option<PixelPass> Pass(Panorama state, PassContext context) =>
        from camera in context.Camera
        where state.Output is not PanoramaProjection.Rectilinear || state is not { Heading: 0f, Elevation: 0f, Roll: 0f }
        from map in camera.Panorama(state.Output.Of, new PanoramaView(Angle.FromRadians(state.Heading), Angle.FromRadians(state.Elevation), Angle.FromRadians(state.Roll)),
            state.Resampling.Sampling, state.Resampling.Edges)
        select (PixelPass)new PixelPass.Frame(map.Apply);
}

public sealed record PolarCoordinates(
    [property: Display(Name = "Conversion", Description = "Direction the frame converts between rows and rings")] PolarConversion Conversion,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, Resampling.Default);

    public static Option<PixelPass> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(state.Conversion.Map(sampling: state.Resampling.Sampling, wrap: state.Resampling.Edges).Apply));
}

public sealed record Kaleidoscope(
    [property: Display(Name = "Mirrors", Description = "Mirror lines through the center at their turn and zoom, drawing twice as many wedges")] MirrorFold Fold,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<Kaleidoscope> {
    public static Kaleidoscope Default { get; } = new(MirrorFold.Create(3, double.Pi / 2d, FramePosition.Center, 1d), Resampling.Default with { Wrap = WrapMode.Mirror });

    public static Option<PixelPass> Pass(Kaleidoscope state, PassContext context) => Some<PixelPass>(new PixelPass.Frame(state.Fold.Map(state.Resampling.Sampling, state.Resampling.Edges).Apply));
}

public sealed record Shake(
    [property: Display(Name = "Amplitude X", Description = "Largest horizontal shake in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength AmplitudeX,
    [property: Display(Name = "Amplitude Y", Description = "Largest vertical shake in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength AmplitudeY,
    [property: Display(Name = "Rotation", Description = "Largest shake turn")][property: Range(0d, double.Pi)] float Rotation,
    [property: Display(Name = "Zoom", Description = "Largest shake zoom factor, in and out alike")][property: Range(1d, 2d)] float Zoom,
    [property: Display(Name = "Frequency", Description = "Shakes per second")][property: Range(0.1d, 20d)] float Rate,
    [property: Display(Name = "Octaves", Description = "Shake detail across scales")] Octaves Octaves,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Stored time and rate of recorded frame time")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold) {
    public static Shake Default { get; } = new(ShortSideLength.Neutral, ShortSideLength.Neutral, 0f, 1f, 5f, NoiseBasis.Fbm.Default.Octaves, Seed.MinValue, Timing.Default, Hold.MinValue);

    public bool Still => (AmplitudeX, AmplitudeY, Rotation, Zoom) == (Default.AmplitudeX, Default.AmplitudeY, Default.Rotation, Default.Zoom);

    public ShakePath Path => new((AmplitudeX, AmplitudeY), Rotation, Zoom, Rate, Octaves, Seed.Field(11u, 0u));

    public double Opened(PassContext context) => Hold.Held(Timing.At(context));
}

public sealed record ShutterMotion(
    [property: Display(Name = "Shake", Description = "Seeded camera motion over time")] Shake Shake,
    [property: Display(Name = "Shutter", Description = "Seconds the exposure samples along its motion")][property: Range(0d, 0.5d)] float Shutter,
    [property: Display(Name = "Shutter position", Description = "Exposure share before current time, 0% opening and 100% closing now")][property: Range(0d, 1d)] float Position) {
    public static ShutterMotion Default { get; } = new(Shake.Default, 0f, 0.5f);

    public bool Open => Shutter is not 0f;

    public Func<double, Affine> Path(Pose motion, Affine placed, PassContext context) =>
        (Shutter, Position, Shake.Opened(context) - (Shutter * Position), Shake.Path) switch {
            var (duration, position, opened, shake) => share =>
                motion.Partial(share - position).Inverse.Then(shake.Posed(context.Extent, opened + (duration * share)).Inverse).Then(placed),
        };
}

public sealed record AffineTransform(
    [property: Display(Name = "X", Description = "Horizontal move in frame short sides")][property: Range(-1d, 1d)] ShortSideOffset OffsetX,
    [property: Display(Name = "Y", Description = "Downward move in frame short sides")][property: Range(-1d, 1d)] ShortSideOffset OffsetY,
    [property: Display(Name = "Angle", Description = "Counterclockwise turn about pivot")][property: Range(-double.Pi, double.Pi)] float Rotation,
    [property: Display(Name = "Scale X", Description = "Horizontal scale about pivot")][property: Range(1d / 16d, 16d)] float ScaleX,
    [property: Display(Name = "Scale Y", Description = "Vertical scale about pivot")][property: Range(1d / 16d, 16d)] float ScaleY,
    [property: Display(Name = "Skew X", Description = "Horizontal shear about pivot")][property: Range(-1.2490457723982544d, 1.2490457723982544d)] float SkewX,
    [property: Display(Name = "Skew Y", Description = "Vertical shear about pivot")][property: Range(-1.2490457723982544d, 1.2490457723982544d)] float SkewY,
    [property: Display(Name = "Pivot", Description = "Point the frame turns, scales, and shears about")] FramePosition Pivot,
    [property: Display(Name = "Travel X", Description = "Horizontal move across exposure, blurring along it")][property: Range(-1d, 1d)] ShortSideOffset TravelX,
    [property: Display(Name = "Travel Y", Description = "Downward move across exposure, blurring along it")][property: Range(-1d, 1d)] ShortSideOffset TravelY,
    [property: Display(Name = "Spin", Description = "Counterclockwise turn about pivot across exposure")][property: Range(-double.Pi, double.Pi)] float Turn,
    [property: Display(Name = "Zoom", Description = "Scale about pivot at exposure end")][property: Range(1d / 16d, 16d)] float Zoom,
    [property: Display(Name = "Exposure", Description = "Camera shake and the shutter sampling motion")] ShutterMotion Motion,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<AffineTransform> {
    public static AffineTransform Default { get; } = new(
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, 0f, 1f, 1f, 0f, 0f, FramePosition.Center, ShortSideOffset.Neutral, ShortSideOffset.Neutral, 0f, 1f, ShutterMotion.Default, Resampling.Default);

    public static Option<PixelPass> Pass(AffineTransform state, PassContext context) =>
        (Pivot: state.Pivot.Point(context.Extent), Side: context.Extent.ShortSide) switch {
            var (pivot, side) => state.Motion.Path(
                new Pose((ERational.FromSingle(state.TravelX).Multiply(side), ERational.FromSingle(state.TravelY).Multiply(side)), state.Turn, (state.Zoom, state.Zoom), (0d, 0d), pivot),
                new Pose((ERational.FromSingle(state.OffsetX).Multiply(side), ERational.FromSingle(state.OffsetY).Multiply(side)), state.Rotation, (state.ScaleX, state.ScaleY), (state.SkewX, state.SkewY), pivot).Inverse,
                context) switch {
                    var path when (state.TravelX, state.TravelY, state.Turn, state.Zoom) != (Default.TravelX, Default.TravelY, Default.Turn, Default.Zoom) || (state.Motion.Open && !state.Motion.Shake.Still) =>
                        Some<PixelPass>(Affine.Swept(path, state.Resampling.Sampling, state.Resampling.Wrap)),
                    var path => path(0d) switch {
                        { Still: true } => None,
                        var still => Some<PixelPass>(new PixelPass.Frame(still.Map(state.Resampling.Sampling, state.Resampling.Wrap).Apply)),
                    },
                },
        };
}

public sealed record CameraShake(
    [property: Display(Name = "Exposure", Description = "Camera shake and the shutter sampling motion")] ShutterMotion Motion,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<CameraShake> {
    public static CameraShake Default { get; } = new(ShutterMotion.Default, Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Option<PixelPass> Pass(CameraShake state, PassContext context) =>
        AffineTransform.Pass(AffineTransform.Default with { Motion = state.Motion, Resampling = state.Resampling }, context);
}

public sealed record CornerPin(
    [property: Display(Name = "Upper left", Description = "Upper left corner position in frame")] FramePosition TopLeft,
    [property: Display(Name = "Upper right", Description = "Upper right corner position in frame")] FramePosition TopRight,
    [property: Display(Name = "Lower right", Description = "Lower right corner position in frame")] FramePosition BottomRight,
    [property: Display(Name = "Lower left", Description = "Lower left corner position in frame")] FramePosition BottomLeft,
    [property: Display(Name = "Resampling", Description = "Source filter and edge extension")] Resampling Resampling) : IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(FramePosition.TopLeft, FramePosition.TopRight, FramePosition.BottomRight, FramePosition.BottomLeft, Resampling.Default);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Resampling = Default.Resampling } == Default ? None
            : FrameQuad.Validated(state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent)).Match(
                Succ: quad => Some<PixelPass>(new PixelPass.Frame(quad.Pinned(state.Resampling.Sampling, state.Resampling.Edges).Apply)),
                Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}
