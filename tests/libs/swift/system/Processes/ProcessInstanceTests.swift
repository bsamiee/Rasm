import CustomDump
import Darwin
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
            expectNoDifference(try current.listening().filter { listed in listed == port }, [port])
        }
        #expect(try !current.listening().contains(port))
    }
}
