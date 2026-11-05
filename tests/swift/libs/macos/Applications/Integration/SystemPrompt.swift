import AppKit
import CasePaths
import CasePathsCore
import CustomDump
import Encoding
import Foundation
import OrderedCollections
import TestSupport
import Testing

@testable import Applications

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct SystemPrompts {
        @Test(.timeLimit(.minutes(1)))
        func alertNamingAnApplicationShowsWithItsHostUntilAnswered() async throws {
            try checkAccessibilityTrust()
            let name: String = "Rasm Café \(UUID().uuidString.prefix(8))"
            let header: [CFString: String] = [kCFUserNotificationAlertHeaderKey: name]
            let created: CFUserNotification? = CFUserNotificationCreate(nil, 0, kCFUserNotificationPlainAlertLevel, nil, header as CFDictionary)
            let alert: CFUserNotification = try #require(created)
            defer { CFUserNotificationCancel(alert) }
            var shown: (host: String?, element: Element)?
            for await prompts: OrderedDictionary<NSRunningApplication, InstanceState> in NSWorkspace.shared.systemPrompts(naming: [name]) {
                guard let entry: (key: NSRunningApplication, value: InstanceState) = prompts.elements.first,
                    let element: Element = entry.value[case: \.running]?.asking.lazy.compactMap({ row in row.description[case: \.element] }).first
                else {
                    if shown != nil { break }
                    continue
                }
                if shown == nil {
                    shown = (entry.key.bundleIdentifier, element)
                    CFUserNotificationCancel(alert)
                }
            }
            let prompt: (host: String?, element: Element) = try #require(shown)
            expectNoDifference(prompt.host, "com.apple.UserNotificationCenter")
            expectNoDifference(prompt.element.kind, .system)
            expectNoDifference(prompt.element.detail?.text.contains(name), true)
        }
    }
}
