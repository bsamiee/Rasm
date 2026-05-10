using Grasshopper2.UI;
using GhPlugin = Grasshopper2.Framework.Plugin;
namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass explicit manifest metadata
/// and bind <see cref="GhPlugin.Icon"/> in their own constructor.
/// </summary>
public abstract class Plugin(
    Guid id,
    Nomen nomen,
    Version? version,
    string author,
    string copyright) : GhPlugin(
        id: id,
        nomen: nomen,
        version: version) {
    public override string Author =>
        author;
    public override string Copyright =>
        copyright;
}
