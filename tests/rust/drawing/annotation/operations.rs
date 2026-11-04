#![feature(default_field_values, error_iter)]

use core::{error::Error, iter};

use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _};
use parlance::{FontFamily, GenericFamily};
use rasm_annotation::{
    AnnotationError, Lookup,
    text::{Output, Typesetter},
};
use rasm_linework::{Cmyk, Color, Element, FontSize, Placement, Region, Side, Text};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn deep_clip_shaping_preserves_success_and_ordered_typed_failures() -> Result<(), Box<dyn Error>> {
    let depth = 10_000;
    let mut typesetter = Typesetter::new([], [], []).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    let color = Color::Cmyk(Cmyk { cyan: 0, magenta: 0, yellow: 0, black: 255 });
    let fill = Element::Fill { region: Region::default(), color: color.clone(), .. };
    let nested = (0..depth).fold(vec![fill.clone()], |elements, _| vec![Element::Clip { region: Region::default(), keep: Side::Outside, elements: elements.into() }]);
    let shaped = typesetter.shape(nested.clone(), Output::Glyphs).map_err(|errors| errors.into_nonempty_iter().next().0)?;
    assert_eq!(shaped, nested);
    drop(shaped);
    drop(nested);

    let families = [GenericFamily::Serif, GenericFamily::SansSerif];
    let size = FontSize::try_new(12.0)?;
    let leaves = Vec::from(families.map(|family| Element::Text {
        text: Text { content: "unregistered".into(), family: FontFamily::Single(family.into()), size, placement: Placement::default(), .. },
        color: color.clone(),
        ..
    }));
    let Err(errors) =
        typesetter.shape((0..depth).fold(leaves, |elements, _| vec![fill.clone(), Element::Clip { region: Region::default(), keep: Side::Outside, elements: elements.into() }]), Output::Glyphs)
    else {
        return Err("Unregistered families shaped successfully".into());
    };
    assert_eq!(errors.len().get(), families.len());
    for (index, (error, family)) in errors.iter().zip(families).enumerate() {
        let trace = <dyn Error>::sources(error).collect::<Vec<_>>();
        let expected = iter::repeat_n("item 1".to_owned(), depth).chain([format!("item {index}"), format!("no family named {family}")]).collect::<Vec<_>>();
        assert_eq!(trace.iter().map(ToString::to_string).collect::<Vec<_>>(), expected);
        assert!(matches!(
            trace.last().and_then(|cause| cause.downcast_ref::<Box<AnnotationError>>()).map(Box::as_ref),
            Some(AnnotationError::Unknown { lookup: Lookup::Family, name }) if name.as_ref() == family.to_string()
        ));
    }
    assert!(format!("{errors:?}").contains("Unknown"));
    drop(errors);
    Ok(())
}
