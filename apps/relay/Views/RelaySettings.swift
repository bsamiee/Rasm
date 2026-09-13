import AppKit
import SwiftUI

struct RelaySettings: View {
  let store: RelayStore

  @AppStorage("settings.selection") private var savedSelection: SettingsSelection = .general
  @State private var removing: AccountPresentation?

  var body: some View {
    NavigationSplitView {
      List(selection: Binding(get: { selection }, set: { savedSelection = $0 })) {
        Label("General", systemImage: "gearshape")
          .tag(SettingsSelection.general)

        Section("Accounts") {
          ForEach(store.accounts) { presentation in
            Label {
              Text(presentation.account.identity.email)
                .frame(maxWidth: .infinity, alignment: .leading)
              if presentation.isSelected {
                Image(systemName: "checkmark")
                  .foregroundStyle(.secondary)
                  .accessibilityLabel("Working account")
              }
            } icon: {
              Image(presentation.account.provider.name)
            }
            .accessibilityElement(children: .combine)
            .tag(SettingsSelection.account(presentation.id))
            .contextMenu {
              Button("Move Up") { store.moveAccount(presentation.id, by: -1) }
                .disabled(store.accounts.first?.id == presentation.id)
              Button("Move Down") { store.moveAccount(presentation.id, by: 1) }
                .disabled(store.accounts.last?.id == presentation.id)
              Divider()
              Button("Remove Account…", role: .destructive) { removing = presentation }
                .disabled(presentation.isBusy)
            }
          }
          .onMove { offsets, destination in store.moveAccounts(from: offsets, to: destination) }
        }
      }
      .listStyle(.sidebar)
      .safeAreaInset(edge: .bottom) {
        ControlGroup {
          Menu {
            ForEach(RelayProvider.allCases, id: \.self) { provider in
              Button {
                store.addAccount(provider)
              } label: {
                Label(provider.name, image: provider.name)
              }
            }
          } label: {
            Label("Add Account", systemImage: "plus")
          }
          .menuIndicator(.hidden)
          .disabled(!store.canAddAccount)
          Button {
            removing = selectedAccount
          } label: {
            Label("Remove Account", systemImage: "minus")
          }
          .disabled(selectedAccount?.isBusy ?? true)
        }
        .labelStyle(.iconOnly)
        .fixedSize()
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(8)
      }
      .navigationSplitViewColumnWidth(min: 220, ideal: 240, max: 280)
    } detail: {
      if let selectedAccount {
        RelayAccountSettings(store: store, presentation: selectedAccount)
      } else {
        RelayGeneralSettings(store: store)
      }
    }
    .frame(minWidth: 600, minHeight: 380)
    .safeAreaInset(edge: .bottom) {
      if let issue: String = store.issue {
        Text(issue)
          .font(.callout)
          .foregroundStyle(.secondary)
          .frame(maxWidth: .infinity, alignment: .leading)
          .padding(.horizontal, 16)
          .padding(.vertical, 8)
          .background(.bar)
      }
    }
    .sheet(
      item: Binding(
        get: { store.authentication },
        set: { pending in if pending == nil { store.cancelAuthentication() } })
    ) { pending in
      RelayAuthenticationSheet(store: store, provider: pending.provider)
    }
    .alert(
      "Remove Account?",
      isPresented: Binding(get: { removing != nil }, set: { if !$0 { removing = nil } }),
      presenting: removing
    ) { presentation in
      Button("Cancel", role: .cancel) {}
        .keyboardShortcut(.defaultAction)
      Button("Remove Account", role: .destructive) { store.remove(presentation.id) }
    } message: { presentation in
      Text(removalMessage(presentation))
    }
    .onChange(of: store.authentication?.id) { previous, current in
      if current == nil, let previous, store.accounts.contains(where: { $0.id == previous }) {
        savedSelection = .account(previous)
      }
    }
  }

  private var selectedAccount: AccountPresentation? {
    store.accounts.first { .account($0.id) == savedSelection }
  }

  private var selection: SettingsSelection {
    selectedAccount.map { .account($0.id) } ?? .general
  }

  private func removalMessage(_ presentation: AccountPresentation) -> String {
    let removal: String =
      "This removes \(presentation.account.identity.email) and its saved sign-in from Relay."
    return presentation.isSelected && presentation.account.provider == .claude
      ? "\(removal) The current Claude Code account will be signed out."
      : removal
  }

  private enum SettingsSelection: RawRepresentable, Hashable {
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

private struct RelayAccountSettings: View {
  let store: RelayStore
  let presentation: AccountPresentation

  var body: some View {
    Form {
      Section {
        LabeledContent("Working account") {
          if presentation.operation == .selecting {
            ProgressView().controlSize(.small)
          } else if presentation.isSelected {
            Label("Current", systemImage: "checkmark")
              .foregroundStyle(.secondary)
          } else {
            Button("Use Account") { store.select(presentation.id) }
              .disabled(presentation.authentication != .connected || presentation.isBusy)
          }
        }
      }

      Section {
        Picker(
          "Session start",
          selection: Binding(
            get: { presentation.account.sessionPolicy },
            set: { store.setSessionPolicy($0, for: presentation.id) })
        ) {
          Text("Manual").tag(SessionPolicy.manual)
          Text("Automatic").tag(SessionPolicy.automatic)
        }
        .disabled(presentation.operation == .removing)
      } footer: {
        Text(
          "Manual starts a session when you click the provider symbol. Automatic sends “hi” when the provider reports the session idle."
        )
      }

      Section {
        HStack {
          switch presentation.authentication {
          case .connected: Button("Sign Out") { store.signOut(presentation.id) }
          case .signInRequired: Button("Sign In…") { store.signIn(presentation.id) }
          }
          Spacer()
          if presentation.operation == .signingOut || presentation.operation == .removing {
            ProgressView().controlSize(.small)
          }
        }
        .disabled(presentation.isBusy)
      } footer: {
        if let issue: String = presentation.issue { Text(issue) }
      }
    }
    .formStyle(.grouped)
    .navigationTitle(presentation.account.identity.email)
    .navigationSubtitle(presentation.account.provider.name)
  }
}

private struct RelayGeneralSettings: View {
  let store: RelayStore

  var body: some View {
    Form {
      Section {
        Toggle(
          "Launch at login",
          isOn: Binding(
            get: { store.launchAtLogin.isEnabled }, set: { store.setLaunchAtLogin($0) })
        )
        .disabled(store.launchAtLogin.registration == .unavailable)
        if store.launchAtLogin.registration == .requiresApproval {
          Button("Open Login Items…") { store.openLoginItems() }
        }
      } footer: {
        if let note: String = launchAtLoginNote { Text(note) }
      }
    }
    .formStyle(.grouped)
    .navigationTitle("General")
  }

  private var launchAtLoginNote: String? {
    store.launchAtLogin.issue
      ?? (store.launchAtLogin.registration == .unavailable ? "Launch at login is unavailable" : nil)
  }
}

private struct RelayAuthenticationSheet: View {
  let store: RelayStore
  let provider: RelayProvider

  @State private var isCancelling: Bool = false

  var body: some View {
    VStack(alignment: .leading, spacing: 16) {
      Label("Sign In to \(provider.name)", image: provider.name)
        .font(.title3.weight(.medium))

      if let issue: String = store.authentication?.issue, !isCancelling {
        Text(issue)
          .font(.callout)
      } else {
        ProgressView().controlSize(.small)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      }

      Button("Cancel", role: .cancel) {
        isCancelling = true
        store.cancelAuthentication()
      }
      .keyboardShortcut(.cancelAction)
      .disabled(isCancelling)
      .frame(maxWidth: .infinity, alignment: .trailing)
    }
    .padding(24)
    .frame(width: 360)
    .interactiveDismissDisabled()
  }
}

extension AccountPresentation {
  fileprivate var isBusy: Bool { operation != .idle && operation != .awaitingWindow }
}
