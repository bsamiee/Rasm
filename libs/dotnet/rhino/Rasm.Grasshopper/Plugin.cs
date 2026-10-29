using Eto.Forms;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.Data;
using Rasm.Grasshopper.UI;
using Rhino;

namespace Rasm.Grasshopper;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record GrasshopperPlugin {
    // --- [DECLARATION]
    public required ILoggerFactory Loggers { get; init; }
    public Seq<IO<Unit>> Types { get; init; }
    public Seq<GrasshopperTheme> Themes { get; init; }
    public Seq<Command> Commands { get; init; }
    public Seq<Func<Plugin, IO<Unit>>> Lifetimes { get; init; }

    // --- [HOST]
    public static IIcon Icon(Plugin plugin) => DataTypes.Icon(plugin.Assembly, "icon") ?? StandardIcons.LetterDisc(plugin.Name);

    public void Load(Plugin plugin) =>
        (from bound in Hooks.Bind(Loggers, plugin)
         from scope in envIO
         from outcome in Types
             .Concat(Themes.Map(static theme => theme.Install))
             .Add(Chrome.Menu(Commands))
             .Concat(Lifetimes.Map(lifetime => lifetime(plugin)))
             .Map<K<IO, Unit>>(static step => step).PartitionFallible().As()
         from closing in Hooks.On(static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, IO.lift(scope.DisposeResources))
         from loaded in guard(outcome.Fails.IsEmpty, Error.Many(outcome.Fails))
         select unit).BracketFail().Run(Hooks.Scope);
}
