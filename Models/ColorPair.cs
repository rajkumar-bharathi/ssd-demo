using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorCodeDemo.Models
{
    public class ColorPair
    {
        public int PairNumber { get; set; }

        public string MajorColor { get; set; }

        public string MinorColor { get; set; }

        public Brush MajorBrush { get; set; }

        public Brush MinorBrush { get; set; }
    }
}
