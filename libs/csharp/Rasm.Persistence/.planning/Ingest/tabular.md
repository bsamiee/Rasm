# [PERSISTENCE_INGEST_TABULAR]

Rasm.Persistence ingests and emits spreadsheet and delimited tabular data through ONE `TabularSource` owner over the MiniExcel zero-template streaming codec: a workbook or CSV of element rows reads lazily through `Query<T>`/`GetReader` into the same `IDataReader` record rail a CSV ingress feeds (the NodaTime/Thinktecture wire converters, the linq2db/EF bulk-copy path, the Arrow/DuckDB columnar materializer), and a catalog/cost/schedule export writes element-derived tables back through `SaveAs`/`SaveAsByTemplate`. The codec NEVER knows the element graph — the per-app tabular→element map (the wire-composition owner, owned per-app at the host/app composition root) projects each row into a `Rasm.Element` graph node, so `TabularSource` delivers only the row shape at the wire. One `TabularFormat` axis crosses the format (`.xlsx`/`.csv`) with the policy bag (`OpenXmlConfiguration`/`CsvConfiguration`), so a new tabular source is a format row, never a second codec; the typed read folds `ExcelColumnNotFoundException`/`ExcelInvalidCastException` into a `Fin`/`Validation` failure at the row boundary rather than throwing through the receipt path. MiniExcel is the spreadsheet boundary the `Sep` delimited lane cannot reach (and the columnar lane is the wrong shape for); `Sep` stays the high-throughput delimited codec, both projecting into the one record rail. `NodeId`, `ElementGraph` arrive from `Rasm.Element`; the bulk-copy bridge arrives from `api-linq2db-ef`; the columnar materializer from `Query/columnar`; `ClockPolicy`, `ReceiptSinkPort`, `DataClassification` from AppHost.

## [01]-[INDEX]

- [01]-[TABULAR_SOURCE]: the one format-and-policy axis, lazy typed/reader ingress, the row-boundary fault fold, and the egress/transcode surface.

## [02]-[TABULAR_SOURCE]

- Owner: `TabularFormat` the `[SmartEnum<string>]` format axis carrying its `ExcelType` and default policy; `TabularPolicy` the typed configuration carrier (`OpenXmlConfiguration` for `.xlsx`, `CsvConfiguration` for `.csv`); `TabularFault` the closed row-boundary fault deriving from `Expected`; `RowWindow` the cell-range window value; `TabularSource` the static surface owning the lazy typed/reader/range ingress, the row-boundary fault fold, and the egress/template/transcode.
- Cases: `TabularFormat` is `Xlsx` (`ExcelType.XLSX`, `OpenXmlConfiguration` with the shared-string disk cache on) or `Csv` (`ExcelType.CSV`, `CsvConfiguration`); `TabularFault` is `ColumnMissing | CellCast | NotSerializable` lifting the three MiniExcel exceptions; `RowWindow` carries the `A1:C3` start/end cells or the 1-based row/column index span.
- Entry: `public static IO<Validation<TabularFault, Seq<T>>> Read<T>(string path, TabularFormat format, Option<string> sheet, Option<RowWindow> window)` reads typed POCO rows folding the row-boundary faults into one `Validation`; `public static IDataReader Reader(Stream source, TabularFormat format, Option<string> sheet)` projects the streaming `MiniExcelDataReader` the bulk/columnar rail consumes; `public static IO<int[]> Write(string path, object value, TabularFormat format, Option<string> sheet, bool header)` writes any `IEnumerable`/`IDataReader`/`DataTable`/anonymous value; `public static IO<Unit> Render(string path, ReadOnlyMemory<byte> template, object value)` renders an `.xlsx` template; `public static IO<Unit> Transcode(string source, string target, TabularFormat from, TabularFormat to)` is the direct CSV↔XLSX transcode, branching explicitly on the two supported pairs (CSV→XLSX, XLSX→CSV) and failing fast on a CSV→CSV/XLSX→XLSX no-op or any invalid pairing rather than silently misrouting through `ConvertXlsxToCsv`.
- Auto: a typed read uses `Query<T>` with `ExcelColumnAttribute`/`ExcelColumnNameAttribute` aliases (never positional magic-index `dynamic` access on a known schema) so a column binds by header name and a `DynamicExcelColumn.CustomFormatter` projects an `Instant`/`LocalDate`/value-object cell at the boundary through the `api-nodatime-stj`/Thinktecture wire converters; a large read streams through `Query`/`GetReader` (lazy `yield`) with `EnableSharedStringCache` on so the shared-strings table spills to disk and a million-row workbook never fully resides in memory; the egress reuses the reader/lane rail the query produced (`SaveAs(stream, dataReader)`/`Insert`) so a schedule/cost/catalog export never re-materializes a table, and report-shaped output uses `SaveAsByTemplate` with placeholder binding instead of cell-by-cell writes; `GetReader` yields a `MiniExcelDataReader : IDataReader` so a spreadsheet streams into PostgreSQL binary COPY through the same `LinqToDBForEFTools.BulkCopyAsync` rail a CSV uses.
- Receipt: a read rides `store.tabular.read` carrying the row count and the format; a write rides `store.tabular.write` carrying the per-sheet row counts; a transcode rides `store.tabular.transcode`.
- Packages: MiniExcel (`MiniExcel.Query<T>`/`GetReader`/`SaveAs`/`SaveAsByTemplate`/`ConvertCsvToXlsx`/`ConvertXlsxToCsv`/`MiniExcelDataReader`/`OpenXmlConfiguration`/`CsvConfiguration`/`ExcelColumnAttribute`/`DynamicExcelColumn`), Sep (delimited sibling), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new tabular format is one `TabularFormat` row carrying its `ExcelType` and policy; a new column mapping is one `ExcelColumnAttribute`/`DynamicExcelColumn`; zero new surface — a hand-rolled OpenXML/CSV parser, a second spreadsheet codec, a `QueryAsDataTable` whole-sheet buffer on a streaming path, or a tabular→element map inside this codec is the deleted form because MiniExcel owns the spreadsheet/CSV codec, `Sep` the high-throughput delimited lane, and the app composition root owns the tabular→element projection.
- Boundary: `TabularSource` is the ONE tabular ingress/egress owner over MiniExcel — `Sep` (`api-sep`) is the high-throughput delimited-text sibling and the two project into the SAME downstream record rail, the profile selecting one by source format, while `Apache.Arrow`/`DuckDB`/`ParquetSharp` own the binary columnar path (a `.xlsx` is never treated as a columnar file); the codec NEVER knows the element graph — the per-app tabular→element map is the wire-composition owner (owned per-app at the host/app composition root, `§6`), so a workbook of element rows reads through `Query<T>`/`GetReader`, the app projects each row into an `ElementGraph` node, and a catalog/cost/schedule egress writes element-derived tables back through `SaveAs`, the codec seeing only the row shape at the wire; a typed `Query<T>` against a missing header surfaces `ExcelColumnNotFoundException` and a bad cell `ExcelInvalidCastException`, both folded into a typed `Validation` failure at the row boundary rather than thrown through the receipt path; a redacted export wraps the `IDataReader` in a redacting decorator applying `Microsoft.Extensions.Compliance.Redaction` per column so a redacted spreadsheet streams without materializing the table; MiniExcel retires `Sylvan.Data.Excel` as the spreadsheet codec — no `Sylvan` reference remains.

```csharp signature
public sealed class TabularKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TabularKeyPolicy, string>]
[KeyMemberComparer<TabularKeyPolicy, string>]
public sealed partial class TabularFormat {
    public static readonly TabularFormat Xlsx = new("xlsx", ExcelType.XLSX);
    public static readonly TabularFormat Csv = new("csv", ExcelType.CSV);
    public ExcelType Excel { get; }
    private TabularFormat(string key, ExcelType excel) : this(key) => Excel = excel;
    public IConfiguration Policy() => Excel == ExcelType.XLSX
        ? new OpenXmlConfiguration { EnableSharedStringCache = true, TrimColumnNames = true, IgnoreEmptyRows = true }
        : new CsvConfiguration { ReadEmptyStringAsNull = true };
}

public readonly record struct RowWindow(string StartCell, string EndCell);

[Union]
public abstract partial record TabularFault : Expected, IValidationError<TabularFault> {
    private TabularFault(string detail, int code) : base(detail, code, None) { }
    public static TabularFault Create(string message) => new ColumnMissing(message, 0);
    public sealed record ColumnMissing(string Column, int Row) : TabularFault($"<tabular-column-missing:{Column}@{Row}>", 8370);
    public sealed record CellCast(string Column, int Row, string Target) : TabularFault($"<tabular-cell-cast:{Column}@{Row}>{Target}>", 8371);
    public sealed record NotSerializable(string Member) : TabularFault($"<tabular-not-serializable:{Member}>", 8372);

    public static TabularFault Lift(Exception boundary) => boundary switch {
        ExcelColumnNotFoundException c => new ColumnMissing(c.ColumnName ?? "", c.RowIndex ?? 0),
        ExcelInvalidCastException c => new CellCast(c.ColumnName ?? "", c.Row ?? 0, c.InvalidCastType?.Name ?? ""),
        MiniExcelNotSerializableException s => new NotSerializable(s.Member?.Name ?? ""),
        _ => new ColumnMissing(boundary.Message, 0),
    };
}

public static class TabularSource {
    public static IO<Validation<TabularFault, Seq<T>>> Read<T>(string path, TabularFormat format, Option<string> sheet, Option<RowWindow> window) =>
        IO.lift(() => Try.lift(() => toSeq(window.Match(
                Some: w => MiniExcel.Query<T>(path, sheet.IfNoneUnsafe(() => null), format.Excel, w.StartCell, format.Policy()),
                None: () => MiniExcel.Query<T>(path, sheet.IfNoneUnsafe(() => null), format.Excel, "A1", format.Policy()))))
            .Run().Match(Succ: Validation<TabularFault, Seq<T>>.Success, Fail: e => Validation<TabularFault, Seq<T>>.Fail(TabularFault.Lift(e.ToException()))));

    public static IDataReader Reader(Stream source, TabularFormat format, Option<string> sheet) =>
        MiniExcel.GetReader(source, useHeaderRow: true, sheet.IfNoneUnsafe(() => null), format.Excel, "A1", format.Policy());

    public static IO<int[]> Write(string path, object value, TabularFormat format, Option<string> sheet, bool header) =>
        IO.lift(() => MiniExcel.SaveAs(path, value, header, sheet.IfNone("Sheet1"), format.Excel, format.Policy(), overwriteFile: true));

    public static IO<Unit> Render(string path, ReadOnlyMemory<byte> template, object value) =>
        IO.lift(() => { MiniExcel.SaveAsByTemplate(path, template.ToArray(), value); return unit; });

    public static IO<Unit> Transcode(string source, string target, TabularFormat from, TabularFormat to) =>
        IO.lift(() => {
            if (from == TabularFormat.Csv && to == TabularFormat.Xlsx)
                MiniExcel.ConvertCsvToXlsx(source, target);
            else if (from == TabularFormat.Xlsx && to == TabularFormat.Csv)
                MiniExcel.ConvertXlsxToCsv(source, target);
            else
                throw new ArgumentException($"Unsupported transcode {from} -> {to}");
            return unit;
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one tabular owner   | `TabularSource` over MiniExcel         | `Sep` is the delimited sibling; both feed one record rail  |
|  [02]   | typed read          | `Query<T>` by `ExcelColumnAttribute`   | never positional magic-index `dynamic` on a known schema  |
|  [03]   | row-boundary fault  | `Validation<TabularFault, …>`          | column-missing/cell-cast folded, never thrown to receipts  |
|  [04]   | element projection  | per-app tabular→element map            | the codec sees only the row shape; the app projects nodes  |
|  [05]   | bulk ingress        | `MiniExcelDataReader : IDataReader`    | streams into PG binary COPY through the linq2db bulk rail  |
