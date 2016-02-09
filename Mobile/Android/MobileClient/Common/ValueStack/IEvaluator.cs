using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ValueStack
{
    public interface IEvaluator
    {
        object Evaluate(String expression, object root);
    }
}
