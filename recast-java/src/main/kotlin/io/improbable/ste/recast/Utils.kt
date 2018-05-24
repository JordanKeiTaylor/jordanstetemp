package io.improbable.ste.recast

fun dtFailed(dtStatus: DtStatus) = dtStatus.and(1.shl(31)) != 0
