# --- [COMPILERS] ------------------------------------------------------------------------

find_program(CMAKE_C_COMPILER clang REQUIRED)
find_program(CMAKE_CXX_COMPILER clang++ REQUIRED)
set(CMAKE_OBJCXX_COMPILER "${CMAKE_CXX_COMPILER}")

# --- [STANDARD_LIBRARY] -----------------------------------------------------------------

if(CMAKE_HOST_APPLE)
    set(archives libc++.a libc++abi.a)
    set(loaded "-Wl,-load_hidden,")
else()
    set(archives libc++.a libc++abi.a libunwind.a)
    set(CMAKE_LINKER_TYPE LLD)
    set(CMAKE_CXX_FLAGS_INIT "-stdlib=libc++")
    foreach(kind IN ITEMS EXE SHARED MODULE)
        set(CMAKE_${kind}_LINKER_FLAGS_INIT "--rtlib=compiler-rt --unwindlib=libunwind")
    endforeach()
endif()
set(standard -nostdlib++)
foreach(archive IN LISTS archives)
    execute_process(COMMAND "${CMAKE_CXX_COMPILER}" -print-file-name=${archive} OUTPUT_VARIABLE path OUTPUT_STRIP_TRAILING_WHITESPACE COMMAND_ERROR_IS_FATAL ANY)
    cmake_path(NORMAL_PATH path)
    list(APPEND standard "${loaded}${path}")
endforeach()
list(JOIN standard " " standard)
set(CMAKE_CXX_STANDARD_LIBRARIES_INIT "${standard}")
set(CMAKE_OBJCXX_STANDARD_LIBRARIES_INIT "${standard}")
