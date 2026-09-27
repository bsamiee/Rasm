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
import java.util.Arrays;
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

// --- [COMPOSITION] ---------------------------------------------------------------------

public class CallSites extends GhidraScript {
    // --- [CALLEES]

    sealed interface Callee permits Direct, Stub {
        Function function();

        Function target();
    }

    record Direct(Function function, Function target) implements Callee {}

    record Stub(Function function, Function target, Optional<String> selector) implements Callee {}

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

    // --- [CALLS]

    record Call(Callee callee, Function caller, Address address, List<String> arguments) {
        String row() {
            String selector =
                    switch (callee) {
                        case Direct _ -> "";
                        case Stub(_, _, Optional<String> name) -> "$" + name.orElse("?") + " ";
                    };
            return "%s %s: %s%s"
                    .formatted(
                            address, caller.getName(true), selector, String.join(",", arguments));
        }
    }

    private static Stream<Report.Result<Call>> calls(
            Function caller,
            Report.Result<DecompileResults> decompiled,
            Map<Address, List<Callee>> callees) {
        return switch (decompiled) {
            case Report.Success<DecompileResults>(DecompileResults results) ->
                    CollectionUtils.asStream(results.getCCodeMarkup().tokenIterator(true))
                            .filter(ClangFuncNameToken.class::isInstance)
                            .map(ClangFuncNameToken.class::cast)
                            .filter(name -> name.getPcodeOp() != null)
                            .flatMap(
                                    name ->
                                            callees
                                                    .getOrDefault(
                                                            name.getPcodeOp()
                                                                    .getInput(0)
                                                                    .getAddress(),
                                                            List.of())
                                                    .stream()
                                                    .map(callee -> call(callee, caller, name)));
            case Report.Failure<DecompileResults>(String cause) ->
                    Stream.of(
                            new Report.Failure<>(
                                    "%s at %s: %s"
                                            .formatted(
                                                    caller.getName(true),
                                                    caller.getEntryPoint(),
                                                    cause)));
        };
    }

    private static Report.Result<Call> call(
            Callee callee, Function caller, ClangFuncNameToken name) {
        Address address = name.getPcodeOp().getSeqnum().getTarget();
        return arguments(name)
                .<Report.Result<Call>>map(
                        arguments ->
                                new Report.Success<>(new Call(callee, caller, address, arguments)))
                .orElseGet(
                        () ->
                                new Report.Failure<>(
                                        "%s at %s: call to `%s` holds no argument list"
                                                .formatted(
                                                        caller.getName(true),
                                                        address,
                                                        callee.function().getName(true))));
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
        int[] depth = tokens.stream().mapToInt(CallSites::nesting).toArray();
        Arrays.parallelPrefix(depth, Integer::sum);
        IntStream commas =
                IntStream.range(0, tokens.size())
                        .filter(index -> depth[index] == 0)
                        .filter(index -> ",".equals(tokens.get(index).getText()));
        int[] cuts =
                IntStream.concat(
                                IntStream.concat(IntStream.of(-1), commas),
                                IntStream.of(tokens.size()))
                        .toArray();
        return IntStream.range(1, cuts.length)
                .mapToObj(cut -> tokens.subList(cuts[cut - 1] + 1, cuts[cut]))
                .map(part -> part.stream().map(CallSites::text).collect(Collectors.joining()))
                .map(String::strip)
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

    // --- [WRITE]

    private String write(
            Path out,
            SequencedMap<Function, Set<Function>> callersByTarget,
            int callers,
            List<Report.Result<Call>> results,
            String arguments)
            throws IOException {
        List<String> failed = results.stream().flatMap(Report.Result::failures).toList();
        Comparator<Call> order =
                Comparator.comparing((Call call) -> call.caller().getEntryPoint())
                        .thenComparing(Call::address);
        Map<Function, List<Call>> calls =
                results.stream()
                        .flatMap(Report.Result::values)
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
                                Stream.of(Report.section("FAILED")),
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
                Stream.of(Report.subsection(target.getName(true)), header),
                calls.stream().map(Call::row));
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Arguments.Request request =
                Arguments.parse(
                        getScriptName(),
                        getScriptArgs(),
                        EnumSet.of(Arguments.Setting.TIMEOUT, Arguments.Setting.PAYLOAD),
                        currentProgram);
        List<Callee> reached =
                request.seeds().stream()
                        .map(Functions::thunked)
                        .distinct()
                        .flatMap(this::reaching)
                        .toList();
        Map<Address, List<Callee>> callees =
                reached.stream()
                        .collect(
                                Collectors.groupingBy(callee -> callee.function().getEntryPoint()));
        Collector<Callee, ?, Set<Function>> union =
                Collectors.flatMapping(
                        callee -> Functions.callers(callee.function(), monitor),
                        Collectors.toCollection(LinkedHashSet::new));
        SequencedMap<Function, Set<Function>> callersByTarget =
                reached.stream()
                        .collect(Collectors.groupingBy(Callee::target, LinkedHashMap::new, union));
        List<Function> callers =
                callersByTarget.values().stream().flatMap(Set::stream).distinct().toList();
        List<Report.Result<Call>> calls =
                Functions.decompile(currentProgram, callers, request.settings(), monitor)
                        .entrySet()
                        .stream()
                        .flatMap(entry -> calls(entry.getKey(), entry.getValue(), callees))
                        .toList();
        println(write(request.out(), callersByTarget, callers.size(), calls, request.arguments()));
    }
}
