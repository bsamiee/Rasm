namespace Lab.F10;

[Union]
internal abstract partial record UserInput {
    internal sealed record TextInput(string Input) : UserInput;
    internal sealed record NoInput : UserInput;
    internal sealed record IntegerInput(int Input) : UserInput;
    internal sealed record ConsoleError(Error Error) : UserInput;
}

internal static class Input {
    public static IO<UserInput> Read(Func<string> console) =>
        IO.lift(() => Classify(console()))
            .Catch(static error => error.IsExceptional, static error => IO.pure<UserInput>(new UserInput.ConsoleError(error)));
    public static UserInput Classify(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? new UserInput.NoInput()
            : parseInt(text).Match<UserInput>(Some: static value => new UserInput.IntegerInput(value), None: () => new UserInput.TextInput(text));
}
