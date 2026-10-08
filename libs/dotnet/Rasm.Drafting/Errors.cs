namespace Rasm.Drafting;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidDrafting = 1,
    MissingTitleField = 2,
    InvalidTitleField = 3,
    MalformedName = 4,
    SheetNumberCollision = 5,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidDrafting() : Expected("drafting value rejected", (int)Codes.InvalidDrafting), IValidationError<InvalidDrafting> {
    public static InvalidDrafting Create(string message) => new();
}

public sealed record MissingTitleField(TitleField Field) : Expected("title-block field has no entry", (int)Codes.MissingTitleField);

public sealed record InvalidTitleField : Expected {
    public InvalidTitleField(TitleField field, Error cause) : base("title-block field entry is invalid", (int)Codes.InvalidTitleField, cause) => Field = field;

    public TitleField Field { get; }
}

public sealed record MalformedName(NamingStandard Standard, string Text) : Expected("text is no name under the naming standard", (int)Codes.MalformedName);
public sealed record SheetNumberCollision(int Index, StandardName Number) : Expected("renumbered sheet number repeats another sheet number", (int)Codes.SheetNumberCollision);
