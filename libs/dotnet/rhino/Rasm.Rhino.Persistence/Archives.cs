using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveSource {
    public sealed record File(string Path, Option<(File3dm.TableTypeFilter Tables, File3dm.ObjectTypeFilter Objects)> Filters) : ArchiveSource;

    public sealed record Bytes(ReadOnlyMemory<byte> Data) : ArchiveSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveOp {
    public sealed record Notes(File3dmNotes Value) : ArchiveOp;

    public sealed record SetString(DocumentTextKey Key, string Value) : ArchiveOp;

    public sealed record Delete(DocumentTextKey Key) : ArchiveOp;

    public sealed record DeleteNamedView(int Index) : ArchiveOp;

    public sealed record ModelUnits(LengthUnit Unit) : ArchiveOp;

    public sealed record EarthAnchor(Func<EarthAnchorPoint, IO<Unit>> Edit) : ArchiveOp;

    public sealed record AddObject(GeometryBase Geometry, Option<ObjectAttributes> Attributes) : ArchiveOp;

    public sealed record DeleteObject(Guid Id) : ArchiveOp;

    public sealed record EmbedFile(string Path) : ArchiveOp;
}

public sealed record RevisionHistoryState(Option<string> CreatedBy, Option<string> LastEditedBy, int Revision, Option<DateTime> CreatedOn, Option<DateTime> LastEditedOn);

public sealed record ApplicationDataState(Option<string> ApplicationName, Option<string> ApplicationUrl, Option<string> ApplicationDetails);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Archives {
    // --- [SCOPES]
    public static IO<TValue> WithArchive<TValue>(ArchiveSource source, Func<File3dm, IO<TValue>> body) =>
        Disposal.Using(
            source.Switch(
                file: static file =>
                    from existing in Answers.ExistingPath(file.Path)
                    from archive in IO.lift(() => file.Filters.Match(
                        Some: filters => Optional(File3dm.ReadWithLog(existing, filters.Tables, filters.Objects, out string log)).ToFin(new ArchiveRejected(nameof(File3dm.ReadWithLog), log)),
                        None: () => Optional(File3dm.ReadWithLog(existing, out string log)).ToFin(new ArchiveRejected(nameof(File3dm.ReadWithLog), log))))
                    select archive,
                bytes: static bytes => IO.lift(() => Missing.Unless(File3dm.FromByteArray(bytes.Data.ToArray()), nameof(File3dm.FromByteArray)))),
            body);

    // --- [HEADERS]
    public static IO<Option<string>> ReadNotes(string path) =>
        Answers.ExistingPath(path).Map(static existing => Answers.Present(File3dm.ReadNotes(existing)));

    public static IO<int> ReadArchiveVersion(string path) =>
        Answers.ExistingPath(path).Map(static existing => File3dm.ReadArchiveVersion(existing));

    public static IO<RevisionHistoryState> ReadRevisionHistory(string path) =>
        from existing in Answers.ExistingPath(path)
        from history in IO.lift(() => Refused.Unless(
            File3dm.ReadRevisionHistory(existing, out string createdBy, out string lastEditedBy, out int revision, out DateTime createdOn, out DateTime lastEditedOn),
            new RevisionHistoryState(
                Answers.Present(createdBy),
                Answers.Present(lastEditedBy),
                revision,
                Some(createdOn).Filter(static date => date != DateTime.MinValue),
                Some(lastEditedOn).Filter(static date => date != DateTime.MinValue)),
            nameof(File3dm.ReadRevisionHistory)))
        select history;

    public static IO<ApplicationDataState> ReadApplicationData(string path) =>
        from existing in Answers.ExistingPath(path)
        from data in IO.lift(() => {
            File3dm.ReadApplicationData(existing, out string name, out string url, out string details);
            return new ApplicationDataState(Answers.Present(name), Answers.Present(url), Answers.Present(details));
        })
        select data;

    public static IO<EarthAnchorState> ReadEarthAnchorPoint(string path) =>
        from existing in Answers.ExistingPath(path)
        from anchor in Disposal.Using(
            IO.lift(() => Missing.Unless(File3dm.ReadEarthAnchorPoint(existing), nameof(File3dm.ReadEarthAnchorPoint))),
            DocumentUnits.ReadEarthAnchor)
        select anchor;

    public static IO<Seq<TRow>> ReadPageViews<TRow>(string path, Func<ViewInfo, IO<TRow>> project) =>
        Answers.ExistingPath(path).Bind(existing => TableOps.ReadRows(() => File3dm.ReadPageViews(existing), project));

    public static IO<Seq<TRow>> ReadDimensionStyles<TRow>(string path, Func<DimensionStyle, IO<TRow>> project) =>
        Answers.ExistingPath(path).Bind(existing => TableOps.ReadRows(() => File3dm.ReadDimensionStyles(existing), project));

    // --- [PREVIEW]
    public static IO<Option<Bitmap>> ReadPreviewImage(string path) =>
        Answers.ExistingPath(path).Map(static existing => Optional(File3dm.ReadPreviewImage(existing)));

    public static IO<Option<Bitmap>> GetPreviewImage(File3dm archive) =>
        IO.lift(() => Optional(archive.GetPreviewImage()));

    public static IO<Unit> SetPreviewImage(File3dm archive, Option<Bitmap> image) =>
        IO.lift(() => archive.SetPreviewImage(image.ValueUnsafe()));

    // --- [EDITS]
    public static IO<Unit> ApplyArchiveOp(File3dm archive, ArchiveOp op) =>
        op.Switch(
            archive,
            notes: static (target, notes) => IO.lift(() => { target.Notes = notes.Value; }),
            setString: static (target, set) => IO.lift(() => target.Strings.SetString(set.Key.Key, set.Value)).Map(static _ => unit),
            delete: static (target, delete) => IO.lift(() => target.Strings.Delete(delete.Key.Key)),
            deleteNamedView: static (target, view) => IO.lift(() => Refused.Unless(target.AllNamedViews.Delete(view.Index), nameof(File3dmViewTable.Delete))),
            modelUnits: static (target, units) => IO.lift(() => { target.Settings.ModelUnits = units.Unit; }),
            earthAnchor: static (target, anchor) => DocumentUnits.ModifyEarthAnchor(() => target.EarthAnchorPoint, point => target.EarthAnchorPoint = point, anchor.Edit),
            addObject: static (target, add) => IO.lift(() =>
                Answers.NonEmpty(target.Objects.Add(add.Geometry, add.Attributes.ValueUnsafe()), nameof(File3dmObjectTable.Add)).Map(static _ => unit)),
            deleteObject: static (target, delete) => IO.lift(() => Refused.Unless(target.Objects.Delete(delete.Id), nameof(File3dmObjectTable.Delete))),
            embedFile: static (target, embed) =>
                from existing in Answers.ExistingPath(embed.Path)
                from added in IO.lift(() => Refused.Unless(target.EmbeddedFiles.Add(existing), nameof(File3dmEmbeddedFiles.Add)))
                select added);

    // --- [WRITES]
    public static IO<Unit> WriteWithLog(File3dm archive, string path, File3dmWriteOptions options) =>
        from qualified in Answers.QualifiedPath(path)
        from written in IO.lift(() => archive.WriteWithLog(qualified, options, out string log) ? Fin.Succ(unit) : new ArchiveRejected(nameof(File3dm.WriteWithLog), log))
        select written;

    public static IO<byte[]> ToByteArray(File3dm archive, File3dmWriteOptions options) =>
        IO.lift(() => Missing.Unless(archive.ToByteArray(options), nameof(File3dm.ToByteArray)));

    // --- [EMBEDDED_FILES]
    public static IO<Seq<string>> ExtractEmbeddedFiles(File3dm archive, string folder) =>
        from qualified in Answers.QualifiedPath(folder)
        let rows = toSeq(archive.EmbeddedFiles).Map(embedded => (Embedded: embedded, Target: Path.Combine(qualified, Path.GetFileName(embedded.Filename)))).Strict()
        from distinct in IO.lift(() => Answers.Unique(rows.Map(static row => row.Target), static (target, count) => new DuplicateTarget(target, count)).ToFin())
        from written in rows
            .TraverseM(static row => IO.lift(() => Refused.Unless(row.Embedded.SaveToFile(row.Target), nameof(File3dmEmbeddedFile.SaveToFile))).Map(_ => row.Target))
            .As()
        select written;
}
