import SwiftUI

struct UsageGauge<Accessory: View>: View {
  let title: String
  let reading: String?
  let fraction: Double
  let isCurrent: Bool
  let detail: String
  let leadsGlyphRow: Bool
  @ViewBuilder let accessory: Accessory

  var body: some View {
    VStack(alignment: .leading, spacing: 4) {
      HStack(alignment: .firstTextBaseline, spacing: 8) {
        label
        Spacer(minLength: 8)
        if let reading {
          Text(reading)
            .monospacedDigit()
            .foregroundStyle(isCurrent ? .secondary : .tertiary)
            .contentTransition(.opacity)
        }
      }
      .accessibilityRepresentation {
        ProgressView(value: fraction) {
          Text(title)
        } currentValueLabel: {
          if let reading { Text(reading) }
        }
      }
      .help(detail)
      .overlay(alignment: .trailing) { accessory }
      .font(.subheadline)

      ProgressView(value: fraction)
        .progressViewStyle(CapsuleProgressStyle(tint: isCurrent ? Color.accentColor : .secondary))
        .accessibilityHidden(true)
        .help(detail)
    }
  }

  @ViewBuilder
  private var label: some View {
    let text: Text = Text(title).foregroundStyle(isCurrent ? .primary : .secondary)
    if leadsGlyphRow {
      text.alignmentGuide(.glyphRow) { dimensions in dimensions[VerticalAlignment.center] }
    } else {
      text
    }
  }
}

struct CapsuleProgressStyle: ProgressViewStyle {
  let tint: Color

  func makeBody(configuration: Configuration) -> some View {
    Capsule()
      .fill(.quaternary)
      .overlay {
        Rectangle()
          .fill(tint)
          .scaleEffect(x: configuration.fractionCompleted ?? 0, y: 1, anchor: .leading)
      }
      .clipShape(Capsule())
      .frame(height: 6)
  }
}

extension VerticalAlignment {
  private enum GlyphRow: AlignmentID {
    static func defaultValue(in context: ViewDimensions) -> CGFloat {
      context[VerticalAlignment.center]
    }
  }

  static let glyphRow: VerticalAlignment = VerticalAlignment(GlyphRow.self)
}
