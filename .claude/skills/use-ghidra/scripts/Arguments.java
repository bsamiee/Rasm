import ghidra.app.decompiler.DecompileOptions;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionTag;
import ghidra.program.model.listing.Program;
import ghidra.program.util.DefinedDataIterator;

import util.CollectionUtils;

import java.nio.file.Path;
import java.util.Arrays;
import java.util.EnumMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.function.Predicate;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [OPERATIONS] ----------------------------------------------------------------------

final class Arguments {
    // --- [OPERANDS]

    enum Setting {
        CALLERS("depth", 0, 0),
        CALLEES("depth", 0, 0),
        TIMEOUT("seconds", DecompileOptions.SUGGESTED_DECOMPILE_TIMEOUT_SECS, 1),
        PAYLOAD("megabytes", DecompileOptions.SUGGESTED_MAX_PAYLOAD_BYTES, 1);

        private static final Pattern COUNT = Pattern.compile("\\d{1,9}");

        final String unit;
        final int defaultValue;
        final int minimum;

        Setting(String unit, int defaultValue, int minimum) {
            this.unit = unit;
            this.defaultValue = defaultValue;
            this.minimum = minimum;
        }

        String label() {
            return name().toLowerCase(Locale.ROOT);
        }

        String key() {
            return label() + "=";
        }

        String usage() {
            return "[%s<%s>]".formatted(key(), unit);
        }

        static Optional<Setting> of(String argument) {
            return Arrays.stream(values())
                    .filter(setting -> argument.startsWith(setting.key()))
                    .findFirst();
        }

        Report.Result<Operand> assign(String argument, String script, Set<Setting> accepted) {
            return accepted.contains(this)
                    ? Optional.of(argument.substring(key().length()))
                            .filter(COUNT.asMatchPredicate())
                            .map(Integer::valueOf)
                            .filter(count -> count >= minimum)
                            .<Report.Result<Operand>>map(
                                    count -> new Report.Success<>(new SettingValue(this, count)))
                            .orElseGet(
                                    () ->
                                            new Report.Failure<>(
                                                    "Setting `%s` takes integer %d or more"
                                                            .formatted(argument, minimum)))
                    : new Report.Failure<>(
                            "Script `%s` takes no setting `%s`".formatted(script, label()));
        }
    }

    enum Seed {
        ADDRESS("0x", "<hex>") {
            @Override
            Report.Result<List<Function>> find(String hex, Program program) {
                return Optional.ofNullable(program.getAddressFactory().getAddress(hex))
                        .<Report.Result<List<Function>>>map(
                                address ->
                                        new Report.Success<>(
                                                Stream.ofNullable(
                                                                program.getFunctionManager()
                                                                        .getFunctionContaining(
                                                                                address))
                                                        .toList()))
                        .orElseGet(() -> new Report.Failure<>("names no address"));
            }
        },
        REGEX("re:", "<regex>") {
            @Override
            Report.Result<List<Function>> find(String regex, Program program) {
                try {
                    return new Report.Success<>(
                            named(Pattern.compile(regex).asPredicate(), program));
                } catch (PatternSyntaxException exception) {
                    return new Report.Failure<>(
                            "did not parse (%s)".formatted(exception.getDescription()));
                }
            }
        },
        STRING("str:", "<needle>") {
            @Override
            Report.Result<List<Function>> find(String needle, Program program) {
                String lowercase = needle.toLowerCase(Locale.ROOT);
                Predicate<Data> holdsNeedle =
                        data ->
                                StringDataInstance.isString(data)
                                        && data.getValue() instanceof String value
                                        && value.toLowerCase(Locale.ROOT).contains(lowercase);
                return new Report.Success<>(
                        CollectionUtils.asStream(
                                        DefinedDataIterator.byDataInstance(program, holdsNeedle))
                                .flatMap(Functions::referrers)
                                .distinct()
                                .toList());
            }
        },
        TAG("tag:", "<tag>") {
            @Override
            Report.Result<List<Function>> find(String name, Program program) {
                return program.getFunctionManager().getFunctionTagManager().getFunctionTag(name)
                                instanceof FunctionTag tag
                        ? new Report.Success<>(
                                Functions.internal(program)
                                        .filter(function -> function.getTags().contains(tag))
                                        .toList())
                        : new Report.Failure<>("names no function tag");
            }
        },
        NAME("", "<name> | <namespace>::<name>") {
            @Override
            Report.Result<List<Function>> find(String name, Program program) {
                return new Report.Success<>(named(name::equals, program));
            }
        };

        final String prefix;
        final String placeholder;

        Seed(String prefix, String placeholder) {
            this.prefix = prefix;
            this.placeholder = placeholder;
        }

        abstract Report.Result<List<Function>> find(String text, Program program);

        String usage() {
            return prefix + placeholder;
        }

        static Report.Result<Operand> resolve(String seed, Program program) {
            Seed form =
                    Arrays.stream(values())
                            .filter(each -> seed.startsWith(each.prefix))
                            .findFirst()
                            .orElseThrow();
            return switch (form.find(seed.substring(form.prefix.length()), program)) {
                case Report.Success<List<Function>>(List<Function> found) when found.isEmpty() ->
                        new Report.Failure<>("Seed `%s` matches no function".formatted(seed));
                case Report.Success<List<Function>>(List<Function> found) ->
                        new Report.Success<>(new SeedMatch(found));
                case Report.Failure<List<Function>>(String cause) ->
                        new Report.Failure<>("Seed `%s` %s".formatted(seed, cause));
            };
        }

        private static List<Function> named(Predicate<String> test, Program program) {
            return Functions.internal(program)
                    .filter(
                            function ->
                                    test.test(function.getName(true))
                                            || test.test(function.getName()))
                    .toList();
        }
    }

    sealed interface Operand permits SeedMatch, SettingValue {
        default Stream<Function> seeds() {
            return switch (this) {
                case SeedMatch(List<Function> functions) -> functions.stream();
                case SettingValue _ -> Stream.empty();
            };
        }

        default Stream<SettingValue> assignments() {
            return switch (this) {
                case SeedMatch _ -> Stream.empty();
                case SettingValue value -> Stream.of(value);
            };
        }
    }

    record SeedMatch(List<Function> functions) implements Operand {}

    record SettingValue(Setting setting, int value) implements Operand {}

    record Request(
            Path out, List<Function> seeds, Map<Setting, Integer> settings, String arguments) {}

    private Arguments() {}

    // --- [PARSING]

    static <T> List<T> values(List<Report.Result<T>> results, String usage) {
        List<String> failures = results.stream().flatMap(Report.Result::failures).toList();
        if (!failures.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", failures) + "\n" + usage);
        }
        return results.stream().flatMap(Report.Result::values).toList();
    }

    static Path out(String script, String... args) {
        return Optional.of(args)
                .filter(arguments -> arguments.length == 1)
                .map(arguments -> Path.of(arguments[0]))
                .orElseThrow(
                        () -> new IllegalArgumentException("usage: %s <out>".formatted(script)));
    }

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
        if (args.length == 0) {
            throw new IllegalArgumentException(usage);
        }
        List<String> arguments = Arrays.asList(args).subList(1, args.length);
        Stream<Report.Result<Operand>> missing =
                arguments.stream().map(Setting::of).allMatch(Optional::isPresent)
                        ? Stream.of(new Report.Failure<>("Arguments name no seed"))
                        : Stream.empty();
        Stream<Report.Result<Operand>> resolved =
                arguments.stream()
                        .map(
                                argument ->
                                        Setting.of(argument)
                                                .map(
                                                        setting ->
                                                                setting.assign(
                                                                        argument, script, accepted))
                                                .orElseGet(() -> Seed.resolve(argument, program)));
        List<Operand> operands = values(Stream.concat(resolved, missing).toList(), usage);
        Stream<SettingValue> defaults =
                accepted.stream().map(setting -> new SettingValue(setting, setting.defaultValue));
        Map<Setting, Integer> settings =
                Stream.concat(defaults, operands.stream().flatMap(Operand::assignments))
                        .collect(
                                Collectors.toMap(
                                        SettingValue::setting,
                                        SettingValue::value,
                                        (_, last) -> last,
                                        () -> new EnumMap<>(Setting.class)));
        List<Function> seeds = operands.stream().flatMap(Operand::seeds).distinct().toList();
        return new Request(Path.of(args[0]), seeds, settings, String.join(" ", arguments));
    }
}
