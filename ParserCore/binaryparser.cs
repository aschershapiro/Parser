using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using csmatio.io;
using csmatio.types;
namespace Parser
{
    public class BinaryParser
    {
        public DataLabels dataLabels;
        private const int HeaderSize = 3;
        private int FloatSize;
        private int UInt16Size;
        private const int CRCSize = 2;
        private const int FooterSize = 1;
        private int PacketSize;
        private byte[] Header = new byte[3];
        Dictionary<string, float> floatDic = new Dictionary<string, float>();
        Dictionary<string, ushort> UInt16Dic = new Dictionary<string, ushort>();
        Dictionary<string, List<float>> floatarrays = new Dictionary<string, List<float>>();
        Dictionary<string, List<ushort>> ushortarrays = new Dictionary<string, List<ushort>>();
        private enum ParseState { Idle, Header, Data };
        public BinaryParser()
        {
            //Serialize(new DataLabels(), "DataLabels.xml");
            dataLabels = Deserialize("DataLabels.xml");
            FloatSize = dataLabels!.FloatSize;
            UInt16Size = dataLabels!.UInt16Size;
            PacketSize = HeaderSize + FloatSize * 4 + UInt16Size * 2 + CRCSize + FooterSize;
            Header[0] = (byte)dataLabels.Header[0];
            Header[1] = (byte)dataLabels.Header[1];
            Header[2] = (byte)dataLabels.Header[2];
            foreach (var element in dataLabels.FloatLabels)
            {
                floatarrays.Add(element, new List<float>());
            }
            foreach (var element in dataLabels.UInt16Labels)
            {
                ushortarrays.Add(element, new List<ushort>());
            }
        }
        public void ParseFile(string filePath)
        {
            byte[] buffer;
            byte[] packet = new byte[PacketSize];

            try
            {
                buffer = File.ReadAllBytes(filePath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading the file: {ex.Message}");
                return;
            }

            ParseState state = ParseState.Idle;
            int packetStartIndex = -1;

            for (int i = 0; i < buffer.Length; i++)
            {
                byte currentByte = buffer[i];

                switch (state)
                {
                    case ParseState.Idle:
                        if (currentByte == Header[0])
                        {
                            state = ParseState.Header;
                            packetStartIndex = i;
                        }
                        break;

                    case ParseState.Header:
                        if (currentByte == Header[1])
                        {
                            state = ParseState.Data;
                        }
                        else
                        {
                            state = ParseState.Idle;
                        }
                        break;

                    case ParseState.Data:
                        if (i - packetStartIndex < PacketSize)
                        {
                            //Console.WriteLine($"Data byte: {currentByte}");
                        }
                        else
                        {
                            // Found a complete packet
                            //Console.WriteLine("Found a complete packet!");
                            Buffer.BlockCopy(buffer, packetStartIndex, packet, 0, PacketSize);
                            ProcessPacket(packet);
                            state = ParseState.Idle;
                        }
                        break;
                }
            }
            List<MLArray> list = new List<MLArray>();
            foreach (var item in floatarrays)
            {
                list.Add(new MLSingle("floats_" + item.Key.Replace('[', '_').Replace(']', '_'), item.Value.ToArray(), item.Value.Count));
            }
            foreach (var item in ushortarrays)
            {
                list.Add(new MLUInt16("floats_" + item.Key, item.Value.ToArray(), item.Value.Count));
            }
            MatFileWriter mfw = new MatFileWriter(filePath.Split('.')[0]+".mat", list, true);
        }
        public int packages = 0;
        private void ProcessPacket(byte[] packetData)
        {
            ushort crc = CRC16(packetData, (packetData.Length - 3));
            byte b1 = (byte)((crc >> 8) & 0xFF);
            byte b2 = (byte)(crc & 0xFF);

            if (packetData[packetData.Length - 3] == b2 && packetData[packetData.Length - 2] == b1)
            {
                packages++;

                // parse a single packet
                for (int i = 0; i < FloatSize; i++)
                {
                    floatDic[dataLabels.FloatLabels[i]] = BitConverter.ToSingle(packetData, i * 4 + 3);
                }
                for (int i = 0; i < UInt16Size; i++)
                {
                    UInt16Dic[dataLabels.UInt16Labels[i]] = BitConverter.ToUInt16(packetData, i * 2 + 3 + FloatSize * 4);
                }

                //save current packet to an array
                foreach (var element in floatDic)
                {
                    floatarrays[element.Key].Add(element.Value);
                }
                foreach (var element in UInt16Dic)
                {
                    ushortarrays[element.Key].Add(element.Value);
                }


            }
        }
        public ushort CRC16(byte[] data, int length)
        {
            ushort tmp;
            ushort crc = ushort.MaxValue;

            for (int i = 0; i < length; i++)
            {
                tmp = (ushort)((crc & 255) ^ data[i]);
                tmp = (ushort)(((ushort)(tmp << 4) ^ tmp) & 255);
                crc = (ushort)(((int)(crc >> 8) ^ tmp << 8 ^ tmp << 3 ^ (int)(tmp >> 4)));
            }

            return crc;
        }
        public void Serialize(DataLabels dataLabels, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(DataLabels));
                xmlSerializer.Serialize(stream, dataLabels);
            }
        }
        public DataLabels? Deserialize(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(DataLabels));
                return xmlSerializer.Deserialize(stream) as DataLabels;
            }
        }
    }
}
