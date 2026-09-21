using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorCodeDemo.ViewModels
{
    using ColorCodeDemo.Models;
    using System.Collections.ObjectModel;
    using System.Windows.Media;

    public class MainViewModel
    {
        public ObservableCollection<ColorPair> Pairs { get; }

        public MainViewModel()
        {
            Pairs = CreatePairs();
        }

        private ObservableCollection<ColorPair> CreatePairs()
        {
            var majorColors = new[]
            {
            ("White", Brushes.White),
            ("Red", Brushes.Red),
            ("Black", Brushes.Black),
            ("Yellow", Brushes.Yellow),
            ("Violet", Brushes.Violet)
        };

            var minorColors = new[]
            {
            ("Blue", Brushes.Blue),
            ("Orange", Brushes.Orange),
            ("Green", Brushes.Green),
            ("Brown", Brushes.Brown),
            ("Slate", Brushes.SlateGray)
        };

            var result = new ObservableCollection<ColorPair>();

            int pairNo = 1;

            foreach (var major in majorColors)
            {
                foreach (var minor in minorColors)
                {
                    result.Add(new ColorPair
                    {
                        PairNumber = pairNo++,
                        MajorColor = major.Item1,
                        MinorColor = minor.Item1,
                        MajorBrush = major.Item2,
                        MinorBrush = minor.Item2
                    });
                }
            }

            return result;
        }
    }
}
