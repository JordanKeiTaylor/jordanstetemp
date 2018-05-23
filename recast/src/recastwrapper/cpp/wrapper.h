#include <stdio.h>
#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshBuilder.h>
#include "MeshLoaderObj.h"
#include "InputGeom.h"

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

struct NavMeshDataResult {
    unsigned char* data;
    int size;
};

extern "C" rcContext* rcContext_create();
extern "C" void rcContext_delete(rcContext* ctx);
extern "C" InputGeom* load_mesh(rcContext* context, const char* path, bool invertYZ);
extern "C" rcCompactHeightfield* compact_heightfield_create(rcContext* context, rcConfig* config, InputGeom* geom);
extern "C" rcPolyMesh* polymesh_create(rcContext* m_ctx, rcConfig* m_cfg, rcCompactHeightfield* m_chf);
extern "C" rcPolyMeshDetail* polymesh_detail_create(rcContext* m_ctx, rcConfig* m_cfg, rcPolyMesh* m_pmesh, rcCompactHeightfield* m_chf);
extern "C" NavMeshDataResult* navmesh_data_create(rcContext* context, rcConfig* m_cfg, rcPolyMeshDetail* m_dmesh, rcPolyMesh* m_pmesh, InputGeom* m_geom, int tx, int ty, float agentHeight, float agentRadius, float agentMaxClimb);
extern "C" void rcConfig_calc_grid_size(rcConfig* config, InputGeom* geom);