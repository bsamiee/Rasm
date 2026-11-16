use core::cell::{Cell, RefCell};
use core::hint::black_box;
use core::iter::FusedIterator;

// --- [MODELS] --------------------------------------------------------------------------

struct Countdown {
    left: u8,
}

#[derive(Clone)]
struct Repeated {
    left: u8,
}

struct Restart {
    left: u8,
}

struct Hinted {
    left: u8,
}

struct Alternating {
    ready: bool,
}

struct Token;

struct Tokens {
    ready: bool,
}

struct Finished<'count>(&'count Cell<u8>);

struct Branch {
    left: u8,
}

struct Events<'events> {
    log: &'events RefCell<Vec<u8>>,
    tag: u8,
}

impl Drop for Events<'_> {
    fn drop(&mut self) { self.log.borrow_mut().push(self.tag); }
}

struct Dropped<'events> {
    events: Events<'events>,
    left: u8,
}

impl Iterator for Countdown {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.left = self.left.checked_sub(1)?;
        Some(self.left)
    }
}

impl Iterator for Restart {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        *self = Self { left: self.left.checked_sub(1)? };
        Some(self.left)
    }
}

impl Iterator for Hinted {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        let left = self.left.checked_sub(1)?;
        self.left = left;
        Some(left)
    }

    fn size_hint(&self) -> (usize, Option<usize>) { (usize::from(self.left), Some(usize::from(self.left))) }
}

impl Iterator for Repeated {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.left = self.left.checked_sub(1)?;
        Some(self.left)
    }
}

impl Iterator for Alternating {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.ready = !self.ready;
        self.ready.then_some(1)
    }
}

impl Iterator for Tokens {
    type Item = Token;

    fn next(&mut self) -> Option<Token> {
        self.ready = !self.ready;
        self.ready.then_some(Token)
    }
}

impl FromIterator<Token> for Vec<u8> {
    fn from_iter<T: IntoIterator<Item = Token>>(tokens: T) -> Self {
        let mut tokens = tokens.into_iter();
        vec![u8::from(tokens.next().is_some()), u8::from(tokens.next().is_some())]
    }
}

impl Iterator for Finished<'_> {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.0.set(self.0.get().saturating_add(1));
        None
    }
}

impl FusedIterator for Finished<'_> {}

impl Iterator for Branch {
    type Item = Self;

    fn next(&mut self) -> Option<Self> {
        self.left = self.left.checked_sub(1)?;
        Some(Self { left: self.left })
    }
}

impl Iterator for Dropped<'_> {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.events.log.borrow_mut().push(self.left);
        self.left = self.left.checked_sub(1)?;
        Some(self.left)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let counted: u8 = Countdown { left: 3 }.sum();
    let repeated: u8 = Repeated { left: 2 }.sum();
    let restarted: u8 = Iterator::sum(Restart { left: 4 });
    let sized = Hinted { left: 5 }.collect::<Vec<_>>();
    let mut alternating = Alternating { ready: true };
    black_box([alternating.next(), alternating.next(), alternating.next()]);
    black_box(Tokens { ready: true }.collect::<Vec<u8>>());
    let calls = Cell::new(0);
    let mut finished = Finished(&calls);
    black_box([finished.next(), finished.next()]);
    black_box(calls.get());
    black_box(Branch { left: 3 }.fold(0_u8, |count, mut branch| count.saturating_add(u8::from(branch.next().is_some()))));
    let events = RefCell::new(Vec::new());
    let callback = Events { log: &events, tag: 2 };
    black_box(Dropped { events: Events { log: &events, tag: 1 }, left: 1 }.fold(0_u8, move |count, value| {
        black_box(&callback);
        count.saturating_add(value)
    }));
    black_box(events.into_inner());
    black_box(([counted, repeated, restarted], sized));
}
