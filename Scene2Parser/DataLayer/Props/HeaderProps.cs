using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using YAMSE.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace YAMSE.DataLayer
{
    public class HeaderProps : IDncProps
    {
        public int DataBegin { get; set; }

        public float ViewDistance { get; set; }
        public float CameraDistance { get; set; }
        public float NearClipping { get; set; }
        public float FarClipping { get; set; }
        public int HeaderLength { get; set; }
        public string Text { get; set; }
        public string TextPrevious { get; set; }

        public byte[] RawData { get; set; }

        private readonly Dnc _dnc;

        public HeaderProps(Dnc dnc)
        {
            _dnc = dnc;
            DataBeginLocator();

            RevertData();
        }

        public int DataBeginLocator()
        {
            DataBegin = 0;
            return DataBegin;
        }

        public void RevertData()
        {
            HeaderLength = _dnc.RawData.Skip(DataBegin + 2).Take(1).First();
            Text = Scene2Parser.GetStringFromDnc(_dnc, DataBegin, 10);
            TextPrevious = Text;
            ViewDistance = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + Text.Length + 60).Take(4).ToArray(), 0);
            CameraDistance = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + Text.Length + 50).Take(4).ToArray(), 0);
            NearClipping = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + Text.Length + 70).Take(4).ToArray(), 0);
            FarClipping = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + Text.Length + 74).Take(4).ToArray(), 0);
        }

        public virtual void SaveData()
        {
            //Scene2Parser.UpdateStringInDnc(_dnc, Text, 10);
            // update text
            var startOfArray = _dnc.RawDataBackup.Take(10).ToArray();
            var endOfArray = _dnc.RawDataBackup.Skip(TextPrevious.Length + 10).ToArray();
            var textInBytes = Encoding.UTF8.GetBytes(Text);

            HeaderLength = Text.Length + 12;
            TextPrevious = Text;

            // recalculate array length
            _dnc.RawData = startOfArray.Concat(textInBytes).Concat(endOfArray).ToArray();

            Scene2Parser.WriteToDnc(_dnc, DataBegin, HeaderLength, 2, false);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, ViewDistance, Text.Length + 60);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, CameraDistance, Text.Length + 50);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, NearClipping, Text.Length + 70);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, FarClipping, Text.Length + 74);
     
        }
    }
}
