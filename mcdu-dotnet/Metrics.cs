using System;
using System.Collections.Generic;
using System.Text;

namespace McduDotNet
{
    public static class Metrics
    {
        /// <summary>
        /// The number of lines of display on the MCDU.
        /// </summary>
        public const int Lines = 14;

        /// <summary>
        /// The number of columns of display on the MCDU.
        /// </summary>
        public const int Columns = 24;

        /// <summary>
        /// The total number of cells on the MCDU display.
        /// </summary>
        public const int Cells = Lines * Columns;
    }
}
