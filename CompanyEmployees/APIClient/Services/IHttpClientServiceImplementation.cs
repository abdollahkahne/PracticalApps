using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClient.Services
{
    public interface IHttpClientServiceImplementation
    {
        Task Execute();
    }
}