using System;
using System.IO;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Runtime.Serialization.Formatters.Binary;
using vultureCommunicator;
using SlimDX;

namespace VultureGH
{
    public class VultureGHComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VultureGHComponent()
            : base("VultureGH", "Vulture",
                "Component to communicate with VVVV",
                "Vulture", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
         //   pManager.AddPointParameter("Vertices", "V", "Vertices", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddBooleanParameter("WriteFile", "WF", "Write the File to Disk", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Path", "P", "File Path", GH_ParamAccess.item, "C:\\TEMP\\");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("MessageOut", "sM", "Message that has been send", GH_ParamAccess.item);
        }

        List<Point3d> testList = new List<Point3d>();
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Inputs abrufen und speichern
            List<Point3d> verticesList = new List<Point3d>();
            List<Mesh> meshList = new List<Mesh>();
            Boolean writeFile = false;
            String filePath = "";

            // vulture Initialisieren
            vulture vvvv = new vulture();
            vulture Vulture = new vulture();

            //if (!DA.GetDataList(0, intList)) return;
           // if (!DA.GetDataList(0, verticesList)) return;
            if (!DA.GetDataList(0, meshList)) return;
            if (!DA.GetData(1, ref writeFile)) return;
            if (!DA.GetData(2, ref filePath)) return;

            Vulture.meshes = new vultureMesh[meshList.Count];
            vvvv.meshes = new vultureMesh[meshList.Count];

            // vultureMesh vMesh = new vultureMesh();

            //vMesh.description = "test";
            //vMesh.vertices = new vultureVertex[verticesList.Count];
            //vultureVertex[] vVertex = new vultureVertex[verticesList.Count];

            // for (int i = 0; i < verticesList.Count; i++)
            // {
            //     vVertex[i] = new vultureVertex();
            //     vVertex[i].x = verticesList[i].X;
            //     vVertex[i].y = verticesList[i].Y;
            //     vVertex[i].z = verticesList[i].Z;
            // }

            //vMesh.vertices = vVertex;

            for (int i = 0; i < meshList.Count; i++)
            {
                vvvv.meshes[i] = MeshToVultureMesh(meshList[i], "Test " + i.ToString());
                Vulture.meshes[i] = MeshToVultureMesh3(meshList[i], "Test " + i.ToString());
            }

            WriteObjectToMMF(Vulture, filePath, writeFile);
            DA.SetDataList(0, testList);
            // DA.SetData(0, test.ToString());
            //  DA.SetData(0, verticesList[0].X.ToString());
        }

        public vultureMesh MeshToVultureMesh3(Mesh mesh, String description)
        {
            mesh.Faces.ConvertQuadsToTriangles();
            vultureMesh vMesh = new vultureMesh();
            vMesh.description = description;

            Vector3[] vertices3 = new Vector3[mesh.Vertices.Count];
            Vector3[] normals3 = new Vector3[mesh.Normals.Count];
            Vector2[] tex2 = new Vector2[mesh.TextureCoordinates.Count];
            Vector3[] indices3 = new Vector3[mesh.Faces.Count];

            int[] indicesInt = new int[mesh.Faces.Count*3];
           
            for (int v = 0; v < mesh.Vertices.Count; v++)
            {
                vertices3[v] = new Vector3(mesh.Vertices[v].X, mesh.Vertices[v].Y, mesh.Vertices[v].Z);
                normals3[v] = new Vector3(mesh.Normals[v].X, mesh.Normals[v].Y, mesh.Normals[v].Z);
                tex2[v] = new Vector2(mesh.TextureCoordinates[v].X, mesh.TextureCoordinates[v].Y);
            }

            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                indices3[f] = new Vector3(mesh.Faces[f].A, mesh.Faces[f].B, mesh.Faces[f].C);
            }

            vMesh.verticesVec3 = vertices3;
            vMesh.normalsVec3 = normals3;
            vMesh.texVec2 = tex2;
            vMesh.indicesVec3 = indices3;


            return vMesh;
        }

        public vultureMesh MeshToVultureMesh(Mesh mesh, String description)
        {

            vultureMesh vMesh = new vultureMesh();
            vMesh.description = description;
            Point3d Location = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
            Vector3d Direction = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraDirection;
            vMesh.description = Location.X.ToString() + " " + Location.Y.ToString() + " " + Location.Z.ToString() + "|" + Direction.X.ToString() + " " + Direction.Y.ToString() + " " + Direction.Z.ToString();

            Point3d[] vertices = mesh.Vertices.ToPoint3dArray();
            // mesh.Vertices.
            // vMesh.vertices = new vultureVertex[vertices.Length];
            vultureVertex[] vVertex = new vultureVertex[vertices.Length];
            vultureVertex[] vNormal = new vultureVertex[vertices.Length];
          
            testList.Clear();
            for (int i = 0; i < vertices.Length; i++)
            {
                vVertex[i] = new vultureVertex();
                vVertex[i].x = vertices[i].X;
                vVertex[i].y = vertices[i].Y;
                vVertex[i].z = vertices[i].Z;
                testList.Add(vertices[i]);
                vNormal[i] = new vultureVertex();
                vNormal[i].x = mesh.Normals[i].X;
                vNormal[i].y = mesh.Normals[i].Y;
                vNormal[i].z = mesh.Normals[i].Z;

            }
            vMesh.vertices = vVertex;
            vMesh.normals = vNormal;
           // int[] vIndices = mesh.Faces.ToIntArray(true);
            vMesh.faceCount = mesh.Faces.Count;
           // vMesh.indices = vIndices;

            return vMesh;
        }

        //------------------------------------------------------------------------------------------------------------------------- SPEICHERN

        public void WriteObjectToMMF(object objectData, string filePath, Boolean writeFile)
        {
            byte[] buffer = ObjectToByteArray(objectData);

            String name = "vvvvulture.vulture";
            try
            {
                if (writeFile)
                {
                    using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath + name, FileMode.Create, null, buffer.Length))
                    {
                        using (MemoryMappedViewAccessor mmfWriter = mmf.CreateViewAccessor(0, buffer.Length))
                        {
                            mmfWriter.WriteArray<byte>(0, buffer, 0, buffer.Length);
                        }
                    }
                }
                else
                {
                    using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(name, buffer.Length))//MemoryMappedFile.CreateFromFile(mmfName, FileMode.Create, null, buffer.Length))
                    {
                        using (MemoryMappedViewAccessor mmfWriter = mmf.CreateViewAccessor(0, buffer.Length))
                        {
                            mmfWriter.WriteArray<byte>(0, buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            catch { }
        }

        public byte[] ObjectToByteArray(object inputObject)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();    // Create new BinaryFormatter
            MemoryStream memoryStream = new MemoryStream();             // Create target memory stream
            binaryFormatter.Serialize(memoryStream, inputObject);       // Serialize object to stream
            return memoryStream.ToArray();                              // Return stream as byte array
        }

        //------------------------------------------------------------------------------------------------------------------------- SPEICHERN ENDE








        private String sendString(string data)
        {

            byte[] dataBy = StringToByteArray(data);
            MemoryMappedFile file = MemoryMappedFile.CreateOrOpen("wolvvvv", dataBy.Length + 62);
            MemoryMappedViewAccessor writer = file.CreateViewAccessor(0, dataBy.Length + 62);
            writer.Write(54, (ulong)dataBy.Length);
            writer.WriteArray(54 + 8, dataBy, 0, dataBy.Length);
            //writer.WriteArray(54 + 8, data, 0, data.Length);

            return data;
        }

        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5d8612f6-6f72-4578-9d66-d1db10615179}"); }
        }
    }
}
