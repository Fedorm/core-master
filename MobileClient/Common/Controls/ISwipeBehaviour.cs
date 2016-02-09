using System;
using System.Collections.Generic;

namespace BitMobile.Common.Controls
{
    public interface ISwipeBehaviour
    {
        event Action<float> Scroll;
		event Action IndexChanged;
        float Percent { get; set; }
        int Index { get; set; }
        string Alignment { get; set; }
        bool CenterAlignment { get; }
        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        IReadOnlyCollection<float> Borders { get; }
        float ScrollingArea { get; set; }
        float OffsetByIndex { get; }
        void SetBorders(IEnumerable<float> borders);
        float? HandleSwipe(float current, float last);
    }
}
