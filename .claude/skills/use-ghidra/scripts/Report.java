import ghidra.program.model.listing.Program;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.stream.Stream;

// --- [OPERATIONS] ----------------------------------------------------------------------

final class Report {
    // --- [RESULTS]

    sealed interface Result<T> permits Success, Failure {
        default Stream<T> values() {
            return switch (this) {
                case Success<T>(T value) -> Stream.of(value);
                case Failure<T> _ -> Stream.empty();
            };
        }

        default Stream<String> failures() {
            return switch (this) {
                case Success<T> _ -> Stream.empty();
                case Failure<T>(String cause) -> Stream.of(cause);
            };
        }
    }

    record Success<T>(T value) implements Result<T> {}

    record Failure<T>(String cause) implements Result<T> {}

    private Report() {}

    // --- [LINES]

    static String section(String name) {
        String divider = "// --- [" + name + "] ";
        return divider + "-".repeat(90 - divider.length());
    }

    static String subsection(String name) {
        return "// --- [" + name + "]";
    }

    static String write(Path out, Program program, String counts, Stream<String> body)
            throws IOException {
        Stream<String> index =
                Stream.of(
                        section("INDEX"),
                        "// %s %s %s"
                                .formatted(program.getName(), program.getLanguageID(), counts));
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, Stream.concat(index, body).toList());
        return "Wrote %s: %s".formatted(out, counts);
    }
}
