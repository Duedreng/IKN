using System;
using System.IO.Ports;


/// <summary>
/// Link.
/// </summary>
namespace Linklaget
{
    /// <summary>
    /// Link.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The DELIMITE for slip protocol.
        /// </summary>
        const byte DELIMITER = (byte)'A';
        /// <summary>
        /// The buffer for link.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The serial port.
        /// </summary>
        SerialPort serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="link"/> class.
        /// </summary>
        public Link(int BUFSIZE, string APP)
        {
            // Create a new SerialPort object with default settings.
#if DEBUG
            if (APP.Equals("FILE_SERVER"))
            {
                serialPort = new SerialPort("/dev/ttySn0", 115200, Parity.None, 8, StopBits.One);
            }
            else
            {
                serialPort = new SerialPort("/dev/ttySn1", 115200, Parity.None, 8, StopBits.One);
            }
#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
#endif
            if (!serialPort.IsOpen)
                serialPort.Open();

            buffer = new byte[(BUFSIZE * 2)];

            // Uncomment the next line to use timeout
            //serialPort.ReadTimeout = 500;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Send the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        /// <param name='result'>
        /// Result.
        /// </param>
        public void send(byte[] buf, int size)      //EGEN KODE
        {
            byte[] result = new byte[buf.Length];
            int counter = 0;
            result[counter] = (byte)'A';

            bool CFlag = false;
            bool DFlag = false;

            foreach (byte i in buf)
            {

                if (CFlag) {
                    result[i] = (byte)'C';
                    CFlag = false;
                        }
                else if (DFlag)
                {
                    result[i] = (byte)'D';
                    DFlag = false;
                }
                else if (buf[i] == 'A')
                {
                    //Bytestuffing, switch to 'BC'
                    result[i] = (byte)'B';
                    CFlag = true;
                }
                else if (i == 'B')
                {
                    //Bytestuffing, switch to 'BD'
                    result[i] = (byte)'B';
                    CFlag = true;
                }
                else
                {
                    //Tilføj resultat
                    result[counter] = buf[i];
                }

                if (i == size - 1)
                {
                    result[i] = (byte)'A';
                    //CFlag = false;
                    //DFlag = false;
                }
                counter++;
            }
            //Returner resultat
            serialPort.Write(result, 0, result.Length);
        }
        /// <summary>
        /// Receive the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        /// <param name='result'>
        /// Result.
        /// </param>
        public int receive(ref byte[] buf)      //EGEN KODE
        {
            byte[] result = new byte[buf.Length];
            bool BFlag = false;

            while(serialPort.Read(buf, 0, buf.Length) != 'A') { }

            foreach (byte i in buf)
            {
                if (BFlag)
                {
                    if (i == 'C')
                        result[i] = (byte)'A';
                    else if (i == 'D')
                        result[i] = (byte)'B';

                    BFlag = false;
                }
                else if (i == 'A')
                {
                    return 0;
                }
                else if (i == 'B')
                {
                    BFlag = true;
                }
                else
                {
                    //Read result
                }

                if (i == buf.Length)
                {
                    //Forward array
                }
            }
            
            return 0;
        }
    }
}
