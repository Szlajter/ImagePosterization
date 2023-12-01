#pragma once

#ifdef POSTERIZATIONCPPLIB_EXPORTS
#define POSTERIZATIONCPPLIB_API __declspec(dllexport)
#else
#define POSTERIZATIONCPPLIB_API __declspec(dllimport)
#endif

extern "C" POSTERIZATIONCPPLIB_API int posterization_init();

