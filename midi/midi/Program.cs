using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace midi
{
    class Program
    {
        static void Main(string[] args)
        {
            //ファイルを開く
            System.IO.FileStream fs = new System.IO.FileStream(
                @"C:\Users\banan\Google ドライブ\研究室\頼まれごと\ひでぽん\sunset clouds.mid",
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
            //ファイルを読み込むバイト型配列を作成する
            byte[] bs = new byte[fs.Length];
            //ファイルの内容をすべて読み込む
            fs.Read(bs, 0, bs.Length);
            //閉じる
            fs.Close();

            Queue<byte> data = new Queue<byte>();
            foreach (var a in bs) data.Enqueue(a);

            //ヘッダチャンク
            List<byte> chankType = new List<byte>();
            for (int i = 0; i < 4; i++) chankType.Add(data.Dequeue());
            if (CheckSame(chankType.ToArray(), new List<byte>() { 0x4D, 0x54, 0x68, 0x64 }.ToArray())) Console.WriteLine("-ここからヘッダチャンク");

            //データ長
            byte[] chankLength = new byte[4];
            for (int i = 0; i < chankLength.Length; i++) chankLength[i] = data.Dequeue();
            int chankLengthInt = ToInt(chankLength);
            Console.WriteLine("ヘッダチャンク長:{0}", chankLengthInt);

            //フォーマット
            byte[] format = new byte[2];
            for (int i = 0; i < format.Length; i++) format[i] = data.Dequeue();
            int formatInt = ToInt(format);
            Console.WriteLine("フォーマット：{0}", formatInt);

            //トラック数
            byte[] trackCount = new byte[2];
            for (int i = 0; i < trackCount.Length; i++) trackCount[i] = data.Dequeue();
            Console.WriteLine("トラック数：{0}", trackCount[1]);//簡単

            //時間単位
            byte[] time = new byte[2];
            for (int i = 0; i < time.Length; i++) time[i] = data.Dequeue();
            int timeInt = ToInt(time);
            if (time[0] < 0x80) Console.WriteLine("時間単位：{0}", timeInt);
            else Console.WriteLine("時間単位：{0}", timeInt);


            //トラックチャンク
            List<byte> chankType2 = new List<byte>();
            for (int i = 0; i < 4; i++) chankType2.Add(data.Dequeue());
            if (CheckSame(chankType2.ToArray(), new List<byte>() { 0x4D, 0x54, 0x72, 0x6B }.ToArray())) Console.WriteLine("-ここからトラックチャンク");

            //データ長
            byte[] chankLengh2 = new byte[4];
            for (int i = 0; i < chankLengh2.Length; i++) chankLengh2[i] = data.Dequeue();
            int chankLength2Int = ToInt(chankLengh2);
            Console.WriteLine("トラックチャンク長：{0}", chankLength2Int);

            while (true)
            {
                //デルタタイム
                List<byte> deltas = new List<byte>();
                while (true)
                {
                    deltas.Add(data.Dequeue());
                    if (deltas.Last() < 0x80) break;
                }
                int delta = ToInt(deltas.ToArray());

                //イベント
                break;
            }

            Console.ReadKey();
        }

        static bool CheckSame(byte[] check1, byte[] check2)
        {
            if (check1.Count() != check2.Count()) return false;
            for (int i = 0; i < check1.Count(); i++) if (check1[i] != check2[i]) return false;
            return true;
        }

        static int ToInt(byte[] data)
        {
            int Length = 0;
            for (int i = 0; i < data.Length; i++) Length += (int)Math.Pow(16, data.Length - 1 - i) * data[i];

            return Length;
        }
    }

    class Note
    {

    }
}
