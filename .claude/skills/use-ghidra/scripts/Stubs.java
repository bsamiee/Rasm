import ghidra.app.script.GhidraScript;
import ghidra.app.util.bin.format.objc.ObjcUtils;
import ghidra.app.util.bin.format.objc.objc1.Objc1Constants;
import ghidra.program.model.data.PointerDataType;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Function.FunctionUpdateType;
import ghidra.program.model.listing.ParameterImpl;
import ghidra.program.model.listing.Program;
import ghidra.program.model.listing.ReturnParameterImpl;
import ghidra.program.model.symbol.SourceType;
import ghidra.util.exception.DuplicateNameException;
import ghidra.util.exception.InvalidInputException;

import java.nio.file.Path;
import java.util.List;
import java.util.Locale;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Stubs extends GhidraScript {
    // --- [RENAMES]

    sealed interface Rename permits Renamed, Unchanged, Unresolved, Rejected {
        Function stub();
    }

    record Renamed(Function stub, String previous) implements Rename {}

    record Unchanged(Function stub) implements Rename {}

    record Unresolved(Function stub) implements Rename {}

    record Rejected(Function stub, String message) implements Rename {}

    private static Rename rename(Function stub) {
        return Functions.selector(stub)
                .map(selector -> rename(stub, Objc1Constants.OBJC_MSG_SEND + "$" + selector))
                .orElseGet(() -> new Unresolved(stub));
    }

    private static Rename rename(Function stub, String name) {
        String previous = stub.getName();
        return previous.equals(name)
                        && ObjcUtils.OBJC_MSGSEND_STUBS_CC.equals(stub.getCallingConventionName())
                ? new Unchanged(stub)
                : apply(stub, name, previous);
    }

    private static Rename apply(Function stub, String name, String previous) {
        Program program = stub.getProgram();
        try {
            stub.updateFunction(
                    ObjcUtils.OBJC_MSGSEND_STUBS_CC,
                    new ReturnParameterImpl(PointerDataType.dataType, program),
                    List.of(new ParameterImpl("self", PointerDataType.dataType, program)),
                    FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS,
                    true,
                    SourceType.ANALYSIS);
            stub.setVarArgs(true);
            stub.setName(name, SourceType.ANALYSIS);
            return new Renamed(stub, previous);
        } catch (DuplicateNameException | InvalidInputException exception) {
            return new Rejected(stub, exception.getMessage());
        }
    }

    // --- [ROWS]

    private static String row(Rename rename) {
        String entry = rename.stub().getEntryPoint() + " ";
        return entry
                + switch (rename) {
                    case Renamed(Function stub, String previous) ->
                            previous + " -> " + stub.getName(true);
                    case Unchanged(Function stub) -> stub.getName(true) + " unchanged";
                    case Unresolved(Function stub) -> stub.getName(true) + " unresolved";
                    case Rejected(Function stub, String message) ->
                            stub.getName(true) + " rejected: " + message;
                };
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Path out = Arguments.out(getScriptName(), getScriptArgs());
        List<Rename> renames =
                Functions.internal(currentProgram)
                        .filter(Functions::isStub)
                        .sorted(Functions.BY_ENTRY)
                        .map(Stubs::rename)
                        .toList();
        String counts =
                Stream.of(Renamed.class, Unchanged.class, Unresolved.class, Rejected.class)
                        .map(
                                type ->
                                        type.getSimpleName().toLowerCase(Locale.ROOT)
                                                + "="
                                                + renames.stream().filter(type::isInstance).count())
                        .collect(Collectors.joining(" ", "stubs=" + renames.size() + " ", ""));
        println(
                Report.write(
                        out,
                        currentProgram,
                        counts,
                        Stream.concat(
                                Stream.of(Report.section("STUBS")),
                                renames.stream().map(Stubs::row))));
    }
}
