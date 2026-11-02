use core::{iter, num::NonZero, range::RangeInclusive};

use anyhow::Error;
use argmin::{
    core::{CostFunction, Executor},
    solver::brent::BrentRoot,
};
use faer::{MatRef, linalg::evd::EvdError};
use itertools::Itertools as _;
use nalgebra::{DMatrix, convert, linalg::balancing::balance_parlett_reinsch};
use nonempty_collections::NEVec;
use num_complex::Complex;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_stats::{Bounds, Tolerance};
use rayon::{
    iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _, once},
    slice::ParallelSlice as _,
};
use validated::Validated;

use crate::AnalysisError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Scan {
    pub steps: NonZero<usize>,
    pub tolerance: Option<Tolerance> = None,
}

struct Equation<'function, F>(&'function F);

impl<F> CostFunction for Equation<'_, F>
where F: Fn(f64) -> f64
{
    type Param = f64;
    type Output = f64;

    fn cost(&self, param: &f64) -> Result<f64, Error> {
        match (self.0)(*param) {
            value if value.is_finite() => Ok(value),
            _ => Err(Error::new(AnalysisError::Function { at: *param })),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn bracketed<F>(function: F, interval: Bounds, scan: Scan) -> Result<Vec<f64>, NEVec<AnalysisError>>
where F: Fn(f64) -> f64 + Sync {
    let steps = convert::<usize, f64>(scan.steps.get());
    let width = interval.last - interval.start;
    let nodes = (0..scan.steps.get())
        .into_par_iter()
        .map(|step| width.mul_add(convert::<usize, f64>(step) / steps, interval.start))
        .chain(once(interval.last))
        .map(|at| (at, Some(function(at)).filter(|value| value.is_finite())))
        .collect::<Vec<_>>();
    let solve = |bracket: Bounds, bound: f64| {
        Executor::new(Equation(&function), BrentRoot::new(bracket.start, bracket.last, scan.tolerance.map_or_default(Tolerance::into_inner)))
            .run()
            .map_err(|report| {
                report.downcast::<AnalysisError>().unwrap_or_else(|report| {
                    tracing::error!({ EXCEPTION_MESSAGE } = %report, start = bracket.start, last = bracket.last, "root search raised a report with no typed source");
                    AnalysisError::Root { bracket }
                })
            })
            .and_then(|result| result.state.param.ok_or(AnalysisError::Root { bracket }).map(|root| (result.state.cost <= bound).then_some(root)))
            .transpose()
    };
    nodes
        .par_iter()
        .map(|&(at, value)| match value {
            None => Some(Err(AnalysisError::Function { at })),
            Some(0.0) => Some(Ok(at)),
            Some(_) => None,
        })
        .interleave(nodes.par_array_windows().map(|&[(start, below), (last, above)]| {
            below
                .zip(above)
                .filter(|&(below, above)| below != 0.0 && above != 0.0 && below.is_sign_negative() != above.is_sign_negative())
                .and_then(|(below, above)| Bounds::try_new(RangeInclusive { start, last }).ok().and_then(|bracket| solve(bracket, below.abs().max(above.abs()))))
        }))
        .flatten_iter()
        .collect::<Vec<_>>()
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub fn polynomial(coefficients: &[f64]) -> Result<Vec<Complex<f64>>, AnalysisError> {
    coefficients.iter().position(|coefficient| !coefficient.is_finite()).map_or(Ok(()), |index| Err(AnalysisError::Value { index }))?;
    let (origin, leading, lower) = coefficients
        .iter()
        .positions(|&coefficient| coefficient != 0.0)
        .minmax()
        .into_option()
        .and_then(|(origin, degree)| coefficients.get(origin..=degree)?.split_last().map(|(leading, lower)| (origin, leading, lower)))
        .ok_or(AnalysisError::Zero)?;
    let monic = lower.iter().map(|coefficient| -coefficient / leading).collect::<Vec<_>>();
    monic.iter().position(|entry| !entry.is_finite()).map_or(Ok(()), |index| Err(AnalysisError::Value { index: origin.strict_add(index) }))?;
    let degree = monic.len();
    let scale = monic.iter().map(|entry| entry.abs()).fold(1.0, f64::max).log2().ceil().min(f64::from(f64::MAX_EXP) - 1.0).exp2();
    let mut companion = DMatrix::from_iterator(
        degree,
        degree,
        monic.iter().rev().enumerate().flat_map(|(column, &entry)| iter::once(entry).chain((0..degree.strict_sub(1)).map(move |below| f64::from(below == column)))),
    ) / scale;
    balance_parlett_reinsch(&mut companion);
    MatRef::from_column_major_slice(companion.as_slice(), degree, degree)
        .eigenvalues()
        .map(|roots| roots.into_iter().map(|root| root * scale).chain(iter::repeat_n(Complex::ZERO, origin)).collect())
        .map_err(|EvdError::NoConvergence| AnalysisError::Evd)
}
