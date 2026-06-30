# [PERSISTENCE_INGEST_TABULAR]

Rasm.Persistence ingests and emits spreadsheet and delimited tabular data through ONE `TabularSource` owner over the MiniExcel zero-template streaming codec. A `TabularSpec` fixes a read once — its format, source (a path or a caller-owned stream), sheet, header stance, and an `Option<RowWindow>` sub-rectangle — and the owner discriminates every modality on that one value rather than on a method-name family: a typed POCO read, a `dynamic` read, a streaming `IDataReader`, a sheet/column probe, and the egress (write, sheet-append, template render, transcode) are all dispatches over `TabularSpec`, never parallel `ReadTyped`/`ReadDynamic`/`ReadStream` entrypoints. The codec NEVER knows the element graph — the per-app tabular→element map (the wire-composition owner, owned per-app at the host/app composition root, `§6`) projects each delivered row into a `Rasm.Element` graph node, so `TabularSource` delivers only the row shape at the wire and a workbook of element rows is to it an anonymous record stream.

One `TabularFormat` `[SmartEnum<string>]` crosses the format axis (`.xlsx`/`.csv`) with its `ExcelType`, its typed policy carrier (`OpenXmlConfiguration` with the shared-string disk cache on for `.xlsx`, `CsvConfiguration` for `.csv`), and the closed set of transcode targets it can reach, so a new tabular source is a format row and a transcode is a frozen `(from,to)` table lookup — never a second codec and never an `if`-ladder that can silently misroute a `.csv→.csv` no-op. Both the read boundary and the write boundary fold every MiniExcel exception — `ExcelColumnNotFoundException`, `ExcelInvalidCastException`, `MiniExcelNotSerializableException` — through one `TabularFault.Lift` funnel into a `Validation<TabularFault, …>` at the row boundary, so the receipt path never sees a thrown codec exception on either leg. MiniExcel is the spreadsheet boundary the `Sep` delimited lane cannot reach and the columnar lane is the wrong shape for; `Sep` (`api-sep`) stays the high-throughput delimited codec, both projecting into the one downstream record rail. A typed read folds each header-keyed `dynamic` row through the ONE STJ wire factory (`api-nodatime-stj`/`api-thinktecture-json`) so an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the SAME converter set a JSON ingress uses and a column binds by the POCO's `[JsonPropertyName]` alias — NOT `MiniExcel.Query<T>` (whose reflective POCO binder neither invokes a `DynamicExcelColumn.CustomFormatter` nor mints a semantic type, the bypass-the-wire-factory hollow form), and never positional magic-index `dynamic` access on a known schema (`DynamicExcelColumn.CustomFormatter` is the cell projection on the `dynamic`/reader/write legs, where MiniExcel's own `DynamicColumns` honour it); a redacted egress re-emits each row through a `RedactionPlan` that resolves a `Microsoft.Extensions.Compliance.Redaction` `Redactor` PER COLUMN from the field's `DataClassificationSet` (`api-redaction`) and redacts each classified cell through the generic typed-value `Redact<T>` — never a `value.ToString()`-then-redact hop — into `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table. The streaming `MiniExcelDataReader : IDataReader` feeds the `Query/columnar` Arrow/DuckDB materializer directly; the linq2db/EF bulk-copy lane (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a typed read yields, never the reader, because `BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced. The codec composes NO `Rasm.Element` type — it delivers an anonymous record stream and the per-app wire-composition owner (NOT this codec) imports `Rasm.Element` to map a row to a `NodeId`/`ElementGraph` node; the bulk-copy bridge arrives from `api-linq2db-ef`; the columnar materializer from `Query/columnar`; the redaction `IRedactorProvider`, `DataClassificationSet`, `ClockPolicy`, `ReceiptSinkPort` from AppHost.

## [01]-[INDEX]

- [01]-[TABULAR_SOURCE]: the one format-and-policy-and-transcode axis, the `TabularSpec` source discriminant, the spec-dispatched typed/dynamic/reader/probe ingress, the row-boundary fault fold over both legs, the wire-converter and redaction projections, and the egress/template/transcode surface with its typed fact stream.

## [02]-[TABULAR_SOURCE]

- Owner: `TabularFormat` the `[SmartEnum<string>]` format axis carrying its `ExcelType`, its policy factory, and the frozen set of transcode targets it reaches; `TabularSpec` the `[ComplexValueObject]` read descriptor fixing format + source + sheet + header stance + window in one value so every read modality discriminates on it; `TabularSource.Origin` the closed `[Union]` source family (`FromPath | FromStream`) absorbing the path-vs-stream boundary modality the codec exposes as parallel overloads; `RowWindow` the cell-range window value; `RedactionPlan` the per-column `IRedactorProvider`-plus-`DataClassificationSet`-map a classified export carries; `TabularFault` the closed row-boundary fault band deriving from the KERNEL `Rasm.Domain.Expected` (the federation base) folding all three MiniExcel exceptions; `TabularFact` the typed receipt-stream record; `TabularSource` the static surface owning the spec-dispatched lazy typed/dynamic/reader/probe ingress, the row-boundary fault fold, the wire-converter + redaction projections, and the egress/template/transcode.
- Cases: `TabularFormat` is `Xlsx` (`ExcelType.XLSX`, an `OpenXmlConfiguration` with the shared-string disk cache on, transcode target `{Csv}`) or `Csv` (`ExcelType.CSV`, a `CsvConfiguration`, transcode target `{Xlsx}`); `Origin` is `FromPath(string)` (opens a shared-read `FileStream` via the path overload) or `FromStream(Func<Stream>)` (a caller-owned stream the `this Stream` overload reads); `RowWindow` carries the `A1:C3` start/end cells the `dynamic` `QueryRange` reads — a typed `Query<T>` reaches only `StartCell`, so a windowed typed read lowers through the `dynamic` `QueryRange` projected to `T`, never a non-existent typed `endCell` overload; `TabularFault` is `ColumnMissing | CellCast | NotSerializable` (the three lifted MiniExcel exceptions) plus `TranscodeUnreachable` (the routing-only rejection a `(from,to)` miss rails, never the `CellCast` cell-coercion arm); `TabularFact` kinds are `read | write | append | template | transcode | redact`.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` reads typed rows — the header-keyed `dynamic` `Query` from `StartCell`, or the `dynamic` `QueryRange` when the spec carries a `RowWindow`, BOTH projected to `T` through the ONE `Project<T>`→`TabularWire.Bind<T>` STJ wire factory (`T` is unconstrained — the page never calls the `where T : class, new()` `MiniExcel.Query<T>` POCO binder, which would both refuse an unconstrained `T` and bypass the NodaTime/Thinktecture cell minting) — folding the row-boundary faults into one `Validation` and riding a `read` fact; `public static IO<Validation<TabularFault, Seq<HashMap<string,object>>>> Scan(TabularSpec spec, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` reads header-keyed `dynamic` rows for the schema-unknown ingress; `public static IO<MiniExcelDataReader> Reader(TabularSpec spec)` projects the streaming reader the `Query/columnar` materializer consumes; `public static IO<TabularSheet> Probe(TabularSpec spec)` reads the sheet roster, per-sheet `SheetInfo`, used-range `ExcelRange`, and first-row column keys without reading the body; `public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, bool header, Option<RedactionPlan> plan, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` writes any `IEnumerable`/`IDataReader`/`DataTable`/anonymous value, optionally re-emitting each row through the `RedactionPlan` per-column redactor map (a typed POCO lowered to its bag first so every write shape redacts), riding a `write` (or `redact`) fact; `public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, bool header, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` appends or overwrites a sheet in an existing workbook through `Insert`; `public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` renders an `.xlsx` template through `SaveAsByTemplate`; `public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink)` is the direct transcode discriminating on the SAME `TabularSpec` value (its `Format` IS the source format, its `Source` the origin — no redundant `from` knob), the `(spec.Format, to)` pair resolved against `spec.Format.Transcodes` so a no-op or unreachable pair rails the typed `TabularFault.TranscodeUnreachable` rather than a silent misroute or a mis-cased cell fault.
- Auto: a typed read folds the header-keyed `dynamic` `Query` (or `QueryRange` for a window) rows through `Project<T>`→`TabularWire.Bind<T>` — the ONE STJ wire factory round-trip (`api-nodatime-stj`/`api-thinktecture-json`) where an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the SAME converter set a JSON ingress uses and a column binds by the POCO's `[JsonPropertyName]` alias — so MiniExcel delivers only the header-keyed cell bag and the wire rail mints the semantic type, while `DateOnlyConversionMode.RequireMidnight` governs the boundary `DateOnly` admission; the windowed read lowers through `QueryRange` (the only public windowing surface, `dynamic`-yielding) projecting each `IDictionary<string,object>` row through the SAME `Bind<T>`, so `RowWindow.EndCell` is honoured rather than discarded and the windowed and non-windowed typed paths share one projection; the `DynamicExcelColumn.CustomFormatter` (`Func<object,object>`) that `TabularWire.Wire` builds is the cell projection on the `dynamic`/reader/write legs (where MiniExcel's `DynamicColumns` honour it), NOT the `Query<T>` POCO binder (which honours neither); a large read streams through `Query`/`GetReader` (lazy `yield`) with `EnableSharedStringCache` on so the shared-strings table spills to `SharedStringCachePath` and a million-row workbook never fully resides in memory; the egress reuses the reader/lane rail the query produced (`SaveAs(stream, dataReader)`) so a schedule/cost/catalog export never re-materializes a table, report-shaped output uses `SaveAsByTemplate` with placeholder binding, a multi-sheet workbook lands sheet-by-sheet through `Insert`, and `Reader` yields a `MiniExcelDataReader : IDataReader` the columnar materializer pulls; the redacted egress wraps the row enumerable in a projection that resolves a `Redactor` PER COLUMN from the `RedactionPlan`'s `DataClassificationSet` map through the AppHost `IRedactorProvider.GetRedactor` and rewrites each classified cell through the generic `Redactor.Redact<object>(value)` typed-value seam before `SaveAs` — a typed POCO lowered to its header-keyed bag first so every write shape redacts, the same span-based redactor the telemetry exporters use, so no unredacted value is `ToString`-stringified into transit first (the prior uniform-`Redactor`-plus-`value.ToString()` shape that left a typed POCO unredacted is the deleted hollow form).
- Receipt: every fact-bearing operation rides a `TabularFact` through the `ReceiptSinkPort` envelope under `store.tabular.*` — a `read` fact carrying the row count, sheet, and format; a `write`/`append` fact carrying the per-sheet row counts; a `template` fact; a `transcode` fact carrying the resolved `(from,to)`; a `redact` fact carrying the redacted-column count — one fact stream with a kind discriminant, never parallel receipt records; `Reader` alone emits no fact (it hands the raw `MiniExcelDataReader` to the `Query/columnar` materializer, which owns its own receipt over the bytes it pulls).
- Packages: MiniExcel (`MiniExcel.Query`/`QueryRange`/`GetReader`/`GetSheetNames`/`GetSheetInformations`/`GetColumns`/`GetSheetDimensions`/`SaveAs`/`Insert`/`SaveAsByTemplate`/`ConvertCsvToXlsx`/`ConvertXlsxToCsv`/`MiniExcelDataReader`/`OpenXmlConfiguration`/`CsvConfiguration`/`DateOnlyConversionMode`/`DynamicExcelColumn`/`SheetInfo`/`ExcelRange` — the typed read rides the `dynamic` `Query`/`QueryRange` header-keyed bag, never the `where T : class, new()` `Query<T>` POCO binder), Microsoft.Extensions.Compliance.Redaction (`Redactor`/`IRedactorProvider`/`DataClassificationSet`), Sep (delimited sibling), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new tabular format is one `TabularFormat` row carrying its `ExcelType`, policy, and transcode set (a new transcode target widens the row's `Transcodes` set, never a new entrypoint); a new column mapping is one `ExcelColumnAttribute`/`DynamicExcelColumn`; a new wire-cell projection is one `CustomFormatter` registration; a new source kind is one `Origin` case; a new boundary-fault class is one `TabularFault` case; zero new surface — a hand-rolled OpenXML/CSV parser, a second spreadsheet codec, a `QueryAsDataTable` whole-sheet buffer on a streaming path, an `if`-ladder transcode, a parallel `ReadTyped`/`ReadDynamic`/`ReadStream` family beside the spec discriminant, or a tabular→element map inside this codec is the deleted form because MiniExcel owns the spreadsheet/CSV codec, `Sep` the high-throughput delimited lane, the `TabularSpec` value the modality discriminant, and the app composition root the tabular→element projection.
- Boundary: `TabularSource` is the ONE tabular ingress/egress owner over MiniExcel — `Sep` (`api-sep`) is the high-throughput delimited-text sibling and the two project into the SAME downstream record rail, the spec format selecting one by source format, while `Apache.Arrow`/`DuckDB`/`ParquetSharp` own the binary columnar path (a `.xlsx` is never treated as a columnar file); the path-vs-stream modality the codec exposes as twin overloads is the `Origin` `[Union]`, so a read does not split into a path family and a stream family — one spec carries the source and one dispatch opens it; the typed read NEVER rides the `where T : class, new()` `MiniExcel.Query<T>` POCO binder — that overload both refuses an unconstrained `T` and bypasses the NodaTime/Thinktecture cell minting (so an `Instant`/value-object/smart-enum column would silently fail to bind, the deleted hollow form) — so BOTH the start-cell typed read and the windowed typed read fold the header-keyed `dynamic` `Query`/`QueryRange` rows through the ONE `Project<T>`→`TabularWire.Bind<T>` STJ wire factory, the window adding only the `endCell` argument `Query<T>` could not express; the codec NEVER knows the element graph — the per-app tabular→element map is the wire-composition owner (owned per-app at the host/app composition root, `§6`), so a workbook of element rows reads through `Read<T>`/`Reader`, the app projects each row into an `ElementGraph` node, and a catalog/cost/schedule egress writes element-derived tables back through `Write`/`Append`/`Render`, the codec seeing only the row shape; a typed read against a missing header surfaces `ExcelColumnNotFoundException`, a bad cell `ExcelInvalidCastException`, and a non-serializable write member `MiniExcelNotSerializableException`, all folded through the one `TabularFault.Lift` funnel into a typed `Validation` at the row boundary on BOTH the read and the write leg rather than thrown through the receipt path — the write leg folds faults exactly as the read leg does, so `NotSerializable` is a reachable case, not decoration; the bulk-copy rail (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a `Read<T>` yields (`BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced), while the `MiniExcelDataReader : IDataReader` from `Reader` feeds the `Query/columnar` Arrow/DuckDB materializer — the reader is the columnar source, the typed enumerable the bulk source, never conflated; a redacted export resolves the per-column `Redactor` from the field's `DataClassificationSet` through the AppHost `IRedactorProvider` and rewrites each classified cell before `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table (MiniExcel exposes no reader→writer copy bridge, so the redaction rides the row-enumerable egress, not a `DbDataReader` decorator); MiniExcel retires `Sylvan.Data.Excel` as the spreadsheet codec — no `Sylvan` reference remains.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// The fault band derives from the KERNEL Expected (the federation base), aliased so the bare `Expected`
// names `Rasm.Domain.Expected` and never the `LanguageExt.Common.Expected` whose `(string,int,Option)`
// ctor is the deleted form.
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

    public IConfiguration Policy() => Excel == ExcelType.XLSX
        ? new OpenXmlConfiguration {
            EnableSharedStringCache = true, TrimColumnNames = true, IgnoreEmptyRows = true,
            DateOnlyConversionMode = DateOnlyConversionMode.RequireMidnight }
        : new CsvConfiguration { ReadEmptyStringAsNull = true };

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

    static Validation<ValidationError, TabularSpec> Validate(
        TabularFormat format, Origin source, Option<string> sheet, bool headerRow, Option<RowWindow> window) =>
        window.Match(
            Some: w => string.IsNullOrWhiteSpace(w.StartCell)
                ? Validation<ValidationError, TabularSpec>.Fail(ValidationError.Create("<tabular-spec-window-start>"))
                : Validation<ValidationError, TabularSpec>.Success(new TabularSpec(format, source, sheet, headerRow, window)),
            None: () => Validation<ValidationError, TabularSpec>.Success(new TabularSpec(format, source, sheet, headerRow, window)));

    public string StartCell => Window.Match(Some: static w => w.StartCell, None: static () => "A1");
}

// The row-boundary fault band (837x): a closed [Union] over the KERNEL `Rasm.Domain.Expected`
// (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation
// base the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`. The `base(detail, code, None)` ctor belongs to that
// OTHER `Expected` (which carries no `Category` to override) and is the deleted form; band membership is a per-case
// `Code => 837x` override, `Message => Detail` projects the case detail, and `Category` projects the telemetry label
// through the generated `Switch`, so a recovery reads `error.IsType<TabularFault.CellCast>()` / `error.HasCode(8371)`
// / `error.Category()`, never a message substring. `[SkipUnionOps]` is the canonical fault-band annotation (the
// production `UiFault` shape) — it skips the generated implicit-conversion ops while the generated `Switch`/`Map`
// survives, and the `Expected` derivation makes a bare case an `Error` directly so it lifts onto `Fin<T>`/`Validation`
// with no `.ToError()` hop and an `IValidationError` accumulation stays recoverable through `Error.Combine`.
[SkipUnionOps]
[Union]
public abstract partial record TabularFault : Expected, IValidationError<TabularFault> {
    private TabularFault() : base() { }
    public sealed record ColumnMissing(string Column, int Row) : TabularFault;
    public sealed record CellCast(string Column, int Row, string Target) : TabularFault;
    public sealed record NotSerializable(string Member) : TabularFault;
    // The transcode-routing rejection: an unreachable `(from,to)` pair (a no-op `.csv->.csv` or an unsupported target)
    // is its OWN case, never the `CellCast` cell-coercion arm — the codec misroute is a routing fault, not a cell fault.
    public sealed record TranscodeUnreachable(string From, string To) : TabularFault;

    public override int Code => Switch(
        columnMissing:        static _ => 8370,
        cellCast:             static _ => 8371,
        notSerializable:      static _ => 8372,
        transcodeUnreachable: static _ => 8373);

    public override string Message => Switch(
        columnMissing:        static c => $"<tabular-column-missing:{c.Column}@{c.Row}>",
        cellCast:             static c => $"<tabular-cell-cast:{c.Column}@{c.Row}:{c.Target}>",
        notSerializable:      static c => $"<tabular-not-serializable:{c.Member}>",
        transcodeUnreachable: static c => $"<tabular-transcode-unreachable:{c.From}->{c.To}>");

    public override string Category => Switch(
        columnMissing:        static _ => "Column",
        cellCast:             static _ => "Cell",
        notSerializable:      static _ => "Serialize",
        transcodeUnreachable: static _ => "Transcode");

    // The IValidationError<TabularFault>.Create admission the generated converter bridge calls on a deserialization
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

// The redacted-egress plan: the AppHost `IRedactorProvider` plus the per-column `DataClassificationSet` map a
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
    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Window.IsSome
            ? spec.Source.Read(
                path:   p => Project<T>(MiniExcel.QueryRange(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Window.ValueUnsafe().EndCell, spec.Format.Policy())),
                stream: s => Project<T>(s.QueryRange(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Window.ValueUnsafe().EndCell, spec.Format.Policy())))
            : spec.Source.Read(
                path:   p => Project<T>(MiniExcel.Query(p, useHeaderRow: true, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy()).Cast<IDictionary<string, object>>()),
                stream: s => Project<T>(s.Query(useHeaderRow: true, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy()).Cast<IDictionary<string, object>>()))))
        from _ in rows.Match(
            Succ: r => sink(new TabularFact("read", spec.Format.Key, spec.Sheet, r.Count, clocks.Now)),
            Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Seq<HashMap<string, object>>>> Scan(TabularSpec spec, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => toSeq(MiniExcel.Query(p, useHeaderRow: true, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy()).Select(Bag)),
            stream: s => toSeq(s.Query(useHeaderRow: true, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy()).Select(Bag)))))
        from _ in rows.Match(Succ: r => sink(new TabularFact("read", spec.Format.Key, spec.Sheet, r.Count, clocks.Now)), Fail: _ => IO.pure(unit))
        select rows;

    public static IO<MiniExcelDataReader> Reader(TabularSpec spec) =>
        IO.lift(() => spec.Source.Read(
            path:   p => MiniExcel.GetReader(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy()),
            stream: s => s.GetReader(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy())));

    public static IO<TabularSheet> Probe(TabularSpec spec) =>
        IO.lift(() => spec.Source.Read(
            path: p => new TabularSheet(
                toSeq(MiniExcel.GetSheetInformations(p)),
                toSeq(MiniExcel.GetColumns(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy())),
                Optional(MiniExcel.GetSheetDimensions(p).FirstOrDefault())),
            stream: s => new TabularSheet(
                toSeq(s.GetSheetInformations()),
                toSeq(s.GetColumns(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Format.Policy())),
                Optional(s.GetSheetDimensions().FirstOrDefault()))));

    public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, bool header, Option<RedactionPlan> plan, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) {
        var (payload, kind) = plan.Match(Some: p => (Redact(value, p), "redact"), None: () => (value, "write"));
        return from counts in IO.lift(() => Capture(() => spec.Source.Read(
                   path:   p => MiniExcel.SaveAs(p, payload, header, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Format.Policy(), overwriteFile: true),
                   stream: s => s.SaveAs(payload, header, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Format.Policy()))))
               from _ in counts.Match(Succ: c => sink(new TabularFact(kind, spec.Format.Key, spec.Sheet, c.Sum(), clocks.Now)), Fail: _ => IO.pure(unit))
               select counts;
    }

    public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, bool header, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => MiniExcel.Insert(p, value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Format.Policy(), header, overwriteSheet: false),
            stream: s => s.Insert(value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Format.Policy(), header, overwriteSheet: false))))
        from _ in rows.Match(Succ: r => sink(new TabularFact("append", spec.Format.Key, spec.Sheet, r, clocks.Now)), Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => { MiniExcel.SaveAsByTemplate(p, template.ToArray(), value); return unit; },
            stream: s => { s.SaveAsByTemplate(template.ToArray(), value); return unit; })))
        from _ in done.Match(Succ: _ => sink(new TabularFact("template", spec.Format.Key, spec.Sheet, 0, clocks.Now)), Fail: _ => IO.pure(unit))
        select done;

    // The `from` format is the spec's OWN `Format` (the source carries it), so transcode discriminates on the SAME
    // `TabularSpec` value every other modality does — `to` is the only added axis. A redundant `from` parameter beside
    // `spec.Format` is the knob the spec discriminant deletes.
    public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ClockPolicy clocks, Func<TabularFact, IO<Unit>> sink) =>
        !spec.Format.CanReach(to)
            ? IO.pure(Validation<TabularFault, Unit>.Fail(new TabularFault.TranscodeUnreachable(spec.Format.Key, to.Key)))
            : from done in IO.lift(() => Capture(() => spec.Source.Read(
                  path:   p => { Convert(p, target, spec.Format, to); return unit; },
                  stream: s => { Convert(s, target, spec.Format, to); return unit; })))
              from _ in done.Match(Succ: _ => sink(new TabularFact("transcode", $"{spec.Format.Key}->{to.Key}", None, 0, clocks.Now)), Fail: _ => IO.pure(unit))
              select done;

    static Validation<TabularFault, TValue> Capture<TValue>(Func<TValue> codec) =>
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

    // The runtime column form for the `dynamic`/reader/write legs (where MiniExcel's `DynamicColumns` honour a
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

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one tabular owner   | `TabularSource` over MiniExcel         | `Sep` is the delimited sibling; both feed one record rail  |
|  [02]   | modality discriminant | `TabularSpec` + `Origin` `[Union]`   | one value selects typed/dynamic/reader/probe; path or stream is a case |
|  [03]   | typed read          | header-keyed `dynamic` `Query` → STJ `Bind<T>` | unconstrained `T`; never the `class,new()` `Query<T>` POCO binder that bypasses the wire factory |
|  [04]   | windowed typed read | `dynamic` `QueryRange` → the SAME `Bind<T>` | `QueryRange` adds only `endCell`; windowed + non-windowed share one projection |
|  [05]   | wire-cell projection| `DynamicExcelColumn.CustomFormatter`   | `Instant`/`LocalDate`/value-object minted by the wire rail |
|  [06]   | row-boundary fault  | `Validation<TabularFault, …>` on both legs | read AND write fold; `NotSerializable` is reachable   |
|  [07]   | bulk vs columnar    | typed `IEnumerable<T>` vs `IDataReader`| bulk-copy is `IEnumerable<T>`-sourced; the reader feeds the columnar materializer |
|  [08]   | redacted egress     | per-column `Redactor` before `SaveAs`  | streams column-redacted; no `DbDataReader` decorator      |
|  [09]   | transcode           | frozen `(from,to)` table lookup        | a no-op or unreachable pair is a typed fault, never a misroute |
|  [10]   | receipt             | one `TabularFact` stream under `store.tabular.*` | kind-discriminated; never parallel receipt records |
|  [11]   | element projection  | per-app tabular→element map            | the codec sees only the row shape; the app projects nodes  |
