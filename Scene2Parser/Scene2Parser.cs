using YAMSE.DataLayer;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace YAMSE
{
    public static class Scene2Parser
    {
        private const int maxObjectNameLength = 50;
        private const int IdLen = 2; // length of ID

        public static void LoadScene(MemoryStream inputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            byte[] tmpBuff = inputStream.ToArray();

            bool headerParsed = false;
            bool sectionEnded = false;
            bool definitionsParsed = false;

            int i = 0;
            int objectID = 0;

            // loading
            bool loadingHeaderShown = false;
            bool loadingObjectsShown = false;
            bool loadingObjectsDefinitionsShown = false;
            bool loadingInitScriptsShown = false;

            const int IdLen = 2; // length of ID

            Scene2Section currSection = null;

            int positionIterator = 0;

            while (i < tmpBuff.Length)
            {
                if (!headerParsed)
                {
                    // parse header
                    if (!loadingHeaderShown)
                    {
                        loggingList.Add("Loading header...");
                        loadingHeaderShown = true;
                    }
                    if (tmpBuff[i] == 0 && tmpBuff[i + 1] == 0x40)
                    {
                        headerParsed = true;

                        scene2Data.header.Magic = tmpBuff.Take(2).ToList();
                        scene2Data.header.Size = tmpBuff.Skip(2).Take(4).ToList();
                        scene2Data.header.Content = tmpBuff.Skip(6).Take(i - 6).ToList();

                        var sectionLen = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                        scene2Data.standardObjectsLength = BitConverter.ToInt32(sectionLen, 0);

                        currSection = new Scene2Section() 
                        { 
                            Position = positionIterator, 
                            SectionName = "Objects", 
                            SectionType = NodeType.Object, 
                            SectionStart = i, 
                            SectionIdArr = tmpBuff.Skip(i).Take(2).ToArray(),
                            SectionLength = BitConverter.ToInt32(sectionLen, 0) 
                        };

                        positionIterator++;

                        scene2Data.Sections.Add(currSection);

                        loggingList.Add("Loading objects...");
                        loadingObjectsShown = true;

                        i += 6;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    // parse dncs objects
                    if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                    {
                        Dnc currDnc = new Dnc
                        {
                            dncType = DncType.Unknown,
                        };

                        // get length
                        int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                        currDnc.objectIDArr = tmpBuff.Skip(i).Take(IdLen).ToArray();
                        currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();

                        currDnc.rawDataBackup = new byte[currDnc.rawData.Length];
                        currDnc.rawData.CopyTo(currDnc.rawDataBackup, 0);

                        currDnc.dncType = GetObjectType(currDnc);
                        currDnc.ID = objectID;
                        currDnc.name = GetNameByID(currDnc);

                        currSection.Dncs.Add(currDnc);

                        objectID++;
                        i += IdLen + lenCurr;
                    }
                    else
                    {
                        if (i == currSection.SectionEnd)
                        {
                            sectionEnded = true;
                            objectID = 0;
                        }
                    }

                    
                    if (sectionEnded)
                    {
                        // parse dncs definitions
                        if (tmpBuff[i] == 0x20 && tmpBuff[i + 1] == 0xAE)
                        {
                            sectionEnded = false;

                            var sectionLen = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                            currSection = new Scene2Section()
                            {
                                Position = positionIterator,
                                SectionName = "Object definitions",
                                SectionType = NodeType.Definition,
                                SectionStart = i,
                                SectionIdArr = tmpBuff.Skip(i).Take(2).ToArray(),
                                SectionLength = BitConverter.ToInt32(sectionLen, 0)
                            };

                            positionIterator++;

                            scene2Data.Sections.Add(currSection);

                            i += 6;

                            loggingList.Add("Loading object definitions...");
                            loadingObjectsDefinitionsShown = true;
                        }

                        // init scripts
                        if (tmpBuff[i] == 0x50 && tmpBuff[i + 1] == 0xAE)
                        {
                            sectionEnded = false;

                            if (!loadingInitScriptsShown)
                            {
                                loggingList.Add("Loading init scripts...");
                                loadingInitScriptsShown = true;

                                var sectionLen = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                                currSection = new Scene2Section()
                                {
                                    Position = positionIterator,
                                    SectionName = "Init scripts",
                                    SectionType = NodeType.InitScript,
                                    SectionStart = i,
                                    SectionIdArr = tmpBuff.Skip(i).Take(2).ToArray(),
                                    SectionLength = BitConverter.ToInt32(sectionLen, 0)
                                };

                                scene2Data.Sections.Add(currSection);

                                positionIterator++;

                                i += 6; // might cause problems
                            }
                        }

                        if (sectionEnded)
                        { // if still true we have found unknown section

                            sectionEnded = false;
                            int sectionLen = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(2).Take(4).ToArray(), 0);

                            currSection = new Scene2Section()
                            {
                                Position = positionIterator,
                                SectionName = $"Unknown {positionIterator}",
                                SectionType = NodeType.Unknown,
                                SectionStart = i,
                                SectionIdArr = tmpBuff.Skip(i).Take(2).ToArray(),
                                SectionLength = sectionLen
                            };

                            scene2Data.Sections.Add(currSection);

                            positionIterator++;

                            // get dncs

                            i += 6; // move to first dnc

                            int tmpPos = 6;
                            objectID = 0;
                            while(tmpPos < sectionLen)
                            {
                                int dncLen = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(2).Take(4).ToArray(), 0);
                                
                                Dnc currDnc = new Dnc
                                {
                                    dncType = DncType.Unknown,
                                    name = $"Unknown {objectID}",
                                    ID = objectID,
                                    objectIDArr = tmpBuff.Skip(i).Take(IdLen).ToArray(),
                                    rawData = tmpBuff.Skip(i).Skip(IdLen).Take(dncLen - IdLen).ToArray(),
                                };

                                currSection.Dncs.Add(currDnc);

                                tmpPos += dncLen;
                                objectID++;
                                i += dncLen; // move to next dnc
                            }
                        }
                    }

                    if (tmpBuff[i] == 0x21 && tmpBuff[i + 1] == 0xAE)
                    {
                        Dnc currDnc = new Dnc
                        {
                            dncType = DncType.Unknown,
                        };

                        // get length
                        int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                        currDnc.objectIDArr = tmpBuff.Skip(i).Take(IdLen).ToArray();
                        currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();

                        currDnc.rawDataBackup = new byte[currDnc.rawData.Length];
                        currDnc.rawData.CopyTo(currDnc.rawDataBackup, 0);

                        currDnc.dncType = GetObjectDefinitionType(currDnc);
                        currDnc.ID = objectID;
                        currDnc.name = GetNameByDefinitionID(currDnc);

                        currSection.Dncs.Add(currDnc);

                        objectID++;
                        i = i + IdLen + lenCurr;
                        //i--;
                    }
                    else
                    {
                        if (i == currSection.SectionEnd)
                        {
                            sectionEnded = true;
                            objectID = 0;
                        }
                    }

                    if (tmpBuff[i] == 0x51 && tmpBuff[i + 1] == 0xAE)
                    {
                        Dnc currDnc = new Dnc
                        {
                            dncType = DncType.Unknown,
                        };

                        // get length
                        int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                        currDnc.objectIDArr = tmpBuff.Skip(i).Take(IdLen).ToArray();
                        currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();

                        currDnc.rawDataBackup = new byte[currDnc.rawData.Length];
                        currDnc.rawData.CopyTo(currDnc.rawDataBackup, 0);

                        currDnc.dncType = DncType.InitScript;
                        currDnc.name = GetNameByDefinitionID(currDnc);
                        currDnc.ID = objectID;

                        currSection.Dncs.Add(currDnc);

                        objectID++;
                        i = i + IdLen + lenCurr;
                    }
                }
            }
        }

        public static void SaveScene(Stream outputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            loggingList.Add("Starting to save the file.");

            outputStream.Write(scene2Data.header.Magic.ToArray(), 0, scene2Data.header.Magic.ToArray().Length);

            // calculate file size
            var fileSize = 6 + scene2Data.header.Content.Count + (scene2Data.Sections.Count * 6);

            fileSize += scene2Data.Sections.SelectMany(x => x.Dncs).Sum(x => x.rawData.Length);
            fileSize += scene2Data.Sections.SelectMany(x => x.Dncs).Count() * 2;

            var fileSizeArr = BitConverter.GetBytes(fileSize);
            outputStream.Write(fileSizeArr, 0, fileSizeArr.Length);

            outputStream.Write(scene2Data.header.Content.ToArray(), 0, scene2Data.header.Content.ToArray().Length);

            foreach (var section in scene2Data.Sections.OrderBy(x => x.Position))
            {
                outputStream.Write(section.SectionIdArr, 0, section.SectionIdArr.Length);

                var dncLenArr =  BitConverter.GetBytes(section.Dncs.Sum(x => x.rawData.Length) + (section.Dncs.Count * 2) + 6);
                outputStream.Write(dncLenArr, 0, dncLenArr.Length);

                foreach (var dnc in section.Dncs.OrderBy(x => x.ID))
                {
                    outputStream.Write(dnc.objectIDArr, 0, dnc.objectIDArr.Length);
                    outputStream.Write(dnc.rawData, 0, dnc.rawData.Length);
                }
            }

            /*
            
            objectsDncs
            objectDefinitionsDncs
            initScriptsDncs

             */

            outputStream.Close();
            loggingList.Add("File saving done.");
        }

        public static string GetStringFromDnc(Dnc dnc, bool useBackup = false)
        {
            if (useBackup)
            {
                return Encoding.UTF8.GetString(dnc.rawDataBackup.Skip(dnc.name.Length + 41).ToArray());
            }
            return Encoding.UTF8.GetString(dnc.rawData.Skip(dnc.name.Length + 41).ToArray());
        }

        public static string GetStringFromInitScript(Dnc dnc, bool useBackup = false)
        {
            if (useBackup)
            {
                return Encoding.UTF8.GetString(dnc.rawDataBackup.Skip(dnc.name.Length + 13).ToArray());
            }
            return Encoding.UTF8.GetString(dnc.rawData.Skip(dnc.name.Length + 13).ToArray());
        }

        public static void UpdateStringInDnc(Dnc dnc, string text)
        {
            var startArray = dnc.rawData.Take(dnc.name.Length + 41).ToArray();

            // recalculate array length
            var textInBytes = Encoding.UTF8.GetBytes(text);
            var bytesLen = BitConverter.GetBytes(textInBytes.Length + startArray.Length + IdLen);

            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[i] = bytesLen[i];
            }

            // recalculate additional size
            bytesLen = BitConverter.GetBytes(textInBytes.Length);
            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[dnc.name.Length + 37 + i] = bytesLen[i];
            }

            bytesLen = BitConverter.GetBytes(textInBytes.Length + 20);
            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[dnc.name.Length + 23 + i] = bytesLen[i];
            }

            dnc.rawData = startArray.Concat(textInBytes).ToArray();
        }

        private static string GetNameByDefinitionID(Dnc dnc)
        {
            switch (dnc.dncType)
            {
                case DncType.Unknown:
                    return $"Unknown {dnc.ID}";
                case DncType.MovableBridge:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Car:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Script:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.InitScript:

                    var len = dnc.rawData[5];

                    return Encoding.ASCII.GetString(dnc.rawData, 0x9, len);

                //return GetCStringFromByteArray(dnc.rawData.Skip(0x9).Take(maxObjectNameLength).ToArray());


                case DncType.PhysicalObject:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Door:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Tram:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.GasStation:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.PedestrianSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Enemy:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Plane:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Player:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.TrafficSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByDefinitionID));
            }
        }

        private static string GetCStringFromByteArray(byte[] arr)
        {
            return Encoding.ASCII.GetString(arr, 0, Array.IndexOf(arr, (byte)0));
        }

        private static DncType GetObjectType(Dnc dnc)
        {
            if (dnc.rawData[4] == 0x10)
            { // either LMAP or sector
                dnc.dncType = DncType.LMAP;
                dnc.name = GetNameByID(dnc);

                if (dnc.rawData.FindIndexOf(Encoding.ASCII.GetBytes("LMAP")).Any())
                { // is LMAP
                    return DncType.LMAP;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x01, 0xB4, 0xF2 }).Any())
                    {
                        return DncType.Sector;
                    }
                    else
                    {
                        return DncType.Unknown;
                    }
                }
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0C }).Any())
                {
                    return DncType.Occluder;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x09 }).Any())
                    {
                        return DncType.Model;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
                        {
                            return DncType.Sound;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x03 }).Any())
                            {
                                return DncType.Camera;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0E }).Any())
                                {
                                    return DncType.CityMusic;
                                }
                                else
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x02 }).Any())
                                    {
                                        return DncType.Light;
                                    }
                                }
                            }
                        }
                        return DncType.Standard;
                    }
                }
            }
        }

        private static string GetNameByID(Dnc dnc)
        {
            switch (dnc.dncType)
            {
                case DncType.Unknown:
                    return $"Unknown {dnc.ID}";
                case DncType.LMAP:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Standard:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.Sector:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DncType.Occluder:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.Model:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.Sound:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.Camera:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.CityMusic:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case DncType.Light:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByID));
            }
        }

        private static DncType GetObjectDefinitionType(Dnc dnc)
        {
            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
            {
                return DncType.Car;
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x14 }).Any())
                {
                    return DncType.MovableBridge;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x5 }).Any())
                    {
                        return DncType.Script;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x23 }).Any())
                        {
                            return DncType.PhysicalObject;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x6 }).Any())
                            {
                                return DncType.Door;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x8 }).Any())
                                {
                                    return DncType.Tram;
                                }
                                else
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x19 }).Any())
                                    {
                                        return DncType.GasStation;
                                    }
                                    else
                                    {
                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x12 }).Any())
                                        {
                                            return DncType.PedestrianSetup;
                                        }
                                        else
                                        {
                                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x1B }).Any())
                                            {
                                                return DncType.Enemy;
                                            }
                                            else
                                            {
                                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x16 }).Any())
                                                {
                                                    return DncType.Plane;
                                                }
                                                else
                                                {
                                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x2 }).Any())
                                                    {
                                                        return DncType.Player;
                                                    }
                                                    else
                                                    {
                                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0xC }).Any())
                                                        {
                                                            return DncType.TrafficSetup;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return DncType.Unknown;
            }
        }
    }
}
