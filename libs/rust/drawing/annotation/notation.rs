use core::{iter, num::NonZero};

use itertools::Itertools as _;
use rasm_linework::Visibility;
use uom::{
    Conversion,
    si::{
        Unit as _,
        angle::{self, degree, minute, radian, second},
        f64::{Angle, Length},
        length::{self, inch},
        ratio::ratio,
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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Unit<Measure> {
    one: Measure,
    symbol: &'static str,
}

impl Unit<Length> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: length::Unit + Conversion<f64, T = f64> {
        Self { one: Length::new::<N>(1.0), symbol: N::abbreviation() }
    }
}

impl Unit<Angle> {
    #[must_use]
    pub fn new<N>() -> Self
    where N: angle::Unit + Conversion<f64, T = f64> {
        Self { one: Angle::new::<N>(1.0), symbol: N::abbreviation() }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Increment {
    pub significand: NonZero<u32>,
    pub exponent: i8,
}

#[nutype::nutype(validate(predicate = |denominator: &NonZero<u16>| denominator.is_power_of_two()), derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord))]
pub struct Denominator(NonZero<u16>);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Decimal {
    pub increment: Increment,
    pub leading: Visibility = Visibility::Visible,
    pub trailing: Visibility = Visibility::Visible,
    pub separator: Separator = Separator::Point,
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
    Decimal { unit: Unit<Length>, decimal: Decimal, symbol: Visibility = Visibility::Hidden },
    Imperial { inches: Inches, feet: Option<Zeros> = Some(Zeros { .. }) },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Angular {
    Decimal { unit: Unit<Angle>, decimal: Decimal },
    Sexagesimal { subdivision: Subdivision, decimal: Decimal },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn decimal(notation: &Decimal, value: f64) -> String {
    rounded(value, notation.increment).map_or_else(|| value.to_string(), |(sign, whole, fraction)| format!("{sign}{}", written(notation, &whole, &fraction)))
}

#[must_use]
pub fn linear(notation: &Linear, length: Length) -> String {
    match *notation {
        Linear::Decimal { unit, decimal: rounding, symbol } => {
            let number = decimal(&rounding, (length / unit.one).get::<ratio>());
            match symbol {
                Visibility::Visible => marked(&number, unit.symbol),
                Visibility::Hidden => number,
            }
        }
        Linear::Imperial { inches: field, feet } => {
            let measure = length.get::<inch>();
            let (scale, increment) = match field {
                Inches::Fractional(denominator) => (f64::from(denominator.into_inner().get()), Increment { significand: NonZero::<u32>::MIN, exponent: 0 }),
                Inches::Decimal(rounding) => (1.0, rounding.increment),
            };
            let Some((sign, whole, fraction)) = rounded(measure * scale, increment) else { return measure.to_string() };
            let (inches, reduced) = match field {
                Inches::Fractional(denominator) => {
                    let denominator = denominator.into_inner();
                    let (inches, remainder) = divided(&whole, denominator.into());
                    let lowest = |numerator: NonZero<u64>| {
                        let shift = numerator.trailing_zeros().min(denominator.trailing_zeros());
                        (numerator.get().strict_shr(shift), denominator.get().strict_shr(shift))
                    };
                    (inches, NonZero::new(remainder).map(lowest))
                }
                Inches::Decimal(_) => (whole, None),
            };
            let (count, inches) = match feet {
                Some(_) => {
                    let (count, rest) = divided(&inches, const { NonZero::new(12).unwrap() });
                    (count, vec![rest])
                }
                None => (Vec::new(), inches),
            };
            let zero = reduced.is_none() && iter::chain(&inches, &fraction).all(|&digit| digit == 0);
            let foot = integral(&count);
            let shown = feet.is_some_and(|zeros| zeros.feet == Visibility::Visible || foot != "0");
            let elided = shown && zero && feet.is_some_and(|zeros| zeros.inches == Visibility::Hidden);
            let integer = integral(&inches);
            let written_inches = match (field, reduced) {
                (Inches::Decimal(rounding), _) => written(&rounding, &inches, &fraction),
                (Inches::Fractional(_), Some((numerator, denominator))) if !shown && integer == "0" => format!("{numerator}/{denominator}"),
                (Inches::Fractional(_), Some((numerator, denominator))) => format!("{integer} {numerator}/{denominator}"),
                (Inches::Fractional(_), None) => integer,
            };
            match (shown, elided) {
                (false, _) => format!("{sign}{written_inches}\""),
                (true, true) => format!("{sign}{foot}'"),
                (true, false) => format!("{sign}{foot}'-{written_inches}\""),
            }
        }
    }
}

#[must_use]
pub fn angular(notation: &Angular, angle: Angle) -> String {
    match *notation {
        Angular::Decimal { unit, decimal: rounding } => marked(&decimal(&rounding, (angle / unit.one).get::<ratio>()), unit.symbol),
        Angular::Sexagesimal { subdivision, decimal: rounding } => {
            let sexagesimal = const { NonZero::new(60).unwrap() };
            let measure = match subdivision {
                Subdivision::Minute => angle.get::<minute>(),
                Subdivision::Second => angle.get::<second>(),
            };
            let [degrees_mark, minutes_mark, seconds_mark] = [degree::abbreviation(), minute::abbreviation(), second::abbreviation()];
            let Some((sign, whole, fraction)) = rounded(measure, rounding.increment) else { return measure.to_string() };
            match subdivision {
                Subdivision::Minute => {
                    let (degrees, minutes) = divided(&whole, sexagesimal);
                    format!("{sign}{}{degrees_mark}{}{minutes_mark}", integral(&degrees), written(&rounding, &[minutes], &fraction))
                }
                Subdivision::Second => {
                    let (total, seconds) = divided(&whole, sexagesimal);
                    let (degrees, minutes) = divided(&total, sexagesimal);
                    format!("{sign}{}{degrees_mark}{minutes}{minutes_mark}{}{seconds_mark}", integral(&degrees), written(&rounding, &[seconds], &fraction))
                }
            }
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
    quadrant.map_or_else(|| angular(notation, azimuth), |(meridian, reduced, departure)| format!("{meridian} {} {departure}", angular(notation, reduced)))
}

fn rounded(value: f64, increment: Increment) -> Option<(&'static str, Vec<u64>, Vec<u64>)> {
    let digits = |text: &str| text.bytes().map(|byte| u64::from(byte.strict_sub(b'0'))).collect::<Vec<_>>();
    let Increment { significand, exponent } = increment;
    let magnitude = usize::from(exponent.unsigned_abs());
    let (places, zeros) = if exponent.is_negative() { (magnitude, 0) } else { (0, magnitude) };
    let divisor = NonZero::<u64>::from(significand);
    value.is_finite().then(|| {
        let written = value.abs().to_string();
        let (whole, fraction) = written.split_once('.').unwrap_or((&written, ""));
        let shifted = iter::chain(digits(whole), digits(fraction)).chain(iter::repeat_n(0, places)).collect::<Vec<_>>();
        let (kept, next) = match shifted.len().checked_sub(fraction.len().strict_add(zeros)).and_then(|at| shifted.split_at_checked(at)) {
            Some((kept, [next, ..])) => (kept, *next),
            Some((kept, [])) => (kept, 0),
            None => ([].as_slice(), 0),
        };
        let (quotient, remainder) = divided(kept, divisor);
        let carry = if remainder.strict_mul(2).strict_add(u64::from(next >= 5)) >= divisor.get() { divisor.get() } else { 0 };
        let (product, overflow) = quotient.iter().rev().fold((Vec::with_capacity(quotient.len()), carry), |(mut product, carry), &digit| {
            let current = digit.strict_mul(divisor.get()).strict_add(carry);
            product.push(current % 10);
            (product, current / 10)
        });
        let mut total = iter::chain(digits(&overflow.to_string()), product.into_iter().rev()).chain(iter::repeat_n(0, zeros)).collect::<Vec<_>>();
        let decimals = total.split_off(total.len().strict_sub(places));
        let sign = if value.is_sign_negative() && iter::chain(&total, &decimals).any(|&digit| digit != 0) { "-" } else { "" };
        (sign, total, decimals)
    })
}

fn written(notation: &Decimal, whole: &[u64], fraction: &[u64]) -> String {
    let Decimal { increment: _, leading, trailing, separator } = *notation;
    let integer = integral(whole);
    let digits = fraction.iter().join("");
    let decimals = match trailing {
        Visibility::Visible => digits.as_str(),
        Visibility::Hidden => digits.trim_end_matches('0'),
    };
    match (decimals, leading) {
        ("", Visibility::Visible | Visibility::Hidden) => integer,
        (_, Visibility::Hidden) if integer == "0" => format!("{separator}{decimals}"),
        (_, Visibility::Visible | Visibility::Hidden) => format!("{integer}{separator}{decimals}"),
    }
}

fn integral(whole: &[u64]) -> String {
    let digits = whole.iter().join("");
    match digits.trim_start_matches('0') {
        "" => "0".to_owned(),
        significant => significant.to_owned(),
    }
}

fn marked(number: &str, symbol: &str) -> String {
    if [degree::abbreviation(), minute::abbreviation(), second::abbreviation()].contains(&symbol) { format!("{number}{symbol}") } else { format!("{number} {symbol}") }
}

fn divided(dividend: &[u64], divisor: NonZero<u64>) -> (Vec<u64>, u64) {
    dividend.iter().fold((Vec::with_capacity(dividend.len()), 0), |(mut quotient, remainder), &digit| {
        let current = remainder.strict_mul(10).strict_add(digit);
        quotient.push(current / divisor);
        (quotient, current % divisor)
    })
}
