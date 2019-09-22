using System.Collections.Generic;
using System.Reflection;

namespace LiteDbExplorer.Wpf.Framework.FileAssociation
{
    public class ApplicationInfo
    {
        #region Constructors
        public ApplicationInfo(string company, string name, string title, string location)
        {
            Company = company;
            Name = name;
            Title = title;
            Location = location;
            SupportedExtensions = new List<string>();
        }

        public ApplicationInfo(Assembly assembly)
            : this(assembly.Company(), assembly.GetName().Name, assembly.Title(), assembly.Location)
        {
        }
        #endregion

        #region Properties
        public string Company { get; private set; }

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string Location { get; private set; }

        public List<string> SupportedExtensions { get; private set; }
        #endregion
    }
}