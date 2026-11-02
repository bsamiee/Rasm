import ghidra.app.script.GhidraScript;
import ghidra.app.script.GhidraState;
import ghidra.framework.model.DomainFile;
import ghidra.program.model.listing.Program;

import rasm.Arguments;
import rasm.Report;

import java.io.BufferedReader;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Serve extends GhidraScript {
    // --- [PROGRAMS]

    private static final String STOP = "stop";

    private static final String FORM = "<reply> <program> <script> <argument>...";

    private final Map<String, Program> programs = new ConcurrentHashMap<>();

    private Report.Result<Program> program(String name) {
        String path = name.startsWith("/") ? name : "/" + name;
        return Optional.ofNullable(programs.get(path))
                .<Report.Result<Program>>map(Report.Success::new)
                .orElseGet(() -> open(path));
    }

    private Report.Result<Program> open(String path) {
        DomainFile file = state.getProject().getProjectData().getFile(path);
        if (file == null) {
            return new Report.Failure<>("Project holds no program `%s`".formatted(path));
        }
        try {
            Program opened = (Program) file.getDomainObject(this, true, false, monitor);
            programs.put(path, opened);
            return new Report.Success<>(opened);
        } catch (Exception exception) {
            return new Report.Failure<>("Program `%s` did not open. %s".formatted(path, exception));
        }
    }

    // --- [REQUESTS]

    private Report.Result<String> execute(List<String> tokens, Program program) {
        String[] arguments = tokens.subList(3, tokens.size()).toArray(String[]::new);
        try {
            runScript(
                    tokens.get(2),
                    arguments,
                    new GhidraState(state.getTool(), state.getProject(), program, null, null, null));
            if (!program.isChanged()) {
                return new Report.Success<>("");
            }
            program.save(String.join(" ", tokens.subList(2, tokens.size())), monitor);
            return new Report.Success<>(" saved " + program.getDomainFile().getPathname());
        } catch (Exception exception) {
            return new Report.Failure<>(String.valueOf(exception.getMessage()));
        }
    }

    private String answer(List<String> tokens) {
        long start = System.nanoTime();
        Report.Result<String> outcome =
                tokens.size() < 3
                        ? new Report.Failure<>("Request matches no form " + FORM)
                        : switch (program(tokens.get(1))) {
                            case Report.Success<Program>(Program program) ->
                                    execute(tokens, program);
                            case Report.Failure<Program>(String cause) ->
                                    new Report.Failure<>(cause);
                        };
        String elapsed = "%.1fs".formatted((System.nanoTime() - start) / 1e9);
        return switch (outcome) {
            case Report.Success<String>(String saved) -> "ok %s%s\n".formatted(elapsed, saved);
            case Report.Failure<String>(String cause) -> "failed %s %s\n".formatted(elapsed, cause);
        };
    }

    // --- [RUN]

    private boolean session(Path requests) throws IOException {
        try (BufferedReader reader = Files.newBufferedReader(requests)) {
            for (String line = reader.readLine(); line != null; line = reader.readLine()) {
                List<String> tokens =
                        Arrays.stream(line.strip().split("\\s+"))
                                .filter(token -> !token.isEmpty())
                                .toList();
                if (List.of(STOP).equals(tokens)) {
                    return false;
                }
                if (!tokens.isEmpty()) {
                    Files.writeString(Path.of(tokens.getFirst()), answer(tokens));
                }
            }
            return true;
        }
    }

    @Override
    public void run() throws Exception {
        Path requests = new Arguments.Usage(getScriptName(), "<requests>").only(getScriptArgs());
        end(true);
        programs.put(currentProgram.getDomainFile().getPathname(), currentProgram);
        try {
            while (session(requests)) {
                monitor.checkCancelled();
            }
        } finally {
            programs.values().stream()
                    .filter(program -> !program.equals(currentProgram))
                    .forEach(program -> program.release(this));
        }
    }
}
