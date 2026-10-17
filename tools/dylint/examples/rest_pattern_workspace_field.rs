use core::{hint::black_box, ops::Range};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
struct Window {
    start: u16,
    end: u16,
    label: &'static str,
}

#[derive(Debug, Clone, Copy)]
enum Gesture {
    Drag { from: u8, to: u8 },
    Pinch(u8, u8),
    Tap,
}

mod sealed {
    #[derive(Debug, Clone, Copy)]
    pub(crate) struct Token {
        pub(crate) id: u8,
        pub(crate) scope: u8,
        secret: u8,
    }

    impl Token {
        pub(crate) const fn new(id: u8) -> Self { Self { id, scope: 0, secret: id } }

        pub(crate) const fn secret(self) -> u8 { self.secret }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn rest(window: Window) -> u16 {
    let Window { start, .. } = window;
    start
}

const fn listed(window: Window) -> (u16, &'static str) {
    let Window { start: _, end, label } = window;
    (end, label)
}

const fn variant(gesture: Gesture) -> u8 {
    match gesture {
        Gesture::Drag { from, .. } => from,
        Gesture::Pinch(first, ..) => first,
        Gesture::Tap => 0,
    }
}

const fn target(gesture: Gesture) -> u8 {
    match gesture {
        Gesture::Drag { from: _, to } => to,
        Gesture::Pinch(_, second) => second,
        Gesture::Tap => 0,
    }
}

const fn private(token: sealed::Token) -> u8 {
    let sealed::Token { id, .. } = token;
    id
}

const fn inaccessible(token: sealed::Token) -> u8 {
    let sealed::Token { id: _, scope, .. } = token;
    scope
}

const fn external(range: Range<u8>) -> u8 {
    let Range { start, .. } = range;
    start
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (window, gesture, token) = (Window { start: 0, end: 1, label: "window" }, Gesture::Drag { from: 0, to: 1 }, sealed::Token::new(1));
    black_box(&(rest(window), listed(window), variant(gesture), target(gesture), private(token), inaccessible(token), token.secret(), external(0..1), Gesture::Tap, Gesture::Pinch(0, 1)));
}
