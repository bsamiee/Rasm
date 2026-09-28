import AppKit
import SwiftUI

// --- [VIEWS] ---------------------------------------------------------------------------

struct AccountRemovalAlert: ViewModifier {
    static let title: LocalizedStringKey = "Remove Account"

    let store: AccountStore
    @Binding var model: AccountModel?

    func body(content: Content) -> some View {
        content.alert(
            Self.title,
            isPresented: Binding(
                get: { model != nil },
                set: { if !$0 { model = nil } },
            ),
            presenting: model,
        ) { model in
            Button("Cancel", role: .cancel) {}
                .keyboardShortcut(.defaultAction)
            Button(Self.title, role: .destructive) { store.remove(model.account.id) }
        } message: { model in
            let email: String = model.account.identity.email
            let message: String =
                switch (model.isSelected, model.account.provider) {
                    case (true, .claude): "Removing \(email) deletes its credential and signs Claude Code out"
                    case (true, .openAI): "Removing \(email) deletes its credential and signs Codex out"
                    case (false, _): "Removing \(email) deletes its credential"
                }
            Text(message)
        }
    }
}

struct AccountMenuItems: View {
    let store: AccountStore
    let model: AccountModel
    let remove: () -> Void

    var body: some View {
        Button("Move Up") { store.moveAccount(model.account.id, by: -1) }
            .disabled(store.accounts.first?.account.id == model.account.id)
        Button("Move Down") { store.moveAccount(model.account.id, by: 1) }
            .disabled(store.accounts.last?.account.id == model.account.id)
        Divider()
        Button(AccountRemovalAlert.title, role: .destructive, action: remove)
            .disabled(model.isBusy)
    }
}

struct MenuBarExtraContent: View {
    let store: AccountStore

    @Environment(\.openWindow) private var openWindow: OpenWindowAction
    @Environment(\.dismiss) private var dismiss: DismissAction
    @State private var accountToRemove: AccountModel?

    var body: some View {
        VStack(spacing: 8) {
            ScrollView {
                TimelineView(.everyMinute) { context in accounts(at: context.date) }
            }
            .fixedSize(horizontal: false, vertical: true)
            .scrollBounceBehavior(.basedOnSize)

            HStack {
                Spacer()
                Menu {
                    Button("Settings", action: showSettings)
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
        .background(PanelToolTips())
        .onAppear { store.setMenuBarExtraVisible(true) }
        .onDisappear { store.setMenuBarExtraVisible(false) }
        .modifier(AccountRemovalAlert(store: store, model: $accountToRemove))
    }

    private func accounts(at now: Date) -> some View {
        VStack(spacing: 24) {
            if store.isLoading {
                ProgressView().controlSize(.small)
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 8)
            } else if store.accounts.isEmpty {
                Button("Add Account", action: showSettings)
                    .disabled(!store.canAddAccount)
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 8)
            } else {
                ForEach(store.accounts, id: \.account.id) { model in
                    AccountCard(model: model, store: store, now: now, remove: { accountToRemove = model })
                }
            }

            if let issue: String = store.issue(at: now) {
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
        Task(name: "Bring Relay to front") { await Activation.bringToFront() }
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
                    Text(UsagePresentation.signInExpiry(expiry))
                        .font(.caption)
                        .monospacedDigit()
                        .foregroundStyle(.secondary)
                        .help(UsagePresentation.signInExpiryTooltip(expiry))
                        .accessibilityLabel(UsagePresentation.signInExpiryTooltip(expiry))
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
            if model.running?.kind == .selecting { ProgressView().controlSize(.mini) }
        }
        .accessibilityLabel(model.account.provider.name)
        .accessibilityAddTraits(model.isSelected ? .isSelected : [])
        .help(model.isSelected ? "Active account" : "Switch account")
    }

    @ViewBuilder
    private var gauges: some View {
        let usage: AccountUsage? = model.usage.usage
        let isCurrent: Bool = model.usage.isCurrent
        let session: QuotaWindow? = usage?.session
        let hasSession: Bool = usage.map { current in current.session != nil } ?? true
        let isStarting: Bool = model.running?.kind == .starting
        let showsStart: Bool = isStarting || model.canStartSession(at: now)
        if hasSession {
            UsageGauge(
                title: "Session",
                reading: UsagePresentation.sessionReading(
                    session,
                    availability: model.availability(at: now),
                    isCurrent: isCurrent,
                    at: now,
                ),
                fraction: showsStart ? 0 : session?.used.fraction ?? 0,
                isCurrent: isCurrent,
                detail: UsagePresentation.resetTooltip(session?.resetsAt),
                leadsGlyphRow: true,
            ) {
                if showsStart {
                    sessionStart(isStarting: isStarting)
                        .transition(.opacity)
                }
            }
            .animation(.default, value: showsStart)
        }
        if let usage {
            let weekly: [QuotaWindow] = [usage.weekly].compactMap(\.self) + usage.models
            ForEach(Array(weekly.enumerated()), id: \.offset) { offset, window in
                UsageGauge(
                    title: window.kind.name,
                    reading: UsagePresentation.reading(window, at: now),
                    fraction: window.used.fraction,
                    isCurrent: isCurrent,
                    detail: UsagePresentation.resetTooltip(window.resetsAt),
                    leadsGlyphRow: !hasSession && offset == 0,
                ) {}
            }
        }
    }

    private func sessionStart(isStarting: Bool) -> some View {
        let label: String = isStarting ? "Cancel" : "Start session"
        return Button {
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
        .accessibilityLabel(label)
        .help(label)
    }

    private var note: String? {
        switch (model.issue, model.running?.kind, model.authentication) {
            case (.some(let issue), _, _): UsagePresentation.issue(issue, at: now)
            case (.none, .signingOut, _), (.none, .removing, _): model.running?.kind.description
            case (.none, _, .signInRequired): "Sign in required"
            case (.none, _, .connected):
                UsagePresentation.blockedLine(model.availability(at: now), at: now)
                    ?? UsagePresentation.signInLine(expiring: model.usage.usage?.signInExpiresAt, at: now)
        }
    }
}
