#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * You must attribute the work in the manner specified by the author or licensor:
//          - If you find this component useful a link to http://www.newtonsoft.com would be appreciated.
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
#endregion

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if SILVERLIGHT
[assembly: AssemblyTitle("Newtonsoft Json.NET Silverlight")]
#elif PocketPC
[assembly: AssemblyTitle("Newtonsoft Json.NET Compact")]
#elif NET20
[assembly: AssemblyTitle("Newtonsoft Json.NET .NET 2.0")]
[assembly: AllowPartiallyTrustedCallers]
#else
[assembly: AssemblyTitle("Newtonsoft Json.NET")]
[assembly: AllowPartiallyTrustedCallers]
#endif

#if !SIGNED

#if SILVERLIGHT
[assembly: InternalsVisibleTo("Newtonsoft.Json.Tests.Silverlight")]
#elif PocketPC
[assembly: InternalsVisibleTo("Newtonsoft.Json.Tests.Compact")]
#elif NET20
[assembly: InternalsVisibleTo("Newtonsoft.Json.Tests.Net20")]
#else
[assembly: InternalsVisibleTo("Newtonsoft.Json.Tests")]
#endif

#endif



[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Newtonsoft")]
[assembly: AssemblyProduct("Newtonsoft Json.NET")]
[assembly: AssemblyCopyright("Copyright � Newtonsoft 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM componenets.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9ca358aa-317b-4925-8ada-4a29e943a363")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("3.5.0.0")]
#if !PocketPC
[assembly: AssemblyFileVersion("3.5.0.0")]
#endif