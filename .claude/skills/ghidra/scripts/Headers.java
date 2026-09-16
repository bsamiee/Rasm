// Parses C headers into the program and applies their prototypes to the functions they name

// --- [IMPORTS] -------------------------------------------------------------------------

import ghidra.app.cmd.function.ApplyFunctionSignatureCmd;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.cparser.C.CParser;
import ghidra.app.util.cparser.C.ParseException;
import ghidra.app.util.cparser.CPP.PreProcessor;
import ghidra.app.util.cparser.CPP.TokenMgrError;
import ghidra.app.util.opinion.MachoLoader;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.FunctionDefinition;
import ghidra.program.model.lang.Language;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;
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
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [SCRIPT] --------------------------------------------------------------------------

public class Headers extends GhidraScript {
    private static final Pattern DEFINE = Pattern.compile("-D[A-Za-z_]\\w*(=.*)?");
    private static final Pattern INCLUDE = Pattern.compile("-I.+");

    @Override
    public void run() throws Exception {
        String usage =
                "usage: %s <report> <header>... [-I<dir>]... [-D<name>[=<value>]]..."
                        .formatted(getScriptName());
        List<String> args = List.of(getScriptArgs());
        if (args.isEmpty()) {
            throw new IllegalArgumentException(usage);
        }
        List<String> rest = args.subList(1, args.size());
        Map<Boolean, List<String>> split =
                rest.stream().collect(Collectors.partitioningBy(arg -> arg.startsWith("-")));
        List<Job.Result<String>> options = split.get(true).stream().map(Headers::option).toList();
        List<Job.Result<String>> headers =
                split.get(false).isEmpty()
                        ? List.of(new Job.Problem<>("no header given"))
                        : split.get(false).stream().map(Headers::header).toList();
        List<String> problems =
                Stream.concat(headers.stream(), options.stream())
                        .flatMap(Job.Result::problems)
                        .toList();
        if (!problems.isEmpty()) {
            throw new IllegalArgumentException(String.join("\n", problems) + "\n" + usage);
        }
        PreProcessor cpp = new PreProcessor(InputStream.nullInputStream());
        cpp.setArgs(options.stream().flatMap(Job.Result::values).toArray(String[]::new));
        cpp.setMonitor(monitor);
        cpp.setOutputStream(OutputStream.nullOutputStream());
        cpp.ReInit(
                new ByteArrayInputStream(prelude(currentProgram).getBytes(StandardCharsets.UTF_8)));
        cpp.Input();
        CParser parser = new CParser(currentProgram.getDataTypeManager(), true, null);
        parser.setMonitor(monitor);
        List<Job.Result<String>> units =
                headers.stream()
                        .flatMap(Job.Result::values)
                        .map(header -> parse(header, cpp, parser))
                        .toList();
        cpp.getDefinitions().populateDefineEquates(null, currentProgram.getDataTypeManager());
        List<Job.Result<String>> applied =
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
                        units,
                        applied,
                        String.join(" ", rest)));
    }

    // --- [ARGUMENTS] -------------------------------------------------------------------

    private static Job.Result<String> option(String arg) {
        return switch (arg) {
            case String define when DEFINE.matcher(define).matches() -> new Job.Ok<>(define);
            case String include
                    when INCLUDE.matcher(include).matches()
                            && Files.isDirectory(Path.of(include.substring(2))) ->
                    new Job.Ok<>(include);
            default -> new Job.Problem<>(arg + ": not -I<dir> or -D<name>[=<value>]");
        };
    }

    private static Job.Result<String> header(String arg) {
        Path path = Path.of(arg);
        return Files.isReadable(path) && Files.isRegularFile(path)
                ? new Job.Ok<>(arg)
                : new Job.Problem<>(arg + ": not a readable file");
    }

    // Predefines the program answers and parser shims, a -D on the command defined earlier wins
    private static String prelude(Program program) {
        Language language = program.getLanguage();
        // CFAvailability.h's fixed enum switch evaluates true here, CParser reads no such enum
        Stream<String> platform =
                MachoLoader.MACH_O_NAME.equals(program.getExecutableFormat())
                        ? Stream.of(
                                "__APPLE__ 1",
                                "__MACH__ 1",
                                "__APPLE_CC__ 6000",
                                "_DARWIN_C_SOURCE 1",
                                "__CF_ENUM_FIXED_IS_AVAILABLE 0")
                        : Stream.of();
        Stream<String> target =
                switch (language.getProcessor().toString()) {
                    case "AARCH64" -> Stream.of("__arm64__ 1", "__aarch64__ 1");
                    case "x86" ->
                            Stream.of(
                                    program.getDefaultPointerSize() == Long.BYTES
                                            ? "__x86_64__ 1"
                                            : "__i386__ 1");
                    default -> Stream.of();
                };
        Stream<String> model =
                Stream.of(
                        program.getDefaultPointerSize() == Long.BYTES
                                ? "__LP64__ 1"
                                : "__ILP32__ 1",
                        language.isBigEndian() ? "__BIG_ENDIAN__ 1" : "__LITTLE_ENDIAN__ 1");
        Stream<String> shims =
                Stream.of(
                        "__GNUC__ 4",
                        "__builtin_va_list void *",
                        "__const",
                        "restrict",
                        "__restrict",
                        "__has_cpp_attribute(x) 0",
                        "__has_builtin(x) 0",
                        "__has_feature(x) 0",
                        "__has_attribute(x) 0",
                        "__has_extension(x) 0");
        return Stream.of(platform, target, model, shims)
                .flatMap(group -> group)
                .map("#define "::concat)
                .collect(Collectors.joining("\n", "", "\n"));
    }

    // --- [PARSE] -----------------------------------------------------------------------

    private static Job.Result<String> parse(String header, PreProcessor cpp, CParser parser) {
        ByteArrayOutputStream source = new ByteArrayOutputStream();
        cpp.setOutputStream(source);
        try {
            cpp.parse(header);
            parser.setParseFileName(header);
            parser.parse(new ByteArrayInputStream(source.toByteArray()));
            return new Job.Ok<>("parsed " + header);
        } catch (ParseException | ghidra.app.util.cparser.CPP.ParseException | TokenMgrError e) {
            return new Job.Problem<>("failed " + header + "\n" + e.getMessage());
        }
    }

    // --- [APPLY] -----------------------------------------------------------------------

    private Stream<Job.Result<String>> apply(FunctionDefinition definition) {
        return Stream.of(definition.getName(), "_" + definition.getName())
                .map(currentProgram.getSymbolTable()::getSymbols)
                .flatMap(CollectionUtils::asStream)
                .map(Symbol::getAddress)
                .map(currentProgram.getFunctionManager()::getFunctionAt)
                .filter(Objects::nonNull)
                .map(function -> function.isThunk() ? function.getThunkedFunction(true) : function)
                .distinct()
                .sorted(Comparator.comparing(Function::getEntryPoint))
                .map(function -> apply(function, definition));
    }

    private Job.Result<String> apply(Function function, FunctionDefinition definition) {
        ApplyFunctionSignatureCmd command =
                new ApplyFunctionSignatureCmd(
                        function.getEntryPoint(), definition, SourceType.USER_DEFINED);
        return runCommand(command)
                ? new Job.Ok<>(
                        "applied %s %s"
                                .formatted(
                                        function.getEntryPoint(),
                                        function.getPrototypeString(true, false)))
                : new Job.Problem<>(
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
            List<Job.Result<String>> units,
            List<Job.Result<String>> applied,
            String request)
            throws IOException {
        long failed = units.stream().flatMap(Job.Result::problems).count();
        long rejected = applied.stream().flatMap(Job.Result::problems).count();
        int types =
                Stream.of(parser.getComposites(), parser.getEnums(), parser.getTypes())
                        .mapToInt(Map::size)
                        .sum();
        String counts =
                "headers=%d failed=%d types=%d functions=%d applied=%d rejected=%d"
                        .formatted(
                                units.size(),
                                failed,
                                types,
                                parser.getFunctions().size(),
                                applied.size() - rejected,
                                rejected);
        List<String> lines =
                Stream.of(
                                Job.index(currentProgram, counts + " args=" + request),
                                Stream.of(Job.divider("HEADERS")),
                                units.stream().flatMap(Headers::rows),
                                Stream.of(Job.divider("PREPROCESSOR")),
                                comments(cpp.getParseMessages()),
                                Stream.of(Job.divider("PARSER")),
                                comments(parser.getParseMessages()),
                                Stream.of(Job.divider("APPLIED")),
                                applied.stream().flatMap(Headers::rows))
                        .flatMap(section -> section)
                        .toList();
        Files.createDirectories(out.toAbsolutePath().getParent());
        Files.write(out, lines);
        return "wrote " + out + ": " + counts;
    }

    private static Stream<String> rows(Job.Result<String> result) {
        return Stream.concat(result.values(), result.problems()).flatMap(Headers::comments);
    }

    private static Stream<String> comments(String text) {
        return text.lines().map("// "::concat);
    }
}
