using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.FaceMesh
{
    public class EyeContourMesh : MonoBehaviour
    {

        [SerializeField] Shader _shader;
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
            for (int i = 0; i < 17; i++)
            {
                meshParams.vertices.Add(new Vector3(0, 0, 0));

                meshParams.vertices.Add(new Vector2(0, 0));
            }

            //Setup Triangles
            for (int i = 0; i < 7; i++)
            {

                int[] triangle =
                    {
                        i, i+9, i+1,
                        i+1, i+9, i+10
                        };

                meshParams.triangles.AddRange(triangle);
            }

            _mesh.SetVertices(meshParams.vertices);
            _mesh.SetUVs(0, meshParams.uv);
            _mesh.SetTriangles(meshParams.triangles, 0);

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