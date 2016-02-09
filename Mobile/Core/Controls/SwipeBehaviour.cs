using System;
using System.Collections.Generic;
using System.Linq;

namespace BitMobile.Controls
{
    public class SwipeBehaviour
    {
        int _index;
        SwipeAlignment _alignment;

        public SwipeBehaviour(Action<float> scroll)
        {
            Scroll = scroll;

            Borders = new List<float>();
            _alignment = SwipeAlignment.Default;
            Percent = 0.5f;
        }

        public event Action<float> Scroll;

        public float Percent { get; set; }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                Scroll(OffsetByIndex);
            }
        }

        public string Alignment
        {
            get { return _alignment.ToString().ToLower(); }
            set
            {
                SwipeAlignment alignment;
                if (!Enum.TryParse(value.Trim(), true, out alignment))
                    throw new Exception(string.Format("Invalid alignment: {0}", value));
                _alignment = alignment;
            }
        }

        public List<float> Borders { get; private set; }

        public float ScrolledMeasure { get; set; }

        public float HandleSwipe(float start, float end, int initialScroll)
        {
            float result;

            float measure = 0;
            float scroll = initialScroll;
            if (start > end)
            {
                scroll += ScrolledMeasure;
                result = Borders.Last();
                for (int i = 2; i < Borders.Count; i++)
                {
                    float lastBorder = Borders[i - 1];
                    float border = Borders[i];
                    measure = border - lastBorder;
					if (scroll <= border)
                    {
                        double point = measure * Percent;
                        if (scroll > lastBorder + point)
                        {
                            result = border;
                            _index = i - 1;
                        }
                        else
                        {
                            result = lastBorder;
                            measure = lastBorder - Borders[i - 2];
                            _index = i - 2;
                        }
                        break;
                    }
                }

                result -= ScrolledMeasure;
                if (result < 0) // because we need to return left/top offset
                    result = 0;

                if (_alignment == SwipeAlignment.Center && measure < ScrolledMeasure)
                {
                    float d = (ScrolledMeasure - measure) / 2;
                    if (result == 0)
                    {
                        result = -1 * d; // only for first item
                    }
                    else
                        result += d;
                }
            }
            else if (start < end)
            {
                result = 0;
                for (int i = Borders.Count - 3; i >= 0; i--)
                {
                    float lastBorder = Borders[i + 1];
                    float border = Borders[i];
                    measure = lastBorder - border;
					if (scroll >= border)
                    {
                        double point = measure * Percent;
                        if (scroll < lastBorder - point)
                        {
                            result = border;
                            _index = i;
                        }
                        else
                        {
                            result = lastBorder;
                            measure = Borders[i + 2] - lastBorder;
                            _index = i + 1;
                        }
                        break;
                    }
                }

                if (result + ScrolledMeasure > Borders.Last())
                    result = Borders.Last() - ScrolledMeasure;

                if (_alignment == SwipeAlignment.Center && measure < ScrolledMeasure)
                {
                    float d = (ScrolledMeasure - measure) / 2;
                    if (result == Borders.Last() - ScrolledMeasure)
                    {
                        result += d;
                    }
                    else
                        result -= d;
                }
            }
            else
                result = initialScroll;

            return result;
        }

        public float OffsetByIndex
        {
            get
            {
                if (Borders.Count > 1)
                {
                    if (_index < 0)
                        _index = 0;
                    else if (_index >= Borders.Count - 1)
                        _index = Borders.Count - 1;

                    float measure = Borders[_index + 1] - Borders[_index];
                    float offset = Borders[_index] + ScrolledMeasure;

                    if (offset > Borders.Last())
                    {
                        offset = Borders.Last();
                        if (_alignment == SwipeAlignment.Center && measure < ScrolledMeasure)
                            offset += (ScrolledMeasure - measure) / 2;
                    }
                    else if (_alignment == SwipeAlignment.Center && measure < ScrolledMeasure)
                        offset -= (ScrolledMeasure - measure) / 2;

                    offset -= ScrolledMeasure;

                    return offset;
                }
                return 0;
            }
        }

        enum SwipeAlignment
        {
            Default,
            Center
        }
    }
}
