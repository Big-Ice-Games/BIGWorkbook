#region license

// Copyright (c) 2025, Big Ice Games
// All rights reserved.

#endregion

using System;

namespace BIGWorkbook.Runtime
{
    public class ClassMappingAttribute : Attribute
    {
        /// <summary>
        /// Value indicating whether this class should be mapped automatic.
        /// If not you should use <see cref="FieldMappingAttribute"/> above each field that you want to map from excel file.
        /// </summary>
        public bool Automapping { get; }
        public Type ScriptableType { get; }
        public string WorkbookPath { get; }
        public string SheetName { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="automapping"> Value indicating whether you should try to map properties by name.
        /// If false each field you want to map should be decorated by <see cref="FieldMappingAttribute"/>.</param>
        public ClassMappingAttribute(bool automapping, Type scriptableType, string workbookPath, string sheetName = "")
        {
            Automapping = automapping;
            ScriptableType = scriptableType;
            WorkbookPath = workbookPath;
            SheetName = sheetName;
        }

        private ClassMappingAttribute()
        {
        }
    }
}