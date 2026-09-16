// Seeds, settings, and report markers every script of this bundle shares

// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.decompiler.DecompileOptions;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionTag;
import ghidra.program.model.listing.Program;
import ghidra.program.model.symbol.Reference;
import ghidra.program.util.DefinedDataIterator;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Set;
import java.util.function.BiFunction;
import java.util.function.Predicate;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [JOB] -----------------------------------------------------------------------------

final class Job {
    sealed interface Result<T> permits Ok, Problem {
        default Stream<T> values() {
            return switch (this) {
                case Ok<T>(T value) -> Stream.of(value);
                case Problem<T> _ -> Stream.empty();
            };
        }

        default Stream<String> problems() {
            return switch (this) {
                case Ok<T> _ -> Stream.empty();
                case Problem<T>(String message) -> Stream.of(message);
            };
        }
    }

    record Ok<T>(T value) implements Result<T> {}

    record Problem<T>(String message) implements Result<T> {}

    enum Setting {
        CALLERS("depth", 0),
        CALLEES("depth", 0),
        TIMEOUT("seconds", DecompileOptions.SUGGESTED_DECOMPILE_TIMEOUT_SECS),
        PAYLOAD("megabytes", DecompileOptions.SUGGESTED_MAX_PAYLOAD_BYTES);

        final String unit;
        final int preset;

        Setting(String unit, int preset) {
            this.unit = unit;
            this.preset = preset;
        }

        String usage() {
            return "[%s=<%s>]".formatted(name().toLowerCase(Locale.ROOT), unit);
        }
    }

    // Forms in the order resolution tries them, the empty prefix of NAME matches every seed
    enum Seed {
        ADDRESS("0x", "<hex>", Job::containing),
        REGEX("re:", "<regex>", Job::matching),
        STRING("str:", "<needle>", Job::referencing),
        TAG("tag:", "<tag>", Job::tagged),
        NAME("", "<name> | <namespace>::<name>", Job::named);

        final String prefix;
        final String argument;
        final BiFunction<String, Program, Result<List<Function>>> resolver;

        Seed(
                String prefix,
                String argument,
                BiFunction<String, Program, Result<List<Function>>> resolver) {
            this.prefix = prefix;
            this.argument = argument;
            this.resolver = resolver;
        }

        String usage() {
            return prefix + argument;
        }
    }

    record Request(Path out, List<Function> seeds, Map<Setting, Integer> settings, String text) {}

    private Job() {}

    // --- [ARGUMENTS] -----------------------------------------------------------------------

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
        List<String> given = List.of(args);
        if (given.isEmpty()) {
            throw new IllegalArgumentException(usage);
        }
        List<String> rest = given.subList(1, given.size());
        Map<Boolean, List<String>> split =
                rest.stream().collect(Collectors.partitioningBy(arg -> arg.contains("=")));
        List<Result<Map.Entry<Setting, Integer>>> settings =
                split.get(true).stream().map(arg -> setting(arg, accepted)).toList();
        List<Result<List<Function>>> seeds =
                split.get(false).isEmpty()
                        ? List.of(new Problem<>("no seed given"))
                        : split.get(false).stream().map(seed -> resolve(seed, program)).toList();
        List<String> problems =
                Stream.concat(seeds.stream(), settings.stream()).flatMap(Result::problems).toList();
        if (!problems.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", problems) + "\n" + usage);
        }
        Stream<Map.Entry<Setting, Integer>> presets =
                accepted.stream().map(key -> Map.entry(key, key.preset));
        Map<Setting, Integer> values =
                Stream.concat(presets, settings.stream().flatMap(Result::values))
                        .collect(
                                Collectors.toMap(
                                        Map.Entry::getKey, Map.Entry::getValue, (_, last) -> last));
        List<Function> functions =
                seeds.stream().flatMap(Result::values).flatMap(List::stream).distinct().toList();
        return new Request(Path.of(given.getFirst()), functions, values, String.join(" ", rest));
    }

    private static Result<Map.Entry<Setting, Integer>> setting(String arg, Set<Setting> accepted) {
        int at = arg.indexOf('=');
        try {
            Setting key = Setting.valueOf(arg.substring(0, at).toUpperCase(Locale.ROOT));
            Map.Entry<Setting, Integer> entry =
                    Map.entry(key, Integer.parseInt(arg.substring(at + 1)));
            return accepted.contains(key)
                    ? new Ok<>(entry)
                    : new Problem<>(arg + ": not a setting of this script");
        } catch (IllegalArgumentException _) {
            return new Problem<>(arg + ": not <setting>=<integer>");
        }
    }

    // --- [SEEDS] ---------------------------------------------------------------------------

    static Result<List<Function>> resolve(String seed, Program program) {
        Seed form =
                Arrays.stream(Seed.values())
                        .filter(each -> seed.startsWith(each.prefix))
                        .findFirst()
                        .orElseThrow();
        Result<List<Function>> matches =
                form.resolver.apply(seed.substring(form.prefix.length()), program);
        return matches instanceof Ok<List<Function>>(List<Function> found) && found.isEmpty()
                ? new Problem<>(seed + ": matches no function")
                : matches;
    }

    private static Result<List<Function>> containing(String hex, Program program) {
        try {
            Address address =
                    program.getAddressFactory()
                            .getDefaultAddressSpace()
                            .getAddress(Long.parseUnsignedLong(hex, 16));
            Function found = program.getFunctionManager().getFunctionContaining(address);
            return new Ok<>(Stream.ofNullable(found).toList());
        } catch (NumberFormatException _) {
            return new Problem<>("0x" + hex + ": not a hexadecimal address");
        }
    }

    private static Result<List<Function>> matching(String regex, Program program) {
        try {
            return new Ok<>(named(Pattern.compile(regex).asPredicate(), program));
        } catch (PatternSyntaxException exception) {
            return new Problem<>("re:" + regex + ": " + exception.getDescription());
        }
    }

    private static Result<List<Function>> named(String name, Program program) {
        return new Ok<>(named(name::equals, program));
    }

    private static Result<List<Function>> referencing(String needle, Program program) {
        String lowered = needle.toLowerCase(Locale.ROOT);
        Predicate<Data> hit =
                data ->
                        StringDataInstance.isString(data)
                                && data.getValue() instanceof String text
                                && text.toLowerCase(Locale.ROOT).contains(lowered);
        return new Ok<>(
                CollectionUtils.asStream(DefinedDataIterator.byDataInstance(program, hit))
                        .flatMap(Job::referrers)
                        .distinct()
                        .toList());
    }

    private static Result<List<Function>> tagged(String name, Program program) {
        return program.getFunctionManager().getFunctionTagManager().getFunctionTag(name)
                        instanceof FunctionTag tag
                ? new Ok<>(
                        functions(program)
                                .filter(function -> function.getTags().contains(tag))
                                .toList())
                : new Problem<>("tag:" + name + ": no such function tag");
    }

    private static List<Function> named(Predicate<String> test, Program program) {
        return functions(program)
                .filter(
                        function ->
                                test.test(function.getName(true)) || test.test(function.getName()))
                .toList();
    }

    private static Stream<Function> functions(Program program) {
        return CollectionUtils.asStream(program.getFunctionManager().getFunctions(true));
    }

    // A `__cfstring` struct references its text and functions reference the struct
    static Stream<Function> referrers(Data data) {
        Program program = data.getProgram();
        Stream<Data> holders =
                CollectionUtils.asStream(data.getReferenceIteratorTo())
                        .map(Reference::getFromAddress)
                        .map(program.getListing()::getDataContaining)
                        .filter(Objects::nonNull);
        return Stream.concat(Stream.of(data), holders)
                .map(Data::getReferenceIteratorTo)
                .flatMap(CollectionUtils::asStream)
                .map(Reference::getFromAddress)
                .map(program.getFunctionManager()::getFunctionContaining)
                .filter(Objects::nonNull)
                .distinct();
    }

    // --- [MARKERS] -------------------------------------------------------------------------

    static Stream<String> index(Program program, String facts) {
        return Stream.of(
                divider("INDEX"),
                "// %s %s %s".formatted(program.getName(), program.getLanguageID(), facts));
    }

    static String divider(String name) {
        String head = "// --- [" + name + "] ";
        return head + "-".repeat(90 - head.length());
    }

    static String item(String name) {
        return "// --- [" + name + "]";
    }
}
