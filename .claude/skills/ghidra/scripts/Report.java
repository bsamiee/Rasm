// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.program.model.listing.Program;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Stream;

// --- [REPORT] --------------------------------------------------------------------------

final class Report {
    private Report() {}

    static String divider(String name) {
        String prefix = "// --- [" + name + "] ";
        return prefix + "-".repeat(90 - prefix.length());
    }

    static String item(String name) {
        return "// --- [" + name + "]";
    }

    static String write(Path out, Program program, String counts, Stream<String> body)
            throws IOException {
        List<String> lines =
                Stream.concat(
                                Stream.of(
                                        divider("INDEX"),
                                        "// %s %s %s"
                                                .formatted(
                                                        program.getName(),
                                                        program.getLanguageID(),
                                                        counts)),
                                body)
                        .toList();
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, lines);
        return "wrote " + out + ": " + counts;
    }
}
