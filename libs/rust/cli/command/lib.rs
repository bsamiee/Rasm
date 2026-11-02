#![feature(error_reporter, iterator_try_collect)]

use core::{error::Error, fmt};
use std::{
    error::Report,
    io,
    mem::DropGuard,
    process::{ExitCode, Termination},
};

use camino::Utf8PathBuf;
use clap::{ArgMatches, Args, Command, CommandFactory, FromArgMatches, ValueHint, builder::Resettable, error::ErrorKind};
use clap_complete::CompleteEnv;
use futures::stream;
use nonempty_collections::NEVec;
use rasm_telemetry::{Defaults, Handle, Layer, MakeWriter};
use tokio::runtime::Runtime;
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
#[cfg(windows)]
use tokio_stream::wrappers::CtrlCStream;
#[cfg(unix)]
use tokio_stream::wrappers::SignalStream;
use toml::de;
use tracing_appender::{
    non_blocking::NonBlockingBuilder,
    rolling::{InitError, RollingFileAppender, Rotation},
};
use tracing_subscriber::fmt::writer::{MakeWriterExt as _, OptionalWriter};

// --- [TYPES] ---------------------------------------------------------------------------

cfg_select! {
    unix => {
        pub type Signals = stream::SelectAll<SignalStream>;
    }
    windows => {
        pub type Signals = stream::SelectAll<CtrlCStream>;
    }
}

#[derive(Debug, derive_more::Display)]
pub enum Operation {
    #[display("start async runtime")]
    Runtime,
    #[display("listen for shutdown signals")]
    Signal,
    #[display("read settings")]
    Read,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Process")]
struct Options {
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Directory receiving daily JSON line files in place of stderr")]
    log_directory: Option<Utf8PathBuf>,
    #[arg(long, exclusive = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Write man pages into DIRECTORY and exit")]
    manual: Option<Utf8PathBuf>,
    #[command(flatten)]
    color: colorchoice_clap::Color,
    #[command(flatten)]
    telemetry: rasm_telemetry::Options,
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

#[derive(Debug)]
pub struct Process<A> {
    pub args: A,
    pub signals: Signals,
    pub filter: Handle,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum CommandError {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error("cannot write man pages into {directory}")]
    Manual { directory: Utf8PathBuf, source: io::Error },
    #[error("cannot open log directory {directory}")]
    Journal { directory: Utf8PathBuf, source: InitError },
    #[error("{}", settings::excerpt("cannot parse settings", origin))]
    Syntax { origin: settings::Origin, source: Box<de::Error> },
    #[error("{}", fmt::from_fn(|formatter| match origin {
        Some(origin) => formatter.write_str(&settings::excerpt(&format!("cannot decode settings key `{}`", source.path()), origin)),
        None => write!(formatter, "cannot decode settings key `{}` from field defaults", source.path()),
    }))]
    Settings { origin: Option<settings::Origin>, source: Box<serde_path_to_error::Error<de::Error>> },
}

impl From<CommandError> for sysexits::ExitCode {
    fn from(value: CommandError) -> Self {
        match value {
            CommandError::Io { operation: _, source } | CommandError::Manual { directory: _, source } => source.into(),
            CommandError::Journal { directory: _, source: _ } => Self::CantCreat,
            CommandError::Syntax { origin: _, source: _ } | CommandError::Settings { origin: _, source: _ } => Self::Config,
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
    E: Error + From<CommandError> + Into<sysexits::ExitCode>,
{
    fn reported(command: &mut Command, error: CommandError) -> ExitCode {
        command.error(ErrorKind::Io, Report::new(&error)).print().map_or_else(|source| sysexits::ExitCode::from(source).into(), |()| sysexits::ExitCode::from(error).into())
    }
    CompleteEnv::with_factory(Augmented::<Options, A>::command).complete();
    let mut command = Augmented::<Options, A>::command().subcommand_required(false);
    let mut matches = command.get_matches_mut();
    let Options { log_directory, manual, color, telemetry } = Options::from_arg_matches_mut(&mut matches).unwrap_or_else(|error| error.format(&mut command).exit());
    color.write_global();
    if let Some(directory) = manual {
        return clap_mangen::generate_to(Augmented::<Options, A>::command(), &directory)
            .map_or_else(|source| reported(&mut command, CommandError::Manual { directory, source }), |()| ExitCode::SUCCESS);
    }
    let args = A::from_arg_matches_mut(&mut matches).unwrap_or_else(|error| error.format(&mut command).exit());
    let opened = log_directory
        .map(|directory| {
            RollingFileAppender::builder()
                .rotation(Rotation::DAILY)
                .filename_prefix(command.get_name())
                .filename_suffix("jsonl")
                .build(&directory)
                .map(|appender| NonBlockingBuilder::default().lossy(false).finish(appender))
                .map_err(|source| CommandError::Journal { directory, source })
        })
        .transpose();
    let (journal, _worker) = match opened {
        Ok(opened) => opened.unzip(),
        Err(error) => return reported(&mut command, error),
    };
    telemetry.run(&command, defaults, (move || OptionalWriter::from(journal.clone())).or_else(stderr), layer, |filter| {
        let runtime = DropGuard::new(Runtime::new().map_err(|source| NEVec::new(E::from(CommandError::Io { operation: Operation::Runtime, source })))?, Runtime::shutdown_background);
        runtime.block_on(async {
            let signals = cfg_select! {
                unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)),
                windows => [windows::ctrl_c().map(CtrlCStream::new)],
            }
            .into_iter()
            .try_collect::<Signals>()
            .map_err(|source| NEVec::new(E::from(CommandError::Io { operation: Operation::Signal, source })))?;
            work(Process { args, signals, filter }).await
        })
    })
}

pub mod settings;
