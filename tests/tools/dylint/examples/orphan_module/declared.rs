mod plain; //~ orphan_module orphan_module/lib.rs
//~| orphan_module orphan_module/nested/mod.rs
//~| orphan_module orphan_module/undeclared.rs

// --- [EXPORTS] ------------------------------------------------------------------------

pub(crate) use plain::counted;
