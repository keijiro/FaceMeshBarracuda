using Unity.Barracuda;
using UnityEngine;

namespace FaceMesh {

static class IWorkerExtensions
{
    //
    // Retrieves an output tensor from a NN worker and returns it as a
    // temporary render texture. The caller must release it using
    // RenderTexture.ReleaseTemporary.
    //
    public static RenderTexture
      CopyOutputToTempRT(this IWorker worker, int w, int h)
    {
        var fmt = RenderTextureFormat.RFloat;
        var shape = new TensorShape(1, h, w, 1);
        var rt = RenderTexture.GetTemporary(w, h, 0, fmt);
        using (var tensor = worker.PeekOutput().Reshape(shape))
            tensor.ToRenderTexture(rt);
        return rt;
    }
}

} // namespace FaceMesh
