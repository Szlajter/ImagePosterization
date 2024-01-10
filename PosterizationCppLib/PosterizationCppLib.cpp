#include "pch.h"
#include <utility>
#include <limits.h>
#include <cmath>
#include "PosterizationCppLib.h"

void posterize(BYTE* image, int width, int height, int level) {

    float interval1 = (level - 1) / 255.0;
    float interval2 = 255.0 / (level - 1);

    // Iterate through each pixel in the image
    for (int i = 0; i < width * height * 3; i ++) {
            image[i] = std::round((image[i] * interval1)) * interval2;
    }
}