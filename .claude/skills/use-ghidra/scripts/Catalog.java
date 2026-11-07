import ghidra.app.script.GhidraScript;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.util.DefinedDataIterator;
import ghidra.util.task.TaskMonitor;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.Comparator;
import java.util.List;
import java.util.function.BiFunction;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Catalog extends GhidraScript {

    // --- [ROWS]

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
        StringRow(Data data) {
            this(data, Functions.referrers(data).sorted(Functions.BY_ENTRY).toList());
        }

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
        ImportRow(Function function, TaskMonitor monitor) {
            this(
                    function,
                    Functions.callers(function, monitor).sorted(Functions.BY_ENTRY).toList());
        }

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
        FunctionRow(Function function, TaskMonitor monitor) {
            this(
                    function,
                    Functions.callers(function, monitor).count(),
                    Functions.callees(function, monitor).count());
        }

        @Override
        public long rank() {
            return function.getBody().getNumAddresses();
        }

        @Override
        public String text() {
            return "%s %s size=%d callers=%d callees=%d source=%s"
                    .formatted(
                            function.getEntryPoint(),
                            function.getName(true),
                            rank(),
                            callers,
                            callees,
                            function.getSymbol().getSource());
        }
    }

    // --- [EXECUTION]

    @Override
    public void run() throws Exception {
        Path out = new Arguments.Usage(getScriptName(), "<out>").only(getScriptArgs());
        BiFunction<String, Stream<? extends Row>, Report.Section> section =
                (name, rows) ->
                        Report.Section.rows(name, rows.sorted(Row.ORDER).map(Row::text).toList());
        List<Report.Section> sections =
                List.of(
                        section.apply(
                                "STRINGS",
                                CollectionUtils.asStream(
                                                DefinedDataIterator.byDataInstance(
                                                        currentProgram,
                                                        StringDataInstance::isString))
                                        .map(StringRow::new)),
                        section.apply(
                                "IMPORTS",
                                CollectionUtils.asStream(
                                                currentProgram
                                                        .getFunctionManager()
                                                        .getExternalFunctions())
                                        .map(function -> new ImportRow(function, monitor))),
                        section.apply(
                                "FUNCTIONS",
                                Functions.internal(currentProgram)
                                        .filter(Predicate.not(Function::isThunk))
                                        .filter(Predicate.not(Functions::isStub))
                                        .map(function -> new FunctionRow(function, monitor))));
        println(Report.write(out, currentProgram, sections, ""));
    }
}
