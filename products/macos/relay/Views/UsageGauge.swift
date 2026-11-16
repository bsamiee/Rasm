import DeveloperToolsSupport
import Foundation
import SwiftUI

// --- [CONSTANTS] -----------------------------------------------------------------------

nonisolated let readingSeparator: String = " · "

// --- [OPERATIONS] ----------------------------------------------------------------------

extension VerticalAlignment {
    private enum GlyphRow: AlignmentID {
        static func defaultValue(in context: ViewDimensions) -> CGFloat {
            context[VerticalAlignment.center]
        }
    }

    static let glyphRow = VerticalAlignment(GlyphRow.self)
}

extension Provider {
    var symbol: ImageResource {
        switch self {
            case .claude: .claude
            case .openAI: .openAI
        }
    }
}

nonisolated extension FormatStyle where Self == Date.FormatStyle {
    static var monthDay: Self { .dateTime.month(.abbreviated).day() }
    static var detail: Self { .dateTime.weekday(.wide).month().day().hour().minute() }
}

extension QuotaWindow {
    func resetText(at now: Date) -> String? {
        guard !hasReset(at: now), let resetsAt else { return nil }
        return switch resetsAt.timeIntervalSince(now) {
            case ..<60: "<1m"
            case let remaining where remaining < 86_400: Duration.seconds(remaining).formatted(Duration.UnitsFormatStyle(allowedUnits: [.hours, .minutes], width: .narrow))
            case ..<TimeInterval(6 * 86_400): resetsAt.formatted(.dateTime.weekday(.abbreviated).hour().minute())
            default: resetsAt.formatted(.monthDay)
        }
    }

    func reading(at now: Date) -> String {
        let percentage: String? =
            switch percent {
                case 0: nil
                case ..<1: "<1%"
                default: fraction.formatted(.percent.rounded(rule: .down).precision(.fractionLength(0)))
            }
        return hasReset(at: now) ? "Reset" : [percentage, resetText(at: now)].compactMap(\.self).joined(separator: readingSeparator)
    }
}

// --- [VIEWS] ---------------------------------------------------------------------------

struct UsageGauge: View {
    let window: QuotaWindow
    let isCurrent: Bool
    let now: Date
    let leadsGlyphRow: Bool

    var body: some View {
        let reading = window.reading(at: now)
        let detail = window.resetsAt.map { reset in reset.formatted(.detail) } ?? "No reset"
        let hasReset = window.hasReset(at: now)
        let isCurrent = isCurrent && !hasReset
        let fraction = hasReset ? 0 : window.fraction
        let label = Text(window.kind.name).foregroundStyle(isCurrent ? .primary : .secondary)
        VStack(alignment: .leading, spacing: 4) {
            HStack(alignment: .firstTextBaseline, spacing: 8) {
                if leadsGlyphRow { label.alignmentGuide(.glyphRow) { dimensions in dimensions[VerticalAlignment.center] } } else { label }
                Spacer(minLength: 8)
                Text(reading)
                    .monospacedDigit()
                    .foregroundStyle(isCurrent ? .secondary : .tertiary)
                    .contentTransition(.opacity)
            }
            .accessibilityRepresentation {
                ProgressView(value: fraction) {
                    Text(window.kind.name)
                } currentValueLabel: {
                    Text(reading)
                }
            }
            .help(detail)
            .font(.subheadline)

            ProgressView(value: fraction)
                .tint(isCurrent ? Color.accentColor : .secondary)
                .accessibilityHidden(true)
                .help(detail)
        }
    }
}
