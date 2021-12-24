using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MediaPipe.FaceMesh
{
    public class IrisMesh : MonoBehaviour
    {

        [SerializeField] Shader _shader;
        [SerializeField] Camera targetCamera;
        Mesh _mesh;
        Material _material;

        class MeshParams
        {
            public List<Vector3> vertices;

            public List<Vector2> uv;

            public List<int> triangles;

            public MeshParams()
            {
                vertices = new();
                uv = new();
                triangles = new();
            }

        }
     
        void Start()
        {

            _material = new Material(_shader);

            _mesh = new Mesh();


            //Setup mesh for eye contour

            MeshParams meshParams = new();

            //Setup vertex, UV
            for (int i = 0; i < 16 ; i++)
            {
                meshParams.vertices.Add(new Vector3(0, 0, 0));

                meshParams.vertices.Add(new Vector2(0, 0));
            }

            //Setup Triangles
            for (int i = 0; i < 7; i++)
            {

                int[] triangle =
                    {
                        i, 15-i, i+1,
                        15-i, 14-i, i+1
                        };

                meshParams.triangles.AddRange(triangle);
            }

            _mesh.SetVertices(meshParams.vertices);
            _mesh.SetUVs(0, meshParams.uv);
            _mesh.SetTriangles(meshParams.triangles, 0);

            //_mesh.SetIndices(_mesh.GetIndices(0), MeshTopology.LineStrip, 0);
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

        public void UpdateMesh(ComputeBuffer vertexBuffer, float4x4 cropMatrix)
        {
            var fF = MathUtil.ScaleOffset(1f, math.float2(0f, 0f));
            cropMatrix = math.mul(fF, cropMatrix);

            _material.SetMatrix("_Xform", cropMatrix);
            UpdateMesh(vertexBuffer);
        }

            public void Draw(Texture texture)
        {
            _material.SetTexture("_MainTex", texture);
            
            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material,0,targetCamera);
        }
    }
}