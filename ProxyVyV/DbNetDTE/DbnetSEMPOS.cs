using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBNeT.SEMPOS;
using System.Threading;

namespace VyVDbNet
{
    //public static class DbnetSEMPOS
    //{

    //    public  static string jsonResp =null;
    //    private static bool esperar = true;


    //    public static void  SendMessage(string json)
    //    {
    //        try
    //        {
    //            ResponseCallback resp = new ResponseCallback(Response);

    //            SEMContext.Instance.Initialize(json, resp);


    //            while (esperar)
    //            {
    //                Thread.Sleep(100);
    //            }
    //        }
    //        catch (Exception e)
    //        {

    //            jsonResp = e.ToString();
    //        }


    //    }

    //    public static void Response(string message)
    //    {
    //        jsonResp = message;
    //        esperar = false;

    //        //Console.WriteLine(message);
    //    }
    //}
}
