use core::{
    hint::black_box,
    net::{AddrParseError, SocketAddr},
    num::ParseIntError,
    str::FromStr,
};

use clap::Parser;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
struct Options {
    #[arg(long)]
    level: String,
    #[arg(long)]
    listen: String,
    #[arg(long)]
    name: String,
    #[arg(long)]
    port: String,
}

#[derive(Debug)]
pub struct Exported {
    pub level: String,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn verbosity(options: &Options) -> Result<u8, ParseIntError> { options.level.parse() }

fn threshold(options: &Options) -> Result<u8, ParseIntError> { u8::from_str(&options.level) }

fn bound(options: &Options) -> Result<SocketAddr, AddrParseError> { options.listen.parse() }

fn shown(options: &Options) -> usize { options.listen.len().strict_add(options.name.len()) }

fn narrow(options: &Options) -> Result<u16, ParseIntError> { options.port.parse() }

fn wide(options: &Options) -> Result<u32, ParseIntError> { options.port.parse() }

pub fn exported(exported: &Exported) -> Result<u8, ParseIntError> { exported.level.parse() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(Options::parse, verbosity, threshold, bound, shown, narrow, wide, exported)); }
