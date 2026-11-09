#![feature(default_field_values, error_reporter, trait_alias, try_blocks_heterogeneous)]
#![cfg_attr(unix, feature(iterator_try_collect))]
#![cfg_attr(panic = "abort", feature(lock_value_accessors, nonpoison_mutex, panic_update_hook, sync_nonpoison))]

#[cfg(windows)]
use core::task::{Context, Poll};
#[cfg(panic = "abort")]
use core::mem::DropGuard;
use core::{fmt, num::NonZero};
use std::{
    env,
    error::Report,
    io,
    process::{ExitCode, Termination},
};
#[cfg(panic = "abort")]
use std::{
    panic,
    sync::{Arc, nonpoison::Mutex},
};

use camino::Utf8PathBuf;
use clap::{
    ArgMatches, Args, Command, CommandFactory, FromArgMatches, Parser, ValueEnum, ValueHint,
    builder::{Resettable, Styles},
    error::ErrorKind,
};
use clap_complete::CompleteEnv;
use futures::stream;
use nonempty_collections::NEVec;
use tokio::runtime::Runtime;
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
#[cfg(unix)]
use tokio_stream::wrappers::SignalStream;
use toml::de;
use tracing_appender::{
    non_blocking::NonBlockingBuilder,
    rolling::{self, InitError, RollingFileAppender},
};
use tracing_subscriber::{
    filter::LevelFilter,
    fmt::writer::{MakeWriterExt as _, OptionalWriter},
};

use self::telemetry::{Defaults, Failure, Handle, Layer, MakeWriter};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Signals = stream::SelectAll<cfg_select! { unix => { SignalStream } windows => { stream::PollFn<Box<dyn FnMut(&mut Context<'_>) -> Poll<Option<()>> + Send + Sync>> } }>;

#[derive(Clone, Copy, Default, ValueEnum)]
enum Rotation {
    Minutely,
    Hourly,
    #[default]
    Daily,
    Weekly,
    Never,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::Display)]
pub enum Operation {
    #[display("start async runtime")]
    Runtime,
    #[display("listen for shutdown signals")]
    Signal,
    #[display("write man pages into {_0}")]
    Manual(Utf8PathBuf),
}

#[derive(Args)]
#[group(skip)]
struct Manual {
    #[arg(long, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Write man pages into DIRECTORY and exit")]
    manual: Option<Utf8PathBuf>,
}

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Process")]
struct Options {
    #[arg(long, global = true, env, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Directory receiving rotated JSON line files in place of stderr")]
    log_directory: Option<Utf8PathBuf>,
    #[arg(long, global = true, env, value_name = "PERIOD", value_enum, default_value_t, requires = "log_directory", help = "Period each JSON line file covers")]
    log_rotation: Rotation,
    #[arg(long, global = true, env, value_name = "COUNT", requires = "log_directory", help = "JSON line files the log directory keeps, oldest removed first")]
    max_log_files: Option<NonZero<usize>>,
    #[command(flatten)]
    manual: Manual,
    #[command(flatten)]
    color: colorchoice_clap::Color,
    #[command(flatten)]
    telemetry: telemetry::Options,
}

#[derive(Debug)]
pub struct Augmented<O, P> {
    pub options: O,
    pub args: P,
}

impl<O, P> CommandFactory for Augmented<O, P>
where
    O: Args,
    P: CommandFactory,
{
    fn command() -> Command { augmented(P::command(), O::augment_args) }

    fn command_for_update() -> Command { augmented(P::command_for_update(), O::augment_args_for_update) }
}

impl<O, P> FromArgMatches for Augmented<O, P>
where
    O: FromArgMatches,
    P: FromArgMatches,
{
    fn from_arg_matches(matches: &ArgMatches) -> Result<Self, clap::Error> { Self::from_arg_matches_mut(&mut matches.clone()) }

    fn from_arg_matches_mut(matches: &mut ArgMatches) -> Result<Self, clap::Error> { Ok(Self { options: O::from_arg_matches_mut(matches)?, args: P::from_arg_matches_mut(matches)? }) }

    fn update_from_arg_matches(&mut self, matches: &ArgMatches) -> Result<(), clap::Error> { self.update_from_arg_matches_mut(&mut matches.clone()) }

    fn update_from_arg_matches_mut(&mut self, matches: &mut ArgMatches) -> Result<(), clap::Error> {
        self.options.update_from_arg_matches_mut(matches)?;
        self.args.update_from_arg_matches_mut(matches)
    }
}

impl<O, P> Parser for Augmented<O, P>
where
    O: Args,
    P: CommandFactory + FromArgMatches,
{
}

#[derive(Debug)]
pub struct Process<A> {
    pub args: A,
    pub styles: Styles,
    pub signals: Signals,
    pub filter: Handle,
    pub level: LevelFilter,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum CommandError {
    #[error("cannot {operation}")]
    Io {
        operation: Operation,
        source: io::Error,
    },
    #[error(transparent)]
    File(#[from] io::Error),
    #[error("cannot open log directory {directory}")]
    Appender {
        directory: Utf8PathBuf,
        source: InitError,
    },
    #[error("{}", fmt::from_fn(|formatter| match phase {
        settings::Phase::Parse(origin) | settings::Phase::Decode { path: _, origin: Some(origin) } => formatter.write_str(&origin.excerpt(&phase.to_string())),
        settings::Phase::Decode { path: _, origin: None } => write!(formatter, "{phase}"),
    }))]
    Settings {
        phase: Phase,
        source: Box<de::Error>,
    },
}

impl From<CommandError> for sysexits::ExitCode {
    fn from(value: CommandError) -> Self {
        match value {
            CommandError::Io { operation: _, source } | CommandError::File(source) => source.into(),
            CommandError::Appender { directory: _, source: _ } => Self::CantCreat,
            CommandError::Settings { phase: _, source: _ } => Self::Config,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn augmented(program: Command, augment: impl FnOnce(Command) -> Command) -> Command {
    let (about, long_about) = (program.get_about().cloned(), program.get_long_about().cloned());
    augment(program).about(Resettable::from(about)).long_about(Resettable::from(long_about))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, W, L, T, E>(defaults: Defaults, stderr: W, layer: L, work: impl AsyncFnOnce(Process<A>) -> Result<T, NEVec<E>>) -> ExitCode
where
    A: CommandFactory + FromArgMatches,
    W: MakeWriter,
    L: Layer,
    T: Termination,
    E: Failure + From<CommandError>,
{
    let factory = Augmented::<Options, A>::command;
    CompleteEnv::with_factory(factory).complete();
    let mut command = factory();
    #[expect(clippy::disallowed_methods, reason = "clap's parse entry reads OS arguments")]
    let parsed = command.try_get_matches_from_mut(env::args_os()).and_then(|mut matches| Augmented::<Options, A>::from_arg_matches_mut(&mut matches).map_err(|error| error.format(&mut command)));
    let printed = |error: &clap::Error, status: sysexits::ExitCode| error.print().map_or_else(|source| sysexits::ExitCode::from(source).into(), |()| status.into());
    let exited = |error: &clap::Error| printed(error, if error.use_stderr() { sysexits::ExitCode::Usage } else { sysexits::ExitCode::Ok });
    let reported = |error: CommandError| printed(&clap::Error::raw(ErrorKind::Io, format_args!("{}\n", Report::new(&error))).with_cmd(&command), error.into());
    let written = |directory: Utf8PathBuf| {
        try bikeshed Result<(), CommandError> {
            fs_err::create_dir_all(&directory).map_err(CommandError::File)?;
            clap_mangen::generate_to(command.clone(), &directory).map_err(|source| CommandError::Io { operation: Operation::Manual(directory), source })?
        }
        .map_or_else(reported, |()| ExitCode::SUCCESS)
    };
    let failed = |operation: Operation| move |source: io::Error| NEVec::new(E::from(CommandError::Io { operation, source }));
    let Augmented { options: Options { log_directory, log_rotation, max_log_files, manual: Manual { manual: _ }, color, telemetry }, args } = match parsed {
        Ok(Augmented { options: Options { log_directory: _, log_rotation: _, max_log_files: _, manual: Manual { manual: Some(directory) }, color: _, telemetry: _ }, args: _ }) =>
            return written(directory),
        Ok(parsed) => parsed,
        Err(error) => {
            return match factory().ignore_errors(true).try_get_matches().and_then(|mut matches| Manual::from_arg_matches_mut(&mut matches)) {
                Ok(Manual { manual: Some(directory) }) => written(directory),
                Ok(Manual { manual: None }) => exited(&error),
                Err(displayed) => exited(&displayed),
            };
        }
    };
    color.write_global();
    let styles = command.get_styles().clone();
    let rotation = match log_rotation {
        Rotation::Minutely => rolling::Rotation::MINUTELY,
        Rotation::Hourly => rolling::Rotation::HOURLY,
        Rotation::Daily => rolling::Rotation::DAILY,
        Rotation::Weekly => rolling::Rotation::WEEKLY,
        Rotation::Never => rolling::Rotation::NEVER,
    };
    let opened = log_directory
        .map(|directory| {
            max_log_files
                .map(NonZero::get)
                .into_iter()
                .fold(RollingFileAppender::builder().rotation(rotation).filename_prefix(command.get_name()).filename_suffix("jsonl"), rolling::Builder::max_log_files)
                .build(&directory)
                .map(|appender| NonBlockingBuilder::default().lossy(false).finish(appender))
                .map_err(|source| CommandError::Appender { directory, source })
        })
        .transpose();
    let (appender, worker) = match opened {
        Ok(opened) => opened.unzip(),
        Err(error) => return reported(error),
    };
    let _worker = cfg_select! {
        panic = "abort" => {{
            let worker = Arc::new(Mutex::new(worker));
            let held = Arc::clone(&worker);
            panic::update_hook(move |hook, info| {
                held.set(None);
                hook(info);
            });
            DropGuard::new(worker, |worker| worker.set(None))
        }}
        _ => worker,
    };
    telemetry.run(&command, defaults, (move || OptionalWriter::from(appender.clone())).or_else(stderr), layer, |filter, level| {
        let runtime = Runtime::new().map_err(failed(Operation::Runtime))?;
        runtime.block_on(async {
            let signals = (try bikeshed io::Result<Signals> {
                cfg_select! {
                    unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)).into_iter().try_collect::<Signals>()?,
                    windows => {
                        let (mut interrupt, mut interrupt_break, mut close, mut logoff, mut shutdown) =
                            (windows::ctrl_c()?, windows::ctrl_break()?, windows::ctrl_close()?, windows::ctrl_logoff()?, windows::ctrl_shutdown()?);
                        let receivers: [Box<dyn FnMut(&mut Context<'_>) -> Poll<Option<()>> + Send + Sync>; 5] = [
                            Box::new(move |context| interrupt.poll_recv(context)),
                            Box::new(move |context| interrupt_break.poll_recv(context)),
                            Box::new(move |context| close.poll_recv(context)),
                            Box::new(move |context| logoff.poll_recv(context)),
                            Box::new(move |context| shutdown.poll_recv(context)),
                        ];
                        receivers.into_iter().map(stream::poll_fn).collect::<Signals>()
                    }
                }
            })
            .map_err(failed(Operation::Signal))?;
            work(Process { args, styles, signals, filter, level }).await
        })
    })
}

mod settings;
pub mod telemetry;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use settings::{Layers, Origin, Phase, Provenance, decoded};
pub use camino;
pub use clap;
pub use futures;
pub use nonempty_collections;
pub use opentelemetry;
pub use serde;
pub use serde_path_to_error;
pub use sysexits;
pub use tokio_stream;
pub use toml;
pub use tracing_appender;
pub use tracing_subscriber;
