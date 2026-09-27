import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.util.bin.format.objc.objc2.Objc2Constants;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.ReferenceManager;
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

// --- [OPERATIONS] ----------------------------------------------------------------------

final class Functions {
    // --- [GRAPH]

    static final Comparator<Function> BY_ENTRY = Comparator.comparing(Function::getEntryPoint);

    private Functions() {}

    static Stream<Function> internal(Program program) {
        return CollectionUtils.asStream(program.getFunctionManager().getFunctions(true));
    }

    static boolean isStub(Function function) {
        MemoryBlock block = function.getProgram().getMemory().getBlock(function.getEntryPoint());
        return Stream.ofNullable(block)
                .map(MemoryBlock::getName)
                .anyMatch(Objc2Constants.OBJC2_STUBS::equals);
    }

    static Function thunked(Function function) {
        return function.isThunk() ? function.getThunkedFunction(true) : function;
    }

    static Function target(Function function) {
        return isStub(function)
                ? function.getCalledFunctions(TaskMonitor.DUMMY).stream()
                        .min(BY_ENTRY)
                        .map(Functions::target)
                        .orElse(function)
                : thunked(function);
    }

    static Stream<Function> thunks(Function function) {
        return Stream.ofNullable(function.getFunctionThunkAddresses(true))
                .flatMap(Arrays::stream)
                .map(function.getProgram().getFunctionManager()::getFunctionAt)
                .filter(Objects::nonNull);
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

    static Stream<Function> callees(Function function, TaskMonitor monitor) {
        return function.getCalledFunctions(monitor).stream().map(Functions::target).distinct();
    }

    // --- [DATA]

    static Optional<String> selector(Function stub) {
        Program program = stub.getProgram();
        ReferenceManager references = program.getReferenceManager();
        return CollectionUtils.asStream(references.getReferenceSourceIterator(stub.getBody(), true))
                .map(references::getReferencesFrom)
                .flatMap(Arrays::stream)
                .map(Reference::getToAddress)
                .map(program.getListing()::getDataAt)
                .filter(Objects::nonNull)
                .filter(StringDataInstance::isString)
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

    // --- [DECOMPILE]

    static SequencedMap<Function, Report.Result<DecompileResults>> decompile(
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
                                            DecompileResults::getFunction, results -> results));
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

    private static Report.Result<DecompileResults> result(DecompileResults results, int timeout) {
        return results.decompileCompleted()
                ? new Report.Success<>(results)
                : new Report.Failure<>(
                        results.isTimedOut()
                                ? "Decompile timed out after %d s".formatted(timeout)
                                : results.getErrorMessage().strip());
    }
}
