using System.Drawing;
using Rasm.Rhino.Document;
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
    public sealed record Set(Action<File3dm> Write) : ArchiveOp;

    public sealed record SetString(DocumentTextKey Key, string Value) : ArchiveOp;

    public sealed record Delete(DocumentTextKey Key) : ArchiveOp;

    public sealed record DeleteNamedView(int Index) : ArchiveOp;

    public sealed record EarthAnchor(Func<EarthAnchorPoint, IO<Unit>> Edit) : ArchiveOp;

    public sealed record AddObject(GeometryBase Geometry, Option<ObjectAttributes> Attributes) : ArchiveOp;

    public sealed record DeleteObject(Guid Id) : ArchiveOp;

    public sealed record EmbedFile(string Path) : ArchiveOp;
}

public sealed record RevisionHistoryState(Option<string> CreatedBy, Option<string> LastEditedBy, int Revision, Option<DateTime> CreatedOn, Option<DateTime> LastEditedOn);

public sealed record ApplicationDataState(Option<string> ApplicationName, Option<string> ApplicationUrl, Option<string> ApplicationDetails);

public sealed record EarthAnchorState(
    Option<(double EarthBasepointLatitude, double EarthBasepointLongitude, double EarthBasepointElevation, EarthCoordinateSystem EarthBasepointElevationCoordinateSystem)> Earth,
    Option<(Point3d ModelBasePoint, Vector3d ModelNorth, Vector3d ModelEast)> Model,
    Option<string> Name,
    Option<string> Description);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Archives {
    // --- [SCOPES]
    public static IO<TValue> WithArchive<TValue>(ArchiveSource source, Func<File3dm, IO<TValue>> body) =>
        DisposalOps.Using(
            source.Switch(
                file: static file => FromFile(file.Path, qualified => file.Filters.Match(
                    Some: filters => Optional(File3dm.ReadWithLog(qualified, filters.Tables, filters.Objects, out string log)).ToFin(new ArchiveRejected(nameof(File3dm.ReadWithLog), log)),
                    None: () => Optional(File3dm.ReadWithLog(qualified, out string log)).ToFin(new ArchiveRejected(nameof(File3dm.ReadWithLog), log)))),
                bytes: static bytes => IO.lift(() => Missing.Unless(File3dm.FromByteArray(bytes.Data.ToArray()), nameof(File3dm.FromByteArray)))),
            body);

    private static IO<TValue> FromFile<TValue>(string path, Func<string, Fin<TValue>> read) =>
        Answers.QualifiedPath(path)
            .Bind(qualified => IO.lift(() => read(qualified)))
            .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.fail<TValue>(new Missing(nameof(File.Exists))));

    // --- [HEADERS]
    public static IO<Option<string>> ReadNotes(string path) =>
        FromFile<Option<string>>(path, static qualified => Answers.Present(File3dm.ReadNotes(qualified)));

    public static IO<int> ReadArchiveVersion(string path) =>
        FromFile<int>(path, static qualified => File3dm.ReadArchiveVersion(qualified));

    public static IO<RevisionHistoryState> ReadRevisionHistory(string path) =>
        from qualified in Answers.QualifiedPath(path)
        from history in IO.lift(() => Refused.Unless(
            File3dm.ReadRevisionHistory(qualified, out string createdBy, out string lastEditedBy, out int revision, out DateTime createdOn, out DateTime lastEditedOn),
            new RevisionHistoryState(
                Answers.Present(createdBy),
                Answers.Present(lastEditedBy),
                revision,
                Some(createdOn).Filter(static date => date != DateTime.MinValue),
                Some(lastEditedOn).Filter(static date => date != DateTime.MinValue)),
            nameof(File3dm.ReadRevisionHistory)))
        select history;

    public static IO<ApplicationDataState> ReadApplicationData(string path) =>
        FromFile<ApplicationDataState>(path, static qualified => {
            File3dm.ReadApplicationData(qualified, out string name, out string url, out string details);
            return new ApplicationDataState(Answers.Present(name), Answers.Present(url), Answers.Present(details));
        });

    public static IO<EarthAnchorState> ReadEarthAnchorPoint(string path) =>
        from qualified in Answers.QualifiedPath(path)
        from anchor in DisposalOps.Using(
            IO.lift(() => Missing.Unless(File3dm.ReadEarthAnchorPoint(qualified), nameof(File3dm.ReadEarthAnchorPoint))),
            static anchor => IO.lift(() => new EarthAnchorState(
                anchor.EarthLocationIsSet()
                    ? Some((anchor.EarthBasepointLatitude, anchor.EarthBasepointLongitude, anchor.EarthBasepointElevation, anchor.EarthBasepointElevationCoordinateSystem))
                    : Option<(double, double, double, EarthCoordinateSystem)>.None,
                anchor.ModelLocationIsSet() ? Some((anchor.ModelBasePoint, anchor.ModelNorth, anchor.ModelEast)) : Option<(Point3d, Vector3d, Vector3d)>.None,
                Answers.Present(anchor.Name),
                Answers.Present(anchor.Description))))
        select anchor;

    public static IO<Seq<TRow>> ReadPageViews<TRow>(string path, Func<ViewInfo, IO<TRow>> project) =>
        Answers.ExistingPath(path).Bind(existing => TableOps.ReadRows(() => File3dm.ReadPageViews(existing), project));

    public static IO<Seq<TRow>> ReadDimensionStyles<TRow>(string path, Func<DimensionStyle, IO<TRow>> project) =>
        FromFile(path, static qualified => Missing.Unless(File3dm.ReadDimensionStyles(qualified), nameof(File3dm.ReadDimensionStyles)))
            .Bind(styles => TableOps.ReadRows(() => styles, project));

    // --- [PREVIEW]
    public static IO<Option<Bitmap>> ReadPreviewImage(string path) =>
        FromFile<Option<Bitmap>>(path, static qualified => Optional(File3dm.ReadPreviewImage(qualified)));

    public static IO<Option<Bitmap>> GetPreviewImage(File3dm archive) =>
        IO.lift(() => Optional(archive.GetPreviewImage()));

    public static IO<Unit> SetPreviewImage(File3dm archive, Option<Bitmap> image) =>
        IO.lift(() => archive.SetPreviewImage(image.ValueUnsafe()));

    // --- [EDITS]
    public static IO<Unit> ApplyArchiveOp(File3dm archive, ArchiveOp op) =>
        op.Switch(
            archive,
            set: static (target, set) => IO.lift(() => set.Write(target)),
            setString: static (target, text) => IO.lift(() => text.Key.Switch(
                (target.Strings, text.Value),
                flat: static (scope, flat) => scope.Strings.SetString(flat.Value, scope.Value),
                section: static (scope, section) => scope.Strings.SetString(section.Name, section.Entry, scope.Value))).Map(static _ => unit),
            delete: static (target, delete) => IO.lift(() => delete.Key.Switch(
                target.Strings,
                flat: static (strings, flat) => strings.Delete(flat.Value),
                section: static (strings, section) => strings.Delete(section.Name, section.Entry))),
            deleteNamedView: static (target, view) => IO.lift(() => Refused.Unless(target.AllNamedViews.Delete(view.Index), nameof(File3dmViewTable.Delete))),
            earthAnchor: static (target, anchor) => DisposalOps.Using(() => target.EarthAnchorPoint, point =>
                from edited in anchor.Edit(point)
                from written in IO.lift(() => { target.EarthAnchorPoint = point; })
                select written),
            addObject: static (target, add) => IO.lift(() =>
                Answers.Required(target.Objects.Add(add.Geometry, add.Attributes.ValueUnsafe()), nameof(File3dmObjectTable.Add)).Map(static _ => unit)),
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
        from distinct in IO.lift(() => Answers.Unique(rows.Map(static row => row.Target), StringComparer.OrdinalIgnoreCase, nameof(File3dmEmbeddedFile.SaveToFile)).ToFin())
        from written in rows
            .TraverseM(static row => IO.lift(() => Refused.Unless(row.Embedded.SaveToFile(row.Target), nameof(File3dmEmbeddedFile.SaveToFile))).Map(_ => row.Target))
            .As()
        select written;
}
