import com.sun.jna.Pointer;
import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class NavMeshDataResult extends Structure {
	public static class ByReference extends NavMeshDataResult implements Structure.ByReference {}

	public Pointer data;
	public int size;

	@Override
	protected List<String> getFieldOrder() {
		return Arrays.asList("data", "size");
	}
}
