using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKMCSv3
{
    public partial class DGVForm2 : Form
    {
        private const int INTCHAR = 56667;//特殊記号のChar値（F000）61440-4773
        private const char SCHAR = '.';
        private string logPath = "";
        private string fileTXT = "";

        public DGVForm2(string log, DataTable dt, string str)
        {
            InitializeComponent();
            logPath = log;
            dataGridView1.DataSource = dt;
            fileTXT = str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //キャンセルボタン
            this.closeD();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //実行処理
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                setFile(saveFileDialog1.FileName);
            }
            closeD();
        }

        private void setFile(string str)
        {
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("写研テキストファイルを開く:" + fileTXT);
            Encoding enc = Encoding.GetEncoding("shift_jis");
            //写研テキストファイルを開く
            List<string> lines = new List<string>();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileTXT, enc);
                while (sr.Peek() > -1)
                {
                    lines.Add(sr.ReadLine());
                }
            }
            catch(Exception ex)
            {                
                cl.logError(fileTXT + "の読み込み失敗：", ex);
                return;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }

            //変換処理
            lines.Remove("\n");
            replaceT(lines);
            //書き込み
            cl.logInfo("書き込み開始：" + str);
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(str, false, Encoding.GetEncoding("shift_jis"));
                foreach (string line in lines) sw.WriteLine(line);
            }
            catch (Exception ex)
            {
                cl.logError(str + "の書き込み失敗：", ex);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    //cl.dispose();
                    lines.Clear();
                }
            }
        }

        private void replaceT(List<string> lines)
        {
            //タグの処理(優先順位のあるタグから実行している)
            patTai(lines);
            fontSet(lines);
            colorSet(lines);
            patMath(lines);
            changeTag(lines);

            //test(lines);
        }

        private void changeTag(List<string> lines)
        {
            //置き換える変換タグ組み
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("パターン5、置き換える変換タグ組み開始");

            List<string> sk = new List<string>();
            List<string> ms = new List<string>();
            List<char> pat = new List<char>();
            getPat("5", sk, ms);

            foreach (string tmp in sk)
            {
                string[] sp = tmp.Split(SCHAR);
                if (sp.Length > 0)
                {
                    foreach (string s in sp)
                    {
                        pat.Add(convToInt32(s));
                    }
                }
                else
                {
                    //変換タグの設定情報のフォーマットが合っていない
                    cl.logDebug("変換タグの設定情報のフォーマットが合っていない");
                    return;
                }
            }
            sk.Clear();
            foreach(string tmp in ms)
            {
                string[] sp = tmp.Split(SCHAR);
                if (sp.Length > 0)
                {
                    foreach(string s in sp)
                    {
                        sk.Add(s);
                    }
                }
                else
                {
                    //変換タグの設定情報のフォーマットが合っていない
                    cl.logDebug("変換タグの設定情報のフォーマットが合っていない");
                    return;
                }
            }
            ms.Clear();
            if (pat.Count != sk.Count)
            {
                //変換タグの設定情報のフォーマットが合っていない
                cl.logDebug("対応タグの数が合っていない");
                return;
            }

            //変換を開始
            int i = 0, ii, index;
            int istart = 0;
            StringBuilder sb = new StringBuilder();
            while (i < lines.Count)
            {
                char[] ch = lines[i].ToCharArray();
                sb.Clear();
                ii = 0;
                istart = 0;
                while (ii < ch.Length)
                {
                    if (ch[ii] > INTCHAR)
                    {
                        index = pat.IndexOf(ch[ii]);
                        if (index >= 0)
                        {
                            sb.Append(lines[i].Substring(istart, ii - istart));
                            sb.Append(sk[index]);
                            istart = ii + 1;
                        }
                    }
                    ii++;
                }
                if (sb.Length > 0)
                {
                    sb.Append(lines[i].Substring(istart));
                    lines[i] = sb.ToString();
                }
                i++;
            }
            sb.Clear();
            cl.logInfo("パターン5、置き換える変換タグ組み終了");
            sk.Clear();
            pat.Clear();
        }

        private void colorSet(List<string> lines)
        {
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("パターン4、いろの設定開始");

            List<string> sk = new List<string>();
            List<string> ms = new List<string>();
            List<char> pat = new List<char>();
            getPat("4", sk, ms);

            if (sk.Count > 1)
            {
                foreach(string tmp in sk)
                {
                    string[] sp = tmp.Split(SCHAR);
                    if (sp.Length > 0)
                    {
                        foreach (string s in sp)
                        {
                            pat.Add(convToInt32(s));
                        }
                    }
                    else
                    {
                        //色の設定情報のフォーマットが合っていない
                        cl.logDebug("色の設定情報のフォーマットが合っていない");
                        return;
                    }
                }
            }
            else
            {
                //色の設定情報が足りない
                cl.logDebug("色の設定情報が足りない");
                return;
            }
            sk.Clear();
            if (ms.Count > 1)
            {
                foreach (string tmp in ms)
                {
                    string[] sp = tmp.Split(SCHAR);
                    if (sp.Length > 0)
                    {
                        foreach (string s in sp)
                        {
                            sk.Add(s);
                        }
                    }
                    else
                    {
                        //色の設定情報のフォーマットが合っていない
                        cl.logDebug("色の設定情報のフォーマットが合っていない");
                        return;
                    }
                }
            }
            else
            {
                //色の設定情報が足りない
                cl.logDebug("色の設定情報が足りない");
                return;
            }
            ms.Clear();
            //「F始」CO「F終」「/」色の設定「/」
            int i = 0;
            int ii;
            int istart = 0, iend = 0;
            string co = "";
            string val = "";
            StringBuilder sb = new StringBuilder();
            while (i < lines.Count)
            {
                char[] ch = lines[i].ToCharArray();
                sb.Clear();
                ii = 0;
                istart = 0;
                while (ii < ch.Length)
                {
                    if(ch[ii]> INTCHAR)
                    {
                        if (ch[ii] == pat[0])
                        {
                            //「F始」
                            string tmp = "";
                            while (ii++ < ch.Length && ch[ii] != pat[1])
                            {
                                //COを格納
                                tmp += ch[ii];
                            }
                            if (tmp == sk[0])
                            {
                                //COで、次の「/」色の設定「/」
                                iend = ii - 3;
                                sb.Append(lines[i].Substring(istart, iend - istart));
                                tmp = ch[ii].ToString();

                                if (++ii < ch.Length && ch[ii] == pat[2])
                                {
                                    tmp = "";
                                    while (++ii < ch.Length)
                                    {
                                        if (ch[ii] == pat[2])
                                        {
                                            break;
                                        }
                                        tmp += ch[ii];
                                    }
                                    //色の設定
                                    if (tmp.Length != 0)
                                    {
                                        co = "";
                                        int j = 0;
                                        if (tmp.IndexOf(pat[3]) > 0)
                                        {
                                            while (tmp[j] != pat[3] && j < tmp.Length)
                                            {
                                                co += tmp[j++];
                                            }
                                            //co=black
                                            int index = tmp.Substring(j).IndexOf(co);
                                            if (index > 0)
                                            {
                                                tmp = tmp.Substring(co.Length * 2 + index);
                                                val = "";
                                                foreach (char c in tmp)
                                                {
                                                    //F942,F943,F977,F975が既定したので4から数字が始める
                                                    bool flg = false;
                                                    int jj;
                                                    for (jj = 4; jj < pat.Count; jj++)
                                                    {
                                                        if (c == pat[jj])
                                                        {
                                                            flg = true;
                                                            break;
                                                        }
                                                    }
                                                    if (flg)
                                                    {
                                                        val += sk[jj];
                                                        if (int.Parse(val) > 100)
                                                        {
                                                            val = val.Substring(0, val.Length - 1);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            co = tmp;
                                            val = "100";
                                        }
                                        //<J color=",",%>のパターンで代入
                                        sb.Append(sk[1]).Append(co).Append(sk[2]).Append(val).Append(sk[3]);
                                    }
                                    else
                                    {
                                        //色の設定の取得に失敗した
                                        cl.logDebug(i + "行" + ii + "列：色の設定の取得に失敗した");
                                    }
                                }
                                istart = ii + 1;                                
                            }
                        }
                    }
                    ii++;
                }
                if (sb.Length > 0)
                {
                    sb.Append(lines[i].Substring(istart));
                    lines[i] = sb.ToString();
                }
                i++;
            }
            cl.logInfo("パターン4、いろの設定終了");
            sb.Clear();
            sk.Clear();
            pat.Clear();
        }

        private void test(List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < lines.Count; i++)
            {
                char[] ch = lines[i].ToCharArray();
                int ii = 0;
                sb.Clear();
                while (ii < ch.Length) 
                {
                    if (ch[ii] > INTCHAR)
                    {
                        sb.Append(ch[ii]);
                        sb.Append(' ');
                    }
                    ii++;
                }
                lines[i] = sb.ToString();
            }
            sb.Clear();
        }

        private void fontSet(List<string> lines)
        {
            //「F始」〇「F終」「/」「E1」～「E3」のパターン化
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("パターン2、フォント設定開始");

            List<string> sk = new List<string>();
            List<string> ms = new List<string>();
            List<char> pat = new List<char>();
            List<string> font = new List<string>();
            getPat("2", sk, ms);

            foreach (string tmp in sk)
            {
                string[] sp = tmp.Split(SCHAR);
                if (sp.Length > 0)
                {
                    foreach (string s in sp)
                    {
                        pat.Add(convToInt32(s));
                    }
                }
            }
            sk.Clear();

            foreach (string tmp in ms)
            {
                string[] sp = tmp.Split(SCHAR);
                if (sp.Length > 0)
                {
                    foreach (string s in sp)
                    {
                        font.Add(s);
                    }
                }
            }
            ms.Clear();

            //最初の要素Start=「体」とする
            int ii, start, end;
            bool flg, first;
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < lines.Count; i++)
            {
                char[] ch = lines[i].ToCharArray();
                ii = 0;
                start = 0;
                end = 0;
                sb.Clear();
                while (ii < ch.Length)
                {
                    if (ch[ii] > INTCHAR)
                    {
                        first = true;
                        //「F始」で始めるパターンかを判定
                        while (ii < ch.Length && ch[ii] == pat[0])
                        {
                            //xx=KJ,KN,FP,FC,GR,RBなので
                            string tmp = "";
                            flg = false;
                            while (ch[++ii] != pat[1] && ii < ch.Length)
                            {
                                tmp += ch[ii].ToString();
                            }
                            foreach (string f in font)
                            {
                                if (tmp == f)
                                {
                                    flg = true;
                                    break;
                                }
                            }
                            if (flg)
                            {
                                if (first)
                                {
                                    first = false;
                                    end = ii - 3;
                                }
                                //「F終」「/」ときてるので
                            Label1:
                                //どこかに出力tmp
                                if (ch[++ii] == pat[2])
                                {   //次の「/」が来るまで回す
                                    tmp = "";
                                    while (ch[++ii] != pat[2] && ii < ch.Length)
                                    {
                                        tmp += ch[ii].ToString();
                                    }
                                    //「E1」～「E3」のとき
                                    if (++ii < ch.Length && ch[ii] > INTCHAR)
                                    {
                                        flg = false;
                                        for (int j = 3; j < pat.Count; j++)
                                        {
                                            if (ch[ii] == pat[j])
                                            {
                                                flg = true;
                                                break;
                                            }
                                        }
                                        if (flg == true) 
                                        {
                                            goto Label1;
                                        }
                                    }
                                }
                            }
                        }
                        if (first == false)
                        {
                            sb.Append(lines[i].Substring(start, end - start));
                            start = ii;
                        }
                    }
                    ii++;
                }
                sb.Append(lines[i].Substring(start));
                lines[i] = sb.ToString();
            }
            cl.logInfo("パターン2、フォント設定終了");
            sb.Clear();
            font.Clear();
            pat.Clear();
        }

        private void patTai(List<string> lines)
        {
            //「体」で始めるパターンを消す作業
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("パターン1、「体」で始めるパターンを消す作業開始");

            List<string> sk = new List<string>();
            List<string> ms = new List<string>();
            List<char> pat = new List<char>();
            getPat("1", sk, ms);
            
            foreach(string tmp in sk)
            {
                string[] sp = tmp.Split(SCHAR);
                if (sp.Length > 0)
                {
                    foreach(string s in sp)
                    {
                        pat.Add(convToInt32(s));
                    }
                }
            }
            sk.Clear();
            ms.Clear();
            if (pat.Count <= 0)
            {
                //タグ取得失敗
                cl.logDebug("タグ取得失敗");
                return;
            }
            if (pat.Count == 1)
            {
                //パターンがただしくない
                cl.logDebug("パターンがただしくない");
                return;
            }
            //最初の要素Start=「体」とする
            int ii = 0;
            int start = 0, end;
            bool flg = false;
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < lines.Count; i++)
            {
                ii = 0; start = 0;
                sb.Clear();
                char[] ch = lines[i].ToCharArray();
                while (ii < lines[i].Length)
                {
                    if (ch[ii] >= INTCHAR)
                    {
                        //「体」で始めるパターンかを判定
                        if (ch[ii] == pat[0])
                        {
                            //「体」〇「－」〇でパターン化
                            end = ii;
                            ii++;
                            while(ii < lines[i].Length)
                            {
                                flg = false;
                                for (int j = 0; j < pat.Count; j++)
                                {
                                    if (ch[ii] == pat[j])
                                    {
                                        flg = true;
                                        break;
                                    }
                                }
                                if (flg == false) break;
                                ii++;
                            }
                            //start->iiまでがパターン分になる
                            sb.Append(lines[i].Substring(start, end - start));
                            start = ii;
                        }
                    }
                    ii++;
                }
                sb.Append(lines[i].Substring(start));
                lines[i] = sb.ToString();
            }
            cl.logInfo("パターン1、「体」で始めるパターンを消す作業終了");
            sb.Clear();
            pat.Clear();
        }

        private void patMath(List<string> lines)
        {
            ClassLog cl = new ClassLog(logPath);
            cl.logInfo("パターン3、数式変換開始");

            //パターン3のタグを
            List<string> sk = new List<string>();
            List<string> ms = new List<string>();

            getPat("3", sk, ms);

            //最初の要素StartとEndとする
            char start;
            List<char> end = new List<char>();
            List<string> msMath = new List<string>();
            string[] sp = sk[0].Split(SCHAR);
            start = convToInt32(sp[0]);
            for (int j = 1; j < sp.Length; j++) end.Add(convToInt32(sp[j]));

            sp = ms[0].Split(SCHAR);
            foreach (string str in sp) msMath.Add(str);

            //
            int i = 0;
            int istart;
            StringBuilder sb = new StringBuilder();
            string tmp;
            while (i < lines.Count)
            {
                char[] ch = lines[i].ToCharArray();
                int ii = 0;
                istart = 0;
                sb.Clear();
                while (ii < ch.Length)
                {
                    if (ch[ii] > INTCHAR)
                    {
                        if (ch[ii] == start)
                        {
                            //「E2」〇「E1」
                            tmp = "";
                            sb.Append(lines[i].Substring(istart, ii - istart));
                            if (msMath.Count > 0)
                                sb.Append(msMath[0]);
                            else
                                cl.logDebug(i + "行：<MATH>定義（msタグ）にエラー");
                            istart = ii + 1;
                            while (ii < ch.Length)
                            {
                                //end->「E1」が「送2」か判定する
                                if (ch[++ii] == end[0] || ii == ch.Length - 1)
                                {                                    
                                    istart = ii + 1;
                                    break;
                                }                                
                                tmp += ch[ii];                                
                            }
                            //本番処理:〇
                            for(int j = 1; j < sk.Count; j++)
                            {
                                string[] ss = sk[j].Split(SCHAR);
                                switch (ss.Length)
                                {
                                    case 1:
                                        tmp = subMath1(tmp, ss[0], ms[j]);
                                        break;
                                    case 2:
                                        tmp = subMath2(tmp, ss[0], ss[1], ms[j]);
                                        break;
                                    default:
                                        cl.logDebug("パターン3のSKタグの宣言が合ってない");
                                        break;
                                }
                            }
                            if(tmp=="")
                                cl.logDebug("パターン3のSKタグの宣言が合ってない");
                            else
                            {
                                int ist = 0, ind;
                                //string lol = "</MX<MX>><MF>";
                                //while (true)
                                //{
                                //    ind = tmp.Substring(ist).IndexOf(lol);
                                //    if (ind >= 0)
                                //    {
                                //        sb.Append(tmp.Substring(ist, ind - ist));
                                //        ist = ind + lol.Length;
                                //    }
                                //    else break;
                                //}
                                sb.Append(tmp.Substring(ist));
                            }
                                
                            //end->「E1」が「送2」か判定したところに</MATH>追加
                            if (msMath.Count > 1)
                                sb.Append(msMath[1]);
                            else
                                cl.logDebug(i + "行：</MATH>定義（msタグ）にエラー");
                        }
                    }
                    ii++;
                }
                sb.Append(lines[i].Substring(istart));
                lines[i++] = sb.ToString();
            }
            cl.logInfo("パターン3、数式変換終了");
            end.Clear();
            sk.Clear();
            ms.Clear();
            msMath.Clear();
            sb.Clear();
        }

        private string subMath1(string str, string sk, string ms)
        {
            //< S cd = 3775 >;< MX >.< MF > *</ MX >; 下後付き* 記号
            StringBuilder sb = new StringBuilder();
            int index, istart, cnt=1;

            string[] sp = ms.Split(SCHAR);
            if (sp.Length != 2) return "";
            istart = 0;
            while (true)
            {
                index = str.Substring(istart).IndexOf(sk);
                if (index >= 0)
                {
                    sb.Append(str.Substring(istart, index - cnt));
                    sb.Append(sp[0]).Append(str[istart + index - 1]).Append(sp[1]);
                    istart += index + sk.Length;
                    if (istart > str.Length) break;
                }
                else
                    break;
            }

            sb.Append(str.Substring(istart));
            return sb.ToString();
        }

        private string subMath2(string str, string sk1, string sk2, string ms)
        {
            //F8E7.F8E8;<MX>.<MF>.</MX>;
            int i, cnt = 1;
            string tmp = "";
            StringBuilder sb = new StringBuilder();
            string[] sp = ms.Split(SCHAR);
            if (sp.Length != 3) return "";

            i = 0;
            char[] ch = str.ToCharArray();
            while (i < ch.Length)
            {
                if (ch[i] == convToInt32(sk1))
                {
                    tmp = sb.ToString();
                    sb.Clear();
                    sb.Append(tmp.Substring(0, tmp.Length - cnt));
                    sb.Append(sp[0]).Append(tmp.Substring(tmp.Length - cnt)).Append(sp[1]);
                    tmp = "";
                    while (++i < ch.Length)
                    {
                        if (ch[i] == convToInt32(sk2))
                        {
                            sb.Append(tmp).Append(sp[2]);
                            break;
                        }
                        tmp += ch[i];
                    }
                }
                else
                    sb.Append(ch[i]);
                i++;
            }


            return sb.ToString();
        }

        private void getPat(string index, List<string> sk, List<string> ms)
        {
            for(int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (index == dataGridView1.Rows[i].Cells[0].Value.ToString())
                {
                    sk.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    ms.Add(dataGridView1.Rows[i].Cells[2].Value.ToString());
                }
            }
        }

        private char convToInt32(string str)
        {
            // F97F がフォントで定義されていないため、ズレが生じた
            //F9xxとF0xxのとき異なるので後で変更する
            int a = Convert.ToInt32(str, 16);
            int b = (a < 63871) ? (a - 4772) : (a - 4773);//F97F->63871
            char c = Convert.ToChar(b);

            return c;
        }

        private void closeD()
        {
            //DGVFormを閉じる処理
            this.Close();
        }
    }
}
