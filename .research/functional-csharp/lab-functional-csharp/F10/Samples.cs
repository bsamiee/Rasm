namespace Lab.F10;

internal static class Samples {
    public static Fin<Unit> Run() =>
        OfferingsSample()
            .Bind(static _ => LookupSample())
            .Bind(static _ => InputSample());

    private static Fin<Unit> OfferingsSample() {
        Location paris = new("Paris");
        Location airport = new("LHR");
        CustomerOffering.Holiday holiday = new() { Id = 1, Destination = paris, DepartureAirport = airport, StartDate = DateTime.UnixEpoch, DurationOfStay = 7 };
        CustomerOffering.DayTrip trip = new() { Id = 3, DateOfTrip = DateTime.UnixEpoch, Attraction = paris, CoachTripRequired = true };
        Seq<CustomerOffering> offerings = Seq<CustomerOffering>(holiday, trip);
        FlaggedCustomer flagged = new("g@example.com", IsRegistered: false, Name: "", IsEligible: false);
        Seq<Customer> customers = Seq<Customer>(new Customer.RegisteredCustomer("Ada", "ada@example.com", IsEligible: true), new Customer.GuestCustomer("g@example.com"));
        return Check(
            nameof(OfferingsSample),
            ("Holiday", string.Equals(Offerings.Format(holiday), "Paris, 7 nights", StringComparison.Ordinal)),
            ("DayTrip", string.Equals(Offerings.Format(trip), "Paris", StringComparison.Ordinal)),
            ("Collection", offerings.Count == 2),
            ("Flagged", !flagged.IsRegistered),
            ("Registered", customers.Exists(static c => c is Customer.RegisteredCustomer { IsEligible: true })),
            ("Guest", customers.Exists(static c => c is Customer.GuestCustomer)));
    }

    private static Fin<Unit> LookupSample() {
        Fin<string> found = Greeting.Describe(People.GetPerson(Database, 1)).RunSafe();
        Fin<string> missing = Greeting.Describe(People.GetPerson(Database, 2)).RunSafe();
        Fin<string> failed = Greeting.Describe(People.GetPerson(Database, 13)).RunSafe();
        Fin<Unit> sent = Mail.SendEmail(Transport, "ada@example.com").RunSafe();
        Fin<Unit> bounced = Mail.SendEmail(Transport, "").RunSafe();
        return Check(
            nameof(LookupSample),
            ("Found", found == Pure("Ada")),
            ("Missing", missing == Pure("no such person")),
            ("Failed", failed.Match(Succ: static _ => false, Fail: static e => e.IsExceptional)),
            ("Sent", sent.IsSucc),
            ("Bounced", bounced.Match(Succ: static _ => false, Fail: static e => e.IsExceptional)));
    }

    private static Fin<Unit> InputSample() {
        Fin<UserInput> number = Input.Read(static () => "42").RunSafe();
        Fin<UserInput> blank = Input.Read(static () => "  ").RunSafe();
        Fin<UserInput> text = Input.Read(static () => "abc").RunSafe();
        Fin<UserInput> broken = Input.Read(static () => throw new IOException("console closed")).RunSafe();
        return Check(
            nameof(InputSample),
            ("Integer", number.Exists(static input => input is UserInput.IntegerInput { Input: 42 })),
            ("NoInput", blank.Exists(static input => input is UserInput.NoInput)),
            ("Text", text.Exists(static input => input is UserInput.TextInput t && string.Equals(t.Input, "abc", StringComparison.Ordinal))),
            ("ConsoleError", broken.Exists(static input => input is UserInput.ConsoleError)),
            ("Blank", Input.Classify(" ") is UserInput.NoInput));
    }

    private static Person? Database(int id) => id switch {
        1 => new Person(1, "Ada"),
        13 => throw new InvalidOperationException("database offline"),
        _ => null,
    };

    private static void Transport(string address) => ArgumentException.ThrowIfNullOrEmpty(address);

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
