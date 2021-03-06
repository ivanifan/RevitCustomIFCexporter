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
using System.Collections.ObjectModel;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Exporter.PropertySet;
using BIM.IFC.Exporter.PropertySet.Calculators;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Initializes user defined parameters and quantities.
    /// </summary>
    class ExporterInitializer
    {
        /// <summary>
        /// Initializes property sets.
        /// </summary>
        /// <param name="propertySetsToExport">Existing functions to call for property set initialization.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        public static void InitPropertySets(Exporter.PropertySetsToExport propertySetsToExport, IFCVersion fileVersion)
        {
            ParameterCache cache = ExporterCacheManager.ParameterCache;

            if (ExporterCacheManager.ExportOptionsCache.PropertySetOptions.ExportIFCCommon)
            {
                if (propertySetsToExport == null)
                    propertySetsToExport = InitCommonPropertySets;
                else
                    propertySetsToExport += InitCommonPropertySets;
            }

            if (fileVersion == IFCVersion.IFCCOBIE)
            {
                if (propertySetsToExport == null)
                    propertySetsToExport = InitCOBIEPropertySets;
                else
                    propertySetsToExport += InitCOBIEPropertySets;
            }

            if (propertySetsToExport != null)
                propertySetsToExport(cache.PropertySets, fileVersion);
        }

        /// <summary>
        /// Initializes quantities.
        /// </summary>
        /// <param name="fileVersion">The IFC file version.</param>
        /// <param name="exportBaseQuantities">True if export base quantities.</param>
        public static void InitQuantities(Exporter.QuantitiesToExport quantitiesToExport, IFCVersion fileVersion, bool exportBaseQuantities)
        {
            ParameterCache cache = ExporterCacheManager.ParameterCache;

            if (exportBaseQuantities)
            {
                if (quantitiesToExport == null)
                    quantitiesToExport = InitBaseQuantities;
                else
                    quantitiesToExport += InitBaseQuantities;
            }

            if (fileVersion == IFCVersion.IFCCOBIE)
            {
                if (quantitiesToExport == null)
                    quantitiesToExport = InitCOBIEQuantities;
                else
                    quantitiesToExport += InitCOBIEQuantities; 
            }

            if (quantitiesToExport != null)
                quantitiesToExport(cache.Quantities, fileVersion);
        }

        // Properties
        /// <summary>
        /// Initializes common property sets.
        /// </summary>
        /// <param name="propertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitCommonPropertySets(IList<IList<PropertySetDescription>> propertySets, IFCVersion fileVersion)
        {
            IList<PropertySetDescription> commonPropertySets = new List<PropertySetDescription>();

            // Building/Site property sets.
            InitPropertySetBuildingCommon(commonPropertySets, fileVersion);
            InitPropertySetBuildingWaterStorage(commonPropertySets);
            InitPropertySetSiteCommon(commonPropertySets);

            // Architectural property sets.
            InitPropertySetBuildingElementProxyCommon(commonPropertySets);
            InitPropertySetCoveringCommon(commonPropertySets, fileVersion);
            InitPropertySetCurtainWallCommon(commonPropertySets);
            InitPropertySetDoorCommon(commonPropertySets);
            InitPropertySetLevelCommon(commonPropertySets, fileVersion);
            InitPropertySetRailingCommon(commonPropertySets);
            InitPropertySetRampCommon(commonPropertySets);
            InitPropertySetRampFlightCommon(commonPropertySets);
            InitPropertySetRoofCommon(commonPropertySets, fileVersion);
            InitPropertySetSlabCommon(commonPropertySets);
            InitPropertySetStairCommon(commonPropertySets);
            InitPropertySetStairFlightCommon(commonPropertySets);
            InitPropertySetWallCommon(commonPropertySets);
            InitPropertySetWindowCommon(commonPropertySets);

            // Building property sets.
            InitPropertySetBuildingCommon(commonPropertySets, fileVersion);
            InitPropertySetBuildingWaterStorage(commonPropertySets);

            // Proxy property sets.
            InitPropertySetElementShading(commonPropertySets);

            // Level property sets.
            InitPropertySetLevelCommon(commonPropertySets, fileVersion);

            // Site property sets.
            InitPropertySetSiteCommon(commonPropertySets);

            // Building Element Proxy
            InitPropertySetBuildingElementProxyCommon(commonPropertySets);

            // Space
            InitPropertySetSpaceCommon(commonPropertySets, fileVersion);
            InitPropertySetSpaceFireSafetyRequirements(commonPropertySets);
            InitPropertySetSpaceLightingRequirements(commonPropertySets);
            InitPropertySetSpaceThermalRequirements(commonPropertySets, fileVersion);
            InitPropertySetGSASpaceCategories(commonPropertySets);
            InitPropertySetSpaceOccupant(commonPropertySets);
            InitPropertySetSpaceOccupancyRequirements(commonPropertySets);
            InitPropertySetSpaceZones(commonPropertySets, fileVersion);

            // Structural property sets.
            InitPropertySetBeamCommon(commonPropertySets);
            InitPropertySetColumnCommon(commonPropertySets);
            InitPropertySetMemberCommon(commonPropertySets);

            // MEP property sets.
            InitPropertySetAirTerminalTypeCommon(commonPropertySets);
            InitPropertySetDistributionFlowElementCommon(commonPropertySets);
            InitPropertySetFlowTerminalAirTerminal(commonPropertySets);
            InitPropertySetLightFixtureTypeCommon(commonPropertySets);

            // Energy Analysis property sets.
            InitPropertySetElementShading(commonPropertySets);

            // Misc. property sets
            InitPropertySetManufacturerTypeInformation(commonPropertySets);

            propertySets.Add(commonPropertySets);
        }

        /// <summary>
        /// Initializes manufacturer type information property sets for all IfcElements.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetManufacturerTypeInformation(IList<PropertySetDescription> commonPropertySets)
        {
            //property set Manufacturer Information
            PropertySetDescription propertySetManufacturer = new PropertySetDescription();
            propertySetManufacturer.Name = "Pset_ManufacturerTypeInformation";

            // sub type of IfcElement
            propertySetManufacturer.EntityTypes.Add(IFCEntityType.IfcElement);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("ArticleNumber");
            propertySetManufacturer.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ModelReference");
            propertySetManufacturer.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ModelLabel");
            propertySetManufacturer.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Manufacturer");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.ALL_MODEL_MANUFACTURER;
            propertySetManufacturer.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ProductionYear");
            propertySetManufacturer.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetManufacturer);
        }

        /// <summary>
        /// Initializes common wall property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetWallCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property set wall common
            PropertySetDescription propertySetWallCommon = new PropertySetDescription();
            propertySetWallCommon.Name = "Pset_WallCommon";
            propertySetWallCommon.SubElementIndex = (int)IFCWallSubElements.PSetWallCommon;

            propertySetWallCommon.EntityTypes.Add(IFCEntityType.IfcWall);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateLoadBearingEntry(LoadBearingCalculator.Instance);
            propertySetWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateExtendToStructureEntry();
            propertySetWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateThermalTransmittanceEntry();
            propertySetWallCommon.AddEntry(ifcPSE);

            propertySetWallCommon.AddEntry(PropertySetEntryUtil.CreateAcousticRatingEntry());
            propertySetWallCommon.AddEntry(PropertySetEntryUtil.CreateSurfaceSpreadOfFlameEntry());
            propertySetWallCommon.AddEntry(PropertySetEntryUtil.CreateCombustibleEntry());
            propertySetWallCommon.AddEntry(PropertySetEntryUtil.CreateCompartmentationEntry());
            
            commonPropertySets.Add(propertySetWallCommon);
        }

        /// <summary>
        /// Initializes common curtain wall property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetCurtainWallCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property set curtain wall common
            PropertySetDescription propertySetCurtainWallCommon = new PropertySetDescription();
            propertySetCurtainWallCommon.Name = "Pset_CurtainWallCommon";
            propertySetCurtainWallCommon.SubElementIndex = (int)IFCCurtainWallSubElements.PSetCurtainWallCommon;

            propertySetCurtainWallCommon.EntityTypes.Add(IFCEntityType.IfcCurtainWall);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetCurtainWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetCurtainWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetCurtainWallCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateThermalTransmittanceEntry();
            propertySetCurtainWallCommon.AddEntry(ifcPSE);

            propertySetCurtainWallCommon.AddEntry(PropertySetEntryUtil.CreateAcousticRatingEntry());
            propertySetCurtainWallCommon.AddEntry(PropertySetEntryUtil.CreateSurfaceSpreadOfFlameEntry());
            propertySetCurtainWallCommon.AddEntry(PropertySetEntryUtil.CreateCombustibleEntry());

            commonPropertySets.Add(propertySetCurtainWallCommon);
        }

        /// <summary>
        /// Initializes common covering property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetCoveringCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property set covering common
            PropertySetDescription propertySetCoveringCommon = new PropertySetDescription();
            propertySetCoveringCommon.Name = "Pset_CoveringCommon";
            propertySetCoveringCommon.SubElementIndex = (int)IFCCoveringSubElements.PSetCoveringCommon;

            propertySetCoveringCommon.EntityTypes.Add(IFCEntityType.IfcCovering);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetCoveringCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetCoveringCommon.AddEntry(ifcPSE);

            propertySetCoveringCommon.AddEntry(PropertySetEntryUtil.CreateAcousticRatingEntry());
            propertySetCoveringCommon.AddEntry(PropertySetEntry.CreateLabel("FlammabilityRating"));
            propertySetCoveringCommon.AddEntry(PropertySetEntryUtil.CreateSurfaceSpreadOfFlameEntry());
            propertySetCoveringCommon.AddEntry(PropertySetEntryUtil.CreateCombustibleEntry());


            if (fileVersion == IFCVersion.IFC2x2)
                propertySetCoveringCommon.AddEntry(PropertySetEntry.CreateLabel("Fragility"));
            else
                propertySetCoveringCommon.AddEntry(PropertySetEntry.CreateLabel("FragilityRating"));

            ifcPSE = PropertySetEntry.CreateText("Finish");
            ifcPSE.PropertyCalculator = CoveringFinishCalculator.Instance;
            propertySetCoveringCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TotalThickness");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.CEILING_THICKNESS;
            propertySetCoveringCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetCoveringCommon);
        }

        /// <summary>
        /// Initializes common door property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetDoorCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property set wall common
            PropertySetDescription propertySetDoorCommon = new PropertySetDescription();
            propertySetDoorCommon.Name = "Pset_DoorCommon";
            propertySetDoorCommon.SubElementIndex = (int)IFCDoorSubElements.PSetDoorCommon;

            propertySetDoorCommon.EntityTypes.Add(IFCEntityType.IfcDoor);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetDoorCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetDoorCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetDoorCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateThermalTransmittanceEntry();
            propertySetDoorCommon.AddEntry(ifcPSE);

            propertySetDoorCommon.AddEntry(PropertySetEntryUtil.CreateAcousticRatingEntry());
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateLabel("SecurityRating"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateBoolean("SelfClosing"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateBoolean("SmokeStop"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateReal("GlazingAreaFraction"));
            propertySetDoorCommon.AddEntry(PropertySetEntry.CreateVolumetricFlowRate("Infiltration"));

            commonPropertySets.Add(propertySetDoorCommon);
        }

        /// <summary>
        /// Initializes common window property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetWindowCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property set wall common
            PropertySetDescription propertySetWindowCommon = new PropertySetDescription();
            propertySetWindowCommon.Name = "Pset_WindowCommon";
            propertySetWindowCommon.SubElementIndex = (int)IFCWindowSubElements.PSetWindowCommon;

            propertySetWindowCommon.EntityTypes.Add(IFCEntityType.IfcWindow);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetWindowCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetWindowCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetWindowCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateThermalTransmittanceEntry();
            propertySetWindowCommon.AddEntry(ifcPSE);

            propertySetWindowCommon.AddEntry(PropertySetEntryUtil.CreateAcousticRatingEntry());
            propertySetWindowCommon.AddEntry(PropertySetEntry.CreateLabel("SecurityRating"));

            propertySetWindowCommon.AddEntry(PropertySetEntry.CreateBoolean("SmokeStop"));
            propertySetWindowCommon.AddEntry(PropertySetEntry.CreateReal("GlazingAreaFraction"));
            propertySetWindowCommon.AddEntry(PropertySetEntry.CreateVolumetricFlowRate("Infiltration"));

            commonPropertySets.Add(propertySetWindowCommon);
        }

        /// <summary>
        /// Initializes common LightFixtureType property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetLightFixtureTypeCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertySetLightFixtureTypeCommon = new PropertySetDescription();
            propertySetLightFixtureTypeCommon.Name = "Pset_LightFixtureTypeCommon";
            propertySetLightFixtureTypeCommon.SubElementIndex = (int)IFCLightFixtureTypeSubElements.PSetLightFixtureTypeCommon;

            propertySetLightFixtureTypeCommon.EntityTypes.Add(IFCEntityType.IfcLightFixtureType);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateInteger("NumberOfSources");
            propertySetLightFixtureTypeCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePower("TotalWattage");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.LIGHTING_FIXTURE_WATTAGE;
            propertySetLightFixtureTypeCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateEnumeratedValue("LightFixtureMountingType", PropertyType.Label,
                typeof(PSetLightFixtureTypeCommon_LightFixtureMountingType));
            propertySetLightFixtureTypeCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateEnumeratedValue("LightFixturePlacingType", PropertyType.Label,
                typeof(PSetLightFixtureTypeCommon_LightFixturePlacingType));
            propertySetLightFixtureTypeCommon.AddEntry(ifcPSE);

            propertySetLightFixtureTypeCommon.AddEntry(PropertySetEntry.CreateReal("MaintenanceFactor"));
            propertySetLightFixtureTypeCommon.AddEntry(PropertySetEntry.CreateText("ManufacturersSpecificInformation"));
            propertySetLightFixtureTypeCommon.AddEntry(PropertySetEntry.CreateClassificationReference("ArticleNumber"));

            commonPropertySets.Add(propertySetLightFixtureTypeCommon);
        }


        /// <summary>
        /// Initializes the PSet_DistributionFlowElementCommon property set.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetFlowTerminalAirTerminal(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertyFlowTerminalAirTerminal = new PropertySetDescription();
            propertyFlowTerminalAirTerminal.Name = "Pset_FlowTerminalAirTerminal";
            propertyFlowTerminalAirTerminal.SubElementIndex = (int)IFCDistributionFlowElementSubElements.PSetFlowTerminalAirTerminal;

            propertyFlowTerminalAirTerminal.EntityTypes.Add(IFCEntityType.IfcFlowTerminal);

            propertyFlowTerminalAirTerminal.AddEntry(PropertySetEntry.CreateEnumeratedValue("AirflowType", PropertyType.Label,
                typeof(PSetFlowTerminalAirTerminal_AirTerminalAirflowType)));
            propertyFlowTerminalAirTerminal.AddEntry(PropertySetEntry.CreateEnumeratedValue("Location", PropertyType.Label,
                typeof(PSetFlowTerminalAirTerminal_AirTerminalLocation)));
            
            commonPropertySets.Add(propertyFlowTerminalAirTerminal);
        }

        /// <summary>
        /// Initializes the PSet_AirTerminalTypeCommon property set.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetAirTerminalTypeCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property air terminal type common
            PropertySetDescription propertyAirTerminalTypeCommon = new PropertySetDescription();
            propertyAirTerminalTypeCommon.Name = "PSet_AirTerminalTypeCommon";
            propertyAirTerminalTypeCommon.SubElementIndex = (int)IFCDistributionFlowElementSubElements.PSetAirTerminalTypeCommon;

            propertyAirTerminalTypeCommon.EntityTypes.Add(IFCEntityType.IfcAirTerminalType);

            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("Shape", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalShape)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("FlowPattern", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalFlowPattern)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateVolumetricFlowRate("AirFlowrateRange"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("TemperatureRange"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("DischargeDirection", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalDischargeDirection)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateLength("ThrowLength"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateReal("AirDiffusionPerformanceIndex"));
            //propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateMaterial("Material"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("FinishType", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalFinishType)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateLabel("FinishColor"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("MountingType", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalMountingType)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("CoreType", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalCoreType)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreatePlaneAngle("CoreSetHorizontal"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreatePlaneAngle("CoreSetVertical"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateBoolean("HasIntegralControl"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateEnumeratedValue("FlowControlType", PropertyType.Label,
                typeof(PSetAirTerminalTypeCommon_AirTerminalFlowControlType)));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateBoolean("HasSoundAttenuator"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateBoolean("HasThermalInsulation"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateArea("NeckArea"));
            propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateArea("EffectiveArea"));
            //propertyAirTerminalTypeCommon.AddEntry(PropertySetEntry.CreateMass("Weight"));
            //AirFlowrateVersusFlowControlElement: IfcPropertyTableValue not supported.
            
            commonPropertySets.Add(propertyAirTerminalTypeCommon);
        }
        
        /// <summary>
        /// Initializes common beam property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBeamCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertySetBeamCommon = new PropertySetDescription();
            propertySetBeamCommon.Name = "Pset_BeamCommon";
            propertySetBeamCommon.SubElementIndex = (int)IFCBeamSubElements.PSetBeamCommon;

            propertySetBeamCommon.EntityTypes.Add(IFCEntityType.IfcBeam);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetBeamCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateLoadBearingEntry(BeamLoadBearingCalculator.Instance);
            propertySetBeamCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetBeamCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetBeamCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("Span");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.INSTANCE_LENGTH_PARAM;
            ifcPSE.PropertyCalculator = BeamSpanCalculator.Instance;
            propertySetBeamCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = BeamSlopeCalculator.Instance;
            propertySetBeamCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetBeamCommon);
        }

        /// <summary>
        /// Initializes common IfcMember property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        /// <remarks>Reuses beam calculators for some values.</remarks>
        private static void InitPropertySetMemberCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertySetMemberCommon = new PropertySetDescription();
            propertySetMemberCommon.Name = "Pset_MemberCommon";
            propertySetMemberCommon.SubElementIndex = (int)IFCMemberSubElements.PSetMemberCommon;

            propertySetMemberCommon.EntityTypes.Add(IFCEntityType.IfcMember);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetMemberCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateLoadBearingEntry(BeamLoadBearingCalculator.Instance);
            propertySetMemberCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetMemberCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetMemberCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("Span");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.INSTANCE_LENGTH_PARAM;
            ifcPSE.PropertyCalculator = BeamSpanCalculator.Instance;
            propertySetMemberCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = BeamSlopeCalculator.Instance;
            propertySetMemberCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetMemberCommon);
        }

        /// <summary>
        /// Initializes common column property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetColumnCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property column common
            PropertySetDescription propertySetColumnCommon = new PropertySetDescription();
            propertySetColumnCommon.Name = "Pset_ColumnCommon";

            propertySetColumnCommon.EntityTypes.Add(IFCEntityType.IfcColumn);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetColumnCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateLoadBearingEntry(ColumnLoadBearingCalculator.Instance);
            propertySetColumnCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetColumnCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetColumnCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = BeamSlopeCalculator.Instance;
            propertySetColumnCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetColumnCommon);
        }

        /// <summary>
        /// Initializes common roof property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRoofCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            // PSet_RoofCommon
            PropertySetDescription propertySetRoofCommon = new PropertySetDescription();
            propertySetRoofCommon.Name = "Pset_RoofCommon";
            propertySetRoofCommon.SubElementIndex = (int)IFCRoofSubElements.PSetRoofCommon;

            propertySetRoofCommon.EntityTypes.Add(IFCEntityType.IfcRoof);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetRoofCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetRoofCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetRoofCommon.AddEntry(ifcPSE);

            if (fileVersion != IFCVersion.IFC2x2)
            {
                ifcPSE = PropertySetEntry.CreateArea("TotalArea");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
                propertySetRoofCommon.AddEntry(ifcPSE);

                ifcPSE = PropertySetEntry.CreateArea("ProjectedArea");
                ifcPSE.PropertyCalculator = RoofProjectedAreaCalculator.Instance;
                propertySetRoofCommon.AddEntry(ifcPSE);
            }

            commonPropertySets.Add(propertySetRoofCommon);
        }

        /// <summary>
        /// Initializes common slab property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSlabCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_SlabCommon
            PropertySetDescription propertySetSlabCommon = new PropertySetDescription();
            propertySetSlabCommon.Name = "Pset_SlabCommon";
            propertySetSlabCommon.SubElementIndex = (int)IFCSlabSubElements.PSetSlabCommon;
            
            propertySetSlabCommon.EntityTypes.Add(IFCEntityType.IfcSlab);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateLoadBearingEntry(SlabLoadBearingCalculator.Instance); // always true
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateAcousticRatingEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateSurfaceSpreadOfFlameEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateCombustibleEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateCompartmentationEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateThermalTransmittanceEntry();
            propertySetSlabCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetSlabCommon);
        }

        /// <summary>
        /// Initializes common railing property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRailingCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_RailingCommon
            PropertySetDescription propertySetRailingCommon = new PropertySetDescription();
            propertySetRailingCommon.Name = "Pset_RailingCommon";

            propertySetRailingCommon.EntityTypes.Add(IFCEntityType.IfcRailing);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetRailingCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetRailingCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("Height");
            ifcPSE.PropertyCalculator = RailingHeightCalculator.Instance;
            propertySetRailingCommon.AddEntry(ifcPSE);

            // Railing diameter not supported.

            commonPropertySets.Add(propertySetRailingCommon);
        }

        /// <summary>
        /// Initializes common ramp property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRampCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_RampCommon
            PropertySetDescription propertySetRampCommon = new PropertySetDescription();
            propertySetRampCommon.Name = "Pset_RampCommon";
            propertySetRampCommon.SubElementIndex = (int)IFCRampSubElements.PSetRampCommon;

            propertySetRampCommon.EntityTypes.Add(IFCEntityType.IfcRamp);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetRampCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetRampCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetRampCommon.AddEntry(ifcPSE);

            propertySetRampCommon.AddEntry(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetRampCommon.AddEntry(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetRampCommon.AddEntry(PropertySetEntry.CreateBoolean("HasNonSkidSurface"));
            propertySetRampCommon.AddEntry(PropertySetEntry.CreateReal("RequiredHeadroom"));
            propertySetRampCommon.AddEntry(PropertySetEntry.CreateReal("RequiredSlope"));

            commonPropertySets.Add(propertySetRampCommon);
        }

        /// <summary>
        /// Initializes common stair flight property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetStairFlightCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_StairFlightCommon
            PropertySetDescription propertySetStairFlightCommon = new PropertySetDescription();
            propertySetStairFlightCommon.Name = "Pset_StairFlightCommon";
            // Add Calculator for SubElementIndex.

            propertySetStairFlightCommon.EntityTypes.Add(IFCEntityType.IfcStairFlight);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            PropertyCalculator stairRiserAndTreadsCalculator = StairRiserTreadsCalculator.Instance;
            ifcPSE = PropertySetEntry.CreateCount("NumberOfRiser");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateCount("NumberOfTreads");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("RiserHeight");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLengthAtOffset");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLengthAtInnerSide");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLength("NosingLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("WalkingLineOffset");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("WaistThickness");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.AddEntry(ifcPSE);

            propertySetStairFlightCommon.AddEntry(PropertySetEntry.CreatePositiveLength("Headroom"));

            commonPropertySets.Add(propertySetStairFlightCommon);
        }

        /// <summary>
        /// Initializes common ramp flight property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRampFlightCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // Pset_RampFlightCommon
            PropertySetDescription propertySetRampFlightCommon = new PropertySetDescription();
            propertySetRampFlightCommon.Name = "Pset_RampFlightCommon";

            propertySetRampFlightCommon.EntityTypes.Add(IFCEntityType.IfcRampFlight);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetRampFlightCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = RampFlightSlopeCalculator.Instance;
            propertySetRampFlightCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetRampFlightCommon);
        }

        /// <summary>
        /// Initializes common stair property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetStairCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_StairCommon
            PropertySetDescription propertySetStairCommon = new PropertySetDescription();
            propertySetStairCommon.Name = "Pset_StairCommon";
            propertySetStairCommon.SubElementIndex = (int)IFCStairSubElements.PSetStairCommon;

            propertySetStairCommon.EntityTypes.Add(IFCEntityType.IfcStair);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetStairCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateIsExternalEntry();
            propertySetStairCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntryUtil.CreateFireRatingEntry();
            propertySetStairCommon.AddEntry(ifcPSE);

            PropertyCalculator stairRiserAndTreadsCalculator = StairRiserTreadsCalculator.Instance;
            ifcPSE = PropertySetEntry.CreateCount("NumberOfRiser");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateCount("NumberOfTreads");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("RiserHeight");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.AddEntry(ifcPSE);

            propertySetStairCommon.AddEntry(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetStairCommon.AddEntry(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetStairCommon.AddEntry(PropertySetEntry.CreateBoolean("HasNonSkidSurface"));
            propertySetStairCommon.AddEntry(PropertySetEntry.CreateBoolean("RequiredHeadroom"));

            commonPropertySets.Add(propertySetStairCommon);
        }

        /// <summary>
        /// Initializes common building property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitPropertySetBuildingCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            // PSet_BuildingCommon
            PropertySetDescription propertySetBuildingCommon = new PropertySetDescription();
            propertySetBuildingCommon.Name = "Pset_BuildingCommon";
            propertySetBuildingCommon.EntityTypes.Add(IFCEntityType.IfcBuilding);

            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateIdentifier("BuildingID"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateBoolean("IsPermanentID"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateLabel("MainFireUse"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateLabel("AncillaryFireUse"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateBoolean("SprinklerProtection"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateLabel("OccupancyType"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateArea("GrossPlannedArea"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateLabel("YearOfConstruction"));
            propertySetBuildingCommon.AddEntry(PropertySetEntry.CreateBoolean("IsLandmarked"));

            if (fileVersion != IFCVersion.IFC2x2)
            {
                PropertySetEntry ifcPSE = PropertySetEntry.CreateInteger("NumberOfStoreys");
                ifcPSE.PropertyCalculator = NumberOfStoreysCalculator.Instance;
                propertySetBuildingCommon.AddEntry(ifcPSE);
            }

            commonPropertySets.Add(propertySetBuildingCommon);
        }

        /// <summary>
        /// Initializes common level property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitPropertySetLevelCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property level common
            PropertySetDescription propertySetLevelCommon = new PropertySetDescription();
            propertySetLevelCommon.Name = "Pset_BuildingStoreyCommon";
            propertySetLevelCommon.EntityTypes.Add(IFCEntityType.IfcBuildingStorey);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateBoolean("EntranceLevel");
            propertySetLevelCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLogical("AboveGround");
            propertySetLevelCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("SprinklerProtection");
            propertySetLevelCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic");
            propertySetLevelCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("GrossAreaPlanned");
            propertySetLevelCommon.AddEntry(ifcPSE);

            if (fileVersion != IFCVersion.IFC2x2)
            {
                ifcPSE = PropertySetEntry.CreateReal("NetAreaPlanned");
                propertySetLevelCommon.AddEntry(ifcPSE);
            }

            commonPropertySets.Add(propertySetLevelCommon);
        }

        /// <summary>
        /// Initializes common site property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSiteCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property site common
            PropertySetDescription propertySetSiteCommon = new PropertySetDescription();
            propertySetSiteCommon.Name = "Pset_SiteCommon";
            propertySetSiteCommon.EntityTypes.Add(IFCEntityType.IfcSite);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateArea("BuildableArea");
            propertySetSiteCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("BuildingHeightLimit");
            propertySetSiteCommon.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateArea("TotalArea");
            propertySetSiteCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetSiteCommon);
        }

        /// <summary>
        /// Initializes common building element proxy property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBuildingElementProxyCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property building element proxy common
            PropertySetDescription propertySetBuildingElementProxyCommon = new PropertySetDescription();
            propertySetBuildingElementProxyCommon.Name = "Pset_BuildingElementProxyCommon";
            propertySetBuildingElementProxyCommon.EntityTypes.Add(IFCEntityType.IfcBuildingElementProxy);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetBuildingElementProxyCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetBuildingElementProxyCommon);
        }

        /// <summary>
        /// Initializes the PSet_DistributionFlowElementCommon property set.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetDistributionFlowElementCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertyDistributionFlowElementCommon = new PropertySetDescription();
            propertyDistributionFlowElementCommon.Name = "PSet_DistributionFlowElementCommon";
            propertyDistributionFlowElementCommon.SubElementIndex = (int)IFCDistributionFlowElementSubElements.PSetDistributionFlowElementCommon;

            propertyDistributionFlowElementCommon.EntityTypes.Add(IFCEntityType.IfcDistributionFlowElement);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertyDistributionFlowElementCommon.AddEntry(ifcPSE);

            commonPropertySets.Add(propertyDistributionFlowElementCommon);
        }


        /// <summary>
        /// Initializes common space property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property set space common
            PropertySetDescription propertySetSpaceCommon = new PropertySetDescription();
            propertySetSpaceCommon.Name = "Pset_SpaceCommon";

            propertySetSpaceCommon.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetSpaceCommon.AddEntry(ifcPSE);

            propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateBoolean("PubliclyAccessible"));
            propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateArea("GrossPlannedArea"));

            if (fileVersion == IFCVersion.IFC2x2)
            {
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateLabel("OccupancyType"));
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateReal("OccupancyNumber"));
            
                ifcPSE = PropertySetEntry.CreateBoolean("Concealed");
                ifcPSE.PropertyCalculator = SpaceConcealCalculator.Instance;
                propertySetSpaceCommon.AddEntry(ifcPSE);
            }
            else
            {
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateLabel("Category"));
                
                ifcPSE = PropertySetEntry.CreateLabel("CeilingCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_CEILING;
                propertySetSpaceCommon.AddEntry(ifcPSE);

                ifcPSE = PropertySetEntry.CreateLabel("WallCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_WALL;
                propertySetSpaceCommon.AddEntry(ifcPSE);

                ifcPSE = PropertySetEntry.CreateLabel("FloorCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_FLOOR;
                propertySetSpaceCommon.AddEntry(ifcPSE);

                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateLabel("SkirtingBoard"));
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateArea("NetPlannedArea"));
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateBoolean("ConcealedFlooring"));
                propertySetSpaceCommon.AddEntry(PropertySetEntry.CreateBoolean("ConcealedCeiling"));
            }

            commonPropertySets.Add(propertySetSpaceCommon);
        }

        /// <summary>
        /// Initializes SpaceOccupancyRequirements property set.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceOccupancyRequirements(IList<PropertySetDescription> commonPropertySets)
        {
            //property set space common
            PropertySetDescription propertySetSpaceOccupancyRequirements = new PropertySetDescription();
            propertySetSpaceOccupancyRequirements.Name = "Pset_SpaceOccupancyRequirements";

            propertySetSpaceOccupancyRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateLabel("OccupancyType"));
            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateCount("OccupancyNumber"));
            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateCount("OccupancyNumberPeak"));
            //propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateTime("OccupancyTimePerDay"));
            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateArea("AreaPerOccupant"));
            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateLength("MinimumHeadroom"));
            propertySetSpaceOccupancyRequirements.AddEntry(PropertySetEntry.CreateBoolean("IsOutlookDesirable"));

            commonPropertySets.Add(propertySetSpaceOccupancyRequirements);
        }

        /// <summary>
        /// Initializes common zone property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetZoneCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property set zone common
            PropertySetDescription propertySetZoneCommon = new PropertySetDescription();
            propertySetZoneCommon.Name = "Pset_ZoneCommon";

            propertySetZoneCommon.EntityTypes.Add(IFCEntityType.IfcZone);

            PropertySetEntry ifcPSE = PropertySetEntryUtil.CreateReferenceEntry();
            propertySetZoneCommon.AddEntry(ifcPSE);

            propertySetZoneCommon.AddEntry(PropertySetEntry.CreateLabel("Category"));
            propertySetZoneCommon.AddEntry(PropertySetEntry.CreateArea("GrossPlannedArea"));
            propertySetZoneCommon.AddEntry(PropertySetEntry.CreateArea("NetPlannedArea"));
            propertySetZoneCommon.AddEntry(PropertySetEntry.CreateBoolean("PubliclyAccessible"));
            propertySetZoneCommon.AddEntry(PropertySetEntry.CreateBoolean("HandicapAccessible"));

            commonPropertySets.Add(propertySetZoneCommon);
        }

        /// <summary>
        /// Initializes SpaceFireSafetyRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceFireSafetyRequirements(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceFireSafetyRequirements = new PropertySetDescription();
            propertySetSpaceFireSafetyRequirements.Name = "Pset_SpaceFireSafetyRequirements";

            propertySetSpaceFireSafetyRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateLabel("MainFireUse"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateLabel("AncillaryFireUse"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateLabel("FireRiskFactor"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateLabel("FireHazardFactor"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateBoolean("FlammableStorage"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateBoolean("SprinklerProtection"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic"));
            propertySetSpaceFireSafetyRequirements.AddEntry(PropertySetEntry.CreateBoolean("AirPressurization"));

            commonPropertySets.Add(propertySetSpaceFireSafetyRequirements);
        }

        /// <summary>
        /// Initializes SpaceLightingRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceLightingRequirements(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceLightingRequirements = new PropertySetDescription();
            propertySetSpaceLightingRequirements.Name = "Pset_SpaceLightingRequirements";

            propertySetSpaceLightingRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceLightingRequirements.AddEntry(PropertySetEntry.CreateBoolean("ArtificialLighting"));
            propertySetSpaceLightingRequirements.AddEntry(PropertySetEntry.CreateReal("Illuminance"));

            commonPropertySets.Add(propertySetSpaceLightingRequirements);
        }

        /// <summary>
        /// Initializes SpaceThermalRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceThermalRequirements(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            PropertySetDescription propertySetSpaceThermalRequirements = new PropertySetDescription();
            propertySetSpaceThermalRequirements.Name = "Pset_SpaceThermalRequirements";

            propertySetSpaceThermalRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureMax"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureMin"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateRatio("SpaceHumidity"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateRatio("SpaceHumiditySummer"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateRatio("SpaceHumidityWinter"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateBoolean("DiscontinuedHeating"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateBoolean("NaturalVentilation"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateCount("NaturalVentilationRate"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateCount("MechanicalVentilationRate"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateBoolean("AirConditioning"));
            propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateBoolean("AirConditioningCentral"));

            if (fileVersion == IFCVersion.IFC2x2)
            {
                PropertySetEntry ifcPSE = PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureSummer");
                ifcPSE.PropertyCalculator = new SpaceTemperatureCalculator("SpaceTemperatureSummer");
                propertySetSpaceThermalRequirements.AddEntry(ifcPSE);

                ifcPSE = PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureWinter");
                ifcPSE.PropertyCalculator = new SpaceTemperatureCalculator("SpaceTemperatureWinter");
                propertySetSpaceThermalRequirements.AddEntry(ifcPSE);
            }
            else
            {
                propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureSummerMax"));
                propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureSummerMin"));
                propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureWinterMax"));
                propertySetSpaceThermalRequirements.AddEntry(PropertySetEntry.CreateThermodynamicTemperature("SpaceTemperatureWinterMin"));
            }

            commonPropertySets.Add(propertySetSpaceThermalRequirements);
        }

        /// <summary>
        /// Initializes GSA Space Categories property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetGSASpaceCategories(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetGSASpaceCategories = new PropertySetDescription();
            propertySetGSASpaceCategories.Name = "GSA Space Categories";

            propertySetGSASpaceCategories.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetGSASpaceCategories.AddEntry(PropertySetEntry.CreateLabel("GSA STAR Space Type"));
            propertySetGSASpaceCategories.AddEntry(PropertySetEntry.CreateLabel("GSA STAR Space Category"));
            propertySetGSASpaceCategories.AddEntry(PropertySetEntry.CreateLabel("ANSI/BOMA Space Category"));

            commonPropertySets.Add(propertySetGSASpaceCategories);
        }

        /// <summary>
        /// Initializes Space Occupant Properties sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceOccupant(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceOccupant = new PropertySetDescription();
            propertySetSpaceOccupant.Name = "Space Occupant Properties";

            propertySetSpaceOccupant.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceOccupant.AddEntry(PropertySetEntry.CreateLabel("Occupant Organization Code"));
            propertySetSpaceOccupant.AddEntry(PropertySetEntry.CreateLabel("Occupant Organization Abbreviation"));
            propertySetSpaceOccupant.AddEntry(PropertySetEntry.CreateLabel("Occupant Organization Name"));
            propertySetSpaceOccupant.AddEntry(PropertySetEntry.CreateLabel("Occupant Sub-Organization Code"));
            propertySetSpaceOccupant.AddEntry(PropertySetEntry.CreateLabel("Occupant Billing ID"));

            commonPropertySets.Add(propertySetSpaceOccupant);
        }

        /// <summary>
        /// Initializes Space Zones property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceZones(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            PropertySetDescription propertySetSpaceZones = new PropertySetDescription();
            propertySetSpaceZones.Name = "Space Zones";

            propertySetSpaceZones.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceZones.AddEntry(PropertySetEntry.CreateLabel("Security Zone"));
            propertySetSpaceZones.AddEntry(PropertySetEntry.CreateLabel("Preservation Zone"));
            propertySetSpaceZones.AddEntry(PropertySetEntry.CreateLabel("Privacy Zone"));
            if (fileVersion != IFCVersion.IFC2x2)
            {
                propertySetSpaceZones.AddEntry(PropertySetEntry.CreateLabel("Zone GrossAreaPlanned"));
                propertySetSpaceZones.AddEntry(PropertySetEntry.CreateLabel("Zone NetAreaPlanned"));
            }

            PropertySetEntry ifcPSE = PropertySetEntry.CreateListValue("Project Specific Zone", PropertyType.Label);
            ifcPSE.PropertyCalculator = SpecificZoneCalculator.Instance;
            ifcPSE.UseCalculatorOnly = true;
            propertySetSpaceZones.AddEntry(ifcPSE);

            commonPropertySets.Add(propertySetSpaceZones);
        }

        /// <summary>
        /// Initializes building water storage property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBuildingWaterStorage(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetBuildingWaterStorage = new PropertySetDescription();
            propertySetBuildingWaterStorage.Name = "Pset_BuildingWaterStorage";
            propertySetBuildingWaterStorage.EntityTypes.Add(IFCEntityType.IfcBuilding);

            propertySetBuildingWaterStorage.AddEntry(PropertySetEntry.CreateReal("OneDayPotableWater"));
            propertySetBuildingWaterStorage.AddEntry(PropertySetEntry.CreateReal("OneDayProcessOrProductionWater"));
            propertySetBuildingWaterStorage.AddEntry(PropertySetEntry.CreateReal("OneDayCoolingTowerMakeupWater"));

            commonPropertySets.Add(propertySetBuildingWaterStorage);
        }

        /// <summary>
        /// Initializes element shading property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetElementShading(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetElementShading = new PropertySetDescription();
            propertySetElementShading.Name = "Pset_ElementShading";
            propertySetElementShading.EntityTypes.Add(IFCEntityType.IfcBuildingElementProxy);
            propertySetElementShading.ObjectType = "Solar Shade";

            propertySetElementShading.AddEntry(PropertySetEntry.CreateEnumeratedValue("ShadingDeviceType", PropertyType.Label,
                typeof(PSetElementShading_ShadingDeviceType)));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePlaneAngle("Azimuth"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePlaneAngle("Inclination"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePlaneAngle("TiltRange"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePositiveRatio("AverageSolarTransmittance"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePositiveRatio("AverageVisibleTransmittance"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePositiveRatio("Reflectance"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreatePositiveLength("Roughness"));
            propertySetElementShading.AddEntry(PropertySetEntry.CreateLabel("Color"));

            commonPropertySets.Add(propertySetElementShading);
        }

        /// <summary>
        /// Initializes COBIE property sets.
        /// </summary>
        /// <param name="propertySets">List to store property sets.</param>
        private static void InitCOBIEPropertySets(IList<IList<PropertySetDescription>> propertySets, IFCVersion fileVersion)
        {
            IList<PropertySetDescription> cobiePSets = new List<PropertySetDescription>();
            InitCOBIEPSetSpaceThermalSimulationProperties(cobiePSets);
            InitCOBIEPSetSpaceThermalDesign(cobiePSets);
            InitCOBIEPSetSpaceVentilationCriteria(cobiePSets);
            InitCOBIEPSetBuildingEnergyTarget(cobiePSets);
            InitCOBIEPSetGlazingPropertiesEnergyAnalysis(cobiePSets);
            InitCOBIEPSetPhotovoltaicArray(cobiePSets);
            propertySets.Add(cobiePSets);
        }

        /// <summary>
        /// Initializes COBIE space thermal simulation property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceThermalSimulationProperties(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceThermalSimulationProperties = new PropertySetDescription();
            propertySetSpaceThermalSimulationProperties.Name = "ePset_SpaceThermalSimulationProperties";
            propertySetSpaceThermalSimulationProperties.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Space Thermal Simulation Type");
            ifcPSE.PropertyName = "SpaceThermalSimulationType";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Space Conditioning Requirement");
            ifcPSE.PropertyName = "SpaceConditioningRequirement";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Density");
            ifcPSE.PropertyName = "SpaceOccupantDensity";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Heat Rate");
            ifcPSE.PropertyName = "SpaceOccupantHeatRate";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Load");
            ifcPSE.PropertyName = "SpaceOccupantLoad";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Equipment Load");
            ifcPSE.PropertyName = "SpaceEquipmentLoad";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Lighting Load");
            ifcPSE.PropertyName = "SpaceLightingLoad";
            propertySetSpaceThermalSimulationProperties.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceThermalSimulationProperties);
        }

        /// <summary>
        /// Initializes COBIE space thermal design property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceThermalDesign(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceThermalDesign = new PropertySetDescription();
            propertySetSpaceThermalDesign.Name = "Pset_SpaceThermalDesign";
            propertySetSpaceThermalDesign.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateThermodynamicTemperature("Inside Dry Bulb Temperature - Heating");
            ifcPSE.PropertyName = "HeatingDryBulb";
            propertySetSpaceThermalDesign.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Relative Humidity - Heating");
            ifcPSE.PropertyName = "HeatingRelativeHumidity";
            propertySetSpaceThermalDesign.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateThermodynamicTemperature("Inside Dry Bulb Temperature - Cooling");
            ifcPSE.PropertyName = "CoolingDryBulb";
            propertySetSpaceThermalDesign.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Relative Humidity - Cooling");
            ifcPSE.PropertyName = "CoolingRelativeHumidity";
            propertySetSpaceThermalDesign.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Return Air Plenum");
            ifcPSE.PropertyName = "InsideReturnAirPlenum";
            propertySetSpaceThermalDesign.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceThermalDesign);
        }

        /// <summary>
        /// Initializes COBIE space ventilation criteria property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceVentilationCriteria(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceVentilationCriteria = new PropertySetDescription();
            propertySetSpaceVentilationCriteria.Name = "ePset_SpaceVentilationCriteria";
            propertySetSpaceVentilationCriteria.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Ventilation Type");
            ifcPSE.PropertyName = "VentilationType";
            propertySetSpaceVentilationCriteria.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Outside Air Per Person");
            ifcPSE.PropertyName = "OutsideAirPerPerson";
            propertySetSpaceVentilationCriteria.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceVentilationCriteria);
        }

        /// <summary>
        /// Initializes COBIE building energy target property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetBuildingEnergyTarget(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetBuildingEnergyTarget = new PropertySetDescription();
            propertySetBuildingEnergyTarget.Name = "ePset_BuildingEnergyTarget";
            propertySetBuildingEnergyTarget.EntityTypes.Add(IFCEntityType.IfcBuilding);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateReal("Building Energy Target Value");
            ifcPSE.PropertyName = "BuildingEnergyTargetValue";
            propertySetBuildingEnergyTarget.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Building Energy Target Units");
            ifcPSE.PropertyName = "BuildingEnergyTargetUnits";
            propertySetBuildingEnergyTarget.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetBuildingEnergyTarget);
        }

        /// <summary>
        /// Initializes COBIE glazing properties energy analysis property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetGlazingPropertiesEnergyAnalysis(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetGlazingPropertiesEnergyAnalysis = new PropertySetDescription();
            propertySetGlazingPropertiesEnergyAnalysis.Name = "ePset_GlazingPropertiesEnergyAnalysis";
            propertySetGlazingPropertiesEnergyAnalysis.EntityTypes.Add(IFCEntityType.IfcCurtainWall);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Windows 6 Glazing System Name");
            ifcPSE.PropertyName = "Windows6GlazingSystemName";
            propertySetGlazingPropertiesEnergyAnalysis.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetGlazingPropertiesEnergyAnalysis);
        }

        /// <summary>
        /// Initializes COBIE photo voltaic array property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetPhotovoltaicArray(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetPhotovoltaicArray = new PropertySetDescription();
            propertySetPhotovoltaicArray.Name = "ePset_PhotovoltaicArray";
            propertySetPhotovoltaicArray.EntityTypes.Add(IFCEntityType.IfcRoof);
            propertySetPhotovoltaicArray.EntityTypes.Add(IFCEntityType.IfcWall);
            propertySetPhotovoltaicArray.EntityTypes.Add(IFCEntityType.IfcSlab);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateBoolean("Hosts Photovoltaic Array");
            ifcPSE.PropertyName = "HostsPhotovoltaicArray";
            propertySetPhotovoltaicArray.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Active Area Ratio");
            ifcPSE.PropertyName = "ActiveAreaRatio";
            propertySetPhotovoltaicArray.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("DC to AC Conversion Efficiency");
            ifcPSE.PropertyName = "DcToAcConversionEfficiency";
            propertySetPhotovoltaicArray.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Photovoltaic Surface Integration");
            ifcPSE.PropertyName = "PhotovoltaicSurfaceIntegration";
            propertySetPhotovoltaicArray.AddEntry(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Photovoltaic Cell Efficiency");
            ifcPSE.PropertyName = "PhotovoltaicCellEfficiency";
            propertySetPhotovoltaicArray.AddEntry(ifcPSE);

            cobiePropertySets.Add(propertySetPhotovoltaicArray);
        }

        // Quantities

        /// <summary>
        /// Initializes ceiling base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitCeilingBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcCeilingQuantity = new QuantityDescription();
            ifcCeilingQuantity.Name = "BaseQuantities";
            ifcCeilingQuantity.EntityTypes.Add(IFCEntityType.IfcCovering);

            QuantityEntry ifcQE = new QuantityEntry("GrossCeilingArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
            ifcCeilingQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcCeilingQuantity);
        }

        /// <summary>
        /// Initializes railing base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitRailingBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcRailingQuantity = new QuantityDescription();
            ifcRailingQuantity.Name = "BaseQuantities";
            ifcRailingQuantity.EntityTypes.Add(IFCEntityType.IfcRailing);

            QuantityEntry ifcQE = new QuantityEntry("Length");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;
            ifcRailingQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcRailingQuantity);
        }

        /// <summary>
        /// Initializes slab base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitSlabBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcSlabQuantity = new QuantityDescription();
            ifcSlabQuantity.Name = "BaseQuantities";
            ifcSlabQuantity.EntityTypes.Add(IFCEntityType.IfcSlab);

            QuantityEntry ifcQE = new QuantityEntry("GrossArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SlabGrossAreaCalculator.Instance;
            ifcSlabQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossVolume");
            ifcQE.QuantityType = QuantityType.Volume;
            ifcQE.PropertyCalculator = SlabGrossVolumeCalculator.Instance;
            ifcSlabQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Perimeter");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SlabPerimeterCalculator.Instance;
            ifcSlabQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SlabWidthCalculator.Instance;
            ifcSlabQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcSlabQuantity);
        }

        /// <summary>
        /// Initializes ramp flight base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitRampFlightBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcRampFlight);

            QuantityEntry ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH;
            ifcBaseQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes Building Storey base quantity
        /// </summary>
        /// <param name="baseQuantities"></param>
        private static void InitBuildingStoreyBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcBuildingStorey);

            QuantityEntry ifcQE = new QuantityEntry("NetHeight");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitParameterName = "IfcQtyNetHeight";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossHeight");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitParameterName = "IfcQtyGrossHeight";
            ifcBaseQuantity.AddEntry(ifcQE);

            ExportOptionsCache exportOptionsCache = ExporterCacheManager.ExportOptionsCache;
            if (String.Compare(exportOptionsCache.SelectedConfigName, "FMHandOverView") != 0)   // FMHandOver view exclude NetArea, GrossArea, NetVolume and GrossVolumne
            {
                ifcQE = new QuantityEntry("NetFloorArea");
                ifcQE.QuantityType = QuantityType.Area;
                ifcQE.PropertyCalculator = SpaceLevelAreaCalculator.Instance;
                ifcBaseQuantity.AddEntry(ifcQE);

                ifcQE = new QuantityEntry("GrossFloorArea");
                ifcQE.QuantityType = QuantityType.Area;
                ifcQE.PropertyCalculator = SpaceLevelAreaCalculator.Instance;
                ifcBaseQuantity.AddEntry(ifcQE);

                ifcQE = new QuantityEntry("GrossPerimeter");
                ifcQE.QuantityType = QuantityType.PositiveLength;
                ifcQE.RevitParameterName = "IfcQtyGrossPerimeter";
                ifcBaseQuantity.AddEntry(ifcQE);

                ifcQE = new QuantityEntry("NetVolume");
                ifcQE.QuantityType = QuantityType.Volume;
                ifcQE.RevitParameterName = "IfcQtyNetVolume";
                ifcBaseQuantity.AddEntry(ifcQE);

                ifcQE = new QuantityEntry("GrossVolume");
                ifcQE.QuantityType = QuantityType.Volume;
                ifcQE.RevitParameterName = "IfcQtyGrossVolume";
                ifcBaseQuantity.AddEntry(ifcQE);
            }

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes Space base quantity
        /// </summary>
        /// <param name="baseQuantities"></param>
        private static void InitSpaceBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);

            QuantityEntry ifcQE = new QuantityEntry("NetFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("FinishCeilingHeight");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitParameterName = "IfcQtyFinishCeilingHeight";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("NetCeilingArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyNetCeilingArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossCeilingArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyGrossCeilingArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("NetWallArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyNetWallArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossWallArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyGrossWallArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Height");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpaceHeightCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("NetPerimeter");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitParameterName = "IfcQtyNetPerimeter";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossPerimeter");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpacePerimeterCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            ExportOptionsCache exportOptionsCache = ExporterCacheManager.ExportOptionsCache;
            if (String.Compare(exportOptionsCache.SelectedConfigName, "FMHandOverView") != 0)   // FMHandOver view exclude GrossVolumne, FinishFloorHeight
            {
                ifcQE = new QuantityEntry("GrossVolume");
                ifcQE.MethodOfMeasurement = "volume measured in geometry";
                ifcQE.QuantityType = QuantityType.Volume;
                ifcQE.PropertyCalculator = SpaceVolumeCalculator.Instance;
                ifcBaseQuantity.AddEntry(ifcQE);

                ifcQE = new QuantityEntry("FinishFloorHeight");
                ifcQE.QuantityType = QuantityType.PositiveLength;
                ifcQE.RevitParameterName = "IfcQtyFinishFloorHeight";
                ifcBaseQuantity.AddEntry(ifcQE);
            }

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes Covering base quantity
        /// </summary>
        /// <param name="baseQuantities"></param>
        private static void InitCoveringBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcCovering);

            QuantityEntry ifcQE = new QuantityEntry("GrossArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyGrossArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("NetArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitParameterName = "IfcQtyNetArea";
            ifcBaseQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes Window base quantity
        /// </summary>
        /// <param name="baseQuantities"></param>
        private static void InitWindowBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcWindow);

            QuantityEntry ifcQE = new QuantityEntry("Height");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.WINDOW_HEIGHT;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.WINDOW_WIDTH;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Area");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = WindowAreaCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes Door base quantity
        /// </summary>
        /// <param name="baseQuantities"></param>
        private static void InitDoorBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcDoor);

            QuantityEntry ifcQE = new QuantityEntry("Height");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.DOOR_HEIGHT;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.DOOR_WIDTH;
            ifcBaseQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("Area");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = DoorAreaCalculator.Instance;
            ifcBaseQuantity.AddEntry(ifcQE);

            baseQuantities.Add(ifcBaseQuantity);
        }

        /// <summary>
        /// Initializes base quantities.
        /// </summary>
        /// <param name="quantities">List to store quantities.</param>
        /// <param name="fileVersion">The file version, currently unused.</param>
        private static void InitBaseQuantities(IList<IList<QuantityDescription>> quantities, IFCVersion fileVersion)
        {
            IList<QuantityDescription> baseQuantities = new List<QuantityDescription>();
            InitCeilingBaseQuantities(baseQuantities);
            InitRailingBaseQuantities(baseQuantities);
            InitSlabBaseQuantities(baseQuantities);
            InitRampFlightBaseQuantities(baseQuantities);
            InitBuildingStoreyBaseQuantities(baseQuantities);
            InitSpaceBaseQuantities(baseQuantities);
            InitCoveringBaseQuantities(baseQuantities);
            InitWindowBaseQuantities(baseQuantities);
            InitDoorBaseQuantities(baseQuantities);

            quantities.Add(baseQuantities);
        }

        /// <summary>
        /// Initializes COBIE quantities.
        /// </summary>
        /// <param name="quantities">List to store quantities.</param>
        /// <param name="fileVersion">The file version, currently unused.</param>
        private static void InitCOBIEQuantities(IList<IList<QuantityDescription>> quantities, IFCVersion fileVersion)
        {
            IList<QuantityDescription> cobieQuantities = new List<QuantityDescription>();
            InitCOBIESpaceQuantities(cobieQuantities);
            InitCOBIESpaceLevelQuantities(cobieQuantities);
            InitCOBIEPMSpaceQuantities(cobieQuantities);
            quantities.Add(cobieQuantities);
        }

        /// <summary>
        /// Initializes COBIE space quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIESpaceQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "BaseQuantities";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);

            QuantityEntry ifcQE = new QuantityEntry("Height");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpaceHeightCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossPerimeter");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpacePerimeterCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("NetFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            ifcQE = new QuantityEntry("GrossVolume");
            ifcQE.MethodOfMeasurement = "volume measured in geometry";
            ifcQE.QuantityType = QuantityType.Volume;
            ifcQE.PropertyCalculator = SpaceVolumeCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }

        /// <summary>
        /// Initializes COBIE space level quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIESpaceLevelQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "BaseQuantities";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);
            ifcCOBIEQuantity.DescriptionCalculator = SpaceLevelDescriptionCalculator.Instance;

            QuantityEntry ifcQE = new QuantityEntry("GrossFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceLevelAreaCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }

        /// <summary>
        /// Initializes COBIE BM space quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIEPMSpaceQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "Space Quantities (Property Management)";
            ifcCOBIEQuantity.MethodOfMeasurement = "As defined by BOMA (see www.boma.org)";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);

            QuantityEntry ifcQE = new QuantityEntry("NetFloorArea_BOMA");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.AddEntry(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }
    }
}
