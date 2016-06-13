﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.PS.Common.Vault.Explorer
{
    /// <summary>
    /// Simple wrapper on top of ObservableCollection, so we can enforce some validation logic plus register for:
    /// protected event PropertyChangedEventHandler PropertyChanged;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TypeConverter(typeof(ExpandableCollectionObjectConverter))]
    public class ObservableTagItemsCollection : ObservableCollection<TagItem>, ICustomTypeDescriptor
    {
        public ObservableTagItemsCollection() : base() { }

        public void SetPropertyChangedEventHandler(PropertyChangedEventHandler propertyChanged)
        {
            PropertyChanged += propertyChanged;
        }

        protected override void InsertItem(int index, TagItem item)
        {
            if (this.Count >= Consts.MaxNumberOfTags)
            {
                throw new ArgumentOutOfRangeException("Tags.Count", $"Too many tags, maximum number of tags for secret is only {Consts.MaxNumberOfTags}");
            }
            base.InsertItem(index, item);
        }

        public void AddOrReplace(TagItem item)
        {
            int i = IndexOf(item);
            if (i == -1)
            {
                Add(item);
            }
            else
            {
                SetItem(i, item);
            }
        }

        #region ICustomTypeDescriptor interface to show properties in PropertyGrid

        public string GetComponentName() => TypeDescriptor.GetComponentName(this, true);

        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);

        public string GetClassName() => TypeDescriptor.GetClassName(this, true);

        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);

        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);

        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);

        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);

        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

        public PropertyDescriptor GetDefaultProperty() => null;

        public PropertyDescriptorCollection GetProperties() => GetProperties(new Attribute[0]);

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {            
            return new PropertyDescriptorCollection((from tag in this select new ReadOnlyPropertyDescriptor(tag.Name, tag.Value)).ToArray());
        }

        #endregion
    }
    
    public class ExpandableCollectionObjectConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return (destinationType == typeof(string) && value is ICollection) ?
                $"{(value as ICollection).Count} item(s)" :
                base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
