import db.Transaction;

import ghidra.app.script.GhidraScript;
import ghidra.app.script.GhidraState;
import ghidra.framework.model.DomainFile;
import ghidra.framework.model.ProjectLocator;
import ghidra.program.model.listing.Program;
import ghidra.util.exception.CancelledException;
import ghidra.util.exception.VersionException;
import ghidra.util.task.CancelledListener;

import headless.Report;

import org.apache.commons.io.input.BoundedInputStream;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.OutputStream;
import java.io.UncheckedIOException;
import java.net.StandardProtocolFamily;
import java.net.UnixDomainSocketAddress;
import java.nio.channels.Channels;
import java.nio.channels.ServerSocketChannel;
import java.nio.channels.SocketChannel;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CompletionException;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;
import java.util.function.BiFunction;
import java.util.function.Function;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Serve extends GhidraScript {

    // --- [REQUESTS]

    private static final Pattern CELL = Pattern.compile("\t");
    private static final int REQUEST_BYTES = 64 * 1024;
    private static final int CONNECTIONS = 32;
    private static final long TRANSFER_SECONDS = 5;

    private final Map<String, Program> programs = new ConcurrentHashMap<>();

    private record Connection(SocketChannel channel, String line) {}

    record Request(String program, String script, List<String> arguments) {
        static Report.Result<Request> of(String line) {
            List<String> cells = CELL.splitAsStream(line).filter(cell -> !cell.isEmpty()).toList();
            return cells.size() < 2
                    ? new Report.Failure<>(
                            "Request `%s` matches no form <program> <script> <argument>..."
                                    .formatted(line))
                    : new Report.Success<>(
                            new Request(
                                    cells.get(0), cells.get(1), cells.subList(2, cells.size())));
        }
    }

    private String answer(String line) {
        long start = System.nanoTime();
        Function<String, Report.Result<Program>> open =
                name -> {
                    String path = name.startsWith("/") ? name : "/" + name;
                    Program cached = programs.get(path);
                    if (cached != null) {
                        return new Report.Success<>(cached);
                    }
                    DomainFile file = state.getProject().getProjectData().getFile(path);
                    if (file == null) {
                        return new Report.Failure<>(
                                "Project holds no program `%s`".formatted(path));
                    }
                    try {
                        Program opened = (Program) file.getDomainObject(this, true, false, monitor);
                        programs.put(path, opened);
                        return new Report.Success<>(opened);
                    } catch (VersionException | IOException | CancelledException exception) {
                        return new Report.Failure<>(
                                "Program `%s` did not open, %s".formatted(path, exception));
                    }
                };
        BiFunction<Request, Program, Report.Result<Optional<DomainFile>>> execute =
                (request, program) -> {
                    try {
                        try (Transaction transaction = program.openTransaction(request.script())) {
                            transaction.abortOnClose();
                            runScript(
                                    request.script(),
                                    request.arguments().toArray(String[]::new),
                                    new GhidraState(
                                            state.getTool(),
                                            state.getProject(),
                                            program,
                                            null,
                                            null,
                                            null));
                            transaction.commit();
                        }
                        if (!program.isChanged()) {
                            return new Report.Success<>(Optional.empty());
                        }
                        program.save(
                                Stream.concat(
                                                Stream.of(request.script()),
                                                request.arguments().stream())
                                        .collect(Collectors.joining(" ")),
                                monitor);
                        return new Report.Success<>(Optional.of(program.getDomainFile()));
                    } catch (Exception exception) {
                        return new Report.Failure<>(
                                Optional.ofNullable(exception.getMessage())
                                        .orElseGet(exception::toString));
                    }
                };
        Report.Result<Optional<DomainFile>> outcome =
                Request.of(line)
                        .flatMap(request ->
                                open.apply(request.program())
                                        .flatMap(program -> execute.apply(request, program)));
        String elapsed = String.format(Locale.ROOT, "%.1fs", (System.nanoTime() - start) / 1e9);
        return switch (outcome) {
            case Report.Success<Optional<DomainFile>>(Optional<DomainFile> saved) ->
                    saved.map(file -> "ok %s saved %s\n".formatted(elapsed, file.getPathname()))
                            .orElseGet(() -> "ok %s\n".formatted(elapsed));
            case Report.Failure<Optional<DomainFile>>(String cause) ->
                    "failed %s %s\n".formatted(elapsed, cause);
        };
    }

    // --- [CONNECTIONS]

    private static Report.Result<String> read(SocketChannel channel) {
        try (BoundedInputStream input =
                        BoundedInputStream.builder()
                                .setInputStream(Channels.newInputStream(channel))
                                .setMaxCount(REQUEST_BYTES + 1L)
                                .setPropagateClose(false)
                                .get();
                BufferedReader reader =
                        new BufferedReader(
                                Channels.newReader(
                                        Channels.newChannel(input), StandardCharsets.UTF_8))) {
            Optional<String> line = Optional.ofNullable(reader.readLine());
            return input.getCount() > REQUEST_BYTES
                    ? new Report.Failure<>(
                            String.format(Locale.ROOT, "Request exceeds %d bytes", REQUEST_BYTES))
                    : line.map(String::strip)
                            .<Report.Result<String>>map(Report.Success::new)
                            .orElseGet(() -> new Report.Failure<>("Request closed before a line"));
        } catch (IOException exception) {
            return new Report.Failure<>(exception.toString());
        }
    }

    private void disconnect(SocketChannel channel, Set<SocketChannel> clients) {
        try {
            channel.close();
        } catch (IOException exception) {
            printerr("Request connection did not close, %s".formatted(exception));
        } finally {
            clients.remove(channel);
        }
    }

    private void receive(
            ServerSocketChannel server,
            ExecutorService transfers,
            Set<SocketChannel> clients,
            BlockingQueue<Report.Result<Connection>> pending) {
        try {
            while (server.isOpen()) {
                SocketChannel channel = server.accept();
                if (clients.size() >= CONNECTIONS) {
                    channel.close();
                    continue;
                }
                clients.add(channel);
                CompletableFuture<Report.Result<String>> _ =
                        CompletableFuture.supplyAsync(() -> read(channel), transfers)
                                .orTimeout(TRANSFER_SECONDS, TimeUnit.SECONDS)
                                .whenComplete((result, failure) -> {
                                    if (result instanceof Report.Success<String>(String line)) {
                                        pending.add(
                                                new Report.Success<>(
                                                        new Connection(channel, line)));
                                        return;
                                    }
                                    disconnect(channel, clients);
                                    String cause =
                                            result instanceof Report.Failure<String>(String message)
                                                    ? message
                                                    : String.valueOf(failure);
                                    printerr("Request intake failed, %s".formatted(cause));
                                });
            }
        } catch (IOException exception) {
            pending.add(new Report.Failure<>(exception.toString()));
        } finally {
            clients.forEach(channel -> disconnect(channel, clients));
        }
    }

    private void respond(
            Connection request, ExecutorService transfers, Set<SocketChannel> clients) {
        try {
            byte[] reply = answer(request.line()).getBytes(StandardCharsets.UTF_8);
            CompletableFuture.runAsync(
                            () -> {
                                try (OutputStream output =
                                        Channels.newOutputStream(request.channel())) {
                                    output.write(reply);
                                } catch (IOException exception) {
                                    throw new UncheckedIOException(exception);
                                }
                            },
                            transfers)
                    .orTimeout(TRANSFER_SECONDS, TimeUnit.SECONDS)
                    .join();
        } catch (CompletionException exception) {
            printerr("Request reply failed, %s".formatted(exception.getCause()));
        } finally {
            disconnect(request.channel(), clients);
        }
    }

    // --- [EXECUTION]

    @Override
    public void run() throws CancelledException, IOException, InterruptedException {
        String[] args = getScriptArgs();
        if (args.length > 0) {
            throw new IllegalArgumentException(
                    "%s takes no arguments, got `%s`"
                            .formatted(getScriptName(), String.join(" ", args)));
        }
        ProjectLocator locator = state.getProject().getProjectLocator();
        Path socket = locator.getMarkerFile().toPath().resolveSibling(locator.getName() + ".sock");
        end(true);
        programs.put(currentProgram.getDomainFile().getPathname(), currentProgram);
        Files.deleteIfExists(socket);
        try (ExecutorService transfers = Executors.newVirtualThreadPerTaskExecutor();
                ServerSocketChannel server =
                        ServerSocketChannel.open(StandardProtocolFamily.UNIX)) {
            server.bind(UnixDomainSocketAddress.of(socket), CONNECTIONS);
            println(
                    String.format(
                            Locale.ROOT,
                            "Listening on %s, process %d",
                            socket,
                            ProcessHandle.current().pid()));
            BlockingQueue<Report.Result<Connection>> pending = new LinkedBlockingQueue<>();
            Set<SocketChannel> clients = ConcurrentHashMap.newKeySet();
            CancelledListener cancelled =
                    () -> pending.add(new Report.Failure<>("Ghidra cancelled the server"));
            monitor.addCancelledListener(cancelled);
            transfers.execute(() -> receive(server, transfers, clients, pending));
            try {
                monitor.checkCancelled();
                for (; ; ) {
                    Report.Result<Connection> received = pending.take();
                    monitor.checkCancelled();
                    switch (received) {
                        case Report.Success<Connection>(Connection request)
                                when Objects.equals(request.line(), "stop") -> {
                            return;
                        }
                        case Report.Success<Connection>(Connection request) ->
                                respond(request, transfers, clients);
                        case Report.Failure<Connection>(String cause) ->
                                throw new IOException(cause);
                    }
                }
            } finally {
                monitor.removeCancelledListener(cancelled);
            }
        } finally {
            Files.deleteIfExists(socket);
            programs.values().stream()
                    .filter(program -> !program.equals(currentProgram))
                    .forEach(program -> program.release(this));
        }
    }
}
