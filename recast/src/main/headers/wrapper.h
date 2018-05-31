#include <stdio.h>

#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshBuilder.h>
#include <DetourNavMeshQuery.h>

#include "Common.h"
#include "MeshLoaderObj.h"
#include "InputGeom.h"
#include "NavMeshTesterTool_subset.h"

class IoRcContext : public rcContext {
    public:
    IoRcContext(): rcContext(true) {}
    void doLog(const rcLogCategory category, const char* msg, const int len) override {
        printf("LOG: %s\n", msg);
    }
};

enum SamplePartitionType
{
	SAMPLE_PARTITION_WATERSHED,
	SAMPLE_PARTITION_MONOTONE,
	SAMPLE_PARTITION_LAYERS,
};

/// These are just sample areas to use consistent values across the samples.
/// The use should specify these base on his needs.
enum SamplePolyAreas
{
	SAMPLE_POLYAREA_GROUND,
	SAMPLE_POLYAREA_WATER,
	SAMPLE_POLYAREA_ROAD,
	SAMPLE_POLYAREA_DOOR,
	SAMPLE_POLYAREA_GRASS,
	SAMPLE_POLYAREA_JUMP,
};

enum SamplePolyFlags
{
	SAMPLE_POLYFLAGS_WALK		= 0x01,		// Ability to walk (ground, grass, road)
	SAMPLE_POLYFLAGS_SWIM		= 0x02,		// Ability to swim (water).
	SAMPLE_POLYFLAGS_DOOR		= 0x04,		// Ability to move through doors.
	SAMPLE_POLYFLAGS_JUMP		= 0x08,		// Ability to jump.
	SAMPLE_POLYFLAGS_DISABLED	= 0x10,		// Disabled polygon
	SAMPLE_POLYFLAGS_ALL		= 0xffff	// All abilities.
};

extern "C"
struct NavMeshDataResult {
    unsigned char* data;
    int size;
};

extern "C"
struct PolyPointResult {
    dtStatus status;
	dtPolyRef polyRef;
	float point[3];
};

extern "C" rcContext* rcContext_create();
extern "C" void rcContext_delete(rcContext* ctx);
extern "C" InputGeom* InputGeom_load(rcContext* context, const char* path, bool invertYZ);
extern "C" void InputGeom_delete(InputGeom* geom);
extern "C" rcCompactHeightfield* compact_heightfield_create(rcContext* context, rcConfig* config, InputGeom* geom);
extern "C" void compact_heightfield_delete(rcCompactHeightfield* chf);
extern "C" rcPolyMesh* polymesh_create(rcContext* m_ctx, rcConfig* m_cfg, rcCompactHeightfield* m_chf);
extern "C" void polymesh_delete(rcPolyMesh* polyMesh);
extern "C" rcPolyMeshDetail* polymesh_detail_create(rcContext* m_ctx, rcConfig* m_cfg, rcPolyMesh* m_pmesh, rcCompactHeightfield* m_chf);
extern "C" void polymesh_detail_delete(rcPolyMeshDetail* polyMeshDetail);
extern "C" NavMeshDataResult* navmesh_data_create(rcContext* context, rcConfig* m_cfg, rcPolyMeshDetail* m_dmesh, rcPolyMesh* m_pmesh, InputGeom* m_geom, int tx, int ty, float agentHeight, float agentRadius, float agentMaxClimb);
extern "C" void rcConfig_calc_grid_size(rcConfig* config, InputGeom* geom);
extern "C" dtNavMesh* navmesh_create(rcContext* context, NavMeshDataResult* navmesh_data);
extern "C" void navmesh_delete(dtNavMesh* navmesh);
extern "C" dtNavMeshQuery* navmesh_query_create(dtNavMesh* navmesh);
extern "C" PolyPointResult navmesh_query_find_nearest_poly(dtNavMeshQuery* navQuery, float* point, float* half_extents);
extern "C" FindPathResult navmesh_query_find_path(dtNavMeshQuery* navQuery, dtPolyRef startRef, dtPolyRef endRef, float* startPos, float* endPos, const dtQueryFilter* filter);
extern "C" PolyPointResult navmesh_query_find_random_point(dtNavMeshQuery* navQuery);
extern "C" dtQueryFilter* dtQueryFilter_create();
extern "C" void dtQueryFilter_delete(dtQueryFilter* filter);
extern "C" SmoothPathResult navmesh_query_get_smooth_path(float* startPos, dtPolyRef startRef, float* endPos, FindPathResult* path, const dtQueryFilter* filter, dtNavMesh* navMesh, dtNavMeshQuery* navQuery);
extern "C" bool dtStatus_failed(dtStatus status);
