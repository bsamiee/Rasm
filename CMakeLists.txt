cmake_minimum_required(VERSION 4.5)

project(rasm LANGUAGES CXX)

# --- [POLICY] ---------------------------------------------------------------------------

set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_COMPILE_WARNING_AS_ERROR ON)
set(CMAKE_LINK_LIBRARIES_ONLY_TARGETS ON)
set(CMAKE_VERIFY_INTERFACE_HEADER_SETS ON)
set(CMAKE_VERIFY_PRIVATE_HEADER_SETS ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_COLOR_DIAGNOSTICS ON)
set(flags
    -Wall
    -Wextra
    -Wpedantic
    -Wshadow
    -Wconversion
    -Wsign-conversion
    -Wdouble-promotion
    -Wold-style-cast
    -Wcast-align
    -Wcast-qual
    -Wnon-virtual-dtor
    -Wnull-dereference
    -Wformat=2
    -Wimplicit-fallthrough
    -Wthread-safety
    -D_LIBCPP_HARDENING_MODE=_LIBCPP_HARDENING_MODE_EXTENSIVE
)

# --- [INSTRUMENTATION] ------------------------------------------------------------------

set(RASM_SANITIZERS "" CACHE STRING "Sanitizers every target compiles and links with")
set(RASM_COVERAGE OFF CACHE BOOL "Whether every target records source-based coverage")
if(RASM_SANITIZERS)
    list(JOIN RASM_SANITIZERS "," sanitizers)
    list(APPEND flags -fsanitize=${sanitizers} -fno-sanitize-recover=all -fno-omit-frame-pointer)
endif()
if(RASM_COVERAGE)
    list(APPEND flags -fprofile-instr-generate=${CMAKE_BINARY_DIR}/profiles/%m-%p.profraw -fcoverage-mapping)
endif()

# --- [LANGUAGES] ------------------------------------------------------------------------

list(JOIN flags " " flags)
string(APPEND CMAKE_CXX_FLAGS " ${flags}")
if(APPLE)
    enable_language(OBJCXX)
    set(CMAKE_OBJCXX_STANDARD 26)
    set(CMAKE_OBJCXX_STANDARD_REQUIRED ON)
    string(APPEND CMAKE_OBJCXX_FLAGS " ${flags} -fobjc-arc")
endif()

# --- [PACKAGING] ------------------------------------------------------------------------

set(CMAKE_PROJECT_INCLUDE "${CMAKE_CURRENT_SOURCE_DIR}/.config/cmake/package.cmake")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/.config/cmake/triplets/${VCPKG_TARGET_TRIPLET}.cmake")

# --- [MEMBERS] --------------------------------------------------------------------------

option(BUILD_TESTING "Whether the test project of each package builds" ON)
file(GLOB manifests CONFIGURE_DEPENDS RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}" "libs/cpp/*/*/CMakeLists.txt")
if(BUILD_TESTING)
    enable_testing()
    set(CMAKE_CATCH_DISCOVER_TESTS_DISCOVERY_MODE PRE_TEST)
    include(.config/cmake/tests.cmake)
    file(GLOB suites CONFIGURE_DEPENDS RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}" "libs/cpp/*/tests/*/CMakeLists.txt")
endif()
foreach(manifest IN LISTS manifests suites)
    cmake_path(GET manifest PARENT_PATH member)
    add_subdirectory("${member}")
    get_property(targets DIRECTORY "${member}" PROPERTY BUILDSYSTEM_TARGETS)
    foreach(target IN LISTS targets)
        get_target_property(type "${target}" TYPE)
        if(type STREQUAL "EXECUTABLE")
            list(APPEND instrumented "--object=$<TARGET_FILE:${target}>")
        endif()
    endforeach()
endforeach()

# --- [COVERAGE] -------------------------------------------------------------------------

if(RASM_COVERAGE)
    add_custom_target(
        coverage
        COMMAND llvm-profdata merge --sparse --output=coverage.profdata profiles
        COMMAND "${CMAKE_COMMAND}" -E rm -rf profiles
        COMMAND llvm-cov show --instr-profile=coverage.profdata --format=html --output-dir=coverage ${instrumented}
        COMMAND llvm-cov report --instr-profile=coverage.profdata ${instrumented}
        WORKING_DIRECTORY "${CMAKE_BINARY_DIR}"
        VERBATIM
    )
endif()
