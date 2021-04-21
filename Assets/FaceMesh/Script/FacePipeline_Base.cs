using MediaPipe.BlazeFace;
using MediaPipe.FaceLandmark;
using MediaPipe.Iris;
using UnityEngine;

namespace MediaPipe.FaceMesh {

//
// Basic implementation of the face pipeline class
//

sealed partial class FacePipeline : System.IDisposable
{
    #region Private objects

    ResourceSet _resources;

    FaceDetector _faceDetector;

    (FaceLandmarkDetector face,
     EyeLandmarkDetector eyeL,
     EyeLandmarkDetector eyeR) _landmarkDetector;

    Material _preprocess;

    (RenderTexture face,
     RenderTexture eyeL,
     RenderTexture eyeR) _cropRT;

    (ComputeBuffer post,
     ComputeBuffer filter,
     ComputeBuffer bbox,
     ComputeBuffer eyeToFace) _computeBuffer;

    #endregion

    #region Object allocation/deallocation

    void AllocateObjects(ResourceSet resources)
    {
        _resources = resources;

        _faceDetector = new FaceDetector(_resources.blazeFace);

        _landmarkDetector =
          (new FaceLandmarkDetector(_resources.faceLandmark),
           new EyeLandmarkDetector(_resources.iris),
           new EyeLandmarkDetector(_resources.iris));

        _preprocess = new Material(_resources.preprocessShader);

        _cropRT = (new RenderTexture(192, 192, 0),
                   new RenderTexture(64, 64, 0),
                   new RenderTexture(64, 64, 0));

        var faceVCount = FaceLandmarkDetector.VertexCount;
        _computeBuffer =
          (new ComputeBuffer(faceVCount, sizeof(float) * 4),
           new ComputeBuffer(faceVCount * 2, sizeof(float) * 4),
           new ComputeBuffer(1, sizeof(float) * 4),
           IndexTable.CreateEyeToFaceLandmarkBuffer());
    }

    void DeallocateObjects()
    {
        _faceDetector.Dispose();

        _landmarkDetector.face.Dispose();
        _landmarkDetector.eyeL.Dispose();
        _landmarkDetector.eyeR.Dispose();

        Object.Destroy(_preprocess);

        Object.Destroy(_cropRT.face);
        Object.Destroy(_cropRT.eyeL);
        Object.Destroy(_cropRT.eyeR);

        _computeBuffer.post.Dispose();
        _computeBuffer.filter.Dispose();
        _computeBuffer.bbox.Dispose();
        _computeBuffer.eyeToFace.Dispose();
    }

    #endregion
}

} // namespace MediaPipe.FaceMesh
