/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros.Mega {
    public static class Base64 {
        // modified base64 conversion (no trailing '=' and '-_' instead of '+/')
        public static char to64(byte c) {
            c &= 63;
            if (c < 26) return (char)('A' + (char)c);
            if (c < 52) return (char)((char)c - 26 + 'a');
            if (c < 62) return (char)((char)c - 52 + '0');
            if (c == 62) return '-';
            return '_';
        }

        public static byte from64(char c) {
            if (c >= 'A' && c <= 'Z') return (byte)(c - 'A');
            if (c >= 'a' && c <= 'z') return (byte)(c - 'a' + 26);
            if (c >= '0' && c <= '9') return (byte)(c - '0' + 52);
            if (c == '-') return 62;
            if (c == '_') return 63;
            return 255;
        }

        public static long atol(string a) {
            byte[] b = atob(a);
            if (b.Length < 8) Array.Resize(ref b, 8);
            return BitConverter.ToInt64(b, 0);
        }

        public static byte[] atob(string a) {
            byte[] c = new byte[4];
            int i;

            c[3] = 0;
            List<byte> b = new List<byte>();

            do {
                for (i = 0; i < 4; i++) if ((i == a.Length) || ((c[i] = from64(a[i])) == 255)) break;

                a = a.Substring(i);

                if (i == 0) return b.ToArray();
                b.Add((byte)((c[0] << 2) | ((c[1] & 0x30) >> 4)));
                if (i < 3) return b.ToArray();
                b.Add((byte)((c[1] << 4) | ((c[2] & 0x3c) >> 2)));
                if (i < 4) return b.ToArray();
                b.Add((byte)((c[2] << 6) | c[3]));
            } while (true);
        }

        public static string btoa(long data) {
            return btoa(BitConverter.GetBytes(data));
        }

        public static string btoa(byte[] data, int blen = -1) {
            StringBuilder sb = new StringBuilder();
            if (blen == -1) {
                blen = data.Length;
                while (data[blen - 1] == 0 && blen > 0) blen--;
            }
            int b = 0;
            do {
                if (blen <= 0) break;
                sb.Append(to64((byte)(data[b] >> 2)));
                sb.Append(to64((byte)((data[b] << 4) | (((blen > 1) ? data[b + 1] : 0) >> 4))));
                if (blen < 2) break;
                sb.Append(to64((byte)(data[b + 1] << 2 | (((blen > 2) ? data[b + 2] : 0) >> 6))));
                if (blen < 3) break;
                sb.Append(to64(data[b + 2]));

                blen -= 3;
                b += 3;
            } while (true);
            return sb.ToString();
        }
    }
}
