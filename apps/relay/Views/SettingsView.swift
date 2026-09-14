import ServiceManagement
import SwiftUI

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
          ForEach(store.accounts) { model in
            Label {
              Text(model.account.identity.email)
                .frame(maxWidth: .infinity, alignment: .leading)
              if model.isSelected {
                Image(systemName: "checkmark")
                  .foregroundStyle(.secondary)
                  .accessibilityLabel("In use")
              }
            } icon: {
              Image(model.account.provider.symbol)
            }
            .accessibilityElement(children: .combine)
            .tag(SidebarItem.account(model.id))
            .contextMenu {
              Button("Move Up") { store.moveAccount(model.id, by: -1) }
                .disabled(store.accounts.first?.id == model.id)
              Button("Move Down") { store.moveAccount(model.id, by: 1) }
                .disabled(store.accounts.last?.id == model.id)
              Divider()
              Button("Remove Account…", role: .destructive) { accountToRemove = model }
                .disabled(model.isBusy)
            }
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
        AccountSettings(store: store, model: selectedAccount)
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
    .sheet(item: Binding(get: { store.authentication }, set: { _ in })) { pending in
      SignInSheet(store: store, provider: pending.provider)
    }
    .alert(
      "Remove Account?",
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
    .onChange(of: store.authentication?.id) { previous, current in
      if current == nil, let previous, store.accounts.contains(where: { $0.id == previous }) {
        savedSelection = .account(previous)
      }
    }
  }

  private var selectedAccount: AccountModel? {
    store.accounts.first { .account($0.id) == savedSelection }
  }

  private var selection: SidebarItem {
    selectedAccount.map { .account($0.id) } ?? .general
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
        LabeledContent("Working account") {
          if model.operation == .selecting {
            ProgressView().controlSize(.small)
          } else if model.isSelected {
            Label("In use", systemImage: "checkmark")
              .foregroundStyle(.secondary)
          } else {
            Button("Use Account") { store.select(model.id) }
              .disabled(!model.canSelect)
          }
        }
      } footer: {
        if model.account.provider == .openAI, let note: String = store.codexPrecondition {
          Text(note)
        }
      }

      Section {
        Picker(
          "Session start",
          selection: Binding(
            get: { model.account.sessionPolicy },
            set: { store.setSessionPolicy($0, for: model.id) })
        ) {
          Text("Manual").tag(SessionPolicy.manual)
          Text("Automatic").tag(SessionPolicy.automatic)
        }
      } footer: {
        Text(
          "Manual starts a session when you click Start Session. Automatic sends “hi” when the provider reports the session ready."
        )
      }

      Section {
        HStack {
          switch model.authentication {
          case .connected: Button("Sign Out") { store.signOut(model.id) }
          case .signInRequired: Button("Sign In…") { store.signIn(model.id) }
          }
          Spacer()
          if let operation: AccountOperation = model.operation {
            ProgressView().controlSize(.small)
            Text(operation.description)
              .font(.caption)
              .foregroundStyle(.secondary)
          }
        }
        .disabled(model.isBusy)
      } footer: {
        if let issue: String = model.issue { Text(issue) }
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
          "Launch at login",
          isOn: Binding(get: { store.loginItem.isEnabled }, set: store.setLoginItemEnabled)
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
    .task {
      while case .success = await Result(catching: { try await Task.sleep(for: .seconds(1)) }) {
        store.refreshLoginItem()
      }
    }
  }
}

private struct SignInSheet: View {
  let store: AccountStore
  let provider: Provider

  var body: some View {
    VStack(alignment: .leading, spacing: 16) {
      Label("Sign In to \(provider.name)", image: provider.symbol)
        .font(.title3.weight(.medium))

      switch store.authentication?.phase {
      case .refused(let issue):
        Text(issue)
          .font(.callout)
      case .pending, .cancelling, .none:
        HStack(spacing: 8) {
          ProgressView().controlSize(.small)
          Text("Finish signing in to \(provider.name) in your browser.")
            .font(.callout)
          Spacer()
          if let started: Date = store.authentication?.startedAt {
            Text(.durationOffset(to: started), format: .time(pattern: .minuteSecond))
              .font(.caption)
              .monospacedDigit()
              .foregroundStyle(.secondary)
          }
        }
      }

      Button(store.authentication?.phase == .cancelling ? "Cancelling…" : "Cancel", role: .cancel) {
        store.cancelAuthentication()
      }
      .keyboardShortcut(.cancelAction)
      .frame(maxWidth: .infinity, alignment: .trailing)
    }
    .padding(24)
    .frame(width: 360)
    .interactiveDismissDisabled()
  }
}
