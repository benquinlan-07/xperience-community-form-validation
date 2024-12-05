using Kentico.Xperience.Admin.Base;
using XperienceCommunity.FormValidation;

[assembly: CMS.AssemblyDiscoverable]
[assembly: CMS.RegisterModule(typeof(ExtensionAdminModule))]

namespace XperienceCommunity.FormValidation;

internal class ExtensionAdminModule : AdminModule
{
    public ExtensionAdminModule()
        : base(Constants.ModuleName)
    {
    }
}