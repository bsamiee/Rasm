import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.Reference;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.List;
import java.util.Optional;
import java.util.function.Predicate;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Graph extends GhidraScript {
    // --- [EDGES]

    private static String node(Function function) {
        return function.getEntryPoint() + " " + function.getName(true);
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Path out = new Arguments.Usage(getScriptName(), "<out>").only(getScriptArgs());
        List<Function> functions =
                Functions.internal(currentProgram)
                        .filter(Predicate.not(Function::isThunk))
                        .filter(Predicate.not(Functions::isStub))
                        .toList();
        List<Report.Section> sections =
                List.of(
                        Report.Section.rows(
                                "CALLS",
                                functions.stream()
                                        .flatMap(caller ->
                                                Functions.callees(caller, monitor)
                                                        .sorted(Functions.BY_ENTRY)
                                                        .map(callee ->
                                                                node(caller)
                                                                        + " -> "
                                                                        + node(callee)))
                                        .toList()),
                        Report.Section.rows(
                                "REFERENCES",
                                functions.stream()
                                        .flatMap(function ->
                                                CollectionUtils.asStream(
                                                                currentProgram
                                                                        .getReferenceManager()
                                                                        .getReferencesTo(
                                                                                function
                                                                                        .getEntryPoint()))
                                                        .filter(reference ->
                                                                !reference
                                                                        .getReferenceType()
                                                                        .isCall())
                                                        .map(Reference::getFromAddress)
                                                        .map(from ->
                                                                "%s <- %s %s"
                                                                        .formatted(
                                                                                node(function),
                                                                                from,
                                                                                Optional.ofNullable(
                                                                                                function.getProgram()
                                                                                                        .getFunctionManager()
                                                                                                        .getFunctionContaining(
                                                                                                                from))
                                                                                        .map(Graph::node)
                                                                                        .orElse("-"))))
                                        .toList()));
        println(Report.write(out, currentProgram, sections, ""));
    }
}
