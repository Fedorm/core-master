//
// System.Security.Cryptography.MiniParser: Internal XML parser implementation
//
// Authors:
//	Sergey Chaban
//

/*
 * Copyright (c) 2001, 2002 Wild West Software
 * Copyright (c) 2002 Sergey Chaban
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Telegram.Math
{
    [CLSCompliant(false)]
    internal class MiniParser
    {
        private static readonly int INPUT_RANGE = 13;

        private static readonly ushort[] tbl =
        {
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 1,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 0,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 128,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 2,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 133,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 16,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.FLUSH_CHARS_STATE_CHANGE << 8) | 4,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.END_ELEM << 8) | 0,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 2,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 2,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 5,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 4,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.END_NAME << 8) | 6,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.END_NAME << 8) | 7,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.END_NAME << 8) | 8,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 0,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 3,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 0,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.FLUSH_CHARS_STATE_CHANGE << 8) | 1,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.PROC_CHAR_REF << 8) | 10,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 7,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 9,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.START_ELEM << 8) | 6,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.START_ELEM << 8) | 7,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 8,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 9,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.SET_ATTR_NAME << 8) | 11,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 12,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 13,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.PROC_CHAR_REF << 8) | 10,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 10,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 11,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ERROR << 8) | 132,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.SET_ATTR_NAME << 8) | 11,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 12,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ERROR << 8) | 130,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.SEND_CHARS << 8) | 2,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 16,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ERROR << 8) | 134,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.SET_ATTR_VAL << 8) | 17,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.PROC_CHAR_REF << 8) | 14,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 14,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.SET_ATTR_VAL << 8) | 17,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.PROC_CHAR_REF << 8) | 15,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 15,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.START_CDATA << 8) | 18,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 0,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.START_CDATA << 8) | 19,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.START_ELEM << 8) | 6,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.START_ELEM << 8) | 7,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.STATE_CHANGE << 8) | 17,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CHARS_STATE_CHANGE << 8) | 9,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ERROR << 8) | 129,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.END_CDATA << 8) | 10,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 18,
            ((ushort) CharKind.LEFT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.SLASH << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.RIGHT_BR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.PI_MARK << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.EQ << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.AMP << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.SQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.BANG << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.LEFT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.SPACE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.RIGHT_SQBR << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.DQUOTE << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.ACC_CDATA << 8) | 19,
            ((ushort) CharKind.CHARS << 12) | ((ushort) ActionCode.UNKNOWN << 8) | 255,
            0xFFFF
        };

        protected static string[] errors =
        {
            /* 0 */ "Expected element",
            /* 1 */ "Invalid character in tag",
            /* 2 */ "No '='",
            /* 3 */ "Invalid character entity",
            /* 4 */ "Invalid attr value",
            /* 5 */ "Empty tag",
            /* 6 */ "No end tag",
            /* 7 */ "Bad entity ref"
        };

        protected int col;
        protected int line;
        protected bool splitCData;
        protected int[] twoCharBuff;

        public MiniParser()
        {
            twoCharBuff = new int[2];
            splitCData = false;
            Reset();
        }

        public void Reset()
        {
            line = 0;
            col = 0;
        }

        protected static bool StrEquals(string str, StringBuilder sb, int sbStart, int len)
        {
            if (len != str.Length) return false;
            for (int i = 0; i < len; i++)
            {
                if (str[i] != sb[sbStart + i]) return false;
            }
            return true;
        }

        protected void FatalErr(string descr)
        {
            throw new XMLError(descr, line, col);
        }

        protected static int Xlat(int charCode, int state)
        {
            int p = state*INPUT_RANGE;
            int n = System.Math.Min(tbl.Length - p, INPUT_RANGE);
            for (; --n >= 0;)
            {
                ushort code = tbl[p];
                if (charCode == (code >> 12)) return (code & 0xFFF);
                p++;
            }
            return 0xFFF;
        }

        public void Parse(IReader reader, IHandler handler)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (handler == null) handler = new HandlerAdapter();

            var attrList = new AttrListImpl();
            string lastAttrName = null;
            var tagStack = new Stack();
            string elementName = null;
            line = 1;
            col = 0;
            int currCh = 0;
            int stateCode = 0;
            var sbChars = new StringBuilder();
            bool seenCData = false;
            bool isComment = false;
            bool isDTD = false;
            int bracketSwitch = 0;

            handler.OnStartParsing(this);

            while (true)
            {
                ++col;
                int prevCh = currCh;

                currCh = reader.Read();

                if (currCh == -1)
                {
                    if (stateCode != 0)
                    {
                        FatalErr("Unexpected EOF");
                    }
                    break;
                }

                int charCode = "<>/?=&'\"![ ]\t\r\n".IndexOf((char) currCh) & 0xF;
                if (charCode == (int) CharKind.CR) continue; // ignore
                // whitepace ::= (#x20 | #x9 | #xd | #xa)+
                if (charCode == (int) CharKind.TAB) charCode = (int) CharKind.SPACE; // tab == space
                if (charCode == (int) CharKind.EOL)
                {
                    col = 0;
                    line++;
                    charCode = (int) CharKind.SPACE;
                }

                int actionCode = Xlat(charCode, stateCode);
                stateCode = actionCode & 0xFF;
                // Ignore newline inside attribute value.
                if (currCh == '\n' && (stateCode == 0xE || stateCode == 0xF)) continue;
                actionCode >>= 8;

                if (stateCode >= 0x80)
                {
                    if (stateCode == 0xFF)
                    {
                        FatalErr("State dispatch error.");
                    }
                    else
                    {
                        FatalErr(errors[stateCode ^ 0x80]);
                    }
                }

                switch (actionCode)
                {
                    case (int) ActionCode.START_ELEM:
                        handler.OnStartElement(elementName, attrList);
                        if (currCh != '/')
                        {
                            tagStack.Push(elementName);
                        }
                        else
                        {
                            handler.OnEndElement(elementName);
                        }
                        attrList.Clear();
                        break;

                    case (int) ActionCode.END_ELEM:
                        elementName = sbChars.ToString();
                        sbChars = new StringBuilder();
                        string endName = null;
                        if (tagStack.Count == 0 ||
                            elementName != (endName = tagStack.Pop() as string))
                        {
                            if (endName == null)
                            {
                                FatalErr("Tag stack underflow");
                            }
                            else
                            {
                                FatalErr(String.Format("Expected end tag '{0}' but found '{1}'", elementName, endName));
                            }
                        }
                        handler.OnEndElement(elementName);
                        break;

                    case (int) ActionCode.END_NAME:
                        elementName = sbChars.ToString();
                        sbChars = new StringBuilder();
                        if (currCh != '/' && currCh != '>') break;
                        goto case (int) ActionCode.START_ELEM;

                    case (int) ActionCode.SET_ATTR_NAME:
                        lastAttrName = sbChars.ToString();
                        sbChars = new StringBuilder();
                        break;

                    case (int) ActionCode.SET_ATTR_VAL:
                        if (lastAttrName == null) FatalErr("Internal error.");
                        attrList.Add(lastAttrName, sbChars.ToString());
                        sbChars = new StringBuilder();
                        lastAttrName = null;
                        break;

                    case (int) ActionCode.SEND_CHARS:
                        handler.OnChars(sbChars.ToString());
                        sbChars = new StringBuilder();
                        break;

                    case (int) ActionCode.START_CDATA:
                        string cdata = "CDATA[";
                        isComment = false;
                        isDTD = false;

                        if (currCh == '-')
                        {
                            currCh = reader.Read();

                            if (currCh != '-') FatalErr("Invalid comment");

                            col++;
                            isComment = true;
                            twoCharBuff[0] = -1;
                            twoCharBuff[1] = -1;
                        }
                        else
                        {
                            if (currCh != '[')
                            {
                                isDTD = true;
                                bracketSwitch = 0;
                                break;
                            }

                            for (int i = 0; i < cdata.Length; i++)
                            {
                                if (reader.Read() != cdata[i])
                                {
                                    col += i + 1;
                                    break;
                                }
                            }
                            col += cdata.Length;
                            seenCData = true;
                        }
                        break;

                    case (int) ActionCode.END_CDATA:
                        int n = 0;
                        currCh = ']';

                        while (currCh == ']')
                        {
                            currCh = reader.Read();
                            n++;
                        }

                        if (currCh != '>')
                        {
                            for (int i = 0; i < n; i++) sbChars.Append(']');
                            sbChars.Append((char) currCh);
                            stateCode = 0x12;
                        }
                        else
                        {
                            for (int i = 0; i < n - 2; i++) sbChars.Append(']');
                            seenCData = false;
                        }

                        col += n;
                        break;

                    case (int) ActionCode.ERROR:
                        FatalErr(String.Format("Error {0}", stateCode));
                        break;

                    case (int) ActionCode.STATE_CHANGE:
                        break;

                    case (int) ActionCode.FLUSH_CHARS_STATE_CHANGE:
                        sbChars = new StringBuilder();
                        if (currCh != '<') goto case (int) ActionCode.ACC_CHARS_STATE_CHANGE;
                        break;

                    case (int) ActionCode.ACC_CHARS_STATE_CHANGE:
                        sbChars.Append((char) currCh);
                        break;

                    case (int) ActionCode.ACC_CDATA:
                        if (isComment)
                        {
                            if (currCh == '>'
                                && twoCharBuff[0] == '-'
                                && twoCharBuff[1] == '-')
                            {
                                isComment = false;
                                stateCode = 0;
                            }
                            else
                            {
                                twoCharBuff[0] = twoCharBuff[1];
                                twoCharBuff[1] = currCh;
                            }
                        }
                        else if (isDTD)
                        {
                            if (currCh == '<' || currCh == '>') bracketSwitch ^= 1;
                            if (currCh == '>' && bracketSwitch != 0)
                            {
                                isDTD = false;
                                stateCode = 0;
                            }
                        }
                        else
                        {
                            if (splitCData
                                && sbChars.Length > 0
                                && seenCData)
                            {
                                handler.OnChars(sbChars.ToString());
                                sbChars = new StringBuilder();
                            }
                            seenCData = false;
                            sbChars.Append((char) currCh);
                        }
                        break;

                    case (int) ActionCode.PROC_CHAR_REF:
                        currCh = reader.Read();
                        int cl = col + 1;
                        if (currCh == '#')
                        {
                            // character reference
                            int r = 10;
                            int chCode = 0;
                            int nDigits = 0;
                            currCh = reader.Read();
                            cl++;

                            if (currCh == 'x')
                            {
                                currCh = reader.Read();
                                cl++;
                                r = 16;
                            }

                            NumberStyles style = r == 16 ? NumberStyles.HexNumber : NumberStyles.Integer;

                            while (true)
                            {
                                int x = -1;
                                if (Char.IsNumber((char) currCh) || "abcdef".IndexOf(Char.ToLower((char) currCh)) != -1)
                                {
                                    try
                                    {
                                        x = Int32.Parse(new string((char) currCh, 1), style);
                                    }
                                    catch (FormatException)
                                    {
                                        x = -1;
                                    }
                                }
                                if (x == -1) break;
                                chCode *= r;
                                chCode += x;
                                nDigits++;
                                currCh = reader.Read();
                                cl++;
                            }

                            if (currCh == ';' && nDigits > 0)
                            {
                                sbChars.Append((char) chCode);
                            }
                            else
                            {
                                FatalErr("Bad char ref");
                            }
                        }
                        else
                        {
                            // entity reference
                            string entityRefChars = "aglmopqstu"; // amp | apos | quot | gt | lt
                            string entities = "&'\"><";

                            int pos = 0;
                            int entIdx = 0xF;
                            int pred = 0;
                            int predShift = 0;

                            int sbLen = sbChars.Length;

                            while (true)
                            {
                                if (pos != 0xF) pos = entityRefChars.IndexOf((char) currCh) & 0xF;
                                if (pos == 0xF) FatalErr(errors[7]);
                                sbChars.Append((char) currCh);

                                int path = "\uFF35\u3F8F\u4F8F\u0F5F\uFF78\uE1F4\u2299\uEEFF\uEEFF\uFF4F"[pos];
                                int lBr = (path >> 4) & 0xF;
                                int rBr = path & 0xF;
                                int lPred = path >> 12;
                                int rPred = (path >> 8) & 0xF;
                                currCh = reader.Read();
                                cl++;
                                pos = 0xF;
                                if (lBr != 0xF && currCh == entityRefChars[lBr])
                                {
                                    if (lPred < 0xE) entIdx = lPred;
                                    pred = lPred;
                                    predShift = 12; // left
                                }
                                else if (rBr != 0xF && currCh == entityRefChars[rBr])
                                {
                                    if (rPred < 0xE) entIdx = rPred;
                                    pred = rPred;
                                    predShift = 8; // right
                                }
                                else if (currCh == ';')
                                {
                                    if (entIdx != 0xF
                                        && predShift != 0
                                        && ((path >> predShift) & 0xF) == 0xE) break;
                                    continue; // pos == 0xF
                                }

                                pos = 0;
                            }

                            int l = cl - col - 1;

                            if ((l > 0 && l < 5)
                                && (StrEquals("amp", sbChars, sbLen, l)
                                    || StrEquals("apos", sbChars, sbLen, l)
                                    || StrEquals("quot", sbChars, sbLen, l)
                                    || StrEquals("lt", sbChars, sbLen, l)
                                    || StrEquals("gt", sbChars, sbLen, l))
                                )
                            {
                                sbChars.Length = sbLen;
                                sbChars.Append(entities[entIdx]);
                            }
                            else FatalErr(errors[7]);
                        }

                        col = cl;
                        break;

                    default:
                        FatalErr(String.Format("Unexpected action code - {0}.", actionCode));
                        break;
                }
            } // while (true)

            handler.OnEndParsing(this);
        }

        private enum ActionCode : byte
        {
            START_ELEM = 0,
            END_ELEM = 1,
            END_NAME = 2,
            SET_ATTR_NAME = 3,
            SET_ATTR_VAL = 4,
            SEND_CHARS = 5,
            START_CDATA = 6,
            END_CDATA = 7,
            ERROR = 8,
            STATE_CHANGE = 9,
            FLUSH_CHARS_STATE_CHANGE = 0xA,
            ACC_CHARS_STATE_CHANGE = 0xB,
            ACC_CDATA = 0xC,
            PROC_CHAR_REF = 0xD,
            UNKNOWN = 0xF
        }

        public class AttrListImpl : IMutableAttrList
        {
            protected ArrayList names;
            protected ArrayList values;

            public AttrListImpl() : this(0)
            {
            }

            public AttrListImpl(int initialCapacity)
            {
                if (initialCapacity <= 0)
                {
                    names = new ArrayList();
                    values = new ArrayList();
                }
                else
                {
                    names = new ArrayList(initialCapacity);
                    values = new ArrayList(initialCapacity);
                }
            }

            public AttrListImpl(IAttrList attrs)
                : this(attrs != null ? attrs.Length : 0)
            {
                if (attrs != null) CopyFrom(attrs);
            }

            public int Length
            {
                get { return names.Count; }
            }

            public bool IsEmpty
            {
                get { return Length != 0; }
            }

            public string GetName(int i)
            {
                string res = null;
                if (i >= 0 && i < Length)
                {
                    res = names[i] as string;
                }
                return res;
            }

            public string GetValue(int i)
            {
                string res = null;
                if (i >= 0 && i < Length)
                {
                    res = values[i] as string;
                }
                return res;
            }

            public string GetValue(string name)
            {
                return GetValue(names.IndexOf(name));
            }

            public void ChangeValue(string name, string newValue)
            {
                int i = names.IndexOf(name);
                if (i >= 0 && i < Length)
                {
                    values[i] = newValue;
                }
            }

            public string[] Names
            {
                get { return names.ToArray(typeof (string)) as string[]; }
            }

            public string[] Values
            {
                get { return values.ToArray(typeof (string)) as string[]; }
            }

            public void Clear()
            {
                names.Clear();
                values.Clear();
            }

            public void Add(string name, string value)
            {
                names.Add(name);
                values.Add(value);
            }

            public void Remove(int i)
            {
                if (i >= 0)
                {
                    names.RemoveAt(i);
                    values.RemoveAt(i);
                }
            }

            public void Remove(string name)
            {
                Remove(names.IndexOf(name));
            }

            public void CopyFrom(IAttrList attrs)
            {
                if (attrs != null && (this == attrs))
                {
                    Clear();
                    int n = attrs.Length;
                    for (int i = 0; i < n; i++)
                    {
                        Add(attrs.GetName(i), attrs.GetValue(i));
                    }
                }
            }
        }

        private enum CharKind : byte
        {
            LEFT_BR = 0,
            RIGHT_BR = 1,
            SLASH = 2,
            PI_MARK = 3,
            EQ = 4,
            AMP = 5,
            SQUOTE = 6,
            DQUOTE = 7,
            BANG = 8,
            LEFT_SQBR = 9,
            SPACE = 0xA,
            RIGHT_SQBR = 0xB,
            TAB = 0xC,
            CR = 0xD,
            EOL = 0xE,
            CHARS = 0xF,
            UNKNOWN = 0x1F
        }

        public class HandlerAdapter : IHandler
        {
            public void OnStartParsing(MiniParser parser)
            {
            }

            public void OnStartElement(string name, IAttrList attrs)
            {
            }

            public void OnEndElement(string name)
            {
            }

            public void OnChars(string ch)
            {
            }

            public void OnEndParsing(MiniParser parser)
            {
            }
        }

        public interface IAttrList
        {
            int Length { get; }
            bool IsEmpty { get; }
            string[] Names { get; }
            string[] Values { get; }
            string GetName(int i);
            string GetValue(int i);
            string GetValue(string name);
            void ChangeValue(string name, string newValue);
        }

        public interface IHandler
        {
            void OnStartParsing(MiniParser parser);
            void OnStartElement(string name, IAttrList attrs);
            void OnEndElement(string name);
            void OnChars(string ch);
            void OnEndParsing(MiniParser parser);
        }

        public interface IMutableAttrList : IAttrList
        {
            void Clear();
            void Add(string name, string value);
            void CopyFrom(IAttrList attrs);
            void Remove(int i);
            void Remove(string name);
        }

        public interface IReader
        {
            int Read();
        }

        public class XMLError : Exception
        {
            protected int column;
            protected string descr;
            protected int line;

            public XMLError() : this("Unknown")
            {
            }

            public XMLError(string descr) : this(descr, -1, -1)
            {
            }

            public XMLError(string descr, int line, int column)
                : base(descr)
            {
                this.descr = descr;
                this.line = line;
                this.column = column;
            }

            public int Line
            {
                get { return line; }
            }

            public int Column
            {
                get { return column; }
            }

            public override string ToString()
            {
                return (String.Format("{0} @ (line = {1}, col = {2})", descr, line, column));
            }
        }

        // Parse
    }
}