namespace Improbable.Enterprise.Sandbox.Interpolations
{
    /// <summary>
    /// Interface for interpolating.
    /// </summary>
    public interface IInterpolate
    {
        /// <summary>
        /// Gets the full length of interpolation.
        /// </summary>
        /// <value>The length.</value>
        double Length { get; }

        /// <summary>
        /// Gets length at specified step.
        /// </summary>
        /// <returns>The length.</returns>
        /// <param name="step">Step from 0 to 1.</param>
        double LengthAt(double step);

        /// <summary>
        /// Gets length at specified vector.
        /// </summary>
        /// <returns>The length.</returns>
        /// <param name="vector">Vector along interpolation.</param>
        double LengthAt(Vector3d vector);

        /// <summary>
        /// Gets positions at step.
        /// </summary>
        /// <returns>Vector along interpolation.</returns>
        /// <param name="step">Step from 0 to 1.</param>
        Vector3d PositionAt(double step);

        /// <summary>
        /// Gets step at specified distance.
        /// </summary>
        /// <returns>Step value from 0 to 1.</returns>
        /// <param name="distance">Distance.</param>
        double StepAt(double distance);

        /// <summary>
        /// Gets step at specified distance.
        /// </summary>
        /// <returns>Step value from 0 to 1.</returns>
        /// <param name="vector">Vector along interpolation.</param>
        double StepAt(Vector3d vector);
    }
}