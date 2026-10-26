namespace Rasm.Drafting;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DraftingFault(string Message) : Expected(Message, 0), IValidationError<DraftingFault> {
    public static DraftingFault Create(string message) => new(message);
}
