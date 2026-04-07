using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boilerplate.Application.Common.Results
{
    public enum ErrorType
    {
        Validation = 0,
        NotFound = 1,
        Unauthorized = 2,
        Forbidden = 3,
        Conflict = 4,
        Unexpected = 5
    }
}
