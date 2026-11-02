import AppKit
import Applications
import CustomDump
import Encoding
import Foundation
import OrderedCollections
import TestSupport
import Testing

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
            var shown: (host: String, element: Element)?
            for await prompt: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>) in await NSWorkspace.shared.systemPrompts(
                naming: [name],
                notifications: [kAXWindowCreatedNotification],
                polling: heartbeat,
            ) {
                guard let element: Element = prompt.prompts.values.lazy.flatMap(\.elements).first(where: \.requiresAnswer) else {
                    if shown != nil { break }
                    continue
                }
                if shown == nil {
                    shown = (prompt.host, element)
                    CFUserNotificationCancel(alert)
                }
            }
            let prompt: (host: String, element: Element) = try #require(shown)
            expectNoDifference(prompt.host, "com.apple.UserNotificationCenter")
            expectNoDifference(prompt.element.subrole, kAXSystemDialogSubrole)
            expectNoDifference(prompt.element.detail?.text.contains(name), true)
        }
    }
}
