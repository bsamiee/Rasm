import AppKit
import SwiftUI

struct RelayMenu: View {
  let store: RelayStore

  @Environment(\.openSettings) private var openSettings: OpenSettingsAction
  @Environment(\.dismiss) private var dismiss: DismissAction
  @State private var contentHeight: CGFloat?

  private static let inset: CGFloat = 16
  private static let accountSpacing: CGFloat = 24
  private static let footerSpacing: CGFloat = 8
  private static let footerHeight: CGFloat = 28
  private static let menuBarClearance: CGFloat = 24

  var body: some View {
    VStack(spacing: Self.footerSpacing) {
      ScrollView {
        accounts
          .onGeometryChange(for: CGFloat.self) { geometry in
            geometry.size.height
          } action: { height in
            contentHeight = height
          }
      }
      .frame(height: viewportHeight)
      .scrollBounceBehavior(.basedOnSize)

      Menu {
        Button("Settings…", action: showSettings)
          .keyboardShortcut(",", modifiers: .command)
        Divider()
        Button("Quit Relay") { NSApplication.shared.terminate(nil) }
          .keyboardShortcut("q", modifiers: .command)
      } label: {
        Image(systemName: "gearshape")
          .font(.callout)
          .frame(width: Self.footerHeight, height: Self.footerHeight)
      }
      .menuIndicator(.hidden)
      .buttonStyle(.accessoryBar)
      .fixedSize()
      .frame(maxWidth: .infinity, alignment: .trailing)
      .accessibilityLabel("Relay menu")
    }
    .padding(Self.inset)
    .frame(width: 384)
    .onAppear { store.setPanelVisible(true) }
    .onDisappear { store.setPanelVisible(false) }
  }

  private var accounts: some View {
    VStack(spacing: Self.accountSpacing) {
      if store.isLoading {
        ProgressView().controlSize(.small)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      } else if store.accounts.isEmpty {
        Button("Add Account…", action: showSettings)
          .disabled(!store.canAddAccount)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      } else {
        ForEach(store.accounts) { presentation in
          RelayAccountCard(
            presentation: presentation,
            isExpanded: store.expandedSessions.contains(presentation.id),
            select: { store.select(presentation.id) },
            activateProvider: { store.activateProvider(presentation.id) }
          )
        }
      }

      if let issue: String = store.issue {
        Text(issue)
          .font(.subheadline)
          .foregroundStyle(.secondary)
          .frame(maxWidth: .infinity, alignment: .leading)
          .fixedSize(horizontal: false, vertical: true)
      }
    }
  }

  private var viewportHeight: CGFloat? {
    guard let contentHeight else { return nil }
    guard let screen: NSScreen = NSScreen.main else { return contentHeight }
    let available: CGFloat =
      screen.visibleFrame.height - Self.menuBarClearance - Self.inset * 2 - Self.footerSpacing
      - Self.footerHeight
    return min(contentHeight, max(0, available))
  }

  private func showSettings() {
    dismiss()
    NSApplication.shared.activate()
    openSettings()
  }
}

private struct RelayAccountCard: View {
  let presentation: AccountPresentation
  let isExpanded: Bool
  let select: () -> Void
  let activateProvider: () -> Void

  var body: some View {
    VStack(alignment: .leading, spacing: 8) {
      Button(action: select) {
        HStack(alignment: .firstTextBaseline, spacing: 8) {
          Text(presentation.account.identity.email)
            .fixedSize(horizontal: false, vertical: true)
            .frame(maxWidth: .infinity, alignment: .leading)
          selectionIndicator
            .animation(.default, value: presentation.isSelected)
        }
      }
      .buttonStyle(.accessoryBar)
      .disabled(
        presentation.authentication != .connected
          || (presentation.operation != .idle && presentation.operation != .awaitingWindow)
      )
      .accessibilityLabel(presentation.account.identity.email)
      .accessibilityValue(presentation.isSelected ? "Working account" : "")
      .accessibilityHint("Use this \(presentation.account.provider.name) account")
      .help(presentation.isSelected ? "Working account" : "Use this account")

      HStack(alignment: .top, spacing: 8) {
        ProviderSessionButton(presentation: presentation, action: activateProvider)

        VStack(alignment: .leading, spacing: 8) {
          UsageGauge(
            period: .session, quota: presentation.usage?.session,
            isCurrent: presentation.isUsageCurrent)
          if isExpanded, let reset: Date = presentation.usage?.session?.resetsAt {
            Text(
              "Resets \(reset, format: .dateTime.year().month(.abbreviated).day().hour().minute().timeZone(.specificName(.short)))"
            )
            .font(.subheadline)
            .foregroundStyle(.secondary)
            .fixedSize(horizontal: false, vertical: true)
          }
          UsageGauge(
            period: .weekly, quota: presentation.usage?.weekly,
            isCurrent: presentation.isUsageCurrent)
          if presentation.account.provider == .claude,
            let fable: QuotaWindow = presentation.usage?.fable
          {
            UsageGauge(period: .fable, quota: fable, isCurrent: presentation.isUsageCurrent)
          }
          if let note: String =
            presentation.issue
            ?? (presentation.authentication == .signInRequired ? "Sign in required" : nil)
          {
            Text(note)
              .font(.subheadline)
              .foregroundStyle(.secondary)
              .fixedSize(horizontal: false, vertical: true)
          }
        }
        .animation(.default, value: isExpanded)
      }
    }
  }

  @ViewBuilder
  private var selectionIndicator: some View {
    if presentation.operation == .selecting {
      ProgressView().controlSize(.mini)
    } else if presentation.isSelected {
      Image(systemName: "checkmark")
        .font(.subheadline.weight(.medium))
        .foregroundStyle(.secondary)
        .accessibilityHidden(true)
    }
  }
}

private struct ProviderSessionButton: View {
  let presentation: AccountPresentation
  let action: () -> Void

  private static let markSize: CGFloat = 20

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      let running: Bool = isRunning(at: context.date)
      Button(action: action) {
        Group {
          switch presentation.operation {
          case .starting, .awaitingWindow:
            ProgressView().controlSize(.small)
          case .idle, .refreshing, .selecting, .signingIn, .signingOut, .removing:
            Image(presentation.account.provider.name)
              .resizable()
              .scaledToFit()
              .overlay(alignment: .bottomTrailing) {
                if running {
                  Circle().fill(.green).frame(width: 6, height: 6)
                }
              }
          }
        }
        .frame(width: Self.markSize, height: Self.markSize)
        .animation(.default, value: running)
      }
      .buttonStyle(.accessoryBar)
      .disabled(presentation.authentication != .connected || presentation.operation != .idle)
      .accessibilityLabel(presentation.account.provider.name)
      .accessibilityValue(status(at: context.date))
      .accessibilityHint(running ? "Show session details" : "Start the usage window")
      .help(actionDescription(at: context.date))
    }
  }

  private func isRunning(at now: Date) -> Bool {
    guard presentation.isUsageCurrent, let usage: UsageSnapshot = presentation.usage else {
      return false
    }
    if case .running = usage.sessionState(at: now) { return true }
    return false
  }

  private func status(at now: Date) -> String {
    switch presentation.operation {
    case .starting: return "Starting session"
    case .awaitingWindow: return "Waiting for the provider’s reset time"
    case .idle, .refreshing, .selecting, .signingIn, .signingOut, .removing:
      if presentation.authentication == .signInRequired { return "Sign in required" }
      guard presentation.isUsageCurrent, let usage: UsageSnapshot = presentation.usage else {
        return "Usage unavailable"
      }
      switch usage.sessionState(at: now) {
      case .idle: return "Ready to start"
      case .running: return "Session running"
      case .unknown: return "Session state unavailable"
      }
    }
  }

  private func actionDescription(at now: Date) -> String {
    if presentation.authentication == .signInRequired { return "Sign in from Settings" }
    switch presentation.operation {
    case .idle: return isRunning(at: now) ? "Session details" : "Start session"
    case .refreshing: return "Refreshing usage"
    case .selecting: return "Switching account"
    case .starting, .awaitingWindow: return status(at: now)
    case .signingIn: return "Signing in"
    case .signingOut: return "Signing out"
    case .removing: return "Removing account"
    }
  }
}
