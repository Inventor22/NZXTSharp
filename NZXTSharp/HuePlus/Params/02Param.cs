/*
02Param.cs
Copyright (C) 2019  Ari Madian

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NZXTSharp.HuePlus
{

    /// <summary>
    /// Represents an 02 effect Param
    /// </summary>
    internal class _02Param : IParam {
        private readonly int _Value = 0x02;

        /// <inheritdoc/>
        public int Value { get => GetValue(); }

        /// <inheritdoc/>
        public int GetValue() {
            return 0x02;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        public static implicit operator byte(_02Param param) {
            return (byte)0x02;
        }
    }
}
