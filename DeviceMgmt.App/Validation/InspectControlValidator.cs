namespace DeviceMgmt.App.Validation;

/// <summary>点检项控件类型与上下限校验（0=是否型，1=数值型）</summary>
public static class InspectControlValidator
{
    /// <summary>数值型（ControlType≠0）时要求已填最小/最大值，且最大值必须大于最小值。</summary>
    public static string? ValidateNumericRange(int controlType, decimal? minValue, decimal? maxValue)
    {
        if (controlType == 0) return null;
        if (!minValue.HasValue || !maxValue.HasValue)
            return "数值型点检项须同时填写最小值和最大值";
        if (maxValue.Value <= minValue.Value)
            return "最大值必须大于最小值";
        return null;
    }
}
