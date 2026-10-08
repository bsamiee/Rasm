using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Text;
using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.ColorManagement;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using TinyEXR;

namespace Rasm.Imaging.Output;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOutput>]
public readonly partial struct PixelDensity : IMinMaxValue<PixelDensity> {
    public static PixelDensity MinValue { get; } = new(1d / (double)Length.FromMeters(1).Inches);
    public static PixelDensity MaxValue { get; } = new(uint.MaxValue / (double)Length.FromMeters(1).Inches);

    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOutput();
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public sealed partial class SoftwareName {
    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAnyExceptInRange(' ', '~') ? null : new InvalidOutput();
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public sealed partial class FieldName {
    private const int KeywordLength = 79;

    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref string value) =>
        validationError = value.Length is > 0 and <= KeywordLength
            && value[0] != ' ' && value[^1] != ' ' && !value.Contains("  ", StringComparison.Ordinal)
            && !value.AsSpan().ContainsAnyExceptInRange(' ', 'ÿ') && !value.AsSpan().ContainsAnyInRange('\u007F', ' ')
            && !FileMetadata.HeaderNames.Contains(value)
            ? null
            : new InvalidOutput();
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public sealed partial class FieldText {
    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref string value) =>
        validationError = value.Contains('\0', StringComparison.Ordinal) ? new InvalidOutput() : null;
}

public sealed record FileMetadata(SoftwareName Software, DateTimeOffset Created, PixelDensity Density, Map<FieldName, FieldText> Fields) {
    private const string CapDateAttribute = "capDate";
    private const string SoftwareAttribute = "Software";
    private const string UtcOffsetAttribute = "utcOffset";
    private const string XDensityAttribute = "xDensity";
    private const string InteropIdAttribute = "colorInteropID";

    internal static FrozenSet<string> HeaderNames { get; } = FrozenSet.Create(
        StringComparer.Ordinal,
        "channels", "compression", "dataWindow", "displayWindow", "lineOrder", "pixelAspectRatio", "screenWindowCenter", "screenWindowWidth", "tiles", "name", "type", "chunkCount", "version",
        "maxSamplesPerPixel", "chromaticities", CapDateAttribute, SoftwareAttribute, UtcOffsetAttribute, XDensityAttribute, InteropIdAttribute);

    private string Stamp => Created.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

    internal ImageMetadata Png(Option<ColorEncoding> encoding) {
        ExifProfile exif = new();
        exif.SetValue(ExifTag.Software, (string)Software);
        exif.SetValue(ExifTag.DateTimeOriginal, Stamp);
        exif.SetValue(ExifTag.OffsetTimeOriginal, Created.ToString("zzz", CultureInfo.InvariantCulture));
        _ = encoding.Iter(encoded => exif.SetValue(ExifTag.ColorSpace, ColorTags.ExifColorSpace(encoded)));
        ImageMetadata metadata = new() { ResolutionUnits = PixelResolutionUnit.PixelsPerInch, HorizontalResolution = Density, VerticalResolution = Density, ExifProfile = exif };
        _ = encoding.Bind(encoded => ColorTags.Icc(encoded, Created)).Iter(bytes => metadata.IccProfile = new IccProfile(bytes));
        _ = encoding.Bind(ColorTags.Cicp).Iter(cicp => metadata.CicpProfile = cicp);
        metadata.GetPngMetadata().TextData = [.. Fields.ToSeq().Map(static field => new PngTextData((string)field.Key, (string)field.Value, "", ""))];
        return metadata;
    }

    internal Seq<(TiffTag Tag, object[] Values)> Tiff() => [
        (TiffTag.SOFTWARE, [(string)Software]),
        (TiffTag.DATETIME, [Stamp]),
        (TiffTag.XRESOLUTION, [(float)(double)Density]),
        (TiffTag.YRESOLUTION, [(float)(double)Density]),
        (TiffTag.RESOLUTIONUNIT, [ResUnit.INCH]),
    ];

    internal Seq<HeaderAttribute> Exr(Option<Gamut> gamut) {
        static HeaderAttribute Text(string name, string text) => new(name, "string", Encoding.UTF8.GetBytes(text));
        static HeaderAttribute Single(string name, float value) {
            Span<byte> bytes = stackalloc byte[sizeof(float)];
            BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
            return new HeaderAttribute(name, "float", bytes);
        }
        return [
            Text(CapDateAttribute, Stamp),
            Text(SoftwareAttribute, (string)Software),
            Single(UtcOffsetAttribute, (float)-Created.Offset.TotalSeconds),
            Single(XDensityAttribute, (float)(double)Density),
            .. gamut.Bind(static held => held.InteropId).Map(static id => Text(InteropIdAttribute, id)).ToSeq(),
            .. Fields.ToSeq().Map(static field => Text((string)field.Key, (string)field.Value)),
        ];
    }
}
