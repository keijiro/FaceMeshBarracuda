using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class MyVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] MyWebcamInput _webcam = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] UI.RawImage _rightEyeUI = null;
        [SerializeField] UI.RawImage _rightEyeDebug = null;
        [Space]
        [SerializeField] Shader _maskShader = null;
        [SerializeField] Shader _cropShader = null;
        [SerializeField] Shader _shader = null;
        [Space]
        [SerializeField] MeshFilter meshFilter;
        #endregion

        #region Private members

        FacePipeline _pipeline;
        Material _crop, _mask, _material;
        RenderTexture _croppedEyeRT, _maskedEyeRT;
        Mesh mesh;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            //Material初期化
            _mask = new Material(_maskShader);
            _crop = new Material(_cropShader);
            _material = new Material(_shader);

            //RenderTexture確保
            _croppedEyeRT = new RenderTexture(256, 256, 0);
            _maskedEyeRT = new RenderTexture(1024, 1024, 0);

            mesh = meshFilter.mesh;
            mesh.SetIndices(mesh.GetIndices(0), MeshTopology.LineStrip, 0);
        }

        void OnDestroy()
        {
            _pipeline.Dispose();
            Destroy(_mask);
            Destroy(_crop);
        }

        void LateUpdate()
        {
            // Processing on the face pipeline
            _pipeline.ProcessImage(_webcam.Texture);

            // UI update
            //_rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;

            //シェーダーに変換行列を適用
           _crop.SetMatrix("_Xform", _pipeline.RightEyeCropMatrix);

            //_mask.SetMatrix("_Xform", _pipeline.RightEyeCropMatrix);

            //maskシェーダーに頂点を設定
            _mask.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

            //webcamTextureを、_cropシェーダーを通して、RenderTextureに描画する
            Graphics.Blit(_webcam.Texture, _croppedEyeRT, _crop);

            Graphics.Blit(_webcam.Texture, _maskedEyeRT, _mask);

            //RawImageにRenderTextrueを反映
            if(_pipeline.IsFaceTracking)
            {
                _rightEyeUI.texture = _croppedEyeRT;
            }
            else
            {
                _rightEyeUI.texture = null;
            }
            //Debug.Log(_pipeline.RawRightEyeVertexBuffer.count);

            _rightEyeDebug.texture = _maskedEyeRT;


            //computebufferから頂点データ取得
            float4[] vertexData = new float4[_pipeline.RawRightEyeVertexBuffer.count];

            _pipeline.RawRightEyeVertexBuffer.GetData(vertexData);

            //頂点を描画
            List<Vector3> meshVert = new List<Vector3>();

            foreach(float4 vertex in vertexData)
            {
                meshVert.Add(vertex.xyz);
            }

            var vertices = new List<Vector3> {
                  new Vector3 (0.5f, 0.5f, 0),
                  new Vector3 (-0.5f, 0.5f, 0),
                  new Vector3 (0.5f, -0.5f, 0),
                  new Vector3 (-0.5f, -0.5f, 0),
             };
            var triangles = new List<int> {
                  1,
                  0,
                  2,
                  1,
                  2,
                  3
             };

           //mesh.SetVertices(meshVert);

            foreach(var vert in mesh.vertices)
            {
            //    Debug.Log(vert);
            }


            //mesh.SetVertices(vertices);

            //_material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

            //Graphics.DrawMesh(mesh, transform.position, transform.rotation, _material, 0);


            //Debug.Log(_pipeline.RawRightEyeVertexBuffer.GetData)

            meshFilter.GetComponent<Renderer>().material = new Material(_mask);

            _mask.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

        }

        #endregion
    }

} // namespace MediaPipe.FaceMesh
