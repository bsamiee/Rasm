use core::assert_matches;

use nalgebra::DMatrix;
use rasm_optimize::{Input, OptimizeError, pareto::rank};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn crowding_retains_extreme_finite_spans() -> Result<(), OptimizeError> {
    [f64::MAX, f64::from_bits(1)].into_iter().try_for_each(|extent| {
        let objectives = DMatrix::from_row_slice(2, 3, &[-extent, 0.0, extent, extent, 0.0, -extent]);
        let ranked = rank(objectives.as_view(), None)?;
        assert_eq!(
            ranked.into_iter().map(|rank| (rank.point, rank.front, rank.violation, rank.crowding)).collect::<Vec<_>>(),
            vec![(0, 0, 0.0, f64::INFINITY), (2, 0, 0.0, f64::INFINITY), (1, 0, 0.0, 2.0)],
            "overflowing and subnormal spans must retain normalized interior crowding"
        );
        Ok(())
    })
}

#[test]
fn equal_objectives_keep_boundary_identity() -> Result<(), OptimizeError> {
    let ranked = rank(DMatrix::repeat(2, 4, 1.0).as_view(), None)?;
    assert_eq!(
        ranked.into_iter().map(|rank| (rank.point, rank.crowding)).collect::<Vec<_>>(),
        vec![(0, f64::INFINITY), (3, f64::INFINITY), (1, 0.0), (2, 0.0)],
        "equal objective values must choose boundaries by population order"
    );
    Ok(())
}

#[test]
fn nan_objectives_preserve_members() -> Result<(), OptimizeError> {
    let ranked = rank(DMatrix::repeat(2, 3, f64::NAN).as_view(), None)?;
    assert_eq!(
        ranked.into_iter().map(|rank| (rank.point, rank.front, rank.crowding)).collect::<Vec<_>>(),
        vec![(0, 0, 0.0), (1, 0, 0.0), (2, 0, 0.0)],
        "NaN exclusion from crowding must retain every population member"
    );
    Ok(())
}

#[test]
fn constraint_shape_retains_dimensions() {
    let outcome = rank(DMatrix::zeros(2, 3).as_view(), Some(DMatrix::zeros(1, 2).as_view()));
    assert_matches!(outcome, Err(OptimizeError::Shape { input: Input::Constraints, expected: 3, found: 2 }), "constraint column mismatch must retain its input and both dimensions");
}
