# [COMPILE_TIME]

## [01]-[LITERALS]

Literals with an invariant convert through a `consteval` constructor, and an invalid one fails to compile:

```cpp
auto refused(std::string_view rule) -> void;

class Hex {
public:
    template <std::convertible_to<std::string_view> Text>
    consteval explicit(false) Hex(const Text& text)
        : value_{parsed(text)} {}

    [[nodiscard]] constexpr auto value() const noexcept -> std::uint32_t { return value_; }

private:
    static constexpr auto parsed(std::string_view text) -> std::uint32_t {
        std::uint32_t value{};
        const auto [end, code] = std::from_chars(text.data(), std::to_address(text.end()), value, 16);
        return text.size() == 6 && code == std::errc{} && end == std::to_address(text.end()) ? value : (refused("six hexadecimal digits"), 0U);
    }

    std::uint32_t value_;
};

[[nodiscard]] constexpr auto tint(Hex color) noexcept -> std::uint32_t { return color.value(); }
```

`refused` is declared and never defined. Calling it ends constant evaluation, and the diagnostic quotes that call, where a `throw` fails to compile in a translation unit with exceptions off. `tint("ff8800")` compiles and `tint("ff880")` does not.

## [02]-[TABLES]

Tables a formula fills are computed by a `constexpr` lambda called in place:

```cpp
inline constexpr auto squares = [] {
    std::array<std::uint8_t, 256> table{};
    std::ranges::copy(std::views::iota(0U, 256U) | std::views::transform([](std::uint32_t level) { return static_cast<std::uint8_t>(level * level / 255U); }), table.begin());
    return table;
}();
```

## [03]-[CONSTRAINTS]

Requirements a template places on its types are a constraint, and a type failing it leaves no viable overload:

```cpp
template <typename Pixel>
    requires std::has_unique_object_representations_v<Pixel>
[[nodiscard]] constexpr auto bytes_of(std::span<const Pixel> pixels) noexcept -> std::span<const std::byte> {
    return std::as_bytes(pixels);
}
```

`bytes_of` accepts a trivially copyable type with one object representation per value, so a type with padding or a floating-point member has no overload. A `static_assert` in its body would reject the same types where no `requires` expression reads that refusal.

## [04]-[PACKS]

Packs index by position, structured bindings bind a pack, and `template for` visits each element of a tuple:

```cpp
template <typename... Planes>
[[nodiscard]] constexpr auto first_of(const Planes&... planes) noexcept -> const Planes...[0]& {
    return planes...[0];
}

template <typename Record>
[[nodiscard]] constexpr auto field_bytes(const Record& record) noexcept -> std::size_t {
    const auto& [... fields] = record;
    return (sizeof(fields) + ...);
}

template <typename... Planes>
constexpr auto clear(std::tuple<Planes...>& planes) noexcept -> void {
    template for (auto& plane : planes) { std::ranges::fill(plane, std::ranges::range_value_t<decltype(plane)>{}); }
}
```
