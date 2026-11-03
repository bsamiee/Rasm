import ghidra.app.script.GhidraScript;
import ghidra.app.util.bin.BinaryReader;
import ghidra.app.util.bin.MemoryByteProvider;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressSet;
import ghidra.program.model.address.AddressSpace;
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
import java.util.function.BiFunction;
import java.util.function.Function;
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

        Address point(Address vtable) {
            return vtable.add(2L * pointer);
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
        String row(Function<Address, String> names) {
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

    record Slot(int index, Address target, Slot.Kind kind, String name) {
        enum Kind {
            FUNCTION,
            IMPORT,
            CODE;

            static Optional<Kind> of(boolean function, MemoryBlock block) {
                return function
                        ? Optional.of(FUNCTION)
                        : Optional.ofNullable(block)
                                .flatMap(each ->
                                        each.getName().startsWith(MemoryBlock.EXTERNAL_BLOCK_NAME)
                                                ? Optional.of(IMPORT)
                                                : Optional.of(CODE).filter(_ -> each.isExecute()));
            }
        }
    }

    record Vtable(
            Address address, TypeInfo type, long offset, List<Slot> slots, List<Address> users) {}

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
                                                Stream.concat(
                                                                Stream.of(symbol.getAddress()),
                                                                Stream.of(symbol.getAddress())
                                                                        .filter(Address::isMemoryAddress)
                                                                        .map(image::point))
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
        Function<Address, String> names =
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
        AddressSet typeinfos = new AddressSet();
        types.forEach(type ->
                typeinfos.add(type.address(), type.address().add(type.size() - 1)));
        List<Map.Entry<TypeInfo, Address>> headers =
                types.stream()
                        .flatMap(type ->
                                image.referrers(type.address())
                                        .filter(from -> !memory.getBlock(from).isExecute())
                                        .filter(from -> !typeinfos.contains(from))
                                        .map(from ->
                                                Map.entry(type, from.subtract(image.pointer()))))
                        .toList();
        AddressSet ends = new AddressSet(typeinfos);
        headers.forEach(entry ->
                ends.add(entry.getValue(), image.point(entry.getValue()).subtract(1)));
        BiFunction<Address, Address, Report.Result<Optional<Slot>>> slotAt =
                (point, at) -> {
                    try {
                        Address target = image.target(at, 0);
                        return new Report.Success<>(
                                Slot.Kind.of(getFunctionAt(target) != null, memory.getBlock(target))
                                        .map(kind ->
                                                new Slot(
                                                        (int) (at.subtract(point) / image.pointer()),
                                                        target,
                                                        kind,
                                                        Optional.ofNullable(
                                                                        currentProgram
                                                                                .getSymbolTable()
                                                                                .getPrimarySymbol(
                                                                                        target))
                                                                .map(each -> each.getName(true))
                                                                .orElse("-"))));
                    } catch (IOException exception) {
                        return new Report.Failure<>(
                                "%s slot reads unmapped memory: %s"
                                        .formatted(at, exception.getMessage()));
                    }
                };
        List<Report.Result<Vtable>> read =
                headers.stream()
                        .<Report.Result<Vtable>>flatMap(entry -> {
                            Address vtable = entry.getValue();
                            Address point = image.point(vtable);
                            MemoryBlock block = memory.getBlock(point);
                            long offset;
                            try {
                                offset = image.word(vtable, 0);
                            } catch (IOException exception) {
                                return Stream.of(
                                        new Report.Failure<>(
                                                "%s vtable reads unmapped memory: %s"
                                                        .formatted(
                                                                vtable, exception.getMessage())));
                            }
                            return offset > 0
                                    ? Stream.empty()
                                    : Stream.of(
                                            Report.all(
                                                            Stream.iterate(
                                                                            point,
                                                                            at ->
                                                                                    at.add(
                                                                                            image
                                                                                                    .pointer()))
                                                                    .takeWhile(block::contains)
                                                                    .takeWhile(at ->
                                                                            !ends.contains(at))
                                                                    .map(at ->
                                                                            slotAt.apply(point, at))
                                                                    .takeWhile(slot ->
                                                                            slot.values()
                                                                                    .allMatch(
                                                                                            Optional
                                                                                                    ::isPresent)))
                                                    .map(slots ->
                                                            new Vtable(
                                                                    vtable,
                                                                    entry.getKey(),
                                                                    offset,
                                                                    slots.stream()
                                                                            .flatMap(Optional::stream)
                                                                            .toList(),
                                                                    image.referrers(point)
                                                                            .sorted()
                                                                            .toList())));
                        })
                        .toList();
        List<Vtable> vtables =
                read.stream()
                        .flatMap(Report.Result::values)
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
                                                                                slot.kind()
                                                                                        .name()
                                                                                        .toLowerCase(
                                                                                                Locale
                                                                                                        .ROOT),
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
                                Stream.concat(parsed.stream(), read.stream())
                                        .flatMap(Report.Result::failures)
                                        .toList()));
        println(Report.write(out, currentProgram, sections, ""));
    }
}
