using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Obfuscator.A;
using Obfuscator.A.P;
//using Obfuscator.A.P.Packers;
using Obfuscator.A.P.AddRandoms;
using Obfuscator.A.P.MetaStrip;
using Obfuscator.A.P.StringEncrypt;
using Obfuscator.A.Utils;
using System;
using System.IO;

internal class Program
{
    public static bool IsWinForms = false;
    public static string FileExtension = string.Empty;

    private static void Main()
    {
       // 获.获址();  Cracked by adding // ಠ_ಠ

        Console.Title = Reference.Name + " v" + Reference.Version;

        Console.WriteLine(@"
 ██████╗ ██████╗ ███████╗██╗   ██╗███████╗ ██████╗ █████╗ ████████╗ ██████╗ ██████╗  
██╔═══██╗██╔══██╗██╔════╝██║   ██║██╔════╝██╔════╝██╔══██╗╚══██╔══╝██╔═══██╗██╔══██╗
██║   ██║██████╔╝█████╗  ██║   ██║███████╗██║     ███████║   ██║   ██║   ██║██████╔╝
██║   ██║██╔══██╗██╔══╝  ██║   ██║╚════██║██║     ██╔══██║   ██║   ██║   ██║██╔══██╗
╚██████╔╝██████╔╝██║     ╚██████╔╝███████║╚██████╗██║  ██║   ██║   ╚██████╔╝██║  ██║
 ╚═════╝ ╚═════╝ ╚═╝      ╚═════╝ ╚══════╝ ╚═════╝╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝
");

        Console.WriteLine("========================================================================");
        Console.WriteLine("Drag & drop your file here:");
        Console.WriteLine("========================================================================");

        string file = Console.ReadLine().Replace("\"", "");
        FileExtension = Path.GetExtension(file);

        if (FileExtension.Contains("exe"))
        {
            Console.WriteLine();
            Console.WriteLine("Is your file a Windows Forms application?");
            Console.WriteLine("Type 'true' for WinForms, 'false' for Console:");
            IsWinForms = Convert.ToBoolean(Console.ReadLine());
        }

        ModuleDefMD module = ModuleDefMD.Load(file);
        string fileName = Path.GetFileNameWithoutExtension(file);

        Console.WriteLine();
        Console.WriteLine("========================================================================");
        Console.WriteLine("Loaded Assembly     : " + module.Assembly.FullName);
        Console.WriteLine("Has Resources       : " + module.HasResources);
        if (FileExtension.Contains("exe"))
        {
            Console.WriteLine("Is Windows Forms    : " + IsWinForms);
        }
        Console.WriteLine("File Extension      : " + FileExtension.Replace(".", "").ToUpper());
        Console.WriteLine("========================================================================");
        Console.WriteLine();

        Execute(module);

        Console.WriteLine("Saving obfuscated file to Desktop...");

        var opts = new ModuleWriterOptions(module);
        opts.Logger = DummyLogger.NoThrowInstance;

        string outputPath = $@"C:\Users\{Environment.UserName}\Desktop\{fileName}_obf{FileExtension}";
        module.Write(outputPath, opts);

        Console.WriteLine("Obfuscation complete.");
        Console.WriteLine("Output file saved as: " + outputPath);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void Execute(ModuleDefMD module)
    {
        Console.WriteLine("Starting Obfuscation Steps...");
        Console.WriteLine();

        Console.WriteLine("[1] Renamer                 : Applying...");
        Renamer.Execute(module);

        Console.WriteLine("[2] RandomOutlinedMethods  : Applying...");
        RandomOutlinedMethods.Execute(module);

        Console.WriteLine("[3] MetaStrip              : Applying...");
        MetaStrip.Execute(module);

        Console.WriteLine("[4] Junk                   : Applying...");
        Junk.Execute(module);

        Console.WriteLine("[5] StringEncryption       : Applying...");
        StringEncryption.Execute(module);

        Console.WriteLine("[6] OBAdder                : Applying...");
        OBAdder.Execute(module);

      //  Console.WriteLine("[7] Packer (Anti-Reverse)  : Applying...");
       // Packer.Execute(module); 

        Console.WriteLine();
        Console.WriteLine("Obfuscation Complete!");
    }
}
