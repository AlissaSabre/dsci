using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    /// <summary>
    /// An output Stream that compares data against another Stream.
    /// </summary>
    /// <remarks>
    /// An instance of this class is a no-seeking, write-only Stream.
    /// The data written to it are compared against the data read from another stream.
    /// <see cref="IsEqual"/> indicates whether the contents are equal.
    /// </remarks>
    public class ComparingOutputStream : Stream
    {
        private readonly Stream Against;

        private readonly bool DifferenceThrowsException;

        public ComparingOutputStream(Stream against, bool difference_throws_exception)
        {
            Against = against;
            DifferenceThrowsException = difference_throws_exception;
        }

        private bool _IsEqual = true;

        public bool IsEqual { get { return _IsEqual; } }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_IsEqual) return;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (Against.ReadByte() != buffer[offset + i])
                    {
                        _IsEqual = false;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                _IsEqual = false;
                throw;
            }
            if (!_IsEqual && DifferenceThrowsException)
            {
                throw new DifferenceDetectedException();
            }
        }

        public class DifferenceDetectedException : IOException
        {
            public DifferenceDetectedException() : base("Difference detected") { }
        }

        public override bool CanRead { get { return false; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
