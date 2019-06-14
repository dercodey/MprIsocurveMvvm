using System;
using System.Threading.Tasks;
using FsRenderModule.Interfaces;
using Infrastructure.Interfaces;

using RenderModule.Interfaces;

namespace RenderModule.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MprImageModel : MprImageModelBase
    {
        IMprGenerationFunction _mprFunction;

        public MprImageModel(IMprGenerationFunction mprFunction)
            : base(mprFunction)
        {
            _mprFunction = mprFunction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpr"></param>
        /// <param name="orientation"></param>
        /// <param name="nSliceNumber"></param>
        /// <returns></returns>
        public static bool CheckAndUpdate(ref MprImageModel mpr, 
            Orientation orientation, int nSliceNumber)
        {
            bool bUpdated = false;
            if (mpr.MprOrientation != orientation
                        || mpr.SlicePosition != nSliceNumber
                        || mpr._pixelTask == null
                        || mpr._volumeSlicesCompleted < mpr.InputVolume.SlicesCompleted)
            {
                switch (orientation)
                {
                    case Orientation.Coronal:
                        mpr.MprOrientation = Orientation.Coronal;
                        break;
                    case Orientation.Sagittal:
                        mpr.MprOrientation = Orientation.Sagittal;
                        break;
                    case Orientation.Transverse:
                        mpr.MprOrientation = Orientation.Transverse;
                        break;
                }
                mpr.SlicePosition = nSliceNumber;
                bUpdated = true;
            }

            return bUpdated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<byte[,]> GetPixelsAsync()
        {
            if (_pixelTask == null
                || MprOrientation.CompareTo(_taskMprOrientation) != 0
                || SlicePosition != _taskSlicePosition
                || _volumeSlicesCompleted != InputVolume.SlicesCompleted)
            { 
                _taskMprOrientation = MprOrientation;
                _taskSlicePosition = SlicePosition;
                _pixelTask = this._mprFunction.GenerateMprAsync(InputVolume, this);
                _volumeSlicesCompleted = InputVolume.SlicesCompleted;
            }
            return await _pixelTask;
        }

        Task<byte[,]> _pixelTask;

        // these hold the current values being used for the current pixel task
        Orientation _taskMprOrientation;
        int _taskSlicePosition;
        int _volumeSlicesCompleted = 0;
    }
}
