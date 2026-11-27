package import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

private enum CodingKeys: CodingKey {
    case subject, reason, values
}

package protocol StatusCode {
    var name: String { get }
}

extension StatusCode {
    package static func constant(of status: OSStatus) -> String {
        let stated: Any? = NSError.userInfoValueProvider(forDomain: NSOSStatusErrorDomain)?(NSError(domain: NSOSStatusErrorDomain, code: Int(status)), NSDebugDescriptionErrorKey)
        return (stated as? String)?.split(separator: ":", maxSplits: 1).first.map { constant in constant.trimmingCharacters(in: .whitespaces) } ?? String(status)
    }
}

package protocol ModuleError: CustomNSError, Encodable, LocalizedError {
    associatedtype Reason: Sendable

    var subject: String { get }
    var reason: Reason { get }
}

extension ModuleError {
    // --- [TEXT]

    private func fact(_ value: Any) -> String? {
        let mirror = Mirror(reflecting: value)
        let separator: String? =
            switch mirror.displayStyle {
                case .optional, .tuple: " "
                case .collection, .set, .dictionary: ", "
                default: nil
            }
        let parts: [String] = separator == nil ? [] : mirror.children.compactMap { (child: Mirror.Child) -> String? in fact(child.value) }
        let localized: Bool = type(of: value) is NSError.Type || value is any CustomNSError || (value as? any LocalizedError)?.errorDescription != nil
        let text: String =
            switch (value, separator) {
                case (let duration as Duration, _): duration.formatted(.units(allowed: [.hours, .minutes, .seconds, .milliseconds], width: .narrow))
                case (let nested as Reason, _): stated(nested).text
                case (let status as any StatusCode, _): status.name
                case (let error as NSError, _) where localized: [error.localizedDescription, error.userInfo[NSDebugDescriptionErrorKey] as? String].compactMap(\.self).joined(separator: " ")
                case (_, let separator?): parts.joined(separator: separator)
                case (_, nil): String(describing: value)
            }
        return text.isEmpty ? nil : text
    }

    private func stated(_ reason: Reason) -> (label: String, values: [String], text: String) {
        let held: Mirror.Child? = Mirror(reflecting: reason).children.first
        let payload: Mirror? = held.map { (held: Mirror.Child) -> Mirror in Mirror(reflecting: held.value) }
        let facts: [Any] =
            switch (held, payload) {
                case (_, let payload?) where payload.displayStyle == .tuple: payload.children.map(\.value)
                case (let held?, _): [held.value]
                case (nil, _): []
            }
        let label: String = held?.label ?? String(describing: reason)
        let words: String = label.replacing(/\p{Lu}/) { (capital: Regex<Substring>.Match) -> String in " " + capital.output.lowercased() }
        let values: [String] = facts.compactMap { (value: Any) -> String? in fact(value) }
        return (label, values, values.isEmpty ? words : "\(words) \(values.joined(separator: ", "))")
    }

    package var errorDescription: String? { "\(subject) \(stated(reason).text)" }

    // --- [ENCODING]

    package func encode(to encoder: any Encoder) throws {
        let (label, values, _): (String, [String], String) = stated(reason)
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(subject, forKey: .subject)
        try fields.encode(label, forKey: .reason)
        try fields.encode(values, forKey: .values)
    }
}
