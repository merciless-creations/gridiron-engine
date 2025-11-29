using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain.Time
{
    /// <summary>
    /// Represents the second half of a football game (quarters 3 and 4).
    /// </summary>
    public class SecondHalf : Half
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondHalf"/> class.
        /// Creates quarters 3 and 4 with 15 minutes each.
        /// </summary>
        public SecondHalf() : base(HalfType.Second)
        {
        }
    }
}