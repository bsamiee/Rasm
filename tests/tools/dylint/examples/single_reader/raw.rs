// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! cycle {
    () => {
        #[path = "nested/../raw.rs"]
        mod cycle;
    };
}

mod r#sibling;
