package io.improbable.ste.recast

import java.awt.Color
import java.awt.Graphics2D

fun Short.toUInt() = this.toInt().and(0xffff)
fun Char.toUInt() = this.toInt().and(0xffffff)

fun drawPolyMesh(polymesh: RcPolyMesh, cg: Graphics2D, width: Int, height: Int) {
    val nvp = polymesh.nvp
    val cs = polymesh.cs
    val ch = polymesh.ch
    val orig = polymesh.bmin
    val verts = polymesh.verts.getShortArray(0, 3 * polymesh.nverts)
    val polys = polymesh.polys.getShortArray(0, polymesh.maxpolys*2*polymesh.nvp)

    for (i in 0 until polymesh.npolys) {
        val area = polymesh.areas.getChar(i.toLong()).toUInt()

        val color = if (area == 0) {
            Color(0,0,0,64);
        } else {
            Color(0, 192, 255, 64)
        }

        for (j in 2 until nvp) {
            val offset = (i * nvp * 2)
            val s = polys[offset + j].toUInt()
            if (s == 0xffff) break
            val vi: Array<Int> = Array(3, {0 as Int})
            vi[0] = polys[offset + 0].toUInt()
            vi[1] = polys[offset + j - 1].toUInt()
            vi[2] = polys[offset + j].toUInt()
            val xp = mutableListOf<Int>()
            val yp = mutableListOf<Int>()

            for (k in 0 until 3) {
                val vOffset = vi[k] * 3
                val xi = verts[vOffset + 0].toUInt()
                val yi = verts[vOffset + 1].toUInt()
                val zi = verts[vOffset + 2].toUInt()

                val x = orig[0] + xi * cs
                val y = orig[1] + yi * ch
                val z = orig[2] + zi * cs


                val xScale = (x - polymesh.bmin[0]) / (polymesh.bmax[0] - polymesh.bmin[0])
                xp.add((width * xScale).toInt())
                val zScale = (z - polymesh.bmin[2]) / (polymesh.bmax[2] - polymesh.bmin[2])
                yp.add((height * zScale).toInt())
            }

            cg.color = color
            cg.fillPolygon(xp.toIntArray(), yp.toIntArray(), xp.size)
            cg.color = Color(255, 255, 255, 64)
            cg.drawPolygon(xp.toIntArray(), yp.toIntArray(), xp.size)
        }
    }

}
