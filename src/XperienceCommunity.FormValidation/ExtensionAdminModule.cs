﻿using CMS.Base;
using CMS.Core;
using Kentico.Xperience.Admin.Base;
using System;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.FormValidation;

[assembly: CMS.AssemblyDiscoverable]
[assembly: CMS.RegisterModule(typeof(ExtensionAdminModule))]

namespace XperienceCommunity.FormValidation;

internal class ExtensionAdminModule : AdminModule
{
    private ExtensionModuleInstaller? _installer;

    public ExtensionAdminModule()
        : base(Constants.ModuleName)
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        _installer = services.GetRequiredService<ExtensionModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) =>
        _installer?.Install();
}