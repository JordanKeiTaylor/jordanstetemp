package io.improbable.ste.recast;

import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class FindPathResult extends Structure {
    public static class ByValue extends FindPathResult implements Structure.ByValue {}
    public int status;
    public int path[] = new int[Const.MAX_PATH_LEN];
    public int pathCount;

    @Override
    protected List<String> getFieldOrder() {
        return Arrays.asList("status", "path", "pathCount");
    }
}
