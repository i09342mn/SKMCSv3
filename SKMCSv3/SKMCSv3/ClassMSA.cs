using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Data.OleDb;

namespace SKMCSv3
{
    class ClassMSA
    {
        private const string strOLEDB = @"Provider=Microsoft.ACE.OLEDB.12.0;";
        private const string strDS = @"Data source= ";
        private const string tmpDB = "Template.mdb";
        private const string sql1 = @"SELECT [pattern],[SK],[MCS],[detail] FROM MasterTable;";
        //private const string strType = @";Jet OLEDB:Engine Type=6";

        private string logPath = "";
        private string mdb = "";

        
        public int checkDB(string str, string log, DataTable dt)
        {
            ClassLog cl = new ClassLog(log);
            cl.logInfo("MSAccess接続します。");
            logPath = log;
            mdb = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + str;
            //mdbファイルの存在確認
            if (!File.Exists(mdb))
            {
                //mdbファイルの作成
                cl.logInfo("mdbファイルが存在しません。作成します。");
                try
                {
                    File.Copy(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + tmpDB, mdb);
                }
                catch (Exception ex)
                {
                    cl.logError("tmpmdbファイル移動エラー", ex);
                }
                //cl.dispose();
                return 1;
            }
            else
            {
                //mdbファイルの中身、テーブルが格納されているか確認する  
                cl.logInfo("テーブルの確認、開始");

                OleDbConnection con = new OleDbConnection();
                OleDbCommand cmd = null;
                OleDbDataReader odr = null;
                con.ConnectionString = strOLEDB + strDS + mdb;
                try
                {
                    con.Open();
                    cmd = new OleDbCommand();
                    cmd.Connection = con;
                    cmd.CommandText = sql1;
                    odr = cmd.ExecuteReader();
                    if (odr != null)
                    {
                        cl.logInfo("データ読み込み開始");
                        dt.Load(odr);
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    cl.logError("テーブルが破壊されています。", ex);
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        cmd.Dispose();
                        con.Dispose();
                    }
                }
                return 1;
            }            
        }
        
        
    }
}
