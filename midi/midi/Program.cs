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
            byte[] data = LoadData(path);

            while (data.Length > 0)
            {
                //check chank type
                byte[] chankType = data.Take(4).ToArray();
                data = data.Skip(4).ToArray();

                //chank length
                int chankLength = 0;
                chankLength = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0);
                data = data.Skip(4).ToArray();

                //chank data
                byte[] chankData = data.Take(chankLength).ToArray();
                data = data.Skip(chankLength).ToArray();

                if (CheckSame(chankType.ToArray(), new List<byte>() { 0x4D, 0x54, 0x68, 0x64 }.ToArray())) HeaderChank(chankData);
                else if (CheckSame(chankType.ToArray(), new List<byte>() { 0x4D, 0x54, 0x72, 0x6B }.ToArray())) TrackChank(chankData);
                else
                {
                    Console.WriteLine("this is not a midi file");
                }
            }

            Console.WriteLine("-----------------finish loading");
            Console.WriteLine("channelNumber OnDeltaTime OffDeltaTime NoteNumber");
            foreach (var note in Notes)
            {
                Console.Write("{0} ", note.ChannelNumber);
                Console.Write("{0} ", note.OnDeltaTime);
                Console.Write("{0} ", note.OffDeltaTime);
                Console.Write("{0} ", note.NoteNumber);
                Console.Write("{0} ", note.OffFlag);
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        static byte[] LoadData(string path)
        {
            byte[] data = new byte[0];
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception e) { return data; }
            return data;
        }

        static void HeaderChank(byte[] data)
        {
            Console.WriteLine("-----------------header chank");
            Console.WriteLine("chank length:{0}", data.Length);

            //format
            int format = BitConverter.ToInt16(data.Take(2).Reverse().ToArray(), 0);
            data = data.Skip(2).ToArray();
            Console.WriteLine("format：{0}", format);

            //track
            int trackNum = BitConverter.ToInt16(data.Take(2).Reverse().ToArray(), 0);
            data = data.Skip(2).ToArray();
            Console.WriteLine("trackNum：{0}", trackNum);

            //time resolution
            int timeResolution = BitConverter.ToInt16(data.Take(2).Reverse().ToArray(), 0);
            data = data.Skip(2).ToArray();
            if (timeResolution < 0x80) Console.WriteLine("time resolution：{0}", timeResolution);
            else Console.WriteLine("time resolution：{0}", timeResolution);
        }

        static List<Note> Notes = new List<Note>();
        static void TrackChank(byte[] data)
        {
            Console.WriteLine("-----------------track chank");
            Console.WriteLine("chank length:{0}", data.Length);

            int lastStatus = -1;
            int totalTime = 0;
            while (data.Length > 0)
            {
                //delta time
                int deltaTime = VariableLengthDataToInt(ref data);
                totalTime += deltaTime;

                int status = data[0];
                if (status < 0x80) status = lastStatus; //running status rule:前のステータスと同じ場合省略可能
                else data = data.Skip(1).ToArray();
                lastStatus = status;

                //case sysEx event
                if (status == 0xf0 || status == 0xf7)
                {
                    Console.WriteLine("sysEx event");
                    int dataLength = VariableLengthDataToInt(ref data);

                    //todo:エクスクルーシブメッセージ
                    byte[] message = data.Take(dataLength).ToArray();
                    data = data.Skip(dataLength).ToArray();

                    if (status == 0xf7) data = data.Skip(1).ToArray();
                    continue;
                }

                //case meta event
                if (status == 0xff)
                {
                    Console.WriteLine("meta event");
                    int eventType = data[0];
                    data = data.Skip(1).ToArray();

                    int dataLength = VariableLengthDataToInt(ref data);

                    //todo:イベント
                    byte[] metaEventData = data.Take(dataLength).ToArray();
                    data = data.Skip(dataLength).ToArray();

                    continue;
                }

                //case midi event
                Console.WriteLine("midi event");
                if (status < 0x90)
                {
                    int channelNumber = status - 0x80;
                    int noteNumber = data[0];
                    int offVelocity = data[1];
                    data = data.Skip(2).ToArray();

                    var lastNoteOfNoteNumber = Notes.FindLast(n => n.NoteNumber == noteNumber);
                    lastNoteOfNoteNumber.SetOffInformation(totalTime, offVelocity);

                    continue;
                }
                else if (status < 0xa0)
                {
                    int channelNumber = status - 0x90;
                    int noteNumber = data[0];
                    int onVelocity = data[1];
                    data = data.Skip(2).ToArray();

                    if (onVelocity == 0)
                    {
                        var lastNoteOfNoteNumber = Notes.FindLast(n => n.NoteNumber == noteNumber);
                        lastNoteOfNoteNumber.SetOffInformation(totalTime, 0);

                        continue;
                    }

                    Note note = new Note(totalTime, channelNumber, noteNumber, onVelocity);
                    Notes.Add(note);

                    continue;
                }
                else if (status < 0xb0)
                {
                    data = data.Skip(3).ToArray();
                    Console.WriteLine("polyphonic key pressure");
                    continue;
                }
                else if (status < 0xc0)
                {
                    int channelNumber = status - 0xb0;
                    int controlNumber = data[0];
                    int controlerData = data[1];
                    data = data.Skip(2).ToArray();

                    Console.WriteLine("control change");

                    continue;
                }
                else if (status < 0xd0)
                {
                    data = data.Skip(2).ToArray();
                    Console.WriteLine("program change");
                    continue;
                }
                else if (status < 0xe0)
                {
                    data = data.Skip(2).ToArray();
                    Console.WriteLine("channel pressure");
                    continue;
                }
                else if (status < 0xf0)
                {
                    data = data.Skip(3).ToArray();
                    Console.WriteLine("pitch bend");
                    continue;
                }
                Console.WriteLine("status error");

                break;
            }
        }

        static int VariableLengthDataToInt(ref byte[] data)
        {
            List<byte> bytes = new List<byte>();
            while (true)
            {
                bytes.Add(data[0]);
                data = data.Skip(1).ToArray();
                if (bytes.Last() < 0x80) break;
            }
            int rtn = ToInt(bytes.ToArray());

            return rtn;
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
        public int OnDeltaTime { get; }
        public int ChannelNumber { get; }
        public int NoteNumber { get; }
        public int OnVelocity { get; set; }

        public int OffDeltaTime { get; private set; }
        public int OffVelocity { get; private set; }

        public bool OffFlag { get; private set; }

        public Note(int onDeltaTime, int channelNumber, int noteNumber, int onVelocity)
        {
            OnDeltaTime = onDeltaTime;
            ChannelNumber = channelNumber;
            NoteNumber = noteNumber;
            OnVelocity = onVelocity;

            OffFlag = false;
        }

        public void SetOffInformation(int offDeltaTIme, int offVelocity)
        {
            OffDeltaTime = offDeltaTIme;
            OffVelocity = offVelocity;

            OffFlag = true;
        }
    }
}
