#![feature(default_field_values)]

use core::{assert_matches, error::Error, iter};

use claims::assert_err;
use kurbo::{Affine, BezPath, Line, PathEl, Point, Rect, Shape as _};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _, nev};
use rasm_linework::{Alignment, Cmyk, Color, DashOffset, Dashes, Element, Linetype, LineworkError, Path, Pen, Placement, Region, Side, Target, Tolerance, Width, cleanup, rebase, transform};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn rectangle_recognition_preserves_a_resolvable_corner_cut() -> Result<(), Box<dyn Error>> {
    let tolerance = Tolerance::try_new(1.0e-10)?;
    let bounds = Rect::new(0.0, 0.0, 1.0, 1.0);
    let rectangle = Region::new(bounds, FillRule::NonZero, tolerance)?;
    let cut = BezPath::from_vec(vec![
        PathEl::MoveTo(Point::ORIGIN),
        PathEl::LineTo(Point::new(1.0, 0.0)),
        PathEl::LineTo(Point::new(1.0, 1.0 - 1.0e-8)),
        PathEl::LineTo(Point::new(1.0 - 1.0e-8, 1.0)),
        PathEl::LineTo(Point::new(0.0, 1.0)),
        PathEl::ClosePath,
    ]);
    let clipped = Region::new(cut, FillRule::NonZero, tolerance)?;
    assert_eq!(rectangle.as_rect(), Some(bounds));
    assert_eq!(clipped.as_rect(), None);
    Ok(())
}

#[test]
fn viewport_rebasing_scales_drafting_dashes_and_preserves_model_dashes() -> Result<(), Box<dyn Error>> {
    let path = Path::try_new(Line::new(Point::ORIGIN, Point::new(4.0, 0.0)).to_path(0.0))?;
    let pen = Pen { width: Width::try_new(0.25)?, color: Color::Cmyk(Cmyk { cyan: 0, magenta: 0, yellow: 0, black: 255 }), .. };
    let linetype = Linetype { dashes: Dashes::try_new(nev![2.0, 4.0])?, alignment: Alignment::Start { dash_offset: DashOffset::try_new(6.0)? }, target: Target::Drafting };
    let drafting = Element::Stroke { path: path.clone(), pen: pen.clone(), linetype: Some(linetype.clone()) };
    let model = Element::Stroke { path: path.clone(), pen: pen.clone(), linetype: Some(Linetype { target: Target::Model, ..linetype }) };
    let region = Region::default();
    let input = vec![Element::Clip { region: region.clone(), keep: Side::Outside, elements: vec![drafting].into() }, model.clone()];
    let output = rebase(input, Placement::try_new(Affine::scale(2.0))?).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    let expected = Element::Stroke {
        path,
        pen,
        linetype: Some(Linetype { dashes: Dashes::try_new(nev![1.0, 2.0])?, alignment: Alignment::Start { dash_offset: DashOffset::try_new(3.0)? }, target: Target::Model }),
    };
    assert_eq!(output, vec![Element::Clip { region, keep: Side::Outside, elements: vec![expected].into() }, model]);
    Ok(())
}

#[test]
fn deep_clip_ownership_preserves_parallel_operations_clone_equality_and_drop() -> Result<(), Box<dyn Error>> {
    let tolerance = Tolerance::try_new(0.01)?;
    let leaf = Element::Fill { region: Region::default(), color: Color::Cmyk(Cmyk { cyan: 0, magenta: 0, yellow: 0, black: 255 }), .. };
    let elements = (0..10_000).fold(vec![leaf], |elements, _| vec![Element::Clip { region: Region::default(), keep: Side::Outside, elements: elements.into() }]);
    let cloned = elements.clone();
    assert_eq!(elements, cloned);
    let rebased = rebase(elements, Placement::default()).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    assert_eq!(rebased, cloned);
    let coincident = cleanup::coincident(rebased, |_| (), tolerance).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    assert_eq!(coincident, cloned);
    let mask = Region::new(Rect::new(0.0, 0.0, 1.0, 1.0), FillRule::NonZero, tolerance)?;
    let clipped = cleanup::clip(coincident, &mask, Side::Outside, tolerance).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    assert_eq!(clipped, []);
    drop(cloned);
    Ok(())
}

#[test]
#[stacksafe::stacksafe]
fn deep_clip_failure_preserves_typed_cause_display_debug_and_drop() -> Result<(), Box<dyn Error>> {
    let path = Path::try_new(Line::new(Point::new(f64::MAX, 0.0), Point::new(f64::MAX, 1.0)).to_path(0.0))?;
    let pen = Pen { width: Width::try_new(0.25)?, color: Color::Cmyk(Cmyk { cyan: 0, magenta: 0, yellow: 0, black: 255 }), .. };
    let elements =
        (0..10_000).fold(vec![Element::Stroke { path, pen, linetype: None }], |elements, _| vec![Element::Clip { region: Region::default(), keep: Side::Outside, elements: elements.into() }]);
    let error = assert_err!(transform(elements, Placement::try_new(Affine::scale(2.0))?)).into_nonempty_iter().next().0;
    let cause = iter::successors(Some(&error), |cause| match cause {
        LineworkError::Element { index: _, errors } | LineworkError::Layer { name: _, errors } => Some(&**errors.first()),
        LineworkError::Sweep(_) | LineworkError::Placement(_) | LineworkError::Scaled(_) | LineworkError::Spacing { family: _, spacing: _ } => None,
    })
    .last();
    assert_matches!(cause, Some(LineworkError::Sweep(linesweeper::Error::Infinity)));
    assert!(error.to_string().ends_with("one of the inputs was infinite"));
    assert!(format!("{error:?}").contains("Infinity"));
    drop(error);
    Ok(())
}
