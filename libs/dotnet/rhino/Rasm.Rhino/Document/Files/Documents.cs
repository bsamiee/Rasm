using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record DocumentSource {
    public sealed record Live(RhinoDoc Doc) : DocumentSource;

    public sealed record Active() : DocumentSource;

    public sealed record Keyed(uint Serial) : DocumentSource;

    public sealed record Opened(InputPath Path) : DocumentSource;

    public sealed record Headless(Option<InputPath> Template) : DocumentSource;

    public sealed record Archive(InputPath Path, Option<ArchivableDictionary> Options) : DocumentSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Documents {
    // --- [SOURCES]
    private const string WorksessionExtension = ".rws";

    public static IO<T> With<T>(DocumentSource source, Func<RhinoDoc, IO<T>> body) =>
        (from doc in source.Switch(
                 live: static live => IO.pure(live.Doc),
                 active: static _ => IO.lift(static () => Conversions.Required(RhinoDoc.ActiveDoc)),
                 keyed: static keyed => IO.lift(() => Conversions.Required(RhinoDoc.FromRuntimeSerialNumber(keyed.Serial))),
                 opened: static opened => IO.lift(() => Conversions.Required(
                     string.Equals(Path.GetExtension(opened.Path), WorksessionExtension, StringComparison.OrdinalIgnoreCase)
                         ? Worksession.Open(opened.Path)
                         : RhinoDoc.Open(opened.Path, out bool _))),
                 headless: static headless => headless.Template.Map(static template => (string)template) switch {
                     var template => Owned(IO.lift(() => Conversions.Required(RhinoDoc.CreateHeadless(Conversions.Unset(template))))
                         .MapFail(error => error.HasException<ArgumentException>() ? new RhinoFault(Conversions.Unset(template), RhinoReason.Unreadable, Inner: error) : error)),
                 },
                 archive: static archive => Owned(IO.lift(() => Conversions.Required(RhinoDoc.OpenHeadless(archive.Path, archive.Options.ValueUnsafe())))))
             .OnUiThread().Uninterruptible()
         from value in body(doc)
         select value).Bracket();

    private static IO<RhinoDoc> Owned(IO<RhinoDoc> open) =>
        use(open, static held => IO.lift(held.Dispose).OnUiThread());

    // --- [ARRIVALS]
    public static readonly HostEvent<uint> Arrived =
        Subscriptions.Merged(
            Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.NewDocument += h, static h => RhinoDoc.NewDocument -= h).Choose(static args => Some(args.DocumentSerialNumber)),
            Subscriptions.On<DocumentOpenEventArgs>(static h => RhinoDoc.EndOpenDocument += h, static h => RhinoDoc.EndOpenDocument -= h)
                .Choose(static args => Conversions.Present(!args.Merge && !args.Reference, args.DocumentSerialNumber))) switch {
                    var arrivals => arrivals with {
                        Attach = deliver =>
                            from attached in arrivals.Attach(deliver)
                            from opened in IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments()))
                            from delivered in Products.Each(opened.Map(doc => IO.pure(doc.RuntimeSerialNumber).Bind(deliver)))
                            select unit,
                    },
                };

    // --- [SCRIPTS]
    public static Fin<RhinoDoc> Scripted(RhinoDoc doc) =>
        Conversions.Required(RhinoDoc.FromRuntimeSerialNumber(doc.RuntimeSerialNumber))
            .Bind(static open => (
                    RhinoFault.Unless(!Command.InCommand() || Command.InScriptRunnerCommand(), RhinoReason.Unavailable, Command.InCommand()),
                    RhinoFault.Unless(!open.IsHeadless, RhinoReason.Unsupported, open.IsHeadless))
                .Apply((_, _) => open)
                .As());

    public static IO<Unit> RunScript(RhinoDoc doc, string script, bool echo, Option<string> display) =>
        IO.lift(() => Scripted(doc)).Bind(open => Products.Accepted(() => RhinoApp.RunScript(open.RuntimeSerialNumber, script, Conversions.Unset(display), echo)));
}
