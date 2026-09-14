import AppKit
import SwiftUI

struct MenuBarExtraContent: View {
  let store: AccountStore

  @Environment(\.openWindow) private var openWindow: OpenWindowAction
  @Environment(\.dismiss) private var dismiss: DismissAction
  @State private var accountToRemove: AccountModel?

  private static let width: CGFloat = 384
  private static let contentMargin: CGFloat = 16
  private static let rowSpacing: CGFloat = 24
  private static let footerSpacing: CGFloat = 8
  private static let footerHeight: CGFloat = 28

  var body: some View {
    VStack(spacing: Self.footerSpacing) {
      ScrollView {
        accounts
      }
      .fixedSize(horizontal: false, vertical: true)
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
    .alert(
      "Remove Account",
      isPresented: Binding(
        get: { accountToRemove != nil }, set: { if !$0 { accountToRemove = nil } }),
      presenting: accountToRemove
    ) { model in
      Button("Cancel", role: .cancel) {}
        .keyboardShortcut(.defaultAction)
      Button("Remove Account", role: .destructive) { store.remove(model.id) }
    } message: { model in
      Text(RemovalMessage.text(for: model))
    }
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
        ForEach(store.accounts) { model in
          AccountCard(model: model, store: store, remove: { accountToRemove = model })
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

  private func openSettings() {
    dismiss()
    NSApplication.shared.activate()
    openWindow(id: "settings")
  }
}

enum RemovalMessage {
  static func text(for model: AccountModel) -> String {
    let removal: String = "Removing \(model.account.identity.email) deletes its saved sign-in"
    return model.isSelected && model.account.provider == .claude
      ? "\(removal) and signs Claude Code out"
      : removal
  }
}

private struct AccountCard: View {
  let model: AccountModel
  let store: AccountStore
  let remove: () -> Void

  private static let labelMinHeight: CGFloat = 20
  private static let glyphColumn: CGFloat = 24

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      VStack(alignment: .leading, spacing: 8) {
        emailRow
        HStack(alignment: .top, spacing: 12) {
          Image(model.account.provider.symbol)
            .foregroundStyle(.secondary)
            .frame(width: Self.glyphColumn, height: Self.labelMinHeight)
            .accessibilityLabel(model.account.provider.name)
          VStack(alignment: .leading, spacing: 8) {
            gauges(at: context.date)
            SessionControl(model: model, store: store, now: context.date)
            if let note: String = note(at: context.date) {
              Text(note)
                .font(.subheadline)
                .foregroundStyle(.secondary)
                .fixedSize(horizontal: false, vertical: true)
            }
          }
        }
      }
      .contextMenu {
        Button("Move Up") { store.moveAccount(model.id, by: -1) }
          .disabled(store.accounts.first?.id == model.id)
        Button("Move Down") { store.moveAccount(model.id, by: 1) }
          .disabled(store.accounts.last?.id == model.id)
        Divider()
        Button("Remove Account…", role: .destructive, action: remove)
          .disabled(model.isBusy)
      }
    }
  }

  private var emailRow: some View {
    Button {
      store.select(model.id)
    } label: {
      HStack(alignment: .firstTextBaseline, spacing: 8) {
        Text(model.account.identity.email)
          .fixedSize(horizontal: false, vertical: true)
          .frame(maxWidth: .infinity, alignment: .leading)
        if model.isSelected {
          Text("In use")
            .font(.caption)
            .foregroundStyle(.secondary)
          Image(systemName: "checkmark")
            .font(.subheadline.weight(.medium))
            .foregroundStyle(.secondary)
            .accessibilityHidden(true)
        }
      }
      .frame(minHeight: Self.labelMinHeight)
      .opacity(model.operation == .selecting ? 0.5 : 1)
      .overlay {
        if model.operation == .selecting { ProgressView().controlSize(.mini) }
      }
    }
    .buttonStyle(.accessoryBar)
    .disabled(!model.canSelect)
    .accessibilityLabel(model.account.identity.email)
    .accessibilityAddTraits(model.isSelected ? .isSelected : [])
    .accessibilityHint("Switches \(model.account.provider.name) to the account")
    .help(model.isSelected ? "In use" : "Switch \(model.account.provider.name) to the account")
  }

  @ViewBuilder
  private func gauges(at now: Date) -> some View {
    let usage: AccountUsage? = model.usage.usage
    let isCurrent: Bool = model.usage.isCurrent
    let availability: Availability? = model.availability(at: now)
    let session: QuotaWindow? = usage?.session
    UsageGauge(
      title: "Session",
      reading: UsagePresentation.sessionReading(
        session, availability: availability, isCurrent: isCurrent, at: now),
      fraction: session?.used.fraction, isCurrent: isCurrent,
      detail: session?.resetsAt.map { reset in UsagePresentation.remaining(until: reset, at: now) }
        ?? "No session window reported")
    if let usage {
      let weekly: [QuotaWindow] = (usage.weekly.map { [$0] } ?? []) + usage.models
      ForEach(Array(weekly.enumerated()), id: \.offset) { _, window in
        UsageGauge(
          title: window.kind.name,
          reading: UsagePresentation.reading(window, at: now),
          fraction: window.used.fraction, isCurrent: isCurrent,
          detail: window.resetsAt.map { reset in UsagePresentation.remaining(until: reset, at: now)
          }
            ?? "No reset time reported")
      }
      if let line: String = UsagePresentation.sharedResetLine(weekly, at: now) {
        Text(line)
          .font(.caption)
          .foregroundStyle(.tertiary)
          .fixedSize(horizontal: false, vertical: true)
      }
    }
  }

  private func note(at now: Date) -> String? {
    switch (model.issue, model.authentication) {
    case (.some(let issue), _): issue
    case (.none, .signInRequired): "Sign in required"
    case (.none, .connected): UsagePresentation.blockedLine(model.availability(at: now))
    }
  }
}

private struct SessionControl: View {
  let model: AccountModel
  let store: AccountStore
  let now: Date

  var body: some View {
    HStack(spacing: 8) {
      Button(action: act) {
        Text(label)
          .monospacedDigit()
          .frame(minWidth: 96)
      }
      .buttonStyle(.bordered)
      .controlSize(.small)
      .disabled(!isEnabled)
      .overlay {
        if model.operation == .starting { ProgressView().controlSize(.mini) }
      }
      .accessibilityLabel("Session")
      .accessibilityValue(label)
      .help(help)
      if model.operation == .starting, let started: Date = model.operationStartedAt {
        Text(.durationOffset(to: started), format: .time(pattern: .minuteSecond))
          .font(.caption)
          .monospacedDigit()
          .foregroundStyle(.secondary)
      } else if let operation: AccountOperation = model.operation, operation != .starting {
        Text(operation.description)
          .font(.caption)
          .foregroundStyle(.secondary)
      }
    }
  }

  private var label: String {
    switch (model.operation, model.availability(at: now)) {
    case (.starting, _): "Cancel"
    case (_, .running(let until)): UsagePresentation.countdown(until: until, at: now)
    case (_, .blocked): "Blocked"
    case (_, .ready), (_, .none): "Start Session"
    }
  }

  private var isEnabled: Bool {
    model.operation == .starting || model.canStartSession(at: now)
  }

  private var help: String {
    switch (model.operation, model.availability(at: now)) {
    case (.starting, _): "Cancel the session start"
    case (.some(let operation), _): operation.description
    case (_, .running(let until)): UsagePresentation.remaining(until: until, at: now)
    case (_, .blocked): UsagePresentation.blockedLine(model.availability(at: now)) ?? "Blocked"
    case (_, .ready), (_, .none):
      model.isConnected ? "Open a 5-hour window with one short message" : "Sign in from Settings"
    }
  }

  private func act() {
    if model.operation == .starting {
      store.cancelOperation(model.id)
    } else {
      store.startSession(model.id)
    }
  }
}
