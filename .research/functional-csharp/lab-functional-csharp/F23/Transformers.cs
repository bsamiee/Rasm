namespace Lab.F23;

internal sealed record Account(Guid Id, decimal Balance);

internal sealed record Guest(string Name);

internal sealed record Member(int Id);

internal sealed record Overdrawn() : Expected("account overdrawn", 2302);

internal sealed record EmptyName() : Expected("name is empty", 2303);

internal static class Transformers {
    public static OptionT<IO, decimal> Converted(OptionT<IO, Account> lookup, IO<decimal> rate) =>
        from account in lookup
        from factor in OptionT.liftIO<IO, decimal>(rate)
        select account.Balance * factor;

    public static Fin<Account> Charge(Account account, decimal amount) =>
        account.Balance >= amount ? account with { Balance = account.Balance - amount } : new Overdrawn();
    public static FinT<IO, Account> Charged(IO<Account> load, decimal amount) =>
        from account in FinT.liftIO<IO, Account>(load)
        from charged in FinT.lift<IO, Account>(Charge(account, amount))
        select charged;
    public static IO<Account> Settled(FinT<IO, Account> charged) => charged.runFin.As().Bind(static fin => IO.lift(fin));

    public static Either<Guest, Member> Classify(int id, string name) => id > 0 ? Right(new Member(id)) : Left(new Guest(name));
    public static EitherT<Guest, IO, Member> Visitor(IO<int> loadId, string name) =>
        from id in EitherT.liftIO<Guest, IO, int>(loadId)
        from member in EitherT.lift<Guest, IO, Member>(Classify(id, name))
        select member;

    public static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;
    public static ValidationT<Error, IO, string> Checked(IO<string> loadName) =>
        from name in ValidationT.liftIO<Error, IO, string>(loadName)
        from valid in ValidationT.lift<Error, IO, string>(ValidName(name))
        select valid;
    public static ValidationT<Error, IO, string> Both(IO<string> first, IO<string> second) =>
        (Checked(first), Checked(second)).Apply(static (a, b) => a + b).As();

    public static ReaderT<Settings, OptionT<IO>, int> Priced(Option<int> quantity, IO<int> unitPrice) =>
        from settings in ReaderT.ask<OptionT<IO>, Settings>()
        from count in ReaderT.lift<Settings, OptionT<IO>, int>(OptionT.lift<IO, int>(quantity))
        from price in ReaderT.liftIO<Settings, OptionT<IO>, int>(unitPrice)
        select count * price * settings.Factor;
    public static Fin<Option<int>> Run(ReaderT<Settings, OptionT<IO>, int> priced, Settings settings) =>
        priced.Run(settings).As().Run().As().RunSafe();

    public static WriterT<Seq<string>, IO, int> Audited(IO<int> load) =>
        from value in WriterT.liftIO<Seq<string>, IO, int>(load)
        from _ in WriterT.tell<Seq<string>, IO>(Seq(string.Create(CultureInfo.InvariantCulture, $"loaded {value}")))
        select value;
    public static Fin<(int Value, Seq<string> Output)> Run(WriterT<Seq<string>, IO, int> audited) => audited.Run().As().RunSafe();

    public static RWST<Settings, Seq<string>, int, IO, int> Stepped =>
        from settings in RWST.ask<Settings, Seq<string>, int, IO>()
        from count in RWST.get<Settings, Seq<string>, int, IO>()
        from _ in RWST.put<Settings, Seq<string>, int, IO>(count + 1)
        from __ in RWST.tell<Settings, Seq<string>, int, IO>(Seq("stepped"))
        select count * settings.Factor;
}

internal sealed record Counter<A>(StateT<int, IO, A> Step) : K<Counter, A>;

internal abstract class Counter : Deriving.Monad<Counter, StateT<int, IO>>, Deriving.Stateful<Counter, StateT<int, IO>, int> {
    public static K<StateT<int, IO>, A> Transform<A>(K<Counter, A> fa) => fa.As().Step;
    public static K<Counter, A> CoTransform<A>(K<StateT<int, IO>, A> fa) => new Counter<A>(fa.As());
}

internal static class CounterExtensions {
    public static Counter<A> As<A>(this K<Counter, A> ma) => (Counter<A>)ma;
    public static Fin<(int Value, int State)> Exit(this Counter<int> counter, int seed) => counter.Step.Run(seed).As().RunSafe();
}

internal static class Counters {
    public static Counter<int> Increment =>
        (from current in Stateful.get<Counter, int>()
         from _ in Stateful.put<Counter, int>(current + 1)
         select current).As();

    public static Counter<A> Lifted<A>(IO<A> effect) => Counter.CoTransform(StateT.liftIO<int, IO, A>(effect)).As();
}
