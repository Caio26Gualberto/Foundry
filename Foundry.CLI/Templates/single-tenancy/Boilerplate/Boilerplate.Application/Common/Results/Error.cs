using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boilerplate.Application.Common.Results
{
    public class Error
    {
        public string Code { get; }
        public string Message { get; }
        public ErrorType Type { get; }

        public Error(string code, string message, ErrorType type)
        {
            Code = code;
            Message = message;
            Type = type;
        }
    }
}
