#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using vultureCommunicator;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "VVVVulture", Category = "EX9.Geometry", Help = "Node to communicate with Grasshopper via Vulture", Tags = "")]
	#endregion PluginInfo
	public class VVVVultureNode : DXMeshOutPluginBase,IPluginEvaluate
	{
		#region fields & pins
		[Input("Enabled")]
		public ISpread<Boolean> Fenabled;
		
		[Input("Read from File")]
		public ISpread<Boolean> FreadFile;
		
		[Input("File", StringType = StringType.Filename)]
		public ISpread<string> FfilePath;		
	
		[Output("Output")]
		public ISpread<string> FOutput;
		
		
		//[Output("Indices")]
		// ISpread<ISpread<Vector3>> Findices;
		//[Output("Vertices3")]
		// ISpread<Vector3> Fvertices3;
		//[Output("Normals3")]
		// ISpread<Vector3> Fnormals3;
		//[Output("Tex2")]
		// ISpread<Vector2> Ftex2;
		
		//[Output("Centroid", Order = 1)]
		//ISpread<Vector3D> FCen;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		vulture Vulture;
		private bool reinit;
		Vector3[] FPos;
		Vector3[] FNorm;
		Vector3[] FInd;
		Vector2[] FTexcd;
		
		[ImportingConstructor()]
		public VVVVultureNode(IPluginHost host) : base(host)
		{
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			int x = 0;
			reinit = false;
			if (Fenabled[0]){

				try{

					Vulture = ReadObjectFromMMF(FreadFile[0],FfilePath[0]) as vulture;	

					
					if (Vulture != null){
						
						FOutput[0] = "Vulture is thereeeeeeee!";
				
						FPos = Vulture.meshes[x].verticesVec3;
						FNorm = Vulture.meshes[x].normalsVec3;
						FTexcd = Vulture.meshes[x].texVec2;
						FInd = Vulture.meshes[x].indicesVec3;
						
						FOutput[0] = FInd.Length.ToString();
						Reinitialize();
						

					}
					else{
						FOutput[0] = "Something is wrong!";
					}
				}
				catch{}
			
			}
			else{
				FOutput[0] = "Hmpf";
				
			}
			

			
			
		}
		
		public object ByteArrayToObject(byte[] buffer)
		{ 
		    BinaryFormatter binaryFormatter = new BinaryFormatter(); // Create new BinaryFormatter
		    MemoryStream memoryStream = new MemoryStream(buffer);    // Convert buffer to memorystream
		    return binaryFormatter.Deserialize(memoryStream);        // Deserialize stream to an object
		}   
		
		public object ReadObjectFromMMF(bool readFile, string filePath)
		{
			string name="vvvvulture.vulture";
			try{
				if (readFile){
				    using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open))
				    {
				        using (MemoryMappedViewAccessor mmfReader = mmf.CreateViewAccessor())
				        {
				            byte[] buffer = new byte[mmfReader.Capacity];
				            mmfReader.ReadArray<byte>(0, buffer, 0, buffer.Length);
			
			            return ByteArrayToObject(buffer);
				        }
				    }			
				}else{
					 using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(name)) //MemoryMappedFile.CreateFromFile(mmfFile, FileMode.Open))
				    {
				        using (MemoryMappedViewAccessor mmfReader = mmf.CreateViewAccessor())
				        {
				            byte[] buffer = new byte[mmfReader.Capacity];
				            mmfReader.ReadArray<byte>(0, buffer, 0, buffer.Length);
			
			            return ByteArrayToObject(buffer);
				        }
				    }
				}
			}
			catch {return null;}
	
			
		} 
		
				//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Mesh CreateMesh(Device device)
		{
			var meshPin = FMeshOut;
			meshPin.SliceCount = FInd.Length;
			
			int faceCount = FInd.Length;

            Mesh mesh;
            MeshFlags flags = 0;
            if (faceCount > ushort.MaxValue)
                flags = MeshFlags.Use32Bit;
            
            if (device is DeviceEx)
            {
                mesh = new Mesh(device, faceCount, FPos.Length, flags | MeshFlags.Dynamic, VertexFormat.PositionNormal | VertexFormat.Texture1);
            }
            else
            {
                mesh = new Mesh(device, faceCount , FPos.Length, flags | MeshFlags.Managed, VertexFormat.PositionNormal | VertexFormat.Texture1);
            }

			
			var vertices = mesh.LockVertexBuffer(LockFlags.None);
			for (int v = 0; v < FPos.Length; v++) {
				vertices.Write(FPos[v]);
				vertices.Write(FNorm[v]);
				vertices.Write(FTexcd[v]);
			}
			mesh.UnlockVertexBuffer();
			
			Vector3 min = new Vector3(0), max = new Vector3(0), newmin, newmax;
//			FCen.SliceCount=FInd.SliceCount;
			
			var attributes = mesh.LockAttributeBuffer(LockFlags.None);			
			var indices = mesh.LockIndexBuffer(LockFlags.None);
			for (int a = 0; a < FInd.Length; a++)
			{
			
					attributes.Write(a);
					if (flags == MeshFlags.Use32Bit)
					{
    					indices.Write((uint)(FInd[a].X));
    					indices.Write((uint)(FInd[a].Y));
    					indices.Write((uint)(FInd[a].Z));
					}
					else
					{
    					indices.Write((ushort)(FInd[a].X));
    					indices.Write((ushort)(FInd[a].Y));
    					indices.Write((ushort)(FInd[a].Z));
					}
					
					Vector3 pos = FPos[(int)FInd[a].X];

						Vector3.Minimize(ref min, ref pos,out newmin);
						Vector3.Maximize(ref max, ref pos,out newmax);
						min = newmin;
						max = newmax;
					
					pos = FPos[(int)FInd[a].Y];
					Vector3.Minimize(ref min, ref pos,out newmin);
					Vector3.Maximize(ref max, ref pos,out newmax);
					min = newmin;
					max = newmax;
					pos = FPos[(int)FInd[a].Z];
					Vector3.Minimize(ref min, ref pos,out newmin);
					Vector3.Maximize(ref max, ref pos,out newmax);
					min = newmin;
					max = newmax;
				
				
//				FCen[a]=new Vector3D((min.X+max.X)/2.0,(min.Y+max.Y)/2.0,(min.Z+max.Z)/2.0);
			}
			mesh.UnlockAttributeBuffer();
			mesh.UnlockIndexBuffer();

			return mesh;
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its mesh, here you can alter the data of the mesh
		protected override void UpdateMesh(Mesh mesh)
		{
			//do something with the mesh data
			var vertices = mesh.LockVertexBuffer(LockFlags.None);
			int vCount = Math.Max(FPos.Length, FNorm.Length);
			vCount = Math.Max(vCount, FTexcd.Length);
			
			if (mesh.VertexCount == vCount)
			{
				for (int i = 0; i < mesh.VertexCount; i++) {
					vertices.Write(FPos[i]);
					vertices.Write(FNorm[i]);
					vertices.Write(FTexcd[i]);
				}
			}
			else
			{
				reinit = true;
			}
			mesh.UnlockVertexBuffer();
		}
	}
}
