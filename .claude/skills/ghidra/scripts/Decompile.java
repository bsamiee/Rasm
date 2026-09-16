// Decompiles a selection of functions with their callers and callees into one indexed file

// --- [IMPORTS] ----------------------------------------------------------------------------

import ghidra.app.decompiler.ClangTokenGroup;
import ghidra.app.decompiler.ClangTypeToken;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
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
import ghidra.program.model.listing.FunctionTag;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.ReferenceManager;
import ghidra.program.util.DefinedDataIterator;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.io.BufferedWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Comparator;
import java.util.EnumMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.Set;
import java.util.SortedMap;
import java.util.TreeMap;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] -----------------------------------------------------------------------------

public class Decompile extends GhidraScript {
    private static final String USAGE =
            """
            usage: Decompile.java <out> <seed>... [callers=<depth>] [callees=<depth>] [timeout=<seconds>]
            seed: all | 0x<hex> | <name> | <namespace>::<name> | re:<regex> | str:<needle> | tag:<tag>\
            """;

    enum Setting {
        CALLERS(0),
        CALLEES(0),
        TIMEOUT(60);

        private final int preset;

        Setting(int preset) {
            this.preset = preset;
        }

        String key() {
            return name().toLowerCase(Locale.ROOT);
        }
    }

    enum Kind {
        SEED,
        ALL,
        CALLER,
        CALLEE
    }

    record Role(Kind kind, int depth) {
        String label() {
            return kind.name().toLowerCase(Locale.ROOT) + (depth == 0 ? "" : ":" + depth);
        }
    }

    sealed interface Result<T> permits Ok, Problem {
        default Stream<T> values() {
            return this instanceof Ok<T> ok ? Stream.of(ok.value()) : Stream.empty();
        }

        default Stream<String> problems() {
            return this instanceof Problem<T> problem
                    ? Stream.of(problem.message())
                    : Stream.empty();
        }
    }

    record Ok<T>(T value) implements Result<T> {}

    record Problem<T>(String message) implements Result<T> {}

    record Block(Function function, Role role, String text, boolean failed, List<DataType> types) {}

    @Override
    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length == 0) {
            throw new IllegalArgumentException(USAGE);
        }
        List<String> rest = Arrays.asList(args).subList(1, args.length);
        List<Result<List<Function>>> seeds =
                rest.stream().filter(arg -> !arg.contains("=")).map(this::resolve).toList();
        List<Result<Map.Entry<Setting, Integer>>> settings =
                rest.stream().filter(arg -> arg.contains("=")).map(Decompile::setting).toList();
        List<String> problems =
                Stream.concat(
                                seeds.isEmpty()
                                        ? Stream.of("no seed given")
                                        : Stream.<String>empty(),
                                Stream.concat(seeds.stream(), settings.stream())
                                        .flatMap(Result::problems))
                        .toList();
        if (!problems.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", problems) + "\n" + USAGE);
        }
        Map<Setting, Integer> values =
                Stream.concat(
                                Arrays.stream(Setting.values())
                                        .map(setting -> Map.entry(setting, setting.preset)),
                                settings.stream().flatMap(Result::values))
                        .collect(
                                Collectors.toMap(
                                        Map.Entry::getKey,
                                        Map.Entry::getValue,
                                        (preset, given) -> given,
                                        () -> new EnumMap<>(Setting.class)));
        List<Function> functions =
                seeds.stream().flatMap(Result::values).flatMap(List::stream).distinct().toList();
        Role role = new Role(rest.contains("all") ? Kind.ALL : Kind.SEED, 0);
        SequencedMap<Function, Role> picks =
                select(functions, role, values.get(Setting.CALLERS), values.get(Setting.CALLEES));
        Path out = Path.of(args[0]);
        println("decompiling " + picks.size() + " functions into " + out);
        println(write(out, picks, values.get(Setting.TIMEOUT), String.join(" ", rest)));
    }

    // --- [ARGUMENTS] ----------------------------------------------------------------------

    private static Result<Map.Entry<Setting, Integer>> setting(String arg) {
        String key = arg.substring(0, arg.indexOf('='));
        String value = arg.substring(arg.indexOf('=') + 1);
        return Arrays.stream(Setting.values())
                .filter(candidate -> candidate.key().equals(key))
                .findFirst()
                .map(setting -> amount(arg, setting, value))
                .orElseGet(
                        () ->
                                new Problem<>(
                                        arg
                                                + ": unknown setting, one of "
                                                + Arrays.stream(Setting.values())
                                                        .map(Setting::key)
                                                        .toList()));
    }

    private static Result<Map.Entry<Setting, Integer>> amount(
            String arg, Setting setting, String value) {
        try {
            return new Ok<>(Map.entry(setting, Integer.parseInt(value)));
        } catch (NumberFormatException exception) {
            return new Problem<>(arg + ": " + setting.key() + " takes an integer");
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
            Address address = toAddr(Long.parseUnsignedLong(text.substring(2), 16));
            Optional<Function> function =
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
            Pattern pattern = Pattern.compile(regex);
            return new Ok<>(
                    functions()
                            .filter(function -> pattern.matcher(function.getName(true)).find())
                            .toList());
        } catch (PatternSyntaxException exception) {
            return new Problem<>("re:" + regex + ": " + exception.getDescription());
        }
    }

    private List<Function> referencing(String needle) {
        String lowered = needle.toLowerCase(Locale.ROOT);
        ReferenceManager references = currentProgram.getReferenceManager();
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
        FunctionTag tag =
                currentProgram.getFunctionManager().getFunctionTagManager().getFunctionTag(name);
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

    private SequencedMap<Function, Role> select(
            List<Function> seeds, Role role, int callers, int callees) {
        SequencedMap<Function, Role> picks = new LinkedHashMap<>();
        seeds.forEach(function -> picks.put(function, role));
        expand(picks, seeds, 1, callees, Kind.CALLEE);
        expand(picks, seeds, 1, callers, Kind.CALLER);
        return picks;
    }

    private void expand(
            Map<Function, Role> picks, List<Function> frontier, int level, int depth, Kind kind) {
        if (level > depth || frontier.isEmpty()) {
            return;
        }
        Role role = new Role(kind, level);
        List<Function> next =
                frontier.stream()
                        .flatMap(function -> neighbors(function, kind))
                        .filter(Decompile::decompilable)
                        .filter(function -> !picks.containsKey(function))
                        .distinct()
                        .sorted(Comparator.comparing(Function::getEntryPoint))
                        .toList();
        next.forEach(function -> picks.put(function, role));
        expand(picks, next, level + 1, depth, kind);
    }

    private Stream<Function> neighbors(Function function, Kind kind) {
        return switch (kind) {
            case CALLER ->
                    function.getCallingFunctions(monitor).stream()
                            .flatMap(
                                    caller ->
                                            caller.isThunk()
                                                    ? neighbors(caller, Kind.CALLER)
                                                    : Stream.of(caller));
            case CALLEE ->
                    function.getCalledFunctions(monitor).stream()
                            .map(
                                    callee ->
                                            callee.isThunk()
                                                    ? callee.getThunkedFunction(true)
                                                    : callee);
            case SEED, ALL -> Stream.empty();
        };
    }

    private static boolean decompilable(Function function) {
        return !function.isThunk() && !function.isExternal();
    }

    // --- [DECOMPILE] ----------------------------------------------------------------------

    private String write(Path out, SequencedMap<Function, Role> picks, int timeout, String request)
            throws Exception {
        DecompileOptions options = new DecompileOptions();
        options.grabFromProgram(currentProgram);
        options.setDefaultTimeout(timeout);
        options.setIndentWidth(4);
        DecompilerCallback<Block> callback =
                new DecompilerCallback<>(
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
        List<Block> decompiled;
        try {
            decompiled =
                    ParallelDecompiler.decompileFunctions(
                            callback,
                            picks.keySet().stream().filter(Decompile::decompilable).toList(),
                            monitor);
        } finally {
            callback.dispose();
        }
        Map<Function, Block> byFunction =
                Stream.concat(
                                picks.entrySet().stream()
                                        .filter(pick -> !decompilable(pick.getKey()))
                                        .map(pick -> stub(pick.getKey(), pick.getValue())),
                                decompiled.stream())
                        .collect(Collectors.toMap(Block::function, block -> block));
        List<Block> blocks = picks.keySet().stream().map(byFunction::get).toList();
        long failed = blocks.stream().filter(Block::failed).count();
        SortedMap<String, DataType> types =
                blocks.stream()
                        .flatMap(block -> block.types().stream())
                        .collect(
                                Collectors.toMap(
                                        DataType::getPathName,
                                        type -> type,
                                        (first, second) -> first,
                                        TreeMap::new));
        Files.createDirectories(out.toAbsolutePath().getParent());
        try (BufferedWriter writer = Files.newBufferedWriter(out)) {
            writer.write(
                    "// ==== INDEX %s %s functions=%d failed=%d types=%d args=%s\n"
                            .formatted(
                                    currentProgram.getName(),
                                    currentProgram.getLanguageID(),
                                    blocks.size(),
                                    failed,
                                    types.size(),
                                    request));
            long line = blocks.size() + 3L;
            for (Block block : blocks) {
                writer.write(row(block, line));
                line += block.text().lines().count();
            }
            writer.write("\n");
            for (Block block : blocks) {
                writer.write(block.text());
            }
            writer.write("// ==== TYPES " + types.size() + "\n");
            new DataTypeWriter(currentProgram.getDataTypeManager(), writer, false)
                    .write(List.copyOf(types.values()), monitor);
        }
        return "wrote %s: %d functions, %d failed, %d types"
                .formatted(out, blocks.size(), failed, types.size());
    }

    private Block block(DecompileResults results, Role role, int timeout) {
        Function function = results.getFunction();
        Set<Function> callers = function.getCallingFunctions(monitor);
        String names =
                role.kind() == Kind.SEED
                        ? callers.stream()
                                .sorted(Comparator.comparing(Function::getEntryPoint))
                                .map(caller -> caller.getName(true))
                                .collect(Collectors.joining(", ", ": ", ""))
                        : "";
        String header =
                "// ==== FUNC %s @ %s size=%d %s callers=%d%s\n"
                        .formatted(
                                function.getName(true),
                                function.getEntryPoint(),
                                function.getBody().getNumAddresses(),
                                role.label(),
                                callers.size(),
                                callers.isEmpty() ? "" : names);
        if (!results.decompileCompleted()) {
            String cause =
                    results.isTimedOut()
                            ? "timeout after " + timeout + " s"
                            : results.getErrorMessage();
            return new Block(
                    function, role, header + "// failed: " + cause.strip() + "\n", true, List.of());
        }
        String code = results.getDecompiledFunction().getC().strip() + "\n";
        return new Block(function, role, header + code, false, types(results.getCCodeMarkup()));
    }

    private static Block stub(Function function, Role role) {
        String text =
                function.isThunk()
                        ? "// ==== THUNK %s @ %s -> %s\n"
                                .formatted(
                                        function.getName(true),
                                        function.getEntryPoint(),
                                        function.getThunkedFunction(true).getName(true))
                        : "// ==== EXTERNAL %s @ %s\n"
                                .formatted(function.getName(true), function.getEntryPoint());
        return new Block(function, role, text, false, List.of());
    }

    private static String row(Block block, long line) {
        Function function = block.function();
        String status =
                block.failed()
                        ? " failed"
                        : function.isThunk() ? " thunk" : function.isExternal() ? " external" : "";
        return "// %s %s size=%d %s line=%d%s\n"
                .formatted(
                        function.getEntryPoint(),
                        function.getName(true),
                        function.getBody().getNumAddresses(),
                        block.role().label(),
                        line,
                        status);
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
}
