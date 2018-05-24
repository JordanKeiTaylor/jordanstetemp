package io.improbable.ste.recast;

import com.sun.jna.Pointer;
import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class FindPathResult extends Structure {
    public static class ByValue extends FindPathResult implements Structure.ByValue {}
    public int status;
    public Pointer path;
    public int pathCount;

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList("status", "path", "pathCount");
    }
}
