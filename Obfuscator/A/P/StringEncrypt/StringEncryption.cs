using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Obfuscator.A.P.StringEncrypt
{
    internal class StringEncryption
    {
        private static readonly Random rng = new Random();
        private static readonly Dictionary<string, MethodDef> stringMethodMap = new Dictionary<string, MethodDef>();
        private static MethodDef decryptMethod;
        private static bool hasDecryptMethod = false;

        public static void Execute(ModuleDefMD module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            var globalType = module.GlobalType;
            var cctor = globalType.FindOrCreateStaticConstructor();

            int count = 0;

            foreach (var type in module.Types.ToList())
            {
                foreach (var method in type.Methods.ToList())
                {
                    if (!method.HasBody || method.Body == null)
                        continue;

                    var instrs = method.Body.Instructions;
                    for (int i = 0; i < instrs.Count; i++)
                    {
                        if (instrs[i].OpCode == OpCodes.Ldstr)
                        {
                            string original = instrs[i].Operand as string;
                            if (string.IsNullOrEmpty(original))
                                continue;

                            if (!stringMethodMap.TryGetValue(original, out MethodDef stringGetter))
                            {
                                stringGetter = CreateStringMethod(original, module);
                                globalType.Methods.Add(stringGetter);
                                stringMethodMap[original] = stringGetter;
                                count++;
                            }

                            instrs[i] = Instruction.Create(OpCodes.Call, stringGetter);
                        }
                    }
                }
            }

            Console.WriteLine($"  [STRINGENCRYPTION] Obfuscated {count} unique strings.");
        }

        private static MethodDef CreateStringMethod(string value, ModuleDef module)
        {
            if (IsUrl(value))
            {
                if (!hasDecryptMethod)
                {
                    CreateDecryptMethod(module);
                    hasDecryptMethod = true;
                }
                return CreateEncryptedMethod(value, module);
            }
            else
            {
                return CreatePlainMethod(value, module);
            }
        }

        private static bool IsUrl(string s) =>
            !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^https?:\/\/", RegexOptions.IgnoreCase);

        private static MethodDef CreatePlainMethod(string value, ModuleDef module)
        {
            string methodName = GenerateRandomName(10);

            var method = new MethodDefUser(methodName,
                MethodSig.CreateStatic(module.CorLibTypes.String),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Body = new CilBody()
            };

            method.Body.Instructions.Add(OpCodes.Ldstr.ToInstruction(value));
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

            return method;
        }

        private static MethodDef CreateEncryptedMethod(string value, ModuleDef module)
        {
            string methodName = GenerateRandomName(10);
            byte key = (byte)rng.Next(1, 256);
            string encrypted = XorEncryptDecrypt(value, key);

            var method = new MethodDefUser(methodName,
                MethodSig.CreateStatic(module.CorLibTypes.String),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Body = new CilBody()
            };

            var body = method.Body.Instructions;

            body.Add(OpCodes.Ldstr.ToInstruction(encrypted));
            body.Add(OpCodes.Ldc_I4.ToInstruction((int)key));
            body.Add(OpCodes.Call.ToInstruction(decryptMethod));
            body.Add(OpCodes.Ret.ToInstruction());

            return method;
        }

        private static void CreateDecryptMethod(ModuleDef module)
        {
            var globalType = module.GlobalType;
            string decryptName = "Decrypt" + GenerateRandomName(8);

            decryptMethod = new MethodDefUser(decryptName,
                MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String, module.CorLibTypes.Int32),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Body = new CilBody { InitLocals = true }
            };

            var instrs = decryptMethod.Body.Instructions;

            var bytesLocal = new Local(new SZArraySig(module.CorLibTypes.Byte));
            var iLocal = new Local(module.CorLibTypes.Int32);
            var lengthLocal = new Local(module.CorLibTypes.Int32);

            decryptMethod.Body.Variables.Add(bytesLocal);
            decryptMethod.Body.Variables.Add(iLocal);
            decryptMethod.Body.Variables.Add(lengthLocal);

            instrs.Add(OpCodes.Call.ToInstruction(module.Import(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
            instrs.Add(OpCodes.Ldarg_0.ToInstruction()); 
            instrs.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(Encoding).GetMethod("GetBytes", new[] { typeof(string) }))));
            instrs.Add(OpCodes.Stloc_0.ToInstruction()); 
            instrs.Add(OpCodes.Ldloc_0.ToInstruction());
            instrs.Add(OpCodes.Ldlen.ToInstruction());
            instrs.Add(OpCodes.Conv_I4.ToInstruction());
            instrs.Add(OpCodes.Stloc_2.ToInstruction());
            instrs.Add(OpCodes.Ldc_I4_0.ToInstruction());
            instrs.Add(OpCodes.Stloc_1.ToInstruction());
            Instruction loopCheck = Instruction.Create(OpCodes.Ldloc_1);
            instrs.Add(loopCheck);
            instrs.Add(OpCodes.Ldloc_2.ToInstruction());
            instrs.Add(OpCodes.Clt.ToInstruction());
            Instruction loopEnd = Instruction.Create(OpCodes.Nop);
            instrs.Add(OpCodes.Brfalse_S.ToInstruction(loopEnd));
            instrs.Add(OpCodes.Ldloc_0.ToInstruction());
            instrs.Add(OpCodes.Ldloc_1.ToInstruction());
            instrs.Add(OpCodes.Ldelem_U1.ToInstruction());
            instrs.Add(OpCodes.Ldarg_1.ToInstruction());
            instrs.Add(OpCodes.Xor.ToInstruction());
            instrs.Add(OpCodes.Conv_U1.ToInstruction());
            instrs.Add(OpCodes.Ldloc_0.ToInstruction());
            instrs.Add(OpCodes.Ldloc_1.ToInstruction());
            instrs.Add(OpCodes.Stelem_I1.ToInstruction());
            instrs.Add(OpCodes.Ldloc_1.ToInstruction());
            instrs.Add(OpCodes.Ldc_I4_1.ToInstruction());
            instrs.Add(OpCodes.Add.ToInstruction());
            instrs.Add(OpCodes.Stloc_1.ToInstruction());
            instrs.Add(OpCodes.Br_S.ToInstruction(loopCheck));
            instrs.Add(loopEnd);
            instrs.Add(OpCodes.Call.ToInstruction(module.Import(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
            instrs.Add(OpCodes.Ldloc_0.ToInstruction());
            instrs.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(Encoding).GetMethod("GetString", new[] { typeof(byte[]) }))));
            instrs.Add(OpCodes.Ret.ToInstruction());

            globalType.Methods.Add(decryptMethod);
        }

        private static string XorEncryptDecrypt(string input, byte key)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= key;
            return Encoding.UTF8.GetString(bytes);
        }

        private static string GenerateRandomName(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char c = (char)rng.Next(0x4E00, 0x9FFF);  
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
