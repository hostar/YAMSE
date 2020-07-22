using MafiaSceneEditor.DataLayer;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using WpfHexaEditor.Core.MethodExtention;

namespace MafiaSceneEditor
{
    public static class Scene2Parser
    {
        private const int maxObjectNameLength = 50;
        private const int IdLen = 2; // length of ID

        public static void LoadScene(MemoryStream inputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            byte[] tmpBuff = inputStream.ToArray();

            bool headerParsed = false;
            bool objectsParsed = false;
            bool definitionsParsed = false;

            int i = 0;
            int objectID = 0;

            // loading
            bool loadingHeaderShown = false;
            bool loadingObjectsShown = false;
            bool loadingObjectsDefinitionsShown = false;
            bool loadingInitScriptsShown = false;

            const int IdLen = 2; // length of ID

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
                        scene2Data.rawDataHeader = tmpBuff.Take(i).ToList();

                        var arr = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                        scene2Data.standardObjectsStartPosition = i;
                        scene2Data.standardObjectsLength = BitConverter.ToInt32(arr, 0);

                        i += 4;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    // parse dncs objects
                    if (!loadingObjectsShown)
                    {
                        loggingList.Add("Loading objects...");
                        loadingObjectsShown = true;
                    }
                    if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                    {
                        Dnc currDnc = new Dnc
                        {
                            objectType = ObjectIDs.Unknown,
                        };

                        // get length
                        int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                        currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                        currDnc.objectType = GetObjectType(currDnc);
                        currDnc.name = GetNameByID(currDnc);
                        currDnc.ID = objectID;

                        scene2Data.objectsDncs.Add(currDnc);

                        objectID++;
                        i = i + IdLen + lenCurr;
                    }
                    else
                    {
                        i++;

                        if (!objectsParsed)
                        {
                            if (scene2Data.objectsDncs.Count != 0)
                            {
                                objectsParsed = true;
                                objectID = 0;
                                i--;
                            }
                        }
                        else
                        {
                            //i++;
                        }
                    }

                    // parse dncs definitions
                    if ((i >= scene2Data.standardObjectsLength) && !definitionsParsed)
                    {
                        if (scene2Data.objectsDefinitionStartPosition > 0)
                        {
                            if (i > (scene2Data.objectsDefinitionStartPosition + scene2Data.objectsDefinitionLength))
                            {
                                definitionsParsed = true;
                                objectID = 0;
                            }
                        }

                        if (tmpBuff[i] == 0x20 && tmpBuff[i + 1] == 0xAE)
                        {
                            var arr = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                            scene2Data.objectsDefinitionStartPosition = i;
                            scene2Data.objectsDefinitionLength = BitConverter.ToInt32(arr, 0);

                            i += 6;
                        }

                        // parse dncs object definitions
                        if (!loadingObjectsDefinitionsShown)
                        {
                            loggingList.Add("Loading object definitions...");
                            loadingObjectsDefinitionsShown = true;
                        }
                        if (tmpBuff[i] == 0x21 && tmpBuff[i + 1] == 0xAE)
                        {
                            Dnc currDnc = new Dnc
                            {
                                objectType = ObjectIDs.Unknown,
                            };

                            // get length
                            int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                            currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                            currDnc.definitionType = GetObjectDefinitionType(currDnc);
                            currDnc.name = GetNameByDefinitionID(currDnc);
                            currDnc.ID = objectID;

                            scene2Data.objectDefinitionsDncs.Add(currDnc);

                            objectID++;
                            i = i + IdLen + lenCurr;
                            i--;
                        }
                    }

                    if (definitionsParsed)
                    {
                        if (tmpBuff.Length <= i)
                        {
                            break;
                        }

                        // init scripts
                        if (!loadingInitScriptsShown)
                        {
                            loggingList.Add("Loading init scripts...");
                            loadingInitScriptsShown = true;
                        }
                        if (tmpBuff[i] == 0x51 && tmpBuff[i + 1] == 0xAE)
                        {
                            Dnc currDnc = new Dnc
                            {
                                objectType = ObjectIDs.Unknown,
                            };

                            // get length
                            int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                            currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                            currDnc.definitionType = DefinitionIDs.InitScript;
                            currDnc.name = GetNameByDefinitionID(currDnc);
                            currDnc.ID = objectID;

                            scene2Data.initScriptsDncs.Add(currDnc);

                            objectID++;
                            i = i + IdLen + lenCurr;
                            i--;
                        }
                    }
                }
            }
        }

        public static void SaveScene(Stream outputStream, ref Scene2Data scene2Data, IList loggingList)
        {
            loggingList.Add("Starting to save the file.");
            outputStream.Write(scene2Data.rawDataHeader.ToArray(), 0, scene2Data.rawDataHeader.ToArray().Length);

            var sectionID = new byte[] { 0x00, 0x40 };

            // objects
            outputStream.Write(sectionID, 0, sectionID.Length);

            var sumOfLengths = scene2Data.objectsDncs.Sum(x => x.rawData.Length + IdLen);

            var standardObjectsLengthArr = BitConverter.GetBytes(sumOfLengths + 6 /* 6 is length of section start */);

            outputStream.Write(standardObjectsLengthArr, 0, standardObjectsLengthArr.Length);

            var objectsID = new byte[] { 0x10, 0x40 };

            foreach (Dnc dnc in scene2Data.objectsDncs)
            {
                outputStream.Write(objectsID, 0, objectsID.Length);
                outputStream.Write(dnc.rawData, 0, dnc.rawData.Length);
            }
            loggingList.Add("Objects saved.");

            // object definitions
            var sectionIDdefs = new byte[] { 0x20, 0xAE };

            outputStream.Write(sectionIDdefs, 0, sectionIDdefs.Length);

            sumOfLengths = scene2Data.objectDefinitionsDncs.Sum(x => x.rawData.Length + IdLen);
            var objectsDefsLengthArr = BitConverter.GetBytes(sumOfLengths + 6 /* 6 is length of section start */);

            outputStream.Write(objectsDefsLengthArr, 0, objectsDefsLengthArr.Length);

            var objectDefsID = new byte[] { 0x21, 0xAE };

            foreach (Dnc dnc in scene2Data.objectDefinitionsDncs)
            {
                outputStream.Write(objectDefsID, 0, objectDefsID.Length);
                outputStream.Write(dnc.rawData, 0, dnc.rawData.Length);
            }

            loggingList.Add("Object definitions saved.");

            //mysterious section
            var mystery = new byte[] { 0x02, 0xae, 0x06, 0x00, 0x00, 0x00 };
            outputStream.Write(mystery, 0, mystery.Length);

            // init scripts
            var sectionInitScripts = new byte[] { 0x50, 0xAE };

            outputStream.Write(sectionInitScripts, 0, sectionInitScripts.Length);

            sumOfLengths = scene2Data.initScriptsDncs.Sum(x => x.rawData.Length + IdLen);
            var initScriptsLengthArr = BitConverter.GetBytes(sumOfLengths + 6 /* 6 is length of section start */);

            outputStream.Write(initScriptsLengthArr, 0, initScriptsLengthArr.Length);

            var initScriptsID = new byte[] { 0x51, 0xAE };

            foreach (Dnc dnc in scene2Data.initScriptsDncs)
            {
                outputStream.Write(initScriptsID, 0, initScriptsID.Length);
                outputStream.Write(dnc.rawData, 0, dnc.rawData.Length);
            }
            loggingList.Add("Init scripts saved.");

            /*
            
            objectsDncs
            objectDefinitionsDncs
            initScriptsDncs

             */

            outputStream.Close();
            loggingList.Add("File saving done.");
            //streamWriter.Write(scene2Data.rawDataHeader);
            //streamWriter.Close();
        }

        private static string GetNameByDefinitionID(Dnc dnc)
        {
            switch (dnc.definitionType)
            {
                case DefinitionIDs.Unknown:
                    return "Unknown";
                case DefinitionIDs.MovableBridge:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Car:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Script:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.InitScript:

                    var len = dnc.rawData[5];

                    return Encoding.ASCII.GetString(dnc.rawData, 0x9, len);

                //return GetCStringFromByteArray(dnc.rawData.Skip(0x9).Take(maxObjectNameLength).ToArray());


                case DefinitionIDs.PhysicalObject:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Door:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Tram:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.GasStation:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.PedestrianSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Enemy:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Plane:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Player:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.TrafficSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByDefinitionID));
            }
        }

        private static string GetCStringFromByteArray(byte[] arr)
        {
            return Encoding.ASCII.GetString(arr, 0, Array.IndexOf(arr, (byte)0));
        }

        private static ObjectIDs GetObjectType(Dnc dnc)
        {
            if (dnc.rawData[4] == 0x10)
            { // either LMAP or sector
                dnc.objectType = ObjectIDs.LMAP;
                dnc.name = GetNameByID(dnc);

                if (dnc.rawData.FindIndexOf(Encoding.ASCII.GetBytes("LMAP")).Any())
                { // is LMAP
                    return ObjectIDs.LMAP;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x01, 0xB4, 0xF2 }).Any())
                    {
                        return ObjectIDs.Sector;
                    }
                    else
                    {
                        return ObjectIDs.Unknown;
                    }
                }
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0C }).Any())
                {
                    return ObjectIDs.Occluder;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x09 }).Any())
                    {
                        return ObjectIDs.Model;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
                        {
                            return ObjectIDs.Sound;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x03 }).Any())
                            {
                                return ObjectIDs.Camera;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0E }).Any())
                                {
                                    return ObjectIDs.CityMusic;
                                }
                                else
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x02 }).Any())
                                    {
                                        return ObjectIDs.Light;
                                    }
                                }
                            }
                        }
                        return ObjectIDs.Standard;
                    }
                }
            }
        }

        private static string GetNameByID(Dnc dnc)
        {
            switch (dnc.objectType)
            {
                case ObjectIDs.Unknown:
                    return "Unknown";
                case ObjectIDs.LMAP:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Standard:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Sector:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Occluder:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Model:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Sound:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Camera:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.CityMusic:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Light:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByID));
            }
        }

        private static DefinitionIDs GetObjectDefinitionType(Dnc dnc)
        {
            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
            {
                return DefinitionIDs.Car;
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x14 }).Any())
                {
                    return DefinitionIDs.MovableBridge;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x5 }).Any())
                    {
                        return DefinitionIDs.Script;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x23 }).Any())
                        {
                            return DefinitionIDs.PhysicalObject;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x6 }).Any())
                            {
                                return DefinitionIDs.Door;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x8 }).Any())
                                {
                                    return DefinitionIDs.Tram;
                                }
                                else
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x19 }).Any())
                                    {
                                        return DefinitionIDs.GasStation;
                                    }
                                    else
                                    {
                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x12 }).Any())
                                        {
                                            return DefinitionIDs.PedestrianSetup;
                                        }
                                        else
                                        {
                                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x1B }).Any())
                                            {
                                                return DefinitionIDs.Enemy;
                                            }
                                            else
                                            {
                                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x16 }).Any())
                                                {
                                                    return DefinitionIDs.Plane;
                                                }
                                                else
                                                {
                                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x2 }).Any())
                                                    {
                                                        return DefinitionIDs.Player;
                                                    }
                                                    else
                                                    {
                                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0xC }).Any())
                                                        {
                                                            return DefinitionIDs.TrafficSetup;
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
                return DefinitionIDs.Unknown;
            }
        }
    }
}
