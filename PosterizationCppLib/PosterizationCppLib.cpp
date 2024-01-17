#include "pch.h"
#include <utility>
#include <limits.h>
#include <cmath>
#include "PosterizationCppLib.h"

void posterize(BYTE* image, int width, int height, int level) {

    float interval1 = (level - 1) / 255.0;
    float interval2 = 255.0 / (level - 1);

    for (int i = 0; i < width * height * 4; i += 4) {
        for (int channel = 0; channel < 3; ++channel) {
            image[i + channel] = std::round(image[i + channel] * interval1) * interval2;
        }
    }
}

