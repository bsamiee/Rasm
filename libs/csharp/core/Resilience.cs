using Core.Domain;
using LanguageExt.Common;
namespace Core;

// --- [CONSTANTS] -------------------------------------------------------------------------------

public static class Resilience {
    public static readonly Schedule StandardPolicy =
        Schedule.exponential(seed: new Duration(milliseconds: 50.0))
            | Schedule.recurs(times: 3)
            | Schedule.jitter(factor: 0.1);

    // --- [OPERATIONS] --------------------------------------------------------------------------

    public static Eff<AnalysisRuntime, T> WithStandardResilience<T>(
        this Eff<AnalysisRuntime, T> effect) =>
        retry(schedule: StandardPolicy, ma: effect)
            .As();
    public static Eff<AnalysisRuntime, T> ToEff<T>(this Fin<T> fin) =>
        fin;
    public static Eff<AnalysisRuntime, T> ToEff<T>(this Validation<Error, T> validation) =>
        validation.ToFin();
}
