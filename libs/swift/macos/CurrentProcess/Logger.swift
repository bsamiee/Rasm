import Foundation
public import os

// --- [SERVICES] ------------------------------------------------------------------------

extension Logger {
    public init(category: String) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: category)
    }
}
