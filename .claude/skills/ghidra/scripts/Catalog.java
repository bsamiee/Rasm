// Catalogs every string, import, and function with the functions that reference or call it

// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.script.GhidraScript;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionManager;
import ghidra.program.util.DefinedDataIterator;
import ghidra.util.task.TaskMonitor;

import util.CollectionUtils;

import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;
import java.util.function.Predicate;
import java.util.stream.Collector;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class Catalog extends GhidraScript {
    // A line ranked by the number printed between its head and tail
    record Row(long rank, String head, String tail) {
        static final Comparator<Row> ORDER =
                Comparator.comparingLong(Row::rank).reversed().thenComparing(Row::text);

        Row(Data data, Set<Function> functions) {
            this(
                    data.getAddress() + " " + data.getDefaultValueRepresentation() + " functions=",
                    functions);
        }

        Row(Function external, Set<Function> callers) {
            this(
                    Path.of(external.getExternalLocation().getLibraryName()).getFileName()
                            + " "
                            + external.getPrototypeString(false, false)
                            + " callers=",
                    callers);
        }

        Row(Function function, Set<Function> callers, TaskMonitor monitor) {
            this(
                    function.getBody().getNumAddresses(),
                    function.getEntryPoint() + " " + function.getName(true) + " size=",
                    " callers="
                            + callers.size()
                            + " callees="
                            + function.getCalledFunctions(monitor).stream()
                                    .map(Catalog::target)
                                    .distinct()
                                    .count());
        }

        Row(String head, Set<Function> functions) {
            this(
                    functions.size(),
                    head,
                    functions.stream()
                            .map(function -> " " + function.getName(true))
                            .sorted()
                            .collect(Collectors.joining()));
        }

        String text() {
            return head + rank + tail;
        }
    }

    record Section(String name, List<Row> rows) {
        String count() {
            return name.toLowerCase(Locale.ROOT) + "=" + rows.size();
        }

        Stream<String> lines() {
            return Stream.concat(
                    Stream.of(Job.divider(name)), rows.stream().sorted(Row.ORDER).map(Row::text));
        }
    }

    @Override
    public void run() throws Exception {
        Path out =
                switch (getScriptArgs()) {
                    case String[] one when one.length == 1 -> Path.of(one[0]);
                    default ->
                            throw new IllegalArgumentException(
                                    "usage: %s <out>".formatted(getScriptName()));
                };
        FunctionManager manager = currentProgram.getFunctionManager();
        List<Function> internal = CollectionUtils.asStream(manager.getFunctions(true)).toList();
        List<Function> external = CollectionUtils.asStream(manager.getExternalFunctions()).toList();
        // Callers of a thunk count as callers of the function it reaches
        Collector<Function, ?, Set<Function>> calling =
                Collectors.flatMapping(
                        function -> function.getCallingFunctions(monitor).stream(),
                        Collectors.toSet());
        Map<Function, Set<Function>> callers =
                Stream.concat(internal.stream(), external.stream())
                        .collect(Collectors.groupingBy(Catalog::target, calling));
        DefinedDataIterator defined =
                DefinedDataIterator.byDataInstance(currentProgram, StringDataInstance::isString);
        Section strings =
                new Section(
                        "STRINGS",
                        CollectionUtils.asStream(defined)
                                .map(
                                        data ->
                                                new Row(
                                                        data,
                                                        Job.referrers(data)
                                                                .collect(Collectors.toSet())))
                                .toList());
        Section imports =
                new Section(
                        "IMPORTS",
                        external.stream()
                                .map(function -> new Row(function, callers.get(function)))
                                .toList());
        Section functions =
                new Section(
                        "FUNCTIONS",
                        internal.stream()
                                .filter(Predicate.not(Function::isThunk))
                                .map(function -> new Row(function, callers.get(function), monitor))
                                .toList());
        List<Section> sections = List.of(strings, imports, functions);
        String facts = sections.stream().map(Section::count).collect(Collectors.joining(" "));
        List<String> lines =
                Stream.concat(
                                Job.index(currentProgram, facts),
                                sections.stream().flatMap(Section::lines))
                        .toList();
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, lines);
        println("wrote " + out + ": " + facts);
    }

    private static Function target(Function function) {
        return function.isThunk() ? function.getThunkedFunction(true) : function;
    }
}
