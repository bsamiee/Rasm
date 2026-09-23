// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.ClangBreak;
import ghidra.app.decompiler.ClangFuncNameToken;
import ghidra.app.decompiler.ClangSyntaxToken;
import ghidra.app.decompiler.ClangToken;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;

import util.CollectionUtils;

import java.io.IOException;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.EnumSet;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.Set;
import java.util.stream.Collector;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class CallSites extends GhidraScript {
    private static final Set<Arguments.Setting> SETTINGS =
            EnumSet.of(Arguments.Setting.TIMEOUT, Arguments.Setting.PAYLOAD);

    sealed interface Callee permits Direct, Stub {
        Function function();

        Function target();
    }

    record Direct(Function function, Function target) implements Callee {}

    record Stub(Function function, Function target, Optional<String> selector) implements Callee {}

    record Call(Callee callee, Function caller, Address address, List<String> arguments) {
        String row() {
            String prefix =
                    switch (callee) {
                        case Direct _ -> "";
                        case Stub(_, _, Optional<String> selector) ->
                                "$" + selector.orElse("?") + " ";
                    };
            return "%s %s: %s%s"
                    .formatted(address, caller.getName(true), prefix, String.join(",", arguments));
        }
    }

    @Override
    public void run() throws Exception {
        Arguments.Request request =
                Arguments.parse(getScriptName(), getScriptArgs(), SETTINGS, currentProgram);
        SequencedMap<Address, Callee> callees =
                request.seeds().stream()
                        .map(Functions::target)
                        .distinct()
                        .flatMap(this::reaching)
                        .collect(
                                Collectors.toMap(
                                        callee -> callee.function().getEntryPoint(),
                                        callee -> callee,
                                        (first, _) -> first,
                                        LinkedHashMap::new));
        Collector<Callee, ?, Set<Function>> union =
                Collectors.flatMapping(
                        callee -> Functions.callers(callee.function(), monitor),
                        Collectors.toCollection(LinkedHashSet::new));
        SequencedMap<Function, Set<Function>> callersByTarget =
                callees.values().stream()
                        .collect(Collectors.groupingBy(Callee::target, LinkedHashMap::new, union));
        List<Function> callers =
                callersByTarget.values().stream().flatMap(Set::stream).distinct().toList();
        List<Arguments.Result<Call>> calls =
                Functions.decompile(currentProgram, callers, request.settings(), monitor)
                        .entrySet()
                        .stream()
                        .flatMap(entry -> calls(entry.getKey(), entry.getValue(), callees))
                        .toList();
        println(write(request.out(), callersByTarget, callers.size(), calls, request.arguments()));
    }

    // --- [CALLEES] ---------------------------------------------------------------------

    private Stream<Callee> reaching(Function target) {
        List<Function> direct = Stream.concat(Stream.of(target), Functions.thunks(target)).toList();
        Stream<Callee> stubs =
                direct.stream()
                        .flatMap(function -> function.getCallingFunctions(monitor).stream())
                        .filter(Functions::isStub)
                        .distinct()
                        .map(stub -> new Stub(stub, target, Functions.selector(stub)));
        return Stream.concat(direct.stream().map(function -> new Direct(function, target)), stubs);
    }

    // --- [CALLS] -----------------------------------------------------------------------

    private static Stream<Arguments.Result<Call>> calls(
            Function caller,
            Arguments.Result<DecompileResults> decompiled,
            Map<Address, Callee> callees) {
        return switch (decompiled) {
            case Arguments.Success<DecompileResults>(DecompileResults results) ->
                    CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                            .filter(ClangFuncNameToken.class::isInstance)
                            .map(ClangFuncNameToken.class::cast)
                            .flatMap(
                                    name ->
                                            Optional.ofNullable(name.getPcodeOp())
                                                    .map(
                                                            op ->
                                                                    callees.get(
                                                                            op.getInput(0)
                                                                                    .getAddress()))
                                                    .map(callee -> call(callee, caller, name))
                                                    .stream());
            case Arguments.Failure<DecompileResults>(String cause) ->
                    Stream.of(
                            new Arguments.Failure<>(
                                    "%s @ %s: %s"
                                            .formatted(
                                                    caller.getName(true),
                                                    caller.getEntryPoint(),
                                                    cause)));
        };
    }

    private static Arguments.Result<Call> call(
            Callee callee, Function caller, ClangFuncNameToken name) {
        Address address = name.getPcodeOp().getSeqnum().getTarget();
        return arguments(name)
                .<Arguments.Result<Call>>map(
                        arguments ->
                                new Arguments.Success<>(
                                        new Call(
                                                callee,
                                                caller,
                                                address,
                                                switch (callee) {
                                                    case Direct _ -> arguments;
                                                    case Stub _ ->
                                                            withoutSelectorRegister(arguments);
                                                })))
                .orElseGet(
                        () ->
                                new Arguments.Failure<>(
                                        "%s @ %s: %s call without an argument list"
                                                .formatted(
                                                        caller.getName(true),
                                                        address,
                                                        callee.function().getName(true))));
    }

    private static List<String> withoutSelectorRegister(List<String> arguments) {
        return Stream.concat(arguments.stream().limit(1), arguments.stream().skip(2)).toList();
    }

    private static Optional<List<String>> arguments(ClangToken name) {
        return CollectionUtils.asStream(name.iterator(true))
                .filter(ClangSyntaxToken.class::isInstance)
                .map(ClangSyntaxToken.class::cast)
                .filter(open -> open.getOpen() >= 0)
                .findFirst()
                .map(
                        open ->
                                split(
                                        CollectionUtils.asStream(open.iterator(true))
                                                .skip(1)
                                                .takeWhile(token -> !closes(token, open.getOpen()))
                                                .toList()));
    }

    private static boolean closes(ClangToken token, int pair) {
        return token instanceof ClangSyntaxToken syntax && syntax.getClose() == pair;
    }

    private static List<String> split(List<ClangToken> tokens) {
        IntStream commas =
                IntStream.range(0, tokens.size())
                        .filter(index -> ",".equals(tokens.get(index).getText()))
                        .filter(
                                index ->
                                        IntStream.range(0, index)
                                                        .map(before -> nesting(tokens.get(before)))
                                                        .sum()
                                                == 0);
        List<Integer> cuts =
                IntStream.concat(
                                IntStream.concat(IntStream.of(-1), commas),
                                IntStream.of(tokens.size()))
                        .boxed()
                        .toList();
        return IntStream.range(1, cuts.size())
                .mapToObj(cut -> tokens.subList(cuts.get(cut - 1) + 1, cuts.get(cut)))
                .map(part -> part.stream().map(CallSites::text).collect(Collectors.joining()))
                .toList();
    }

    private static int nesting(ClangToken token) {
        return token instanceof ClangSyntaxToken syntax
                ? (syntax.getOpen() >= 0 ? 1 : 0) - (syntax.getClose() >= 0 ? 1 : 0)
                : 0;
    }

    private static String text(ClangToken token) {
        return token instanceof ClangBreak ? " " : token.getText();
    }

    // --- [WRITE] -----------------------------------------------------------------------

    private String write(
            Path out,
            SequencedMap<Function, Set<Function>> callersByTarget,
            int callers,
            List<Arguments.Result<Call>> results,
            String arguments)
            throws IOException {
        List<String> failed = results.stream().flatMap(Arguments.Result::failures).toList();
        Comparator<Call> order =
                Comparator.comparing((Call call) -> call.caller().getEntryPoint())
                        .thenComparing(Call::address);
        Map<Function, List<Call>> calls =
                results.stream()
                        .flatMap(Arguments.Result::values)
                        .sorted(order)
                        .collect(Collectors.groupingBy(call -> call.callee().target()));
        long total = calls.values().stream().mapToLong(List::size).sum();
        String counts =
                "targets=%d callers=%d calls=%d failed=%d args=%s"
                        .formatted(
                                callersByTarget.size(), callers, total, failed.size(), arguments);
        Stream<String> sections =
                callersByTarget.entrySet().stream()
                        .flatMap(
                                entry ->
                                        section(
                                                entry.getKey(),
                                                entry.getValue(),
                                                calls.getOrDefault(entry.getKey(), List.of())));
        Stream<String> body =
                Stream.of(
                                sections,
                                Stream.of(Report.divider("FAILED")),
                                failed.stream().map("// "::concat))
                        .flatMap(section -> section);
        return Report.write(out, currentProgram, counts, body);
    }

    private static Stream<String> section(
            Function target, Set<Function> callers, List<Call> calls) {
        String header =
                "// %s @ %s callers=%d calls=%d"
                        .formatted(
                                target.getName(true),
                                target.getEntryPoint(),
                                callers.size(),
                                calls.size());
        return Stream.concat(
                Stream.of(Report.item(target.getName(true)), header),
                calls.stream().map(Call::row));
    }
}
