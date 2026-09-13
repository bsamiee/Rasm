import ServiceManagement
import SwiftUI

struct SettingsView: View {
  let store: AccountStore

  @AppStorage("settings.selection") private var savedSelection: SidebarItem = .general
  @State private var accountToRemove: AccountViewModel?

  var body: some View {
    NavigationSplitView {
      List(selection: Binding(get: { selection }, set: { savedSelection = $0 })) {
        Label("General", systemImage: "gearshape")
          .tag(SidebarItem.general)

        Section("Accounts") {
          ForEach(store.accounts) { viewModel in
            Label {
              Text(viewModel.account.identity.email)
                .frame(maxWidth: .infinity, alignment: .leading)
              if viewModel.isSelected {
                Image(systemName: "checkmark")
                  .foregroundStyle(.secondary)
                  .accessibilityLabel("Working account")
              }
            } icon: {
              Image(viewModel.account.provider.symbol)
            }
            .accessibilityElement(children: .combine)
            .tag(SidebarItem.account(viewModel.id))
            .contextMenu {
              Button("Move Up") { store.moveAccount(viewModel.id, by: -1) }
                .disabled(store.accounts.first?.id == viewModel.id)
              Button("Move Down") { store.moveAccount(viewModel.id, by: 1) }
                .disabled(store.accounts.last?.id == viewModel.id)
              Divider()
              Button("Remove Account…", role: .destructive) { accountToRemove = viewModel }
                .disabled(viewModel.isBusy)
            }
          }
          .onMove { offsets, destination in store.moveAccounts(from: offsets, to: destination) }
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
        AccountSettings(store: store, viewModel: selectedAccount)
      } else {
        GeneralSettings(store: store)
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
    .sheet(item: .constant(store.authentication)) { pending in
      SignInSheet(store: store, provider: pending.provider)
    }
    .alert(
      "Remove Account?",
      isPresented: Binding(
        get: { accountToRemove != nil }, set: { if !$0 { accountToRemove = nil } }),
      presenting: accountToRemove
    ) { viewModel in
      Button("Cancel", role: .cancel) {}
        .keyboardShortcut(.defaultAction)
      Button("Remove Account", role: .destructive) { store.remove(viewModel.id) }
    } message: { viewModel in
      Text(removalMessage(viewModel))
    }
    .onChange(of: store.authentication?.id) { previous, current in
      if current == nil, let previous, store.accounts.contains(where: { $0.id == previous }) {
        savedSelection = .account(previous)
      }
    }
  }

  private var selectedAccount: AccountViewModel? {
    store.accounts.first { .account($0.id) == savedSelection }
  }

  private var selection: SidebarItem {
    selectedAccount.map { .account($0.id) } ?? .general
  }

  private func removalMessage(_ viewModel: AccountViewModel) -> String {
    let removal: String =
      "This removes \(viewModel.account.identity.email) and its saved sign-in from Relay."
    return viewModel.isSelected && viewModel.account.provider == .claude
      ? "\(removal) The current Claude Code account will be signed out."
      : removal
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
  let viewModel: AccountViewModel

  var body: some View {
    Form {
      Section {
        LabeledContent("Working account") {
          if viewModel.operation == .selecting {
            ProgressView().controlSize(.small)
          } else if viewModel.isSelected {
            Label("Current", systemImage: "checkmark")
              .foregroundStyle(.secondary)
          } else {
            Button("Use Account") { store.select(viewModel.id) }
              .disabled(!viewModel.canSelect)
          }
        }
      }

      Section {
        Picker(
          "Session start",
          selection: Binding(
            get: { viewModel.account.sessionPolicy },
            set: { store.setSessionPolicy($0, for: viewModel.id) })
        ) {
          Text("Manual").tag(SessionPolicy.manual)
          Text("Automatic").tag(SessionPolicy.automatic)
        }
      } footer: {
        Text(
          "Manual starts a session when you click the provider symbol. Automatic sends “hi” when the provider reports the session idle."
        )
      }

      Section {
        HStack {
          switch viewModel.authentication {
          case .connected: Button("Sign Out") { store.signOut(viewModel.id) }
          case .signInRequired: Button("Sign In…") { store.signIn(viewModel.id) }
          }
          Spacer()
          if viewModel.operation == .signingOut || viewModel.operation == .removing {
            ProgressView().controlSize(.small)
          }
        }
        .disabled(viewModel.isBusy)
      } footer: {
        if let issue: String = viewModel.issue { Text(issue) }
      }
    }
    .formStyle(.grouped)
    .navigationTitle(viewModel.account.identity.email)
    .navigationSubtitle(viewModel.account.provider.name)
  }
}

private struct GeneralSettings: View {
  let store: AccountStore

  var body: some View {
    Form {
      Section {
        Toggle(
          "Launch at login",
          isOn: Binding(
            get: { store.loginItem.isEnabled }, set: { store.setLoginItemEnabled($0) })
        )
        .disabled(store.loginItem.status == .notFound)
        if store.loginItem.status == .requiresApproval {
          Button("Open Login Items…") { SMAppService.openSystemSettingsLoginItems() }
        }
      } footer: {
        if let issue: String = store.loginItem.issue { Text(issue) }
      }
    }
    .formStyle(.grouped)
    .navigationTitle("General")
  }
}

private struct SignInSheet: View {
  let store: AccountStore
  let provider: Provider

  var body: some View {
    VStack(alignment: .leading, spacing: 16) {
      Label("Sign In to \(provider.name)", image: provider.symbol)
        .font(.title3.weight(.medium))

      if case .refused(let issue) = store.authentication?.phase {
        Text(issue)
          .font(.callout)
      } else {
        ProgressView().controlSize(.small)
          .frame(maxWidth: .infinity)
          .padding(.vertical, 8)
      }

      Button("Cancel", role: .cancel) { store.cancelAuthentication() }
        .keyboardShortcut(.cancelAction)
        .disabled(store.authentication?.phase == .cancelling)
        .frame(maxWidth: .infinity, alignment: .trailing)
    }
    .padding(24)
    .frame(width: 360)
    .interactiveDismissDisabled()
  }
}
