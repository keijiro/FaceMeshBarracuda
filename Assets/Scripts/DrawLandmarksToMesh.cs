using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace MediaPipe.FaceMesh
{

    public class DrawLandmarksToMesh : MonoBehaviour
    {
        //[SerializeField] Shader _shader = null;

        Mesh _mesh;
        Material _material;

        // Start is called before the first frame update
        private void Start()
        {
            Shader shader = Shader.Find("Hidden/MediaPipe/FaceMesh/Mask");

            _material = new Material(shader);

            _mesh = new Mesh();
            _mesh.SetIndices(_mesh.GetIndices(0), MeshTopology.LineStrip, 0);


        }

        public void DrawEye(ComputeBuffer vertexBuffer, Texture texture)
        {




        }


        // Update is called once per frame
        public void Draw(ComputeBuffer vertexBuffer, Texture texture)
        {

            //処理結果にアクセス
            float4[] vertexData = new float4[vertexBuffer.count];

            vertexBuffer.GetData(vertexData);

            //頂点を描画
            List<Vector3> meshVert = new();

            List<Vector2> UVs = new();

            List<int> triangles = new();

            try
            {
                //頂点とUV座標を設定
                for (int i = 5; i < 22; i++)//目の周りの頂点だけを選択
                //for (int i = 21; i < 38; i++)//目の周りをやや広めに選択
                {
                    meshVert.Add(vertexData[i].xyz);

                    UVs.Add(vertexData[i].xy);
                }

                //ポリゴンを設定
                for (int i = 0; i < 7; i++)
                {

                    int[] triangle =
                        {
                        i, i+9, i+1,
                        i+1, i+9, i+10
                        };

                    triangles.AddRange(triangle);
                }
            }

            catch
            {

            }

            UpdateMesh(_mesh, meshVert, UVs, triangles);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);

        }

        private void UpdateMesh(Mesh mesh, List<Vector3> meshVert, List<Vector2> UVs, List<int> triangles)
        {
            mesh.SetVertices(meshVert);
            mesh.SetUVs(0, UVs);
            mesh.SetTriangles(triangles, 0);
        }
    }
}