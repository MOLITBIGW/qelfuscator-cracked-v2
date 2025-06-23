using dnlib.DotNet;

namespace Obfuscator.A.P.Analyzer
{
	public class TypeDefAnalyzer : iAnalyze
	{
		public override bool Execute(object context)
		{
			TypeDef type = (TypeDef)context;
			if (type.IsRuntimeSpecialName)
				return false;
			if (type.IsGlobalModuleType)
				return false;
			return true;
		}
	}
}
