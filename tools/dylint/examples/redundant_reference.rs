use core::hint::black_box;
use std::rc::Rc;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug)]
struct Interner<'tcx> {
    names: &'tcx Vec<&'tcx str>,
}

#[derive(Debug)]
struct Context<'tcx> {
    tcx: Interner<'tcx>,
    depth: usize,
}

#[derive(Debug)]
pub struct Settings {
    name: String,
    port: u16,
    retries: u8,
}

struct Walker<'cx, 'tcx> {
    cx: &'cx Context<'tcx>,
}

struct Shared<'settings> {
    settings: &'settings Settings,
    label: &'settings str,
}

struct Named<'settings> {
    settings: &'settings Settings,
}

#[derive(Debug)]
pub struct Exposed<'settings> {
    pub settings: &'settings Settings,
}

struct Exclusive<'settings> {
    settings: &'settings mut Settings,
}

struct Paired<'settings> {
    settings: &'settings Settings,
}

struct Lent<'settings> {
    settings: &'settings Settings,
}

struct Destructured<'settings> {
    settings: &'settings Settings,
}

struct Pointed<'settings> {
    settings: &'settings Settings,
}

struct Holder<'settings> {
    pointed: Rc<Pointed<'settings>>,
}

#[derive(Clone, Debug)]
struct Derived<'settings> {
    settings: &'settings Settings,
}

#[derive(Clone, Copy, Debug)]
struct Matrix {
    cells: [u64; 64],
}

struct Kernel<'matrix> {
    matrix: &'matrix Matrix,
}

struct Lifted<'settings> {
    settings: &'settings Settings,
}

struct Rendered<'settings> {
    settings: &'settings Settings,
}

struct Tagged<'settings>(&'settings Settings);

struct Unpacked<'settings>(&'settings Settings);

// --- [OPERATIONS] ----------------------------------------------------------------------

impl<'tcx> Walker<'_, 'tcx> {
    const fn interner(&self) -> Interner<'tcx> { self.cx.tcx }
}

impl Shared<'_> {
    const fn port(&self) -> u16 { self.settings.port }

    const fn label(&self) -> &str { self.label }
}

impl Named<'_> {
    fn name(&self) -> String { self.settings.name.clone() }
}

impl Exposed<'_> {
    const fn port(&self) -> u16 { self.settings.port }
}

impl Exclusive<'_> {
    const fn reset(&mut self) { self.settings.port = 0; }
}

impl Paired<'_> {
    fn budget(&self) -> u32 { u32::from(self.settings.port).strict_add(u32::from(self.settings.retries)) }
}

impl Lent<'_> {
    const fn settings(&self) -> &Settings { self.settings }
}

impl Destructured<'_> {
    const fn port(&self) -> u16 { self.settings.port }

    const fn settings(&self) -> &Settings {
        let Self { settings } = self;
        settings
    }
}

impl Pointed<'_> {
    const fn port(&self) -> u16 { self.settings.port }
}

impl<'settings> Holder<'settings> {
    fn settings(&self) -> &'settings Settings { self.pointed.settings }
}

impl Derived<'_> {
    const fn port(&self) -> u16 { self.settings.port }
}

impl Kernel<'_> {
    fn cell(&self, index: usize) -> Option<u64> { self.matrix.cells.get(index).copied() }
}

impl<'settings> Lifted<'settings> {
    const fn port(&self) -> &'settings u16 { &self.settings.port }
}

impl Rendered<'_> {
    fn port(&self) -> String { self.settings.port.to_string() }
}

impl Tagged<'_> {
    const fn retries(&self) -> u8 { self.0.retries }
}

impl Unpacked<'_> {
    const fn retries(&self) -> u8 { self.0.retries }

    const fn settings(&self) -> &Settings {
        let Self(settings) = self;
        settings
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let names = vec!["root"];
    let context = Context { tcx: Interner { names: &names }, depth: 1 };
    let walker = Walker { cx: &context };
    let mut settings = Settings { name: "bridge".to_owned(), port: 8080, retries: 3 };
    Exclusive { settings: &mut settings }.reset();
    let shared = Shared { settings: &settings, label: "shared" };
    let exposed = Exposed { settings: &settings };
    let destructured = Destructured { settings: &settings };
    let holder = Holder { pointed: Rc::new(Pointed { settings: &settings }) };
    black_box((
        (walker.interner().names.len(), context.depth),
        (shared.port(), shared.label()),
        Named { settings: &settings }.name(),
        exposed.port(),
        Paired { settings: &settings }.budget(),
        Lent { settings: &settings }.settings().retries,
        (destructured.port(), destructured.settings().port),
        (holder.pointed.port(), holder.settings().retries),
        Derived { settings: &settings }.clone().port(),
    ));
    let unpacked = Unpacked(&settings);
    black_box((
        Kernel { matrix: &Matrix { cells: [1; 64] } }.cell(0),
        *Lifted { settings: &settings }.port(),
        Tagged(&settings).retries(),
        Rendered { settings: &settings }.port(),
        (unpacked.retries(), unpacked.settings().port),
    ));
}
