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
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export generic MEP family instances.
    /// </summary>
    class GenericMEPExporter
    {
        /// <summary>
        /// Exports a MEP family instance.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The ProductWrapper.
        /// </param>
        public static void Export(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, ProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                string ifcEnumType;
                IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, element, out ifcEnumType);

                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element))
                {
                    IFCAnyHandle localPlacementToUse = setter.GetPlacement();
                    using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
                    {

                        ElementId catId = CategoryUtil.GetSafeCategoryId(element);

                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                        IFCAnyHandle productRepresentation = RepresentationUtil.CreateAppropriateProductDefinitionShape(exporterIFC, 
                            element, catId, geometryElement, bodyExporterOptions, null, extraParams);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(productRepresentation))
                        {
                            extraParams.ClearOpenings();
                            return;
                        }

                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        ElementId typeId = element.GetTypeId();
                        ElementType type = element.Document.GetElement(typeId) as ElementType;
                        FamilyTypeInfo currentTypeInfo = ExporterCacheManager.TypeObjectsCache.Find(typeId, false);

                        bool found = currentTypeInfo.IsValid();
                        if (!found)
                        {
                            string typeGUID = GUIDUtil.CreateGUID(type);
                            string typeName = NamingUtil.GetNameOverride(type, NamingUtil.GetIFCName(type));
                            string typeObjectType = NamingUtil.GetObjectTypeOverride(type, NamingUtil.CreateIFCObjectName(exporterIFC, type));
                            string applicableOccurence = NamingUtil.GetOverrideStringValue(type, "IfcApplicableOccurrence", typeObjectType);
                            string typeDescription = NamingUtil.GetDescriptionOverride(type, null);
                            string typeTag = NamingUtil.GetTagOverride(type, NamingUtil.CreateIFCElementId(type));
                            string typeElementType = NamingUtil.GetOverrideStringValue(type, "IfcElementType", typeName);

                            HashSet<IFCAnyHandle> propertySetsOpt = new HashSet<IFCAnyHandle>();
                            IList<IFCAnyHandle> repMapListOpt = new List<IFCAnyHandle>();

                            IFCAnyHandle styleHandle = FamilyExporterUtil.ExportGenericType(exporterIFC, exportType, ifcEnumType, typeGUID, typeName,
                               typeDescription, applicableOccurence, propertySetsOpt, repMapListOpt, typeTag, typeElementType, element, type);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(styleHandle))
                            {
                                currentTypeInfo.Style = styleHandle;
                                ExporterCacheManager.TypeObjectsCache.Register(typeId, false, currentTypeInfo);
                            }
                        }
                        string instanceGUID = GUIDUtil.CreateGUID(element);
                        string instanceName = NamingUtil.GetNameOverride(element, NamingUtil.GetIFCName(element));
                        string instanceObjectType = NamingUtil.GetObjectTypeOverride(element, NamingUtil.CreateIFCObjectName(exporterIFC, element));
                        string instanceDescription = NamingUtil.GetDescriptionOverride(element, null);
                        string instanceTag = NamingUtil.GetTagOverride(element, NamingUtil.CreateIFCElementId(element));

                        bool roomRelated = !FamilyExporterUtil.IsDistributionFlowElementSubType(exportType);

                        ElementId roomId = ElementId.InvalidElementId;
                        if (roomRelated)
                        {
                            roomId = setter.UpdateRoomRelativeCoordinates(element, out localPlacementToUse);
                        }

                        IFCAnyHandle instanceHandle = null;
                        if (FamilyExporterUtil.IsFurnishingElementSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFurnishingElement(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsDistributionFlowElementSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateDistributionFlowElement(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsEnergyConversionDeviceSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateEnergyConversionDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowFittingSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowFitting(file, instanceGUID, ownerHistory,
                              instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowMovingDeviceSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowMovingDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowSegmentSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowSegment(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowStorageDeviceSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowStorageDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowTerminalSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowTerminal(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowTreatmentDeviceSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowTreatmentDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }
                        else if (FamilyExporterUtil.IsFlowControllerSubType(exportType))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowController(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceTag);
                        }

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(instanceHandle))
                            return;

                        if (roomId != ElementId.InvalidElementId)
                        {
                            exporterIFC.RelateSpatialElement(roomId, instanceHandle);
                            productWrapper.AddElement(instanceHandle, setter, extraParams, false);
                        }
                        else
                        {
                            productWrapper.AddElement(instanceHandle, setter, extraParams, LevelUtil.AssociateElementToLevel(element));
                        }

                        OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, element, extraParams, exporterIFC, localPlacementToUse, setter, productWrapper);

                        if (currentTypeInfo.IsValid())
                            ExporterCacheManager.TypeRelationsCache.Add(currentTypeInfo.Style, instanceHandle);

                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);

                        ExporterCacheManager.MEPCache.Register(element, instanceHandle);

                        tr.Commit();
                    }
                }
            }
        }
    }
}
