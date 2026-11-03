import ghidra.app.script.GhidraScript;
import ghidra.app.script.GhidraState;
import ghidra.framework.model.DomainFile;
import ghidra.framework.model.ProjectLocator;
import ghidra.program.model.listing.Program;
import ghidra.util.exception.CancelledException;
import ghidra.util.exception.VersionException;

import rasm.Arguments;
import rasm.Report;

import java.io.BufferedReader;
import java.io.IOException;
import java.net.StandardProtocolFamily;
import java.net.UnixDomainSocketAddress;
import java.nio.ByteBuffer;
import java.nio.channels.Channels;
import java.nio.channels.ServerSocketChannel;
import java.nio.channels.SocketChannel;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import java.util.function.BiFunction;
import java.util.function.BooleanSupplier;
import java.util.function.Function;
import java.util.regex.Pattern;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Serve extends GhidraScript {
    // --- [REQUESTS]

    private static final Pattern CELL = Pattern.compile("\t");

    private final Map<String, Program> programs = new ConcurrentHashMap<>();

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
                    if (programs.get(path) instanceof Program program) {
                        return new Report.Success<>(program);
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
                        if (!program.isChanged()) {
                            return new Report.Success<>(Optional.empty());
                        }
                        program.save(
                                request.script() + " " + String.join(" ", request.arguments()),
                                monitor);
                        return new Report.Success<>(Optional.of(program.getDomainFile()));
                    } catch (Exception exception) {
                        return new Report.Failure<>(String.valueOf(exception.getMessage()));
                    }
                };
        Report.Result<Optional<DomainFile>> outcome =
                Request.of(line)
                        .flatMap(request ->
                                open.apply(request.program())
                                        .flatMap(program -> execute.apply(request, program)));
        String elapsed = "%.1fs".formatted((System.nanoTime() - start) / 1e9);
        return switch (outcome) {
            case Report.Success<Optional<DomainFile>>(Optional<DomainFile> saved) ->
                    "ok %s%s\n"
                            .formatted(
                                    elapsed,
                                    saved.map(file -> " saved " + file.getPathname()).orElse(""));
            case Report.Failure<Optional<DomainFile>>(String cause) ->
                    "failed %s %s\n".formatted(elapsed, cause);
        };
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        new Arguments.Usage(getScriptName(), "")
                .values(
                        Arrays.stream(getScriptArgs())
                                .<Report.Result<String>>map(argument ->
                                        new Report.Failure<>(
                                                "Argument `%s` follows the script name"
                                                        .formatted(argument))));
        ProjectLocator locator = state.getProject().getProjectLocator();
        Path socket = locator.getMarkerFile().toPath().resolveSibling(locator.getName() + ".sock");
        end(true);
        programs.put(currentProgram.getDomainFile().getPathname(), currentProgram);
        Files.deleteIfExists(socket);
        try (ServerSocketChannel server = ServerSocketChannel.open(StandardProtocolFamily.UNIX)) {
            server.bind(UnixDomainSocketAddress.of(socket));
            BooleanSupplier session =
                    () -> {
                        try (SocketChannel client = server.accept();
                                BufferedReader reader =
                                        new BufferedReader(
                                                Channels.newReader(
                                                        client, StandardCharsets.UTF_8))) {
                            String line = Objects.requireNonNullElse(reader.readLine(), "").strip();
                            boolean serving = !"stop".equals(line);
                            if (serving) {
                                client.write(
                                        ByteBuffer.wrap(
                                                answer(line).getBytes(StandardCharsets.UTF_8)));
                            }
                            return serving;
                        } catch (IOException exception) {
                            printerr(
                                    "Request connection closed, %s"
                                            .formatted(exception.getMessage()));
                            return server.isOpen();
                        }
                    };
            while (session.getAsBoolean()) {
                monitor.checkCancelled();
            }
        } finally {
            Files.deleteIfExists(socket);
            programs.values().stream()
                    .filter(program -> !program.equals(currentProgram))
                    .forEach(program -> program.release(this));
        }
    }
}
