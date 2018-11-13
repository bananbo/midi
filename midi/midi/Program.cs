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
            string path = Console.ReadLine();
            byte[] data;

            //load midi file
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
            }
            catch(Exception e) { return; }
       
            while (true)
            {
                //check chank type
                List<byte> chankType = new List<byte>();
                for (int i = 0; i < 4; i++) chankType.Add(data[i]);
                data = data.Skip(4).ToArray();

                //data length
                int chankLength = BitConverter.ToInt32(data.Take(4).ToArray(), 0);
                data = data.Skip(4).ToArray();

                //Console.WriteLine("チャンク長:{0}", chankLengthInt);

                if (CheckSame(chankType.ToArray(), new List<byte>() { 0x4D, 0x54, 0x68, 0x64 }.ToArray())) HeaderChank();
                else if (CheckSame(chankType.ToArray(), new List<byte>() { 0x4D, 0x54, 0x72, 0x6B }.ToArray())) TrackChank();
                else
                {
                    Console.WriteLine("this is not a midi file");
                    Console.ReadKey();
                }
            }

            


            ////データ長
            //byte[] chankLengh2 = new byte[4];
            //for (int i = 0; i < chankLengh2.Length; i++) chankLengh2[i] = data.Dequeue();
            //int chankLength2Int = ToInt(chankLengh2);
            //Console.WriteLine("トラックチャンク長：{0}", chankLength2Int);

            //while (true)
            //{
            //    //デルタタイム
            //    List<byte> deltas = new List<byte>();
            //    while (true)
            //    {
            //        deltas.Add(data.Dequeue());
            //        if (deltas.Last() < 0x80) break;
            //    }
            //    int delta = ToInt(deltas.ToArray());

            //    //イベント
            //    break;
            //}

            Console.ReadKey();
        }

        static void HeaderChank()
        {
            Console.WriteLine("-ここからヘッダチャンク");

            

            ////フォーマット
            //byte[] format = new byte[2];
            //for (int i = 0; i < format.Length; i++) format[i] = data.Dequeue();
            //int formatInt = ToInt(format);
            //Console.WriteLine("フォーマット：{0}", formatInt);

            ////トラック数
            //byte[] trackCount = new byte[2];
            //for (int i = 0; i < trackCount.Length; i++) trackCount[i] = data.Dequeue();
            //Console.WriteLine("トラック数：{0}", trackCount[1]);//簡単

            ////時間単位
            //byte[] time = new byte[2];
            //for (int i = 0; i < time.Length; i++) time[i] = data.Dequeue();
            //int timeInt = ToInt(time);
            //if (time[0] < 0x80) Console.WriteLine("時間単位：{0}", timeInt);
            //else Console.WriteLine("時間単位：{0}", timeInt);
        }

        static void TrackChank()
        {
            Console.WriteLine("-ここからトラックチャンク");

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
