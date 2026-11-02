use core::{borrow::Borrow, num::NonZero, ops::ControlFlow, range::RangeInclusive};

use gauss_quad::GaussLegendre;
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use rasm_stats::{Bounds, BoundsError};

use crate::{AnalysisError, Tolerance};

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOLERANCE: Tolerance = Tolerance::try_new(1.49e-8).ok().unwrap();

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Adaptive {
    pub degree: NonZero<usize> = NonZero::new(10).unwrap(),
    pub absolute: Tolerance = TOLERANCE,
    pub relative: Tolerance = TOLERANCE,
    pub limit: NonZero<usize> = NonZero::new(50).unwrap(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn integrate<F, I>(integrand: F, parts: I, adaptive: Adaptive) -> Result<f64, AnalysisError>
where
    F: Fn(f64) -> f64 + Sync,
    I: IntoNonEmptyIterator<Item: Borrow<Bounds>>,
{
    let rule = GaussLegendre::new(adaptive.degree);
    let value = |part: Bounds| {
        let integral = rule.par_integrate(part.start, part.last, &integrand);
        integral.is_finite().ok_or(AnalysisError::Integrand { interval: part }).map(|()| integral)
    };
    let bisect = |part: Bounds, whole: f64, estimate: f64, error: f64| {
        let middle = part.start.midpoint(part.last);
        let half = |start, last| {
            Bounds::try_new(RangeInclusive { start, last })
                .map_err(|BoundsError::PredicateViolated| AnalysisError::Integral { estimate, error })
                .and_then(|half| value(half).map(|integral| (half, integral)))
        };
        let (left, right) = rayon::join(|| half(part.start, middle), || half(middle, part.last));
        let (left, right) = (left?, right?);
        Ok::<_, AnalysisError>(((whole - (left.1 + right.1)).abs(), [left, right]))
    };
    let settled = |pieces: &NEVec<(f64, [(Bounds, f64); 2])>| {
        let (estimate, error) = pieces.iter().fold((0.0, 0.0), |(estimate, error), &(deviation, [(_, low), (_, high)])| (estimate + low + high, error + deviation));
        match (estimate, error) {
            (estimate, error) if !estimate.is_finite() => ControlFlow::Break(Err(AnalysisError::Integral { estimate, error })),
            (estimate, error) if error <= adaptive.absolute.into_inner().max(adaptive.relative.into_inner() * estimate.abs()) => ControlFlow::Break(Ok(estimate)),
            (estimate, error) => ControlFlow::Continue((estimate, error)),
        }
    };
    let refine = |mut pieces: NEVec<(f64, [(Bounds, f64); 2])>, estimate: f64, error: f64| {
        let worst = pieces.nonempty_iter_mut().max_by(|left, right| left.0.total_cmp(&right.0));
        let (_, [(left, low), (right, high)]) = *worst;
        let (lower, upper) = rayon::join(|| bisect(left, low, estimate, error), || bisect(right, high, estimate, error));
        let (lower, upper) = (lower?, upper?);
        *worst = lower;
        pieces.push(upper);
        Ok(pieces)
    };
    let pieces = parts
        .into_nonempty_iter()
        .map(|part| {
            let part = *part.borrow();
            value(part).and_then(|whole| bisect(part, whole, whole, f64::INFINITY))
        })
        .collect::<Result<NEVec<_>, _>>()?;
    let refinements = adaptive.limit.get().saturating_sub(pieces.len().get());
    match (0..refinements).try_fold(pieces, |pieces, _| {
        let (estimate, error) = settled(&pieces)?;
        refine(pieces, estimate, error).map_or_else(|failure| ControlFlow::Break(Err(failure)), ControlFlow::Continue)
    }) {
        ControlFlow::Break(integral) => integral,
        ControlFlow::Continue(pieces) => match settled(&pieces) {
            ControlFlow::Break(integral) => integral,
            ControlFlow::Continue((estimate, error)) => Err(AnalysisError::Integral { estimate, error }),
        },
    }
}
