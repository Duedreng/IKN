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
        public void send(byte[] buf, int size)      
        {
            int counter = 0;
            buffer[counter] = DELIMITER;
            counter++;

            for (int i = 0; i < size - 1; i++)
            {
                if (buf[i] == DELIMITER) //A
                {
                    //Bytestuffing, switch to 'BC'
                    buffer[counter] = (byte)'B';
                    counter++;
                    buffer[counter] = (byte)'C';
                    counter++;
                }
                else if (i == 'B')
                {
                    //Bytestuffing, switch to 'BD'
                    buffer[counter] = (byte)'B';
                    counter++;
                    buffer[counter] = (byte)'D';
                    counter++;
                }
                else
                {
                    //Tilføj resultat normalt
                    buffer[counter] = buf[i];
                    counter++;
                }
                counter++;
            }
            //Sætter stop karakter og returnerer resultat
            buffer[counter] = DELIMITER; //A
            counter++;
            serialPort.Write(buffer, 0, counter);
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
            int counter = 0;

            //Vent på startkarakter
            while(serialPort.Read(buffer, 0, 1) != DELIMITER) { }

            foreach (byte i in buf)
            {
                //Læser 1 byte ind i bufferTemp, fra position 0
                serialPort.Read(buffer, 0, 1);
                if (i == DELIMITER) //A
                {
                    //Stop
                    return counter;
                }
                else if (buffer[0] == 'B')
                {
                    //BC = A, BD = B
                    serialPort.Read(buffer, 0, 1);
                    if (i == 'C')
                    {
                        buffer[counter] = DELIMITER;
                    }
                    else if (buffer[0] == 'D')
                    {
                        buffer[counter] = (byte)'B';
                    }
                    counter++;
                }
                else
                {
                    //Read result
                    buf[counter] = buffer[0];
                }
            }
            return counter;
        }
    }
}
