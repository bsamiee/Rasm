using System.Collections.Generic;
using System.Security;
using Grasshopper2.Components.Standard;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.UI.Canvas;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Solutions {
    // --- [RUNS]
    public static IO<Solution> Run(Document document, SolutionMode mode) =>
        (from source in use(static env => CancellationTokenSource.CreateLinkedTokenSource(env.Token))
         from token in cancelToken
         from faults in atomIO(HashMap<SolutionId, Error>())
         from watched in Hooks.On<SolutionExceptionEventArgs>(
             h => document.Solution.SolutionFaulted += h, h => document.Solution.SolutionFaulted -= h,
             args => faults.SwapIO(held => held.AddOrUpdate(args.Solution.Id, Error.New(args.Exception))).Map(static _ => unit))
         from solution in document.State is DocumentState.Inert
             ? IO.lift(() => document.Solution.StartWait(source, mode))
             : IO.liftAsync(() => document.Solution.Start(source, mode))
                 .Catch(static error => error.Is(Errors.Cancelled), _ => IO.fail<Solution>(Cancellation(nameof(SolutionServer.Start), token)))
         from owned in IO.lift(() => GrasshopperFault.Unless(solution.Token == source.Token, GrasshopperReason.Taken, solution.Id))
         from finished in IO.lift(() => Finished(document, solution, faults.Value.Find(solution.Id), token))
         select finished).Bracket();

    public static IO<A> Paused<A>(Seq<Document> documents, IO<A> body) =>
        from prior in IO.lift(static () => SolutionServer.EnableSolutions)
        from paused in IO.lift(() => {
            SolutionServer.EnableSolutions = false;
            _ = documents.Iter(static document => document.Solution.Stop());
        })
        from value in body.Finally(IO.lift(() => {
            SolutionServer.EnableSolutions = prior;
            if (prior) _ = documents.Iter(static document => _ = document.Solution.Start());
        }))
        select value;

    private static Fin<Solution> Finished(Document document, Solution solution, Option<Error> fault, CancellationToken token) =>
        (solution.Phase, Messages: fault.ToSeq()
            + (from member in toSeq(document.Objects.Forwards)
               where member.State?.RelevantId == solution.Id
               from message in toSeq(member.IterateOverAllMessages())
               where message.Level is MessageLevel.Error
               select (Error)new GrasshopperFault($"{member.DisplayName}: {message.Text}", GrasshopperReason.Unfinished, Inner: Optional(message.Info).Filter(static info => info.Length > 0).Map(static info => Error.New(info))))) switch {
                   (SolutionPhase.Cancelled, _) => Cancellation(solution.Id.ToString(), token),
                   (SolutionPhase.Completed, { IsEmpty: true }) => solution,
                   (var phase, var messages) => new GrasshopperFault($"solution {solution.Id} in {phase}", GrasshopperReason.Unfinished, Inner: messages.IsEmpty ? None : Some(Error.Many(messages))),
               };

    private static Error Cancellation(string subject, CancellationToken token) =>
        token.IsCancellationRequested ? Errors.Cancelled : new GrasshopperFault(subject, SolutionServer.EnableSolutions ? GrasshopperReason.Taken : GrasshopperReason.Declined);
}

public static class Documents {
    // --- [FILES]
    public static IO<Document> Load(string path, bool resolvePlugins = true, bool trackFiles = false, Seq<Document> blocking = default) =>
        (from held in atomIO(Option<Document>.None)
         from opened in IO.lift(() => {
             DocumentIO io = new(document: null, trackFiles, reportErrors: false, retainRoot: false, resolvePlugins);
             return io.Open(path, blocking, found => held.Swap(_ => Some(found))) ? Optional(io.Document) : None;
         })
         from document in IO.lift(() => (opened | held.Value).ToFin(new GrasshopperFault(path, GrasshopperReason.Unreadable)))
         select document)
        .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), error => IO.fail<Document>(new GrasshopperFault(path, GrasshopperReason.Missing, Inner: error)))
        .Catch(
            static error => error.HasException<UnauthorizedAccessException>() || error.HasException<SecurityException>() || error.HasException<IOException>() || error.HasException<InvalidDataException>(),
            error => IO.fail<Document>(new GrasshopperFault(path, GrasshopperReason.Unreadable, Inner: error)));

    public static IO<Document> Open(DocumentBag bag, string path) =>
        from roots in IO.lift(() => toSeq(bag.All).Map(static stack => stack.Root).Strict())
        from document in Load(path, trackFiles: true, blocking: roots)
        from pushed in IO.lift(() => ignore(bag.Push(document)))
        select document;

    public static IO<Seq<SessionEntry>> Recoverable =>
        IO.lift(static () => Optional(SessionFile.TryLoad()).ToSeq()
            .Bind(static record => toSeq(record.Entries))
            .Filter(static entry => entry is { WasModified: true } and not { Kind: SessionEntryKind.Unnamed, ParentEntryIndex: >= 0 } && File.Exists(entry.AutoSavePath))
            .Strict());

    public static IO<Document> Recover(DocumentBag bag, SessionEntry entry) =>
        from roots in IO.lift(() => toSeq(bag.All).Map(static stack => stack.Root).Strict())
        from document in roots.Find(root => entry.Kind is SessionEntryKind.Named && string.Equals(root.File.Path, entry.OriginalPath, StringComparison.OrdinalIgnoreCase)).Match(
            Some: static open => IO.pure(open),
            None: () =>
                from changed in IO.lift(() => entry.Kind is SessionEntryKind.Named && entry.OriginalLastWriteUtc != default && File.Exists(entry.OriginalPath)
                    && File.GetLastWriteTimeUtc(entry.OriginalPath) > entry.OriginalLastWriteUtc.AddSeconds(1))
                from loaded in changed
                    ? Load(entry.OriginalPath, trackFiles: true, blocking: roots)
                    : from saved in IO.lift(() => GrasshopperFault.Unless(File.Exists(entry.AutoSavePath), GrasshopperReason.Missing, entry.AutoSavePath))
                      from recovered in Load(entry.AutoSavePath, blocking: roots)
                      from restored in IO.lift(() => {
                          recovered.File.Path = entry.Kind is SessionEntryKind.Named ? entry.OriginalPath : null;
                          recovered.File.FileOverwriteWarning = null;
                          recovered.Modify();
                      })
                      select recovered
                from pushed in IO.lift(() => ignore(bag.Push(loaded)))
                select loaded)
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
    public static IO<Unit> Pop(DocumentBag bag, Document document, bool accept) =>
        from head in IO.lift(() => GrasshopperFault.Unless(bag.All.Any(stack => stack.Head == document), GrasshopperReason.Missing, document))
        from hidden in IO.lift(() => { document.Display.Enabled = false; })
        from popped in IO.lift(() => ignore(bag.Pop(document, accept)))
        select unit;

    // --- [PROGRAMS]
    public static IO<Map<string, ITree>> Evaluate(Document document, Map<string, ITree> inputs) =>
        (from inner in use(() => GrasshopperIO.IO.Duplicate(document), static held => held.Close())
         from host in use(static () => Document.NewInertDocument(), static held => held.Close())
         from cluster in IO.lift(() => new Cluster { InnerDocument = inner, RelayMessages = true })
         from added in IO.lift(() => GrasshopperFault.Unless(host.Objects.Add(cluster), GrasshopperReason.Declined, cluster.Nomen.Name))
         from boundary in IO.lift(() => Boundary(cluster))
         from fed in IO.lift(() => inputs.ToSeq()
             .Traverse(input => boundary.Inputs.Find(input.Key).ToFin(new GrasshopperFault(input.Key, GrasshopperReason.Missing)).Map(parameter => (Parameter: parameter, Tree: input.Value)))
             .As())
         from assigned in IO.lift(() => fed.Iter(static input => input.Parameter.PersistentDataWeak = input.Tree))
         from solved in Solutions.Run(host, SolutionMode.Headless)
         select toMap(boundary.Outputs.Choose(static output => Optional(output.Parameter.State?.Data?.Tree()).Map(tree => (output.Name, tree)))))
        .Bracket();

    private static Fin<(HashMap<string, IParameter> Inputs, Seq<(string Name, IParameter Parameter)> Outputs)> Boundary(Cluster cluster) {
        cluster.EnsureMaps(out Listen[] listens, out Shout[] shouts);
        return (Unique(Paired(listens, cluster.Parameters.Input)), Unique(Paired(shouts, cluster.Parameters.Output)))
            .Apply(static (inputs, outputs) => (toHashMap(inputs), outputs)).As();

        static Seq<(string Name, IParameter Parameter)> Paired(IEnumerable<IDocumentObject?> members, Func<int, IParameter> parameter) =>
            toSeq(members).Map(static (member, index) => (Member: Optional(member), Index: index)).Choose(slot => slot.Member.Map(found => (found.UserName, parameter(slot.Index)))).Strict();

        static Fin<Seq<(string Name, IParameter Parameter)>> Unique(Seq<(string Name, IParameter Parameter)> ports) =>
            toSeq(ports.GroupBy(static port => port.Name, StringComparer.Ordinal).Where(static named => named.Skip(1).Any()))
                .Traverse(static named => Fin.Fail<Unit>(new GrasshopperFault(named.Key, GrasshopperReason.Invalid)))
                .As()
                .Map(_ => ports);
    }
}
