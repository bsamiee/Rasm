use core::hint::black_box;
use core::num::Wrapping;

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn stepped(value: u32) -> u32 { value.wrapping_add(1) }

const fn flagged(value: i64) -> (i64, bool) { value.overflowing_mul(3) }

const fn qualified(value: u32) -> u32 { u32::wrapping_add(value, 1) }

const fn qualified_overflow(value: i64) -> (i64, bool) { i64::overflowing_mul(value, 3) }

fn wrapped(value: u8) -> Wrapping<u8> { Wrapping(value) + Wrapping(1) }

fn referenced(left: &Wrapping<i16>, right: &Wrapping<i16>) -> Wrapping<i16> { left + right }

fn referenced_negative(value: &Wrapping<i16>) -> Wrapping<i16> { -value }

const fn checked(value: u32) -> Option<u32> { value.checked_add(1) }

const fn saturated(value: u32) -> u32 { value.saturating_add(1) }

const fn offset(cursor: *const u8) -> *const u8 { cursor.wrapping_add(1) }

macro_rules! bumped {
    ($value:expr) => {
        $value.wrapping_sub(1)
    };
}

const fn expanded(value: u16) -> u16 { bumped!(value) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(stepped(1), flagged(2), wrapped(3), checked(4), saturated(5), offset(core::ptr::null()), expanded(6), (signed(7), accumulated(Wrapping(8)), masked(9))));
    black_box((referenced(&Wrapping(1), &Wrapping(2)), referenced_negative(&Wrapping(3))));
    black_box((qualified(1), qualified_overflow(2)));
}

const fn signed(value: u32) -> u32 { value.wrapping_add_signed(-1) }

fn accumulated(mut total: Wrapping<i8>) -> Wrapping<i8> {
    total += Wrapping(1);
    -total
}

fn masked(value: u8) -> Wrapping<u8> { Wrapping(value) & Wrapping(1) }
