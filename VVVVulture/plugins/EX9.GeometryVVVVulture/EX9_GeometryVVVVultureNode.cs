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
	[PluginInfo(Name = "VVVVulture", 
				Category = "EX9.Geometry", 
				Help = "Node to communicate with Grasshopper via Vulture", 
				Tags = "",
				Author = "Wolf Moritz Cramer | wolfmoritzcramer.de"
	)]
	#endregion PluginInfo
	public class VVVVultureNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Enabled")]
		public ISpread<Boolean> Fenabled;
		
		[Input("Read from File")]
		public ISpread<Boolean> FreadFile;
		
		[Input("File", StringType = StringType.Filename)]
		public IDiffSpread<string> FfilePath;		
	
		[Output("Output")]
		public ISpread<string> FOutput;
		
		

		[Output("Vertices")]
		 ISpread<ISpread<Vector3>> Fvertices3;
		[Output("Normals")]
		 ISpread<ISpread<Vector3>> Fnormals3;
		[Output("TexCoord")]
		 ISpread<ISpread<Vector2>> Ftex2;
		[Output("Indices")]
		 ISpread<ISpread<Vector3>> Findices;
		[Output("RhinoCam")]
		 ISpread<Vector3> Frhinocam;
		[Output("Points")]
		 ISpread<Vector3> Fpoints;
		[Output("Integers")]
		ISpread<int> Fintegers;
		[Output("Strings")]
		ISpread<string> Fstrings;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		vulture Vulture;
		bool read = true;
		bool init = true;
				   
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (Fenabled[0]){
				if (read){
					try{				
						// Open the file
						Vulture = ReadObjectFromMMF(FreadFile[0],FfilePath[0]) as vulture;	
						
						// Init FileSystemWatcher
						if (init || FfilePath.IsChanged){initWatcher(FfilePath[0]);}
						
						if (Vulture != null){
		
							// Get the data
						//	FOutput[0] = "Look! There he is! And he has "  + Vulture.meshes.Length + " Mesh(es) for you!";
							Fvertices3.SliceCount	=	Vulture.meshes.Length;
							Fnormals3.SliceCount	=	Vulture.meshes.Length;
							Ftex2.SliceCount		=	Vulture.meshes.Length;
							Findices.SliceCount		=	Vulture.meshes.Length;
							
							for (int i = 0; i < Vulture.meshes.Length; i++){
								
								Fvertices3[i]	=	Vulture.meshes[i].verticesVec3.ToSpread();
								Fnormals3[i]	=	Vulture.meshes[i].normalsVec3.ToSpread();						
								Ftex2[i]		=	Vulture.meshes[i].texVec2.ToSpread();			
								Findices[i]		= 	Vulture.meshes[i].indicesVec3.ToSpread();
									
							}
						
							Frhinocam.SliceCount = Vulture.cameraPoints.Length;
							for (int i = 0; i < Vulture.cameraPoints.Length; i++){
								Frhinocam[i] = Vulture.cameraPoints[i];								
							}
							
							Fpoints.SliceCount = Vulture.points.Length;
							for (int i = 0; i < Vulture.points.Length; i++){
								Fpoints[i] = Vulture.points[i];								
							}
							Fintegers.SliceCount = Vulture.integers.Length;
							for (int i = 0; i < Vulture.integers.Length; i++){
								Fintegers[i] = Vulture.integers[i];								
							}
							Fstrings.SliceCount = Vulture.strings.Length;
							for (int i = 0; i < Vulture.strings.Length; i++){
								Fstrings[i] = Vulture.strings[i];								
							}
							

							//FOutput[0] = Vulture.lightPoints[0].X.ToString();
							
							read = false;
						}
						else{
							FOutput[0] = "Seems the vulture flew away!";
						}
					}
					catch{}
				}	
				
			}
		}
		
		public void initWatcher(string filePath){
			
			   FileSystemWatcher _fileWatcher = new FileSystemWatcher();
			  _fileWatcher.Path = Path.GetDirectoryName(filePath);
			  _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
			  _fileWatcher.Filter = Path.GetFileName(filePath);
			  _fileWatcher.Changed += new FileSystemEventHandler(OnChanged);
			  _fileWatcher.EnableRaisingEvents = true;
			init = false;
		}
		
		private void OnChanged(object source, FileSystemEventArgs e)
		{	
			read = true;
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
		
	}
}
