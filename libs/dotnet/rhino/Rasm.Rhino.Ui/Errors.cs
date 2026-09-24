using Rasm.Rhino.Document;

namespace Rasm.Rhino.Ui;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NameTaken(string Name) : Expected("Named callback {Name} is already registered", ErrorOps.Code<NameTaken>()) {
    public static Fin<Unit> Unless(bool free, string name) => free ? unit : new NameTaken(name);
}

public sealed record ParameterMissing(string Name) : Expected("Required named parameter {Name} is missing or has another type", ErrorOps.Code<ParameterMissing>()) {
    public static Fin<Unit> Unless(bool present, string name) => present ? unit : new ParameterMissing(name);
}

public sealed record WindowsOnly(string Member) : Expected("{Member} runs only on Windows", ErrorOps.Code<WindowsOnly>()) {
    public static Fin<Unit> Unless(bool windows, string member) => windows ? unit : new WindowsOnly(member);
}

public sealed record ManyFullHeightSections(int Sections) : Expected("{Sections} sections request full height where the holder accepts one", ErrorOps.Code<ManyFullHeightSections>()) {
    public static Fin<Unit> Unless(bool single, int sections) => single ? unit : new ManyFullHeightSections(sections);
}
