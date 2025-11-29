using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Base class representing a person in the football simulation.
    /// Serves as the base for <see cref="Player"/>, <see cref="Coach"/>, <see cref="Scout"/>, and <see cref="Trainer"/>.
    /// </summary>
    public class Person
    {
        private static int _personCounter = 0;

        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// Auto-generates default names to ensure the person is always in a valid state.
        /// Production code should override with actual names.
        /// </summary>
        public Person()
        {
            // Auto-generate default names to ensure Person is always in valid state
            // Production code should override with actual names
            var id = System.Threading.Interlocked.Increment(ref _personCounter);
            FirstName = "Person";
            LastName = $"{id}";
        }
    }
}
