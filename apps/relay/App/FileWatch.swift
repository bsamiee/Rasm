import CryptoKit
import Dispatch
import Foundation
import System

nonisolated enum FileWatch {
  static func changes(of file: URL) -> AsyncThrowingStream<SHA256Digest?, any Error> {
    marks(of: file, probe: contentDigest)
  }

  static func presence(of entry: URL) -> AsyncThrowingStream<Bool, any Error> {
    marks(of: entry, probe: exists)
  }

  private static func marks<Mark: Equatable & Sendable>(
    of file: URL, probe: @escaping @Sendable (URL) -> Mark
  ) -> AsyncThrowingStream<Mark, any Error> {
    AsyncThrowingStream { continuation in
      let watcher: Watcher<Mark> = Watcher(file: file, probe: probe, continuation: continuation)
      continuation.onTermination = { _ in watcher.stop() }
      watcher.start()
    }
  }

  private static func contentDigest(_ file: URL) -> SHA256Digest? {
    (try? Data(contentsOf: file)).map(SHA256.hash(data:))
  }

  private static func exists(_ entry: URL) -> Bool {
    FileManager.default.fileExists(atPath: entry.path)
  }

  private final class Watcher<Mark: Equatable & Sendable>: @unchecked Sendable {
    private let file: URL
    private let probe: @Sendable (URL) -> Mark
    private let continuation: AsyncThrowingStream<Mark, any Error>.Continuation
    private let queue: DispatchQueue = DispatchQueue(label: "app.rasm.relay.filewatch")
    private var directorySource: (any DispatchSourceFileSystemObject)?
    private var fileSource: (any DispatchSourceFileSystemObject)?
    private var pending: DispatchWorkItem?
    private var mark: Mark?
    private var stopped: Bool = false

    init(
      file: URL, probe: @escaping @Sendable (URL) -> Mark,
      continuation: AsyncThrowingStream<Mark, any Error>.Continuation
    ) {
      self.file = file
      self.probe = probe
      self.continuation = continuation
    }

    func start() {
      queue.async { [self] in
        mark = probe(file)
        let directory: Result<any DispatchSourceFileSystemObject, any Error> = source(
          path: file.deletingLastPathComponent().path, events: .write
        ) { [weak self] in
          self?.openFileSource()
          self?.scheduleEmit()
        }
        switch directory {
        case .success(let source):
          directorySource = source
          openFileSource()
        case .failure(let error): continuation.finish(throwing: error)
        }
      }
    }

    func stop() {
      queue.async {
        self.stopped = true
        self.pending?.cancel()
        self.directorySource?.cancel()
        self.fileSource?.cancel()
        self.directorySource = nil
        self.fileSource = nil
      }
    }

    private func openFileSource() {
      guard !stopped, fileSource == nil else { return }
      let opened: Result<any DispatchSourceFileSystemObject, any Error> = source(
        path: file.path, events: [.write, .extend, .delete, .rename, .attrib]
      ) { [weak self] in
        guard let self else { return }
        if let source: any DispatchSourceFileSystemObject = fileSource,
          !source.data.intersection([.delete, .rename]).isEmpty
        {
          source.cancel()
          fileSource = nil
        }
        scheduleEmit()
      }
      switch opened {
      case .success(let source): fileSource = source
      case .failure(let error as Errno) where error == .noSuchFileOrDirectory: return
      case .failure(let error): continuation.finish(throwing: error)
      }
    }

    private func source(
      path: String, events: DispatchSource.FileSystemEvent, handler: @escaping @Sendable () -> Void
    ) -> Result<any DispatchSourceFileSystemObject, any Error> {
      Result { try FileDescriptor.open(FilePath(path), .readOnly, options: .eventOnly) }.map {
        descriptor in
        let source: any DispatchSourceFileSystemObject = DispatchSource.makeFileSystemObjectSource(
          fileDescriptor: descriptor.rawValue, eventMask: events, queue: queue)
        source.setEventHandler(handler: handler)
        source.setCancelHandler { try? descriptor.close() }
        source.activate()
        return source
      }
    }

    private func scheduleEmit() {
      pending?.cancel()
      let work: DispatchWorkItem = DispatchWorkItem { [weak self] in self?.emitIfChanged() }
      pending = work
      queue.asyncAfter(deadline: .now() + .milliseconds(300), execute: work)
    }

    private func emitIfChanged() {
      guard !stopped else { return }
      let current: Mark = probe(file)
      guard current != mark else { return }
      mark = current
      continuation.yield(current)
    }
  }
}
