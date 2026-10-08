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
import ghidra.program.model.listing.Function;
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
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.regex.MatchResult;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Headers extends GhidraScript {
    // --- [ARGUMENTS]

    sealed interface Argument permits Header, Include, Define, Macros {}

    record Header(Path file) implements Argument {}

    record Include(Path directory) implements Argument {}

    record Define(String option) implements Argument {}

    record Macros(String defines) implements Argument {}

    private static Stream<Report.Result<Argument>> arguments(List<String> tokens) {
        return tokens.isEmpty()
                ? Stream.empty()
                : "-imacros".equals(tokens.getFirst()) && tokens.size() > 1
                        ? Stream.concat(
                                Stream.of(macros(tokens.get(1))),
                                arguments(tokens.subList(2, tokens.size())))
                        : Stream.concat(
                                Stream.of(argument(tokens.getFirst())),
                                arguments(tokens.subList(1, tokens.size())));
    }

    private static Report.Result<Argument> argument(String token) {
        return switch (token) {
            case String include when include.startsWith("-I") -> include(include.substring(2));
            case String define when define.matches("-D[A-Za-z_]\\w*(=.*)?") ->
                    new Report.Success<>(new Define(define));
            case String header when !header.startsWith("-") -> header(header);
            default ->
                    new Report.Failure<>(
                            ("Argument `%s` is no header, `-I<dir>`,"
                                            + " `-D<name>[=<value>]`, or `-imacros <file>`")
                                    .formatted(token));
        };
    }

    private static Report.Result<Argument> header(String file) {
        Path path = Path.of(file).toAbsolutePath().normalize();
        return Files.isRegularFile(path)
                ? new Report.Success<>(new Header(path))
                : new Report.Failure<>("Header `%s` names no file".formatted(file));
    }

    private static Report.Result<Argument> include(String directory) {
        Path path = Path.of(directory).toAbsolutePath().normalize();
        return Files.isDirectory(path)
                ? new Report.Success<>(new Include(path))
                : new Report.Failure<>("Include `-I%s` names no directory".formatted(directory));
    }

    private static Report.Result<Argument> macros(String file) {
        try {
            return new Report.Success<>(new Macros(Files.readString(Path.of(file))));
        } catch (IOException exception) {
            return new Report.Failure<>(
                    "Macros file `%s` is unreadable: %s".formatted(file, exception));
        }
    }

    private static <T extends Argument> Stream<T> select(List<Argument> arguments, Class<T> type) {
        return arguments.stream().filter(type::isInstance).map(type::cast);
    }

    // --- [PREPROCESSOR]

    static final class Defines extends DefineTable {
        /** Returns macro argument text at {@code start}, remaining text for a variadic argument */
        @Override
        public String getParams(StringBuffer buf, int start, char endChar) {
            return Optional.of(start)
                    .filter(index -> index < buf.length())
                    .map(buf::substring)
                    .map(
                            text ->
                                    endChar == 0 && (start == 0 || buf.charAt(start - 1) == ',')
                                            ? text
                                            : argument(text, endChar))
                    .orElse("");
        }

        private static String argument(String text, char endChar) {
            List<MatchResult> tokens =
                    Pattern.compile(
                                    "\"(?:\\\\.|[^\"\\\\])*\"?|'(?:\\\\.|[^'\\\\])*'?|[(),]|[^\"'(),]+")
                            .matcher(text)
                            .results()
                            .toList();
            int[] depths =
                    tokens.stream()
                            .mapToInt(
                                    token ->
                                            switch (token.group()) {
                                                case "(" -> 1;
                                                case ")" -> -1;
                                                default -> 0;
                                            })
                            .toArray();
            Arrays.parallelPrefix(depths, Integer::sum);
            return IntStream.range(0, tokens.size())
                    .mapToObj(index -> end(tokens.get(index), depths[index], endChar))
                    .flatMap(Optional::stream)
                    .findFirst()
                    .map(end -> text.substring(0, end))
                    .orElse(text);
        }

        private static Optional<Integer> end(MatchResult token, int depth, char endChar) {
            boolean close = ")".equals(token.group());
            return close && depth < 0 || depth == 0 && token.group().equals(String.valueOf(endChar))
                    ? Optional.of(token.start())
                    : close && depth == 0 && endChar == 0
                            ? Optional.of(token.end())
                            : Optional.empty();
        }
    }

    private static void patch(List<Path> overlays, List<Path> includes, int index)
            throws IOException {
        try (Stream<Path> files =
                Files.find(
                        includes.get(index),
                        Integer.MAX_VALUE,
                        (_, file) -> file.isRegularFile())) {
            for (Path file : files.toList()) {
                String text = Files.readString(file, StandardCharsets.ISO_8859_1);
                String resolved = resolve(text, file, includes, index);
                if (!resolved.equals(text)) {
                    Path copy = copy(overlays, includes, index, file);
                    Files.createDirectories(copy.getParent());
                    Files.writeString(copy, resolved, StandardCharsets.ISO_8859_1);
                }
            }
        }
    }

    private static Path copy(List<Path> overlays, List<Path> includes, int index, Path file) {
        return overlays.get(index).resolve(includes.get(index).relativize(file));
    }

    private static Path source(List<Path> overlays, List<Path> includes, Path header) {
        return IntStream.range(0, includes.size())
                .filter(index -> header.startsWith(includes.get(index)))
                .mapToObj(index -> copy(overlays, includes, index, header))
                .filter(Files::isRegularFile)
                .findFirst()
                .orElse(header);
    }

    private static String resolve(String text, Path file, List<Path> includes, int index) {
        List<Path> current = Stream.concat(includes.stream(), Stream.of(file.getParent())).toList();
        List<Path> later = includes.subList(index + 1, includes.size());
        return Pattern.compile("__has_include(_next)?\\s*\\(\\s*[<\"]([^>\"]+)[>\"]\\s*\\)")
                .matcher(text)
                .replaceAll(
                        check ->
                                found(check.group(1) == null ? current : later, check.group(2))
                                        ? "1"
                                        : "0")
                .replaceAll("(?m)^(\\s*#\\s*(?:el)?if)\\b.*::.*$", "$1 0");
    }

    private static boolean found(List<Path> directories, String name) {
        return directories.stream()
                .anyMatch(directory -> CParserUtils.getFile(directory.toString(), name) != null);
    }

    // --- [PARSE]

    private static Report.Result<String> parse(
            Path header, Path file, PreProcessor cpp, CParser parser) {
        ByteArrayOutputStream source = new ByteArrayOutputStream();
        cpp.setOutputStream(source);
        try {
            if (!cpp.parse(file.toString())) {
                return new Report.Failure<>("failed %s\nFile is unreadable".formatted(header));
            }
            parser.setParseFileName(header.toString());
            parser.parse(
                    new ByteArrayInputStream(
                            source.toString(StandardCharsets.ISO_8859_1)
                                    .replaceAll(
                                            "(\\*(?:\\s*(?:(?:__)?(?:const|volatile|restrict)|_Atomic)\\b)*)\\s*\\[[^\\]]*\\](?=\\s*[,)])",
                                            "$1 *")
                                    .replaceAll(
                                            "(?<![\\w.])((?:\\d+\\.\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?|\\d+[eE][-+]?\\d+)(?:[lL]|[fF]16)\\b",
                                            "$1")
                                    .getBytes(StandardCharsets.ISO_8859_1)));
            return new Report.Success<>("parsed " + header);
        } catch (ParseException
                | ghidra.app.util.cparser.CPP.ParseException
                | TokenMgrError exception) {
            return new Report.Failure<>("failed " + header + "\n" + exception.getMessage());
        }
    }

    // --- [APPLY]

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
                .map(function -> apply(function, definition));
    }

    private Report.Result<String> apply(Function function, FunctionDefinition definition) {
        ApplyFunctionSignatureCmd command =
                new ApplyFunctionSignatureCmd(
                        function.getEntryPoint(), definition, SourceType.USER_DEFINED);
        return runCommand(command)
                ? new Report.Success<>(
                        "applied %s %s"
                                .formatted(
                                        function.getEntryPoint(),
                                        function.getPrototypeString(true, false)))
                : new Report.Failure<>(
                        "rejected %s %s: %s"
                                .formatted(
                                        function.getEntryPoint(),
                                        function.getName(true),
                                        command.getStatusMsg()));
    }

    // --- [WRITE]

    private String write(
            Path out,
            PreProcessor cpp,
            CParser parser,
            List<Report.Result<String>> parsed,
            List<Report.Result<String>> applied,
            String arguments)
            throws IOException {
        long failed = parsed.stream().flatMap(Report.Result::failures).count();
        long rejected = applied.stream().flatMap(Report.Result::failures).count();
        int types =
                Stream.of(parser.getComposites(), parser.getEnums(), parser.getTypes())
                        .mapToInt(Map::size)
                        .sum();
        String counts =
                "headers=%d failed=%d types=%d functions=%d applied=%d rejected=%d args=%s"
                        .formatted(
                                parsed.size(),
                                failed,
                                types,
                                parser.getFunctions().size(),
                                applied.size() - rejected,
                                rejected,
                                arguments);
        Stream<String> body =
                Stream.of(
                                Stream.of(Report.section("HEADERS")),
                                parsed.stream().flatMap(Headers::rows),
                                Stream.of(Report.section("PREPROCESSOR")),
                                comments(cpp.getParseMessages()),
                                Stream.of(Report.section("PARSER")),
                                comments(parser.getParseMessages()),
                                Stream.of(Report.section("APPLIED")),
                                applied.stream().flatMap(Headers::rows))
                        .flatMap(section -> section);
        return Report.write(out, currentProgram, counts, body);
    }

    private static Stream<String> rows(Report.Result<String> result) {
        return Stream.concat(result.values(), result.failures()).flatMap(Headers::comments);
    }

    private static Stream<String> comments(String text) {
        return text.lines().map("// "::concat);
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
            List<Path> includes = select(arguments, Include.class).map(Include::directory).toList();
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
                patch(overlays, includes, index);
                cpp.addIncludePath(overlays.get(index).toString());
                cpp.addIncludePath(includes.get(index).toString());
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
                            .map(Header::file)
                            .map(
                                    header ->
                                            parse(
                                                    header,
                                                    source(overlays, includes, header),
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
            println(
                    write(
                            Path.of(args.getFirst()),
                            cpp,
                            parser,
                            parsed,
                            applied,
                            String.join(" ", args.subList(1, args.size()))));
        } finally {
            FileUtilities.deleteDir(overlay);
        }
    }
}
