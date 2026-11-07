use core::{num::NonZero, pin::pin, time::Duration};
use std::{
    env,
    ffi::OsStr,
    io,
    path::{Path, PathBuf},
};

use aws_lc_rs::digest::{SHA256, digest};
use camino::Utf8Path;
use data_encoding::HEXLOWER;
use fs_err::{self as fs, tokio as async_fs};
use futures::{
    StreamExt as _,
    future::{Either, select},
};
use notify::{RecursiveMode, Watcher as _};
use rasm_process::{macos::Launch, report::Run};
use rmcp::model::{CallToolRequest, CallToolRequestParams, ClientJsonRpcMessage, ClientRequest, JsonRpcMessage, NumberOrString, ServerJsonRpcMessage, ServerResult};
use serde::{Deserialize, de::DeserializeOwned};
use serde_json::{Value, json};
use tokio::{
    io::AsyncWriteExt as _,
    net::UnixStream,
    process::Command,
    sync::mpsc,
    time::{Instant, timeout},
};
use tokio_util::codec::{FramedRead, LinesCodec};

use crate::desktop::{login_home, socket, with_app};

// --- [CONSTANTS] -----------------------------------------------------------------------

const DISPATCH: Duration = Duration::from_secs(30);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(clap::Args)]
pub(crate) struct Suite {
    project: PathBuf,
    #[arg(long)]
    filter: Option<String>,
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct Build {
    target_path: PathBuf,
    rhino_install_path: PathBuf,
    rhino_target_framework: String,
}

#[derive(Deserialize)]
struct Listener {
    #[serde(rename = "v")]
    protocol_version: u32,
    pid: NonZero<u32>,
    port: NonZero<u16>,
    version: String,
}

#[derive(Deserialize)]
struct Completion {
    status: i32,
    error: Option<String>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decode<T>(bytes: &[u8]) -> io::Result<T>
where T: DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_slice(bytes);
    let value = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
    decoder.end()?;
    Ok(value)
}

fn profile() -> io::Result<PathBuf> {
    let selected = env::var_os("RHINO_TEST_PROFILE")
        .map(PathBuf::from)
        .filter(|path| path.is_absolute())
        .ok_or_else(|| io::Error::other("RHINO_TEST_PROFILE must name the absolute path of a dedicated, activated Rhino profile"))?;
    let home = fs::canonicalize(selected)?;
    if fs::canonicalize(login_home()?)?.starts_with(&home) {
        return Err(io::Error::other(format!("RHINO_TEST_PROFILE resolves to the login home or an ancestor ({})", home.display())));
    }
    let support = home.join("Library/Application Support");
    let rhino = support.join("McNeel/Rhinoceros");
    let grasshopper = support.join("Grasshopper2");
    let owned = |path: &Path| match fs::symlink_metadata(path) {
        Ok(_) if !fs::canonicalize(path)?.starts_with(&home) => Err(io::Error::other(format!("{} resolves outside the test profile", path.display()))),
        Ok(_) => Ok(true),
        Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(false),
        Err(error) => Err(error),
    };
    for path in [&support, &rhino, &grasshopper, &grasshopper.join("Settings"), &grasshopper.join("Resources"), &grasshopper.join("Diagnostics")] {
        owned(path)?;
    }
    fs::read_dir(&rhino)?.try_for_each(|entry| {
        let entry = entry?;
        let name = entry.file_name();
        if !name.to_str().and_then(|name| name.split_once('.')).is_some_and(|(major, minor)| major.parse::<u32>().is_ok() && minor.parse::<u32>().is_ok()) {
            return Ok(());
        }
        let version = entry.path();
        let manager = version.join("License Manager");
        let licenses = manager.join("Licenses");
        owned(&version)?;
        owned(&manager)?;
        if owned(&licenses)? {
            for license in fs::read_dir(licenses)? {
                let license = license?;
                if license.file_type()?.is_symlink() {
                    return Err(io::Error::other(format!("Linked Rhino license file: {}", license.path().display())));
                }
            }
        }
        Ok(())
    })?;
    let ledger = grasshopper.join("Settings/FleetingPackages.txt");
    if owned(&ledger)? && fs::metadata(&ledger)?.len() != 0 {
        return Err(io::Error::other(format!("The Grasshopper profile contains pending package removals in {}", ledger.display())));
    }
    Ok(home)
}

async fn dispatch(run: &Run, pid: NonZero<u32>, port: NonZero<u16>, fixture: &Path, sha256: &str, socket: &Path, filter: Option<&str>) -> io::Result<()> {
    let remaining = run
        .deadline()
        .checked_duration_since(Instant::now())
        .and_then(|budget| budget.checked_sub(DISPATCH))
        .filter(|budget| !budget.is_zero())
        .ok_or_else(|| io::Error::other("No time remains for the Rhino test assembly"))?;
    let arguments = [serde_json::to_string(run.directory())?, serde_json::to_string(sha256)?, serde_json::to_string(&remaining.as_millis().to_string())?, serde_json::to_string(socket)?];
    let filters = filter.map(|filter| serde_json::to_string(filter).map(|filter| [r#""-filterVSTest""#.to_owned(), filter])).transpose()?;
    let arguments = arguments.into_iter().chain(filters.into_iter().flatten()).collect::<Vec<_>>().join(", ");
    let fixture = serde_json::to_string(fixture)?;
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
    let tool = CallToolRequestParams::new("run_csharp").with_arguments(serde_json::Map::from_iter([("script".to_owned(), Value::String(script))]));
    let request = ClientJsonRpcMessage::request(ClientRequest::CallToolRequest(CallToolRequest::new(tool)), id.clone());
    let client = reqwest::Client::builder().no_proxy().redirect(reqwest::redirect::Policy::none()).timeout(DISPATCH).build().map_err(io::Error::other)?;
    run.record("Rhino command sent", json!({ "pid": pid, "port": port, "deadlineMs": remaining.as_millis() })).await?;
    let response = client.post(format!("http://localhost:{port}/")).json(&request).send().await.and_then(reqwest::Response::error_for_status).map_err(io::Error::other)?;
    let result = match response.json::<ServerJsonRpcMessage>().await.map_err(io::Error::other)? {
        JsonRpcMessage::Response(response) if response.id == id => match response.result {
            ServerResult::CallToolResult(result) => result,
            _ => return Err(io::Error::other("RhinoAI returned a different result type")),
        },
        JsonRpcMessage::Error(response) if response.id.as_ref() == Some(&id) => return Err(io::Error::other(response.error)),
        _ => return Err(io::Error::other("RhinoAI returned a different request ID")),
    };
    let output = result.content.iter().filter_map(rmcp::model::ContentBlock::as_text).map(|part| part.text.as_str()).collect::<Vec<_>>();
    if result.is_error == Some(true) {
        return Err(io::Error::other(output.join("\n")));
    }
    let reply: Value = decode(output.last().ok_or_else(|| io::Error::other("RhinoAI returned no result"))?.as_bytes())?;
    let document = reply["stdout"].as_str().and_then(|serial| serial.trim().parse::<NonZero<u32>>().ok()).ok_or_else(|| io::Error::other("RhinoAI did not confirm the test document"))?;
    run.record("Rhino command replied", json!({ "pid": pid, "document": document })).await
}

async fn results(run: &Run, pid: NonZero<u32>, socket: UnixStream) -> io::Result<()> {
    if socket.peer_cred()?.pid() != Some(i32::try_from(pid.get()).map_err(io::Error::other)?) {
        return Err(io::Error::other(format!("Rhino test socket is not connected to process {pid}")));
    }
    let report = run.directory().join("xunit.jsonl");
    let mut file = async_fs::File::create_new(&report).await?;
    let mut frames = FramedRead::new(socket, LinesCodec::new_with_max_length(8 * 1024 * 1024));
    let mut summary = None;
    let mut active = None;
    while let Some(mut line) = frames.next().await.transpose().map_err(io::Error::other)? {
        let event: Value = decode(line.as_bytes())?;
        let Some(kind) = event["$type"].as_str() else {
            let Completion { status, error } = serde_path_to_error::deserialize(event).map_err(io::Error::other)?;
            async_fs::write(run.directory().join("exit.json"), line).await?;
            if let Some(error) = error {
                return Err(io::Error::other(error));
            }
            let summary = summary
                .filter(|event: &Value| event["TestsTotal"].as_u64().is_some_and(|count| count > 0))
                .ok_or_else(|| io::Error::other(format!("xUnit did not complete a matching test assembly. See {}", report.display())))?;
            run.record("Rhino xUnit results", json!({ "pid": pid, "status": status, "summary": summary })).await?;
            return if status == 0 && ["TestsFailed", "TestsSkipped", "TestsNotRun"].into_iter().all(|key| summary[key].as_u64() == Some(0)) {
                Ok(())
            } else {
                Err(io::Error::other(format!("Rhino tests failed (exit {status}). See {}", report.display())))
            };
        };
        line.push('\n');
        file.write_all(line.as_bytes()).await?;
        file.flush().await?;
        match kind {
            "test-starting" => active = Some(event.clone()),
            "test-finished" => active = None,
            "test-assembly-finished" if summary.is_none() => {
                summary = Some(event);
                continue;
            }
            "test-assembly-finished" => return Err(io::Error::other("xUnit reported more than one test assembly")),
            "test-output" | "test-failed" => (),
            _ => continue,
        }
        run.record("Rhino xUnit event", json!({ "pid": pid, "event": event })).await?;
    }
    let error = format!("Rhino test output ended before completion. See {}", report.display());
    run.fault("Rhino test output ended", json!({ "pid": pid, "test": active }), &error).await?;
    Err(io::Error::other(error))
}

async fn listener(directory: &Path, pid: NonZero<u32>, events: &mut mpsc::UnboundedReceiver<notify::Result<notify::Event>>) -> io::Result<Listener> {
    let prefix = format!("{pid}-");
    loop {
        let mut entries = async_fs::read_dir(directory).await?;
        let mut matching = None;
        while let Some(entry) = entries.next_entry().await? {
            let path = entry.path();
            let named = path.file_name().and_then(OsStr::to_str).is_some_and(|name| name.starts_with(&prefix));
            if named && path.extension() == Some(OsStr::new("json")) && matching.replace(path).is_some() {
                return Err(io::Error::other(format!("Multiple RhinoAI announcements for process {pid}")));
            }
        }
        let Some(path) = matching else {
            events.recv().await.ok_or_else(|| io::Error::other("Rhino listener observation ended"))?.map_err(io::Error::other)?;
            continue;
        };
        let listener: Listener = decode(&async_fs::read(&path).await?)?;
        let name = format!("{pid}-{}.json", listener.port);
        if listener.protocol_version != 1 || listener.pid != pid || path.file_name() != Some(OsStr::new(&name)) {
            return Err(io::Error::other(format!("RhinoAI announced a different listener in {}", path.display())));
        }
        return Ok(listener);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(suite: Suite, run: &Run) -> io::Result<()> {
    let home = tokio::task::spawn_blocking(profile).await.map_err(io::Error::other)??;
    run.record("Rhino test profile", &home).await?;
    let mut command = Command::new("dotnet");
    command.arg("msbuild").arg(&suite.project).args(["-t:ResolveRhinoInstallation", "-getProperty:TargetPath,RhinoInstallPath,RhinoTargetFramework"]);
    run.command(command, "msbuild", None).await?;
    let resolved: Value = decode(&async_fs::read(run.directory().join("msbuild.log")).await?)?;
    let Build { target_path, rhino_install_path, rhino_target_framework } = serde_path_to_error::deserialize(&resolved["Properties"]).map_err(io::Error::other)?;
    let sha256 = HEXLOWER.encode(digest(&SHA256, &async_fs::read(&target_path).await?).as_ref());
    run.record("Rhino test assembly", json!({ "path": target_path, "sha256": sha256, "framework": rhino_target_framework, "filter": suite.filter })).await?;
    let listeners = run.scratch().join("ai/listeners");
    async_fs::create_dir_all(&listeners).await?;
    let (notifications, mut events) = mpsc::unbounded_channel();
    let mut watcher = notify::recommended_watcher(notifications).map_err(io::Error::other)?;
    watcher.watch(&listeners, RecursiveMode::NonRecursive).map_err(io::Error::other)?;
    let (_socket, socket_path, output) = socket(run.scratch())?;
    let Some((home, scratch)) = Utf8Path::from_path(&home).zip(Utf8Path::from_path(run.scratch())) else {
        return Err(io::Error::other("Rhino requires UTF-8 profile and scratch paths"));
    };
    let environment = [("CFFIXED_USER_HOME", home.as_str()), ("HOME", home.as_str()), ("RHINO_MCP_HOME", scratch.as_str()), ("RHINO_MCP_AUTOSTART_PORT", "1")];
    with_app(run, Launch { bundle: &rhino_install_path, documents: &[], arguments: &[], environment: &environment }, async |pid| {
        let listener = timeout(Duration::from_secs(45), listener(&listeners, pid, &mut events)).await.map_err(io::Error::other)??;
        run.record("RhinoAI listener ready", json!({ "pid": pid, "port": listener.port, "version": listener.version })).await?;
        let sent = pin!(dispatch(run, pid, listener.port, &target_path, &sha256, &socket_path, suite.filter.as_deref()));
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
