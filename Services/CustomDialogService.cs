using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomsComponents.Components;
using CustomsComponents.Common;

namespace CustomsComponents.Services
{
  public class CustomDialogService
  {
    public delegate void DialogUpdate(IEnumerable<CustomDialog.Dialog> dialogs);

    public event DialogUpdate DialogUpdateEvent;

    public void Update()
    {
      DialogUpdateEvent?.Invoke(Dialogs);
    }

    public CustomDialog.DialogHandler Open(CustomDialog.Dialog dialog)
    {
      Dialogs.Add(dialog);
      Update();
      return new CustomDialog.DialogHandler(this, dialog, dialog.Task.Task);
    }

    public void CloseDialog(CustomDialog.Dialog dialog, dynamic result = null)
    {
      Dialogs.Remove(dialog);
      Update();
      dialog.Task.TrySetResult(result);
    }

    public void CloseDialogException(CustomDialog.Dialog dialog, Exception exception)
    {
      Dialogs.Remove(dialog);
      Update();
      dialog.Task.TrySetException(exception);
    }

    // use CloseSelf method instead
    /*public void Close(dynamic result = null)
    {
      if (Dialogs.Any())
      {
        CloseDialog(Dialogs.Last(), result);
      }
    }*/

    protected List<CustomDialog.Dialog> Dialogs { get; set; } = new List<CustomDialog.Dialog>();

    protected CustomDialog.DialogHandler Open(RenderFragment renderFragment, CustomDialog.DialogType type, string title = null, CustomDialog.DialogOptions options = null)
    {
      var dialog = new CustomDialog.Dialog(this)
      {
        Type = type,
        Title = title,
        Component = null,
        RenderFragment = renderFragment,
        Task = new TaskCompletionSource<dynamic>(),
        Options = options
      };
      return Open(dialog);
    }

    protected CustomDialog.DialogHandler Open<T>(CustomDialog.DialogType type, string title = null, Action<T> setParameters = null, Dictionary<string, object> parameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase
    {
      var dialog = new CustomDialog.Dialog(this)
      {
        Type = type,
        Title = title,
        Component = null,
        RenderFragment = null,
        Task = new TaskCompletionSource<dynamic>(),
        Options = options
      };
      dialog.RenderFragment = builder =>
      {
        var i = 0;
        builder.OpenComponent(i++, typeof(T));
        if (parameters != null)
        {
          foreach (var parameter in parameters)
          {
            builder.AddAttribute(i++, parameter.Key, parameter.Value);
          }
        }
        if (setParameters != null)
        {
          builder.AddAttribute(i++, "SetParameters", new Action<object>(obj => setParameters((T)obj)));
        }
        builder.AddAttribute(i++, "DialogThis", dialog);
        builder.AddComponentReferenceCapture(i++, component => dialog.Component = component);
        builder.CloseComponent();

      };
      return Open(dialog);
    }

    public CustomDialog.DialogHandler Open<T>(string title = null, Action<T> setParameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase
    {
      return Open<T>(CustomDialog.DialogType.SIMPLE, title, setParameters, null, options);
    }

    public CustomDialog.DialogHandler Open(RenderFragment renderFragment, string title = null, CustomDialog.DialogOptions options = null)
    {
      return Open(renderFragment, CustomDialog.DialogType.SIMPLE, title, options);
    }

    public async Task<dynamic> OpenAsync<T>(string title = null, Action<T> setParameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase
    {
      return await Open<T>(title, setParameters, options).GetResultAsync();
    }

    public async Task<dynamic> OpenAsync(RenderFragment renderFragment, string title = null, CustomDialog.DialogOptions options = null)
    {
      return await Open(renderFragment, title, options).GetResultAsync();
    }

    public async Task<dynamic> OpenInfoAsync<T>(string title = null, Action<T> setParameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase, CustomDialog.IInfoDialog
    {
      return await Open<T>(CustomDialog.DialogType.INFO, title, setParameters, null, options).GetResultAsync();
    }

    public async Task<CustomDialog.ConfirmDialogResult> OpenConfirmAsync<T>(string title = null, Action<T> setParameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase, CustomDialog.IConfirmDialog
    {
      return await Open<T>(CustomDialog.DialogType.CONFIRM, title, setParameters, null, options).GetResultAsync();
    }

    public async Task<CustomDialog.ConfirmWithCancelDialogResult> OpenConfirmWithCancelAsync<T>(string title = null, Action<T> setParameters = null, CustomDialog.DialogOptions options = null) where T : CustomDialogBase, CustomDialog.IConfirmWithCancelDialog
    {
      return await Open<T>(CustomDialog.DialogType.CONFIRM_WITH_CANCEL, title, setParameters, null, options).GetResultAsync();
    }

    public async Task<bool?> ConfirmWithCancel(string message = null, string title = null, CustomDialog.ConfirmWithCancelOptions options = null)
    {
      return (await OpenConfirmWithCancelAsync<InfoDialog>(title, (dialog) => {

        dialog.Message = message;
        dialog.ShowExclamationMark = options?.ShowExclamationMark ?? false;

      }, new CustomDialog.DialogOptions { AcceptButtonText = options?.YesButtonText, CancelButtonText = options?.NoButtonText, ThirdButtonText = options?.CancelButtonText, Top = "40vh" })).IsAccepted;
    }

    public async Task<bool> Confirm(string message = null, string title = null, CustomDialog.ConfirmOptions options = null)
    {
      return (await OpenConfirmAsync<InfoDialog>(title, (dialog) => {

        dialog.Message = message;
        dialog.ShowExclamationMark = options?.ShowExclamationMark ?? false;

      }, new CustomDialog.DialogOptions { AcceptButtonText = options?.OkButtonText, CancelButtonText = options?.CancelButtonText, Top = "40vh" })).IsAccepted;
    }

    public async Task<string> Promt(string message = null, string title = null, CustomDialog.PromtOptions options = null)
    {
      var result = await OpenConfirmAsync<PromtDialog>(title, (dialog) => {

        dialog.Message = message;
        dialog.AllowEmpty = options?.AllowEmpty ?? false;
        dialog.InitialText = options?.InitialText;
        dialog.MaxLength=options?.MaxLength ?? Int32.MaxValue;
        dialog.Placeholder = options?.Placeholder;
        if ((options?.Lines ?? 0) != 0)
          dialog.Lines = options.Lines;
      }, new CustomDialog.DialogOptions { AcceptButtonText = options?.OkButtonText, CancelButtonText = options?.CancelButtonText, Top = "40vh" });
      if (!result.IsAccepted)
        return null;
      return (string)result.Result;
    }

    public async Task Info(string message = null, string title = null, CustomDialog.InfoOptions options = null)
    {
      await OpenInfoAsync<InfoDialog>(title, (dialog)=> {

        dialog.Message = message;
        dialog.ShowExclamationMark = options?.ShowExclamationMark ?? false;

      }, new CustomDialog.DialogOptions { InfoButtonText = options?.OkButtonText, Top = "40vh" });
    }

    public async Task<KeyValuePair<T, string>?> SelectSingle<T>(string caption, Dictionary<T, string> items,
      SelectSingleDialogAppearance appearance = SelectSingleDialogAppearance.RadionButtonsVertical, 
      string title = null, CustomDialog.DialogOptions options = null)
    {
      var result = await OpenConfirmAsync<SelectSingleDialog<T>>(title, (dialog) => {

        dialog.Caption = caption;
        dialog.Items = items;
        dialog.Appearance = appearance;
      }, options);
      if (!result.IsAccepted)
        return default;
      return result.Result;
    }

    public async Task<KeyValuePair<T, string>?> SelectSingle<T>(string caption, Dictionary<T,string> items, T initValue, 
        SelectSingleDialogAppearance appearance= SelectSingleDialogAppearance.RadionButtonsVertical,
        string title = null, CustomDialog.DialogOptions options = null)
    {
      var result = await OpenConfirmAsync<SelectSingleDialog<T>>(title, (dialog) => {

        dialog.Caption = caption;
        dialog.Items = items;
        dialog.SelectedValue = initValue;
        dialog.Appearance = appearance; 
      }, options);
      if (!result.IsAccepted)
        return default;
      return result.Result;
    }
  }
}
