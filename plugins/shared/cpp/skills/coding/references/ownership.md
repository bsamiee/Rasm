# [OWNERSHIP]

## [01]-[VIEWS]

Members returning a view into their object mark the object parameter `[[clang::lifetimebound]]`:

```cpp
class Raster {
public:
    explicit Raster(Extent extent)
        : cells_(std::size_t{extent.width()} * extent.height()), extent_{extent} {}

    template <typename Self>
    [[nodiscard]] auto row(this Self& self [[clang::lifetimebound]], std::uint32_t index) noexcept {
        return std::span{self.cells_}.subspan(std::size_t{index} * self.extent_.width(), self.extent_.width());
    }

    [[nodiscard]] auto grid() noexcept [[clang::lifetimebound]] -> std::mdspan<Rgb, std::dextents<std::size_t, 2>> {
        return std::mdspan{cells_.data(), std::size_t{extent_.height()}, std::size_t{extent_.width()}};
    }

private:
    std::vector<Rgb> cells_;
    Extent extent_;
};
```

`Rgb` is an aggregate of `red`, `green`, and `blue`, and `Extent` a value with a nonzero `width` and `height`. `row` takes an explicit object parameter: one member returns `std::span<const Rgb>` from a `const` raster and `std::span<Rgb>` from a mutable one, and a temporary `Raster` has no `row`. `grid()[row, column]` reads one cell of the same storage.

## [02]-[OPTIONAL_BORROWS]

Borrows that can be absent return `std::optional<T&>`:

```cpp
class Layers {
public:
    auto add(std::string name, Raster raster) -> void { layers_.emplace_back(std::move(name), std::move(raster)); }

    [[nodiscard]] auto find(std::string_view name) noexcept [[clang::lifetimebound]] -> std::optional<Raster&> {
        const auto found = std::ranges::find(layers_, name, &std::pair<std::string, Raster>::first);
        return found == layers_.end() ? std::nullopt : std::optional<Raster&>{found->second};
    }

private:
    std::vector<std::pair<std::string, Raster>> layers_;
};
```

`layers.find(name).transform(<step>)` runs a step on a present raster. References from `find` end at the next `add`, which can move every element.

## [03]-[CAPTURES]

Lambdas returned or stored hold copies or moved values, and their borrowed subject arrives as a parameter:

```cpp
[[nodiscard]] auto painter(Rgb color) {
    return [color](std::span<Rgb> cells) { std::ranges::fill(cells, color); };
}
```

## [04]-[ARENAS]

Functions whose caller picks the source of their temporary storage take a memory resource:

```cpp
[[nodiscard]] auto median(std::span<const Rgb> row, std::pmr::memory_resource& scratch) -> std::optional<std::uint32_t> {
    std::pmr::vector<std::uint32_t> sums{&scratch};
    sums.append_range(row | std::views::transform([](Rgb color) { return std::uint32_t{color.red} + color.green + color.blue; }));
    const auto middle = sums.begin() + (std::ssize(sums) / 2);
    std::ranges::nth_element(sums, middle);
    return middle == sums.end() ? std::nullopt : std::optional{*middle};
}
```

Callers on a hot path pass a `std::pmr::monotonic_buffer_resource` over a stack array sized for their largest row, and a larger row allocates from its upstream resource.
