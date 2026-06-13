using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor.Gate;

// Phase-2 gate: every matrix row produces its named discriminated outcome. Stand-in rows use
// processes the gate spawns; --live additionally launches and faults a RhinoWIP the gate owns,
// guarded by the assay bridge flock so no operator session is ever touched.
internal static partial class Program {
    private static readonly List<JsonObject> Rows = [];

    internal static int Main(string[] args) {
        bool live = args.Contains("--live");
        string scratch = Directory.CreateTempSubdirectory(prefix: "rbx-gate-").FullName;
        SessionPolicy policy = SessionPolicy.Default with { QuitRungDeadline = TimeSpan.FromSeconds(2) };
        try {
            RowBogusBundle();
            RowSigstopDiscrimination(policy);
            RowKillMidConnect(policy, scratch);
            RowDeadPidEndpoint();
            RowDeadLeaseReclaim(scratch);
            RowSecondSupervisorBusy(scratch);
            RowQuitLadderEscalation(policy, scratch);
            RowReconcileInstanceScoped(policy, scratch);
            RowStagingHash(scratch);
            RowSpoolHarvest(scratch);
            RowGcDump(scratch);
            RowIpsParser();
            Proof7McpSuppression();
            Proof11MarkerDerivation(policy);
            if (live)
                LiveLane(policy);
        } finally {
            try { Directory.Delete(scratch, recursive: true); } catch (IOException) { }
        }
        JsonArray table = [.. Rows.Select(static row => (JsonNode)row).ToArray()];
        Console.Out.WriteLine(table.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        bool pass = Rows.All(static row => (bool?)row["pass"] ?? false);
        return pass ? 0 : 1;
    }

    private static void Report(string row, bool pass, JsonObject detail) =>
        Rows.Add(new JsonObject { ["row"] = row, ["pass"] = pass, ["detail"] = detail });

    // --- stand-in process plumbing ---

    // Detached double-fork: the stand-in must NOT be a .NET-tracked child — SIGSTOP on a direct
    // child wedges the runtime's SIGCHLD reaper while it holds the global process lock, which
    // deadlocks the next Process.Start. sh exits immediately; sleep reparents to launchd.
    private static int Spawn() {
        Fin<ExecResult> spawned = Exec.Run("/bin/sh", ["-c", "/bin/sleep 300 >/dev/null 2>&1 & echo $!"], TimeSpan.FromSeconds(5));
        return spawned is Fin<ExecResult>.Succ(ExecResult forked) && int.TryParse(forked.StdOut.Trim(), CultureInfo.InvariantCulture, out int pid) ? pid : -1;
    }

    private static LiveHost StandIn(int pid) {
        long started = Posix.StartedAtUnixMs(pid).IfNone(0L);
        EndpointRecord endpoint = EndpointRecord.Create(
            pipeName: $"{EndpointRecord.PipePrefix}gate-{pid}", rhinoPid: pid, rhinoStartedAtUnixMs: started,
            contractVersion: Handshake.CurrentVersion, shellVersion: "gate", rhinoVersion: "gate");
        return new LiveHost(Pid: pid, StartedAtUnixMs: started, Endpoint: endpoint, Fingerprint: default);
    }

    private static void Signal(int pid, string signal) => _ = Exec.Run(file: "/bin/kill", args: [signal, pid.ToString(CultureInfo.InvariantCulture)], deadline: TimeSpan.FromSeconds(5));

    // --- matrix rows ---

    // bogus bundle -> launch-failed
    private static void RowBogusBundle() {
        BundleInfo bogus = new(AppPath: "/tmp/rbx-gate-bogus.app", CFBundleName: "Bogus", CFBundleExecutable: "Bogus", CFBundleVersion: "0.0");
        Fin<Unit> launched = bogus.Launch(toolDeadline: TimeSpan.FromSeconds(10));
        BridgeFault fault = new BridgeFault.LaunchFailed(Detail: launched is Fin<Unit>.Fail(Error error) ? error.Message : "unexpected success");
        Report("bogus-bundle", launched.IsFail && fault.Status == PhaseStatus.Failed, new JsonObject {
            ["outcome"] = "launch-failed", ["exit"] = fault.Status.ExitCode, ["detail"] = fault.Prescription,
        });
    }

    // SIGSTOP -> host alive + silent -> dialog-suspected (pre-session) / ui-wedged (running)
    private static void RowSigstopDiscrimination(SessionPolicy policy) {
        int child = Spawn();
        try {
            LiveHost host = StandIn(child);
            bool exitSeen = false;
            using HostWatch watch = HostWatch.Attach(child, _ => exitSeen = true, policy.WatchPoll, TimeProvider.System);
            Signal(child, "-STOP");
            Thread.Sleep(750);
            bool aliveSilent = Posix.Alive(child) && !exitSeen;
            SessionState connecting = new SessionState.Connecting(Host: host, PollsRemaining: 0);
            SessionState dialog = SessionDispatch.Apply(connecting, new SessionSignal.HeartbeatSilent(SilentFor: policy.HeartbeatWindow + TimeSpan.FromSeconds(1)), policy);
            SessionState running = new SessionState.Running(Host: host, Cargo: new CargoReceipt("hash", 0, [], []),
                Done: Seq<ScenarioReceipt>(), Remaining: Seq(new ScenarioEntry("gate", "gate.standin", [], 0)), RestartBudget: 1);
            SessionState wedged = SessionDispatch.Apply(running, new SessionSignal.HeartbeatSilent(SilentFor: policy.HeartbeatWindow + TimeSpan.FromSeconds(1)), policy);
            Signal(child, "-CONT");
            bool pass = aliveSilent
                && dialog is SessionState.Faulted { Fault: BridgeFault.DialogSuspected }
                && wedged is SessionState.Faulted { Fault: BridgeFault.UiWedged } wedge && wedge.Fault is BridgeFault.UiWedged w && w.Scenario == "gate.standin";
            Report("sigstop", pass, new JsonObject {
                ["aliveAndSilent"] = aliveSilent,
                ["connecting"] = (dialog as SessionState.Faulted)?.Fault is BridgeFault.DialogSuspected ? "dialog-suspected" : dialog.GetType().Name,
                ["running"] = (wedged as SessionState.Faulted)?.Fault is BridgeFault.UiWedged ? "ui-wedged" : wedged.GetType().Name,
                ["watchMode"] = watch.Mode,
            });
        } finally {
            Signal(child, "-CONT");
            _ = Posix.Kill(child);
        }
    }

    // kill -9 mid-connect -> kqueue HostExited -> launch-failed(exited during connect) + .ips check;
    // the DeadlineHit lane in Connecting is the connect-failed discrimination.
    private static void RowKillMidConnect(SessionPolicy policy, string scratch) {
        int child = Spawn();
        LiveHost host = StandIn(child);
        SessionSignal? seen = null;
        using ManualResetEventSlim raised = new(false);
        using HostWatch watch = HostWatch.Attach(child, signal => { seen = signal; raised.Set(); }, policy.WatchPoll, TimeProvider.System);
        BundleInfo standInBundle = new(AppPath: "/tmp", CFBundleName: "GateProbe", CFBundleExecutable: "GateProbeExec", CFBundleVersion: "0.0");
        Seq<string> baseline = Evidence.IpsBaseline(standInBundle);
        _ = Posix.Kill(child);
        bool observed = raised.Wait(TimeSpan.FromSeconds(5));
        SessionState connecting = new SessionState.Connecting(Host: host, PollsRemaining: 0);
        SessionState afterExit = seen is null ? connecting : SessionDispatch.Apply(connecting, seen, policy);
        Option<CrashSummary> ips = Evidence.IpsDiff(baseline, standInBundle);
        SessionState afterDeadline = SessionDispatch.Apply(connecting, new SessionSignal.DeadlineHit(Phase: SessionPhase.Connect, Elapsed: policy.ConnectDeadline + TimeSpan.FromSeconds(1)), policy);
        bool pass = observed && watch.Mode == "kqueue"
            && afterExit is SessionState.Faulted { Fault: BridgeFault.LaunchFailed } exited
            && ((BridgeFault.LaunchFailed)exited.Fault).Detail.Contains("during connect", StringComparison.Ordinal)
            && ips.IsNone
            && afterDeadline is SessionState.Faulted { Fault: BridgeFault.ConnectFailed };
        Report("kill9-mid-connect", pass, new JsonObject {
            ["watchMode"] = watch.Mode,
            ["hostExitedObservedMs"] = observed,
            ["onHostExited"] = afterExit is SessionState.Faulted { Fault: BridgeFault.LaunchFailed lf } ? $"launch-failed: {lf.Detail}" : afterExit.GetType().Name,
            ["ipsDiff"] = ips.IsNone ? "none (SIGKILL writes no report)" : "report found",
            ["onConnectDeadline"] = (afterDeadline as SessionState.Faulted)?.Fault is BridgeFault.ConnectFailed ? "connect-failed" : afterDeadline.GetType().Name,
        });
    }

    // dead-PID endpoint -> discriminated (pid dead vs pid recycled/start-time drift)
    private static void RowDeadPidEndpoint() {
        int deadPid = Spawn();
        _ = Posix.Kill(deadPid);
        long until = Environment.TickCount64 + 5_000;
        while (Posix.Alive(deadPid) && Environment.TickCount64 < until)
            Thread.Sleep(50);
        EndpointRecord dead = EndpointRecord.Create(
            pipeName: $"{EndpointRecord.PipePrefix}gate-dead", rhinoPid: deadPid, rhinoStartedAtUnixMs: 1L,
            contractVersion: Handshake.CurrentVersion, shellVersion: "gate", rhinoVersion: "gate");
        Fin<LiveHost> deadAdmit = LiveHost.Admit(dead, default);
        EndpointRecord drifted = EndpointRecord.Create(
            pipeName: $"{EndpointRecord.PipePrefix}gate-drift", rhinoPid: Environment.ProcessId, rhinoStartedAtUnixMs: 1L,
            contractVersion: Handshake.CurrentVersion, shellVersion: "gate", rhinoVersion: "gate");
        Fin<LiveHost> driftAdmit = LiveHost.Admit(drifted, default);
        bool pass = deadAdmit is Fin<LiveHost>.Fail(Error gone) && gone.Message.Contains("not alive", StringComparison.Ordinal)
            && driftAdmit is Fin<LiveHost>.Fail(Error recycled) && recycled.Message.Contains("recycled", StringComparison.Ordinal);
        Report("dead-pid-endpoint", pass, new JsonObject {
            ["deadPid"] = deadAdmit is Fin<LiveHost>.Fail(Error d) ? d.Message : "admitted",
            ["pidRecycled"] = driftAdmit is Fin<LiveHost>.Fail(Error r) ? r.Message : "admitted",
        });
    }

    // dead lease -> lease.reclaimed fact + successful claim
    private static void RowDeadLeaseReclaim(string scratch) {
        string path = Path.Combine(scratch, "dead.lease");
        File.WriteAllText(path, """{"holderPid":4999999,"holderStartedAtUnixMs":1,"acquiredAtUnixMs":1}""");
        List<BridgeEvent> published = [];
        Fin<LeaseToken> token = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, published.Add);
        bool reclaimed = published.Any(static evt => evt is BridgeEvent.FactCase { Key: "lease.reclaimed" });
        Report("dead-lease", token.IsSucc && reclaimed, new JsonObject {
            ["outcome"] = token.IsSucc ? "acquired" : "failed",
            ["fact"] = reclaimed ? "lease.reclaimed" : "missing",
        });
        if (token is Fin<LeaseToken>.Succ(LeaseToken held))
            _ = Lease.Release(held);
    }

    // second supervisor -> busy, exit 5, holder pid + age named
    private static void RowSecondSupervisorBusy(string scratch) {
        string path = Path.Combine(scratch, "busy.lease");
        Fin<LeaseToken> first = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, _ => { });
        Fin<LeaseToken> second = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, _ => { });
        bool pass = first.IsSucc && second is Fin<LeaseToken>.Fail(Error busy)
            && busy.Code == PhaseStatus.Busy.ExitCode
            && busy.Message.Contains(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Report("second-supervisor", pass, new JsonObject {
            ["outcome"] = second.IsFail ? "busy-held" : "acquired",
            ["exit"] = second is Fin<LeaseToken>.Fail(Error b) ? b.Code : 0,
            ["prescription"] = second is Fin<LeaseToken>.Fail(Error p) ? p.Message : string.Empty,
        });
        if (first is Fin<LeaseToken>.Succ(LeaseToken winner))
            _ = Lease.Release(winner);
    }

    // quit ladder vs a non-AppKit process: ae rung FAILS (closed:false), force rung FAILS,
    // kill(2) rung closes -> escalation order + journal write proven; SIGTERM never sent.
    private static void RowQuitLadderEscalation(SessionPolicy policy, string scratch) {
        int child = Spawn();
        LiveHost host = StandIn(child);
        string journal = Path.Combine(scratch, "quits.jsonl");
        SupervisorRuntime runtime = new(
            Lease: Atom(Option<LeaseToken>.None), Clock: TimeProvider.System, Policy: policy,
            ArtifactRoot: scratch, LeasePath: Path.Combine(scratch, "gate.lease"), JournalPath: journal,
            Bundle: new BundleInfo("/tmp", "GateProbe", "GateProbeExec", "0.0"), Root: CancellationToken.None);
        List<BridgeEvent> published = [];
        Fin<PhaseStatus> outcome = QuitLadder.Run(host, Guid.NewGuid(), published.Add).Run(runtime);
        Seq<QuitJournalEntry> entries = QuitJournal.Read(journal);
        string[] rungs = [.. published.OfType<BridgeEvent.PhaseCase>().Select(static phase => $"{phase.Phase.Key}:{phase.Status.Key}")];
        bool pass = outcome is Fin<PhaseStatus>.Succ(PhaseStatus closed) && closed == PhaseStatus.Ok
            && rungs.SequenceEqual(["quit.ae:failed", "quit.force:failed", "quit.kill:ok"])
            && entries.Count == 1 && entries.Head.Case is QuitJournalEntry entry && entry.Rung == "quit.kill" && entry.Pid == child;
        Report("quit-ladder-escalation", pass, new JsonObject {
            ["rungs"] = new JsonArray(rungs.Select(static rung => (JsonNode)rung).ToArray()),
            ["journaled"] = entries.Count,
            ["sigterm"] = "never sent (rungs are AE terminate / forceTerminate / kill(2) SIGKILL)",
        });
    }

    // instance-scoped reconcile vs synthetic markers: windowed marker cleared, foreign skipped+reported.
    private static void RowReconcileInstanceScoped(SessionPolicy policy, string scratch) {
        string autosave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Autosave Information");
        _ = Directory.CreateDirectory(autosave);
        BundleInfo synthetic = new("/tmp", "GateProbe", "GateProbeExecNone", "0.0");
        string windowed = Path.Combine(autosave, synthetic.AutosaveMarker + ".rhl");
        string foreign = Path.Combine(autosave, synthetic.AutosaveMarker);
        try {
            File.WriteAllText(windowed, "gate");
            File.WriteAllText(foreign, "gate");
            long mark = new DateTimeOffset(File.GetLastWriteTimeUtc(windowed)).ToUnixTimeMilliseconds();
            string journal = Path.Combine(scratch, "reconcile.jsonl");
            _ = QuitJournal.Append(journal, new QuitJournalEntry(Pid: 1, StartedAtUnixMs: mark - 60_000, RetiredAtUnixMs: mark + 60_000, Rung: "quit.kill", PipeName: "rbx-gate"));
            File.SetLastWriteTimeUtc(foreign, DateTime.UtcNow.AddDays(-2));
            SupervisorRuntime runtime = new(
                Lease: Atom(Option<LeaseToken>.None), Clock: TimeProvider.System, Policy: policy,
                ArtifactRoot: scratch, LeasePath: Path.Combine(scratch, "gate.lease"), JournalPath: journal,
                Bundle: synthetic, Root: CancellationToken.None);
            Fin<Seq<BridgeEvent>> swept = Reconcile.Run(synthetic, Guid.NewGuid()).Run(runtime);
            Seq<BridgeEvent> facts = swept.IfFail(Seq<BridgeEvent>());
            bool cleared = facts.Exists(evt => evt is BridgeEvent.FactCase { Key: "reconcile.cleared" } fact && fact.Value.GetProperty("path").GetString() == windowed);
            bool skipped = facts.Exists(evt => evt is BridgeEvent.FactCase { Key: "reconcile.skipped.foreign" } fact && fact.Value.GetProperty("path").GetString() == foreign);
            bool pass = cleared && skipped && !File.Exists(windowed) && File.Exists(foreign);
            Report("reconcile-instance-scoped", pass, new JsonObject {
                ["windowedMarker"] = !File.Exists(windowed) ? "cleared + fact" : "still present",
                ["foreignMarker"] = File.Exists(foreign) ? "untouched + reported foreign" : "DELETED (violation)",
                ["facts"] = facts.Count,
            });
        } finally {
            try { File.Delete(windowed); } catch (IOException) { }
            try { File.Delete(foreign); } catch (IOException) { }
        }
    }

    private static void RowStagingHash(string scratch) {
        string closureDir = Path.Combine(scratch, "closure");
        _ = Directory.CreateDirectory(closureDir);
        File.WriteAllText(Path.Combine(closureDir, "A.dll"), "alpha");
        File.WriteAllText(Path.Combine(closureDir, "B.dll"), "beta");
        string manifest = Path.Combine(closureDir, "bridge-closure.json");
        File.WriteAllText(manifest, """{"assemblies":["A.dll","B.dll"],"hostPlugins":["b45a29b1-4343-4035-989e-044e8580d9cf"],"builtAgainst":{"bundleVersion":"9.0","rhinoCommonVersion":"9.0","grasshopper2Version":"2.0","runtimeVersion":"10.0"}}""");
        string refs = Path.Combine(scratch, "refs");
        Guid session = Guid.NewGuid();
        Fin<CargoManifest> first = Evidence.Stage(manifest, session, scratch, refs);
        Fin<CargoManifest> again = Evidence.Stage(manifest, session, scratch, refs);
        bool pass = first is Fin<CargoManifest>.Succ(CargoManifest staged)
            && again is Fin<CargoManifest>.Succ(CargoManifest restaged)
            && staged.ContentHash == restaged.ContentHash
            && File.Exists(Path.Combine(staged.StagePath, "A.dll"))
            && staged.HostPlugins.Length == 1;
        Report("xxhash3-staging", pass, new JsonObject {
            ["contentHash"] = first is Fin<CargoManifest>.Succ(CargoManifest m) ? m.ContentHash : "failed",
            ["deterministic"] = pass,
        });
    }

    private static void RowSpoolHarvest(string scratch) {
        Guid session = Guid.NewGuid();
        BridgeEvent fact = new BridgeEvent.FactCase("gate.fact", JsonDocument.Parse("1").RootElement.Clone()) {
            Stamp = new EventStamp(session, 1, 1, "gate"),
        };
        string spool = Path.Combine(scratch, "gate.jsonl");
        string line = JsonSerializer.Serialize(fact, BridgeJsonContext.Default.BridgeEvent);
        File.WriteAllText(spool, line + "\n" + line + "\n" + line[..(line.Length / 2)]);
        Seq<BridgeEvent> harvested = Evidence.HarvestSpool(scratch, "gate");
        Report("spool-harvest", harvested.Count == 2, new JsonObject {
            ["decoded"] = harvested.Count, ["truncatedTail"] = "dropped (crash-durable: facts to point of death survive)",
        });
    }

    private static void RowGcDump(string scratch) {
        Option<string> dump = Evidence.GcDump(Environment.ProcessId, scratch, SessionPolicy.Default.ForensicsDeadline);
        Report("gcdump-trigger", dump.IsSome, new JsonObject {
            ["artifact"] = dump.Case is string path ? Path.GetFileName(path) : "none",
        });
    }

    // RhinoCrashReportFinder port proof against a REAL macOS .ips (read-only).
    private static void RowIpsParser() {
        string reports = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", "DiagnosticReports");
        string? sample = Directory.Exists(reports) ? Directory.GetFiles(reports, "*.ips").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() : null;
        if (sample is null) {
            Report("ips-parser", true, new JsonObject { ["outcome"] = "no .ips on host; parser exercised via IpsDiff none-path only" });
            return;
        }
        string stem = Path.GetFileName(sample).Split('-')[0];
        BundleInfo probe = new("/tmp", stem, stem, "0.0");
        Option<CrashSummary> parsed = Evidence.IpsDiff(Seq<string>(), probe);
        Report("ips-parser", parsed.IsSome, new JsonObject {
            ["report"] = Path.GetFileName(sample),
            ["thread"] = parsed.Case is CrashSummary summary ? summary.Thread : "none",
            ["exceptionType"] = parsed.Case is CrashSummary s2 ? s2.ExceptionType : "none",
        });
    }

    // Proof 7: RHINO_MCP_AUTOSTART_PORT suppression - env var rides every launch (belt); the
    // verification profile carries no rhinomcp yak (fallback policy holds as truth).
    private static void Proof7McpSuppression() {
        string packages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library", "Application Support", "McNeel", "Rhinoceros", "packages", "9.0");
        string[] installed = Directory.Exists(packages) ? [.. Directory.GetDirectories(packages).Select(static path => Path.GetFileName(path) ?? string.Empty)] : [];
        bool mcpPresent = installed.Any(static name => name.Contains("mcp", StringComparison.OrdinalIgnoreCase));
        Report("proof7-mcp-suppression", !mcpPresent, new JsonObject {
            ["installedYak"] = new JsonArray(installed.Select(static name => (JsonNode)name!).ToArray()),
            ["launchEnv"] = "RHINO_MCP_AUTOSTART_PORT=0 (wired at BundleInfo.Launch)",
            ["verdict"] = mcpPresent ? "rhinomcp installed: suppression must be re-verified per release" : "fallback policy holds: no Rhino-MCP-Platform yak on this verification profile",
        });
    }

    // Proof 11: marker names derive from CFBundleName/CFBundleExecutable, reproduced on the GA bundle.
    private static void Proof11MarkerDerivation(SessionPolicy policy) {
        Fin<BundleInfo> discovered = BundleInfo.Discover(policy.ToolDeadline);
        Option<BundleInfo> ga = BundleInfo.Discover(policy.ToolDeadline) is Fin<BundleInfo>.Succ ? ReadGa(policy) : Option<BundleInfo>.None;
        string gaMarker = ga.Case is BundleInfo gaBundle ? gaBundle.AutosaveMarker : "no GA bundle";
        string gaObserved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Autosave Information", gaMarker);
        Report("proof11-marker-derivation", discovered.IsSucc, new JsonObject {
            ["discovered"] = discovered is Fin<BundleInfo>.Succ(BundleInfo wip)
                ? $"{wip.CFBundleName} {wip.CFBundleVersion} (exec {wip.CFBundleExecutable})" : "none",
            ["wipMarkers"] = discovered is Fin<BundleInfo>.Succ(BundleInfo w) ? $"{w.AutosaveMarker} / {w.CrashReportPattern}" : "none",
            ["gaMarkers"] = ga.Case is BundleInfo g ? $"{g.AutosaveMarker} / {g.CrashReportPattern}" : "no GA bundle on host",
            ["gaDerivationReproduces"] = File.Exists(gaObserved)
                ? $"LIVE: '{gaMarker}' exists in Autosave Information (written by the operator's GA Rhino)"
                : "GA marker not currently on disk (derivation rule unexercised by live state)",
        });
    }

    private static Option<BundleInfo> ReadGa(SessionPolicy policy) {
        string? gaPath = Directory.GetDirectories("/Applications", "Rhino*.app")
            .FirstOrDefault(static path => !path.Contains("WIP", StringComparison.OrdinalIgnoreCase));
        if (gaPath is null)
            return Option<BundleInfo>.None;
        string prior = Environment.GetEnvironmentVariable("RHINO_WIP_APP_PATH") ?? string.Empty;
        Environment.SetEnvironmentVariable("RHINO_WIP_APP_PATH", gaPath);
        Fin<BundleInfo> ga = BundleInfo.Discover(policy.ToolDeadline);
        Environment.SetEnvironmentVariable("RHINO_WIP_APP_PATH", prior.Length == 0 ? null : prior);
        return ga is Fin<BundleInfo>.Succ(BundleInfo bundle) ? Some(bundle) : Option<BundleInfo>.None;
    }

    // --- live lane: a RhinoWIP the gate launches itself, guarded by the assay bridge flock ---

    [LibraryImport("libc", EntryPoint = "flock", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int Flock(int fd, int operation);

    private static void LiveLane(SessionPolicy policy) {
        string root = Environment.CurrentDirectory;
        List<FileStream> locks = [];
        try {
            foreach (string lockPath in new[] { Path.Combine(root, ".artifacts", "assay", "locks", "bridge.lock"), Path.Combine(root, ".artifacts", "locks", "bridge.lock") }) {
                if (!File.Exists(lockPath))
                    continue;
                FileStream stream = new(lockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (Flock((int)stream.SafeFileHandle.DangerousGetHandle(), 2 | 4) != 0) {
                    stream.Dispose();
                    Report("live-lane", false, new JsonObject { ["outcome"] = $"assay bridge lease busy at {lockPath}; live lane skipped" });
                    return;
                }
                locks.Add(stream);
            }
            if (Exec.Run("/usr/bin/pgrep", ["-x", "Rhinoceros"], TimeSpan.FromSeconds(5)) is Fin<ExecResult>.Succ(ExecResult running) && running.ExitCode == 0) {
                Report("live-lane", false, new JsonObject { ["outcome"] = "a Rhinoceros process is already live; refusing to fault-inject" });
                return;
            }
            Fin<BundleInfo> discovered = BundleInfo.Discover(policy.ToolDeadline);
            if (discovered is not Fin<BundleInfo>.Succ(BundleInfo bundle)) {
                Report("live-lane", false, new JsonObject { ["outcome"] = "no bundle discovered" });
                return;
            }
            LiveCycleCleanQuit(bundle);
            LiveCycleKillMidConnect(bundle);
        } finally {
            foreach (FileStream held in locks)
                held.Dispose();
        }
    }

    private static Option<int> AwaitPid(TimeSpan deadline) {
        long until = Environment.TickCount64 + (long)deadline.TotalMilliseconds;
        while (Environment.TickCount64 < until) {
            if (Exec.Run("/usr/bin/pgrep", ["-x", "Rhinoceros"], TimeSpan.FromSeconds(5)) is Fin<ExecResult>.Succ(ExecResult found)
                    && found.ExitCode == 0 && int.TryParse(found.StdOut.Trim().Split('\n')[0], CultureInfo.InvariantCulture, out int pid))
                return Some(pid);
            Thread.Sleep(250);
        }
        return Option<int>.None;
    }

    private static LiveHost RealHost(int pid, BundleInfo bundle) {
        long started = Posix.StartedAtUnixMs(pid).IfNone(0L);
        EndpointRecord endpoint = EndpointRecord.Create(
            pipeName: $"{EndpointRecord.PipePrefix}live-{pid}", rhinoPid: pid, rhinoStartedAtUnixMs: started,
            contractVersion: Handshake.CurrentVersion, shellVersion: "gate", rhinoVersion: bundle.CFBundleVersion);
        return new LiveHost(Pid: pid, StartedAtUnixMs: started, Endpoint: endpoint, Fingerprint: default);
    }

    // Cycle A: launch -> AE-terminate rung closes the host cleanly on the FIRST rung; kqueue confirms.
    private static void LiveCycleCleanQuit(BundleInfo bundle) {
        SessionPolicy policy = SessionPolicy.Default;
        Fin<Unit> launched = bundle.Launch(policy.ToolDeadline);
        Option<int> pid = launched.IsSucc ? AwaitPid(policy.LaunchDeadline) : Option<int>.None;
        if (pid.Case is not int hostPid) {
            Report("live-clean-quit", false, new JsonObject { ["outcome"] = launched.IsFail ? "launch failed" : "no pid within deadline" });
            return;
        }
        Thread.Sleep(8_000); // let AppKit finish booting so the AE terminate lands on a responsive app
        LiveHost host = RealHost(hostPid, bundle);
        bool exitSeen = false;
        using ManualResetEventSlim raised = new(false);
        using HostWatch watch = HostWatch.Attach(hostPid, _ => { exitSeen = true; raised.Set(); }, policy.WatchPoll, TimeProvider.System);
        string journal = QuitJournal.CanonicalPath;
        SupervisorRuntime runtime = new(
            Lease: Atom(Option<LeaseToken>.None), Clock: TimeProvider.System, Policy: policy,
            ArtifactRoot: Environment.CurrentDirectory, LeasePath: Lease.CanonicalPath, JournalPath: journal,
            Bundle: bundle, Root: CancellationToken.None);
        List<BridgeEvent> published = [];
        Fin<PhaseStatus> outcome = QuitLadder.Run(host, Guid.NewGuid(), published.Add).Run(runtime);
        _ = raised.Wait(TimeSpan.FromSeconds(5));
        string[] rungs = [.. published.OfType<BridgeEvent.PhaseCase>().Select(static phase => $"{phase.Phase.Key}:{phase.Status.Key}")];
        bool pass = outcome is Fin<PhaseStatus>.Succ(PhaseStatus status) && status == PhaseStatus.Ok
            && rungs.Length >= 1 && rungs[0] == "quit.ae:ok" && exitSeen;
        Report("live-clean-quit", pass, new JsonObject {
            ["pid"] = hostPid, ["rungs"] = new JsonArray(rungs.Select(static rung => (JsonNode)rung).ToArray()),
            ["kqueueConfirmed"] = exitSeen, ["watchMode"] = watch.Mode, ["journal"] = "appended to ~/.rasm/rhino-bridge-quits.jsonl",
        });
    }

    // Cycle B: launch -> SIGSTOP (silent host, alive) -> SIGCONT -> kill -9 mid-connect -> kqueue
    // HostExited -> .ips diff -> journal the window -> instance-scoped reconcile clears markers.
    private static void LiveCycleKillMidConnect(BundleInfo bundle) {
        SessionPolicy policy = SessionPolicy.Default;
        Seq<string> ipsBaseline = Evidence.IpsBaseline(bundle);
        long launchedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Fin<Unit> launched = bundle.Launch(policy.ToolDeadline);
        Option<int> pid = launched.IsSucc ? AwaitPid(policy.LaunchDeadline) : Option<int>.None;
        if (pid.Case is not int hostPid) {
            Report("live-kill9-mid-connect", false, new JsonObject { ["outcome"] = launched.IsFail ? "launch failed" : "no pid within deadline" });
            return;
        }
        LiveHost host = RealHost(hostPid, bundle);
        SessionSignal? seen = null;
        using ManualResetEventSlim raised = new(false);
        using HostWatch watch = HostWatch.Attach(hostPid, signal => { seen = signal; raised.Set(); }, policy.WatchPoll, TimeProvider.System);
        Signal(hostPid, "-STOP");
        Thread.Sleep(1_500);
        bool aliveSilent = Posix.Alive(hostPid) && seen is null;
        SessionState connecting = new SessionState.Connecting(Host: host, PollsRemaining: 0);
        SessionState dialog = SessionDispatch.Apply(connecting, new SessionSignal.HeartbeatSilent(SilentFor: policy.HeartbeatWindow + TimeSpan.FromSeconds(1)), policy);
        Signal(hostPid, "-CONT");
        _ = Posix.Kill(hostPid);
        bool observed = raised.Wait(TimeSpan.FromSeconds(5));
        SessionState afterExit = seen is null ? connecting : SessionDispatch.Apply(connecting, seen, policy);
        long retiredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _ = QuitJournal.Append(QuitJournal.CanonicalPath, new QuitJournalEntry(
            Pid: hostPid, StartedAtUnixMs: launchedAt, RetiredAtUnixMs: retiredAt, Rung: "gate.kill9", PipeName: host.Endpoint.PipeName));
        Thread.Sleep(4_000); // give macOS its async marker-write window before the pre-launch-placed reconcile
        Option<CrashSummary> ips = Evidence.IpsDiff(ipsBaseline, bundle);
        SupervisorRuntime runtime = new(
            Lease: Atom(Option<LeaseToken>.None), Clock: TimeProvider.System, Policy: policy,
            ArtifactRoot: Environment.CurrentDirectory, LeasePath: Lease.CanonicalPath, JournalPath: QuitJournal.CanonicalPath,
            Bundle: bundle, Root: CancellationToken.None);
        Fin<Seq<BridgeEvent>> swept = Reconcile.Run(bundle, Guid.NewGuid()).Run(runtime);
        Seq<BridgeEvent> facts = swept.IfFail(Seq<BridgeEvent>());
        string autosave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Autosave Information");
        bool residue = Directory.Exists(autosave) && Directory.GetFiles(autosave, $"Unsaved {bundle.CFBundleName} Document*").Length > 0;
        bool pass = aliveSilent && observed
            && dialog is SessionState.Faulted { Fault: BridgeFault.DialogSuspected }
            && afterExit is SessionState.Faulted { Fault: BridgeFault.LaunchFailed }
            && !residue;
        Report("live-kill9-mid-connect", pass, new JsonObject {
            ["pid"] = hostPid,
            ["sigstop"] = aliveSilent ? "alive+silent -> dialog-suspected" : "NOT silent",
            ["kqueue"] = observed ? "HostExited observed" : "missed",
            ["discriminated"] = afterExit is SessionState.Faulted { Fault: BridgeFault.LaunchFailed lf } ? lf.Detail : afterExit.GetType().Name,
            ["ipsDiff"] = ips.Case is CrashSummary crash ? $"{crash.ExceptionType} on {crash.Thread}" : "none (SIGKILL writes no crash report)",
            ["reconcileFacts"] = new JsonArray(facts.AsEnumerable().OfType<BridgeEvent.FactCase>()
                .Select(static fact => (JsonNode)$"{fact.Key}:{fact.Value.GetProperty("path").GetString()}").ToArray()),
            ["markerResidue"] = residue ? "RESIDUE LEFT" : "none",
        });
    }
}
