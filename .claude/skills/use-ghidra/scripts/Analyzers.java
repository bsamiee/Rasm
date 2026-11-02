import ghidra.app.script.GhidraScript;
import ghidra.app.services.Analyzer;
import ghidra.util.classfinder.ClassSearcher;

import rasm.Arguments;
import rasm.Report;

import java.util.Arrays;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Analyzers extends GhidraScript {
    @Override
    public void run() {
        Arguments.Usage usage = new Arguments.Usage(getScriptName(), "<analyzer>...");
        String[] args = getScriptArgs();
        Set<String> analyzers =
                ClassSearcher.getInstances(Analyzer.class).stream()
                        .map(Analyzer::getName)
                        .collect(Collectors.toSet());
        List<String> names =
                usage.values(
                        Stream.concat(
                                Arrays.stream(args)
                                        .distinct()
                                        .<Report.Result<String>>map(
                                                name ->
                                                        analyzers.contains(name)
                                                                ? new Report.Success<>(name)
                                                                : new Report.Failure<>(
                                                                        "Argument `%s` names no analyzer"
                                                                                .formatted(name))),
                                usage.require(args.length > 0, "analyzer")));
        Set<String> registered = getCurrentAnalysisOptionsAndValues(currentProgram).keySet();
        setAnalysisOptions(
                currentProgram,
                names.stream()
                        .filter(registered::contains)
                        .collect(Collectors.toMap(name -> name, _ -> "false")));
        names.forEach(
                name ->
                        println(
                                "%s %s"
                                        .formatted(
                                                name,
                                                registered.contains(name)
                                                        ? "disabled"
                                                        : "registers no option on "
                                                                + currentProgram.getName())));
    }
}
