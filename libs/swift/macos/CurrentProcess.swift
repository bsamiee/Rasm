public import AppKit
public import System
public import os

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Logger {
    public init?(category: String) {
        guard let subsystem: String = Bundle.main.bundleIdentifier else { return nil }
        self.init(subsystem: subsystem, category: category)
    }
}

extension NSApplication {
    public func bringToFront() async -> Result<NSRunningApplication, any Error> {
        await Result { try await NSWorkspace.shared.openApplication(at: Bundle.main.bundleURL, configuration: NSWorkspace.OpenConfiguration()) }
    }
}

public func disclaimResponsibility() throws(Errno) {
    var executable: [CChar] = Array(repeating: 0, count: Int(MAXPATHLEN))
    var responsible: [CChar] = executable
    var length: Int = responsible.count
    guard unsafe proc_pidpath(getpid(), &executable, UInt32(executable.count)) > 0, unsafe responsibility(getpid(), nil, nil, &length, &responsible) == 0, unsafe strcmp(executable, responsible) != 0
    else { return }
    var attributes: posix_spawnattr_t?
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    try check(unsafe posix_spawnattr_init(&attributes))
    try check(unsafe disclaim(&attributes, 1))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}

@_silgen_name("responsibility_get_responsible_for_pid")
private func responsibility(_ pid: pid_t, _: UnsafeMutablePointer<pid_t>?, _: UnsafeMutablePointer<UInt64>?, _ length: UnsafeMutablePointer<Int>, _ path: UnsafeMutablePointer<CChar>) -> Int32
@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_ attributes: UnsafeMutablePointer<posix_spawnattr_t?>, _ disclaim: Int32) -> Int32
