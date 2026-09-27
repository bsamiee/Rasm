import ghidra.app.cmd.function.ApplyFunctionSignatureCmd;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.cparser.C.CParser;
import ghidra.app.util.cparser.C.ParseException;
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
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Headers extends GhidraScript {
    // --- [PREPROCESSOR]

    private static final Pattern DEFINE = Pattern.compile("-D[A-Za-z_]\\w*(=.*)?");
    private static final String INCLUDE = "-I";
    private static final String IMACROS = "-imacros";
    private static final Path CDEFS = Path.of("sys", "cdefs.h");
    private static final Pattern SCOPED_CONDITION =
            Pattern.compile("^(\\s*#\\s*(?:el)?if)\\b.*::.*$", Pattern.MULTILINE);
    private static final String BUILTINS =
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
                            "__const",
                            "restrict",
                            "__restrict",
                            "__CF_ENUM_FIXED_IS_AVAILABLE 0")
                    .map("#define "::concat)
                    .collect(Collectors.joining("\n", "", "\n"));

    private static List<Path> includePath(Path patched, List<Path> includes) throws IOException {
        Optional<Path> cdefs =
                includes.stream()
                        .map(directory -> directory.resolve(CDEFS))
                        .filter(Files::isRegularFile)
                        .findFirst();
        if (cdefs.isPresent()) {
            Path copy = patched.resolve(CDEFS);
            Files.createDirectories(copy.getParent());
            Files.writeString(
                    copy,
                    SCOPED_CONDITION.matcher(Files.readString(cdefs.get())).replaceAll("$1 0"));
        }
        return Stream.concat(cdefs.map(_ -> patched).stream(), includes.stream()).toList();
    }

    private static void predefine(PreProcessor cpp, InputStream defines)
            throws ghidra.app.util.cparser.CPP.ParseException {
        cpp.ReInit(defines);
        cpp.Input();
    }

    // --- [ARGUMENTS]

    sealed interface Argument permits Header, Include, Define, Macros {}

    record Header(Path file) implements Argument {}

    record Include(Path directory) implements Argument {}

    record Define(String option) implements Argument {}

    record Macros(Path file) implements Argument {}

    private static Stream<Report.Result<Argument>> arguments(List<String> tokens) {
        return tokens.isEmpty()
                ? Stream.empty()
                : IMACROS.equals(tokens.getFirst()) && tokens.size() > 1
                        ? Stream.concat(
                                Stream.of(macros(tokens.get(1))),
                                arguments(tokens.subList(2, tokens.size())))
                        : Stream.concat(
                                Stream.of(argument(tokens.getFirst())),
                                arguments(tokens.subList(1, tokens.size())));
    }

    private static Report.Result<Argument> argument(String token) {
        return switch (token) {
            case String header when !header.startsWith("-") && isReadable(Path.of(header)) ->
                    new Report.Success<>(new Header(Path.of(header)));
            case String define when DEFINE.matcher(define).matches() ->
                    new Report.Success<>(new Define(define));
            case String include
                    when include.startsWith(INCLUDE)
                            && Files.isDirectory(Path.of(include.substring(INCLUDE.length()))) ->
                    new Report.Success<>(new Include(Path.of(include.substring(INCLUDE.length()))));
            default ->
                    new Report.Failure<>(
                            ("Argument `%s` names no readable header, `-I<dir>`,"
                                            + " `-D<name>[=<value>]`, or `-imacros <file>`")
                                    .formatted(token));
        };
    }

    private static Report.Result<Argument> macros(String file) {
        return isReadable(Path.of(file))
                ? new Report.Success<>(new Macros(Path.of(file)))
                : new Report.Failure<>("Macros file `%s` is not readable".formatted(file));
    }

    private static boolean isReadable(Path path) {
        return Files.isRegularFile(path) && Files.isReadable(path);
    }

    // --- [PARSE]

    private static Report.Result<String> parse(Path header, PreProcessor cpp, CParser parser) {
        ByteArrayOutputStream source = new ByteArrayOutputStream();
        cpp.setOutputStream(source);
        try {
            cpp.parse(header.toString());
            parser.setParseFileName(header.toString());
            parser.parse(new ByteArrayInputStream(source.toByteArray()));
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
        Path patched =
                Files.createTempDirectory(Application.getUserTempDirectory().toPath(), "headers");
        try {
            PreProcessor cpp = new PreProcessor(InputStream.nullInputStream());
            cpp.setArgs(
                    arguments.stream()
                            .filter(Define.class::isInstance)
                            .map(Define.class::cast)
                            .map(Define::option)
                            .toArray(String[]::new));
            List<Path> includes =
                    arguments.stream()
                            .filter(Include.class::isInstance)
                            .map(Include.class::cast)
                            .map(Include::directory)
                            .toList();
            includePath(patched, includes)
                    .forEach(directory -> cpp.addIncludePath(directory.toString()));
            cpp.setMonitor(monitor);
            cpp.setOutputStream(OutputStream.nullOutputStream());
            for (Argument argument : arguments) {
                if (argument instanceof Macros(Path file)) {
                    try (InputStream macros = Files.newInputStream(file)) {
                        predefine(cpp, macros);
                    }
                }
            }
            predefine(cpp, new ByteArrayInputStream(BUILTINS.getBytes(StandardCharsets.UTF_8)));
            CParser parser = new CParser(currentProgram.getDataTypeManager(), true, null);
            parser.setMonitor(monitor);
            List<Report.Result<String>> parsed =
                    arguments.stream()
                            .filter(Header.class::isInstance)
                            .map(Header.class::cast)
                            .map(header -> parse(header.file(), cpp, parser))
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
            FileUtilities.deleteDir(patched);
        }
    }
}
