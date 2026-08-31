namespace Lab.F01;

internal static partial class CoreProperties {
    public static string Describe(int value) => value == 10
        ? "It was ten"
        : "It was not ten";
}

internal static partial class CoreProperties {
    public static int Add(int left, int right) => left + right;

    public static string Greeting(string? name) =>
        "Hello " + (string.IsNullOrWhiteSpace(name)
            ? "Unknown Person"
            : name);
}

internal static partial class CoreProperties {
    public static string TimestampedGreeting(DateTimeOffset now, string? name) =>
        string.Create(CultureInfo.InvariantCulture, $"{now} - Hello {name ?? "Unknown Person"}");
}

internal sealed record DoctorWho(int NumberOfStories, int CurrentDoctor, string CurrentDoctorActor, int SeasonNumber);

internal static partial class CoreProperties {
    public static DoctorWho RegenerateDoctor(DoctorWho oldState, string newActorName) =>
        oldState with {
            CurrentDoctor = oldState.CurrentDoctor + 1,
            CurrentDoctorActor = newActorName,
        };
}

internal static partial class CoreProperties {
    public static (Func<int, int, string> DescribeSum, Action<string> Log) Delegates() {
        Func<int, int, string> describeSum =
            static (x, y) => string.Create(CultureInfo.InvariantCulture, $"{x} + {y} = {x + y}");

        Action<string> log =
            static message => Console.WriteLine($"message received: {message}");
        return (describeSum, log);
    }
}
