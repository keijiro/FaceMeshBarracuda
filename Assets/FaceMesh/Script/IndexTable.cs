using UnityEngine;

namespace MediaPipe.FaceMesh {

//
// Vertex index table class mainly used to provide eye to face mesh vertex
// transfer table
//
static class IndexTable
{
    // Create a compute buffer holding the eye-to-face transfer table.
    public static ComputeBuffer CreateEyeToFaceLandmarkBuffer()
    {
        var buffer = new ComputeBuffer(EyeToFaceLandmark.Length, sizeof(uint));
        buffer.SetData(EyeToFaceLandmark);
        return buffer;
    }

    // Eye-to-face transfer table
    public static uint[] EyeToFaceLandmark =
    {
        // Left eye
        // eye lower contour
        33,
        7,
        163,
        144,
        145,
        153,
        154,
        155,
        133,
        // eye upper contour (excluding corners)
        246,
        161,
        160,
        159,
        158,
        157,
        173,
        // halo x2 lower contour
        130,
        25,
        110,
        24,
        23,
        22,
        26,
        112,
        243,
        // halo x2 upper contour (excluding corners)
        247,
        30,
        29,
        27,
        28,
        56,
        190,
        // halo x3 lower contour
        226,
        31,
        228,
        229,
        230,
        231,
        232,
        233,
        244,
        // halo x3 upper contour (excluding corners)
        113,
        225,
        224,
        223,
        222,
        221,
        189,
        // halo x4 upper contour (no lower because of mesh structure)
        // or eyebrow inner contour
        35,
        124,
        46,
        53,
        52,
        65,
        // halo x5 lower contour
        143,
        111,
        117,
        118,
        119,
        120,
        121,
        128,
        245,
        // halo x5 upper contour (excluding corners)
        // or eyebrow outer contour
        156,
        70,
        63,
        105,
        66,
        107,
        55,
        193,

        // Right eye
        // eye lower contour
        263,
        249,
        390,
        373,
        374,
        380,
        381,
        382,
        362,
        // eye upper contour (excluding corners)
        466,
        388,
        387,
        386,
        385,
        384,
        398,
        // halo x2 lower contour
        359,
        255,
        339,
        254,
        253,
        252,
        256,
        341,
        463,
        // halo x2 upper contour (excluding corners)
        467,
        260,
        259,
        257,
        258,
        286,
        414,
        // halo x3 lower contour
        446,
        261,
        448,
        449,
        450,
        451,
        452,
        453,
        464,
        // halo x3 upper contour (excluding corners)
        342,
        445,
        444,
        443,
        442,
        441,
        413,
        // halo x4 upper contour (no lower because of mesh structure)
        // or eyebrow inner contour
        265,
        353,
        276,
        283,
        282,
        295,
        // halo x5 lower contour
        372,
        340,
        346,
        347,
        348,
        349,
        350,
        357,
        465,
        // halo x5 upper contour (excluding corners)
        // or eyebrow outer contour
        383,
        300,
        293,
        334,
        296,
        336,
        285,
        417
    };
}

} // namespace MediaPipe.FaceMesh
