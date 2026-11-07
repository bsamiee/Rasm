import SwiftUI

// --- [OPERATIONS] ----------------------------------------------------------------------

extension VerticalAlignment {
    private enum GlyphRow: AlignmentID {
        static func defaultValue(in context: ViewDimensions) -> CGFloat {
            context[VerticalAlignment.center]
        }
    }

    static let glyphRow: VerticalAlignment = VerticalAlignment(GlyphRow.self)
}

// --- [VIEWS] ---------------------------------------------------------------------------

struct UsageGauge: View {
    let window: QuotaWindow
    let isCurrent: Bool
    let now: Date
    let leadsGlyphRow: Bool

    var body: some View {
        let reading: String = window.reading(at: now)
        let detail: String = window.resetsAt.resetTooltip
        let hasReset: Bool = window.hasReset(at: now)
        let isCurrent: Bool = isCurrent && !hasReset
        let fraction: Double = hasReset ? 0 : window.used.fraction
        VStack(alignment: .leading, spacing: 4) {
            HStack(alignment: .firstTextBaseline, spacing: 8) {
                label(isCurrent: isCurrent)
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

    @ViewBuilder
    private func label(isCurrent: Bool) -> some View {
        let text: Text = Text(window.kind.name).foregroundStyle(isCurrent ? .primary : .secondary)
        if leadsGlyphRow {
            text.alignmentGuide(.glyphRow) { dimensions in dimensions[VerticalAlignment.center] }
        } else {
            text
        }
    }
}
