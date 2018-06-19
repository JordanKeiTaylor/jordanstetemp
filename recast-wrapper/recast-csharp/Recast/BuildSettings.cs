using System;

namespace Recast
{
    public static class BuildSettings
    {
        public const float cellSize = 0.3f;
        public const float cellHeight = 0.2f;
        public const float agentHeight = 2.0f;
        public const float agentRadius = 0.6f;
        public const float agentMaxClimb = 0.9f;
        public const float agentMaxSlope = 45.0f;
        public const int regionMinSize = 8;
        public const int regionMergeSize = 20;
        public const float edgeMaxLen = 12.0f;
        public const float edgeMaxError = 1.3f;
        public const float vertsPerPoly = 6.0f;
        public const float detailSampleDist = 6.0f;
        public const float detailSampleMaxError = 1.0f;
        public const float tileSize = 32;

        public static float walkableRadius = (int) Math.Ceiling(agentRadius / cellSize);
        static float borderSize = walkableRadius + 3;

        public static RcConfig createDefault()
        {
            return new RcConfig
            {
                cs = cellSize,
                ch = cellHeight,
                walkableSlopeAngle = agentMaxSlope,
                walkableHeight = (int) Math.Ceiling(agentHeight / cellHeight),
                walkableClimb = (int) Math.Ceiling(agentMaxClimb / cellHeight),
                walkableRadius = (int) walkableRadius,
                maxEdgeLen = (int) (edgeMaxLen / cellSize),
                maxSimplificationError = edgeMaxError,
                minRegionArea = regionMinSize * regionMinSize,
                mergeRegionArea = regionMergeSize * regionMergeSize,
                maxVertsPerPoly = (int) vertsPerPoly,
                detailSampleDist = detailSampleDist < 0.9 ? 0.0f : cellSize * detailSampleDist,
                detailSampleMaxError = (cellHeight * detailSampleMaxError)
            };
        }
    }
}