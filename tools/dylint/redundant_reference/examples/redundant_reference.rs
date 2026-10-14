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
    fn budget(&self) -> u32 { u32::from(self.settings.port) + u32::from(self.settings.retries) }
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
    assert_eq!(walker.interner().names.len() + context.depth, 2, "walker reads the interner");
    assert_eq!((shared.port(), shared.label()), (0, "shared"), "shared reads the port and label");
    assert_eq!(Named { settings: &settings }.name(), "bridge", "named reads the name");
    assert_eq!(exposed.port(), 0, "exposed reads the port");
    assert_eq!(Paired { settings: &settings }.budget(), 3, "paired reads the port and retries");
    assert_eq!(Lent { settings: &settings }.settings().retries, 3, "lent returns the settings");
    assert_eq!((destructured.port(), destructured.settings().port), (0, 0), "destructured reads the port");
    assert_eq!((holder.pointed.port(), holder.settings().retries), (0, 3), "holder returns the settings");
    assert_eq!(Derived { settings: &settings }.clone().port(), 0, "derived reads the port");
    assert_eq!(Kernel { matrix: &Matrix { cells: [1; 64] } }.cell(0), Some(1), "kernel reads one cell");
}
