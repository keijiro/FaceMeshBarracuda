using UnityEngine;
using Unity.Barracuda;

namespace MediaPipe.FaceMesh {

//
// ScriptableObject class used to hold references to internal assets
//
[CreateAssetMenu(fileName = "FaceMesh",
                 menuName = "ScriptableObjects/MediaPipe/FaceMesh Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public ComputeShader preprocess;
    public ComputeShader postprocess;
}

} // namespace MediaPipe.FaceMesh
