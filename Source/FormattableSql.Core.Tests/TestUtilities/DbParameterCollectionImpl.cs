using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class DbParameterCollectionImpl : DbParameterCollection
    {
        private readonly List<DbParameter> mParameters = new List<DbParameter>();

        public override int Count => mParameters.Count;

        public override object SyncRoot => ((IList)mParameters).SyncRoot;

        public override int Add(object value)
        {
            return ((IList)mParameters).Add(value);
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
            {
                ((IList)mParameters).Add(value);
            }
        }

        public override void Clear()
        {
            mParameters.Clear();
        }

        public override bool Contains(object value)
        {
            return mParameters.Contains(value);
        }

        public override bool Contains(string value)
        {
            return mParameters.Any(x => x.ParameterName == value);
        }

        public override void CopyTo(Array array, int index)
        {
            ((IList)mParameters).CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return ((IList)mParameters).GetEnumerator();
        }

        public override int IndexOf(object value)
        {
            return ((IList)mParameters).IndexOf(value);
        }

        public override int IndexOf(string parameterName)
        {
            return mParameters
                       .Select((x, i) => Tuple.Create(x, i))
                       .Where(x => x.Item1.ParameterName == parameterName)
                       .Select(x => (int?)x.Item2)
                       .FirstOrDefault() ?? -1;
        }

        public override void Insert(int index, object value)
        {
            ((IList)mParameters).Insert(index, value);
        }

        public override void Remove(object value)
        {
            ((IList)mParameters).Remove(value);
        }

        public override void RemoveAt(int index)
        {
            mParameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            var param = GetParameter(parameterName);
            if (param != null)
            {
                mParameters.Remove(param);
            }
        }

        protected override DbParameter GetParameter(int index)
        {
            return mParameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return mParameters.FirstOrDefault(x => x.ParameterName == parameterName);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            mParameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var idx = IndexOf(parameterName);
            if (idx >= 0)
            {
                mParameters[idx] = value;
            }
        }
    }
}
