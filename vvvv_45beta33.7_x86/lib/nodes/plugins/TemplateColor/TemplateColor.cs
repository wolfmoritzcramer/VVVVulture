#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template",
	            Category = "Color",
	            Help = "Basic template with one color in/out",
	            Tags = "")]
	#endregion PluginInfo
	public class Template : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultColor = new double[] { 0.1, 0.2, 0.3, 1.0 })]
        public ISpread<RGBAColor> FInput;

		[Output("Output")]
        public ISpread<RGBAColor> FOutput;

		[Import()]
        public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
				FOutput[i] = VColor.Complement(FInput[i]);
		}
	}
}
