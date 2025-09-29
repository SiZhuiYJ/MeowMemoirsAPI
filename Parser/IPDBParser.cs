using MeowMemoirsAPI.Models.IP;
using System.Text;

namespace MeowMemoirsAPI.Parser
{
    public class IPDBParser : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryReader _reader;

        private readonly int _offsetLen;
        private readonly int _ipLen;
        private readonly long _recordCount;
        private readonly long _firstIndexOffset;
        private readonly int _fieldCount;

        public IPDBParser(string filePath)
        {
            _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new BinaryReader(_stream);

            // 读取文件头
            if (Encoding.ASCII.GetString(_reader.ReadBytes(4)) != "IPDB")
                throw new InvalidDataException("Invalid IPDB file");

            _reader.ReadBytes(2); // 版本号
            _offsetLen = _reader.ReadByte();
            _ipLen = _reader.ReadByte();
            _recordCount = ReadInt64();
            _firstIndexOffset = ReadInt64();
            _fieldCount = _reader.ReadByte();
            _reader.ReadBytes(7); // 保留字段
            ReadInt64(); // 数据库版本偏移
        }

        public IPLocation Query(string ip)
        {
            if (!System.Net.IPAddress.TryParse(ip, out var ipAddress))
                throw new ArgumentException("Invalid IP address");

            var ipBytes = ipAddress.GetAddressBytes();

            // 二分查找
            long left = 0;
            long right = _recordCount - 1;
            long foundOffset = -1;

            while (left <= right)
            {
                long mid = (left + right) / 2;
                long indexOffset = _firstIndexOffset + mid * (_ipLen + _offsetLen);

                _stream.Seek(indexOffset, SeekOrigin.Begin);
                var midIPBytes = _reader.ReadBytes(_ipLen);

                int compare = CompareIP(ipBytes, midIPBytes);
                if (compare < 0)
                {
                    right = mid - 1;
                }
                else if (compare > 0)
                {
                    if (mid == _recordCount - 1)
                    {
                        foundOffset = ReadOffset(indexOffset + _ipLen);
                        break;
                    }

                    _stream.Seek(indexOffset + _ipLen + _offsetLen, SeekOrigin.Begin);
                    var nextIPBytes = _reader.ReadBytes(_ipLen);

                    if (CompareIP(ipBytes, nextIPBytes) < 0)
                    {
                        foundOffset = ReadOffset(indexOffset + _ipLen);
                        break;
                    }
                    else
                    {
                        left = mid + 1;
                    }
                }
                else
                {
                    foundOffset = ReadOffset(indexOffset + _ipLen);
                    break;
                }
            }

            if (foundOffset == -1) return null;

            return ReadRecord(foundOffset);
        }

        private IPLocation ReadRecord(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);

            var fields = new string[_fieldCount];
            for (int i = 0; i < _fieldCount; i++)
            {
                byte b = _reader.ReadByte();
                if (b == 0x01) // 重定向
                {
                    long redirectOffset = ReadOffset(_stream.Position);
                    fields[i] = ReadString(redirectOffset);
                    _stream.Seek(_offsetLen, SeekOrigin.Current);
                }
                else if (b == 0x02) // 重定向
                {
                    long redirectOffset = ReadOffset(_stream.Position);
                    fields[i] = ReadString(redirectOffset);
                    // 继续读取下一个字段
                }
                else
                {
                    fields[i] = ReadString(_stream.Position - 1);
                }
            }

            return new IPLocation
            {
                Country = fields.Length > 0 ? fields[0] : "",
                Area = fields.Length > 1 ? fields[1] : "",
                ISP = fields.Length > 2 ? fields[2] : "",
                Source = "IPDB"
            };
        }

        private string ReadString(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            var bytes = new List<byte>();
            byte b;
            while ((b = _reader.ReadByte()) != 0)
            {
                bytes.Add(b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private long ReadOffset(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            byte[] bytes = _reader.ReadBytes(_offsetLen);
            long result = 0;
            for (int i = 0; i < _offsetLen; i++)
            {
                result |= (long)bytes[i] << (8 * i);
            }
            return result;
        }

        private int CompareIP(byte[] ip1, byte[] ip2)
        {
            for (int i = 0; i < Math.Min(ip1.Length, ip2.Length); i++)
            {
                int cmp = ip1[i].CompareTo(ip2[i]);
                if (cmp != 0) return cmp;
            }
            return ip1.Length.CompareTo(ip2.Length);
        }

        private long ReadInt64()
        {
            byte[] bytes = _reader.ReadBytes(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
        }
    }
}
