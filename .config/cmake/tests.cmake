include_guard(GLOBAL)

# --- [REJECTIONS] -----------------------------------------------------------------------

function(add_reject_tests name)
    cmake_parse_arguments(PARSE_ARGV 1 reject "" "SOURCE" "LIBRARIES")
    cmake_path(ABSOLUTE_PATH reject_SOURCE OUTPUT_VARIABLE source)
    set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${source}")
    file(STRINGS "${source}" guards REGEX "^#ifdef REJECT_[A-Za-z0-9_]+$")
    add_library(${name} OBJECT "${source}")
    target_link_libraries(${name} PRIVATE ${reject_LIBRARIES})
    foreach(guard IN LISTS guards)
        string(REPLACE "#ifdef " "" case "${guard}")
        add_test(
            NAME ${name}-${case}
            COMMAND
                sh -c "eval \"$(\"$0\" -t commands -s \"$1\")\" -D${case} -fsyntax-only -Wno-unused-command-line-argument -Xclang -verify -Xclang -verify-ignore-unexpected=note"
                "${CMAKE_MAKE_PROGRAM}" "$<PATH:RELATIVE_PATH,$<TARGET_OBJECTS:${name}>,${CMAKE_BINARY_DIR}>"
            WORKING_DIRECTORY "${CMAKE_BINARY_DIR}"
        )
        set_tests_properties(${name}-${case} PROPERTIES LABELS reject)
    endforeach()
endfunction()

# --- [FUZZ_TARGETS] ---------------------------------------------------------------------

function(add_fuzz_test name)
    cmake_parse_arguments(PARSE_ARGV 1 fuzz "" "CORPUS;RUNS" "SOURCES;LIBRARIES")
    if("fuzzer-no-link" IN_LIST RASM_SANITIZERS)
        add_executable(${name} ${fuzz_SOURCES})
        target_link_options(${name} PRIVATE -fsanitize=fuzzer)
        add_test(
            NAME ${name}
            COMMAND ${name} -runs=${fuzz_RUNS} -create_missing_dirs=1 "${CMAKE_CURRENT_BINARY_DIR}/${name}-corpus" "${CMAKE_CURRENT_SOURCE_DIR}/${fuzz_CORPUS}"
        )
        set_tests_properties(${name} PROPERTIES LABELS fuzz)
    else()
        add_library(${name} OBJECT ${fuzz_SOURCES})
    endif()
    target_link_libraries(${name} PRIVATE ${fuzz_LIBRARIES})
endfunction()

# --- [CONSUMERS] ------------------------------------------------------------------------

function(add_consumer_test name)
    cmake_parse_arguments(PARSE_ARGV 1 consumer "" "PACKAGE;SOURCE" "")
    set(prefix "${CMAKE_CURRENT_BINARY_DIR}/${name}/prefix")
    add_test(
        NAME ${name}-install
        COMMAND "${CMAKE_COMMAND}" --install "${${consumer_PACKAGE}_BINARY_DIR}" --prefix "${prefix}"
    )
    add_test(
        NAME ${name}
        COMMAND
            "${CMAKE_CTEST_COMMAND}" --build-and-test "${CMAKE_CURRENT_SOURCE_DIR}/${consumer_SOURCE}" "${CMAKE_CURRENT_BINARY_DIR}/${name}/build" --build-generator "${CMAKE_GENERATOR}"
            --build-options "-DCMAKE_TOOLCHAIN_FILE=${VCPKG_CHAINLOAD_TOOLCHAIN_FILE}" "-DCMAKE_BUILD_TYPE=${CMAKE_BUILD_TYPE}" "-DCMAKE_CXX_FLAGS=${CMAKE_CXX_FLAGS}"
            "-DCMAKE_PREFIX_PATH=${prefix};${VCPKG_INSTALLED_DIR}/${VCPKG_TARGET_TRIPLET}" --test-command "${CMAKE_CTEST_COMMAND}" --output-on-failure --no-tests=error
    )
    set_tests_properties(${name}-install PROPERTIES FIXTURES_SETUP ${name})
    set_tests_properties(${name} PROPERTIES FIXTURES_REQUIRED ${name})
endfunction()
