use core::hint::black_box;
use std::collections::BTreeMap;

use strum::{IntoEnumIterator as _, VariantArray as _};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, strum::EnumIter, strum::VariantArray)]
enum Unit {
    Meter,
    Foot,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, strum::EnumIter)]
enum Level {
    Low,
    High,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn weight(unit: Unit) -> u32 {
    match unit {
        Unit::Meter => 1,
        Unit::Foot => 3,
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let iterated = Unit::iter().map(|unit| (unit, weight(unit))).collect::<BTreeMap<_, _>>();
    let listed = Unit::VARIANTS.iter().map(|unit| (*unit, weight(*unit))).collect::<BTreeMap<_, _>>();
    let copied = Unit::VARIANTS.iter().copied().map(|unit| (unit, 0_u32)).collect::<BTreeMap<_, _>>();
    let keyed_by_value = Level::iter().map(|level| (u32::from(level == Level::High), level)).collect::<BTreeMap<_, _>>();
    let filtered = Level::iter().filter(|&level| level == Level::Low).map(|level| (level, 0_u32)).collect::<BTreeMap<_, _>>();
    let zipped = Unit::iter().zip([1_u32, 3]).collect::<BTreeMap<_, _>>();
    black_box(&(iterated, listed, copied, keyed_by_value, filtered, zipped));
}
