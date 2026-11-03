import ghidra.app.script.GhidraScript;
import ghidra.app.util.bin.BinaryReader;
import ghidra.app.util.bin.MemoryByteProvider;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressSpace;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;
import ghidra.program.model.mem.Memory;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.Symbol;

import rasm.Arguments;
import rasm.Report;

import util.CollectionUtils;

import java.io.IOException;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.function.BiFunction;
import java.util.function.Predicate;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.Stream;

// --- [COMPOSITION] ---------------------------------------------------------------------

public class Rtti extends GhidraScript {
    // --- [LAYOUT]

    private static final Pattern TYPEINFO_SYMBOL = Pattern.compile("^_*ZTI");

    enum Kind {
        CLASS("17__class_type_infoE"),
        SI("20__si_class_type_infoE"),
        VMI("21__vmi_class_type_infoE");

        final String suffix;

        Kind(String suffix) {
            this.suffix = suffix;
        }

        static Optional<Kind> of(Symbol symbol) {
            return Arrays.stream(values())
                    .filter(kind -> symbol.getName().endsWith(kind.suffix))
                    .findFirst();
        }
    }

    record Image(Program program, BinaryReader reader, int pointer, AddressSpace space) {
        static Image of(Program program) {
            AddressSpace space = program.getAddressFactory().getDefaultAddressSpace();
            return new Image(
                    program,
                    new BinaryReader(
                            new MemoryByteProvider(program.getMemory(), space),
                            !program.getLanguage().isBigEndian()),
                    program.getDefaultPointerSize(),
                    space);
        }

        long word(Address address, long offset) throws IOException {
            return reader.readValue(address.getOffset() + offset, pointer);
        }

        Address target(Address address, long offset) throws IOException {
            return space.getTruncatedAddress(word(address, offset) & Long.MAX_VALUE, true);
        }

        Stream<Address> referrers(Address address) {
            return CollectionUtils.asStream(program.getReferenceManager().getReferencesTo(address))
                    .map(Reference::getFromAddress)
                    .filter(Address::isMemoryAddress);
        }
    }

    // --- [TYPEINFO]

    record Base(Address address, long offset, long flags) {}

    record TypeInfo(Address address, Kind kind, String name, List<Base> bases, long size) {
        boolean holds(Address at) {
            return at.compareTo(address) >= 0 && at.subtract(address) < size;
        }

        String row(java.util.function.Function<Address, String> names) {
            return "%s %s %s%s"
                    .formatted(
                            address,
                            kind.name().toLowerCase(Locale.ROOT),
                            name,
                            bases.stream()
                                    .map(base ->
                                            " %s@%d/%d"
                                                    .formatted(
                                                            names.apply(base.address()),
                                                            base.offset(),
                                                            base.flags()))
                                    .collect(Collectors.joining()));
        }
    }

    private static Report.Result<TypeInfo> typeinfo(Image image, Address address, Kind kind) {
        long header = 2L * image.pointer();
        try {
            String name =
                    image.reader()
                            .readAsciiString(image.target(address, image.pointer()).getOffset());
            List<Base> bases =
                    switch (kind) {
                        case CLASS -> List.of();
                        case SI -> List.of(new Base(image.target(address, header), 0, 2));
                        case VMI -> {
                            long count =
                                    image.reader()
                                            .readUnsignedInt(address.getOffset() + header + 4);
                            List<Base> entries = new ArrayList<>();
                            for (long index = 0; index < count; index++) {
                                long offset = header + 8 + index * header;
                                long flags = image.word(address, offset + image.pointer());
                                entries.add(
                                        new Base(
                                                image.target(address, offset),
                                                flags >> 8,
                                                flags & 0xff));
                            }
                            yield List.copyOf(entries);
                        }
                    };
            long size =
                    switch (kind) {
                        case CLASS -> header;
                        case SI -> header + image.pointer();
                        case VMI -> header + 8 + bases.size() * header;
                    };
            return new Report.Success<>(new TypeInfo(address, kind, name, bases, size));
        } catch (IOException exception) {
            return new Report.Failure<>(
                    "%s %s reads unmapped memory: %s"
                            .formatted(
                                    address,
                                    kind.name().toLowerCase(Locale.ROOT),
                                    exception.getMessage()));
        }
    }

    // --- [VTABLES]

    record Slot(int index, Address target, String kind, String name) {}

    record Vtable(
            Address address, TypeInfo type, long offset, List<Slot> slots, List<Address> users) {
        Address point(int pointer) {
            return address.add(2L * pointer);
        }
    }

    // --- [RUN]

    @Override
    public void run() throws Exception {
        Path out = new Arguments.Usage(getScriptName(), "<out>").only(getScriptArgs());
        Image image = Image.of(currentProgram);
        Memory memory = currentProgram.getMemory();
        List<Report.Result<TypeInfo>> parsed =
                CollectionUtils.asStream(
                                currentProgram
                                        .getSymbolTable()
                                        .getSymbolIterator("*__cxxabiv1*", true))
                        .flatMap(symbol ->
                                Kind.of(symbol).stream()
                                        .flatMap(kind ->
                                                Stream.of(
                                                                symbol.getAddress(),
                                                                symbol.getAddress()
                                                                                .isMemoryAddress()
                                                                        ? symbol.getAddress()
                                                                                .add(
                                                                                        2L
                                                                                                * image
                                                                                                        .pointer())
                                                                        : symbol.getAddress())
                                                        .distinct()
                                                        .flatMap(image::referrers)
                                                        .filter(from ->
                                                                !memory.getBlock(from).isExecute())
                                                        .distinct()
                                                        .map(from -> typeinfo(image, from, kind))))
                        .toList();
        List<TypeInfo> types =
                parsed.stream()
                        .flatMap(Report.Result::values)
                        .sorted(Comparator.comparing(TypeInfo::address))
                        .toList();
        Map<Address, String> named =
                types.stream()
                        .collect(
                                Collectors.toMap(
                                        TypeInfo::address, TypeInfo::name, (first, _) -> first));
        java.util.function.Function<Address, String> names =
                address ->
                        Optional.ofNullable(named.get(address))
                                .or(() ->
                                        Optional.ofNullable(
                                                        currentProgram
                                                                .getSymbolTable()
                                                                .getPrimarySymbol(address))
                                                .map(Symbol::getName)
                                                .map(name ->
                                                        TYPEINFO_SYMBOL
                                                                .matcher(name)
                                                                .replaceFirst("")))
                                .orElse(address.toString());
        List<Map.Entry<TypeInfo, Address>> headers =
                types.stream()
                        .flatMap(type ->
                                image.referrers(type.address())
                                        .filter(from ->
                                                types.stream().noneMatch(each -> each.holds(from)))
                                        .map(from ->
                                                Map.entry(type, from.subtract(image.pointer()))))
                        .toList();
        Set<Address> boundaries =
                headers.stream()
                        .flatMap(entry ->
                                Stream.of(entry.getValue(), entry.getValue().add(image.pointer())))
                        .collect(Collectors.toSet());
        Predicate<Address> ends =
                at -> boundaries.contains(at) || types.stream().anyMatch(type -> type.holds(at));
        BiFunction<Address, Integer, Optional<Slot>> slotAt =
                (at, index) -> {
                    try {
                        Address target = image.target(at, 0);
                        MemoryBlock block = memory.getBlock(target);
                        Function function =
                                currentProgram.getFunctionManager().getFunctionAt(target);
                        String symbol =
                                Optional.ofNullable(
                                                currentProgram
                                                        .getSymbolTable()
                                                        .getPrimarySymbol(target))
                                        .map(each -> each.getName(true))
                                        .orElse("-");
                        return function != null
                                ? Optional.of(
                                        new Slot(index, target, "function", function.getName(true)))
                                : block == null
                                        ? Optional.empty()
                                        : block.getName()
                                                        .startsWith(MemoryBlock.EXTERNAL_BLOCK_NAME)
                                                ? Optional.of(
                                                        new Slot(index, target, "import", symbol))
                                                : block.isExecute()
                                                        ? Optional.of(
                                                                new Slot(
                                                                        index, target, "code",
                                                                        symbol))
                                                        : Optional.empty();
                    } catch (IOException exception) {
                        return Optional.empty();
                    }
                };
        List<Vtable> vtables =
                headers.stream()
                        .flatMap(entry -> {
                            try {
                                long offset =
                                        image.reader()
                                                .readValue(
                                                        entry.getValue().getOffset(),
                                                        image.pointer());
                                if (offset > 0) {
                                    return Stream.empty();
                                }
                                Address point = entry.getValue().add(2L * image.pointer());
                                MemoryBlock block = memory.getBlock(point);
                                List<Slot> slots =
                                        Stream.iterate(0, index -> index + 1)
                                                .map(index ->
                                                        Map.entry(
                                                                index,
                                                                point.add(
                                                                        (long) index
                                                                                * image.pointer())))
                                                .takeWhile(each -> block.contains(each.getValue()))
                                                .takeWhile(each -> !ends.test(each.getValue()))
                                                .map(each ->
                                                        slotAt.apply(
                                                                each.getValue(), each.getKey()))
                                                .takeWhile(Optional::isPresent)
                                                .map(Optional::get)
                                                .toList();
                                return Stream.of(
                                        new Vtable(
                                                entry.getValue(),
                                                entry.getKey(),
                                                offset,
                                                slots,
                                                image.referrers(point).sorted().toList()));
                            } catch (IOException exception) {
                                return Stream.empty();
                            }
                        })
                        .sorted(Comparator.comparing(Vtable::address))
                        .toList();
        List<Report.Section> sections =
                List.of(
                        Report.Section.rows(
                                "TYPEINFO", types.stream().map(type -> type.row(names)).toList()),
                        Report.Section.rows(
                                "VTABLES",
                                vtables.stream()
                                        .map(vtable ->
                                                "%s %s %s offset=%d slots=%d"
                                                        .formatted(
                                                                vtable.address(),
                                                                vtable.type().address(),
                                                                vtable.type().name(),
                                                                vtable.offset(),
                                                                vtable.slots().size()))
                                        .toList()),
                        Report.Section.rows(
                                "SLOTS",
                                vtables.stream()
                                        .flatMap(vtable ->
                                                vtable.slots().stream()
                                                        .map(slot ->
                                                                "%s %d %s %s %s"
                                                                        .formatted(
                                                                                vtable.address(),
                                                                                slot.index(),
                                                                                slot.target(),
                                                                                slot.kind(),
                                                                                slot.name())))
                                        .toList()),
                        Report.Section.rows(
                                "USERS",
                                vtables.stream()
                                        .flatMap(vtable ->
                                                vtable.users().stream()
                                                        .map(from ->
                                                                "%s %s %s"
                                                                        .formatted(
                                                                                vtable.address(),
                                                                                from,
                                                                                Optional.ofNullable(
                                                                                                getFunctionContaining(
                                                                                                        from))
                                                                                        .map(
                                                                                                function ->
                                                                                                        function
                                                                                                                .getName(
                                                                                                                        true))
                                                                                        .orElse(
                                                                                                "-"))))
                                        .toList()),
                        Report.Section.rows(
                                "FAILED",
                                parsed.stream().flatMap(Report.Result::failures).toList()));
        println(Report.write(out, currentProgram, sections, ""));
    }
}
