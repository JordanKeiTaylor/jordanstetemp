#include "wrapper.h"
#include "ChunkyTriMesh.h"

rcContext* rcContext_create() {
    return new IoRcContext();
}

void rcContext_delete(rcContext* ctx) {
    delete ctx;
}

InputGeom* InputGeom_load(rcContext* context, const char* path, bool invertYZ) {
    InputGeom *geom = new InputGeom();

    if (!geom->load(context, std::string(path), invertYZ)) {
        return 0;
    }

    return geom;
}

void InputGeom_delete(InputGeom* geom) {
	delete geom;
}

void rcConfig_calc_grid_size(rcConfig* config, InputGeom* geom) {
    for (int i = 0; i < 3; i++) {
        config->bmin[i] = geom->getMeshBoundsMin()[i];
        config->bmax[i] = geom->getMeshBoundsMax()[i];
    }

    rcCalcGridSize(config->bmin, config->bmax, config->cs, &config->width, &config->height);
}

rcCompactHeightfield* compact_heightfield_create(rcContext* context, rcConfig* config, InputGeom* geom) {
    rcHeightfield* heightfield = 0;
	unsigned char* triareas = 0;
	rcCompactHeightfield* m_chf = 0;
	const float* verts = 0;
	int nverts;
	int ntris;
	const rcChunkyTriMesh* chunkyMesh = 0;
	float tbmin[2], tbmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	int ncid;

	int m_tileTriCount = 0;
	const bool m_filterLowHangingObstacles = false;
	const bool m_filterLedgeSpans = false;
	const bool m_filterWalkableLowHeightSpans = false;

	const ConvexVolume* vols;

    int partitionType = SAMPLE_PARTITION_WATERSHED;

	if (!geom) {
		context->log(RC_LOG_ERROR, "buildNavigation: InputGeometry is null.");
		goto handle_error;
	}

	heightfield = rcAllocHeightfield();
	if (!heightfield)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		goto handle_error;
	}

	if (!rcCreateHeightfield(context, *heightfield, config->width, config->height, config->bmin, config->bmax, config->cs, config->ch))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		goto handle_error;
	}

    verts = geom->getMesh()->getVerts();
	nverts = geom->getMesh()->getVertCount();
	ntris = geom->getMesh()->getTriCount();
	chunkyMesh = geom->getChunkyMesh();

    // Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!triareas)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		goto handle_error;
	}
	
	tbmin[0] = config->bmin[0];
	tbmin[1] = config->bmin[2];
	tbmax[0] = config->bmax[0];
	tbmax[1] = config->bmax[2];
	ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid) {
		context->log(RC_LOG_ERROR, "No chunks.", chunkyMesh->maxTrisPerChunk);
		goto handle_error;
    }
	
	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* ctris = &chunkyMesh->tris[node.i*3];
		const int nctris = node.n;
		
		m_tileTriCount += nctris;
		
		memset(triareas, 0, nctris*sizeof(unsigned char));
		rcMarkWalkableTriangles(context, config->walkableSlopeAngle,
								verts, nverts, ctris, nctris, triareas);
		
		if (!rcRasterizeTriangles(context, verts, nverts, ctris, triareas, nctris, *heightfield, config->walkableClimb)) {
			goto handle_error;
		}
	}

    delete [] triareas;
    triareas = 0;

    if (m_filterLowHangingObstacles) {
		rcFilterLowHangingWalkableObstacles(context, config->walkableClimb, *heightfield);
	}
	if (m_filterLedgeSpans) {
		rcFilterLedgeSpans(context, config->walkableHeight, config->walkableClimb, *heightfield);
	}
	if (m_filterWalkableLowHeightSpans) {
		rcFilterWalkableLowHeightSpans(context, config->walkableHeight, *heightfield);
	}

    m_chf = rcAllocCompactHeightfield();
	if (!m_chf)
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		goto handle_error;
	}
	if (!rcBuildCompactHeightfield(context, config->walkableHeight, config->walkableClimb, *heightfield, *m_chf))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		goto handle_error;
	}
	
    rcFreeHeightField(heightfield);
    heightfield = 0;

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(context, config->walkableRadius, *m_chf))
	{
		context->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		goto handle_error;
	}

	// (Optional) Mark areas.
	vols = geom->getConvexVolumes();
	for (int i  = 0; i < geom->getConvexVolumeCount(); ++i)
		rcMarkConvexPolyArea(context, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);
	
	if (partitionType == SAMPLE_PARTITION_WATERSHED)
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(context, *m_chf))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			goto handle_error;
		}
		
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(context, *m_chf, config->borderSize, config->minRegionArea, config->mergeRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build watershed regions.");
			goto handle_error;
		}
	}
	else if (partitionType == SAMPLE_PARTITION_MONOTONE)
	{
		// Partition the walkable surface into simple regions without holes.
		// Monotone partitioning does not need distancefield.
		if (!rcBuildRegionsMonotone(context, *m_chf, config->borderSize, config->minRegionArea, config->mergeRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build monotone regions.");
			goto handle_error;
		}
	}
	else // SAMPLE_PARTITION_LAYERS
	{
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildLayerRegions(context, *m_chf, config->borderSize, config->minRegionArea))
		{
			context->log(RC_LOG_ERROR, "buildNavigation: Could not build layer regions.");
			goto handle_error;
		}
	}

	return m_chf;

handle_error:
	if (heightfield) {
		rcFreeHeightField(heightfield);
		heightfield = 0;
	}

	if (triareas) {
		delete triareas;
		triareas = 0;
	}

	if (m_chf) {
		rcFreeCompactHeightfield(m_chf);
		m_chf = 0;
	}
	
    return m_chf;
}


void compact_heightfield_delete(rcCompactHeightfield* chf) {
	rcFreeCompactHeightfield(chf);
}

rcPolyMesh* polymesh_create(rcContext* m_ctx, rcConfig* m_cfg, rcCompactHeightfield* m_chf) {
	rcContourSet* m_cset = 0;
	rcPolyMesh* m_pmesh = 0;

    // Create contours.
	m_cset = rcAllocContourSet();
	if (!m_cset)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		goto handle_error;
	}

	if (!rcBuildContours(m_ctx, *m_chf, m_cfg->maxSimplificationError, m_cfg->maxEdgeLen, *m_cset))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		goto handle_error;
	}
	
	if (m_cset->nconts == 0)
	{
		m_ctx->log(RC_LOG_ERROR, "0 contours");
		goto handle_error;
	}
	
	// Build polygon navmesh from the contours.
	m_pmesh = rcAllocPolyMesh();
	if (!m_pmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		goto handle_error;
	}
	if (!rcBuildPolyMesh(m_ctx, *m_cset, m_cfg->maxVertsPerPoly, *m_pmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		goto handle_error;
	}

	return m_pmesh;

handle_error:
	if (m_cset) {
		rcFreeContourSet(m_cset);
		m_cset = 0;
	}

	if (m_pmesh) {
		rcFreePolyMesh(m_pmesh);
		m_pmesh = 0;
	}

    return m_pmesh;
}

void polymesh_delete(rcPolyMesh* polyMesh) {
	rcFreePolyMesh(polyMesh);
}

rcPolyMeshDetail* polymesh_detail_create(rcContext* m_ctx, rcConfig* m_cfg, rcPolyMesh* m_pmesh, rcCompactHeightfield* m_chf) {
	// Build detail mesh.
	rcPolyMeshDetail* m_dmesh = rcAllocPolyMeshDetail();

	if (!m_dmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'dmesh'.");
		goto handle_error;
	}

    if (!m_pmesh)
    {
		m_ctx->log(RC_LOG_ERROR, "m_pmesh is null");
		goto handle_error;
    }
	
	if (!rcBuildPolyMeshDetail(m_ctx, *m_pmesh, *m_chf,
							   m_cfg->detailSampleDist, m_cfg->detailSampleMaxError,
							   *m_dmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could build polymesh detail.");
		goto handle_error;
	}

    return m_dmesh;

handle_error:
	if (m_dmesh) {
		rcFreePolyMeshDetail(m_dmesh);
		m_dmesh = 0;
	}

	return m_dmesh;
}

void polymesh_detail_delete(rcPolyMeshDetail* polyMeshDetail) {
	rcFreePolyMeshDetail(polyMeshDetail);
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

dtNavMesh* navmesh_create(rcContext* context, NavMeshDataResult* navmesh_data) {
    dtNavMesh* navmesh = dtAllocNavMesh();

	if (!navmesh)
	{
		context->log(RC_LOG_ERROR, "Could not create Detour navmesh");
		return 0;
	}

	dtStatus status;
	status = navmesh->init(navmesh_data->data, navmesh_data->size, DT_TILE_FREE_DATA);
	if (dtStatusFailed(status))
	{
		dtFreeNavMesh(navmesh);
		context->log(RC_LOG_ERROR, "Could not init Detour navmesh");
		return 0;
	}

	return navmesh;
}

void navmesh_delete(dtNavMesh* navmesh) {
	dtFreeNavMesh(navmesh);
}

dtNavMeshQuery* navmesh_query_create(dtNavMesh* navmesh) {
	dtNavMeshQuery* navQuery = dtAllocNavMeshQuery();

	if (!navQuery) {
		return 0;
	}

	dtStatus status = navQuery->init(navmesh, 2048);

	if (dtStatusFailed(status)) {
		dtFreeNavMeshQuery(navQuery);
		navQuery = 0;
	}

	return navQuery;
}

PolyPointResult navmesh_query_find_nearest_poly(dtNavMeshQuery* navQuery, float* point, float* half_extents) {
	dtPolyRef polyRef;
	float nearestPoint[3];
	dtQueryFilter filter;
	dtStatus status = navQuery->findNearestPoly(point, half_extents, &filter, &polyRef, nearestPoint);

	PolyPointResult result;
	result.status = status;
	result.point[0] = nearestPoint[0];
	result.point[1] = nearestPoint[1];
	result.point[2] = nearestPoint[2];
	result.polyRef = polyRef;

	return result;
}

FindPathResult* navmesh_query_find_path(dtNavMeshQuery* navQuery, dtPolyRef startRef, dtPolyRef endRef, float* startPos, float* endPos, const dtQueryFilter* filter) {
   	FindPathResult* result = new FindPathResult();
	result->status = navQuery->findPath(startRef, endRef, startPos, endPos, filter, result->path, &result->pathCount, MAX_PATH_LEN);
	return result;
}

static float frand()
{
	return (float)rand()/(float)RAND_MAX;
}

PolyPointResult navmesh_query_find_random_point(dtNavMeshQuery* navQuery) {
    dtQueryFilter filter;
    PolyPointResult result;

	if (!navQuery) {
		result.status = DT_FAILURE;
	}
	else {
		result.status = navQuery->findRandomPoint(&filter, frand, &result.polyRef, result.point);
	}

    return result;
}

dtQueryFilter* dtQueryFilter_create() {
	return new dtQueryFilter();
}

void dtQueryFilter_delete(dtQueryFilter* filter) {
	delete filter;
}

SmoothPathResult navmesh_query_get_smooth_path(float* startPos, dtPolyRef startRef, float* endPos, FindPathResult* path, const dtQueryFilter* filter, dtNavMesh* navMesh, dtNavMeshQuery* navQuery) {
	return getSmoothPath(startPos, startRef, endPos, path, filter, navMesh, navQuery);
}

bool dtStatus_failed(dtStatus status) {
	return dtStatusFailed(status);
}
