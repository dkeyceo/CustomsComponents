using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CustomsComponents.Common
{
  public static class DocProt
  {
    public static string GetDocStatus(DocStatus docStatus)
    {
      switch (docStatus)
      {
        case DocStatus.Register:
          return "R";
        case DocStatus.Cancell:
          return "N";
        case DocStatus.Delete:
          return "D";
        case DocStatus.Work:
          return "W";
        case DocStatus.Refuse:
          return "F";
        case DocStatus.CardOfRefuse:
          return "Del";
        default:
          throw new IndexOutOfRangeException("DocStatus");
      }
    }
    public static DocStatus GetDocStatusString(string docStatus)
    {
      switch (docStatus)
      {
        case "R":
          return DocStatus.Register;
        case "N":
          return DocStatus.Cancell;
        case "D":
          return DocStatus.Delete;
        case "W":
          return DocStatus.Work;
        case "F":
          return DocStatus.Refuse;
        case "Del":
          return DocStatus.CardOfRefuse;
        default:
          throw new IndexOutOfRangeException("DocStatus");
      }
    }
      public static async Task Insert(DocStatus docStatus, Guid docGuid, string user, string userIp = null, short? docType = null)
    {
      short actCode = GetActCode(docStatus);
      await InsertAsync(actCode, docGuid, user, userIp, docType);
    }

    private static short GetActCode(DocStatus docStatus)
    {
      switch (docStatus)
      {
        case DocStatus.Register:
          return 12;          
        case DocStatus.Cancell:
          return 13;          
        case DocStatus.Delete:
          return 14;          
        case DocStatus.Work:
          return 15;          
        case DocStatus.Refuse:
          return 16;          
        default:
          throw new IndexOutOfRangeException("DocStatus");
      }
    }

    public static async Task InsertAsync(short actCode, Guid docGuid, string user, string userIp = null, short? docType = null)
    {
      //try
      //{
        using (DocProtSoapClient ws = new DocProtSoapClient(DocProtSoapClient.EndpointConfiguration.DocProtSoap))
        {
          DocProtReq req = new DocProtReq();
          req.DocGuid = docGuid;
          req.ActCode = actCode;
          req.UserName = user;
          if (!string.IsNullOrEmpty(userIp))
            req.ActIp = userIp;
#if  DEBUG
          req.DebugMode = true;
#endif
          var resp = await ws.DocProtInsertAsync(req);
          if (resp.Error != null)
            throw new ApplicationException(resp.Error);
        }
      //}
      //catch (Exception ex)
      //{
      //  throw;        
      //}
    }
  }
}
