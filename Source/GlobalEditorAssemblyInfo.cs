using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("DeepConfig Editor")]

[assembly: AssemblyCompany("DeepPrinciple")]
[assembly: AssemblyCopyright("Copyright © 2001 DeepPrinciple")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
