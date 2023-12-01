#include "pch.h"
#include <utility>
#include <limits.h>
#include "PosterizationCppLib.h"

// DLL internal state variables:
static unsigned long long example_;  // Previous value, if any


int posterization_init()
{
    return 42;
}