using System;
using System.Collections.Generic;

namespace BitMobile.Common.StyleSheet
{
	public interface IStyleSheetContext
	{
        float Scale { get; set; }
        IBound EmptyBound { get; } 
		IStyleSheet CreateStyleSheet ();
	    IBound CreateBound(float width, float height);
	    IBound CreateBound(float width, float height, float contentWidth, float contentHeight);
        IBound MergeBound(IBound bound, float width, float height, IBound maxBound, bool safeProportion);
		IBound StrechBoundInProportion(IBound styleBound, IBound maxBound, float widthProportion, float heightProportion);
        IStyleHelper CreateHelper(IDictionary<Type, IStyle> styles, IStyleSheet styleSheet, IStyledObject subject);
	}
}
