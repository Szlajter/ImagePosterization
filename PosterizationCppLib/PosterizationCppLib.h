#pragma once
#include <cstddef>

#ifdef POSTERIZATIONCPPLIB_EXPORTS
#define POSTERIZATIONCPPLIB_API __declspec(dllexport)
#else
#define POSTERIZATIONCPPLIB_API __declspec(dllimport)
#endif

extern "C" POSTERIZATIONCPPLIB_API void posterize(BYTE* image, int start, int end, int level);

