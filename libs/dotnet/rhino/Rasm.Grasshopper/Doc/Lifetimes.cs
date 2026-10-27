using System.Security;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lifetimes {
    // --- [FILES]
    public static IO<Document> Load(string path, Seq<Document> blockingList, bool trackFiles, bool resolvePlugins) =>
        (from blocking in atomIO(Option<Document>.None)
         from opened in IO.lift(() => {
             DocumentIO io = new(document: null, trackFiles, reportErrors: false, retainRoot: false, resolvePlugins);
             return io.Open(path, blockingList, found => blocking.Swap(_ => Some(found))) ? Some(io.Document) : None;
         })
         from document in IO.lift(() => (opened | blocking.Value).ToFin(new GrasshopperFault(path, GrasshopperReason.Unreadable)))
         select document)
        .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), error => IO.fail<Document>(new GrasshopperFault(path, GrasshopperReason.Missing, Inner: error)))
        .Catch(
            static error => error.HasException<UnauthorizedAccessException>() || error.HasException<SecurityException>() || error.HasException<IOException>() || error.HasException<InvalidDataException>(),
            error => IO.fail<Document>(new GrasshopperFault(path, GrasshopperReason.Unreadable, Inner: error)));

    public static IO<Document> Open(DocumentBag bag, string path) =>
        from roots in IO.lift(() => toSeq(bag.All.Select(static stack => stack.Root)))
        from document in Load(path, roots, trackFiles: true, resolvePlugins: true)
        from pushed in IO.lift(() => GrasshopperFault.Unless(bag.Push(document)))
        select document;

    public static IO<Document> Recover(string path) =>
        from document in Load(Path.ChangeExtension(path, DocumentIO.G2AutoSaveExtension), Seq<Document>(), trackFiles: false, resolvePlugins: true)
        from restored in IO.lift(() => {
            document.File.Path = path;
            document.File.FileOverwriteWarning = null;
            document.Modify();
        })
        select document;

    // --- [NODES]
    public static IO<byte[]> Store(Document document, FileContents contents) =>
        from node in IO.lift(static () => GrasshopperIO.DataBase.Node.Create("Document"))
        from stored in IO.lift(() => document.Store(node, contents))
        from bytes in IO.lift(() => GrasshopperIO.IO.WriteNodeToByteArray(node, pathHint: null))
        select bytes;

    public static IO<Document> Read(byte[] bytes) =>
        IO.lift(() => new Document(GrasshopperIO.IO.ReadNodeFromByteArray(bytes, pathHint: null)));

    // --- [RELEASE]
    public static IO<Unit> Pop(DocumentBag bag, Document doc, bool accept) =>
        from root in IO.lift(() => bag.All.Any(stack => stack.Head == doc && stack.Root == doc))
        from hidden in when(root, IO.lift(() => { doc.Display.Enabled = false; })).As()
        from popped in IO.lift(() => GrasshopperFault.Unless(bag.Pop(doc, accept)))
        select unit;
}
