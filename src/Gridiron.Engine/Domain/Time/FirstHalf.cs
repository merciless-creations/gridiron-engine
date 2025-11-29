using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain.Time
{
    public class FirstHalf : Half
    {
        public FirstHalf() : base(HalfType.First)
        {
        }
    }
}
