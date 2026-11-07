use core::{iter, num::NonZero, pin::pin, time::Duration};
use std::{
    ffi::{OsStr, OsString},
    io,
    path::{Path, PathBuf},
};

use aws_lc_rs::digest::{SHA256, digest};
use camino::Utf8Path;
use clap::ValueHint;
use data_encoding::HEXLOWER;
use futures::{
    StreamExt as _,
    future::{Either, select},
};
use itertools::Itertools as _;
use nix::unistd::{User, getuid};
use notify::{RecursiveMode, Watcher as _};
use rasm_process::{macos::Launch, report::Run};
use rmcp::model::{
    CallToolRequest, CallToolRequestParams, ClientJsonRpcMessage, ClientRequest, ContentBlock, JsonRpcError, JsonRpcMessage, JsonRpcResponse, NumberOrString, ServerJsonRpcMessage, ServerResult,
    object,
};
use serde::{Deserialize, Serialize, de::DeserializeOwned};
use serde_json::value::RawValue;
use tokio::{
    io::AsyncWriteExt as _,
    net::UnixStream,
    process::Command,
    sync::mpsc,
    task,
    time::{Instant, timeout},
};
use tokio_util::codec::{FramedRead, LinesCodec};
use walkdir::WalkDir;

use crate::desktop::{socket, with_app};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize)]
#[serde(rename_all = "kebab-case")]
enum Kind {
    TestStarting,
    TestFinished,
    TestAssemblyFinished,
    TestOutput,
    TestFailed,
    #[serde(other)]
    Other,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(clap::Args)]
pub(crate) struct Suite {
    #[arg(long, env = "RHINO_TEST_PROFILE", value_hint = ValueHint::DirPath, help = "Home folder of a dedicated, activated Rhino profile")]
    profile: PathBuf,
    project: PathBuf,
    #[arg(long)]
    filter: Option<String>,
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct Resolution {
    properties: Build,
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct Build {
    target_path: PathBuf,
    rhino_install_path: PathBuf,
    rhino_target_framework: String,
}

#[derive(Deserialize, Serialize)]
struct Listener {
    #[serde(rename = "v")]
    protocol_version: u32,
    pid: NonZero<u32>,
    port: NonZero<u16>,
    version: String,
}

#[derive(Serialize)]
struct Script {
    script: String,
}

#[derive(Deserialize)]
struct Reply {
    stdout: String,
}

#[derive(Deserialize)]
struct Summary {
    #[serde(rename = "TestsTotal")]
    total: u64,
    #[serde(rename = "TestsFailed")]
    failed: u64,
    #[serde(rename = "TestsSkipped")]
    skipped: u64,
    #[serde(rename = "TestsNotRun")]
    not_run: u64,
}

#[derive(Deserialize)]
struct Tagged {
    #[serde(rename = "$type")]
    kind: Option<Kind>,
}

#[derive(Deserialize)]
struct Completion {
    status: i32,
    error: Option<String>,
}

#[derive(Serialize)]
#[serde(untagged, rename_all_fields = "camelCase")]
enum Evidence<'evidence> {
    Assembly { path: &'evidence Path, sha256: &'evidence str, framework: &'evidence str, filter: Option<&'evidence str> },
    Sent { pid: NonZero<u32>, port: NonZero<u16>, deadline_ms: u128 },
    Replied { pid: NonZero<u32>, document: NonZero<u32> },
    Event { pid: NonZero<u32>, event: &'evidence RawValue },
    Results { pid: NonZero<u32>, status: i32, summary: &'evidence RawValue },
    Ended { pid: NonZero<u32>, test: Option<&'evidence RawValue> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decode<T>(bytes: &[u8]) -> io::Result<T>
where T: DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_slice(bytes);
    let value = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
    decoder.end()?;
    Ok(value)
}

async fn listener(directory: &Path, pid: NonZero<u32>, events: &mut mpsc::UnboundedReceiver<notify::Result<notify::Event>>) -> io::Result<Listener> {
    loop {
        let mut entries = fs_err::tokio::read_dir(directory).await?;
        let mut announced = None;
        while let Some(entry) = entries.next_entry().await? {
            let path = entry.path();
            if path.extension() == Some(OsStr::new("json"))
                && let listener = decode::<Listener>(&fs_err::tokio::read(&path).await?)?
                && listener.pid == pid
                && announced.replace(listener).is_some()
            {
                return Err(io::Error::other(format!("Multiple RhinoAI announcements for process {pid}")));
            }
        }
        if let Some(listener) = announced {
            return (listener.protocol_version == 1).ok_or_else(|| io::Error::other(format!("RhinoAI announced protocol {} for process {pid}", listener.protocol_version))).map(|()| listener);
        }
        events.recv().await.ok_or_else(|| io::Error::other("Rhino listener observation ended"))?.map_err(io::Error::other)?;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(Suite { profile, project, filter }: Suite, run: &Run) -> io::Result<()> {
    async fn results(run: &Run, pid: NonZero<u32>, socket: UnixStream) -> io::Result<()> {
        (socket.peer_cred()?.pid() == Some(pid.get().cast_signed())).ok_or_else(|| io::Error::other(format!("Rhino test socket is not connected to process {pid}")))?;
        let report = run.directory().join("xunit.jsonl");
        let mut file = fs_err::tokio::File::create_new(&report).await?;
        let mut frames = FramedRead::new(socket, LinesCodec::new_with_max_length(8 << 20));
        let mut summary = None;
        let mut active = None;
        while let Some(line) = frames.next().await.transpose().map_err(io::Error::other)? {
            let Tagged { kind: Some(kind) } = decode(line.as_bytes())? else {
                let Completion { status, error } = decode(line.as_bytes())?;
                fs_err::tokio::write(run.directory().join("exit.json"), &line).await?;
                error.map(io::Error::other).map_or(Ok(()), Err)?;
                let (summary, raw): (Summary, Box<RawValue>) = summary
                    .filter(|(summary, _): &(Summary, Box<RawValue>)| summary.total > 0)
                    .ok_or_else(|| io::Error::other(format!("xUnit did not complete a matching test assembly. See {}", report.display())))?;
                run.record("Rhino xUnit results", Evidence::Results { pid, status, summary: &raw }).await?;
                return (status == 0 && summary.failed == 0 && summary.skipped == 0 && summary.not_run == 0)
                    .ok_or_else(|| io::Error::other(format!("Rhino tests failed (exit {status}). See {}", report.display())));
            };
            file.write_all(format!("{line}\n").as_bytes()).await?;
            file.flush().await?;
            let raw = RawValue::from_string(line)?;
            let recorded = match kind {
                Kind::TestStarting => {
                    active = Some(raw.clone());
                    Some(raw)
                }
                Kind::TestFinished => {
                    active = None;
                    Some(raw)
                }
                Kind::TestOutput | Kind::TestFailed => Some(raw),
                Kind::TestAssemblyFinished if summary.is_none() => {
                    summary = Some((decode::<Summary>(raw.get().as_bytes())?, raw));
                    None
                }
                Kind::TestAssemblyFinished => return Err(io::Error::other("xUnit reported more than one test assembly")),
                Kind::Other => None,
            };
            if let Some(event) = recorded {
                run.record("Rhino xUnit event", Evidence::Event { pid, event: &event }).await?;
            }
        }
        let error = io::Error::other(format!("Rhino test output ended before completion. See {}", report.display()));
        run.fault("Rhino test output ended", Evidence::Ended { pid, test: active.as_deref() }, &error.to_string()).await?;
        Err(error)
    }
    const DISPATCH: Duration = Duration::from_secs(30);
    let home = task::spawn_blocking(move || {
        let home = fs_err::canonicalize(profile)?;
        let login = User::from_uid(getuid()).map_err(io::Error::other)?.ok_or_else(|| io::Error::other("No desktop user home directory"))?.dir;
        (!fs_err::canonicalize(login)?.starts_with(&home)).ok_or_else(|| io::Error::other(format!("The Rhino test profile {} holds the login home", home.display())))?;
        WalkDir::new(&home).into_iter().try_for_each(|entry| {
            let entry = entry?;
            let escaped = entry.path_is_symlink() && !fs_err::canonicalize(entry.path())?.starts_with(&home);
            let pending = entry.file_name() == "FleetingPackages.txt" && entry.metadata()?.len() != 0;
            match (escaped, pending) {
                (false, false) => Ok(()),
                (true, _) => Err(io::Error::other(format!("{} resolves outside the test profile", entry.path().display()))),
                (false, true) => Err(io::Error::other(format!("The Grasshopper profile holds pending package removals in {}", entry.path().display()))),
            }
        })?;
        Ok::<_, io::Error>(home)
    })
    .await
    .map_err(io::Error::other)??;
    run.record("Rhino test profile", &home).await?;
    let resolved = run.directory().join("msbuild.json");
    let mut command = Command::new("dotnet");
    command
        .arg("msbuild")
        .arg(&project)
        .args(["-t:ResolveRhinoInstallation", "-getProperty:TargetPath,RhinoInstallPath,RhinoTargetFramework"])
        .arg([OsStr::new("-getResultOutputFile:"), resolved.as_os_str()].into_iter().collect::<OsString>());
    run.command(command, None).await?;
    let Resolution { properties: Build { target_path, rhino_install_path, rhino_target_framework } } = decode(&fs_err::tokio::read(&resolved).await?)?;
    let sha256 = HEXLOWER.encode(digest(&SHA256, &fs_err::tokio::read(&target_path).await?).as_ref());
    run.record("Rhino test assembly", Evidence::Assembly { path: &target_path, sha256: &sha256, framework: &rhino_target_framework, filter: filter.as_deref() }).await?;
    let listeners = run.scratch().join("ai/listeners");
    fs_err::tokio::create_dir_all(&listeners).await?;
    let (notifications, mut events) = mpsc::unbounded_channel();
    let mut watcher = notify::recommended_watcher(notifications).map_err(io::Error::other)?;
    watcher.watch(&listeners, RecursiveMode::NonRecursive).map_err(io::Error::other)?;
    let (socket_path, output) = socket(run)?;
    let (home, scratch) = Utf8Path::from_path(&home).zip(Utf8Path::from_path(run.scratch())).ok_or_else(|| io::Error::other("Rhino requires UTF-8 profile and scratch paths"))?;
    let environment = [("CFFIXED_USER_HOME", home.as_str()), ("HOME", home.as_str()), ("RHINO_MCP_HOME", scratch.as_str()), ("RHINO_MCP_AUTOSTART_PORT", "1")];
    let dispatch = async |pid: NonZero<u32>, port: NonZero<u16>| {
        let remaining = run
            .deadline()
            .checked_duration_since(Instant::now())
            .and_then(|budget| budget.checked_sub(DISPATCH))
            .filter(|budget| !budget.is_zero())
            .ok_or_else(|| io::Error::other("No time remains for the Rhino test assembly"))?;
        let filters = filter.as_deref().map(|filter| serde_json::to_string(filter).map(|filter| [r#""-filterVSTest""#.to_owned(), filter])).transpose()?;
        let fixed = [serde_json::to_string(run.directory())?, serde_json::to_string(&sha256)?, serde_json::to_string(&remaining.as_millis().to_string())?, serde_json::to_string(&socket_path)?];
        let arguments = iter::chain(fixed, filters.into_iter().flatten()).join(", ");
        let fixture = serde_json::to_string(&target_path)?;
        let script = format!(
            r#"
if (Environment.ProcessId != {pid}) throw new InvalidOperationException("Unexpected Rhino process.");
var fixture = {fixture};
var main = new TestAssemblyContext(fixture).LoadFromAssemblyPath(fixture).EntryPoint
    ?? throw new InvalidOperationException("The test assembly has no entry point.");
var serial = __rhino_doc__.RuntimeSerialNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
_ = System.Threading.Tasks.Task.Run(() => main.Invoke(null, System.Reflection.BindingFlags.DoNotWrapExceptions,
    null, new object[] {{ new string[] {{ serial, {arguments} }} }}, null));
Console.Write(serial);
sealed class TestAssemblyContext(string assembly) : System.Runtime.Loader.AssemblyLoadContext {{
    readonly System.Runtime.Loader.AssemblyDependencyResolver dependencies = new(assembly);
    protected override System.Reflection.Assembly Load(System.Reflection.AssemblyName name) =>
        dependencies.ResolveAssemblyToPath(name) is string path ? LoadFromAssemblyPath(path) : null;
    protected override IntPtr LoadUnmanagedDll(string name) =>
        dependencies.ResolveUnmanagedDllToPath(name) is string path ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
}}
"#
        );
        let id = NumberOrString::Number(NonZero::<i64>::from(pid).get());
        let tool = CallToolRequestParams::new("run_csharp").with_arguments(object(serde_json::to_value(Script { script })?));
        let request = ClientJsonRpcMessage::request(ClientRequest::CallToolRequest(CallToolRequest::new(tool)), id.clone());
        let client = reqwest::Client::builder().no_proxy().redirect(reqwest::redirect::Policy::none()).timeout(DISPATCH).build().map_err(io::Error::other)?;
        run.record("Rhino command sent", Evidence::Sent { pid, port, deadline_ms: remaining.as_millis() }).await?;
        let response = client.post(format!("http://localhost:{port}/")).json(&request).send().await.and_then(reqwest::Response::error_for_status).map_err(io::Error::other)?;
        let result = match response.json::<ServerJsonRpcMessage>().await.map_err(io::Error::other)? {
            JsonRpcMessage::Response(JsonRpcResponse { jsonrpc: _, id: answered, result: ServerResult::CallToolResult(result) }) if answered == id => Ok(result),
            JsonRpcMessage::Error(JsonRpcError { jsonrpc: _, id: Some(answered), error }) if answered == id => Err(io::Error::other(error)),
            _ => Err(io::Error::other("RhinoAI returned a different response")),
        }?;
        let texts = || result.content.iter().filter_map(ContentBlock::as_text).map(|part| part.text.as_str());
        (result.is_error != Some(true)).ok_or_else(|| io::Error::other(texts().join("\n")))?;
        let Reply { stdout } = decode(texts().next_back().ok_or_else(|| io::Error::other("RhinoAI returned no result"))?.as_bytes())?;
        let document = stdout.trim().parse::<NonZero<u32>>().map_err(|source| io::Error::new(io::ErrorKind::InvalidData, source))?;
        run.record("Rhino command replied", Evidence::Replied { pid, document }).await
    };
    with_app(run, Launch { bundle: &rhino_install_path, documents: &[], arguments: &[], environment: &environment }, async |pid| {
        let listener = timeout(Duration::from_secs(45), listener(&listeners, pid, &mut events)).await.map_err(io::Error::other)??;
        run.record("RhinoAI listener ready", &listener).await?;
        let sent = pin!(dispatch(pid, listener.port));
        let received = pin!(async { results(run, pid, output.accept().await?.0).await });
        match select(sent, received).await {
            Either::Left((reply, received)) => {
                reply?;
                received.await
            }
            Either::Right((output, sent)) => {
                output?;
                sent.await
            }
        }
    })
    .await
}
