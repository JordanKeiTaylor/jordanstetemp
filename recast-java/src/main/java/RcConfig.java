import com.sun.jna.Structure;

import java.util.Arrays;
import java.util.List;

public class RcConfig extends Structure {
	public static class ByReference extends RcConfig implements Structure.ByReference {}

	/// The width of the field along the x-axis. [Limit: >= 0] [Units: vx]
	public int width;

	/// The height of the field along the z-axis. [Limit: >= 0] [Units: vx]
	public int height;

	/// The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]
	public int tileSize;

	/// The size of the non-navigable border around the heightfield. [Limit: >=0] [Units: vx]
	public int borderSize;

	/// The xz-plane cell size to use for fields. [Limit: > 0] [Units: wu]
	public float cs;

	/// The y-axis cell size to use for fields. [Limit: > 0] [Units: wu]
	public float ch;

	/// The minimum bounds of the field's AABB. [(x, y, z)] [Units: wu]
	public float[] bmin = new float[3];

	/// The maximum bounds of the field's AABB. [(x, y, z)] [Units: wu]
	public float[] bmax = new float[3];

	/// The maximum slope that is considered walkable. [Limits: 0 <= value < 90] [Units: Degrees]
	public float walkableSlopeAngle;

	/// Minimum floor to 'ceiling' height that will still allow the floor area to
	/// be considered walkable. [Limit: >= 3] [Units: vx]
	public int walkableHeight;

	/// Maximum ledge height that is considered to still be traversable. [Limit: >=0] [Units: vx]
	public int walkableClimb;

	/// The distance to erode/shrink the walkable area of the heightfield away from
	/// obstructions.  [Limit: >=0] [Units: vx]
	public int walkableRadius;

	/// The maximum allowed length for contour edges along the border of the mesh. [Limit: >=0] [Units: vx]
	public int maxEdgeLen;

	/// The maximum distance a simplfied contour's border edges should deviate
	/// the original raw contour. [Limit: >=0] [Units: vx]
	public float maxSimplificationError;

	/// The minimum number of cells allowed to form isolated island areas. [Limit: >=0] [Units: vx]
	public int minRegionArea;

	/// Any regions with a span count smaller than this value will, if possible,
	/// be merged with larger regions. [Limit: >=0] [Units: vx]
	public int mergeRegionArea;

	/// The maximum number of vertices allowed for polygons generated during the
	/// contour to polygon conversion process. [Limit: >= 3]
	public int maxVertsPerPoly;

	/// Sets the sampling distance to use when generating the detail mesh.
	/// (For height detail only.) [Limits: 0 or >= 0.9] [Units: wu]
	public float detailSampleDist;

	/// The maximum distance the detail mesh surface should deviate from heightfield
	/// data. (For height detail only.) [Limit: >=0] [Units: wu]
	public float detailSampleMaxError;

	@Override
	protected List<String> getFieldOrder() {
		return Arrays.asList(
			"width",
			"height",
			"tileSize",
			"borderSize",
			"cs",
			"ch",
			"bmin",
			"bmax",
			"walkableSlopeAngle",
			"walkableHeight",
			"walkableClimb",
			"walkableRadius",
			"maxEdgeLen",
			"maxSimplificationError",
			"minRegionArea",
			"mergeRegionArea",
			"maxVertsPerPoly",
			"detailSampleDist",
			"detailSampleMaxError"
		);
	}
}
