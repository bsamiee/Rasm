namespace Lab.F11;

internal static class Samples {
    private static readonly DateOnly Today = new(2030, 1, 1);

    public static Fin<Unit> Run() =>
        ReturnTypeProbe()
            .Bind(static _ => OperationsProbe())
            .Bind(static _ => FailFastProbe())
            .Bind(static _ => TypedProbe())
            .Bind(static _ => AdaptersProbe())
            .Bind(static _ => SeparationProbe());

    private static Fin<Unit> ReturnTypeProbe() =>
        Check(
            nameof(ReturnTypeProbe),
            ("Calculate(4, 1) == Pure(2.0)", Calculator.Calculate(4, 1) == Pure(2.0)),
            ("Calculate(1, 0).IsFail", Calculator.Calculate(1, 0).IsFail),
            ("Calculate(-1, 1).IsFail", Calculator.Calculate(-1, 1).IsFail));

    private static Fin<Unit> OperationsProbe() {
        bool iterated = false;
        _ = Operations.Root(4).Iter(_ => iterated = true);
        _ = Operations.Root(-4).Iter(_ => iterated = false);
        int matched = Operations.Root(-4).Match(Succ: static _ => 1, Fail: static _ => 0);
        double hostValue = Operations.Root(-4).IfFail(static _ => -1);
        return Check(
            nameof(OperationsProbe),
            ("Describe(4, 1) == Pure(\"2\")", Operations.Describe(4, 1) == Pure("2")),
            ("FourthRoot(16, 1) == Pure(2.0)", Operations.FourthRoot(16, 1) == Pure(2.0)),
            ("FourthRoot(-16, 1).IsFail", Operations.FourthRoot(-16, 1).IsFail),
            ("Root(4) == Pure(2.0)", Operations.Root(4) == Pure(2.0)),
            ("Root(-4).IsFail", Operations.Root(-4).IsFail),
            ("iterated", iterated),
            ("matched == 0", matched == 0),
            ("hostValue == -1", hostValue == -1));
    }

    private static Fin<Unit> FailFastProbe() =>
        Check(
            nameof(FailFastProbe),
            ("Handle(ACC-1, 10) == Pure(unit)", Workflow.Handle(new Request("ACC-1", 10m)) == Pure(unit)),
            ("Handle(ACC-1, 0).IsFail", Workflow.Handle(new Request("ACC-1", 0m)).IsFail),
            ("Handle(ACC-9, 10).IsFail", Workflow.Handle(new Request("ACC-9", 10m)).IsFail),
            ("Handle(ACC-1, 500).IsFail", Workflow.Handle(new Request("ACC-1", 500m)).IsFail));

    private static Fin<Unit> TypedProbe() {
        BookTransfer valid = new("DEUTDEFF", Today.AddDays(1));
        BookTransfer badBic = new("DEUT", Today.AddDays(1));
        BookTransfer past = new("DEUTDEFF", Today.AddDays(-1));
        BookTransfer both = new("DEUT", Today.AddDays(-1));
        Fin<BookTransfer> bothResult = Transfers.Validate(both, Today);
        bool firstOnly = bothResult.Match(Succ: static _ => false, Fail: static error => error.IsType<InvalidBic>() && error.HasCode(1));
        return Check(
            nameof(TypedProbe),
            ("Validate(valid).IsSucc", Transfers.Validate(valid, Today).IsSucc),
            ("Validate(badBic) is InvalidBic", Transfers.Validate(badBic, Today).Match(Succ: static _ => false, Fail: static error => error.Is(new InvalidBic()))),
            ("Validate(past) is TransferDateIsPast", Transfers.Validate(past, Today).Match(Succ: static _ => false, Fail: static error => error.HasCode(2))),
            ("firstOnly", firstOnly));
    }

    private static Fin<Unit> AdaptersProbe() {
        BookTransfer valid = new("DEUTDEFF", Today.AddDays(1));
        Fin<BookTransfer> rejected = new InvalidBic();
        Fin<BookTransfer> past = new TransferDateIsPast();
        Fin<BookTransfer> crashed = Error.New(new InvalidOperationException("boom"));
        bool wrapped = Adapters.WithContext(rejected).Match(Succ: static _ => false, Fail: static error => error.Inner.IsSome);
        Fin<BookTransfer> byError = past.Catch(new TransferDateIsPast(), _ => valid).As();
        Fin<BookTransfer> byPredicate = crashed.Catch(static error => error.IsExceptional, _ => valid).As();
        return Check(
            nameof(AdaptersProbe),
            ("wrapped", wrapped),
            ("Describe(valid) == Pure(\"DEUTDEFF\")", Adapters.Describe(valid) == Pure("DEUTDEFF")),
            ("Describe(rejected).IsFail", Adapters.Describe(rejected).IsFail),
            ("Recover(rejected) == Pure(valid)", Adapters.Recover(rejected, valid) == Pure(valid)),
            ("Recover(past).IsFail", Adapters.Recover(past, valid).IsFail),
            ("byError == Pure(valid)", byError == Pure(valid)),
            ("byPredicate == Pure(valid)", byPredicate == Pure(valid)));
    }

    private static Fin<Unit> SeparationProbe() {
        BookTransfer valid = new("DEUTDEFF", Today.AddDays(1));
        BookTransfer duplicate = new("DEUTDEFFXXX", Today.AddDays(1));
        BookTransfer both = new("DEUT", Today.AddDays(-1));
        int accumulated = Handler.ValidateCommand(both, Today).Match(Fail: static error => error.Count, Succ: static _ => 0);
        bool members = Handler.ValidateCommand(both, Today).Match(Fail: static error => error.IsType<InvalidBic>() && error.IsType<TransferDateIsPast>() && !error.IsType<ManyErrors>(), Succ: static _ => false);
        bool exceptional = Persistence.Save(duplicate).RunSafe().Match(Succ: static _ => false, Fail: static error => error.IsExceptional);
        return Check(
            nameof(SeparationProbe),
            ("accumulated == 2", accumulated == 2),
            ("members", members),
            ("exceptional", exceptional),
            ("Exit(valid) == 200", Handler.Exit(Handler.Handle(valid, Today)) == 200),
            ("Exit(both) == 400", Handler.Exit(Handler.Handle(both, Today)) == 400),
            ("Exit(duplicate) == 500", Handler.Exit(Handler.Handle(duplicate, Today)) == 500));
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }
}
