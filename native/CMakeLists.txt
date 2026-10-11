cmake_minimum_required(VERSION 3.15)
project(TrackSpenderCore C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
# Link the C runtime statically so the DLL runs without the Visual C++ redistributable.
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded")

add_library(trackspender_core SHARED src/storage.c src/stats.cpp)
target_include_directories(trackspender_core PUBLIC include)
target_compile_definitions(trackspender_core PRIVATE TS_BUILD)

enable_testing()
add_executable(test_core tests/test_core.c)
target_link_libraries(test_core PRIVATE trackspender_core)
add_test(NAME core COMMAND test_core)
