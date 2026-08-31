namespace Lab.F00;

internal static class Laws {
    public static Fin<Unit> Probe() {
        Validation<Error, Unit> functor = FunctorLaw<Option>.validate(Some(1));
        Validation<Error, Unit> applicative = ApplicativeLaw<Option>.validate();
        Validation<Error, Unit> monad = MonadLaw<Option>.validate();
        Validation<Error, Unit> finMonad = MonadLaw<Fin>.validate();
        bool property = Try.lift(static () => { Gen.Int.Sample(static x => Some(x).Map(identity) == Some(x)); return true; }).Run().IsSucc;
        return Verify.Check(
            nameof(Laws),
            ("functor.IsSuccess", functor.IsSuccess),
            ("applicative.IsSuccess", applicative.IsSuccess),
            ("monad.IsSuccess", monad.IsSuccess),
            ("finMonad.IsSuccess", finMonad.IsSuccess),
            ("property", property));
    }
}
