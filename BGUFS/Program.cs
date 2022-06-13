using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace BGUFS
{

    class com : IComparer
    {
        public int Compare(object x, object y)
        {
            string s1 = (string)x;
            string s2 = (string)y;
            string[] split1 = s1.Split(",");
            string[] split2 = s2.Split(",");
            if (int.Parse(split1[1]) > int.Parse(split2[1]))
                return -1;
            if (int.Parse(split1[1]) < int.Parse(split2[1]))
                return 1;
            List<string> com = new List<string>();
            com.Add(split1[2]);
            com.Add(split2[2]);
            com.Sort();
            if (com[0] == split1[2])
                return 1;
            else
                return -1;

        }
    }
    class comDate : IComparer
    {
        public int Compare(object x, object y)
        {
            string s1 = (string)x;
            string s2 = (string)y;
            string[] split1 = s1.Split(",");
            string[] split2 = s2.Split(",");
            DateTime time1 = Convert.ToDateTime(split1[1]);
            DateTime time2 = Convert.ToDateTime(split2[1]);
            if (time1 > time2)
                return -1;
            if (time1 < time2)
                return 1;
            List<string> com = new List<string>();
            com.Add(split1[2]);
            com.Add(split2[2]);
            com.Sort();
            if (com[0] == split1[2])
                return 1;
            else
                return -1;

        }
    }
    class BGUFS
    {
        public void create(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -create <filesystem>");
                return;
            }
            FileStream fs = new FileStream(args[1], FileMode.Create, FileAccess.ReadWrite);
            String s = "BGUFS_";
            Byte[] b = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s)));
            fs.Seek(0, SeekOrigin.End);
            fs.Write(b);
            fs.Flush();
            long indexToFIrstHeader = fs.Position;
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("," + indexToFIrstHeader.ToString() + "\n"))));
            fs.Flush();
            long updateIndex = fs.Position;
            fs.Position = indexToFIrstHeader;
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("," + updateIndex.ToString() + "\n"))));
            fs.Flush();
            fs.Position = fs.Length;
            b = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(0 + "\n")));
            fs.Write(b);
            fs.Flush();
            fs.Close();
        }

        private void addFileOrLink(string fileSystem, string fileName, string toLink, bool isLink)
        {
            long linkedFileIndex = 0;
            if (isLink)
                linkedFileIndex = isExist(fileSystem, toLink);
            long maxIndex = 0;
            Boolean isFirstFile = false;
            FileStream fs3 = new FileStream(fileSystem, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs3);
            FileStream newSt = null;
            FileInfo info = null;
            string headerUnderLink = "";
            long saveIndexInNotFullHeaader = 0;
            Boolean notFull = false;
            if (!isLink)
            {
                info = new FileInfo(fileName);
                newSt = File.OpenRead(fileName);
            }
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            string[] split = sr.ReadLine().Split(",");
            long firstPointer = int.Parse(split[1]);
            long nextIndex = int.Parse(sr.ReadLine());
            if (nextIndex > maxIndex)
                maxIndex = nextIndex;
            if (nextIndex == 0)
                isFirstFile = true;
            string getHeader = "";
            string[] splitHeader;
            while (nextIndex != 0 && isLink == false) // check if there is free space in the middle
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                getHeader = sr.ReadLine();
                splitHeader = getHeader.Split(",");
                if (splitHeader[2].Equals("1") && splitHeader.Length == 7)
                {
                    long size = int.Parse(splitHeader[1]);
                    if (size > newSt.Length)
                    {
                        sr.DiscardBufferedData();
                        sr.BaseStream.Position = nextIndex;
                        string s = sr.ReadLine();
                        long oldIndex = nextIndex + s.Length + int.Parse(s.Split(",")[1]) + 2;
                        Byte[] b = File.ReadAllBytes(fileName);
                        DateTime creation = File.GetCreationTime(fileName);
                        string newHeader = splitHeader[0] + "," + b.Length + "," + "2" + "," + fileName + "," + info.Length + "," + creation + "," + "regular" + "," + oldIndex.ToString();
                        fs3.Position = nextIndex;
                        fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(newHeader + "               "))));
                        fs3.Flush();
                        fs3.Position = nextIndex + s.Length + 1;
                        fs3.Write(b);
                        fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("\n"))));
                        fs3.Flush();
                        fs3.Close();
                        return;
                    }
                }
                if (int.Parse(splitHeader[0]) != 0) // check if this header is the last in the header list
                {
                    nextIndex = int.Parse(splitHeader[0]);
                    if (nextIndex > maxIndex)
                        maxIndex = nextIndex;
                }
                else
                    break;
            }
            long firstIndex = 0;
            if (nextIndex == 0 && isFirstFile && isLink == false)// if first file in fileSystem
            {
                fs3.Position = firstPointer;
                firstIndex = fs3.Length + 1;
                fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes((firstIndex + 30).ToString() + "                              \n"))));
                fs3.Flush();
            }
            else// not first file
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                string[] split3 = null;
                if (isLink)// i need to go to the end of the list becuse it doent enter to the first while loop
                {
                    int nextNext = 1;
                    while (nextNext != 0)
                    {
                        nextIndex = nextNext;
                        headerUnderLink = sr.ReadLine();
                        split3 = headerUnderLink.Split(",");
                        nextNext = int.Parse(split3[0]);
                        sr.DiscardBufferedData();
                        sr.BaseStream.Position = nextNext;
                        if (nextNext > maxIndex)
                            maxIndex = nextNext;
                    }

                }
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                if (isLink)
                    splitHeader = split3;
                else
                    splitHeader = getHeader.Split(",");
                if (!isFirstFile)
                {
                    if (splitHeader.Length == 9 && splitHeader[2] != 2.ToString()) // check if my dad header is link
                    {
                        //long x = maxIndex + getHeader.Length + 1;
                       // nextIndex = int.Parse(splitHeader[0]);
                        splitHeader[0] = getLastFileIndex(fs3, sr, maxIndex).ToString(); // prepare his pointer to me
                        //nextIndex = int.Parse(splitHeader[0]);
                        getHeader = newHeader(splitHeader);

                    }
                    else
                    {
                        if (splitHeader[2] == 0.ToString()) // if the block of the header is full of data
                        {
                            //nextIndex = int.Parse(splitHeader[0]);
                            splitHeader[0] = getLastFileIndex(fs3, sr, maxIndex).ToString(); // caculate the last index that nobady write on
                            getHeader = newHeader(splitHeader);
                        }
                        else // the block is not full i write the last index in index 7
                        {
                            if (splitHeader[2] == 2.ToString())
                            {
                                string[] newSplit = new string[splitHeader.Length - 1];
                                Array.Copy(splitHeader, newSplit, splitHeader.Length - 1);
                                saveIndexInNotFullHeaader = getLastFileIndex(fs3, sr, (long)maxIndex);
                                newSplit[0] = getLastFileIndex(fs3, sr, maxIndex).ToString();
                                newSplit[2] = 2.ToString();
                                getHeader = newHeader(newSplit);
                                notFull = true;
                            }
                            else
                            {
                                splitHeader[0] = getLastFileIndex(fs3, sr, maxIndex).ToString(); // caculate the last index that nobady write on
                                getHeader = newHeader(splitHeader);
                            }
                        }
                    }
                }

                if (nextIndex == 0)
                {
                    fs3.Position = firstPointer;
                }
                else
                    fs3.Position = nextIndex; // last header index in the list
                fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getHeader + "               "))));// over write dad header
                fs3.Flush();
            }

            Byte[] b3 = null;
            DateTime creation1 = DateTime.Now;
            if (isLink)
            {
                b3 = File.ReadAllBytes(toLink);
                creation1 = File.GetCreationTime(toLink);
            }
            else
            {
                b3 = File.ReadAllBytes(fileName);
                creation1 = File.GetCreationTime(fileName);
            }
            string newHeader2 = "";
            if (isLink)
                newHeader2 = 0 + "," + b3.Length + "," + "0" + "," + fileName + "," + b3.Length + "," + creation1 + "," + "link" + "," + toLink + "," + linkedFileIndex;
            else
                newHeader2 = 0 + "," + b3.Length + "," + "0" + "," + fileName + "," + info.Length + "," + creation1 + "," + "regular";
            if (isFirstFile)
                fs3.Position = fs3.Length;
            else
            {
                if (notFull)
                    fs3.Position = saveIndexInNotFullHeaader;
                else
                    fs3.Position = getLastFileIndex(fs3, sr, maxIndex);// write to the first free space in the file
            }
            fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(newHeader2 + "                              \n")))); // write new header
            fs3.Flush();
            if (!isLink)
                fs3.Write(b3);// write data to the file
            fs3.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("\n"))));
            fs3.Flush();
            fs3.Close();
            sr.Close();
        }
        public void add(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –add <filesystem> <filename>");
                return;
            }

            if (!args[1].StartsWith("BGUFS_"))
            {
                Console.WriteLine("Not a BGUFS file");
                //fs.Close();
                return;
            }
            if (!File.Exists(args[2]))
            {
                Console.WriteLine("file does not exist");
                //fs.Close();
                return;
            }
            if (isExist(args[1], args[2]) != 0)
            {
                Console.WriteLine("file already exist");
                return;
            }
            addFileOrLink(args[1], args[2], "", false);
        }

        private long isExist(string fileSystemName, string fileName)
        {
            //dic = new Dictionary<string, int>();
            int foundIndex = 0;
            if (!fileSystemName.StartsWith("BGUFS_"))
            {
                Console.WriteLine("Not a BGUFS file");
                return foundIndex;
            }
            FileStream fs2 = new FileStream(fileSystemName, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs2);
            //sr = new StreamReader(fs2,Encoding.UTF8);
            sr.DiscardBufferedData();
            sr.ReadLine();
            long nextIndex = int.Parse(sr.ReadLine());
            while (nextIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                //sr.DiscardBufferedData();
                string getHeader = sr.ReadLine();
                string[] splitHeader = getHeader.Split(",");
                if (fileName.Equals(splitHeader[3]))
                {
                    fs2.Close();
                    sr.Close();
                    return nextIndex;
                }
                nextIndex = int.Parse(splitHeader[0]);
            }
            fs2.Close();
            sr.Close();
            return 0;
        }

        public void remove(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –remove <filesystem> <filename>");
                return;
            }
            string fileSystem = args[1];
            string fileName = args[2];
            long index = isExist(fileSystem, fileName);
            if (index == 0)
            {
                Console.WriteLine("file does not exist");
                return;
            }
            FileStream fs = new FileStream(fileSystem, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = index;
            string getHeader = sr.ReadLine();
            string[] splitHeader = getHeader.Split(",");
            splitHeader[2] = 1.ToString();
            getHeader = newHeader(splitHeader);
            fs.Position = index;
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getHeader))));
            fs.Flush();
            markLinkers(sr, fs, index);
            fs.Close();
            sr.Close();

        }

        private void markLinkers(StreamReader sr, FileStream fs, long index)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            sr.ReadLine();
            int firstIndex = int.Parse(sr.ReadLine());
            while (firstIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = firstIndex;
                string s = sr.ReadLine();
                string[] split = s.Split(",");
                if (split.Length == 9 && int.Parse(split[8]) == index)
                {
                    split[2] = "1";
                    s = newHeader(split);
                    fs.Position = firstIndex;
                    fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s))));
                    fs.Flush();
                }
                firstIndex = int.Parse(split[0]);
            }
        }

        public void rename(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –rename <filesystem> <filename> <newfilename>");
                return;
            }
            string fileSystem = args[1];
            string fileName = args[2];
            string newFileName = args[3];
            long index = isExist(fileSystem, fileName);
            if (index == 0)
            {
                Console.WriteLine("file does not exist");
                return;
            }
            long index2 = isExist(fileSystem, newFileName);
            if (index2 != 0)
            {
                Console.WriteLine("file " + newFileName + " already exists");
                return;
            }
            FileStream fs = new FileStream(fileSystem, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = index;
            string getHeader = sr.ReadLine();
            string[] splitHeader = getHeader.Split(",");
            splitHeader[3] = newFileName;
            getHeader = newHeader(splitHeader);
            fs.Position = index;
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getHeader))));
            fs.Flush();
            fs.Close();
            sr.Close();
        }

        public void extract(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –extract <filesystem> <filename> <extractedfilename>");
                return;
            }
            string fileSystem = args[1];
            string fileName = args[2];
            string extractFileName = args[3];
            long index = isExist(fileSystem, fileName);
            if (index == 0)
            {
                Console.WriteLine("file does not exist");
                return;
            }
            FileStream fs = new FileStream(fileSystem, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(index, SeekOrigin.Begin);
            string s = sr.ReadLine();
            fs.Position = index + s.Length + 1;
            string[] splitHeader = s.Split(",");
            long len = int.Parse(splitHeader[1]);
            sr.DiscardBufferedData();
            Byte[] content = new Byte[len + 10]; //We do not have to open the file for reading
            fs.Read(content, 0, (int)len);
            s = Encoding.UTF8.GetString(content);
            s = s.Trim('\0');
            FileStream fs2 = new FileStream(extractFileName, FileMode.Create, FileAccess.ReadWrite);
            // fs2.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s))));
            fs2.Write(content);
            fs2.Flush();
            sr.Close();
            fs.Close();
            fs2.Close();
        }

        public void dir(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –dir <filesystem>");
                return;
            }
            string fileSystem = args[1];
            int foundIndex = 0;
            if (!fileSystem.StartsWith("BGUFS"))
            {
                Console.WriteLine("Not a BGUFS file");
                return;
            }
            FileStream fs2 = new FileStream(fileSystem, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs2);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            sr.ReadLine();
            long nextIndex = int.Parse(sr.ReadLine());
            while (nextIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                string getHeader = sr.ReadLine();
                string[] splitHeader = getHeader.Split(",");
                if (splitHeader[2] != "1")
                {
                    if (splitHeader.Length < 8)
                        Console.WriteLine(splitHeader[3] + "," + splitHeader[4] + "," + splitHeader[5] + "," + splitHeader[6]);
                    else
                        Console.WriteLine(splitHeader[3] + "," + splitHeader[4] + "," + splitHeader[5] + "," + splitHeader[6] + "," + splitHeader[7]);
                }
                nextIndex = int.Parse(splitHeader[0]);
            }
            fs2.Close();
            sr.Close();
        }

        public void hash(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS –hash <filesystem> <filename>");
                return;
            }
            string fileSystem = args[1];
            string fileName = args[2];
            long index = isExist(fileSystem, fileName);
            if (index == 0)
            {
                Console.WriteLine("file does not exist");
                return;
            }
            StringBuilder sb = new StringBuilder();
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(fileName);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
            }
           Console.WriteLine(sb.ToString());
        }

        private List<string> getHeaderWithInfo(string fileSystem, int index, bool isSize)// return list every cell has(index of header,name/size/date) index = 3 - fileName, index = 5 - date, index = 4- size
        {
            if (!fileSystem.StartsWith("BGUFS"))
            {
                Console.WriteLine("Not a BGUFS file");
                return null;
            }
            List<string> headers = new List<string>();
            FileStream fs2 = new FileStream(fileSystem, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs2);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            string[] split = sr.ReadLine().Split(",");
            sr.BaseStream.Position = int.Parse(split[1]);
            long nextIndex = int.Parse(sr.ReadLine());
            while (nextIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                string getHeader = sr.ReadLine();
                string[] splitHeader = getHeader.Split(",");
                string s = "";
                if (isSize)
                    s = nextIndex + "," + splitHeader[index] + "," + splitHeader[3];
                else
                    s = nextIndex + "," + splitHeader[index];
                nextIndex = int.Parse(splitHeader[0]);
                headers.Add(s);
            }
            fs2.Close();
            sr.Close();
            return headers;
        }


        private List<string> headersIndexesInOrder(List<string> headers)
        {
            List<string> finalIndexes = new List<string>();
            List<string> fileData = new List<string>();
            for (int i = 0; i < headers.Count; i++)
            {
                string[] split = headers[i].Split(",");
                fileData.Add(split[1]);
            }
            fileData.Sort();

            for (int i = 0; i < fileData.Count; i++)
            {
                for (int j = 0; j < headers.Count; j++)
                {
                    if (headers[j].Contains(fileData[i]))
                    {
                        string[] split = headers[j].Split(",");
                        finalIndexes.Add(split[0]);
                        break;
                    }
                }

            }
            return finalIndexes;
        }


        private void sortHeaders(string fileSystem, List<string> indexes)
        {
            FileStream fs2 = new FileStream(fileSystem, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs2);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            string[] split = sr.ReadLine().Split(",");
            sr.DiscardBufferedData();
            sr.BaseStream.Position = int.Parse(split[1]);
            string oldNum = sr.ReadLine();
            //int distance = digitDistance(oldNum, indexes[0]);
            fs2.Position = int.Parse(split[1]);
            fs2.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(indexes[0]+"                "))));
            fs2.Flush();
            for (int i = 0; i < indexes.Count; i++)
            {
                sr.DiscardBufferedData();
                if (int.Parse(indexes[i]) < 20)
                    sr.BaseStream.Position = int.Parse(indexes[i]);
                else
                    sr.BaseStream.Position = int.Parse(indexes[i]);
                string getHeader = sr.ReadLine();
                string[] splitHeader = getHeader.Split(",");
                if (i + 1 != indexes.Count)
                {
                    int x = 0;
                    if (int.Parse(indexes[i + 1]) < 20)
                        x = int.Parse(indexes[i + 1]);
                    else
                        x = int.Parse(indexes[i + 1]);
                    splitHeader[0] = x.ToString();
                }
                else
                    splitHeader[0] = "0";
                getHeader = newHeader(splitHeader);
                if (int.Parse(indexes[i]) < 20)
                    fs2.Position = int.Parse(indexes[i]);
                else
                    fs2.Position = int.Parse(indexes[i]);
                fs2.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getHeader + "      "))));
                fs2.Flush();
            }
            fs2.Close();
            sr.Close();
        }
        private List<string> headersIndexesInOrderBySize(List<string> headers)//(index,size,name)
        {
            List<string> finalIndexes = new List<string>();
            List<string> fileData = new List<string>();
            com c = new com();
            headers.Sort(c.Compare);
            headers.Reverse();
            for (int i = 0; i < headers.Count; i++)
            {
                string[] split = headers[i].Split(",");
                finalIndexes.Add(split[0]);

            }
            return finalIndexes;
        }

        private List<string> headersIndexesInOrderByDate(List<string> headers)//(index,date,name)
        {
            List<string> finalIndexes = new List<string>();
            List<string> fileData = new List<string>();
            comDate c = new comDate();
            headers.Sort(c.Compare);
            headers.Reverse();
            for (int i = 0; i < headers.Count; i++)
            {
                string[] split = headers[i].Split(",");
                finalIndexes.Add(split[0]);

            }
            return finalIndexes;
        }

        public void sortAB(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -sortAB <filesystem>");
                return;
            }
            string fileSystem = args[1];
            List<string> indexAndNames = getHeaderWithInfo(fileSystem, 3, false);
            List<string> headers = headersIndexesInOrder(indexAndNames);
            sortHeaders(fileSystem, headers);
            // dir(fileSystem);
        }

        public void sortDate(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -sortDate <filesystem>");
                return;
            }
            string fileSystem = args[1];
            List<string> indexAndNames = getHeaderWithInfo(fileSystem, 5, true);
            List<string> headers = headersIndexesInOrderByDate(indexAndNames);
            sortHeaders(fileSystem, headers);
        }
        public void sortSize(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -sortSize <filesystem>");
                return;
            }
            string fileSystem = args[1];
            List<string> indexAndNames = getHeaderWithInfo(fileSystem, 4, true);
            List<string> headers = headersIndexesInOrderBySize(indexAndNames);
            sortHeaders(fileSystem, headers);
        }

        public void addLink(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -addLink <filesystem> <linkfilename> <existingfilename>");
                return;
            }
            string fileSystem = args[1];
            string linkName = args[2];
            string linkTo = args[3];
            long index = isExist(fileSystem, linkTo);
            if (index == 0)
            {
                Console.WriteLine("file does not exist");
                return;
            }
            index = isExist(fileSystem, linkName);
            if (index != 0)
            {
                Console.WriteLine("Link file already exist");
                return;
            }
            addFileOrLink(fileSystem, linkName, linkTo, true);

        }

        private long getLastFileIndex(FileStream fs, StreamReader sr, long next)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = next;
            string s = sr.ReadLine();
            string[] split = s.Split(",");
            if (split.Length == 8)
            {
                return int.Parse(split[7]);
            }
            if (split.Length == 7)
                return next + s.Length + int.Parse(split[1]) + 2;
            else
                return next + s.Length + 1;

        }

        private List<int> getOnlyIndexes(List<string> headersIndexesBig)
        {
            List<int> headerIndexesSmall = new List<int>();
            for (int i = 0; i < headersIndexesBig.Count; i++)
            {
                string s = headersIndexesBig[i];
                string[] split = s.Split(",");
                headerIndexesSmall.Add(int.Parse(split[0]));
            }
            headerIndexesSmall.Sort();
            return headerIndexesSmall;
        }

        private void deleteBackPointer(FileStream fs, StreamReader sr)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            int next = int.Parse(sr.ReadLine().Split(",")[1]);
            sr.DiscardBufferedData();
            sr.BaseStream.Position = next;
            int nextIndex = int.Parse(sr.ReadLine());
            while (nextIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                string s = sr.ReadLine();
                string[] split = s.Split(",");
                s = deleteOneBackPointer(split);
                fs.Position = nextIndex;
                fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s))));
                fs.Flush();
                nextIndex = int.Parse(split[0]);
            }
        }


        private void addBackPointer(FileStream fs, StreamReader sr, long nextIndex, int backIndex)
        {
            while (nextIndex != 0)
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Position = nextIndex;
                string s = sr.ReadLine();
                string[] split = s.Split(",");
                if (split.Length == 8)
                {
                    //split[2] = "0";
                    string getHeader = "";
                    for (int j = 0; j < split.Length - 1; j++)
                    {
                        if (j != split.Length - 2)
                            getHeader += split[j] + ",";
                        else
                            getHeader += split[j].Trim() + "," + backIndex.ToString();
                    }
                    s = getHeader;
                    fs.Position = nextIndex;
                    fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s + ""))));
                    fs.Flush();
                }
                else
                {
                    s = newBackPointer(split, backIndex);
                    fs.Position = nextIndex;
                    fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s))));
                    fs.Flush();
                }
                backIndex = (int)nextIndex;
                nextIndex = int.Parse(split[0]);
            }
        }

        public string newHeader(string[] split)
        {
            string getHeader = "";
            for (int j = 0; j < split.Length; j++)
            {
                if (j != split.Length - 1)
                    getHeader += split[j] + ",";
                else
                    getHeader += split[j].Trim(' ');
            }
            return getHeader;

        }

        public string newBackPointer(string[] split, int backIndex)
        {
            string getHeader = "";
            for (int j = 0; j < split.Length; j++)
            {
                if (j != split.Length - 1)
                    getHeader += split[j] + ",";
                else
                    getHeader += split[j].Trim() + "," + backIndex.ToString();
            }
            return getHeader;

        }
        public string deleteOneBackPointer(string[] split)
        {
            string getHeader = "";
            if (split.Length == 8)
                for (int j = 0; j < split.Length; j++)
                {
                    if (j != split.Length - 2)
                        getHeader += split[j] + ",";
                    else
                    {
                        getHeader += split[j] + "      ";
                        break;
                    }
                }
            else
            {
                for (int j = 0; j < split.Length; j++)
                {
                    if (j != split.Length - 2)
                        getHeader += split[j] + ",";
                    else
                    {
                        getHeader += split[j] + "      ";
                        break;
                    }
                }
            }

            return getHeader;

        }


        private void changeNextHeader(FileStream fs, StreamReader sr, int firstIndex, string whereToChange, string WhatValue, bool isLink)
        {
            if (whereToChange == "0")
                return;
            sr.DiscardBufferedData();
            sr.BaseStream.Position = firstIndex;
            string s = sr.ReadLine();
            string[] split = s.Split(",");
            if (split[0] == "0")
                return;
            sr.DiscardBufferedData();
            sr.BaseStream.Position = int.Parse(whereToChange);
            string s2 = sr.ReadLine();
            string[] split2 = s2.Split(",");
            if (isLink)
            {
                if (split2.Length == 10)
                    split2[9] = split[9];
                else
                    split2[7] = split[9];
            }
            else
            {
                if (split2.Length == 10)
                    split2[9] = split[7];
                else
                    split2[7] = split[7];
            }
            string getH = newHeader(split2);
            fs.Position = int.Parse(whereToChange);
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getH + "               "))));
            fs.Flush();

        }

        private void changeDadPointer(StreamReader sr, FileStream fs, string prev, string next)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = int.Parse(prev);
            string s = sr.ReadLine();
            string[] split = s.Split(",");
            split[0] = next;
            string getH = newHeader(split);
            fs.Position = int.Parse(prev);
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(getH + "               "))));
            fs.Flush();

        }


        private void updateBackHeader(FileStream fs, StreamReader sr, int newIndex, string changeHeaderIndex)
        {
            if (newIndex == int.Parse(changeHeaderIndex) || changeHeaderIndex == "0")
                return;
            sr.DiscardBufferedData();
            sr.BaseStream.Position = int.Parse(changeHeaderIndex);
            string s = sr.ReadLine();
            string[] split = s.Split(",");
            split[0] = newIndex.ToString();
            s = newHeader(split);
            fs.Position = int.Parse(changeHeaderIndex);
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s + "              "))));
            fs.Flush();
        }





        public void optimize(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Not in the right format...\n> BGUFS -optimize <filesystem>");
                return;
            }
            string fileSystem = args[1];
            int lastIndexThatFree = 0;
            bool lastFIleIsDeleted = false;
            bool isDistance = false;
            bool isDelete = false;
            List<string> getIndexWithName = getHeaderWithInfo(fileSystem, 3, false);
            List<int> indexesInPhysicalOrder = getOnlyIndexes(getIndexWithName);
            FileStream fs = new FileStream(fileSystem, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            sr.DiscardBufferedData();
            int firstPointer = int.Parse(sr.ReadLine().Split(",")[1]);
            int secondIndex = int.Parse(sr.ReadLine());
            bool isHalfFull = false;
            addBackPointer(fs, sr, secondIndex, firstPointer);
            string[] split = null;
            int lastIndex = 0;
            int i = 0;
            while (i < indexesInPhysicalOrder.Count)
            {
                /// 3 options - 1.delete header+content , 2.delete link, 3.not full header
                /// 1.the file below if regual file or link

                sr.DiscardBufferedData();
                sr.BaseStream.Position = indexesInPhysicalOrder[i];
                string currentHeader = sr.ReadLine();
                split = currentHeader.Split(",");
                if (i + 1 == indexesInPhysicalOrder.Count)
                    break;
                if (split[2] == "2")
                    isHalfFull = true;
                if (!isHalfFull && indexesInPhysicalOrder[i + 1] > indexesInPhysicalOrder[i] + int.Parse(split[1]) + currentHeader.Length + 2)
                    isDistance = true;
                // 1,2
                if (split[2] == "1" || isHalfFull || isDistance)
                {

                    sr.DiscardBufferedData();
                    sr.BaseStream.Position = indexesInPhysicalOrder[i];
                    currentHeader = sr.ReadLine();
                    split = currentHeader.Split(",");
                    isHalfFull = false;
                    isDistance = false;
                    isDelete = false;
                    if (split[2] == "2")
                        isHalfFull = true;
                    if (split[2] == "1")
                        isDelete = true;
                    if (!isHalfFull && !isDelete && indexesInPhysicalOrder[i + 1] > indexesInPhysicalOrder[i] + int.Parse(split[1]) + currentHeader.Length + 2)
                        isDistance = true;
                    if (split.Length == 8 && !isHalfFull && !isDistance) //header delete regular file - uptade header linkedList
                    {
                        changeDadPointer(sr, fs, split[7], split[0]);
                        changeNextHeader(fs, sr, indexesInPhysicalOrder[i], split[0], split[7], false);
                    }
                    else if (!isHalfFull && !isDistance)//header delete link file - uptade header linkedList
                    {
                        changeDadPointer(sr, fs, split[9], split[0]);
                        changeNextHeader(fs, sr, indexesInPhysicalOrder[i], split[0], split[9], true);
                    }
                    int fileSize = 0;
                    sr.DiscardBufferedData();
                    sr.BaseStream.Position = indexesInPhysicalOrder[i + 1];
                    string nextHeader = sr.ReadLine();
                    string[] splitNext = nextHeader.Split(",");
                    if (splitNext.Length == 8) // if we copy regular file
                        fileSize = nextHeader.Length + 1 + int.Parse(splitNext[1]) + 1;
                    else
                        fileSize = nextHeader.Length + 1;
                    byte[] fullFile = new byte[fileSize];
                    fs.Position = indexesInPhysicalOrder[i + 1];
                    fs.Read(fullFile, 0, fileSize);
                    string s = Encoding.UTF8.GetString(fullFile);
                    if (isHalfFull)
                    {
                        fs.Position = indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 1;
                        indexesInPhysicalOrder[i + 1] = indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2;
                    }
                    else if (isDistance && !isDelete)
                    {
                        if (split.Length == 8)
                            lastIndexThatFree = indexesInPhysicalOrder[i] + int.Parse(split[1]) + currentHeader.Length + 2;
                        else
                            lastIndexThatFree = indexesInPhysicalOrder[i] + currentHeader.Length + 1;
                        fs.Position = lastIndexThatFree;
                        indexesInPhysicalOrder[i + 1] = lastIndexThatFree;
                    }
                    else
                        fs.Position = indexesInPhysicalOrder[i];
                    if (isHalfFull)
                        fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("\n"))));
                    fs.Flush();
                    fs.Write(fullFile);
                    lastIndexThatFree = indexesInPhysicalOrder[i] + fileSize;
                    fs.Flush();
                    if (splitNext.Length == 8)
                    {
                        if (!isHalfFull && !isDistance)
                        {
                            updateBackHeader(fs, sr, indexesInPhysicalOrder[i], splitNext[7]);
                            updateNextHeader(fs, sr, indexesInPhysicalOrder[i], splitNext[0]);
                            indexesInPhysicalOrder.RemoveAt(i + 1);
                        }
                        else
                        {
                            updateBackHeader(fs, sr, indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2, splitNext[7]);
                            updateNextHeader(fs, sr, indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2, splitNext[0]);
                        }
                    }
                    else
                    {
                        if (!isHalfFull && !isDistance)
                        {
                            updateBackHeader(fs, sr, indexesInPhysicalOrder[i], splitNext[9]);
                            updateNextHeader(fs, sr, indexesInPhysicalOrder[i], splitNext[0]);
                            indexesInPhysicalOrder.RemoveAt(i + 1);
                        }
                        else
                        {
                            updateBackHeader(fs, sr, indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2, splitNext[9]);
                            updateNextHeader(fs, sr, indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2, splitNext[0]);
                        }
                    }
                }
                else
                {
                    if (split.Length == 8)
                        lastIndexThatFree = indexesInPhysicalOrder[i] + currentHeader.Length + int.Parse(split[1]) + 2;
                    else
                        lastIndexThatFree = indexesInPhysicalOrder[i] + currentHeader.Length + 1;
                }
                if(!isDelete)
                    i++;

            }
            if (split[2] == "1")
            {
                sr.DiscardBufferedData();
                if (split.Length == 10)
                    sr.BaseStream.Position = int.Parse(split[9]);
                else
                    sr.BaseStream.Position = int.Parse(split[7]);
                string s = sr.ReadLine();
                string[] split3 = s.Split(",");
                split3[0] = split[0];
                string hed = newHeader(split3);
                if (split.Length == 10)
                    fs.Position = int.Parse(split[9]);
                else
                    fs.Position = int.Parse(split[7]);
                fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(hed + "               "))));
                fs.Flush();
            }
            deleteBackPointer(fs, sr);
            fs.Close();
            sr.Close();

        }

        private void updateNextHeader(FileStream fs, StreamReader sr, int newIndex, string changeHeaderIndex)
        {
            if (newIndex == int.Parse(changeHeaderIndex) || changeHeaderIndex == "0")
                return;
            sr.DiscardBufferedData();
            sr.BaseStream.Position = int.Parse(changeHeaderIndex);
            string s = sr.ReadLine();
            string[] split = s.Split(",");
            if(split.Length == 8)
                 split[7] = newIndex.ToString();
            else
                split[9] = newIndex.ToString();
            s = newHeader(split);
            fs.Position = int.Parse(changeHeaderIndex);
            fs.Write(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(s + "              "))));
            fs.Flush();
        }

        static void Main(string[] args)
        {
            BGUFS bg = new BGUFS();
            string opCode = args[0];

            switch (opCode)
            {
                case "-create":
                    bg.create(args);
                    break;
                case "-add":
                    bg.add(args);
                    break;
                case "-remove":
                    bg.remove(args);
                    break;
                case "-rename":
                    bg.rename(args);
                    break;
                case "-extract":
                    bg.extract(args);
                    break;
                case "-dir":
                    bg.dir(args);
                    break;
                case "-hash":
                    bg.hash(args);
                    break;
                case "-optimize":
                    bg.optimize(args);
                    break;
                case "-sortAB":
                    bg.sortAB(args);
                    break;
                case "-sortDate":
                    bg.sortDate(args);
                    break;
                case "-sortSize":
                    bg.sortSize(args);
                    break;
                case "-addLink":
                    bg.addLink(args);
                    break;
                default:
                    Console.Out.WriteLine("not in the right format...");
                    Console.Out.WriteLine("try this: BGUFS -OPCODE <filesystem> <...>");
                    break;
            }


        }
    }
}

