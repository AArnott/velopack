#ifndef __PLATFORM_UTIL_H__
#define __PLATFORM_UTIL_H__

#include <string>

namespace util
{
    std::wstring get_temp_file_path(std::wstring extension);
    std::wstring get_current_process_path();
    bool check_diskspace(uint64_t requiredSpace);
    void wexec(const wchar_t* cmd);
    void show_error_dialog(std::wstring msg);
    uint8_t* mmap_read(const std::wstring& filePath, size_t* length);
    bool munmap(uint8_t* addr);
    std::wstring pretty_bytes(uint64_t bytes);
}

#endif // __PLATFORM_UTIL_H__