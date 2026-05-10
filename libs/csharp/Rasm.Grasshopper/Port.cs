using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
namespace Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IPort {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public Type Type { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
    public bool IsIndex { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    Access Access,
    Requirement Requirement,
    bool IsIndex = false) : IPort {
    public Type Type => typeof(TVal);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info) =>
        new(Name: name, Code: code, Info: info, Access: Access.Item, Requirement: Requirement.MustExist);
    public static Port<TVal> Optional<TVal>(string name, string code, string info) =>
        new(Name: name, Code: code, Info: info, Access: Access.Item, Requirement: Requirement.MayBeMissing);
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist) =>
        new(Name: name, Code: code, Info: info, Access: Access.Twig, Requirement: requirement);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        new(Name: name, Code: code, Info: info, Access: Access.Item, Requirement: Requirement.MayBeMissing, IsIndex: true);
}
