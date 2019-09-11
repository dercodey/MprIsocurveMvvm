namespace Infrastructure.Interfaces
{
    public interface IUniformImageVolumeModel
    {
        int Height { get; }
        int Width { get; }
        int Depth { get; }

        byte[,,] Voxels { get; set; }

        int SlicesCompleted { get; set; }
    }
}