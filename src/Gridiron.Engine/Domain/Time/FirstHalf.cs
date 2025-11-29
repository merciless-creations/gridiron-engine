using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain.Time
{
    /// <summary>
    /// Represents the first half of a football game (quarters 1 and 2).
    /// </summary>
    public class FirstHalf : Half
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstHalf"/> class.
        /// Creates quarters 1 and 2 with 15 minutes each.
        /// </summary>
        public FirstHalf() : base(HalfType.First)
        {
        }
    }
}
