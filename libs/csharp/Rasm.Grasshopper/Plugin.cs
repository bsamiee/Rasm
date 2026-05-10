using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI;
using GhPlugin = Grasshopper2.Framework.Plugin;
namespace Grasshopper;

// --- [SERVICES] --------------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass an id and a fallback
/// display name; the calling assembly (resolved via <see cref="Assembly.GetCallingAssembly"/>,
/// pinned with <see cref="MethodImplOptions.NoInlining"/> to defeat JIT inlining) supplies the
/// title/description/version metadata at base construction. Author/Copyright at runtime read
/// from <see cref="GhPlugin.Assembly"/>, which Grasshopper2 populates after construction.
/// Subclasses bind <see cref="GhPlugin.Icon"/> in their own constructor.
/// </summary>
public abstract class Plugin : GhPlugin {
    private readonly string fallbackName;

    [SuppressMessage(category: "Style", checkId: "IDE0290:Use primary constructor", Justification = "MethodImpl(NoInlining) cannot decorate primary constructor parameters.")]
    [BoundaryImperativeExemption(
        ruleId: "IDE0290",
        reason: BoundaryImperativeReason.ProtocolRequired,
        ticket: "RASM-PLUGIN-CTOR",
        expiresOnUtc: "2027-12-31T00:00:00Z")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected Plugin(Guid id, string fallbackName) : base(
        id: id,
        nomen: new Nomen(
            Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? fallbackName,
            Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty),
        version: Assembly.GetCallingAssembly().GetName().Version) =>
        this.fallbackName = fallbackName;
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? fallbackName;
    public override string Copyright =>
        Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
}
