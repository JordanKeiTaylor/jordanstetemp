//
// Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//

// Code is a subset of the code found in Sample.cpp
// https://github.com/recastnavigation/recastnavigation/blob/4988ecbaf094df18b1bf61a27a018d7e4eef4225/RecastDemo/Source/Sample.cpp#L342

#include <stdio.h>
#include <string.h>

#include "Sample_subset.h"

namespace Sample {
    static const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'MSET';
    static const int NAVMESHSET_VERSION = 1;

    struct NavMeshSetHeader {
        int magic;
        int version;
        int numTiles;
        dtNavMeshParams params;
    };

    struct NavMeshTileHeader {
        dtTileRef tileRef;
        int dataSize;
    };

    dtNavMesh *loadAll(const char *path) {
        FILE *fp = fopen(path, "rb");
        if (!fp) return 0;

        // Read header.
        NavMeshSetHeader header;
        size_t readLen = fread(&header, sizeof(NavMeshSetHeader), 1, fp);
        if (readLen != 1) {
            fclose(fp);
            return 0;
        }
        if (header.magic != NAVMESHSET_MAGIC) {
            fclose(fp);
            return 0;
        }
        if (header.version != NAVMESHSET_VERSION) {
            fclose(fp);
            return 0;
        }

        dtNavMesh *mesh = dtAllocNavMesh();
        if (!mesh) {
            fclose(fp);
            return 0;
        }
        dtStatus status = mesh->init(&header.params);
        if (dtStatusFailed(status)) {
            fclose(fp);
            return 0;
        }

        // Read tiles.
        for (int i = 0; i < header.numTiles; ++i) {
            NavMeshTileHeader tileHeader;
            readLen = fread(&tileHeader, sizeof(tileHeader), 1, fp);
            if (readLen != 1) {
                fclose(fp);
                return 0;
            }

            if (!tileHeader.tileRef || !tileHeader.dataSize)
                break;

            unsigned char *data = (unsigned char *) dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
            if (!data) break;
            memset(data, 0, tileHeader.dataSize);
            readLen = fread(data, tileHeader.dataSize, 1, fp);
            if (readLen != 1) {
                dtFree(data);
                fclose(fp);
                return 0;
            }

            mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
        }

        fclose(fp);

        return mesh;
    }
}