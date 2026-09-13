import CryptoKit
import Foundation
import Security

struct ClaudeGrant: Sendable {
  let value: JSONValue
  let accessToken: String
  let refreshToken: String?
  let expiresAt: Date?
  let scopes: [String]
  let clientID: String?
  let plan: String?

  var fingerprint: String {
    SHA256.hash(data: Data(accessToken.utf8)).map { byte in String(format: "%02x", byte) }.joined()
  }

  func needsRefresh(at now: Date) -> Bool {
    switch expiresAt {
    case .some(let expiry): expiry <= now.addingTimeInterval(300)
    case .none: true
    }
  }

  static func make(_ value: JSONValue) -> Result<ClaudeGrant, ClaudeFailure> {
    guard let token: String = value["accessToken"]?.stringValue, !token.isEmpty,
      let scopeValues: [JSONValue] = value["scopes"]?.arrayValue
    else {
      return .failure(.invalidCredentials)
    }
    var scopes: [String] = []
    for scope in scopeValues {
      guard let name: String = scope.stringValue, !name.isEmpty else {
        return .failure(.invalidCredentials)
      }
      scopes.append(name)
    }
    let expiry: Date?
    switch value["expiresAt"] {
    case .some(.number(let milliseconds)) where milliseconds.isFinite && milliseconds > 0:
      expiry = Date(timeIntervalSince1970: milliseconds / 1000)
    case .some(.null), .none:
      expiry = nil
    case .some:
      return .failure(.invalidCredentials)
    }
    return .success(
      ClaudeGrant(
        value: value,
        accessToken: token,
        refreshToken: value["refreshToken"]?.stringValue,
        expiresAt: expiry,
        scopes: scopes,
        clientID: value["clientId"]?.stringValue,
        plan: value["subscriptionType"]?.stringValue
      ))
  }
}

struct ClaudeCredentialSnapshot: Sendable {
  let grant: ClaudeGrant
  let account: JSONValue
  let identity: AccountIdentity
}

private enum ClaudeCredentialStorage: Sendable {
  case item([String: JSONValue])
  case file([String: JSONValue])

  var object: [String: JSONValue] {
    switch self {
    case .item(let value), .file(let value): value
    }
  }
}

struct ClaudeNativeStore: Sendable {
  private static let queue: DispatchQueue = DispatchQueue(label: "app.relay.claude.credentials")

  let directory: URL
  let configuration: URL
  let service: String
  let username: String

  init(directory: URL, configuration: URL, defaultNamespace: Bool, username: String) {
    self.directory = directory
    self.configuration = configuration
    let canonicalPath: String = directory.path.precomposedStringWithCanonicalMapping
    let suffix: String =
      if defaultNamespace {
        ""
      } else {
        "-"
          + SHA256.hash(data: Data(canonicalPath.utf8))
          .prefix(4).map { byte in String(format: "%02x", byte) }.joined()
      }
    service = "Claude Code-credentials\(suffix)"
    self.username = username
  }

  func read() async -> Result<ClaudeCredentialSnapshot?, ClaudeFailure> {
    await withCheckedContinuation { continuation in
      Self.queue.async {
        continuation.resume(returning: readSynchronously())
      }
    }
  }

  func park(_ snapshot: ClaudeCredentialSnapshot) async -> Result<Void, ClaudeFailure> {
    let storage: ClaudeCredentialStorage
    switch await storedCredentials() {
    case .success(let value): storage = value
    case .failure(let error): return .failure(error)
    }
    if case .failure(let error) = await ensureCredentialItem(storage) { return .failure(error) }
    return await withCheckedContinuation { continuation in
      Self.queue.async {
        continuation.resume(returning: parkSynchronously(snapshot, credentials: storage.object))
      }
    }
  }

  func eraseSavedSignIn(matching expected: JSONValue) async -> Result<Void, ClaudeFailure> {
    let storage: ClaudeCredentialStorage
    switch await storedCredentials() {
    case .success(let value): storage = value
    case .failure(let error): return .failure(error)
    }
    guard storage.object["claudeAiOauth"] == expected else { return .failure(.accountChanged) }
    if case .failure(let error) = await ensureCredentialItem(storage) { return .failure(error) }
    return await withCheckedContinuation { continuation in
      Self.queue.async {
        var replacement: [String: JSONValue] = storage.object
        replacement.removeValue(forKey: "claudeAiOauth")
        continuation.resume(returning: writeCredentialObject(replacement))
      }
    }
  }

  func deletePrivateCredentials() async -> Result<Void, ClaudeFailure> {
    await withCheckedContinuation { continuation in
      Self.queue.async {
        let status: OSStatus = SecItemDelete(query as CFDictionary)
        guard status == errSecSuccess || status == errSecItemNotFound else {
          continuation.resume(returning: .failure(.keychain(status)))
          return
        }
        do {
          if FileManager.default.fileExists(atPath: credentialFile.path) {
            try FileManager.default.removeItem(at: credentialFile)
          }
          continuation.resume(returning: .success(()))
        } catch {
          continuation.resume(returning: .failure(.filesystem(error)))
        }
      }
    }
  }

  private func storedCredentials() async -> Result<ClaudeCredentialStorage, ClaudeFailure> {
    await withCheckedContinuation { continuation in
      Self.queue.async { continuation.resume(returning: readCredentialStorage()) }
    }
  }

  private var credentialFile: URL { directory.appending(path: ".credentials.json") }

  private var query: [String: Any] {
    [
      kSecClass as String: kSecClassGenericPassword,
      kSecAttrService as String: service,
      kSecAttrAccount as String: username,
    ]
  }

  private func readSynchronously() -> Result<ClaudeCredentialSnapshot?, ClaudeFailure> {
    switch readCredentialStorage() {
    case .failure(let error): return .failure(error)
    case .success(let storage):
      guard let value: JSONValue = storage.object["claudeAiOauth"], value != .null else {
        return .success(nil)
      }
      switch ClaudeGrant.make(value) {
      case .failure(let error): return .failure(error)
      case .success(let grant):
        return readObject(at: configuration, missingIsEmpty: false).flatMap { configuration in
          guard let account: JSONValue = configuration["oauthAccount"],
            let accountID: String = account["accountUuid"]?.stringValue,
            let email: String = account["emailAddress"]?.stringValue
          else {
            return .failure(.invalidCredentials)
          }
          return AccountIdentity.make(
            accountID: accountID,
            organizationID: account["organizationUuid"]?.stringValue,
            email: email,
            plan: grant.plan
          ).mapError(ClaudeFailure.invalidIdentity).map { identity in
            ClaudeCredentialSnapshot(grant: grant, account: account, identity: identity)
          }
        }
      }
    }
  }

  private func readCredentialStorage() -> Result<ClaudeCredentialStorage, ClaudeFailure> {
    var request: [String: Any] = query
    request[kSecReturnData as String] = true
    request[kSecMatchLimit as String] = kSecMatchLimitOne
    var item: CFTypeRef?
    let status: OSStatus = SecItemCopyMatching(request as CFDictionary, &item)
    switch status {
    case errSecSuccess:
      guard let data: Data = item as? Data else { return .failure(.invalidCredentials) }
      return decodeObject(data).map(ClaudeCredentialStorage.item)
    case errSecItemNotFound:
      return readObject(at: credentialFile, missingIsEmpty: true).map(ClaudeCredentialStorage.file)
    default:
      return .failure(.keychain(status))
    }
  }

  private func parkSynchronously(
    _ snapshot: ClaudeCredentialSnapshot, credentials: [String: JSONValue]
  ) -> Result<Void, ClaudeFailure> {
    let configurationResult: Result<[String: JSONValue], ClaudeFailure> = readObject(
      at: configuration, missingIsEmpty: true
    )
    switch configurationResult {
    case .failure(let error):
      return .failure(error)
    case .success(let configurationValues):
      var updatedConfiguration: [String: JSONValue] = configurationValues
      updatedConfiguration["oauthAccount"] = snapshot.account
      updatedConfiguration["hasCompletedOnboarding"] = .bool(true)
      var updatedCredentials: [String: JSONValue] = credentials
      updatedCredentials["claudeAiOauth"] = snapshot.grant.value
      return writeObject(updatedConfiguration, to: configuration).flatMap { _ in
        writeCredentialObject(updatedCredentials)
      }
    }
  }

  private func writeCredentialObject(_ object: [String: JSONValue]) -> Result<Void, ClaudeFailure> {
    let data: Data
    do {
      data = try JSONEncoder().encode(JSONValue.object(object))
    } catch {
      return .failure(.filesystem(error))
    }
    let status: OSStatus = SecItemUpdate(
      query as CFDictionary, [kSecValueData as String: data] as CFDictionary
    )
    switch status {
    case errSecSuccess:
      do {
        if FileManager.default.fileExists(atPath: credentialFile.path) {
          try FileManager.default.removeItem(at: credentialFile)
        }
        return .success(())
      } catch {
        return .failure(.filesystem(error))
      }
    default:
      return .failure(.keychain(status))
    }
  }

  // Claude Code reads its item through /usr/bin/security, so the item's trusted
  // application list names that tool beside Relay. SecItemAdd trusts the creating
  // app alone, and the API that names other apps is deprecated.
  private func ensureCredentialItem(_ storage: ClaudeCredentialStorage) async -> Result<
    Void, ClaudeFailure
  > {
    if case .item = storage { return .success(()) }
    guard let relay: URL = Bundle.main.executableURL else { return .failure(.invalidCredentials) }
    do {
      try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
    } catch {
      return .failure(.filesystem(error))
    }
    let invocation: ProcessInvocation = ProcessInvocation(
      executable: URL(fileURLWithPath: "/usr/bin/security"),
      arguments: [
        "add-generic-password", "-a", username, "-s", service, "-w", "{}",
        "-T", "/usr/bin/security", "-T", relay.path,
      ],
      environment: [:], workingDirectory: directory
    )
    let process: NativeProcess
    switch await NativeProcess.launch(invocation) {
    case .success(let value): process = value
    case .failure(let error): return .failure(.native(error))
    }
    return await withTaskCancellationHandler {
      await process.closeInput()
      return await process.waitForExit().mapError(ClaudeFailure.native).flatMap { output in
        output.exitCode == 0 ? .success(()) : .failure(.native(.exit(output.exitCode)))
      }
    } onCancel: {
      Task { await process.cancel() }
    }
  }

  private func readObject(at url: URL, missingIsEmpty: Bool) -> Result<
    [String: JSONValue], ClaudeFailure
  > {
    do {
      return decodeObject(try Data(contentsOf: url))
    } catch let error as CocoaError where error.code == .fileReadNoSuchFile && missingIsEmpty {
      return .success([:])
    } catch {
      return .failure(.filesystem(error))
    }
  }

  private func decodeObject(_ data: Data) -> Result<[String: JSONValue], ClaudeFailure> {
    do {
      let value: JSONValue = try JSONDecoder().decode(JSONValue.self, from: data)
      guard let object: [String: JSONValue] = value.objectValue else {
        return .failure(.invalidCredentials)
      }
      return .success(object)
    } catch {
      return .failure(.invalidCredentials)
    }
  }

  private func writeObject(_ object: [String: JSONValue], to url: URL) -> Result<
    Void, ClaudeFailure
  > {
    do {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      let data: Data = try JSONEncoder().encode(JSONValue.object(object))
      try data.write(to: url, options: .atomic)
      return .success(())
    } catch {
      return .failure(.filesystem(error))
    }
  }
}
