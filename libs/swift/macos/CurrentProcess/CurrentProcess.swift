import Foundation
public import System
public import os

// --- [SERVICES] ------------------------------------------------------------------------

extension Logger {
    public init(category: String) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: category)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func disclaimResponsibility() throws(Errno) {
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    guard responsiblePID(getpid()) != getpid() else { return }
    guard let executable: String = Bundle.main.executablePath else { throw .noSuchFileOrDirectory }
    var attributes: posix_spawnattr_t?
    try check(unsafe posix_spawnattr_init(&attributes))
    defer { _ = unsafe posix_spawnattr_destroy(&attributes) }
    try check(unsafe disclaim(&attributes, 1))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}

@_silgen_name("responsibility_get_pid_responsible_for_pid")
private func responsiblePID(_: pid_t) -> pid_t
@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_: UnsafeMutablePointer<posix_spawnattr_t?>, _: Int32) -> Int32
