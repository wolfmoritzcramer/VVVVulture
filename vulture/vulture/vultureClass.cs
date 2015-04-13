using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace vultureCommunicator
{
    [Serializable]
    public class vulture
    {
        public string description;
        public vultureMesh[] meshes;
        public Vector3[] cameraPoints;
        public Vector3[] lightPoints;
        public Vector3[] points;
        public int[] integers;
        public string[] strings;

    }

    [Serializable]
    public class vultureMesh
    {
        public string description;
        public Vector3[] verticesVec3;
        public Vector3[] normalsVec3;
        public Vector3[] indicesVec3;
        public Vector2[] texVec2;
    }

}
