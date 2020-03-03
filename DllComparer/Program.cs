using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace DllComparer
{
    class Program
    {
        internal static List<Process> RunningProcesses = Process.GetProcesses().ToList();
        internal static List<Module> Unique_DLL_List = new List<Module>();
        internal static List<Processes_W_DLLs> ProcessDLL = new List<Processes_W_DLLs>();
        private static List<string> Program_Args = new List<string>();

        public static void Main(string[] args)
        {

            if (IsAdministrator()==false)
            {
                Console.WriteLine("[WARNING] The current account running this app is not an admin.\n[About the Warning]This means that it will only be able to see DLLs in the same user context that the app is running in.");
                Thread.Sleep(3000);
            }
            CollectProcessAndDLLInfo();

        }
        internal static void ParseArgs(string[] args)
        {
            Program_Args = Environment.GetCommandLineArgs().ToList();
            if (Program_Args.Count != 0)
            {
                for (int x = 0; x < Program_Args.Count; ++x)
                {
                    switch (Program_Args.ElementAt(x).ToLower())
                    {
                        case "?":
                            {
                                HelpMenu();
                                break;
                            }
                        case "-":
                            {
                                HelpMenu();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }
        internal static void HelpMenu()
        {

        }

        internal static void CollectProcessAndDLLInfo()
        {
            for (int x = 0; x < RunningProcesses.Count; ++x)
            {
                try
                {
                    Processes_W_DLLs tmp = new Processes_W_DLLs();

                    //add all DLLs in the process to the Master DLL list
                    Unique_DLL_List.AddRange(CollectModules(RunningProcesses.ElementAt(x)));
                    //mkae sure list is unique
                    Unique_DLL_List = Unique_DLL_List.Distinct().ToList();

                    //Create object that contains Process info and DLL info needed for later analysis
                    tmp.DLL_List.AddRange(CollectModules(RunningProcesses.ElementAt(x)));
                    tmp.PID = RunningProcesses.ElementAt(x).Id;
                    tmp.ProcessName = RunningProcesses.ElementAt(x).ProcessName;
                    ProcessDLL.Add(tmp);
                }
                catch (Exception e)
                {
                    Console.WriteLine("-----------------");
                    Console.WriteLine("[!ERROR!] App unable to evaluate the following process.\nProcess Name:" + RunningProcesses.ElementAt(x).ProcessName + "\nPID:" + RunningProcesses.ElementAt(x).Id + "\nDue to:" + e.Message.ToString());
                }
            }
        }
        internal static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        internal static List<Module> CollectModules(Process process)
        {
            //REF:https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
            List<Module> collectedModules = new List<Module>();

            IntPtr[] modulePointers = new IntPtr[0];
            int bytesNeeded = 0;

            // Determine number of modules
            if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                return collectedModules;
            }

            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalNumberofModules];

            // Collect modules from the process
            if (Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                for (int index = 0; index < totalNumberofModules; index++)
                {
                    StringBuilder moduleFilePath = new StringBuilder(1024);
                    Native.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (uint)(moduleFilePath.Capacity));

                    string moduleName = Path.GetFileName(moduleFilePath.ToString());
                    Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                    Native.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation, (uint)(IntPtr.Size * (modulePointers.Length)));

                    // Convert to a normalized module and add it to our list
                    Module module = new Module(moduleName, moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
                    collectedModules.Add(module);
                }
            }

            return collectedModules;
        }
    }


    public class Native
    {
        //REF:https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault = 0x0,
            ListModules32Bit = 0x01,
            ListModules64Bit = 0x02,
            ListModulesAll = 0x03,
        }

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);
    }

    public class Module
    {
        //REF:https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        public Module(string moduleName, IntPtr baseAddress, uint size)
        {
            this.ModuleName = moduleName;
            this.BaseAddress = baseAddress;
            this.Size = size;
        }

        public string ModuleName { get; set; }
        public IntPtr BaseAddress { get; set; }
        public uint Size { get; set; }
    }

    //Class used to do analysis 
    public class Processes_W_DLLs
    {
        public List<Module> DLL_List = new List<Module>();
        public int PID;
        public string ProcessName = "";
    }
}
