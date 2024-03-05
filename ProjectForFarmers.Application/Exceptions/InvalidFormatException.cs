﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmersMarketplace.Application.Exceptions
{
    public class InvalidFormatException : ApplicationException
    {
        public InvalidFormatException(string message, string userFacingMessage) : base(message, userFacingMessage)
        {
        }

        public InvalidFormatException() : base()
        {
        }

        public InvalidFormatException(string message, string userFacingMessage, string? environment, string? action) : base(message, userFacingMessage, environment, action)
        {
        }
    }

}
