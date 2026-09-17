// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.DecompileOptions;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionTag;
import ghidra.program.model.listing.Program;
import ghidra.program.util.DefinedDataIterator;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.function.BiFunction;
import java.util.function.Predicate;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [ARGUMENTS] -----------------------------------------------------------------------

final class Arguments {
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
                case Failure<T>(String message) -> Stream.of(message);
            };
        }
    }

    record Success<T>(T value) implements Result<T> {}

    record Failure<T>(String message) implements Result<T> {}

    enum Setting {
        CALLERS("depth", 0),
        CALLEES("depth", 0),
        TIMEOUT("seconds", DecompileOptions.SUGGESTED_DECOMPILE_TIMEOUT_SECS),
        PAYLOAD("megabytes", DecompileOptions.SUGGESTED_MAX_PAYLOAD_BYTES);

        final String unit;
        final int defaultValue;

        Setting(String unit, int defaultValue) {
            this.unit = unit;
            this.defaultValue = defaultValue;
        }

        String usage() {
            return "[%s=<%s>]".formatted(name().toLowerCase(Locale.ROOT), unit);
        }
    }

    enum Seed {
        ADDRESS("0x", "<hex>", Arguments::containing),
        REGEX("re:", "<regex>", Arguments::matching),
        STRING("str:", "<needle>", Arguments::referencing),
        TAG("tag:", "<tag>", Arguments::tagged),
        NAME("", "<name> | <namespace>::<name>", Arguments::named);

        final String prefix;
        final String placeholder;
        final BiFunction<String, Program, Result<List<Function>>> resolver;

        Seed(
                String prefix,
                String placeholder,
                BiFunction<String, Program, Result<List<Function>>> resolver) {
            this.prefix = prefix;
            this.placeholder = placeholder;
            this.resolver = resolver;
        }

        String usage() {
            return prefix + placeholder;
        }
    }

    record Request(
            Path out, List<Function> seeds, Map<Setting, Integer> settings, String arguments) {}

    private Arguments() {}

    // --- [PARSING] ---------------------------------------------------------------------

    static Request parse(String script, String[] args, Set<Setting> accepted, Program program) {
        String usage =
                "usage: %s <out> <seed>... %s\nseed: %s"
                        .formatted(
                                script,
                                accepted.stream()
                                        .map(Setting::usage)
                                        .collect(Collectors.joining(" ")),
                                Arrays.stream(Seed.values())
                                        .map(Seed::usage)
                                        .collect(Collectors.joining(" | ")));
        List<String> arguments = List.of(args);
        if (arguments.isEmpty()) {
            throw new IllegalArgumentException(usage);
        }
        List<String> remaining = arguments.subList(1, arguments.size());
        Map<Boolean, List<String>> partition =
                remaining.stream().collect(Collectors.partitioningBy(arg -> arg.contains("=")));
        List<Result<Map.Entry<Setting, Integer>>> entries =
                partition.get(true).stream().map(arg -> setting(arg, accepted)).toList();
        List<Result<List<Function>>> seeds =
                partition.get(false).isEmpty()
                        ? List.of(new Failure<>("no seed given"))
                        : partition.get(false).stream()
                                .map(seed -> resolve(seed, program))
                                .toList();
        List<String> failures =
                Stream.concat(seeds.stream(), entries.stream()).flatMap(Result::failures).toList();
        if (!failures.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", failures) + "\n" + usage);
        }
        Stream<Map.Entry<Setting, Integer>> defaults =
                accepted.stream().map(key -> Map.entry(key, key.defaultValue));
        Map<Setting, Integer> settings =
                Stream.concat(defaults, entries.stream().flatMap(Result::values))
                        .collect(
                                Collectors.toMap(
                                        Map.Entry::getKey, Map.Entry::getValue, (_, last) -> last));
        List<Function> functions =
                seeds.stream().flatMap(Result::values).flatMap(List::stream).distinct().toList();
        return new Request(
                Path.of(arguments.getFirst()), functions, settings, String.join(" ", remaining));
    }

    static Path out(String script, String... args) {
        return Optional.of(args)
                .filter(arguments -> arguments.length == 1)
                .map(arguments -> Path.of(arguments[0]))
                .orElseThrow(
                        () -> new IllegalArgumentException("usage: %s <out>".formatted(script)));
    }

    static <T> List<T> values(List<Result<T>> results, String usage) {
        List<String> failures = results.stream().flatMap(Result::failures).toList();
        if (!failures.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", failures) + "\n" + usage);
        }
        return results.stream().flatMap(Result::values).toList();
    }

    private static Result<Map.Entry<Setting, Integer>> setting(String arg, Set<Setting> accepted) {
        int at = arg.indexOf('=');
        try {
            Setting key = Setting.valueOf(arg.substring(0, at).toUpperCase(Locale.ROOT));
            Map.Entry<Setting, Integer> entry =
                    Map.entry(key, Integer.parseInt(arg.substring(at + 1)));
            return accepted.contains(key)
                    ? new Success<>(entry)
                    : new Failure<>(arg + ": not a setting of this script");
        } catch (IllegalArgumentException _) {
            return new Failure<>(arg + ": not <setting>=<integer>");
        }
    }

    // --- [SEEDS] -----------------------------------------------------------------------

    static Result<List<Function>> resolve(String seed, Program program) {
        Seed form =
                Arrays.stream(Seed.values())
                        .filter(each -> seed.startsWith(each.prefix))
                        .findFirst()
                        .orElseThrow();
        Result<List<Function>> matches =
                form.resolver.apply(seed.substring(form.prefix.length()), program);
        return matches instanceof Success<List<Function>>(List<Function> found) && found.isEmpty()
                ? new Failure<>(seed + ": matches no function")
                : matches;
    }

    private static Result<List<Function>> containing(String hex, Program program) {
        try {
            Address address =
                    program.getAddressFactory()
                            .getDefaultAddressSpace()
                            .getAddress(Long.parseUnsignedLong(hex, 16));
            Function found = program.getFunctionManager().getFunctionContaining(address);
            return new Success<>(Stream.ofNullable(found).toList());
        } catch (NumberFormatException _) {
            return new Failure<>("0x" + hex + ": not a hexadecimal address");
        }
    }

    private static Result<List<Function>> matching(String regex, Program program) {
        try {
            return new Success<>(select(Pattern.compile(regex).asPredicate(), program));
        } catch (PatternSyntaxException exception) {
            return new Failure<>("re:" + regex + ": " + exception.getDescription());
        }
    }

    private static Result<List<Function>> named(String name, Program program) {
        return new Success<>(select(name::equals, program));
    }

    private static Result<List<Function>> referencing(String needle, Program program) {
        String lowercase = needle.toLowerCase(Locale.ROOT);
        Predicate<Data> containsNeedle =
                data ->
                        StringDataInstance.isString(data)
                                && data.getValue() instanceof String value
                                && value.toLowerCase(Locale.ROOT).contains(lowercase);
        return new Success<>(
                CollectionUtils.asStream(
                                DefinedDataIterator.byDataInstance(program, containsNeedle))
                        .flatMap(Functions::referrers)
                        .distinct()
                        .toList());
    }

    private static Result<List<Function>> tagged(String name, Program program) {
        return program.getFunctionManager().getFunctionTagManager().getFunctionTag(name)
                        instanceof FunctionTag tag
                ? new Success<>(
                        Functions.internal(program)
                                .filter(function -> function.getTags().contains(tag))
                                .toList())
                : new Failure<>("tag:" + name + ": no such function tag");
    }

    private static List<Function> select(Predicate<String> test, Program program) {
        return Functions.internal(program)
                .filter(
                        function ->
                                test.test(function.getName(true)) || test.test(function.getName()))
                .toList();
    }
}
