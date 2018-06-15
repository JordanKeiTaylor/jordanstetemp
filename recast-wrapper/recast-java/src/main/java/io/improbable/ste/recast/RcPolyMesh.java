package io.improbable.ste.recast;

import com.sun.jna.Pointer;
import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class RcPolyMesh extends Structure {
    public static class ByReference extends RcPolyMesh implements Structure.ByReference {}
    public Pointer verts;
    public Pointer polys;
    public Pointer regs;
    public Pointer flags;
    public Pointer areas;
    public int nverts;
    public int npolys;
    public int maxpolys;
    public int nvp;
    public float[] bmin = new float[3];
    public float[] bmax = new float[3];
    public float cs;
    public float ch;
    public int borderSize;
    public float maxEdgeError;

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList(
                "verts",
                "polys",
                "regs",
                "flags",
                "areas",
                "nverts",
                "npolys",
                "maxpolys",
                "nvp",
                "bmin",
                "bmax",
                "cs",
                "ch",
                "borderSize",
                "maxEdgeError"
        );
    }
}
