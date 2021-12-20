using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.FaceMesh
{

    public class FaceMeshTransformed : MonoBehaviour
    {
        [SerializeField] Shader _transformShader;
        [SerializeField] ResourceSet _resource;

        Mesh _mesh;
        Material _material;

        void Start()
        {

            _material = new Material(_transformShader);

            _mesh = _resource.faceMeshTemplate;
        }

        public void UpdateMesh(ComputeBuffer vertexBuffer)
        {
            try
            {
                _material.SetBuffer("_Vertices", vertexBuffer);
            }
            catch
            {

            }
        }

        public void Draw(Texture texture)
        {
            _material.SetTexture("_MainTex", texture);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);
        }
    }
}