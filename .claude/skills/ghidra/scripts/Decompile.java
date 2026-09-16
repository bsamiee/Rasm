// Decompiles a selection of functions with their callers and callees into one indexed file

// --- [IMPORTS] ----------------------------------------------------------------------------

import ghidra.app.decompiler.ClangTokenGroup;
import ghidra.app.decompiler.ClangTypeToken;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.data.Array;
import ghidra.program.model.data.Composite;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.DataTypeWriter;
import ghidra.program.model.data.Enum;
import ghidra.program.model.data.FunctionDefinition;
import ghidra.program.model.data.Pointer;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.data.TypeDef;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.Reference;
import ghidra.program.util.DefinedDataIterator;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.io.IOException;
import java.io.UncheckedIOException;
import java.io.Writer;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Comparator;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.SortedMap;
import java.util.TreeMap;
import java.util.TreeSet;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.function.Consumer;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [SCRIPT] -----------------------------------------------------------------------------

public class Decompile extends GhidraScript {
    private static final String USAGE =
            """
            usage: Decompile.java <out> <seed>... [callers=<depth>] [callees=<depth>] [timeout=<seconds>]
            seed: all | 0x<hex> | <name> | <namespace>::<name> | re:<regex> | str:<needle> | tag:<tag>\
            """;
    private static final Map<String, Integer> DEFAULTS =
            Map.of("callers", 0, "callees", 0, "timeout", 60);

    enum Direction {
        CALLER,
        CALLEE
    }

    sealed interface Result<T> permits Ok, Problem {}

    record Ok<T>(T value) implements Result<T> {}

    record Problem<T>(String message) implements Result<T> {}

    record Setting(String key, int value) {}

    record Block(
            Function function, String role, String text, boolean failed, List<DataType> types) {}

    @Override
    public void run() throws Exception {
        var args = getScriptArgs();
        if (args.length == 0) {
            throw new IllegalArgumentException(USAGE);
        }
        var rest = Arrays.asList(args).subList(1, args.length);
        var seeds = rest.stream().filter(arg -> !arg.contains("=")).map(this::resolve).toList();
        var settings =
                rest.stream().filter(arg -> arg.contains("=")).map(Decompile::setting).toList();
        var problems =
                Stream.concat(
                                seeds.isEmpty()
                                        ? Stream.of("no seed given")
                                        : Stream.<String>empty(),
                                Stream.concat(seeds.stream(), settings.stream())
                                        .<String>mapMulti(
                                                (result, sink) -> {
                                                    if (result instanceof Problem<?> problem) {
                                                        sink.accept(problem.message());
                                                    }
                                                }))
                        .toList();
        if (!problems.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", problems) + "\n" + USAGE);
        }
        var values = new HashMap<>(DEFAULTS);
        settings.forEach(
                result -> {
                    if (result instanceof Ok<Setting> ok) {
                        values.put(ok.value().key(), ok.value().value());
                    }
                });
        var functions =
                seeds.stream()
                        .flatMap(
                                result ->
                                        result instanceof Ok<List<Function>> ok
                                                ? ok.value().stream()
                                                : Stream.empty())
                        .distinct()
                        .toList();
        var role = rest.contains("all") ? "all" : "seed";
        var picks = select(functions, role, values.get("callers"), values.get("callees"));
        var out = Path.of(args[0]);
        println("decompiling " + picks.size() + " functions into " + out);
        println(write(out, picks, values.get("timeout"), String.join(" ", rest)));
    }

    // --- [ARGUMENTS] ----------------------------------------------------------------------

    private static Result<Setting> setting(String arg) {
        var key = arg.substring(0, arg.indexOf('='));
        var value = arg.substring(arg.indexOf('=') + 1);
        if (!DEFAULTS.containsKey(key)) {
            return new Problem<>(
                    arg + ": unknown setting, one of " + new TreeSet<>(DEFAULTS.keySet()));
        }
        try {
            return new Ok<>(new Setting(key, Integer.parseInt(value)));
        } catch (NumberFormatException exception) {
            return new Problem<>(arg + ": " + key + " takes an integer");
        }
    }

    private Result<List<Function>> resolve(String seed) {
        Result<List<Function>> matches =
                switch (seed) {
                    case "all" -> new Ok<>(functions().toList());
                    case String text when text.startsWith("0x") -> containing(text);
                    case String text when text.startsWith("re:") -> matching(text.substring(3));
                    case String text when text.startsWith("str:") ->
                            new Ok<>(referencing(text.substring(4)));
                    case String text when text.startsWith("tag:") -> tagged(text.substring(4));
                    default -> new Ok<>(named(seed));
                };
        return matches instanceof Ok<List<Function>> ok && ok.value().isEmpty()
                ? new Problem<>(seed + ": matches no function")
                : matches;
    }

    private Result<List<Function>> containing(String text) {
        try {
            var address = toAddr(Long.parseUnsignedLong(text.substring(2), 16));
            var function =
                    Optional.ofNullable(getFunctionContaining(address))
                            .or(
                                    () -> {
                                        disassemble(address);
                                        return Optional.ofNullable(createFunction(address, null));
                                    });
            return function.<Result<List<Function>>>map(found -> new Ok<>(List.of(found)))
                    .orElseGet(
                            () ->
                                    new Problem<>(
                                            text + ": no function contains it or starts there"));
        } catch (NumberFormatException exception) {
            return new Problem<>(text + ": not a hexadecimal address");
        }
    }

    private Result<List<Function>> matching(String regex) {
        try {
            var pattern = Pattern.compile(regex);
            return new Ok<>(
                    functions()
                            .filter(function -> pattern.matcher(function.getName(true)).find())
                            .toList());
        } catch (PatternSyntaxException exception) {
            return new Problem<>("re:" + regex + ": " + exception.getDescription());
        }
    }

    private List<Function> referencing(String needle) {
        var lowered = needle.toLowerCase(Locale.ROOT);
        var references = currentProgram.getReferenceManager();
        return CollectionUtils.asStream(
                        DefinedDataIterator.byDataInstance(
                                currentProgram, StringDataInstance::isString))
                .filter(
                        data ->
                                data.getValue() instanceof String text
                                        && text.toLowerCase(Locale.ROOT).contains(lowered))
                .flatMap(
                        data ->
                                CollectionUtils.asStream(
                                        references.getReferencesTo(data.getAddress())))
                .map(Reference::getFromAddress)
                .map(this::getFunctionContaining)
                .filter(Objects::nonNull)
                .distinct()
                .toList();
    }

    private Result<List<Function>> tagged(String name) {
        var tag = currentProgram.getFunctionManager().getFunctionTagManager().getFunctionTag(name);
        return tag == null
                ? new Problem<>("tag:" + name + ": the program defines no such tag")
                : new Ok<>(
                        functions().filter(function -> function.getTags().contains(tag)).toList());
    }

    private List<Function> named(String name) {
        return functions()
                .filter(
                        function ->
                                name.equals(function.getName(true))
                                        || name.equals(function.getName()))
                .toList();
    }

    private Stream<Function> functions() {
        return CollectionUtils.asStream(currentProgram.getFunctionManager().getFunctions(true));
    }

    // --- [SELECTION] ----------------------------------------------------------------------

    private SequencedMap<Function, String> select(
            List<Function> seeds, String role, int callers, int callees) {
        var picks = new LinkedHashMap<Function, String>();
        seeds.forEach(function -> picks.put(function, role));
        expand(picks, seeds, callees, Direction.CALLEE);
        expand(picks, seeds, callers, Direction.CALLER);
        return picks;
    }

    private void expand(
            Map<Function, String> picks, List<Function> from, int depth, Direction direction) {
        var frontier = from;
        for (var level = 1; level <= depth && !frontier.isEmpty(); level++) {
            var role = direction.name().toLowerCase(Locale.ROOT) + ":" + level;
            var next =
                    frontier.stream()
                            .flatMap(function -> neighbors(function, direction))
                            .filter(Decompile::decompilable)
                            .filter(function -> !picks.containsKey(function))
                            .distinct()
                            .sorted(Comparator.comparing(Function::getEntryPoint))
                            .toList();
            next.forEach(function -> picks.put(function, role));
            frontier = next;
        }
    }

    private Stream<Function> neighbors(Function function, Direction direction) {
        return switch (direction) {
            case CALLER ->
                    function.getCallingFunctions(monitor).stream()
                            .flatMap(
                                    caller ->
                                            caller.isThunk()
                                                    ? neighbors(caller, Direction.CALLER)
                                                    : Stream.of(caller));
            case CALLEE ->
                    function.getCalledFunctions(monitor).stream()
                            .map(
                                    callee ->
                                            callee.isThunk()
                                                    ? callee.getThunkedFunction(true)
                                                    : callee);
        };
    }

    private static boolean decompilable(Function function) {
        return !function.isThunk() && !function.isExternal();
    }

    // --- [DECOMPILE] ----------------------------------------------------------------------

    private String write(
            Path out, SequencedMap<Function, String> picks, int timeout, String request)
            throws Exception {
        var options = new DecompileOptions();
        options.grabFromProgram(currentProgram);
        options.setDefaultTimeout(timeout);
        options.setIndentWidth(4);
        var callback =
                new DecompilerCallback<Block>(
                        currentProgram,
                        decompiler -> {
                            decompiler.setOptions(options);
                            decompiler.toggleSyntaxTree(false);
                        }) {
                    @Override
                    public Block process(DecompileResults results, TaskMonitor taskMonitor) {
                        return block(results, picks.get(results.getFunction()), timeout);
                    }
                };
        callback.setTimeout(timeout);
        var part = out.resolveSibling(out.getFileName() + ".part");
        Files.createDirectories(out.toAbsolutePath().getParent());
        try {
            Output output;
            try (var writer = Files.newBufferedWriter(part)) {
                output = new Output(writer, List.copyOf(picks.keySet()));
                picks.entrySet().stream()
                        .filter(pick -> !decompilable(pick.getKey()))
                        .map(pick -> stub(pick.getKey(), pick.getValue()))
                        .forEach(output);
                ParallelDecompiler.decompileFunctions(
                        callback,
                        currentProgram,
                        picks.keySet().stream().filter(Decompile::decompilable).iterator(),
                        output,
                        monitor);
            }
            try (var writer = Files.newBufferedWriter(out)) {
                writer.write(
                        "// ==== INDEX %s %s functions=%d failed=%d types=%d args=%s\n"
                                .formatted(
                                        currentProgram.getName(),
                                        currentProgram.getLanguageID(),
                                        picks.size(),
                                        output.failed(),
                                        output.types().size(),
                                        request));
                for (var row : output.rows()) {
                    writer.write(row);
                }
                writer.write("\n");
                try (var reader = Files.newBufferedReader(part)) {
                    reader.transferTo(writer);
                }
                writer.write("// ==== TYPES " + output.types().size() + "\n");
                new DataTypeWriter(currentProgram.getDataTypeManager(), writer, false)
                        .write(List.copyOf(output.types().values()), monitor);
            }
            return "wrote %s: %d functions, %d failed, %d types"
                    .formatted(out, picks.size(), output.failed(), output.types().size());
        } finally {
            callback.dispose();
            Files.deleteIfExists(part);
        }
    }

    private Block block(DecompileResults results, String role, int timeout) {
        var function = results.getFunction();
        var callers = function.getCallingFunctions(monitor);
        var names =
                "seed".equals(role)
                        ? callers.stream()
                                .sorted(Comparator.comparing(Function::getEntryPoint))
                                .map(caller -> caller.getName(true))
                                .collect(Collectors.joining(", ", ": ", ""))
                        : "";
        var header =
                "// ==== FUNC %s @ %s size=%d %s callers=%d%s\n"
                        .formatted(
                                function.getName(true),
                                function.getEntryPoint(),
                                function.getBody().getNumAddresses(),
                                role,
                                callers.size(),
                                callers.isEmpty() ? "" : names);
        if (!results.decompileCompleted()) {
            var cause =
                    results.isTimedOut()
                            ? "timeout after " + timeout + " s"
                            : results.getErrorMessage();
            return new Block(
                    function, role, header + "// failed: " + cause.strip() + "\n", true, List.of());
        }
        var code = results.getDecompiledFunction().getC().strip() + "\n";
        return new Block(function, role, header + code, false, types(results.getCCodeMarkup()));
    }

    private static Block stub(Function function, String role) {
        var text =
                "// ==== THUNK %s @ %s -> %s\n"
                        .formatted(
                                function.getName(true),
                                function.getEntryPoint(),
                                function.getThunkedFunction(true).getName(true));
        return new Block(function, role, text, false, List.of());
    }

    private static List<DataType> types(ClangTokenGroup markup) {
        return CollectionUtils.asStream(markup.tokenIterator(true))
                .<DataType>mapMulti(
                        (token, sink) -> {
                            if (token instanceof ClangTypeToken typed) {
                                sink.accept(typed.getDataType());
                            }
                        })
                .flatMap(type -> named(type).stream())
                .distinct()
                .toList();
    }

    private static Optional<DataType> named(DataType type) {
        return switch (type) {
            case null -> Optional.empty();
            case Pointer pointer -> named(pointer.getDataType());
            case Array array -> named(array.getDataType());
            case Composite composite -> Optional.of(composite);
            case Enum enumeration -> Optional.of(enumeration);
            case TypeDef typedef -> Optional.of(typedef);
            case FunctionDefinition definition -> Optional.of(definition);
            default -> Optional.empty();
        };
    }

    // --- [OUTPUT] -------------------------------------------------------------------------

    private static final class Output implements Consumer<Block> {
        private final Lock lock = new ReentrantLock();
        private final Writer writer;
        private final Map<Function, Integer> positions;
        private final SortedMap<Integer, Block> pending = new TreeMap<>();
        private final SortedMap<String, DataType> namedTypes = new TreeMap<>();
        private final String[] indexRows;
        private int next;
        private int failures;
        private long line;

        Output(Writer writer, List<Function> order) {
            this.writer = writer;
            this.positions =
                    IntStream.range(0, order.size())
                            .boxed()
                            .collect(Collectors.toMap(order::get, position -> position));
            this.indexRows = new String[order.size()];
            this.line = order.size() + 3L;
        }

        @Override
        public void accept(Block block) {
            lock.lock();
            try {
                pending.put(positions.get(block.function()), block);
                while (pending.containsKey(next)) {
                    var ready = pending.remove(next);
                    writer.write(ready.text());
                    indexRows[next] = row(ready, line);
                    line += ready.text().chars().filter(character -> character == '\n').count();
                    failures += ready.failed() ? 1 : 0;
                    ready.types().forEach(type -> namedTypes.put(type.getPathName(), type));
                    next++;
                }
            } catch (IOException exception) {
                throw new UncheckedIOException(exception);
            } finally {
                lock.unlock();
            }
        }

        List<String> rows() {
            return List.of(indexRows);
        }

        SortedMap<String, DataType> types() {
            return namedTypes;
        }

        int failed() {
            return failures;
        }

        private static String row(Block block, long line) {
            var function = block.function();
            var status = block.failed() ? " failed" : function.isThunk() ? " thunk" : "";
            return "// %s %s size=%d %s line=%d%s\n"
                    .formatted(
                            function.getEntryPoint(),
                            function.getName(true),
                            function.getBody().getNumAddresses(),
                            block.role(),
                            line,
                            status);
        }
    }
}
