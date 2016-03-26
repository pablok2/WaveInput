using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveInput
{
    class Loader
    {
        // Local class variables
        short[] _samplesArray;
        bool _headerWasRead;
        bool _stereo;

        // Header stuff
        string _riff;
        uint _size;
        string _wave;
        string _fmt_;
        uint _chunckSize;
        ushort _format;
        ushort _channels;
        uint _sampleRate;
        uint _avgBytesSec;
        ushort _blockAlign;
        ushort _bitsPerSample;
        string _data;
        uint _chunckSize2;

        // Constructor
        public Loader(string fileName)
        {
            LoadFile(fileName);
        }

        public void LoadFile(string filename)
        {
            // Check if the file exists
            if (!File.Exists(filename))
            {
                return;
            }

            // Read the file into memory
            using (MemoryStream ms = new MemoryStream())
            { 
                using (FileStream fs = File.OpenRead(filename))
                {
                    fs.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                }

                // Get header info for later use
                ReadInHeader(ms);

                // Get the samles into the queue
                ReadInSamples(ms);
            }

            for (int i = 0; i < _samplesArray.Length; i++)
                Console.WriteLine(_samplesArray[i]);

        }

        private void ReadInHeader(Stream fs)
        {
            // read all the header stuff - use fs.ReadByte().ToString("X") for hex

            //// Read the header (in this order)            
            _riff = GetFourChars(fs); // "RIFF" type char
            _size = GetUInt32(fs); // 0 type uint //
            _wave = GetFourChars(fs); // "WAVE" type char

            //// read the format chunk
            _fmt_ = GetFourChars(fs); // Four bytes: "fmt " type char
            _chunckSize = GetUInt32(fs); // 16 chucksize type uint
            _format = GetUShort(fs); // 1 format tag type ushort
            _channels = GetUShort(fs); // 2 channels type ushort
            _sampleRate = GetUInt32(fs); // 44100 samples per sec, sample rate type uint
            _avgBytesSec = GetUInt32(fs); // avg bytes per sec //dwSamplesPerSec * wBlockAlign; type uint
            _blockAlign = GetUShort(fs); // (ushort)(wChannels * (wBitsPerSample / 8)) blockalign type ushort
            _bitsPerSample = GetUShort(fs); // 16 bitspersample type ushort

            //// read the data chunk
            _data = GetFourChars(fs); // "data" type char
            _chunckSize2 = GetUInt32(fs); // 0 chunck size type uint

            // to figure out
            //writer.Write(filesize - 8); // size get recorded as this

            // Header was read flag
            if (_channels == 2) _stereo = true;
            _headerWasRead = true;
            
        }

        // Gets the next ushort
        static private ushort GetUShort(Stream fs)
        {
            byte[] bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        // Gets the next short
        static private short GetShort(Stream fs)
        {
            byte[] bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            return BitConverter.ToInt16(bytes, 0);
        }

        // Gets the next uint
        static private uint GetUInt32(Stream fs)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        // Gets the next four chars
        static private string GetFourChars(Stream fs)
        {
            char[] output = new char[4];
            for (int i = 0; i < 4; i++)
                output[i] = (char)fs.ReadByte();
            return new string(output, 0, 4);
        }

        private void ReadInSamples(Stream fs)
        {
            uint totalSample = _chunckSize2/ (uint)(_bitsPerSample/8);
            _samplesArray = new short[totalSample]; // need to subtract the header size

            // Read in each sample
            for (int i = 0; i < totalSample; i++) // Read till end of file
            {
                _samplesArray[i] = GetShort(fs);
            }
        }
    }
}