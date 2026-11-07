use core::fmt::Display;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait View: Display {
    fn as_display(&self) -> &dyn Display;
}

impl View for u8 {
    fn as_display(&self) -> &dyn Display { self }
}
