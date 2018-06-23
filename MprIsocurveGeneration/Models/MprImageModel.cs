using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using MprIsocurveGeneration.Services;

namespace MprIsocurveGeneration.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MprImageModel
    {
        IMprGenerationFunction _mprFunction;

        public MprImageModel(IMprGenerationFunction mprFunction)
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
                        mpr.MprOrientation = MprImageModel.Orientation.Coronal;
                        break;
                    case Orientation.Sagittal:
                        mpr.MprOrientation = MprImageModel.Orientation.Sagittal;
                        break;
                    case Orientation.Transverse:
                        mpr.MprOrientation = MprImageModel.Orientation.Transverse;
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
        public Guid InputVolumeGuid
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal UniformImageVolumeModel InputVolume
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Orientation
        {
            Transverse,
            Sagittal,
            Coronal
        };

        /// <summary>
        /// 
        /// </summary>
        public Orientation MprOrientation
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int SlicePosition
        {
            get;
            set;
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
                _pixelTask = _mprFunction.GenerateMprAsync(InputVolume, this);
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
