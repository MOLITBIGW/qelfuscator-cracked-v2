using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Obfuscator.A.Utils
{
    internal class Junk
    {
        private static Random rng = new Random();

        public static void Execute(ModuleDefMD module)
        {
            Console.WriteLine("[JUNK] Starting deep junk code injection...");

            foreach (var type in module.Types)
            {
                InjectJunkMethodsRecursive(type, 15);
            }

            Console.WriteLine("[JUNK] Finished junk code injection.");
        }

        private static void InjectJunkMethodsRecursive(TypeDef type, int count)
        {
            if (type.IsGlobalModuleType || type.Name.StartsWith("<"))
                return;

            for (int i = 0; i < count; i++)
            {
                var junkMethod = CreateAdvancedJunkMethod(type.Module, type);
                type.Methods.Insert(0, junkMethod);
            }

            if (type.HasNestedTypes)
            {
                foreach (var nested in type.NestedTypes)
                    InjectJunkMethodsRecursive(nested, count);
            }
        }

        private static MethodDef CreateAdvancedJunkMethod(ModuleDef module, TypeDef parentType)
        {
            string name = RandomUnicodeString(rng.Next(20, 40));
            var method = new MethodDefUser(
                name,
                MethodSig.CreateStatic(module.CorLibTypes.Void),
                dnlib.DotNet.MethodAttributes.Private | dnlib.DotNet.MethodAttributes.Static | dnlib.DotNet.MethodAttributes.HideBySig)
            {
                Body = new CilBody()
            };

            var body = method.Body;
            var instrs = body.Instructions;
            body.InitLocals = true;

            var locals = new List<Local>
            {
                new Local(module.CorLibTypes.Int32),
                new Local(module.CorLibTypes.Int32),
                new Local(module.CorLibTypes.String),
                new Local(module.CorLibTypes.Boolean),
                new Local(module.CorLibTypes.Object),
                new Local(module.CorLibTypes.String),
                new Local(module.CorLibTypes.Int32),
            };
            foreach (var local in locals)
                body.Variables.Add(local);

            var skipDead = Instruction.Create(OpCodes.Nop);
            var skipIf = Instruction.Create(OpCodes.Nop);
            var loopStart = Instruction.Create(OpCodes.Nop);
            var loopEnd = Instruction.Create(OpCodes.Nop);

            for (int i = 0; i < 20; i++)
                instrs.Add(Instruction.Create(OpCodes.Nop));

            instrs.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instrs.Add(Instruction.Create(OpCodes.Brfalse, skipDead));
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4, 999));
            instrs.Add(Instruction.Create(OpCodes.Pop));
            instrs.Add(skipDead);

            for (int i = 0; i < 30; i++)
            {
                instrs.Add(Instruction.Create(OpCodes.Ldstr, RandomUnicodeString(rng.Next(20, 40))));
                instrs.Add(Instruction.Create(OpCodes.Stloc, locals[2]));
                instrs.Add(Instruction.Create(OpCodes.Ldloc, locals[2]));
                instrs.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(string).GetMethod("ToUpper", Type.EmptyTypes))));
                instrs.Add(Instruction.Create(OpCodes.Pop));
                instrs.Add(Instruction.Create(OpCodes.Ldloc, locals[2]));
                instrs.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(string).GetProperty("Length").GetGetMethod())));
                instrs.Add(Instruction.Create(OpCodes.Pop));
            }

            string junkStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(RandomChineseString(rng.Next(20, 40))));
            instrs.Add(Instruction.Create(OpCodes.Ldstr, junkStr));
            instrs.Add(Instruction.Create(OpCodes.Stloc, locals[5]));
            instrs.Add(Instruction.Create(OpCodes.Ldloc, locals[5]));
            instrs.Add(Instruction.Create(OpCodes.Pop));

            var tryStart = Instruction.Create(OpCodes.Nop);
            var tryEnd = Instruction.Create(OpCodes.Nop);
            var handlerStart = Instruction.Create(OpCodes.Nop);
            var handlerEnd = Instruction.Create(OpCodes.Nop);

            instrs.Add(tryStart);
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next()));
            instrs.Add(Instruction.Create(OpCodes.Stloc, locals[0]));
            instrs.Add(tryEnd);

            instrs.Add(handlerStart);
            instrs.Add(Instruction.Create(OpCodes.Pop));
            instrs.Add(handlerEnd);

            var exTypeSig = module.Import(typeof(Exception)).ToTypeSig();
            var exTypeDefOrRef = exTypeSig.ToTypeDefOrRef();

            body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = exTypeDefOrRef,
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd
            });

            var c1 = Instruction.Create(OpCodes.Nop);
            var c2 = Instruction.Create(OpCodes.Nop);
            var c3 = Instruction.Create(OpCodes.Nop);
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(0, 3)));
            instrs.Add(Instruction.Create(OpCodes.Switch, new Instruction[] { c1, c2, c3 }));
            instrs.Add(Instruction.Create(OpCodes.Br, c3));
            instrs.Add(c1);
            instrs.Add(Instruction.Create(OpCodes.Nop));
            instrs.Add(c2);
            instrs.Add(Instruction.Create(OpCodes.Nop));
            instrs.Add(c3);

            instrs.Add(loopStart);
            instrs.Add(Instruction.Create(OpCodes.Ldloc, locals[0]));
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4, 20));
            instrs.Add(Instruction.Create(OpCodes.Bge, loopEnd));
            instrs.Add(Instruction.Create(OpCodes.Ldloc, locals[0]));
            instrs.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instrs.Add(Instruction.Create(OpCodes.Add));
            instrs.Add(Instruction.Create(OpCodes.Stloc, locals[0]));
            instrs.Add(Instruction.Create(OpCodes.Br, loopStart));
            instrs.Add(loopEnd);

            instrs.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instrs.Add(Instruction.Create(OpCodes.Brtrue, skipIf));
            instrs.Add(Instruction.Create(OpCodes.Nop));
            instrs.Add(skipIf);

            string sbType = typeof(StringBuilder).FullName;
            instrs.Add(Instruction.Create(OpCodes.Ldstr, sbType));
            var getTypeFromHandle = module.Import(typeof(Type).GetMethod("GetType", new[] { typeof(string) }));
            instrs.Add(Instruction.Create(OpCodes.Call, getTypeFromHandle));
            instrs.Add(Instruction.Create(OpCodes.Pop));

            var collect = module.Import(typeof(GC).GetMethod("Collect", Type.EmptyTypes));
            instrs.Add(Instruction.Create(OpCodes.Call, collect));

            var field = new FieldDefUser(
                RandomUnicodeString(rng.Next(20, 40)),
                new FieldSig(module.CorLibTypes.Int32),
                dnlib.DotNet.FieldAttributes.Static | dnlib.DotNet.FieldAttributes.Public);
            parentType.Fields.Add(field);

            instrs.Add(Instruction.Create(OpCodes.Ldsfld, field));
            instrs.Add(Instruction.Create(OpCodes.Pop));

            for (int i = 0; i < 30; i++)
                instrs.Add(Instruction.Create(OpCodes.Nop));

            instrs.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        private static string RandomChineseString(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append((char)rng.Next(0x4E00, 0x9FFF));
            return sb.ToString();
        }

        private static string RandomUnicodeString(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append((char)rng.Next(0x3000, 0x9FFF));
            return sb.ToString();
        }
    }
}
