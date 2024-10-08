﻿using System;

namespace J113D.Common.Json
{
    /// <summary>
    /// Exception for when reading invalid properties. Should only be thrown in mis-implementation.
    /// </summary>
    public class InvalidPropertyException : Exception
    {
        /// <summary>
        /// Creates a new oinstance of the invalid property exception.
        /// </summary>
        public InvalidPropertyException() : base("Invalid property implementation!") { }
    }
}
