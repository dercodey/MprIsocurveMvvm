namespace DataLoaderModule.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UniformImageVolumeModel    
    {
        public int SlicesCompleted = 0;

        /// <summary>
        /// 
        /// </summary>
        public byte[, ,] Voxels
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Depth
        {
            get { return Voxels.GetLength(0); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Height
        {
            get { return Voxels.GetLength(1); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Width
        {
            get { return Voxels.GetLength(2); }
        }
    }
}
