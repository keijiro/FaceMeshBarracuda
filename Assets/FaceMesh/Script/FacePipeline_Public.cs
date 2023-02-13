using UnityEngine;
using Unity.Mathematics;

namespace MediaPipe.FaceMesh {

//
// Public part of the face pipeline class
//

partial class FacePipeline
{
    #region Accessors for vertex buffers

    public GraphicsBuffer RawFaceVertexBuffer
      => _landmarkDetector.face.VertexBuffer;

    public GraphicsBuffer RawLeftEyeVertexBuffer
      => _landmarkDetector.eyeL.VertexBuffer;

    public GraphicsBuffer RawRightEyeVertexBuffer
      => _landmarkDetector.eyeR.VertexBuffer;

    public ComputeBuffer RefinedFaceVertexBuffer
      => _computeBuffer.filter;

    #endregion

    #region Accessors for cropped textures

    public Texture CroppedFaceTexture
      => _cropRT.face;

    public Texture CroppedLeftEyeTexture
      => _cropRT.eyeL;

    public Texture CroppedRightEyeTexture
      => _cropRT.eyeR;

    #endregion

    #region Accessors for crop region matrices

    public float4x4 FaceCropMatrix
      => _faceRegion.CropMatrix;

    public float4x4 LeftEyeCropMatrix
      => _leyeRegion.CropMatrix;

    public float4x4 RightEyeCropMatrix
      => _reyeRegion.CropMatrix;

    #endregion

    #region Public methods

    public FacePipeline(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture image)
      => RunPipeline(image);

    #endregion
}

} // namespace MediaPipe.FaceMesh
