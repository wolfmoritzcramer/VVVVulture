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
    }

    [Serializable]
    public class vultureMesh
    {
        public string description;
        public vultureVertex[] vertices;
        public vultureVertex[] normals;
        public int[] indices;
        public int faceCount;
        public int vertexCount;
        public Vector3[] verticesVec3;
        public Vector3[] normalsVec3;
    }

    [Serializable]
    public class vultureVertex
    {
        public double x;
        public double y;
        public double z;
    }

}
