use core::{cmp::Ordering, iter, num::NonZero, range::RangeInclusive};

use csaps::CubicSmoothingSpline;
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrixView, DVector, convert};
use ndarray::{Array1, Array2, ArrayD, ArrayRef1, ArrayView, ArrayView1, ArrayView2, Axis, Dimension, IxDyn, Zip, concatenate};
use nonempty_collections::{NESlice, NEVec};
use rasm_stats::{Bounds, Tolerance};
use validated::Validated;

use crate::{
    AnalysisError, Input, Observations, Placement, RootSource, SearchHooks, finite, joined, refused_sites, refused_weights,
    roots::{minimum, real},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Method {
    Smoothing(Option<Smooth>),
    Gcv,
    Monotone,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Extrapolation {
    Refuse,
    Clamp,
    Linear,
    Cubic,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Reading {
    Value,
    Derivative(NonZero<u8>),
    Integral(RangeInclusive<f64>),
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0, less_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq))]
pub struct Smooth(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct GridAxis<'data> {
    pub sites: ArrayView1<'data, f64>,
    pub weights: Option<ArrayView1<'data, f64>> = None,
    pub method: Method = Method::Smoothing(None),
    pub period: Option<f64> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct SplineAxis {
    pub breaks: Box<[f64]>,
    pub span: RangeInclusive<f64>,
    pub period: Option<f64>,
    pub smooth: Option<f64>,
    pub freedom: Option<f64>,
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Spline {
    #[as_ref([SplineAxis])]
    axes: Vec<SplineAxis>,
    #[as_ref]
    coefficients: ArrayD<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FITS]

pub fn grid<D>(axes: NESlice<'_, GridAxis<'_>>, values: ArrayView<'_, f64, D>, hooks: SearchHooks<'_>) -> Result<Spline, NEVec<AnalysisError>>
where D: Dimension {
    let smoothed = |sites: ArrayView1<'_, f64>, weights: Option<ArrayView1<'_, f64>>, lanes: ArrayView2<'_, f64>, smooth: Option<f64>| -> Result<(Array2<f64>, Option<f64>), AnalysisError> {
        let fitted = CubicSmoothingSpline::new(sites, lanes).with_optional_weights(weights).with_optional_smooth(smooth).make()?;
        fitted.spline().map(|spline| (spline.coeffs().to_owned(), fitted.smooth())).ok_or(AnalysisError::Underdetermined { found: sites.len(), needed: 2 })
    };
    let monotone = |sites: ArrayView1<'_, f64>, lane: ArrayView1<'_, f64>| -> Vec<f64> {
        let widths = sites.iter().tuple_windows().map(|(start, last)| last - start).collect::<Vec<_>>();
        let secants = iter::zip(&widths, lane.iter().tuple_windows()).map(|(width, (start, last))| (last - start) / width).collect::<Vec<_>>();
        let edge = |near: f64, far: f64, width: f64, next: f64| {
            let slope = 2.0_f64.mul_add(width, next).mul_add(near, -width * far) / (width + next);
            if near == 0.0 || slope.is_sign_negative() != near.is_sign_negative() {
                0.0
            } else if near.is_sign_negative() != far.is_sign_negative() && slope.abs() > 3.0 * near.abs() {
                3.0 * near
            } else {
                slope
            }
        };
        let interior = widths.iter().tuple_windows().zip(secants.iter().tuple_windows()).map(|((&before, &after), (&left, &right))| {
            let (first, second) = (2.0_f64.mul_add(after, before), 2.0_f64.mul_add(before, after));
            if left == 0.0 || right == 0.0 || left.is_sign_negative() != right.is_sign_negative() { 0.0 } else { (first + second) / (first / left + second / right) }
        });
        let slopes = match (widths.first_chunk().zip(secants.first_chunk()), widths.last_chunk().zip(secants.last_chunk())) {
            (Some((&[width, next], &[near, far])), Some((&[last_next, last_width], &[last_far, last_near]))) =>
                iter::once(edge(near, far, width, next)).chain(interior).chain(iter::once(edge(last_near, last_far, last_width, last_next))).collect::<Vec<_>>(),
            _ => vec![secants[0]; 2],
        };
        let rows = izip!(&widths, &secants, slopes.iter().tuple_windows(), lane.iter())
            .map(|(&width, &secant, (&start, &last), &value)| {
                [2.0_f64.mul_add(-secant, start + last) / (width * width), 3.0_f64.mul_add(secant, -(2.0_f64.mul_add(start, last))) / width, start, value]
            })
            .collect::<Vec<_>>();
        (0..4).flat_map(|block| rows.iter().map(move |row| row[block])).collect()
    };
    let cross_validated = |sites: ArrayView1<'_, f64>, weights: Option<ArrayView1<'_, f64>>, lanes: ArrayView2<'_, f64>| -> Result<(Array2<f64>, Option<f64>, f64), AnalysisError> {
        let points = convert::<usize, f64>(sites.len());
        let unit = Array2::<f64>::eye(sites.len());
        let readings = |responses: ArrayView2<'_, f64>, smooth: f64| {
            CubicSmoothingSpline::new(sites, responses).with_optional_weights(weights).with_smooth(smooth).make().and_then(|fitted| fitted.evaluate(sites)).map_err(AnalysisError::from)
        };
        let scores = |smooth: f64| -> Result<(f64, f64), AnalysisError> {
            let trace = readings(unit.view(), smooth)?.diag().sum();
            let lack = Zip::indexed(lanes)
                .and(&readings(lanes, smooth)?)
                .fold(0.0, |sum, (_, point), observed, fitted| (observed - fitted).powi(2).mul_add(weights.map_or(1.0, |weights| weights[point]), sum));
            Ok((points * lack / (points - trace).powi(2), trace))
        };
        let reach = -f64::EPSILON.ln();
        let bracket = RangeInclusive { start: -reach, last: reach };
        let interval = Bounds::try_new(bracket).map_err(|source| AnalysisError::Root { bracket, source: RootSource::Bounds(source) })?;
        let smoothing = |exponent: f64| (1.0 + exponent.exp()).recip();
        let tolerance = const { Tolerance::try_new(f64::from(almost::F32_TOLERANCE)).ok().unwrap() };
        let (exponent, _) = minimum(|exponent| scores(smoothing(exponent)).map(|(score, _)| score), interval, tolerance, hooks)?;
        let smooth = smoothing(exponent);
        let (_, freedom) = scores(smooth)?;
        smoothed(sites, weights, lanes, Some(smooth)).map(|(coefficients, chosen)| (coefficients, chosen, freedom))
    };
    let refused_axis = |dimension: usize, axis: &GridAxis<'_>, expected: usize| {
        let found = axis.sites.len();
        let span = axis.sites.last().zip(axis.sites.first()).map_or_default(|(last, first)| last - first);
        iter::chain(
            [
                (found == expected).ok_or(AnalysisError::Shape { input: Input::AxisSites { dimension }, expected, found }),
                (found > 1).ok_or(AnalysisError::Underdetermined { found, needed: 2 }),
                axis.weights.map_or(Ok(()), |weights| (weights.len() == found).ok_or(AnalysisError::Shape { input: Input::AxisWeights { dimension }, expected: found, found: weights.len() })),
                axis.period.map_or(Ok(()), |period| (period.is_finite() && span < period).ok_or(AnalysisError::Period { dimension, span, period })),
            ],
            finite(axis.sites, move |point| AnalysisError::Site { dimension, point, placement: Placement::Unrepresentable }),
        )
        .chain(
            axis.sites
                .iter()
                .tuple_windows()
                .positions(|(earlier, later)| later <= earlier || almost::equal(*later, *earlier))
                .map(move |window| Err(AnalysisError::Site { dimension, point: window.strict_add(1), placement: Placement::Unordered })),
        )
        .chain(refused_weights(Some(dimension), axis.weights.into_iter().flatten()))
        .collect::<Vec<_>>()
    };
    let values = values.into_dyn();
    iter::once((axes.len().get() == values.ndim()).ok_or(AnalysisError::Shape { input: Input::Axes, expected: values.ndim(), found: axes.len().get() }))
        .chain(iter::zip(&axes, values.shape()).enumerate().flat_map(|(dimension, (axis, &expected))| refused_axis(dimension, axis, expected)))
        .chain(values.indexed_iter().filter(|(_, value)| !value.is_finite()).map(|(index, _)| Err(AnalysisError::Value { index: index.slice().to_vec() })))
        .collect::<Validated<(), _>>()
        .ok()?;
    let last = values.ndim().strict_sub(1);
    let fitted = axes.into_iter().enumerate().rev().try_fold((values.to_owned(), Vec::new()), |(current, mut fitted), (dimension, axis)| -> Result<_, AnalysisError> {
        let (sites, weights, current) = match axis.period {
            Some(period) => (
                [-period, 0.0, period].into_iter().flat_map(|offset| axis.sites.iter().map(move |site| site + offset)).collect::<Box<[_]>>(),
                axis.weights.map(|weights| iter::repeat_n(weights, 3).flatten().copied().collect::<Array1<_>>()),
                concatenate(Axis(dimension), &[current.view(), current.view(), current.view()])?,
            ),
            None => (axis.sites.iter().copied().collect::<Box<[_]>>(), axis.weights.map(|weights| weights.to_owned()), current),
        };
        let order = (0..=last).filter(|&index| index != dimension).chain(iter::once(dimension)).collect::<Vec<_>>();
        let moved = current.view().permuted_axes(IxDyn(&order));
        let outer = moved.shape().iter().copied().take(last).collect::<Vec<_>>();
        let lanes = moved.to_shape((outer.iter().product::<usize>(), sites.len()))?;
        let (coefficients, smooth, freedom) = match axis.method {
            Method::Smoothing(smooth) =>
                smoothed(ArrayView1::from(&*sites), weights.as_deref().map(ArrayRef1::view), lanes.view(), smooth.map(Smooth::into_inner)).map(|(coefficients, smooth)| (coefficients, smooth, None))?,
            Method::Gcv =>
                cross_validated(ArrayView1::from(&*sites), weights.as_deref().map(ArrayRef1::view), lanes.view()).map(|(coefficients, smooth, freedom)| (coefficients, smooth, Some(freedom)))?,
            Method::Monotone => (
                Array2::from_shape_vec((lanes.nrows(), 4_usize.strict_mul(sites.len().strict_sub(1))), lanes.rows().into_iter().flat_map(|lane| monotone(ArrayView1::from(&*sites), lane)).collect())?,
                None,
                None,
            ),
        };
        let shape = outer.iter().copied().chain(iter::once(coefficients.ncols())).collect::<Vec<_>>();
        let restored = (0..=last)
            .map(|index| match index.cmp(&dimension) {
                Ordering::Less => index,
                Ordering::Equal => last,
                Ordering::Greater => index.strict_sub(1),
            })
            .collect::<Vec<_>>();
        let reshaped = coefficients.as_standard_layout().into_owned().into_shape_with_order(IxDyn(&shape))?.permuted_axes(IxDyn(&restored));
        let start = axis.sites[0];
        fitted.push(SplineAxis {
            breaks: sites,
            span: RangeInclusive { start, last: axis.period.map_or_else(|| axis.sites[axis.sites.len().strict_sub(1)], |period| start + period) },
            period: axis.period,
            smooth,
            freedom,
        });
        Ok((reshaped, fitted))
    });
    fitted.map(|(coefficients, fitted)| Spline { axes: fitted.into_iter().rev().collect(), coefficients }).map_err(NEVec::new)
}

pub fn curve(observations: &Observations, method: Method, period: Option<f64>, hooks: SearchHooks<'_>) -> Result<Spline, NEVec<AnalysisError>> {
    let Observations { sites, values, weights } = observations;
    [
        (sites.nrows() == 1).ok_or(AnalysisError::Shape { input: Input::Sites, expected: 1, found: sites.nrows() }),
        (values.nrows() == 1).ok_or(AnalysisError::Shape { input: Input::Responses, expected: 1, found: values.nrows() }),
    ]
    .into_iter()
    .collect::<Validated<(), _>>()
    .ok()?;
    let axes = NEVec::new(GridAxis { sites: ArrayView1::from(sites.as_slice()), weights: Some(ArrayView1::from(weights.as_slice())), method, period });
    grid(axes.as_nonempty_slice(), ArrayView1::from(values.as_slice()), hooks)
}

// --- [READINGS]

fn falling(exponent: usize, times: usize) -> f64 { (exponent.strict_sub(times).strict_add(1)..=exponent).map(convert::<usize, f64>).product() }

pub fn evaluate(spline: &Spline, sites: DMatrixView<'_, f64>, readings: &[Reading], extrapolation: Extrapolation) -> Result<DVector<f64>, NEVec<AnalysisError>> {
    let dimensions = spline.axes.len();
    let functional = |breaks: &[f64], width: usize, reading: &Reading, at: f64| -> Vec<(usize, f64)> {
        let pieces = breaks.len().strict_sub(1);
        #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot infer the nonzero coefficient partition guaranteed by spline construction")]
        let order = width / pieces;
        let raised = |base: f64, exponent: usize| base.powi(convert::<usize, i32>(exponent));
        let local = |power: usize| {
            let piece = breaks[..pieces].partition_point(|&start| start <= at).saturating_sub(1);
            (0..order)
                .map(|block| (block, order.strict_sub(1).strict_sub(block)))
                .filter(|&(_, exponent)| exponent >= power)
                .map(move |(block, exponent)| (block.strict_mul(pieces).strict_add(piece), falling(exponent, power) * raised(at - breaks[piece], exponent.strict_sub(power))))
                .collect()
        };
        match reading {
            Reading::Value => local(0),
            Reading::Derivative(times) => local(usize::from(times.get())),
            Reading::Integral(interval) => breaks
                .iter()
                .tuple_windows()
                .enumerate()
                .flat_map(|(piece, (&start, &last))| {
                    let (lower, upper) = (interval.start.max(start) - start, interval.last.min(last) - start);
                    (0..order).filter(move |_| upper > lower).map(move |block| {
                        let exponent = order.strict_sub(block);
                        (block.strict_mul(pieces).strict_add(piece), (raised(upper, exponent) - raised(lower, exponent)) / convert::<usize, f64>(exponent))
                    })
                })
                .collect(),
        }
    };
    let functionals = |dimension: usize, axis: &SplineAxis, reading: &Reading, at: f64| -> Result<Vec<(usize, f64)>, AnalysisError> {
        let (breaks, span) = (axis.breaks.as_ref(), axis.span);
        let width = spline.coefficients.shape()[dimension];
        let at = axis.period.map_or(at, |period| span.start + (at - span.start).rem_euclid(period));
        let edge = at.clamp(span.start, span.last);
        match (reading, extrapolation, span.contains(&at)) {
            (Reading::Integral(interval), Extrapolation::Refuse | Extrapolation::Clamp | Extrapolation::Linear | Extrapolation::Cubic, true | false) => [interval.start, interval.last]
                .into_iter()
                .find(|end| !span.contains(end))
                .map_or_else(|| Ok(functional(breaks, width, reading, at)), |end| Err(AnalysisError::Outside { dimension, at: end })),
            (Reading::Value | Reading::Derivative(_), Extrapolation::Cubic, true | false)
            | (Reading::Value | Reading::Derivative(_), Extrapolation::Refuse | Extrapolation::Clamp | Extrapolation::Linear, true) => Ok(functional(breaks, width, reading, at)),
            (Reading::Value | Reading::Derivative(_), Extrapolation::Refuse, false) => Err(AnalysisError::Outside { dimension, at }),
            (Reading::Value, Extrapolation::Clamp, false) => Ok(functional(breaks, width, reading, edge)),
            (Reading::Value, Extrapolation::Linear, false) => {
                let slope = functional(breaks, width, &Reading::Derivative(NonZero::<u8>::MIN), edge);
                Ok(iter::chain(functional(breaks, width, reading, edge), slope.into_iter().map(|(index, weight)| (index, weight * (at - edge)))).collect())
            }
            (Reading::Derivative(times), Extrapolation::Linear, false) if *times == NonZero::<u8>::MIN => Ok(functional(breaks, width, reading, edge)),
            (Reading::Derivative(_), Extrapolation::Clamp | Extrapolation::Linear, false) => Ok(Vec::new()),
        }
    };
    iter::chain(
        [
            (sites.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Sites, expected: dimensions, found: sites.nrows() }),
            (readings.len() == dimensions).ok_or(AnalysisError::Shape { input: Input::Readings, expected: dimensions, found: readings.len() }),
        ],
        refused_sites(&sites, |coordinate: &f64| coordinate.is_finite()),
    )
    .collect::<Validated<(), _>>()
    .ok()?;
    sites
        .column_iter()
        .map(|site| {
            iter::zip(spline.axes.iter().enumerate(), iter::zip(readings, site.iter()))
                .map(|((dimension, axis), (reading, &at))| functionals(dimension, axis, reading, at))
                .collect::<Validated<Vec<_>, _>>()
                .map(|terms| {
                    let mut index = vec![0; dimensions];
                    terms
                        .into_iter()
                        .multi_cartesian_product()
                        .map(|term| {
                            iter::zip(&mut index, &term).for_each(|(index, &(entry, _))| *index = entry);
                            spline.coefficients[index.as_slice()] * term.iter().map(|&(_, factor)| factor).product::<f64>()
                        })
                        .sum::<f64>()
                })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(DVector::from_vec)
}

pub fn crossings(spline: &Spline, level: f64, derivative: u8) -> Result<Vec<f64>, NEVec<AnalysisError>> {
    let [axis] = &spline.axes else {
        return Err(NEVec::new(AnalysisError::Shape { input: Input::Axes, expected: 1, found: spline.axes.len() }));
    };
    let pieces = axis.breaks.len().strict_sub(1);
    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot infer the nonzero coefficient partition guaranteed by spline construction")]
    let order = spline.coefficients.len() / pieces;
    let times = usize::from(derivative);
    let found = axis.breaks.iter().tuple_windows().enumerate().map(|(piece, (&start, &last))| {
        let shifted = (times..order)
            .map(|exponent| {
                spline.coefficients[[order.strict_sub(1).strict_sub(exponent).strict_mul(pieces).strict_add(piece)]].mul_add(falling(exponent, times), -level * f64::from(exponent == times))
            })
            .collect::<Vec<_>>();
        match real(&shifted, RangeInclusive { start: 0.0, last: last - start }) {
            Ok(roots) => Ok(roots.into_iter().map(|root| root + start).collect::<Vec<_>>()),
            Err(errors) if errors.iter().all(AnalysisError::is_zero) => Ok(Vec::new()),
            Err(errors) => Err(errors),
        }
    });
    joined(found).map(|roots| roots.into_iter().flatten().filter(|at| axis.span.contains(at)).dedup_by(|left, right| almost::equal(*left, *right)).collect())
}
