package io.improbable.ste.recast

fun dtFailed(dtStatus: DtStatus) = dtStatus.and(1.shl(31)) != 0
fun dtSuccess(dtStatus: DtStatus) = dtStatus.and(1.shl(30)) != 0

fun dtStatusDetailString(status: DtStatus): String {
    val DT_WRONG_MAGIC: DtStatus = 1.shl(0)
    val DT_WRONG_VERSION: DtStatus = 1.shl(1)
    val DT_OUT_OF_MEMORY: DtStatus = 1.shl(2)
    val DT_INVALID_PARAM: DtStatus = 1.shl(3)
    val DT_BUFFER_TOO_SMALL: DtStatus = 1.shl(4)
    val DT_OUT_OF_NODES: DtStatus = 1.shl(5)
    val DT_PARTIAL_RESULT: DtStatus = 1.shl(6)
    val DT_ALREADY_OCCUPIED: DtStatus = 1.shl(7)

	if (0 != status.and(DT_WRONG_MAGIC)) return "DT_WRONG_MAGIC"
	if (0 != status.and(DT_WRONG_VERSION)) return "DT_WRONG_VERSION"
	if (0 != status.and(DT_OUT_OF_MEMORY)) return "DT_OUT_OF_MEMORY"
	if (0 != status.and(DT_INVALID_PARAM)) return "DT_INVALID_PARAM"
	if (0 != status.and(DT_BUFFER_TOO_SMALL)) return "DT_BUFFER_TOO_SMALL"
	if (0 != status.and(DT_OUT_OF_NODES)) return "DT_OUT_OF_NODES"
	if (0 != status.and(DT_PARTIAL_RESULT)) return "DT_PARTIAL_RESULT"
	if (0 != status.and(DT_ALREADY_OCCUPIED)) return "DT_ALREADY_OCCUPIED"
	return "Unknown (" + status.toString(2) + " | " + status.toString() + ")"
}
