use core::hint::black_box;
use core::num::NonZero;

// --- [MODELS] --------------------------------------------------------------------------

struct Limits {
    retries: NonZero<u32>,
}

struct Dereferenced(Limits);

impl core::ops::Deref for Dereferenced {
    type Target = Limits;

    fn deref(&self) -> &Self::Target {
        black_box(());
        &self.0
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn empty(count: NonZero<u32>) -> bool { count.get() == 0 } //~ absurd_nonzero_comparison always `false`

fn positive(count: NonZero<u64>) -> bool { count.get() > 0 } //~ absurd_nonzero_comparison always `true`

fn least(count: NonZero<usize>) -> bool { 1 <= count.get() } //~ absurd_nonzero_comparison always `true`

fn below(count: NonZero<u8>) -> bool { count.get() < 1 } //~ absurd_nonzero_comparison always `false`

fn most(count: NonZero<u8>) -> bool { count.get() <= 0 } //~ absurd_nonzero_comparison always `false`

fn called(count: NonZero<u16>) -> bool { NonZero::get(count) == 0 } //~ absurd_nonzero_comparison always `false`

fn signed(delta: NonZero<i32>) -> bool { delta.get() > 0 }

fn signed_below(delta: NonZero<i32>) -> bool { delta.get() < 1 }

fn signed_zero(delta: NonZero<i32>) -> bool { delta.get() != 0 } //~ absurd_nonzero_comparison always `true`

fn several(count: NonZero<u32>) -> bool { count.get() > 1 }

fn computed(values: &[NonZero<u32>]) -> bool { values.iter().map(|value| value.get()).sum::<u32>() > 0 }

fn converted(count: NonZero<u32>) -> bool { u32::from(count) == 0 } //~ absurd_nonzero_comparison always `false`

fn converted_into(count: NonZero<u64>) -> bool { 0 < Into::<u64>::into(count) } //~ absurd_nonzero_comparison always `true`

fn named_bound(count: NonZero<u32>) -> bool {
    const NONE: u32 = 0;
    count.get() == NONE //~ absurd_nonzero_comparison always `false`
}

macro_rules! empty_count {
    ($count:expr) => {
        //~v absurd_nonzero_comparison always `false`
        $count.get() == 0
    };
}

fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }

fn other_bound(count: NonZero<u32>) -> bool { count.get() != 1 }

fn indexed(values: &[NonZero<u32>], index: impl Fn() -> usize) -> bool { values[index()].get() != 0 } //~ absurd_nonzero_comparison retain operand evaluation and replace comparison with `true`

fn projected(value: &Dereferenced) -> bool { value.retries.get() > 0 } //~ absurd_nonzero_comparison retain operand evaluation and replace comparison with `true`

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let one = NonZero::<u32>::MIN;
    black_box(&(
        [empty(one), positive(NonZero::<u64>::MIN), least(NonZero::<usize>::MIN), below(NonZero::<u8>::MIN), most(NonZero::<u8>::MIN), signed_below(NonZero::<i32>::MIN)],
        [called(NonZero::<u16>::MIN), signed(NonZero::<i32>::MIN), signed_zero(NonZero::<i32>::MIN), several(one), computed(&[one])],
        [converted(one), converted_into(NonZero::<u64>::MIN), named_bound(one), expanded(one), other_bound(one)],
    ));
    black_box((indexed(&[one], || 0), projected(&Dereferenced(Limits { retries: one }))));
}
