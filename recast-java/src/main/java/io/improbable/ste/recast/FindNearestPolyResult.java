package io.improbable.ste.recast;

import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class FindNearestPolyResult extends Structure {
    public static class ByValue extends FindNearestPolyResult implements Structure.ByValue {}
    // NOTE: These two should match Types.kt
    public int status;
    public int polyRef;
    public float[] nearestPoint = new float[3];

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList("status", "polyRef", "nearestPoint");
    }
}
