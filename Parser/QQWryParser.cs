using MeowMemoirsAPI.Models.IP;
using System.Text;


namespace MeowMemoirsAPI.Parser
{

    public class QQWryParser : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryReader _reader;
        private readonly long _firstIndexOffset;
        private readonly long _lastIndexOffset;
        private readonly long _indexCount;

        private const byte RedirectMode1 = 0x01;
        private const byte RedirectMode2 = 0x02;
        private readonly Encoding _gbkEncoding;

        public QQWryParser(string filePath)
        {
            _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new BinaryReader(_stream);

            // 获取 GBK 编码，如果失败则使用默认编码
            try
            {
                _gbkEncoding = Encoding.GetEncoding("GBK");
            }
            catch
            {
                _gbkEncoding = Encoding.Default;
                Console.WriteLine("Warning: GBK encoding not available, using default encoding");
            }

            // 读取文件头
            _firstIndexOffset = ReadUInt32();
            _lastIndexOffset = ReadUInt32();
            _indexCount = (_lastIndexOffset - _firstIndexOffset) / 7 + 1;
        }

        public IPLocation Query(string ip)
        {
            if (!System.Net.IPAddress.TryParse(ip, out var ipAddress))
                throw new ArgumentException("Invalid IP address");

            var ipBytes = ipAddress.GetAddressBytes();
            if (ipBytes.Length != 4)
                throw new ArgumentException("Only IPv4 is supported by QQWry database");

            var ipNumber = BitConverter.ToUInt32(ipBytes, 0);

            // 二分查找索引
            var indexOffset = BinarySearchIndex(ipNumber);
            if (indexOffset == -1) return null;

            // 读取记录
            return ReadIPRecord(indexOffset);
        }

        private long BinarySearchIndex(uint ip)
        {
            long left = 0;
            long right = _indexCount - 1;

            while (left <= right)
            {
                long mid = (left + right) / 2;
                long offset = _firstIndexOffset + mid * 7;
                _stream.Seek(offset, SeekOrigin.Begin);

                uint midIP = ReadUInt32();
                if (ip < midIP)
                {
                    right = mid - 1;
                }
                else
                {
                    if (mid == _indexCount - 1)
                    {
                        return offset;
                    }

                    _stream.Seek(offset + 7, SeekOrigin.Begin);
                    uint nextIP = ReadUInt32();
                    if (ip < nextIP)
                    {
                        return offset;
                    }
                    else
                    {
                        left = mid + 1;
                    }
                }
            }

            return -1;
        }

        private IPLocation ReadIPRecord(long indexOffset)
        {
            try
            {
                _stream.Seek(indexOffset + 4, SeekOrigin.Begin);
                long recordOffset = ReadUInt24();

                _stream.Seek(recordOffset + 4, SeekOrigin.Begin);

                var location = new IPLocation { Source = "QQWry" };

                byte flag = _reader.ReadByte();

                if (flag == RedirectMode1)
                {
                    long countryOffset = ReadUInt24();
                    _stream.Seek(countryOffset, SeekOrigin.Begin);

                    byte flag2 = _reader.ReadByte();
                    if (flag2 == RedirectMode2)
                    {
                        location.Country = ReadString(ReadUInt24());
                        _stream.Seek(countryOffset + 4, SeekOrigin.Begin);
                    }
                    else
                    {
                        location.Country = ReadString(countryOffset);
                    }
                    location.Area = ReadArea(_stream.Position);
                }
                else if (flag == RedirectMode2)
                {
                    location.Country = ReadString(ReadUInt24());
                    location.Area = ReadArea(indexOffset + 8);
                }
                else
                {
                    location.Country = ReadString(_stream.Position - 1);
                    location.Area = ReadArea(_stream.Position);
                }

                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading IP record: {ex.Message}");
                return new IPLocation
                {
                    Country = "未知",
                    Area = "未知",
                    Source = "QQWry"
                };
            }
        }

        private string ReadArea(long offset)
        {
            try
            {
                _stream.Seek(offset, SeekOrigin.Begin);
                byte flag = _reader.ReadByte();

                if (flag == RedirectMode1 || flag == RedirectMode2)
                {
                    long areaOffset = ReadUInt24(offset + 1);
                    if (areaOffset == 0)
                        return "未知地区";
                    return ReadString(areaOffset);
                }
                return ReadString(offset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading area at offset {offset}: {ex.Message}");
                return "未知地区";
            }
        }

        private string ReadString(long offset)
        {
            try
            {
                _stream.Seek(offset, SeekOrigin.Begin);
                var bytes = new List<byte>();
                byte b;
                while ((b = _reader.ReadByte()) != 0)
                {
                    bytes.Add(b);
                }
                return _gbkEncoding.GetString(bytes.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading string at offset {offset}: {ex.Message}");
                return "未知";
            }
        }

        private uint ReadUInt32()
        {
            return BitConverter.ToUInt32(_reader.ReadBytes(4), 0);
        }

        private long ReadUInt24()
        {
            byte[] bytes = _reader.ReadBytes(3);
            return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16);
        }

        private long ReadUInt24(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return ReadUInt24();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
        }
    }
}
