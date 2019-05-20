using System;
using Microsoft.Win32;

namespace Wes.Utilities
{
	public static class DotnetDetection
	{
		public static bool IsDotnet35SP1Installed()
		{
			using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5")) {
				return key != null && (key.GetValue("SP") as int?) >= 1;
			}
		}
	
		public static bool IsDotnet40Installed()
		{
			return true; 
		}
		
		public static bool IsDotnet45Installed()
		{
			return GetDotnet4Release() >= 378389;
		}
		
		public static bool IsDotnet451Installed()
		{
			// According to: http://blogs.msdn.com/b/astebner/archive/2013/11/11/10466402.aspx
			// 378675 is .NET 4.5.1 on Win8
			// 378758 is .NET 4.5.1 on Win7
			return GetDotnet4Release() >= 378675;
		}
		
		public static bool IsDotnet452Installed()
		{
			// 379893 is .NET 4.5.2 on my Win7 machine
			return GetDotnet4Release() >= 379893;
		}
		
		public static bool IsDotnet46Installed()
		{
			// 393273 is .NET 4.6 on my Win7 machine with VS 2015 RC installed
			return GetDotnet4Release() >= 393273;
		}
		
		static int? GetDotnet4Release()
		{
			using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full")) {
				if (key != null)
					return key.GetValue("Release") as int?;
			}
			return null;
		}
		
		public static bool IsBuildTools2013Installed()
		{
			// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DevDiv\BuildTools\Servicing\12.0
			using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\BuildTools\Servicing\12.0\MSBuild")) {
				return key != null && key.GetValue("Install") as int? >= 1;
			}
		}
	}
}
