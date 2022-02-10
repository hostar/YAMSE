using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace YAMSE.Helpers
{
    public class DefLoader
    {
        public Dictionary<int, string> Data = new Dictionary<int, string>();

        public DefLoader(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);

            byte[] buffer = new byte[4];

            fs.Read(buffer);

            int len = BitConverter.ToInt32(buffer);

            fs.Read(buffer);

            int read_result;

            IList<int> list = new List<int>();
            for (int i = 0; i < (len - 1); i++)
            {
                read_result = fs.Read(buffer);
                if (ShouldStopRead(read_result))
                {
                    break;
                }
                int id = BitConverter.ToInt32(buffer);
                id--;

                read_result = fs.Read(buffer);
                if (ShouldStopRead(read_result))
                {
                    break;
                }

                if (id == 99999999)
                {

                }

                int pos = BitConverter.ToInt32(buffer);

                list.Add(id);
                //fs.Seek(4L, SeekOrigin.Current);
            }
            foreach (int id in list)
            {
                string str = Read_C_String(fs);

                if (!Data.ContainsKey(id))
                {
                    Data.Add(id, str);
                }
            }
        }

        private bool ShouldStopRead(int result)
        {
            return result == 0 ? true : false;
        }

        private string Read_C_String(FileStream fileStream)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int tmp = fileStream.ReadByte();
            while (tmp != 0)
            {
                stringBuilder.Append((char)tmp);
                tmp = fileStream.ReadByte();
            }
            return stringBuilder.ToString();
        }

        public string GetRecordById(int id)
        {
            if (Data.ContainsKey(id))
            {
                return Data[id];
            }
            return string.Empty;
        }
    }
}
