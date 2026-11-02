package rasm;

import ghidra.program.model.listing.Program;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Locale;
import java.util.function.Function;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [MODELS] --------------------------------------------------------------------------

public final class Report {
    // --- [RESULTS]

    public sealed interface Result<T> permits Success, Failure {
        default <R> Result<R> map(Function<? super T, ? extends R> mapper) {
            return switch (this) {
                case Success<T>(T value) -> new Success<>(mapper.apply(value));
                case Failure<T>(String cause) -> new Failure<>(cause);
            };
        }

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

    public record Success<T>(T value) implements Result<T> {}

    public record Failure<T>(String cause) implements Result<T> {}

    public static <T> Result<List<T>> all(Stream<Result<T>> results) {
        List<Result<T>> collected = results.toList();
        List<String> failures = collected.stream().flatMap(Result::failures).toList();
        return failures.isEmpty()
                ? new Success<>(collected.stream().flatMap(Result::values).toList())
                : new Failure<>(String.join("\n", failures));
    }

    // --- [SECTIONS]

    public record Section(String name, long count, List<String> lines) {
        public static Section rows(String name, List<String> rows) {
            return new Section(name, rows.size(), rows);
        }

        public static Section comments(String name, List<String> texts) {
            List<String> written = texts.stream().filter(Predicate.not(String::isBlank)).toList();
            return new Section(
                    name,
                    written.size(),
                    written.stream().flatMap(String::lines).map("// "::concat).toList());
        }

        String counter() {
            return name.toLowerCase(Locale.ROOT) + "=" + count;
        }

        Stream<String> text() {
            return Stream.concat(Stream.of(divider(name)), lines.stream());
        }
    }

    private Report() {}

    // --- [LINES]

    public static String divider(String name) {
        String divider = "// --- [" + name + "] ";
        return divider + "-".repeat(90 - divider.length());
    }

    public static String subdivider(String name) {
        return "// --- [" + name + "]";
    }

    public static String write(Path out, Program program, List<Section> sections, String arguments)
            throws IOException {
        String counts =
                Stream.concat(
                                sections.stream().map(Section::counter),
                                Stream.of(arguments)
                                        .filter(Predicate.not(String::isEmpty))
                                        .map("args="::concat))
                        .collect(Collectors.joining(" "));
        Stream<String> index =
                Stream.of(
                        divider("INDEX"),
                        "// %s %s %s"
                                .formatted(program.getName(), program.getLanguageID(), counts));
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, Stream.concat(index, sections.stream().flatMap(Section::text)).toList());
        return "Wrote %s %s".formatted(out, counts);
    }
}
