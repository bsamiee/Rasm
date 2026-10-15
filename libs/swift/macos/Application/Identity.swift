public import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Bundle {
    public static var identity: String {
        main.bundleIdentifier ?? ProcessInfo.processInfo.processName
    }
}
