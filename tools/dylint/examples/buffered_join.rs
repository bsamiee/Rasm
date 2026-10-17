use core::hint::black_box;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn listed(names: &[&str]) -> String {
    let mut parts = Vec::new();
    for name in names {
        parts.push(name.to_uppercase());
    }
    parts.join(", ")
}

fn counted(names: &[&str]) -> (String, usize) {
    let mut parts = Vec::with_capacity(names.len());
    for name in names {
        parts.push(name.to_uppercase());
    }
    let count = parts.len();
    (parts.join(", "), count)
}

fn per_item(names: &[&str]) -> Vec<String> {
    let mut lines = Vec::new();
    for name in names {
        let mut parts = Vec::new();
        parts.push(name.to_uppercase());
        lines.push(parts.join(", "));
    }
    lines
}

fn extended(names: &[&str]) -> String {
    let mut parts = names.iter().map(|name| name.to_uppercase()).collect::<Vec<_>>();
    for name in names {
        parts.push(name.to_lowercase());
    }
    parts.join(", ")
}

fn unfilled() -> String {
    let parts: Vec<String> = Vec::new();
    parts.join(", ")
}

fn direct(names: &[&str]) -> String { names.join(", ") }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(listed(&["a"]), counted(&["b"]), per_item(&["c"]), extended(&["d"]), unfilled(), direct(&["e"]))); }
