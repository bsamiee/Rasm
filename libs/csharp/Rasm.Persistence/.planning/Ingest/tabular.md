# [PERSISTENCE_INGEST_TABULAR]

Rasm.Persistence ingests and emits spreadsheet and delimited tabular data through ONE `TabularSource` owner over the MiniExcel zero-template streaming codec. A `TabularSpec` fixes a read once — its format, source (a path or a caller-owned stream), sheet, header stance, and an `Option<RowWindow>` sub-rectangle — and the owner discriminates every modality on that one value rather than on a method-name family: a typed POCO read, a `dynamic` read, a streaming `IDataReader`, a sheet/column probe, and the egress (write, sheet-append, template render, adorn, transcode) are all dispatches over `TabularSpec`, never parallel `ReadTyped`/`ReadDynamic`/`ReadStream` entrypoints. This codec NEVER knows the element graph — the per-app tabular→element map (the wire-composition owner, owned per-app at the host/app composition root, `§6`) projects each delivered row into a `Rasm.Element` graph node, so `TabularSource` delivers only the row shape at the wire and a workbook of element rows is to it an anonymous record stream.

One `TabularFormat` `[SmartEnum<string>]` crosses the format axis (`.xlsx`/`.csv`) with its `ExcelType` and the closed set of transcode targets it can reach, while the SPEC's `Policy()` composes the typed policy carrier (`OpenXmlConfiguration` with the shared-string disk cache on for `.xlsx`, `CsvConfiguration` for `.csv`) together with the registered wire columns and the report style — so a new tabular source is a format row and a transcode is a frozen `(from,to)` table lookup, never a second codec and never an `if`-ladder that can silently misroute a `.csv→.csv` no-op. Both the read boundary and the write boundary fold every MiniExcel exception — `ExcelColumnNotFoundException`, `ExcelInvalidCastException`, `MiniExcelNotSerializableException` — through one `TabularFault.Lift` funnel into a `Validation<TabularFault, …>` at the row boundary, so the receipt path never sees a thrown codec exception on either leg; the band derives `Code => FaultBand.Tabular + n` off the `Element/graph#FAULT_TABLES` registry (RE-BANDED 839x off the 837x three-way topology collision), with `BulkRefused` the bulk-lane refusal in-band. MiniExcel is the spreadsheet boundary the `Sep` delimited lane cannot reach and the columnar lane is the wrong shape for; `Sep` (`api-sep`) is the EXPLICIT high-throughput SIMD delimited owner — the fenced `DelimitedSource` surface over zero-allocation `ref struct` rows (`Enumerate`/`EnumerateAsync`/`ParallelEnumerate` materialization, `ISpanParsable<T>` typed columns, the pooled-string interning family, the reader→writer redaction/projection bridge), never a prose-only sibling — both projecting into the one downstream record rail. A typed read folds each header-keyed `dynamic` row through the ONE STJ wire factory (`api-nodatime-stj`/`api-thinktecture-json`) so an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the SAME converter set a JSON ingress uses and a column binds by the POCO's `[JsonPropertyName]` alias — NOT `MiniExcel.Query<T>` (whose reflective POCO binder neither invokes a `DynamicExcelColumn.CustomFormatter` nor mints a semantic type, the bypass-the-wire-factory hollow form), and never positional magic-index `dynamic` access on a known schema (`DynamicExcelColumn.CustomFormatter` is the cell projection on the `dynamic`/reader/write legs, where MiniExcel's own `DynamicColumns` honour it — and the spec's `Policy()` COMPOSES the `TabularWire.Wire`-built columns onto `Configuration.DynamicColumns`, so the wire column is a REGISTERED member, never a built-but-never-composed phantom); a redacted egress re-emits each row through a `RedactionPlan` that resolves a `Microsoft.Extensions.Compliance.Redaction` `Redactor` PER COLUMN from the field's `DataClassificationSet` (`api-redaction`) and redacts each classified cell through the generic typed-value `Redact<T>` — never a `value.ToString()`-then-redact hop — into `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table. Streaming `MiniExcelDataReader : IDataReader` feeds the `Query/columnar` Arrow/DuckDB materializer directly; the linq2db/EF bulk-copy lane (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a typed read yields, never the reader, because `BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced — the fenced `TabularBulk.Copy<T>` surface over the V6 identity `DbContext`, `BulkCopyType.ProviderSpecific` lowering to PG binary COPY, the mapping derived from the EF model (`EFCoreMetadataReader`/`GetMappingSchema`, no second mapping), and the bulk-ingress boundary stated as LAW: DuckDB `COPY` owns columnar FILES, the Npgsql binary importer owns raw-wire streams, linq2db owns the EF-MODEL-MAPPED lane — three non-overlapping rows on one rail. This codec composes NO `Rasm.Element` type — it delivers an anonymous record stream and the per-app wire-composition owner (NOT this codec) imports `Rasm.Element` to map a row to a `NodeId`/`ElementGraph` node; the bulk-copy bridge arrives from `api-linq2db-ef`; the columnar materializer from `Query/columnar`; the redaction `IRedactorProvider`/`DataClassificationSet` and `ReceiptSinkPort` are composition-root inputs; wall clock and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame ([A.1] — a `ClockPolicy` parameter on any signature here is the named strata inversion, no AppHost type crosses down).

## [01]-[INDEX]

- [01]-[TABULAR_SOURCE]: the one format-and-policy-and-transcode axis, the `TabularSpec` source discriminant (wire columns and report style as policy DATA), the spec-dispatched typed/dynamic/reader/probe ingress, the row-boundary fault fold over both legs, the wire-converter and redaction projections, and the egress/template/adorn/transcode surface with its typed fact stream.
- [02]-[DELIMITED_SOURCE]: the `Sep` SIMD delimited owner — the `DelimitedSpec` parse-policy value, the `Enumerate`/`EnumerateAsync`/`ParallelEnumerate` record-rail materialization, and the reader→writer redaction/projection bridge.
- [03]-[BULK_LANE]: the linq2db EF-model-mapped bulk-copy fence over the V6 identity `DbContext`, the `BulkCopyRowsCopied` typed receipt, and the three-row bulk-ingress boundary law.

## [02]-[TABULAR_SOURCE]

- Owner: `TabularFormat` the `[SmartEnum<string>]` format axis carrying its `ExcelType` and the frozen set of transcode targets it reaches; `TabularSpec` the `[ComplexValueObject]` read descriptor fixing format + source + sheet + header stance + window + WIRE COLUMNS + report STYLE in one value so every modality discriminates on it AND `Policy()` composes the typed configuration — the format policy, the `TabularWire.Wire` columns registered on `Configuration.DynamicColumns` (the phantom-resolution: the built formatter is now COMPOSED where MiniExcel honours it), and the `OpenXmlStyleOptions` report styling; `TabularSource.Origin` the closed `[Union]` source family (`FromPath | FromStream`) absorbing the path-vs-stream boundary modality the codec exposes as parallel overloads; `RowWindow` the cell-range window value; `RedactionPlan` the per-column `IRedactorProvider`-plus-`DataClassificationSet`-map a classified export carries; `TabularAdornment` the closed report-egress `[Union]` (`MergeCells | Pictures`); `TabularFault` the closed row-boundary fault band deriving `FaultBand.Tabular + n` off the `Element/graph#FAULT_TABLES` registry (re-banded 839x) folding all three MiniExcel exceptions plus the transcode and bulk refusals; `TabularFact` the typed receipt-stream record; `TabularSource` the static surface owning the spec-dispatched lazy typed/dynamic/reader/probe ingress, the row-boundary fault fold, the wire-converter + redaction projections, and the egress/template/adorn/transcode.
- Cases: `TabularFormat` is `Xlsx` (`ExcelType.XLSX`, an `OpenXmlConfiguration` with the shared-string disk cache on, transcode target `{Csv}`) or `Csv` (`ExcelType.CSV`, a `CsvConfiguration`, transcode target `{Xlsx}`); `Origin` is `FromPath(string)` (opens a shared-read `FileStream` via the path overload) or `FromStream(Func<Stream>)` (a caller-owned stream the `this Stream` overload reads); `RowWindow` carries the `A1:C3` start/end cells the `dynamic` `QueryRange` reads — a typed `Query<T>` reaches only `StartCell`, so a windowed typed read lowers through the `dynamic` `QueryRange` projected to `T`, never a non-existent typed `endCell` overload; `TabularFault` is `ColumnMissing | CellCast | NotSerializable` (the three lifted MiniExcel exceptions) plus `TranscodeUnreachable` (the routing-only rejection a `(from,to)` miss rails, never the `CellCast` cell-coercion arm) and `BulkRefused` (the `#BULK_LANE` copy refusal — a store/bridge fault, never a cell fault); `TabularAdornment` is `MergeCells(target)` (`MergeSameCells` vertical fold) or `Pictures(images)` (`AddPicture` embeds at cell addresses); `TabularFact` kinds are `read | write | append | template | adorn | transcode | redact | bulk`.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` reads typed rows — the header-keyed `dynamic` `Query` from `StartCell`, or the `dynamic` `QueryRange` when the spec carries a `RowWindow`, BOTH projected to `T` through the ONE `Project<T>`→`TabularWire.Bind<T>` STJ wire factory (`T` is unconstrained — the page never calls the `where T : class, new()` `MiniExcel.Query<T>` POCO binder, which both refuses an unconstrained `T` and bypasses the NodaTime/Thinktecture cell minting) — folding the row-boundary faults into one `Validation` and riding a `read` fact; `public static IO<Validation<TabularFault, Seq<HashMap<string,object>>>> Scan(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` reads header-keyed `dynamic` rows for the schema-unknown ingress; `public static IO<MiniExcelDataReader> Reader(TabularSpec spec)` projects the streaming reader the `Query/columnar` materializer consumes; `public static IO<TabularSheet> Probe(TabularSpec spec)` reads the sheet roster, per-sheet `SheetInfo`, used-range `ExcelRange`, and first-row column keys without reading the body; `public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, Option<RedactionPlan> plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` writes any `IEnumerable`/`IDataReader`/`DataTable`/anonymous value under the spec's own `HeaderRow` stance (a `bool header` knob beside the spec re-describes a value it already fixes — deleted), optionally re-emitting each row through the `RedactionPlan` per-column redactor map (a typed POCO lowered to its bag first so every write shape redacts), riding a `write` (or `redact`) fact; `public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` appends or overwrites a sheet in an existing workbook through `Insert`; `public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` renders an `.xlsx` template through `SaveAsByTemplate`; `public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` is the direct transcode discriminating on the SAME `TabularSpec` value (its `Format` IS the source format, its `Source` the origin — no redundant `from` knob), the `(spec.Format, to)` pair resolved against `spec.Format.Transcodes` so a no-op or unreachable pair rails the typed `TabularFault.TranscodeUnreachable` rather than a silent misroute or a mis-cased cell fault; `public static IO<Validation<TabularFault, Unit>> Adorn(TabularSpec spec, TabularAdornment adornment, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` is the report-egress finisher — `MergeCells` folds vertically-repeated values through `MergeSameCells`, `Pictures` embeds `MiniExcelPicture` payloads through `AddPicture` — one polymorphic dispatch riding an `adorn` fact, never sibling `Merge`/`Illustrate` entrypoints.
- Auto: a typed read folds the header-keyed `dynamic` `Query` (or `QueryRange` for a window) rows through `Project<T>`→`TabularWire.Bind<T>` — the ONE STJ wire factory round-trip (`api-nodatime-stj`/`api-thinktecture-json`) where an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the SAME converter set a JSON ingress uses and a column binds by the POCO's `[JsonPropertyName]` alias — so MiniExcel delivers only the header-keyed cell bag and the wire rail mints the semantic type, while `DateOnlyConversionMode.RequireMidnight` governs the boundary `DateOnly` admission; the windowed read lowers through `QueryRange` (the only public windowing surface, `dynamic`-yielding) projecting each `IDictionary<string,object>` row through the SAME `Bind<T>`, so `RowWindow.EndCell` is honoured rather than discarded and the windowed and non-windowed typed paths share one projection; the `DynamicExcelColumn.CustomFormatter` (`Func<object,object>`) that `TabularWire.Wire` builds is the cell projection on the `dynamic`/reader/write legs (where MiniExcel's `DynamicColumns` honour it), NOT the `Query<T>` POCO binder (which honours neither) — and `spec.Policy()` REGISTERS the built columns onto `Configuration.DynamicColumns` for every `WireColumns` name, so the projection actually fires on those legs (a `Wire` built but never assigned to `DynamicColumns` was the E9 dead carrier this composition retires); a large read streams through `Query`/`GetReader` (lazy `yield`) with `EnableSharedStringCache` on so the shared-strings table spills to `SharedStringCachePath` and a million-row workbook never fully resides in memory; the egress reuses the reader/lane rail the query produced (`SaveAs(stream, dataReader)`) so a schedule/cost/catalog export never re-materializes a table, report-shaped output uses `SaveAsByTemplate` with placeholder binding plus the `Adorn` finisher (`MergeSameCells`/`AddPicture`), header/cell styling rides the spec's `OpenXmlStyleOptions` policy value (`OpenXmlHeaderStyle`/`HorizontalCellAlignment` — never per-cell writes), a formula column is the declarative `ColumnType.Formula` attribute row and a runtime multi-sheet binding the `OpenXmlConfiguration.DynamicSheets` row, a multi-sheet workbook lands sheet-by-sheet through `Insert`, and `Reader` yields a `MiniExcelDataReader : IDataReader` the columnar materializer pulls; the redacted egress wraps the row enumerable in a projection that resolves a `Redactor` PER COLUMN from the `RedactionPlan`'s `DataClassificationSet` map through the AppHost `IRedactorProvider.GetRedactor` and rewrites each classified cell through the generic `Redactor.Redact<object>(value)` typed-value seam before `SaveAs` — a typed POCO lowered to its header-keyed bag first so every write shape redacts, the same span-based redactor the telemetry exporters use, so no unredacted value is `ToString`-stringified into transit first (the prior uniform-`Redactor`-plus-`value.ToString()` shape that left a typed POCO unredacted is the deleted hollow form).
- Receipt: every fact-bearing operation rides a `TabularFact` through the `ReceiptSinkPort` envelope under `store.tabular.*` — a `read` fact carrying the row count, sheet, and format; a `write`/`append` fact carrying the per-sheet row counts; a `template` fact; a `transcode` fact carrying the resolved `(from,to)`; a `redact` fact carrying the redacted-column count — one fact stream with a kind discriminant, never parallel receipt records; `Reader` alone emits no fact (it hands the raw `MiniExcelDataReader` to the `Query/columnar` materializer, which owns its own receipt over the bytes it pulls).
- Packages: MiniExcel (`MiniExcel.Query`/`QueryRange`/`GetReader`/`GetSheetNames`/`GetSheetInformations`/`GetColumns`/`GetSheetDimensions`/`SaveAs`/`Insert`/`SaveAsByTemplate`/`MergeSameCells`/`AddPicture`/`ConvertCsvToXlsx`/`ConvertXlsxToCsv`/`MiniExcelDataReader`/`OpenXmlConfiguration`/`CsvConfiguration`/`OpenXmlStyleOptions`/`OpenXmlHeaderStyle`/`MiniExcelPicture`/`ColumnType`/`DateOnlyConversionMode`/`DynamicExcelColumn`/`SheetInfo`/`ExcelRange` — the typed read rides the `dynamic` `Query`/`QueryRange` header-keyed bag, never the `where T : class, new()` `Query<T>` POCO binder), Microsoft.Extensions.Compliance.Redaction (`Redactor`/`IRedactorProvider`/`DataClassificationSet`), Sep (the `#DELIMITED_SOURCE` owner), linq2db.EntityFrameworkCore (the `#BULK_LANE` bridge), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new tabular format is one `TabularFormat` row carrying its `ExcelType` and transcode set (a new transcode target widens the row's `Transcodes` set, never a new entrypoint); a new column mapping is one `ExcelColumnAttribute`/`DynamicExcelColumn`; a new wire-cell projection is one `WireColumns` name the spec's `Policy()` registers; a new report decoration is one `TabularAdornment` case; a new style stance is one `OpenXmlStyleOptions` policy value on the spec; a new source kind is one `Origin` case; a new boundary-fault class is one `TabularFault` case inside the registry decade; zero new surface — a hand-rolled OpenXML/CSV parser, a second spreadsheet codec, a `QueryAsDataTable` whole-sheet buffer on a streaming path, an `if`-ladder transcode, a parallel `ReadTyped`/`ReadDynamic`/`ReadStream` family beside the spec discriminant, or a tabular→element map inside this codec is the deleted form because MiniExcel owns the spreadsheet/CSV codec, `Sep` the high-throughput delimited lane, the `TabularSpec` value the modality discriminant, and the app composition root the tabular→element projection.
- Boundary: `TabularSource` is the ONE tabular ingress/egress owner over MiniExcel — the `#DELIMITED_SOURCE` `Sep` owner is the high-throughput delimited-text sibling and the two project into the SAME downstream record rail, the source format selecting one, while `Apache.Arrow`/`DuckDB`/`ParquetSharp` own the binary columnar path (a `.xlsx` is never treated as a columnar file); the path-vs-stream modality the codec exposes as twin overloads is the `Origin` `[Union]`, so a read does not split into a path family and a stream family — one spec carries the source and one dispatch opens it; the typed read NEVER rides the `where T : class, new()` `MiniExcel.Query<T>` POCO binder — that overload both refuses an unconstrained `T` and bypasses the NodaTime/Thinktecture cell minting (so an `Instant`/value-object/smart-enum column silently fails to bind, the deleted hollow form) — so BOTH the start-cell typed read and the windowed typed read fold the header-keyed `dynamic` `Query`/`QueryRange` rows through the ONE `Project<T>`→`TabularWire.Bind<T>` STJ wire factory, the window adding only the `endCell` argument `Query<T>` cannot express; the codec NEVER knows the element graph — the per-app tabular→element map is the wire-composition owner (owned per-app at the host/app composition root, `§6`), so a workbook of element rows reads through `Read<T>`/`Reader`, the app projects each row into an `ElementGraph` node, and a catalog/cost/schedule egress writes element-derived tables back through `Write`/`Append`/`Render`, the codec seeing only the row shape; a typed read against a missing header surfaces `ExcelColumnNotFoundException`, a bad cell `ExcelInvalidCastException`, and a non-serializable write member `MiniExcelNotSerializableException`, all folded through the one `TabularFault.Lift` funnel into a typed `Validation` at the row boundary on BOTH the read and the write leg rather than thrown through the receipt path — the write leg folds faults exactly as the read leg does, so `NotSerializable` is a reachable case, not decoration; the bulk-copy rail (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a `Read<T>` yields (`BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced), while the `MiniExcelDataReader : IDataReader` from `Reader` feeds the `Query/columnar` Arrow/DuckDB materializer — the reader is the columnar source, the typed enumerable the bulk source, never conflated; a redacted export resolves the per-column `Redactor` from the field's `DataClassificationSet` through the AppHost `IRedactorProvider` and rewrites each classified cell before `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table (MiniExcel exposes no reader→writer copy bridge, so the redaction rides the row-enumerable egress, not a `DbDataReader` decorator — the DELIMITED redaction egress owns the real bridge: `#DELIMITED_SOURCE` re-emits Sep rows column-by-column through the writer `NewRow` with the classified cells redacted before `Col.Set`); MiniExcel retires `Sylvan.Data.Excel` as the spreadsheet codec — no `Sylvan` reference remains.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// the fault band derives from the KERNEL Expected (the federation base), aliased so the bare `Expected`
// names `Rasm.Domain.Expected` and never the `LanguageExt.Common.Expected` whose `(string,int,Option)`
// ctor is the deleted form. FaultBand arrives from the Element/graph#FAULT_TABLES registry.
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TabularFormat {
    public static readonly TabularFormat Xlsx = new("xlsx", ExcelType.XLSX, ["csv"]);
    public static readonly TabularFormat Csv = new("csv", ExcelType.CSV, ["xlsx"]);
    public ExcelType Excel { get; }
    public FrozenSet<string> Transcodes { get; }
    private TabularFormat(string key, ExcelType excel, string[] transcodes) : this(key) =>
        (Excel, Transcodes) = (excel, transcodes.ToFrozenSet(StringComparer.Ordinal));

    public bool CanReach(TabularFormat to) => Transcodes.Contains(to.Key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Origin {
    private Origin() { }
    public sealed record FromPath(string Path) : Origin;
    public sealed record FromStream(Func<Stream> Open) : Origin;

    public TResult Read<TResult>(Func<string, TResult> path, Func<Stream, TResult> stream) => Switch(
        fromPath:   static (project, c) => project.Path(c.Path),
        fromStream: static (project, c) => project.Stream(c.Open()),
        state: (Path: path, Stream: stream));
}

public readonly record struct RowWindow(string StartCell, string EndCell);

[ComplexValueObject]
public sealed partial class TabularSpec {
    public TabularFormat Format { get; }
    public Origin Source { get; }
    public Option<string> Sheet { get; }
    public bool HeaderRow { get; }
    public Option<RowWindow> Window { get; }
    public Seq<string> WireColumns { get; }
    public Option<OpenXmlStyleOptions> Style { get; }

    static Validation<ValidationError, TabularSpec> Validate(
        TabularFormat format, Origin source, Option<string> sheet, bool headerRow, Option<RowWindow> window, Seq<string> wireColumns, Option<OpenXmlStyleOptions> style) =>
        window.Match(
            Some: w => string.IsNullOrWhiteSpace(w.StartCell)
                ? Validation<ValidationError, TabularSpec>.Fail(ValidationError.Create("<tabular-spec-window-start>"))
                : Validation<ValidationError, TabularSpec>.Success(new TabularSpec(format, source, sheet, headerRow, window, wireColumns, style)),
            None: () => Validation<ValidationError, TabularSpec>.Success(new TabularSpec(format, source, sheet, headerRow, window, wireColumns, style)));

    public string StartCell => Window.Match(Some: static w => w.StartCell, None: static () => "A1");

    // the ONE policy composition: format policy + the REGISTERED wire columns + the report style. Registering the
    // TabularWire.Wire columns on Configuration.DynamicColumns is what makes the CustomFormatter fire on the
    // dynamic/reader/write legs — a built-but-unassigned column was the E9 dead carrier this member retires.
    public IConfiguration Policy() {
        Configuration policy = Format.Excel == ExcelType.XLSX
            ? new OpenXmlConfiguration {
                EnableSharedStringCache = true, TrimColumnNames = true, IgnoreEmptyRows = true,
                DateOnlyConversionMode = DateOnlyConversionMode.RequireMidnight,
                StyleOptions = Style.IfNoneUnsafe(() => null!) }
            : new CsvConfiguration { ReadEmptyStringAsNull = true };
        policy.DynamicColumns = [.. WireColumns.Map(TabularWire.Wire)];
        return policy;
    }
}

// the row-boundary fault band, RE-BANDED off the 837x three-way topology collision: a closed [Union] over the
// KERNEL `Rasm.Domain.Expected` (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited
// from `Error`), the SAME federation base the seam `ElementFault` (2500) and `BimFault` (2600) realize — NOT
// `LanguageExt.Common.Expected`, whose `base(detail, code, None)` ctor is the deleted form. Band membership
// derives `Code => FaultBand.Tabular + n` through the ONE Element/graph#FAULT_TABLES registry row (never a bare
// integer), so a recovery reads `error.IsType<TabularFault.CellCast>()` / `error.HasCode(8391)` /
// `error.Category()`, never a message substring. No `[GenerateUnionOps]` — the kernel union-ops generator is
// strictly opt-in; the `Expected` derivation lifts a bare case onto `Fin<T>`/`Validation` with no `.ToError()`
// hop and an `IValidationError` accumulation stays recoverable through `Error.Combine`.
[Union]
public abstract partial record TabularFault : Expected, IValidationError<TabularFault> {
    private TabularFault() : base() { }
    public sealed record ColumnMissing(string Column, int Row) : TabularFault;
    public sealed record CellCast(string Column, int Row, string Target) : TabularFault;
    public sealed record NotSerializable(string Member) : TabularFault;
    // the transcode-routing rejection: an unreachable `(from,to)` pair (a no-op `.csv->.csv` or an unsupported target)
    // is its OWN case, never the `CellCast` cell-coercion arm — the codec misroute is a routing fault, not a cell fault.
    public sealed record TranscodeUnreachable(string From, string To) : TabularFault;
    // the bulk-lane refusal: a BulkCopyAsync bridge/store fault — a copy fault, never a cell fault.
    public sealed record BulkRefused(string Detail) : TabularFault;

    public override int Code => FaultBand.Tabular + Switch(
        columnMissing:        static _ => 0,
        cellCast:             static _ => 1,
        notSerializable:      static _ => 2,
        transcodeUnreachable: static _ => 3,
        bulkRefused:          static _ => 4);

    public override string Message => Switch(
        columnMissing:        static c => $"<tabular-column-missing:{c.Column}@{c.Row}>",
        cellCast:             static c => $"<tabular-cell-cast:{c.Column}@{c.Row}:{c.Target}>",
        notSerializable:      static c => $"<tabular-not-serializable:{c.Member}>",
        transcodeUnreachable: static c => $"<tabular-transcode-unreachable:{c.From}->{c.To}>",
        bulkRefused:          static c => $"<tabular-bulk-refused:{c.Detail}>");

    public override string Category => Switch(
        columnMissing:        static _ => "Column",
        cellCast:             static _ => "Cell",
        notSerializable:      static _ => "Serialize",
        transcodeUnreachable: static _ => "Transcode",
        bulkRefused:          static _ => "Bulk");

    // the IValidationError<TabularFault>.Create admission the generated converter bridge calls on a deserialization
    // reject — routes the unspecific case rather than letting a raw message escape the typed family.
    public static TabularFault Create(string message) => new ColumnMissing(message, 0);

    public static TabularFault Lift(Exception boundary) => boundary switch {
        ExcelColumnNotFoundException c => new ColumnMissing(c.ColumnName ?? "", c.RowIndex ?? 0),
        ExcelInvalidCastException c => new CellCast(c.ColumnName ?? "", c.Row ?? 0, c.InvalidCastType?.Name ?? ""),
        MiniExcelNotSerializableException s => new NotSerializable(s.Member?.Name ?? ""),
        _ => new ColumnMissing(boundary.Message, 0),
    };
}

public readonly record struct TabularFact(string Kind, string Format, Option<string> Sheet, long Rows, Instant At);

public readonly record struct TabularSheet(Seq<SheetInfo> Sheets, Seq<string> Columns, Option<ExcelRange> Dimension);

// the report-egress finisher family: one closed [Union], one Adorn dispatch — never sibling Merge/Illustrate
// entrypoints. MergeCells folds vertically-repeated values (MergeSameCells writes the merged workbook to Target);
// Pictures embeds MiniExcelPicture payloads at their cell addresses (AddPicture mutates the workbook in place).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TabularAdornment {
    private TabularAdornment() { }
    public sealed record MergeCells(string Target) : TabularAdornment;
    public sealed record Pictures(Seq<MiniExcelPicture> Images) : TabularAdornment;
}

// the redacted-egress plan: the AppHost `IRedactorProvider` plus the per-column `DataClassificationSet` map a
// classified export carries, so `Cell` resolves a `Redactor` PER COLUMN from the field's classification through the
// `api-redaction` `GetRedactor(DataClassificationSet)` seam (the provider caches its resolved redactors, so a
// streamed million-row export pays the lookup but not the construction per cell) rather than one uniform redactor
// stamped on every cell; an unclassified column passes its cell through unchanged so only classified cells are
// rewritten, and `SetFallbackRedactor<ErasingRedactor>` fail-closes an unmapped-but-classified column at provider
// registration so a classified-shaped value erases rather than leaks.
public readonly record struct RedactionPlan(IRedactorProvider Provider, HashMap<string, DataClassificationSet> Columns) {
    // Redact ONE cell: a classified column rewrites its value through the resolved `Redactor.Redact<object>(value)`
    // typed-value seam (the redacted form is a string by contract); an UNCLASSIFIED column passes the cell through
    // UNCHANGED as its original typed `object?` — never stringified through a `NullRedactor`, so only classified
    // cells are rewritten and a typed number/instant in an unclassified column survives as itself in the export.
    public object? Cell(string column, object? value) =>
        Columns.Find(column).Match(Some: set => Provider.GetRedactor(set).Redact(value), None: () => value);
}

public static class TabularSource {
    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Window.IsSome
            ? spec.Source.Read(
                path:   p => Project<T>(MiniExcel.QueryRange(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Window.ValueUnsafe().EndCell, spec.Policy()).Cast<IDictionary<string, object>>()),
                stream: s => Project<T>(s.QueryRange(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Window.ValueUnsafe().EndCell, spec.Policy()).Cast<IDictionary<string, object>>()))
            : spec.Source.Read(
                path:   p => Project<T>(MiniExcel.Query(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Cast<IDictionary<string, object>>()),
                stream: s => Project<T>(s.Query(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Cast<IDictionary<string, object>>()))))
        from _ in rows.Match(
            Succ: r => sink(new TabularFact("read", spec.Format.Key, spec.Sheet, r.Count, frame.Now())),
            Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Seq<HashMap<string, object>>>> Scan(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => toSeq(MiniExcel.Query(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Select(Bag)),
            stream: s => toSeq(s.Query(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Select(Bag)))))
        from _ in rows.Match(Succ: r => sink(new TabularFact("read", spec.Format.Key, spec.Sheet, r.Count, frame.Now())), Fail: _ => IO.pure(unit))
        select rows;

    public static IO<MiniExcelDataReader> Reader(TabularSpec spec) =>
        IO.lift(() => spec.Source.Read(
            path:   p => MiniExcel.GetReader(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()),
            stream: s => s.GetReader(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy())));

    public static IO<TabularSheet> Probe(TabularSpec spec) =>
        IO.lift(() => spec.Source.Read(
            path: p => new TabularSheet(
                toSeq(MiniExcel.GetSheetInformations(p)),
                toSeq(MiniExcel.GetColumns(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy())),
                Optional(MiniExcel.GetSheetDimensions(p).FirstOrDefault())),
            stream: s => new TabularSheet(
                toSeq(s.GetSheetInformations()),
                toSeq(s.GetColumns(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy())),
                Optional(s.GetSheetDimensions().FirstOrDefault()))));

    // the header stance is the spec's own `HeaderRow` — a `bool header` parameter beside the spec re-describes
    // a value the discriminant already carries, the deleted knob.
    public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, Option<RedactionPlan> plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) {
        var (payload, kind) = plan.Match(Some: p => (Redact(value, p), "redact"), None: () => (value, "write"));
        return from counts in IO.lift(() => Capture(() => spec.Source.Read(
                   path:   p => MiniExcel.SaveAs(p, payload, spec.HeaderRow, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), overwriteFile: true),
                   stream: s => s.SaveAs(payload, spec.HeaderRow, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy()))))
               from _ in counts.Match(Succ: c => sink(new TabularFact(kind, spec.Format.Key, spec.Sheet, c.Sum(), frame.Now())), Fail: _ => IO.pure(unit))
               select counts;
    }

    public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => MiniExcel.Insert(p, value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), spec.HeaderRow, overwriteSheet: false),
            stream: s => s.Insert(value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), spec.HeaderRow, overwriteSheet: false))))
        from _ in rows.Match(Succ: r => sink(new TabularFact("append", spec.Format.Key, spec.Sheet, r, frame.Now())), Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => { MiniExcel.SaveAsByTemplate(p, template.ToArray(), value); return unit; },
            stream: s => { s.SaveAsByTemplate(template.ToArray(), value); return unit; })))
        from _ in done.Match(Succ: _ => sink(new TabularFact("template", spec.Format.Key, spec.Sheet, 0, frame.Now())), Fail: _ => IO.pure(unit))
        select done;

    // the `from` format is the spec's OWN `Format` (the source carries it), so transcode discriminates on the SAME
    // `TabularSpec` value every other modality does — `to` is the only added axis. A redundant `from` parameter beside
    // `spec.Format` is the knob the spec discriminant deletes.
    public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        !spec.Format.CanReach(to)
            ? IO.pure(Validation<TabularFault, Unit>.Fail(new TabularFault.TranscodeUnreachable(spec.Format.Key, to.Key)))
            : from done in IO.lift(() => Capture(() => spec.Source.Read(
                  path:   p => { Convert(p, target, spec.Format, to); return unit; },
                  stream: s => { Convert(s, target, spec.Format, to); return unit; })))
              from _ in done.Match(Succ: _ => sink(new TabularFact("transcode", $"{spec.Format.Key}->{to.Key}", None, 0, frame.Now())), Fail: _ => IO.pure(unit))
              select done;

    // the report-egress finisher: one dispatch over the closed adornment family, riding an `adorn` fact.
    public static IO<Validation<TabularFault, Unit>> Adorn(TabularSpec spec, TabularAdornment adornment, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => adornment.Switch(
            state: spec,
            mergeCells: static (s, m) => s.Source.Read(
                path:   p => { MiniExcel.MergeSameCells(m.Target, p); return unit; },
                stream: source => { using var buffered = new MemoryStream(); source.CopyTo(buffered); MiniExcel.MergeSameCells(m.Target, buffered.ToArray()); return unit; }),
            pictures: static (s, images) => s.Source.Read(
                path:   p => { MiniExcel.AddPicture(p, [.. images.Images]); return unit; },
                stream: source => { MiniExcel.AddPicture(source, [.. images.Images]); return unit; }))))
        from _ in done.Match(Succ: _ => sink(new TabularFact("adorn", spec.Format.Key, spec.Sheet, 0, frame.Now())), Fail: _ => IO.pure(unit))
        select done;

    internal static Validation<TabularFault, TValue> Capture<TValue>(Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: Validation<TabularFault, TValue>.Success,
            Fail: e => Validation<TabularFault, TValue>.Fail(TabularFault.Lift(e.ToException())));

    static Seq<T> Project<T>(IEnumerable<IDictionary<string, object>> rows) =>
        toSeq(rows.Select(row => TabularWire.Bind<T>(row)));

    static HashMap<string, object> Bag(dynamic row) =>
        toHashMap(((IDictionary<string, object>)row).Map(static kv => (kv.Key, kv.Value)));

    // A write `value` is an `IEnumerable` of rows (typed POCOs or `dynamic` bags) or a single row; each row redacts
    // through the plan and the lazy `Select` keeps the egress streamed — the redacted row enumerable feeds `SaveAs`
    // without materializing the table, so no unredacted cell is ever stringified into a buffer first.
    static object Redact(object value, RedactionPlan plan) =>
        value is IEnumerable rows and not string
            ? rows.Cast<object>().Select(row => TabularWire.RedactRow(row, plan))
            : TabularWire.RedactRow(value, plan);

    static void Convert(string source, string target, TabularFormat from, TabularFormat to) {
        if (from == TabularFormat.Csv && to == TabularFormat.Xlsx) { MiniExcel.ConvertCsvToXlsx(source, target); }
        else { MiniExcel.ConvertXlsxToCsv(source, target); }
    }

    static void Convert(Stream source, string target, TabularFormat from, TabularFormat to) {
        using var sink = File.Create(target);
        if (from == TabularFormat.Csv && to == TabularFormat.Xlsx) { MiniExcel.ConvertCsvToXlsx(source, sink); }
        else { MiniExcel.ConvertXlsxToCsv(source, sink); }
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------

public static class TabularWire {
    public static readonly JsonSerializerOptions Options = SuiteContracts.Wire;            // Thinktecture factory then ConfigureForNodaTime (api-thinktecture-json + api-nodatime-stj)

    // A header-keyed dynamic row is the one shape Query/QueryRange yield; serialize it through the configured
    // wire options so an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the SAME STJ factory
    // a JSON ingress uses — never a per-type cell coercion table re-derived here.
    public static T Bind<T>(IDictionary<string, object> row) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToUtf8Bytes(row, Options), Options)!;

    // the runtime column form for the `dynamic`/reader/write legs (where MiniExcel's `DynamicColumns` honour a
    // `CustomFormatter`): a `DynamicExcelColumn` whose `CustomFormatter` routes the raw cell through the wire
    // options, registered on the spec's policy `DynamicColumns`. NOT the typed-READ path — the typed read folds the
    // header-keyed `dynamic` row through `Bind<T>` (the STJ factory mints the cell), because the `Query<T>` POCO
    // binder honours no `CustomFormatter`; this projects a cell on a `dynamic` read/write where the binder does.
    public static DynamicExcelColumn Wire(string column) =>
        new(column) { CustomFormatter = cell => JsonSerializer.Serialize(cell, Options) };

    // Redact one row PER COLUMN against the plan (`plan.Cell`): a TYPED POCO (the common `Write` shape) lowers to its
    // header-keyed bag FIRST through the same STJ wire `Options` a read uses, so every write shape is redactable — a
    // typed POCO is never returned unredacted (the prior `: row` fall-through silently leaked its classified members,
    // the deleted hollow form). A classified column rewrites its cell through the generic `Redactor.Redact<object>(value)`
    // (the `api-redaction` typed-value seam), NOT `value.ToString()` then `Redact(string)` — the unredacted value is
    // never stringified into transit first, the same span-based path the telemetry exporters use — while an unclassified
    // column passes the cell through unchanged, so only classified cells are rewritten.
    public static object RedactRow(object row, RedactionPlan plan) =>
        Bag(row).ToDictionary(static kv => kv.Key, kv => plan.Cell(kv.Key, kv.Value));

    // Lower any row to its header-keyed cell bag: a `dynamic`/`IDictionary` row is its own bag; a typed POCO
    // round-trips through the one STJ wire `Options` (the SAME factory the `Bind`/`CustomFormatter` reads compose,
    // never a second reflection table) so a classified-member POCO surfaces its cells for per-column redaction.
    static IDictionary<string, object?> Bag(object row) =>
        row is IDictionary<string, object> dict
            ? dict.ToDictionary(static kv => kv.Key, static kv => (object?)kv.Value)
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.SerializeToUtf8Bytes(row, Options), Options) ?? [];
}
```

| [INDEX] | [POLICY]              | [VALUE]                                          | [BINDING]                                                  |
| :-----: | :-------------------- | :----------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | one tabular owner     | `TabularSource` over MiniExcel                   | `#DELIMITED_SOURCE` owns delimited text; one record rail   |
|  [02]   | modality discriminant | `TabularSpec` + `Origin` `[Union]`               | one value selects typed/dynamic/reader/probe               |
|  [03]   | typed read            | header-keyed `dynamic` `Query` → STJ `Bind<T>`   | unconstrained `T`; not the `class,new()` `Query<T>` binder |
|  [04]   | windowed typed read   | `dynamic` `QueryRange` → the SAME `Bind<T>`      | `QueryRange` adds only `endCell`; one shared projection    |
|  [05]   | wire-cell projection  | `DynamicExcelColumn.CustomFormatter`             | `spec.Policy()` registers on `DynamicColumns`, wire-minted |
|  [06]   | row-boundary fault    | `Validation<TabularFault, …>` on both legs       | read AND write fold; `NotSerializable` is reachable        |
|  [07]   | bulk vs columnar      | typed `IEnumerable<T>` vs `IDataReader`          | bulk-copy from `IEnumerable<T>`; reader feeds columnar     |
|  [08]   | redacted egress       | per-column `Redactor` before `SaveAs`            | streams column-redacted; no `DbDataReader` decorator       |
|  [09]   | transcode             | frozen `(from,to)` table lookup                  | a no-op or unreachable pair is a typed fault               |
|  [10]   | receipt               | one `TabularFact` stream under `store.tabular.*` | kind-discriminated; never parallel receipt records         |
|  [11]   | element projection    | per-app tabular→element map                      | the codec sees only the row shape; the app projects nodes  |
|  [12]   | fault band            | `Code => FaultBand.Tabular + n`                  | re-banded 839x off the 837x collision                      |
|  [13]   | report egress         | `Adorn` over `TabularAdornment` + spec `Style`   | merge/pictures/styling as policy DATA                      |

## [03]-[DELIMITED_SOURCE]

- Owner: `DelimitedSpec` the parse-policy value fixing separator (`None` = auto-detect from the first row), culture, trim classifier, header stance, parallelism, and the string-pool caps in one declaration; `DelimitedSource` the static surface owning the record-rail materialization (`Read<T>`), the async bulk stream (`Stream<T>`), and the reader→writer redaction/projection bridge (`Project`) — the `Sep` SIMD delimited owner made an EXPLICIT fence, projecting into the SAME downstream record rail the MiniExcel legs feed.
- Cases: a read materializes through `Enumerate(RowFunc<T>)` (sequential), `ParallelEnumerate(RowFunc<T>, degreeOfParallelism)` (order-preserving parallel projection — the spec's `Parallelism > 1` selects it and the THREAD-SAFE pool variant with it), or `EnumerateAsync(RowFunc<T>)` (the `IAsyncEnumerable<T>` bulk source); the `RowFunc<T>` projection is the ONE boundary where `ref struct` rows lift into domain records — `Col.Parse<T>()` for `ISpanParsable<T>` columns, the column span handed to the wire converters otherwise, never `string`-materialize-then-parse in the hot path.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` opens the reader over the `Origin` (`FromFile` on a path, `From` on a caller-owned stream), materializes through the spec-selected enumerator, folds the boundary through the shared `TabularSource.Capture` funnel, and rides a `read` fact; `public static async IAsyncEnumerable<T> Stream<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, CancellationToken token = default)` is the `#BULK_LANE` source — `FromFileAsync`/`FromAsync` then `EnumerateAsync`, streaming a delimited file into the bulk copy without buffering; `public static IO<Validation<TabularFault, long>> Project(DelimitedSpec spec, Origin source, Origin target, RedactionPlan plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` is the redaction/column-projection egress — reader→writer per row, an UNCLASSIFIED column copied span-to-span (`Col.Set(span)`, zero materialization) and a classified column redacted through `plan.Cell` before `Col.Set`, riding a `redact` fact.
- Auto: the options compose functionally off the spec — `Sep.New(char)` or the `Sep.Auto` `null` auto-detect, `SepReaderOptions.CultureInfo` `null` for the invariant fast path, `Trim` the `[Flags]` `SepTrim` classifier, `CreateToString` the pool policy (`PoolPerColThreadSafeFixedCapacity` under parallel consumption, `PoolPerCol` otherwise — low-cardinality identifier columns collapse to pooled lookups); header resolution rides `Header.IndexOf`/`IndicesOf(Span<string>, Span<int>)` (caller-buffer, zero-alloc) and prefix windows ride `Header.NamesStartingWith`; `Strict` hardens column-count + quoting on both reader and writer options; rows, cols, and headers are `ref struct` projections that NEVER escape the read scope — materialization is the one `Enumerate` boundary.
- Receipt: a delimited read rides `store.tabular.read` carrying the row count and `"delimited"` as the format; a projection/redaction pass rides `store.tabular.redact` carrying the emitted row count; the bulk stream emits no fact of its own (the `#BULK_LANE` receipt carries the copied count).
- Packages: Sep (`Sep`/`SepSpec`/`SepReaderOptions`/`SepWriterOptions`/`SepTrim`/`SepToString` pool family/`SepReader` + `Row`/`Col`/`Cols`/`RowFunc<T>`/`SepReaderHeader`/`Enumerate`/`EnumerateAsync`/`ParallelEnumerate`/`FromFile`/`From`/`FromFileAsync`/`FromAsync`/`Strict`/`SepWriter` + `NewRow` — namespace `nietras.SeparatedValues`), Microsoft.Extensions.Compliance.Redaction (the per-column redactor the bridge resolves), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new parse stance is one `DelimitedSpec` field consumed by the options fold; a new projection is one `RowFunc<T>` at the call site; zero new surface — a hand-rolled CSV split/parse pipeline, `string.Split` on a parse path, a `ref struct` row escaping the read scope, per-column `string`-materialize-then-parse in a hot path, a second delimited codec beside Sep, or MiniExcel's convenience CSV leg used for the high-throughput lane is the deleted form because Sep owns SIMD delimited text, the options are one declared policy value, and the record rail is the one materialization boundary.
- Boundary: `DelimitedSource` and `TabularSource` are TWO codecs on ONE record rail — the source format selects the owner (`.xlsx` and transcode symmetry → MiniExcel; high-throughput delimited text → Sep) and both deliver anonymous record streams the per-app composition root maps to elements (ARCH:61 row-shape law holds verbatim); the redaction bridge is the genuine reader→writer copy seam MiniExcel lacks (the writer `NewRow` re-emits a row column-by-column with only classified cells rewritten — the same `Redactor.Redact` typed-value seam, no unredacted value stringified into transit); the `Stream<T>` form is the `#BULK_LANE`'s `IAsyncEnumerable<T>` source (the `api-sep` stacking law: `Enumerate → BulkCopyAsync` streams a delimited file into PG binary COPY without buffering); Arrow/DuckDB own the BINARY columnar path — Sep never re-implements a columnar reader.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// the parse-policy value: separator None = auto-detect (Sep.Auto), culture None = the invariant fast path,
// Parallelism > 1 selects ParallelEnumerate + the thread-safe pool. The pool caps are policy fields — a
// per-call-site pool literal is the same policy decided twice.
public readonly record struct DelimitedSpec(
    Option<char> Separator, Option<CultureInfo> Culture, SepTrim Trim, bool Header, int Parallelism, int PoolMaxLength, int PoolCapacity) {
    public static readonly DelimitedSpec Default = new(None, None, SepTrim.None, true, 1, 128, 2048);

    public SepReaderOptions Options() =>
        (Separator.Map(Sep.New).Match(Some: static s => s.Reader(), None: static () => Sep.Auto.Reader()) with {
            CultureInfo = Culture.IfNoneUnsafe(() => null!),
            HasHeader = Header,
            Trim = Trim,
        }) with {
            CreateToString = Parallelism > 1
                ? SepToString.PoolPerColThreadSafeFixedCapacity(PoolMaxLength, PoolCapacity)
                : SepToString.PoolPerCol(PoolMaxLength, PoolCapacity, PoolCapacity),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class DelimitedSource {
    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => TabularSource.Capture(() => {
            using SepReader reader = Open(spec, source);
            return spec.Parallelism > 1
                ? toSeq(reader.ParallelEnumerate(shape, spec.Parallelism))
                : toSeq(reader.Enumerate(shape));
        }))
        from _ in rows.Match(Succ: r => sink(new TabularFact("read", "delimited", None, r.Count, frame.Now())), Fail: _ => IO.pure(unit))
        select rows;

    // the #BULK_LANE source: EnumerateAsync streams ref-struct rows into domain records with no file buffer —
    // the api-sep stacking law (Enumerate → BulkCopyAsync → PG binary COPY).
    public static async IAsyncEnumerable<T> Stream<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, [EnumeratorCancellation] CancellationToken token = default) {
        await using SepReader reader = await source.Read(
            path: p => spec.Options().FromFileAsync(p),
            stream: s => spec.Options().FromAsync(s)).ConfigureAwait(false);
        await foreach (T row in reader.EnumerateAsync(shape).WithCancellation(token).ConfigureAwait(false)) { yield return row; }
    }

    // the reader→writer redaction/projection egress: an UNCLASSIFIED column copies span-to-span (zero
    // materialization); a CLASSIFIED column redacts through plan.Cell before Col.Set. Exemption: the copy kernel
    // is the platform-forced ref-struct row loop — the rows never escape this scope.
    public static IO<Validation<TabularFault, long>> Project(DelimitedSpec spec, Origin source, Origin target, RedactionPlan plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from emitted in IO.lift(() => TabularSource.Capture(() => {
            using SepReader reader = Open(spec, source);
            using SepWriter writer = target.Read(
                path: p => Writer(spec).ToFile(p),
                stream: s => Writer(spec).To(s));
            long rows = 0;
            foreach (SepReader.Row row in reader) {
                using SepWriter.Row emit = writer.NewRow();
                for (int i = 0; i < row.ColCount; i++) {
                    string name = reader.Header.ColNames[i];
                    if (plan.Columns.ContainsKey(name)) { emit[i].Set($"{plan.Cell(name, row[i].Span.ToString())}"); }
                    else { emit[i].Set(row[i].Span); }
                }
                rows++;
            }
            return rows;
        }))
        from _ in emitted.Match(Succ: r => sink(new TabularFact("redact", "delimited", None, r, frame.Now())), Fail: _ => IO.pure(unit))
        select emitted;

    static SepReader Open(DelimitedSpec spec, Origin source) =>
        source.Read(
            path: p => spec.Options().FromFile(p),
            stream: s => spec.Options().From(s));

    // the projection writer inherits the spec's separator so the emitted file parses like its source — a
    // Sep.Default writer beside a comma-separated read is a silent dialect flip; auto-detect emits the default.
    static SepWriterOptions Writer(DelimitedSpec spec) =>
        spec.Separator.Map(Sep.New).Match(Some: static s => s.Writer(static o => o), None: static () => Sep.Default.Writer(static o => o));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                              | [BINDING]                                               |
| :-----: | :--------------- | :--------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | delimited owner  | Sep `ref struct` rows + `Enumerate`                  | SIMD parse; materialization is the record-rail boundary |
|  [02]   | parse policy     | `DelimitedSpec` one declared value                   | separator/culture/trim/pool/parallelism                 |
|  [03]   | parallel reads   | `ParallelEnumerate` + thread-safe pool               | order-preserving; `Parallelism` selects it              |
|  [04]   | redaction egress | `NewRow` per-column re-emit, redact before `Col.Set` | unclassified columns copy span-to-span                  |
|  [05]   | bulk source      | `Stream<T>` `IAsyncEnumerable<T>`                    | feeds `#BULK_LANE` `BulkCopyAsync`; no file buffering   |

## [04]-[BULK_LANE]

- Owner: `TabularBulk` the static bulk-copy fence over the linq2db EF bridge — ONE `Copy<T>` operation whose input SHAPE discriminates the source (`IEnumerable<T>` the materialized read, `IAsyncEnumerable<T>` the streamed `#DELIMITED_SOURCE`/`Read<T>` rail), lowering to PostgreSQL binary COPY over the V6 identity `DbContext` with the mapping DERIVED from the EF model — never a second mapping, never a per-source method family.
- Cases: the source is `IEnumerable<T>` or `IAsyncEnumerable<T>` (overloads on input shape, one semantic operation); the copy strategy is `BulkCopyOptions.BulkCopyType.ProviderSpecific` (PG binary COPY — the default this fence pins) with `MultipleRows` the engine-neutral fallback a profile may select; a refused copy rails `TabularFault.BulkRefused` in-band.
- Entry: `public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IAsyncEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class` and the `IEnumerable<T>` twin compose `LinqToDBForEFTools.BulkCopyAsync<T>(context, BulkCopyOptions, rows, token)` returning the TYPED `BulkCopyRowsCopied` receipt (rows copied + the options that produced it); `LinqToDBForEFTools.Initialize()` runs ONCE at the composition root (bridge-cache activation), never per call.
- Auto: the mapping schema derives from the EF model through `EFCoreMetadataReader`/`GetMappingSchema` so a `[ValueObject]`/NodaTime/NTS column that the V6 generated EF mapping owns survives the COPY path with ZERO second mapping; the high-throughput path rides `CreateLinqToDBConnectionDetached` semantics — a lifted read never feeds the EF change tracker (`EnableChangeTracker` stays false); the `RETURNING`-streaming upsert growth row is `MergeWithOutputAsync<TTarget,TSource,TOutput>` (`(action, deleted, inserted)` projection) on the same bridged context, a growth row on this owner, never a sibling surface.
- Receipt: a copy rides `store.tabular.bulk` carrying the copied-row count off the `BulkCopyRowsCopied` receipt and the batch bound; a refusal rides the typed `BulkRefused` on the rail.
- Packages: linq2db.EntityFrameworkCore (`LinqToDBForEFTools.BulkCopyAsync<T>`/`BulkCopy<T>`/`Initialize`/`GetMappingSchema`/`GetMetadataReader`/`EFCoreMetadataReader`/`CreateLinqToDBConnectionDetached`; core `linq2db` `BulkCopyOptions`/`BulkCopyType`/`BulkCopyRowsCopied`/`MergeWithOutputAsync`), Microsoft.EntityFrameworkCore (`DbContext` — the V6 identity context), LanguageExt.Core, BCL inbox.
- Growth: a new copy stance is one `BulkCopyOptions` field on the composed options value (`MaxBatchSize`/`KeepIdentity`/`ConflictAction`); a new upsert shape is one `MergeWithOutputAsync` projection; zero new surface — a hand-rolled INSERT loop, a second mapping beside the EF model, a change-tracked bulk lift, or a fourth bulk lane is the deleted form.
- Boundary: the three-row bulk-ingress boundary is LAW — DuckDB `COPY` owns columnar FILES (`Query/columnar`), the Npgsql binary importer owns raw-wire streams (`domain` bulk law at the store profile), and linq2db owns the EF-MODEL-MAPPED lane over the V6 identity `DbContext` — three non-overlapping rows on one rail, so a row entering through a typed domain record with an EF mapping takes THIS lane and never re-spells a COPY writer; the lane is `Ingest`-owned because its sources are the tabular/delimited codecs' record rails, while the target schema stays `Element/identity`'s (the DDL/migration owner) — this fence writes rows, never DDL.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
// ONE bulk operation, input-shape discriminated: the async form consumes the #DELIMITED_SOURCE Stream<T> rail,
// the sync form a materialized Read<T> result. ProviderSpecific = PG binary COPY; the mapping derives from the
// EF model (EFCoreMetadataReader) — no second mapping, no change-tracked lift, no per-source method family.
public static class TabularBulk {
    public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IAsyncEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class =>
        IO.liftAsync(async () => Validation<TabularFault, BulkCopyRowsCopied>.Success(
            await LinqToDBForEFTools.BulkCopyAsync(identity, Options(batch), rows, token).ConfigureAwait(false)))
        | @catch<IO, Validation<TabularFault, BulkCopyRowsCopied>>(static _ => true, static e => IO.pure(Validation<TabularFault, BulkCopyRowsCopied>.Fail(new TabularFault.BulkRefused(e.Message))));

    public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class =>
        IO.liftAsync(async () => Validation<TabularFault, BulkCopyRowsCopied>.Success(
            await LinqToDBForEFTools.BulkCopyAsync(identity, Options(batch), rows, token).ConfigureAwait(false)))
        | @catch<IO, Validation<TabularFault, BulkCopyRowsCopied>>(static _ => true, static e => IO.pure(Validation<TabularFault, BulkCopyRowsCopied>.Fail(new TabularFault.BulkRefused(e.Message))));

    static BulkCopyOptions Options(Option<int> batch) =>
        BulkCopyOptions.Default with {
            BulkCopyType = BulkCopyType.ProviderSpecific,
            MaxBatchSize = batch.Match(Some: static b => (int?)b, None: static () => null),
        };
}
```

| [INDEX] | [POLICY]      | [VALUE]                                                   | [BINDING]                                                  |
| :-----: | :------------ | :-------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | copy strategy | `BulkCopyType.ProviderSpecific`                           | PG binary COPY; `MultipleRows` the engine-neutral fallback |
|  [02]   | mapping       | EF model via `EFCoreMetadataReader`                       | the V6 generated mapping; never a second mapping           |
|  [03]   | source shape  | `IEnumerable<T>` / `IAsyncEnumerable<T>` overloads        | input shape discriminates; never a method family           |
|  [04]   | receipt       | `BulkCopyRowsCopied`                                      | typed copied-count evidence riding `store.tabular.bulk`    |
|  [05]   | bulk boundary | files → DuckDB `COPY`; wire → Npgsql; EF-mapped → linq2db | three non-overlapping rows; never a fourth lane            |
