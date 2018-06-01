package io.improbable.ste.recast;

import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class SmoothPathResult extends Structure {
    public static class ByValue extends SmoothPathResult implements Structure.ByValue {}
    public static class ByReference extends SmoothPathResult implements Structure.ByReference {}
    public float path[] = new float[3 * Const.MAX_SMOOTH_PATH_LEN];
    public int pathCount;

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList("path", "pathCount");
    }
}
