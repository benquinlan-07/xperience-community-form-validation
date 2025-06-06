﻿using System.Linq;
using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace XperienceCommunity.FormValidation;

public static class ExtensionStartupExtensions
{
    /// <summary>
    /// Adds form validation extension dependencies
    /// </summary>
    /// <param name="serviceCollection">the <see cref="IServiceCollection"/> which will be modified</param>
    /// <returns>Returns this instance of <see cref="IServiceCollection"/>, allowing for further configuration in a fluent manner.</returns>
    public static IServiceCollection AddFormValidationExtensionServices(this IServiceCollection serviceCollection)
    {
        var formComponentValidator = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(IFormComponentValidator));
        serviceCollection.Remove(formComponentValidator);
        serviceCollection.AddKeyedSingleton(typeof(IFormComponentValidator), "kentico", formComponentValidator.ImplementationType);
        serviceCollection.AddSingleton<IFormComponentValidator, CustomFormComponentValidator>();

        serviceCollection.AddSingleton<ExtensionModuleInstaller>();

        FormWidgetRenderingConfiguration.GetConfiguration.Execute += FormComponentEvents.GetFormConfiguration;
        FormFieldRenderingConfiguration.GetConfiguration.Execute += FormComponentEvents.GetFormFieldConfiguration;

        return serviceCollection;
    }
}