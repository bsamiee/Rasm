using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using CommunityToolkit.HighPerformance;
using MathNet.Numerics.Integration;
using NSubstitute;
using Rasm.Imaging;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Warping;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Images {
    public static TheoryData<LutFormat> LookupFormats => [LutFormat.Hald, LutFormat.KeyShotLookup];

    [Fact]
    public unsafe void SignedPixelStride() {
        Bgra32[] storage = [new(255, 0, 0, 255), new(0, 255, 0, 0), new(255, 0, 255, 255), new(0, 0, 255, 255), new(255, 255, 255, 255), new(255, 0, 255, 255)];
        Vector4[] expected = [new(1f, 0f, 0f, 1f), new(0f, 1f, 0f, 0f), new(0f, 0f, 1f, 1f), Vector4.One];
        PixelExtent size = PixelExtent.Validated(2, 2).ThrowIfFail();
        PixelExtent extent = PixelExtent.Validated(16, 16).ThrowIfFail();
        System.Drawing.Point origin = new(5, 7);
        fixed (Bgra32* address = storage) {
            PixelFrame forward = PixelFrame.From<Bgra32>((nint)address, 3 * sizeof(Bgra32), origin, size, extent);
            PixelFrame backward = PixelFrame.From<Bgra32>((nint)(address + 3), -3 * sizeof(Bgra32), origin, size, extent);
            ReadOnlySpan2D<Vector4> pixels = forward.View.Span;
            Assert.Equal(expected, pixels.ToArray().Cast<Vector4>());
            Assert.Equal(MemoryMarshal.Cast<Vector4, float>([expected[2], expected[3], expected[0], expected[1]]), backward.Block);
            Assert.Equal(origin, backward.Origin);
            Assert.Equal(size, backward.Size);
            Assert.Equal(extent, backward.Extent);
        }
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(LookupFormats))]
    public void LookupPngSamples(LutFormat format) {
        const int edge = 4;
        float[] samples = [.. from blue in Enumerable.Range(0, edge)
                              from green in Enumerable.Range(0, edge)
                              from red in Enumerable.Range(0, edge)
                              from value in new[] { red, green, blue }
                              select value / (edge - 1f)];
        LutTable table = new LutTable.Cube(Lut3D.From(edge, samples, Vector3.Zero, Vector3.One).ThrowIfFail(), LutInterpolation.Tetrahedral);
        using MemoryStream stream = new();
        _ = format.Encode(table, nameof(LookupPngSamples), new FileMetadata(nameof(Images), DateTimeOffset.UnixEpoch, 1d, [])).ThrowIfFail()(stream).Run();
        using Image<Rgba64> image = SixLabors.ImageSharp.Image.Load<Rgba64>(stream.ToArray());
        Rgba64[] pixels = new Rgba64[image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        IEnumerable<(ushort Red, ushort Green, ushort Blue, ushort Alpha)> actual = pixels.Select(static pixel => (pixel.R, pixel.G, pixel.B, pixel.A));
        IEnumerable<(ushort Red, ushort Green, ushort Blue, ushort Alpha)> expected =
            from blue in Enumerable.Range(0, edge)
            from green in Enumerable.Range(0, edge)
            from red in Enumerable.Range(0, edge)
            select ((ushort)(red * ushort.MaxValue / (edge - 1)), (ushort)(green * ushort.MaxValue / (edge - 1)), (ushort)(blue * ushort.MaxValue / (edge - 1)), ushort.MaxValue);
        Assert.Equal(format == LutFormat.Hald ? expected : expected.Order(), format == LutFormat.Hald ? actual : actual.Order());
    }

    [Theory]
    [InlineData("false")]
    [InlineData("invalid")]
    public void HalfAttributesRequireTrue(string value) {
        (string Name, string Value)[] flags = [("rawHalfs", value), ("halfDomain", value)];
        foreach (LutFormat format in new[] { LutFormat.Clf, LutFormat.Ctf }) {
            XElement process = new("ProcessList", new XAttribute("id", nameof(HalfAttributesRequireTrue)),
                format == LutFormat.Clf ? new XAttribute("compCLFversion", "3") : new XAttribute("version", "2"),
                new XElement("LUT1D", new XAttribute("inBitDepth", "32f"), new XAttribute("outBitDepth", "32f"),
                    flags.Select(static flag => new XAttribute(flag.Name, flag.Value)), new XElement("Array", new XAttribute("dim", "2 1"), "0 1")));
            Error failure = Assert.IsType<Fin<LutTable>.Fail>(format.Decode(Encoding.UTF8.GetBytes($"""<?xml version="1.0" encoding="UTF-8"?>{process}"""))).Error;
            Assert.Equal(flags.Select(static flag => (ImagingReason.Malformed, flag)), failure.AsIterable().Map(static error => Assert.IsType<ImagingFault>(error) switch {
                var fault => (fault.Reason, Assert.IsType<(string Name, string Value)>(fault.Subject)),
            }).ToSeq());
        }
    }

    [Fact]
    public void InverseCurveUsesInputDepth() {
        LutTable table = LutFormat.Ctf.Decode("""
            <ProcessList version="2" id="identity">
              <InverseLUT1D inBitDepth="10i" outBitDepth="8i"><Array dim="2 1">0 1023</Array></InverseLUT1D>
            </ProcessList>
            """u8.ToArray()).ThrowIfFail();
        Vector4[] expected = [new(0f, 0.5f, 1f, 0.25f), new(0.25f, 0.75f, 0.5f, 0.5f)];
        Vector4[] actual = [.. expected];
        table.Apply(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NarrowRampIntegrals() {
        Vector4 red = new(1f, 0f, 0f, 1f);
        Vector4 blue = new(0f, 0f, 1f, 1f);
        Vector4 mean = (red + blue) / 2f;
        RampIntegral ramp = RampIntegral.From([red, blue]).ThrowIfFail();
        Assert.Equal(red, ramp.Mean(0.25d, double.Epsilon));
        Assert.Equal(mean, ramp.Mean(0.5d, double.Epsilon));
        Assert.Equal(red, ramp.Mean(0d, double.Epsilon));
        Assert.Equal(mean, ramp.PeriodicMean(0d, double.Epsilon));
        Assert.Equal(new Vector4(1f, 0f, 0f, 0.5f), RampIntegral.From([red, blue with { W = 0f }]).ThrowIfFail().Mean(0.5d, double.Epsilon));
    }

    [Fact]
    public void InfiniteRampIntegrals() {
        RampIntegral ramp = RampIntegral.From([new(1f, 0f, 0f, 1f), new(0f, 1f, 0f, 1f), new(0f, 0f, 1f, 1f)]).ThrowIfFail();
        Assert.Equal(new Vector4(0.5f, 0f, 0.5f, 1f), ramp.Mean(0.5d, double.PositiveInfinity));
        Assert.Equal(new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 1f), ramp.PeriodicMean(0.5d, double.PositiveInfinity));
    }

    [Fact]
    public void IntegratedMapFailure() {
        GaussLegendreRule rule = new(0d, 1d, 3);
        (Error Error, Func<double, Fin<(CoordinateMap Map, Vector4 Gains)>> Path)[] paths = [.. new[] { Error.New("first"), Error.New("second") }
            .Select(static error => (
                error,
                Substitute.For<Func<double, Fin<(CoordinateMap Map, Vector4 Gains)>>>()
            ))];
        foreach ((Error error, Func<double, Fin<(CoordinateMap Map, Vector4 Gains)>> path) in paths) {
            _ = path(default).ReturnsForAnyArgs(error);
            path.ClearReceivedCalls();
        }
        PixelExtent size = PixelExtent.Validated(2, 2).ThrowIfFail();
        PixelFrame frame = new(default, size, size, static pixels => pixels.Fill(0.25f));
        Fin<Unit> result = CoordinateMap.Integrated(rule, paths[0].Path, paths[1].Path).Run(frame);
        Error failure = Assert.IsType<Fin<Unit>.Fail>(result).Error;
        Assert.Equal(paths.SelectMany(path => Enumerable.Repeat(path.Error, rule.Order)), failure.AsIterable().ToSeq());
        foreach ((Error _, Func<double, Fin<(CoordinateMap Map, Vector4 Gains)>> path) in paths)
            Assert.Equal(rule.Abscissas, path.ReceivedCalls().Select(static call => Assert.IsType<double>(Assert.Single(call.GetArguments()))));
        Assert.All(frame.Block.ToArray(), static pixel => Assert.Equal(0.25f, pixel));
    }
}
