using System;
using System.Data;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.FormValidation.Models;

[assembly: RegisterObjectType(typeof(FormFieldValidationInfo), FormFieldValidationInfo.OBJECT_TYPE)]

namespace XperienceCommunity.FormValidation.Models;

/// <summary>
/// Data container class for <see cref="FormFieldValidationInfo"/>.
/// </summary>
[Serializable]
public partial class FormFieldValidationInfo : AbstractInfo<FormFieldValidationInfo, IInfoProvider<FormFieldValidationInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "xpcm.formfieldvalidation";
    public const string OBJECT_CLASS_NAME = "XPCM.FormFieldValidation";
    public const string OBJECT_CLASS_DISPLAYNAME = "Form Field Validation";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<FormFieldValidationInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(FormFieldValidationID), null, nameof(FormFieldValidationGUID), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Form Field Validation ID
    /// </summary>
    [DatabaseField]
    public virtual int FormFieldValidationID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FormFieldValidationID)), 0);
        set => SetValue(nameof(FormFieldValidationID), value);
    }


    /// <summary>
    /// Form Field Validation GUID
    /// </summary>
    [DatabaseField]
    public virtual Guid FormFieldValidationGUID
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormFieldValidationGUID)), default);
        set => SetValue(nameof(FormFieldValidationGUID), value);
    }


    /// <summary>
    /// Form GUID
    /// </summary>
    [DatabaseField]
    public virtual Guid FormFieldValidationFormGUID
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormFieldValidationFormGUID)), default);
        set => SetValue(nameof(FormFieldValidationFormGUID), value);
    }


    /// <summary>
    /// Field GUID
    /// </summary>
    [DatabaseField]
    public virtual Guid FormFieldValidationFieldGUID
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormFieldValidationFieldGUID)), default);
        set => SetValue(nameof(FormFieldValidationFieldGUID), value);
    }

    /// <summary>
    /// Required error message for form field
    /// </summary>
    [DatabaseField]
    public virtual string FormFieldValidationRequiredError
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormFieldValidationRequiredError)), default);
        set => SetValue(nameof(FormFieldValidationRequiredError), value);
    }

    /// <summary>
    /// JSON serialised validation rules.
    /// </summary>
    [DatabaseField]
    public virtual string FormFieldValidationRules
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormFieldValidationRules)), default);
        set => SetValue(nameof(FormFieldValidationRules), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="FormFieldValidationInfo"/> class.
    /// </summary>
    public FormFieldValidationInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="FormFieldValidationInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public FormFieldValidationInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
