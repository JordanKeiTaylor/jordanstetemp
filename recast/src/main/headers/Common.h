//
//  Common.h
//

#pragma once

const int MAX_PATH_LEN = 1024;
const int MAX_SMOOTH_PATH_LEN = 4096;

struct SmoothPathResult {
    float path[3 * MAX_SMOOTH_PATH_LEN];
    int pathCount;
};

struct FindPathResult {
    dtStatus status;
    dtPolyRef path[MAX_PATH_LEN];
    int pathCount;
};
