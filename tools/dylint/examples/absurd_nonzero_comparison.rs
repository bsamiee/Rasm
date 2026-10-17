use core::{hint::black_box, num::NonZero};

// --- [MODELS] --------------------------------------------------------------------------

struct Limits {
    retries: NonZero<u32>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn empty(count: NonZero<u32>) -> bool { count.get() == 0 }

fn present(count: NonZero<u32>) -> bool { count.get() != 0 }

fn positive(count: NonZero<u64>) -> bool { count.get() > 0 }

fn least(count: NonZero<usize>) -> bool { 1 <= count.get() }

fn below(count: NonZero<u8>) -> bool { count.get() < 1 }

fn most(count: NonZero<u8>) -> bool { count.get() <= 0 }

fn minimum() -> bool { NonZero::<u32>::MIN.get() == 0 }

fn retried(limits: &Limits) -> bool { 0 < limits.retries.get() }

fn called(count: NonZero<u16>) -> bool { NonZero::get(count) == 0 }

fn signed(delta: NonZero<i32>) -> bool { delta.get() > 0 }

fn signed_zero(delta: NonZero<i32>) -> bool { delta.get() != 0 }

fn several(count: NonZero<u32>) -> bool { count.get() > 1 }

fn computed(values: &[NonZero<u32>]) -> bool { values.iter().map(|value| value.get()).sum::<u32>() > 0 }

fn converted(count: NonZero<u32>) -> bool { u32::from(count) == 0 }

fn converted_into(count: NonZero<u64>) -> bool { 0 < Into::<u64>::into(count) }

const NONE: u32 = 0;

fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }

macro_rules! empty_count {
    ($count:expr) => {
        $count.get() == 0
    };
}

fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }

fn other_bound(count: NonZero<u32>) -> bool { count.get() != 1 }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (one, limits) = (NonZero::<u32>::MIN, Limits { retries: NonZero::<u32>::MIN });
    black_box(&(
        [empty(one), present(one), positive(NonZero::<u64>::MIN), least(NonZero::<usize>::MIN), below(NonZero::<u8>::MIN), most(NonZero::<u8>::MIN), minimum(), retried(&limits)],
        [called(NonZero::<u16>::MIN), signed(NonZero::<i32>::MIN), signed_zero(NonZero::<i32>::MIN), several(one), computed(&[one])],
        [converted(one), converted_into(NonZero::<u64>::MIN), named_bound(one), expanded(one), other_bound(one)],
    ));
}
