// Lists every call site of seed functions with the argument text the decompiler resolved there

// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.ClangFuncNameToken;
import ghidra.app.decompiler.ClangSyntaxToken;
import ghidra.app.decompiler.ClangToken;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.decompiler.parallel.DecompilerCallback;
import ghidra.app.decompiler.parallel.ParallelDecompiler;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.bin.format.objc.objc2.Objc2Constants;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.pcode.PcodeOp;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Comparator;
import java.util.EnumSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.Set;
import java.util.function.Predicate;
import java.util.stream.Collector;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class CallSites extends GhidraScript {
    private static final Set<Job.Setting> SETTINGS =
            EnumSet.of(Job.Setting.TIMEOUT, Job.Setting.PAYLOAD);

    // A function a call names to reach a target: the target, a thunk of it, or an objc stub
    // binding a selector to it, whose prefix is the linker's name suffix $<selector>
    record Alias(Function function, Function target, String prefix) {
        // Thunks and stubs call on behalf of their own callers
        Stream<Function> callers(TaskMonitor monitor) {
            return function.getCallingFunctions(monitor).stream()
                    .filter(caller -> !caller.isThunk() && !stub(caller));
        }
    }

    record Call(Alias alias, Function caller, Address address, String arguments) {
        // A call's name token carries the call op, whose first input is the named function
        Call(Map<Address, Alias> aliases, Function caller, ClangToken name) {
            this(
                    aliases.get(name.getPcodeOp().getInput(0).getAddress()),
                    caller,
                    name.getPcodeOp().getSeqnum().getTarget(),
                    CallSites.arguments(name));
        }

        String row() {
            return "%s %s: %s%s"
                    .formatted(address, caller.getName(true), alias.prefix(), arguments);
        }
    }

    @Override
    public void run() throws Exception {
        Job.Request request = Job.parse(getScriptName(), getScriptArgs(), SETTINGS, currentProgram);
        List<Alias> aliases =
                request.seeds().stream()
                        .map(seed -> seed.isThunk() ? seed.getThunkedFunction(true) : seed)
                        .distinct()
                        .flatMap(this::aliases)
                        .toList();
        Map<Address, Alias> byEntry =
                aliases.stream()
                        .collect(
                                Collectors.toMap(
                                        alias -> alias.function().getEntryPoint(), alias -> alias));
        Collector<Alias, ?, Set<Function>> union =
                Collectors.flatMapping(alias -> alias.callers(monitor), Collectors.toSet());
        SequencedMap<Function, Set<Function>> plan =
                aliases.stream()
                        .collect(Collectors.groupingBy(Alias::target, LinkedHashMap::new, union));
        List<Function> callers = plan.values().stream().flatMap(Set::stream).distinct().toList();
        List<Job.Result<List<Call>>> results = decompile(callers, byEntry, request.settings());
        println(write(request.out(), plan, results, request.text()));
    }

    // --- [ALIASES] ---------------------------------------------------------------------

    private Stream<Alias> aliases(Function target) {
        Stream<Address> thunks =
                Stream.ofNullable(target.getFunctionThunkAddresses(true)).flatMap(Arrays::stream);
        List<Function> plain =
                Stream.concat(Stream.of(target), thunks.map(this::getFunctionAt)).toList();
        Stream<Function> stubs =
                plain.stream()
                        .flatMap(function -> function.getCallingFunctions(monitor).stream())
                        .filter(CallSites::stub);
        Stream<Alias> bound =
                stubs.map(
                        stub ->
                                new Alias(
                                        stub,
                                        target,
                                        selector(stub)
                                                .map(name -> "$" + name + " ")
                                                .orElse("$? ")));
        return Stream.concat(
                plain.stream().map(function -> new Alias(function, target, "")), bound);
    }

    private static boolean stub(Function function) {
        Address entry = function.getEntryPoint();
        return Objc2Constants.OBJC2_STUBS.equals(
                function.getProgram().getMemory().getBlock(entry).getName());
    }

    // The selector a stub loads, the string Ghidra resolved for the load's parameter reference
    private Optional<String> selector(Function stub) {
        return CollectionUtils.asStream(stub.getBody().getAddresses(true))
                .map(this::getReferencesFrom)
                .flatMap(Arrays::stream)
                .map(reference -> getDataAt(reference.getToAddress()))
                .filter(Objects::nonNull)
                .map(Data::getValue)
                .filter(String.class::isInstance)
                .map(String.class::cast)
                .findFirst();
    }

    // --- [DECOMPILE] -------------------------------------------------------------------

    private List<Job.Result<List<Call>>> decompile(
            List<Function> callers, Map<Address, Alias> aliases, Map<Job.Setting, Integer> settings)
            throws Exception {
        int timeout = settings.get(Job.Setting.TIMEOUT);
        DecompileOptions options = new DecompileOptions();
        options.grabFromProgram(currentProgram);
        options.setDefaultTimeout(timeout);
        options.setMaxPayloadMBytes(settings.get(Job.Setting.PAYLOAD));
        // Widest line the decompiler accepts, a wrapped argument list would lose its break spaces
        options.setMaxWidth(10_000);
        DecompilerCallback<Job.Result<List<Call>>> callback =
                new DecompilerCallback<>(
                        currentProgram, decompiler -> decompiler.setOptions(options)) {
                    @Override
                    public Job.Result<List<Call>> process(
                            DecompileResults results, TaskMonitor taskMonitor) {
                        return calls(results, aliases, timeout);
                    }
                };
        callback.setTimeout(timeout);
        try {
            List<Job.Result<List<Call>>> results =
                    ParallelDecompiler.decompileFunctions(callback, callers, monitor);
            // A cancelled item yields null, every other item its result or the exception it threw
            monitor.checkCancelled();
            return results;
        } finally {
            callback.dispose();
        }
    }

    private static Job.Result<List<Call>> calls(
            DecompileResults results, Map<Address, Alias> aliases, int timeout) {
        Function caller = results.getFunction();
        if (!results.decompileCompleted()) {
            String cause =
                    results.isTimedOut()
                            ? "timed out after " + timeout + " s"
                            : results.getErrorMessage().strip();
            return new Job.Problem<>(
                    "%s @ %s: %s".formatted(caller.getName(true), caller.getEntryPoint(), cause));
        }
        Predicate<ClangToken> aliased =
                token ->
                        token instanceof ClangFuncNameToken
                                && token.getPcodeOp() instanceof PcodeOp op
                                && aliases.containsKey(op.getInput(0).getAddress());
        return new Job.Ok<>(
                CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                        .filter(aliased)
                        .map(name -> new Call(aliases, caller, name))
                        .toList());
    }

    // Text inside the parentheses after a call's name, paired by the printer's own pair ids
    private static String arguments(ClangToken name) {
        ClangSyntaxToken open =
                CollectionUtils.asStream(name.iterator(true))
                        .filter(ClangSyntaxToken.class::isInstance)
                        .map(ClangSyntaxToken.class::cast)
                        .filter(syntax -> syntax.getOpen() >= 0)
                        .findFirst()
                        .orElseThrow();
        int pair = open.getOpen();
        Predicate<ClangToken> closes =
                token -> token instanceof ClangSyntaxToken syntax && syntax.getClose() == pair;
        return CollectionUtils.asStream(open.iterator(true))
                .skip(1)
                .takeWhile(Predicate.not(closes))
                .map(ClangToken::getText)
                .collect(Collectors.joining());
    }

    // --- [WRITE] -----------------------------------------------------------------------

    private String write(
            Path out,
            SequencedMap<Function, Set<Function>> plan,
            List<Job.Result<List<Call>>> results,
            String request)
            throws IOException {
        List<String> failed = results.stream().flatMap(Job.Result::problems).toList();
        Comparator<Call> order =
                Comparator.comparing((Call call) -> call.caller().getEntryPoint())
                        .thenComparing(Call::address);
        Map<Function, List<Call>> calls =
                results.stream()
                        .flatMap(Job.Result::values)
                        .flatMap(List::stream)
                        .sorted(order)
                        .collect(Collectors.groupingBy(call -> call.alias().target()));
        long callers = plan.values().stream().flatMap(Set::stream).distinct().count();
        long total = calls.values().stream().mapToLong(List::size).sum();
        String facts =
                "targets=%d callers=%d calls=%d failed=%d args=%s"
                        .formatted(plan.size(), callers, total, failed.size(), request);
        Stream<String> sections =
                plan.entrySet().stream()
                        .flatMap(
                                entry ->
                                        section(
                                                entry.getKey(),
                                                entry.getValue(),
                                                calls.getOrDefault(entry.getKey(), List.of())));
        List<String> lines =
                Stream.of(
                                Job.index(currentProgram, facts),
                                sections,
                                Stream.of(Job.divider("FAILED")),
                                failed.stream().map("// "::concat))
                        .flatMap(section -> section)
                        .toList();
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, lines);
        return "wrote %s: %d targets, %d callers, %d calls, %d failed"
                .formatted(out, plan.size(), callers, total, failed.size());
    }

    private static Stream<String> section(
            Function target, Set<Function> callers, List<Call> calls) {
        String facts =
                "// %s @ %s callers=%d calls=%d"
                        .formatted(
                                target.getName(true),
                                target.getEntryPoint(),
                                callers.size(),
                                calls.size());
        return Stream.concat(
                Stream.of(Job.item(target.getName()), facts), calls.stream().map(Call::row));
    }
}
