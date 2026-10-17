use core::hint::black_box;
use std::path::{Component, Path, PathBuf};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn blank(path: &Path) -> bool { path.as_os_str().is_empty() }

fn blank_owned(path: &PathBuf) -> bool { path.as_os_str().is_empty() }

fn joined(root: &Path, child: &str) -> bool { root.join(child).as_os_str().is_empty() }

fn blank_component(component: Component<'_>) -> bool { component.as_os_str().is_empty() }

fn direct(path: &Path) -> bool { path.is_empty() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let root = PathBuf::from("root");
    black_box([blank(&root), blank_owned(&root), joined(&root, "child"), blank_component(Component::CurDir), direct(&root)]);
}
