import ghidra.app.script.GhidraScript;
import ghidra.app.services.Analyzer;
import ghidra.util.classfinder.ClassSearcher;

import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Analyzers extends GhidraScript {
    @Override
    public void run() {
        String usage = "usage: %s <analyzer>...".formatted(getScriptName());
        List<String> args = List.of(getScriptArgs());
        if (args.isEmpty()) {
            throw new IllegalArgumentException(usage);
        }
        Set<String> analyzers =
                ClassSearcher.getInstances(Analyzer.class).stream()
                        .map(Analyzer::getName)
                        .collect(Collectors.toSet());
        List<String> names =
                Arguments.values(
                        args.stream()
                                .distinct()
                                .<Report.Result<String>>map(
                                        name ->
                                                analyzers.contains(name)
                                                        ? new Report.Success<>(name)
                                                        : new Report.Failure<>(
                                                                "Argument `%s` names no analyzer"
                                                                        .formatted(name)))
                                .toList(),
                        usage);
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
