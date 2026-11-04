#![feature(default_field_values)]

use core::assert_matches;

use approx::assert_abs_diff_eq;
use claims::{assert_err, assert_ok};
use clarabel::{
    algebra::{CscMatrix, SparseFormatError},
    solver::{DataUpdateError, DefaultSettings, SolverStatus, SupportedConeT},
};
use nonempty_collections::NEVec;
use rasm_optimize::{
    OptimizeError,
    conic::{Block, Refusal, Update, solve},
};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn singleton_cones_accept_unbounded_constants() -> Result<(), NEVec<OptimizeError>> {
    let quadratic = CscMatrix::from(&[[2.0]]);
    let linear = [-4.0];
    let settings = DefaultSettings { presolve_enable: false, ..DefaultSettings::default() };
    let tolerance = 1.0e-7;
    let bound = 1.0;
    let active = (bound, 0.0, 2.0);
    let absent = (2.0, 1.0, 0.0);
    [SupportedConeT::NonnegativeConeT(1), SupportedConeT::SecondOrderConeT(1), SupportedConeT::PSDTriangleConeT(1)].into_iter().try_for_each(|cone| -> Result<(), NEVec<OptimizeError>> {
        Block::new(cone.clone(), CscMatrix::zeros((1, 1)), vec![f64::INFINITY]).map_err(NEVec::new)?;
        let block = |bound| Block::new(cone.clone(), CscMatrix::from(&[[1.0]]), vec![bound]).map_err(NEVec::new);
        let constructed = solve(settings.clone(), |_| false, Some(&quadratic), &linear, &[block(f64::INFINITY)?], &[])?;
        let updated = solve(
            settings.clone(),
            |_| false,
            Some(&quadratic),
            &linear,
            &[block(f64::INFINITY)?],
            &[
                Update { blocks: vec![block(bound)?], .. },
                Update { blocks: vec![block(f64::INFINITY)?], .. },
                Update { linear: vec![0.0; linear.len().strict_add(1)], blocks: vec![block(bound)?], .. },
                Update { linear: linear.to_vec(), .. },
            ],
        )?;
        let expected = [Some(absent), Some(absent), Some(active), Some(absent), None, Some(absent)];
        assert_eq!(constructed.len().get(), 1);
        assert_eq!(updated.len().get(), expected.len().strict_sub(constructed.len().get()));
        for (ordinal, (outcome, expected)) in constructed.into_iter().chain(updated).zip(expected).enumerate() {
            match expected {
                Some((point, slack, dual)) => {
                    let solution = assert_ok!(outcome, "cone {cone}, result {ordinal}");
                    assert_eq!(solution.status, SolverStatus::Solved);
                    assert_abs_diff_eq!(solution.x[0], point, epsilon = tolerance);
                    assert_abs_diff_eq!(solution.s[0], slack, epsilon = tolerance);
                    assert_abs_diff_eq!(solution.z[0], dual, epsilon = tolerance);
                }
                None => assert_matches!(assert_err!(outcome).first(), OptimizeError::Update(DataUpdateError::BadFormat(SparseFormatError::IncompatibleDimension))),
            }
        }
        Ok(())
    })?;
    assert_matches!(
        assert_err!(Block::new(SupportedConeT::PSDTriangleConeT(2), CscMatrix::zeros((3, 1)), vec![f64::INFINITY, 0.0, 0.0])),
        OptimizeError::Cone { cone: SupportedConeT::PSDTriangleConeT(2), reason: Refusal::Constant { index: 0 } },
        "larger semidefinite cones must retain their finite constant domain"
    );
    Ok(())
}
