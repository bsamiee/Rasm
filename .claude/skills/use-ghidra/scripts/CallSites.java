import ghidra.app.decompiler.ClangBreak;
import ghidra.app.decompiler.ClangFuncNameToken;
import ghidra.app.decompiler.ClangSyntaxToken;
import ghidra.app.decompiler.ClangToken;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;

import util.CollectionUtils;

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
import java.util.stream.Gatherers;
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

    // --- [CALLS]

    record Call(Callee callee, Function caller, Address address, List<String> arguments) {
        static Report.Result<Call> of(Callee callee, Function caller, ClangFuncNameToken name) {
            Address address = name.getPcodeOp().getSeqnum().getTarget();
            Optional<ClangSyntaxToken> open =
                    CollectionUtils.asStream(name.iterator(true))
                            .filter(ClangSyntaxToken.class::isInstance)
                            .map(ClangSyntaxToken.class::cast)
                            .filter(token -> token.getOpen() >= 0)
                            .findFirst();
            List<ClangToken> tokens =
                    open.stream()
                            .flatMap(
                                    paren ->
                                            CollectionUtils.asStream(paren.iterator(true))
                                                    .skip(1)
                                                    .takeWhile(
                                                            token ->
                                                                    !(token
                                                                                    instanceof
                                                                                    ClangSyntaxToken
                                                                                            close
                                                                            && close.getClose()
                                                                                    == paren
                                                                                            .getOpen())))
                            .toList();
            List<String> texts =
                    tokens.stream()
                            .map(token -> token instanceof ClangBreak ? " " : token.getText())
                            .toList();
            IntStream commas =
                    IntStream.range(0, tokens.size())
                            .filter(index -> ",".equals(tokens.get(index).getText()))
                            .filter(index -> name.Parent().equals(tokens.get(index).Parent()));
            List<String> arguments =
                    IntStream.concat(
                                    IntStream.of(-1),
                                    IntStream.concat(commas, IntStream.of(tokens.size())))
                            .boxed()
                            .gather(Gatherers.windowSliding(2))
                            .map(
                                    cut ->
                                            String.join(
                                                            "",
                                                            texts.subList(
                                                                    cut.getFirst() + 1,
                                                                    cut.getLast()))
                                                    .strip())
                            .toList();
            return open.<Report.Result<Call>>map(
                            _ -> new Report.Success<>(new Call(callee, caller, address, arguments)))
                    .orElseGet(
                            () ->
                                    new Report.Failure<>(
                                            "%s at %s: call to `%s` holds no argument list"
                                                    .formatted(
                                                            caller.getName(true),
                                                            address,
                                                            callee.function().getName(true))));
        }

        static Stream<Report.Result<Call>> read(
                Map<Address, List<Callee>> callees, Function caller, DecompileResults decompiled) {
            return CollectionUtils.asStream(decompiled.getCCodeMarkup().tokenIterator(true))
                    .filter(ClangFuncNameToken.class::isInstance)
                    .map(ClangFuncNameToken.class::cast)
                    .filter(name -> name.getPcodeOp() != null)
                    .flatMap(
                            name ->
                                    callees
                                            .getOrDefault(
                                                    name.getPcodeOp().getInput(0).getAddress(),
                                                    List.of())
                                            .stream()
                                            .map(callee -> of(callee, caller, name)));
        }

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
                        .flatMap(
                                target -> {
                                    List<Function> direct =
                                            Stream.concat(
                                                            Stream.of(target),
                                                            Functions.thunks(target))
                                                    .toList();
                                    return Stream.<Callee>concat(
                                            direct.stream()
                                                    .map(function -> new Direct(function, target)),
                                            direct.stream()
                                                    .flatMap(
                                                            function ->
                                                                    function
                                                                            .getCallingFunctions(
                                                                                    monitor)
                                                                            .stream())
                                                    .filter(Functions::isStub)
                                                    .distinct()
                                                    .map(
                                                            stub ->
                                                                    new Stub(
                                                                            stub,
                                                                            target,
                                                                            Functions.selector(
                                                                                    stub))));
                                })
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
        List<Report.Result<Call>> results =
                Functions.decompile(currentProgram, callers, request.settings(), monitor)
                        .entrySet()
                        .stream()
                        .flatMap(
                                entry ->
                                        switch (entry.getValue()) {
                                            case Report.Success<DecompileResults>(
                                                            DecompileResults decompiled) ->
                                                    Call.read(callees, entry.getKey(), decompiled);
                                            case Report.Failure<DecompileResults>(String cause) ->
                                                    Stream.of(
                                                            new Report.Failure<Call>(
                                                                    "%s at %s: %s"
                                                                            .formatted(
                                                                                    entry.getKey()
                                                                                            .getName(
                                                                                                    true),
                                                                                    entry.getKey()
                                                                                            .getEntryPoint(),
                                                                                    cause)));
                                        })
                        .toList();
        List<String> failed = results.stream().flatMap(Report.Result::failures).toList();
        Map<Function, List<Call>> calls =
                results.stream()
                        .flatMap(Report.Result::values)
                        .sorted(
                                Comparator.comparing((Call call) -> call.caller().getEntryPoint())
                                        .thenComparing(Call::address))
                        .collect(Collectors.groupingBy(call -> call.callee().target()));
        String counts =
                "targets=%d callers=%d calls=%d failed=%d args=%s"
                        .formatted(
                                callersByTarget.size(),
                                callers.size(),
                                calls.values().stream().mapToLong(List::size).sum(),
                                failed.size(),
                                request.arguments());
        Stream<String> sections =
                callersByTarget.entrySet().stream()
                        .flatMap(
                                entry -> {
                                    Function target = entry.getKey();
                                    List<Call> targetCalls = calls.getOrDefault(target, List.of());
                                    return Stream.concat(
                                            Stream.of(
                                                    Report.subsection(target.getName(true)),
                                                    "// %s @ %s callers=%d calls=%d"
                                                            .formatted(
                                                                    target.getName(true),
                                                                    target.getEntryPoint(),
                                                                    entry.getValue().size(),
                                                                    targetCalls.size())),
                                            targetCalls.stream().map(Call::row));
                                });
        Stream<String> body =
                Stream.of(
                                sections,
                                Stream.of(Report.section("FAILED")),
                                failed.stream().map("// "::concat))
                        .flatMap(section -> section);
        println(Report.write(request.out(), currentProgram, counts, body));
    }
}
