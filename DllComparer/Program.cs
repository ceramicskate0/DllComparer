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
        internal static List<Module> DLL_List = new List<Module>();
        internal static List<Processes_W_DLLs> Processes_Info = new List<Processes_W_DLLs>();
        private static List<string> Program_Args = new List<string>();
        internal static bool ShowErrors = false;
        public static void Main(string[] args)
        {

            if (IsAdministrator() == false)
            {
                Console.WriteLine("[WARNING] The current account running this app is not an admin.\n[About the Warning]This means that it will only be able to see DLLs in the same user context that the app is running in.");
                Thread.Sleep(3000);
            }
            CollectProcessAndDLLInfo();
            ParseArgs(args);
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
                        case "-e"://dont show errors
                            {
                                ShowErrors = false;
                                break;
                            }
                        case "-t"://Dump all DLL's and count how many times each seen for a period of time
                            {
                                //CHECK_If_App_Has_Run_To_Long(Convert.ToInt32(Program_Args.ElementAt(x + 1)));
                                break;
                            }
                        case "-h":
                            {
                                HelpMenu();
                                break;
                            }
                        case "-s"://Show all process and show their Dll's
                            {
                                ShowDLLTree();
                                break;
                            }
                        case "-d"://Dump all DLL's and count how many times each seen
                            {
                                CountOccurances();
                                break;
                            }
                        case "-f"://Search/Find for Process name, PID, or DLL name
                            {
                                SearchDLL(Program_Args.ElementAt(x+1));
                                break;
                            }
                        default:
                            {
                                //HelpMenu();
                                break;
                            }
                    }
                }
            }
        }
        internal static void HelpMenu()
        {
            Console.WriteLine(@"
            created by: Ceramicskate0

            Commands Menu:
            -h
            Show Help Menu

            -d 
            Dump all the DLL's seen with the count of how many times each was seen.

            -s
            Dump all process and show their Dll's

            -e
            Show errors

            -f {SearchTerm}
            Search for Process name, PID, or DLL name

            ");
           /* 
            -t {# of seconds to run, ie 30}
            Look at all DLL's and count how many times each seen for a period of time
            */
        }
        internal static void SearchDLL(string Obj)
        {
            for (int x = 0; x < Processes_Info.Count; ++x)
            {
               bool ProcessNamePrinted = false;
                for (int y = 0; y < Processes_Info.ElementAt(x).DLL_List.Count; ++y)
                {
                    if (Processes_Info.ElementAt(x).ProcessName.ToLower().Contains(Obj.ToLower())==true || Processes_Info.ElementAt(x).PID.ToString().Contains(Obj)==true || Processes_Info.ElementAt(x).DLL_List.ElementAt(y).ModuleName.ToString().ToLower().Contains(Obj.ToLower())==true)
                    {
                        if (ProcessNamePrinted == false)
                        {
                            Console.WriteLine("\n"+Processes_Info.ElementAt(x).ProcessName);
                            ProcessNamePrinted = true;
                        }
                            Console.WriteLine(@"-" + Processes_Info.ElementAt(x).DLL_List.ElementAt(y).ModuleName);
                    }
                }
            }
        }
        internal static void ShowDLLTree()
        {
            for (int x=0;x< Processes_Info.Count;++x)
            {
                Console.WriteLine(Processes_Info.ElementAt(x).ProcessName);
                for (int y = 0; y < Processes_Info.ElementAt(x).DLL_List.Count; ++y)
                {
                    Console.WriteLine(@"---" + Processes_Info.ElementAt(x).DLL_List.ElementAt(y).ModuleName);
                }
                Console.WriteLine("----------------------");
            }
        }
        internal static void CountOccurances()
        {
            Dictionary<string, int> CountDLL = new Dictionary<string, int>();
            foreach (var dll in DLL_List)
            {
                try
                {
                    if (CountDLL.ContainsKey(dll.ModuleName) == false)
                    {
                        CountDLL.Add(dll.ModuleName, 1);
                    }
                    else
                    {
                        CountDLL[dll.ModuleName] = CountDLL[dll.ModuleName] + 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            CountDLL = CountDLL.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            Console.WriteLine("List of Modules in CSV format");
            for (int x = 0; x < CountDLL.Count; ++x)
            {
                Console.WriteLine(CountDLL.ElementAt(x).Key + "," + CountDLL.ElementAt(x).Value);
            }
        }
        internal static void CollectProcessAndDLLInfo()
        {
            for (int x = 0; x < RunningProcesses.Count; ++x)
            {
                try
                {
                    Processes_W_DLLs tmp = new Processes_W_DLLs();

                    //add all DLLs in the process to the Master DLL list
                    //RunningProcesses.ElementAt(x).Modules.InnerList
                    Unique_DLL_List.AddRange(CollectModules(RunningProcesses.ElementAt(x)));
                    DLL_List.AddRange(CollectModules(RunningProcesses.ElementAt(x)));
                    //mkae sure list is unique
                    Unique_DLL_List = Unique_DLL_List.Distinct().ToList();

                    //Create object that contains Process info and DLL info needed for later analysis
                    tmp.DLL_List.AddRange(CollectModules(RunningProcesses.ElementAt(x)));
                    tmp.PID = RunningProcesses.ElementAt(x).Id;
                    tmp.ProcessName = RunningProcesses.ElementAt(x).MainModule.FileName;
                    Processes_Info.Add(tmp);
                }
                catch (Exception e)
                {
                    if (ShowErrors)
                    {
                        Console.WriteLine("-----------------");
                        Console.WriteLine("[!ERROR!] App unable to evaluate the following process.\nProcess Name:" + RunningProcesses.ElementAt(x).ProcessName + "\nPID:" + RunningProcesses.ElementAt(x).Id + "\nDue to:" + e.Message.ToString());
                    }
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

                    string moduleName = Path.GetFullPath(moduleFilePath.ToString());
                    
                    Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                    Native.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation, (uint)(IntPtr.Size * (modulePointers.Length)));

                    // Convert to a normalized module and add it to our list
                    Module module = new Module(moduleName, moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
                    collectedModules.Add(module);
                }
            }

            return collectedModules;
        }
        private static void CHECK_If_App_Has_Run_To_Long(int WaitTimeSeconds)
        {
            RunningProcesses = Process.GetProcesses().ToList();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (watch.Elapsed.Seconds < WaitTimeSeconds)
            {

            }
            watch.Stop();
            var elapsedTime = watch.Elapsed;        
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
