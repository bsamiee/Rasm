using Rasm.Rhino.Document.Files;
using Rasm.Testing;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Xunit;

namespace Rasm.Rhino.Tests.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DocumentsTests(Fixtures fixtures) {

    // --- [SCOPES]

    private static readonly Error Refused = Error.New("body refused");

    private static IO<Fixture<RhinoDoc>> Headless { get; } =
        from doc in IO.lift(static () => Conversions.Required(RhinoDoc.CreateHeadless(file3dmTemplatePath: null))).OnUiThread()
        select new Fixture<RhinoDoc>(doc, _ => IO.lift(doc.Dispose).OnUiThread());

    [Fact]
    public async Task ArchiveScopeReadsThePersistedModelAndClosesItsDocument() {
        (uint serial, Fin<Point3d[]> read) = await ArchivedAsync(static points => IO.pure(points));
        Assert.Equal([new Point3d(1.5, -2.25, 4)], read.ThrowIfFail());
        Assert.False(await OpenAsync(serial));
    }

    [Fact]
    public async Task ArchiveScopeClosesItsDocumentWhenTheBodyFailsAndKeepsTheBodyError() {
        (uint serial, Fin<Point3d[]> read) = await ArchivedAsync(static _ => IO.fail<Point3d[]>(Refused));
        Assert.Equal(Refused, Expect<Error>.Failed(read));
        Assert.False(await OpenAsync(serial));
    }

    [Fact]
    public async Task LiveScopeLeavesTheDocumentItWasGivenOpen() {
        RhinoDoc doc = await fixtures.TakeAsync(Headless);
        uint serial = doc.RuntimeSerialNumber;
        Assert.Equal(serial, await RhinoHost.RunAsync(Documents.With(new DocumentSource.Live(doc), static held => IO.pure(held.RuntimeSerialNumber))));
        Assert.True(await OpenAsync(serial));
    }

    [Fact]
    public async Task LiveScopeLeavesTheDocumentItWasGivenOpenWhenTheBodyFails() {
        RhinoDoc doc = await fixtures.TakeAsync(Headless);
        uint serial = doc.RuntimeSerialNumber;
        Assert.Equal(Refused, await Expect<Error>.FailAsync(Documents.With(new DocumentSource.Live(doc), static _ => IO.fail<uint>(Refused))));
        Assert.True(await OpenAsync(serial));
    }

    [Fact]
    public async Task HeadlessTemplateOfAnotherFormatFailsAsUnreadable() {
        DirectoryInfo folder = await fixtures.TakeAsync(Fixtures.Folder);
        string template = Path.Combine(folder.FullName, "template.txt");
        await File.WriteAllTextAsync(template, nameof(HeadlessTemplateOfAnotherFormatFailsAsUnreadable), TestContext.Current.CancellationToken);
        Assert.Equal(RhinoReason.Unreadable, (await Expect<Fault<RhinoReason>>.FailAsync(
            IO.lift(InputPath.Of(template)).Bind(static path => Documents.With(new DocumentSource.Headless(Some(path)), static doc => IO.pure(doc.RuntimeSerialNumber))))).Reason);
    }

    // --- [SCRIPTS]

    [Fact]
    public async Task ScriptedRefusesAHeadlessDocumentAsUnsupported() =>
        Assert.Equal(RhinoReason.Unsupported, (await Expect<Fault<RhinoReason>>.FailAsync(RhinoHost.Headless(static held => IO.lift(() => Documents.Scripted(held))))).Reason);

    [Fact(Skip = "configuration Headless runs Rhino without views", SkipType = typeof(RhinoHost), SkipUnless = nameof(RhinoHost.Interfaced))]
    public async Task ScriptRunsItsCommandInTheViewedDocumentItNames() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        Seq<double> lines = await RhinoHost.RunAsync((
            from view in IO.lift(() => Conversions.Required(doc.Views.Add(nameof(DocumentsTests), DefinedViewportProjection.Perspective, new System.Drawing.Rectangle(0, 0, 600, 400), floating: true)))
            from scripted in Documents.RunScript(doc, "-_Line 0,0,0 10,0,0", echo: false, display: None)
            select toSeq(doc.Objects.FindByObjectType(ObjectType.Curve)).Map(static found => ((Curve)found.Geometry).GetLength()).Strict()).OnUiThread());
        Assert.Equal(10d, Assert.Single(lines), RhinoMath.SqrtEpsilon * 10d);
    }

    // --- [ARCHIVES]

    private async Task<(uint Serial, Fin<Point3d[]> Read)> ArchivedAsync(Func<Point3d[], IO<Point3d[]>> body) {
        string archive = Path.Combine((await fixtures.TakeAsync(Fixtures.Folder)).FullName, "model.3dm");
        using (File3dm model = new()) {
            model.Objects.AddPoint(new Point3d(1.5, -2.25, 4));
            Assert.True(model.Write(archive, new File3dmWriteOptions()));
        }
        Atom<uint> serial = Atom(0u);
        Fin<Point3d[]> read = await RhinoHost.RunSafeAsync(
            IO.lift(InputPath.Of(archive)).Bind(path => Documents.With(new DocumentSource.Archive(path, None), doc =>
                IO.lift(() => {
                    serial.Swap(_ => doc.RuntimeSerialNumber);
                    return doc.Objects.Select(static held => ((Point)held.Geometry).Location).ToArray();
                }).Bind(body))));
        Assert.NotEqual(0u, serial.Value);
        return (serial.Value, read);
    }

    private static Task<bool> OpenAsync(uint serial) => RhinoHost.RunAsync(IO.lift(() => RhinoDoc.FromRuntimeSerialNumber(serial) is not null).OnUiThread());
}
