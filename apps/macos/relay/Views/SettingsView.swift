import AppKit
import ServiceManagement
import SwiftUI

// --- [VIEWS] ---------------------------------------------------------------------------

struct SettingsView: View {
    let store: AccountStore

    @AppStorage("settings.selection") private var savedSelection: SidebarItem = .general
    @State private var accountToRemove: AccountModel?

    var body: some View {
        NavigationSplitView {
            List(selection: Binding(get: { selection }, set: { savedSelection = $0 })) {
                Label("General", systemImage: "gearshape")
                    .tag(SidebarItem.general)

                Section("Accounts") {
                    ForEach(store.accounts, id: \.account.id) { model in
                        Label {
                            Text(model.account.identity.email)
                                .frame(maxWidth: .infinity, alignment: .leading)
                            if model.isSelected {
                                Image(systemName: "checkmark")
                                    .foregroundStyle(.secondary)
                                    .accessibilityLabel("Active")
                                    .help("Active account")
                            }
                        } icon: {
                            Image(model.account.provider.symbol)
                        }
                        .accessibilityElement(children: .combine)
                        .tag(SidebarItem.account(model.account.id))
                        .contextMenu { AccountMenuItems(store: store, model: model) { accountToRemove = model } }
                    }
                    .onMove(perform: store.moveAccounts(from:to:))
                }
            }
            .listStyle(.sidebar)
            .safeAreaInset(edge: .bottom) {
                ControlGroup {
                    Menu {
                        ForEach(Provider.allCases, id: \.self) { provider in
                            Button {
                                store.addAccount(provider)
                            } label: {
                                Label(provider.name, image: provider.symbol)
                            }
                        }
                    } label: {
                        Label("Add Account", systemImage: "plus")
                    }
                    .menuIndicator(.hidden)
                    .disabled(!store.canAddAccount)
                    Button {
                        accountToRemove = selectedAccount
                    } label: {
                        Label(AccountRemovalAlert.title, systemImage: "minus")
                    }
                    .disabled(selectedAccount?.isBusy ?? true)
                }
                .labelStyle(.iconOnly)
                .fixedSize()
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(8)
            }
            .navigationSplitViewColumnWidth(min: 220, ideal: 240, max: 280)
            .toolbar(removing: .sidebarToggle)
        } detail: {
            if let selectedAccount {
                AccountSettings(store: store, model: selectedAccount)
            } else {
                GeneralSettings(store: store)
            }
        }
        .frame(minWidth: 600, minHeight: 380)
        .safeAreaInset(edge: .bottom) {
            TimelineView(.everyMinute) { context in
                if let issue: String = store.issue(at: context.date) {
                    Text(issue)
                        .font(.callout)
                        .foregroundStyle(.secondary)
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .padding(.horizontal, 16)
                        .padding(.vertical, 8)
                        .background(.bar)
                }
            }
        }
        .sheet(item: Binding(get: { store.authentication }, set: { _ in })) { pending in
            SignInSheet(store: store, provider: pending.provider)
        }
        .modifier(AccountRemovalAlert(store: store, model: $accountToRemove))
        .onChange(of: store.authentication?.id) { previous, current in
            if current == nil, let previous, store.accounts.contains(where: { model in model.account.id == previous }) {
                savedSelection = .account(previous)
            }
        }
    }

    private var selectedAccount: AccountModel? {
        store.accounts.first { model in .account(model.account.id) == savedSelection }
    }

    private var selection: SidebarItem {
        selectedAccount.map { model in .account(model.account.id) } ?? .general
    }

    private enum SidebarItem: RawRepresentable, Hashable {
        case general
        case account(UUID)

        init?(rawValue: String) {
            switch (rawValue, UUID(uuidString: rawValue)) {
                case ("general", _): self = .general
                case (_, .some(let id)): self = .account(id)
                case (_, .none): return nil
            }
        }

        var rawValue: String {
            switch self {
                case .general: "general"
                case .account(let id): id.uuidString
            }
        }
    }
}

private struct AccountSettings: View {
    let store: AccountStore
    let model: AccountModel

    var body: some View {
        Form {
            Section {
                LabeledContent("Active account") {
                    if model.isSelected {
                        Label("Active", systemImage: "checkmark")
                            .foregroundStyle(.secondary)
                    } else {
                        let isSelecting: Bool = model.running?.kind == .selecting
                        Button {
                            store.select(model.account.id)
                        } label: {
                            Text("Use Account").opacity(isSelecting ? 0 : 1)
                        }
                        .disabled(!model.canSelect)
                        .overlay {
                            if isSelecting { ProgressView().controlSize(.small) }
                        }
                    }
                }
            } footer: {
                if model.account.provider == .openAI, let precondition: CodexFailure = store.codexPrecondition {
                    Text(precondition.localizedDescription)
                }
            }

            Section {
                Picker(
                    "Session start",
                    selection: Binding(
                        get: { model.account.sessionPolicy },
                        set: { store.setSessionPolicy($0, for: model.account.id) },
                    ),
                ) {
                    Text("Manual").tag(SessionPolicy.manual)
                    Text("Automatic").tag(SessionPolicy.automatic)
                }
            } footer: {
                Text(
                    "Manual starts a session from the card, Automatic sends “hi” when a new session can start"
                )
            }

            Section {
                HStack {
                    switch model.authentication {
                        case .connected: Button("Sign Out") { store.signOut(model.account.id) }
                        case .signInRequired: Button("Sign In") { store.signIn(model.account.id) }
                    }
                    Spacer()
                    if let operation: AccountOperation = model.running?.kind {
                        ProgressView().controlSize(.small)
                        Text(operation.description)
                            .font(.caption)
                            .foregroundStyle(.secondary)
                    }
                }
                .disabled(model.isBusy)
            } footer: {
                if let issue: ProviderError = model.issue {
                    TimelineView(.everyMinute) { context in Text(UsagePresentation.issue(issue, at: context.date)) }
                }
                if let blocking: AccountOperation = model.blockingOperation { Text(blocking.refusalDescription) }
            }
        }
        .formStyle(.grouped)
        .navigationTitle(model.account.identity.email)
        .navigationSubtitle(model.account.provider.name)
    }
}

private struct GeneralSettings: View {
    let store: AccountStore

    var body: some View {
        Form {
            Section {
                Toggle(
                    "Open at login",
                    isOn: Binding(get: { store.loginItem.isEnabled }, set: store.setLoginItemEnabled),
                )
                if store.loginItem.requiresApproval {
                    Button("Open Login Items") { SMAppService.openSystemSettingsLoginItems() }
                }
            } footer: {
                if let issue: String = store.loginItem.issue { Text(issue) }
            }
        }
        .formStyle(.grouped)
        .navigationTitle("General")
        .task {
            store.refreshLoginItem()
            for await _ in NotificationCenter.default.notifications(
                named: NSApplication.didBecomeActiveNotification
            ) {
                store.refreshLoginItem()
            }
        }
    }
}

private struct SignInSheet: View {
    let store: AccountStore
    let provider: Provider

    @State private var code: String = ""
    @FocusState private var isCodeFocused: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: 16) {
            Label("Sign In to \(provider.name)", image: provider.symbol)
                .font(.title3.weight(.medium))
            if let pending: AuthenticationPresentation = store.authentication {
                switch pending.phase {
                    case .refused(let refusal):
                        TimelineView(.everyMinute) { context in
                            Text(UsagePresentation.refusal(refusal, at: context.date))
                                .font(.callout)
                        }
                        Button("Done") { store.dismissAuthentication() }
                            .keyboardShortcut(.defaultAction)
                            .frame(maxWidth: .infinity, alignment: .trailing)
                    case .pending, .completing, .cancelling:
                        let accepting: Bool = if case .pending = pending.phase { true } else { false }
                        let cancelling: Bool = if case .cancelling = pending.phase { true } else { false }
                        HStack(spacing: 8) {
                            ProgressView().controlSize(.small)
                            Text(pending.phase.description)
                                .font(.callout)
                            Spacer()
                            Text(.durationOffset(to: pending.startedAt), format: .time(pattern: .minuteSecond))
                                .font(.caption)
                                .monospacedDigit()
                                .foregroundStyle(.secondary)
                        }
                        if provider == .claude {
                            TextField("Code", text: $code, prompt: Text("Paste code if prompted"))
                                .focused($isCodeFocused)
                                .onSubmit { store.submitAuthenticationCode(code) }
                                .disabled(!accepting)
                        }
                        HStack {
                            Spacer()
                            Button("Cancel", role: .cancel) { store.cancelAuthentication() }
                                .keyboardShortcut(.cancelAction)
                                .disabled(cancelling)
                            if provider == .claude {
                                Button("Continue") { store.submitAuthenticationCode(code) }
                                    .keyboardShortcut(.defaultAction)
                                    .disabled(
                                        !accepting || code.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
                                    )
                            }
                        }
                }
            }
        }
        .padding(24)
        .frame(width: 360)
        .interactiveDismissDisabled()
        .defaultFocus($isCodeFocused, true)
    }
}
