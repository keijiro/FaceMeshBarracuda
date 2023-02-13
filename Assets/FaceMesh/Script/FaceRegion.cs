using MediaPipe.BlazeFace;
using Unity.Mathematics;

namespace MediaPipe.FaceMesh {

//
// Face region tracker class
//

sealed class FaceRegion
{
    #region Exposed properties

    public float4x4 RotationMatrix { get; private set; }
    public float4x4 CropMatrix { get; private set; }

    #endregion

    #region Internal state

    Float4Filter _box = new Float4Filter(2, 1.5f);
    FloatFilter _angle = new FloatFilter(1.3f, 1.5f);

    void UpdateMatrices()
    {
        var box = new BoundingBox(_box.Value);
        RotationMatrix = MathUtil.ZRotateAtCenter(_angle.Value);
        CropMatrix = math.mul(box.CropMatrix, RotationMatrix);
    }

    #endregion

    #region Private methods

    static BoundingBox AddPadding(BoundingBox box)
      => box.Squarified * 1.5f;

    #endregion

    #region Public methods

    public float4 Transform(float4 point)
      => math.mul(CropMatrix, point);

    public void Step(BoundingBox box, float2 up)
    {
        var t_e = UnityEngine.Time.deltaTime;
        var angle = MathUtil.Angle(up) - math.PI / 2;

        _box = _box.Next(AddPadding(box).AsFloat4, t_e);
        _angle = _angle.Next(angle, t_e);

        UpdateMatrices();
    }

    public void TryUpdateWithDetection(in Detection face)
    {
        var box = AddPadding(new BoundingBox(face));
        var angle = MathUtil.Angle(face.nose - face.mouth) - math.PI / 2;

        // Do nothing if the boxes overlap much.
        var iou = BoundingBox.CalculateIOU(box, new BoundingBox(_box.Value));
        if (iou > 0.5f) return;

        _angle = new FloatFilter(_angle, angle);
        _box = new Float4Filter(_box, box.AsFloat4);

        UpdateMatrices();
    }

    #endregion
}

} // namespace MediaPipe.FaceMesh
