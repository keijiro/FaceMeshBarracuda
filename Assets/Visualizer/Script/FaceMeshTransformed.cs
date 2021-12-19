using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.FaceMesh
{

    public class FaceMeshTransformed : MonoBehaviour
    {
        [SerializeField] ResourceSet _resource;

        Mesh _mesh;
        Material _material;

        void Start()
        {
            Shader transformShader = Shader.Find("Hidden/MediaPipe/FaceMesh/FaceTextureTransform");

            _material = new Material(transformShader);

            _mesh = _resource.faceMeshTemplate;
        }

        public void UpdateMesh(ComputeBuffer vertexBuffer)
        {
            _material.SetBuffer("_Vertices", vertexBuffer);
        }

        public void Draw(Texture texture)
        {
            _material.SetTexture("_MainTex", texture);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);
        }
    }
}