using FluentAssertions.Common;
using Moq;
using Moq.Protected;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class DbDataReaderBuilder
    {
        private readonly DataTable mData = new DataTable();
        private readonly List<ICollection<DbDataReader>> mDataReaderTrackers = new List<ICollection<DbDataReader>>();

        public DbDataReader Build()
        {
            var reader = mData.CreateDataReader();

            foreach (var tracker in mDataReaderTrackers)
            {
                tracker.Add(reader);
            }

            return reader;
        }

        public DbDataReaderBuilder TrackWith(ICollection<DbDataReader> collection)
        {
            mDataReaderTrackers.Add(collection);
            return this;
        }

        public DbDataReaderBuilder WithResults<T>(params T[] data)
        {
            mData.Columns.Clear();
            mData.Rows.Clear();

            var properties = typeof(T)
                .GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !property.IsIndexer())
                .ToArray();

            foreach (var property in properties)
            {
                mData.Columns.Add(property.Name, property.PropertyType);
            }

            foreach (var datum in data)
            {
                mData.Rows.Add(properties.Select(property => property.GetValue(datum)).ToArray());
            }

            return this;
        }
    }
}
