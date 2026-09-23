// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.script.GhidraScript;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.util.DefinedDataIterator;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class Catalog extends GhidraScript {
    sealed interface Row permits StringRow, ImportRow, FunctionRow {
        Comparator<Row> ORDER =
                Comparator.comparingLong(Row::rank).reversed().thenComparing(Row::text);

        long rank();

        String text();

        static String names(List<Function> functions) {
            return functions.stream()
                    .map(function -> " " + function.getName(true))
                    .collect(Collectors.joining());
        }
    }

    record StringRow(Data data, List<Function> functions) implements Row {
        @Override
        public long rank() {
            return functions.size();
        }

        @Override
        public String text() {
            return "%s %s functions=%d%s"
                    .formatted(
                            data.getAddress(),
                            data.getDefaultValueRepresentation(),
                            functions.size(),
                            Row.names(functions));
        }
    }

    record ImportRow(Function function, List<Function> callers) implements Row {
        @Override
        public long rank() {
            return callers.size();
        }

        @Override
        public String text() {
            return "%s %s callers=%d%s"
                    .formatted(
                            Path.of(function.getExternalLocation().getLibraryName()).getFileName(),
                            function.getPrototypeString(false, false),
                            callers.size(),
                            Row.names(callers));
        }
    }

    record FunctionRow(Function function, long callers, long callees) implements Row {
        @Override
        public long rank() {
            return function.getBody().getNumAddresses();
        }

        @Override
        public String text() {
            return "%s %s size=%d callers=%d callees=%d"
                    .formatted(
                            function.getEntryPoint(),
                            function.getName(true),
                            rank(),
                            callers,
                            callees);
        }
    }

    record Section(String name, List<? extends Row> rows) {
        String count() {
            return name.toLowerCase(Locale.ROOT) + "=" + rows.size();
        }

        Stream<String> lines() {
            return Stream.concat(
                    Stream.of(Report.divider(name)),
                    rows.stream().sorted(Row.ORDER).map(Row::text));
        }
    }

    @Override
    public void run() throws Exception {
        Path out = Arguments.out(getScriptName(), getScriptArgs());
        List<Section> sections = List.of(strings(), imports(), functions());
        String counts = sections.stream().map(Section::count).collect(Collectors.joining(" "));
        println(
                Report.write(
                        out, currentProgram, counts, sections.stream().flatMap(Section::lines)));
    }

    // --- [SECTIONS] --------------------------------------------------------------------

    private Section strings() {
        DefinedDataIterator defined =
                DefinedDataIterator.byDataInstance(currentProgram, StringDataInstance::isString);
        return new Section(
                "STRINGS",
                CollectionUtils.asStream(defined)
                        .map(
                                data ->
                                        new StringRow(
                                                data,
                                                Functions.referrers(data)
                                                        .sorted(Functions.BY_ENTRY)
                                                        .toList()))
                        .toList());
    }

    private Section imports() {
        return new Section(
                "IMPORTS",
                CollectionUtils.asStream(currentProgram.getFunctionManager().getExternalFunctions())
                        .map(
                                function ->
                                        new ImportRow(
                                                function,
                                                Functions.callers(function, monitor)
                                                        .sorted(Functions.BY_ENTRY)
                                                        .toList()))
                        .toList());
    }

    private Section functions() {
        return new Section(
                "FUNCTIONS",
                Functions.internal(currentProgram)
                        .filter(Predicate.not(Function::isThunk))
                        .filter(Predicate.not(Functions::isStub))
                        .map(
                                function ->
                                        new FunctionRow(
                                                function,
                                                Functions.callers(function, monitor).count(),
                                                Functions.callees(function, monitor).count()))
                        .toList());
    }
}
