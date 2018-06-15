package io.improbable.ste.recast

object Constants {
    const val cellSize = 0.3
    const val cellHeight = 0.2
    const val agentHeight = 2.0
    const val agentRadius = 0.6
    const val agentMaxClimb = 0.9
    const val agentMaxSlope = 45.0
    const val regionMinSize = 8
    const val regionMergeSize = 20
    const val edgeMaxLen = 12.0
    const val edgeMaxError = 1.3
    const val vertsPerPoly = 6.0
    const val detailSampleDist = 6.0
    const val detailSampleMaxError = 1.0
    const val tileSize = 32

    val walkableRadius = Math.ceil((agentRadius / cellSize)).toInt()
    val borderSize = walkableRadius + 3
}

fun createDefaultConfig() = RcConfig.ByReference().apply {
    //        width = Constants.tileSize + 2 * Constants.borderSize
//        height = Constants.tileSize + 2 * Constants.borderSize
//        tileSize = Constants.tileSize
//        borderSize = Constants.borderSize
    cs = Constants.cellSize.toFloat()
    ch = Constants.cellHeight.toFloat()
    walkableSlopeAngle = Constants.agentMaxSlope.toFloat()
    walkableHeight = Math.ceil(Constants.agentHeight / Constants.cellHeight).toInt()
    walkableClimb = Math.ceil(Constants.agentMaxClimb / Constants.cellHeight).toInt()
    walkableRadius = Constants.walkableRadius
    maxEdgeLen = (Constants.edgeMaxLen / Constants.cellSize).toInt()
    maxSimplificationError = Constants.edgeMaxError.toFloat()
    minRegionArea = Constants.regionMinSize * Constants.regionMinSize
    mergeRegionArea = Constants.regionMergeSize * Constants.regionMergeSize
    maxVertsPerPoly = Constants.vertsPerPoly.toInt()
    detailSampleDist = if (Constants.detailSampleDist < 0.9) {
        0.0f
    } else {
        (Constants.cellSize * Constants.detailSampleDist).toFloat()
    }
    detailSampleMaxError = (Constants.cellHeight * Constants.detailSampleMaxError).toFloat()
}