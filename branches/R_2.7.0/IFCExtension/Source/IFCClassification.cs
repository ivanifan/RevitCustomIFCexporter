﻿//
// BIM IFC export alternate UI library: this library works with Autodesk(R) Revit(R) to provide an alternate user interface for the export of IFC files from Revit.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Revit.IFC.Common.Extensions
{
    public class IFCClassification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The Classification system name, e.g. DIN or STABU
        /// </summary>
        private string classificationName;
        public string ClassificationName
        {
            get { return classificationName; }
            set
            {
                classificationName = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationNameTextBox");
            }
        }

        /// <summary>
        /// The Classification system edition number.
        /// </summary>
        private string classificationEdition;
        public string ClassificationEdition
        {
            get { return classificationEdition; }
            set
            {
                classificationEdition = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationEditionTextBox");
            }
        }

        /// <summary>
        /// The Source or publisher of the Classification system, usually in form of URL.
        /// </summary>
        private string classificationSource;
        public string ClassificationSource
        {
            get { return classificationSource; }
            set
            {
                classificationSource = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationSourceTextBox");
            }
        }

        /// <summary>
        /// The edition date (optional).
        /// </summary>
        private DateTime classificationEditionDate;
        public DateTime ClassificationEditionDate
        {
            get { return classificationEditionDate; }
            set
            {
                classificationEditionDate = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationEditionDateTextBox");
            }
        }

        /// <summary>
        /// The Classification location
        /// </summary>
        private string classificationLocation;
        public string ClassificationLocation
        {
            get { return classificationLocation; }
            set
            {
                classificationLocation = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationLocationTextBox");
            }
        }

        /// <summary>
        /// This property is only used for the UI message. It will not be stored in the schema
        /// </summary>
        private string classificationTabMsg;
        public string ClassificationTabMsg
        {
            get { return classificationTabMsg; }
            set
            {
                classificationTabMsg = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClassificationTabMsg");
            }
        }

        /// <summary>
        /// Check whether the items are the same
        /// </summary>
        public Boolean isUnchanged(IFCClassification classificationToCheck)
        {
            // Only check 4 properties that are stored into the schema.
            
            if (string.Compare(this.ClassificationName, classificationToCheck.ClassificationName) == 0
                && string.Compare(this.ClassificationSource, classificationToCheck.ClassificationSource) == 0
                && string.Compare(this.ClassificationEdition, classificationToCheck.ClassificationEdition) == 0
                && this.ClassificationEditionDate.Equals(classificationToCheck.ClassificationEditionDate)
                && string.Compare(this.ClassificationLocation, classificationToCheck.ClassificationLocation) == 0)
                    return true;      

            return false;
        }

        /// <summary>
        /// Check mandatory fields are filled in
        /// </summary>
        /// <returns></returns>
        public Boolean areMandatoryFieldsFilled()
        {
            if (string.IsNullOrEmpty(this.ClassificationName)
                || string.IsNullOrEmpty(this.ClassificationSource)
                || string.IsNullOrEmpty(this.ClassificationEdition))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Determine that the 3 mandatory fields are empty
        /// </summary>
        /// <returns></returns>
        public Boolean isMandatoryEmpty()
        {
            if (string.IsNullOrEmpty(this.ClassificationName)
                && string.IsNullOrEmpty(this.ClassificationSource)
                && string.IsNullOrEmpty(this.ClassificationEdition))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Event handler when the property is changed.
        /// </summary>
        /// <param name="name">name of the property.</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// To clone the IFC Classification
        /// </summary>
        /// <returns></returns>
        public IFCClassification Clone()
        {
            return new IFCClassification(this);
        }

        public IFCClassification ()
        {
        }

        /// <summary>
        /// Actual copy/clone of the IFC Classification.
        /// </summary>
        /// <param name="other">the source File header to clone.</param>
        private IFCClassification(IFCClassification other)
        {
            this.ClassificationName = other.ClassificationName;
            this.ClassificationSource = other.ClassificationSource;
            this.ClassificationEdition = other.ClassificationEdition;
            this.ClassificationEditionDate = other.ClassificationEditionDate;
            this.ClassificationLocation = other.ClassificationLocation;
        }

    }
}
