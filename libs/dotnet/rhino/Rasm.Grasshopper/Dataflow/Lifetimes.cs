using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Dataflow;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lifetimes {
    // --- [FILES]
    public static IO<Document> Load(string path, Seq<Document> blockingList, bool trackFiles, bool resolvePlugins) =>
        Callbacks.CapturedOptional<Document>(receive => IO.lift(() => {
            DocumentIO io = new(document: null, trackFiles, reportErrors: false, retainRoot: false, resolvePlugins);
            if (io.Open(path, blockingList, found => receive(found)))
                receive(io.Document);
        }))
        .Bind(static document => IO.lift(() => document.ToFin(new Refused(nameof(DocumentIO.Open)))))
        .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), error => IO.fail<Document>(new DocumentNotFound(path, error)))
        .Catch(static error => error.HasException<InvalidDataException>(), error => IO.fail<Document>(new DocumentDamaged(path, error)));

    public static IO<Document> Open(DocumentBag bag, string path) =>
        from roots in IO.lift(() => toSeq(bag.All.Select(static stack => stack.Root)))
        from document in Load(path, roots, trackFiles: true, resolvePlugins: true)
        from _ in IO.lift(() => bag.Push(document))
        select document;

    public static IO<Document> Recover(string path) =>
        from document in Load(Path.ChangeExtension(path, DocumentIO.G2AutoSaveExtension), Seq<Document>(), trackFiles: false, resolvePlugins: true)
        from _ in IO.lift(() => { document.File.Path = path; })
        from __ in IO.lift(() => { document.File.FileOverwriteWarning = null; })
        from ___ in IO.lift(document.Modify)
        select document;

    public static IO<Unit> Save(Document document, string path, FileContents contents, bool trackFiles) =>
        IO.lift(() => Refused.Unless(new DocumentIO(document, trackFiles, reportErrors: false).Save(path, contents), nameof(DocumentIO.Save)));

    public static IO<Unit> SaveCopy(Document document, string path, FileContents contents, BackupMethod method, bool trackFiles) =>
        IO.lift(() => Refused.Unless(new DocumentIO(document, trackFiles, reportErrors: false).SaveCopy(path, contents, method), nameof(DocumentIO.SaveCopy)));

    // --- [NODES]
    public static IO<byte[]> Store(Document document, FileContents contents) =>
        from node in IO.lift(static () => GrasshopperIO.DataBase.Node.Create("Document"))
        from _ in IO.lift(() => document.Store(node, contents))
        from bytes in IO.lift(() => GrasshopperIO.IO.WriteNodeToByteArray(node, pathHint: null))
        select bytes;

    public static IO<Document> Read(byte[] bytes) =>
        from reader in IO.lift(() => Missing.Unless(GrasshopperIO.IO.ReadNodeFromByteArray(bytes, pathHint: null), nameof(GrasshopperIO.IO.ReadNodeFromByteArray)))
        select new Document(reader);

    // --- [RELEASE]
    public static void Close(Document document) {
        document.Display.Enabled = false;
        document.Close();
    }

    public static IO<Unit> Pop(DocumentBag bag, Document doc, bool accept) =>
        from root in IO.lift(() => bag.All.Any(stack => stack.Head == doc && stack.Root == doc))
        from _ in when(root, IO.lift(() => { doc.Display.Enabled = false; })).As()
        from __ in IO.lift(() => Refused.Unless(bag.Pop(doc, accept), nameof(DocumentBag.Pop)))
        select unit;
}
