namespace Jint.Runtime.Modules
{

    public class VirtualModuleLoader : ModuleLoader
    {

        public VirtualModuleLoader()
        {

        }

        public override ResolvedSpecifier Resolve(string? referencingModuleLocation, ModuleRequest moduleRequest)
        {
            var specifier = moduleRequest.Specifier;

            return new ResolvedSpecifier(
                moduleRequest,
                specifier,
                Uri: null,
                SpecifierType.Bare
            );
        }


        protected override string LoadModuleContents(Engine engine, ResolvedSpecifier resolved)
        {


            return "";
        }

    }
}