﻿//******************************************************************************************************
//  SortedTreeTypeBase.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/1/2013 - Steven E. Chisholm
//       Generated original version of source code. 
//     
//******************************************************************************************************

using System;
using System.Collections;
using GSF.IO;
using GSF.IO.Unmanaged;

namespace GSF.SortedTreeStore.Tree
{
    /// <summary>
    /// The interface that is required to use as a value in <see cref="SortedTree"/> 
    /// </summary>
    public abstract class SortedTreeTypeBase
    {
        /// <summary>
        /// The Guid uniquely defining this type. 
        /// It is important to uniquely tie 1 type to 1 guid.
        /// </summary>
        public abstract Guid GenericTypeGuid { get; }

        /// <summary>
        /// Gets the size of this class when serialized
        /// </summary>
        /// <returns></returns>
        public abstract int Size { get; }

        /// <summary>
        /// Sets the provided key to it's minimum value
        /// </summary>
        public abstract void SetMin();

        /// <summary>
        /// Sets the privided key to it's maximum value
        /// </summary>
        public abstract void SetMax();

        /// <summary>
        /// Clears the key
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Reads the provided key from the stream.
        /// </summary>
        /// <param name="stream"></param>
        public abstract void Read(BinaryStreamBase stream);

        /// <summary>
        /// Writes the provided data to the BinaryWriter
        /// </summary>
        /// <param name="stream"></param>
        public abstract void Write(BinaryStreamBase stream);

        /// <summary>
        /// Reads the key from the stream
        /// </summary>
        /// <param name="stream"></param>
        public virtual unsafe void Read(byte* stream)
        {
            var reader = new BinaryStreamPointerWrapper(stream, Size);
            Read(reader);
        }

        /// <summary>
        /// Writes the key to the stream
        /// </summary>
        /// <param name="stream"></param>
        public virtual unsafe void Write(byte* stream)
        {
            var writer = new BinaryStreamPointerWrapper(stream, Size);
            Write(writer);
        }

        /// <summary>
        /// Gets all available encoding methods for a specific type. May return null if none exists.
        /// </summary>
        /// <returns>null or an IEnumerable of all encoding methods.</returns>
        public virtual IEnumerable GetEncodingMethods()
        {
            return null;
        }

        /// <summary>
        /// Executes a copy command without modifying the current class.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public virtual unsafe void MethodCopy(byte* source, byte* destination)
        {
            Memory.Copy(source, destination, Size);
        }

    }
}
