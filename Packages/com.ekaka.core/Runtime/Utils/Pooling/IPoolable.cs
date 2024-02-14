using System.Collections;
using System.Collections.Generic;

namespace Core.Utils
{
    public interface IPoolable
    {
        public void Renew();

        public void Release();

        public void Retire();
    }
}
