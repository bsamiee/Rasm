#![feature(default_field_values)]

use core::f64::consts::FRAC_PI_2;

use approx::assert_relative_eq;
use manifold_rust::types::OpType;
use nalgebra::{Isometry3, Point2, Translation3, UnitQuaternion, Vector3, convert};
use rasm_mesh::{
    Extrusion, MeshError, Operation, Solid,
    booleans::{Csg, boolean},
    decomposition::decompose,
    export,
    measures::measure,
};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum TestError {
    #[error(transparent)]
    Mesh(#[from] MeshError),
    #[error("{0} produced no solid")]
    Empty(Operation),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[test]
fn nested_transforms_compose_in_parent_order() -> Result<(), TestError> {
    let base =
        Solid::extrude([[Point2::new(-1.0, -1.0), Point2::new(1.0, -1.0), Point2::new(1.0, 1.0), Point2::new(-1.0, 1.0)]], 2.0, Extrusion { .. }, None)?.ok_or(TestError::Empty(Operation::Extrude))?;
    let translation = |x, y, z| convert(Isometry3::translation(x, y, z));
    let union = Csg::op(OpType::Add, [[3.0, 0.0, 0.0], [0.0, 5.0, 0.0]].map(|[x, y, z]| Csg::leaf(base.clone()).transform(&translation(x, y, z)))).transform(&translation(2.0, 0.0, 0.0));
    let tree = Csg::op(OpType::Subtract, [union, Csg::leaf(base).transform(&translation(1000.0, 0.0, 0.0))]);
    let placement = convert(Isometry3::from_parts(Translation3::new(10.0, 20.0, 30.0), UnitQuaternion::from_axis_angle(&Vector3::z_axis(), FRAC_PI_2)));
    let solid = boolean(&tree.transform(&placement), None, None)?.ok_or(TestError::Empty(Operation::Boolean))?;
    let bounds = export(&solid)?.0.local_aabb();
    let measures = measure(&solid);
    let tolerance = f64::EPSILON.sqrt();
    assert_relative_eq!([bounds.mins.to_array(), bounds.maxs.to_array()], [[4.0, 21.0, 30.0], [11.0, 26.0, 32.0]], max_relative = tolerance);
    assert_relative_eq!([measures.mass_properties.mass(), measures.surface_area], [16.0, 48.0], max_relative = tolerance);
    assert_eq!(decompose(&solid).len(), 2);
    Ok(())
}
