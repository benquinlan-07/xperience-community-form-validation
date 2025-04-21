using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using XperienceCommunity.FormValidation.Models;

namespace XperienceCommunity.FormValidation;

internal class ExtensionModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> _resourceProvider;

    public ExtensionModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider)
    {
        _resourceProvider = resourceProvider;
    }

    public void Install()
    {
        var resource = _resourceProvider.Get(Constants.ResourceName)
                       ?? new ResourceInfo();

        InitializeResource(resource);
        InstallWebsiteChannelDomainAliasInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = Constants.ResourceDisplayName;
        resource.ResourceName = Constants.ResourceName;
        resource.ResourceDescription = Constants.ResourceDescription;
        resource.ResourceIsInDevelopment = false;

        if (resource.HasChanged)
            _resourceProvider.Set(resource);

        return resource;
    }

    public void InstallWebsiteChannelDomainAliasInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(FormFieldValidationInfo.OBJECT_TYPE) ?? DataClassInfo.New(FormFieldValidationInfo.OBJECT_TYPE);

        info.ClassName = FormFieldValidationInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = FormFieldValidationInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = FormFieldValidationInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FormFieldValidationInfo.FormFieldValidationID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FormFieldValidationInfo.FormFieldValidationGUID),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormFieldValidationInfo.FormFieldValidationFormGUID),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormFieldValidationInfo.FormFieldValidationFieldGUID),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormFieldValidationInfo.FormFieldValidationRequiredError),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormFieldValidationInfo.FormFieldValidationRules),
            AllowEmpty = true,
            Visible = true,
            Size = 0,
            DataType = "longtext"
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}