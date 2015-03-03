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
	public class EX9_GeometryVVVVultureNode : IPluginEvaluate
	{
		#region fields & pins
				[Input("Enabled")]
		public ISpread<Boolean> Fenabled;
		
		[Input("Read from File")]
		public ISpread<Boolean> FreadFile;
		
		[Input("Path")]
		public ISpread<string> FfilePath;		
	
		[Output("Output")]
		public ISpread<string> FOutput;
		
		[Output("VertLentgth")]
		public ISpread<int> FOutVL;
		
		[Output("Vertices")]
		public ISpread<double> FOutDouble;
		
		[Output("Normals")]
		public ISpread<double> FOutNormals;
		
		[Output("Indices")]
		public ISpread<int> FOutIndices;
		[Output("Vertices3")]
		public ISpread<Vector3> Fvertices3;
		

		
//		[Output("NewData", DefaultBoolean = false)]
//		public ISpread<Boolean> NewData;
		

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		vulture Vulture;
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (Fenabled[0]){
				try{
					Vulture = ReadObjectFromMMF(FreadFile[0],FfilePath[0]) as vulture;				
				}
				catch{}
			}
			

			
			
		}
		public void alt()
		{
		if (Fenabled[0]){
			Vulture = ReadObjectFromMMF(FreadFile[0],FfilePath[0]) as vulture;
			
//				if (Vulture.meshes[0].vertices3.Length > 0){
//				Fvertices3.SliceCount = Vulture.meshes[0].vertices3.Length;}
			
			if (Vulture != null){
				FOutDouble.SliceCount = Vulture.meshes[0].vertices.Length*3;
				FOutNormals.SliceCount = Vulture.meshes[0].vertices.Length*3;
				FOutIndices.SliceCount = Vulture.meshes[0].indices.Length;
				FOutput[0] = Vulture.meshes[0].description;
				FOutVL[0] = Vulture.meshes[0].vertices.Length;
	
				//FLogger.Log(LogType.Debug, Vulture.meshes[0].vertices[1].x.ToString());
				for (int i = 0; i < Vulture.meshes[0].vertices.Length; i++)
				{
					FOutDouble[i*3] = Vulture.meshes[0].vertices[i].x;
					FOutDouble[i*3+1] = Vulture.meshes[0].vertices[i].y;
					FOutDouble[i*3+2] = Vulture.meshes[0].vertices[i].z;
				}
				for (int i = 0; i < Vulture.meshes[0].indices.Length; i++)
				{
					FOutIndices[i] = Vulture.meshes[0].indices[i];
				}
				for (int i = 0; i < Vulture.meshes[0].normals.Length; i++)
				{
					FOutNormals[i*3] = Vulture.meshes[0].normals[i].x;
					FOutNormals[i*3+1] = Vulture.meshes[0].normals[i].y;
					FOutNormals[i*3+2] = Vulture.meshes[0].normals[i].z;
				}
			}
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
				    using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath + name, FileMode.Open))
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
		
	}
}
