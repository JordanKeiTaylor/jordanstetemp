namespace Shared.Extensions
{
    public struct AccuratePositionConstants
    {
        public readonly DynamicFlag<double> CoarseGridSize;
        public readonly DynamicFlag<double> FineGridSize;

        public AccuratePositionConstants(DynamicFlag<double> coarse, DynamicFlag<double> fine)
        {
            CoarseGridSize = coarse;
            FineGridSize = fine;
        }
    }
}