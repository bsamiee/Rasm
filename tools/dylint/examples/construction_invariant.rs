use core::hint::black_box;
use std::process::ExitCode;

use units::{Level, Meters, Packet, Port, Weight, Window, weighed};

// --- [MODELS] --------------------------------------------------------------------------

mod units {
    use thiserror::Error;

    #[derive(Debug)]
    pub(crate) struct Port(pub u16);

    impl TryFrom<u16> for Port {
        type Error = RangeError;

        fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(value)) } }
    }

    #[derive(Debug)]
    pub(crate) struct Window {
        pub width: u32,
        pub height: u32,
    }

    impl Window {
        pub(crate) const fn new(width: u32, height: u32) -> Option<Self> { if width == 0 || height == 0 { None } else { Some(Self { width, height }) } }

        pub(crate) const fn area(&self) -> u32 { self.width.saturating_mul(self.height) }
    }

    #[derive(Debug)]
    pub(crate) struct Weight(u32);

    impl TryFrom<u32> for Weight {
        type Error = RangeError;

        fn try_from(value: u32) -> Result<Self, Self::Error> { if value > 500 { Err(RangeError(value)) } else { Ok(Self(value)) } }
    }

    impl Weight {
        pub(crate) const fn unchecked(weight: u32) -> Self { Self(weight) }

        pub(crate) const fn halved(&self) -> Self { Self(self.0 / 2) }

        pub(crate) const fn get(&self) -> u32 { self.0 }
    }

    #[derive(Debug)]
    pub(crate) struct Packet {
        pub payload: Vec<u8>,
    }

    impl Packet {
        pub(crate) fn decode(payload: &[u8]) -> Option<Self> { payload.first().map(|&tag| Self { payload: vec![tag] }) }
    }

    #[derive(Debug)]
    pub(crate) struct Meters(pub u32);

    impl From<u32> for Meters {
        fn from(value: u32) -> Self { Self(value) }
    }

    #[derive(Debug)]
    pub(crate) struct Level {
        pub value: u8,
        pub label: &'static str,
    }

    impl Level {
        pub(crate) fn new(value: u8) -> Option<Self> { (value <= 10).then_some(Self { value, label: "level" }) }

        pub(crate) fn lowered(&self) -> Self { raw(self.value.saturating_sub(1)) }
    }

    fn raw(value: u8) -> Level { Level { value, label: "raw" } }

    #[derive(Debug, Error)]
    #[error("value {0} is out of range")]
    pub(crate) struct RangeError(u32);

    pub(crate) const fn weighed(value: u32) -> Weight { Weight(value) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    let port = Port::try_from(8080).map_or(0, |port| port.0);
    let area = Window::new(2, 3).map_or([0, 0], |window| [window.area(), window.width]);
    let weights = [Weight::try_from(10).map_or(0, |weight| weight.get()), Weight::unchecked(1).halved().get(), weighed(3).get(), Meters::from(4).0];
    let level = Level::new(3).map(|level| (level.lowered().value, level.label));
    black_box((area, weights, level, Packet::decode(&[7]).map(|packet| packet.payload)));
    ExitCode::from(u8::from(port == 0))
}
