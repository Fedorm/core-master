// Copyright 2010 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Globalization;
using BitMobile.SyncLibrary.Formatters;

namespace Microsoft.Synchronization.Services.Formatters
{
    class BMEntryInfoWrapper : EntryInfoWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader"></param>
        public BMEntryInfoWrapper(XElement reader)
            : base(reader)
        { }

        /// <summary>
        /// Looks for a sync:syncConflict or an sync:syncError element
        /// </summary>
        /// <param name="entry">entry element</param>
        protected override void LoadConflictEntry(XElement entry)
        {
            XElement conflictElement = entry.Element(FormatterConstants.SyncNamespace + FormatterConstants.SyncConlflictElementName);
            if (conflictElement != null)
            {
                // Its an conflict
                this.IsConflict = true;

                // Make sure it has an sync:conflictResolution element
                XElement resolutionType = conflictElement.Element(FormatterConstants.SyncNamespace + FormatterConstants.ConflictResolutionElementName);
                if (resolutionType == null)
                {
                    throw new InvalidOperationException("Conflict resolution not specified for entry element " + this.TypeName);
                }
                this.ConflictDesc = resolutionType.Value;

                XElement conflictingChangeElement = conflictElement.Element(FormatterConstants.SyncNamespace + FormatterConstants.ConflictEntryElementName);
                if (conflictingChangeElement == null)
                {
                    throw new InvalidOperationException("conflictingChange not specified for syncConflict element " + this.TypeName);
                }

                this.ConflictWrapper = new BMEntryInfoWrapper(GetSubElement(conflictingChangeElement));
                return;
            }

            // Look for an errorElement element
            XElement errorElement = entry.Element(FormatterConstants.SyncNamespace + FormatterConstants.SyncErrorElementName);
            if (errorElement != null)
            {
                // Its not an conflict
                this.IsConflict = false;

                // Make sure it has an sync:errorDescription element
                XElement errorDesc = errorElement.Element(FormatterConstants.SyncNamespace + FormatterConstants.ErrorDescriptionElementName);
                if (errorDesc != null)
                {
                    this.ConflictDesc = errorDesc.Value;
                }

                XElement errorChangeElement = errorElement.Element(FormatterConstants.SyncNamespace + FormatterConstants.ErrorEntryElementName);
                if (errorChangeElement == null)
                {
                    throw new InvalidOperationException("errorInChange not specified for syncError element " + this.TypeName);
                }

                this.ConflictWrapper = new BMEntryInfoWrapper(GetSubElement(errorChangeElement));
            }
        }

        /// <summary>
        /// Looks for either a &lt;entry/&gt; or a &lt;deleted-entry/&gt; subelement within the outer element.
        /// </summary>
        /// <param name="entryElement">The outer entry element</param>
        /// <returns>The inner entry or the deleted-entry subelement</returns>
        private XElement GetSubElement(XElement entryElement)
        {
            return entryElement.Element(BMFormatterConstants.Entry) ?? entryElement.Element(BMFormatterConstants.Tombstone);
        }

        /// <summary>
        /// Inspects all m.properties element in the entry element to load all properties.
        /// </summary>
        /// <param name="entry">Entry element</param>
        protected override void LoadEntryProperties(XElement entry)
        {
            if (entry.Name.LocalName.Equals(BMFormatterConstants.Tombstone))
            {
                // Read the tombstone
                IsTombstone = true;
                Values.Add(entry.Value);
                if (string.IsNullOrEmpty(entry.Value))
                {
                    // No atom:id element was found in the tombstone. Throw.
                    throw new InvalidOperationException("A atom:ref element must be present for a tombstone entry. Entity in error: " + entry.ToString(SaveOptions.None));
                }
            }
            else
            {
                foreach (XElement property in entry.Elements())
                {
                    Values.Add(property.Name.LocalName == BMFormatterConstants.Property ? property.Value : null);
                }
            }
        }

        /// <summary>
        /// Looks for the category element in the entry for the type name
        /// </summary>
        /// <param name="entry">Entry element</param>
        protected override void LoadTypeName(XElement entry)
        {
            var attribute = entry.Attribute(BMFormatterConstants.Caption);
            if (attribute != null)
                TypeName = attribute.Value;
            else
                throw new InvalidOperationException("Entry doesn't have caption");

            if (attribute == null || string.IsNullOrEmpty(TypeName))
                throw new InvalidOperationException("TypeName is null or empty");
        }
    }
}
