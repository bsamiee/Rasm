import AppKit
import CasePathsCore
import CurrentProcess
import Processes
import Sparkle
import SwiftUI
import os

// --- [VIEWS] ---------------------------------------------------------------------------

struct AccountRemovalAlert: ViewModifier {
    static let title: LocalizedStringKey = "Remove Account"

    let store: AccountStore
    @Binding var model: AccountModel?

    func body(content: Content) -> some View {
        content.alert(Self.title, item: $model) { model in
            Button("Cancel", role: .cancel) {}
                .keyboardShortcut(.defaultAction)
            Button(Self.title, role: .destructive) { store.remove(model.account.id) }
        } message: { model in
            let message: String = "Removal deletes \(model.account.identity.email) credential" + (model.isSelected ? " and signs \(model.account.provider.tool) out" : "")
            Text(message)
        }
    }
}

struct AccountMenuItems: View {
    let store: AccountStore
    let model: AccountModel
    let remove: () -> Void

    var body: some View {
        if let index: Int = store.accounts.firstIndex(where: { other in other.account.id == model.account.id }) {
            Button("Move Up") { store.moveAccounts(from: [index], to: index - 1) }
                .disabled(index == store.accounts.startIndex)
            Button("Move Down") { store.moveAccounts(from: [index], to: index + 2) }
                .disabled(index == store.accounts.endIndex - 1)
        }
        Divider()
        Button(AccountRemovalAlert.title, role: .destructive, action: remove)
            .disabled(model.isBusy)
    }
}

struct MenuBarExtraContent: View {
    let store: Result<AccountStore, LoginShellFailure>?
    let updater: SPUUpdater

    @Environment(\.openWindow) private var openWindow: OpenWindowAction
    @Environment(\.dismiss) private var dismiss: DismissAction
    @State private var accountToRemove: AccountModel?
    @State private var canCheckForUpdates: Bool = false

    var body: some View {
        VStack(spacing: 8) {
            ScrollView {
                TimelineView(.everyMinute) { context in accounts(at: context.date) }
            }
            .contentMargins(.horizontal, 16, for: .scrollContent)
            .frame(maxHeight: 960)
            .fixedSize(horizontal: false, vertical: true)
            .scrollBounceBehavior(.basedOnSize)

            HStack {
                Spacer()
                Menu {
                    Button("Settings", action: showSettings)
                        .keyboardShortcut(",", modifiers: .command)
                    Button("Check for Updates…", action: updater.checkForUpdates)
                        .disabled(!canCheckForUpdates)
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
            .padding(.horizontal, 16)
        }
        .padding(.vertical, 16)
        .frame(width: 384)
        .background { PanelToolTips() }
        .onReceive(updater.publisher(for: \.canCheckForUpdates)) { canCheck in canCheckForUpdates = canCheck }
    }

    private func accounts(at now: Date) -> some View {
        VStack(spacing: 24) {
            switch store {
                case .success(let store) where store.storageState != .loading:
                    VStack(spacing: 24) {
                        if store.accounts.isEmpty {
                            Button("Add Account", action: showSettings)
                                .disabled(!store.canAddAccount)
                                .frame(maxWidth: .infinity)
                                .padding(.vertical, 8)
                        } else {
                            ForEach(store.accounts, id: \.account.id) { model in
                                AccountCard(model: model, store: store, now: now, remove: { accountToRemove = model })
                            }
                        }
                    }
                    .onAppear { store.setMenuBarExtraVisible(true) }
                    .onDisappear { store.setMenuBarExtraVisible(false) }
                    .modifier(AccountRemovalAlert(store: store, model: $accountToRemove))
                case .success, .none:
                    ProgressView().controlSize(.small)
                        .frame(maxWidth: .infinity)
                        .padding(.vertical, 8)
                case .failure: EmptyView()
            }

            let issue: String? =
                switch store {
                    case .success(let store): store.issue(at: now)
                    case .failure(let error): error.localizedDescription
                    case .none: nil
                }
            if let issue {
                Text(issue)
                    .font(.subheadline)
                    .foregroundStyle(.secondary)
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .fixedSize(horizontal: false, vertical: true)
            }
        }
    }

    private func showSettings() {
        dismiss()
        openWindow(id: RelayApp.settingsWindow)
        Task(name: "Activate Relay") {
            let reopened: Result<NSRunningApplication, any Error> = await Result {
                try await NSWorkspace.shared.openApplication(at: Bundle.main.bundleURL, configuration: NSWorkspace.OpenConfiguration())
            }
            if case .failure(let error) = reopened {
                Logger(for: Self.self).error("Relay activation failed with \(error, privacy: .public)")
            }
        }
    }
}

private struct AccountCard: View {
    let model: AccountModel
    let store: AccountStore
    let now: Date
    let remove: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack(alignment: .firstTextBaseline) {
                Text(model.account.identity.email)
                    .fixedSize(horizontal: false, vertical: true)
                Spacer(minLength: 12)
                if let expiry: Date = model.usage.usage?.signInExpiresAt {
                    let tooltip: String = "Sign-in expires \(expiry.formatted(.detail))"
                    Text(expiry, format: .monthDay)
                        .font(.caption)
                        .monospacedDigit()
                        .foregroundStyle(.secondary)
                        .help(tooltip)
                        .accessibilityLabel(tooltip)
                }
            }
            HStack(alignment: .glyphRow, spacing: 12) {
                providerGlyph
                VStack(alignment: .leading, spacing: 8) {
                    gauges
                    Group {
                        if let note: String { Text(note) }
                        if let blocking: AccountOperation = model.blockingOperation {
                            Text(blocking.refusalDescription)
                        }
                    }
                    .font(.subheadline)
                    .foregroundStyle(.secondary)
                    .fixedSize(horizontal: false, vertical: true)
                }
            }
        }
        .contextMenu { AccountMenuItems(store: store, model: model, remove: remove) }
    }

    private var providerGlyph: some View {
        Button {
            store.select(model.account.id)
        } label: {
            Image(model.account.provider.symbol)
        }
        .buttonStyle(.accessoryBar)
        .foregroundStyle(model.isSelected ? AnyShapeStyle(Color.accentColor) : AnyShapeStyle(.secondary))
        .disabled(!model.isSelected && !model.canSelect)
        .overlay(alignment: .center) {
            if model.running?.kind == .selecting { ProgressView().controlSize(.mini) }
        }
        .accessibilityLabel(model.account.provider.name)
        .accessibilityAddTraits(model.isSelected ? .isSelected : [])
        .help(model.isSelected ? "Active account" : "Switch account")
    }

    @ViewBuilder
    private var gauges: some View {
        let usage: AccountUsage? = model.usage.usage
        let isCurrent: Bool = model.usage.is(\.current)
        let session: QuotaWindow? = usage?.session
        let hasSession: Bool = usage == nil || session != nil
        let isStarting: Bool = model.running?.kind == .starting
        let showsStart: Bool = isStarting || model.canStartSession(at: now)
        let startLabel: String = isStarting ? "Cancel" : "Start session"
        if hasSession {
            UsageGauge(
                title: "Session",
                reading: model.usage.sessionReading(at: now),
                fraction: showsStart ? 0 : session?.used.fraction ?? 0,
                isCurrent: isCurrent,
                detail: session.flatMap(\.resetsAt).resetTooltip,
                leadsGlyphRow: true,
            ) {
                if showsStart {
                    Button {
                        if isStarting { store.cancelOperation(model.account.id) } else { store.startSession(model.account.id) }
                    } label: {
                        Image(systemName: "arrow.trianglehead.clockwise")
                            .font(.body)
                            .opacity(isStarting ? 0 : 1)
                    }
                    .buttonStyle(.accessoryBar)
                    .overlay {
                        if isStarting { ProgressView().controlSize(.small) }
                    }
                    .accessibilityLabel(startLabel)
                    .help(startLabel)
                    .transition(.opacity)
                }
            }
            .animation(.default, value: showsStart)
        }
        if let usage {
            ForEach(usage.windows.filter { window in window.kind != .session }.enumerated(), id: \.offset) { offset, window in
                UsageGauge(
                    title: window.kind.name,
                    reading: window.reading(at: now),
                    fraction: window.used.fraction,
                    isCurrent: isCurrent,
                    detail: window.resetsAt.resetTooltip,
                    leadsGlyphRow: !hasSession && offset == 0,
                ) {}
            }
        }
    }

    private var note: String? {
        switch (model.issue, model.running?.kind, model.authentication) {
            case (.some(let issue), _, _): issue.description(at: now)
            case (.none, .signingOut, _), (.none, .removing, _): model.running?.kind.name.description
            case (.none, _, .signInRequired): "Sign in required"
            case (.none, _, .connected):
                model.usage.usage?.availability(at: now).blockedLine(at: now) ?? model.usage.usage?.signInLine(at: now)
        }
    }
}
