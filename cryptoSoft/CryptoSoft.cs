using System;
using System.IO;
using System.Text;

namespace cryptoSoft
{
    /// <summary>
    /// Class used for xor file encryption 
    /// </summary>
    public class cryptoSoftObj
    {
        /// <summary>
        /// Run the Xor enxryption bytes per bytes (take in parameter the source path of the file and the encryption key)
        /// </summary>
        /// <param name="src_Path"></param>
        /// <param name="crypt_key"></param>
        /// <returns></returns>
        public byte[] run_XOR(string src_Path, string crypt_key)
        {
            byte[] text = File.ReadAllBytes(src_Path);
            byte[] result = new byte[text.Length];
            int count = 0;
            StringBuilder bin_key = new StringBuilder();

            foreach (char c in crypt_key)
            {
                //formatting of the encryption key in bytes
                bin_key.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }

            int lenght = bin_key.Length;

            foreach (byte c in text)
            {
                //Text formatting according to the length of the key
                string textToBinary = Convert.ToString(c, 2).PadLeft(8, '0');
                var temp_result = new StringBuilder();

                for (int i = 0; i < 8; i++)
                {
                    temp_result.Append(Convert.ToInt64(textToBinary[i]) ^ Convert.ToInt64(bin_key[i]));
                }

                //Converting string text to bytes
                byte charConvert = Convert.ToByte(Convert.ToString(temp_result), 2);

                //Concatenation of bytes in result
                result[count] = charConvert;
                count++;
            }

          
            return result;
            
        }
    }
}
