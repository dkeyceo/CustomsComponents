using CustomsComponents.Common;
using CustomsComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomsComponents.Components
{
  public partial class CSNDocList : ComponentBase
  {
    #region Inject
    [Inject]
    CustomDialogService cds { get; set; }
    [Inject]
    Microsoft.JSInterop.IJSRuntime jsrt { get; set; }
    [Inject]
    protected MessageBox MsgBox { get; set; }
    #endregion

    #region Parameters - propertis
    [Parameter]
    public string Title { get; set; }
    [Parameter]
    public bool ProtButtonVisible { get; set; }

    private IEnumerable<DataRow> dataSource;
    bool dataSourceHasBeenSet = false;
    [Parameter]
    public IEnumerable<DataRow> DataSource
    {
      get
      {
        return dataSource;
      }
      set
      {
        dataSource = value;
        dataSourceHasBeenSet = true;
      }
    }

    [Parameter]
    public List<ColumnStyle> Columns { get; set; }
    [Parameter]
    public bool HideHeader { get; set; }
    [Parameter]
    public bool HideSearch { get; set; } = false;
    [Parameter]
    public bool HideTitle { get; set; } = false;
    [Parameter]
    public List<ButtonProperties> AddButtonList { get; set; }
    #endregion

    #region Parameters - markup
    [Parameter]
    public RenderFragment FindsCriteria { get; set; }

    [Parameter]
    public RenderFragment Grid { get; set; }

    [Parameter]
    public RenderFragment BeforeHeader { get; set; }

    [Parameter]
    public RenderFragment AfterHeader { get; set; }
    [Parameter]
    public RenderFragment AddingFoundText { get; set; }
    [Parameter]
    public RenderFragment AddButtonLine { get; set; }
    [Parameter]
    public RenderFragment Footer { get; set; }

    [Parameter]
    public RenderFragment<DataRow> Expand { get; set; }

    #endregion

    #region Parameters - functions
    [Parameter]
    public Func<Task> OnAdd { get; set; }

    [Parameter]
    public Func<Task> OnFind { get; set; }

    [Parameter]
    public Action OnClearFind { get; set; }

    [Parameter]
    public Func<string, Task> OnAddWithOption { get; set; }

    [Parameter]
    public Func<DataRow, string, Task> OnChangeStatus { get; set; }

    [Parameter]
    public Func<DataRow, IEnumerable<(string, string)>> OnGetRowIcons { get; set; }

    [Parameter]
    public Func<DataRow, string, Task> OnClickCell { get; set; }
    #endregion

    #region Properties
    RadzenGrid<DataRow> grid { get; set; }
    #endregion

    #region Private actions

    async Task PrintToExcel()
    {
      await jsrt.InvokeVoidAsync("downloadFromByteArray", ExcelGenerator.WriteToExcel(DataSource, "Книга1",
                                 Columns), $"Excel_{DateTime.Now:dd.MM.yyyy}.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
    async Task ShowProt(DataRow row)
    {
      try
      {
        await cds.OpenAsync<DProt.DocProtDlg>("Протокол дій", (dialog) =>
        {

          dialog.DocGuid = (Guid)row["guid"];

        }, new CustomDialog.DialogOptions { Width = "80%", Top = "5%" });
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }
    async Task OnEditCurrent(DataRow row, string link)
    {
      using (new LoadingDialog(cds))
        await jsrt.InvokeAsync<object>("blazorOpen", new object[] { $"{link}/{row["guid"]}", "_blank" });
    }
    #endregion

    #region External Actions

    async Task ChangeStatus(DataRow row, RadzenSplitButtonItem item)
    {
      try
      {
        if (item?.Value == "Prot")
          await ShowProt(row);
        else
          await OnChangeStatus?.Invoke(row, item?.Value);
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }


    async Task ClickCell(DataRow row, string linkColumnIdentifier)
    {
      try
      {
        await OnClickCell?.Invoke(row, linkColumnIdentifier);
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }

    async Task AddSplitButtonClick(RadzenSplitButtonItem item)
    {
      try
      {
        if (item == null || item.Value == "add")
          await OnAdd?.Invoke();
        else
          await OnAddWithOption?.Invoke(item.Value);
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }

    bool searching = false;
    async Task Find()
    {
      try
      {
        searching = true;
        await OnFind?.Invoke();
        if (grid != null && !(grid.PagedView?.Any() ?? false))
          await grid.FirstPage();
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
      finally
      {
        searching = false;
      }
    }

    void ClearFind()
    {
      try
      {
        OnClearFind?.Invoke();
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }

    IEnumerable<(string icon, string title)> GetRowIcons(DataRow row)
    {
      var retval = OnGetRowIcons?.Invoke(row);
      return retval ?? (new List<(string, string)>());
    }
    #endregion

    #region ObjToString
    string ObjToString(object val)
    {
      if (val == null)
        return null;
      else if (val is DateTime)
      {
        DateTime dt = (DateTime)val;
        if (dt.TimeOfDay != TimeSpan.Zero)
          return StringLib.DTOCT(dt);
        else
          return StringLib.DTOC(dt);
      }
      else if (val is decimal)
        return ((decimal)val).ToString("N");
      else
        return val.ToString();
    }
    #endregion

    #region OnInitialized
    protected override void OnInitialized()
    {
      try
      {
        if (DataSource == null && !dataSourceHasBeenSet && Grid == null)
          throw new ApplicationException("Не указано ні DataSource, ні Grid. Зверніться до розробника");
        if (Columns == null && Grid == null)
          throw new ApplicationException("Не указано ні Columns, ні Grid. Зверніться до розробника");
        if (FindsCriteria == null)
          throw new ApplicationException("Не указано FindsCriteria. Зверніться до розробника");
        if (OnFind == null)
          throw new ApplicationException("Не указано OnFind. Зверніться до розробника");
        if (OnClearFind == null)
          throw new ApplicationException("Не указано OnClearFind. Зверніться до розробника");
      }
      catch (Exception err)
      {
        MsgBox.ErrorInfo(err);
      }
    }
    #endregion

    #region GetTitle
    string GetTitle(ColumnStyle cs, DataRow row)
    {
      if (cs == null || row == null)
        return null;
      if (cs.TitleGetter == null)
        return null;
      return cs.TitleGetter(row);
    }
    #endregion 

    #region RowRender
    void RowRender(RowRenderEventArgs<DataRow> args)
    {
      args.Expandable = Expand != null;
    }
    #endregion

    #region IsAnyButtonVisible
    bool IsAnyButtonVisible(DataRow row, ColumnStyle column)
    {
      return ProtButtonVisible ||
        row[column.StatusColumn].ToString() == column.ValueStatusVisible.Split(",")[0] ||
        row[column.StatusColumn].ToString() == column.ValueStatusVisible.Split(",")[1] ||
        row[column.StatusColumn].ToString() == column.ValueStatusVisible.Split(",")[2] ||
        (column.AddingButtonList != null && column.AddingButtonList.Any(button => button.Visible != null ? button.Visible(row) : (row[column.StatusColumn].ToString() == button.StatusVisible)));
    }
    #endregion
  }
}

#region Classes
public enum DocStatus
{
  Refuse,
  Delete,
  Cancell,
  Register,
  Work,
  CardOfRefuse
}
public class ColumnStyle
{
  public ColumnStyle(string caption, string colName, string width, string link4Open = null, bool isBtnColumn = false, bool isIconColumn = false, string statusColumn = null, string valueStatusVisible = null, List<ButtonProperties> addingButtonList = null)
  {
    Caption = caption;
    ColName = colName;
    Width = width;
    Link4Open = link4Open;
    IsBtnColumn = isBtnColumn;
    IsIconColumn = isIconColumn;
    StatusColumn = statusColumn;
    ValueStatusVisible = valueStatusVisible;
    AddingButtonList = addingButtonList;
  }

  public ColumnStyle(string caption, Func<DataRow, string> textGetter, string width)
  {
    Caption = caption;
    TextGetter = textGetter;
    Width = width;
  }

  public string Caption { get; set; }
  public string StatusColumn { get; set; }
  public string ValueStatusVisible { get; set; }
  public string ColName { get; set; }
  public string Link4Open { get; set; }
  public string Width { get; set; }
  public bool IsBtnColumn { get; set; }
  public List<ButtonProperties> AddingButtonList { get; set; }
  public bool IsIconColumn { get; set; }
  public bool BtnColumnAlwaysOpenPopup { get; set; }
  public string LinkColumnIdentifier { get; set; }

  public Func<DataRow, string> ExcelTextGetter { get; set; }
  public Func<DataRow, string> TextGetter { get; set; }
  public Func<DataRow, string> TitleGetter { get; set; }
}
public class ButtonProperties
{
  public ButtonProperties(string caption, string value, string icon = null, string statusVisible = null,string buttonColor = null, string iconColor=null)
  {
    Caption = caption;
    Icon = icon;
    StatusVisible = statusVisible;
    Value = value;
		ButtonColor=buttonColor;
    IconColor=iconColor;
  }
  public string Value { get; set; }
  public string Caption { get; set; }
  public string Icon { get; set; }	
	public string StatusVisible { get; set; }
	public string ButtonColor { get; set; }
  public string IconColor { get; set; }
	public Func<DataRow, bool> Visible { get; set; }
}
#endregion