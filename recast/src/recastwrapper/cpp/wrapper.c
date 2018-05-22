#include "wrapper.h"
#include "MeshLoaderObj.h"
#include "ChunkyTriMesh.h"
#include "InputGeom.h"

rcContext* rcContext_create() {
    return new IoRcContext();
}

void rcContext_delete(rcContext* ctx) {
    delete ctx;
}

InputGeom* load_mesh(rcContext* context, const char* path) {
    InputGeom *geom = new InputGeom();

    if (!geom->load(context, std::string(path))) {
        return 0;
    }

    return geom;
}

rcCompactHeightfield* compact_heightfield_create(rcContext* context, rcConfig* config, InputGeom* geom) {
    for (int i = 0; i < 3; i++) {
        config->bmin[i] = geom->getMeshBoundsMin()[i];
        config->bmax[i] = geom->getMeshBoundsMax()[i];
        printf("bmin[%d] = %.2f ", i, config->bmin[i]);
        printf("bmax[%d] = %.2f\n", i, config->bmax[i]);
    }
    rcHeightfield* heightfield = rcAllocHeightfield();

	if (!heightfield)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return 0;
	}

	if (!rcCreateHeightfield(context, *heightfield, config->width, config->height, config->bmin, config->bmax, config->cs, config->ch))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return 0;
	}

    const float* verts = geom->getMesh()->getVerts();
	const int nverts = geom->getMesh()->getVertCount();
	const int ntris = geom->getMesh()->getTriCount();
	const rcChunkyTriMesh* chunkyMesh = geom->getChunkyMesh();
    context->log(RC_LOG_PROGRESS, "buildNavigation: verts=%d, tris=%d", nverts, ntris);

    // Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	unsigned char* triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!triareas)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return 0;
	}
	
	float tbmin[2], tbmax[2];
	tbmin[0] = config->bmin[0];
	tbmin[1] = config->bmin[2];
	tbmax[0] = config->bmax[0];
	tbmax[1] = config->bmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	const int ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid) {
		context->log(RC_LOG_ERROR, "No chunks.", chunkyMesh->maxTrisPerChunk);
		return 0;
    }
	
	int m_tileTriCount = 0;
	
	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* ctris = &chunkyMesh->tris[node.i*3];
		const int nctris = node.n;
		
		m_tileTriCount += nctris;
		
		memset(triareas, 0, nctris*sizeof(unsigned char));
		rcMarkWalkableTriangles(context, config->walkableSlopeAngle,
								verts, nverts, ctris, nctris, triareas);
		
		if (!rcRasterizeTriangles(context, verts, nverts, ctris, triareas, nctris, *heightfield, config->walkableClimb))
			return 0;
	}

    context->log(RC_LOG_PROGRESS, "Triangles overlapping: %d", m_tileTriCount);
    context->log(RC_LOG_PROGRESS, "Span : %d -> %d", heightfield->spans[0]->smin, heightfield->spans[0]->smax);
    context->log(RC_LOG_PROGRESS, "Heightfield : %d x %d", heightfield->width, heightfield->height);

    delete [] triareas;
    triareas = 0;

    bool m_filterLowHangingObstacles = false;
    bool m_filterLedgeSpans = false;
    bool m_filterWalkableLowHeightSpans = false;

    if (m_filterLowHangingObstacles)
		rcFilterLowHangingWalkableObstacles(context, config->walkableClimb, *heightfield);
	if (m_filterLedgeSpans)
		rcFilterLedgeSpans(context, config->walkableHeight, config->walkableClimb, *heightfield);
	if (m_filterWalkableLowHeightSpans)
		rcFilterWalkableLowHeightSpans(context, config->walkableHeight, *heightfield);

    rcCompactHeightfield* m_chf = rcAllocCompactHeightfield();
	if (!m_chf)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return 0;
	}
	if (!rcBuildCompactHeightfield(context, config->walkableHeight, config->walkableClimb, *heightfield, *m_chf))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return 0;
	}
    context->log(RC_LOG_PROGRESS, "CHF Spans: %d", m_chf->spanCount);
	
    rcFreeHeightField(heightfield);
    heightfield = 0;

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(context, config->walkableRadius, *m_chf))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return 0;
	}

	// (Optional) Mark areas.
	const ConvexVolume* vols = geom->getConvexVolumes();
	for (int i  = 0; i < geom->getConvexVolumeCount(); ++i)
		rcMarkConvexPolyArea(context, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);

    	// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
	// There are 3 martitioning methods, each with some pros and cons:
	// 1) Watershed partitioning
	//   - the classic Recast partitioning
	//   - creates the nicest tessellation
	//   - usually slowest
	//   - partitions the heightfield into nice regions without holes or overlaps
	//   - the are some corner cases where this method creates produces holes and overlaps
	//      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
	//      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
	//   * generally the best choice if you precompute the nacmesh, use this if you have large open areas
	// 2) Monotone partioning
	//   - fastest
	//   - partitions the heightfield into regions without holes and overlaps (guaranteed)
	//   - creates long thin polygons, which sometimes causes paths with detours
	//   * use this if you want fast navmesh generation
	// 3) Layer partitoining
	//   - quite fast
	//   - partitions the heighfield into non-overlapping regions
	//   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
	//   - produces better triangles than monotone partitioning
	//   - does not have the corner cases of watershed partitioning
	//   - can be slow and create a bit ugly tessellation (still better than monotone)
	//     if you have large open areas with small obstacles (not a problem if you use tiles)
	//   * good choice to use for tiled navmesh with medium and small sized tiles

    int partitionType = SAMPLE_PARTITION_WATERSHED;
	
	if (partitionType == SAMPLE_PARTITION_WATERSHED)
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(context, *m_chf))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			return 0;
		}
		
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(context, *m_chf, config->borderSize, config->minRegionArea, config->mergeRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build watershed regions.");
			return 0;
		}
	}
	else if (partitionType == SAMPLE_PARTITION_MONOTONE)
	{
		// Partition the walkable surface into simple regions without holes.
		// Monotone partitioning does not need distancefield.
		if (!rcBuildRegionsMonotone(context, *m_chf, config->borderSize, config->minRegionArea, config->mergeRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build monotone regions.");
			return 0;
		}
	}
	else // SAMPLE_PARTITION_LAYERS
	{
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildLayerRegions(context, *m_chf, config->borderSize, config->minRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build layer regions.");
			return 0;
		}
	}
	
    return m_chf;
}

rcPolyMesh* polymesh_create(rcContext* m_ctx, rcConfig* m_cfg, rcCompactHeightfield* m_chf) {
    // Create contours.
	rcContourSet* m_cset = rcAllocContourSet();

	if (!m_cset)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		return 0;
	}
	if (!rcBuildContours(m_ctx, *m_chf, m_cfg->maxSimplificationError, m_cfg->maxEdgeLen, *m_cset))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		return 0;
	}
	
	if (m_cset->nconts == 0)
	{
		m_ctx->log(RC_LOG_ERROR, "0 contours");
		return 0;
	}
	
	// Build polygon navmesh from the contours.
	rcPolyMesh* m_pmesh = rcAllocPolyMesh();
	if (!m_pmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		return 0;
	}
	if (!rcBuildPolyMesh(m_ctx, *m_cset, m_cfg->maxVertsPerPoly, *m_pmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		return 0;
	}

    return m_pmesh;
}

rcPolyMeshDetail* polymesh_detail_create(rcContext* m_ctx, rcConfig* m_cfg, rcPolyMesh* m_pmesh, rcCompactHeightfield* m_chf) {
	// Build detail mesh.
	rcPolyMeshDetail* m_dmesh = rcAllocPolyMeshDetail();

	if (!m_dmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'dmesh'.");
		return 0;
	}

    if (!m_pmesh)
    {
		m_ctx->log(RC_LOG_ERROR, "m_pmesh is null");
        return 0;
    }
	
	if (!rcBuildPolyMeshDetail(m_ctx, *m_pmesh, *m_chf,
							   m_cfg->detailSampleDist, m_cfg->detailSampleMaxError,
							   *m_dmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could build polymesh detail.");
		return 0;
	}

    return m_dmesh;
}

NavMeshDataResult* navmesh_data_create(rcContext* context, rcConfig* m_cfg, rcPolyMeshDetail* m_dmesh, rcPolyMesh* m_pmesh, InputGeom* m_geom, int tx, int ty, float agentHeight, float agentRadius, float agentMaxClimb) {
    unsigned char* navData = 0;
	int navDataSize = 0;
	if (m_cfg->maxVertsPerPoly <= DT_VERTS_PER_POLYGON)
	{
		if (m_pmesh->nverts >= 0xffff)
		{
			// The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
			context->log(RC_LOG_ERROR, "Too many vertices per tile %d (max: %d).", m_pmesh->nverts, 0xffff);
			return 0;
		}
		
		// Update poly flags from areas.
		for (int i = 0; i < m_pmesh->npolys; ++i)
		{
			if (m_pmesh->areas[i] == RC_WALKABLE_AREA)
				m_pmesh->areas[i] = SAMPLE_POLYAREA_GROUND;
			
			if (m_pmesh->areas[i] == SAMPLE_POLYAREA_GROUND ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_GRASS ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_ROAD)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_WATER)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_SWIM;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_DOOR)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
			}
		}
		
		dtNavMeshCreateParams params;
		memset(&params, 0, sizeof(params));
		params.verts = m_pmesh->verts;
		params.vertCount = m_pmesh->nverts;
		params.polys = m_pmesh->polys;
		params.polyAreas = m_pmesh->areas;
		params.polyFlags = m_pmesh->flags;
		params.polyCount = m_pmesh->npolys;
		params.nvp = m_pmesh->nvp;
		params.detailMeshes = m_dmesh->meshes;
		params.detailVerts = m_dmesh->verts;
		params.detailVertsCount = m_dmesh->nverts;
		params.detailTris = m_dmesh->tris;
		params.detailTriCount = m_dmesh->ntris;
		params.offMeshConVerts = m_geom->getOffMeshConnectionVerts();
		params.offMeshConRad = m_geom->getOffMeshConnectionRads();
		params.offMeshConDir = m_geom->getOffMeshConnectionDirs();
		params.offMeshConAreas = m_geom->getOffMeshConnectionAreas();
		params.offMeshConFlags = m_geom->getOffMeshConnectionFlags();
		params.offMeshConUserID = m_geom->getOffMeshConnectionId();
		params.offMeshConCount = m_geom->getOffMeshConnectionCount();
		params.walkableHeight = agentHeight;
		params.walkableRadius = agentRadius;
		params.walkableClimb = agentMaxClimb;
		params.tileX = tx;
		params.tileY = ty;
		params.tileLayer = 0;
		rcVcopy(params.bmin, m_pmesh->bmin);
		rcVcopy(params.bmax, m_pmesh->bmax);
		params.cs = m_cfg->cs;
		params.ch = m_cfg->ch;
		params.buildBvTree = true;
		
		if (!dtCreateNavMeshData(&params, &navData, &navDataSize))
		{
			context->log(RC_LOG_ERROR, "Could not build Detour navmesh.");
			return 0;
		}		
	}

    NavMeshDataResult* result = new NavMeshDataResult();

    result->data = navData;
    result->size = navDataSize;

    return result;
}