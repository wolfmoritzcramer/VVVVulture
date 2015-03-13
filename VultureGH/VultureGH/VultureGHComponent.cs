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
            pManager.AddPointParameter("Lights", "L", "Lights (1.Location 2.Direction | Same: PointLight)", GH_ParamAccess.list);
            pManager.AddPointParameter("Cameras", "C", "Cameras (1.Location 2.Direction)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("WriteFile", "WF", "Write the File to Disk", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Path", "P", "File Path", GH_ParamAccess.item, "");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("MessageOut", "sM", "Message that has been send", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Inputs abrufen und speichern
            
            List<Mesh> meshList = new List<Mesh>();
            List<Point3d> lightsList = new List<Point3d>();
            List<Point3d> camerasList = new List<Point3d>();

            Boolean writeFile = false;
            String filePath = "";


            // vulture Initialisieren
   
            vulture Vulture = new vulture();

            //if (!DA.GetDataList(0, intList)) return;
           // if (!DA.GetDataList(0, verticesList)) return;
            if (!DA.GetDataList(0, meshList)) return;
            if (!DA.GetDataList(1, lightsList)) return;
            if (!DA.GetDataList(2, camerasList)) return;
            if (!DA.GetData(3, ref writeFile)) return;
            if (!DA.GetData(4, ref filePath)) return;
            
            Vulture.meshes = new vultureMesh[meshList.Count];

            int offset = 0;
            for (int i = 0; i < meshList.Count; i++)
            {
                Vulture.meshes[i] = MeshToVultureMesh3(meshList[i],offset, "Test " + i.ToString());
                offset += Vulture.meshes[i].verticesVec3.Length;
            }

            Vulture.lightPoints = new Vector3[lightsList.Count];

            for (int l = 0; l < lightsList.Count; l++)
            {
                //Vulture.lightPoints[l] = new Vector3(toFloat(lightsList[l].X), toFloat(lightsList[l].Y), toFloat(lightsList[l].Z));
                Vulture.lightPoints[l] = new Vector3(0, 1 ,0);
            }

            Vulture.cameraPoints = new Vector3[camerasList.Count];

            for (int c = 0; c < camerasList.Count; c++)
            {
                Vulture.cameraPoints[c] = new Vector3(toFloat(camerasList[c].X), toFloat(camerasList[c].Y), toFloat(camerasList[c].Z));
            }

            WriteObjectToMMF(Vulture, filePath, writeFile);

             DA.SetData(0, Vulture.lightPoints.Length.ToString());
            //  DA.SetData(0, verticesList[0].X.ToString());
        }

        public vultureMesh MeshToVultureMesh3(Mesh mesh,int offset, String description)
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
                indices3[f] = new Vector3(mesh.Faces[f].A + offset, mesh.Faces[f].B + offset, mesh.Faces[f].C + offset);
            }

            vMesh.verticesVec3 = vertices3;
            vMesh.normalsVec3 = normals3;
            vMesh.texVec2 = tex2;
            vMesh.indicesVec3 = indices3;


            return vMesh;
        }


        public float toFloat(Double input)
        {
            float result = (float) input;
            if (float.IsPositiveInfinity(result))
            {
                result = float.MaxValue;
            } else if (float.IsNegativeInfinity(result))
            {
                result = float.MinValue;
            }
            return result;
        }

        //------------------------------------------------------------------------------------------------------------------------- SPEICHERN

        public void WriteObjectToMMF(object objectData, string filePath, Boolean writeFile)
        {
            byte[] buffer = ObjectToByteArray(objectData);

            String name = "vvvvulture.vulture";
            try
            {
                if (!filePath.Equals(""))
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
