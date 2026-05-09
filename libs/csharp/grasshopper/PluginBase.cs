using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper2.Framework;
using Grasshopper2.UI;
namespace Grasshopper;

// --- [SERVICES] --------------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass an id and a fallback
/// display name; the calling assembly (resolved via <see cref="Assembly.GetCallingAssembly"/>,
/// pinned with <see cref="MethodImplOptions.NoInlining"/> to defeat JIT inlining) supplies the
/// title/description/version metadata at base construction. Author/Copyright at runtime read
/// from <see cref="Plugin.Assembly"/>, which Grasshopper2 populates after construction.
/// Subclasses bind <see cref="Plugin.Icon"/> in their own constructor.
/// </summary>
public abstract class PluginBase : Plugin {
    private readonly string fallbackName;

#pragma warning disable IDE0290 // primary constructor cannot carry MethodImpl(NoInlining)
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected PluginBase(Guid id, string fallbackName) : base(
        id: id,
        nomen: new Nomen(
            Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? fallbackName,
            Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty),
        version: Assembly.GetCallingAssembly().GetName().Version) =>
        this.fallbackName = fallbackName;
#pragma warning restore IDE0290
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? fallbackName;
    public override string Copyright =>
        Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
}
