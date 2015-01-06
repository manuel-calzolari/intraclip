/*
 * IntraClip - A simple clipboard manager for Windows
 * Copyright (C) 2011  Manuel Calzolari
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, version 3 of the License.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace IntraClip
{
    public static class Utils
    {
        public static bool AreEqual(Image a, Image b)
        {
            if (a == b)
                return true;
            if ((a != null) && (b != null))
            {
                if (a.Size != b.Size)
                    return false;
                ImageConverter imageConverter = new ImageConverter();
                byte[] imageA = (byte[])imageConverter.ConvertTo(a, typeof(byte[]));
                byte[] imageB = (byte[])imageConverter.ConvertTo(b, typeof(byte[]));
                for (int i = 0; i < imageA.Length; i++)
                    if (imageA[i] != imageB[i])
                        return false;
                return true;
            }
            return false;
        }

        public static bool AreEqual(Stream a, Stream b)
        {
            if (a == b)
                return true;
            if ((a != null) && (b != null))
            {
                if (a.Length != b.Length)
                    return false;
                byte buf;
                while ((buf = (byte)a.ReadByte()) >= 0)
                {
                    int diff = buf.CompareTo(b.ReadByte());
                    if (diff != 0)
                        return false;
                }
                return true;
            }
            return false;

        }

        public static bool AreEqual(StringCollection a, StringCollection b)
        {
            if (a == b)
                return true;
            if ((a != null) && (b != null))
            {
                if (a.Count != b.Count)
                    return false;
                ArrayList.Adapter(a).Sort();
                ArrayList.Adapter(b).Sort();
                for (int i = 0; i < a.Count; i++)
                    if (a[i] != b[i])
                        return false;
                return true;
            }
            return false;
        }

        public static string FormatItem(StringCollection v)
        {
            v[0] = Regex.Replace(v[0], @"\r\n?|\n", " ");
            if (v[0].Length > 35)
                v[0] = v[0].Substring(0, 16) + "..." + v[0].Substring(v[0].Length - 16, 16);
            if (v.Count > 1)
                v[0] += ", ...";
            return v[0];
        }

        public static string FormatItem(string v)
        {
            v = Regex.Replace(v, @"\r\n?|\n", " ");
            if (v.Length > 35)
                v = v.Substring(0, 16) + "..." + v.Substring(v.Length - 16, 16);
            return v;
        }

        public static string FormatLog(string v)
        {
            return "[" + DateTime.Now + "] " + v;
        }
    }
}
