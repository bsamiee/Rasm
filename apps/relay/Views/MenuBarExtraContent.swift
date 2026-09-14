import AppKit
import SwiftUI

struct MenuBarExtraContent: View {
  let store: AccountStore

  @Environment(\.openWindow) private var openWindow: OpenWindowAction
  @Environment(\.dismiss) private var dismiss: DismissAction
  @State private var accountToRemove: AccountModel?

  var body: some View {
    VStack(spacing: 8) {
      ScrollView {
        accounts
      }
      .fixedSize(horizontal: false, vertical: true)
      .scrollBounceBehavior(.basedOnSize)

      HStack {
        Spacer()
        Menu {
          Button("Settings…", action: openSettings)
            .keyboardShortcut(",", modifiers: .command)
          Divider()
          Button("Quit Relay") { NSApplication.shared.terminate(nil) }
            .keyboardShortcut("q", modifiers: .command)
        } label: {
          Image(systemName: "gearshape")
            .font(.body)
        }
        .menuIndicator(.hidden)
        .buttonStyle(.accessoryBar)
        .accessibilityLabel("Relay menu")
      }
    }
    .padding(16)
    .frame(width: 384)
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
    VStack(spacing: 24) {
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

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      VStack(alignment: .leading, spacing: 8) {
        Text(model.account.identity.email)
          .fixedSize(horizontal: false, vertical: true)
        HStack(alignment: .glyphRow, spacing: 12) {
          providerGlyph
          VStack(alignment: .leading, spacing: 8) {
            gauges(at: context.date)
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

  private var providerGlyph: some View {
    Button {
      store.select(model.id)
    } label: {
      if model.isSelected {
        Image(model.account.provider.symbol)
          .foregroundStyle(Color.accentColor)
      } else {
        Image(model.account.provider.symbol)
      }
    }
    .buttonStyle(.accessoryBar)
    .foregroundStyle(.secondary)
    .disabled(!model.isSelected && !model.canSelect)
    .overlay {
      if model.operation == .selecting { ProgressView().controlSize(.mini) }
    }
    .accessibilityLabel(model.account.provider.name)
    .accessibilityAddTraits(model.isSelected ? .isSelected : [])
    .help(model.isSelected ? "Active account" : "Switch account")
  }

  @ViewBuilder
  private func gauges(at now: Date) -> some View {
    let usage: AccountUsage? = model.usage.usage
    let isCurrent: Bool = model.usage.isCurrent
    let session: QuotaWindow? = usage?.session
    let isStarting: Bool = model.operation == .starting
    let showsStart: Bool = isStarting || model.canStartSession(at: now)
    UsageGauge(
      title: "Session",
      reading: UsagePresentation.sessionReading(
        session, availability: model.availability(at: now), isCurrent: isCurrent, at: now),
      fraction: showsStart ? 0 : session?.used.fraction ?? 0, isCurrent: isCurrent,
      detail: UsagePresentation.resetTooltip(session?.resetsAt), leadsGlyphRow: true
    ) {
      if showsStart {
        HStack(alignment: .firstTextBaseline, spacing: 8) {
          if isStarting, let started: Date = model.operationStartedAt {
            Text(.durationOffset(to: started), format: .time(pattern: .minuteSecond))
              .font(.caption)
              .monospacedDigit()
              .foregroundStyle(.secondary)
          }
          sessionStart(isStarting: isStarting)
        }
        .transition(.opacity)
      }
    }
    .animation(.default, value: showsStart)
    if let usage {
      let weekly: [QuotaWindow] = (usage.weekly.map { [$0] } ?? []) + usage.models
      ForEach(Array(weekly.enumerated()), id: \.offset) { _, window in
        UsageGauge(
          title: window.kind.name, reading: UsagePresentation.reading(window, at: now),
          fraction: window.used.fraction, isCurrent: isCurrent,
          detail: UsagePresentation.resetTooltip(window.resetsAt), leadsGlyphRow: false
        ) {}
      }
    }
  }

  private func sessionStart(isStarting: Bool) -> some View {
    Button {
      if isStarting { store.cancelOperation(model.id) } else { store.startSession(model.id) }
    } label: {
      Image(systemName: "arrow.trianglehead.clockwise")
        .font(.body)
    }
    .buttonStyle(.accessoryBar)
    .overlay {
      if isStarting { ProgressView().controlSize(.mini) }
    }
    .accessibilityLabel(isStarting ? "Cancel" : "Start session")
    .help(isStarting ? "Cancel" : "Start session")
  }

  private func note(at now: Date) -> String? {
    switch (model.issue, model.operation, model.authentication) {
    case (.some(let issue), _, _): issue
    case (.none, .signingOut, _), (.none, .removing, _): model.operation?.description
    case (.none, _, .signInRequired): "Sign in required"
    case (.none, _, .connected): UsagePresentation.blockedLine(model.availability(at: now))
    }
  }
}
