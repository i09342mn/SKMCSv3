using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace SKMCSv3
{
    public partial class Main : Form
    {
        //変数定義
        private string fileTXT = "";
        private string fileSKT = "";
        public string logPath = "";
        private string fileMdb = "";

        public Main()
        {
            InitializeComponent();

            //開始と設定準備
            openIni();
            textBox3.Text = logPath;           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //写研データ選択
            dialogTXT();           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //タグ変換テーブル選択
            dialogSKT();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //ログファイルバスの設定
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                logPath = folderBrowserDialog1.SelectedPath;
                textBox3.Text = logPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //マスタテーブル出力処理
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

            }
            else
                return;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //読込実行処理
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("読込処理開始");
            //TXTファイルが選択されていない場合
            if (fileTXT == "")
                if (dialogTXT() != 0) return;

            //SKTファイルが選択されていない場合
            DataTable dt = new DataTable();
            if (fileSKT == "")
            {
                //DB接続
                ClassMSA msa = new ClassMSA();
                if (msa.checkDB(fileMdb, logPath, dt) != 0)
                {
                    //MSAccessデータに問題があるとき、SKTタグを参照
                    cl.logWarning("MSAccessデータに問題があり、SKTタグを参照", null);
                    MessageBox.Show("MSAccessデータに問題があります。\nSKTファイルを選択してください。", "Warning", MessageBoxButtons.OK);
                    return;
                }
            }
            else
            {
                //SKTファイルを開いて、タグを格納する
                setDT(fileSKT, dt);
            }

            //写研TXTファイルを開いて、整理されたタグと写研データを表示する
            //this.Enabled = false;
            DGVForm1 dgvf1 = new DGVForm1(logPath);
            dgvf1.setFile(fileSKT, fileTXT, dt);
            dgvf1.ShowDialog();
            dgvf1.Dispose();
            dt.Clear();
            cl.logInfo("読込処理終了");
            //cl.dispose();
            GC.Collect();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //変換実行処理
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("変換実行処理開始");
            //TXTファイルが選択されていない場合
            if (fileTXT == "")
                if (dialogTXT() != 0) return;

            //SKTファイルが選択されていない場合
            DataTable dt = new DataTable();
            if (fileSKT == "")
            {
                //DB接続
                ClassMSA msa = new ClassMSA();
                if (msa.checkDB(fileMdb, logPath, dt) != 0)
                {
                    //MSAccessデータに問題があるとき、SKTタグを参照
                    cl.logWarning("MSAccessデータに問題があり、SKTタグを参照", null);
                    MessageBox.Show("MSAccessデータに問題があります。\nSKTファイルを選択してください。", "Warning", MessageBoxButtons.OK);
                    return;
                }
            }
            else
            {
                //SKTファイルを開いて、タグを格納する
                setDT(fileSKT, dt);
            }

            //写研TXTファイルを開いて、整理されたタグと写研データを表示する
            DGVForm2 dgvf2 = new DGVForm2(logPath, dt, fileTXT);
            dgvf2.ShowDialog();
            dgvf2.Dispose();
            dt.Clear();
            cl.logInfo("変換実行処理終了");
            //cl.dispose();
            GC.Collect();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //終了処理を行う
            GC.Collect();
            Application.Exit();
        }

        private int dialogTXT()
        {
            //写研データ選択
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileTXT = openFileDialog1.FileName;
                textBox1.Text = Path.GetFileName(fileTXT);
                return 0;
            }
            else
                return 1;            
        }

        private int dialogSKT()
        {
            //タグ変換テーブル選択
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                fileSKT = openFileDialog2.FileName;
                textBox2.Text = Path.GetFileName(fileSKT);
                return 0;
            }
            else           
                return 1;            
        }

        private int openFile(List<string> str, string file)
        {
            //ファイルを開いて、strリストの中身を入れる
            try
            {
                Encoding enc = Encoding.GetEncoding("shift_jis");
                str.AddRange(File.ReadAllLines(file, enc));
            }
            catch(Exception ex)
            {
                //ファイルの読み込み時のエラー
                if (logPath == "")
                {
                    string path = Assembly.GetExecutingAssembly().Location;
                    string dflt = path + "\\default.log";
                    ClassLog cl = new ClassLog(path);
                    cl.logError(file, ex);
                    //cl.dispose();
                }
                else
                {
                    ClassLog cl = new ClassLog(logPath);
                    cl.logError(file, ex);
                    //cl.dispose();
                }
                return 1;
            }            
            return 0;
        }
                
        private void openIni()
        {
            const string DATAFILENAME = "datafilename";
            const string LOGPATH = "logpath";
            //ini設定を行う
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iniFile = appPath + "\\setup.ini";

            //setup.iniファイルの存在確認
            if (File.Exists(iniFile))
            {
                //ファイルを開き、設定値を行う
                List<string> lines = new List<string>();
                if (openFile(lines, iniFile) == 0)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    foreach(string line in lines)
                    {
                        if (line != "")
                        {
                            string[] tmp = line.Split('=');
                            if (tmp.Length == 2)
                            {
                                dict[tmp[0].Trim()] = tmp[1].Trim();
                            }
                        }
                    }
                    fileMdb = dict[DATAFILENAME];
                    logPath = dict[LOGPATH];
                    ClassLog cl1 = new ClassLog(logPath);
                    cl1.logInfo("Main開始");
                    //cl1.dispose();

                    lines.Clear();
                    return;
                }
            }
            //デフォルト値を設定し、Setup.iniファイルを作成する
            const string DT = "skttable.mdb";
            const string LP = "C:\\log";
            Encoding enc = Encoding.GetEncoding("shift_jis");
            StreamWriter sw = null;
            ClassLog cl = new ClassLog(LP);
            logPath = LP;
            try
            {
                cl.logInfo("Main開始/setup.ini作成");
                sw = new StreamWriter(iniFile, false, enc);
                sw.WriteLine(LOGPATH + '=' + LP);
                sw.WriteLine(DATAFILENAME + '=' + DT);
            }
            catch (Exception ex)
            {
                cl.logError(iniFile, ex);
            }
            finally
            {
                if (sw != null) sw.Close();
                //cl.dispose();
            }
        }

        private void setDT(string str, DataTable dt)
        {
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("SKTファイルの読み込み開始");
            try
            {
                Encoding enc = Encoding.GetEncoding("shift_jis");
                string[] lines = File.ReadAllLines(str, enc);

                dt.Columns.Add("pattern", typeof(int));
                dt.Columns.Add("SK", typeof(string));
                dt.Columns.Add("MCS", typeof(string));
                dt.Columns.Add("detail", typeof(string));

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] != "")
                    {
                        string[] parts = lines[i].Split(';');
                        if (parts.Length == 4)
                            dt.Rows.Add(parts[0], parts[1], parts[2], parts[3]);
                        else
                            cl.logDebug(str + ":" + i + "行目フォーマットエラー：" + lines[i]);
                    }                    
                }
            }
            catch(Exception ex)
            {
                cl.logError(str, ex);
                //cl.dispose();
            }
        }
    }
}
