import ghidra.app.script.GhidraScript;
import ghidra.program.model.data.DataTypeManager;

import rasm.Arguments;
import rasm.Report;

import java.nio.file.Path;
import java.util.List;
import java.util.Optional;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Drop extends GhidraScript {
    // --- [RUN]

    @Override
    public void run() throws Exception {
        Arguments.Usage usage = new Arguments.Usage(getScriptName(), "<out> <path>...");
        String[] args = getScriptArgs();
        Path out = usage.out(args);
        List<String> paths = usage.operands(args);
        usage.values(usage.<String>require(!paths.isEmpty(), "data type path"));
        DataTypeManager types = currentProgram.getDataTypeManager();
        List<Report.Result<String>> results =
                paths.stream()
                        .map(
                                path ->
                                        Optional.ofNullable(types.getDataType(path))
                                                .filter(types::remove)
                                                .<Report.Result<String>>map(_ -> new Report.Success<>(path))
                                                .orElseGet(() -> new Report.Failure<>(path)))
                        .toList();
        List<Report.Section> sections =
                List.of(
                        Report.Section.rows(
                                "REMOVED", results.stream().flatMap(Report.Result::values).toList()),
                        Report.Section.rows(
                                "ABSENT", results.stream().flatMap(Report.Result::failures).toList()));
        println(Report.write(out, currentProgram, sections, String.join(" ", paths)));
    }
}
