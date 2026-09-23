// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.util.bin.format.objc.objc2.Objc2Constants;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.Reference;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.util.Arrays;
import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [FUNCTIONS] -----------------------------------------------------------------------

final class Functions {
    static final Comparator<Function> BY_ENTRY = Comparator.comparing(Function::getEntryPoint);

    private Functions() {}

    // --- [GRAPH] -----------------------------------------------------------------------

    static Function target(Function function) {
        Optional<Function> reached =
                function.isThunk()
                        ? Optional.ofNullable(function.getThunkedFunction(true))
                        : isStub(function)
                                ? callee(function).map(Functions::target)
                                : Optional.empty();
        return reached.orElse(function);
    }

    private static Optional<Function> callee(Function stub) {
        return stub.getCalledFunctions(TaskMonitor.DUMMY).stream().min(BY_ENTRY);
    }

    static boolean isStub(Function function) {
        MemoryBlock block = function.getProgram().getMemory().getBlock(function.getEntryPoint());
        return Stream.ofNullable(block)
                .map(MemoryBlock::getName)
                .anyMatch(Objc2Constants.OBJC2_STUBS::equals);
    }

    static Stream<Function> callers(Function function, TaskMonitor monitor) {
        return Stream.concat(Stream.of(function), thunks(function))
                .flatMap(each -> each.getCallingFunctions(monitor).stream())
                .flatMap(
                        caller ->
                                caller.isThunk() || isStub(caller)
                                        ? callers(caller, monitor)
                                        : Stream.of(caller))
                .distinct();
    }

    static Stream<Function> thunks(Function function) {
        return Stream.ofNullable(function.getFunctionThunkAddresses(true))
                .flatMap(Arrays::stream)
                .map(function.getProgram().getFunctionManager()::getFunctionAt)
                .filter(Objects::nonNull);
    }

    static Stream<Function> callees(Function function, TaskMonitor monitor) {
        return function.getCalledFunctions(monitor).stream().map(Functions::target).distinct();
    }

    static Stream<Function> internal(Program program) {
        return CollectionUtils.asStream(program.getFunctionManager().getFunctions(true));
    }

    // --- [DATA] ------------------------------------------------------------------------

    static Optional<String> selector(Function stub) {
        Program program = stub.getProgram();
        return CollectionUtils.asStream(stub.getBody().getAddresses(true))
                .map(program.getReferenceManager()::getReferencesFrom)
                .flatMap(Arrays::stream)
                .map(Reference::getToAddress)
                .map(program.getListing()::getDataAt)
                .filter(Objects::nonNull)
                .map(Data::getValue)
                .filter(String.class::isInstance)
                .map(String.class::cast)
                .findFirst();
    }

    static Stream<Function> referrers(Data data) {
        Program program = data.getProgram();
        Stream<Data> structs =
                CollectionUtils.asStream(data.getReferenceIteratorTo())
                        .map(Reference::getFromAddress)
                        .map(program.getListing()::getDataContaining)
                        .filter(Objects::nonNull);
        return Stream.concat(Stream.of(data), structs)
                .map(Data::getReferenceIteratorTo)
                .flatMap(CollectionUtils::asStream)
                .map(Reference::getFromAddress)
                .map(program.getFunctionManager()::getFunctionContaining)
                .filter(Objects::nonNull)
                .distinct();
    }

    // --- [DECOMPILE] -------------------------------------------------------------------

    static SequencedMap<Function, Arguments.Result<DecompileResults>> decompile(
            Program program,
            List<Function> functions,
            Map<Arguments.Setting, Integer> settings,
            TaskMonitor monitor)
            throws Exception {
        int timeout = settings.get(Arguments.Setting.TIMEOUT);
        DecompileOptions options = new DecompileOptions();
        options.grabFromProgram(program);
        options.setMaxPayloadMBytes(settings.get(Arguments.Setting.PAYLOAD));
        DecompilerCallback<DecompileResults> callback =
                new DecompilerCallback<>(program, decompiler -> decompiler.setOptions(options)) {
                    @Override
                    public DecompileResults process(
                            DecompileResults results, TaskMonitor taskMonitor) {
                        return results;
                    }
                };
        callback.setTimeout(timeout);
        try {
            List<DecompileResults> completed =
                    ParallelDecompiler.decompileFunctions(callback, functions, monitor);
            monitor.checkCancelled();
            Map<Function, DecompileResults> byFunction =
                    completed.stream()
                            .collect(
                                    Collectors.toMap(
                                            DecompileResults::getFunction,
                                            results -> results,
                                            (first, _) -> first));
            return functions.stream()
                    .collect(
                            Collectors.toMap(
                                    function -> function,
                                    function -> result(byFunction.get(function), timeout),
                                    (first, _) -> first,
                                    LinkedHashMap::new));
        } finally {
            callback.dispose();
        }
    }

    private static Arguments.Result<DecompileResults> result(
            DecompileResults results, int timeout) {
        return results.decompileCompleted()
                ? new Arguments.Success<>(results)
                : new Arguments.Failure<>(
                        results.isTimedOut()
                                ? "timed out after " + timeout + " s"
                                : results.getErrorMessage().strip());
    }
}
