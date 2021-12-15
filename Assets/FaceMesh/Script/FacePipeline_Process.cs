using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace MediaPipe.FaceMesh {

//
// Image processing part of the face pipeline class
//

partial class FacePipeline
{
    // Face/eye region trackers
    FaceRegion _faceRegion = new FaceRegion();
    EyeRegion _leyeRegion = new EyeRegion();
    EyeRegion _reyeRegion = new EyeRegion(true);

    // Vertex retrieval from the face landmark detector
    float4 GetFaceVertex(int index)
      => _landmarkDetector.face.VertexArray.ElementAt(index);

        //tracking status
        bool _isFaceTracking = false;

    void RunPipeline(Texture input)
    {
        // Face detection
        _faceDetector.ProcessImage(input);

        // Cancel if the face detection score is too low.
        var face = _faceDetector.Detections.FirstOrDefault();
        if (face.score < 0.5f)
            {
                _isFaceTracking = false;
                return;
            }

            else
            {
                _isFaceTracking = true;
            }

            // Try updating the face region with the detection result. It's
            // actually updated only when there is a noticeable jump from the last
            // frame.
            _faceRegion.TryUpdateWithDetection(face);

        // Face region cropping
        _preprocess.SetMatrix("_Xform", _faceRegion.CropMatrix);
        Graphics.Blit(input, _cropRT.face, _preprocess, 0);

        // Face landmark detection
        _landmarkDetector.face.ProcessImage(_cropRT.face);

        // Key points from the face landmark
        var mouth    = _faceRegion.Transform(GetFaceVertex( 13)).xy;
        var mid_eyes = _faceRegion.Transform(GetFaceVertex(168)).xy;
        var eye_l0   = _faceRegion.Transform(GetFaceVertex( 33)).xy;
        var eye_l1   = _faceRegion.Transform(GetFaceVertex(133)).xy;
        var eye_r0   = _faceRegion.Transform(GetFaceVertex(362)).xy;
        var eye_r1   = _faceRegion.Transform(GetFaceVertex(263)).xy;

        // Eye region update
        _leyeRegion.Update(eye_l0, eye_l1, _faceRegion.RotationMatrix);
        _reyeRegion.Update(eye_r0, eye_r1, _faceRegion.RotationMatrix);

        // Eye region cropping
        _preprocess.SetMatrix("_Xform", _leyeRegion.CropMatrix);
        Graphics.Blit(input, _cropRT.eyeL, _preprocess, 0);

        _preprocess.SetMatrix("_Xform", _reyeRegion.CropMatrix);
        Graphics.Blit(input, _cropRT.eyeR, _preprocess, 0);

        // Eye landmark detection
        _landmarkDetector.eyeL.ProcessImage(_cropRT.eyeL);
        _landmarkDetector.eyeR.ProcessImage(_cropRT.eyeR);

        // Postprocess for face mesh construction
        var post = _resources.postprocessCompute;

        post.SetMatrix("_fx_xform", _faceRegion.CropMatrix);
        post.SetBuffer(0, "_fx_input", _landmarkDetector.face.VertexBuffer);
        post.SetBuffer(0, "_fx_output", _computeBuffer.post);
        post.SetBuffer(0, "_fx_bbox", _computeBuffer.bbox);
        post.Dispatch(0, 1, 1, 1);

        post.SetBuffer(1, "_e2f_index_table", _computeBuffer.eyeToFace);
        post.SetBuffer(1, "_e2f_eye_l", _landmarkDetector.eyeL.VertexBuffer);
        post.SetBuffer(1, "_e2f_eye_r", _landmarkDetector.eyeR.VertexBuffer);
        post.SetMatrix("_e2f_xform_l", _leyeRegion.CropMatrix);
        post.SetMatrix("_e2f_xform_r", _reyeRegion.CropMatrix);
        post.SetBuffer(1, "_e2f_face", _computeBuffer.post);
        post.Dispatch(1, 1, 1, 1);

        post.SetBuffer(2, "_lpf_input", _computeBuffer.post);
        post.SetBuffer(2, "_lpf_output", _computeBuffer.filter);
        post.SetFloat("_lpf_beta", 30.0f);
        post.SetFloat("_lpf_cutoff_min", 1.5f);
        post.SetFloat("_lpf_t_e", Time.deltaTime);
        post.Dispatch(2, 468 / 52, 1, 1);

        // Face region update based on the postprocessed face mesh
        _faceRegion.Step
          (_computeBuffer.bbox.GetBoundingBoxData(), mid_eyes - mouth);
    }
}

} // namespace MediaPipe.FaceMesh
