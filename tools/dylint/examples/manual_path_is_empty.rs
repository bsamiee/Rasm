use core::{cell::Cell, hint::black_box, ops::Deref};
use std::path::{Component, Path, PathBuf};

// --- [MODELS] --------------------------------------------------------------------------

struct Projected {
    path: PathBuf,
    reads: Cell<u32>,
}

impl Deref for Projected {
    type Target = Path;
    fn deref(&self) -> &Self::Target {
        self.reads.update(|reads| reads + 1);
        &self.path
    }
}

impl Projected {
    fn is_empty(&self) -> bool { true }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn blank(path: &Path) -> bool { path.as_os_str().is_empty() }

fn blank_owned(path: &PathBuf) -> bool { path.as_os_str().is_empty() }

fn joined(root: &Path, child: &str) -> bool { root.join(child).as_os_str().is_empty() }

fn blank_component(component: Component<'_>) -> bool { component.as_os_str().is_empty() }

fn direct(path: &Path) -> bool { path.is_empty() }

fn qualified(path: &Path) -> bool { std::ffi::OsStr::is_empty(Path::as_os_str(path)) }

fn projected(path: &Projected) -> bool { path.as_os_str().is_empty() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let root = PathBuf::from("root");
    black_box([blank(&root), blank_owned(&root), joined(&root, "child"), blank_component(Component::CurDir), direct(&root), qualified(&root)]);
    let view = Projected { path: root, reads: Cell::new(0) };
    black_box((projected(&view), view.is_empty(), view.reads.get()));
}
