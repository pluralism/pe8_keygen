using Microsoft.Win32;
using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;

namespace pe8_keygen
{
    public static class ExtensionMethods
    {
        // https://stackoverflow.com/questions/5015593/how-to-replace-part-of-string-by-position
        public static string ReplaceAt(this string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replace);
        }
    }

    class Keygen
    {
        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string PathName,
            StringBuilder VolumeNameBuffer,
            UInt32 VolumeNameSize,
            ref UInt32 VolumeSerialNumber,
            ref UInt32 MaximumComponentLength,
            ref UInt32 FileSystemFlags,
            StringBuilder FileSystemNameBuffer,
            UInt32 FileSystemNameSize);

        private int[] SecretMatrix = { 0x16, 0x0C, 0x10, 0x21, 0x1C, 0x05, 0x24,
                                       0x0A, 0x14, 0x18, 0x08, 0x20, 0x1A, 0x23,
                                       0x0F, 0x0E, 0x11, 0x02, 0x0B, 0x06, 0x1F,
                                       0x22, 0x03, 0x13, 0x0D, 0x1B, 0x19, 0x12,
                                       0x1D, 0x09, 0x1e, 0x17, 0x04, 0x15, 0x07, 0x01 };

        private string codeKey = string.Empty;

        private string CryptLoop(string st, string pw)
        {
            int p = 0;
            int j = 0;
            int n = 0;
            string x = string.Empty;
            bool loop = true;

            for(int i = 0; i < st.Length; i++)
            {
                p = p + 1;
                p = p > pw.Length ? 1 : p;
                j = pw[p - 1] | 0x80;
                n = st.Substring(i)[0];
                loop = true;

                while(loop)
                {
                    n = n ^ j;
                    if(n < 0x1F)
                    {
                        n += 0x80;
                        continue;
                    }

                    if(n > 0x7F && n < 0x9F)
                    {
                        n -= 0x80;
                        continue;
                    }
                    loop = false;
                }
                x += (char)n;
            }

            return x;
        }

        private string CryptTP(string code, int size)
        {
            byte[] bytes = { 0x54, 0x45, 0x52, 0x52, 0x41, 0x50, 0x52, 0x4F, 0x54 };
            return CryptLoop(code.PadRight(size, ' '), Encoding.ASCII.GetString(bytes)).Trim();
        }

        private int CalculateKeySum(string key)
        {
            int sum = 0;
            int multiplier = 1;
            string reversedKey = key.Reverse().ToString();

            for(int i = 0; i < reversedKey.Length; i++, multiplier++)
            {
                sum += Convert.ToInt32(reversedKey[i]) * (multiplier + 1);
            }

            return sum;
        }

        private int CalculateSecretNumber(int sum)
        {
            sum %= 11;
            return sum < 2 ? 0 : 11 - sum;
        }

        private void ClearRegistryKeys()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\VB and VBA Program Settings\TZ");
            if(registryKey != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\VB and VBA Program Settings\TZ");
            }
        }

        public string GeneratePassword(string hddID, string pdNum)
        {
            DateTime now = DateTime.Now;
            string dateKey = now.Day.ToString().PadLeft(2, '0') + now.Year.ToString().PadLeft(4, '0') +
                now.Month.ToString().PadLeft(2, '0');

            string hourKey = now.Second.ToString().PadLeft(2, '0') + now.Minute.ToString().PadLeft(2, '0') +
                now.Hour.ToString().PadLeft(2, '0');

            string hddIdKey = hddID.PadLeft(9, '0');

            string productNumKey = pdNum.PadLeft(10, '0');

            return hourKey + dateKey + hddIdKey + productNumKey;
        }

        private string GenerateRandomKey()
        {
            string finalKey = string.Empty;
            int keySum = 0;

            for (int i = 0; i < 10; i++)
            {
                codeKey += NumberUtils.GetRandomNumber(2, 9);
            }

            keySum = CalculateKeySum(codeKey);
            codeKey += CalculateSecretNumber(keySum);

            for(int i = 0; i < 11; i++)
            {
                codeKey += NumberUtils.GetRandomNumber(2, 9);
            }

            keySum = CalculateKeySum(codeKey);
            codeKey += CalculateSecretNumber(keySum);

            for(int i = 0; i < 4; i++)
            {
                codeKey += NumberUtils.GetRandomNumber(2, 9);
            }

            codeKey += DateTime.Now.ToString("ddMMyyyy");
            keySum = CalculateKeySum(codeKey);
            codeKey += CalculateSecretNumber(keySum);

            string scrambledKey = new string(' ', SecretMatrix.Length);
            for(int i = 0; i < SecretMatrix.Length; i++)
            {
                scrambledKey = scrambledKey.ReplaceAt(i, 1, codeKey[SecretMatrix[i] - 1].ToString());
            }

            for (int i = 0; i < 10; i++)
            {
                finalKey += scrambledKey[i];
            }

            finalKey += "-";

            for (int i = 10; i < 18; i++)
            {
                finalKey += scrambledKey[i];
            }

            finalKey += "-";

            for (int i = 18; i < 28; i++)
            {
                finalKey += scrambledKey[i];
            }

            finalKey += "-";

            for (int i = 28; i < scrambledKey.Length; i++)
            {
                finalKey += scrambledKey[i];
            }

            return finalKey;
        }

        private string GetHDDID()
        {
            uint serialNumber = 0;
            uint maxComponentLength = 0;
            StringBuilder volumeName = new StringBuilder(256);
            UInt32 fileSystemFlags = new UInt32();
            StringBuilder fileSystemName = new StringBuilder(256);

            string rootDrive = Path.GetPathRoot(Environment.SystemDirectory);
            GetVolumeInformation(rootDrive, volumeName,
                (UInt32)volumeName.Capacity, ref serialNumber,
                ref maxComponentLength, ref fileSystemFlags,
                fileSystemName, (UInt32)fileSystemName.Capacity);

            return serialNumber.ToString().Substring(0, serialNumber.ToString().Length - 1);
        }

        private string ClearBase10(string val)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in val)
            {
                if(Char.IsDigit(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string ChrBase(int pos)
        {
            if (pos > 29 || pos < 0)
            {
                return string.Empty;
            }

            return "NT9BVR8CX7ZL6KYJ5EHG4FD3SA2PUW".Substring(pos, 1);
        }

        private string ConvertToBase29(string val)
        {
            int index = 0;
            val = ClearBase10(val);

            if(val.Length <= 9)
            {
                index = 1;
            } else
            {
                index = val.Length / 9;
                if(val.Length % 9 > 0)
                {
                    index += 1;
                }
            }

            string part = string.Empty;
            string result = string.Empty;
            string numStr = string.Empty;
            string zeroNs = string.Empty;
            long number = 0;
            long resMod = 0;
            long numAux = 0;
            int pos = 0;

            for(int i = 0; i < index; i++)
            {
                part = string.Empty;
                if(i > 0)
                {
                    result += "-";
                }
                pos = 0;
                numStr = val.Substring(i * 9, i * 9 + 9 >= val.Length ? val.Length - i * 9 : 9);
                zeroNs = string.Empty;
                
                while(numStr.Substring(pos, 1) == "0")
                {
                    zeroNs += "0";
                    pos++;
                    if(pos > numStr.Length)
                    {
                        break;
                    }
                }

                number = Convert.ToInt64(numStr);
                while(true)
                {
                    numAux = Convert.ToInt64(number / 29);
                    resMod = number - (numAux * 29);
                    part = ChrBase(Convert.ToInt32(resMod)) + part;
                    if(numAux > 29)
                    {
                        number = numAux;
                    } else
                    {
                        if(numAux > 0)
                        {
                            part = ChrBase(Convert.ToInt32(numAux)) + part;
                        }
                        break;
                    }
                }
                result = result + zeroNs + part;
            }
            return result;
        }

        public void CreateKey()
        {
            ClearRegistryKeys();

            string key = GenerateRandomKey();
            long serialNumberAux = Convert.ToInt64(codeKey.Substring(0, 10));

            string hddID = GetHDDID();
            Console.WriteLine($"HDD ID: {hddID}");

            string ckKey = CryptTP(key, 39);
            Console.WriteLine($"CK Key: {ckKey}");

            string password = GeneratePassword(hddID, codeKey.Substring(0, 10));
            string base29Password = ConvertToBase29(password);
            string csKey = CryptTP(base29Password, 39);
            Console.WriteLine($"CS Key: {csKey}");

            string datetimeWithTT = DateTime.Now.ToString("MM/dd/yy hh:mm:ss tt", CultureInfo.InvariantCulture);
            datetimeWithTT = datetimeWithTT + 1.ToString().PadLeft(3, '0');
            string daKey = CryptTP(datetimeWithTT, 25);
            Console.WriteLine($"DA Key: {daKey}");

            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\VB and VBA Program Settings\TZ\P0106");
            registryKey.SetValue("CK", ckKey);
            registryKey.SetValue("CS", csKey);
            registryKey.SetValue("DA", daKey);
        }
    }
}
