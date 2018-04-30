using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.WebClient.Services
{
    public interface ITenantSelector<T> where T:class
    {
        T Current { get; }
        T Select();
    }
}
