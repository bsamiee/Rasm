import ghidra.app.cmd.function.ApplyFunctionSignatureCmd;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.cparser.C.CParser;
import ghidra.app.util.cparser.C.CParserUtils;
import ghidra.app.util.cparser.C.ParseException;
import ghidra.app.util.cparser.CPP.DefineTable;
import ghidra.app.util.cparser.CPP.PreProcessor;
import ghidra.app.util.cparser.CPP.TokenMgrError;
import ghidra.framework.Application;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.FunctionDefinition;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;

import util.CollectionUtils;

import utilities.util.FileUtilities;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.function.Function;
import java.util.regex.MatchResult;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.Gatherers;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Headers extends GhidraScript {
    // --- [PATTERNS]

    private static final Pattern DEFINE = Pattern.compile("-D[A-Za-z_]\\w*(=.*)?");

    private static final Pattern HAS_INCLUDE =
            Pattern.compile("__has_include(_next)?\\s*\\(\\s*[<\"]([^>\"]+)[>\"]\\s*\\)");

    private static final Pattern SCOPED_CONDITION =
            Pattern.compile("(?m)^(\\s*#\\s*(?:el)?if)\\b.*::.*$");

    private static final Pattern ARRAY_PARAMETER =
            Pattern.compile(
                    "(\\*(?:\\s*(?:(?:__)?(?:const|volatile|restrict)|_Atomic)\\b)*)\\s*\\[[^\\]]*\\](?=\\s*[,)])");

    private static final Pattern FLOAT_SUFFIX =
            Pattern.compile(
                    "(?<![\\w.])((?:\\d+\\.\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?|\\d+[eE][-+]?\\d+)(?:[lL]|[fF]16)\\b");

    // --- [ARGUMENTS]

    sealed interface Argument permits Header, Include, Define, Macros {
        static Report.Result<Argument> of(String token) {
            return switch (token) {
                case String include when include.startsWith("-I") ->
                        Optional.of(Path.of(include.substring(2)).toAbsolutePath().normalize())
                                .filter(Files::isDirectory)
                                .<Report.Result<Argument>>map(
                                        path -> new Report.Success<>(new Include(path)))
                                .orElseGet(
                                        () ->
                                                new Report.Failure<>(
                                                        "Include `%s` names no directory"
                                                                .formatted(include)));
                case String define when DEFINE.matcher(define).matches() ->
                        new Report.Success<>(new Define(define));
                case String header when !header.startsWith("-") ->
                        Optional.of(Path.of(header).toAbsolutePath().normalize())
                                .filter(Files::isRegularFile)
                                .<Report.Result<Argument>>map(
                                        path -> new Report.Success<>(new Header(path)))
                                .orElseGet(
                                        () ->
                                                new Report.Failure<>(
                                                        "Header `%s` names no file"
                                                                .formatted(header)));
                default ->
                        new Report.Failure<>(
                                ("Argument `%s` is no header, `-I<dir>`,"
                                                + " `-D<name>[=<value>]`, or `-imacros <file>`")
                                        .formatted(token));
            };
        }
    }

    record Header(Path file) implements Argument {
        Report.Result<String> parse(Path source, PreProcessor cpp, CParser parser) {
            ByteArrayOutputStream output = new ByteArrayOutputStream();
            cpp.setOutputStream(output);
            try {
                if (!cpp.parse(source.toString())) {
                    return new Report.Failure<>("failed %s\nFile is unreadable".formatted(file));
                }
                parser.setParseFileName(file.toString());
                String pointers =
                        ARRAY_PARAMETER
                                .matcher(output.toString(StandardCharsets.ISO_8859_1))
                                .replaceAll("$1 *");
                parser.parse(
                        new ByteArrayInputStream(
                                FLOAT_SUFFIX
                                        .matcher(pointers)
                                        .replaceAll("$1")
                                        .getBytes(StandardCharsets.ISO_8859_1)));
                return new Report.Success<>("parsed " + file);
            } catch (ParseException
                    | ghidra.app.util.cparser.CPP.ParseException
                    | TokenMgrError exception) {
                return new Report.Failure<>("failed " + file + "\n" + exception.getMessage());
            }
        }
    }

    record Include(Path directory) implements Argument {
        Path copy(Path overlay, Path file) {
            return overlay.resolve(directory.relativize(file));
        }

        Optional<Path> source(Path overlay, Path header) {
            return Optional.of(header)
                    .filter(file -> file.startsWith(directory))
                    .map(file -> copy(overlay, file))
                    .filter(Files::isRegularFile);
        }

        void patch(Path overlay, List<Path> includes, List<Path> later) throws IOException {
            try (Stream<Path> files =
                    Files.find(directory, Integer.MAX_VALUE, (_, file) -> file.isRegularFile())) {
                for (Path file : files.toList()) {
                    String text = Files.readString(file, StandardCharsets.ISO_8859_1);
                    String resolved =
                            resolve(
                                    text,
                                    Stream.concat(includes.stream(), Stream.of(file.getParent()))
                                            .toList(),
                                    later);
                    if (!resolved.equals(text)) {
                        Path target = copy(overlay, file);
                        Files.createDirectories(target.getParent());
                        Files.writeString(target, resolved, StandardCharsets.ISO_8859_1);
                    }
                }
            }
        }

        private static String resolve(String text, List<Path> current, List<Path> later) {
            String checked =
                    HAS_INCLUDE
                            .matcher(text)
                            .replaceAll(
                                    check -> {
                                        String name = check.group(2);
                                        List<Path> searched =
                                                check.group(1) == null ? current : later;
                                        boolean found =
                                                searched.stream()
                                                        .map(Path::toString)
                                                        .anyMatch(
                                                                path ->
                                                                        CParserUtils.getFile(
                                                                                        path, name)
                                                                                != null);
                                        return found ? "1" : "0";
                                    });
            return SCOPED_CONDITION.matcher(checked).replaceAll("$1 0");
        }
    }

    record Define(String option) implements Argument {}

    record Macros(String defines) implements Argument {
        static Report.Result<Argument> read(String file) {
            try {
                return new Report.Success<>(new Macros(Files.readString(Path.of(file))));
            } catch (IOException exception) {
                return new Report.Failure<>(
                        "Macros file `%s` is unreadable: %s".formatted(file, exception));
            }
        }
    }

    private static Stream<Report.Result<Argument>> arguments(List<String> tokens) {
        return tokens.isEmpty()
                ? Stream.empty()
                : "-imacros".equals(tokens.getFirst()) && tokens.size() > 1
                        ? Stream.concat(
                                Stream.of(Macros.read(tokens.get(1))),
                                arguments(tokens.subList(2, tokens.size())))
                        : Stream.concat(
                                Stream.of(Argument.of(tokens.getFirst())),
                                arguments(tokens.subList(1, tokens.size())));
    }

    private static <T extends Argument> Stream<T> select(List<Argument> arguments, Class<T> type) {
        return arguments.stream().filter(type::isInstance).map(type::cast);
    }

    // --- [PREPROCESSOR]

    static final class Defines extends DefineTable {
        private static final Pattern TOKEN =
                Pattern.compile(
                        "\"(?:\\\\.|[^\"\\\\])*\"?|'(?:\\\\.|[^'\\\\])*'?|[(),]|[^\"'(),]+");

        @Override
        public String getParams(StringBuffer buf, int start, char endChar) {
            if (start >= buf.length()) {
                return "";
            }
            String text = buf.substring(start);
            if (endChar == 0 && (start == 0 || buf.charAt(start - 1) == ',')) {
                return text;
            }
            List<MatchResult> tokens = TOKEN.matcher(text).results().toList();
            List<Integer> depths =
                    tokens.stream()
                            .map(
                                    token ->
                                            switch (token.group()) {
                                                case "(" -> 1;
                                                case ")" -> -1;
                                                default -> 0;
                                            })
                            .gather(Gatherers.scan(() -> 0, Integer::sum))
                            .toList();
            return IntStream.range(0, tokens.size())
                    .mapToObj(
                            index -> {
                                MatchResult token = tokens.get(index);
                                int depth = depths.get(index);
                                boolean close = ")".equals(token.group());
                                return close && depth < 0
                                                || depth == 0
                                                        && token.group()
                                                                .equals(String.valueOf(endChar))
                                        ? Optional.of(token.start())
                                        : close && depth == 0 && endChar == 0
                                                ? Optional.of(token.end())
                                                : Optional.<Integer>empty();
                            })
                    .flatMap(Optional::stream)
                    .findFirst()
                    .map(end -> text.substring(0, end))
                    .orElse(text);
        }
    }

    // --- [SIGNATURES]

    private Stream<Report.Result<String>> apply(FunctionDefinition definition) {
        return Stream.of(definition.getName(), "_" + definition.getName())
                .map(currentProgram.getSymbolTable()::getSymbols)
                .flatMap(CollectionUtils::asStream)
                .map(Symbol::getAddress)
                .map(currentProgram.getFunctionManager()::getFunctionAt)
                .filter(Objects::nonNull)
                .map(Functions::target)
                .distinct()
                .sorted(Functions.BY_ENTRY)
                .map(
                        function -> {
                            ApplyFunctionSignatureCmd command =
                                    new ApplyFunctionSignatureCmd(
                                            function.getEntryPoint(),
                                            definition,
                                            SourceType.USER_DEFINED);
                            return runCommand(command)
                                    ? new Report.Success<>(
                                            "applied %s %s"
                                                    .formatted(
                                                            function.getEntryPoint(),
                                                            function.getPrototypeString(
                                                                    true, false)))
                                    : new Report.Failure<>(
                                            "rejected %s %s: %s"
                                                    .formatted(
                                                            function.getEntryPoint(),
                                                            function.getName(true),
                                                            command.getStatusMsg()));
                        });
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        String usage =
                "usage: %s <report> <header>... [-I<dir>]... [-D<name>[=<value>]]... [-imacros <file>]..."
                        .formatted(getScriptName());
        List<String> args = List.of(getScriptArgs());
        if (args.isEmpty()) {
            throw new IllegalArgumentException(usage);
        }
        List<Report.Result<Argument>> results = arguments(args.subList(1, args.size())).toList();
        Stream<Report.Result<Argument>> missing =
                results.stream().flatMap(Report.Result::values).anyMatch(Header.class::isInstance)
                        ? Stream.empty()
                        : Stream.of(new Report.Failure<>("Arguments name no header"));
        List<Argument> arguments =
                Arguments.values(Stream.concat(results.stream(), missing).toList(), usage);
        Path overlay =
                Files.createTempDirectory(Application.getUserTempDirectory().toPath(), "headers");
        try {
            List<Include> includes = select(arguments, Include.class).toList();
            List<Path> directories = includes.stream().map(Include::directory).toList();
            List<Path> overlays =
                    IntStream.range(0, includes.size())
                            .mapToObj(index -> overlay.resolve(Integer.toString(index)))
                            .toList();
            PreProcessor cpp = new PreProcessor(InputStream.nullInputStream());
            Field defines = PreProcessor.class.getDeclaredField("defs");
            defines.setAccessible(true);
            defines.set(cpp, new Defines());
            cpp.setArgs(select(arguments, Define.class).map(Define::option).toArray(String[]::new));
            for (int index = 0; index < includes.size(); index++) {
                includes.get(index)
                        .patch(
                                overlays.get(index),
                                directories,
                                directories.subList(index + 1, directories.size()));
                cpp.addIncludePath(overlays.get(index).toString());
                cpp.addIncludePath(directories.get(index).toString());
            }
            cpp.setMonitor(monitor);
            cpp.setOutputStream(OutputStream.nullOutputStream());
            String prelude =
                    Stream.concat(
                                    select(arguments, Macros.class).map(Macros::defines),
                                    Stream.of(
                                                    "__has_cpp_attribute(x) 0",
                                                    "__has_builtin(x) 0",
                                                    "__has_feature(x) 0",
                                                    "__has_attribute(x) 0",
                                                    "__has_extension(x) 0",
                                                    "__has_include(x) 1",
                                                    "__builtin_va_list void *",
                                                    "__int128_t int16",
                                                    "__uint128_t uint16",
                                                    "_Float16 float2",
                                                    "__FLT_EVAL_METHOD__ 0",
                                                    "restrict",
                                                    "__CF_ENUM_FIXED_IS_AVAILABLE 0")
                                            .map("#define "::concat))
                            .collect(Collectors.joining("\n", "", "\n"));
            cpp.ReInit(new ByteArrayInputStream(prelude.getBytes(StandardCharsets.UTF_8)));
            cpp.Input();
            CParser parser = new CParser(currentProgram.getDataTypeManager(), true, null);
            parser.setMonitor(monitor);
            List<Report.Result<String>> parsed =
                    select(arguments, Header.class)
                            .map(
                                    header ->
                                            header.parse(
                                                    IntStream.range(0, includes.size())
                                                            .mapToObj(
                                                                    index ->
                                                                            includes.get(index)
                                                                                    .source(
                                                                                            overlays
                                                                                                    .get(
                                                                                                            index),
                                                                                            header
                                                                                                    .file()))
                                                            .flatMap(Optional::stream)
                                                            .findFirst()
                                                            .orElse(header.file()),
                                                    cpp,
                                                    parser))
                            .toList();
            cpp.getDefinitions().populateDefineEquates(null, currentProgram.getDataTypeManager());
            List<Report.Result<String>> applied =
                    parser.getFunctions().values().stream()
                            .sorted(Comparator.comparing(DataType::getName))
                            .filter(FunctionDefinition.class::isInstance)
                            .map(FunctionDefinition.class::cast)
                            .flatMap(this::apply)
                            .toList();
            long failed = parsed.stream().flatMap(Report.Result::failures).count();
            long rejected = applied.stream().flatMap(Report.Result::failures).count();
            String counts =
                    "headers=%d failed=%d types=%d functions=%d applied=%d rejected=%d args=%s"
                            .formatted(
                                    parsed.size(),
                                    failed,
                                    Stream.of(
                                                    parser.getComposites(),
                                                    parser.getEnums(),
                                                    parser.getTypes())
                                            .mapToInt(Map::size)
                                            .sum(),
                                    parser.getFunctions().size(),
                                    applied.size() - rejected,
                                    rejected,
                                    String.join(" ", args.subList(1, args.size())));
            Function<Report.Result<String>, Stream<String>> text =
                    result -> Stream.concat(result.values(), result.failures());
            Stream<String> body =
                    Stream.of(
                                    Map.entry("HEADERS", parsed.stream().flatMap(text)),
                                    Map.entry("PREPROCESSOR", Stream.of(cpp.getParseMessages())),
                                    Map.entry("PARSER", Stream.of(parser.getParseMessages())),
                                    Map.entry("APPLIED", applied.stream().flatMap(text)))
                            .flatMap(
                                    section ->
                                            Stream.concat(
                                                    Stream.of(Report.section(section.getKey())),
                                                    section.getValue()
                                                            .flatMap(String::lines)
                                                            .map("// "::concat)));
            println(Report.write(Path.of(args.getFirst()), currentProgram, counts, body));
        } finally {
            FileUtilities.deleteDir(overlay);
        }
    }
}
