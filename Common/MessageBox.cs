using CustomsComponents.Components;
using CustomsComponents.Services;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomsComponents.Common
{
  public class MessageBox
  {
    #region Constructor
    private readonly NotificationService notificationService;
    private readonly CustomDialogService customDialogService;
    private const string DEFAULT_INFO_SUMMARY = "Повідомлення";
    private const string DEFAULT_WARNING_SUMMARY = "Повідомлення";
    private const string DEFAULT_ERROR_SUMMARY = "Помилка";
    public MessageBox(NotificationService notificationService, CustomDialogService customDialogService)
    {
      this.notificationService = notificationService;
      this.customDialogService = customDialogService;
    }
    #endregion

    #region Info
    //Информация в виде нотификации
    public void InfoLight(string message, string summary = DEFAULT_INFO_SUMMARY)
    {
      NotifyInfo(notificationService, message, summary);
    }

    //Информация в виде модального окна
    public Task InfoAsync(string message, string summary = DEFAULT_INFO_SUMMARY)
    {
      return InfoAsync(message, false, summary);
    }
    #endregion

    #region ErrorInfo
    public void ErrorInfoLight(string message, string summary = DEFAULT_WARNING_SUMMARY)
    {
      NotifyWarning(notificationService, message, summary);
    }

    public void ErrorInfoLight(Exception exception)
    {
      NotifyError(notificationService, exception);
    }

    public void ErrorNotify(string message, string summary = DEFAULT_ERROR_SUMMARY)
    {
      NotifyError(notificationService, message, summary);
    }

    public async Task ErrorInfoAsync(string message, string summary = DEFAULT_ERROR_SUMMARY)
    {
      await InfoAsync(message, true, summary);
    }

    public void ErrorInfo(string message)
    {
      _ = ErrorInfoAsync(message);
    }

    public void ErrorInfo(Exception err)
    {
      _ = ErrorInfoAsync(err);
    }

    public async Task ErrorInfoAsync(Exception err)
    {
      if (err is ApplicationException)
        await InfoAsync(err.Message, true);
      else
      {
        WriteErrorToLog(err);
#if DEBUG
        await InfoAsync(err.ToString(), true);
#else
        await InfoAsync(err.Message, true);
#endif
      }
    }
    #endregion

    #region WriteErrorToLog
    public static void WriteErrorToLog(Exception err, string source = null)
    {
      WriteErrorToLog(err.ToString(), source);
    }
    public static void WriteErrorToLog(string err, string source = null)
    {
      //using (var eventLog = new EventLog())
      //{
      //  eventLog.Log = "Application";
      //  eventLog.Source = "EAIS";
      //  eventLog.WriteEntry(err.Replace('%', '_'), EventLogEntryType.Warning, 555);
      //}
    }
    #endregion

    #region Confirm
    public async Task<bool> ConfirmAsync(string message)
    {
      return await ConfirmAsync(message, false);
    }

    public async Task<bool> ConfirmHardAsync(string message)
    {
      return await ConfirmAsync(message, true);
    }
    #endregion

    #region Promt
    public async Task<string> Promt(string message = null, string title = null, CustomDialog.PromtOptions options = null)
    {
      return await customDialogService.Promt(message, title, options);
    }

    public async Task<string> Promt(string message = null, string title = null, bool allowEmpty = false)
    {
      return await Promt(message, title, new CustomDialog.PromtOptions { OkButtonText = "OK", CancelButtonText = "Відмінити", AllowEmpty = allowEmpty });
    }
    #endregion

    #region SelectSingle
    public async Task<KeyValuePair<T, string>?> SelectSingle<T>(string caption, Dictionary<T, string> items, T initValue,
        SelectSingleDialogAppearance appearance = SelectSingleDialogAppearance.RadionButtonsVertical,
        string title = null, CustomDialog.DialogOptions options = null)
    {
      return await customDialogService.SelectSingle(caption, items, initValue, appearance, title, options);
    }
    public async Task<KeyValuePair<T, string>?> SelectSingle<T>(string caption, Dictionary<T, string> items,
      SelectSingleDialogAppearance appearance = SelectSingleDialogAppearance.RadionButtonsVertical,
      string title = null, CustomDialog.DialogOptions options = null)
    {
      return await customDialogService.SelectSingle(caption, items, appearance, title, options);
    }
    #endregion

    #region ThreeBox
    public Task<bool?> ThreeBoxAsync(string message)
    {
      return ThreeBoxAsync(message, false);

    }

    public Task<bool?> ThreeBoxHardAsync(string message)
    {
      return ThreeBoxAsync(message, true);

    }
    #endregion

    #region Static
    //public static void Info(NotificationService ns, string text)
    //{
    //  NotifyInfo(ns, text);
    //}

    //public static void ErrorInfo(NotificationService ns, string errorText)
    //{
    //  NotifyWarning(ns, errorText);
    //}

    //public static void ErrorInfo(NotificationService ns, Exception err)
    //{
    //  NotifyError(ns, err);
    //}
    #endregion

    #region Private
    private static void NotifyInfo(NotificationService notificationService, string message, string summary = DEFAULT_INFO_SUMMARY)
    {
      var notificationMessage = new NotificationMessage();
      notificationMessage.Duration = 5000;
      notificationMessage.Severity = NotificationSeverity.Info;
      notificationMessage.Summary = summary;
      notificationMessage.Detail = message;
      notificationService.Notify(notificationMessage);
    }

    private static void NotifyWarning(NotificationService notificationService, string message, string summary = DEFAULT_WARNING_SUMMARY)
    {
      var notificationMessage = new NotificationMessage();
      notificationMessage.Duration = 5000;
      notificationMessage.Severity = NotificationSeverity.Warning;
      notificationMessage.Summary = summary;
      notificationMessage.Detail = message;
      notificationService.Notify(notificationMessage);
    }

    private static void NotifyError(NotificationService notificationService, string message, string summary = DEFAULT_ERROR_SUMMARY)
    {
      var notificationMessage = new NotificationMessage();
      notificationMessage.Duration = 10000;
      notificationMessage.Severity = NotificationSeverity.Error;
      notificationMessage.Summary = summary;
      notificationMessage.Detail = message;
      notificationService.Notify(notificationMessage);
    }

    private static void NotifyError(NotificationService notificationService, Exception exception)
    {
      if (exception is ApplicationException)
      {
        NotifyWarning(notificationService, exception.Message);
      }
      else
      {
        WriteErrorToLog(exception);
#if DEBUG
        string message = exception.ToString();
#else
        string message = exception.Message;
#endif
        NotifyError(notificationService, message);
      }
    }
    #endregion

    #region Private Async
    private async Task InfoAsync(string message, bool hard, string summary = DEFAULT_INFO_SUMMARY)
    {
      await customDialogService.Info(message, summary, new CustomDialog.InfoOptions { OkButtonText = "OK", ShowExclamationMark = hard });
    }

    private async Task<bool> ConfirmAsync(string message, bool hard, string summary = DEFAULT_INFO_SUMMARY)
    {
      return await customDialogService.Confirm(message, summary, new CustomDialog.ConfirmOptions { OkButtonText = "OK", CancelButtonText = "Відмінити", ShowExclamationMark = hard });
    }

    private async Task<bool?> ThreeBoxAsync(string message, bool hard, string summary = DEFAULT_INFO_SUMMARY)
    {
      return await customDialogService.ConfirmWithCancel(message, summary, new CustomDialog.ConfirmWithCancelOptions { YesButtonText = "Так", NoButtonText = "Ні", CancelButtonText = "Відмінити", ShowExclamationMark = hard });
    }
    #endregion
  }
  public enum SelectSingleDialogAppearance
  {
    RadionButtonsVertical, RadionButtonsHorizontal, Grid
  }
}
