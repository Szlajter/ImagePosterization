#include "pch.h"
#include <utility>
#include <limits.h>
#include <cmath>
#include "PosterizationCppLib.h"

void posterize(BYTE* image, int start, int end, int level) {

    float interval1 = (level - 1) / 255.0;
    float interval2 = 255.0 / (level - 1);

    for (int y = start; y < end; y+=3)
    {
        for (int channel = 0; channel < 3; ++channel)
        {
            image[y + channel] = std::round(image[y + channel] * interval1) * interval2;
        }
    }
}

