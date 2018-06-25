using Infrastructure.Interfaces;

namespace DataLoaderModule.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UniformImageVolumeModel : IUniformImageVolumeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public byte[, ,] Voxels { get; set;}

        /// <summary>
        /// 
        /// </summary>
        public int Depth
        {
            get => Voxels.GetLength(0);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Height
        {
            get => Voxels.GetLength(1);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Width
        {
            get => Voxels.GetLength(2);
        }

        /// <summary>
        /// 
        /// </summary>
        public int SlicesCompleted { get; set; }
    }
}
