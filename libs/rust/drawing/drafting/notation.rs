
use core::{any::TypeId, iter, num::NonZero, ops::Div};

use itertools::{Itertools as _, Position};
use rasm_drawing::Visibility;
use rust_decimal::{
    RoundingStrategy,
    prelude::{FromPrimitive as _, ToPrimitive as _},
};
use uom::{
    Conversion,
    si::{
        self, Unit as _,
        angle::{self, degree, minute, radian, second},
        area,
        f64::{Angle, Area, Length, Ratio, Volume},
        length::{self, inch},
        ratio::{percent, ratio},
        volume,
    },
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Subdivision {
    Minute,
    Second,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Separator {
    #[display(".")]
    Point,
    #[display(",")]
    Comma,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Grouping {
    #[display("\u{202f}")]
    Space,
    #[display(",")]
    Comma,
    #[display(".")]
    Point,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Sign {
    Negative,
    Explicit,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Unit<Quantity> {
    one: Quantity,
    symbol: &'static str,
    spaced: bool,
}

impl Unit<Length> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: length::Unit + Conversion<f64, T = f64> {
        Self { one: Length::new::<N>(1.0), symbol: N::abbreviation(), spaced: true }
    }
}

impl Unit<Area> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: area::Unit + Conversion<f64, T = f64> {
        Self { one: Area::new::<N>(1.0), symbol: N::abbreviation(), spaced: true }
    }
}

impl Unit<Volume> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: volume::Unit + Conversion<f64, T = f64> {
        Self { one: Volume::new::<N>(1.0), symbol: N::abbreviation(), spaced: true }
    }
}

impl Unit<Angle> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: angle::Unit + Conversion<f64, T = f64> + 'static {
        Self { one: Angle::new::<N>(1.0), symbol: N::abbreviation(), spaced: ![TypeId::of::<degree>(), TypeId::of::<minute>(), TypeId::of::<second>()].contains(&TypeId::of::<N>()) }
    }
}

#[nutype::nutype(validate(predicate = |step: &rust_decimal::Decimal| step.is_sign_positive() && !step.is_zero()), derive(Debug, Clone, Copy, PartialEq, Eq, Hash))]
pub struct Increment(rust_decimal::Decimal);

#[nutype::nutype(validate(predicate = |denominator: &NonZero<u16>| denominator.is_power_of_two()), derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord))]
pub struct Denominator(NonZero<u16>);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Decimal {
    pub increment: Increment,
    pub leading: Visibility = Visibility::Visible,
    pub trailing: Visibility = Visibility::Visible,
    pub separator: Separator = Separator::Point,
    pub grouping: Option<Grouping> = None,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Inches {
    Fractional(Denominator),
    Decimal(Decimal),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Zeros {
    pub feet: Visibility = Visibility::Visible,
    pub inches: Visibility = Visibility::Visible,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Linear {
    Decimal {
        unit: Unit<Length>,
        decimal: Decimal,
        symbol: Visibility = Visibility::Hidden,
    },
    Imperial {
        inches: Inches,
        feet: Option<Zeros> = Some(Zeros { .. }),
    },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Angular {
    Decimal {
        unit: Unit<Angle>,
        decimal: Decimal,
    },
    Sexagesimal {
        subdivision: Subdivision,
        decimal: Decimal,
    },
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Slope {
    Ratio(Decimal),
    Percent(Decimal),
    Pitch {
        run: NonZero<u16>,
        decimal: Decimal,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn decimal(notation: &Decimal, value: f64, sign: Sign) -> String {
    rounded(value, notation.increment.into_inner()).map_or_else(|| value.to_string(), |total| format!("{}{}", signed(total, sign), written(notation, total.abs())))
}

#[must_use]
pub fn quantity<D>(notation: &Decimal, unit: Unit<si::Quantity<D, si::SI<f64>, f64>>, symbol: Visibility, value: si::Quantity<D, si::SI<f64>, f64>, sign: Sign) -> String
where
    D: si::Dimension + ?Sized,
    si::Quantity<D, si::SI<f64>, f64>: Div<Output = Ratio>,
{
    let number = decimal(notation, (value / unit.one).get::<ratio>(), sign);
    match symbol {
        Visibility::Visible => format!("{number}{}{}", if unit.spaced { " " } else { "" }, unit.symbol),
        Visibility::Hidden => number,
    }
}

#[must_use]
pub fn linear(notation: &Linear, length: Length, sign: Sign) -> String {
    fn imperial(sign: &str, magnitude: rust_decimal::Decimal, feet: Option<Zeros>, exact: bool, inches: impl Fn(rust_decimal::Decimal, bool) -> String) -> String {
        match feet.zip(split(magnitude, rust_decimal::Decimal::from(12))) {
            None => format!("{sign}{}\"", inches(magnitude, false)),
            Some((zeros, (count, rest))) => match (zeros.feet == Visibility::Visible || !count.is_zero(), exact && rest.is_zero() && zeros.inches == Visibility::Hidden) {
                (false, _) => format!("{sign}{}\"", inches(rest, false)),
                (true, true) => format!("{sign}{count}'"),
                (true, false) => format!("{sign}{count}'-{}\"", inches(rest, true)),
            },
        }
    }
    match *notation {
        Linear::Decimal { unit, decimal: digits, symbol } => quantity(&digits, unit, symbol, length, sign),
        Linear::Imperial { inches: Inches::Fractional(denominator), feet } => {
            let value = length.get::<inch>();
            let parts = denominator.into_inner();
            let Some((count, (whole, numerator))) = rounded(value * f64::from(parts.get()), rust_decimal::Decimal::ONE).and_then(|count| Some((count, split(count.abs(), parts.get().into())?))) else {
                return value.to_string();
            };
            let fraction = numerator.to_u16().and_then(NonZero::new).map(|numerator| {
                let shift = numerator.trailing_zeros().min(parts.trailing_zeros());
                format!("{}/{}", numerator.get().strict_shr(shift), parts.get().strict_shr(shift))
            });
            imperial(signed(count, sign), whole, feet, fraction.is_none(), |inches, shown| match &fraction {
                Some(reduced) if !shown && inches.is_zero() => reduced.clone(),
                Some(reduced) => format!("{inches} {reduced}"),
                None => inches.to_string(),
            })
        }
        Linear::Imperial { inches: Inches::Decimal(digits), feet } => {
            let value = length.get::<inch>();
            rounded(value, digits.increment.into_inner()).map_or_else(|| value.to_string(), |total| imperial(signed(total, sign), total.abs(), feet, true, |inches, _| written(&digits, inches)))
        }
    }
}

#[must_use]
pub fn angular(notation: &Angular, angle: Angle, sign: Sign) -> String {
    match *notation {
        Angular::Decimal { unit, decimal: digits } => quantity(&digits, unit, Visibility::Visible, angle, sign),
        Angular::Sexagesimal { subdivision, decimal: digits } => {
            let (rank, value) = match subdivision {
                Subdivision::Minute => (1, angle.get::<minute>()),
                Subdivision::Second => (2, angle.get::<second>()),
            };
            let sixty = rust_decimal::Decimal::from(60);
            let Some((total, (degrees, places))) = rounded(value, digits.increment.into_inner()).and_then(|total| {
                let parts = (0..rank).try_fold((total.abs(), Vec::new()), |(rest, mut places), _| {
                    split(rest, sixty).map(|(quotient, remainder)| {
                        places.push(remainder);
                        (quotient, places)
                    })
                })?;
                Some((total, parts))
            }) else {
                return value.to_string();
            };
            let numbers = iter::once(degrees.to_string()).chain(places.iter().rev().with_position().map(|(position, place)| match position {
                Position { is_first: _, is_last: true } => written(&digits, *place),
                Position { is_first: _, is_last: false } => place.to_string(),
            }));
            let marks = [degree::abbreviation(), minute::abbreviation(), second::abbreviation()];
            format!("{}{}", signed(total, sign), numbers.zip(marks).map(|(number, mark)| format!("{number}{mark}")).join(""))
        }
    }
}

#[must_use]
pub fn bearing(notation: &Angular, azimuth: Angle) -> String {
    let [half, full] = [Angle::HALF_TURN, Angle::FULL_TURN];
    let quarter = half / 2.0;
    let quadrant = match Angle::new::<radian>(azimuth.get::<radian>().rem_euclid(full.get::<radian>())) {
        turned if turned < quarter => Some(('N', turned, 'E')),
        turned if turned < half => Some(('S', half - turned, 'E')),
        turned if turned < half + quarter => Some(('S', turned - half, 'W')),
        turned if turned <= full => Some(('N', full - turned, 'W')),
        _ => None,
    };
    quadrant.map_or_else(|| angular(notation, azimuth, Sign::Negative), |(meridian, reduced, departure)| format!("{meridian} {} {departure}", angular(notation, reduced, Sign::Negative)))
}

#[must_use]
pub fn slope(notation: &Slope, gradient: Ratio) -> String {
    let rise = gradient.abs();
    match *notation {
        Slope::Ratio(digits) => format!("1:{}", decimal(&digits, rise.get::<ratio>().recip(), Sign::Negative)),
        Slope::Percent(digits) => format!("{}%", decimal(&digits, rise.get::<percent>(), Sign::Negative)),
        Slope::Pitch { run, decimal: digits } => format!("{}:{run}", decimal(&digits, rise.get::<ratio>() * f64::from(run.get()), Sign::Negative)),
    }
}

fn rounded(value: f64, step: rust_decimal::Decimal) -> Option<rust_decimal::Decimal> {
    rust_decimal::Decimal::from_f64(value)?.checked_div(step)?.round_dp_with_strategy(0, RoundingStrategy::MidpointAwayFromZero).checked_mul(step)
}

fn split(value: rust_decimal::Decimal, radix: rust_decimal::Decimal) -> Option<(rust_decimal::Decimal, rust_decimal::Decimal)> { Some((value.checked_div(radix)?.trunc(), value.checked_rem(radix)?)) }

const fn signed(value: rust_decimal::Decimal, sign: Sign) -> &'static str {
    match (sign, value.is_zero(), value.is_sign_negative()) {
        (Sign::Negative | Sign::Explicit, true, _) | (Sign::Negative, false, false) => "",
        (Sign::Negative | Sign::Explicit, false, true) => "-",
        (Sign::Explicit, false, false) => "+",
    }
}

fn written(notation: &Decimal, magnitude: rust_decimal::Decimal) -> String {
    let Decimal { increment: _, leading, trailing, separator, grouping } = *notation;
    let shown = match trailing {
        Visibility::Visible => magnitude,
        Visibility::Hidden => magnitude.normalize(),
    };
    let text = shown.to_string();
    let (integer, decimals) = match text.split_once('.') {
        Some((integer, decimals)) => (integer, Some(decimals)),
        None => (text.as_str(), None),
    };
    let whole = match (leading, decimals.is_some() && shown.trunc().is_zero()) {
        (Visibility::Hidden, true) => String::new(),
        (Visibility::Visible, true | false) | (Visibility::Hidden, false) =>
            grouping.map_or_else(|| integer.to_owned(), |mark| integer.chars().collect::<Vec<_>>().rchunks(3).rev().map(|group| group.iter().collect::<String>()).join(&mark.to_string())),
    };
    match decimals {
        Some(decimals) => format!("{whole}{separator}{decimals}"),
        None => whole,
    }
}
