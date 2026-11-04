#![feature(default_field_values, deref_patterns)]

use acadrust::{
    CadDocument, DxfError, Vector3,
    entities::{EntityType, Insert, Line, Spline, SplineFlags, Wipeout},
    tables::BlockRecord,
};
use itertools::Either;
use kurbo::{PathEl, Point};
use nonempty_collections::NEVec;
use rasm_cad::{
    CadError, Refusal, Unit,
    linework::{self, Projection},
};
use rasm_linework::{Element, Side, Tolerance, ToleranceError, pages};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn project(document: &mut CadDocument) -> Result<Vec<pages::Layer>, Either<ToleranceError, NEVec<CadError>>> {
    let tolerance = Tolerance::try_new(0.01).map_err(Either::Left)?;
    linework::layers(document, &Projection { tolerance, unit: Some(Unit::Millimeters), .. }).map_err(Either::Right)
}

fn rational(weights: [f64; 3], control: [Vector3; 3]) -> Result<CadDocument, DxfError> {
    let mut document = CadDocument::new();
    document
        .add_entity(EntityType::Spline(Spline {
            degree: 2,
            flags: SplineFlags { rational: true, ..SplineFlags::new() },
            weights: weights.into(),
            control_points: control.into(),
            knots: vec![0.0, 0.0, 0.0, 1.0, 1.0, 1.0],
            ..Spline::new()
        }))
        .map(|_| document)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn shared_null_block_handles_preserve_nested_geometry_and_sibling_reuse() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    let line = document.add_entity(EntityType::Line(Line::from_points(Vector3::ZERO, Vector3::UNIT_X)))?;
    let nested = document.add_entity(EntityType::Insert(Insert::new("inner", Vector3::ZERO)))?;
    let first_handle = document.add_entity(EntityType::Insert(Insert::new("outer", Vector3::ZERO)))?;
    let second_handle = document.add_entity(EntityType::Insert(Insert::new("outer", Vector3::new(5.0, 0.0, 0.0))))?;
    document.block_records.add(BlockRecord { entity_handles: vec![line], ..BlockRecord::new("inner") })?;
    document.block_records.add(BlockRecord { entity_handles: vec![nested], ..BlockRecord::new("outer") })?;
    document.block_records.add_or_replace(BlockRecord { handle: document.header.model_space_block_handle, entity_handles: vec![first_handle, second_handle], ..BlockRecord::model_space() });
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements,
                    [Element::Stroke { path: first, pen: _, linetype: _ }, Element::Stroke { path: second, pen: _, linetype: _ }]
                        if first.elements() == [PathEl::MoveTo(Point::ORIGIN), PathEl::LineTo(Point::new(1.0, 0.0))]
                            && second.elements() == [PathEl::MoveTo(Point::new(5.0, 0.0)), PathEl::LineTo(Point::new(6.0, 0.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn native_unicode_cycle_identity_preserves_sibling_failure_order() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    let recursive = document.add_entity(EntityType::Insert(Insert::new("ÖLÇÜ", Vector3::ZERO)))?;
    let cycle = document.add_entity(EntityType::Insert(Insert::new("ölçü", Vector3::ZERO)))?;
    let undefined = document.add_entity(EntityType::Insert(Insert::new("undefined", Vector3::ZERO)))?;
    document.block_records.add(BlockRecord { entity_handles: vec![recursive], ..BlockRecord::new("ölçü") })?;
    document.block_records.add_or_replace(BlockRecord { handle: document.header.model_space_block_handle, entity_handles: vec![cycle, undefined], ..BlockRecord::model_space() });
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Err(Either::Right(errors)) if matches!(errors.as_ref(),
                [CadError::Refused { subject: _, reason: Refusal::Cycle }, CadError::Refused { subject: _, reason: Refusal::Undefined }]
            )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn finite_rational_products_preserve_large_coordinates() -> Result<(), DxfError> {
    let mut document = rational([f64::MAX, f64::MAX / 2.0, f64::MAX], [Vector3::new(f64::MAX, 0.0, 0.0), Vector3::new(f64::MAX, 1.0, 0.0), Vector3::new(f64::MAX, 2.0, 0.0)])?;
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [Element::Stroke { path, pen: _, linetype: _ }]
                    if path.elements() == [PathEl::MoveTo(Point::new(f64::MAX, 0.0)), PathEl::LineTo(Point::new(f64::MAX, 2.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn rational_weight_range_preserves_small_weight_endpoints() -> Result<(), DxfError> {
    let smallest = 0.0_f64.next_up();
    let mut document = rational([smallest, f64::MAX, smallest], [Vector3::ZERO, Vector3::new(f64::MAX, 1.0, 0.0), Vector3::new(0.0, 2.0, 0.0)])?;
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [Element::Stroke { path, pen: _, linetype: _ }]
                    if path.elements() == [PathEl::MoveTo(Point::ORIGIN), PathEl::LineTo(Point::new(f64::MAX, 1.0)), PathEl::LineTo(Point::new(0.0, 2.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn wide_knot_intervals_preserve_quadratic_control_points() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    document.add_entity(EntityType::Spline(Spline {
        degree: 2,
        control_points: vec![Vector3::ZERO, Vector3::new(1.0, 1.0, 0.0), Vector3::new(2.0, 0.0, 0.0), Vector3::new(3.0, 1.0, 0.0)],
        knots: vec![-f64::MAX, -f64::MAX, -f64::MAX, 0.0, f64::MAX, f64::MAX, f64::MAX],
        ..Spline::new()
    }))?;
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [Element::Stroke { path, pen: _, linetype: _ }]
                    if path.elements() == [
                        PathEl::MoveTo(Point::ORIGIN),
                        PathEl::QuadTo(Point::new(1.0, 1.0), Point::new(1.5, 0.5)),
                        PathEl::QuadTo(Point::new(2.0, 0.0), Point::new(3.0, 1.0))
                    ]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn unequal_knot_intervals_preserve_positive_rational_weights() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    let smallest = 0.0_f64.next_up();
    document.add_entity(EntityType::Spline(Spline {
        degree: 2,
        flags: SplineFlags { rational: true, ..SplineFlags::new() },
        weights: vec![1.0, f64::MAX, smallest, 1.0],
        control_points: vec![Vector3::ZERO, Vector3::ZERO, Vector3::new(2.0, 0.0, 0.0), Vector3::new(2.0, 0.0, 0.0)],
        knots: vec![-f64::MAX, -f64::MAX, -f64::MAX, 0.0, smallest, smallest, smallest],
        ..Spline::new()
    }))?;
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [Element::Stroke { path, pen: _, linetype: _ }]
                    if path.elements() == [PathEl::MoveTo(Point::ORIGIN), PathEl::LineTo(Point::new(1.0, 0.0)), PathEl::LineTo(Point::new(2.0, 0.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn signed_zero_knots_preserve_empty_domain_refusal() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    document.add_entity(EntityType::Spline(Spline {
        degree: 2,
        control_points: vec![Vector3::ZERO, Vector3::UNIT_X, Vector3::UNIT_Y],
        knots: vec![-0.0, -0.0, -0.0, 0.0, 0.0, 0.0],
        ..Spline::new()
    }))?;
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Err(Either::Right(errors)) if matches!(errors.as_ref(), [CadError::Refused { subject: _, reason: Refusal::Unsupported }])
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
#[stacksafe::stacksafe]
fn nested_wipeout_preserves_parent_draw_order() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    let before_handle = document.add_entity(EntityType::Line(Line::from_coords(-5.0, 0.0, 0.0, -4.0, 0.0, 0.0)))?;
    let inner_handle = document.add_entity(EntityType::Line(Line::from_points(Vector3::ZERO, Vector3::UNIT_X)))?;
    let wipeout = document.add_entity(EntityType::Wipeout(Wipeout::rectangular(Vector3::ZERO, 1.0, 1.0)))?;
    let after_handle = document.add_entity(EntityType::Line(Line::from_coords(2.0, 0.0, 0.0, 3.0, 0.0, 0.0)))?;
    let inserted = document.add_entity(EntityType::Insert(Insert::new("masked", Vector3::UNIT_X)))?;
    let last_handle = document.add_entity(EntityType::Line(Line::from_coords(10.0, 0.0, 0.0, 11.0, 0.0, 0.0)))?;
    document.block_records.add(BlockRecord { entity_handles: vec![inner_handle, wipeout, after_handle], ..BlockRecord::new("masked") })?;
    document.block_records.add_or_replace(BlockRecord { handle: document.header.model_space_block_handle, entity_handles: vec![before_handle, inserted, last_handle], ..BlockRecord::model_space() });
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [
                    Element::Stroke { path: before, pen: _, linetype: _ },
                    Element::Clip { region: _, keep: Side::Outside, elements: clipped },
                    Element::Stroke { path: after, pen: _, linetype: _ },
                    Element::Stroke { path: last, pen: _, linetype: _ }
                ] if before.elements() == [PathEl::MoveTo(Point::new(-5.0, 0.0)), PathEl::LineTo(Point::new(-4.0, 0.0))]
                    && matches!(&**clipped, [Element::Stroke { path: inner, pen: _, linetype: _ }]
                        if inner.elements() == [PathEl::MoveTo(Point::new(1.0, 0.0)), PathEl::LineTo(Point::new(2.0, 0.0))]
                    )
                    && after.elements() == [PathEl::MoveTo(Point::new(3.0, 0.0)), PathEl::LineTo(Point::new(4.0, 0.0))]
                    && last.elements() == [PathEl::MoveTo(Point::new(10.0, 0.0)), PathEl::LineTo(Point::new(11.0, 0.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}

#[test]
fn deep_acyclic_blocks_preserve_leaf_geometry() -> Result<(), DxfError> {
    let mut document = CadDocument::new();
    let leaf = document.add_entity(EntityType::Line(Line::from_points(Vector3::ZERO, Vector3::UNIT_X)))?;
    let (root, records) = (0..10_000).rev().try_fold((leaf, Vec::new()), |(member, mut records), index| {
        let name = format!("block {index}");
        let record = BlockRecord { handle: document.allocate_handle(), entity_handles: vec![member], ..BlockRecord::new(name.clone()) };
        let next = document.add_entity(EntityType::Insert(Insert::new(name, Vector3::ZERO)))?;
        records.push(record);
        Ok::<_, DxfError>((next, records))
    })?;
    records.into_iter().try_for_each(|record| document.block_records.add(record))?;
    document.block_records.add_or_replace(BlockRecord { handle: document.header.model_space_block_handle, entity_handles: vec![root], ..BlockRecord::model_space() });
    let projected = project(&mut document);
    assert!(
        matches!(&projected,
            Ok([pages::Layer { name: _, print: _, view: _, elements }])
                if matches!(elements, [Element::Stroke { path, pen: _, linetype: _ }]
                    if path.elements() == [PathEl::MoveTo(Point::ORIGIN), PathEl::LineTo(Point::new(1.0, 0.0))]
                )
        ),
        "{projected:?}"
    );
    Ok(())
}
