// Exception and marshaling helpers every generated gmsh wrapper calls
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Rasm.Gmsh
{
    /// <summary>Failure one gmsh API call reported.</summary>
    public sealed class GmshException : Exception
    {
        /// <summary>Nonzero status the C API returned through ierr.</summary>
        public int Code { get; }

        /// <summary>Create the exception for one status and message.</summary>
        public GmshException(int code, string message) : base(message)
        {
            Code = code;
        }
    }

    internal static partial class GmshMarshal
    {
        internal static void KeepAlive(object callback)
        {
            // A normal handle keeps the delegate alive for the process lifetime, gmsh holds the pointer until exit
            GCHandle.Alloc(callback);
        }

        internal static void Check(int ierr)
        {
            if (ierr == 0)
            {
                return;
            }
            GmshNative.gmshLoggerGetLastError(out IntPtr message, out int status);
            throw new GmshException(ierr, status == 0 ? OutString(message) : "gmsh call failed with status " + ierr.ToString());
        }

        internal static string OutString(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                return string.Empty;
            }
            int length = 0;
            while (Marshal.ReadByte(value, length) != 0)
            {
                length++;
            }
            byte[] buffer = new byte[length];
            Marshal.Copy(value, buffer, 0, length);
            GmshNative.gmshFree(value);
            return Encoding.UTF8.GetString(buffer);
        }

        internal static IntPtr[] InStrings(string[] values)
        {
            string[] source = values ?? Array.Empty<string>();
            IntPtr[] buffers = new IntPtr[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(source[i] ?? string.Empty);
                buffers[i] = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, buffers[i], bytes.Length);
                Marshal.WriteByte(buffers[i], bytes.Length, 0);
            }
            return buffers;
        }

        internal static void FreeAll(IntPtr[] buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                Marshal.FreeHGlobal(buffers[i]);
            }
        }

        internal static int[] Flatten((int, int)[] pairs)
        {
            (int, int)[] source = pairs ?? Array.Empty<(int, int)>();
            int[] flat = new int[source.Length * 2];
            for (int i = 0; i < source.Length; i++)
            {
                flat[2 * i] = source[i].Item1;
                flat[2 * i + 1] = source[i].Item2;
            }
            return flat;
        }

        internal static (int, int)[] OutPairs(IntPtr values, long count)
        {
            int[] flat = OutInts(values, count);
            (int, int)[] pairs = new (int, int)[flat.Length / 2];
            for (int i = 0; i < pairs.Length; i++)
            {
                pairs[i] = (flat[2 * i], flat[2 * i + 1]);
            }
            return pairs;
        }

        internal static string[] OutStrings(IntPtr values, long count)
        {
            string[] result = new string[count];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutString(Marshal.ReadIntPtr(values, i * IntPtr.Size));
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static (int, int)[][] OutJaggedPairs(IntPtr values, IntPtr counts, long count)
        {
            long[] lengths = OutLongs(counts, count);
            (int, int)[][] result = new (int, int)[count][];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutPairs(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }
    }
}
