use core::cell::Cell;
use core::hint::black_box;
use core::ops::Deref;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug)]
enum State {
    Ready(u8),
    Waiting,
    Stopped,
}

#[derive(Debug)]
struct Traced {
    visited: Cell<bool>,
    state: State,
}

#[repr(packed)]
struct Packed {
    prefix: u8,
    value: u32,
}

impl Traced {
    fn is_some(&self) -> bool { self.visited.get() }
}

impl Deref for Traced {
    type Target = State;

    fn deref(&self) -> &State {
        self.visited.update(core::ops::Not::not);
        &self.state
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(not(test))]
fn alternatives(value: &State, states: &[State], index: usize, traced: &Traced) -> impl IntoIterator<Item = bool> {
    [
        matches!(value, State::Ready(_)) || matches!(value, State::Waiting),
        !matches!(value, State::Ready(_)) && !matches!(value, State::Waiting),
        matches!(states[index], State::Ready(_)) | matches!(states[index], State::Waiting),
        matches!(traced.state, State::Ready(1)) || matches!(traced.state, State::Ready(2)) || matches!(traced.state, State::Waiting),
        matches!(*value, State::Ready(_)) || matches!(*value, State::Waiting),
    ]
}

#[cfg(not(test))]
fn effectful(mut values: impl Iterator<Item = State>, traced: &Traced) -> impl IntoIterator<Item = bool> {
    [
        matches!(values.next(), Some(State::Ready(_))) || matches!(values.next(), Some(State::Waiting)),
        matches!(**traced, State::Ready(_)) || matches!(**traced, State::Waiting),
        matches!(traced.state, State::Ready(_) if traced.visited.get()) || matches!(traced.state, State::Waiting),
        matches!(Some(1_u8), Some(_binding)) || matches!(Some(1_u8), None),
    ]
}

#[cfg(not(test))]
fn shadowed(value: &State) -> bool {
    macro_rules! matches {
        ($value:expr, $($pattern:tt)*) => {{
            black_box($value);
            false
        }};
    }
    matches!(value, State::Ready(_)) || matches!(value, State::Waiting)
}

#[cfg(not(test))]
fn bindings(value: itertools::Either<u8, u8>) -> impl IntoIterator<Item = bool> {
    [
        matches!(value, itertools::Either::Left(_part @ 1)) || matches!(value, itertools::Either::Right(_part @ 2)),
        matches!(value, itertools::Either::Left(ref _part @ 1)) || matches!(value, itertools::Either::Right(ref _part @ 2)),
    ]
}

#[cfg(not(test))]
fn borrowed(value: &itertools::Either<String, u8>) -> bool { matches!(value, itertools::Either::Left(_)) || matches!(value, itertools::Either::Right(1)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(not(test))]
fn main() {
    let traced = Traced { visited: Cell::new(false), state: State::Ready(1) };
    black_box((alternatives(&traced.state, &[State::Waiting], 0, &traced), effectful([State::Ready(1), State::Waiting].into_iter(), &traced), shadowed(&State::Stopped)));
    let packed = Packed { prefix: 0, value: 1 };
    black_box((traced.is_some(), bindings(itertools::Either::Left(1)), packed.prefix, packed.value, borrowed(&itertools::Either::Left(String::from("reading")))));
}

#[cfg(test)]
#[test]
fn pattern_assertions() {
    let value = Some(black_box([State::Waiting, State::Stopped]));
    assert!(value.is_some(), "received value");
    debug_assert!(value.is_none());
    assert!(matches!(value, Some(_)));
    assert!(matches!(value, Some(_) if black_box(true)), "guard");
}

#[cfg(test)]
#[test]
fn assertion_contracts() {
    let opaque = black_box(Some(async {}));
    assert!(opaque.is_some());
    assert!(matches!(opaque, Some(_)));
    assert!(matches!(Some(String::from("owned")), None));
    assert!(matches!(&Some(String::from("borrowed")), None));
    assert!(matches!(Box::new(1), _ if black_box(false)));
    let traced = Traced { visited: Cell::new(false), state: State::Ready(1) };
    assert!(traced.is_some());
    assert!(matches!(traced.state, State::Ready(1)));
}

#[cfg(test)]
#[test]
fn aliased_assertions() {
    use core::{assert as require, matches as fits};

    let value = Some(black_box(3_u8));
    require!(fits!(value, Some(_)));
    require!(fits!(value, Some(count) if count > 1));
}

#[cfg(test)]
#[test]
fn shadowed_assertions() {
    macro_rules! assert {
        ($condition:expr) => {
            black_box($condition);
        };
    }
    let value = Some(black_box(3_u8));
    assert!(matches!(value, Some(_)));
}

#[cfg(test)]
#[test]
fn message_borrows() {
    let mut value = Some(black_box(3_u8));
    assert!(value.is_none(), "{:?}", value.take());
    assert!(matches!(value, None), "{:?}", value.take());
    assert!(matches!(value, Some(_)), "{value:?}");
    let alias = &mut value;
    let alias2 = &mut *alias;
    assert!(alias2.is_none(), "{alias:?}");
}

#[cfg(test)]
#[test]
fn packed_field() {
    let packed = Packed { prefix: 0, value: black_box(1) };
    black_box(packed.prefix);
    assert!(matches!(packed.value, 1));
}

#[cfg(test)]
#[test]
fn shadowed_option_constructors() {
    enum Other {
        Some,
        None,
    }
    use Other::{None, Some};

    let present = Option::Some(black_box(3_u8));
    let absent = Option::<u8>::None;
    assert!(present.is_some());
    assert!(absent.is_none());
    black_box((Some, None));
}

#[cfg(test)]
#[test]
fn constant_assertions() {
    const fn required(value: Option<u8>) {
        assert!(value.is_some());
        assert!(matches!(value, Some(_)));
    }

    const {
        required(Some(1));
        let value = Some(2_u8);
        assert!(value.is_some());
        assert!(matches!(value, Some(_)));
    }
}
