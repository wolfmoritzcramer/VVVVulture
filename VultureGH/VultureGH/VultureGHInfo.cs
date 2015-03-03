using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace VultureGH
{
    public class VultureGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "VultureGH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("5c535af6-db13-40c3-b88c-9e985cc78d5d");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Wolf Moritz Cramer";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "info@moritzcramer.com";
            }
        }
    }
}
