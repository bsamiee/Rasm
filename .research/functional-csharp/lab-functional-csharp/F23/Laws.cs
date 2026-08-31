namespace Lab.F23;

internal static class Laws {
    public static Validation<Error, Unit> OptionFunctor => FunctorLaw<Option>.validate(Some(1));
    public static Validation<Error, Unit> OptionApplicative => ApplicativeLaw<Option>.validate();
    public static Validation<Error, Unit> OptionMonad => MonadLaw<Option>.validate();

    public static Validation<Error, Unit> FinFunctor => FunctorLaw<Fin>.validate(Pure(1).ToFin());

    public static Validation<Error, Unit> FinApplicative => ApplicativeLaw<Fin>.validate();

    public static Validation<Error, Unit> FinMonad => MonadLaw<Fin>.validate();
}
