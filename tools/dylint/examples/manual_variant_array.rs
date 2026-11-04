use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, strum::VariantArray)]
enum Mode {
    Fast,
    Slow,
}

impl Mode {
    const VARIANTS: &'static [Self] = &[Self::Fast];
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Measure<const DIM: usize> {
    Low,
    High,
}

#[derive(Debug, PartialEq, Eq)]
enum State {
    Open,
    Closed,
}

#[derive(Debug, PartialEq, Eq)]
enum Resource {
    Open,
    Closed,
}

impl Drop for Resource {
    fn drop(&mut self) { black_box(self); }
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, strum::VariantArray)]
enum Visible {
    Shown,
    #[strum(disabled)]
    Hidden,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALL: [Mode; 2] = [Mode::Fast, Mode::Slow];
const HOT: [Mode; 1] = [Mode::Fast];
const PRIORITY: [Mode; 2] = [Mode::Slow, Mode::Fast];

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&([ALL.len(), HOT.len(), PRIORITY.len(), Mode::VARIANTS.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow)));
    black_box([Measure::<4>::Low, Measure::<4>::High].contains(&Measure::<4>::High));
    black_box([Mode::Fast, Mode::Slow].into_iter().count());
    black_box([State::Open, State::Closed].contains(&State::Open));
    black_box([Resource::Open, Resource::Closed].into_iter().take(1).count());
    black_box([Visible::Shown, Visible::Hidden].contains(&Visible::Hidden));
}
