using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Obfuscator.A.Utils
{
    internal class OBAdder
    {
        public static void Execute(ModuleDefMD module)
        {
            string value = "Obfuscated With QelFuscator";

            MethodDef globalCctor = module.GlobalType.FindOrCreateStaticConstructor();
            MethodDef globalMethod = CreateReturnMethodDef(value, module.GlobalType.Module);
            Console.WriteLine($"  [OBADDER] Adding method \"{globalMethod.Name}\" at top of global type...");
            module.GlobalType.Methods.Insert(0, globalMethod);

            foreach (TypeDef type in module.Types)
            {
                AddMethodToTypeRecursive(type, value);
            }
        }

        private static void AddMethodToTypeRecursive(TypeDef type, string value)
        {
            if (type.IsGlobalModuleType)
                return;

            MethodDef method = CreateReturnMethodDef(value, type.Module);

            Console.WriteLine($"  [OBADDER] Adding method \"{method.Name}\" at top of type \"{type.FullName}\"...");
            type.Methods.Insert(0, method);

            if (type.HasNestedTypes)
            {
                foreach (TypeDef nested in type.NestedTypes)
                {
                    AddMethodToTypeRecursive(nested, value);
                }
            }
        }

        private static MethodDef CreateReturnMethodDef(string value, ModuleDef module)
        {
            var corlibString = module.CorLibTypes.String;

            MethodDef newMethod = new MethodDefUser(
                "QelFuscator",
                MethodSig.CreateStatic(corlibString),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Body = new CilBody()
            };

            var instrs = newMethod.Body.Instructions;
            instrs.Add(Instruction.Create(OpCodes.Ldstr, value));
            instrs.Add(Instruction.Create(OpCodes.Ret));
            newMethod.Body.InitLocals = true;

            return newMethod;
        }
    }
}
