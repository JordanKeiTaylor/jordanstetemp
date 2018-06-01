//
//  NavMeshTesterTool_subset.h
//

#ifndef NavMeshTesterTool_subset_h
#define NavMeshTesterTool_subset_h

#include "DetourCommon.h"
#include "DetourNavMeshQuery.h"
#include "Common.h"

#endif /* NavMeshTesterTool_subset_h */

SmoothPathResult* getSmoothPath(float* startPos, dtPolyRef startRef, float* endPos,
                               FindPathResult* path,
                               const dtQueryFilter* filter,
                               dtNavMesh* navMesh, dtNavMeshQuery* navQuery);
