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

        public VultureGHComponent()
            : base("VultureGH", "Vulture",
                "Component to communicate with VVVV",
                "Vulture", "vltr")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh (Please flatten any datatrees)", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Pt", "Points List (Please flatten any datatrees)", GH_ParamAccess.list, new Point3d(0, 0, 0));
            pManager.AddNumberParameter("Numbers", "N", "Number List (Please flatten any datatrees)", GH_ParamAccess.list, 0.0);
            pManager.AddTextParameter("Text", "Txt", "String/Text List (Please flatten any datatrees)", GH_ParamAccess.list, "");        
            pManager.AddBooleanParameter("SendCam", "RhCam", "Send RhinoCamera (ActiveViewport) Use TIMER to keep updated...", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Path", "Path", "File Path (Please end with the filename without extension)", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Enable", "On", "Enables the Plugin", GH_ParamAccess.item, false);
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
            // Retrieve and save data from inputs 
            
            List<Mesh> meshList = new List<Mesh>();
            List<Point3d> pointsList = new List<Point3d>();
            List<double> dblList = new List<double>();
            List<string> strList = new List<string>();
            String filePath = "";
            Boolean enabled = false;
            Boolean sendCam = false;

            if (!DA.GetDataList(0, meshList)) return;
            if (!DA.GetDataList(1, pointsList)) return;
            if (!DA.GetDataList(2, dblList)) return;
            if (!DA.GetDataList(3, strList)) return;
            if (!DA.GetData(4, ref sendCam)) return;
            if (!DA.GetData(5, ref filePath)) return;
            if (!DA.GetData(6, ref enabled)) return;

            // vulture init
   
            vulture Vulture = new vulture();

            if (enabled)
            {

                // Retrieve and write Meshes
                Vulture.meshes = new vultureMesh[meshList.Count];

                int offset = 0;
                for (int i = 0; i < meshList.Count; i++)
                {
                    Vulture.meshes[i] = MeshToVultureMesh3(meshList[i], offset, "" + i.ToString());
                    offset += Vulture.meshes[i].verticesVec3.Length;
                }

                // Retrieve and write Rhino Camera
                if (sendCam)
                {
                    Vulture.cameraPoints = new Vector3[3];
                    Point3d location = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                    Point3d target = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraTarget;
                    double hDAngle, hVAngle, hHAngle;
                    Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetCameraAngle(out hDAngle, out hVAngle, out hHAngle);
                    Vulture.cameraPoints[0] = new Vector3(toFloat(location.X), toFloat(location.Z), toFloat(location.Y));
                    Vulture.cameraPoints[1] = new Vector3(toFloat(target.X), toFloat(target.Z), toFloat(target.Y));
                    Vulture.cameraPoints[2] = new Vector3(toFloat(hDAngle), toFloat(hVAngle), toFloat(hHAngle));
                }
                else
                {
                    Vulture.cameraPoints = new Vector3[1];
                    Vulture.cameraPoints[0] = new Vector3(0, 0, 0);
                }

                // Retrieve and write all other data lists
                Vulture.points = new Vector3[pointsList.Count];
                for (int p = 0; p < pointsList.Count; p++)
                {
                    Vulture.points[p] = new Vector3(toFloat(pointsList[p].X), toFloat(pointsList[p].Z), toFloat(pointsList[p].Y));
            
                }

                Vulture.floats = new float[dblList.Count];
                for (int I = 0; I < dblList.Count; I++)
                {
                    Vulture.floats[I] = toFloat(dblList[I]);

                }

                Vulture.strings = new string[strList.Count];
                for (int I = 0; I < strList.Count; I++)
                {
                    Vulture.strings[I] = strList[I];

                }

                // Save vulture object
                WriteObjectToMMF(Vulture, filePath);

            }
             // Maybe add a message?!
             DA.SetData(0, "");
        }

        // Generate vultureMesh from Grasshopper Mesh (Change also the coordinate system to fit to vvvv)
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
                vertices3[v] = new Vector3(mesh.Vertices[v].X, mesh.Vertices[v].Z, mesh.Vertices[v].Y);
                normals3[v] = new Vector3(mesh.Normals[v].X, mesh.Normals[v].Z, mesh.Normals[v].Y);
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

        // Change Double to Float to match vvvv
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

        //------------------------------------------------------------------------------------------------------------------------- SAVE

        public void WriteObjectToMMF(object objectData, string filePath)
        {
            byte[] buffer = ObjectToByteArray(objectData);

            String name = ".vltr";
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
                    using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(name, buffer.Length))
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
            BinaryFormatter binaryFormatter = new BinaryFormatter();    
            MemoryStream memoryStream = new MemoryStream();             
            binaryFormatter.Serialize(memoryStream, inputObject);       
            return memoryStream.ToArray();                              
        }

        //------------------------------------------------------------------------------------------------------------------------- SAVE END

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.v4LogoGH;
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
