use core::hint::black_box;
use std::collections::BTreeMap;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
enum Unit {
    Meter,
    Foot,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
enum Level {
    Low,
    High,
}

#[derive(Clone, Copy, Debug)]
enum Tone {
    Warm,
    Cool,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Mode {
    Fast,
    Slow,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALL: [Mode; 2] = [Mode::Fast, Mode::Slow];
const HOT: [Mode; 1] = [Mode::Fast];
const PRIORITY: [Mode; 2] = [Mode::Slow, Mode::Fast];

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Unit {
    const fn factor(self) -> f64 {
        match self {
            Self::Meter => 1.0,
            Self::Foot => 0.3048,
        }
    }

    const fn offset(self) -> f64 {
        match self {
            Self::Meter => 0.0,
            Self::Foot => 0.5,
        }
    }

    const fn symbol(self) -> &'static str {
        match self {
            Self::Meter => "m",
            Self::Foot => "ft",
        }
    }
}

impl Level {
    const fn threshold(self) -> u8 {
        match self {
            Self::Low => 10,
            Self::High => 90,
        }
    }

    const fn label(self) -> &'static str {
        match self {
            Self::Low => "low",
            Self::High => "high",
        }
    }
}

impl Tone {
    const fn hue(self) -> u16 {
        match self {
            Self::Warm => 30,
            Self::Cool => 210,
        }
    }

    fn scaled(self, value: f64) -> f64 {
        match self {
            Self::Warm => value,
            Self::Cool => value * f64::from(self.hue()),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let counts = BTreeMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]);
    let partial = BTreeMap::from([(Level::Low, 1_u32)]);
    let measured = [Unit::Foot, Unit::Meter].iter().map(|unit| unit.factor() * unit.offset() * f64::from(u32::try_from(unit.symbol().len()).unwrap_or_default())).sum::<f64>();
    let leveled = [Level::High, Level::Low].iter().filter(|&&level| level == Level::High).map(|level| (level.threshold(), level.label())).collect::<Vec<_>>();
    let toned = Tone::Warm.scaled(1.0) + Tone::Cool.scaled(2.0);
    black_box((measured, toned, Tone::Cool.hue(), leveled, counts, partial, [ALL.len(), HOT.len(), PRIORITY.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow)));
}
