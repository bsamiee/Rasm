import Foundation
public import os

// --- [SERVICES] ------------------------------------------------------------------------

extension Logger {
    public init(for owner: any Any.Type) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: String(reflecting: owner))
    }
}
