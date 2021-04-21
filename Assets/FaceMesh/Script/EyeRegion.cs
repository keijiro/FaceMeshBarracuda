using Unity.Mathematics;

namespace MediaPipe.FaceMesh {

//
// Eye region calculator class
//

sealed class EyeRegion
{
    #region Exposed proeprties

    public float4x4 CropMatrix { get; private set; }

    #endregion

    #region Internal state

    bool _flipped;

    #endregion

    #region Public method

    public EyeRegion(bool flipped = false)
      => _flipped = flipped;

    public void Update(float2 p0, float2 p1, float4x4 rotation)
    {
        var box = BoundingBox.CenterExtent
          ((p0 + p1) / 2, math.distance(p0, p1) * 1.4f);

        CropMatrix = math.mul(box.CropMatrix, rotation);

        if (_flipped)
            CropMatrix = math.mul(CropMatrix, MathUtil.HorizontalFlip());
    }

    #endregion
}

} // namespace MediaPipe.FaceMesh
