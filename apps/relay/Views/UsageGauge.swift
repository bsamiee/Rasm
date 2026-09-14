import SwiftUI

struct UsageGauge: View {
  let title: String
  let reading: String
  let fraction: Double?
  let isCurrent: Bool
  let detail: String

  var body: some View {
    VStack(alignment: .leading, spacing: 4) {
      HStack(alignment: .firstTextBaseline, spacing: 8) {
        Text(title)
          .foregroundStyle(isCurrent ? .primary : .secondary)
        Spacer(minLength: 8)
        Text(reading)
          .monospacedDigit()
          .foregroundStyle(isCurrent ? .secondary : .tertiary)
      }
      .font(.subheadline)

      ProgressView(value: fraction ?? 0)
        .progressViewStyle(CapsuleProgressStyle(tint: isCurrent ? Color.accentColor : .secondary))
    }
    .accessibilityElement(children: .ignore)
    .accessibilityLabel("\(title) usage")
    .accessibilityValue(reading + (isCurrent ? "" : ", last reported usage"))
    .accessibilityHint(detail)
    .help(detail)
  }
}

struct CapsuleProgressStyle: ProgressViewStyle {
  let tint: Color

  private static let trackHeight: CGFloat = 6

  func makeBody(configuration: Configuration) -> some View {
    Capsule()
      .fill(.quaternary)
      .overlay {
        Rectangle()
          .fill(tint)
          .scaleEffect(x: configuration.fractionCompleted ?? 0, y: 1, anchor: .leading)
      }
      .clipShape(Capsule())
      .frame(height: Self.trackHeight)
  }
}
