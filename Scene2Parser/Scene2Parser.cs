using YAMSE.DataLayer;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace YAMSE
{
    public static class Scene2Parser
    {
        private const int maxObjectNameLength = 50;
        private const int IdLen = 2; // length of ID

        public static string SectionNameObjects = "Objects";
        public static string SectionNameDefs = "Object definitions";
        public static string SectionNameInitScripts = "Init scripts";

        public static void LoadScene(MemoryStream inputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            byte[] tmpBuff = inputStream.ToArray();

            bool headerParsed = false;
            bool sectionEnded = false;

            int i = 0;
            int objectID = 0;

            // loading
            bool loadingHeaderShown = false;

            const int IdLen = 2; // length of ID

            Scene2Section currSection = null;

            int positionIterator = 0;

            int antiInfiniteLoopLastValue = 0;

            bool headerParsingBeforeText = true;
            bool headerParsingText = false;
            bool headerParsingAfterText = false;

            while (i < tmpBuff.Length)
            {
                if ((antiInfiniteLoopLastValue == i) && (i != 0))
                {
                    throw new InvalidOperationException($"File corrupted near 0x{i:X} position. Last object loaded: {currSection.Dncs.Last().Name}");
                }
                antiInfiniteLoopLastValue = i;

                if (!headerParsed)
                {
                    // parse header
                    if (!loadingHeaderShown)
                    {
                        loggingList.Add("Loading header...");
                        loadingHeaderShown = true;
                    }

                    if (headerParsingBeforeText)
                    {
                        i += 16;
                        headerParsingBeforeText = false;
                        headerParsingText = true;
                    }

                    if (headerParsingText)
                    {
                        if (tmpBuff[i] == 0) // TODO: use HeaderLength instead
                        {
                            headerParsingAfterText = true;
                            i += 68;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (headerParsingAfterText)
                    {
                        if (tmpBuff[i] == 0 && tmpBuff[i + 1] == 0x40)
                        {
                            headerParsed = true;

                            scene2Data.Header.Magic = tmpBuff.Take(2).ToList();
                            scene2Data.Header.Size = tmpBuff.Skip(2).Take(4).ToList();
                            scene2Data.Header.Content = new Dnc()
                            {
                                Name = "Header",
                                dncKind = NodeType.Header,
                                RawData = tmpBuff.Skip(6).Take(i - 6).ToArray(),
                                RawDataBackup = tmpBuff.Skip(6).Take(i - 6).ToArray()
                            };

                            scene2Data.Header.Content.DncProps = new HeaderProps(scene2Data.Header.Content);

                            ParseKnownSection(scene2Data, loggingList, tmpBuff, ref i, ref currSection, ref positionIterator, "Loading objects...", SectionNameObjects, NodeType.Object);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    // load dncs

                    // definitions
                    if (tmpBuff[i] == 0x21 && tmpBuff[i + 1] == 0xAE)
                    {
                        LoadDnc(tmpBuff, ref i, ref objectID, IdLen, currSection);
                    }
                    else
                    {
                        if (i == currSection.SectionEnd)
                        {
                            sectionEnded = true;
                            objectID = 0;
                        }
                    }

                    // init scripts
                    if (tmpBuff[i] == 0x51 && tmpBuff[i + 1] == 0xAE)
                    {
                        LoadDnc(tmpBuff, ref i, ref objectID, IdLen, currSection);
                        if (i == tmpBuff.Length)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (i == currSection.SectionEnd)
                        {
                            sectionEnded = true;
                            objectID = 0;
                        }
                    }

                    // parse dncs objects
                    if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                    {
                        LoadDnc(tmpBuff, ref i, ref objectID, IdLen, currSection);
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

                            ParseKnownSection(scene2Data, loggingList, tmpBuff, ref i, ref currSection, ref positionIterator, "Loading object definitions...", SectionNameDefs, NodeType.Definition);
                        }

                        // init scripts
                        if (tmpBuff[i] == 0x50 && tmpBuff[i + 1] == 0xAE)
                        {
                            sectionEnded = false;
                            ParseKnownSection(scene2Data, loggingList, tmpBuff, ref i, ref currSection, ref positionIterator, "Loading init scripts...", SectionNameInitScripts, NodeType.InitScript);
                        }

                        if (sectionEnded)
                        { // if still true we have found unknown section
                            sectionEnded = false;
                            ParseUnknownSection(scene2Data, tmpBuff, ref i, out objectID, IdLen, out currSection, ref positionIterator);
                        }
                    }
                }
            }
        }

        private static void LoadDnc(byte[] inputBuff, ref int i, ref int objectID, int IdLen, Scene2Section currSection)
        {
            Dnc currDnc = new Dnc
            {
                dncType = DncType.Unknown,
            };

            // get length
            int lenCurr = BitConverter.ToInt32(inputBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

            currDnc.objectIDArr = inputBuff.Skip(i).Take(IdLen).ToArray();
            currDnc.RawData = inputBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();

            currDnc.RawDataBackup = new byte[currDnc.RawData.Length];
            currDnc.RawData.CopyTo(currDnc.RawDataBackup, 0);

            currDnc.dncKind = currSection.SectionType;
            currDnc.dncType = GetObjectDefinitionType(currDnc);

            if (currDnc.dncType == DncType.Unknown)
            {
                currDnc.dncType = GetObjectType(currDnc);
            }

            currDnc.ID = objectID;
            currDnc.Name = GetNameOfDnc(currDnc);

            PopulateProps(currDnc);

            currSection.Dncs.Add(currDnc);

            objectID++;
            i = i + IdLen + lenCurr;
        }

        public static void PopulateProps(Dnc currDnc)
        {
            switch (currDnc.dncType)
            {
                case DncType.Unknown:
                    break;
                case DncType.MovableBridge:
                    break;
                case DncType.Car:
                    break;
                case DncType.Script:
                    break;
                case DncType.InitScript:
                    break;
                case DncType.PhysicalObject:
                    break;
                case DncType.Door:
                    break;
                case DncType.Tram:
                    break;
                case DncType.GasStation:
                    break;
                case DncType.PedestrianSetup:
                    break;
                case DncType.Enemy:
                    currDnc.DncProps = new EnemyProps(currDnc);
                    break;
                case DncType.Plane:
                    break;
                case DncType.Player:
                    break;
                case DncType.TrafficSetup:
                    break;
                case DncType.LMAP:
                    break;
                case DncType.Sector:
                    break;
                case DncType.Standard:
                    currDnc.DncProps = new StandardProps(currDnc);
                    break;
                case DncType.Occluder:
                    break;
                case DncType.Model:
                    currDnc.DncProps = new ModelProps(currDnc);
                    break;
                case DncType.Sound:
                    break;
                case DncType.Camera:
                    break;
                case DncType.CityMusic:
                    break;
                case DncType.Light:
                    break;
                case DncType.Clock:
                    break;
                case DncType.Wagon:
                    break;
                case DncType.Route:
                    break;
                default:
                    break;
            }
        }

        private static void ParseKnownSection(Scene2Data scene2Data, IList loggingList, byte[] tmpBuff, ref int i, ref Scene2Section currSection, ref int positionIterator, string logMsg, string sectionName, NodeType nodeType)
        {
            loggingList.Add(logMsg);

            var sectionLen = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

            currSection = new Scene2Section()
            {
                Position = positionIterator,
                SectionName = sectionName,
                SectionType = nodeType,
                SectionStart = i,
                SectionIdArr = tmpBuff.Skip(i).Take(2).ToArray(),
                SectionLength = BitConverter.ToInt32(sectionLen, 0)
            };

            scene2Data.Sections.Add(currSection);

            positionIterator++;

            i += 6;
        }

        private static void ParseUnknownSection(Scene2Data scene2Data, byte[] tmpBuff, ref int i, out int objectID, int IdLen, out Scene2Section currSection, ref int positionIterator)
        {
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
            while (tmpPos < sectionLen)
            {
                int dncLen = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(2).Take(4).ToArray(), 0);

                Dnc currDnc = new Dnc
                {
                    dncType = DncType.Unknown,
                    Name = $"Unknown {objectID}",
                    ID = objectID,
                    objectIDArr = tmpBuff.Skip(i).Take(IdLen).ToArray(),
                    RawData = tmpBuff.Skip(i).Skip(IdLen).Take(dncLen - IdLen).ToArray(),
                };

                currDnc.RawDataBackup = new byte[currDnc.RawData.Length];
                currDnc.RawData.CopyTo(currDnc.RawDataBackup, 0);

                currSection.Dncs.Add(currDnc);

                tmpPos += dncLen;
                objectID++;
                i += dncLen; // move to next dnc
            }
        }

        public static void SaveScene(Stream outputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            loggingList.Add("Starting to save the file.");

            outputStream.Write(scene2Data.Header.Magic.ToArray(), 0, scene2Data.Header.Magic.ToArray().Length);

            // calculate file size
            var fileSize = 6 + scene2Data.Header.Content.RawData.Count() + (scene2Data.Sections.Count * 6);

            fileSize += scene2Data.Sections.SelectMany(x => x.Dncs).Sum(x => x.RawData.Length);
            fileSize += scene2Data.Sections.SelectMany(x => x.Dncs).Count() * 2;

            var fileSizeArr = BitConverter.GetBytes(fileSize);
            outputStream.Write(fileSizeArr, 0, fileSizeArr.Length);

            outputStream.Write(scene2Data.Header.Content.RawData.ToArray(), 0, scene2Data.Header.Content.RawData.ToArray().Length);

            foreach (var section in scene2Data.Sections.OrderBy(x => x.Position))
            {
                outputStream.Write(section.SectionIdArr, 0, section.SectionIdArr.Length);

                var dncLenArr =  BitConverter.GetBytes(section.Dncs.Sum(x => x.RawData.Length) + (section.Dncs.Count * 2) + 6);
                outputStream.Write(dncLenArr, 0, dncLenArr.Length);

                foreach (var dnc in section.Dncs.OrderBy(x => x.ID))
                {
                    outputStream.Write(dnc.objectIDArr, 0, dnc.objectIDArr.Length);
                    outputStream.Write(dnc.RawData, 0, dnc.RawData.Length);
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

        public static string GetScriptFromDnc(Dnc dnc, bool useBackup = false)
        {
            int offset = 0;

            switch (dnc.dncType)
            {
                case DncType.Script:
                    offset = 41;
                    break;
                case DncType.InitScript:
                    offset = 13;
                    break;
                case DncType.Enemy:
                    offset = 110;
                    break;
            }

            if (useBackup)
            {
                return Encoding.UTF8.GetString(dnc.RawDataBackup.Skip(dnc.Name.Length + offset).ToArray());
            }
            return Encoding.UTF8.GetString(dnc.RawData.Skip(dnc.Name.Length + offset).ToArray());
        }

        public static Dnc DuplicateDnc(Dnc dnc, string newName)
        {
            var newNameBytes = Encoding.UTF8.GetBytes(newName);
            var pos = GetPositionOfNameByID(dnc);

            var lenDiff = dnc.Name.Length > newName.Length ? dnc.Name.Length - newName.Length : newName.Length - dnc.Name.Length;

            Dnc newDnc = new Dnc { dncKind = dnc.dncKind, DncProps = dnc.DncProps, dncType = dnc.dncType, objectIDArr = dnc.objectIDArr };

            // replace binary data
            byte[] rawData = dnc.RawData.Take(pos).Concat(newNameBytes).ToArray();

            // put new data to object
            newDnc.RawData = rawData.Concat(dnc.RawData.Skip(pos).Skip(dnc.Name.Length).Take(dnc.RawData.Length - dnc.Name.Length - pos)).ToArray();

            // update length data
            newDnc.RawData[pos - 4] = (byte)(dnc.RawData[pos - 4] + lenDiff);
            newDnc.RawData[0] = (byte)(dnc.RawData[0] + lenDiff);

            newDnc.Name = newName;
            newDnc.RawDataBackup = dnc.RawDataBackup;
            return newDnc;
        }

        public static string GetStringFromDnc(Dnc dnc, int dataBegin, int offset, bool useBackup = false)
        {
            if (useBackup)
            {
                return Encoding.ASCII.GetString(dnc.RawDataBackup, dataBegin + offset, Array.IndexOf(dnc.RawDataBackup, (byte)0, dnc.Name.Length + offset) - (dnc.Name.Length + offset));
            }

            var tmp = Array.IndexOf(dnc.RawData, (byte)0, dataBegin + offset);
            var tmp2 = tmp - (dataBegin + offset);
            return Encoding.ASCII.GetString(dnc.RawData, dataBegin + offset, tmp2);
        }

        public static void UpdateStringInScriptDnc(Dnc dnc, string text)
        {
            UpdateStringInDnc(dnc, text, 41);
        }

        public static void UpdateStringInEnemyDnc(Dnc dnc, string text)
        {
            UpdateStringInDnc(dnc, text, 110);
        }

        public static void UpdateStringInInitScriptDnc(Dnc dnc, string text)
        {
            var startArray = dnc.RawData.Take(dnc.Name.Length + 13).ToArray();

            // recalculate array length
            var textInBytes = Encoding.UTF8.GetBytes(text);
            var bytesLen = BitConverter.GetBytes(textInBytes.Length + startArray.Length + IdLen);

            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[i] = bytesLen[i];
            }

            // recalculate additional size
            /*
            bytesLen = BitConverter.GetBytes(textInBytes.Length);
            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[dnc.Name.Length + 9 + i] = bytesLen[i];
            }
            */

            /*
            bytesLen = BitConverter.GetBytes(textInBytes.Length + 20);
            for (int i = 0; i < bytesLen.Length; i++)
            {
                startArray[dnc.Name.Length + 23 + i] = bytesLen[i];
            }*/

            dnc.RawData = startArray.Concat(textInBytes).ToArray();
        }

        public static void UpdateStringInDnc(Dnc dnc, string text, int offset)
        {
            var startArray = dnc.RawData.Take(dnc.Name.Length + offset).ToArray();

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
                startArray[dnc.Name.Length + (offset - 4) + i] = bytesLen[i];
            }

            if (dnc.dncType != DncType.Enemy)
            {
                bytesLen = BitConverter.GetBytes(textInBytes.Length + 20);
                for (int i = 0; i < bytesLen.Length; i++)
                {
                    startArray[dnc.Name.Length + (offset - 18) + i] = bytesLen[i];
                }
            }

            if (dnc.dncType == DncType.Enemy)
            {
                bytesLen = BitConverter.GetBytes(textInBytes.Length + 2 + 87);
                for (int i = 0; i < bytesLen.Length; i++)
                {
                    startArray[dnc.Name.Length + (offset - 87) + i] = bytesLen[i];
                }
            }

            dnc.RawData = startArray.Concat(textInBytes).ToArray();
        }

        public static string GetNameOfDnc(Dnc dnc)
        {
            switch (dnc.dncType)
            {
                case DncType.Unknown:
                    //return $"Unknown {dnc.ID}";
                    return GetCStringFromByteArray(dnc.RawData.Skip(10).Take(maxObjectNameLength).ToArray());

                case DncType.InitScript:

                    var len = dnc.RawData[5];

                    return Encoding.ASCII.GetString(dnc.RawData, 0x9, len);
                case DncType.MovableBridge:
                case DncType.Car:
                case DncType.Script:
                case DncType.PhysicalObject:
                case DncType.Door:
                case DncType.Tram:
                case DncType.GasStation:
                case DncType.PedestrianSetup:
                case DncType.Enemy:
                case DncType.Plane:
                case DncType.Player:
                case DncType.TrafficSetup:
                case DncType.LMAP:
                case DncType.Sector:
                case DncType.Wagon:
                case DncType.Route:
                case DncType.Clock:
                case DncType.GhostObject:
                case DncType.Zidle:
                    return GetCStringFromByteArray(dnc.RawData.Skip(10).Take(maxObjectNameLength).ToArray());

                case DncType.Standard:
                case DncType.Occluder:
                case DncType.Model:
                case DncType.Sound:
                case DncType.Camera:
                case DncType.CityMusic:
                case DncType.Light:
                    return GetCStringFromByteArray(dnc.RawData.Skip(20).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameOfDnc));
            }
        }

        public static int GetPositionOfNameByID(Dnc dnc)
        {
            switch (dnc.dncType)
            {
                case DncType.Unknown:
                    return 10;

                case DncType.InitScript:

                    return 9;
                case DncType.MovableBridge:
                case DncType.Car:
                case DncType.Script:
                case DncType.PhysicalObject:
                case DncType.Door:
                case DncType.Tram:
                case DncType.GasStation:
                case DncType.PedestrianSetup:
                case DncType.Enemy:
                case DncType.Plane:
                case DncType.Player:
                case DncType.TrafficSetup:
                case DncType.LMAP:
                case DncType.Sector:
                case DncType.Wagon:
                case DncType.Route:
                case DncType.Clock:
                case DncType.GhostObject:
                    return 10;

                case DncType.Standard:
                case DncType.Occluder:
                case DncType.Model:
                case DncType.Sound:
                case DncType.Camera:
                case DncType.CityMusic:
                case DncType.Light:
                    return 20;
                default:
                    throw new InvalidOperationException(nameof(GetNameOfDnc));
            }
        }

        private static string GetCStringFromByteArray(byte[] arr)
        {
            var fixedArr = CutStartingZeros(arr);
            return Encoding.ASCII.GetString(fixedArr, 0, Array.IndexOf(fixedArr, (byte)0));
        }

        private static byte[] CutStartingZeros(byte[] arr)
        {
            int index = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != 0)
                {
                    index = i;
                    break;
                }
            }

            return arr.Skip(index).ToArray();
        }

        public static DncType GetObjectType(Dnc dnc)
        {
            if (dnc.RawData[4] == 0x10)
            { // either LMAP or sector
                if (dnc.RawData.FindIndexOf(Encoding.ASCII.GetBytes("LMAP")).Any())
                { // is LMAP
                    return DncType.LMAP;
                }
                else
                {
                    if (dnc.RawData.FindIndexOf(new byte[] { 0x01, 0xB4, 0xF2 }).Any())
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
                var firstN = dnc.RawData.Take(20 + maxObjectNameLength).ToArray();
                if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0C }).Any())
                {
                    return DncType.Occluder;
                }
                else
                {
                    if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x09 }).Any())
                    {
                        return DncType.Model;
                    }
                    else
                    {
                        if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
                        {
                            return DncType.Sound;
                        }
                        else
                        {
                            if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x03 }).Any())
                            {
                                return DncType.Camera;
                            }
                            else
                            {
                                if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0E }).Any())
                                {
                                    return DncType.CityMusic;
                                }
                                else
                                {
                                    if (firstN.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x02 }).Any())
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

        public static DncType GetObjectDefinitionType(Dnc dnc)
        {
            if (dnc.RawData.Skip(4).Take(1).ToArray().FindIndexOf(new byte[] { 0x01 /*, 0x0d */ }).Any())
            {
                return DncType.InitScript;
            }

            var firstN = dnc.RawData.Take(20 + maxObjectNameLength).ToArray();
            if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
            {
                return DncType.Car;
            }
            else
            {
                if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x14 }).Any())
                {
                    return DncType.MovableBridge;
                }
                else
                {
                    if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x5 }).Any())
                    {
                        return DncType.Script;
                    }
                    else
                    {
                        if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x23 }).Any())
                        {
                            return DncType.PhysicalObject;
                        }
                        else
                        {
                            if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x6 }).Any())
                            {
                                return DncType.Door;
                            }
                            else
                            {
                                if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x8 }).Any())
                                {
                                    return DncType.Tram;
                                }
                                else
                                {
                                    if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x19 }).Any())
                                    {
                                        return DncType.GasStation;
                                    }
                                    else
                                    {
                                        if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x12 }).Any())
                                        {
                                            return DncType.PedestrianSetup;
                                        }
                                        else
                                        {
                                            if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x1B }).Any())
                                            {
                                                return DncType.Enemy;
                                            }
                                            else
                                            {
                                                if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x16 }).Any())
                                                {
                                                    return DncType.Plane;
                                                }
                                                else
                                                {
                                                    if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x2 }).Any())
                                                    {
                                                        return DncType.Player;
                                                    }
                                                    else
                                                    {
                                                        if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0xC }).Any())
                                                        {
                                                            return DncType.TrafficSetup;
                                                        }
                                                        else
                                                        {
                                                            if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x22 }).Any())
                                                            {
                                                                return DncType.Clock;
                                                            }
                                                            else
                                                            {
                                                                if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x1E }).Any())
                                                                {
                                                                    return DncType.Wagon;
                                                                }
                                                                else
                                                                {
                                                                    if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x18 }).Any())
                                                                    {
                                                                        return DncType.Route;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x01 }).Any())
                                                                        {
                                                                            return DncType.GhostObject;
                                                                        }
                                                                        else
                                                                        {
                                                                            if (firstN.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x09 }).Any())
                                                                            {
                                                                                return DncType.Zidle;
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
                                }
                            }
                        }
                    }
                }
                return DncType.Unknown;
            }
        }

        public static DncType TestIfInitScript(Dnc dnc)
        {
            if (dnc.RawData[0] == 0x51 && dnc.RawData[1] == 0xAE)
            {
                return DncType.InitScript;
            }
            else
            {
                return DncType.Unknown;
            }
        }

        public static void WriteToDnc(Dnc dnc, int DataBegin, float value, int indexInArray, bool isFloat = true)
        {
            if (isFloat)
            {
                Array.Copy(BitConverter.GetBytes(value), 0, dnc.RawData, DataBegin + indexInArray, 4);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes((int)value).Take(1).ToArray(), 0, dnc.RawData, DataBegin + indexInArray, 1);
            }
        }

        public static void WriteArrayToDnc(byte[] inputArray, int destinationIndex, Dnc dnc)
        {
            Array.Copy(inputArray, 0, dnc.RawData, destinationIndex, inputArray.Length);
        }

        public static void RenameDnc(Dnc dnc, string newName)
        {
            if (newName != dnc.Name)
            {
                var newNameBytes = Encoding.UTF8.GetBytes(newName);
                var pos = GetPositionOfNameByID(dnc);

                var lenDiff = dnc.Name.Length > newName.Length ? dnc.Name.Length - newName.Length : newName.Length - dnc.Name.Length;

                // replace binary data
                byte[] rawData = dnc.RawData.Take(pos).Concat(newNameBytes).ToArray();

                // put new data to object
                dnc.RawData = rawData.Concat(dnc.RawData.Skip(pos).Skip(dnc.Name.Length).Take(dnc.RawData.Length - dnc.Name.Length - pos)).ToArray();

                // update length data
                dnc.RawData[pos - 4] = (byte)(dnc.RawData[pos - 4] + lenDiff);
                dnc.RawData[0] = (byte)(dnc.RawData[0] + lenDiff);

                dnc.Name = newName;
            }
        }

        public static void CutZerosAtEndOfArray(Dnc dnc)
        {
            int notZeroIndex = 0;
            for (int i = dnc.RawData.Length - 1; i > 0; i--)
            {
                if (dnc.RawData[i] != 0)
                {
                    notZeroIndex = i + 2;
                    break;
                }
            }

            dnc.RawData = dnc.RawData.Take(notZeroIndex).ToArray();
        }

        public static int SearchForByteSequence(byte[] dncRawData, byte[] seq)
        {
            for (int i = 0; i < dncRawData.Length; i++)
            {
                if (dncRawData.Skip(i).Take(4).SequenceEqual(seq))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
