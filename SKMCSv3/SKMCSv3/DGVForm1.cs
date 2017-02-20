using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SKMCSv3
{
    public partial class DGVForm1 : Form
    {
        //16進数F0⇒10進数240
        private const int F0 = 240;
        private string logPath = "";
        private string fileSKT = "";

        public DGVForm1(string log)
        {
            InitializeComponent();
            logPath = log;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //別名で保存
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                writeTXTFile(saveFileDialog1.FileName);
            }
            closeD();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            writeTXTFile(fileSKT);
            closeD();
        }

        public void setFile(string skt, string txt, DataTable dt)
        {
            fileSKT = skt;
            List<string> listTXT = new List<string>();
            List<string> tmp = new List<string>();
            if (readTXTFile(txt, listTXT) == 0)
            {
                //listTXTとdtのデータを比較する
                for (int i = 0; i < dt.Rows.Count; i++)
                    foreach (string str in listTXT)
                        if (dt.Rows[i][1].ToString().IndexOf(str) > 0)
                            tmp.Add(str);
                foreach (string str in tmp)
                    listTXT.Remove(str);
                foreach (string str in listTXT)
                    dt.Rows.Add(null, str, null, null);                              
            }
            tmp.Clear();
            listTXT.Clear();
            //datagridviewに設定を行う
            dataGridView1.DataSource = dt;
            if (skt == "") button2.Enabled = false;
        }

        private int readTXTFile(string txt,List<string> skt)
        {
            //写研データを読込んで、タグを区別する
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("写研データを(byte)読み込んで、タグを収集します。");
            FileStream fs = null;
            byte[] bs = new byte[0x1000];
            bool flg = false;
            string tmp = "";
            try
            {
                fs = new FileStream(txt, FileMode.Open, FileAccess.Read);
                while (fs.Read(bs, 0, bs.Length) > 0) 
                {
                    foreach(byte b in bs)
                    {
                        if (b >= F0)
                        {
                            flg = true;
                            tmp = String.Format("{0:X}", b);
                        }
                        else if (flg)
                        {
                            flg = false;
                            tmp+=String.Format("{0:X}", b);
                            //同じ値が入らないようソート
                            if (!skt.Contains(tmp)) skt.Add(tmp);
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                cl.logError(txt, ex);
                return 1;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            skt.Remove("");
            return 0;
        }

        private int writeTXTFile(string txt)
        {
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("タグファイルの書き込み開始：" + txt);

            StreamWriter sw = null;
            StringBuilder tmp = new StringBuilder();
            try
            {
                sw = new StreamWriter(txt, false, Encoding.GetEncoding("shift_jis"));
                for(int i = 0; i < dataGridView1.RowCount; i++)
                {
                    for(int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null) 
                            tmp.Append(dataGridView1.Rows[i].Cells[j].Value.ToString());
                        tmp.Append(';');
                    }
                    sw.WriteLine(tmp.ToString().Substring(0, tmp.ToString().Length - 1));
                    tmp.Clear();
                }
            }
            catch(Exception ex)
            {
                cl.logError(txt + "の書き込み失敗：", ex);
                return 1;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    //cl.dispose();
                }
            }

            return 0;
        }

        private void closeD()
        {
            //DGVFormを閉じる処理
            //dataGridView1.Dispose();
            
            this.Close();
        }
    }
}
