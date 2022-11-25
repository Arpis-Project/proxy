using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV
{
    public partial class Utils
    {
        const string strCnnOra_cms = "Provider=OraOLEDB.Oracle;Data Source=rproods;User Id=cms;Password=p3rsef0ne;";

        public bool actualizaNumFolio(string documentID, string folio, Log objLog)
        {
            bool resp = false;
            double dblCantRegistros = default(double);
            try
            {
                objLog.write("ENTRE actualizaNumFolio");
                objLog.write("documentID" + documentID);
                objLog.write("folio" + folio);

                using (Oracle oOraCms = new Oracle(120))
                {
                    oOraCms.ConnectionOpen(strCnnOra_cms);
                    string strSql = string.Empty;

                    strSql = "update rps.document set doc_no='$folio$', fiscal_doc_id='$folio$', tracking_no= '$folio$' where sid='$sid$' ";
                    strSql = strSql.Replace("$folio$", folio);
                    strSql = strSql.Replace("$sid$", documentID);
                    oOraCms.Write(strSql, ref dblCantRegistros);
                    oOraCms.ConnectionClose();

                    resp = true;
                }
            }
            catch (Exception ExCms)
            {
                objLog.write("ERROR");
                objLog.write(ExCms.Message);
                resp = false;
            }

            return resp;
        }

    }


    internal class Oracle : IDisposable
    {
        private OleDbTransaction oTransaccion;
        private OleDbConnection oConexion;
        private bool InTrans = false;
        private int gTimeOut;
        private OleDbDataAdapter DataAdapter = new OleDbDataAdapter();
        bool disposed = true;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                oTransaccion.Dispose();
                oConexion.Dispose();
                DataAdapter.Dispose();
                //
            }
            disposed = true;
        }

        public void BeginTransaction()
        {
            try
            {
                oTransaccion = oConexion.BeginTransaction();
                InTrans = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Commit()
        {
            try
            {
                if (InTrans)
                {
                    oTransaccion.Commit();
                    InTrans = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Rollback()
        {
            try
            {
                if (InTrans)
                {
                    oTransaccion.Rollback();
                    InTrans = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConnectionOpen(string StringConexion)
        {
            try
            {
                oConexion = new OleDbConnection(StringConexion);
                oConexion.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConnectionClose()
        {
            oConexion.Close();
            oConexion.Dispose();
        }

        public long Write(string sSql, ref double nFilasAfectadas)
        {
            OleDbCommand oComando = default(OleDbCommand);

            try
            {
                oComando = new OleDbCommand(sSql, oConexion);
                oComando.Transaction = oTransaccion;
                oComando.CommandTimeout = gTimeOut;

                nFilasAfectadas = oComando.ExecuteNonQuery();
                oComando.Dispose();
                return (long)Results.Ok;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long Read(string sSql, ref DataSet oDataSet)
        {
            OleDbCommand oComando = default(OleDbCommand);
            OleDbDataAdapter oDataAD = default(OleDbDataAdapter);

            try
            {
                oComando = new OleDbCommand(sSql, oConexion);
                oComando.CommandTimeout = gTimeOut;

                oDataAD = new OleDbDataAdapter(oComando);

                oDataSet = new DataSet();
                oDataAD.Fill(oDataSet);
                oComando.Dispose();
                oDataAD.Dispose();
                return (long)Results.Ok;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long Read(string sSql, ref DataTable oDataTable)
        {
            OleDbCommand oComando = default(OleDbCommand);
            OleDbDataAdapter oDataAD = default(OleDbDataAdapter);
            try
            {
                oComando = new OleDbCommand(sSql, oConexion);
                oComando.CommandTimeout = gTimeOut;

                oDataAD = new OleDbDataAdapter(oComando);

                oDataTable = new DataTable();
                oDataAD.Fill(oDataTable);

                oComando.Dispose();
                oDataAD.Dispose();

                return (long)Results.Ok;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long Read(string sSql, ref OleDbDataReader oDataReader)
        {
            OleDbCommand oComando = default(OleDbCommand);

            try
            {
                oComando = new OleDbCommand(sSql, oConexion);
                oComando.CommandTimeout = gTimeOut;

                if (InTrans)
                    oComando.Transaction = oTransaccion;
                oDataReader = oComando.ExecuteReader();

                oComando.Dispose();
                return (int)Results.Ok;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Oracle(int QueryTimeOut)
        {
            gTimeOut = QueryTimeOut;
        }

        enum Results
        {
            Empty = 0,
            Ok = 1,
            Err = 2
        }
    }

}
