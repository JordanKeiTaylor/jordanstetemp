import com.sun.jna.Memory
import com.sun.jna.Pointer

import io.improbable.ste.recast.*

class Common {
    companion object {
        public fun toFloat3 (point: PolyPointResult): Pointer {
            val float3 = Memory((3 * 4).toLong())
            float3.setFloat(0, point.point[0])
            float3.setFloat(4, point.point[1])
            float3.setFloat(8, point.point[2])
            return float3
        }
    }
}
