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
using BIM.IFC.Toolkit;
using BIM.IFC.Utility;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// Represents the type of the container for a property.
    /// </summary>
    public enum PropertyValueType
    {
        /// <summary>
        /// A single property (IfcSingleValue)
        /// </summary>
        SingleValue,
        /// <summary>
        /// An enumerated property (IfcEnumeratedValue)
        /// </summary>
        EnumeratedValue,
        /// <summary>
        /// A list property (IfcListValue)
        /// </summary>
        ListValue,
        /// <summary>
        /// A reference property (IfcPropertyReferenceValue)
        /// </summary>
        ReferenceValue
    }

    /// <summary>
    /// Represents the type of a property.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// A label (string value), up to 255 characters in length.
        /// </summary>
        Label,
        /// <summary>
        /// A text (string value) of unlimited length.
        /// </summary>
        Text,
        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean,
        /// <summary>
        /// A real number value.
        /// </summary>
        Integer,
        /// <summary>
        /// An integer number value.
        /// </summary>
        Real,
        /// <summary>
        /// A positive length value.
        /// </summary>
        PositiveLength,
        /// <summary>
        /// A positive ratio value.
        /// </summary>
        PositiveRatio,
        /// <summary>
        /// An angular value.
        /// </summary>
        PlaneAngle,
        /// <summary>
        /// An area value.
        /// </summary>
        Area,
        /// <summary>
        /// An identifier value.
        /// </summary>
        Identifier,
        /// <summary>
        /// A count value.
        /// </summary>
        Count,
        /// <summary>
        /// A thermodynamic temperature value.
        /// </summary>
        ThermodynamicTemperature,
        /// <summary>
        /// A length value.
        /// </summary>
        Length, // NOTE: This is currently only used for Revit internal properties, and doesn't have all support routines written.
        /// <summary>
        /// A ratio value.
        /// </summary>
        Ratio,
        /// <summary>
        /// A thermal transmittance (coefficient of heat transfer) value.
        /// </summary>
        ThermalTransmittance,
        /// <summary>
        /// A volumetric flow rate value.
        /// </summary>
        VolumetricFlowRate,
        /// <summary>
        /// A logical value: true, false, or unknown.
        /// </summary>
        Logical,
        /// <summary>
        /// A power value.
        /// </summary>
        Power,
        /// <summary>
        /// An IfcClassificationReference value.
        /// </summary>
        ClassificationReference
    }

    /// <summary>
    /// Represents a mapping from a Revit parameter or calculated quantity to an IFC property.
    /// </summary>
    public class PropertySetEntry : Entry
    {
        /// <summary>
        /// The type of the IFC property set entry. Default is label.
        /// </summary>
        PropertyType m_PropertyType = PropertyType.Label;

        /// <summary>
        /// The value type of the IFC property set entry.
        /// </summary>
        PropertyValueType m_PropertyValueType = PropertyValueType.SingleValue;

        /// <summary>
        /// The type of the Enum that will validate the value for an enumeration.
        /// </summary>
        Type m_PropertyEnumerationType = null;

        /// <summary>
        /// Constructs a PropertySetEntry object.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        private PropertySetEntry(string revitParameterName)
            : base(revitParameterName)
        {

        }

        /// <summary>
        /// The type of the IFC property set entry.
        /// </summary>
        public PropertyType PropertyType
        {
            get
            {
                return m_PropertyType;
            }
            private set
            {
                m_PropertyType = value;
            }
        }

        /// <summary>
        /// The value type of the IFC property set entry.
        /// </summary>
        public PropertyValueType PropertyValueType
        {
            get
            {
                return m_PropertyValueType;
            }
            private set
            {
                m_PropertyValueType = value;
            }
        }

        /// <summary>
        /// The type of the Enum that will validate the value for an enumeration.
        /// </summary>
        public Type PropertyEnumerationType
        {
            get
            {
                return m_PropertyEnumerationType;
            }
            private set
            {
                m_PropertyEnumerationType = value;
            }
        }

        /// <summary>
        /// Creates an entry of type real.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateReal(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Real;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type Power.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreatePower(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Power;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type VolumetricFlowRate.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateVolumetricFlowRate(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.VolumetricFlowRate;
            return pse;
        }
        
        /// <summary>
        /// Creates an entry of type ThermodynamicTemperature.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateThermodynamicTemperature(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.ThermodynamicTemperature;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type ThermalTransmittance.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateThermalTransmittance(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.ThermalTransmittance;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type boolean.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateBoolean(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Boolean;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type logical.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateLogical(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Logical;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type label.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateLabel(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Label;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type text.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateText(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Text;
            return pse;
        }
        
        /// <summary>
        /// Creates an entry of type identifier.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateIdentifier(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Identifier;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type integer.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateInteger(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Integer;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type area.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateArea(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Area;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type length.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateLength(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Length;
            return pse;
        }
        
        /// <summary>
        /// Creates an entry of type positive length.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePositiveLength(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PositiveLength;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type ratio.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateRatio(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Ratio;
            return pse;
        }
        
        /// <summary>
        /// Creates an entry of type positive ratio.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePositiveRatio(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PositiveRatio;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type enumerated value.
        /// The type of the enumarated value is also given.
        /// Note that the enumeration list is not supported here.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateEnumeratedValue(string revitParameterName, PropertyType propertyType, Type enumType)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = propertyType;
            pse.PropertyValueType = PropertyValueType.EnumeratedValue;
            pse.PropertyEnumerationType = enumType;
            return pse;
        }

        /// <summary>
        /// Creates an external reference to IfcClassificationReference.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateClassificationReference(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.ClassificationReference;
            pse.PropertyValueType = PropertyValueType.ReferenceValue;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type list value.
        /// The type of the list value is also given.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateListValue(string revitParameterName, PropertyType propertyType)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = propertyType;
            pse.PropertyValueType = PropertyValueType.ListValue;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type plane angle.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePlaneAngle(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PlaneAngle;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type count.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateCount(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Count;
            return pse;
        }

        /// <summary>
        /// Process to create element property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="extrusionCreationData">
        /// The IFCExtrusionCreationData.
        /// </param>
        /// <param name="element">
        /// The element of which this property is created for.
        /// </param>
        /// <param name="elementType">
        /// The element type of which this property is created for.
        /// </param>
        /// <returns>
        /// Then created property handle.
        /// </returns>
        public IFCAnyHandle ProcessEntry(IFCFile file, ExporterIFC exporterIFC,
           IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            IFCAnyHandle propHnd = null;

            if (ParameterNameIsValid)
            {
                propHnd = CreatePropertyFromElementOrSymbol(file, exporterIFC, element);
            }

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
            {
                propHnd = CreatePropertyFromCalculator(file, exporterIFC, extrusionCreationData, element, elementType);
            }

            return propHnd;
        }

        // This function is static to make sure that no properties are used directly from the entry.
        private static IFCAnyHandle CreatePropertyFromElementOrSymbolBase(IFCFile file, ExporterIFC exporterIFC, Element element,
            string revitParamNameToUse, string ifcPropertyName, BuiltInParameter builtInParameter,
            PropertyType propertyType, PropertyValueType valueType, Type propertyEnumerationType)
        {
            IFCAnyHandle propHnd = null;

            switch (propertyType)
            {
                case PropertyType.Text:
                    {
                        propHnd = PropertyUtil.CreateTextPropertyFromElementOrSymbol(file, element, revitParamNameToUse, builtInParameter, ifcPropertyName, valueType, propertyEnumerationType);
                        break;
                    }
                case PropertyType.Label:
                    {
                        propHnd = PropertyUtil.CreateLabelPropertyFromElementOrSymbol(file, element, revitParamNameToUse, builtInParameter, ifcPropertyName, valueType, propertyEnumerationType);
                        break;
                    }
                case PropertyType.Identifier:
                    {
                        propHnd = PropertyUtil.CreateIdentifierPropertyFromElementOrSymbol(file, element, revitParamNameToUse, builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Boolean:
                    {
                        propHnd = PropertyUtil.CreateBooleanPropertyFromElementOrSymbol(file, element, revitParamNameToUse, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Logical:
                    {
                        propHnd = PropertyUtil.CreateLogicalPropertyFromElementOrSymbol(file, element, revitParamNameToUse, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Integer:
                    {
                        propHnd = PropertyUtil.CreateIntegerPropertyFromElementOrSymbol(file, element, revitParamNameToUse, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Real:
                    {
                        double scale = exporterIFC.LinearScale;
                        propHnd = PropertyUtil.CreateRealPropertyFromElementOrSymbol(file, scale, element, revitParamNameToUse, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Length:
                    {
                        propHnd = PropertyUtil.CreateLengthMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.PositiveLength:
                    {
                        propHnd = PropertyUtil.CreatePositiveLengthMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.PositiveRatio:
                    {
                        propHnd = PropertyUtil.CreatePositiveRatioPropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse, 
                            ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Ratio:
                    {
                        propHnd = PropertyUtil.CreateRatioPropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse, ifcPropertyName, 
                            valueType);
                        break;
                    }
                case PropertyType.PlaneAngle:
                    {
                        propHnd = PropertyUtil.CreatePlaneAngleMeasurePropertyFromElementOrSymbol(file, element, revitParamNameToUse, ifcPropertyName, 
                            valueType);
                        break;
                    }
                case PropertyType.Area:
                    {
                        propHnd = PropertyUtil.CreateAreaMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Count:
                    {
                        propHnd = PropertyUtil.CreateCountMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Power:
                    {
                        propHnd = PropertyUtil.CreatePowerPropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse, builtInParameter, 
                            ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.ThermodynamicTemperature:
                    {
                        propHnd = PropertyUtil.CreateThermodynamicTemperaturePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.ThermalTransmittance:
                    {
                        propHnd = PropertyUtil.CreateThermalTransmittancePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.VolumetricFlowRate:
                    {
                        propHnd = PropertyUtil.CreateVolumetricFlowRatePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.ClassificationReference:
                    {
                        propHnd = PropertyUtil.CreateClassificationReferencePropertyFromElementOrSymbol(file, exporterIFC, element, revitParamNameToUse,
                            builtInParameter, ifcPropertyName);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            return propHnd;
        }
        
        /// <summary>
        /// Creates a property from element or its type's parameter.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="element">The element.</param>
        /// <returns>The property handle.</returns>
        IFCAnyHandle CreatePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element element)
        {
            PropertyType propertyType = PropertyType;
            PropertyValueType valueType = PropertyValueType;
            Type propertyEnumerationType = PropertyEnumerationType;

            string ifcPropertyName = ParameterNameToUse;

            string localizedRevitParameterName = LocalizedRevitParameterName(ExporterCacheManager.LanguageType);
            string revitParameterName = RevitParameterName;

            IFCAnyHandle propHnd = null;
            if (localizedRevitParameterName != null)
            {
                propHnd = PropertySetEntry.CreatePropertyFromElementOrSymbolBase(file, exporterIFC, element,
                     localizedRevitParameterName, ifcPropertyName, BuiltInParameter.INVALID,
                     propertyType, valueType, propertyEnumerationType);
            }

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
            {
                propHnd = PropertySetEntry.CreatePropertyFromElementOrSymbolBase(file, exporterIFC, element,
                     revitParameterName, ifcPropertyName, RevitBuiltInParameter,
                     propertyType, valueType, propertyEnumerationType);
            }
            return propHnd;
        }

        /// <summary>
        /// Creates a property from the calculator.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="extrusionCreationData">The IFCExtrusionCreationData.</param>
        /// <param name="element">The element.</param>
        /// <param name="elementType">The element type.</param>
        /// <returns>The property handle.</returns>
        IFCAnyHandle CreatePropertyFromCalculator(IFCFile file, ExporterIFC exporterIFC,
           IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            IFCAnyHandle propHnd = null;

            if (PropertyCalculator != null && PropertyCalculator.Calculate(exporterIFC, extrusionCreationData, element, elementType))
            {
                PropertyType propertyType = PropertyType;
                PropertyValueType valueType = PropertyValueType;
                Type propertyEnumerationType = PropertyEnumerationType;

                switch (propertyType)
                {
                    case PropertyType.Label:
                        {
                            if (PropertyCalculator.CalculatesMutipleValues)
                                propHnd = PropertyUtil.CreateLabelProperty(file, PropertyName, PropertyCalculator.GetStringValues(), valueType, propertyEnumerationType);
                            else
                                propHnd = PropertyUtil.CreateLabelPropertyFromCache(file, PropertyName, PropertyCalculator.GetStringValue(), valueType, false, propertyEnumerationType);
                            break;
                        }
                    case PropertyType.Text:
                        {
                            propHnd = PropertyUtil.CreateTextPropertyFromCache(file, PropertyName, PropertyCalculator.GetStringValue(), valueType);
                            break;
                        }
                    case PropertyType.Identifier:
                        {
                            propHnd = PropertyUtil.CreateIdentifierPropertyFromCache(file, PropertyName, PropertyCalculator.GetStringValue(), valueType);
                            break;
                        }
                    case PropertyType.Boolean:
                        {
                            propHnd = PropertyUtil.CreateBooleanPropertyFromCache(file, PropertyName, PropertyCalculator.GetBooleanValue(), valueType);
                            break;
                        }
                    case PropertyType.Logical:
                        {
                            propHnd = PropertyUtil.CreateLogicalPropertyFromCache(file, PropertyName, PropertyCalculator.GetLogicalValue(), valueType);
                            break;
                        }
                    case PropertyType.Integer:
                        {
                            if (PropertyCalculator.CalculatesMultipleParameters)
                                propHnd = PropertyUtil.CreateIntegerPropertyFromCache(file, PropertyName, PropertyCalculator.GetIntValue(PropertyName), valueType);
                            else
                                propHnd = PropertyUtil.CreateIntegerPropertyFromCache(file, PropertyName, PropertyCalculator.GetIntValue(), valueType);
                            break;
                        }
                    case PropertyType.Real:
                        {
                            double scale = exporterIFC.LinearScale;
                            propHnd = PropertyUtil.CreateRealPropertyFromCache(file, scale, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PositiveLength:
                        {
                            if (PropertyCalculator.CalculatesMultipleParameters)
                                propHnd = PropertyUtil.CreatePositiveLengthMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(PropertyName), valueType);
                            else
                                propHnd = PropertyUtil.CreatePositiveLengthMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PositiveRatio:
                        {
                            propHnd = PropertyUtil.CreatePositiveRatioMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.Ratio:
                        {
                            propHnd = PropertyUtil.CreateRatioMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PlaneAngle:
                        {
                            propHnd = PropertyUtil.CreatePlaneAngleMeasurePropertyFromCache(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.Area:
                        {
                            propHnd = PropertyUtil.CreateAreaMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.Count:
                        {
                            if (PropertyCalculator.CalculatesMultipleParameters)
                                propHnd = PropertyUtil.CreateCountMeasureProperty(file, PropertyName, PropertyCalculator.GetIntValue(PropertyName), valueType);
                            else
                                propHnd = PropertyUtil.CreateCountMeasureProperty(file, PropertyName, PropertyCalculator.GetIntValue(), valueType);
                            break;
                        }
                    case PropertyType.Power:
                        {
                            propHnd = PropertyUtil.CreatePowerPropertyFromCache(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.ThermodynamicTemperature:
                        {
                            propHnd = PropertyUtil.CreateThermodynamicTemperaturePropertyFromCache(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.ThermalTransmittance:
                        {
                            propHnd = PropertyUtil.CreateThermalTransmittancePropertyFromCache(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.VolumetricFlowRate:
                        {
                            propHnd = PropertyUtil.CreateVolumetricFlowRateMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }

            return propHnd;
        }
    }
}
