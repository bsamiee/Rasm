using System.Reflection;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Styling;
using Rhino.FileIO;
using Rhino.PlugIns;

namespace Rasm.Grasshopper;

// --- [COMPOSITION] ---------------------------------------------------------------------

public static class PluginShell {

    // --- [PLUG_INS]

    public static (Action OnLoaded, IIcon Icon) Plugin<TDefinition>(Plugin host) where TDefinition : IDefined<PlugInDefinition> =>
        Hosted(host, typeof(TDefinition).Assembly, TDefinition.Definition);

    public static (Action OnLoaded, IIcon Icon) Import<TDefinition>(Plugin host) where TDefinition : IDefined<FileDefinition<FileReadOptions>> =>
        Hosted(host, typeof(TDefinition).Assembly, TDefinition.Definition.PlugIn);

    public static (Action OnLoaded, IIcon Icon) Export<TDefinition>(Plugin host) where TDefinition : IDefined<FileDefinition<FileWriteOptions>> =>
        Hosted(host, typeof(TDefinition).Assembly, TDefinition.Definition.PlugIn);

    public static (Action OnLoaded, IIcon Icon) Render<TDefinition>(Plugin host) where TDefinition : IDefined<RendererDefinition> =>
        Hosted(host, typeof(TDefinition).Assembly, TDefinition.Definition.PlugIn);

    private static (Action OnLoaded, IIcon Icon) Hosted(Plugin host, Assembly assembly, PlugInDefinition definition) =>
        (() => Toolbars.Identity(assembly)
                .Bind(static id => InputPath.Of(PlugIn.PathFromId(id)))
                .Bind(static path => PlugInRegistry.LoadPlugIn(path).RunSafe())
                .Bind(_ => Session.RunSafe(
                    (from errors in IO.lift(() => toSeq(assembly.GetTypes())
                             .Filter(static type => type is { IsAbstract: false, IsVisible: false, ContainsGenericParameters: false }
                                 && type.IsAssignableTo(typeof(IDocumentObject)) && type.GetConstructor(Type.EmptyTypes) is not null)
                             .Filter(type => !ObjectProxies.RegisterObject(type, host))
                             .Strict())
                         .Bind(static declined => IO.lift(Fault.Unless(declined.IsEmpty, RhinoReason.Declined, declined)))
                         .Cons(definition.Grasshopper)
                         .Map<K<IO, Unit>>(static step => step).Fails().As()
                     from loaded in guard(errors.IsEmpty, Error.Many(errors))
                     select unit).BracketFail().Scoped(),
                    CancellationToken.None))
                .ThrowIfFail(),
            Optional(AbstractIcon.FromResource(IconResource.Mark, assembly)).IfNone(() => StandardIcons.LetterDisc(host.Name)));
}
