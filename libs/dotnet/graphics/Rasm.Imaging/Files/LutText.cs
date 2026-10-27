using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LutText {
    // --- [TOKENS]
    internal static readonly char[] Blanks = [' ', '\t', '\r', '\n'];
    private const string Title = "TITLE";
    private const string Lut1DSize = "LUT_1D_SIZE";
    private const string Lut3DSize = "LUT_3D_SIZE";
    private const string DomainMin = "DOMAIN_MIN";
    private const string DomainMax = "DOMAIN_MAX";
    private const string Lut1DInputRange = "LUT_1D_INPUT_RANGE";
    private const string Lut3DInputRange = "LUT_3D_INPUT_RANGE";
    private const string Version = "Version";
    private const string From = "From";
    private const string Length = "Length";
    private const string Components = "Components";
    private const string SpiLut = "SPILUT";

    // --- [CODECS]
    internal static string Text(ReadOnlyMemory<byte> bytes) {
        using StreamReader reader = new(bytes.AsStream());
        return reader.ReadToEnd();
    }

    internal static Fin<Func<Stream, IO<Unit>>> Written(Fin<string> rendered) =>
        rendered.Map<Func<Stream, IO<Unit>>>(static text => stream => IO.liftAsync(async environment => {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(text), environment.Token).ConfigureAwait(false);
            return unit;
        }));

    // --- [GRAMMARS]
    internal static Fin<LutTable> ParseCube(string text) =>
        from header in Header.Read(text, LutFormat.Cube, [Title, Lut1DSize, Lut3DSize, DomainMin, DomainMax, Lut1DInputRange, Lut3DInputRange], static line => char.IsAsciiLetter(line[0]))
        from stated in (
                header.Stated(Lut1DSize, static value => Integer(value).Bind(Entries)),
                header.Stated(Lut3DSize, Integer),
                header.Stated(DomainMin, Triple),
                header.Stated(DomainMax, Triple),
                header.Stated(Lut1DInputRange, Pair),
                header.Stated(Lut3DInputRange, Pair))
            .Apply(static (curve, lattice, low, high, curveRange, latticeRange) =>
                (Curve: curve, Lattice: lattice, Low: low, High: high, CurveRange: curveRange, LatticeRange: latticeRange))
            .As()
        from table in (
                stated.Curve.IsSome || stated.Lattice.IsSome ? Fin.Succ(unit) : Sizeless(LutFormat.Cube),
                ((stated.Low | stated.High).Filter(_ => (stated.Curve.IsSome && stated.Lattice.IsSome) || stated.CurveRange.IsSome || stated.LatticeRange.IsSome).Map(static domain => domain.Line)
                    | stated.CurveRange.Filter(_ => stated.Curve.IsNone).Map(static range => range.Line)
                    | stated.LatticeRange.Filter(_ => stated.Lattice.IsNone).Map(static range => range.Line))
                .Traverse(static line => Fin.Fail<Unit>(Misstated(LutFormat.Cube, line))).As(),
                stated.Lattice.Filter(_ => stated.Curve.IsNone).Match(
                    Some: lattice => (Lut3D.TryParseCube(text, out Lut3D? parsed), parsed, Domain(stated.LatticeRange, stated.Low, stated.High)) switch {
                        (ExrResult.Success, Lut3D held, var domain) => Lut3D.From(lattice.Value, held.Data.ToArray(), domain.Minimum, domain.Maximum).Map(static cube => (LutTable)new LutTable.Cube(cube, LutInterpolation.Tetrahedral)),
                        (var refused, _, _) => new ImagingFault($"{LutFormat.Cube} LUT file read as {refused}", ImagingReason.Refused),
                    },
                    None: () => Tabled(LutFormat.Cube, text, header.End, braced: false, width: 3,
                        stated.Curve.Map(length => CurveTable(length.Value, Domain(stated.CurveRange, stated.Low, stated.High))).ToSeq()
                        + stated.Lattice.Map(size => LatticeTable(size.Value, Domain(stated.LatticeRange, stated.Low, stated.High), LutInterpolation.Tetrahedral)).ToSeq())))
            .Apply(static (_, _, table) => table)
            .As()
        select table;

    internal static Fin<LutTable> ParseSpi1d(string text) =>
        from header in Header.Read(text, LutFormat.Spi1d, [Version, From, Length, Components], static line => line[0] != '{')
        from stated in (
                header.Stated(Version, static value => value is "1" ? Some(unit) : None),
                header.Stated(From, Pair),
                header.Stated(Length, static value => Integer(value).Bind(Entries)),
                header.Stated(Components, static value => Integer(value).Filter(static count => count is 1 or 3)))
            .Apply(static (version, domain, length, components) => (Version: version, Domain: domain, Length: length, Components: components))
            .As()
        from shape in (
                stated.Length.ToFin(Sizeless(LutFormat.Spi1d)),
                stated.Components.Filter(_ => stated.Version.IsSome).ToFin(Misstated(LutFormat.Spi1d, header.End)))
            .Apply(static (size, width) => (Size: size.Value, Width: width.Value))
            .As()
        from table in Tabled(LutFormat.Spi1d, text, header.End + 1, braced: true, shape.Width, [CurveTable(shape.Size, Domain(stated.Domain, None, None))])
        select table;

    internal static Fin<LutTable> ParseSpi3d(string text) {
        Span<System.Range> tokens = stackalloc System.Range[7];
        Span<int> codes = stackalloc int[3];
        SpanLineEnumerator lines = text.AsSpan().EnumerateLines();
        if (!lines.MoveNext() || !lines.Current.TrimStart().StartsWith(SpiLut, StringComparison.OrdinalIgnoreCase))
            return Misstated(LutFormat.Spi3d, 1);
        if (!lines.MoveNext() || !lines.MoveNext())
            return Sizeless(LutFormat.Spi3d);
        if (lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries) is not 3 || Parsed(lines.Current, tokens, codes) is not 3 || codes.ContainsAnyExcept(codes[0]))
            return Misstated(LutFormat.Spi3d, 3);
        (int size, int number, List<(long Node, Vector3 Value, int Line)> rows) = (codes[0], 3, []);
        Span<float> value = stackalloc float[3];
        while (lines.MoveNext()) {
            number++;
            if (lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries) is not 6 || Parsed(lines.Current, tokens, codes) is not 3)
                continue;
            if (codes.ContainsAnyExceptInRange(0, size - 1) || Parsed(lines.Current, tokens[3..6], value) is not 3)
                return Misstated(LutFormat.Spi3d, number);
            rows.Add((((((long)codes[2] * size) + codes[1]) * size) + codes[0], new Vector3(value), number));
        }
        if (rows.Count != (long)size * size * size)
            return Miscounted(LutFormat.Spi3d, (long)size * size * size, rows.Count);
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * rows.Count);
        using SpanOwner<bool> filled = SpanOwner<bool>.Allocate(rows.Count, AllocationMode.Clear);
        foreach ((long node, Vector3 entry, int line) in rows) {
            if (filled.Span[(int)node])
                return Misstated(LutFormat.Spi3d, line);
            (filled.Span[(int)node], MemoryMarshal.Cast<float, Vector3>(samples.Span)[(int)node]) = (true, entry);
        }
        return UnitLattice(size, samples.Memory);
    }

    internal static Fin<LutTable> ParseSpiMatrix(string text) {
        const int Cells = 12;
        (Seq<double> values, int number) = ([], 0);
        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines()) {
            number++;
            if (Numbers<double>(line).Case is not double[] row)
                return Misstated(LutFormat.SpiMatrix, number);
            values += toSeq(row);
        }
        return values.Count != Cells
            ? Miscounted(LutFormat.SpiMatrix, Cells, values.Count)
            : Affine(values.AsSpan()[..Cells], 4, (ushort.MaxValue, ushort.MaxValue));
    }

    internal static Fin<LutTable> Parse3dl(string text) {
        Span<System.Range> tokens = stackalloc System.Range[4];
        Span<int> codes = stackalloc int[4];
        SpanLineEnumerator lines = text.AsSpan().EnumerateLines();
        (int number, Option<int[]> stated) = (0, None);
        while (stated.IsNone && lines.MoveNext()) {
            number++;
            stated = Numbers<int>(lines.Current).Filter(static mesh => mesh.Length > 0);
            if (stated.IsNone && lines.Current.SplitAny(tokens[..2], Blanks, StringSplitOptions.RemoveEmptyEntries) > 0 && Parsed(lines.Current, tokens[..1], codes[..1]) is 1)
                return Misstated(LutFormat.Autodesk, number);
        }
        if (stated.Case is not int[] { Length: var size } mesh)
            return Sizeless(LutFormat.Autodesk);
        if (size <= 3 || mesh[^1] < 128 || !Ramped(mesh, mesh[^1]))
            return Misstated(LutFormat.Autodesk, number);
        (List<Vector3> rows, int largest) = ([], 0);
        while (lines.MoveNext()) {
            number++;
            int count = lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries);
            int read = Parsed(lines.Current, tokens[..count], codes[..count]);
            if (read is 0)
                continue;
            if (count is not 3 || read != count || codes[..3].ContainsAnyInRange(int.MinValue, -1))
                return Misstated(LutFormat.Autodesk, number);
            rows.Add(new Vector3(codes[0], codes[1], codes[2]));
            largest = int.Max(largest, TensorPrimitives.Max(codes[..3]));
        }
        if (rows.Count != (long)size * size * size)
            return Miscounted(LutFormat.Autodesk, (long)size * size * size, rows.Count);
        float[] samples = Transposed(MemoryMarshal.Cast<Vector3, float>(CollectionsMarshal.AsSpan(rows)), size, 1f / (uint.Clamp(BitOperations.RoundUpToPowerOf2((uint)largest), 128u, 65_536u) - 1f));
        TensorPrimitives.Min(samples, 1f, samples);
        return UnitLattice(size, samples);

        static bool Ramped(ReadOnlySpan<int> mesh, int last) {
            for (int at = 0; at < mesh.Length; at++)
                if (float.Abs(mesh[at] - ((float)at * last / (mesh.Length - 1))) > 1f)
                    return false;
            return true;
        }
    }

    private static Fin<LutTable> Tabled(LutFormat format, string text, int start, bool braced, int width, Seq<(int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build)> tables) {
        int stated = tables.Fold(0, static (sum, table) => sum + table.Floats) / 3;
        Span<System.Range> tokens = stackalloc System.Range[4];
        Span<float> row = stackalloc float[3];
        using MemoryOwner<float> rows = MemoryOwner<float>.Allocate(3 * stated);
        Span<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(rows.Span);
        (int number, int read) = (0, 0);
        foreach (ReadOnlySpan<char> raw in text.AsSpan().EnumerateLines()) {
            ReadOnlySpan<char> line = raw.Trim();
            if (++number < start || line.IsEmpty || line[0] == '#')
                continue;
            if (braced && line[0] == '}')
                break;
            if (line.SplitAny(tokens[..(width + 1)], Blanks, StringSplitOptions.RemoveEmptyEntries) != width || Parsed(line, tokens, row[..width]) != width)
                return Misstated(format, number);
            row[width..].Fill(row[0]);
            if (read < stated)
                entries[read] = new Vector3(row);
            read++;
        }
        return read != stated
            ? Miscounted(format, stated, read)
            : tables.Zip(tables.Map(static table => table.Floats).Scan(0, static (offset, floats) => offset + floats))
                .Traverse(pair => pair.First.Build(rows.Memory.Slice(pair.Second, pair.First.Floats)))
                .As()
                .Map(Chained);
    }

    private static (int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build) CurveTable(CurveLength length, (Vector3 Minimum, Vector3 Maximum) domain) =>
        (3 * length, rows => Lut1D.From(length, rows, domain.Minimum, domain.Maximum).Map(static table => (LutTable)new LutTable.ChannelCurve(table)));

    internal static (int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build) LatticeTable(int size, (Vector3 Minimum, Vector3 Maximum) domain, LutInterpolation interpolation) =>
        (3 * size * size * size, rows => Lut3D.From(size, rows, domain.Minimum, domain.Maximum).Map(lattice => (LutTable)new LutTable.Cube(lattice, interpolation)));

    internal static Fin<LutTable> UnitLattice(int size, ReadOnlyMemory<float> samples) =>
        LatticeTable(size, (Vector3.Zero, Vector3.One), LutInterpolation.Tetrahedral).Build(samples);

    internal static LutTable Chained(Seq<LutTable> steps) => steps is [var step] ? step : new LutTable.Sequence(steps);

    private static (Vector3 Minimum, Vector3 Maximum) Domain(Option<(Vector2 Value, int Line)> range, Option<(Vector3 Value, int Line)> low, Option<(Vector3 Value, int Line)> high) =>
        range.Match(
            Some: static held => (new Vector3(held.Value.X), new Vector3(held.Value.Y)),
            None: (low.Match(Some: static held => held.Value, None: Vector3.Zero), high.Match(Some: static held => held.Value, None: Vector3.One)));

    internal static LutTable.Affine Affine(ReadOnlySpan<double> values, int columns, (float In, float Out) scales) =>
        ((double)scales.In / scales.Out) switch {
            var gain => new LutTable.Affine(
                Matrix<double>.Build.DenseOfRowMajor(3, 3, [
                    values[0] * gain, values[1] * gain, values[2] * gain,
                    values[columns] * gain, values[columns + 1] * gain, values[columns + 2] * gain,
                    values[2 * columns] * gain, values[(2 * columns) + 1] * gain, values[(2 * columns) + 2] * gain,
                ]),
                columns is 4 ? (values[3] / scales.Out, values[7] / scales.Out, values[11] / scales.Out) : default),
        };

    // --- [RENDERS]
    internal static Fin<string> RenderCube(LutTable table) =>
        (Flat(table) switch {
            [LutTable.Cube { Lattice: var lattice }] =>
                Rows(Domained(Sized(new StringBuilder(), Lut3DSize, lattice.Size), lattice.DomainMinimum, lattice.DomainMaximum), lattice.Data, 3).ToString(),
            [LutTable.ChannelCurve { Table: var curve }] =>
                Rows(Domained(Sized(new StringBuilder(), Lut1DSize, curve.Length), curve.DomainMinimum, curve.DomainMaximum), Interleaved(curve), 3).ToString(),
            [LutTable.ChannelCurve { Table: var shaper }, LutTable.Cube { Lattice: var lattice }] => Resolve(shaper, lattice),
            [LutTable.Log { Curve: var curve }, LutTable.Cube { Lattice: var lattice }] => Some(curve).Filter(Uniform).Bind(Shaper).Bind(shaper => Resolve(shaper, lattice)),
            _ => Option<string>.None,
        }).ToFin(Unsupported(LutFormat.Cube));

    internal static Fin<string> RenderSpi1d(LutTable table) =>
        (from curve in Only<LutTable.ChannelCurve>(Flat(table)).Map(static step => step.Table)
         where Uniform(curve.DomainMinimum) && Uniform(curve.DomainMaximum)
         let width = curve.Channel(0).SequenceEqual(curve.Channel(1)) && curve.Channel(1).SequenceEqual(curve.Channel(2)) ? 1 : 3
         select Rows(
             new StringBuilder().Append(CultureInfo.InvariantCulture, $"{Version} 1\n{From} {curve.DomainMinimum.X} {curve.DomainMaximum.X}\n{Length} {curve.Length}\n{Components} {width}\n{{\n"),
             width is 1 ? curve.Channel(0) : Interleaved(curve),
             width).Append("}\n").ToString())
            .ToFin(Unsupported(LutFormat.Spi1d));

    internal static Fin<string> RenderSpi3d(LutTable table) =>
        (from lattice in Only<LutTable.Cube>(Flat(table)).Map(static step => step.Lattice)
         where lattice.DomainMinimum == Vector3.Zero && lattice.DomainMaximum == Vector3.One
         select Rows(new StringBuilder().Append(CultureInfo.InvariantCulture, $"{SpiLut} 1.0\n3 3\n{lattice.Size} {lattice.Size} {lattice.Size}\n"), Indexed(lattice), 6).ToString())
            .ToFin(Unsupported(LutFormat.Spi3d));

    internal static Fin<string> RenderSpiMatrix(LutTable table) =>
        (from affine in Only<LutTable.Affine>(Flat(table))
         select Rows(
             new StringBuilder(),
             [
                 affine.Matrix[0, 0], affine.Matrix[0, 1], affine.Matrix[0, 2], affine.Offset.X * ushort.MaxValue,
                 affine.Matrix[1, 0], affine.Matrix[1, 1], affine.Matrix[1, 2], affine.Offset.Y * ushort.MaxValue,
                 affine.Matrix[2, 0], affine.Matrix[2, 1], affine.Matrix[2, 2], affine.Offset.Z * ushort.MaxValue,
             ],
             4).ToString())
            .ToFin(Unsupported(LutFormat.SpiMatrix));

    internal static Fin<string> Render3dl(LutTable table) =>
        (from lattice in Only<LutTable.Cube>(Flat(table)).Map(static step => step.Lattice)
         where Coded(lattice) && lattice.Size > 3
         from mesh in OutputDepth.UInt10.Levels
         from codes in OutputDepth.UInt12.Levels
         select Rows(
             Rows(new StringBuilder(), Quantized(Enumerable.Range(0, lattice.Size).Select(at => (float)at / (lattice.Size - 1)).ToArray(), mesh), lattice.Size),
             Quantized(Transposed(lattice.Data, lattice.Size, 1f), codes),
             3).ToString())
            .ToFin(Unsupported(LutFormat.Autodesk));

    private static Option<string> Resolve(Lut1D shaper, Lut3D lattice) =>
        Uniform(shaper.DomainMinimum) && Uniform(shaper.DomainMaximum) && Uniform(lattice.DomainMinimum) && Uniform(lattice.DomainMaximum)
            ? Rows(Rows(Ranged(Sized(Ranged(Sized(new StringBuilder(), Lut1DSize, shaper.Length), Lut1DInputRange, shaper.DomainMinimum.X, shaper.DomainMaximum.X), Lut3DSize, lattice.Size),
                Lut3DInputRange, lattice.DomainMinimum.X, lattice.DomainMaximum.X), Interleaved(shaper), 3), lattice.Data, 3).ToString()
            : None;

    private static Option<Lut1D> Shaper(LogCurve curve) {
        float top = curve.Inverse(Vector3.One).X;
        using MemoryOwner<float> rows = MemoryOwner<float>.Allocate(3 * CurveLength.UInt16Codes);
        Span<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(rows.Span);
        for (int entry = 0; entry < entries.Length; entry++)
            entries[entry] = curve.Forward(new Vector3(entry * top / (entries.Length - 1)));
        return Lut1D.From(CurveLength.UInt16Codes, rows.Memory, Vector3.Zero, new Vector3(top)).ToOption();
    }

    private static float[] Indexed(Lut3D lattice) {
        int edge = lattice.Size;
        float[] rows = GC.AllocateUninitializedArray<float>(6 * edge * edge * edge);
        Span2D<float> grid = rows.AsSpan().AsSpan2D(edge * edge * edge, 6);
        ReadOnlySpan2D<float> samples = lattice.Data.AsSpan2D(edge * edge * edge, 3);
        for (int blue = 0; blue < edge; blue++)
            for (int green = 0; green < edge; green++)
                for (int red = 0; red < edge; red++) {
                    int node = (((blue * edge) + green) * edge) + red;
                    Span<float> row = grid.GetRowSpan(node);
                    (row[0], row[1], row[2]) = (red, green, blue);
                    samples.GetRowSpan(node).CopyTo(row[3..]);
                }
        return rows;
    }

    // --- [VALUES]
    internal static Seq<LutTable> Flat(LutTable table) => table is LutTable.Sequence { Steps: var steps } ? steps.Bind(Flat) : [table];

    internal static Option<T> Only<T>(Seq<LutTable> steps) where T : LutTable => steps is [T only] ? only : None;

    internal static bool Coded(Lut3D lattice) =>
        lattice.DomainMinimum == Vector3.Zero && lattice.DomainMaximum == Vector3.One && !lattice.Data.ContainsAnyExceptInRange(0f, 1f);

    private static bool Uniform(Vector3 value) => value.X == value.Y && value.Y == value.Z;

    private static bool Uniform(LogCurve curve) =>
        Uniform(curve.LogSlope) && Uniform(curve.LogOffset) && Uniform(curve.LinSlope) && Uniform(curve.LinOffset)
            && curve.Camera.ForAll(static camera => Uniform(camera.Break) && Uniform(camera.Slope));

    internal static float[] Interleaved(Lut1D curve) {
        float[] rows = GC.AllocateUninitializedArray<float>(3 * curve.Length);
        Span2D<float> grid = rows.AsSpan().AsSpan2D(curve.Length, 3);
        for (int channel = 0; channel < 3; channel++)
            curve.Channel(channel).AsSpan2D(curve.Length, 1).CopyTo(grid.Slice(0, channel, curve.Length, 1));
        return rows;
    }

    internal static float[] Transposed(ReadOnlySpan<float> values, int edge, float scale) {
        float[] reordered = GC.AllocateUninitializedArray<float>(values.Length);
        ReadOnlySpan2D<float> source = values.AsSpan2D(values.Length / 3, 3);
        Span2D<float> target = reordered.AsSpan().AsSpan2D(values.Length / 3, 3);
        for (int blue = 0; blue < edge; blue++)
            for (int green = 0; green < edge; green++)
                for (int red = 0; red < edge; red++)
                    source.GetRowSpan((((red * edge) + green) * edge) + blue).CopyTo(target.GetRowSpan((((blue * edge) + green) * edge) + red));
        TensorPrimitives.Multiply(reordered, scale, reordered);
        return reordered;
    }

    private static float[] Quantized(ReadOnlySpan<float> values, Quantizer levels) {
        float[] codes = GC.AllocateUninitializedArray<float>(values.Length);
        levels.Coded(values, codes);
        return codes;
    }

    // --- [PARSING]
    private static Option<int> Integer(string text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? Some(value) : None;

    internal static Option<float> Number(string text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? Some(value) : None;

    internal static Option<CurveLength> Entries(int count) => CurveLength.Validated(count).ToOption();

    internal static Option<Vector3> Triple(string text) => Numbers<float>(text).Bind(static values => values is [var r, var g, var b] ? Some(new Vector3(r, g, b)) : None);

    private static Option<Vector2> Pair(string text) => Numbers<float>(text).Bind(static values => values is [var low, var high] ? Some(new Vector2(low, high)) : None);

    internal static Option<T[]> Numbers<T>(ReadOnlySpan<char> text) where T : struct, INumberBase<T> {
        using SpanOwner<System.Range> tokens = SpanOwner<System.Range>.Allocate(text.Length + 1);
        T[] values = new T[text.SplitAny(tokens.Span, Blanks, StringSplitOptions.RemoveEmptyEntries)];
        return Parsed(text, tokens.Span[..values.Length], values) == values.Length ? values : None;
    }

    private static int Parsed<T>(ReadOnlySpan<char> text, ReadOnlySpan<System.Range> tokens, Span<T> into) where T : struct, INumberBase<T> {
        int at = 0;
        while (at < into.Length && T.TryParse(text[tokens[at]], NumberStyles.Float, CultureInfo.InvariantCulture, out into[at]))
            at++;
        return at;
    }

    // --- [PRINTING]
    internal static string Spaced(Vector3 value) => string.Create(CultureInfo.InvariantCulture, $"{value.X} {value.Y} {value.Z}");

    private static StringBuilder Sized(StringBuilder builder, string keyword, int size) => builder.Append(CultureInfo.InvariantCulture, $"{keyword} {size}\n");

    private static StringBuilder Domained(StringBuilder builder, Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? builder : builder.Append(CultureInfo.InvariantCulture, $"{DomainMin} {Spaced(minimum)}\n{DomainMax} {Spaced(maximum)}\n");

    private static StringBuilder Ranged(StringBuilder builder, string keyword, float minimum, float maximum) =>
        minimum is 0f && maximum is 1f ? builder : builder.Append(CultureInfo.InvariantCulture, $"{keyword} {minimum} {maximum}\n");

    internal static StringBuilder Rows<T>(StringBuilder builder, ReadOnlySpan<T> values, int width) where T : struct, INumberBase<T> {
        for (int at = 0; at < values.Length; at++)
            _ = builder.Append(CultureInfo.InvariantCulture, $"{values[at]}{(at % width == width - 1 ? '\n' : ' ')}");
        return builder;
    }

    // --- [FAULTS]
    internal static ImagingFault Misstated(LutFormat format, int line) => new(string.Create(CultureInfo.InvariantCulture, $"{format} LUT line {line}"), ImagingReason.Malformed);

    private static ImagingFault Sizeless(LutFormat format) => new($"{format} LUT size", ImagingReason.Empty);

    private static ImagingFault Miscounted(LutFormat format, long stated, long read) =>
        new(string.Create(CultureInfo.InvariantCulture, $"{format} LUT file of {read} entries for {stated} stated"), ImagingReason.Miscounted);

    internal static ImagingFault Unsupported(LutFormat format) => new($"{format} LUT table", ImagingReason.Unsupported);

    // --- [HEADERS]
    private sealed record Header(LutFormat Format, HashMap<string, (string Value, int Line)> Lines, int End) {
        public static Fin<Header> Read(string text, LutFormat format, Seq<string> keywords, Func<ReadOnlySpan<char>, bool> heading) {
            Span<System.Range> parts = stackalloc System.Range[2];
            (HashMap<string, (string Value, int Line)> lines, int number) = (HashMap<string, (string Value, int Line)>(), 0);
            foreach (ReadOnlySpan<char> raw in text.AsSpan().EnumerateLines()) {
                ReadOnlySpan<char> line = raw.Trim();
                number++;
                if (line.IsEmpty || line[0] == '#')
                    continue;
                if (!heading(line))
                    return new Header(format, lines, number);
                int count = line.SplitAny(parts, Blanks, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string keyword = line[parts[0]].ToString();
                if (keywords.Find(known => string.Equals(known, keyword, StringComparison.OrdinalIgnoreCase)).Filter(known => !lines.ContainsKey(known)).Case is not string key)
                    return Misstated(format, number);
                lines = lines.Add(key, (count is 2 ? line[parts[1]].ToString() : "", number));
            }
            return new Header(format, lines, number + 1);
        }

        public Fin<Option<(T Value, int Line)>> Stated<T>(string keyword, Func<string, Option<T>> parse) =>
            Lines.Find(keyword).Traverse(line => parse(line.Value).Map(value => (value, line.Line)).ToFin(Misstated(Format, line.Line))).As();
    }
}
