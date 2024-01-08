#include "pch.h"
#include <utility>
#include <limits.h>
#include <cmath>
#include "PosterizationCppLib.h"

void posterize(BYTE* image, int width, int height, int levels) {

    // Iterate through each pixel in the image
    for (int i = 0; i < width * height * 4; i += 4) {
        // For each channel (R, G, B), quantize the color intensity
        for (int channel = 0; channel < 3; ++channel) {
            // Calculate the quantized intensity
            int intensity = image[i + channel];
            int quantized_intensity = static_cast<int>(std::round((intensity * (levels - 1)) / 255.0));

            // Map the quantized intensity back to the 0-255 range
            image[i + channel] = static_cast<BYTE>((quantized_intensity * 255) / (levels - 1));
        }
    }
}
