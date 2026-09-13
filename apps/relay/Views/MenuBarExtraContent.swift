import AppKit
import SwiftUI

struct MenuBarExtraContent: View {
  let store: AccountStore

  @Environment(\.openWindow) private var openWindow: OpenWindowAction
  @Environment(\.dismiss) private var dismiss: DismissAction
  @State private var contentHeight: CGFloat?

  private static let width: CGFloat = 384
  private static let contentMargin: CGFloat = 16
  private static let rowSpacing: CGFloat = 24
  private static let footerSpacing: CGFloat = 8
  private static let footerHeight: CGFloat = 28
  private static let screenMargin: CGFloat = 24

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
      .frame(height: scrollViewHeight)
      .scrollBounceBehavior(.basedOnSize)

      Menu {
        Button("Settings…", action: openSettings)
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
    .padding(Self.contentMargin)
    .frame(width: Self.width)
    .onAppear { store.setMenuBarExtraVisible(true) }
    .onDisappear { store.setMenuBarExtraVisible(false) }
  }

  private var accounts: some View {
    VStack(spacing: Self.rowSpacing) {
      if store.isLoading {
        ProgressView().controlSize(.small)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      } else if store.accounts.isEmpty {
        Button("Add Account…", action: openSettings)
          .disabled(!store.canAddAccount)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      } else {
        ForEach(store.accounts) { viewModel in
          AccountRow(
            viewModel: viewModel,
            select: { store.select(viewModel.id) },
            startSession: { store.startSession(viewModel.id) }
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

  private var scrollViewHeight: CGFloat? {
    contentHeight.map { height in
      NSScreen.main.map { screen in
        let available: CGFloat =
          screen.visibleFrame.height - Self.screenMargin - Self.contentMargin * 2
          - Self.footerSpacing - Self.footerHeight
        return min(height, max(0, available))
      } ?? height
    }
  }

  private func openSettings() {
    dismiss()
    NSApplication.shared.activate()
    openWindow(id: "settings")
  }
}

private struct AccountRow: View {
  let viewModel: AccountViewModel
  let select: () -> Void
  let startSession: () -> Void

  @State private var isExpanded: Bool = false

  private static let labelMinHeight: CGFloat = 20

  var body: some View {
    VStack(alignment: .leading, spacing: 8) {
      Button(action: select) {
        HStack(alignment: .firstTextBaseline, spacing: 8) {
          Text(viewModel.account.identity.email)
            .fixedSize(horizontal: false, vertical: true)
            .frame(maxWidth: .infinity, alignment: .leading)
          selectionIndicator
            .animation(.default, value: viewModel.isSelected)
        }
        .frame(minHeight: Self.labelMinHeight)
      }
      .buttonStyle(.accessoryBar)
      .disabled(!viewModel.canSelect)
      .accessibilityLabel(viewModel.account.identity.email)
      .accessibilityAddTraits(viewModel.isSelected ? .isSelected : [])
      .accessibilityHint("Use this \(viewModel.account.provider.name) account")
      .help(viewModel.isSelected ? "Working account" : "Use this account")

      HStack(alignment: .top, spacing: 8) {
        SessionButton(
          viewModel: viewModel, toggleDetails: { isExpanded.toggle() },
          startSession: startSession)

        VStack(alignment: .leading, spacing: 8) {
          UsageGauge(
            quota: .session, window: viewModel.usageState.snapshot?.session,
            isCurrent: viewModel.isUsageCurrent)
          if isExpanded, let reset: Date = viewModel.usageState.snapshot?.session?.resetsAt {
            Text(
              "Resets \(reset, format: .dateTime.year().month(.abbreviated).day().hour().minute().timeZone(.specificName(.short)))"
            )
            .font(.subheadline)
            .foregroundStyle(.secondary)
            .fixedSize(horizontal: false, vertical: true)
          }
          UsageGauge(
            quota: .weekly, window: viewModel.usageState.snapshot?.weekly,
            isCurrent: viewModel.isUsageCurrent)
          if viewModel.account.provider == .claude,
            let fable: QuotaWindow = viewModel.usageState.snapshot?.fable
          {
            UsageGauge(quota: .fable, window: fable, isCurrent: viewModel.isUsageCurrent)
          }
          let note: String? =
            switch (viewModel.issue, viewModel.authentication) {
            case (.some(let issue), _): issue
            case (.none, .signInRequired): "Sign in required"
            case (.none, .connected): nil
            }
          if let note {
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
    if viewModel.operation == .selecting {
      ProgressView().controlSize(.mini)
    } else if viewModel.isSelected {
      Image(systemName: "checkmark")
        .font(.subheadline.weight(.medium))
        .foregroundStyle(.secondary)
        .accessibilityHidden(true)
    }
  }
}

private struct SessionButton: View {
  let viewModel: AccountViewModel
  let toggleDetails: () -> Void
  let startSession: () -> Void

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      let running: Bool = isRunning(at: context.date)
      let button = Button {
        if running { toggleDetails() } else { startSession() }
      } label: {
        switch viewModel.operation {
        case .starting, .awaitingWindow:
          ProgressView().controlSize(.small)
        case .idle, .refreshing, .selecting, .signingIn, .signingOut, .removing:
          Image(viewModel.account.provider.symbol)
        }
      }
      Group {
        if running {
          button.buttonStyle(.borderedProminent)
        } else {
          button.buttonStyle(.bordered)
        }
      }
      .buttonBorderShape(.circle)
      .controlSize(.large)
      .animation(.default, value: running)
      .disabled(!viewModel.canStartSession)
      .accessibilityLabel(viewModel.account.provider.name)
      .accessibilityValue(status(at: context.date))
      .accessibilityHint(running ? "Show session details" : "Start a session")
      .help(actionDescription(at: context.date))
    }
  }

  private func isRunning(at now: Date) -> Bool {
    guard case .current(let usage) = viewModel.usageState else { return false }
    return if case .running = usage.sessionState(at: now) { true } else { false }
  }

  private func status(at now: Date) -> String {
    switch viewModel.operation {
    case .starting: return "Starting session"
    case .awaitingWindow: return "Waiting for the provider’s reset time"
    case .idle, .refreshing, .selecting, .signingIn, .signingOut, .removing:
      if viewModel.authentication == .signInRequired { return "Sign in required" }
      guard case .current(let usage) = viewModel.usageState else { return "Usage unavailable" }
      switch usage.sessionState(at: now) {
      case .idle: return "Ready to start"
      case .running: return "Session running"
      case .unknown: return "Session state unavailable"
      }
    }
  }

  private func actionDescription(at now: Date) -> String {
    if viewModel.authentication == .signInRequired { return "Sign in from Settings" }
    switch viewModel.operation {
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
