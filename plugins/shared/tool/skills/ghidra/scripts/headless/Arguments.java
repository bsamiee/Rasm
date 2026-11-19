package rasm;

import ghidra.app.decompiler.DecompileOptions;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
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

public final class Arguments {

    // --- [USAGE]

    public record Usage(String script, String form) {
        public String text() {
            return "usage: %s %s".formatted(script, form);
        }

        public <T> List<T> values(Stream<Report.Result<T>> results) {
            return switch (Report.all(results)) {
                case Report.Success<List<T>>(List<T> values) -> values;
                case Report.Failure<List<T>>(String cause) ->
                        throw new IllegalArgumentException(cause + "\n" + text());
            };
        }

        public <T> Stream<Report.Result<T>> require(boolean present, String noun) {
            return present
                    ? Stream.empty()
                    : Stream.of(new Report.Failure<>("Arguments name no " + noun));
        }

        public Path out(String... args) {
            return values(
                            Stream.concat(
                                    Arrays.stream(args)
                                            .limit(1)
                                            .<Report.Result<Path>>map(argument ->
                                                    new Report.Success<>(Path.of(argument))),
                                    require(args.length > 0, form.split(" ", 2)[0])))
                    .getFirst();
        }

        public Path only(String... args) {
            Path out = out(args);
            values(
                    Arrays.stream(args)
                            .skip(1)
                            .<Report.Result<Path>>map(argument ->
                                    new Report.Failure<>(
                                            "Argument `%s` follows the output path"
                                                    .formatted(argument))));
            return out;
        }

        public List<String> operands(String... args) {
            return Arrays.asList(args).subList(Math.min(1, args.length), args.length);
        }
    }

    // --- [OPERANDS]

    public enum Setting {
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

        public int read(Map<Setting, Integer> settings) {
            return settings.getOrDefault(this, defaultValue);
        }

        public static Map<Setting, Integer> defaults() {
            return Arrays.stream(values())
                    .collect(
                            Collectors.toMap(
                                    setting -> setting,
                                    setting -> setting.defaultValue,
                                    (first, _) -> first,
                                    () -> new EnumMap<>(Setting.class)));
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
                            .orElseGet(() ->
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
                        .<Report.Result<List<Function>>>map(address ->
                                new Report.Success<>(
                                        Stream.ofNullable(
                                                        program.getFunctionManager()
                                                                .getFunctionContaining(address))
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
                return Optional.ofNullable(
                                program.getFunctionManager()
                                        .getFunctionTagManager()
                                        .getFunctionTag(name))
                        .<Report.Result<List<Function>>>map(tag ->
                                new Report.Success<>(
                                        Functions.internal(program)
                                                .filter(function ->
                                                        function.getTags().contains(tag))
                                                .toList()))
                        .orElseGet(() -> new Report.Failure<>("names no function tag"));
            }
        },
        NAME("<name> | <namespace>::<name>") {
            @Override
            Report.Result<List<Function>> find(String name, Program program) {
                return new Report.Success<>(named(name::equals, program));
            }
        };

        final Optional<String> prefix;
        final String placeholder;

        Seed(String prefix, String placeholder) {
            this.prefix = Optional.of(prefix);
            this.placeholder = placeholder;
        }

        Seed(String placeholder) {
            this.prefix = Optional.empty();
            this.placeholder = placeholder;
        }

        abstract Report.Result<List<Function>> find(String text, Program program);

        String usage() {
            return prefix.map(each -> each + placeholder).orElse(placeholder);
        }

        Optional<String> operand(String seed) {
            return prefix.filter(seed::startsWith).map(each -> seed.substring(each.length()));
        }

        private static List<Function> named(Predicate<String> test, Program program) {
            return Functions.internal(program)
                    .filter(function ->
                            test.test(function.getName(true)) || test.test(function.getName()))
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

    public record Request(
            Path out,
            List<Function> seeds,
            Map<Setting, Integer> settings,
            List<String> arguments) {}

    private Arguments() {}

    // --- [PARSING]

    public static Report.Result<List<Function>> functions(String seed, Program program) {
        return switch (Arrays.stream(Seed.values())
                .flatMap(form -> form.operand(seed).map(text -> form.find(text, program)).stream())
                .findFirst()
                .orElseGet(() -> Seed.NAME.find(seed, program))) {
            case Report.Success<List<Function>>(List<Function> found) when found.isEmpty() ->
                    new Report.Failure<>("Seed `%s` matches no function".formatted(seed));
            case Report.Success<List<Function>> found -> found;
            case Report.Failure<List<Function>>(String cause) ->
                    new Report.Failure<>("Seed `%s` %s".formatted(seed, cause));
        };
    }

    public static Request parse(
            String script, String[] args, Set<Setting> accepted, Program program) {
        Usage usage =
                new Usage(
                        script,
                        "<out> <seed>... %s\nseed: %s"
                                .formatted(
                                        accepted.stream()
                                                .map(Setting::usage)
                                                .collect(Collectors.joining(" ")),
                                        Arrays.stream(Seed.values())
                                                .map(Seed::usage)
                                                .collect(Collectors.joining(" | "))));
        Path out = usage.out(args);
        List<String> arguments = usage.operands(args);
        List<Operand> operands =
                usage.values(
                        Stream.concat(
                                arguments.stream()
                                        .map(argument ->
                                                Setting.of(argument)
                                                        .map(setting ->
                                                                setting.assign(
                                                                        argument, script, accepted))
                                                        .orElseGet(() ->
                                                                functions(argument, program)
                                                                        .map(SeedMatch::new))),
                                usage.require(
                                        arguments.stream()
                                                .map(Setting::of)
                                                .anyMatch(Optional::isEmpty),
                                        "seed")));
        Map<Setting, Integer> settings =
                Stream.concat(
                                Setting.defaults().entrySet().stream()
                                        .filter(entry -> accepted.contains(entry.getKey()))
                                        .map(entry ->
                                                new SettingValue(entry.getKey(), entry.getValue())),
                                operands.stream().flatMap(Operand::assignments))
                        .collect(
                                Collectors.toMap(
                                        SettingValue::setting,
                                        SettingValue::value,
                                        (_, last) -> last,
                                        () -> new EnumMap<>(Setting.class)));
        List<Function> seeds = operands.stream().flatMap(Operand::seeds).distinct().toList();
        return new Request(out, seeds, settings, arguments);
    }
}
