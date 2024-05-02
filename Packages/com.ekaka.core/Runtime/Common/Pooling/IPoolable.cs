using System.Collections;
using System.Collections.Generic;

namespace Core.Common
{
    public interface IPoolable
    {
        public void Renew();

        public void Release();

        public void Retire();
    }
}
