import CustomDump
import Darwin
import Network
import System
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ProcessInstanceTests {
    @Test(.timeLimit(.minutes(1)), arguments: ["127.0.0.1", "::1"])
    func listsLoopbackPortItListensOnUntilClosed(host: String) async throws {
        let current = try ProcessInstance(of: getpid())
        let parameters = NWParameters.tcp
        parameters.requiredLocalEndpoint = .hostPort(host: NWEndpoint.Host(host), port: .any)
        let listener = try NWListener(using: parameters)
        let (states, state) = AsyncStream<NWListener.State>.makeStream()
        listener.stateUpdateHandler = { update in state.yield(update) }
        listener.newConnectionHandler = { connection in connection.cancel() }
        listener.start(queue: .global())
        _ = await states.first { update in update == .ready }
        let port = try #require(listener.port?.rawValue)
        expectNoDifference(try current.listeningPorts().filter { listed in listed == port }, [port])
        listener.cancel()
        _ = await states.first { update in update == .cancelled }
        expectNoDifference(try current.listeningPorts().filter { listed in listed == port }, [])
    }

    @Test
    func readsAChildSpawnedSuspendedAsStoppedThoughItsMainThreadReportsRunning() throws {
        var attributes: posix_spawnattr_t?
        try #require(unsafe posix_spawnattr_init(&attributes) == 0)
        defer { _ = unsafe posix_spawnattr_destroy(&attributes) }
        try #require(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_START_SUSPENDED)) == 0)
        var child: pid_t = 0
        let arguments = unsafe [strdup("sleep"), strdup("60"), nil]
        defer { for unsafe argument in unsafe arguments { unsafe free(argument) } }
        try #require(unsafe posix_spawn(&child, "/bin/sleep", nil, &attributes, arguments, nil) == 0)
        defer {
            kill(child, SIGKILL)
            var status: Int32 = 0
            _ = unsafe waitpid(child, &status, 0)
        }
        let process = try ProcessInstance(of: child)
        let first = try MainThread(of: process)
        let second = try MainThread(of: process)
        expectNoDifference([first.state, second.status(since: first)], [.stopped, .stopped])
    }

    @Test
    func identifiesAnotherUsersProcessAndListsItByExecutable() throws {
        try #require(getuid() != 0)
        let launchd = try ProcessInstance(of: 1)
        expectNoDifference(try ProcessInstance.all { executing in executing == "/sbin/launchd" }, [launchd])
        expectNoDifference(try launchd.executable(), "/sbin/launchd")
        #expect(!launchd.hasExited)
    }

    @Test
    func startsAfterAWallClockReadBeforeItsSpawnAndEndsAsAZombie() throws {
        let requested = clock_gettime_nsec_np(CLOCK_REALTIME) / NSEC_PER_USEC
        var child: pid_t = 0
        let arguments = unsafe [strdup("sleep"), strdup("60"), nil]
        defer { for unsafe argument in unsafe arguments { unsafe free(argument) } }
        try #require(unsafe posix_spawn(&child, "/bin/sleep", nil, nil, arguments, nil) == 0)
        defer {
            var status: Int32 = 0
            _ = unsafe waitpid(child, &status, 0)
        }
        let process = try ProcessInstance(of: child)
        #expect(process.started >= requested)
        kill(child, SIGKILL)
        var exited = unsafe siginfo_t()
        try #require(unsafe waitid(P_PID, id_t(child), &exited, WEXITED | WNOWAIT) == 0)
        #expect(throws: Errno.noSuchProcess) { try ProcessInstance(of: child) }
        #expect(process.hasExited)
    }
}
