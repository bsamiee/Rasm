import AppKit
import CasePathsCore
import Logging
import Processes
import Sparkle
import SwiftUI

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
            Text(
                model.isActive
                    ? "Removal deletes \(model.account.identity.email) credential and signs \(model.account.provider.tool) out" : "Removal deletes \(model.account.identity.email) credential"
            )
        }
    }
}

struct AccountMenuItems: View {
    let store: AccountStore
    let model: AccountModel
    let remove: () -> Void

    var body: some View {
        if let index = store.accounts.firstIndex(where: { other in other.account.id == model.account.id }) {
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
    let store: Result<AccountStore, ProcessFailure>?
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
        .background { MenuBarExtraToolTips() }
        .onReceive(updater.publisher(for: \.canCheckForUpdates)) { canCheck in canCheckForUpdates = canCheck }
    }

    private func accounts(at now: Date) -> some View {
        Group {
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
                case .failure(let error): ContentUnavailableView("Relay Could Not Start", systemImage: "exclamationmark.triangle", description: Text(error.localizedDescription))
            }
        }
    }

    private func showSettings() {
        dismiss()
        openWindow(id: RelayApp.settingsWindow)
        Task(name: "Activate Relay") {
            let reopened = await Result {
                try await NSWorkspace.shared.openApplication(at: Bundle.main.bundleURL, configuration: NSWorkspace.OpenConfiguration())
            }
            if case .failure(let error) = reopened {
                Logger(label: String(reflecting: Self.self)).error("Relay activation failed", error: error)
            }
        }
    }
}

private struct AccountCard: View {
    let model: AccountModel
    let store: AccountStore
    let now: Date
    let remove: () -> Void

    private var note: String? {
        let expired = model.usage.usage?.signInExpiresAt.flatMap { expiry in expiry <= now ? expiry : nil }
        let parts: [String?] =
            switch (model.signIn, model.usage.usage, expired) {
                case (_, _, .some(let expiry)), (.expired(let expiry), _, .none): ["Sign-in expired \(expiry.formatted(.monthDay))"]
                case (.missing, _, .none): ["Sign in required"]
                case (.ended, _, .none): ["Login ended by \(model.account.provider.tool)"]
                case (.otherAccount, _, .none): ["Signed in to another account"]
                case (.none, .none, .none): []
                case (.none, .some(let reading), .none):
                    (reading.isBlocked(at: now) ? ["Blocked", reading.weekly?.resetText(at: now)] : [])
                        + [model.usage.is(\.stale) ? "Updated \(reading.observedAt.formatted(.relative(presentation: .named)))" : nil]
            }
        let text = parts.compactMap(\.self)
        return text.isEmpty ? nil : text.joined(separator: readingSeparator)
    }

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack(alignment: .firstTextBaseline) {
                Text("\(model.account.identity.email) \(Text(model.account.provider.name).foregroundStyle(.secondary))")
                    .fixedSize(horizontal: false, vertical: true)
                Spacer(minLength: 12)
                if let expiry = model.usage.usage?.signInExpiresAt, expiry > now {
                    let tooltip = "Sign-in expires \(expiry.formatted(.detail))"
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
                    if let note {
                        Text(note)
                            .font(.subheadline)
                            .foregroundStyle(.secondary)
                            .fixedSize(horizontal: false, vertical: true)
                    }
                }
            }
        }
        .contextMenu { AccountMenuItems(store: store, model: model, remove: remove) }
    }

    private var providerGlyph: some View {
        Button {
            store.activate(model.account.id)
        } label: {
            Image(model.account.provider.symbol)
        }
        .buttonStyle(.accessoryBar)
        .foregroundStyle(model.isActive ? AnyShapeStyle(Color.accentColor) : AnyShapeStyle(.secondary))
        .disabled(!model.isActive && !model.canActivate)
        .overlay(alignment: .center) {
            if model.running?.kind == .activate { ProgressView().controlSize(.mini) }
        }
        .accessibilityLabel(model.account.provider.name)
        .accessibilityAddTraits(model.isActive ? .isSelected : [])
        .help(model.isActive ? "Active account" : "Switch account")
    }

    @ViewBuilder
    private var gauges: some View {
        if let usage = model.usage.usage {
            let isCurrent = model.usage.is(\.current)
            ForEach(usage.windows.enumerated(), id: \.offset) { offset, window in
                UsageGauge(window: window, isCurrent: isCurrent, now: now, leadsGlyphRow: offset == 0)
            }
        }
    }
}

private struct MenuBarExtraToolTips: NSViewRepresentable {
    final class View: NSView {
        override func viewWillMove(toWindow newWindow: NSWindow?) {
            super.viewWillMove(toWindow: newWindow)
            newWindow?.allowsToolTipsWhenApplicationIsInactive = true
        }
    }

    func makeNSView(context _: Context) -> View { View() }

    func updateNSView(_: View, context _: Context) {}
}
