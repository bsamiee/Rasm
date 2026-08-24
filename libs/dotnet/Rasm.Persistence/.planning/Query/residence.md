# [PERSISTENCE_QUERY_RESIDENCE]

Rasm.Persistence declares the ANALYTICS RESIDENCE FAMILY here — one parameterized row set spanning the temporal projection tier, the interactive wide-event tier, and the cold tail, each answering the same capability columns and each provisioning itself from one branch-owned emitter. `ColumnType`, `ColumnShape`, `ColumnCell`, and `AnalyticsSchema` are the producer handoff vocabulary — `Rasm.Element` hands its datasets across the `[WIRE]: AnalyticsSchema` seam and `Rasm.Materials` hands its catalogue and texture generations across the `[WIRE]: MaterialsDataset` seam over the same Element table vocabulary; `AnalyticsSeam.Admit` gates that text into admitted identifiers; `ResidenceDdl` derives the ordered statement set a reviewed generation carries.

One declaration serves every consumer of a dataset: the DDL that plants its relation, the Substrait plan that reads it, the binary COPY that lands it, and the `RecordBatch` `ArrowLanding.Build` folds all derive from the same `AnalyticsSchema` rows, so field order, nullability, physical type, and reader ordinals cannot disagree. Residences carry ZERO authority — the receipt stream and the identity-tier journal own truth, a residence drops at warm-up cost and rebuilds from evidence, and no residence carries a cardinality ceiling because unbounded dimensionality is the reason it exists.

## [01]-[INDEX]

- [02]-[COLUMN_VOCABULARY]: `ColumnType` carries one physical token per dialect beside its Arrow type, its Arrow builder, its cell law, and the Substrait literal it admits; `ColumnShape` generates the containers over it; `ColumnCell` is the value a producer hands; `AnalyticsSchema` carries the temporal spine; `ArrowLanding.Build` folds declaration and cells into one metadata-bearing `RecordBatch`.
- [03]-[RESIDENCE_FAMILY]: `Residence` rows answer the estate residence floor beside this plane's dialect tokens, `ResidenceProjection` closes what a residence answers, `ResidenceTenancy` decides where the tenant byte rests, `ResidencePolicy` carries the tuned horizons, `EngineFault` renders every provider failure into one neutral pair, and `ResidenceFault` closes the band.
- [04]-[SEAM_ADMISSION]: `AnalyticsSeam.Admit` accumulates every column, key, spine, and measure refusal into one report, and `Seat` proves a dataset against a residence's floor columns and hands back the consumption descriptor.
- [05]-[PROVISIONING]: `ProvisionStep` types the SELECT-versus-CALL emission law, `ResidenceDdl` derives the column and sort-key projections both relational arms compose, and the three `*Residence.Statements` arms emit their own engine's ordered idempotent set.
- [06]-[RESEARCH]: open verification debts and their routes.

## [02]-[COLUMN_VOCABULARY]

- Owner: `ColumnType` is the physical-type correspondence — one neutral producer token answering the three query dialects, the binary-COPY wire type, the Arrow type, the Arrow BUILDER over a cell run, the CELL LAW proving and staging a producer's value, and the Substrait literal that token admits; `ColumnShape` is the container generator over it; `ColumnCell` is the closed value carrier a producer fills; `ColumnRow` is one admitted column; `TimeSpine` is the declared temporal category deciding who owns a dataset's clock; `AnalyticsSchema` is the seam value carrying the ordered key, the column roster, the time column, its category, and the optional measure a rollup folds; `ArrowLanding` is the ONE record-batch fold, deriving schema, field order, and column construction from that declaration.
- Cases: `ColumnType` rows are `Utf8`, `Float16`, `Float32`, `Float64`, `Int32`, `Int64`, `UInt8`, `UInt32`, `UInt64`, `Bool`, `Date`, `Timestamp`, and `KeyHex` — `Float16` the half-precision lane a kernel `ChannelDtype.Float16` corpus lands at its stored width, Arrow-faced `HalfFloatType.Default` while the three dialects widen to their narrowest real; `ColumnShape` is `Scalar` | `List` | `FixedList` | `Map` | `Dictionary`, the last an ENCODING declaration the Fleet dialect wraps and the other dialects leave bare and `FixedList` the declared-arity run a channel interleave states; `ColumnCell` is `Text` | `Real` | `Half` | `Whole` | `Flag` | `Day` | `Moment` | `Key` | `Items` | `Tags` | `Absent` — `Items`/`Tags` carrying their element type so a composite proves against its column exactly as a scalar does, and `Absent` the one absence spelling: the producer contract (`Rasm.Element` `Graph/table#TABLE_FAMILIES` `TableColumn.Nullable`) already admits an absent cell on a nullable column and refuses it otherwise, and this arm is that contract's landing spelling, proving at the COLUMN grain through `ColumnRow.Admits` on scalar shapes alone; `TimeSpine` is `Event` (the producer stamps its own observation clock as a declared column) and `Landing` (the producer declares none and this custodian stamps the moment it admitted the batch).
- Entry: `public static Fin<RecordBatch> ArrowLanding.Build<TRow>(AnalyticsSchema schema, Seq<TRow> rows, Func<TRow, Seq<ColumnCell>> cells, Seq<(string Key, string Value)> metadata, MemoryAllocator? allocator = null)` is the ONE batch fold — the field list, its order, and every column's construction derive from `schema`, the conformance proof accumulates across columns, and `metadata` carries the producer's receipt facts onto the schema; `ColumnShape.Column(Seq<ColumnCell>, MemoryAllocator?)` is the per-column arm it walks; `ColumnRow.Admits` is the column-grain conformance proof (absence against `Nullable`, presence through the shape's own gate) and `ColumnType.Cell.Stage` the binary-COPY bind, both read off the DECLARED column.
- Auto: adding a physical type is ONE `ColumnType` row answering every dialect column, its Arrow type, its builder, its cell law, and its plan literal; adding a container is ONE `ColumnShape` case whose five composer folds — `Arrow`, `Column`, `Bounded`, `Plan`, `Wire`, `Admits` — break at compile time; a metadata-free batch, a hand-built `Schema` beside a declared dataset, a positional column list, or a per-type builder helper is the deleted form because the declaration generates all of it. `metadata` is REQUIRED and never defaulted: `Schema.Builder` and `RecordBatch.Builder` expose no metadata seat, so a defaulted parameter silently drops the `content_key`/`strategy`/`at`/`points` facts every producer attaches and the batch reaches a reader carrying no provenance.
- Packages: Apache.Arrow (`RecordBatch`/`Schema`/`Field`/`IArrowArray`/`MemoryAllocator`/`ArrowBuffer.Builder<T>`/`ArrayData`/`PrimitiveArrayBuilder<T,TArray,TBuilder>`/`StringArray.Builder`/`BooleanArray.Builder`/`Date32Array.Builder`/`TimestampArray.Builder`/`ListArray`/`MapArray`/`StructArray`/`DictionaryArray`/`FixedSizeListArray`), Apache.Arrow.Arrays (`FixedSizeBinaryArray`), Apache.Arrow.Types, Npgsql (`NpgsqlBinaryImporter`/`NpgsqlDbType`), FlowtideDotNet.Substrait (`Expression`/`StringLiteral`/`NumericLiteral`/`BoolLiteral`), Rasm.Element (`Graph/table#TABLE_FAMILIES` `TableType`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new dialect column on every row is one constructor parameter the twelve rows answer together; a new producer token grammar is one arm in `AnalyticsSeam.Admitted`; a new cell arm is one `ColumnCell` case beside the one `ColumnType` row whose cell law admits it; zero new surface — a per-type Arrow builder helper, a second schema fold, a `RecordBatch.Builder` assembly for a metadata-bearing batch, or a shape-to-type lookup table beside the row set is the deleted form.
- Law: batch construction derives from the DECLARED schema — every column reads its own row's Arrow type and builder, so a positional column list beside a declared schema is the deleted form. Container arrays bind their CTOR over child arrays each element row's own builder produced, because `ListArray.Builder` and `MapArray.Builder` expose only the untyped `IArrowArrayBuilder<IArrowArray, …>` child face and `DictionaryArray` and `FixedSizeBinaryArray` ship no builder at all — one assembly discipline covers all five shapes and no case reaches through a cast the declaration already made unnecessary.
- Boundary: `ColumnType` and `ColumnShape` are BRANCH-LOCAL vocabulary and never a cross-language correspondence — no `libs/contracts/manifest.json` entry names a column type, and a peer runtime planting a residence relation reaches this custodian's DDL rather than spelling its own token set. Every wire token shared with the producer declaration derives from its `Rasm.Element` `Graph/table#TABLE_FAMILIES` `TableType` row (`Utf8`/`Float64`/`Int64`/`Bool`/`Date`/`Timestamp`/`KeyHex` read `.Key`), so the producer's declaration and the custodian's physical row cannot fork on one spelling; the width rows no producer declares (`Float16`, `Float32`, `Int32`, the unsigned trio) stay custodian-local literals; the Arrow face stays `FixedSizeBinaryType(16)` here because the cell packs sixteen big-endian bytes while the producer's own cell crosses as text. `ArrowLanding.Build` proves arity against the WHOLE declared roster because a batch carries every column including the custodian's own stamped ones, while the binary COPY proves against `ResidenceLanding.Supplied` — two rosters, and collapsing them reads a correct producer as a defective one.

```csharp signature
using Apache.Arrow;
using Apache.Arrow.Arrays;                        // FixedSizeBinaryArray — the key column, which ships no builder
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using NodaTime;
using NodaTime.Text;
using Npgsql;
using NpgsqlTypes;
using Rasm.Element.Graph;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// KEY mirrors the producer's own neutral token and every physical column is custodian-only, so a residence spells a
// producer token without the producer ever naming a storage type. `Arrow`, `Builder`, and `Cell` are the
// RECORD-BATCH, the array CONSTRUCTION, and the LANDING faces of one row.
// `Plan` renders a narrowing value as the Substrait literal the column's own type admits: a quoted string compared
// against an `Int64` column is a ClickHouse type error and a silently coerced Postgres one, and the two types
// Substrait carries NO literal for return `None`, because a tenant key and an instant are read SCOPE the frame owns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class ColumnType {
    public static readonly ColumnType Utf8 = new(TableType.Utf8.Key, "text", "String", "VARCHAR", StringType.Default,
        NpgsqlDbType.Text, Text, Chars, Law<ColumnCell.Text>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(c.Value, w)));
    // Half-precision lands at its STORED width on the Arrow face (the api-arrow narrow-lane law: a half lane
    // binds `HalfFloatArray`, never a widened `FloatArray`) while each dialect widens to its narrowest real —
    // no engine here carries a physical float2 — and the COPY stage widens once at the wire for the same reason.
    public static readonly ColumnType Float16 = new("float16", "real", "Float32", "FLOAT", HalfFloatType.Default,
        NpgsqlDbType.Real, Number, Column<ColumnCell.Half, Half>(static c => c.Value, Primitives<Half, HalfFloatArray, HalfFloatArray.Builder>), Halves);
    public static readonly ColumnType Float32 = new("float32", "real", "Float32", "FLOAT", FloatType.Default,
        NpgsqlDbType.Real, Number, Column<ColumnCell.Real, float>(static c => (float)c.Value, Primitives<float, FloatArray, FloatArray.Builder>), Reals);
    public static readonly ColumnType Float64 = new(TableType.Float64.Key, "double precision", "Float64", "DOUBLE", DoubleType.Default,
        NpgsqlDbType.Double, Number, Column<ColumnCell.Real, double>(static c => c.Value, Primitives<double, DoubleArray, DoubleArray.Builder>), Reals);
    public static readonly ColumnType Int32 = new("int32", "integer", "Int32", "INTEGER", Int32Type.Default,
        NpgsqlDbType.Integer, Number, Column<ColumnCell.Whole, int>(static c => (int)c.Value, Primitives<int, Int32Array, Int32Array.Builder>), Wholes(int.MinValue, int.MaxValue));
    public static readonly ColumnType Int64 = new(TableType.Int64.Key, "bigint", "Int64", "BIGINT", Int64Type.Default,
        NpgsqlDbType.Bigint, Number, Column<ColumnCell.Whole, long>(static c => c.Value, Primitives<long, Int64Array, Int64Array.Builder>), Wholes(long.MinValue, long.MaxValue));
    // Unsigned rows widen on the Series dialect because PostgreSQL carries no unsigned integer: a `UInt8` severity
    // lands in `smallint`, a `UInt32` in `bigint`, and a `UInt64` in `numeric(20,0)` because `bigint` is signed 64 and
    // an OTLP counter past 2^63 wraps to a negative rather than refusing. The Fleet and Lake dialects carry the exact
    // width, so the widening is one row's honest column and never a lost value. The cell carries `long`, so the
    // `UInt64` row's own bound is `long.MaxValue` and a counter past it has no cell to arrive in — the ceiling this
    // row states rather than the wrap the dialect widening exists to prevent.
    public static readonly ColumnType UInt8 = new("uint8", "smallint", "UInt8", "UTINYINT", UInt8Type.Default,
        NpgsqlDbType.Smallint, Number, Column<ColumnCell.Whole, byte>(static c => (byte)c.Value, Primitives<byte, UInt8Array, UInt8Array.Builder>), Wholes(byte.MinValue, byte.MaxValue));
    public static readonly ColumnType UInt32 = new("uint32", "bigint", "UInt32", "UINTEGER", UInt32Type.Default,
        NpgsqlDbType.Bigint, Number, Column<ColumnCell.Whole, uint>(static c => (uint)c.Value, Primitives<uint, UInt32Array, UInt32Array.Builder>), Wholes(uint.MinValue, uint.MaxValue));
    public static readonly ColumnType UInt64 = new("uint64", "numeric(20,0)", "UInt64", "UBIGINT", UInt64Type.Default,
        NpgsqlDbType.Numeric, Number, Column<ColumnCell.Whole, ulong>(static c => (ulong)c.Value, Primitives<ulong, UInt64Array, UInt64Array.Builder>), Wholes(0L, long.MaxValue));
    public static readonly ColumnType Bool = new(TableType.Bool.Key, "boolean", "Bool", "BOOLEAN", BooleanType.Default,
        NpgsqlDbType.Boolean, Flag, Flags, Law<ColumnCell.Flag>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(c.Value, w)));
    public static readonly ColumnType Date = new(TableType.Date.Key, "date", "Date32", "DATE", Date32Type.Default,
        NpgsqlDbType.Date, Unplanned, Days, Law<ColumnCell.Day>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(c.Value, w)));
    public static readonly ColumnType Timestamp = new(TableType.Timestamp.Key, "timestamptz", "DateTime64(9)", "TIMESTAMP_NS",
        Nanoseconds, NpgsqlDbType.TimestampTz, Unplanned, Moments,
        Law<ColumnCell.Moment>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(c.Value, w)));
    // Content-key token: the wire text is the `Rasm.Element` `TableType.KeyHex` row's own key, so the producer that
    // declares the column and the custodian that plants it read ONE spelling. Arrow ships no builder for a fixed-width
    // binary column, so `Keys` packs the sixteen big-endian bytes into one contiguous buffer and binds `ArrayData`.
    public static readonly ColumnType KeyHex = new(TableType.KeyHex.Key, "bytea", "FixedString(16)", "BLOB",
        new FixedSizeBinaryType(16), NpgsqlDbType.Bytea, Unplanned, Keys,
        Law<ColumnCell.Key>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(ColumnCell.Packed(c.Value), w)));

    public string Series { get; }
    public string Fleet { get; }
    public string Lake { get; }
    public IArrowType Arrow { get; }
    // Binary-COPY wire type: the Series tier lands through an importer that infers NOTHING from the
    // column list, so a row's declared physical type is the same value its ingest binds.
    public NpgsqlDbType Wire { get; }
    public Func<string, Option<Expression>> Plan { get; }
    public Func<Seq<ColumnCell>, MemoryAllocator?, IArrowArray> Builder { get; }
    public CellLaw Cell { get; }

    private ColumnType(string key, string series, string fleet, string lake, IArrowType arrow, NpgsqlDbType wire,
        Func<string, Option<Expression>> plan, Func<Seq<ColumnCell>, MemoryAllocator?, IArrowArray> builder, CellLaw cell) : this(key) =>
        (Series, Fleet, Lake, Arrow, Wire, Plan, Builder, Cell) = (series, fleet, lake, arrow, wire, plan, builder, cell);

    static readonly TimestampType Nanoseconds = new(TimeUnit.Nanosecond, "UTC");

    // `NumericLiteral.Value` is `decimal`, so every numeric narrowing crosses one parse and a magnitude past that
    // range refuses rather than lowering a rounded operand no predicate would match.
    static Option<Expression> Text(string value) => Some<Expression>(new StringLiteral { Value = value });
    static Option<Expression> Number(string value) =>
        decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal magnitude)
            ? Some<Expression>(new NumericLiteral { Value = magnitude })
            : None;
    static Option<Expression> Flag(string value) =>
        bool.TryParse(value, out bool state) ? Some<Expression>(new BoolLiteral { Value = state }) : None;
    static Option<Expression> Unplanned(string _) => None;

    // ONE mint per arm: `Admits` performs the arm test the whole family used to repeat as a nine-arm fold, so `Stage`
    // runs after the conformance gate proved the cast and carries no second test.
    static CellLaw Law<TCell>(Func<TCell, Fin<Unit>> canonical, Func<TCell, NpgsqlBinaryImporter, NpgsqlDbType, Task> stage)
        where TCell : ColumnCell =>
        new(cell => cell is TCell held ? canonical(held) : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, $"<cell-arm:{cell.GetType().Name}>")),
            (cell, importer, wire) => stage((TCell)cell, importer, wire));

    // Bounded integer rows prove RANGE at the cell, because a narrowing cast into the row's own Arrow width wraps
    // silently while the same value bound to the declared wire type refuses only after the copy staged its predecessors.
    static CellLaw Wholes(long low, long high) => Law<ColumnCell.Whole>(
        cell => cell.Value >= low && cell.Value <= high
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, $"<whole-range:{cell.Value}>")),
        static (cell, importer, wire) => importer.WriteAsync(cell.Value, wire));

    static readonly CellLaw Reals = Law<ColumnCell.Real>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync(c.Value, w));

    static readonly CellLaw Halves = Law<ColumnCell.Half>(static _ => Fin.Succ(unit), static (c, i, w) => i.WriteAsync((float)c.Value, w));

    // One projection per arm feeds ONE builder fold, so a row differs from its siblings by its Arrow width alone and
    // no per-type helper family survives beside the roster. The projection carries absence as the `Option` it is —
    // an `Absent` cell reaches a builder only through a column `ColumnRow.Admits` already proved nullable, and the
    // validity bitmap is the BUILDER's own to write, never this page's bit arithmetic.
    static Func<Seq<ColumnCell>, MemoryAllocator?, IArrowArray> Column<TCell, TValue>(
        Func<TCell, TValue> read, Func<Seq<Option<TValue>>, MemoryAllocator?, IArrowArray> build) where TCell : ColumnCell =>
        (cells, arena) => build(cells.Map(cell => cell is ColumnCell.Absent ? None : Some(read((TCell)cell))), arena);

    // `Reserve` then one span append is the reduced-call path every `PrimitiveArrayBuilder` publishes and stays the
    // all-present fast path; a run carrying absence walks once through the builder's own `AppendNull`. `bool` is
    // excluded from `ArrowBuffer.Builder<T>` outright, so the flag row takes the boolean builder's own arms.
    static IArrowArray Primitives<TValue, TArray, TBuilder>(Seq<Option<TValue>> values, MemoryAllocator? arena)
        where TValue : struct where TArray : IArrowArray
        where TBuilder : PrimitiveArrayBuilder<TValue, TArray, TBuilder>, new() {
        if (values.ForAll(static value => value.IsSome))
            return new TBuilder().Reserve(values.Count).Append(values.Somes().ToArray().AsSpan()).Build(arena);
        TBuilder builder = new();
        builder.Reserve(values.Count);
        values.Iter(value => value.Match(Some: held => builder.Append(held), None: () => builder.AppendNull()));
        return builder.Build(arena);
    }

    static IArrowArray Chars(Seq<ColumnCell> cells, MemoryAllocator? arena) =>
        cells.Fold(new StringArray.Builder(), static (builder, cell) => cell is ColumnCell.Absent
            ? builder.AppendNull()
            : builder.Append(((ColumnCell.Text)cell).Value)).Build(arena);
    static IArrowArray Flags(Seq<ColumnCell> cells, MemoryAllocator? arena) =>
        cells.Fold(new BooleanArray.Builder().Reserve(cells.Count), static (builder, cell) => cell is ColumnCell.Absent
            ? builder.AppendNull()
            : builder.Append(((ColumnCell.Flag)cell).Value)).Build(arena);
    static IArrowArray Days(Seq<ColumnCell> cells, MemoryAllocator? arena) =>
        cells.Fold(new Date32Array.Builder().Reserve(cells.Count), static (builder, cell) => cell is ColumnCell.Absent
            ? builder.AppendNull()
            : builder.Append(((ColumnCell.Day)cell).Value.ToDateOnly())).Build(arena);
    // Timestamp columns build under the FIELD's own unit and zone, so the batch column and the `timestamp-ns` DDL
    // carry one precision — a default-unit builder writes milliseconds under a nanosecond field with nothing raising.
    static IArrowArray Moments(Seq<ColumnCell> cells, MemoryAllocator? arena) =>
        cells.Fold(new TimestampArray.Builder(Nanoseconds), static (builder, cell) => cell is ColumnCell.Absent
            ? builder.AppendNull()
            : builder.Append(((ColumnCell.Moment)cell).Value.ToDateTimeOffset())).Build(arena);
    // Fixed-width binary ships an abstract `BuilderBase` and no concrete builder, so the key column writes its own
    // contiguous run and binds `ArrayData` directly — the zero-copy path rather than a builder that does not exist.
    // Absent keys write sixteen zero bytes under a cleared validity bit: value bytes below a null bit are unread
    // by the Arrow contract, and the fixed stride keeps every present slot addressable.
    static IArrowArray Keys(Seq<ColumnCell> cells, MemoryAllocator? arena) {
        ArrowBuffer.Builder<byte> packed = new(cells.Count * 16);
        ArrowBuffer.BitmapBuilder validity = new(cells.Count);
        int absent = 0;
        cells.Iter(cell => {
            bool present = cell is ColumnCell.Key held;
            packed.Append(present ? ColumnCell.Packed(((ColumnCell.Key)cell).Value).AsSpan() : stackalloc byte[16]);
            validity.Append(present);
            absent += present ? 0 : 1;
        });
        return new FixedSizeBinaryArray(new ArrayData(new FixedSizeBinaryType(16), cells.Count, absent, 0,
            Seq(absent == 0 ? ArrowBuffer.Empty : validity.Build(arena), packed.Build(arena))));
    }
}

// One cell law per physical row, replacing the three parallel per-arm folds a landing counterpart runs today: `Admits`
// proves a producer's cell belongs to this row AND is writable, and `Stage` binds it under the row's own wire type.
public readonly record struct CellLaw(
    Func<ColumnCell, Fin<Unit>> Admits,
    Func<ColumnCell, NpgsqlBinaryImporter, NpgsqlDbType, Task> Stage);

// Producer values arrive closed over the arms the vocabulary admits. Composite arms declare their ELEMENT type, so a
// container proves against its column's declared shape exactly as a scalar does and a heterogeneous bag has no cell
// to arrive in.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnCell {
    private ColumnCell() { }
    public sealed record Text(string Value) : ColumnCell;
    public sealed record Real(double Value) : ColumnCell;
    // Half-precision crosses at its STORED width so the Arrow lane never widens what a producer's tolerance proof
    // certified narrow; the relational stage alone widens, once, at its own wire type.
    public sealed record Half(System.Half Value) : ColumnCell;
    public sealed record Whole(long Value) : ColumnCell;
    public sealed record Flag(bool Value) : ColumnCell;
    public sealed record Day(LocalDate Value) : ColumnCell;
    public sealed record Moment(Instant Value) : ColumnCell;
    public sealed record Key(UInt128 Value) : ColumnCell;
    public sealed record Items(ColumnType Element, Seq<string> Values) : ColumnCell;
    public sealed record Tags(ColumnType Element, Seq<(string Key, string Value)> Pairs) : ColumnCell;
    // Absence spells ONE way at this seam: the producer contract (`Rasm.Element` `TableColumn.Nullable` — its
    // `Conforms` admits an absent cell on a nullable column and refuses it otherwise) needs a landing cell to
    // cross in, and an empty-string, zero, or epoch stand-in past this union is the deleted sentinel form. It
    // proves at the COLUMN grain through `ColumnRow.Admits` — no `ColumnType` row owns it — and lands as SQL
    // NULL on the COPY and a cleared validity bit on the Arrow face, scalar shapes alone.
    public sealed record Absent : ColumnCell;

    // Big-endian 16-byte pack of a key scalar: the tenant reaches it through `TenantId.Value` and a series through its
    // own `UInt128`, so ONE encoder serves every `KeyHex` column on every landing and every read inverse decodes
    // exactly what this staged.
    public static byte[] Packed(UInt128 key) {
        byte[] bytes = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, key);
        return bytes;
    }
}

// COMPOSITE shape over the scalar roster: `Map(Utf8, Utf8)` and `Map(Utf8, Float64)` are two values of one case,
// where a `map-string-string` scalar row would mint a roster entry per element pair and strand every pair nobody
// thought to name. Nesting is by construction, so a `List(Map(Utf8, Utf8))` resource-attribute run needs no new case.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnShape {
    private ColumnShape() { }
    public sealed record Scalar(ColumnType Type) : ColumnShape;
    public sealed record List(ColumnShape Element) : ColumnShape;
    public sealed record FixedList(ColumnShape Element, int Arity) : ColumnShape;
    public sealed record Map(ColumnType Key, ColumnShape Value) : ColumnShape;
    public sealed record Dictionary(ColumnType Element) : ColumnShape;

    // Scalar lift is IMPLICIT so a scalar column declares as its bare row and a composite spells its case — generated
    // conversion stays off because two cases take a `ColumnType` and emit an ambiguous pair.
    public static implicit operator ColumnShape(ColumnType type) => new Scalar(type);

    // Segment and skip-index eligibility: a columnstore segments BOUNDED TEXT and a bloom filter covers it, so a
    // dictionary-encoded text column is the paradigm case rather than an exception. A content key mints one compressed
    // batch per row, and a container carries no single value an index entry addresses.
    public bool Bounded => Switch(
        scalar:     static c => c.Type == ColumnType.Utf8,
        dictionary: static c => c.Element == ColumnType.Utf8,
        list:       static _ => false,
        fixedList:  static _ => false,
        map:        static _ => false);

    // Arrow face folds with the shape: `MapType(key, value)` builds the entries struct itself, and the dictionary
    // index is `Int32Type.Default` because that ctor THROWS on a non-integer index type.
    public IArrowType Arrow => Switch(
        scalar:     static c => c.Type.Arrow,
        list:       static c => new ListType(c.Element.Arrow),
        fixedList:  static c => (IArrowType)new FixedSizeListType(c.Element.Arrow, c.Arity),
        map:        static c => (IArrowType)new MapType(c.Key.Arrow, c.Value.Arrow),
        dictionary: static c => new DictionaryType(Int32Type.Default, c.Element.Arrow, ordered: false));

    // Substrait literal rendering is SCALAR-only: a narrowing predicate compares one value, and a container comparison
    // carries no literal the plan admits, so a filter over a map or list column refuses at lowering.
    public Func<string, Option<Expression>> Plan => Switch(
        scalar:     static c => c.Type.Plan,
        list:       static _ => Unplannable,
        fixedList:  static _ => Unplannable,
        map:        static _ => Unplannable,
        dictionary: static c => c.Element.Plan);

    // Binary-COPY wire type, FALLIBLE where the shape outruns what one `NpgsqlDbType` value spells: `Array` is a flag
    // OR'd onto its element, so a nested list has no second flag bit, a fixed-arity run carries no width the value can
    // state, and a map lands as `Jsonb` whose element typing the wire value cannot carry.
    public Fin<NpgsqlDbType> Wire => Switch(
        scalar:     static c => Fin.Succ(c.Type.Wire),
        dictionary: static c => Fin.Succ(c.Element.Wire),
        map:        static _ => Fin.Succ(NpgsqlDbType.Jsonb),
        fixedList:  static _ => Fin.Fail<NpgsqlDbType>(new ResidenceFault.Unwritable(Residence.Series.Key, "fixed-list")),
        list:       static c => c.Element is Scalar leaf
            ? Fin.Succ(NpgsqlDbType.Array | leaf.Type.Wire)
            : Fin.Fail<NpgsqlDbType>(new ResidenceFault.Unwritable(Residence.Series.Key, "nested-list")));

    // Conformance folds with the shape too: a scalar defers to its row's own cell law, and each composite proves the
    // arm, its element type, and the container's own arity or key rule. Composite arms carry their values as TEXT, so
    // a `list<float64>` column conforming on shape alone would bind a string run under a numeric wire type.
    public Fin<Unit> Admits(ColumnCell cell) => Switch(
        state:      cell,
        scalar:     static (c, s) => s.Type.Cell.Admits(c),
        dictionary: static (c, s) => s.Element.Cell.Admits(c),
        list:       static (c, s) => Run(s.Element, c).Bind(static run => Fin.Succ(unit)),
        fixedList:  static (c, s) => Run(s.Element, c).Bind(run => run.Count == s.Arity
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, $"<fixed-arity:{s.Arity}:{run.Count}>"))),
        map:        static (c, s) => c is ColumnCell.Tags tags && tags.Element == ColumnType.Utf8 && s.Value is Scalar { Type.Key: "utf8" }
            ? tags.Pairs.Map(static pair => pair.Key).Distinct().Count == tags.Pairs.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, "<map-duplicate-key>"))
            : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, "<map-value>")));

    // ONE column build per shape: scalar and dictionary rows reach their own `ColumnType.Builder`, and every container
    // binds its CTOR over the child array that builder produced.
    public IArrowArray Column(Seq<ColumnCell> cells, MemoryAllocator? arena) => Switch(
        state:      (Cells: cells, Arena: arena),
        scalar:     static (s, c) => c.Type.Builder(s.Cells, s.Arena),
        dictionary: static (s, c) => Encoded(c.Element, s.Cells, s.Arena),
        list:       static (s, c) => Nested(new ListType(c.Element.Arrow), c.Element, s.Cells, s.Arena),
        fixedList:  static (s, c) => new FixedSizeListArray(new FixedSizeListType(c.Element.Arrow, c.Arity), s.Cells.Count,
            c.Element.Column(Values(s.Cells), s.Arena), ArrowBuffer.Empty),
        map:        static (s, c) => Pairs(c.Key, c.Value, s.Cells, s.Arena));

    static Fin<Seq<string>> Run(ColumnShape element, ColumnCell cell) =>
        cell is ColumnCell.Items items && items.Element == ColumnType.Utf8 && element is Scalar { Type.Key: "utf8" }
            ? Fin.Succ(items.Values)
            : Fin.Fail<Seq<string>>(new ResidenceFault.Unwritable(Residence.Series.Key, "<list-element>"));

    static Seq<ColumnCell> Values(Seq<ColumnCell> cells) =>
        cells.Bind(static cell => ((ColumnCell.Items)cell).Values.Map(static value => (ColumnCell)new ColumnCell.Text(value)));

    // Offsets are the running start of each run plus the terminal length, which is exactly a prefix scan over the run
    // lengths — the one place a variable-width container needs arithmetic, and it needs it once for both containers.
    static ArrowBuffer Offsets(Seq<int> lengths, MemoryAllocator? arena) {
        ArrowBuffer.Builder<int> starts = new(lengths.Count + 1);
        lengths.Scan(0, static (at, length) => at + length).Iter(at => starts.Append(at));
        return starts.Build(arena);
    }

    static IArrowArray Nested(IArrowType shape, ColumnShape element, Seq<ColumnCell> cells, MemoryAllocator? arena) =>
        new ListArray(shape, cells.Count,
            Offsets(cells.Map(static cell => ((ColumnCell.Items)cell).Values.Count), arena),
            element.Column(Values(cells), arena), ArrowBuffer.Empty);

    static IArrowArray Pairs(ColumnType key, ColumnShape value, Seq<ColumnCell> cells, MemoryAllocator? arena) {
        Seq<(string Key, string Value)> flat = cells.Bind(static cell => ((ColumnCell.Tags)cell).Pairs);
        MapType shape = new(key.Arrow, value.Arrow);
        StructArray entries = new(shape.KeyValueType, flat.Count,
            Seq<IArrowArray>(key.Builder(flat.Map(static pair => (ColumnCell)new ColumnCell.Text(pair.Key)), arena),
                value.Column(flat.Map(static pair => (ColumnCell)new ColumnCell.Text(pair.Value)), arena)),
            ArrowBuffer.Empty);
        return new MapArray(shape, cells.Count,
            Offsets(cells.Map(static cell => ((ColumnCell.Tags)cell).Pairs.Count), arena), entries, ArrowBuffer.Empty);
    }

    // Dictionary encoding is a VALUE roster plus its index run, and `DictionaryArray` ships no builder — the roster
    // derives from the cells themselves, so a producer never freezes a value set the column then rejects.
    static IArrowArray Encoded(ColumnType element, Seq<ColumnCell> cells, MemoryAllocator? arena) {
        Seq<ColumnCell> roster = cells.Distinct().ToSeq();
        FrozenDictionary<ColumnCell, int> slots = roster.Zip(Range(0, roster.Count))
            .ToFrozenDictionary(static pair => pair.Item1, static pair => pair.Item2);
        IArrowArray indices = new Int32Array.Builder().Reserve(cells.Count)
            .Append(cells.Map(cell => slots[cell]).ToArray().AsSpan()).Build(arena);
        return new DictionaryArray(new DictionaryType(Int32Type.Default, element.Arrow, ordered: false),
            indices, element.Builder(roster, arena));
    }

    static readonly Func<string, Option<Expression>> Unplannable = static _ => None;
}

// One admitted column; `Identifier` is the trust gate the raw producer name crosses exactly once. `Admits` is the
// COLUMN-GRAIN conformance both landings run: absence proves against `Nullable` here — the one seat that holds it —
// on scalar shapes alone, and a present cell defers to the declared shape's own gate, so both landing folds read
// one proof and cannot disagree.
public readonly record struct ColumnRow(Identifier Name, ColumnShape Type, bool Nullable) {
    public Fin<Unit> Admits(ColumnCell cell) => cell switch {
        ColumnCell.Absent => Type is ColumnShape.Scalar && Nullable
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key,
                Nullable ? $"<absent-container:{(string)Name}>" : $"<absent-not-null:{(string)Name}>")),
        _ => Type.Admits(cell),
    };
}

// Temporal CATEGORY, declared and never inferred. EVENT-TIME datasets date by when the world produced the fact, so
// each names its own observation column and its producer stamps every cell; LANDING-TIME datasets date by when this
// custodian admitted them, so each names none. An optional clock alone leaves an event-time dataset silently re-dated
// to admission, and a board joining two datasets on time then compares two clocks under one axis with nothing raising.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class TimeSpine {
    public static readonly TimeSpine Event   = new("event");
    public static readonly TimeSpine Landing = new("landing");
}

// `Dataset` keeps the producer's dotted `<producer>.<source>` grammar as the wire value and `Table` is its admitted
// single-identifier projection. `Measure` names the numeric column a rollup folds — a wide-event dataset carries
// none, which keeps the Series arm from emitting a continuous aggregate over a column that dataset never declared.
public sealed record AnalyticsSchema(string Dataset, Seq<Identifier> Key, Seq<ColumnRow> Columns, Identifier Time, TimeSpine Spine, Option<Identifier> Measure) {
    // ONE canonical relation spelling all three dialects agree on: PostgreSQL FOLDS an unquoted identifier to lower
    // case while ClickHouse folds nothing and DuckDB preserves, so a mixed-case dataset provisions one relation and the
    // quoted plan then addresses another that was never created.
    public Identifier Table => Identifier.Create(Dataset.Replace('.', '_').ToLowerInvariant());
    public Seq<ColumnRow> Sorted => Key.Bind(key => Columns.Filter(column => column.Name == key));
    public Seq<ColumnRow> Payload => Columns.Filter(column => !Key.Contains(column.Name));
    public bool Declares(Identifier column) => Columns.Exists(row => row.Name == column);

    // Arrow face of the SAME declaration, metadata carried rather than dropped: `Schema.Builder` and
    // `RecordBatch.Builder` expose no metadata seat, so the receipt facts a producer attaches ride here or nowhere.
    public Schema Fields(Seq<(string Key, string Value)> metadata) =>
        new(Columns.Map(static column => new Field((string)column.Name, column.Type.Arrow, column.Nullable)),
            metadata.ToDictionary(static pair => pair.Key, static pair => pair.Value));

    // Declaration order IS the Substrait field-reference ordinal and the reader's column index alike, so a plan
    // builder addresses a column by NAME and every consumer's ordinals move with one column insert.
    public int Ordinal(Identifier column) => Columns.Map(static row => row.Name).IndexOf(column);
}
```

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
// ONE record-batch fold for the branch: the declaration supplies the field list, its order, and every column's own
// builder, so a landing binds pre-built columns through the metadata-bearing `RecordBatch` ctor and no producer
// re-declares field order beside the dataset it already declared.
public static class ArrowLanding {
    public static Fin<RecordBatch> Build<TRow>(AnalyticsSchema schema, Seq<TRow> rows, Func<TRow, Seq<ColumnCell>> cells,
        Seq<(string Key, string Value)> metadata, MemoryAllocator? allocator = null) {
        Seq<Seq<ColumnCell>> staged = rows.Map(cells);
        return Conformed(schema, staged)
            .Map(columns => new RecordBatch(schema.Fields(metadata), columns, rows.Count))
            .ToFin();
    }

    // Conformance ACCUMULATES: a producer handing one batch learns every offending column at once, because it cannot
    // see the second defect after the first refusal and a per-column round trip costs a whole batch each time. Arity
    // gates the column walk, since a short row shifts every later column onto a neighbour's proof.
    static Validation<Error, Seq<IArrowArray>> Conformed(AnalyticsSchema schema, Seq<Seq<ColumnCell>> rows) =>
        rows.Exists(row => row.Count != schema.Columns.Count)
            ? Fail<Error, Seq<IArrowArray>>(new ResidenceFault.IngestRefused(schema.Dataset,
                new EngineFault("<row-arity>", schema.Columns.Count.ToString(CultureInfo.InvariantCulture))))
            : schema.Columns.Zip(Pivot(rows, schema.Columns.Count))
                .Traverse(pair => Proven(pair.Item1, pair.Item2)).As();

    static Validation<Error, IArrowArray> Proven(ColumnRow column, Seq<ColumnCell> cells) =>
        cells.Traverse(cell => column.Admits(cell).ToValidation<Error>()).As()
            .Map(_ => column.Type.Column(cells, null));

    // ONE pivot: a producer hands ROWS and Arrow takes COLUMNS, so the transpose happens here rather than at every
    // producer arm re-spelling a column-major gather.
    static Seq<Seq<ColumnCell>> Pivot(Seq<Seq<ColumnCell>> rows, int arity) =>
        toSeq(Range(0, arity)).Map(index => rows.Map(row => row[index]));
}
```

## [03]-[RESIDENCE_FAMILY]

- Owner: `Residence` is the `[SmartEnum<string>]` residence family keyed by CAPABILITY — `Series` the temporal projection tier, `Fleet` the interactive wide-event tier, `Lake` the cold tail — each row answering the estate residence floor (`Fits`, `Admit`, `Tenancy`, `Lifetime` carrying both its extent and its ending owner, `Degrade`, and `Cap` stated permanently false) beside this plane's own extension: the projections it answers, its physical projection, its dialect tokens, its tenant and instant literals, and the provisioning statements its own engine runs; `ResidenceProjection` closes the projection vocabulary a residence declares rather than degrading silently; `ResidenceTenancy` decides where the tenant byte rests; `ResidencePolicy` carries the tuned retention, rollup grain, chunk, backfill, and root; `EngineFault` is the one engine-neutral diagnostic pair; `[FaultCase]` is the family's fault roster realizing the kernel `[FaultCase]` floor over the `Series` row; `ResidenceFault` closes that band above it and accumulates.
- Cases: `Residence` is `Series` (relational hypertables under in-database policies), `Fleet` (MergeTree wide events under table TTL), `Lake` (hive Parquet generations under generation eviction); `ResidenceProjection` is `Point`/`Window`/`Quantile`/`Aggregate`/`Fraction` and every residence publishes the subset it answers, so a plan naming an unanswered projection refuses typed at the seam carrying that row's `Degrade` clause instead of rendering an empty tile; `ResidenceTenancy` is `SortKey` (the residence stores the tenant as its leading column) and `Prefix` (a hive directory holds it and the scan projects it back), so tenancy decides where the byte rests and never how a predicate compares it; semantic `IngestRefused`/`ReadRefused` stay cause-less, while `ProviderIngestRefused`/`ProviderReadRefused` retain the exact documented engine cause; independent failures accumulate as `Error.Many`.
- Entry: `Residence.Render(ColumnShape)` is the ONE recursive dialect render every DDL arm and lowered projection reads; `Moment(Instant)` and `Partition(TenantContext)` are the instant and tenancy predicates each row spells for its own engine; `Answers(ResidenceProjection)` gates a read before a plan lowers; `Horizon(AnalyticsSchema)` is the family policy probe; `ReadRefused`/`IngestRefused` mint the two refusal shapes off the row's own `Diagnose`.
- Auto: the residence family is ONE row set answering the estate floor beside this plane's extension, so adding a residence is a row carrying what it fits, the entry that admits into it, its tenancy mechanism, how long a resident row survives beside the owner that ends it, its honest projection subset, its dialect tokens, and the clause naming what it gives up — a residence hardcoded below the family, a second query language, or a raw-SQL reader is the deleted form. `Cap` is STATED and permanently false rather than omitted, computed rather than passed, and INSTANCE rather than static: no row can answer it differently, so a constructor argument re-opens the choice this family exists to refuse, while a type-level member strands the one floor column a fold walking `Items` cannot read off the row beside its five siblings.
- Receipt: a provisioning derivation rides `store.columnar.residence.provision` carrying the residence and the step count; an ingest rides `store.columnar.residence.ingest` naming its dataset beside the staged count; a residence read rides `store.columnar.residence.read` carrying the residence key, the lowered text, the scanned rows, and the elapsed figure.
- Packages: Npgsql (`PostgresException.SqlState`/`MessageText`), ClickHouse.Driver (`ClickHouseServerException.ErrorCode`), DuckDB.NET.Data.Full (`DuckDBException.ErrorType`), NodaTime (`Instant`/`Duration`/`InstantPattern.ExtendedIso`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new residence is one `Residence` row answering the estate floor beside every extension column this plane declares; a new fault cause is one `ResidenceFault` case; a new projection is one `ResidenceProjection` row every residence's declared subset then answers or refuses; zero new surface — a per-environment policy script, an exporter-created table, a per-residence read entry, a metrics-store row, or a cardinality ceiling is the deleted form because the family generates the space and the engines own their own cadence.
- Law: `Lifetime` carries BOTH halves in one string — how long a resident row survives and which owner ends it — because a window stated without its scheduler promises an expiry no owner runs. `Degrade` states the honest clause a row gives up rather than a boolean, since what a residence cannot do is EVIDENCE both a tile and a refusal read. `Literal` is the column every dialect must answer and none can share: the tenant is a 16-byte key whose PHYSICAL spelling differs per engine, and a quoted-text comparison against a `bytea` or a `FixedString(16)` matches nothing and raises nothing.
- Boundary: a residence row is TEMPORAL by construction — every residence partitions, prunes, and expires on time — and no producer ever learns a chunk interval, a TTL, or a partition expression. Every residence is DERIVED and carries zero authority; reading one as authority turns a dropped accelerator into billing loss. NO analytics residence carries a cardinality cap and no row can grow one — a metrics store demands view caps because a TSDB indexes every series, while unbounded dimensionality IS the reason these residences exist. Provider failure renders through a TYPE TEST, never a cast: a driver raising a socket, TLS, or cancellation exception is not a `PostgresException`, and casting one at the fold throws straight out of the `Fin` the rail exists to carry. `Bucket` and `Quantile` are DECLARED rows whose lowering arms are owed at `Query/serving` — until that unit lands, the two projections they answer have no plan arm and a read naming one refuses `Unanswerable`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Projection vocabulary a residence ANSWERS: a residence answering fewer declares the subset on its row and the read
// refuses typed, so a tile degrades visibly rather than a second query path opening beside it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResidenceProjection {
    public static readonly ResidenceProjection Point     = new("point");
    public static readonly ResidenceProjection Window    = new("window");
    public static readonly ResidenceProjection Quantile  = new("quantile");
    public static readonly ResidenceProjection Aggregate = new("aggregate");
    public static readonly ResidenceProjection Fraction  = new("fraction");
}

// Tenancy mechanism per residence: a sort-key column prunes granules before the filter applies, a partition prefix
// prunes whole directories. Both resolve the SAME `TenantId.Wire` text, so a metric series and a residence row join on
// one alphabet.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResidenceTenancy {
    public static readonly ResidenceTenancy SortKey = new("sort-key");
    public static readonly ResidenceTenancy Prefix  = new("prefix");
}

// Retention, rollup grain, partition chunk, and residence root supplied by the composing root rather than baked.
// `Retain` is an INDEPENDENT coordinate, never a multiple of a series or alert window — deriving evidence retention
// from an alert lookback silently shortens the audit trail the moment an operator retunes the alert. `Root` names
// where a dataset's bytes rest, so the Lake arm reads its scan target from policy and never from a literal path.
public readonly record struct ResidencePolicy(Duration Retain, Duration Grain, Duration Chunk, Duration Backfill, StorePath Root);

// ONE diagnostic pair every engine renders its own failure into: a PostgreSQL `SqlState`, a ClickHouse numeric
// `ErrorCode`, and a DuckDB `ErrorType` are three alphabets for one question. `Code` stays the engine's OWN token — an
// estate-normalized code would erase the value an operator searches the engine's own documentation with.
public readonly record struct EngineFault(string Code, string Detail);

// `Fits`, `Admit`, `Tenancy`, `Lifetime`, `Degrade`, and `Cap` are the estate residence floor every branch's family
// answers, so a reader crossing this family and a peer's reads different VALUES under one column set.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Residence {
    public static readonly Residence Series = new("series", ResidenceTenancy.SortKey,
        "temporal projection tier: bounded-key streams a board reads at interactive latency off materialised summaries",
        "`ResidenceLanding.Stage` binary COPY beside `SeriesLane.Ingest`, both against relations this custodian provisioned",
        "the declared `ResidencePolicy.Retain` extent, ended in-database by the Timescale bgworker `add_retention_policy` arms",
        "single-node; admits no wide event — a payload column belongs to the Fleet tier",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Quantile, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Series,
        // PostgreSQL spells an array by suffix and has no inline dictionary wrapper, so the encoding token passes
        // through and TOAST owns the compression. A map lands as `jsonb`: no relational type carries a typed
        // key-value pair and `hstore` is text-to-text alone.
        static element => $"{element}[]",
        static (_, _) => "jsonb",
        static element => element,
        static name => $"\"{name}\"",
        static entry => $"'\\x{entry}'::bytea",
        static iso => $"CAST('{iso}' AS timestamptz)",
        static (column, grain) => $"time_bucket(INTERVAL '{Interval(grain)}', {column})",
        static (column, quantile) => $"percentile_cont({quantile.ToString("0.####", CultureInfo.InvariantCulture)}) WITHIN GROUP (ORDER BY {column})",
        static error => error is PostgresException wire
            ? Some(new EngineFault(wire.SqlState, wire.MessageText))
            : Option<EngineFault>.None,
        SeriesResidence.Statements);

    public static readonly Residence Fleet = new("fleet", ResidenceTenancy.SortKey,
        "interactive wide-event tier: any cardinality, the tenant leading the sort key so a single-tenant filter prunes granules",
        "the `Version/egress` ClickHouse sink under `insert_deduplication_token`, beside the collector's own OTLP wide events",
        "the declared `ResidencePolicy.Retain` extent, ended by the ClickHouse merge scheduler running the row's `TTL … DELETE`",
        "no transaction — every read is a convergence-consistent view the egress cursor bounds",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Quantile, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Fleet,
        // ClickHouse is the one dialect carrying all three containers natively, so the OTLP attribute map, the
        // span-event array, and the low-cardinality dimension each render exactly rather than widening.
        static element => $"Array({element})",
        static (key, value) => $"Map({key}, {value})",
        static element => $"LowCardinality({element})",
        static name => $"`{name}`",
        static entry => $"unhex('{entry}')",
        // ClickHouse's own CAST parses a space-separated datetime, never the ISO `T`/`Z` form, so this row spells the
        // best-effort parser rather than the shared cast its two siblings share.
        static iso => $"parseDateTime64BestEffort('{iso}', 9, 'UTC')",
        static (column, grain) => $"toStartOfInterval({column}, INTERVAL {(long)grain.TotalSeconds} SECOND)",
        static (column, quantile) => $"quantileTDigest({quantile.ToString("0.####", CultureInfo.InvariantCulture)})({column})",
        // ClickHouse's driver parses `ErrorCode` out of the server's own text and yields `-1` when that text is
        // unparsable, so the render carries whatever the driver resolved rather than asserting a code it never read.
        static error => error is ClickHouseServerException server
            ? Some(new EngineFault(server.ErrorCode.ToString(CultureInfo.InvariantCulture), server.Message))
            : Option<EngineFault>.None,
        FleetResidence.Statements);

    // Cold-tail reads answer scans and shares, never an interactive quantile: a Parquet generation carries
    // per-row-group statistics, not a digest. Its tenant literal is QUOTED TEXT like every other predicate here — a
    // hive key reads back as a `VARCHAR` column under `hive_partitioning`.
    public static readonly Residence Lake = new("lake", ResidenceTenancy.Prefix,
        "cold tail: cheapest per byte, batch scan over hive Parquet generations the object plane holds",
        "`#FLAT_TABLE_EGRESS`'s `LakeGeneration` writes the generations this row's VIEW names",
        "whatever the object plane leaves resident, ended by `Store/blobstore#BLOB_GC` generation eviction; this residence expires nothing itself",
        "no interactive latency and no digest — a quantile tile here reads as a report or refuses",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Lake,
        // DuckDB carries list and map natively; the dictionary token passes through because a DuckDB `ENUM` is a named
        // catalog type and declaring one would bind the scan to a value roster the producer never froze.
        static element => $"{element}[]",
        static (key, value) => $"MAP({key}, {value})",
        static element => element,
        static name => $"\"{name}\"",
        static entry => $"'{entry}'",
        static iso => $"CAST('{iso}' AS TIMESTAMP_NS)",
        static (column, grain) => $"time_bucket(INTERVAL '{Interval(grain)}', {column})",
        static (column, quantile) => $"approx_quantile({column}, {quantile.ToString("0.####", CultureInfo.InvariantCulture)})",
        static error => error is DuckDBException engine
            ? Some(new EngineFault(engine.ErrorType.ToString(), engine.Message))
            : Option<EngineFault>.None,
        LakeResidence.Statements);

    public ResidenceTenancy Tenancy { get; }
    public string Fits { get; }
    public string Admit { get; }
    public string Lifetime { get; }
    public string Degrade { get; }
    public bool Cap => false;
    public Seq<ResidenceProjection> Projections { get; }
    public Func<ColumnType, string> Physical { get; }
    public Func<string, string> ListOf { get; }
    public Func<string, string, string> MapOf { get; }
    public Func<string, string> DictOf { get; }
    public Func<Identifier, string> Quote { get; }
    public Func<string, string> Literal { get; }
    public Func<string, string> Stamp { get; }
    public Func<string, Duration, string> Bucket { get; }
    public Func<string, double, string> Quantile { get; }
    public Func<Exception, Option<EngineFault>> Diagnose { get; }
    public Func<AnalyticsSchema, ResidencePolicy, Seq<ProvisionStep>> Statements { get; }

    private Residence(string key, ResidenceTenancy tenancy, string fits, string admit, string lifetime, string degrade,
        Seq<ResidenceProjection> projections,
        Func<ColumnType, string> physical, Func<string, string> listOf, Func<string, string, string> mapOf,
        Func<string, string> dictOf, Func<Identifier, string> quote, Func<string, string> literal,
        Func<string, string> stamp, Func<string, Duration, string> bucket, Func<string, double, string> quantile,
        Func<Exception, Option<EngineFault>> diagnose,
        Func<AnalyticsSchema, ResidencePolicy, Seq<ProvisionStep>> statements) : this(key) =>
        (Tenancy, Fits, Admit, Lifetime, Degrade, Projections, Physical, ListOf, MapOf, DictOf, Quote, Literal, Stamp, Bucket, Quantile, Diagnose, Statements) =
        (tenancy, fits, admit, lifetime, degrade, projections, physical, listOf, mapOf, dictOf, quote, literal, stamp, bucket, quantile, diagnose, statements);

    // Each refusal is named for the FAULT it mints rather than the verb that raised it: the floor's own column names
    // are held here as properties, and a member group sharing a name with a property does not compile — so a factory
    // spelled `Admit` is unrepresentable while `Ingest` stays free for the landing entry.
    public Error ReadRefused(Error error) => error.Exception.Bind(Diagnose).Match<Error>(
        Some: engine => new ResidenceFault.ProviderReadRefused(Key, engine, error),
        None: () => error);
    public Error IngestRefused(Error error) => error.Exception.Bind(Diagnose).Match<Error>(
        Some: engine => new ResidenceFault.ProviderIngestRefused(Key, engine, error),
        None: () => error);

    // POLICY HEALTH derived from the row's OWN tokens: three engines run three expiry schedulers and each publishes
    // its self-report in a catalog only that engine has, so a probe reading one catalog measures one tier and reports
    // a healthy silence for the other two. This probe measures the resident time extent against the declared horizon,
    // which every residence answers because every residence partitions on time.
    public string Horizon(AnalyticsSchema schema) =>
        $"SELECT MIN({Quote(schema.Time)}), MAX({Quote(schema.Time)}), COUNT(*) FROM {Quote(schema.Table)}";

    public bool Answers(ResidenceProjection projection) => Projections.Contains(projection);

    // ONE recursive render every DDL arm and every lowered projection reads: the shape walks itself and the row
    // supplies four tokens. A `Map` key renders through `Physical` because a key is scalar by construction.
    public string Render(ColumnShape shape) => shape.Switch(
        state: this,
        scalar:     static (row, c) => row.Physical(c.Type),
        list:       static (row, c) => row.ListOf(row.Render(c.Element)),
        fixedList:  static (row, c) => row.ListOf(row.Render(c.Element)),
        map:        static (row, c) => row.MapOf(row.Physical(c.Key), row.Render(c.Value)),
        dictionary: static (row, c) => row.DictOf(row.Physical(c.Element)));

    // ONE instant spelling per dialect off one ISO text: a Substrait plan carries no timestamp literal, so an instant
    // reaching engine SQL as a bare number compares against three dialects as three type errors.
    public string Moment(Instant at) => Stamp(InstantPattern.ExtendedIso.Format(at));

    // ONE tenancy predicate every mechanism resolves: a sort-key residence compares its leading stored column and a
    // prefix residence compares the hive key its scan projects back, so both read as the same equality against
    // `TenantId.Wire`. Tenancy decides where the byte RESTS, never how a scan compares it.
    public string Partition(TenantContext tenant) => $"{Quote(TenantColumn)} = {Literal(tenant.Entry)}";

    public static readonly Identifier TenantColumn = Identifier.Create("tenant");

    // Postgres and DuckDB both read a bare INTERVAL literal; seconds is the one grain both accept without a unit
    // table, and the Fleet arm spells its own seconds form because ClickHouse takes no INTERVAL string.
    internal static string Interval(Duration grain) => $"{(long)grain.TotalSeconds} seconds";
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Series;
    private ResidenceFault() { }
    [FaultCase(0)]
    public sealed partial record IngestRefused(string Residence, EngineFault Engine) : ResidenceFault();
    [FaultCase(1)]
    public sealed partial record Unprovisioned(string Lane) : ResidenceFault();
    [FaultCase(2)]
    public sealed partial record ReadRefused(string Residence, EngineFault Engine) : ResidenceFault();
    // Refusal carries the row's own `Degrade` clause, so an operator reads WHY the residence cannot answer rather
    // than two keys and a shrug.
    [FaultCase(3)]
    public sealed partial record Unanswerable(string Residence, string Projection, string Degrade) : ResidenceFault();
    [FaultCase(4)]
    public sealed partial record Unlowerable(string Residence, string Node) : ResidenceFault();
    [FaultCase(5)]
    public sealed partial record Unwritable(string Residence, string Shape) : ResidenceFault();
    [FaultCase(6)]
    public sealed partial record ProviderReadRefused(string Residence, EngineFault Engine, Error Cause) : ResidenceFault(), ICausedFault;
    [FaultCase(7)]
    public sealed partial record ProviderIngestRefused(string Residence, EngineFault Engine, Error Cause) : ResidenceFault(), ICausedFault;


    public override string Message => Switch(
        ingestRefused: static c => $"<residence-ingest:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>",
        unprovisioned: static c => $"<residence-unprovisioned:{c.Lane}>",
        readRefused:   static c => $"<residence-read:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>",
        unanswerable:  static c => $"<residence-unanswerable:{c.Residence}:{c.Projection}:{c.Degrade}>",
        unlowerable:   static c => $"<residence-unlowerable:{c.Residence}:{c.Node}>",
        unwritable:    static c => $"<residence-unwritable:{c.Residence}:{c.Shape}>",
        providerReadRefused:   static c => $"<residence-read:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>",
        providerIngestRefused: static c => $"<residence-ingest:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>");
}
```

## [04]-[SEAM_ADMISSION]

- Owner: `AnalyticsSeam` is the ONE seam gate turning a producer's wire schema into admitted identifiers and vocabulary rows, and `ResidenceCharter` is the consumption descriptor a seated dataset reads back — what the residence is for, the entry that puts rows in, how long a row survives beside the owner that ends it, and the cap clause, all read off the seated row's own floor columns.
- Entry: `public static Fin<AnalyticsSchema> Admit(string dataset, Seq<(string Name, string Type, bool Nullable)> columns, Seq<string> key, string spine, Option<string> time = default, Option<string> measure = default)` admits a producer's declaration; `public static Validation<Error, ResidenceCharter> Seat(Residence residence, AnalyticsSchema schema, Seq<ResidenceProjection> wanted)` proves an admitted dataset against a residence's floor and hands back the descriptor.
- Auto: the admission ACCUMULATES and collapses to `Fin` exactly once at the seam edge — a producer handing a wire schema learns every bad column name, every unknown type token, every unresolvable key, and a contradicted category in ONE report, because it cannot see the second defect after the first refusal and each round trip costs a whole declaration. Composite type tokens carry the grammar the wide-event seam needs — `list<utf8>`, `fixed<float64,3>`, `map<utf8,float64>`, `dict<utf8>` — over the scalar roster, so an OTLP attribute map, a channel interleave, and a low-cardinality dimension all arrive as text a producer writes and become shape exactly here.
- Receipt: an admission carries no slot of its own — it is the gate every provisioning, landing, and read receipt downstream names its dataset from.
- Packages: Rasm.Element (`Graph/table#TABLE_FAMILIES` — the producer's own declaration owner), Thinktecture.Runtime.Extensions (`Validate` on every gated vocabulary), LanguageExt.Core (`Validation` applicative, `Traverse`), BCL inbox.
- Growth: a new token grammar is one arm in `Admitted`; a new floor column is one field on `ResidenceCharter` reading the row that already declares it; zero new surface — a per-producer admission entry, a second identifier gate, or a residence-specific column parser is the deleted form.
- Law: the seam drops the custodian's own tenant column from BOTH producer rosters exactly here, because a producer naming `tenant` describes the key the seam already stamps — every downstream derivation then reads one roster carrying it once, where a per-site filter leaves whichever site nobody remembered emitting a second column at a second physical type. Category and columns AGREE or the dataset never admits: a landing-time dataset naming its own instant hands the custodian a clock it does not own, and an event-time dataset naming none is re-dated to admission by the same append that serves the other category.
- Boundary: category crosses as TEXT exactly as every column token does, because the two AEC producers this seam names reference the kernel alone and sit BELOW this custodian — a typed parameter is unconstructable at both, and no reference closes that gap without inverting the edge the store already owns. Identity and CARDINALITY are both proven, because a membership test alone reads a repeated name as present: a twice-declared column mints two DDL entries at one name and a twice-named key mints a duplicate `orderby` entry TimescaleDB rejects outright. `Seat` reads the floor columns and never re-derives them — a residence that states a cap refuses an admission rather than accepting a dataset it will silently truncate, and today no row states one.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Consumption descriptor a seated dataset reads back: the residence floor is the ANSWER an admission returns, so a
// producer learns what it was admitted into, how rows enter, how long they survive beside the owner that ends them,
// and whether any ceiling applies. `Cap` is `None` while a residence states no ceiling and carries the row's own
// `Degrade` clause the moment one does, so an added ceiling reaches every producer as evidence rather than as silence.
public readonly record struct ResidenceCharter(string Residence, string Fits, string Admit, string Lifetime, Option<string> Cap);

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam gate: a producer hands dotted names and neutral type tokens as TEXT, and this is the one place that text
// becomes admitted identifiers and vocabulary rows. Every column admits before any statement composes, so a hostile
// producer name is a typed refusal at the seam rather than an interpolation site downstream.
public static class AnalyticsSeam {
    // Landing instant the custodian owns exactly as it owns the tenant column. A snapshot producer hands catalogue
    // columns and learns no storage concern — this seam appends the landing axis — while a producer that already
    // carries its own instant names it and keeps one column.
    public static readonly Identifier LandedColumn = Identifier.Create("landed_at");

    public static Fin<AnalyticsSchema> Admit(
        string dataset, Seq<(string Name, string Type, bool Nullable)> columns, Seq<string> key,
        string spine, Option<string> time = default, Option<string> measure = default) =>
        (columns.Traverse(Column).As(), key.Traverse(static name => Trusted(name)).As(),
            Category(spine), time.Traverse(Trusted).As(), measure.Traverse(Trusted).As())
            .Apply(static (rows, keys, category, at, value) =>
                (Rows: rows, Keys: keys, Spine: category, At: at, Value: value))
            .As()
            .Bind(parts => Spined(dataset, parts.Spine, parts.Rows, parts.Keys, parts.At, parts.Value))
            .Bind(static schema => Resolved(schema))
            .ToFin();

    // Floor read: the seated residence answers every projection the dataset needs and states its own cap, and the
    // charter carries the four columns back. A projection refusal names the row's `Degrade` clause, so the producer
    // reads WHY rather than being handed two keys.
    public static Validation<Error, ResidenceCharter> Seat(Residence residence, AnalyticsSchema schema, Seq<ResidenceProjection> wanted) =>
        (wanted.Traverse(projection => residence.Answers(projection)
            ? Success<Error, ResidenceProjection>(projection)
            : Fail<Error, ResidenceProjection>(new ResidenceFault.Unanswerable(residence.Key, projection.Key, residence.Degrade))).As(),
        schema.Columns.Traverse(column => residence.Tenancy == ResidenceTenancy.SortKey
            ? column.Type.Wire.ToValidation<Error>().Map(_ => column)
            : Success<Error, ColumnRow>(column)).As())
            .Apply(static (_, _) => unit).As()
            .Map(_ => new ResidenceCharter(residence.Key, residence.Fits, residence.Admit, residence.Lifetime,
                residence.Cap ? Some(residence.Degrade) : None));

    static Validation<Error, TimeSpine> Category(string token) =>
        TimeSpine.Validate(token, null, out TimeSpine? spine) is { } fault
            ? Fail<Error, TimeSpine>(fault)
            : Success<Error, TimeSpine>(spine!);

    // Landing columns APPEND, so the custodian's instant is the tail of `Columns` and the provisioned order runs
    // `tenant`, every supplied column, then that instant — the exact order a landing writes, so the COPY column list
    // and the write loop cannot drift.
    static Validation<Error, AnalyticsSchema> Spined(
        string dataset, TimeSpine spine, Seq<ColumnRow> rows, Seq<Identifier> keys,
        Option<Identifier> at, Option<Identifier> measure) {
        Seq<ColumnRow> supplied = rows.Filter(static column => column.Name != Residence.TenantColumn);
        Seq<Identifier> key = keys.Filter(static name => name != Residence.TenantColumn);
        return at.Match(
            Some: named => spine == TimeSpine.Event
                ? Success<Error, AnalyticsSchema>(new AnalyticsSchema(dataset, key, supplied, named, spine, measure))
                : Fail<Error, AnalyticsSchema>(new ResidenceFault.Unprovisioned($"<schema-spine:{dataset}:landing-names-clock>")),
            None: () => spine == TimeSpine.Landing
                ? Success<Error, AnalyticsSchema>(new AnalyticsSchema(dataset, key,
                    supplied + Seq(new ColumnRow(LandedColumn, ColumnType.Timestamp, Nullable: false)), LandedColumn, spine, measure))
                : Fail<Error, AnalyticsSchema>(new ResidenceFault.Unprovisioned($"<schema-spine:{dataset}:event-names-no-clock>")));
    }

    // Every declared identifier resolves against the roster BEFORE a statement composes: a key the columns omit, a
    // time column no residence can partition on, and a rollup measure no aggregate can fold each refuse here rather
    // than emitting DDL the engine rejects at parse time on a table that then half-exists.
    static Validation<Error, AnalyticsSchema> Resolved(AnalyticsSchema schema) =>
        ((schema.Key + Seq(schema.Time) + schema.Measure.ToSeq())
            .Traverse(name => schema.Declares(name)
                ? Success<Error, Identifier>(name)
                : Fail<Error, Identifier>(new ResidenceFault.Unprovisioned($"<schema-undeclared:{schema.Dataset}.{(string)name}>"))).As(),
        Unique(schema.Dataset, "columns", schema.Columns.Map(static column => column.Name)),
        Unique(schema.Dataset, "key", schema.Key))
            .Apply(static (_, _, _) => unit).As()
            .Map(_ => schema);

    static Validation<Error, Unit> Unique(string dataset, string roster, Seq<Identifier> names) =>
        names.Distinct().Count == names.Count
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new ResidenceFault.Unprovisioned($"<schema-duplicate:{dataset}:{roster}>"));

    static Validation<Error, ColumnRow> Column((string Name, string Type, bool Nullable) row) =>
        (Trusted(row.Name), Admitted(row.Type)).Apply((name, type) => new ColumnRow(name, type, row.Nullable)).As();

    static Validation<Error, Identifier> Trusted(string raw) =>
        Identifier.Validate(raw, null, out Identifier admitted) is { } fault
            ? Fail<Error, Identifier>(fault)
            : Success<Error, Identifier>(admitted);

    // Map keys are scalar tokens by construction and carry no comma, so the map split takes the FIRST comma and needs
    // no depth scan; the value recurses, which is what admits `map<utf8,list<utf8>>` whole. Fixed runs split on their
    // own last comma because the arity is the trailing token.
    static Validation<Error, ColumnShape> Admitted(string token) =>
        Wrapped(token, "list<") is { } element ? Admitted(element).Map(static shape => (ColumnShape)new ColumnShape.List(shape))
        : Wrapped(token, "fixed<") is { } run ? Fixed(run)
        : Wrapped(token, "dict<") is { } encoded ? Scalar(encoded).Map(static type => (ColumnShape)new ColumnShape.Dictionary(type))
        : Wrapped(token, "map<") is { } body ? Pair(body)
        : Scalar(token).Map(static type => (ColumnShape)type);

    static string? Wrapped(string token, string opener) =>
        token.StartsWith(opener, StringComparison.Ordinal) && token.EndsWith('>') ? token[opener.Length..^1] : null;

    static Validation<Error, ColumnShape> Pair(string body) =>
        body.IndexOf(',', StringComparison.Ordinal) is int cut && cut > 0
            ? (Scalar(body[..cut]), Admitted(body[(cut + 1)..]))
                .Apply(static (key, value) => (ColumnShape)new ColumnShape.Map(key, value)).As()
            : Fail<Error, ColumnShape>(new ResidenceFault.Unprovisioned($"<column-type:map<{body}>>"));

    // Arity admits POSITIVE alone: `FixedSizeListType` throws on a non-positive width, so the refusal is the seam's
    // rather than a throw the fold cannot carry.
    static Validation<Error, ColumnShape> Fixed(string body) =>
        body.LastIndexOf(',') is int cut && cut > 0
            && int.TryParse(body[(cut + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out int arity) && arity > 0
            ? Admitted(body[..cut]).Map(shape => (ColumnShape)new ColumnShape.FixedList(shape, arity))
            : Fail<Error, ColumnShape>(new ResidenceFault.Unprovisioned($"<column-type:fixed<{body}>>"));

    static Validation<Error, ColumnType> Scalar(string token) =>
        ColumnType.Validate(token, null, out ColumnType? type) is { } fault
            ? Fail<Error, ColumnType>(new ResidenceFault.Unprovisioned($"<column-type:{token}>"))
            : Success<Error, ColumnType>(type!);
}
```

## [05]-[PROVISIONING]

- Owner: `ProvisionStep` carries one emitted statement beside the VERB that runs it and the idempotence it claims; `SqlVerb` closes the emission vocabulary and renders each step's executable text; `ResidenceDdl` derives the column list and the sort key both relational arms compose and gates the derived set; `SeriesResidence`, `FleetResidence`, and `LakeResidence` are the three `Statements` delegate targets each `Residence` row names.
- Cases: `SqlVerb` is `Ddl` (a statement the engine executes as written), `Select` (a function invocation returning a row — `create_hypertable`, `add_retention_policy`, `add_continuous_aggregate_policy`), and `Call` (a procedure invocation returning none — `add_columnstore_policy`); each row renders its own invocation form, so a `CALL` against a function and a `SELECT` against a procedure are both unrepresentable from the derivation.
- Entry: `public static Fin<Seq<ProvisionStep>> ResidenceDdl.Provision(Residence residence, AnalyticsSchema schema, ResidencePolicy policy)` derives the WHOLE ordered statement set the reviewed generation artifact carries, so no environment hand-spells a policy script and no exporter creates a table.
- Auto: provisioning is DERIVED per residence from the schema's own spine — the Series arm splits SELECT functions from CALL procedures, the Fleet arm emits `CREATE TABLE … ENGINE = MergeTree` with the tenant leading and the time column trailing `ORDER BY`, a `TTL … DELETE` from the row's own retention window, and one `bloom_filter` skip index per admitted text column outside the sort key, and the Lake arm creates no storage and emits exactly the VIEW that gives its hive tree the name every lowered plan addresses; a measure-free dataset provisions its hypertable, columnstore, and retention and emits no continuous aggregate, so a wide event never grows a fabricated `avg` over a column it never declared.
- Receipt: a provisioning derivation rides `store.columnar.residence.provision` carrying the residence key and the step count, and each step's verb rides the generation artifact rather than the receipt.
- Packages: timescaledb + timescaledb_toolkit (`create_hypertable`/`by_range`/`add_retention_policy`/`add_columnstore_policy`/`add_continuous_aggregate_policy`/`time_bucket`/`time_weight`/`percentile_agg`/`average`/`approx_percentile`), ClickHouse (`MergeTree`/`TTL … DELETE`/`bloom_filter`), DuckDB (`read_parquet`/`hive_partitioning`/`union_by_name`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new statement is one `ProvisionStep` row in its arm carrying its verb; a new emission form is one `SqlVerb` row breaking every arm at compile time; zero new surface — a raw statement string beside a typed step, a guard ladder around a re-applied script, or a per-environment provisioning path is the deleted form.
- Law: the reviewed generation artifact RE-APPLIES whole, so a derived set carrying a step that cannot restate itself refuses at derivation rather than at the second apply against a half-provisioned relation. Grouping preserves IDENTITY and segmenting preserves COMPRESSION, and they are DIFFERENT lists derived from one schema: the rollup groups the whole key so each stream keeps its own buckets, while the columnstore segments the bounded text keys alone — segmenting a `KeyHex` content key mints one compressed batch per row and deletes the compression the columnstore exists for.
- Boundary: relation arguments are REGCLASS text and parse their own quoting, while a column argument and every storage-parameter entry are attname TEXT compared verbatim — so the relation carries the quoted spelling its own `CREATE` used and a column never does. Time trails the order list exactly once: a dataset naming its instant IN the key otherwise repeats it, and a duplicate `orderby` entry is a storage parameter the engine rejects outright. Rollups materialise toolkit SUMMARY state — `time_weight` beside `percentile_agg` — and the reader names its accessor, so the cheap tile and the expensive raw-chunk investigation answer ONE statistic.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Emission law as a VALUE: a TimescaleDB policy function returns a row and a policy procedure returns none, so the
// verb decides the invocation form and a step carries the invocation alone. `Idempotent` is the step's own claim the
// generation rail reads — every step this custodian derives carries its own `IF NOT EXISTS`, `OR REPLACE`, or
// `if_not_exists => TRUE` guard, and the column is what makes a step that cannot refusable rather than silently applied twice.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SqlVerb {
    public static readonly SqlVerb Ddl    = new("ddl", static text => text);
    public static readonly SqlVerb Select = new("select", static text => $"SELECT {text}");
    public static readonly SqlVerb Call   = new("call", static text => $"CALL {text}");

    public Func<string, string> Render { get; }
    private SqlVerb(string key, Func<string, string> render) : this(key) => Render = render;
}

public readonly record struct ProvisionStep(SqlVerb Verb, string Text, bool Idempotent) {
    public string Statement => Verb.Render(Text);

    public static ProvisionStep Ddl(string text) => new(SqlVerb.Ddl, text, Idempotent: true);
    public static ProvisionStep Select(string text) => new(SqlVerb.Select, text, Idempotent: true);
    public static ProvisionStep Call(string text) => new(SqlVerb.Call, text, Idempotent: true);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Parameterized provisioning: the WHOLE statement set derives from the residence row and the admitted schema.
public static class ResidenceDdl {
    public static Fin<Seq<ProvisionStep>> Provision(Residence residence, AnalyticsSchema schema, ResidencePolicy policy) =>
        residence.Statements(schema, policy) is var steps && steps.Find(static step => !step.Idempotent) is { IsSome: true } stray
            ? Fin.Fail<Seq<ProvisionStep>>(new ResidenceFault.Unprovisioned($"<provision-reapply:{residence.Key}:{stray.Map(static step => step.Text).IfNone(string.Empty)}>"))
            : Fin.Succ(steps);

    // Column list is one projection both the Series and Fleet arms compose. A sort-key residence LEADS with this
    // custodian's own tenant column at the key type every tenancy predicate compares against, while a prefix residence
    // declares none. The admitted roster carries no tenant column of its own, so the lead is unconditional.
    internal static string Columns(Residence residence, AnalyticsSchema schema) =>
        string.Join(", ", (residence.Tenancy.Switch(
                sortKey: () => Seq(new ColumnRow(Residence.TenantColumn, ColumnType.KeyHex, false)),
                prefix:  () => Seq<ColumnRow>())
            + schema.Columns)
            .Map(column => $"{residence.Quote(column.Name)} {residence.Render(column.Type)}{(column.Nullable ? string.Empty : " NOT NULL")}"));

    // Sort key is tenant-first and time-last by construction: the leading column prunes a single-tenant read to its own
    // granules and the trailing one prunes a window, so a schema whose key omits its time column still orders on it.
    internal static string Keys(Residence residence, AnalyticsSchema schema) =>
        string.Join(", ", (residence.Tenancy.Switch(sortKey: () => Seq(Residence.TenantColumn), prefix: () => Seq<Identifier>())
            + schema.Key
            + (schema.Key.Contains(schema.Time) ? Seq<Identifier>() : Seq(schema.Time))).Map(residence.Quote));
}

// Series residence: the relational hypertable tier. Every statement reads its schema's OWN spine, so a measure-free
// wide-event dataset provisions hypertable, columnstore, and retention and emits no rollup.
public static class SeriesResidence {
    public static Seq<ProvisionStep> Statements(AnalyticsSchema schema, ResidencePolicy policy) {
        string table = Residence.Series.Quote(schema.Table);
        string rollup = Residence.Series.Quote(Rollup(schema));
        string at = (string)schema.Time;
        string grouping = Names(Seq(Residence.TenantColumn) + schema.Sorted.Map(static column => column.Name));
        string segments = Names(Seq(Residence.TenantColumn) + Bounded(schema).Map(static column => column.Name));
        string ordering = Names(Unbounded(schema).Map(static column => column.Name).Filter(name => name != schema.Time) + Seq(schema.Time));
        string grain = Residence.Interval(policy.Grain);
        return Seq(
            ProvisionStep.Ddl($"CREATE TABLE IF NOT EXISTS {table} ({ResidenceDdl.Columns(Residence.Series, schema)})"),
            ProvisionStep.Select($"create_hypertable('{table}', by_range('{at}', INTERVAL '{Residence.Interval(policy.Chunk)}'), if_not_exists => TRUE)"),
            ProvisionStep.Ddl($"ALTER TABLE {table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = '{segments}', timescaledb.orderby = '{ordering}')"),
            ProvisionStep.Call($"add_columnstore_policy('{table}', after => INTERVAL '{Residence.Interval(policy.Chunk)}', if_not_exists => TRUE)"),
            ProvisionStep.Select($"add_retention_policy('{table}', drop_after => INTERVAL '{Residence.Interval(policy.Retain)}', if_not_exists => TRUE)"))
        + schema.Measure.ToSeq().Bind(value => Seq(
            ProvisionStep.Ddl($"CREATE MATERIALIZED VIEW IF NOT EXISTS {rollup} WITH (timescaledb.continuous) AS SELECT {grouping}, time_bucket(INTERVAL '{grain}', {at}) AS {Bucket}, time_weight('linear', {at}, {value}) AS {Weight}, percentile_agg({value}) AS {Sketch}, min({value}) AS {Low}, max({value}) AS {High}, count(*) AS {Samples} FROM {table} GROUP BY {grouping}, {Bucket} WITH NO DATA"),
            ProvisionStep.Ddl($"ALTER MATERIALIZED VIEW {rollup} SET (timescaledb.enable_columnstore = true)"),
            ProvisionStep.Select($"add_continuous_aggregate_policy('{rollup}', start_offset => INTERVAL '{Residence.Interval(policy.Backfill)}', end_offset => INTERVAL '{grain}', schedule_interval => INTERVAL '{grain}', if_not_exists => TRUE)")));
    }

    // Cardinality reads off the DECLARED type, never off a hand roster: a `Utf8` key is a bounded facet a filter
    // equals on, and every other key type is identity a segment list must not carry.
    static Seq<ColumnRow> Bounded(AnalyticsSchema schema) => schema.Sorted.Filter(static column => column.Type.Bounded);
    static Seq<ColumnRow> Unbounded(AnalyticsSchema schema) => schema.Sorted.Filter(static column => !column.Type.Bounded);
    static string Names(Seq<Identifier> columns) => string.Join(", ", columns.Map(static column => (string)column));

    // Rollup shape is this arm's own declaration, so a rollup column add moves the view and its reader together and no
    // read site spells a `_rollup` suffix or a fold alias twice.
    public static Identifier Rollup(AnalyticsSchema schema) => Identifier.Create($"{(string)schema.Table}_rollup");
    public static readonly Identifier Bucket = Identifier.Create("bucket");
    public static readonly Identifier Weight = Identifier.Create("weight");
    public static readonly Identifier Sketch = Identifier.Create("sketch");
    public static readonly Identifier Low = Identifier.Create("low");
    public static readonly Identifier High = Identifier.Create("high");
    public static readonly Identifier Samples = Identifier.Create("samples");

    // Read-time accessor projection over the materialised state, in the ordinal order the bucket row binds: the
    // quantile is a read argument rather than a second view, which is the two-stage discipline the toolkit's
    // aggregate/accessor split exists for.
    public static string Projection(double quantile) =>
        $"average({Weight}), approx_percentile({quantile.ToString("0.####", CultureInfo.InvariantCulture)}, {Sketch}), {Low}, {High}, {Samples}";
}

// Fleet residence: the interactive wide-event tier. Tenant leads `ORDER BY` so a single-tenant filter prunes granules
// BEFORE the predicate applies, and one bloom skip index per admitted text column outside the sort key prunes
// attribute-key existence before any value comparison.
public static class FleetResidence {
    public static Seq<ProvisionStep> Statements(AnalyticsSchema schema, ResidencePolicy policy) {
        string table = Residence.Fleet.Quote(schema.Table);
        string at = Residence.Fleet.Quote(schema.Time);
        return Seq(ProvisionStep.Ddl(
            $"CREATE TABLE IF NOT EXISTS {table} ({ResidenceDdl.Columns(Residence.Fleet, schema)}) " +
            $"ENGINE = MergeTree PARTITION BY toYYYYMM({at}) ORDER BY ({ResidenceDdl.Keys(Residence.Fleet, schema)}) " +
            $"TTL toDateTime({at}) + INTERVAL {(long)policy.Retain.TotalSeconds} SECOND DELETE SETTINGS index_granularity = 8192, ttl_only_drop_parts = 1"))
            + schema.Payload
                .Filter(static column => column.Type.Bounded)
                .Map(column => ProvisionStep.Ddl(
                    $"ALTER TABLE {table} ADD INDEX IF NOT EXISTS bloom_{column.Name} {Residence.Fleet.Quote(column.Name)} TYPE bloom_filter(0.01) GRANULARITY 1"));
    }
}

// Lake residence: the cold tail. The hive tree IS the schema, so this arm creates no storage and declares no column
// type. It emits exactly ONE statement, the VIEW that gives the tree the NAME the shared plan lowering addresses:
// without it a lowered `SELECT * FROM "<table>"` names nothing on a DuckDB lane. `union_by_name` makes an additive
// column compatible by construction and `hive_partitioning` projects the tenant directory back as the column the one
// tenancy predicate compares.
public static class LakeResidence {
    public static Seq<ProvisionStep> Statements(AnalyticsSchema schema, ResidencePolicy policy) => Seq(
        ProvisionStep.Ddl($"CREATE OR REPLACE VIEW {Residence.Lake.Quote(schema.Table)} AS SELECT * FROM read_parquet('{(string)policy.Root}/**/*.parquet', hive_partitioning = true, union_by_name = true)"));
}
```

## [06]-[RESEARCH]

(none)
