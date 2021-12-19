using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace MediaPipe.FaceMesh
{

    public class DrawLandmarksToMesh : MonoBehaviour
    {
        [SerializeField] ResourceSet _resources = null;

        Mesh _mesh;
        Material _material;

        bool isFaceTriangleReversed = true;

        class MeshParams
        {
         public   List<Vector3> meshVert;

         public   List<Vector2> UVs;

         public   List<int> triangles;

         public   MeshParams()
            {
                meshVert = new();
                UVs = new();
                triangles = new();
            }

        }

        // Start is called before the first frame update
        private void Start()
        {
            Shader shader = Shader.Find("Hidden/MediaPipe/FaceMesh/Mask");

            _material = new Material(shader);

            _mesh = new Mesh();
            _mesh.SetIndices(_mesh.GetIndices(0), MeshTopology.Triangles, 0);


        }

        public void DrawEye(ComputeBuffer vertexBuffer, Texture texture)
        {
            UpdateMeshWithEye(ref _mesh, vertexBuffer);

            _material.SetTexture("_MainTex", texture);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);

        }




        public void DrawFace(ComputeBuffer vertexBuffer, Texture texture)
        {
            UpdateMeshWithFace(ref _mesh, vertexBuffer);

            _material.SetTexture("_MainTex", texture);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);

        }


        void UpdateMeshWithEye(ref Mesh mesh, ComputeBuffer vertexBuffer)
        {

            //処理結果にアクセス
            float4[] vertexData = new float4[vertexBuffer.count];

            vertexBuffer.GetData(vertexData);

            //頂点を描画
            MeshParams meshParams = new MeshParams();

            try
            {
                //頂点とUV座標を設定
                for (int i = 5; i < 22; i++)//目の周りの頂点だけを選択
                //for (int i = 21; i < 38; i++)//目の周りをやや広めに選択
                {
                    meshParams.meshVert.Add(vertexData[i].xyz);

                    meshParams.UVs.Add(vertexData[i].xy);
                }

                //ポリゴンを設定
                for (int i = 0; i < 7; i++)
                {

                    int[] triangle =
                        {
                        i, i+9, i+1,
                        i+1, i+9, i+10
                        };

                    meshParams.triangles.AddRange(triangle);
                }
            }

            catch
            {

            }

            UpdateMesh(mesh, meshParams.meshVert, meshParams.UVs, meshParams.triangles);


        }

        void UpdateMeshWithFace(ref Mesh mesh, ComputeBuffer vertexBuffer)
        {

            //処理結果にアクセス
            float4[] vertexData = new float4[vertexBuffer.count];

            vertexBuffer.GetData(vertexData);

            MeshParams meshParams = new MeshParams();

            mesh = _resources.faceMeshTemplate;
           
            try
            {
                foreach(float4 vertex in vertexData)
                {
                    meshParams.meshVert.Add(vertex.xyz); 
                }
            }

            catch
            {

            }

            //頂点座標をコピー
            mesh.SetVertices(meshParams.meshVert);

            //trianglesを逆順にして設定
            if (isFaceTriangleReversed)
            {
                meshParams.triangles.AddRange(mesh.triangles);
                meshParams.triangles.Reverse();
                mesh.SetTriangles(meshParams.triangles, 0);
                isFaceTriangleReversed = false;
            }
        }



        private void UpdateMesh(Mesh mesh, List<Vector3> meshVert, List<Vector2> UVs, List<int> triangles)
        {
            mesh.SetVertices(meshVert);
            mesh.SetUVs(0, UVs);
            mesh.SetTriangles(triangles, 0);
        }
    }
}