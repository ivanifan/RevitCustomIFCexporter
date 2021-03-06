﻿//
// BIM IFC library: this library works with Autodesk(R) Revit(R) to export IFC files containing model geometry.
// Copyright (C) 2012  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// Provides static methods to create varies IFC properties.
    /// </summary>
    public class PropertyUtil
    {
        private static void ValidateEnumeratedValue(string value, Type propertyEnumerationType)
        {
            if (propertyEnumerationType != null && propertyEnumerationType.IsEnum)
            {
                foreach (object enumeratedValue in Enum.GetValues(propertyEnumerationType))
                {
                    string enumValue = enumeratedValue.ToString();
                    if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, enumValue))
                    {
                        value = enumValue;
                        return;
                    }
                }
                value = null;
            }
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelProperty(IFCFile file, string propertyName, string value, PropertyValueType valueType,
            Type propertyEnumerationType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        ValidateEnumeratedValue(value, propertyEnumerationType);
                        valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsLabel(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a text property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateTextProperty(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsText(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsText(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a text property, using the cached value if possible.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateTextPropertyFromCache(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            bool canCache = (value == String.Empty);
            StringPropertyInfoCache stringInfoCache = null;
            IFCAnyHandle textHandle = null;

            if (canCache)
            {
                stringInfoCache = ExporterCacheManager.PropertyInfoCache.TextCache;
                textHandle = stringInfoCache.Find(propertyName, value);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(textHandle))
                    return textHandle;
            }

            textHandle = CreateTextProperty(file, propertyName, value, valueType);

            if (canCache)
                stringInfoCache.Add(propertyName, value, textHandle);

            return textHandle;
        }

        /// <summary>
        /// Create a text property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateTextPropertyFromElement(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName,
            PropertyValueType valueType, Type propertyEnumerationType)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateTextPropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create a text property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateTextPropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName,
           BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType, Type propertyEnumerationType)
        {
            // For Instance
            IFCAnyHandle propHnd = CreateTextPropertyFromElement(file, elem, revitParameterName, ifcPropertyName, valueType,
                propertyEnumerationType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateTextPropertyFromElement(file, elem, builtInParamName, ifcPropertyName, valueType, propertyEnumerationType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateTextPropertyFromElementOrSymbol(file, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType,
                    propertyEnumerationType);
            else
                return null;
        }

        /// <summary>
        /// Create a label property, using the cached value if possible.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="cacheAllStrings">Whether to cache all strings (true), or only the empty string (false).</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelPropertyFromCache(IFCFile file, string propertyName, string value, PropertyValueType valueType,
            bool cacheAllStrings, Type propertyEnumerationType)
        {
            bool canCache = (value == String.Empty) || cacheAllStrings;
            StringPropertyInfoCache stringInfoCache = null;
            IFCAnyHandle labelHandle = null;

            if (canCache)
            {
                stringInfoCache = ExporterCacheManager.PropertyInfoCache.LabelCache;
                labelHandle = stringInfoCache.Find(propertyName, value);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(labelHandle))
                    return labelHandle;
            }

            labelHandle = CreateLabelProperty(file, propertyName, value, valueType, propertyEnumerationType);

            if (canCache)
                stringInfoCache.Add(propertyName, value, labelHandle);

            return labelHandle;
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="values">The values of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelProperty(IFCFile file, string propertyName, IList<string> values, PropertyValueType valueType,
            Type propertyEnumerationType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        foreach (string value in values)
                        {
                            valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        }
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.ListValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        foreach (string value in values)
                        {
                            valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        }
                        return IFCInstanceExporter.CreatePropertyListValue(file, propertyName, null, valueList, null);
                    }
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an identifier property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierProperty(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsIdentifier(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    {
                        return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsIdentifier(value), null);
                    }
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an identifier property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromCache(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            StringPropertyInfoCache stringInfoCache = ExporterCacheManager.PropertyInfoCache.IdentifierCache;
            IFCAnyHandle stringHandle = stringInfoCache.Find(propertyName, value);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(stringHandle))
                return stringHandle;

            stringHandle = CreateIdentifierProperty(file, propertyName, value, valueType);

            stringInfoCache.Add(propertyName, value, stringHandle);
            return stringHandle;
        }

        /// <summary>
        /// Create a boolean property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateBooleanProperty(IFCFile file, string propertyName, bool value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsBoolean(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsBoolean(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a logical property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLogicalProperty(IFCFile file, string propertyName, IFCLogical value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsLogical(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsLogical(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a boolean property or gets one from cache.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateBooleanPropertyFromCache(IFCFile file, string propertyName, bool value, PropertyValueType valueType)
        {
            BooleanPropertyInfoCache boolInfoCache = ExporterCacheManager.PropertyInfoCache.BooleanCache;
            IFCAnyHandle boolHandle = boolInfoCache.Find(propertyName, value);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boolHandle))
                return boolHandle;

            boolHandle = CreateBooleanProperty(file, propertyName, value, valueType);
            boolInfoCache.Add(propertyName, value, boolHandle);
            return boolHandle;
        }

        /// <summary>
        /// Create a logical property or gets one from cache.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLogicalPropertyFromCache(IFCFile file, string propertyName, IFCLogical value, PropertyValueType valueType)
        {
            LogicalPropertyInfoCache logicalInfoCache = ExporterCacheManager.PropertyInfoCache.LogicalCache;
            IFCAnyHandle logicalHandle = logicalInfoCache.Find(propertyName, value);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(logicalHandle))
                return logicalHandle;

            logicalHandle = CreateLogicalProperty(file, propertyName, value, valueType);
            logicalInfoCache.Add(propertyName, value, logicalHandle);
            return logicalHandle;
        }

        /// <summary>
        /// Create an integer property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIntegerProperty(IFCFile file, string propertyName, int value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsInteger(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsInteger(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an integer property or gets one from cache.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateIntegerPropertyFromCache(IFCFile file, string propertyName, int value, PropertyValueType valueType)
        {
            bool canCache = (value >= -10 && value <= 10);
            IFCAnyHandle intHandle = null;
            IntegerPropertyInfoCache intInfoCache = null;
            if (canCache)
            {
                intInfoCache = ExporterCacheManager.PropertyInfoCache.IntegerCache;
                intHandle = intInfoCache.Find(propertyName, value);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(intHandle))
                    return intHandle;
            }

            intHandle = CreateIntegerProperty(file, propertyName, value, valueType);
            if (canCache)
            {
                intInfoCache.Add(propertyName, value, intHandle);
            }
            return intHandle;
        }

        internal static double? CanCacheDouble(double scale, double value)
        {
            // We have a partial cache here.
            // For scale = 1.0 (feet), cache multiples of 1/2" up to 10'.
            // For scale = 0.03048 (meter), cache multiples of 50mm up to 10m.
            // For scale = 304.8 (mm), cache multiples of 50mm up to 10m.

            if (MathUtil.IsAlmostZero(value))
            {
                return 0.0;
            }
            else
            {
                // approximate tests for most common scales are good enough here.
                if (MathUtil.IsAlmostEqual(scale, 1.0) || MathUtil.IsAlmostEqual(scale, 12.0))
                {
                    double multiplier = 24 / scale;
                    double lengthInHalfInches = Math.Floor(value * multiplier + 0.5);
                    if (lengthInHalfInches > 0 && lengthInHalfInches <= 240 && MathUtil.IsAlmostZero(value * multiplier - lengthInHalfInches))
                    {
                        return lengthInHalfInches / multiplier;
                    }
                }
                else
                {
                    double multiplier = (304.8 / scale) / 50;
                    double lengthIn50mm = Math.Floor(value * multiplier + 0.5);
                    if (lengthIn50mm > 0 && lengthIn50mm <= 200 && MathUtil.IsAlmostZero(value * multiplier - lengthIn50mm))
                    {
                        return lengthIn50mm / multiplier;
                    }
                }
            }
            return null;
        }

        internal static double? CanCachePower(double value)
        {
            // Allow caching of values between 0 and 300, in multiples of 5
            double eps = MathUtil.Eps();
            if (value < -eps || value > 300.0 + eps)
                return null;
            if (MathUtil.IsAlmostZero(value % 5.0))
                return Math.Truncate(value + 0.5);
            return null;
        }

        internal static double? CanCacheTemperature(double value)
        {
            // Allow caching of integral temperatures and half-degrees.
            if (MathUtil.IsAlmostEqual(value * 2.0, Math.Truncate(value * 2.0)))
                return Math.Truncate(value * 2.0)/2.0;
            return null;
        }

        internal static double? CanCacheThermalTransmittance(double value)
        {
            // Allow caching of values between 0 and 6.0, in multiples of 0.05
            double eps = MathUtil.Eps();
            if (value < -eps || value > 6.0 + eps)
                return null;
            if (MathUtil.IsAlmostEqual(value * 20.0, Math.Truncate(value * 20.0)))
                return Math.Truncate(value * 20.0) / 20.0;
            return null;
        }

        /// <summary>
        /// Create a real property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateRealProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsReal(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsReal(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a real property, using a cached value if possible.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created or cached property handle.</returns>
        public static IFCAnyHandle CreateRealPropertyFromCache(IFCFile file, double scale, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheDouble(scale, value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
            {
                value = adjustedValue.GetValueOrDefault();
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.RealCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreateRealProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.PropertyInfoCache.RealCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>Create a Thermodyanamic Temperature property, using a cached value if possible.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created or cached property handle.</returns>
        public static IFCAnyHandle CreateThermodynamicTemperaturePropertyFromCache(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheTemperature(value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
                value = adjustedValue.GetValueOrDefault();

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.ThermodynamicTemperatureCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreateThermodynamicTemperatureProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
                ExporterCacheManager.PropertyInfoCache.ThermodynamicTemperatureCache.Add(propertyName, value, propertyHandle);
            
            return propertyHandle;
        }

        /// <summary>Create a Power measure property, using a cached value if possible.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created or cached property handle.</returns>
        public static IFCAnyHandle CreatePowerPropertyFromCache(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCachePower(value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
                value = adjustedValue.GetValueOrDefault();

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.PowerCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreatePowerProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
                ExporterCacheManager.PropertyInfoCache.PowerCache.Add(propertyName, value, propertyHandle);

            return propertyHandle;
        }

        /// <summary>Create a Thermal Transmittance property, using a cached value if possible.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created or cached property handle.</returns>
        public static IFCAnyHandle CreateThermalTransmittancePropertyFromCache(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheThermalTransmittance(value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
                value = adjustedValue.GetValueOrDefault();

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.ThermalTransmittanceCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreateThermalTransmittanceProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
                ExporterCacheManager.PropertyInfoCache.ThermalTransmittanceCache.Add(propertyName, value, propertyHandle);

            return propertyHandle;
        }

        /// <summary>
        /// Creates a length measure property or gets one from cache.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLengthMeasurePropertyFromCache(IFCFile file, double scale, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheDouble(scale, value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
            {
                value = adjustedValue.GetValueOrDefault();
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.LengthMeasureCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsLengthMeasure(value));
                        propertyHandle = IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                        break;
                    }
                case PropertyValueType.SingleValue:
                    propertyHandle = IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsLengthMeasure(value), null);
                    break;
                default:
                    throw new InvalidOperationException("Missing case!");
            }

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.PropertyInfoCache.LengthMeasureCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>
        /// Creates a volume measure property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateVolumeMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsVolumeMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsVolumeMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a positive length measure property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePositiveLengthMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            if (value > MathUtil.Eps())
            {
                switch (valueType)
                {
                    case PropertyValueType.EnumeratedValue:
                        {
                            IList<IFCData> valueList = new List<IFCData>();
                            valueList.Add(IFCDataUtil.CreateAsPositiveLengthMeasure(value));
                            return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                        }
                    case PropertyValueType.SingleValue:
                        return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPositiveLengthMeasure(value), null);
                    default:
                        throw new InvalidOperationException("Missing case!");
                }
            }
            return null;
        }

        private static IFCAnyHandle CreateRatioMeasurePropertyCommon(IFCFile file, string propertyName, double value, PropertyValueType valueType,
            bool positiveOnly)
        {
            if (positiveOnly && (value <= MathUtil.Eps()))
                return null;

            IFCData ratioData = positiveOnly ? IFCDataUtil.CreateAsPositiveRatioMeasure(value) : IFCDataUtil.CreateAsRatioMeasure(value);
            
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(ratioData);
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, ratioData, null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a ratio measure property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateRatioMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            return CreateRatioMeasurePropertyCommon(file, propertyName, value, valueType, false);
        }
        
        /// <summary>
        /// Create a positive ratio measure property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePositiveRatioMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            return CreateRatioMeasurePropertyCommon(file, propertyName, value, valueType, true);
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsPlaneAngleMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPlaneAngleMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a label property, or retrieve from cache.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created or cached property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasurePropertyFromCache(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            // We have a partial cache here - we will only cache multiples of 15 degrees.
            bool canCache = false;
            double degreesDiv15 = Math.Floor(value / 15.0 + 0.5);
            double integerDegrees = degreesDiv15 * 15.0;
            if (MathUtil.IsAlmostEqual(value, integerDegrees))
            {
                canCache = true;
                value = integerDegrees;
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.PropertyInfoCache.PlaneAngleCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreatePlaneAngleMeasureProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.PropertyInfoCache.PlaneAngleCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>
        /// Create a area measure property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsAreaMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsAreaMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a count measure property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateCountMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsCountMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsCountMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a ThermodynamicTemperature property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermodynamicTemperatureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsThermodynamicTemperatureMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsThermodynamicTemperatureMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a ClassificationReference property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateClassificationReferenceProperty(IFCFile file, string propertyName, string value)
        {
            IFCAnyHandle classificationReferenceHandle = IFCInstanceExporter.CreateClassificationReference(file, null, value, null, null);
            return IFCInstanceExporter.CreatePropertyReferenceValue(file, propertyName, null, null, classificationReferenceHandle);
        }

        /// <summary>Create a PowerMeasure property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePowerProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsPowerMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPowerMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a ThermalTransmittance property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermalTransmittanceProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsThermalTransmittanceMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsThermalTransmittanceMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>Create a VolumetricFlowRate property.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateVolumetricFlowRateMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsVolumetricFlowRateMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsVolumetricFlowRateMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a VolumetricFlowRate measure property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateVolumetricFlowRatePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
                return CreateVolumetricFlowRateMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            return null;
        }

        /// <summary>
        /// Create a ThermodynamicTemperature measure property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermodynamicTemperaturePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
                return CreateThermodynamicTemperaturePropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            return null;
        }

        /// <summary>
        /// Create a ThermodynamicTemperature measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermodynamicTemperaturePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateThermodynamicTemperaturePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateThermodynamicTemperaturePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateThermodynamicTemperaturePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a VolumetricFlowRate measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateVolumetricFlowRatePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateVolumetricFlowRatePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateVolumetricFlowRatePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateVolumetricFlowRatePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create an IfcClassificationReference property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateClassificationReferencePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateClassificationReferenceProperty(file, ifcPropertyName, propertyValue);
            }
            return null;
        }
        
        /// <summary>
        /// Create a Power measure property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePowerPropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                double scaledpropertyValue = propertyValue * (1 / 0.3048) * (1 / 0.3048);
                return CreatePowerPropertyFromCache(file, ifcPropertyName, scaledpropertyValue, valueType);
            }
            return null;
        }
        
        /// <summary>
        /// Create a ThermalTransmittance measure property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermalTransmittancePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                // TODO: scale!
                return CreateThermalTransmittancePropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create an IfcClassificationReference property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateClassificationReferencePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName)
        {
            IFCAnyHandle propHnd = CreateClassificationReferencePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateClassificationReferencePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateClassificationReferencePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName);
            else
                return null;
        }

        /// <summary>
        /// Create a Power measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePowerPropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreatePowerPropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreatePowerPropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePowerPropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a ThermalTransmittance measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateThermalTransmittancePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateThermalTransmittancePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateThermalTransmittancePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateThermalTransmittancePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }
        
        /// <summary>
        /// Create a label property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelPropertyFromElement(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName,
            PropertyValueType valueType, Type propertyEnumerationType)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateLabelPropertyFromCache(file, ifcPropertyName, propertyValue, valueType, false, propertyEnumerationType);
            }
            return null;
        }

        /// <summary>
        /// Create a label property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <param name="propertyEnumerationType">The type of the enum, null if valueType isn't EnumeratedValue.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelPropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName,
           BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType, Type propertyEnumerationType)
        {
            // For Instance
            IFCAnyHandle propHnd = CreateLabelPropertyFromElement(file, elem, revitParameterName, ifcPropertyName, valueType,
                propertyEnumerationType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateLabelPropertyFromElement(file, elem, builtInParamName, ifcPropertyName, valueType, propertyEnumerationType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateLabelPropertyFromElementOrSymbol(file, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType,
                    propertyEnumerationType);
            else
                return null;
        }

        /// <summary>
        /// Create an identifier property from the element's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromElement(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateIdentifierPropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create an identifier property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="revitBuiltInParam">
        /// The built in parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName,
           BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            // For Instance
            IFCAnyHandle propHnd = CreateIdentifierPropertyFromElement(file, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateIdentifierPropertyFromElement(file, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateIdentifierPropertyFromElementOrSymbol(file, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a boolean property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateBooleanPropertyFromElementOrSymbol(IFCFile file, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            int propertyValue;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateBooleanPropertyFromCache(file, ifcPropertyName, propertyValue != 0, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateBooleanPropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a logical property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLogicalPropertyFromElementOrSymbol(IFCFile file, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCLogical ifcLogical = IFCLogical.Unknown;
            int propertyValue;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
            {
                ifcLogical = propertyValue != 0 ? IFCLogical.True : IFCLogical.False;
            }
            else
            {
                // For Symbol
                Document document = elem.Document;
                ElementId typeId = elem.GetTypeId();
                Element elemType = document.GetElement(typeId);
                if (elemType != null)
                    return CreateLogicalPropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            }

            return CreateLogicalPropertyFromCache(file, ifcPropertyName, ifcLogical, valueType);
        }

        /// <summary>
        /// Create an integer property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIntegerPropertyFromElementOrSymbol(IFCFile file, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            int propertyValue;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateIntegerPropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateIntegerPropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>Create a real property from the element's or type's parameter.</summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="scale">The length scale.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateRealPropertyFromElementOrSymbol(IFCFile file, double scale, Element elem, string revitParameterName, string ifcPropertyName,
            PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateRealPropertyFromCache(file, scale, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateRealPropertyFromElementOrSymbol(file, scale, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a length property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="builtInParameterName">The name of the built-in parameter, can be null.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLengthMeasurePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string builtInParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            double scale = exporterIFC.LinearScale;

            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateLengthMeasurePropertyFromCache(file, scale, ifcPropertyName, propertyValue, valueType);
            }

            if (builtInParameterName != null && ParameterUtil.GetDoubleValueFromElement(elem, builtInParameterName, out propertyValue))
            {
                return CreateLengthMeasurePropertyFromCache(file, scale, ifcPropertyName, propertyValue, valueType);
            }

            return null;
        }
        
        /// <summary>
        /// Create a positive length property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="builtInParameterName">The name of the built-in parameter, can be null.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePositiveLengthMeasurePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string builtInParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue = propertyValue * exporterIFC.LinearScale;
                return CreatePositiveLengthMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }

            if (builtInParameterName != null && ParameterUtil.GetDoubleValueFromElement(elem, builtInParameterName, out propertyValue))
            {
                propertyValue = propertyValue * exporterIFC.LinearScale;
                return CreatePositiveLengthMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }

            return null;
        }

        /// <summary>
        /// Create a length property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The optional built-in parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLengthMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            string builtInParamName = null;
            if (revitBuiltInParam != BuiltInParameter.INVALID)
                builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);

            IFCAnyHandle propHnd = CreateLengthMeasurePropertyFromElement(file, exporterIFC, elem, revitParameterName, builtInParamName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateLengthMeasurePropertyFromElement(file, exporterIFC, elemType, revitParameterName, builtInParamName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a positive length property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The optional built-in parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePositiveLengthMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            string builtInParamName = null;
            if (revitBuiltInParam != BuiltInParameter.INVALID)
                builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                
            IFCAnyHandle propHnd = CreatePositiveLengthMeasurePropertyFromElement(file, exporterIFC, elem, revitParameterName, builtInParamName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePositiveLengthMeasurePropertyFromElement(file, exporterIFC, elemType, revitParameterName, builtInParamName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a ratio property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateRatioPropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
                return CreateRatioMeasureProperty(file, ifcPropertyName, propertyValue, valueType);

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType == null)
                return null;

            return CreateRatioPropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, ifcPropertyName, valueType);
        }
        
        /// <summary>
        /// Create a positive ratio property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreatePositiveRatioPropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue *= exporterIFC.LinearScale;
                return CreatePositiveRatioMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType == null)
                return null;

            return CreatePositiveRatioPropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, ifcPropertyName, valueType);
        }

        /// <summary>
        /// Create a plane angle measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasurePropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                // Although the default units for IFC files is radians, IFC files almost universally use degrees as their unit of measurement. 
                // However, many old IFC files failed to include degrees as the unit of measurement.
                // As such, we assume that the IFC file is in degrees, regardless of whether or not it is explicitly stated in the file.
                propertyValue *= 180 / Math.PI;
                return CreatePlaneAngleMeasurePropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePlaneAngleMeasurePropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create an area measure property from the element's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasurePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue *= (exporterIFC.LinearScale * exporterIFC.LinearScale);
                return CreateAreaMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create a count measure property from the element's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateCountMeasurePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            int propertyValue;
            double propertyValueReal;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
                return CreateCountMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValueReal))
                return CreateCountMeasureProperty(file, ifcPropertyName, propertyValueReal, valueType);
            return null;
        }

        /// <summary>
        /// Create an area measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="revitBuiltInParam">
        /// The built in parameter to use, if revitParameterName isn't found.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateAreaMeasurePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateAreaMeasurePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateAreaMeasurePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a count measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="elem">The Element.</param>
        /// <param name="revitParameterName">The name of the parameter.</param>
        /// <param name="revitBuiltInParam">The built in parameter to use, if revitParameterName isn't found.</param>
        /// <param name="ifcPropertyName">The name of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateCountMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateCountMeasurePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateCountMeasurePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateCountMeasurePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Creates the shared beam and column QTO values.  
        /// </summary>
        /// <remarks>
        /// This code uses the native implementation for creating these quantities, and the native class for storing the information.
        /// This will be obsoleted.
        /// </remarks>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="elemHandle">The element handle.</param>
        /// <param name="element">The beam or column element.</param>
        /// <param name="typeInfo">The FamilyTypeInfo containing the appropriate data.</param>
        public static void CreateBeamColumnBaseQuantities(ExporterIFC exporterIFC, IFCAnyHandle elemHandle, Element element, FamilyTypeInfo typeInfo)
        {
            IFCTypeInfo ifcTypeInfo = new IFCTypeInfo();
            ifcTypeInfo.ScaledDepth = typeInfo.ScaledDepth;
            ifcTypeInfo.ScaledArea = typeInfo.ScaledArea;
            ifcTypeInfo.ScaledInnerPerimeter = typeInfo.ScaledInnerPerimeter;
            ifcTypeInfo.ScaledOuterPerimeter = typeInfo.ScaledOuterPerimeter;
            ExporterIFCUtils.CreateBeamColumnBaseQuantities(exporterIFC, elemHandle, element, ifcTypeInfo);
        }

        /// <summary>
        ///  Creates the shared beam, column and member QTO values.  
        /// </summary>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="elemHandle">The element handle.</param>
        /// <param name="element">The element.</param>
        /// <param name="ecData">The IFCExtrusionCreationData containing the appropriate data.</param>
        public static void CreateBeamColumnMemberBaseQuantities(ExporterIFC exporterIFC, IFCAnyHandle elemHandle, Element element, IFCExtrusionCreationData ecData)
        {
            IFCTypeInfo ifcTypeInfo = new IFCTypeInfo();
            ifcTypeInfo.ScaledDepth = ecData.ScaledLength;
            ifcTypeInfo.ScaledArea = ecData.ScaledArea;
            ifcTypeInfo.ScaledInnerPerimeter = ecData.ScaledInnerPerimeter;
            ifcTypeInfo.ScaledOuterPerimeter = ecData.ScaledOuterPerimeter;
            ExporterIFCUtils.CreateBeamColumnBaseQuantities(exporterIFC, elemHandle, element, ifcTypeInfo);
        }

        /// <summary>
        /// Creates property sets for Revit groups and parameters, if export options is set.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="element">
        /// The Element.
        /// </param>
        /// <param name="elementSets">
        /// The collection of IFCAnyHandles to relate properties to.
        /// </param>
        public static void CreateInternalRevitPropertySets(ExporterIFC exporterIFC, Element element, ICollection<IFCAnyHandle> elementSets)
        {
            if (exporterIFC == null || element == null || elementSets == null || elementSets.Count == 0 ||
                !ExporterCacheManager.ExportOptionsCache.PropertySetOptions.ExportInternalRevit)
                return;

            IFCFile file = exporterIFC.GetFile();

            double lengthScale = exporterIFC.LinearScale;
            double angleScale = 180.0 / Math.PI;

            ElementId typeId = element.GetTypeId();
            Element elementType = element.Document.GetElement(typeId);
            int whichStart = elementType != null ? 0 : (element is ElementType ? 1 : 0);
            if (whichStart == 1)
            {
                typeId = element.Id;
                elementType = element as ElementType;
            }

            Dictionary<BuiltInParameterGroup, int>[] propertyArrIdxMap;
            propertyArrIdxMap = new Dictionary<BuiltInParameterGroup, int>[2]; // one for instance, one for type.
            propertyArrIdxMap[0] = new Dictionary<BuiltInParameterGroup, int>();
            propertyArrIdxMap[1] = new Dictionary<BuiltInParameterGroup, int>();
            List<HashSet<IFCAnyHandle>>[] propertyArr;
            propertyArr = new List<HashSet<IFCAnyHandle>>[2];
            propertyArr[0] = new List<HashSet<IFCAnyHandle>>();
            propertyArr[1] = new List<HashSet<IFCAnyHandle>>();
            List<string>[] propertySetNames;
            propertySetNames = new List<string>[2];
            propertySetNames[0] = new List<string>();
            propertySetNames[1] = new List<string>();

            // pass through: element and element type.  If the element is a ElementType, there will only be one pass.
            for (int which = whichStart; which < 2; which++)
            {
                Element whichElement = (which == 0) ? element : elementType;
                if (whichElement == null)
                    continue;

                bool createType = (which == 1);
                if (createType)
                {
                    if (ExporterCacheManager.TypePropertyInfoCache.HasTypeProperties(typeId))
                    {
                        ExporterCacheManager.TypePropertyInfoCache.AddTypeProperties(typeId, elementSets);
                        continue;
                    }
                }

                ParameterElementCache parameterElementCache = ParameterUtil.GetNonIFCParametersForElement(whichElement);
                if (parameterElementCache == null)
                    continue;

                foreach (Parameter parameter in parameterElementCache.ParameterCache.Values)
                {
                    Definition parameterDefinition = parameter.Definition;
                    if (parameterDefinition == null)
                        continue;

                    BuiltInParameterGroup parameterGroup = parameterDefinition.ParameterGroup;
                    string parameterCaption = parameterDefinition.Name;
                    
                    int idx = -1;
                    if (!propertyArrIdxMap[which].TryGetValue(parameterGroup, out idx))
                    {
                        idx = propertyArr[which].Count;
                        propertyArrIdxMap[which][parameterGroup] = idx;
                        propertyArr[which].Add(new HashSet<IFCAnyHandle>());

                        string groupName = LabelUtils.GetLabelFor(parameterGroup);
                        propertySetNames[which].Add(groupName);
                    }

                    if (!parameter.HasValue)
                        continue;

                    switch (parameter.StorageType)
                    {
                        case StorageType.None:
                            break;
                        case StorageType.Integer:
                            {
                                int value = parameter.AsInteger();
                                string valueAsString = parameter.AsValueString();

                                // YesNo or actual integer?
                                if (parameterDefinition.ParameterType == ParameterType.YesNo)
                                {
                                    propertyArr[which][idx].Add(CreateBooleanPropertyFromCache(file, parameterCaption, value != 0, PropertyValueType.SingleValue));
                                }
                                else if (parameterDefinition.ParameterType == ParameterType.Invalid && (valueAsString != null))
                                {
                                    // This is probably an internal enumerated type that should be exported as a string.
                                    propertyArr[which][idx].Add(CreateIdentifierPropertyFromCache(file, parameterCaption, valueAsString, PropertyValueType.SingleValue));
                                }
                                else
                                {
                                    propertyArr[which][idx].Add(CreateIntegerPropertyFromCache(file, parameterCaption, value, PropertyValueType.SingleValue));
                                }
                                break;
                            }
                        case StorageType.Double:
                            {
                                double value = parameter.AsDouble();
                                bool assigned = false;
                                switch (parameterDefinition.ParameterType)
                                {
                                    case ParameterType.Length:
                                        {
                                            propertyArr[which][idx].Add(CreateLengthMeasurePropertyFromCache(file, lengthScale, parameterCaption,
                                                value * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Angle:
                                        {
                                            propertyArr[which][idx].Add(CreatePlaneAngleMeasurePropertyFromCache(file, parameterCaption,
                                                value * angleScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Area:
                                        {
                                            propertyArr[which][idx].Add(CreateAreaMeasureProperty(file, parameterCaption,
                                                value * lengthScale * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Volume:
                                        {
                                            propertyArr[which][idx].Add(CreateVolumeMeasureProperty(file, parameterCaption,
                                                value * lengthScale * lengthScale * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.HVACAirflow:
                                    case ParameterType.PipingFlow:
                                        {
                                            // We do not have a separate VolumetricFlow scaling; we use the volume scaling.
                                            double scaledValue = value * lengthScale * lengthScale * lengthScale;
                                            propertyArr[which][idx].Add(CreateVolumetricFlowRateMeasureProperty(file, parameterCaption,
                                                scaledValue, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.HVACPower:
                                        {
                                            double scaledValue = value * (1 / 0.3048) * (1 / 0.3048);
                                            propertyArr[which][idx].Add(CreatePowerProperty(file, parameterCaption,
                                                scaledValue, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                }

                                if (!assigned)
                                {
                                    propertyArr[which][idx].Add(CreateRealPropertyFromCache(file, lengthScale, parameterCaption, value, PropertyValueType.SingleValue));
                                }
                                break;
                            }
                        case StorageType.String:
                            {
                                string value = parameter.AsString();

                                propertyArr[which][idx].Add(CreateTextPropertyFromCache(file, parameterCaption, value, PropertyValueType.SingleValue));
                                break;
                            }
                        case StorageType.ElementId:
                            {
                                ElementId value = parameter.AsElementId();
                                if (value == ElementId.InvalidElementId)
                                    continue;

                                Element paramElement = element.Document.GetElement(value);
                                string valueString = (paramElement != null) ? paramElement.Name : null;
                                if (!string.IsNullOrEmpty(valueString))
                                {
                                    ElementType paramElementType = paramElement is ElementType ? paramElement as ElementType :
                                        element.Document.GetElement(paramElement.GetTypeId()) as ElementType;
                                    string paramElementTypeName = (paramElementType != null) ? ExporterIFCUtils.GetFamilyName(paramElementType) : null;
                                    if (!string.IsNullOrEmpty(paramElementTypeName))
                                        valueString = paramElementTypeName + ": " + valueString;
                                }
                                else
                                    valueString = value.ToString();

                                propertyArr[which][idx].Add(CreateLabelPropertyFromCache(file, parameterCaption, valueString, PropertyValueType.SingleValue, true, null));
                                break;
                            }
                    }
                }
            }

            for (int which = whichStart; which < 2; which++)
            {
                HashSet<IFCAnyHandle> propertySets = new HashSet<IFCAnyHandle>();

                int size = propertyArr[which].Count;
                if (size == 0)
                    continue;

                for (int ii = 0; ii < size; ii++)
                {
                    if (propertyArr[which][ii].Count == 0)
                        continue;

                    IFCAnyHandle propertySet = IFCInstanceExporter.CreatePropertySet(file, GUIDUtil.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        propertySetNames[which][ii], null, propertyArr[which][ii]);

                    if (which == 1)
                        propertySets.Add(propertySet);
                    else
                        IFCInstanceExporter.CreateRelDefinesByProperties(file, GUIDUtil.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                            null, null, elementSets, propertySet);
                }

                if (which == 1)
                    ExporterCacheManager.TypePropertyInfoCache.AddNewTypeProperties(typeId, propertySets, elementSets);
            }
        }

        /// <summary>
        /// Creates property sets for Revit groups and parameters, if export options is set.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="element">
        /// The Element.
        /// </param>
        /// <param name="productWrapper">
        /// The product wrapper.
        /// </param>
        public static void CreateInternalRevitPropertySets(ExporterIFC exporterIFC, Element element, ProductWrapper productWrapper)
        {
            if (exporterIFC == null || element == null || productWrapper == null ||
                !ExporterCacheManager.ExportOptionsCache.PropertySetOptions.ExportInternalRevit)
                return;

            IFCFile file = exporterIFC.GetFile();

            ICollection<IFCAnyHandle> elements = productWrapper.GetAllObjects();
            if (elements.Count == 0)
                return;

            CreateInternalRevitPropertySets(exporterIFC, element, elements);
        }
    }
}
