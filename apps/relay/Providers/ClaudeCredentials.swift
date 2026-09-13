import CryptoKit
import Foundation
import Security

nonisolated struct ClaudeOAuthToken: Sendable {
  let value: JSONValue
  let accessToken: String
  let refreshToken: String?
  let expiresAt: Date?
  let scopes: [String]
  let clientID: String?
  let plan: String?

  var fingerprint: SHA256Digest { SHA256.hash(data: Data(accessToken.utf8)) }

  func needsRefresh(at now: Date) -> Bool {
    expiresAt.map { expiry in expiry <= now.addingTimeInterval(300) } ?? true
  }

  static func make(_ value: JSONValue) -> Result<ClaudeOAuthToken, ClaudeFailure> {
    guard let token: String = value["accessToken"]?.stringValue, !token.isEmpty,
      let scopeValues: [JSONValue] = value["scopes"]?.arrayValue
    else {
      return .failure(.invalidCredentials)
    }
    let scopes: [String] = scopeValues.compactMap { scope in
      scope.stringValue.flatMap { name in name.isEmpty ? nil : name }
    }
    guard scopes.count == scopeValues.count else { return .failure(.invalidCredentials) }
    let expiry: Result<Date?, ClaudeFailure> =
      switch value["expiresAt"] {
      case .some(.number(let milliseconds)) where milliseconds.isFinite && milliseconds > 0:
        .success(Date(timeIntervalSince1970: milliseconds / 1000))
      case .some(.null), .none: .success(nil)
      case .some: .failure(.invalidCredentials)
      }
    return expiry.map { expiry in
      ClaudeOAuthToken(
        value: value,
        accessToken: token,
        refreshToken: value["refreshToken"]?.stringValue,
        expiresAt: expiry,
        scopes: scopes,
        clientID: value["clientId"]?.stringValue,
        plan: value["subscriptionType"]?.stringValue
      )
    }
  }
}

nonisolated struct ClaudeCredentials: Sendable {
  let token: ClaudeOAuthToken
  let account: JSONValue
  let identity: AccountIdentity
}

private nonisolated enum ClaudeCredentialStorage: Sendable {
  case keychainItem([String: JSONValue])
  case file([String: JSONValue])

  var object: [String: JSONValue] {
    switch self {
    case .keychainItem(let value), .file(let value): value
    }
  }
}

nonisolated struct ClaudeCredentialStore: Sendable {
  private static let queue: DispatchQueue = DispatchQueue(label: "app.relay.claude.credentials")

  let directory: URL
  let configFile: URL
  let legacyConfigFile: URL
  let configDirectoryPath: String?
  let service: String
  let username: String

  init(
    directory: URL, configFile: URL, legacyConfigFile: URL, configDirectoryPath: String?,
    username: String
  ) {
    self.directory = directory
    self.configFile = configFile
    self.legacyConfigFile = legacyConfigFile
    self.configDirectoryPath = configDirectoryPath
    service =
      configDirectoryPath.map { value in
        "Claude Code-credentials-"
          + SHA256.hash(data: Data(value.utf8))
          .prefix(4).map { byte in String(format: "%02x", byte) }.joined()
      } ?? "Claude Code-credentials"
    self.username = username
  }

  func read() async -> Result<ClaudeCredentials?, ClaudeFailure> {
    await onQueue { readStorage().flatMap { storage in credentials(in: storage) } }
  }

  func write(_ credentials: ClaudeCredentials) async -> Result<Void, ClaudeFailure> {
    await credentialStorage().bind(createKeychainItemIfMissing).bind { storage in
      await onQueue {
        let file: URL = existingConfigFile
        var stored: [String: JSONValue] = storage.object
        stored["claudeAiOauth"] = credentials.token.value
        return readJSONObject(at: file, missingIsEmpty: true)
          .flatMap { configuration -> Result<Void, ClaudeFailure> in
            var updated: [String: JSONValue] = configuration
            updated["oauthAccount"] = credentials.account
            updated["hasCompletedOnboarding"] = .bool(true)
            return writeJSONObject(updated, to: file)
          }
          .flatMap { _ in writeStorage(stored) }
      }
    }
  }

  func removeOAuthToken(matching expected: JSONValue) async -> Result<Void, ClaudeFailure> {
    await credentialStorage()
      .flatMap { storage in
        storage.object["claudeAiOauth"] == expected ? .success(storage) : .failure(.accountChanged)
      }
      .bind(createKeychainItemIfMissing)
      .bind { storage in
        await onQueue {
          writeStorage(storage.object.filter { entry in entry.key != "claudeAiOauth" })
        }
      }
  }

  func deleteCredentials() async -> Result<Void, ClaudeFailure> {
    await onQueue {
      let status: OSStatus = SecItemDelete(query as CFDictionary)
      return status == errSecSuccess || status == errSecItemNotFound
        ? removeCredentialFile() : .failure(.keychain(status))
    }
  }

  private func credentials(
    in storage: ClaudeCredentialStorage
  ) -> Result<ClaudeCredentials?, ClaudeFailure> {
    guard let value: JSONValue = storage.object["claudeAiOauth"], value != .null else {
      return .success(nil)
    }
    return ClaudeOAuthToken.make(value).flatMap { token in
      readJSONObject(at: existingConfigFile, missingIsEmpty: false).flatMap { configuration in
        credentials(in: configuration, token: token).map(Optional.some)
      }
    }
  }

  private func credentials(
    in configuration: [String: JSONValue], token: ClaudeOAuthToken
  ) -> Result<ClaudeCredentials, ClaudeFailure> {
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
      plan: token.plan
    ).mapError(ClaudeFailure.invalidIdentity).map { identity in
      ClaudeCredentials(token: token, account: account, identity: identity)
    }
  }

  private func credentialStorage() async -> Result<ClaudeCredentialStorage, ClaudeFailure> {
    await onQueue { readStorage() }
  }

  private func onQueue<Value: Sendable>(
    _ work: @escaping @Sendable () -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    await withCheckedContinuation { continuation in
      Self.queue.async { continuation.resume(returning: work()) }
    }
  }

  private var credentialFile: URL { directory.appending(path: ".credentials.json") }

  private var existingConfigFile: URL {
    FileManager.default.fileExists(atPath: legacyConfigFile.path) ? legacyConfigFile : configFile
  }

  private var query: [String: Any] {
    [
      kSecClass as String: kSecClassGenericPassword,
      kSecAttrService as String: service,
      kSecAttrAccount as String: username,
    ]
  }

  private func readStorage() -> Result<ClaudeCredentialStorage, ClaudeFailure> {
    var request: [String: Any] = query
    request[kSecReturnData as String] = true
    request[kSecMatchLimit as String] = kSecMatchLimitOne
    var item: CFTypeRef?
    let status: OSStatus = SecItemCopyMatching(request as CFDictionary, &item)
    switch status {
    case errSecSuccess:
      return (item as? Data).map { data in
        decodeJSONObject(data).map(ClaudeCredentialStorage.keychainItem)
      } ?? .failure(.invalidCredentials)
    case errSecItemNotFound:
      return readJSONObject(at: credentialFile, missingIsEmpty: true)
        .map(ClaudeCredentialStorage.file)
    default:
      return .failure(.keychain(status))
    }
  }

  private func writeStorage(_ object: [String: JSONValue]) -> Result<Void, ClaudeFailure> {
    Result { try JSONEncoder().encode(JSONValue.object(object)) }
      .mapError(ClaudeFailure.filesystem)
      .flatMap { data in
        let status: OSStatus = SecItemUpdate(
          query as CFDictionary, [kSecValueData as String: data] as CFDictionary
        )
        return status == errSecSuccess ? removeCredentialFile() : .failure(.keychain(status))
      }
  }

  private func removeCredentialFile() -> Result<Void, ClaudeFailure> {
    FileManager.default.fileExists(atPath: credentialFile.path)
      ? Result { try FileManager.default.removeItem(at: credentialFile) }
        .mapError(ClaudeFailure.filesystem)
      : .success(())
  }

  private func createKeychainItemIfMissing(
    _ storage: ClaudeCredentialStorage
  ) async -> Result<ClaudeCredentialStorage, ClaudeFailure> {
    if case .keychainItem = storage { return .success(storage) }
    guard let relay: URL = Bundle.main.executableURL else { return .failure(.invalidCredentials) }
    let invocation: ProcessInvocation = ProcessInvocation(
      executable: URL(fileURLWithPath: "/usr/bin/security"),
      arguments: [
        "add-generic-password", "-a", username, "-s", service, "-w", "{}",
        "-T", "/usr/bin/security", "-T", relay.path,
      ],
      environment: [:], workingDirectory: directory
    )
    return await Result {
      try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
    }
    .mapError(ClaudeFailure.filesystem)
    .bind { _ in await ChildProcess.launch(invocation).mapError(ClaudeFailure.process) }
    .bind { process in
      await withTaskCancellationHandler {
        await process.closeInput()
        return await process.waitUntilExit().exited().map { _ in storage }
      } onCancel: {
        Task { await process.cancel() }
      }
    }
  }

  private func readJSONObject(at url: URL, missingIsEmpty: Bool) -> Result<
    [String: JSONValue], ClaudeFailure
  > {
    Result { try Data(contentsOf: url) }.map(Optional.some)
      .flatMapError { error -> Result<Data?, ClaudeFailure> in
        missingIsEmpty && (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.filesystem(error))
      }
      .flatMap { data in data.map(decodeJSONObject) ?? .success([:]) }
  }

  private func decodeJSONObject(_ data: Data) -> Result<[String: JSONValue], ClaudeFailure> {
    Result { try JSONDecoder().decode(JSONValue.self, from: data) }
      .mapError { _ in ClaudeFailure.invalidCredentials }
      .flatMap { value -> Result<[String: JSONValue], ClaudeFailure> in
        value.objectValue.map(Result.success) ?? .failure(.invalidCredentials)
      }
  }

  private func writeJSONObject(_ object: [String: JSONValue], to url: URL) -> Result<
    Void, ClaudeFailure
  > {
    Result {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      try JSONEncoder().encode(JSONValue.object(object)).write(to: url, options: .atomic)
    }
    .mapError(ClaudeFailure.filesystem)
  }
}
