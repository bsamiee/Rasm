import ghidra.app.cmd.function.ApplyFunctionSignatureCmd;
import ghidra.app.cmd.function.FunctionRenameOption;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.app.services.DataTypeManagerService;
import ghidra.app.util.NamespaceUtils;
import ghidra.app.util.cparser.C.CParserUtils;
import ghidra.app.util.cparser.C.ParseException;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.DataTypeConflictHandler;
import ghidra.program.model.data.DataTypeManager;
import ghidra.program.model.data.FunctionDefinitionDataType;
import ghidra.program.model.data.InvalidDataTypeException;
import ghidra.program.model.listing.CircularDependencyException;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Variable;
import ghidra.program.model.pcode.HighFunctionDBUtil;
import ghidra.program.model.pcode.HighSymbol;
import ghidra.program.model.symbol.Namespace;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;
import ghidra.program.model.symbol.SymbolType;
import ghidra.program.model.symbol.SymbolUtilities;
import ghidra.util.data.DataTypeParser;
import ghidra.util.data.DataTypeParser.AllowedDataTypes;
import ghidra.util.exception.CancelledException;
import ghidra.util.exception.DuplicateNameException;
import ghidra.util.exception.InvalidInputException;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import util.CollectionUtils;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.function.IntFunction;
import java.util.function.Predicate;
import java.util.regex.Pattern;
import java.util.regex.PatternSyntaxException;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Apply extends GhidraScript {
    // --- [ROWS]

    enum Kind {
        FUNCTION,
        LABEL
    }

    enum Step {
        NAMED,
        LABELED,
        TYPED,
        REMOVED,
        RENAMED
    }

    record Local(
            String name, Predicate<String> matcher, String replacement, Optional<String> type) {
        static Report.Result<Local> of(String at, String entry) {
            int split = entry.lastIndexOf('=');
            String name = (split < 0 ? entry : entry.substring(0, split)).strip();
            String[] named = (split < 0 ? "" : entry.substring(split + 1)).split(":", 2);
            try {
                return new Report.Success<>(
                        new Local(
                                name,
                                name.startsWith("re:")
                                        ? Pattern.compile(name.substring(3)).asMatchPredicate()
                                        : name::equals,
                                named[0].strip(),
                                named.length > 1
                                        ? Optional.of(named[1].strip())
                                        : Optional.empty()));
            } catch (PatternSyntaxException exception) {
                return new Report.Failure<>(
                        "%s locals regex `%s` did not parse (%s)"
                                .formatted(at, name, exception.getDescription()));
            }
        }

        boolean removes() {
            return replacement.isEmpty();
        }
    }

    record Row(
            Address address,
            Optional<String> namespace,
            Optional<String> name,
            Optional<String> prototype,
            Optional<String> comment,
            List<Local> locals,
            Kind kind) {}

    record Outcome(Step step, Address address, Report.Result<String> result) {
        String row() {
            return switch (result) {
                case Report.Success<String>(String text) -> address + " " + text;
                case Report.Failure<String>(String cause) ->
                        "%s %s: %s".formatted(address, step.name().toLowerCase(Locale.ROOT), cause);
            };
        }
    }

    private Report.Result<Row> row(Path file, int number, String line) {
        String[] cells = line.split("\t", -1);
        IntFunction<Optional<String>> cell =
                index ->
                        Optional.of(index < cells.length ? cells[index].strip() : "")
                                .filter(text -> !text.isEmpty() && !"-".equals(text));
        String at = "%s:%d".formatted(file, number);
        Report.Result<Address> address =
                Optional.ofNullable(currentProgram.getAddressFactory().getAddress(cells[0].strip()))
                        .<Report.Result<Address>>map(Report.Success::new)
                        .orElseGet(() ->
                                new Report.Failure<>(
                                        "%s `%s` names no address".formatted(at, cells[0])));
        Report.Result<Kind> kind =
                Arrays.stream(Kind.values())
                        .filter(each ->
                                each.name()
                                        .equalsIgnoreCase(
                                                cell.apply(6).orElse(Kind.FUNCTION.name())))
                        .findFirst()
                        .<Report.Result<Kind>>map(Report.Success::new)
                        .orElseGet(() ->
                                new Report.Failure<>(
                                        "%s kind `%s` is no row kind".formatted(at, cells[6])));
        Report.Result<List<Local>> locals =
                Report.all(
                        cell.apply(5).stream()
                                .flatMap(text -> Arrays.stream(text.split(";")))
                                .filter(Predicate.not(String::isBlank))
                                .map(entry -> Local.of(at, entry)));
        return address.zip(kind, Map::entry)
                .zip(
                        locals,
                        (start, parsed) ->
                                new Row(
                                        start.getKey(),
                                        cell.apply(1)
                                                .map(text ->
                                                        SymbolUtilities.replaceInvalidChars(
                                                                text, true)),
                                        cell.apply(2)
                                                .map(text ->
                                                        SymbolUtilities.replaceInvalidChars(
                                                                text, true)),
                                        cell.apply(3),
                                        cell.apply(4),
                                        parsed,
                                        start.getValue()));
    }

    // --- [NAMES]

    private Outcome name(Row row) {
        Step step = row.kind() == Kind.LABEL ? Step.LABELED : Step.NAMED;
        try {
            Namespace created =
                    row.namespace().isPresent()
                            ? NamespaceUtils.createNamespaceHierarchy(
                                    row.namespace().get(),
                                    null,
                                    currentProgram,
                                    SourceType.USER_DEFINED)
                            : currentProgram.getGlobalNamespace();
            Namespace namespace =
                    row.namespace().isPresent()
                                    && created.getSymbol().getSymbolType() == SymbolType.NAMESPACE
                            ? NamespaceUtils.convertNamespaceToClass(created)
                            : created;
            return switch (row.kind()) {
                case LABEL -> {
                    Symbol label =
                            currentProgram
                                    .getSymbolTable()
                                    .createLabel(
                                            row.address(),
                                            row.name()
                                                    .orElseThrow(() ->
                                                            new InvalidInputException(
                                                                    "Label row names no label")),
                                            namespace,
                                            SourceType.USER_DEFINED);
                    label.setPrimary();
                    row.comment().ifPresent(text -> setPlateComment(row.address(), text));
                    yield new Outcome(
                            step, row.address(), new Report.Success<>(label.getName(true)));
                }
                case FUNCTION -> {
                    Function function =
                            Optional.ofNullable(getFunctionAt(row.address()))
                                    .or(() ->
                                            Optional.ofNullable(
                                                    createFunction(row.address(), null)))
                                    .orElseThrow(() ->
                                            new InvalidInputException(
                                                    "No function starts or disassembles here"));
                    if (row.namespace().isPresent()) {
                        function.setParentNamespace(namespace);
                    }
                    if (row.name().isPresent()) {
                        function.setName(row.name().get(), SourceType.USER_DEFINED);
                    }
                    row.comment().ifPresent(function::setComment);
                    yield new Outcome(
                            step, row.address(), new Report.Success<>(function.getName(true)));
                }
            };
        } catch (InvalidInputException
                | DuplicateNameException
                | CircularDependencyException exception) {
            return new Outcome(step, row.address(), new Report.Failure<>(exception.getMessage()));
        }
    }

    // --- [PROTOTYPES]

    private Outcome type(Row row, Function function, String prototype) {
        try {
            FunctionDefinitionDataType definition =
                    CParserUtils.parseSignature(
                            (DataTypeManagerService) null, currentProgram, prototype, false);
            if (definition == null) {
                return new Outcome(
                        Step.TYPED,
                        row.address(),
                        new Report.Failure<>("`%s` holds no function name".formatted(prototype)));
            }
            ApplyFunctionSignatureCmd command =
                    new ApplyFunctionSignatureCmd(
                            row.address(),
                            definition,
                            SourceType.USER_DEFINED,
                            definition.hasUnknownCallingConventionName(),
                            false,
                            DataTypeConflictHandler.DEFAULT_HANDLER,
                            FunctionRenameOption.NO_CHANGE);
            return new Outcome(
                    Step.TYPED,
                    row.address(),
                    runCommand(command)
                            ? new Report.Success<>(function.getPrototypeString(true, false))
                            : new Report.Failure<>(command.getStatusMsg()));
        } catch (ParseException exception) {
            return new Outcome(
                    Step.TYPED,
                    row.address(),
                    new Report.Failure<>(
                            "`%s` did not parse (%s)"
                                    .formatted(
                                            prototype,
                                            exception.getMessage().lines().findFirst().orElse(""))));
        }
    }

    // --- [LOCALS]

    private Stream<Outcome> remove(Row row, Function function) {
        return row.locals().stream()
                .filter(Local::removes)
                .flatMap(local -> {
                    List<Variable> matched =
                            Arrays.stream(function.getLocalVariables())
                                    .filter(variable -> local.matcher().test(variable.getName()))
                                    .toList();
                    List<String> names = matched.stream().map(Variable::getName).toList();
                    matched.forEach(function::removeVariable);
                    return names.isEmpty()
                            ? Stream.of(
                                    new Outcome(
                                            Step.REMOVED,
                                            row.address(),
                                            new Report.Failure<>(
                                                    "Local `%s` matches no stored local"
                                                            .formatted(local.name()))))
                            : names.stream()
                                    .map(name ->
                                            new Outcome(
                                                    Step.REMOVED,
                                                    row.address(),
                                                    new Report.Success<>(name)));
                });
    }

    private Outcome rename(Address address, Local local, Map<String, HighSymbol> symbols) {
        DataTypeManager types = currentProgram.getDataTypeManager();
        try {
            HighSymbol symbol =
                    Optional.ofNullable(symbols.get(local.name()))
                            .orElseThrow(() ->
                                    new InvalidInputException(
                                            "Local `%s` matches no decompiled local"
                                                    .formatted(local.name())));
            DataType type =
                    local.type().isPresent()
                            ? new DataTypeParser(types, types, null, AllowedDataTypes.ALL)
                                    .parse(local.type().get())
                            : null;
            HighFunctionDBUtil.updateDBVariable(
                    symbol, local.replacement(), type, SourceType.USER_DEFINED);
            return new Outcome(
                    Step.RENAMED,
                    address,
                    new Report.Success<>(
                            "%s -> %s%s"
                                    .formatted(
                                            local.name(),
                                            local.replacement(),
                                            local.type().map(":"::concat).orElse(""))));
        } catch (InvalidInputException
                | DuplicateNameException
                | InvalidDataTypeException
                | CancelledException exception) {
            return new Outcome(Step.RENAMED, address, new Report.Failure<>(exception.getMessage()));
        }
    }

    private Stream<Outcome> rename(Row row, Report.Result<DecompileResults> result) {
        List<Local> renames = row.locals().stream().filter(Predicate.not(Local::removes)).toList();
        return switch (result) {
            case Report.Success<DecompileResults>(DecompileResults decompiled) -> {
                Map<String, HighSymbol> symbols =
                        CollectionUtils.asStream(
                                        decompiled
                                                .getHighFunction()
                                                .getLocalSymbolMap()
                                                .getSymbols())
                                .collect(
                                        Collectors.toMap(
                                                HighSymbol::getName,
                                                symbol -> symbol,
                                                (first, _) -> first));
                yield renames.stream().map(local -> rename(row.address(), local, symbols));
            }
            case Report.Failure<DecompileResults>(String cause) ->
                    Stream.of(
                            new Outcome(Step.RENAMED, row.address(), new Report.Failure<>(cause)));
        };
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Arguments.Usage usage =
                new Arguments.Usage(
                        getScriptName(),
                        "<report> <rows>...\n"
                                + "row: <address> <namespace> <name> <prototype> <comment> <locals>"
                                + " <kind> by tab\n"
                                + "local: <local>=<name>[:<type>] | <local>= | re:<regex>=\n"
                                + "kind: function | label");
        String[] args = getScriptArgs();
        Path out = usage.out(args);
        List<String> files = usage.operands(args);
        List<Row> rows =
                usage.values(
                        Stream.concat(
                                files.stream()
                                        .<Report.Result<Row>>flatMap(file -> {
                                            Path path = Path.of(file);
                                            try {
                                                List<String> lines = Files.readAllLines(path);
                                                return IntStream.range(0, lines.size())
                                                        .filter(index ->
                                                                !lines.get(index).isBlank())
                                                        .filter(index ->
                                                                !lines.get(index).startsWith("#"))
                                                        .mapToObj(index ->
                                                                row(
                                                                        path,
                                                                        index + 1,
                                                                        lines.get(index)));
                                            } catch (IOException exception) {
                                                return Stream.of(
                                                        new Report.Failure<>(
                                                                "Rows file `%s` is unreadable, %s"
                                                                        .formatted(
                                                                                file, exception)));
                                            }
                                        }),
                                usage.require(!files.isEmpty(), "rows file")));
        List<Outcome> named = rows.stream().map(this::name).toList();
        List<Map.Entry<Row, Function>> functions =
                rows.stream()
                        .filter(row -> row.kind() == Kind.FUNCTION)
                        .flatMap(row ->
                                Stream.ofNullable(getFunctionAt(row.address()))
                                        .map(function -> Map.entry(row, function)))
                        .toList();
        List<Outcome> typed =
                functions.stream()
                        .flatMap(entry ->
                                entry.getKey().prototype().stream()
                                        .map(prototype ->
                                                type(entry.getKey(), entry.getValue(), prototype)))
                        .toList();
        List<Outcome> removed =
                functions.stream()
                        .flatMap(entry -> remove(entry.getKey(), entry.getValue()))
                        .toList();
        List<Map.Entry<Row, Function>> renaming =
                functions.stream()
                        .filter(entry ->
                                entry.getKey().locals().stream()
                                        .anyMatch(Predicate.not(Local::removes)))
                        .toList();
        SequencedMap<Function, Report.Result<DecompileResults>> decompiled =
                Functions.decompile(
                        currentProgram,
                        renaming.stream().map(Map.Entry::getValue).distinct().toList(),
                        Arguments.Setting.defaults(),
                        monitor);
        List<Outcome> renamed =
                renaming.stream()
                        .flatMap(entry ->
                                rename(entry.getKey(), decompiled.get(entry.getValue())))
                        .toList();
        List<Outcome> outcomes =
                Stream.of(named, typed, removed, renamed).flatMap(List::stream).toList();
        List<Report.Section> sections =
                Stream.concat(
                                Arrays.stream(Step.values())
                                        .map(step ->
                                                Report.Section.rows(
                                                        step.name(),
                                                        outcomes.stream()
                                                                .filter(outcome ->
                                                                        outcome.step() == step)
                                                                .filter(outcome ->
                                                                        outcome.result()
                                                                                instanceof
                                                                                Report.Success<
                                                                                        String>)
                                                                .map(Outcome::row)
                                                                .toList())),
                                Stream.of(
                                        Report.Section.rows(
                                                "REJECTED",
                                                outcomes.stream()
                                                        .filter(outcome ->
                                                                outcome.result()
                                                                        instanceof
                                                                        Report.Failure<String>)
                                                        .map(Outcome::row)
                                                        .toList())))
                        .toList();
        println(Report.write(out, currentProgram, sections, String.join(" ", files)));
    }
}
