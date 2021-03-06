﻿using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public class JsArguments : JsObject
    {
        protected ValueDescriptor calleeDescriptor;

        protected JsFunction Callee
        {
            get { return this["callee"] as JsFunction; }
            set { this["callee"] = value; }
        }

		private JsInstance[] args = null;

        public JsArguments(IGlobal global, JsFunction callee, JsInstance[] arguments)
            : base()
        {
			this.args = arguments;
            this.global = global;
            Prototype = global.ObjectClass.New();
            // Add the named parameters
            for (int i = 0; i < Math.Max(arguments.Length, callee.Arguments.Count); i++)
            {
                ValueDescriptor d = new ValueDescriptor(this, i < callee.Arguments.Count ? callee.Arguments[i] : i.ToString());

                d.Set(this, i < arguments.Length ? arguments[i] : JsUndefined.Instance);

                if (i < callee.Arguments.Count)
                    this.DefineOwnProperty(callee.Arguments[i], d);
                this.DefineOwnProperty(i.ToString(), d);
            }

            length = arguments.Length;
            calleeDescriptor = new ValueDescriptor(this, "callee");
            DefineOwnProperty("callee", calleeDescriptor);
            calleeDescriptor.Set(this, callee);
            DefineOwnProperty("length", new PropertyDescriptor<JsArguments>(global, this, "length", GetLength));
			DefineOwnProperty("array", new PropertyDescriptor<JsArguments>(global, this, "array", GetArray));
        }

        public override Descriptor GetDescriptor(string index)
        {

            Descriptor result;
            result = base.GetDescriptor(index);
            if (result != null)
                return result;

            if (index == "callee" || Callee == null)
                return null;

            foreach (JsDictionaryObject declaringScope in Callee.DeclaringScopes)
            {
                result = declaringScope.GetDescriptor(index);
                if (result != null)
                    return result;
            }

            return null;
        }

        private new int length;
        private IGlobal global;

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return Length;
        }

        public new const string TYPEOF = "Arguments";

        public override string Class
        {
            get { return TYPEOF; }
        }

        public JsInstance GetLength(JsArguments target)
        {
            return global.NumberClass.New(target.length);
        }

		public JsInstance GetArray(JsArguments target)
		{
			JsArray array = global.ArrayClass.New();	
			if(args!=null)
			{
				for (int i = 0; i < args.Length; i++)
				{
					array[i.ToString()] = args[i];
				}
			}
			return array;
		}
    }
}
