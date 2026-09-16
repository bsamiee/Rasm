// Decompiles seed functions with their callers and callees into one indexed C file

// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.ClangTokenGroup;
import ghidra.app.decompiler.ClangTypeToken;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.script.GhidraScript;
import ghidra.program.database.data.DataTypeUtilities;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.Composite;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.DataTypeWriter;
import ghidra.program.model.data.Enum;
import ghidra.program.model.data.TypeDef;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.pcode.HighSymbol;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.io.StringWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.EnumSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.Set;
import java.util.SortedSet;
import java.util.TreeSet;
import java.util.function.BiFunction;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class Decompile extends GhidraScript {
    enum Reach {
        CALLEE(Function::getCalledFunctions, (thunk, _) -> Set.of(thunk.getThunkedFunction(true))),
        CALLER(Function::getCallingFunctions, Function::getCallingFunctions);

        final BiFunction<Function, TaskMonitor, Set<Function>> adjacent;
        final BiFunction<Function, TaskMonitor, Set<Function>> beyond;

        Reach(
                BiFunction<Function, TaskMonitor, Set<Function>> adjacent,
                BiFunction<Function, TaskMonitor, Set<Function>> beyond) {
            this.adjacent = adjacent;
            this.beyond = beyond;
        }

        Stream<Function> neighbors(Function function, TaskMonitor monitor) {
            Set<Function> step = (function.isThunk() ? beyond : adjacent).apply(function, monitor);
            return step.stream()
                    .flatMap(next -> next.isThunk() ? neighbors(next, monitor) : Stream.of(next));
        }
    }

    record Global(Address address, DataType type, String name) {
        Global(HighSymbol symbol) {
            this(symbol.getStorage().getMinAddress(), symbol.getDataType(), symbol.getName());
        }
    }

    record Block(
            Function function,
            String text,
            boolean failed,
            List<DataType> types,
            List<Global> globals) {}

    @Override
    public void run() throws Exception {
        Job.Request request =
                Job.parse(
                        getScriptName(),
                        getScriptArgs(),
                        EnumSet.allOf(Job.Setting.class),
                        currentProgram);
        SequencedMap<Function, String> picks = select(request.seeds(), request.settings());
        println(write(request.out(), decompile(picks, request.settings()), request.text()));
    }

    // --- [SELECTION] -------------------------------------------------------------------

    private SequencedMap<Function, String> select(
            List<Function> seeds, Map<Job.Setting, Integer> settings) {
        SequencedMap<Function, String> picks = new LinkedHashMap<>();
        seeds.forEach(seed -> picks.put(seed, "seed"));
        expand(picks, seeds, Reach.CALLEE, 1, settings.get(Job.Setting.CALLEES));
        expand(picks, seeds, Reach.CALLER, 1, settings.get(Job.Setting.CALLERS));
        return picks;
    }

    private void expand(
            SequencedMap<Function, String> picks,
            List<Function> frontier,
            Reach reach,
            int depth,
            int limit) {
        if (depth > limit) {
            return;
        }
        List<Function> next =
                frontier.stream()
                        .flatMap(function -> reach.neighbors(function, monitor))
                        .filter(Decompile::decompilable)
                        .filter(function -> !picks.containsKey(function))
                        .distinct()
                        .sorted(Comparator.comparing(Function::getEntryPoint))
                        .toList();
        String role = reach.name().toLowerCase(Locale.ROOT) + ":" + depth;
        next.forEach(function -> picks.put(function, role));
        expand(picks, next, reach, depth + 1, limit);
    }

    private static boolean decompilable(Function function) {
        return !function.isThunk() && !function.isExternal();
    }

    // --- [DECOMPILE] -------------------------------------------------------------------

    private List<Block> decompile(
            SequencedMap<Function, String> picks, Map<Job.Setting, Integer> settings)
            throws Exception {
        int timeout = settings.get(Job.Setting.TIMEOUT);
        DecompileOptions options = new DecompileOptions();
        options.grabFromProgram(currentProgram);
        options.setMaxPayloadMBytes(settings.get(Job.Setting.PAYLOAD));
        DecompilerCallback<Block> callback =
                new DecompilerCallback<>(
                        currentProgram, decompiler -> decompiler.setOptions(options)) {
                    @Override
                    public Block process(DecompileResults results, TaskMonitor taskMonitor) {
                        return block(results, picks.get(results.getFunction()), timeout);
                    }
                };
        callback.setTimeout(timeout);
        List<Function> targets =
                picks.keySet().stream().filter(Predicate.not(Function::isThunk)).toList();
        try {
            List<Block> blocks = ParallelDecompiler.decompileFunctions(callback, targets, monitor);
            // A cancelled item yields null, every other item its block or the exception it threw
            monitor.checkCancelled();
            Map<Function, Block> decompiled =
                    blocks.stream().collect(Collectors.toMap(Block::function, block -> block));
            return picks.keySet().stream()
                    .map(function -> function.isThunk() ? stub(function) : decompiled.get(function))
                    .toList();
        } finally {
            callback.dispose();
        }
    }

    private Block block(DecompileResults results, String role, int timeout) {
        Function function = results.getFunction();
        String header = header(function, role);
        if (!results.decompileCompleted()) {
            String cause =
                    results.isTimedOut()
                            ? "timed out after " + timeout + " s"
                            : results.getErrorMessage().strip();
            return new Block(function, header + "// failed: " + cause, true, List.of(), List.of());
        }
        String code = results.getDecompiledFunction().getC().strip();
        List<Global> globals =
                CollectionUtils.asStream(
                                results.getHighFunction().getGlobalSymbolMap().getSymbols())
                        .filter(symbol -> symbol.getStorage().isMemoryStorage())
                        .map(Global::new)
                        .toList();
        return new Block(function, header + code, false, types(results.getCCodeMarkup()), globals);
    }

    private String header(Function function, String role) {
        List<String> callers =
                function.getCallingFunctions(monitor).stream()
                        .sorted(Comparator.comparing(Function::getEntryPoint))
                        .map(caller -> caller.getName(true))
                        .toList();
        return "%s\n// %s size=%d %s callers=%d%s\n"
                .formatted(
                        Job.item(function.getName(true)),
                        function.getEntryPoint(),
                        function.getBody().getNumAddresses(),
                        role,
                        callers.size(),
                        callers.isEmpty() ? "" : ": " + String.join(", ", callers));
    }

    private static Block stub(Function thunk) {
        String text =
                "%s\n// %s thunk -> %s"
                        .formatted(
                                Job.item(thunk.getName(true)),
                                thunk.getEntryPoint(),
                                thunk.getThunkedFunction(true).getName(true));
        return new Block(thunk, text, false, List.of(), List.of());
    }

    private static List<DataType> types(ClangTokenGroup markup) {
        return CollectionUtils.asStream(markup.tokenIterator(true))
                .filter(ClangTypeToken.class::isInstance)
                .map(ClangTypeToken.class::cast)
                .map(ClangTypeToken::getDataType)
                .map(DataTypeUtilities::getBaseDataType)
                .filter(Decompile::declared)
                .distinct()
                .toList();
    }

    private static boolean declared(DataType type) {
        return switch (type) {
            case Composite _, Enum _, TypeDef _ -> true;
            case null, default -> false;
        };
    }

    // --- [WRITE] -----------------------------------------------------------------------

    private String write(Path out, List<Block> blocks, String request) throws Exception {
        long failed = blocks.stream().filter(Block::failed).count();
        Comparator<Global> byAddress = Comparator.comparing(Global::address);
        List<String> globals =
                blocks.stream()
                        .flatMap(block -> block.globals().stream())
                        .collect(Collectors.toCollection(() -> new TreeSet<>(byAddress)))
                        .stream()
                        .flatMap(global -> row(global).stream())
                        .toList();
        Comparator<DataType> byPath = Comparator.comparing(DataType::getPathName);
        SortedSet<DataType> types =
                blocks.stream()
                        .flatMap(block -> block.types().stream())
                        .collect(Collectors.toCollection(() -> new TreeSet<>(byPath)));
        StringWriter declarations = new StringWriter();
        new DataTypeWriter(currentProgram.getDataTypeManager(), declarations, false)
                .write(List.copyOf(types), monitor);
        String facts =
                "functions=%d failed=%d globals=%d types=%d args=%s"
                        .formatted(blocks.size(), failed, globals.size(), types.size(), request);
        List<String> lines =
                Stream.of(
                                Job.index(currentProgram, facts),
                                blocks.stream().flatMap(block -> block.text().lines()),
                                Stream.of(Job.divider("GLOBALS")),
                                globals.stream(),
                                Stream.of(Job.divider("TYPES")),
                                declarations.toString().lines())
                        .flatMap(section -> section)
                        .toList();
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, lines);
        return "wrote %s: %d functions, %d failed, %d globals, %d types"
                .formatted(out, blocks.size(), failed, globals.size(), types.size());
    }

    private Optional<String> row(Global global) {
        return Optional.ofNullable(getDataAt(global.address()))
                .filter(Data::isDefined)
                .map(Data::getDefaultValueRepresentation)
                .filter(value -> !value.isEmpty())
                .map(
                        value ->
                                "// %s %s %s = %s"
                                        .formatted(
                                                global.address(),
                                                global.type().getDisplayName(),
                                                global.name(),
                                                value));
    }
}
