use core::hint::black_box;

// --- [OPERATIONS] ----------------------------------------------------------------------

macro_rules! applied {
    ($transform:expr, $value:expr) => {
        ($transform)($value)
    };
}

macro_rules! bound {
    (| $parameter:ident | $body:expr, $value:expr) => {{
        let $parameter = $value;
        $body
    }};
}

macro_rules! doubled {
    ($value:expr) => {
        (|value: u32| value * 2)($value)
    };
}

struct Handler {
    apply: fn(u32) -> u32,
}

const fn increment(value: u32) -> u32 { value + 1 }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let handler = Handler { apply: increment };
    black_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4), doubled!(5), (|value: u32| value + 1)(6)]);
}
