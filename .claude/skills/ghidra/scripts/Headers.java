// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.cmd.function.ApplyFunctionSignatureCmd;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.cparser.C.CParser;
import ghidra.app.util.cparser.C.ParseException;
import ghidra.app.util.cparser.CPP.PreProcessor;
import ghidra.app.util.cparser.CPP.TokenMgrError;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.FunctionDefinition;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;

import util.CollectionUtils;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class Headers extends GhidraScript {
    private static final Pattern DEFINE = Pattern.compile("-D[A-Za-z_]\\w*(=.*)?");
    private static final String INCLUDE = "-I";
    private static final String MACROS = "-imacros";
    private static final Path CDEFS = Path.of("sys", "cdefs.h");
    private static final String SHIMS =
            Stream.of(
                            "__has_cpp_attribute(x) 0",
                            "__has_builtin(x) 0",
                            "__has_feature(x) 0",
                            "__has_attribute(x) 0",
                            "__has_extension(x) 0",
                            "__has_include(x) 1",
                            "__builtin_va_list void *",
                            "__const",
                            "restrict",
                            "__restrict",
                            "__CF_ENUM_FIXED_IS_AVAILABLE 0")
                    .map("#define "::concat)
                    .collect(Collectors.joining("\n", "", "\n"));

    sealed interface Argument permits Header, Include, Define, Macros {}

    record Header(String file) implements Argument {}

    record Include(Path dir) implements Argument {}

    record Define(String option) implements Argument {}

    record Macros(Path file) implements Argument {}

    @Override
    public void run() throws Exception {
        String usage =
                "usage: %s <report> <header>... [-I<dir>]... [-D<name>[=<value>]]... [-imacros <file>]..."
                        .formatted(getScriptName());
        List<String> args = List.of(getScriptArgs());
        List<Argument> arguments = Arguments.values(parse(args), usage);
        List<String> headers =
                arguments.stream()
                        .filter(Header.class::isInstance)
                        .map(Header.class::cast)
                        .map(Header::file)
                        .toList();
        List<Path> includes =
                arguments.stream()
                        .filter(Include.class::isInstance)
                        .map(Include.class::cast)
                        .map(Include::dir)
                        .toList();
        Stream<String> defines =
                arguments.stream()
                        .filter(Define.class::isInstance)
                        .map(Define.class::cast)
                        .map(Define::option);
        Path shadow = Files.createTempDirectory("cdefs");
        try {
            PreProcessor cpp = new PreProcessor(InputStream.nullInputStream());
            cpp.setArgs(
                    Stream.concat(
                                    includePath(shadow, includes).stream()
                                            .map(dir -> INCLUDE + dir),
                                    defines)
                            .toArray(String[]::new));
            cpp.setMonitor(monitor);
            cpp.setOutputStream(OutputStream.nullOutputStream());
            for (Argument argument : arguments) {
                if (argument instanceof Macros(Path file)) {
                    try (InputStream macros = Files.newInputStream(file)) {
                        predefine(cpp, macros);
                    }
                }
            }
            predefine(cpp, new ByteArrayInputStream(SHIMS.getBytes(StandardCharsets.UTF_8)));
            CParser parser = new CParser(currentProgram.getDataTypeManager(), true, null);
            parser.setMonitor(monitor);
            List<Arguments.Result<String>> parsed =
                    headers.stream().map(header -> parse(header, cpp, parser)).toList();
            cpp.getDefinitions().populateDefineEquates(null, currentProgram.getDataTypeManager());
            List<Arguments.Result<String>> applied =
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
            Files.deleteIfExists(shadow.resolve(CDEFS));
            Files.deleteIfExists(shadow.resolve(CDEFS.getParent()));
            Files.deleteIfExists(shadow);
        }
    }

    // --- [ARGUMENTS] -------------------------------------------------------------------

    private static List<Arguments.Result<Argument>> parse(List<String> args) {
        if (args.isEmpty()) {
            return List.of(new Arguments.Failure<>("no report given"));
        }
        List<String> remaining = args.subList(1, args.size());
        List<String> tokens =
                IntStream.range(0, remaining.size())
                        .filter(at -> at == 0 || !MACROS.equals(remaining.get(at - 1)))
                        .mapToObj(
                                at ->
                                        MACROS.equals(remaining.get(at))
                                                        && at + 1 < remaining.size()
                                                ? MACROS + remaining.get(at + 1)
                                                : remaining.get(at))
                        .toList();
        Stream<Arguments.Result<Argument>> missing =
                tokens.stream().anyMatch(token -> !token.startsWith("-"))
                        ? Stream.empty()
                        : Stream.of(new Arguments.Failure<>("no header given"));
        return Stream.concat(tokens.stream().map(Headers::argument), missing).toList();
    }

    private static Arguments.Result<Argument> argument(String token) {
        return switch (token) {
            case String header when !header.startsWith("-") && isReadable(Path.of(header)) ->
                    new Arguments.Success<>(new Header(header));
            case String define when DEFINE.matcher(define).matches() ->
                    new Arguments.Success<>(new Define(define));
            case String include
                    when include.startsWith(INCLUDE)
                            && Files.isDirectory(Path.of(include.substring(INCLUDE.length()))) ->
                    new Arguments.Success<>(
                            new Include(Path.of(include.substring(INCLUDE.length()))));
            case String macros
                    when macros.startsWith(MACROS)
                            && isReadable(Path.of(macros.substring(MACROS.length()))) ->
                    new Arguments.Success<>(new Macros(Path.of(macros.substring(MACROS.length()))));
            default ->
                    new Arguments.Failure<>(
                            token
                                    + ": not a readable header, -I<dir>, -D<name>[=<value>], or"
                                    + " -imacros <file>");
        };
    }

    private static boolean isReadable(Path path) {
        return Files.isRegularFile(path) && Files.isReadable(path);
    }

    // --- [PREPROCESSOR] ----------------------------------------------------------------

    private static List<Path> includePath(Path shadow, List<Path> includes) throws IOException {
        Optional<Path> source =
                includes.stream()
                        .map(dir -> dir.resolve(CDEFS))
                        .filter(Files::isRegularFile)
                        .findFirst();
        if (source.isEmpty()) {
            return includes;
        }
        Path copy = shadow.resolve(CDEFS);
        Files.createDirectories(copy.getParent());
        Files.write(
                copy, Files.readAllLines(source.get()).stream().map(Headers::neutralized).toList());
        return Stream.concat(Stream.of(shadow), includes.stream()).toList();
    }

    private static String neutralized(String line) {
        String directive = line.stripLeading();
        boolean conditional = directive.startsWith("#if ") || directive.startsWith("#elif ");
        return conditional && line.contains("::")
                ? directive.substring(0, directive.indexOf(' ')) + " 0"
                : line;
    }

    private static void predefine(PreProcessor cpp, InputStream defines)
            throws ghidra.app.util.cparser.CPP.ParseException {
        cpp.ReInit(defines);
        cpp.Input();
    }

    // --- [PARSE] -----------------------------------------------------------------------

    private static Arguments.Result<String> parse(String header, PreProcessor cpp, CParser parser) {
        ByteArrayOutputStream source = new ByteArrayOutputStream();
        cpp.setOutputStream(source);
        try {
            cpp.parse(header);
            parser.setParseFileName(header);
            parser.parse(new ByteArrayInputStream(source.toByteArray()));
            return new Arguments.Success<>("parsed " + header);
        } catch (ParseException
                | ghidra.app.util.cparser.CPP.ParseException
                | TokenMgrError exception) {
            return new Arguments.Failure<>("failed " + header + "\n" + exception.getMessage());
        }
    }

    // --- [APPLY] -----------------------------------------------------------------------

    private Stream<Arguments.Result<String>> apply(FunctionDefinition definition) {
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

    private Arguments.Result<String> apply(Function function, FunctionDefinition definition) {
        ApplyFunctionSignatureCmd command =
                new ApplyFunctionSignatureCmd(
                        function.getEntryPoint(), definition, SourceType.USER_DEFINED);
        return runCommand(command)
                ? new Arguments.Success<>(
                        "applied %s %s"
                                .formatted(
                                        function.getEntryPoint(),
                                        function.getPrototypeString(true, false)))
                : new Arguments.Failure<>(
                        "rejected %s %s: %s"
                                .formatted(
                                        function.getEntryPoint(),
                                        function.getName(true),
                                        command.getStatusMsg()));
    }

    // --- [WRITE] -----------------------------------------------------------------------

    private String write(
            Path out,
            PreProcessor cpp,
            CParser parser,
            List<Arguments.Result<String>> parsed,
            List<Arguments.Result<String>> applied,
            String arguments)
            throws IOException {
        long failed = parsed.stream().flatMap(Arguments.Result::failures).count();
        long rejected = applied.stream().flatMap(Arguments.Result::failures).count();
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
                                Stream.of(Report.divider("HEADERS")),
                                parsed.stream().flatMap(Headers::rows),
                                Stream.of(Report.divider("PREPROCESSOR")),
                                comments(cpp.getParseMessages()),
                                Stream.of(Report.divider("PARSER")),
                                comments(parser.getParseMessages()),
                                Stream.of(Report.divider("APPLIED")),
                                applied.stream().flatMap(Headers::rows))
                        .flatMap(section -> section);
        return Report.write(out, currentProgram, counts, body);
    }

    private static Stream<String> rows(Arguments.Result<String> result) {
        return Stream.concat(result.values(), result.failures()).flatMap(Headers::comments);
    }

    private static Stream<String> comments(String text) {
        return text.lines().map("// "::concat);
    }
}
