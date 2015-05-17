using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chat
{
    public interface IChatRepository
    {
        void Add(string name, string message);
    }
}
