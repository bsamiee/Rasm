namespace Lab.F21;

internal static class Samples {
    private static readonly Map<string, Person> People = Map(("ada", new Person("Ada", Age.Create(36))));

    public static Fin<Unit> Run() =>
        ConcernsSample()
            .Bind(static _ => BoundarySample())
            .Bind(static _ => LiftsSample())
            .Bind(static _ => ConversionsSample())
            .Bind(static _ => ErrorsSample())
            .Bind(static _ => RecoverySample())
            .Bind(static _ => GuardsSample())
            .Bind(static _ => CorrectFormsSample());

    private static Fin<Unit> ConcernsSample() {
        Runtime runtime = new(new Clock(DateTimeOffset.UnixEpoch));
        return Check(
            nameof(ConcernsSample),
            ("Find present", Concerns.Find(People, "ada").IsSome),
            ("Find absent", Concerns.Find(People, "bob").IsNone),
            ("Admit succeeds", Concerns.Admit(30).IsSucc),
            ("Admit fails", Concerns.Admit(200).IsFail),
            ("Visitor member", Concerns.Visitor("x", 7).IsRight),
            ("Visitor guest", Concerns.Visitor("x", 0).IsLeft),
            ("Register succeeds", Concerns.Register("Ada", 36).IsSuccess),
            ("Register accumulates", Concerns.Register("", 200).Match(Fail: static e => e.Count, Succ: static _ => 0) == 2),
            ("Parse captures", Concerns.Parse("x").Run().IsFail),
            ("Load present", Concerns.Load(People, "ada").RunSafe().IsSucc),
            ("Load absent", Concerns.Load(People, "bob").RunSafe().Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.NotFound))),
            ("Stamp reads runtime", Concerns.Stamp<Runtime>().Run(runtime) == Pure(DateTimeOffset.UnixEpoch)));
    }

    private static Fin<Unit> BoundarySample() =>
        Check(
            nameof(BoundarySample),
            ("Respond adult", string.Equals(Boundary.Respond("Ada", 36), "Ada", StringComparison.Ordinal)),
            ("Handle underage", Boundary.Handle("Ada", 12).Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.Underage))),
            ("Handle invalid form", Boundary.Handle("", 200).Match(Succ: static _ => false, Fail: static e => e.Count == 2)));

    private static Fin<Unit> LiftsSample() =>
        Check(
            nameof(LiftsSample),
            ("FromValue", Lifts.FromValue(1) == Pure(1)),
            ("FromError", Lifts.FromError(new TooLarge()).IsFail),
            ("Halve pure", Lifts.Halve(10) == Pure(5)),
            ("Halve fail", Lifts.Halve(500).Match(Succ: static _ => false, Fail: static e => e.IsType<TooLarge>())),
            ("Age.From", Age.From(30).Map(static a => (int)a) == Pure(30)));

    private static Fin<Unit> ConversionsSample() {
        Runtime runtime = new(new Clock(DateTimeOffset.UnixEpoch));
        return Check(
            nameof(ConversionsSample),
            ("Required", Conversions.Required(Some(1)) == Pure(1)),
            ("Required none", Conversions.Required(None).IsFail),
            ("Admitted", Conversions.Admitted(None).IsFail),
            ("Present", Conversions.Present(Age.From(200)).IsNone),
            ("Split", Conversions.Split(Age.From(1)).IsRight),
            ("Items", Conversions.Items(Some(3)) == Seq(3)),
            ("Exit", Conversions.Exit(Concerns.Register("Ada", 36)).IsSucc),
            ("Widen", Conversions.Widen(Age.From(200)).IsFail),
            ("Captured", Conversions.Captured(Concerns.Parse("4")) == Pure(4)),
            ("Ran", Conversions.Ran(Concerns.Load(People, "ada")).IsSucc),
            ("Stamped", Conversions.Stamped(runtime).IsSucc));
    }

    private static Fin<Unit> ErrorsSample() {
        Error captured = Classify.Captured("x").Match(Succ: static _ => Errors.None, Fail: static e => e);
        Error many = new InvalidAge() + new EmptyName();
        Error accumulated = Concerns.Register("", 200).Match(Fail: static e => e, Succ: static _ => Errors.None);
        return Check(
            nameof(ErrorsSample),
            ("captured is Exceptional", captured.IsType<Exceptional>() && captured.IsExceptional),
            ("Retryable exceptional", Classify.Retryable(captured)),
            ("Retryable timed out", Classify.Retryable(Errors.TimedOut)),
            ("Rejected code", Classify.Rejected(new InvalidAge())),
            ("Rejected type", Classify.Rejected(new EmptyName())),
            ("many is ManyErrors", many is ManyErrors && many.Count == 2 && !many.IsType<ManyErrors>()),
            ("many head", many.Head.HasCode(Codes.InvalidAge)),
            ("accumulated is ManyErrors", accumulated is ManyErrors && accumulated.Count == 2),
            ("AgeFaults", Classify.AgeFaults(many) == 1),
            ("Expected", new InvalidAge().IsExpected));
    }

    private static Fin<Unit> RecoverySample() {
        Fin<Age> invalid = Age.From(200);
        Person cached = new("Cache", Age.Create(1));
        return Check(
            nameof(RecoverySample),
            ("ByCode", Recovery.ByCode(invalid).Map(static a => (int)a) == Pure(0)),
            ("ByValue", Recovery.ByValue(invalid).Map(static a => (int)a) == Pure(0)),
            ("ByPredicate", Recovery.ByPredicate(invalid).Map(static a => (int)a) == Pure(0)),
            ("Cached", Recovery.Cached(Concerns.Load(People, "bob"), cached).RunSafe() == Pure(cached)),
            ("Fallback", Recovery.Fallback(Concerns.Load(People, "bob"), IO.pure(cached)).RunSafe() == Pure(cached)),
            ("Rebound", Recovery.Rebound(invalid).Map(static a => (int)a) == Pure(0)),
            ("Rebound other", Recovery.Rebound(new Underage()).IsFail),
            ("WithContext", Recovery.WithContext(invalid).Match(Succ: static _ => false, Fail: static e => e.Inner.IsSome)),
            ("AtHost", Recovery.AtHost(new TooLarge()) == -1));
    }

    private static Fin<Unit> GuardsSample() =>
        Check(
            nameof(GuardsSample),
            ("Bounded ok", Guards.Bounded(50) == Pure(50)),
            ("Bounded guard", Guards.Bounded(-1).Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.InvalidAge))),
            ("Bounded when", Guards.Bounded(500).Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.TooLarge))),
            ("Metered ok", Guards.Metered(IO.pure(50)).RunSafe() == Pure(50)),
            ("Metered unless", Guards.Metered(IO.pure(500)).RunSafe().Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.TooLarge))));

    private static Fin<Unit> CorrectFormsSample() {
        const string? missing = null;
        Fin<Option<Person>> found = CorrectForms.Lookup(People, "ada").Run().As().RunSafe();
        return Check(
            nameof(CorrectFormsSample),
            ("Older", CorrectForms.Older(30).Map(static a => (int)a) == Pure(31)),
            ("Required", CorrectForms.Required(None).IsFail),
            ("Lookup", found.Exists(static o => o.IsSome)),
            ("Nickname", CorrectForms.Nickname(missing).IsNone),
            ("Some(null) holds null", Some(missing).IsSome));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
