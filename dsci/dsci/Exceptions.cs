
// This file is generated with T4.
// DON'T EDIT THIS FILE MANUALLY.

using System;

namespace dsci
{
	public class UserCancelException : Exception
    {
        public UserCancelException() {}
        public UserCancelException(string message) : base(message) {}
        public UserCancelException(string message, Exception inner) : base(message, inner) {}
    }

    public class ZipSkippedException : Exception
    {
        public ZipSkippedException() {}
        public ZipSkippedException(string message) : base(message) {}
        public ZipSkippedException(string message, Exception inner) : base(message, inner) {}
    }

    public class InternalErrorException : Exception
    {
        public InternalErrorException() {}
        public InternalErrorException(string message) : base(message) {}
        public InternalErrorException(string message, Exception inner) : base(message, inner) {}
    }

}

