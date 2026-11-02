import ghidra.app.script.GhidraScript;
import ghidra.app.util.bin.format.objc.objc1.Objc1Constants;
import ghidra.app.util.sourcelanguage.ObjcSourceLanguageSpecExtension;
import ghidra.program.model.data.PointerDataType;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Function.FunctionUpdateType;
import ghidra.program.model.listing.ParameterImpl;
import ghidra.program.model.listing.Program;
import ghidra.program.model.listing.ReturnParameterImpl;
import ghidra.program.model.symbol.SourceType;
import ghidra.util.exception.DuplicateNameException;
import ghidra.util.exception.InvalidInputException;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import java.nio.file.Path;
import java.util.List;
import java.util.Locale;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Stubs extends GhidraScript {
    // --- [RENAMES]

    sealed interface Rename permits Renamed, Unchanged, Unresolved, Rejected {
        Function stub();

        static Rename of(Function stub) {
            return Functions.selector(stub)
                    .map(selector -> Objc1Constants.OBJC_MSG_SEND + "$" + selector)
                    .map(name ->
                            name.equals(stub.getName())
                                            && ObjcSourceLanguageSpecExtension.OBJC_MSGSEND_STUBS
                                                    .equals(stub.getCallingConventionName())
                                    ? new Unchanged(stub)
                                    : apply(stub, name))
                    .orElseGet(() -> new Unresolved(stub));
        }

        default String row() {
            return stub().getEntryPoint()
                    + " "
                    + switch (this) {
                        case Renamed(Function stub, String previous) ->
                                previous + " -> " + stub.getName(true);
                        case Unchanged(Function stub) -> stub.getName(true);
                        case Unresolved(Function stub) -> stub.getName(true);
                        case Rejected(Function stub, String message) ->
                                stub.getName(true) + ": " + message;
                    };
        }
    }

    record Renamed(Function stub, String previous) implements Rename {}

    record Unchanged(Function stub) implements Rename {}

    record Unresolved(Function stub) implements Rename {}

    record Rejected(Function stub, String message) implements Rename {}

    private static Rename apply(Function stub, String name) {
        Program program = stub.getProgram();
        String previous = stub.getName();
        try {
            stub.updateFunction(
                    ObjcSourceLanguageSpecExtension.OBJC_MSGSEND_STUBS,
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

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Path out = new Arguments.Usage(getScriptName(), "<out>").only(getScriptArgs());
        List<Rename> renames =
                Functions.internal(currentProgram)
                        .filter(Functions::isStub)
                        .sorted(Functions.BY_ENTRY)
                        .map(Rename::of)
                        .toList();
        List<Report.Section> sections =
                Stream.of(Renamed.class, Unchanged.class, Unresolved.class, Rejected.class)
                        .map(outcome ->
                                Report.Section.rows(
                                        outcome.getSimpleName().toUpperCase(Locale.ROOT),
                                        renames.stream()
                                                .filter(outcome::isInstance)
                                                .map(Rename::row)
                                                .toList()))
                        .toList();
        println(Report.write(out, currentProgram, sections, ""));
    }
}
