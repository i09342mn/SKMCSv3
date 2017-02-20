using System;
using System.Text;
using System.IO;

namespace SKMCSv3
{
    class ClassLog
    {
        //ログ記録関数
        //    INFO,DEBUG,WARNING,ERROR
        private string fileLog = "";

        public ClassLog(string str)
        {
            //logの下にYYYY￥mm￥dd.logファイルという構造に
            DateTime dt = DateTime.Now;
            StringBuilder paths = new StringBuilder();
            
            paths.Append(str);
            paths.Append("\\").Append(dt.Year).Append("\\").Append(dt.Month.ToString("00"));
            
            if (!Directory.Exists(paths.ToString()))
                Directory.CreateDirectory(paths.ToString());

            paths.Append("\\").Append(dt.Day).Append(".log");
            fileLog = paths.ToString();

            paths.Clear();
        }

        public void logInfo(string str)
        {
            //YYYY-MM-dd HH:mm:ss[INFO]str;
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append(" [INFO] ").Append(str);

            writeLog(sb);

            sb.Clear();
        }

        public void logWarning(string str, Exception ex)
        {
            //YYYY-MM-dd HH:mm:ss[WARNING]str;例外:ex ⇐フォーマット
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            if (ex == null)
                sb.Append(" [WARNING] ").Append(str);
            else
                sb.Append(" [WARNING] ").Append(str).Append(" ").Append(ex.ToString());

            writeLog(sb);

            sb.Clear();
        }

        public void logDebug(string str)
        {
            //YYYY-MM-dd HH:mm:ss[DEBUG]str;
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append(" [DEBUG] ").Append(str);

            writeLog(sb);

            sb.Clear();
        }
        
        public void logError(string str, Exception ex)
        {
            //YYYY-MM-dd HH:mm:ss[ERROR]str;例外:ex ⇐フォーマット
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            if (ex == null)
                sb.Append(" [WARNING] ").Append(str);
            else
                sb.Append(" [ERROR] ").Append(str).Append(" ").Append(ex.ToString());

            writeLog(sb);

            sb.Clear();
        }

        private void writeLog(StringBuilder sb)
        {
            Encoding enc = Encoding.GetEncoding("shift_jis");

            File.AppendAllText(fileLog, "\r\n" + sb.ToString(), enc);
        }
    }
}
