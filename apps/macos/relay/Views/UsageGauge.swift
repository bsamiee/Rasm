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
        VStack(alignment: .leading, spacing: 4) {
            HStack(alignment: .firstTextBaseline, spacing: 8) {
                label
                Spacer(minLength: 8)
                Text(reading)
                    .monospacedDigit()
                    .foregroundStyle(isCurrent ? .secondary : .tertiary)
                    .contentTransition(.opacity)
            }
            .accessibilityRepresentation {
                ProgressView(value: window.used.fraction) {
                    Text(window.kind.name)
                } currentValueLabel: {
                    Text(reading)
                }
            }
            .help(detail)
            .font(.subheadline)

            ProgressView(value: window.used.fraction)
                .tint(isCurrent ? Color.accentColor : .secondary)
                .accessibilityHidden(true)
                .help(detail)
        }
    }

    @ViewBuilder
    private var label: some View {
        let text: Text = Text(window.kind.name).foregroundStyle(isCurrent ? .primary : .secondary)
        if leadsGlyphRow {
            text.alignmentGuide(.glyphRow) { dimensions in dimensions[VerticalAlignment.center] }
        } else {
            text
        }
    }
}
