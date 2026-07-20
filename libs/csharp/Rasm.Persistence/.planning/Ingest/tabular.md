# [PERSISTENCE_INGEST_TABULAR]

Rasm.Persistence ingests and emits spreadsheet and delimited tabular data through ONE `TabularSource` owner over the MiniExcel zero-template streaming codec. A `TabularSpec` fixes a read once — its format, source (a path or a caller-owned stream), sheet, header stance, and an `Option<RowWindow>` sub-rectangle — and the owner discriminates every modality on that one value rather than on a method-name family: a typed POCO read, a `dynamic` read, a streaming `IDataReader`, a sheet/column probe, and the egress (write, sheet-append, template render, adorn, transcode) are all dispatches over `TabularSpec`, never parallel `ReadTyped`/`ReadDynamic`/`ReadStream` entrypoints. This codec NEVER knows the element graph — the per-app tabular→element map (the wire-composition owner, owned per-app at the host/app composition root) projects each delivered row into a `Rasm.Element` graph node per the `ARCHITECTURE.md` `[02]-[SEAMS]` `Ingest → Rasm.Element` `ElementGraph` wire row, so `TabularSource` delivers only the row shape at the wire and a workbook of element rows is to it an anonymous record stream.

One `TabularFormat` `[SmartEnum<string>]` crosses the format axis (`.xlsx`/`.csv`): each row carries its `ExcelType` and `[UseDelegateFromConstructor]` `Policy(Option<OpenXmlStyleOptions>, Option<TabularWorkbook>)` behavior. `TabularWorkbook` composes freeze panes, auto-filter, table style, auto-width bounds, and `DynamicSheets` as one validated policy value; CSV rejects this OpenXML-only posture. `TabularTranscode` owns the frozen `(from, to)` codec table whose membership derives `CanReach`. Both boundaries fold MiniExcel exceptions through `TabularFault.Lift`, and `Semigroup<TabularFault>` accumulates independent row faults. MiniExcel owns spreadsheets and symmetric CSV transcode; `DelimitedSource` owns high-throughput `Sep` rows; both project into one downstream record rail.

A typed read folds each header-keyed `dynamic` row through the ONE STJ wire projection `TabularWire.Bind<T>` under `TabularWire.Options` — a composed `JsonSerializerOptions` carrying `ThinktectureJsonConverterFactory` plus `ConfigureForNodaTime` (`api-thinktecture-json`/`api-nodatime-stj`), the SAME converter families the `Element/codec#CODEC_AXIS` `ElementJson.Options` mounts but over an OPEN resolver, because a `JsonSerializerContext` resolver resolves only registered types and a consumer POCO is not one — so an `Instant`/`LocalDate`/value-object/smart-enum cell mints through the same converter set a JSON ingress uses and a column binds by the POCO's `[JsonPropertyName]` alias. NOT `MiniExcel.Query<T>` (the `where T : class, new()` reflective POCO binder neither invokes a `DynamicExcelColumn.CustomFormatter` nor mints a semantic type — the bypass-the-wire-factory hollow form), and never positional magic-index `dynamic` access on a known schema. Typed ingress is CONTRACT-CHECKED: every settable wire member of `T` (resolved through `Options.GetTypeInfo(typeof(T)).Properties`, honouring aliases) must resolve a column on the first row, each missing column accumulating a typed `ColumnMissing` — a silently-defaulted member from a missing header is the deleted hollow form. `DynamicExcelColumn.CustomFormatter` is the cell projection on the `dynamic`/reader/write legs (where MiniExcel's own `DynamicColumns` honour it), and the spec's `Policy()` COMPOSES the `TabularWire.Wire`-built columns onto `Configuration.DynamicColumns`, so the wire column is a REGISTERED member, never a built-but-never-composed phantom. A redacted egress re-emits each row through a `RedactionPlan` that resolves a `Microsoft.Extensions.Compliance.Redaction` `Redactor` PER COLUMN from the field's `DataClassificationSet` (`api-redaction`) and redacts each classified cell through the generic typed-value `Redact<T>` — never a `value.ToString()`-then-redact hop — into `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table. Streaming `MiniExcelDataReader : IDataReader` feeds the `Query/columnar` Arrow/DuckDB materializer directly; the linq2db/EF bulk-copy lane (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a typed read yields, never the reader, because `BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced — the fenced `TabularBulk.Copy<T>` surface passes the V6 identity `DbContext` directly to `LinqToDBForEFTools.BulkCopyAsync`, so the bridge consumes the context's EF metadata while `BulkCopyType.ProviderSpecific` lowers to PG binary COPY with `KeepIdentity` pinned. DuckDB `COPY` owns columnar FILES, the Npgsql binary importer owns raw-wire streams, and linq2db owns the EF-MODEL-MAPPED lane. Redaction inputs `IRedactorProvider`/`DataClassificationSet` and `ReceiptSinkPort` belong to the composition root; wall clock and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame (a `ClockPolicy` parameter on any signature here is the named strata inversion, no AppHost type crosses down).

## [01]-[INDEX]

- [01]-[TABULAR_SOURCE]: the format vocabulary with its policy behavior column, the frozen transcode table, the `TabularSpec` source discriminant (wire columns and report style as policy DATA), the spec-dispatched typed/dynamic/reader/probe ingress with the header-contract check and per-row fault accumulation, the row-boundary fault fold over both legs, the wire-converter and redaction projections, and the egress/template/adorn/transcode surface with its typed fact stream.
- [02]-[DELIMITED_SOURCE]: the `Sep` SIMD delimited owner — the `DelimitedSpec` parse-policy value hardening reader and writer as one strict pair, the `Enumerate`/`EnumerateAsync`/`ParallelEnumerate` record-rail materialization, and the reader→writer redaction/projection bridge.
- [03]-[BULK_LANE]: the linq2db EF-model-mapped bulk-copy fence over the V6 identity `DbContext`, the `BulkCopyRowsCopied` typed receipt, and the three-row bulk-ingress boundary law.

## [02]-[TABULAR_SOURCE]

- Owner: `TabularFormat` carries `ExcelType` and the `[UseDelegateFromConstructor]` configuration column; `TabularWorkbook` owns OpenXML workbook policy; `TabularTranscode` owns path and stream codec correspondences; `TabularSpec` fixes format, source, sheet, header, window, wire columns, style, and workbook policy; `Origin` closes path/stream ingress; `RowWindow` factory-gates cell ranges; `RedactionPlan` owns per-column classification; `TabularAdornment` closes report finishing; `TabularFault` closes the accumulating boundary band; `TabularFactKind` closes receipts; `TabularSource` owns dispatch.
- Cases: `TabularFormat` is `Xlsx` (`ExcelType.XLSX`, an `OpenXmlConfiguration` with the shared-string disk cache on) or `Csv` (`ExcelType.CSV`, a `CsvConfiguration`); `TabularTranscode.Table` holds `(Csv, Xlsx)` and `(Xlsx, Csv)`, so a no-op or unsupported pair is a key miss railing the typed fault; `Origin` is `FromPath(string)` (opens a shared-read `FileStream` via the path overload) or `FromStream(Func<Stream>)` (a caller-owned stream the `this Stream` overload reads); `RowWindow` carries the `A1:C3` start/end cells the `dynamic` `QueryRange` reads — a typed `Query<T>` reaches only `startCell`, so a windowed typed read lowers through the `dynamic` `QueryRange` projected to `T`, never a non-existent typed `endCell` overload; `TabularFault` is `ColumnMissing | CellCast | NotSerializable` (the three lifted MiniExcel exceptions, `ColumnMissing` ALSO minted by the typed-read header contract) plus `TranscodeUnreachable` (the routing-only rejection), `BulkRefused` (the `#BULK_LANE` copy refusal), `CodecReject` (the residual boundary throw — never `ColumnMissing` wearing a message), and `Aggregate` (the `Semigroup` fold of independent row faults); `TabularAdornment` is `MergeCells(target)` (`MergeSameCells` vertical fold) or `Pictures(images)` (`AddPicture` embeds at cell addresses); `TabularFact` kinds are `read | write | append | template | adorn | transcode | redact | bulk`.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` reads typed rows — the header-keyed `dynamic` `Query` from `StartCell`, or the `dynamic` `QueryRange` when the spec carries a `RowWindow`, BOTH projected to `T` through the ONE `TabularWire.Project<T>` rail: the header contract accumulates a `ColumnMissing` per unresolvable wire member, then every row mints through `Bind<T>` with per-row faults accumulated applicatively, riding a `read` fact; `public static IO<Validation<TabularFault, Seq<HashMap<string,object>>>> Scan(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` reads header-keyed `dynamic` rows for the schema-unknown ingress; `public static IO<MiniExcelDataReader> Reader(TabularSpec spec)` projects the streaming reader the `Query/columnar` materializer consumes; `public static IO<TabularSheet> Probe(TabularSpec spec)` reads the sheet roster, per-sheet `SheetInfo`, used-range `ExcelRange`, and first-row column keys without reading the body; `public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, Option<RedactionPlan> plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` writes any `IEnumerable`/`IDataReader`/`DataTable`/anonymous value under the spec's own `HeaderRow` stance (a `bool header` knob beside the spec re-describes a value it already fixes — deleted), optionally re-emitting each row through the `RedactionPlan` per-column redactor map (a typed POCO, `DataTable`, or `IDataReader` lowered to its header-keyed bag first so EVERY write shape redacts — the redact fold dispatches on the payload shape, never on an enumerable-only fast path), riding a `write` (or `redact`) fact; `public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` appends rows to a sheet in an existing workbook through `Insert` (`overwriteSheet` pinned false — replacing a sheet is a `Write` of the workbook, never an in-place overwrite); `public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` renders an `.xlsx` template through `SaveAsByTemplate`; `public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` resolves `(spec.Format, to)` against `TabularTranscode.Table` (the spec's `Format` IS the source format, its `Source` the origin — no redundant `from` knob) and invokes the row's path or stream arm, a key miss railing `TranscodeUnreachable`; `public static IO<Validation<TabularFault, Unit>> Adorn(TabularSpec spec, TabularAdornment adornment, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` is the report-egress finisher — `MergeCells` folds vertically-repeated values through `MergeSameCells`, `Pictures` embeds `MiniExcelPicture` payloads through `AddPicture` — one polymorphic dispatch riding an `adorn` fact, never sibling `Merge`/`Illustrate` entrypoints.
- Auto: a typed read folds the header-keyed `dynamic` `Query` (or `QueryRange` for a window) rows through `TabularWire.Project<T>` — header contract first (each settable wire member of `T` resolved against the first row's keys, every miss a typed `ColumnMissing`, accumulated), then each row through `Bind<T>` under the one wire options graph, a `JsonException` lifting to the typed `CellCast` carrying the row ordinal, and row faults accumulating across the sheet through the applicative `Traverse` over the `Semigroup` — so MiniExcel delivers only the header-keyed cell bag and the wire rail mints the semantic type, while `DateOnlyConversionMode.RequireMidnight` governs the boundary `DateOnly` admission; the windowed read lowers through `QueryRange` (the only public windowing surface, `dynamic`-yielding) projecting each `IDictionary<string,object>` row through the SAME `Project<T>`, so `RowWindow.EndCell` is honoured and the windowed and non-windowed typed paths share one projection; the `DynamicExcelColumn.CustomFormatter` (`Func<object,object>`) that `TabularWire.Wire` builds is the cell projection on the `dynamic`/reader/write legs (where MiniExcel's `DynamicColumns` honour it), and `spec.Policy()` REGISTERS the built columns onto `Configuration.DynamicColumns` for every `WireColumns` name, so the projection actually fires on those legs; a large read streams through `Query`/`GetReader` (lazy `yield`) with `EnableSharedStringCache` on so the shared-strings table spills to `SharedStringCachePath` and a million-row workbook never fully resides in memory; the egress reuses the reader/lane rail the query produced (`SaveAs(stream, dataReader)`) so a schedule/cost/catalog export never re-materializes a table, report-shaped output uses `SaveAsByTemplate` with placeholder binding plus the `Adorn` finisher (`MergeSameCells`/`AddPicture`), header/cell styling rides the spec's `OpenXmlStyleOptions` policy value (`OpenXmlHeaderStyle`/`HorizontalCellAlignment` — never per-cell writes), a formula column is the declarative `ColumnType.Formula` attribute row and a runtime multi-sheet binding the `OpenXmlConfiguration.DynamicSheets` row, a multi-sheet workbook lands sheet-by-sheet through `Insert`, and `Reader` yields a `MiniExcelDataReader : IDataReader` the columnar materializer pulls; the redacted egress wraps the row enumerable in a projection that resolves a `Redactor` PER COLUMN from the `RedactionPlan`'s `DataClassificationSet` map through the AppHost `IRedactorProvider.GetRedactor` and rewrites each classified cell through the generic `Redactor.Redact<object>(value)` typed-value seam before `SaveAs` — a typed POCO, `DataTable`, or `IDataReader` lowered to its header-keyed bag first so every write shape redacts, the same span-based redactor the telemetry exporters use, so no unredacted value is `ToString`-stringified into transit first.
- Receipt: every fact-bearing operation rides a `TabularFact` through the `ReceiptSinkPort` envelope under `store.tabular.*` — a `read` fact carrying the row count, sheet, and format; a `write`/`append` fact carrying the per-sheet row counts; a `template` fact; a `transcode` fact carrying the resolved `(from,to)`; a `redact` fact carrying the redacted-row count — one fact stream with a kind discriminant, never parallel receipt records; `Reader` alone emits no fact (it hands the raw `MiniExcelDataReader` to the `Query/columnar` materializer, which owns its own receipt over the bytes it pulls).
- Packages: MiniExcel (`MiniExcel.Query`/`QueryRange`/`GetReader`/`GetSheetNames`/`GetSheetInformations`/`GetColumns`/`GetSheetDimensions`/`SaveAs`/`Insert`/`SaveAsByTemplate`/`MergeSameCells`/`AddPicture`/`ConvertCsvToXlsx`/`ConvertXlsxToCsv`/`MiniExcelDataReader`/`OpenXmlConfiguration`/`CsvConfiguration`/`OpenXmlStyleOptions`/`OpenXmlHeaderStyle`/`MiniExcelPicture`/`ColumnType`/`DateOnlyConversionMode`/`DynamicExcelColumn`/`SheetInfo`/`ExcelRange` — the typed read rides the `dynamic` `Query`/`QueryRange` header-keyed bag, never the `where T : class, new()` `Query<T>` POCO binder), Microsoft.Extensions.Compliance.Redaction (`Redactor`/`IRedactorProvider`/`DataClassificationSet`), Sep (the `#DELIMITED_SOURCE` owner), linq2db.EntityFrameworkCore (the `#BULK_LANE` bridge), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`), LanguageExt.Core, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: a new tabular format is one `TabularFormat` row carrying its `ExcelType` and policy delegate plus its `TabularTranscode.Table` rows (a new transcode target is one table row, never a new entrypoint); a new column mapping is one `ExcelColumnAttribute`/`DynamicExcelColumn`; a new wire-cell projection is one `WireColumns` name the spec's `Policy()` registers; a new report decoration is one `TabularAdornment` case; a new style stance is one `OpenXmlStyleOptions` policy value on the spec; a new source kind is one `Origin` case; a new boundary-fault class is one `TabularFault` case inside the registry decade; zero new surface — a hand-rolled OpenXML/CSV parser, a second spreadsheet codec, a `QueryAsDataTable` whole-sheet buffer on a streaming path, a format-key conditional outside the vocabulary, a parallel `ReadTyped`/`ReadDynamic`/`ReadStream` family beside the spec discriminant, or a tabular→element map inside this codec is the deleted form because MiniExcel owns the spreadsheet/CSV codec, `Sep` the high-throughput delimited lane, the `TabularSpec` value the modality discriminant, and the app composition root the tabular→element projection.
- Boundary: `TabularSource` is the ONE tabular ingress/egress owner over MiniExcel — the `#DELIMITED_SOURCE` `Sep` owner is the high-throughput delimited-text sibling and the two project into the SAME downstream record rail, the source format selecting one, while `Apache.Arrow`/`DuckDB`/`ParquetSharp` own the binary columnar path (a `.xlsx` is never treated as a columnar file); the path-vs-stream modality the codec exposes as twin overloads is the `Origin` `[Union]`, so a read does not split into a path family and a stream family — one spec carries the source and one dispatch opens it; the typed read NEVER rides the `where T : class, new()` `MiniExcel.Query<T>` POCO binder — that overload both refuses an unconstrained `T` and bypasses the NodaTime/Thinktecture cell minting (an `Instant`/value-object/smart-enum column silently fails to bind, the deleted hollow form) — so BOTH the start-cell typed read and the windowed typed read fold the header-keyed `dynamic` `Query`/`QueryRange` rows through the ONE `TabularWire.Project<T>` rail, the window adding only the `endCell` argument `Query<T>` cannot express; the codec NEVER knows the element graph — the per-app tabular→element map is the wire-composition owner at the host/app composition root, so a workbook of element rows reads through `Read<T>`/`Reader`, the app projects each row into an `ElementGraph` node, and a catalog/cost/schedule egress writes element-derived tables back through `Write`/`Append`/`Render`, the codec seeing only the row shape; a typed read against a missing header surfaces `ExcelColumnNotFoundException` (or the header-contract `ColumnMissing`), a bad cell `ExcelInvalidCastException`, and a non-serializable write member `MiniExcelNotSerializableException`, all folded through the one `TabularFault.Lift` funnel into a typed `Validation` at the row boundary on BOTH the read and the write leg rather than thrown through the receipt path — the write leg folds faults exactly as the read leg does, so `NotSerializable` is a reachable case, not decoration; the bulk-copy rail (`api-linq2db-ef`) consumes the typed `IEnumerable<T>`/`IAsyncEnumerable<T>` a `Read<T>` yields (`BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced), while the `MiniExcelDataReader : IDataReader` from `Reader` feeds the `Query/columnar` Arrow/DuckDB materializer — the reader is the columnar source, the typed enumerable the bulk source, never conflated; a redacted export resolves the per-column `Redactor` from the field's `DataClassificationSet` through the AppHost `IRedactorProvider` and rewrites each classified cell before `SaveAs`, so a redacted spreadsheet streams column-redacted without materializing the table (MiniExcel exposes no reader→writer copy bridge, so the redaction rides the row-enumerable egress, not a `DbDataReader` decorator — the DELIMITED redaction egress owns the real bridge: `#DELIMITED_SOURCE` re-emits Sep rows column-by-column through the writer `NewRow` with the classified cells redacted before `Col.Set`); MiniExcel retires `Sylvan.Data.Excel` as the spreadsheet codec — no `Sylvan` reference remains.

```csharp signature
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TabularFormat {
    public static readonly TabularFormat Xlsx = new("xlsx", ExcelType.XLSX, BuildXlsx);
    public static readonly TabularFormat Csv = new("csv", ExcelType.CSV, BuildCsv);

    public ExcelType Excel { get; }

    [UseDelegateFromConstructor]
    public partial Configuration Policy(Option<OpenXmlStyleOptions> style, Option<TabularWorkbook> workbook);

    public bool CanReach(TabularFormat to) => TabularTranscode.Table.ContainsKey((this, to));

    static Configuration BuildXlsx(Option<OpenXmlStyleOptions> style, Option<TabularWorkbook> workbook) {
        OpenXmlConfiguration policy = new() {
            EnableSharedStringCache = true, TrimColumnNames = true, IgnoreEmptyRows = true,
            DateOnlyConversionMode = DateOnlyConversionMode.RequireMidnight,
        };
        style.IfSome(value => policy.StyleOptions = value);
        workbook.IfSome(profile => {
            policy.FreezeRowCount = profile.FreezeRows;
            policy.FreezeColumnCount = profile.FreezeColumns;
            policy.AutoFilter = profile.AutoFilter;
            policy.TableStyles = profile.Tables;
            policy.EnableAutoWidth = profile.AutoWidth;
            policy.MinWidth = profile.MinWidth;
            policy.MaxWidth = profile.MaxWidth;
            policy.DynamicSheets = [.. profile.Sheets];
        });
        return policy;
    }

    static Configuration BuildCsv(Option<OpenXmlStyleOptions> _, Option<TabularWorkbook> __) => new CsvConfiguration { ReadEmptyStringAsNull = true };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Origin {
    private Origin() { }
    public sealed record FromPath(string Path) : Origin;
    public sealed record FromStream(Func<Stream> Open) : Origin;

    public TResult Read<TResult>(Func<string, TResult> path, Func<Stream, TResult> stream) => Switch(
        (Path: path, Stream: stream),
        fromPath:   static (project, c) => project.Path(c.Path),
        fromStream: static (project, c) => project.Stream(c.Open()));
}

[ComplexValueObject]
public sealed partial class RowWindow {
    public string StartCell { get; }
    public string EndCell { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string startCell, ref string endCell) {
        if (string.IsNullOrWhiteSpace(startCell) || string.IsNullOrWhiteSpace(endCell)) {
            validationError = ValidationError.Create("<tabular-window-cell>");
        }
    }
}

[ComplexValueObject]
public sealed partial class TabularWorkbook {
    public int FreezeRows { get; }
    public int FreezeColumns { get; }
    public bool AutoFilter { get; }
    public TableStyles Tables { get; }
    public bool AutoWidth { get; }
    public double MinWidth { get; }
    public double MaxWidth { get; }
    public Seq<DynamicExcelSheet> Sheets { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref int freezeRows, ref int freezeColumns, ref bool autoFilter,
        ref TableStyles tables, ref bool autoWidth, ref double minWidth, ref double maxWidth, ref Seq<DynamicExcelSheet> sheets) {
        if (freezeRows < 0 || freezeColumns < 0 || minWidth < 0d || maxWidth < minWidth) {
            validationError = ValidationError.Create("<tabular-workbook-policy>");
        }
    }
}

// --- [MODELS] ---------------------------------------------------------------------------

[ComplexValueObject]
public sealed partial class TabularSpec {
    public TabularFormat Format { get; }
    public Origin Source { get; }
    public Option<string> Sheet { get; }
    public bool HeaderRow { get; }
    public Option<RowWindow> Window { get; }
    public Seq<string> WireColumns { get; }
    public Option<OpenXmlStyleOptions> Style { get; }
    public Option<TabularWorkbook> Workbook { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref TabularFormat format, ref Origin source, ref Option<string> sheet,
        ref bool headerRow, ref Option<RowWindow> window, ref Seq<string> wireColumns, ref Option<OpenXmlStyleOptions> style,
        ref Option<TabularWorkbook> workbook) {
        if (source is Origin.FromPath { Path: string path } && string.IsNullOrWhiteSpace(path)) {
            validationError = ValidationError.Create("<tabular-spec-path>");
        } else if (sheet.Map(string.IsNullOrWhiteSpace).IfNone(false)) {
            validationError = ValidationError.Create("<tabular-spec-sheet>");
        } else if (wireColumns.Exists(string.IsNullOrWhiteSpace) || wireColumns.Distinct().Count != wireColumns.Count) {
            validationError = ValidationError.Create("<tabular-spec-wire-columns>");
        } else if (format != TabularFormat.Xlsx && (style.IsSome || workbook.IsSome)) {
            validationError = ValidationError.Create("<tabular-spec-openxml-policy>");
        }
    }

    public string StartCell => Window.Match(Some: static w => w.StartCell, None: static () => "A1");

    public IConfiguration Policy() {
        Configuration policy = Format.Policy(Style, Workbook);
        policy.DynamicColumns = [.. WireColumns.Map(TabularWire.Wire)];
        return policy;
    }
}

public static class TabularTranscode {
    public static readonly FrozenDictionary<(TabularFormat From, TabularFormat To), (Action<string, string> Path, Action<Stream, Stream> Stream)> Table =
        new Dictionary<(TabularFormat, TabularFormat), (Action<string, string>, Action<Stream, Stream>)> {
            [(TabularFormat.Csv, TabularFormat.Xlsx)] = (MiniExcel.ConvertCsvToXlsx, MiniExcel.ConvertCsvToXlsx),
            [(TabularFormat.Xlsx, TabularFormat.Csv)] = (MiniExcel.ConvertXlsxToCsv, MiniExcel.ConvertXlsxToCsv),
        }.ToFrozenDictionary();
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
public abstract partial record TabularFault : Expected, IValidationError<TabularFault>, Semigroup<TabularFault> {
    private TabularFault() : base() { }
    public sealed record ColumnMissing(string Column, int Row) : TabularFault;
    public sealed record CellCast(string Column, int Row, string Target) : TabularFault;
    public sealed record NotSerializable(string Member) : TabularFault;
    public sealed record TranscodeUnreachable(string From, string To) : TabularFault;
    public sealed record BulkRefused(string Detail) : TabularFault;
    public sealed record CodecReject(string Detail) : TabularFault;
    public sealed record Aggregate(Seq<TabularFault> Faults) : TabularFault;

    public override int Code => FaultBand.Tabular + Switch(
        columnMissing:        static _ => 0,
        cellCast:             static _ => 1,
        notSerializable:      static _ => 2,
        transcodeUnreachable: static _ => 3,
        bulkRefused:          static _ => 4,
        codecReject:          static _ => 5,
        aggregate:            static _ => 6);

    public override string Message => Switch(
        columnMissing:        static c => $"<tabular-column-missing:{c.Column}@{c.Row}>",
        cellCast:             static c => $"<tabular-cell-cast:{c.Column}@{c.Row}:{c.Target}>",
        notSerializable:      static c => $"<tabular-not-serializable:{c.Member}>",
        transcodeUnreachable: static c => $"<tabular-transcode-unreachable:{c.From}->{c.To}>",
        bulkRefused:          static c => $"<tabular-bulk-refused:{c.Detail}>",
        codecReject:          static c => $"<tabular-codec-reject:{c.Detail}>",
        aggregate:            static c => $"<tabular-aggregate:{c.Faults.Count}>");

    public override string Category => Switch(
        columnMissing:        static _ => "Column",
        cellCast:             static _ => "Cell",
        notSerializable:      static _ => "Serialize",
        transcodeUnreachable: static _ => "Transcode",
        bulkRefused:          static _ => "Bulk",
        codecReject:          static _ => "Codec",
        aggregate:            static _ => "Aggregate");

    public static TabularFault Create(string message) => new CodecReject(message);

    public TabularFault Combine(TabularFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };

    public static TabularFault Lift(Exception boundary) => boundary switch {
        ExcelColumnNotFoundException c => new ColumnMissing(c.ColumnName ?? "", c.RowIndex),
        ExcelInvalidCastException c => new CellCast(c.ColumnName ?? "", c.Row, c.InvalidCastType?.Name ?? ""),
        MiniExcelNotSerializableException s => new NotSerializable(s.Member?.Name ?? ""),
        _ => new CodecReject($"{boundary.GetType().Name}:{boundary.Message}"),
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TabularFactKind {
    public static readonly TabularFactKind Read = new("read");
    public static readonly TabularFactKind Write = new("write");
    public static readonly TabularFactKind Append = new("append");
    public static readonly TabularFactKind Template = new("template");
    public static readonly TabularFactKind Adorn = new("adorn");
    public static readonly TabularFactKind Transcode = new("transcode");
    public static readonly TabularFactKind Redact = new("redact");
    public static readonly TabularFactKind Bulk = new("bulk");
}

public readonly record struct TabularFact(TabularFactKind Kind, string Format, Option<string> Sheet, long Rows, Instant At);

public readonly record struct TabularSheet(Seq<SheetInfo> Sheets, Seq<string> Columns, Option<ExcelRange> Dimension);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TabularAdornment {
    private TabularAdornment() { }
    public sealed record MergeCells(string Target) : TabularAdornment;
    public sealed record Pictures(Seq<MiniExcelPicture> Images) : TabularAdornment;
}

public readonly record struct RedactionPlan(IRedactorProvider Provider, HashMap<string, DataClassificationSet> Columns) {
    public object? Cell(string column, object? value) =>
        Columns.Find(column).Match(Some: set => Provider.GetRedactor(set).Redact(value), None: () => value);

    public string Cell(string column, ReadOnlySpan<char> value) => Provider.GetRedactor(Columns[column]).Redact(value);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class TabularSource {
    // The registry-mounted census derives from the kind vocabulary; the `store.tabular.` prefix declares once here.
    public static readonly Seq<StoreSlot> Slots =
        toSeq(TabularFactKind.Items).Map(static kind => StoreSlot.Create($"store.tabular.{kind.Key}"));

    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => toSeq(spec.Window.Match(
                Some: w => spec.Source.Read(
                    path:   p => MiniExcel.QueryRange(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, w.EndCell, spec.Policy()),
                    stream: s => s.QueryRange(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, w.EndCell, spec.Policy())),
                None: () => spec.Source.Read(
                    path:   p => MiniExcel.Query(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()),
                    stream: s => s.Query(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy())))
                .Cast<IDictionary<string, object>>()))
            .Bind(TabularWire.Project<T>))
        from _ in rows.Match(
            Succ: r => sink(new TabularFact(TabularFactKind.Read, spec.Format.Key, spec.Sheet, r.Count, frame.Now())),
            Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Seq<HashMap<string, object>>>> Scan(TabularSpec spec, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => toSeq(MiniExcel.Query(p, spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Select(Bag)),
            stream: s => toSeq(s.Query(spec.HeaderRow, spec.Sheet.IfNoneUnsafe(() => null), spec.Format.Excel, spec.StartCell, spec.Policy()).Select(Bag)))))
        from _ in rows.Match(Succ: r => sink(new TabularFact(TabularFactKind.Read, spec.Format.Key, spec.Sheet, r.Count, frame.Now())), Fail: _ => IO.pure(unit))
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

    public static IO<Validation<TabularFault, int[]>> Write(TabularSpec spec, object value, Option<RedactionPlan> plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) {
        (object Payload, TabularFactKind Kind) write = plan.Match(
            Some: p => (Redact(value, p), TabularFactKind.Redact),
            None: () => (value, TabularFactKind.Write));
        return from counts in IO.lift(() => Capture(() => spec.Source.Read(
                   path:   p => MiniExcel.SaveAs(p, write.Payload, spec.HeaderRow, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), overwriteFile: true),
                   stream: s => s.SaveAs(write.Payload, spec.HeaderRow, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy()))))
               from _ in counts.Match(Succ: c => sink(new TabularFact(write.Kind, spec.Format.Key, spec.Sheet, c.Sum(), frame.Now())), Fail: _ => IO.pure(unit))
               select counts;
    }

    public static IO<Validation<TabularFault, int>> Append(TabularSpec spec, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => MiniExcel.Insert(p, value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), spec.HeaderRow, overwriteSheet: false),
            stream: s => s.Insert(value, spec.Sheet.IfNone("Sheet1"), spec.Format.Excel, spec.Policy(), spec.HeaderRow, overwriteSheet: false))))
        from _ in rows.Match(Succ: r => sink(new TabularFact(TabularFactKind.Append, spec.Format.Key, spec.Sheet, r, frame.Now())), Fail: _ => IO.pure(unit))
        select rows;

    public static IO<Validation<TabularFault, Unit>> Render(TabularSpec spec, ReadOnlyMemory<byte> template, object value, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => spec.Source.Read(
            path:   p => { MiniExcel.SaveAsByTemplate(p, template.ToArray(), value); return unit; },
            stream: s => { s.SaveAsByTemplate(template.ToArray(), value); return unit; })))
        from _ in done.Match(Succ: _ => sink(new TabularFact(TabularFactKind.Template, spec.Format.Key, spec.Sheet, 0, frame.Now())), Fail: _ => IO.pure(unit))
        select done;

    public static IO<Validation<TabularFault, Unit>> Transcode(TabularSpec spec, TabularFormat to, string target, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        TabularTranscode.Table.TryGetValue((spec.Format, to), out (Action<string, string> Path, Action<Stream, Stream> Stream) codec)
            ? from done in IO.lift(() => Capture(() => spec.Source.Read(
                  path:   p => { codec.Path(p, target); return unit; },
                  stream: s => { using FileStream lowered = File.Create(target); codec.Stream(s, lowered); return unit; })))
              from _ in done.Match(Succ: _ => sink(new TabularFact(TabularFactKind.Transcode, $"{spec.Format.Key}->{to.Key}", None, 0, frame.Now())), Fail: _ => IO.pure(unit))
              select done
            : IO.pure((Validation<TabularFault, Unit>)new TabularFault.TranscodeUnreachable(spec.Format.Key, to.Key));

    public static IO<Validation<TabularFault, Unit>> Adorn(TabularSpec spec, TabularAdornment adornment, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => adornment.Switch(
            spec,
            mergeCells: static (s, m) => s.Source.Read(
                path:   p => { MiniExcel.MergeSameCells(m.Target, p); return unit; },
                stream: source => {
                    using MemoryStream buffered = new();
                    source.CopyTo(buffered);
                    using FileStream merged = File.Create(m.Target);
                    merged.MergeSameCells(buffered.ToArray());
                    return unit;
                }),
            pictures: static (s, images) => s.Source.Read(
                path:   p => { MiniExcel.AddPicture(p, [.. images.Images]); return unit; },
                stream: source => { MiniExcel.AddPicture(source, [.. images.Images]); return unit; }))))
        from _ in done.Match(Succ: _ => sink(new TabularFact(TabularFactKind.Adorn, spec.Format.Key, spec.Sheet, 0, frame.Now())), Fail: _ => IO.pure(unit))
        select done;

    internal static Validation<TabularFault, TValue> Capture<TValue>(Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: static value => (Validation<TabularFault, TValue>)value,
            Fail: static e => (Validation<TabularFault, TValue>)TabularFault.Lift(e.ToException()));

    static HashMap<string, object> Bag(dynamic row) =>
        toHashMap(((IDictionary<string, object>)row).Map(static kv => (kv.Key, kv.Value)));

    static object Redact(object value, RedactionPlan plan) => value switch {
        DataTable table => table.Rows.Cast<DataRow>().Select(row => table.Columns.Cast<DataColumn>()
            .ToDictionary(static column => column.ColumnName, column => plan.Cell(column.ColumnName, row[column] is DBNull ? null : row[column]))),
        IDataReader reader => Drained(reader, plan),
        IEnumerable rows and not string => rows.Cast<object>().Select(row => TabularWire.RedactRow(row, plan)),
        _ => TabularWire.RedactRow(value, plan),
    };

    static IEnumerable<Dictionary<string, object?>> Drained(IDataReader reader, RedactionPlan plan) {
        while (reader.Read()) {
            yield return Enumerable.Range(0, reader.FieldCount).ToDictionary(
                reader.GetName,
                at => plan.Cell(reader.GetName(at), reader.IsDBNull(at) ? null : reader.GetValue(at)));
        }
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------

public static class TabularWire {
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions(JsonSerializerOptions.Default) {
            Converters = { new ThinktectureJsonConverterFactory() },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static Validation<TabularFault, Seq<T>> Project<T>(Seq<IDictionary<string, object>> rows) =>
        Contract<T>(rows).Bind(_ =>
            rows.Map((row, at) => Minted<T>(row, at)).Traverse(identity).As());

    public static T Bind<T>(IDictionary<string, object> row) {
        T? value = JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToUtf8Bytes(row, Options), Options);
        return value is null ? throw new JsonException("<tabular-wire-null>") : value;
    }

    public static DynamicExcelColumn Wire(string column) => new(column) { CustomFormatter = Scalar };

    static object Scalar(object cell) {
        JsonElement scalar = JsonSerializer.SerializeToElement(cell, Options);
        return scalar.ValueKind switch {
            JsonValueKind.String => scalar.GetString() is string text ? text : DBNull.Value,
            JsonValueKind.Number when scalar.TryGetInt64(out long integer) => integer,
            JsonValueKind.Number when scalar.TryGetDecimal(out decimal number) => number,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => DBNull.Value,
            _ => scalar.GetRawText(),
        };
    }

    public static object RedactRow(object row, RedactionPlan plan) =>
        Lowered(row).ToDictionary(static kv => kv.Key, kv => plan.Cell(kv.Key, kv.Value));

    static Validation<TabularFault, Unit> Contract<T>(Seq<IDictionary<string, object>> rows) =>
        rows.HeadOrNone().Match(
            None: static () => (Validation<TabularFault, Unit>)unit,
            Some: static head => toSeq(Options.GetTypeInfo(typeof(T)).Properties)
                .Filter(static p => p.Set is not null)
                .Map(static p => p.Name)
                .Filter(name => !head.ContainsKey(name))
                .Traverse(static name => (Validation<TabularFault, string>)new TabularFault.ColumnMissing(name, 0))
                .As()
                .Map(static _ => unit));

    static Validation<TabularFault, T> Minted<T>(IDictionary<string, object> row, int at) =>
        Try.lift(() => Bind<T>(row)).Run().Match(
            Succ: static value => (Validation<TabularFault, T>)value,
            Fail: e => (Validation<TabularFault, T>)(e.ToException() is JsonException wire
                ? new TabularFault.CellCast(wire.Path ?? "<row>", at, typeof(T).Name)
                : TabularFault.Lift(e.ToException())));

    static IDictionary<string, object?> Lowered(object row) =>
        row is IDictionary<string, object> dict
            ? dict.ToDictionary(static kv => kv.Key, static kv => (object?)kv.Value)
            : Options.GetTypeInfo(row.GetType()).Properties
                .Where(static property => property.Get is not null)
                .ToDictionary(static property => property.Name, property => property.Get?.Invoke(row));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                          | [BINDING]                                                   |
| :-----: | :-------------------- | :----------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | one tabular owner     | `TabularSource` over MiniExcel                   | `#DELIMITED_SOURCE` owns delimited text; one record rail     |
|  [02]   | modality discriminant | `TabularSpec` + `Origin` `[Union]`               | one value selects typed/dynamic/reader/probe                 |
|  [03]   | typed read            | header-keyed `dynamic` rows → `Project<T>`       | contract-checked + accumulated; not the `Query<T>` binder    |
|  [04]   | windowed typed read   | `dynamic` `QueryRange` → the SAME `Project<T>`   | `QueryRange` adds only `endCell`; one shared projection      |
|  [05]   | format policy         | `[UseDelegateFromConstructor]` `Policy` column   | no consumer branches on the format key                       |
|  [06]   | wire-cell projection  | `DynamicExcelColumn.CustomFormatter`             | `spec.Policy()` registers on `DynamicColumns`, wire-minted   |
|  [07]   | row-boundary fault    | `Validation<TabularFault, …>` on both legs       | `Semigroup` + `Aggregate` accumulate; both legs fold         |
|  [08]   | bulk vs columnar      | typed `IEnumerable<T>` vs `IDataReader`          | bulk-copy from `IEnumerable<T>`; reader feeds columnar       |
|  [09]   | redacted egress       | per-column `Redactor` before `SaveAs`            | streams column-redacted; no `DbDataReader` decorator         |
|  [10]   | transcode             | `TabularTranscode.Table` frozen correspondence   | `CanReach` derives; a miss is the typed fault                |
|  [11]   | receipt               | one `TabularFact` stream under `store.tabular.*` | kind-discriminated; never parallel receipt records           |
|  [12]   | element projection    | per-app tabular→element map                      | `[02]-[SEAMS]` `Ingest → Rasm.Element` wire; row shape only  |
|  [13]   | fault band            | `Code => FaultBand.Tabular + n`                  | re-banded 839x off the 837x collision                        |
|  [14]   | report egress         | `Adorn` over `TabularAdornment` + spec `Style`   | merge/pictures/styling as policy DATA                        |

## [03]-[DELIMITED_SOURCE]

- Owner: `DelimitedSpec` the parse-policy value fixing separator, culture, trim classifier, header stance, parallelism, and the string-pool caps in one declaration — reader and writer options BOTH derive from it as one hardened pair; `DelimitedSource` the static surface owning the record-rail materialization (`Read<T>`), the async bulk stream (`Stream<T>`), and the reader→writer redaction/projection bridge (`Project`) — the `Sep` SIMD delimited owner made an EXPLICIT fence, projecting into the SAME downstream record rail the MiniExcel legs feed.
- Cases: a read materializes through `Enumerate(RowFunc<T>)` (sequential), `ParallelEnumerate(RowFunc<T>, degreeOfParallelism)` (order-preserving parallel projection — the spec's `Parallelism > 1` selects it and the THREAD-SAFE pool variant with it), or `EnumerateAsync(RowFunc<T>)` (the `IAsyncEnumerable<T>` bulk source); the `RowFunc<T>` projection is the ONE boundary where `ref struct` rows lift into domain records — `Col.Parse<T>()` for `ISpanParsable<T>` columns, the column span handed to the wire converters otherwise, never `string`-materialize-then-parse in the hot path.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` opens the reader over the `Origin` (`FromFile` on a path, `From` on a caller-owned stream), materializes through the spec-selected enumerator, folds the boundary through the shared `TabularSource.Capture` funnel, and rides a `read` fact; `public static async IAsyncEnumerable<T> Stream<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, CancellationToken token = default)` is the `#BULK_LANE` source — `FromFileAsync`/`FromAsync` then `EnumerateAsync`, streaming a delimited file into the bulk copy without buffering; `public static IO<Validation<TabularFault, long>> Project(DelimitedSpec spec, Origin source, Origin target, RedactionPlan plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink)` is the redaction/column-projection egress — reader→writer per row, an UNCLASSIFIED column copied span-to-span (`Col.Set(span)`, zero materialization) and a classified column redacted through `plan.Cell` before `Col.Set`, riding a `redact` fact.
- Auto: the options compose functionally off the spec as one HARDENED PAIR — `Sep.New(char)` on a declared separator or `Sep.Default` (the semicolon contract) on `None`, `SepReaderOptions.CultureInfo` `null` for the invariant fast path, `Trim` the `[Flags]` `SepTrim` classifier, `CreateToString` the pool policy (`PoolPerColThreadSafeFixedCapacity` under parallel consumption, `PoolPerCol` otherwise — low-cardinality identifier columns collapse to pooled lookups), and `Strict()` applied to BOTH the reader and the writer options because one side hardened alone produces files the other cannot round-trip; separator auto-detection is exploratory ingest, never a contract profile, so an exploratory read probes first and declares the found separator; header resolution rides `Header.IndexOf`/`IndicesOf(Span<string>, Span<int>)` (caller-buffer, zero-alloc) and prefix windows ride `Header.NamesStartingWith`; rows, cols, and headers are `ref struct` projections that NEVER escape the read scope — materialization is the one `Enumerate` boundary.
- Receipt: a delimited read rides `store.tabular.read` carrying the row count and `"delimited"` as the format; a projection/redaction pass rides `store.tabular.redact` carrying the emitted row count; the bulk stream emits no fact of its own (the `#BULK_LANE` receipt carries the copied count).
- Packages: Sep (`Sep`/`SepSpec`/`SepReaderOptions`/`SepWriterOptions`/`SepTrim`/`SepToString` pool family/`SepReader` + `Row`/`Col`/`Cols`/`RowFunc<T>`/`SepReaderHeader`/`Enumerate`/`EnumerateAsync`/`ParallelEnumerate`/`FromFile`/`From`/`FromFileAsync`/`FromAsync`/`Strict`/`SepWriter` + `NewRow` — namespace `nietras.SeparatedValues`), Microsoft.Extensions.Compliance.Redaction (the per-column redactor the bridge resolves), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new parse stance is one `DelimitedSpec` field consumed by the options fold; a new projection is one `RowFunc<T>` at the call site; zero new surface — a hand-rolled CSV split/parse pipeline, `string.Split` on a parse path, a `ref struct` row escaping the read scope, per-column `string`-materialize-then-parse in a hot path, a reader hardened without its writer, auto-detect as a contract profile, a second delimited codec beside Sep, or MiniExcel's convenience CSV leg used for the high-throughput lane is the deleted form because Sep owns SIMD delimited text, the options are one declared policy value, and the record rail is the one materialization boundary.
- Boundary: `DelimitedSource` and `TabularSource` are TWO codecs on ONE record rail — the source format selects the owner (`.xlsx` and transcode symmetry → MiniExcel; high-throughput delimited text → Sep) and both deliver anonymous record streams the per-app composition root maps to elements (the `[02]-[SEAMS]` `Ingest → Rasm.Element` row-shape law holds verbatim); the redaction bridge is the genuine reader→writer copy seam MiniExcel lacks (the writer `NewRow` re-emits a row column-by-column with only classified cells rewritten — the same `Redactor.Redact` typed-value seam, no unredacted value stringified into transit); the `Stream<T>` form is the `#BULK_LANE`'s `IAsyncEnumerable<T>` source (the `api-sep` stacking law: `Enumerate → BulkCopyAsync` streams a delimited file into PG binary COPY without buffering); Arrow/DuckDB own the BINARY columnar path — Sep never re-implements a columnar reader.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct DelimitedSpec(
    Option<char> Separator, Option<CultureInfo> Culture, SepTrim Trim, bool Header, int Parallelism, int PoolMaxLength, int PoolCapacity) {
    public static readonly DelimitedSpec Default = new(None, None, SepTrim.None, true, 1, 128, 2048);

    public SepReaderOptions Options() =>
        Root().Reader(o => o with {
            CultureInfo = Culture.Match(Some: static value => value, None: static () => (CultureInfo?)null),
            HasHeader = Header,
            Trim = Trim,
            CreateToString = Parallelism > 1
                ? SepToString.PoolPerColThreadSafeFixedCapacity(PoolMaxLength, PoolCapacity)
                : SepToString.PoolPerCol(PoolMaxLength, PoolCapacity, PoolCapacity),
        }).Strict();

    public SepWriterOptions Writer() =>
        Root().Writer(o => o with {
            CultureInfo = Culture.Match(Some: static value => value, None: static () => (CultureInfo?)null),
            ColNotSetOption = SepColNotSetOption.Empty,
        }).Strict();

    Sep Root() => Separator.Map(Sep.New).IfNone(Sep.Default);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class DelimitedSource {
    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => TabularSource.Capture(() => {
            using SepReader reader = Open(spec, source);
            // Strict() drains inside the using scope — a lazy Seq escaping the disposed reader is a use-after-dispose.
            return (spec.Parallelism > 1
                ? toSeq(reader.ParallelEnumerate(shape, spec.Parallelism))
                : toSeq(reader.Enumerate(shape))).Strict();
        }))
        from _ in rows.Match(Succ: r => sink(new TabularFact(TabularFactKind.Read, "delimited", None, r.Count, frame.Now())), Fail: _ => IO.pure(unit))
        select rows;

    public static async IAsyncEnumerable<T> Stream<T>(DelimitedSpec spec, Origin source, SepReader.RowFunc<T> shape, [EnumeratorCancellation] CancellationToken token = default) {
        await using SepReader reader = await source.Read(
            path: p => spec.Options().FromFileAsync(p),
            stream: s => spec.Options().FromAsync(s)).ConfigureAwait(false);
        await foreach (T row in reader.EnumerateAsync(shape).WithCancellation(token).ConfigureAwait(false)) { yield return row; }
    }

    public static IO<Validation<TabularFault, long>> Project(DelimitedSpec spec, Origin source, Origin target, RedactionPlan plan, ProjectionContext frame, Func<TabularFact, IO<Unit>> sink) =>
        from emitted in IO.lift(() => TabularSource.Capture(() => {
            using SepReader reader = Open(spec, source);
            using SepWriter writer = target.Read(
                path: p => spec.Writer().ToFile(p),
                stream: s => spec.Writer().To(s));
            long rows = 0;
            foreach (SepReader.Row row in reader) {
                using SepWriter.Row emit = writer.NewRow();
                for (int i = 0; i < row.ColCount; i++) {
                    string name = reader.Header.ColNames[i];
                    if (plan.Columns.ContainsKey(name)) { emit[name].Set(plan.Cell(name, row[i].Span)); }
                    else { emit[name].Set(row[i].Span); }
                }
                rows++;
            }
            return rows;
        }))
        from _ in emitted.Match(Succ: r => sink(new TabularFact(TabularFactKind.Redact, "delimited", None, r, frame.Now())), Fail: _ => IO.pure(unit))
        select emitted;

    static SepReader Open(DelimitedSpec spec, Origin source) =>
        source.Read(
            path: p => spec.Options().FromFile(p),
            stream: s => spec.Options().From(s));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                              | [BINDING]                                                |
| :-----: | :--------------- | :--------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | delimited owner  | Sep `ref struct` rows + `Enumerate`                  | SIMD parse; materialization is the record-rail boundary   |
|  [02]   | parse policy     | `DelimitedSpec` one declared value                   | separator/culture/trim/pool/parallelism; strict BOTH ends |
|  [03]   | separator        | `None` = `Sep.Default` semicolon contract            | auto-detect is exploratory ingest, never a profile        |
|  [04]   | parallel reads   | `ParallelEnumerate` + thread-safe pool               | order-preserving; `Parallelism` selects it                |
|  [05]   | redaction egress | `NewRow` per-column re-emit, redact before `Col.Set` | unclassified columns copy span-to-span                    |
|  [06]   | bulk source      | `Stream<T>` `IAsyncEnumerable<T>`                    | feeds `#BULK_LANE` `BulkCopyAsync`; no file buffering     |

## [04]-[BULK_LANE]

- Owner: `TabularBulk` the static bulk-copy fence over the linq2db EF bridge — ONE `Copy<T>` operation whose input SHAPE discriminates the source (`IEnumerable<T>` the materialized read, `IAsyncEnumerable<T>` the streamed `#DELIMITED_SOURCE`/`Read<T>` rail), lowering to PostgreSQL binary COPY over the V6 identity `DbContext` with the mapping DERIVED from the EF model — never a second mapping, never a per-source method family.
- Cases: the source is `IEnumerable<T>` or `IAsyncEnumerable<T>` (overloads on input shape, one semantic operation); the copy strategy is `BulkCopyOptions.BulkCopyType.ProviderSpecific` (PG binary COPY — the default this fence pins) with `MultipleRows` the engine-neutral fallback a profile may select; `KeepIdentity` is PINNED true — under the time-ordered Guid-v7 identity row an unset flag lets the store re-mint identities and admission identity is lost; a refused copy rails `TabularFault.BulkRefused` in-band.
- Entry: `public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IAsyncEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class` and the `IEnumerable<T>` twin compose `LinqToDBForEFTools.BulkCopyAsync<T>(context, BulkCopyOptions, rows, token)` returning the TYPED `BulkCopyRowsCopied` receipt (rows copied + the options that produced it); `LinqToDBForEFTools.Initialize()` runs ONCE at the composition root (bridge-cache activation), never per call.
- Auto: `LinqToDBForEFTools.BulkCopyAsync<T>(DbContext, BulkCopyOptions, IEnumerable<T>, CancellationToken)` derives mapping from the supplied EF context; a lifted read remains outside EF change tracking because bulk copy consumes its source directly. `MergeWithOutputAsync<TTarget,TSource,TOutput>` supplies the `(action, deleted, inserted)` projection for an upsert on the same bridged context.
- Receipt: a copy rides `store.tabular.bulk` carrying the copied-row count off the `BulkCopyRowsCopied` receipt and the batch bound; a refusal rides the typed `BulkRefused` on the rail.
- Packages: linq2db.EntityFrameworkCore (`LinqToDBForEFTools.BulkCopyAsync<T>`); core `linq2db` (`BulkCopyOptions`/`BulkCopyType`/`BulkCopyRowsCopied`/`MergeWithOutputAsync`), Microsoft.EntityFrameworkCore (`DbContext` — the V6 identity context), LanguageExt.Core, BCL inbox.
- Growth: a new copy stance is one `BulkCopyOptions` field on the composed options value (`MaxBatchSize`/`ConflictAction`/`MaxDegreeOfParallelism` — the parallelism knob consumes the suite budget, never an independent pool); a new upsert shape is one `MergeWithOutputAsync` projection; zero new surface — a hand-rolled INSERT loop, a second mapping beside the EF model, a change-tracked bulk lift, an unset `KeepIdentity`, or a fourth bulk lane is the deleted form.
- Boundary: the three-row bulk-ingress boundary is LAW — DuckDB `COPY` owns columnar FILES (`Query/columnar`), the Npgsql binary importer owns raw-wire streams (`domain` bulk law at the store profile), and linq2db owns the EF-MODEL-MAPPED lane over the V6 identity `DbContext` — three non-overlapping rows on one rail, so a row entering through a typed domain record with an EF mapping takes THIS lane and never re-spells a COPY writer; the lane is `Ingest`-owned because its sources are the tabular/delimited codecs' record rails, while the target schema stays `Element/identity`'s (the DDL/migration owner) — this fence writes rows, never DDL.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class TabularBulk {
    public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IAsyncEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class =>
        Copied(() => LinqToDBForEFTools.BulkCopyAsync(identity, Options(batch), rows, token));

    public static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copy<T>(DbContext identity, IEnumerable<T> rows, Option<int> batch = default, CancellationToken token = default) where T : class =>
        Copied(() => LinqToDBForEFTools.BulkCopyAsync(identity, Options(batch), rows, token));

    static BulkCopyOptions Options(Option<int> batch) =>
        new(BulkCopyType: BulkCopyType.ProviderSpecific, KeepIdentity: true, MaxBatchSize: batch.ToNullable());

    static IO<Validation<TabularFault, BulkCopyRowsCopied>> Copied(Func<Task<BulkCopyRowsCopied>> copy) =>
        IO.liftAsync(async () => (Validation<TabularFault, BulkCopyRowsCopied>)await copy().ConfigureAwait(false))
        | @catch<IO, Validation<TabularFault, BulkCopyRowsCopied>>(static _ => true, static e => IO.pure((Validation<TabularFault, BulkCopyRowsCopied>)new TabularFault.BulkRefused(e.Message)));
}
```

| [INDEX] | [POLICY]      | [VALUE]                                                   | [BINDING]                                                  |
| :-----: | :------------ | :-------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | copy strategy | `BulkCopyType.ProviderSpecific` + `KeepIdentity`          | PG binary COPY; unset identity re-mints the Guid-v7 row    |
|  [02]   | mapping       | EF model via `EFCoreMetadataReader`                       | the V6 generated mapping; never a second mapping           |
|  [03]   | source shape  | `IEnumerable<T>` / `IAsyncEnumerable<T>` overloads        | input shape discriminates; never a method family           |
|  [04]   | receipt       | `BulkCopyRowsCopied`                                      | typed copied-count evidence riding `store.tabular.bulk`    |
|  [05]   | bulk boundary | files → DuckDB `COPY`; wire → Npgsql; EF-mapped → linq2db | three non-overlapping rows; never a fourth lane            |
