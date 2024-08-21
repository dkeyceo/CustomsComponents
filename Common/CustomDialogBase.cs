using Microsoft.AspNetCore.Components;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel;
using CustomsComponents.Components;

namespace CustomsComponents.Common
{
  public class CustomDialogBase : ComponentBase
  {
    public override sealed async Task SetParametersAsync(ParameterView parameters)
    {
      if (parameters.TryGetValue("SetParameters", out Action<object> setParameters) && setParameters != null)
      {
        try
        {
          setParameters.Invoke(this);
        }
        catch(Exception exception)
        {
          if (parameters.TryGetValue("DialogThis", out CustomDialog.Dialog dialogThis) && dialogThis != null)
            dialogThis.CloseException(exception);
          else
            throw;
        }
      }
      await base.SetParametersAsync(parameters);
    }

    [Parameter]
    public Action<object> SetParameters { get; set; }

    [Parameter]
    public CustomDialog.Dialog DialogThis { get; set; }

    public void CloseSelf(dynamic result = null)
    {
      DialogThis?.Close(result);
    }

    public void CloseSelfException(Exception exception)
    {
      DialogThis?.CloseException(exception);
    }

    public virtual Task OnCloseFromTitleAsync(CancelEventArgs cancelEvent)
    {
      return Task.CompletedTask;
    }
  }

  public static class CustomDialogBaseExtensions
  {
    public static bool SetParameter<T, F>(this T dialog, string name, F value) where T : CustomDialogBase
    {
      var prop = dialog.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
      if (prop == null) return false;
      prop.SetValue(dialog, value);
      return true;
    }
  }
}
