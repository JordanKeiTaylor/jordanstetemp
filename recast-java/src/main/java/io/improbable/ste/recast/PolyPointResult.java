package io.improbable.ste.recast;

import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class PolyPointResult extends Structure {
    public static class ByValue extends PolyPointResult implements Structure.ByValue {}
    // NOTE: These two should match Types.kt
    public int status;
    public int polyRef;
    public float[] point = new float[3];

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList("status", "polyRef", "point");
    }
}
