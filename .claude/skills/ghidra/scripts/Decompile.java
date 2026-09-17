// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.ClangTokenGroup;
import ghidra.app.decompiler.ClangTypeToken;
import ghidra.app.decompiler.ClangVariableToken;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.database.data.DataTypeUtilities;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressFactory;
import ghidra.program.model.data.Composite;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.DataTypeWriter;
import ghidra.program.model.data.Enum;
import ghidra.program.model.data.TypeDef;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.pcode.HighFunction;
import ghidra.program.model.pcode.HighFunctionDBUtil;
import ghidra.program.model.pcode.HighLocal;
import ghidra.program.model.pcode.HighSymbol;
import ghidra.program.model.pcode.PcodeOp;
import ghidra.program.model.pcode.Varnode;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.io.StringWriter;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.EnumSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
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
    record Global(Address address, String name, DataType type) {}

    sealed interface Block permits Decompiled, Failed, Stub {
        Function function();

        Stream<String> lines(List<Function> callers);
    }

    record Decompiled(
            Function function, String role, String code, List<DataType> types, List<Global> globals)
            implements Block {
        @Override
        public Stream<String> lines(List<Function> callers) {
            return Stream.concat(
                    Stream.of(Report.item(function.getName(true)), header(function, role, callers)),
                    code.lines());
        }
    }

    record Failed(Function function, String role, String cause) implements Block {
        @Override
        public Stream<String> lines(List<Function> callers) {
            return Stream.of(
                    Report.item(function.getName(true)),
                    header(function, role, callers),
                    "// failed: " + cause);
        }
    }

    record Stub(Function function) implements Block {
        @Override
        public Stream<String> lines(List<Function> callers) {
            return Stream.of(
                    Report.item(function.getName(true)),
                    "// %s %s -> %s"
                            .formatted(
                                    function.getEntryPoint(),
                                    function.isThunk() ? "thunk" : "stub",
                                    Functions.target(function).getName(true)));
        }
    }

    @Override
    public void run() throws Exception {
        Arguments.Request request =
                Arguments.parse(
                        getScriptName(),
                        getScriptArgs(),
                        EnumSet.allOf(Arguments.Setting.class),
                        currentProgram);
        SequencedMap<Function, String> selection = select(request.seeds(), request.settings());
        println(
                write(
                        request.out(),
                        decompile(selection, request.settings()),
                        request.arguments()));
    }

    // --- [SELECTION] -------------------------------------------------------------------

    private SequencedMap<Function, String> select(
            List<Function> seeds, Map<Arguments.Setting, Integer> settings) {
        SequencedMap<Function, String> seeded =
                seeds.stream()
                        .collect(
                                Collectors.toMap(
                                        seed -> seed,
                                        seed -> "seed",
                                        (first, _) -> first,
                                        LinkedHashMap::new));
        SequencedMap<Function, String> callees =
                expand(
                        seeded,
                        seeds,
                        Functions::callees,
                        "callee",
                        1,
                        settings.get(Arguments.Setting.CALLEES));
        return expand(
                callees,
                seeds,
                Functions::callers,
                "caller",
                1,
                settings.get(Arguments.Setting.CALLERS));
    }

    private SequencedMap<Function, String> expand(
            SequencedMap<Function, String> selection,
            List<Function> frontier,
            BiFunction<Function, TaskMonitor, Stream<Function>> neighbors,
            String role,
            int depth,
            int limit) {
        if (depth > limit) {
            return selection;
        }
        List<Function> next =
                frontier.stream()
                        .flatMap(function -> neighbors.apply(function, monitor))
                        .filter(Decompile::isDecompilable)
                        .filter(Predicate.not(selection::containsKey))
                        .distinct()
                        .sorted(Functions.BY_ENTRY)
                        .toList();
        SequencedMap<Function, String> expanded = new LinkedHashMap<>(selection);
        next.forEach(function -> expanded.put(function, role + ":" + depth));
        return expand(expanded, next, neighbors, role, depth + 1, limit);
    }

    private static boolean isDecompilable(Function function) {
        return !function.isThunk() && !function.isExternal() && !Functions.isStub(function);
    }

    // --- [DECOMPILE] -------------------------------------------------------------------

    private List<Block> decompile(
            SequencedMap<Function, String> selection, Map<Arguments.Setting, Integer> settings)
            throws Exception {
        List<Function> targets =
                selection.keySet().stream().filter(Decompile::isDecompilable).toList();
        SequencedMap<Function, Arguments.Result<DecompileResults>> results =
                Functions.decompile(currentProgram, targets, settings, monitor);
        return selection.entrySet().stream()
                .map(
                        entry ->
                                isDecompilable(entry.getKey())
                                        ? block(entry.getKey(), entry.getValue(), results)
                                        : new Stub(entry.getKey()))
                .toList();
    }

    private Block block(
            Function function,
            String role,
            SequencedMap<Function, Arguments.Result<DecompileResults>> results) {
        return switch (results.get(function)) {
            case Arguments.Success<DecompileResults>(DecompileResults decompiled) ->
                    new Decompiled(
                            function,
                            role,
                            decompiled.getDecompiledFunction().getC().strip(),
                            types(decompiled.getCCodeMarkup()),
                            globals(decompiled));
            case Arguments.Failure<DecompileResults>(String cause) ->
                    new Failed(function, role, cause);
        };
    }

    private static String header(Function function, String role, List<Function> callers) {
        String names =
                callers.stream()
                        .map(caller -> caller.getName(true))
                        .collect(Collectors.joining(", "));
        return "// %s size=%d %s callers=%d%s"
                .formatted(
                        function.getEntryPoint(),
                        function.getBody().getNumAddresses(),
                        role,
                        callers.size(),
                        names.isEmpty() ? "" : ": " + names);
    }

    private static List<DataType> types(ClangTokenGroup markup) {
        return CollectionUtils.asStream(markup.tokenIterator(true))
                .filter(ClangTypeToken.class::isInstance)
                .map(ClangTypeToken.class::cast)
                .map(ClangTypeToken::getDataType)
                .map(DataTypeUtilities::getBaseDataType)
                .filter(Decompile::isDeclared)
                .distinct()
                .toList();
    }

    private static boolean isDeclared(DataType type) {
        return switch (type) {
            case Composite _, Enum _, TypeDef _ -> true;
            case null, default -> false;
        };
    }

    private List<Global> globals(DecompileResults results) {
        HighFunction highFunction = results.getHighFunction();
        return CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                .filter(ClangVariableToken.class::isInstance)
                .map(ClangVariableToken.class::cast)
                .flatMap(token -> global(token, highFunction).stream())
                .distinct()
                .toList();
    }

    private Optional<Global> global(ClangVariableToken token, HighFunction highFunction) {
        AddressFactory factory = currentProgram.getAddressFactory();
        Stream<Address> varnodeAddress =
                Stream.ofNullable(token.getVarnode()).map(Varnode::getAddress);
        Stream<Address> referenceAddress =
                Stream.ofNullable(token.getPcodeOp())
                        .filter(op -> op.getOpcode() == PcodeOp.PTRSUB)
                        .filter(_ -> !(token.getHighVariable() instanceof HighLocal))
                        .map(op -> HighFunctionDBUtil.getSpacebaseReferenceAddress(factory, op));
        return Stream.concat(varnodeAddress, referenceAddress)
                .filter(Objects::nonNull)
                .filter(Address::isMemoryAddress)
                .findFirst()
                .map(
                        address ->
                                new Global(
                                        address,
                                        token.getText(),
                                        type(token, highFunction, address)));
    }

    private DataType type(ClangVariableToken token, HighFunction highFunction, Address address) {
        return switch (token.getHighSymbol(highFunction)) {
            case HighSymbol symbol -> symbol.getDataType();
            case null ->
                    Optional.ofNullable(getDataAt(address))
                            .filter(Data::isDefined)
                            .map(Data::getDataType)
                            .orElse(DataType.DEFAULT);
        };
    }

    // --- [WRITE] -----------------------------------------------------------------------

    private String write(Path out, List<Block> blocks, String arguments) throws Exception {
        Set<Function> members = blocks.stream().map(Block::function).collect(Collectors.toSet());
        Map<Function, List<Function>> callers =
                blocks.stream()
                        .map(Block::function)
                        .collect(
                                Collectors.toMap(
                                        function -> function,
                                        function ->
                                                Functions.callers(function, monitor)
                                                        .filter(members::contains)
                                                        .sorted(Functions.BY_ENTRY)
                                                        .toList()));
        long failed = blocks.stream().filter(Failed.class::isInstance).count();
        SortedSet<Global> globals =
                blocks.stream()
                        .filter(Decompiled.class::isInstance)
                        .map(Decompiled.class::cast)
                        .flatMap(block -> block.globals().stream())
                        .collect(
                                Collectors.toCollection(
                                        () ->
                                                new TreeSet<>(
                                                        Comparator.comparing(Global::address))));
        SortedSet<DataType> types =
                blocks.stream()
                        .filter(Decompiled.class::isInstance)
                        .map(Decompiled.class::cast)
                        .flatMap(block -> block.types().stream())
                        .collect(
                                Collectors.toCollection(
                                        () ->
                                                new TreeSet<>(
                                                        Comparator.comparing(
                                                                DataType::getPathName))));
        StringWriter declarations = new StringWriter();
        new DataTypeWriter(currentProgram.getDataTypeManager(), declarations, false)
                .write(List.copyOf(types), monitor);
        String counts =
                "functions=%d failed=%d globals=%d types=%d args=%s"
                        .formatted(blocks.size(), failed, globals.size(), types.size(), arguments);
        Stream<String> body =
                Stream.of(
                                blocks.stream()
                                        .flatMap(
                                                block ->
                                                        block.lines(callers.get(block.function()))),
                                Stream.of(Report.divider("GLOBALS")),
                                globals.stream().map(this::row),
                                Stream.of(Report.divider("TYPES")),
                                declarations.toString().lines())
                        .flatMap(section -> section);
        return Report.write(out, currentProgram, counts, body);
    }

    private String row(Global global) {
        String value =
                Optional.ofNullable(getDataAt(global.address()))
                        .filter(Data::isDefined)
                        .map(Data::getDefaultValueRepresentation)
                        .filter(Predicate.not(String::isEmpty))
                        .map(" = "::concat)
                        .orElse("");
        return "// %s %s %s%s"
                .formatted(global.address(), global.type().getDisplayName(), global.name(), value);
    }
}
