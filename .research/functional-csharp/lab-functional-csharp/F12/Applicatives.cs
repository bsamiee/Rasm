namespace Lab.F12;

internal static class Applicatives {
    public static Fin<Unit> ApplyProbe() {
        Func<int, Func<int, int>> multiply = static x => y => x * y;
        Option<Func<int, int>> multiplyBy3 = Some(3).Map(multiply);

        Option<int> product = multiplyBy3.Apply(Some(4));
        Option<int> viaTuple = (Some(3), Some(4)).Apply(static (x, y) => x * y).As();
        Option<int> absent = multiplyBy3.Apply(Option<int>.None);
        return Samples.Check(
            nameof(ApplyProbe),
            ("product == Some(12)", product == Some(12)),
            ("viaTuple == Some(12)", viaTuple == Some(12)),
            ("absent.IsNone", absent.IsNone));
    }

    public static Fin<Unit> LiftProbe() {
        Func<int, int, int> multiply = static (x, y) => x * y;
        Option<Func<int, int, int>> lifted = Pure(multiply);
        Option<int> result = lifted.Apply(Some(3)).Apply(Some(4)).As();
        // Some(12)

        Option<int> mapped = fun<int, int, int>(static (x, y) => x * y).Map(Some(3)).Apply(Some(4)).As();
        Option<int> applied = lifted.Apply(Some(3)).Apply(Some(4)).As();
        return Samples.Check(
            nameof(LiftProbe),
            ("result == Some(12)", result == Some(12)),
            ("mapped == applied", mapped == applied));
    }

    public static Fin<Unit> LawsProbe() {
        Option<int> option = Some(2);
        Func<int, int> f = static x => x + 1;
        Func<int, int> g = static x => x * 2;
        Option<int> sequential = option.Map(g).Map(f);

        Option<int> composed = option.Map(x => f(g(x)));

        Validation<Error, Unit> functor = FunctorLaw<Option>.validate(Some(1));
        Validation<Error, Unit> applicative = ApplicativeLaw<Option>.validate();
        Validation<Error, Unit> monad = MonadLaw<Fin>.validate();
        return Samples.Check(
            nameof(LawsProbe),
            ("sequential == composed", sequential == composed),
            ("sequential == Some(5)", sequential == Some(5)),
            ("functor.IsSuccess", functor.IsSuccess),
            ("applicative.IsSuccess", applicative.IsSuccess),
            ("monad.IsSuccess", monad.IsSuccess));
    }
}
