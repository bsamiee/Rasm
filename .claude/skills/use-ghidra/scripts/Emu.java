import com.google.gson.Gson;

import ghidra.app.script.GhidraScript;
import ghidra.pcode.emu.PcodeEmulationCallbacks;
import ghidra.pcode.emu.PcodeEmulator;
import ghidra.pcode.emu.PcodeThread;
import ghidra.pcode.exec.PcodeExecutionException;
import ghidra.pcode.exec.PcodeExecutorStatePiece;
import ghidra.pcode.exec.PcodeExecutorStatePiece.Reason;
import ghidra.pcode.exec.PcodeFrame;
import ghidra.pcode.exec.PcodeProgram;
import ghidra.pcode.exec.PcodeStateCallbacks;
import ghidra.pcode.exec.PcodeUseropLibrary;
import ghidra.pcode.exec.SleighProgramCompiler;
import ghidra.pcode.utils.Utils;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressRange;
import ghidra.program.model.address.AddressSet;
import ghidra.program.model.address.AddressSetView;
import ghidra.program.model.address.AddressSpace;
import ghidra.program.model.data.DataType;
import ghidra.program.model.data.PointerDataType;
import ghidra.program.model.lang.CompilerSpec;
import ghidra.program.model.lang.Language;
import ghidra.program.model.lang.PrototypeModel;
import ghidra.program.model.lang.Register;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;
import ghidra.program.model.mem.Memory;
import ghidra.program.model.mem.MemoryAccessException;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.pcode.PcodeOp;
import ghidra.program.model.pcode.Varnode;
import ghidra.util.NumericUtilities;

import rasm.Arguments;
import rasm.Functions;
import rasm.Report;

import util.CollectionUtils;

import java.io.IOException;
import java.math.BigInteger;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.SequencedMap;
import java.util.TreeMap;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Emu extends GhidraScript {
    // --- [PLAN]

    private static final long REGION = 0x100_0000L;

    private static final long PAGE = 0x1000L;

    private static final int TRAP = 0x10;

    record Plan(long steps, Map<String, String> stubs, List<Run> runs) {
        Plan {
            stubs = Objects.requireNonNullElse(stubs, Map.of());
            runs = Objects.requireNonNullElse(runs, List.of());
        }
    }

    record Run(
            String id,
            String entry,
            boolean keep,
            Long steps,
            Map<String, String> registers,
            Map<String, String> memory,
            Map<String, Integer> reads,
            List<String> results) {
        Run {
            registers = Objects.requireNonNullElse(registers, Map.of());
            memory = Objects.requireNonNullElse(memory, Map.of());
            reads = Objects.requireNonNullElse(reads, Map.of());
            results = Objects.requireNonNullElse(results, List.of());
        }
    }

    record Prepared(
            Run run,
            Address entry,
            long steps,
            Map<Register, byte[]> registers,
            Map<Address, byte[]> memory,
            Map<Address, Integer> reads,
            List<Register> results) {}

    // --- [CONVENTION]

    record Convention(
            String target, boolean register, int size, String stack, int pop, Register result) {
        static Report.Result<Convention> of(Program program) {
            CompilerSpec spec = program.getCompilerSpec();
            PrototypeModel model = spec.getDefaultCallingConvention();
            String stack = spec.getStackPointer().getName();
            int pop =
                    model.getExtrapop() == PrototypeModel.UNKNOWN_EXTRAPOP
                            ? model.getStackshift()
                            : model.getExtrapop();
            Register result =
                    model.getStorageLocations(
                                    program,
                                    new DataType[] {
                                        new PointerDataType(program.getDataTypeManager())
                                    },
                                    false,
                                    false)[0]
                            .getRegister();
            return CollectionUtils.asStream(program.getListing().getInstructions(true))
                    .filter(instruction -> instruction.getFlowType().isCall())
                    .map(instruction -> List.of(instruction.getPcode()))
                    .flatMap(
                            ops ->
                                    IntStream.range(1, ops.size())
                                            .filter(index -> ops.get(index).getOpcode() == PcodeOp.CALL)
                                            .mapToObj(index -> ops.get(index - 1))
                                            .filter(
                                                    op ->
                                                            op.getOpcode() == PcodeOp.STORE
                                                                    || op.getOutput() != null
                                                                            && op.getOutput().isRegister()))
                    .findFirst()
                    .<Report.Result<Convention>>map(
                            op -> {
                                boolean register = op.getOpcode() != PcodeOp.STORE;
                                int size = register ? op.getOutput().getSize() : op.getInput(2).getSize();
                                return new Report.Success<>(
                                        register
                                                ? new Convention(
                                                        program.getRegister(op.getOutput()).getName(),
                                                        true,
                                                        size,
                                                        stack,
                                                        pop,
                                                        result)
                                                : new Convention(
                                                        "*[%s]:%d %s"
                                                                .formatted(
                                                                        spec.getStackBaseSpace()
                                                                                .getName(),
                                                                        size,
                                                                        stack),
                                                        false,
                                                        size,
                                                        stack,
                                                        pop,
                                                        result));
                            })
                    .orElseGet(
                            () ->
                                    new Report.Failure<>(
                                            "Program `%s` holds no call storing its return address"
                                                    .formatted(program.getName())));
        }

        String stub(String effect) {
            return "%s\nlocal target:%d = %s;\n%s = %s + %d;\nreturn [target];"
                    .formatted(effect, size, target, stack, stack, pop);
        }

        String fallback() {
            return result.getName() + " = 0;";
        }

        String enter(Layout layout) {
            long top = layout.top().getOffset() - (register ? 0 : size);
            return "%s = 0x%x;\n%s = 0x%x;"
                    .formatted(stack, top, target, layout.sentinel().getOffset());
        }
    }

    record Layout(Address sentinel, Address traps, Address top) {
        static Layout of(Program program) {
            AddressSpace space = program.getAddressFactory().getDefaultAddressSpace();
            long end =
                    Arrays.stream(program.getMemory().getBlocks())
                            .map(MemoryBlock::getEnd)
                            .filter(address -> address.getAddressSpace().equals(space))
                            .mapToLong(Address::getOffset)
                            .max()
                            .orElse(0);
            long base = (end / REGION + 1) * REGION;
            return new Layout(
                    space.getAddress(base),
                    space.getAddress(base + PAGE),
                    space.getAddress(base + REGION));
        }

        List<String> rows() {
            return List.of("sentinel " + sentinel, "traps " + traps, "stack " + top);
        }
    }

    // --- [STUBS]

    record Stub(Address address, String name, List<Address> slots) {
        String row(Map<String, String> effects) {
            return "%s %s %s%s"
                    .formatted(
                            address,
                            name,
                            effects.containsKey(name) ? "requested" : "fallback",
                            slots.stream().map(slot -> " slot=" + slot).collect(Collectors.joining()));
        }
    }

    private static Stream<Stub> stubs(Program program, Layout layout) {
        Stream<Stub> thunks =
                Functions.internal(program)
                        .filter(Function::isThunk)
                        .filter(thunk -> thunk.getThunkedFunction(true).isExternal())
                        .map(
                                thunk ->
                                        new Stub(
                                                thunk.getEntryPoint(),
                                                thunk.getThunkedFunction(true).getName(),
                                                List.of()));
        List<Map.Entry<Function, List<Address>>> pointed =
                CollectionUtils.asStream(program.getFunctionManager().getExternalFunctions())
                        .map(
                                external ->
                                        Map.entry(
                                                external,
                                                CollectionUtils.asStream(
                                                                program.getReferenceManager()
                                                                        .getReferencesTo(
                                                                                external
                                                                                        .getEntryPoint()))
                                                        .map(reference -> reference.getFromAddress())
                                                        .filter(
                                                                from ->
                                                                        program.getListing()
                                                                                        .getDataAt(from)
                                                                                instanceof Data data
                                                                                && data.isPointer())
                                                        .toList()))
                        .filter(entry -> !entry.getValue().isEmpty())
                        .toList();
        return Stream.concat(
                thunks,
                IntStream.range(0, pointed.size())
                        .mapToObj(
                                index ->
                                        new Stub(
                                                layout.traps().add((long) index * TRAP),
                                                pointed.get(index).getKey().getName(),
                                                pointed.get(index).getValue())));
    }

    // --- [TRACE]

    sealed interface Outcome permits Returned, Exhausted, Faulted {
        long steps();

        Address counter();

        default String text() {
            return switch (this) {
                case Returned(long steps, Address counter) ->
                        "returned steps=%d pc=%s".formatted(steps, counter);
                case Exhausted(long steps, Address counter) ->
                        "exhausted steps=%d pc=%s".formatted(steps, counter);
                case Faulted(long steps, Address counter, String cause) ->
                        "faulted steps=%d pc=%s: %s".formatted(steps, counter, cause);
            };
        }
    }

    record Returned(long steps, Address counter) implements Outcome {}

    record Exhausted(long steps, Address counter) implements Outcome {}

    record Faulted(long steps, Address counter, String cause) implements Outcome {}

    static final class Recorder implements PcodeEmulationCallbacks<byte[]> {
        private final Memory memory;
        private final AddressSetView loaded;
        private final Map<Address, Stub> stubs;
        private final Convention convention;
        private final List<String> calls = new ArrayList<>();
        private final AddressSet uninitialized = new AddressSet();
        private final Map<String, Integer> userops = new TreeMap<>();
        private long step;

        Recorder(Program program, Map<Address, Stub> stubs, Convention convention) {
            this.memory = program.getMemory();
            this.loaded = program.getMemory().getLoadedAndInitializedAddressSet();
            this.stubs = stubs;
            this.convention = convention;
        }

        void begin() {
            calls.clear();
            uninitialized.clear();
            userops.clear();
            step = 0;
        }

        @Override
        public <A, U> AddressSetView readUninitialized(
                PcodeThread<byte[]> thread,
                PcodeExecutorStatePiece<A, U> piece,
                AddressSetView set,
                Reason reason) {
            if (set.isEmpty() || !set.getMinAddress().getAddressSpace().isMemorySpace()) {
                return set;
            }
            AddressSetView image = set.intersect(loaded);
            PcodeExecutorStatePiece<A, byte[]> bytes =
                    PcodeStateCallbacks.checkValueDomain(piece, byte[].class);
            image.forEach(range -> load(bytes, range));
            AddressSetView missing = image.isEmpty() ? set : set.subtract(image);
            if (reason == Reason.EXECUTE_READ) {
                uninitialized.add(missing);
            }
            return missing;
        }

        private <A> void load(PcodeExecutorStatePiece<A, byte[]> piece, AddressRange range) {
            byte[] buffer = new byte[(int) range.getLength()];
            try {
                memory.getBytes(range.getMinAddress(), buffer);
            } catch (MemoryAccessException exception) {
                throw new IllegalStateException(exception);
            }
            piece.setVarInternal(
                    range.getAddressSpace(), range.getMinAddress().getOffset(), buffer.length, buffer);
        }

        @Override
        public void beforeExecuteInject(
                PcodeThread<byte[]> thread, Address address, PcodeProgram program) {
            Optional.ofNullable(stubs.get(address))
                    .ifPresent(
                            stub -> {
                                byte[] value =
                                        SleighProgramCompiler.compileExpression(
                                                        thread.getLanguage(), convention.target())
                                                .evaluate(thread.getExecutor());
                                calls.add(
                                        "%d %s from %s"
                                                .formatted(
                                                        step,
                                                        stub.name(),
                                                        address.getNewAddress(
                                                                Utils.bytesToLong(
                                                                        value,
                                                                        value.length,
                                                                        thread.getLanguage()
                                                                                .isBigEndian()))));
                            });
        }

        @Override
        public boolean handleMissingUserop(
                PcodeThread<byte[]> thread,
                PcodeOp op,
                PcodeFrame frame,
                String opName,
                PcodeUseropLibrary<byte[]> library) {
            userops.merge(opName, 1, Integer::sum);
            return true;
        }
    }

    // --- [PREPARATION]

    private static <T> T value(Report.Result<T> result) {
        return result.values().findFirst().orElseThrow();
    }

    private static <K, V, R> Report.Result<Map<K, V>> entries(
            Map<String, R> raw,
            java.util.function.Function<String, Report.Result<K>> key,
            java.util.function.Function<R, Report.Result<V>> parse) {
        List<Report.Result<K>> keys = raw.keySet().stream().map(key).toList();
        List<Report.Result<V>> values = raw.values().stream().map(parse).toList();
        String failures =
                Stream.concat(
                                keys.stream().flatMap(Report.Result::failures),
                                values.stream().flatMap(Report.Result::failures))
                        .collect(Collectors.joining("\n"));
        return failures.isEmpty()
                ? new Report.Success<>(
                        IntStream.range(0, keys.size())
                                .boxed()
                                .collect(
                                        Collectors.toMap(
                                                index -> value(keys.get(index)),
                                                index -> value(values.get(index)),
                                                (first, _) -> first,
                                                LinkedHashMap::new)))
                : new Report.Failure<>(failures);
    }

    private Report.Result<Address> address(String text) {
        return Optional.ofNullable(currentProgram.getAddressFactory().getAddress(text))
                .<Report.Result<Address>>map(Report.Success::new)
                .orElseGet(() -> new Report.Failure<>("`%s` names no address".formatted(text)));
    }

    private Report.Result<Register> register(String name) {
        return Optional.ofNullable(currentProgram.getLanguage().getRegister(name))
                .<Report.Result<Register>>map(Report.Success::new)
                .orElseGet(() -> new Report.Failure<>("`%s` names no register".formatted(name)));
    }

    private Report.Result<Address> entry(String text) {
        return text.startsWith("0x")
                ? address(text)
                : switch (Arguments.functions(text, currentProgram)) {
                    case Report.Success<List<Function>>(List<Function> found)
                            when found.size() == 1 ->
                            new Report.Success<>(found.getFirst().getEntryPoint());
                    case Report.Success<List<Function>>(List<Function> found) ->
                            new Report.Failure<>(
                                    "Entry `%s` matches %d functions".formatted(text, found.size()));
                    case Report.Failure<List<Function>>(String cause) -> new Report.Failure<>(cause);
                };
    }

    private Report.Result<Prepared> prepare(Run run, long steps, Convention convention) {
        boolean bigEndian = currentProgram.getLanguage().isBigEndian();
        Report.Result<Address> entry = entry(Objects.requireNonNullElse(run.entry(), ""));
        Report.Result<Map<Register, byte[]>> registers =
                Emu.<Register, BigInteger, String>entries(
                                run.registers(), this::register, Emu::number)
                        .map(
                                values ->
                                        values.entrySet().stream()
                                                .collect(
                                                        Collectors.toMap(
                                                                Map.Entry::getKey,
                                                                each ->
                                                                        Utils.bigIntegerToBytes(
                                                                                each.getValue(),
                                                                                each.getKey()
                                                                                        .getMinimumByteSize(),
                                                                                bigEndian),
                                                                (first, _) -> first,
                                                                LinkedHashMap::new)));
        Report.Result<Map<Address, byte[]>> memory =
                entries(run.memory(), this::address, Emu::bytes);
        Report.Result<Map<Address, Integer>> reads =
                entries(
                        run.reads(),
                        this::address,
                        length ->
                                length != null && length > 0
                                        ? new Report.Success<>(length)
                                        : new Report.Failure<>(
                                                "Read length `%s` is no count".formatted(length)));
        Report.Result<List<Register>> results =
                Report.all(
                        (run.results().isEmpty()
                                        ? List.of(convention.result().getName())
                                        : run.results())
                                .stream()
                                .map(this::register));
        String failures =
                Stream.of(entry, registers, memory, reads, results)
                        .flatMap(Report.Result::failures)
                        .map(cause -> "Run `%s`: %s".formatted(run.id(), cause))
                        .collect(Collectors.joining("\n"));
        return failures.isEmpty()
                ? new Report.Success<>(
                        new Prepared(
                                run,
                                value(entry),
                                Objects.requireNonNullElse(run.steps(), steps),
                                value(registers),
                                value(memory),
                                value(reads),
                                value(results)))
                : new Report.Failure<>(failures);
    }

    private static Report.Result<BigInteger> number(String text) {
        try {
            return new Report.Success<>(NumericUtilities.decodeBigInteger(text));
        } catch (NumberFormatException exception) {
            return new Report.Failure<>("`%s` is no number".formatted(text));
        }
    }

    private static Report.Result<byte[]> bytes(String text) {
        return Optional.ofNullable(NumericUtilities.convertStringToBytes(text))
                .<Report.Result<byte[]>>map(Report.Success::new)
                .orElseGet(() -> new Report.Failure<>("`%s` is no hex bytes".formatted(text)));
    }

    // --- [EXECUTION]

    record Machine(PcodeEmulator emulator, Recorder recorder) {}

    private Machine machine(Map<Address, Stub> stubs, Map<String, String> effects, Convention convention) {
        Recorder recorder = new Recorder(currentProgram, stubs, convention);
        PcodeEmulator emulator = new PcodeEmulator(currentProgram.getLanguage(), recorder);
        stubs.values()
                .forEach(
                        stub -> {
                            emulator.inject(
                                    stub.address(),
                                    convention.stub(
                                            effects.getOrDefault(stub.name(), convention.fallback())));
                            stub.slots()
                                    .forEach(
                                            slot ->
                                                    emulator.getSharedState()
                                                            .setVar(
                                                                    slot,
                                                                    currentProgram
                                                                            .getDefaultPointerSize(),
                                                                    true,
                                                                    Utils.longToBytes(
                                                                            stub.address().getOffset(),
                                                                            currentProgram
                                                                                    .getDefaultPointerSize(),
                                                                            currentProgram
                                                                                    .getLanguage()
                                                                                    .isBigEndian())));
                        });
        return new Machine(emulator, recorder);
    }

    private Outcome execute(PcodeThread<byte[]> thread, Recorder recorder, Address sentinel, long budget) {
        long steps = 0;
        try {
            for (; steps < budget && !thread.getCounter().equals(sentinel); steps++) {
                recorder.step = steps;
                thread.stepInstruction();
            }
        } catch (PcodeExecutionException exception) {
            return new Faulted(steps, thread.getCounter(), exception.getMessage());
        }
        return thread.getCounter().equals(sentinel)
                ? new Returned(steps, thread.getCounter())
                : new Exhausted(steps, thread.getCounter());
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Arguments.Usage usage = new Arguments.Usage(getScriptName(), "<out> <plan>");
        String[] args = getScriptArgs();
        Path out = usage.out(args);
        Plan plan = usage.values(Stream.of(read(usage.operands(args)))).getFirst();
        Layout layout = Layout.of(currentProgram);
        Convention convention = usage.values(Stream.of(Convention.of(currentProgram))).getFirst();
        List<Stub> stubs = stubs(currentProgram, layout).toList();
        List<String> names = stubs.stream().map(Stub::name).toList();
        List<Prepared> runs =
                usage.values(
                        Stream.concat(
                                plan.stubs().keySet().stream()
                                        .filter(name -> !names.contains(name))
                                        .map(
                                                name ->
                                                        new Report.Failure<Prepared>(
                                                                "Stub `%s` names no external function"
                                                                        .formatted(name))),
                                plan.runs().stream().map(run -> prepare(run, plan.steps(), convention))));
        SequencedMap<Address, Stub> byAddress =
                stubs.stream()
                        .collect(
                                Collectors.toMap(
                                        Stub::address,
                                        stub -> stub,
                                        (first, _) -> first,
                                        LinkedHashMap::new));
        Machine machine = machine(byAddress, plan.stubs(), convention);
        List<String> outcomes = new ArrayList<>();
        List<String> calls = new ArrayList<>();
        List<String> reads = new ArrayList<>();
        List<String> uninitialized = new ArrayList<>();
        List<String> userops = new ArrayList<>();
        for (Prepared prepared : runs) {
            monitor.checkCancelled();
            machine = prepared.run().keep() ? machine : machine(byAddress, plan.stubs(), convention);
            String id = prepared.run().id();
            PcodeEmulator emulator = machine.emulator();
            PcodeThread<byte[]> thread = emulator.newThread(id);
            thread.getExecutor()
                    .execute(
                            emulator.compileSleigh(
                                    "enter",
                                    convention.enter(layout)),
                            thread.getUseropLibrary());
            prepared.registers().forEach(thread.getState()::setVar);
            prepared.memory()
                    .forEach(
                            (address, data) ->
                                    emulator.getSharedState().setVar(address, data.length, true, data));
            thread.overrideCounter(prepared.entry());
            Optional.ofNullable(
                            currentProgram.getProgramContext().getDisassemblyContext(prepared.entry()))
                    .ifPresentOrElse(thread::overrideContext, thread::overrideContextWithDefault);
            machine.recorder().begin();
            Outcome outcome = execute(thread, machine.recorder(), layout.sentinel(), prepared.steps());
            outcomes.add(
                    "%s %s%s"
                            .formatted(
                                    id,
                                    outcome.text(),
                                    prepared.results().stream()
                                            .map(
                                                    result ->
                                                            " %s=0x%s"
                                                                    .formatted(
                                                                            result.getName(),
                                                                            Utils.bytesToBigInteger(
                                                                                            thread.getState()
                                                                                                    .getVar(
                                                                                                            result,
                                                                                                            Reason
                                                                                                                    .INSPECT),
                                                                                            result
                                                                                                    .getMinimumByteSize(),
                                                                                            currentProgram
                                                                                                    .getLanguage()
                                                                                                    .isBigEndian(),
                                                                                            false)
                                                                                    .toString(16)))
                                            .collect(Collectors.joining())));
            machine.recorder().calls.stream().map(call -> id + " " + call).forEach(calls::add);
            prepared.reads()
                    .forEach(
                            (address, length) ->
                                    reads.add(
                                            "%s %s %s"
                                                    .formatted(
                                                            id,
                                                            address,
                                                            NumericUtilities.convertBytesToString(
                                                                    emulator.getSharedState()
                                                                            .getVar(
                                                                                    address,
                                                                                    length,
                                                                                    true,
                                                                                    Reason.INSPECT)))));
            machine.recorder().uninitialized.forEach(range -> uninitialized.add(id + " " + range));
            machine.recorder().userops.forEach((name, count) -> userops.add("%s %s count=%d".formatted(id, name, count)));
        }
        List<Report.Section> sections =
                List.of(
                        Report.Section.rows("LAYOUT", layout.rows()),
                        Report.Section.rows(
                                "STUBS", stubs.stream().map(stub -> stub.row(plan.stubs())).toList()),
                        Report.Section.rows("RUNS", outcomes),
                        Report.Section.rows("CALLS", calls),
                        Report.Section.rows("READS", reads),
                        Report.Section.rows("UNINITIALIZED", uninitialized),
                        Report.Section.rows("USEROPS", userops));
        println(Report.write(out, currentProgram, sections, String.join(" ", usage.operands(args))));
    }

    private static Report.Result<Plan> read(List<String> operands) {
        if (operands.size() != 1) {
            return new Report.Failure<>("Arguments name %d plans".formatted(operands.size()));
        }
        try {
            return Optional.ofNullable(new Gson().fromJson(Files.readString(Path.of(operands.getFirst())), Plan.class))
                    .filter(plan -> plan.steps() > 0)
                    .<Report.Result<Plan>>map(Report.Success::new)
                    .orElseGet(() -> new Report.Failure<>("Plan `%s` sets no positive steps".formatted(operands.getFirst())));
        } catch (IOException | RuntimeException exception) {
            return new Report.Failure<>("Plan `%s` is unreadable. %s".formatted(operands.getFirst(), exception.getMessage()));
        }
    }
}
