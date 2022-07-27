using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Helpers
{
    public interface ILoadable<T>
    {
        public T? Load(string Path);
    }
}
