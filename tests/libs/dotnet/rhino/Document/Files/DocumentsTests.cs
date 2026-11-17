using Rasm.Rhino.Document.Files;
using Rasm.Testing;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DocumentsTests(Fixtures fixtures) {

    // --- [SCOPES]

    [Fact]
    public async Task HeadlessScopeClosesTheDocumentItOpened() =>
        Assert.False(await RhinoHost.RunAsync(
            from serial in Documents.With(new DocumentSource.Headless(None), static doc => IO.pure(doc.RuntimeSerialNumber))
            from open in IO.lift(() => RhinoDoc.FromRuntimeSerialNumber(serial) is not null).OnUiThread()
            select open));

    [Fact]
    public async Task LiveScopeLeavesTheDocumentItWasGivenOpen() =>
        Assert.True(await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from scoped in Documents.With(new DocumentSource.Live(doc), static held => IO.pure(held.RuntimeSerialNumber))
            from open in IO.lift(() => RhinoDoc.FromRuntimeSerialNumber(scoped) is not null).OnUiThread()
            select open)));

    [Fact]
    public async Task HeadlessTemplateOfAnotherFormatFailsAsUnreadable() {
        DirectoryInfo folder = await fixtures.TakeAsync(Fixtures.Folder);
        string template = Path.Combine(folder.FullName, "template.txt");
        await File.WriteAllTextAsync(template, nameof(HeadlessTemplateOfAnotherFormatFailsAsUnreadable), TestContext.Current.CancellationToken);
        Assert.Equal(RhinoReason.Unreadable, (await Expect<RhinoFault>.FailAsync(
            IO.lift(InputPath.Of(template)).Bind(static path => Documents.With(new DocumentSource.Headless(Some(path)), static doc => IO.pure(doc.RuntimeSerialNumber))))).Reason);
    }

    // --- [SCRIPTS]

    [Fact]
    public async Task ScriptedAcceptsOnlyDocumentsWithViews() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        (Fin<RhinoDoc> live, Fin<RhinoDoc> headless) = await RhinoHost.RunAsync(RhinoHost.Headless(held => IO.lift(() => (Documents.Scripted(doc), Documents.Scripted(held)))));
        Assert.Equal(RhinoProcess.Mode is RhinoMode.Interface, live.IsSucc);
        Assert.Equal(RhinoReason.Unsupported, Expect<RhinoFault>.Failed(headless).Reason);
    }
}
