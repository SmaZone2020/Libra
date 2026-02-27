using System.Text;

namespace Libra.Server.Service.Agent
{
    public static class BinaryPatcher
    {
        public static bool ReplaceString(string filePath, string oldStr, string newStr, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.ASCII;

            byte[] oldBytes = encoding.GetBytes(oldStr);
            byte[] newBytes = encoding.GetBytes(newStr);

            if (newBytes.Length > oldBytes.Length)
                throw new ArgumentException("New string is longer than old string.");

            byte[] paddedNewBytes = new byte[oldBytes.Length];
            Array.Copy(newBytes, paddedNewBytes, newBytes.Length);

            byte[] fileBytes = File.ReadAllBytes(filePath);

            int index = FindBytes(fileBytes, oldBytes);
            if (index == -1)
                return false;

            Array.Copy(paddedNewBytes, 0, fileBytes, index, paddedNewBytes.Length);

            File.WriteAllBytes(filePath, fileBytes);
            return true;
        }

        public static bool ReplaceInt32(string filePath, int oldValue, int newValue)
        {
            byte[] oldBytes = BitConverter.GetBytes(oldValue);
            byte[] newBytes = BitConverter.GetBytes(newValue);

            byte[] fileBytes = File.ReadAllBytes(filePath);

            int index = FindBytes(fileBytes, oldBytes);
            if (index == -1)
                return false;

            Array.Copy(newBytes, 0, fileBytes, index, 4);

            File.WriteAllBytes(filePath, fileBytes);
            return true;
        }

        private static int FindBytes(byte[] haystack, byte[] needle)
        {
            int limit = haystack.Length - needle.Length;
            for (int i = 0; i <= limit; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            return -1;
        }
    }
}
