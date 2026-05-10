using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IPort {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public Type Type { get; }
    public Param Param { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    Param Param,
    Access Access,
    Requirement Requirement) : IPort {
    public Type Type => typeof(TVal);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info) =>
        Required<TVal>(param: Param.From(type: typeof(TVal)).IfNone(Param.Generic), name: name, code: code, info: info);
    public static Port<TVal> Required<TVal>(Param param, string name, string code, string info) =>
        new(Name: name, Code: code, Info: info, Param: param, Access: Access.Item, Requirement: Requirement.MustExist);
    public static Port<TVal> Optional<TVal>(string name, string code, string info) =>
        Optional<TVal>(param: Param.From(type: typeof(TVal)).IfNone(Param.Generic), name: name, code: code, info: info);
    public static Port<TVal> Optional<TVal>(Param param, string name, string code, string info) =>
        new(Name: name, Code: code, Info: info, Param: param, Access: Access.Item, Requirement: Requirement.MayBeMissing);
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist) =>
        List<TVal>(param: Param.From(type: typeof(TVal)).IfNone(Param.Generic), name: name, code: code, info: info, requirement: requirement);
    public static Port<TVal> List<TVal>(Param param, string name, string code, string info, Requirement requirement = Requirement.MustExist) =>
        new(Name: name, Code: code, Info: info, Param: param, Access: Access.Twig, Requirement: requirement);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        new(Name: name, Code: code, Info: info, Param: Param.Index, Access: Access.Item, Requirement: Requirement.MayBeMissing);
}
