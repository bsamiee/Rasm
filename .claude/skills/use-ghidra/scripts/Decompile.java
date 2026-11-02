import ghidra.app.decompiler.ClangTypeToken;
import ghidra.app.decompiler.ClangVariableToken;
import ghidra.app.decompiler.DecompileResults;
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
import ghidra.program.model.listing.Listing;
import ghidra.program.model.listing.Program;
import ghidra.program.model.pcode.HighFunction;
import ghidra.program.model.pcode.HighFunctionDBUtil;
import ghidra.program.model.pcode.HighLocal;
import ghidra.program.model.pcode.HighSymbol;
import ghidra.program.model.pcode.PcodeOp;
import ghidra.program.model.pcode.Varnode;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import util.CollectionUtils;

import java.io.StringWriter;
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
import java.util.concurrent.ConcurrentHashMap;
import java.util.function.IntFunction;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Decompile extends GhidraScript {
    // --- [CACHE]

    private final Map<Function, List<Function>> callers = new ConcurrentHashMap<>();

    // --- [ROLES]

    @FunctionalInterface
    interface Neighbors {
        List<Function> of(Function function);
    }

    sealed interface Role permits Seed, Caller, Callee {
        default String text() {
            return switch (this) {
                case Seed _ -> "seed";
                case Caller(int depth) -> "caller:" + depth;
                case Callee(int depth) -> "callee:" + depth;
            };
        }
    }

    record Seed() implements Role {}

    record Caller(int depth) implements Role {}

    record Callee(int depth) implements Role {}

    // --- [BLOCKS]

    record Global(Address address, String name, DataType type) {
        static Optional<Global> of(ClangVariableToken token, HighFunction highFunction) {
            Program program = highFunction.getFunction().getProgram();
            return Stream.concat(
                            Stream.ofNullable(token.getVarnode()).map(Varnode::getAddress),
                            Stream.ofNullable(token.getPcodeOp())
                                    .filter(op -> op.getOpcode() == PcodeOp.PTRSUB)
                                    .filter(_ -> !(token.getHighVariable() instanceof HighLocal))
                                    .map(op ->
                                            HighFunctionDBUtil.getSpacebaseReferenceAddress(
                                                    program.getAddressFactory(), op)))
                    .filter(Objects::nonNull)
                    .filter(Address::isMemoryAddress)
                    .findFirst()
                    .map(address ->
                            new Global(
                                    address,
                                    token.getText(),
                                    Optional.ofNullable(token.getHighSymbol(highFunction))
                                            .map(HighSymbol::getDataType)
                                            .or(() ->
                                                    Optional.ofNullable(
                                                                    program.getListing()
                                                                            .getDefinedDataAt(
                                                                                    address))
                                                            .map(Data::getDataType))
                                            .orElse(DataType.DEFAULT)));
        }

        String row(Listing listing) {
            return "// %s %s %s%s"
                    .formatted(
                            address,
                            type.getDisplayName(),
                            name,
                            Optional.ofNullable(listing.getDefinedDataAt(address))
                                    .map(Data::getDefaultValueRepresentation)
                                    .filter(Predicate.not(String::isEmpty))
                                    .map(" = "::concat)
                                    .orElse(""));
        }
    }

    sealed interface Block permits Decompiled, Failed, Stub {
        static Block of(Function function, Role role, Report.Result<DecompileResults> result) {
            return switch (result) {
                case Report.Success<DecompileResults>(DecompileResults decompiled) ->
                        new Decompiled(function, role, decompiled);
                case Report.Failure<DecompileResults>(String cause) ->
                        new Failed(function, role, cause);
            };
        }

        Function function();

        default Stream<String> lines(Neighbors callers) {
            return switch (this) {
                case Decompiled(Function function, Role role, String code, _, _) ->
                        Stream.concat(
                                Stream.of(
                                        Report.subdivider(function.getName(true)),
                                        header(function, role, callers.of(function))),
                                code.lines());
                case Stub(Function function) ->
                        Stream.of(
                                Report.subdivider(function.getName(true)),
                                "// %s %s -> %s"
                                        .formatted(
                                                function.getEntryPoint(),
                                                function.isThunk() ? "thunk" : "stub",
                                                Functions.target(function).getName(true)));
                case Failed _ -> Stream.empty();
            };
        }
    }

    record Decompiled(
            Function function, Role role, String code, List<DataType> types, List<Global> globals)
            implements Block {
        Decompiled(Function function, Role role, DecompileResults results) {
            this(
                    function,
                    role,
                    results.getDecompiledFunction().getC().strip(),
                    CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                            .filter(ClangTypeToken.class::isInstance)
                            .map(ClangTypeToken.class::cast)
                            .map(ClangTypeToken::getDataType)
                            .map(DataTypeUtilities::getBaseDataType)
                            .filter(type ->
                                    type instanceof Composite
                                            || type instanceof Enum
                                            || type instanceof TypeDef)
                            .distinct()
                            .toList(),
                    CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                            .filter(ClangVariableToken.class::isInstance)
                            .map(ClangVariableToken.class::cast)
                            .flatMap(token -> Global.of(token, results.getHighFunction()).stream())
                            .distinct()
                            .toList());
        }
    }

    record Failed(Function function, Role role, String cause) implements Block {
        String row() {
            return "// %s %s %s: %s"
                    .formatted(
                            function.getEntryPoint(), function.getName(true), role.text(), cause);
        }
    }

    record Stub(Function function) implements Block {}

    private static String header(Function function, Role role, List<Function> callers) {
        String names =
                callers.stream()
                        .map(caller -> caller.getName(true))
                        .collect(Collectors.joining(", "));
        return "// %s size=%d %s callers=%d%s"
                .formatted(
                        function.getEntryPoint(),
                        function.getBody().getNumAddresses(),
                        role.text(),
                        callers.size(),
                        names.isEmpty() ? "" : ": " + names);
    }

    // --- [SELECTION]

    private List<Function> callersOf(Function function) {
        return callers.computeIfAbsent(function, each -> Functions.callers(each, monitor).toList());
    }

    private static SequencedMap<Function, Role> expand(
            SequencedMap<Function, Role> selection,
            List<Function> frontier,
            Neighbors neighbors,
            IntFunction<Role> role,
            int depth,
            int limit) {
        if (depth > limit) {
            return selection;
        }
        List<Function> next =
                frontier.stream()
                        .flatMap(function -> neighbors.of(function).stream())
                        .filter(Decompile::isDecompilable)
                        .filter(Predicate.not(selection::containsKey))
                        .distinct()
                        .sorted(Functions.BY_ENTRY)
                        .toList();
        SequencedMap<Function, Role> expanded = new LinkedHashMap<>(selection);
        next.forEach(function -> expanded.put(function, role.apply(depth)));
        return expand(expanded, next, neighbors, role, depth + 1, limit);
    }

    private static boolean isDecompilable(Function function) {
        return !function.isThunk() && !function.isExternal() && !Functions.isStub(function);
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Arguments.Request request =
                Arguments.parse(
                        getScriptName(),
                        getScriptArgs(),
                        EnumSet.allOf(Arguments.Setting.class),
                        currentProgram);
        SequencedMap<Function, Role> seeded =
                request.seeds().stream()
                        .collect(
                                Collectors.toMap(
                                        seed -> seed,
                                        _ -> new Seed(),
                                        (first, _) -> first,
                                        LinkedHashMap::new));
        SequencedMap<Function, Role> selection =
                expand(
                        expand(
                                seeded,
                                request.seeds(),
                                function -> Functions.callees(function, monitor).toList(),
                                Callee::new,
                                1,
                                request.settings().get(Arguments.Setting.CALLEES)),
                        request.seeds(),
                        this::callersOf,
                        Caller::new,
                        1,
                        request.settings().get(Arguments.Setting.CALLERS));
        SequencedMap<Function, Report.Result<DecompileResults>> results =
                Functions.decompile(
                        currentProgram,
                        selection.keySet().stream().filter(Decompile::isDecompilable).toList(),
                        request.settings(),
                        monitor);
        List<Block> blocks =
                selection.entrySet().stream()
                        .map(entry ->
                                isDecompilable(entry.getKey())
                                        ? Block.of(
                                                entry.getKey(),
                                                entry.getValue(),
                                                results.get(entry.getKey()))
                                        : new Stub(entry.getKey()))
                        .toList();
        Set<Function> members =
                blocks.stream()
                        .filter(Predicate.not(Failed.class::isInstance))
                        .map(Block::function)
                        .collect(Collectors.toSet());
        List<Decompiled> decompiled =
                blocks.stream()
                        .filter(Decompiled.class::isInstance)
                        .map(Decompiled.class::cast)
                        .toList();
        SortedSet<Global> globals =
                decompiled.stream()
                        .flatMap(block -> block.globals().stream())
                        .collect(
                                Collectors.toCollection(() ->
                                        new TreeSet<>(Comparator.comparing(Global::address))));
        SortedSet<DataType> types =
                decompiled.stream()
                        .flatMap(block -> block.types().stream())
                        .collect(
                                Collectors.toCollection(() ->
                                        new TreeSet<>(
                                                Comparator.comparing(DataType::getPathName))));
        StringWriter declarations = new StringWriter();
        new DataTypeWriter(currentProgram.getDataTypeManager(), declarations, false)
                .write(List.copyOf(types), monitor);
        List<Report.Section> sections =
                List.of(
                        new Report.Section(
                                "FUNCTIONS",
                                members.size(),
                                blocks.stream()
                                        .flatMap(block ->
                                                block.lines(function ->
                                                        callersOf(function).stream()
                                                                .filter(members::contains)
                                                                .sorted(Functions.BY_ENTRY)
                                                                .toList()))
                                        .toList()),
                        Report.Section.rows(
                                "FAILED",
                                blocks.stream()
                                        .filter(Failed.class::isInstance)
                                        .map(Failed.class::cast)
                                        .map(Failed::row)
                                        .toList()),
                        Report.Section.rows(
                                "GLOBALS",
                                globals.stream()
                                        .map(global -> global.row(currentProgram.getListing()))
                                        .toList()),
                        new Report.Section(
                                "TYPES", types.size(), declarations.toString().lines().toList()));
        println(Report.write(request.out(), currentProgram, sections, request.arguments()));
    }
}
