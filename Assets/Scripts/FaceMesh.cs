using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MediaPipe.FaceMesh
{
    public class FaceMesh : MonoBehaviour
    {
        [SerializeField] Shader _shader;
        [SerializeField] ResourceSet _resource;

        Mesh _mesh;
        Material _material;

        void Start()
        {

            _material = new Material(_shader);

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