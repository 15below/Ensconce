
namespace NDjango 

open System.Reflection;

open System.Runtime.CompilerServices;

open System.Runtime.InteropServices;

#if FRAMEWORK40
module AssemblyInfo =
  [<Literal>] 
  let public Version = "4.0.0.1"
  [<Literal>]
  let public FileVersion = "4.0.0.1"
#else
module AssemblyInfo =
  [<Literal>] 
  let public Version = "2.0.0.1"
  [<Literal>]
  let public FileVersion = "2.0.0.1"
#endif

// General Information about an assembly is controlled through the following

// set of attributes. Change these attribute values to modify the information

// associated with an assembly.

[<assembly: AssemblyTitle("NDjango.Core")>]

[<assembly: AssemblyDescription("")>]

[<assembly: AssemblyConfiguration("")>]

[<assembly: AssemblyCompany("Hill30, Inc.")>]

[<assembly: AssemblyProduct("NDjango")>]

[<assembly: AssemblyCopyright("Copyright © Hill30, Inc. 2008-2013")>]

[<assembly: AssemblyTrademark("")>]

[<assembly: AssemblyCulture("")>]

 

// Setting ComVisible to false makes the types in this assembly not visible

// to COM components.  If you need to access a type in this assembly from

// COM, set the ComVisible attribute to true on that type.

//[<assembly: ComVisible(false)>]

 

// The following GUID is for the ID of the typelib if this project is exposed to COM

//[<assembly: Guid("69242052-144C-4F38-A91B-179D29D326A1")>]

 

// Version information for an assembly consists of the following four values:

//

//      Major Version

//      Minor Version

//      Build Number

//      Revision

//

// You can specify all the values or you can default the Build and Revision Numbers

// by using the ‘*’ as shown below:

[<assembly: AssemblyVersion(AssemblyInfo.Version)>]

[<assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)>]

()