use core::hint::black_box;

#[path = "orphan_module/declared.rs"]
mod declared;
#[path = "orphan_module_target/shared.rs"]
mod shared;

// --- [COMPOSITION] ---------------------------------------------------------------------

#[expect(unused_macro_rules, reason = "The `module` rule declares a module no expansion compiles")]
macro_rules! observed {
    (module) => {
        #[path = "orphan_module/undeclared.rs"]
        mod unused;
    };
    () => {
        declared::counted()
    };
}

fn main() { black_box((observed!(), shared::SHARED)); }
