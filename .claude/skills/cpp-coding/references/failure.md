# [FAILURE]

## [01]-[PIPELINES]

Dependent steps chain through the members of `std::expected`, and a failed step skips each later one:

```cpp
[[nodiscard]] constexpr auto number(std::string_view text) noexcept -> std::expected<std::uint32_t, Refusal> {
    std::uint32_t value{};
    const auto [end, code] = std::from_chars(text.data(), std::to_address(text.end()), value);
    return code == std::errc{} && end == std::to_address(text.end()) ? std::expected<std::uint32_t, Refusal>{value} : std::unexpected{Refusal::Malformed};
}

[[nodiscard]] constexpr auto bounded(Extent extent, std::uint64_t limit) noexcept -> std::expected<Extent, Refusal> {
    return extent.cells() > limit ? std::unexpected{Refusal::Oversized} : std::expected<Extent, Refusal>{extent};
}

[[nodiscard]] constexpr auto extent_of(std::string_view text, std::uint64_t limit) noexcept -> std::expected<Extent, Refusal> {
    const auto split = text.find('x');
    return split == std::string_view::npos
               ? std::unexpected{Refusal::Malformed}
               : number(text.substr(0, split))
                     .and_then([&](std::uint32_t width) { return number(text.substr(split + 1)).and_then([&](std::uint32_t height) { return Extent::make(width, height); }); })
                     .and_then([&](Extent parsed) { return bounded(parsed, limit); });
}
```

`Extent::make` refuses a zero dimension with `Refusal::Empty`, and `cells` returns width times height. `std::from_chars` allocates nothing, reads no locale, and evaluates in a constant expression.

## [02]-[ACCUMULATION]

Independent checks all run, and their failures return together with the subject of each:

```cpp
struct Defect {
    std::size_t position;
    Refusal refusal;
};

[[nodiscard]] constexpr auto extents_of(std::span<const std::string_view> texts, std::uint64_t limit) -> std::expected<std::vector<Extent>, std::vector<Defect>> {
    std::vector<Extent> extents;
    std::vector<Defect> defects;
    extents.reserve(texts.size());
    for (const auto [position, text] : std::views::enumerate(texts)) {
        if (const auto parsed = extent_of(text, limit)) {
            extents.push_back(*parsed);
        } else {
            defects.push_back({.position = static_cast<std::size_t>(position), .refusal = parsed.error()});
        }
    }
    return defects.empty() ? std::expected<std::vector<Extent>, std::vector<Defect>>{std::move(extents)} : std::unexpected{std::move(defects)};
}
```

## [03]-[FOREIGN_FAILURES]

Failures of a dependency stay `std::error_code`, read through its overload that reports one:

```cpp
[[nodiscard]] auto bytes_of(const std::filesystem::path& file) noexcept -> std::expected<std::uintmax_t, std::error_code> {
    std::error_code code;
    const auto bytes = std::filesystem::file_size(file, code);
    return code ? std::unexpected{code} : std::expected<std::uintmax_t, std::error_code>{bytes};
}
```

## [04]-[MESSAGES]

Refusals name themselves through one `switch` in their namespace:

```cpp
[[nodiscard]] constexpr auto label(Refusal refusal) noexcept -> std::string_view {
    switch (refusal) {
    case Refusal::Empty:
        return "empty";
    case Refusal::Oversized:
        return "oversized";
    case Refusal::Malformed:
        return "malformed";
    }
    std::unreachable();
}
```

`std::formatter` specializations sit in the global namespace and print through it:

```cpp
template <>
struct std::formatter<raster::Refusal> : std::formatter<std::string_view> {
    auto format(raster::Refusal refusal, std::format_context& context) const { return std::formatter<std::string_view>::format(raster::label(refusal), context); }
};
```

## [05]-[ENTRY_POINT]

Entry points decode their arguments once, print each failure, and return an exit status:

```cpp
auto main(int count, char** values) -> int {
    const auto texts =
        std::span{values, static_cast<std::size_t>(count)} | std::views::drop(1) | std::views::transform([](const char* value) { return std::string_view{value}; }) | std::ranges::to<std::vector>();
    const auto parsed = raster::extents_of(texts, 1'000'000);
    if (!parsed) {
        std::ranges::for_each(parsed.error(), [](raster::Defect defect) { std::println(std::cerr, "argument {}: {}", defect.position + 1, defect.refusal); });
        return 2;
    }
    std::println("{} extents", parsed->size());
    return 0;
}
```
