using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Text;

namespace Obfuscator.A.P
{
    internal class StringObfuscate
    {
        private static readonly byte[] xorKey = Encoding.UTF8.GetBytes("NIGGER");

        public static void Execute(ModuleDefMD module)
        {
            InjectDecrypt(module);
            ObfuscateStrings(module);
            ObfuscateNames(module);
            ObfuscateMethods(module);
        }

        private static void ObfuscateStrings(ModuleDefMD module)
        {
            var decryptMethod = module.GlobalType.FindMethod("DecryptString");

            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var instrs = method.Body.Instructions;
                    for (int i = 0; i < instrs.Count; i++)
                    {
                        var instr = instrs[i];
                        if (instr.OpCode == OpCodes.Ldstr)
                        {
                            string original = (string)instr.Operand;
                            if (string.IsNullOrEmpty(original)) continue;

                            string encrypted = EncryptString(original);
                            instr.Operand = encrypted;

                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Call, decryptMethod));
                            i++;
                        }
                    }
                }
            }
        }

        private static void ObfuscateNames(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                if (type == module.GlobalType)
                    continue;

                type.Name = GenerateName();

                foreach (var method in type.Methods)
                {
                    if (method.IsConstructor) continue;
                    method.Name = GenerateName();
                }

                foreach (var field in type.Fields)
                {
                    field.Name = GenerateName();
                }
            }
        }

        private static void ObfuscateMethods(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var body = method.Body;
                    var instrs = body.Instructions;

                    if (body.Variables.Count > 0)
                    {
                        foreach (var local in body.Variables)
                        {
                            local.Name = GenerateName(8);
                        }
                        body.InitLocals = true;
                    }

                    for (int i = 0; i < instrs.Count; i++)
                    {
                        var instr = instrs[i];

                        if (instr.OpCode == OpCodes.Ldc_I4)
                        {
                            int originalValue = (int)instr.Operand;
                            int xorKeyInt = 0x55AA55AA; 
                            int encodedValue = originalValue ^ xorKeyInt;

                            instrs[i] = Instruction.Create(OpCodes.Ldc_I4, encodedValue);


                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, xorKeyInt));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Xor));
                            i += 2;
                        }

                        if (rng.NextDouble() < 0.09) 
                        {
                            instrs.Insert(i, Instruction.Create(OpCodes.Nop));
                            i++;
                        }
                        else if (rng.NextDouble() < 0.02) 
                        {
                            var skip = Instruction.Create(OpCodes.Nop);
                            instrs.Insert(i, Instruction.Create(OpCodes.Ldc_I4_0));
                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Brtrue, skip));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Nop));
                            instrs.Insert(i + 3, skip);
                            i += 3;
                        }
                    }
                }
            }
        }

        private static string GenerateName(int length = 100)
        {
            const string chars = "贪<=貪,员<=員,贴<=貼康熙字典漢語大字典爾雅";
            var random = new Random();
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[random.Next(chars.Length)]);
            return sb.ToString();
        }

        private static string EncryptString(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= xorKey[i % xorKey.Length];
            return Convert.ToBase64String(bytes);
        }

        private static void InjectDecrypt(ModuleDefMD module)
        {
            var globalType = module.GlobalType;
            if (globalType.FindMethod("DecryptString") != null)
                return;

            var decryptMethod = new MethodDefUser(
                "DecryptString",
                MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String),
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig);

            globalType.Methods.Add(decryptMethod);

            var body = new CilBody();
            decryptMethod.Body = body;

            var instrs = body.Instructions;

            var corLibTypes = module.CorLibTypes;

             Locals: byte[] bytes, int i
            body.Variables.Add(new Local(new SZArraySig(corLibTypes.Byte)));
            body.Variables.Add(new Local(corLibTypes.Int32));
            body.InitLocals = true;

             Decrypt method implementation (XOR key must be same as xorKey field)
            instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
            instrs.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) }))));
            instrs.Add(Instruction.Create(OpCodes.Stloc_0));  bytes

            instrs.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instrs.Add(Instruction.Create(OpCodes.Stloc_1));  i = 0

            var loopStart = Instruction.Create(OpCodes.Ldloc_1);
            instrs.Add(loopStart);

            instrs.Add(Instruction.Create(OpCodes.Ldloc_0));
            instrs.Add(Instruction.Create(OpCodes.Ldlen));
            instrs.Add(Instruction.Create(OpCodes.Conv_I4));
            instrs.Add(Instruction.Create(OpCodes.Clt));

            var loopBodyStart = Instruction.Create(OpCodes.Nop);
            instrs.Add(Instruction.Create(OpCodes.Brfalse_S, loopBodyStart));

             bytes[i] ^= xorKey[i % xorKey.Length]
            instrs.Add(Instruction.Create(OpCodes.Ldloc_0));
            instrs.Add(Instruction.Create(OpCodes.Ldloc_1));
            instrs.Add(Instruction.Create(OpCodes.Ldelem_U1));
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4, xorKey.Length));
            instrs.Add(Instruction.Create(OpCodes.Ldloc_1));
            instrs.Add(Instruction.Create(OpCodes.Rem));
            instrs.Add(Instruction.Create(OpCodes.Conv_U1));
            instrs.Add(Instruction.Create(OpCodes.Ldelem_U1, CreateXorKeyField(module)));
            instrs.Add(Instruction.Create(OpCodes.Xor));
            instrs.Add(Instruction.Create(OpCodes.Conv_U1));
            instrs.Add(Instruction.Create(OpCodes.Ldloc_0));
            instrs.Add(Instruction.Create(OpCodes.Ldloc_1));
            instrs.Add(Instruction.Create(OpCodes.Stelem_I1));

             i++
            instrs.Add(Instruction.Create(OpCodes.Ldloc_1));
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instrs.Add(Instruction.Create(OpCodes.Add));
            instrs.Add(Instruction.Create(OpCodes.Stloc_1));

            instrs.Add(Instruction.Create(OpCodes.Br_S, loopStart));
            instrs.Add(loopBodyStart);

             return Encoding.UTF8.GetString(bytes);
            instrs.Add(Instruction.Create(OpCodes.Ldloc_0));
            instrs.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
            instrs.Add(Instruction.Create(OpCodes.Ldloc_0));
            instrs.Add(Instruction.Create(OpCodes.Callvirt, module.Import(typeof(Encoding).GetMethod("GetString", new[] { typeof(byte[]) }))));

            instrs.Add(Instruction.Create(OpCodes.Ret));
        }

        private static FieldDef CreateXorKeyField(ModuleDefMD module)
        {
            var globalType = module.GlobalType;
            var field = new FieldDefUser("xorKey",
                new FieldSig(new SZArraySig(module.CorLibTypes.Byte)),
                FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

            globalType.Fields.Add(field);

             Initialize field in cctor
            var cctor = globalType.FindOrCreateStaticConstructor();
            var il = cctor.Body.Instructions;

            il.Insert(0, Instruction.Create(OpCodes.Ldtoken, field));
            il.Insert(1, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray"))));
            il.Insert(2, Instruction.Create(OpCodes.Ret));

            return field;
        }

        private static Random rng = new Random();

        private static string GenerateName(int length = 100)
        {
            const string chars = "贪<=貪,员<=員,贴<=貼康熙字典漢語大字典爾雅";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[rng.Next(chars.Length)]);
            return sb.ToString();
        }

        private static string EncryptString(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= xorKey[i % xorKey.Length];
            return Convert.ToBase64String(bytes);
        }
    }
}
