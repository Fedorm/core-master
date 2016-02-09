using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Common.Controls;

namespace BitMobile.Controls
{
    public class SwipeBehaviour : ISwipeBehaviour
    {
        private int _index;
        private SwipeAlignment _alignment;
        private List<float> _borders;

        public SwipeBehaviour(Action<float> scroll)
        {
            Scroll = scroll;

            _alignment = SwipeAlignment.Default;
            Percent = 0.5f;
        }

        public event Action<float> Scroll;

        public event Action IndexChanged;

        public float Percent { get; set; }

        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    int oldIndex = _index;
                    _index = value;

                    FixIndex();

                    float offset = CalcOffsetByIndex(_index > oldIndex);
                    Scroll(offset);
                    if (IndexChanged != null)
                        IndexChanged();
                }
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

        public bool CenterAlignment { get { return _alignment == SwipeAlignment.Center; } }

        public IReadOnlyCollection<float> Borders
        {
            get
            {
                return _borders != null ? _borders.AsReadOnly() : null;
            }
        }

        public float ScrollingArea { get; set; }

        public float OffsetByIndex
        {
            get
            {
                return CalcOffsetByIndex(false);
            }
        }

        public void SetBorders(IEnumerable<float> borders)
        {
            _borders = new List<float>(borders);
            FixIndex();
        }

        public float? HandleSwipe(float current, float last)
        {
            if (IsEmpty())
                return 0;

            float? result;

            int lastIndex = _index;
            if (current > last)
                result = HandleSwipeToStart(current);
            else if (current < last)
                result = HandleSwipeToEnd(current);
            else
                result = null;

            if (_index != lastIndex && IndexChanged != null)
                IndexChanged();

            return result;
        }

        private float HandleSwipeToStart(float scroll)
        {
            int index = _borders.Count - 2;
            scroll += ScrollingArea;
            for (int i = 2; i < _borders.Count; i++)
            {
                float lastBorder = _borders[i - 1];
                float border = _borders[i];
                float size = border - lastBorder;
                if (scroll <= border || scroll <= Math.Round(border))
                {
                    double point = size * Percent;
                    if (scroll > lastBorder + point)
                        index = i - 1;
                    else
                        index = i - 2;
                    break;
                }
            }
            _index = index;

            return CalcOffsetByIndex(true);
        }

        private float HandleSwipeToEnd(float scroll)
        {
            int index = 0;
            for (int i = _borders.Count - 3; i >= 0; i--)
            {
                float lastBorder = _borders[i + 1];
                float border = _borders[i];
                float size = lastBorder - border;
                if (scroll >= border || scroll >= Math.Round(border))
                {
                    double point = size * Percent;
                    if (scroll < lastBorder - point)
                        index = i;
                    else
                        index = i + 1;
                    break;
                }
            }
            _index = index;

            return CalcOffsetByIndex(false);
        }

        private float CalcOffsetByIndex(bool forwardDirection)
        {
            float offset;
            if (!IsEmpty())
            {
                float size = _borders[_index + 1] - _borders[_index];

                if (_alignment != SwipeAlignment.Center)
                {
                    offset = forwardDirection ? _borders[_index + 1] - ScrollingArea : _borders[_index];

                    if (offset < 0)
                        offset = 0;

                    if (_index > 0 && offset + ScrollingArea > _borders.Last())
                        offset = _borders.Last() - ScrollingArea;
                }
                else
                    offset = _borders[_index] - (ScrollingArea - size) / 2;
            }
            else
                offset = 0;
            return offset;
        }

        private void FixIndex()
        {
            if (_index < 0)
                _index = 0;
            else if (!IsEmpty() && _index >= _borders.Count - 1)
                _index = _borders.Count - 2;
        }

        private bool IsEmpty()
        {
            return _borders == null || _borders.Count == 0;
        }

        enum SwipeAlignment
        {
            Default,
            Center
        }
    }
}
