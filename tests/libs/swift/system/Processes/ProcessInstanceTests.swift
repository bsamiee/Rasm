import CustomDump
import Darwin
import System
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ProcessInstanceTests {
    @Test(arguments: [AF_INET, AF_INET6])
    func listsLoopbackPortItListensOnUntilClosed(family: Int32) throws {
        let current = try ProcessInstance(of: getpid())
        let port: in_port_t
        do {
            let listener = socket(family, SOCK_STREAM, IPPROTO_TCP)
            try #require(listener >= 0)
            defer { close(listener) }
            var address = sockaddr_storage()
            withUnsafeMutableBytes(of: &address) { bytes in
                switch family {
                    case AF_INET:
                        unsafe bytes.storeBytes(
                            of: sockaddr_in(
                                sin_len: UInt8(MemoryLayout<sockaddr_in>.size),
                                sin_family: sa_family_t(AF_INET),
                                sin_port: 0,
                                sin_addr: in_addr(s_addr: INADDR_LOOPBACK.bigEndian),
                                sin_zero: (0, 0, 0, 0, 0, 0, 0, 0),
                            ),
                            as: sockaddr_in.self,
                        )
                    default:
                        unsafe bytes.storeBytes(
                            of: sockaddr_in6(
                                sin6_len: UInt8(MemoryLayout<sockaddr_in6>.size),
                                sin6_family: sa_family_t(AF_INET6),
                                sin6_port: 0,
                                sin6_flowinfo: 0,
                                sin6_addr: in6addr_loopback,
                                sin6_scope_id: 0,
                            ),
                            as: sockaddr_in6.self,
                        )
                }
            }
            var length = socklen_t(address.ss_len)
            let bound = withUnsafeMutablePointer(to: &address) { address in
                unsafe address.withMemoryRebound(to: sockaddr.self, capacity: 1) { address in
                    unsafe bind(listener, address, length) == 0 && listen(listener, 1) == 0 && getsockname(listener, address, &length) == 0
                }
            }
            try #require(bound)
            port = withUnsafeBytes(of: address) { bytes in in_port_t(bigEndian: unsafe bytes.load(as: sockaddr_in.self).sin_port) }
            expectNoDifference(try current.listeningPorts().filter { listed in listed == port }, [port])
        }
        #expect(try !current.listeningPorts().contains(port))
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
