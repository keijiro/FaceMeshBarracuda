using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace MediaPipe.FaceMesh
{

    public class DrawLandmarksToMesh : MonoBehaviour
    {
        [SerializeField] Shader _shader = null;

        Mesh _mesh;
        Material _material;

        // Start is called before the first frame update
        private void Start()
        {
            _material = new Material(_shader);

            _mesh = new Mesh();
            _mesh.SetIndices(_mesh.GetIndices(0), MeshTopology.LineStrip, 0);


        }

        // Update is called once per frame
        public void Draw(ComputeBuffer vertexBuffer)
        {

            //処理結果にアクセス
            float4[] vertexData = new float4[vertexBuffer.count];

            vertexBuffer.GetData(vertexData);

            //頂点を描画
            List<Vector3> meshVert = new();

            List<int> indecies = new();

            int index = 0;

            foreach (float4 vertex in vertexData)
            {
                meshVert.Add(vertex.xyz);
                indecies.Add(index);
                index++;
            }

            _mesh.SetVertices(meshVert);
            _mesh.SetIndices(indecies, MeshTopology.Points, 0);
            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);

        }
    }
}