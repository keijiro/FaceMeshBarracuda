using UnityEngine;
using Unity.Barracuda;

namespace MediaPipe.FaceMesh {

//
// ScriptableObject class used to hold references to internal assets
//
[CreateAssetMenu(fileName = "ResourceSet",
                 menuName = "ScriptableObjects/MediaPipe/FaceMesh/Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public MediaPipe.BlazeFace.ResourceSet blazeFace;
    public MediaPipe.FaceLandmark.ResourceSet faceLandmark;
    public MediaPipe.Iris.ResourceSet iris;

    public Shader preprocessShader;
    public ComputeShader postprocessCompute;

    public Mesh faceMeshTemplate;
    public Mesh faceLineTemplate;
}

} // namespace MediaPipe.FaceMesh
